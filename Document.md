# CSharpYoloSimple 技術文件

## 1. 項目概述

### 1.1 項目簡介
CSharpYoloSimple 是一個基於 C# Windows Forms 開發的目標檢測應用程序。該應用使用 ONNX Runtime 加載和運行 YOLO 深度學習模型，實現對圖像中特定目標（YJ）的自動檢測、標記和結果保存功能。

### 1.2 主要功能
- **圖像目標檢測**：使用 YOLO ONNX 模型檢測圖像中的目標對象
- **可視化標記**：在檢測到的目標周圍繪製邊界框和標籤
- **結果保存**：將處理後的圖像和檢測結果保存到指定文件夾，文件名包含時間戳記
- **智能狀態指示**：通過彩色指示燈和文字顯示檢測狀態
  - 藍色 + "_"：尚未載入圖片或沒有檢測內容
  - 綠色 + "OK"：已檢測，未發現問題（正常）
  - 紅色 + "NG"：已檢測，發現問題（異常）
- **多線程處理**：使用後台線程處理圖像，避免 UI 阻塞
- **時間顯示**：實時顯示當前時間
- **現代化UI設計**：採用現代前端設計風格，界面美觀易用

### 1.3 技術特點
- 基於 .NET 8.0 Windows Forms 框架
- 使用 ONNX Runtime 進行模型推理
- 支持 NMS（非極大值抑制）去重算法
- 多線程異步處理，提升用戶體驗
- 完整的錯誤處理機制
- **自動路徑檢測**：模型和輸出路徑自動從項目目錄獲取，無需手動配置
- **中英文並列註釋**：所有代碼註釋採用中英文並列格式，便於中文和英文讀者理解

---

## 2. 代碼註釋規範

### 2.1 註釋格式
本項目採用中英文並列註釋格式，所有註釋均包含中文和英文說明，格式為：
```csharp
// 中文說明 / English description
```

### 2.2 註釋覆蓋範圍
- ✅ 所有類成員變量的用途說明
- ✅ 所有方法的功能說明
- ✅ 關鍵代碼邏輯的步驟說明
- ✅ 配置參數的說明
- ✅ 線程安全機制的說明

### 2.3 註釋示例
```csharp
// 获取时间的线程 / Thread for getting time
private Thread timeThread;

// 自动获取项目所在目录 / Automatically get project directory
string projectDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

// 加载模型 / Load model
LoadModel();
```

---

## 3. 技術架構

### 3.1 開發環境
- **框架**：.NET 8.0-windows
- **UI 框架**：Windows Forms
- **編程語言**：C# 11（啟用可空引用類型）

### 3.2 核心依賴項

| 依賴包 | 版本 | 用途 |
|--------|------|------|
| Microsoft.ML.OnnxRuntime | 1.23.2 | ONNX 模型推理引擎 |
| OpenCvSharp4 | 4.11.0.20250507 | 圖像處理庫（已引用但未在代碼中使用） |

### 3.3 項目結構

```
CSharpYoloSimple/
├── Program.cs              # 應用程序入口點
├── Form1.cs                # 主表單邏輯（核心業務代碼，含中英文註釋）
├── Form1.Designer.cs       # 表單設計器代碼（UI 布局）
├── Form1.resx              # 表單資源文件
├── DetectionBox.cs         # 檢測框類（目前未使用）
├── test.csproj             # 項目配置文件
├── App.config              # 應用程序配置文件
├── sd900.onnx              # ONNX 模型文件（需放置在運行目錄）
├── output/                 # 輸出文件夾（自動創建）
└── README.md               # 項目說明文件
```

---

## 4. 核心功能模塊

### 4.1 模型加載模塊

#### 4.1.1 路徑初始化（構造函數）
**功能**：自動獲取項目目錄並設置模型和輸出路徑

**實現位置**：`Form1.cs:49-77`

**關鍵實現**：
```csharp
// 自动获取项目所在目录 / Automatically get project directory
string projectDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

// 设置模型路径为项目目录下的 sd900.onnx / Set model path to sd900.onnx in project directory
modelPath = Path.Combine(projectDirectory, "sd900.onnx");

// 设置输出文件夹为项目目录下的 output / Set output folder to output in project directory
outputFolder = Path.Combine(projectDirectory, "output");

// 确保输出文件夹存在 / Ensure output folder exists
if (!Directory.Exists(outputFolder))
{
    Directory.CreateDirectory(outputFolder);
}
```

