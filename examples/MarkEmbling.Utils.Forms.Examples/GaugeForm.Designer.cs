namespace MarkEmbling.Utils.Forms.Examples {
    partial class GaugeForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            MarkEmbling.Utils.Forms.Controls.GaugeRange gaugeRange1 = new MarkEmbling.Utils.Forms.Controls.GaugeRange();
            MarkEmbling.Utils.Forms.Controls.GaugeRange gaugeRange2 = new MarkEmbling.Utils.Forms.Controls.GaugeRange();
            this.gauge1 = new MarkEmbling.Utils.Forms.Controls.Gauge();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.valueLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // gauge1
            // 
            this.gauge1.BaseArcColour = System.Drawing.Color.Gray;
            this.gauge1.BaseArcRadius = 80;
            this.gauge1.BaseArcStart = 135;
            this.gauge1.BaseArcSweep = 270;
            this.gauge1.BaseArcWidth = 2;
            this.gauge1.Center = new System.Drawing.Point(100, 100);
            gaugeRange1.Colour = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            gaugeRange1.EndValue = -40F;
            gaugeRange1.InnerRadius = 60;
            gaugeRange1.InRange = false;
            gaugeRange1.Name = "GaugeRange1";
            gaugeRange1.OuterRadius = 80;
            gaugeRange1.StartValue = -100F;
            gaugeRange2.Colour = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            gaugeRange2.EndValue = 100F;
            gaugeRange2.InnerRadius = 60;
            gaugeRange2.InRange = false;
            gaugeRange2.Name = "GaugeRange2";
            gaugeRange2.OuterRadius = 80;
            gaugeRange2.StartValue = 40F;
            this.gauge1.GaugeRanges.Add(gaugeRange1);
            this.gauge1.GaugeRanges.Add(gaugeRange2);
            this.gauge1.Location = new System.Drawing.Point(12, 12);
            this.gauge1.MaxValue = 100F;
            this.gauge1.MinValue = -100F;
            this.gauge1.Name = "gauge1";
            this.gauge1.NeedleColor1 = System.Drawing.Color.Gray;
            this.gauge1.NeedleColor2 = System.Drawing.Color.DimGray;
            this.gauge1.NeedleRadius = 80;
            this.gauge1.NeedleType = MarkEmbling.Utils.Forms.Controls.NeedleType.Simple;
            this.gauge1.NeedleWidth = 2;
            this.gauge1.ScaleLinesInterColor = System.Drawing.Color.DimGray;
            this.gauge1.ScaleLinesInterInnerRadius = 73;
            this.gauge1.ScaleLinesInterOuterRadius = 80;
            this.gauge1.ScaleLinesInterWidth = 1;
            this.gauge1.ScaleLinesMajorColor = System.Drawing.Color.DimGray;
            this.gauge1.ScaleLinesMajorInnerRadius = 70;
            this.gauge1.ScaleLinesMajorOuterRadius = 80;
            this.gauge1.ScaleLinesMajorStepValue = 50F;
            this.gauge1.ScaleLinesMajorWidth = 2;
            this.gauge1.ScaleLinesMinorColor = System.Drawing.Color.Gray;
            this.gauge1.ScaleLinesMinorInnerRadius = 75;
            this.gauge1.ScaleLinesMinorOuterRadius = 80;
            this.gauge1.ScaleLinesMinorTicks = 9;
            this.gauge1.ScaleLinesMinorWidth = 1;
            this.gauge1.ScaleNumbersColor = System.Drawing.Color.Black;
            this.gauge1.ScaleNumbersFormat = null;
            this.gauge1.ScaleNumbersRadius = 95;
            this.gauge1.ScaleNumbersRotation = 0;
            this.gauge1.ScaleNumbersStartScaleLine = 0;
            this.gauge1.ScaleNumbersStepScaleLines = 1;
            this.gauge1.Size = new System.Drawing.Size(205, 180);
            this.gauge1.TabIndex = 0;
            this.gauge1.Text = "gauge1";
            this.gauge1.Value = 0F;
            this.gauge1.ValueChanged += new System.EventHandler(this.gauge1_ValueChanged);
            this.gauge1.ValueInRangeChanged += new System.EventHandler<MarkEmbling.Utils.Forms.Controls.ValueInRangeChangedEventArgs>(this.gauge1_ValueInRangeChanged);
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 50;
            this.trackBar1.Location = new System.Drawing.Point(12, 198);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Minimum = -100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(205, 45);
            this.trackBar1.SmallChange = 5;
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // valueLabel
            // 
            this.valueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.valueLabel.Location = new System.Drawing.Point(12, 234);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(205, 24);
            this.valueLabel.TabIndex = 2;
            this.valueLabel.Text = "0";
            this.valueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GaugeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 267);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.gauge1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GaugeForm";
            this.Text = "Gauge";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.Gauge gauge1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label valueLabel;
    }
}