using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace test
{
    public partial class Form1 : Form
    {
        private Thread timeThread;  // 获取时间的线程 / Thread for getting time
        private bool isRunning = true;  // 控制线程运行的标志 / Flag to control thread execution
        private Thread uiUpdateThread;  // UI更新线程 / UI update thread
        private string currentTime = "";  // 时间数据 / Time data
        private string imagePath = "";    // 图片路径数据 / Image path data
        private Image currentImage = null; // 图片数据 / Image data
        private bool uiNeedsUpdate = false; // UI需要更新的标志 / Flag indicating UI needs update
        private object lockObj = new object(); // 同步锁 / Synchronization lock
        private InferenceSession segSession;
        private string modelPath; // 模型路径（自动获取）/ Model path (auto-detected)
        private string outputFolder; // 输出文件夹（自动获取）/ Output folder (auto-detected)
        private Panel indicatorLight;
        private bool check=false;

        public class DetectionResult
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float Confidence { get; set; }

            public float CenterX { get; set; }

            public float CenterY { get; set; }
        }

        public Form1()
        {
            InitializeComponent();

            // 自动获取项目所在目录 / Automatically get project directory
            string projectDirectory = GetProjectDirectory();
            
            // 设置模型路径为项目目录下的 sd900.onnx / Set model path to sd900.onnx in project directory
            modelPath = Path.Combine(projectDirectory, "sd900.onnx");
            
            // 设置输出文件夹为项目目录下的 output / Set output folder to output in project directory
            outputFolder = Path.Combine(projectDirectory, "output");

            // 确保输出文件夹存在 / Ensure output folder exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // 加载模型 / Load model
            LoadModel();

            // 启动UI更新线程 / Start UI update thread
            StartUIUpdateThread();

            // 启动时间更新线程 / Start time update thread
            StartTimeThread();

            // 初始化UI样式 / Initialize UI styles
            InitializeUIStyles();

        }

        // 初始化UI样式 / Initialize UI styles
        private void InitializeUIStyles()
        {
            // 设置按钮悬停效果 / Set button hover effects
            button1.MouseEnter += (s, e) => 
            {
                button1.BackColor = Color.FromArgb(0, 100, 180);
            };
            button1.MouseLeave += (s, e) => 
            {
                button1.BackColor = Color.FromArgb(0, 120, 215);
            };
            
            // 设置状态指示灯初始颜色和文字 / Set initial status indicator color and text
            // 初始狀態：沒有圖片，顯示藍色和 "_" / Initial state: No image, show blue and "_"
            panel1.BackColor = Color.FromArgb(0, 120, 215);
            labelStatusText.Text = "_";
            labelStatusText.ForeColor = Color.White; // 白色文字 / White text
        }



        private void LoadModel()
        {
            try
            {
                if (!File.Exists(modelPath))
                {
                    MessageBox.Show($"找不到模型文件: {modelPath}");
                    return;
                }

                // 加载ONNX模型 / Load ONNX model
                SessionOptions options = new SessionOptions();
                segSession = new InferenceSession(modelPath, options);

                // 检查模型输入输出 / Check model input and output
                Console.WriteLine("模型加载成功！");
                foreach (var input in segSession.InputMetadata)
                {
                    Console.WriteLine($"输入: {input.Key} 形状: [{string.Join(",", input.Value.Dimensions)}]");
                }
                foreach (var output in segSession.OutputMetadata)
                {
                    Console.WriteLine($"输出: {output.Key} 形状: [{string.Join(",", output.Value.Dimensions)}]");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模型失败: {ex.Message}");
            }
        }

        // ==================== 2. 主处理函数 / Main Processing Function ====================
        private void ProcessAndSaveImage(string imagePath)
        {
            if (segSession == null)
            {
                MessageBox.Show("请先加载模型！");
                LoadModel();
                if (segSession == null) return;
            }

            try
            {
                // 加载图片 / Load image
                using (Image originalImage = Image.FromFile(imagePath))
                {
                    // 获取文件名（不含扩展名）/ Get filename without extension
                    string fileName = Path.GetFileNameWithoutExtension(imagePath);

                    // ==================== 处理图片 / Process Image ====================
                    var (resultImage, results) = ProcessImage(originalImage);

                    if (resultImage != null)
                    {
                        // 生成時間戳記（年月日時分秒）/ Generate timestamp (yyyyMMddHHmmss)
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        
                        // ==================== 保存处理后的图片 / Save Processed Image ====================
                        // 文件名格式：原文件名_時間戳記_processed.jpg / Filename format: originalname_timestamp_processed.jpg
                        string outputImagePath = Path.Combine(outputFolder, $"{fileName}_{timestamp}_processed.jpg");
                        resultImage.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                        // ==================== 保存检测结果到TXT / Save Detection Results to TXT ====================
                        // 文件名格式：原文件名_時間戳記_results.txt / Filename format: originalname_timestamp_results.txt
                        string outputTxtPath = Path.Combine(outputFolder, $"{fileName}_{timestamp}_results.txt");
                        SaveResultsToTxt(outputTxtPath, results, originalImage.Width, originalImage.Height);

                        // ==================== 显示结果 / Display Results ====================
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = (Image)resultImage.Clone();

                        // 显示统计信息 / Display statistics
                        //ShowResultsInTextBox(results);

                        MessageBox.Show($"处理完成！\n图片保存到: {outputImagePath}\n结果保存到: {outputTxtPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理失败: {ex.Message}");
            }
        }

        // ==================== 3. 处理单张图片 / Process Single Image ====================
        private (Image ResultImage, List<DetectionResult> Results) ProcessImage(Image originalImage)
        {
            try
            {
                // 调整大小到640x640 / Resize to 640x640
                using (Bitmap resized = ResizeImage(new Bitmap(originalImage), 640, 640))
                {
                    // 创建输入Tensor [1, 3, 640, 640] / Create input tensor [1, 3, 640, 640]
                    var inputTensor = new DenseTensor<float>(new[] { 1, 3, 640, 640 });

                    // 填充RGB数据（归一化0-1）/ Fill RGB data (normalized 0-1)
                    for (int y = 0; y < 640; y++)
                    {
                        for (int x = 0; x < 640; x++)
                        {
                            Color pixel = resized.GetPixel(x, y);
                            inputTensor[0, 0, y, x] = pixel.R / 255.0f;
                            inputTensor[0, 1, y, x] = pixel.G / 255.0f;
                            inputTensor[0, 2, y, x] = pixel.B / 255.0f;
                        }
                    }

                    // 运行推理 / Run inference
                    string inputName = segSession.InputMetadata.Keys.First();
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
                    };

                    using (var results = segSession.Run(inputs))
                    {
                        // 获取输出Tensor / Get output tensor
                        var output = results.First().AsTensor<float>();

                        // 解析检测结果 / Parse detection results
                        List<DetectionResult> detections = ParseDetections(output, originalImage);
                        if (detections.Count > 0) {check=true;}else{check=false;}

                            // 绘制检测结果 / Draw detection results
                            Image resultImage = DrawDetectionsWithFill(originalImage, detections);

                        return (resultImage, detections);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"图片处理失败: {ex.Message}");
                return (originalImage, new List<DetectionResult>());
            }
        }

        // ==================== 4. 解析检测结果 / Parse Detection Results ====================
        private List<DetectionResult> ParseDetections(Tensor<float> output, Image originalImage)
        {
            List<DetectionResult> results = new List<DetectionResult>();

            // 假设输出形状是 [1, 5, 8400]（xywh + 1个类别置信度）/ Assume output shape is [1, 5, 8400] (xywh + 1 class confidence)
            // 或者 [1, 85, 8400]（xywh + 80个类别置信度 + ...）/ Or [1, 85, 8400] (xywh + 80 class confidences + ...)

            int numBoxes = output.Dimensions[2]; // 通常是8400 / Usually 8400

            for (int i = 0; i < numBoxes; i++)
            {
                try
                {
                    // 获取边界框（归一化坐标）/ Get bounding box (normalized coordinates)
                    float cx = output[0, 0, i];
                    float cy = output[0, 1, i];
                    float w = output[0, 2, i];
                    float h = output[0, 3, i];

                    // 获取"YJ"类别的置信度（假设是第5个值）/ Get "YJ" class confidence (assumed to be the 5th value)
                    float confidence = output[0, 4, i];

                    // 置信度阈值 / Confidence threshold
                    if (confidence > 0.5f)
                    {
                        // 转换为像素坐标 / Convert to pixel coordinates
                        int imgWidth = originalImage.Width;
                        int imgHeight = originalImage.Height;

                        int x = (int)((cx - w / 2) * imgWidth);
                        int y = (int)((cy - h / 2) * imgHeight);
                        int width = (int)(w * imgWidth);
                        int height = (int)(h * imgHeight);

                        // 确保坐标在图片范围内 / Ensure coordinates are within image bounds
                        x = Math.Max(0, Math.Min(x, imgWidth - 1));
                        y = Math.Max(0, Math.Min(y, imgHeight - 1));
                        width = Math.Max(1, Math.Min(width, imgWidth - x));
                        height = Math.Max(1, Math.Min(height, imgHeight - y));

                        results.Add(new DetectionResult
                        {
                            X = x,
                            Y = y,
                            Width = width,
                            Height = height,
                            Confidence = confidence,
                            CenterX = x + width / 2,
                            CenterY = y + height / 2
                        });
                    }
                }
                catch
                {
                    // 跳过错误的检测框 / Skip invalid detection boxes
                    continue;
                }
            }

            // 应用NMS去重 / Apply NMS to remove duplicates
            return ApplyNMS(results);
        }





        // ==================== 5. 绘制检测框 / Draw Detection Boxes ====================


        private Image DrawDetectionsWithFill(Image original, List<DetectionResult> detections)
        {
            Bitmap result = new Bitmap(original);

            using (Graphics g = Graphics.FromImage(result))
            {
                // 使用非常明显的颜色 / Use very visible colors
                Color fillColor = Color.FromArgb(100, 0, 0, 255); // 蓝色半透明 / Blue semi-transparent
                Color borderColor = Color.Red; // 红色边框，这样一定能看见 / Red border, definitely visible
                Color textColor = Color.White;
                Color textBgColor = Color.Black;

                using (SolidBrush fillBrush = new SolidBrush(fillColor))
                using (Pen borderPen = new Pen(borderColor, 3)) // 更粗的边框 / Thicker border
                using (Font labelFont = new Font("Arial", 12, FontStyle.Bold))
                {
                    for (int i = 0; i < detections.Count; i++)
                    {
                        var det = detections[i];

                        // 绘制填充矩形 / Draw filled rectangle
                        g.FillRectangle(fillBrush, det.X, det.Y, det.Width, det.Height);

                        // 绘制边框 / Draw border
                        g.DrawRectangle(borderPen, det.X, det.Y, det.Width, det.Height);

                        // 绘制标签 / Draw label
                        string label = $"目标{i + 1}: {(det.Confidence * 100):F1}%";
                        SizeF textSize = g.MeasureString(label, labelFont);

                        // 标签位置：框内左上角 / Label position: top-left inside box
                        float labelX = det.X + 5;
                        float labelY = det.Y + 5;

                        // 绘制标签背景 / Draw label background
                        g.FillRectangle(new SolidBrush(textBgColor),
                            labelX - 2, labelY - 2,
                            textSize.Width + 4, textSize.Height + 4);

                        // 绘制标签文字 / Draw label text
                        g.DrawString(label, labelFont, new SolidBrush(textColor), labelX, labelY);
                    }
                }

                // 在图片顶部添加大标题 / Add large title at top of image
                if (detections.Count > 0)
                {
                    string title = $"检测到 {detections.Count} 个目标";
                    using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
                    {
                        SizeF titleSize = g.MeasureString(title, titleFont);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)),
                            10, 10, titleSize.Width + 20, titleSize.Height + 10);
                        g.DrawString(title, titleFont, Brushes.White, 20, 15);
                    }
                }
            }

            return result;
        }


        private Image DrawDetections(Image original, List<DetectionResult> detections)
        {
            Bitmap result = new Bitmap(original);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int index = 1;
                foreach (var det in detections)
                {
                    // 选择颜色（循环使用几种颜色）/ Select color (cycle through several colors)
                    Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple };
                    Color boxColor = colors[(index - 1) % colors.Length];

                    using (Pen pen = new Pen(boxColor, 3))
                    {
                        // 绘制边界框 / Draw bounding box
                        g.DrawRectangle(pen, det.X, det.Y, det.Width, det.Height);

                        // 绘制编号 / Draw number
                        string label = $"YJ-{index}: {det.Confidence:P0}";
                        Font font = new Font("Arial", 12, FontStyle.Bold);
                        SizeF textSize = g.MeasureString(label, font);

                        // 绘制标签背景 / Draw label background
                        RectangleF textRect = new RectangleF(
                            det.X, det.Y - textSize.Height - 2,
                            textSize.Width + 4, textSize.Height + 2);

                        g.FillRectangle(Brushes.Black, textRect);

                        // 绘制标签文字 / Draw label text
                        g.DrawString(label, font, Brushes.White, det.X + 2, det.Y - textSize.Height);

                        // 绘制中心点 / Draw center point
                        g.FillEllipse(Brushes.Yellow, det.CenterX - 3, det.CenterY - 3, 6, 6);
                    }

                    index++;
                }

                // 绘制统计信息 / Draw statistics
                if (detections.Count > 0)
                {
                    string stats = $"检测到 {detections.Count} 个YJ目标";
                    g.DrawString(stats, new Font("Arial", 14, FontStyle.Bold),
                                Brushes.Red, 10, 10);
                }
            }

            return result;
        }

        // ==================== 6. 保存结果到TXT文件 / Save Results to TXT File ====================
        private void SaveResultsToTxt(string filePath, List<DetectionResult> results, int imgWidth, int imgHeight)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    writer.WriteLine("=== 目标检测结果 ===");
                    writer.WriteLine($"图片尺寸: {imgWidth} x {imgHeight}");
                    writer.WriteLine($"检测时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"目标总数: {results.Count}");
                    writer.WriteLine();
                    writer.WriteLine("序号\t置信度\tX\tY\t宽度\t高度\t中心X\t中心Y");
                    writer.WriteLine("----------------------------------------------------------------");

                    int index = 1;
                    foreach (var result in results.OrderByDescending(r => r.Confidence))
                    {
                        writer.WriteLine($"{index}\t{result.Confidence:P2}\t" +
                                       $"{result.X}\t{result.Y}\t" +
                                       $"{result.Width}\t{result.Height}\t" +
                                       $"{result.CenterX}\t{result.CenterY}");
                        index++;
                    }

                    writer.WriteLine();
                    writer.WriteLine("=== 坐标归一化 (0-1) ===");
                    index = 1;
                    foreach (var result in results.OrderByDescending(r => r.Confidence))
                    {
                        float normX = (float)result.CenterX / imgWidth;
                        float normY = (float)result.CenterY / imgHeight;
                        float normW = (float)result.Width / imgWidth;
                        float normH = (float)result.Height / imgHeight;

                        writer.WriteLine($"YJ-{index}: 中心({normX:F3},{normY:F3}) 尺寸({normW:F3},{normH:F3})");
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存结果失败: {ex.Message}");
            }
        }

        // ==================== 8. NMS去重 / NMS Deduplication ====================
        private List<DetectionResult> ApplyNMS(List<DetectionResult> detections, float iouThreshold = 0.45f)
        {
            if (detections.Count == 0) return detections;

            // 按置信度降序排序 / Sort by confidence in descending order
            var sorted = detections.OrderByDescending(d => d.Confidence).ToList();
            List<DetectionResult> results = new List<DetectionResult>();

            while (sorted.Count > 0)
            {
                // 取出置信度最高的 / Take the one with highest confidence
                var best = sorted[0];
                results.Add(best);
                sorted.RemoveAt(0);

                // 计算与剩余检测框的IOU / Calculate IOU with remaining detection boxes
                for (int i = sorted.Count - 1; i >= 0; i--)
                {
                    float iou = CalculateIOU(best, sorted[i]);
                    if (iou > iouThreshold)
                    {
                        sorted.RemoveAt(i);
                    }
                }
            }

            return results;
        }

        private float CalculateIOU(DetectionResult box1, DetectionResult box2)
        {
            int x1 = Math.Max(box1.X, box2.X);
            int y1 = Math.Max(box1.Y, box2.Y);
            int x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
            int y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

            int intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            int area1 = box1.Width * box1.Height;
            int area2 = box2.Width * box2.Height;

            return (float)intersection / (area1 + area2 - intersection);
        }

        // ==================== 9. 辅助函数 / Helper Functions ====================
        
        // 获取项目根目录 / Get project root directory
        private string GetProjectDirectory()
        {
            // 从程序集位置开始 / Start from assembly location
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            // 向上查找项目根目录（包含 .sln 或 .csproj 文件的目录）/ Search upward for project root (directory containing .sln or .csproj file)
            while (currentDir != null)
            {
                // 检查当前目录是否包含项目文件 / Check if current directory contains project files
                if (Directory.GetFiles(currentDir, "*.sln").Length > 0 || 
                    Directory.GetFiles(currentDir, "*.csproj").Length > 0)
                {
                    return currentDir;
                }
                
                // 向上查找父目录 / Search parent directory
                DirectoryInfo parent = Directory.GetParent(currentDir);
                if (parent == null)
                {
                    break;
                }
                currentDir = parent.FullName;
            }
            
            // 如果找不到项目文件，返回程序集所在目录 / If project file not found, return assembly directory
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }

            return result;
        }


        private void StartTimeThread()
        {
            isRunning = true;
            timeThread = new Thread(new ThreadStart(UpdateTime));
            timeThread.IsBackground = true;  // 设置为后台线程，窗体关闭时自动结束 / Set as background thread, auto-terminate when form closes
            timeThread.Start();
        }
        private void StartUIUpdateThread()
        {
            uiUpdateThread = new Thread(new ThreadStart(UpdateUI));
            uiUpdateThread.IsBackground = true;
            uiUpdateThread.Name = "UIUpdateThread";
            uiUpdateThread.Start();
        }
        // 时间更新线程的方法 / Time update thread method
        private void UpdateTime()
        {
            while (isRunning)
            {
                try
                {
                    // 获取当前时间 / Get current time
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

                    // 使用Invoke或BeginInvoke更新UI控件 / Use Invoke or BeginInvoke to update UI controls
                    if (labelTime.InvokeRequired)
                    {
                        labelTime.BeginInvoke(new Action(() =>
                        {
                            labelTime.Text = currentTime;
                        }));
                    }
                    else
                    {
                        labelTime.Text = currentTime;
                    }

                    // 每秒更新一次 / Update every second
                    Thread.Sleep(1000);
                }
                catch (ThreadAbortException)
                {
                    // 线程被终止 / Thread aborted
                    break;
                }
                catch (Exception)
                {
                    // 忽略其他异常 / Ignore other exceptions
                }
            }
        }
        private void UpdateUI()
        {
            while (isRunning)
            {
                try
                {
                    bool needsUpdate = false;
                    string timeToDisplay = "";
                    string pathToDisplay = "";
                    Image imageToDisplay = null;

                    // 检查是否需要更新UI / Check if UI needs update
                    lock (lockObj)
                    {
                        if (uiNeedsUpdate)
                        {
                            needsUpdate = true;
                            timeToDisplay = currentTime;
                            pathToDisplay = imagePath;
                            imageToDisplay = currentImage;
                            uiNeedsUpdate = false;
                        }
                    }

                    // 如果需要更新UI / If UI needs update
                    if (needsUpdate)
                    {
                        // 在主线程上更新UI控件 / Update UI controls on main thread
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                UpdateUIControls(timeToDisplay, pathToDisplay, imageToDisplay,check);
                            }));
                        }
                        else
                        {
                            UpdateUIControls(timeToDisplay, pathToDisplay, imageToDisplay,check);
                        }
                    }

                    // 降低UI更新频率，减少CPU占用 / Reduce UI update frequency to decrease CPU usage
                    Thread.Sleep(50);  // 20fps的更新频率 / 20fps update frequency
                }
                catch (Exception)
                {
                    // 忽略异常 / Ignore exceptions
                }
            }
        }

        // 更新UI控件的方法 / Method to update UI controls
        private void UpdateUIControls(string time, string path, Image image,bool check)
        {
            // 更新时间显示 / Update time display
            if (!string.IsNullOrEmpty(time) && labelTime.Text != time)
            {
                labelTime.Text = time;
            }

            // 更新路径显示 / Update path display
            if (!string.IsNullOrEmpty(path) && textBox1.Text != path)
            {
                textBox1.Text = path;
            }

            // 更新图片显示 / Update image display
            if (image != null && pictureBox1.Image != image)
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = image;
            }
            
            // 更新狀態指示燈顏色和文字 / Update status indicator color and text
            // 判斷是否有圖片 / Check if image exists
            bool hasImage = (image != null) || (pictureBox1.Image != null);
            
            if (!hasImage)
            {
                // 沒有圖片：藍色 + "_" / No image: Blue + "_"
                panel1.BackColor = Color.FromArgb(0, 120, 215);
                labelStatusText.Text = "_";
                labelStatusText.ForeColor = Color.White; // 白色文字在藍色背景上清晰可見 / White text visible on blue background
            }
            else if (check)
            {
                // 有抓到目標：NG（紅色）/ Target detected: NG (red)
                panel1.BackColor = Color.FromArgb(220, 53, 69);
                labelStatusText.Text = "NG";
                labelStatusText.ForeColor = Color.White; // 白色文字在紅色背景上清晰可見 / White text visible on red background
            }
            else
            {
                // 沒抓到目標：OK（綠色）/ No target detected: OK (green)
                panel1.BackColor = Color.FromArgb(40, 167, 69);
                labelStatusText.Text = "OK";
                labelStatusText.ForeColor = Color.White; // 白色文字在綠色背景上清晰可見 / White text visible on green background
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // 设置对话框标题 / Set dialog title
            openFileDialog1.Title = "選擇圖片文件";

            // 显示对话框 / Show dialog
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // 处理选择的文件 / Process selected file
                string selectedFilePath = openFileDialog1.FileName;

                try
                {
                    Thread imageLoadThread = new Thread(() => LoadImageInBackground(selectedFilePath));
                    imageLoadThread.IsBackground = true;
                    imageLoadThread.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加載失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadImageInBackground(string filePath)
        {
            try
            {
                // 加载图片（可能在后台线程）/ Load image (may be in background thread)
                Image loadedImage = Image.FromFile(filePath);
                ProcessAndSaveImage(filePath);

                // 更新数据 / Update data
                lock (lockObj)
                {
                    imagePath = filePath;
                    currentImage = loadedImage;
                    uiNeedsUpdate = true;
                }
            }
            catch (Exception ex)
            {
                // 在主线程显示错误信息 / Display error message on main thread
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show($"加载失败：{ex.Message}");
                }));
            }
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // 停止时间线程 / Stop time thread
            isRunning = false;

            // 可选：等待线程结束（最多等待1秒）/ Optional: Wait for thread to end (max 1 second)
            if (timeThread != null && timeThread.IsAlive)
            {
                timeThread.Join(1000);
            }

            // 清理图片资源 / Clean up image resources
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