#### 4.1.2 LoadModel()
**功能**：加載 ONNX 模型文件並初始化推理會話

**實現位置**：`Form1.cs:81-110`

**路徑獲取**：
- 模型路徑在構造函數中自動設置為 `{項目目錄}\sd900.onnx`
- 使用 `Assembly.GetExecutingAssembly().Location` 獲取程序集所在目錄

**關鍵代碼**：
```csharp
// 在構造函數中自動獲取路徑
string projectDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
modelPath = Path.Combine(projectDirectory, "sd900.onnx");
outputFolder = Path.Combine(projectDirectory, "output");

// 確保輸出文件夾存在
if (!Directory.Exists(outputFolder))
{
    Directory.CreateDirectory(outputFolder);
}

private void LoadModel()
{
    // 檢查模型文件是否存在 / Check if model file exists
    if (!File.Exists(modelPath))
    {
        MessageBox.Show($"找不到模型文件: {modelPath}");
        return;
    }
    
    // 創建推理會話 / Create inference session
    SessionOptions options = new SessionOptions();
    segSession = new InferenceSession(modelPath, options);
    
    // 輸出模型輸入輸出信息 / Output model input/output information
    // ...
}
```

**配置參數**：
- `modelPath`: 自動從項目目錄獲取，默認為 `{項目目錄}\sd900.onnx`
- 使用 `Assembly.GetExecutingAssembly().Location` 自動獲取程序所在目錄
- 模型文件需放置在項目輸出目錄（bin\Debug\net8.0-windows\）或項目根目錄

**模型要求**：
- 輸入格式：`[1, 3, 640, 640]`（批次大小=1，通道=3，寬高=640）
- 輸出格式：`[1, 5, 8400]` 或類似格式（xywh + 置信度）
- 輸入數據範圍：0-1（已歸一化）

---

### 4.2 圖像處理模塊

#### 4.2.1 ProcessImage()
**功能**：處理單張圖像，執行模型推理並返回檢測結果

**實現位置**：`Form1.cs:153-203`

**處理流程**：
1. 將圖像調整為 640×640 像素
2. 將像素值歸一化到 0-1 範圍
3. 構建輸入 Tensor
4. 執行模型推理
5. 解析檢測結果
6. 繪製檢測框

**關鍵代碼片段**：
```csharp
// 調整圖像大小
using (Bitmap resized = ResizeImage(new Bitmap(originalImage), 640, 640))
{
    // 創建輸入 Tensor
    var inputTensor = new DenseTensor<float>(new[] { 1, 3, 640, 640 });
    
    // 填充 RGB 數據（歸一化）
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
    
    // 執行推理
    using (var results = segSession.Run(inputs))
    {
        var output = results.First().AsTensor<float>();
        List<DetectionResult> detections = ParseDetections(output, originalImage);
        Image resultImage = DrawDetectionsWithFill(originalImage, detections);
        return (resultImage, detections);
    }
}
```

#### 3.2.2 ResizeImage()
**功能**：使用高質量雙三次插值調整圖像大小

**實現位置**：`Form1.cs:483-494`

**特點**：
- 使用 `InterpolationMode.HighQualityBicubic` 確保圖像質量
- 返回新的 Bitmap 對象

---

### 4.3 檢測結果解析模塊

#### 4.3.1 ParseDetections()
**功能**：解析模型輸出 Tensor，提取檢測框信息

**實現位置**：`Form1.cs:206-267`

**解析邏輯**：
1. 遍歷所有檢測框（通常為 8400 個）
2. 提取中心點坐標 (cx, cy) 和寬高 (w, h)
3. 提取置信度（假設為第 5 個值）
4. 應用置信度閾值（0.5）
5. 將歸一化坐標轉換為像素坐標
6. 確保坐標在圖像範圍內
7. 應用 NMS 去重

**輸出格式**：
```csharp
public class DetectionResult
{
    public int X { get; set; }           // 左上角 X 坐標
    public int Y { get; set; }           // 左上角 Y 坐標
    public int Width { get; set; }       // 寬度
    public int Height { get; set; }      // 高度
    public float Confidence { get; set; } // 置信度
    public float CenterX { get; set; }   // 中心點 X
    public float CenterY { get; set; }   // 中心點 Y
}
```

