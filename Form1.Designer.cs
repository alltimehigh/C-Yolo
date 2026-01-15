namespace test
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            textBox1 = new TextBox();
            button1 = new Button();
            openFileDialog1 = new OpenFileDialog();
            labelTime = new Label();
            panel1 = new Panel();
            labelTitle = new Label();
            labelFilePath = new Label();
            labelStatus = new Label();
            labelTimeTitle = new Label();
            panelImageContainer = new Panel();
            labelStatusText = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panelImageContainer.SuspendLayout();
            SuspendLayout();
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            labelTitle.ForeColor = Color.FromArgb(33, 33, 33);
            labelTitle.Location = new Point(24, 20);
            labelTitle.Margin = new Padding(4, 0, 4, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(216, 31);
            labelTitle.TabIndex = 5;
            labelTitle.Text = "YOLO 目標檢測系統";
            // 
            // labelFilePath
            // 
            labelFilePath.AutoSize = true;
            labelFilePath.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelFilePath.ForeColor = Color.FromArgb(102, 102, 102);
            labelFilePath.Location = new Point(24, 80);
            labelFilePath.Margin = new Padding(4, 0, 4, 0);
            labelFilePath.Name = "labelFilePath";
            labelFilePath.Size = new Size(68, 17);
            labelFilePath.TabIndex = 6;
            labelFilePath.Text = "圖片路徑：";
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.White;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.ForeColor = Color.FromArgb(51, 51, 51);
            textBox1.Location = new Point(140, 76);
            textBox1.Margin = new Padding(4);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(520, 23);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(0, 120, 215);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            button1.ForeColor = Color.White;
            button1.Location = new Point(680, 74);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(120, 32);
            button1.TabIndex = 2;
            button1.Text = "選擇圖片";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // panelImageContainer
            // 
            panelImageContainer.BackColor = Color.FromArgb(245, 245, 245);
            panelImageContainer.BorderStyle = BorderStyle.FixedSingle;
            panelImageContainer.Controls.Add(pictureBox1);
            panelImageContainer.Location = new Point(24, 120);
            panelImageContainer.Margin = new Padding(4);
            panelImageContainer.Name = "panelImageContainer";
            panelImageContainer.Padding = new Padding(8);
            panelImageContainer.Size = new Size(776, 480);
            panelImageContainer.TabIndex = 7;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.BorderStyle = BorderStyle.None;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(8, 8);
            pictureBox1.Margin = new Padding(4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(766, 470);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelStatus.ForeColor = Color.FromArgb(102, 102, 102);
            labelStatus.Location = new Point(820, 120);
            labelStatus.Margin = new Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(68, 17);
            labelStatus.TabIndex = 8;
            labelStatus.Text = "檢測狀態：";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(220, 53, 69);
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(labelStatusText);
            panel1.Location = new Point(820, 145);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new Size(80, 80);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // labelStatusText
            // 
            labelStatusText.Anchor = AnchorStyles.None;
            labelStatusText.AutoSize = false;
            labelStatusText.Dock = DockStyle.Fill;
            labelStatusText.Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            labelStatusText.ForeColor = Color.White;
            labelStatusText.Location = new Point(0, 0);
            labelStatusText.Margin = new Padding(0);
            labelStatusText.Name = "labelStatusText";
            labelStatusText.Size = new Size(78, 78);
            labelStatusText.TabIndex = 10;
            labelStatusText.Text = "OK";
            labelStatusText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTimeTitle
            // 
            labelTimeTitle.AutoSize = true;
            labelTimeTitle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            labelTimeTitle.ForeColor = Color.FromArgb(102, 102, 102);
            labelTimeTitle.Location = new Point(820, 250);
            labelTimeTitle.Margin = new Padding(4, 0, 4, 0);
            labelTimeTitle.Name = "labelTimeTitle";
            labelTimeTitle.Size = new Size(68, 17);
            labelTimeTitle.TabIndex = 9;
            labelTimeTitle.Text = "當前時間：";
            // 
            // labelTime
            // 
            labelTime.AutoSize = true;
            labelTime.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            labelTime.ForeColor = Color.FromArgb(33, 33, 33);
            labelTime.Location = new Point(820, 275);
            labelTime.Margin = new Padding(4, 0, 4, 0);
            labelTime.Name = "labelTime";
            labelTime.Size = new Size(82, 26);
            labelTime.TabIndex = 3;
            labelTime.Text = "00:00:00";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "圖片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*";
            openFileDialog1.FileOk += openFileDialog1_FileOk;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 640);
            Controls.Add(labelTime);
            Controls.Add(labelTimeTitle);
            Controls.Add(panel1);
            Controls.Add(labelStatus);
            Controls.Add(panelImageContainer);
            Controls.Add(labelFilePath);
            Controls.Add(labelTitle);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "Form1";
            Padding = new Padding(20);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "YOLO 目標檢測系統";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panelImageContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label labelTime;
        private Panel panel1;
        private Label labelTitle;
        private Label labelFilePath;
        private Label labelStatus;
        private Label labelTimeTitle;
        private Panel panelImageContainer;
        private Label labelStatusText;
    }
}

