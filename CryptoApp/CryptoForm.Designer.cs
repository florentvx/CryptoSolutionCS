namespace CryptoApp
{
    partial class CryptoForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Allocation = new System.Windows.Forms.TabPage();
            this.dataGridViewAllocation = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.Graphs = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonShow = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.richTextBoxLogger = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxFiat = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxFrequency = new System.Windows.Forms.ComboBox();
            this.buttonFullDataUpdate = new System.Windows.Forms.Button();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.ButtonLedger = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.Allocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAllocation)).BeginInit();
            this.Graphs.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.Allocation);
            this.tabControl1.Controls.Add(this.Graphs);
            this.tabControl1.Location = new System.Drawing.Point(12, 59);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1159, 515);
            this.tabControl1.TabIndex = 1;
            // 
            // Allocation
            // 
            this.Allocation.Controls.Add(this.dataGridViewAllocation);
            this.Allocation.Controls.Add(this.label3);
            this.Allocation.Location = new System.Drawing.Point(4, 25);
            this.Allocation.Name = "Allocation";
            this.Allocation.Padding = new System.Windows.Forms.Padding(3);
            this.Allocation.Size = new System.Drawing.Size(1151, 486);
            this.Allocation.TabIndex = 1;
            this.Allocation.Text = "Allocation";
            this.Allocation.UseVisualStyleBackColor = true;
            // 
            // dataGridViewAllocation
            // 
            this.dataGridViewAllocation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewAllocation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAllocation.Location = new System.Drawing.Point(9, 23);
            this.dataGridViewAllocation.Name = "dataGridViewAllocation";
            this.dataGridViewAllocation.RowTemplate.Height = 24;
            this.dataGridViewAllocation.Size = new System.Drawing.Size(1146, 457);
            this.dataGridViewAllocation.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Last Allocation";
            // 
            // Graphs
            // 
            this.Graphs.Controls.Add(this.groupBox1);
            this.Graphs.Controls.Add(this.chart1);
            this.Graphs.Location = new System.Drawing.Point(4, 25);
            this.Graphs.Name = "Graphs";
            this.Graphs.Padding = new System.Windows.Forms.Padding(3);
            this.Graphs.Size = new System.Drawing.Size(1151, 486);
            this.Graphs.TabIndex = 0;
            this.Graphs.Text = "Graphs";
            this.Graphs.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.buttonShow);
            this.groupBox1.Controls.Add(this.checkedListBox1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(189, 475);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Graphs";
            // 
            // buttonShow
            // 
            this.buttonShow.Location = new System.Drawing.Point(6, 399);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(176, 37);
            this.buttonShow.TabIndex = 2;
            this.buttonShow.Text = "Show";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.ButtonShow_Click);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(6, 218);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(177, 140);
            this.checkedListBox1.TabIndex = 1;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.chart1.Legends.Add(legend2);
            this.chart1.Location = new System.Drawing.Point(235, 6);
            this.chart1.Name = "chart1";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(910, 465);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // richTextBoxLogger
            // 
            this.richTextBoxLogger.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxLogger.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.richTextBoxLogger.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLogger.Location = new System.Drawing.Point(16, 576);
            this.richTextBoxLogger.Name = "richTextBoxLogger";
            this.richTextBoxLogger.ReadOnly = true;
            this.richTextBoxLogger.Size = new System.Drawing.Size(1155, 109);
            this.richTextBoxLogger.TabIndex = 4;
            this.richTextBoxLogger.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Fiat Currency";
            // 
            // comboBoxFiat
            // 
            this.comboBoxFiat.FormattingEnabled = true;
            this.comboBoxFiat.Location = new System.Drawing.Point(12, 29);
            this.comboBoxFiat.Name = "comboBoxFiat";
            this.comboBoxFiat.Size = new System.Drawing.Size(177, 24);
            this.comboBoxFiat.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(192, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Frequency";
            // 
            // comboBoxFrequency
            // 
            this.comboBoxFrequency.FormattingEnabled = true;
            this.comboBoxFrequency.Location = new System.Drawing.Point(195, 29);
            this.comboBoxFrequency.Name = "comboBoxFrequency";
            this.comboBoxFrequency.Size = new System.Drawing.Size(177, 24);
            this.comboBoxFrequency.TabIndex = 5;
            // 
            // buttonFullDataUpdate
            // 
            this.buttonFullDataUpdate.Location = new System.Drawing.Point(511, 29);
            this.buttonFullDataUpdate.Name = "buttonFullDataUpdate";
            this.buttonFullDataUpdate.Size = new System.Drawing.Size(127, 24);
            this.buttonFullDataUpdate.TabIndex = 6;
            this.buttonFullDataUpdate.Text = "Full Data Update";
            this.buttonFullDataUpdate.UseVisualStyleBackColor = true;
            this.buttonFullDataUpdate.Click += new System.EventHandler(this.ButtonFullUpdate_Click);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(378, 29);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(127, 24);
            this.buttonLoad.TabIndex = 7;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
            // 
            // ButtonLedger
            // 
            this.ButtonLedger.Location = new System.Drawing.Point(644, 29);
            this.ButtonLedger.Name = "ButtonLedger";
            this.ButtonLedger.Size = new System.Drawing.Size(127, 24);
            this.ButtonLedger.TabIndex = 8;
            this.ButtonLedger.Text = "Ledger Update";
            this.ButtonLedger.UseVisualStyleBackColor = true;
            this.ButtonLedger.Click += new System.EventHandler(this.ButtonLedger_Click);
            // 
            // CryptoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1183, 697);
            this.Controls.Add(this.ButtonLedger);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.richTextBoxLogger);
            this.Controls.Add(this.buttonFullDataUpdate);
            this.Controls.Add(this.comboBoxFrequency);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.comboBoxFiat);
            this.Name = "CryptoForm";
            this.Text = "CryptoForm";
            this.tabControl1.ResumeLayout(false);
            this.Allocation.ResumeLayout(false);
            this.Allocation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAllocation)).EndInit();
            this.Graphs.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Graphs;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TabPage Allocation;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBoxFiat;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button buttonShow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxFrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonFullDataUpdate;
        private System.Windows.Forms.DataGridView dataGridViewAllocation;
        private System.Windows.Forms.RichTextBox richTextBoxLogger;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button ButtonLedger;
    }
}