**置信度閾值**：0.5（硬編碼）

---

### 4.4 NMS 去重模塊

#### 4.4.1 ApplyNMS()
**功能**：使用非極大值抑制算法去除重疊的檢測框

**實現位置**：`Form1.cs:439-466`

**算法流程**：
1. 按置信度降序排序
2. 選擇置信度最高的檢測框
3. 計算與其他檢測框的 IOU
4. 移除 IOU 超過閾值的檢測框
5. 重複步驟 2-4 直到處理完所有檢測框

**IOU 閾值**：0.45（默認值）

#### 4.4.2 CalculateIOU()
**功能**：計算兩個檢測框的交並比（Intersection over Union）

**實現位置**：`Form1.cs:468-480`

**計算公式**：
```
IOU = intersection_area / (area1 + area2 - intersection_area)
```

---

### 4.5 可視化模塊

#### 3.5.1 DrawDetectionsWithFill()
**功能**：在圖像上繪製檢測框，使用半透明填充和邊框

**實現位置**：`Form1.cs:276-335`

**繪製元素**：
- **填充矩形**：藍色半透明（Alpha=100）
- **邊框**：紅色，線寬 3 像素
- **標籤**：顯示目標編號和置信度百分比
- **標題**：在圖像頂部顯示檢測到的目標數量

**視覺效果**：
- 標籤背景：黑色半透明
- 標籤文字：白色，Arial 12pt 粗體
- 標題：白色，Arial 16pt 粗體

#### 4.5.2 DrawDetections()
**功能**：另一種繪製方式（備用方法，使用不同顏色）

**實現位置**：`Form1.cs:338-390`

**特點**：
- 使用多種顏色循環標記（紅、綠、藍、橙、紫）
- 繪製中心點（黃色圓點）
- 顯示統計信息

---

### 4.6 文件處理模塊

#### 4.6.1 ProcessAndSaveImage()
**功能**：處理圖像並保存結果

**實現位置**：`Form1.cs:105-150`

**處理流程**：
1. 檢查模型是否已加載
2. 加載原始圖像
3. 執行檢測處理
4. 生成時間戳記（格式：yyyyMMddHHmmss）
5. 保存處理後的圖像（JPEG 格式），文件名包含時間戳記
6. 保存檢測結果到 TXT 文件，文件名包含時間戳記
7. 更新 UI 顯示

**輸出文件格式**：
- 圖像：`{原文件名}_{時間戳記}_processed.jpg`
  - 例如：`image_20240115143025_processed.jpg`
- 結果：`{原文件名}_{時間戳記}_results.txt`
  - 例如：`image_20240115143025_results.txt`

**時間戳記格式**：`yyyyMMddHHmmss`（年月日時分秒，14位數字）

**輸出目錄**：自動設置為 `{項目目錄}\output`，如果不存在會自動創建

**優點**：
- 每次保存都有唯一文件名，不會覆蓋之前的文件
- 文件名包含完整時間信息，便於追蹤和管理
- 圖片和對應的結果文件使用相同時間戳記，便於配對

#### 4.6.2 SaveResultsToTxt()
**功能**：將檢測結果保存為文本文件

**實現位置**：`Form1.cs:393-436`

**文件格式**：
```
=== YJ目标检测结果 ===
图片尺寸: {width} x {height}
检测时间: {datetime}
目标总数: {count}

序号    置信度    X    Y    宽度    高度    中心X    中心Y
----------------------------------------------------------------
1       95.23%    100  200   50      60      125      230
...

=== 坐标归一化 (0-1) ===
YJ-1: 中心(0.125,0.230) 尺寸(0.050,0.060)
...
```

**編碼格式**：UTF-8

---

### 4.7 多線程處理模塊

#### 4.7.1 線程架構
應用程序使用三個主要線程：
1. **主 UI 線程**：處理用戶交互和控件更新
2. **時間更新線程**：每秒更新一次時間顯示
3. **UI 更新線程**：定期檢查並更新 UI 控件

#### 4.7.2 StartTimeThread()
**功能**：啟動時間更新線程

**實現位置**：`Form1.cs:514-520`

