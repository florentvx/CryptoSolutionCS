namespace CustomControls
{
    partial class DateSelectorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TenorTextBox = new System.Windows.Forms.TextBox();
            this.DateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.InputTenorCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TenorTextBox
            // 
            this.TenorTextBox.Location = new System.Drawing.Point(113, 21);
            this.TenorTextBox.Name = "TenorTextBox";
            this.TenorTextBox.Size = new System.Drawing.Size(103, 22);
            this.TenorTextBox.TabIndex = 1;
            this.TenorTextBox.TextChanged += new System.EventHandler(this.TenorTextBox_TextChanged);
            // 
            // DateTimePicker
            // 
            this.DateTimePicker.Location = new System.Drawing.Point(6, 47);
            this.DateTimePicker.Name = "DateTimePicker";
            this.DateTimePicker.Size = new System.Drawing.Size(210, 22);
            this.DateTimePicker.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DateTimePicker);
            this.groupBox1.Controls.Add(this.TenorTextBox);
            this.groupBox1.Controls.Add(this.InputTenorCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 75);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Date Selector";
            // 
            // InputTenorCheckBox
            // 
            this.InputTenorCheckBox.AutoSize = true;
            this.InputTenorCheckBox.Location = new System.Drawing.Point(6, 23);
            this.InputTenorCheckBox.Name = "InputTenorCheckBox";
            this.InputTenorCheckBox.Size = new System.Drawing.Size(103, 21);
            this.InputTenorCheckBox.TabIndex = 0;
            this.InputTenorCheckBox.Text = "Input Tenor";
            this.InputTenorCheckBox.UseVisualStyleBackColor = true;
            this.InputTenorCheckBox.CheckedChanged += new System.EventHandler(this.InputTenorCheckBox_CheckedChanged);
            // 
            // DateSelectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "DateSelectorControl";
            this.Size = new System.Drawing.Size(232, 81);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox TenorTextBox;
        private System.Windows.Forms.DateTimePicker DateTimePicker;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox InputTenorCheckBox;
    }
}
