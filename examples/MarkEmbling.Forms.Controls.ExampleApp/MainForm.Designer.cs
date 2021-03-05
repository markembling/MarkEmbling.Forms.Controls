
namespace MarkEmbling.Forms.Controls.ExampleApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.closeButton = new System.Windows.Forms.Button();
            this.viewButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.examplesDropDown = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(484, 115);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(150, 46);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // viewButton
            // 
            this.viewButton.Location = new System.Drawing.Point(328, 115);
            this.viewButton.Name = "viewButton";
            this.viewButton.Size = new System.Drawing.Size(150, 46);
            this.viewButton.TabIndex = 0;
            this.viewButton.Text = "View";
            this.viewButton.UseVisualStyleBackColor = true;
            this.viewButton.Click += new System.EventHandler(this.viewButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(229, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "Choose an example:";
            // 
            // examplesDropDown
            // 
            this.examplesDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.examplesDropDown.FormattingEnabled = true;
            this.examplesDropDown.Items.AddRange(new object[] {
            "DragDropTreeView",
            "Gauge"});
            this.examplesDropDown.Location = new System.Drawing.Point(12, 54);
            this.examplesDropDown.Name = "examplesDropDown";
            this.examplesDropDown.Size = new System.Drawing.Size(622, 40);
            this.examplesDropDown.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 173);
            this.Controls.Add(this.examplesDropDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.viewButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Examples";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button viewButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox examplesDropDown;
    }
}