**特點**：
- 後台線程（IsBackground = true）
- 每秒更新一次
- 使用 `BeginInvoke` 進行線程安全更新

#### 4.7.3 StartUIUpdateThread()
**功能**：啟動 UI 更新線程

**實現位置**：`Form1.cs:521-527`

**更新頻率**：50ms（約 20 FPS）

#### 4.7.4 UpdateUI()
**功能**：UI 更新線程的主循環

**實現位置**：`Form1.cs:565-614`

**線程安全機制**：
- 使用 `lock` 保護共享數據
- 使用 `Invoke` 確保在主線程更新 UI

#### 3.7.5 LoadImageInBackground()
**功能**：在後台線程加載和處理圖像

**實現位置**：`Form1.cs:668-692`

**處理流程**：
1. 在後台線程加載圖像
2. 執行檢測處理
3. 更新共享數據（使用鎖保護）
4. 標記 UI 需要更新

---

### 4.8 UI 設計模塊

#### 4.8.0 現代化UI設計
**設計理念**：採用現代前端設計風格，提供美觀易用的用戶界面

**設計特點**：
- **扁平化設計**：按鈕採用扁平化風格，無邊框設計
- **現代配色方案**：
  - 主色調：白色背景 `Color.White`
  - 主按鈕：藍色 `Color.FromArgb(0, 120, 215)`
  - 成功狀態：綠色 `Color.FromArgb(40, 167, 69)`
  - 錯誤狀態：紅色 `Color.FromArgb(220, 53, 69)`
  - 未檢測狀態：藍色 `Color.FromArgb(0, 120, 215)`
  - 文字顏色：深灰 `Color.FromArgb(33, 33, 33)` 和淺灰 `Color.FromArgb(102, 102, 102)`
- **字體設計**：
  - 標題：Microsoft YaHei UI 18pt 粗體
  - 正文：Microsoft YaHei UI 9pt 常規
  - 時間顯示：14pt 粗體
  - 狀態文字：20pt 粗體
- **布局結構**：
  - 統一的間距和對齊
  - 卡片式設計（圖片容器）
  - 清晰的視覺層次

**控件改進**：
- **按鈕**：扁平化設計，懸停效果（顏色變化）
- **文本框**：只讀模式，白色背景
- **圖片容器**：灰色背景面板，內邊距設計
- **狀態指示器**：80x80 方框，內部顯示狀態文字

---

### 4.9 UI 交互模塊

#### 4.9.1 button1_Click()
**功能**：處理"選擇圖片"按鈕點擊事件

**實現位置**：`Form1.cs:641-667`

**處理流程**：
1. 顯示文件選擇對話框
2. 過濾圖片文件（jpg, jpeg, png, bmp, gif）
3. 在後台線程加載和處理選中的圖片

#### 4.9.2 UpdateUIControls()
**功能**：更新 UI 控件顯示

**實現位置**：`Form1.cs:659-710`

**更新內容**：
- `labelTime`：時間顯示
- `textBox1`：圖片路徑
- `pictureBox1`：處理後的圖像
- `panel1` 和 `labelStatusText`：狀態指示燈和狀態文字

**狀態指示邏輯**：
1. **沒有圖片**（`image == null` 且 `pictureBox1.Image == null`）：
   - 背景顏色：藍色 `Color.FromArgb(0, 120, 215)`
   - 顯示文字：`"_"`
   - 含義：尚未載入圖片或沒有檢測內容

2. **有圖片且檢測到目標**（`check == true`）：
   - 背景顏色：紅色 `Color.FromArgb(220, 53, 69)`
   - 顯示文字：`"NG"`
   - 含義：檢測到問題，需要處理

3. **有圖片但未檢測到目標**（`check == false`）：
   - 背景顏色：綠色 `Color.FromArgb(40, 167, 69)`
   - 顯示文字：`"OK"`
   - 含義：檢測正常，無問題

**視覺設計**：
- 狀態文字使用 20pt 粗體字體，白色文字
- 文字在狀態方框內居中顯示
- 顏色對比度高，易於識別

---

## 5. 數據結構

### 4.1 DetectionResult 類
**定義位置**：`Form1.cs:34-45`

