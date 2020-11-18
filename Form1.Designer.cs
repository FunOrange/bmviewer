namespace bmviewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.openButton = new System.Windows.Forms.Button();
            this.timeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.skControl = new SkiaSharp.Views.Desktop.SKControl();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.restartButton = new System.Windows.Forms.Button();
            this.playPauseButton = new System.Windows.Forms.Button();
            this.aimStrainPlot = new ScottPlot.FormsPlot();
            this.aimStrainMeter = new ScottPlot.FormsPlot();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.sortedPeaksPlot = new ScottPlot.FormsPlot();
            ((System.ComponentModel.ISupportInitialize)(this.timeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(12, 12);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(86, 23);
            this.openButton.TabIndex = 1;
            this.openButton.Text = "Open...";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // timeUpDown
            // 
            this.timeUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.timeUpDown.Location = new System.Drawing.Point(12, 78);
            this.timeUpDown.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.timeUpDown.Name = "timeUpDown";
            this.timeUpDown.Size = new System.Drawing.Size(86, 20);
            this.timeUpDown.TabIndex = 3;
            this.timeUpDown.ValueChanged += new System.EventHandler(this.timeUpDown_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "time (ms)";
            // 
            // skControl
            // 
            this.skControl.BackColor = System.Drawing.Color.Black;
            this.skControl.Location = new System.Drawing.Point(104, 12);
            this.skControl.Name = "skControl";
            this.skControl.Size = new System.Drawing.Size(640, 480);
            this.skControl.TabIndex = 6;
            this.skControl.Text = "skControl";
            this.skControl.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs>(this.skControl_PaintSurface);
            // 
            // gameTimer
            // 
            this.gameTimer.Enabled = true;
            this.gameTimer.Interval = 6;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            // 
            // restartButton
            // 
            this.restartButton.Location = new System.Drawing.Point(12, 133);
            this.restartButton.Name = "restartButton";
            this.restartButton.Size = new System.Drawing.Size(86, 23);
            this.restartButton.TabIndex = 7;
            this.restartButton.Text = "Restart";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // playPauseButton
            // 
            this.playPauseButton.Location = new System.Drawing.Point(12, 104);
            this.playPauseButton.Name = "playPauseButton";
            this.playPauseButton.Size = new System.Drawing.Size(86, 23);
            this.playPauseButton.TabIndex = 7;
            this.playPauseButton.Text = "Play/Pause";
            this.playPauseButton.UseVisualStyleBackColor = true;
            this.playPauseButton.Click += new System.EventHandler(this.playPauseButton_Click);
            // 
            // aimStrainPlot
            // 
            this.aimStrainPlot.Location = new System.Drawing.Point(750, 12);
            this.aimStrainPlot.Name = "aimStrainPlot";
            this.aimStrainPlot.Size = new System.Drawing.Size(467, 278);
            this.aimStrainPlot.TabIndex = 8;
            // 
            // aimStrainMeter
            // 
            this.aimStrainMeter.Location = new System.Drawing.Point(1223, 12);
            this.aimStrainMeter.Name = "aimStrainMeter";
            this.aimStrainMeter.Size = new System.Drawing.Size(108, 278);
            this.aimStrainMeter.TabIndex = 9;
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 100;
            this.trackBar1.Location = new System.Drawing.Point(104, 498);
            this.trackBar1.Maximum = 100000;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(640, 45);
            this.trackBar1.SmallChange = 10;
            this.trackBar1.TabIndex = 10;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // sortedPeaksPlot
            // 
            this.sortedPeaksPlot.Location = new System.Drawing.Point(750, 296);
            this.sortedPeaksPlot.Name = "sortedPeaksPlot";
            this.sortedPeaksPlot.Size = new System.Drawing.Size(581, 196);
            this.sortedPeaksPlot.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1337, 534);
            this.Controls.Add(this.sortedPeaksPlot);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.aimStrainMeter);
            this.Controls.Add(this.aimStrainPlot);
            this.Controls.Add(this.playPauseButton);
            this.Controls.Add(this.restartButton);
            this.Controls.Add(this.skControl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.timeUpDown);
            this.Controls.Add(this.openButton);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "bmviewer";
            ((System.ComponentModel.ISupportInitialize)(this.timeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.NumericUpDown timeUpDown;
        private System.Windows.Forms.Label label1;
        private SkiaSharp.Views.Desktop.SKControl skControl;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Button restartButton;
        private System.Windows.Forms.Button playPauseButton;
        private ScottPlot.FormsPlot aimStrainPlot;
        private ScottPlot.FormsPlot aimStrainMeter;
        private System.Windows.Forms.TrackBar trackBar1;
        private ScottPlot.FormsPlot sortedPeaksPlot;
    }
}