```csharp
public class DetectionResult
{
    public int X { get; set; }           // 邊界框左上角 X 坐標（像素）
    public int Y { get; set; }           // 邊界框左上角 Y 坐標（像素）
    public int Width { get; set; }       // 邊界框寬度（像素）
    public int Height { get; set; }     // 邊界框高度（像素）
    public float Confidence { get; set; } // 檢測置信度（0-1）
    public float CenterX { get; set; }   // 中心點 X 坐標（像素）
    public float CenterY { get; set; }   // 中心點 Y 坐標（像素）
}
```

---

## 6. 配置參數

### 6.1 配置參數

| 參數 | 當前值 | 說明 | 狀態 |
|------|--------|------|------|
| `modelPath` | `{項目目錄}\sd900.onnx` | 模型文件路徑 | ✅ 自動獲取 |
| `outputFolder` | `{項目目錄}\output` | 輸出文件夾 | ✅ 自動創建 |
| 置信度閾值 | `0.5f` | 檢測置信度閾值 | ⚠️ 硬編碼，建議可配置化 |
| IOU 閾值 | `0.45f` | NMS IOU 閾值 | ⚠️ 硬編碼，建議可配置化 |
| 輸入圖像尺寸 | `640×640` | 模型輸入尺寸 | ⚠️ 硬編碼，建議從模型元數據讀取 |

**路徑自動獲取機制**：
- 使用 `Assembly.GetExecutingAssembly().Location` 獲取程序集所在目錄
- 模型文件 `sd900.onnx` 需放置在程序運行目錄
- 輸出文件夾 `output` 會自動創建（如果不存在）

---

## 7. 使用說明

### 7.1 運行前準備
1. 確保已安裝 .NET 8.0 Runtime
2. 準備 ONNX 模型文件 `sd900.onnx`，放置在程序運行目錄（通常為 `bin\Debug\net8.0-windows\`）
3. 輸出文件夾 `output` 會自動創建，無需手動創建
4. 確保程序對運行目錄有讀寫權限

### 7.2 操作流程
1. 啟動應用程序
2. 點擊"選擇圖片"按鈕
3. 在文件對話框中選擇要檢測的圖片
4. 等待處理完成（處理過程在後台進行）
5. 查看處理結果：
   - 圖片顯示區域：顯示帶檢測框的處理後圖像
   - 狀態指示器：
     - 藍色 + "_"：尚未載入圖片
     - 綠色 + "OK"：檢測正常，未發現問題
     - 紅色 + "NG"：檢測到問題，需要處理
   - 時間顯示：實時顯示當前時間
6. 檢查輸出文件夾中的結果文件（文件名包含時間戳記）

### 6.3 支持的圖片格式
- JPEG (.jpg, .jpeg)
- PNG (.png)
- BMP (.bmp)
- GIF (.gif)

---

## 8. 性能優化建議

### 8.1 當前性能特點
- ✅ 使用後台線程處理，避免 UI 阻塞
- ✅ 使用 NMS 去重，減少重複檢測
- ⚠️ 圖像縮放使用高質量插值，可能較慢
- ⚠️ 逐像素讀取和處理，效率較低

### 8.2 優化建議
1. **圖像處理優化**：
   - 使用 OpenCvSharp 進行圖像預處理（已引用但未使用）
   - 使用並行處理加速像素操作
   - 考慮使用 GPU 加速（如果支持）

2. **內存管理**：
   - 確保及時釋放圖像資源（已實現 `Dispose`）
   - 考慮使用對象池重用 Bitmap 對象

3. **配置管理**：
   - 將硬編碼參數移至配置文件
   - 支持命令行參數

4. **錯誤處理**：
   - 增強異常處理和日誌記錄
   - 提供更友好的錯誤提示

---

## 9. 已知問題與限制

### 9.1 已知問題
1. ~~**硬編碼路徑**~~：✅ 已解決 - 路徑現在自動從項目目錄獲取
2. **模型格式假設**：代碼假設輸出格式為 `[1, 5, 8400]`，可能不適用於所有模型
3. **置信度索引硬編碼**：假設置信度在索引 4，可能不適用於多類別模型
4. **DetectionBox 類未使用**：定義了但未在代碼中使用
5. **模型文件名固定**：當前固定使用 `sd900.onnx`，不支持動態切換模型
6. ~~**狀態指示功能不完整**~~：✅ 已解決 - 已實現完整的三種狀態指示（藍色_/綠色OK/紅色NG）

### 9.2 功能限制
1. 僅支持單張圖片處理，不支持批量處理
2. 不支持實時攝像頭檢測
3. 不支持模型切換
4. 不支持參數調整（置信度、IOU 閾值等）

---

## 9. 擴展建議

### 10.1 功能擴展
1. **批量處理**：支持文件夾批量處理
2. **實時檢測**：支持攝像頭實時檢測
3. **模型管理**：支持多模型切換和配置
4. **參數調整**：提供 UI 界面調整檢測參數
5. **結果導出**：支持 JSON、XML 等格式導出
6. **歷史記錄**：保存處理歷史和統計信息

### 10.2 技術改進
1. **配置系統**：使用 JSON 或 XML 配置文件
2. **日誌系統**：集成日誌框架（如 NLog、Serilog）
3. **單元測試**：添加單元測試覆蓋核心功能
4. **文檔完善**：添加 XML 註釋文檔

---

## 11. 代碼質量評估

### 11.1 優點
- ✅ 結構清晰，功能模塊劃分明確
- ✅ 使用多線程提升用戶體驗
- ✅ 實現了完整的檢測流程
- ✅ 包含錯誤處理機制

### 10.2 改進空間
- ✅ 路徑配置已改為自動獲取（已改進）
- ✅ 代碼註釋已完善，採用中英文並列格式（已改進）
- ✅ UI設計已現代化（已改進）
- ✅ 狀態指示功能已完整實現（已改進）
- ✅ 文件名包含時間戳記（已改進）
- ⚠️ 部分參數仍硬編碼（置信度閾值、IOU 閾值等），建議可配置化
- ⚠️ 異常處理可以更細緻
- ⚠️ 未使用的類和引用（DetectionBox、OpenCvSharp）

---

## 12. 版本信息

- **項目名稱**：CSharpYoloSimple
- **框架版本**：.NET 8.0-windows
- **ONNX Runtime**：1.23.2
- **OpenCvSharp4**：4.11.0.20250507
- **文檔版本**：1.2
- **最後更新**：2024
- **更新內容**：
  - ✅ 路徑自動獲取機制（模型和輸出文件夾）
  - ✅ 模型文件名改為 `sd900.onnx`
  - ✅ 輸出文件夾自動創建
  - ✅ 代碼註釋改為中英文並列格式
  - ✅ 現代化UI設計（扁平化設計、現代配色方案）
  - ✅ 智能狀態指示系統（三種狀態：藍色_/綠色OK/紅色NG）
  - ✅ 文件名包含時間戳記（年月日時分秒格式，yyyyMMddHHmmss）
  - ✅ 狀態文字顯示在狀態方框內部（居中顯示）

---

## 12. 參考資料

### 13.1 相關技術文檔
- [ONNX Runtime 官方文檔](https://onnxruntime.ai/docs/)
- [.NET 8.0 文檔](https://learn.microsoft.com/dotnet/)
- [Windows Forms 文檔](https://learn.microsoft.com/dotnet/desktop/winforms/)
- [YOLO 模型格式說明](https://github.com/ultralytics/ultralytics)

### 12.2 相關概念
- **YOLO**：You Only Look Once，實時目標檢測算法
- **ONNX**：Open Neural Network Exchange，開源深度學習模型格式
- **NMS**：Non-Maximum Suppression，非極大值抑制算法
- **IOU**：Intersection over Union，交並比，用於評估檢測框重疊度

---

## 附錄 A：關鍵代碼片段索引

| 功能 | 文件 | 行號範圍 |
|------|------|----------|
| 應用入口 | Program.cs | 15-20 |
| 路徑初始化 | Form1.cs | 49-77 |
| 模型加載 | Form1.cs | 81-110 |
| 圖像處理 | Form1.cs | 160-211 |
| 結果解析 | Form1.cs | 214-275 |
| NMS 去重 | Form1.cs | 447-474 |
| 可視化繪製 | Form1.cs | 284-343 |
| 文件保存（含時間戳記） | Form1.cs | 156-175 |
| 多線程處理 | Form1.cs | 514-635 |
| UI 交互 | Form1.cs | 699-720 |
| 狀態更新 | Form1.cs | 659-710 |

---

**文檔結束**
