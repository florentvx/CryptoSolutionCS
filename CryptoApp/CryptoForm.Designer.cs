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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Allocation = new System.Windows.Forms.TabPage();
            this.dataGridViewAllocation = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.Graphs = new System.Windows.Forms.TabPage();
            this.dateSelectorControlGraph = new CustomControls.DateSelectorControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonShow = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dateSelectorControl1 = new CustomControls.DateSelectorControl();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridViewPnL = new System.Windows.Forms.DataGridView();
            this.TxExplorer = new System.Windows.Forms.TabPage();
            this.dataGridViewTxExplorer = new System.Windows.Forms.DataGridView();
            this.labelTxExplorer = new System.Windows.Forms.Label();
            this.OpenOrders = new System.Windows.Forms.TabPage();
            this.dataGridViewOpenSellOrders = new System.Windows.Forms.DataGridView();
            this.dataGridViewOpenBuyOrders = new System.Windows.Forms.DataGridView();
            this.buttonOpenOrdersShow = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxCcy2 = new System.Windows.Forms.ComboBox();
            this.comboBoxCcy1 = new System.Windows.Forms.ComboBox();
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
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPnL)).BeginInit();
            this.TxExplorer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTxExplorer)).BeginInit();
            this.OpenOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenSellOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenBuyOrders)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.Allocation);
            this.tabControl1.Controls.Add(this.Graphs);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.TxExplorer);
            this.tabControl1.Controls.Add(this.OpenOrders);
            this.tabControl1.Location = new System.Drawing.Point(12, 59);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1278, 590);
            this.tabControl1.TabIndex = 1;
            // 
            // Allocation
            // 
            this.Allocation.Controls.Add(this.dataGridViewAllocation);
            this.Allocation.Controls.Add(this.label3);
            this.Allocation.Location = new System.Drawing.Point(4, 25);
            this.Allocation.Name = "Allocation";
            this.Allocation.Padding = new System.Windows.Forms.Padding(3);
            this.Allocation.Size = new System.Drawing.Size(1270, 561);
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
            this.dataGridViewAllocation.RowHeadersWidth = 51;
            this.dataGridViewAllocation.RowTemplate.Height = 24;
            this.dataGridViewAllocation.Size = new System.Drawing.Size(1255, 532);
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
            this.Graphs.Controls.Add(this.dateSelectorControlGraph);
            this.Graphs.Controls.Add(this.groupBox1);
            this.Graphs.Controls.Add(this.chart1);
            this.Graphs.Location = new System.Drawing.Point(4, 25);
            this.Graphs.Name = "Graphs";
            this.Graphs.Padding = new System.Windows.Forms.Padding(3);
            this.Graphs.Size = new System.Drawing.Size(1270, 561);
            this.Graphs.TabIndex = 0;
            this.Graphs.Text = "Graphs";
            this.Graphs.UseVisualStyleBackColor = true;
            // 
            // dateSelectorControlGraph
            // 
            this.dateSelectorControlGraph.Location = new System.Drawing.Point(6, 0);
            this.dateSelectorControlGraph.Name = "dateSelectorControlGraph";
            this.dateSelectorControlGraph.Size = new System.Drawing.Size(233, 84);
            this.dateSelectorControlGraph.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonShow);
            this.groupBox1.Controls.Add(this.checkedListBox1);
            this.groupBox1.Location = new System.Drawing.Point(11, 90);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 261);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Graphs";
            // 
            // buttonShow
            // 
            this.buttonShow.Location = new System.Drawing.Point(6, 218);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(209, 37);
            this.buttonShow.TabIndex = 2;
            this.buttonShow.Text = "Show";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.ButtonShow_Click);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(6, 21);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(209, 191);
            this.checkedListBox1.TabIndex = 1;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea6.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea6);
            legend6.Name = "Legend1";
            this.chart1.Legends.Add(legend6);
            this.chart1.Location = new System.Drawing.Point(237, 6);
            this.chart1.Name = "chart1";
            series6.ChartArea = "ChartArea1";
            series6.Legend = "Legend1";
            series6.Name = "Series1";
            this.chart1.Series.Add(series6);
            this.chart1.Size = new System.Drawing.Size(1027, 549);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dateSelectorControl1);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.dataGridViewPnL);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1270, 561);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "PnL Explain";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dateSelectorControl1
            // 
            this.dateSelectorControl1.Location = new System.Drawing.Point(163, 0);
            this.dateSelectorControl1.Name = "dateSelectorControl1";
            this.dateSelectorControl1.Size = new System.Drawing.Size(297, 81);
            this.dateSelectorControl1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 31);
            this.button1.TabIndex = 1;
            this.button1.Text = "Calculate PnL";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ButtonCalculatePnL_Click);
            // 
            // dataGridViewPnL
            // 
            this.dataGridViewPnL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewPnL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPnL.Location = new System.Drawing.Point(7, 87);
            this.dataGridViewPnL.Name = "dataGridViewPnL";
            this.dataGridViewPnL.RowHeadersWidth = 51;
            this.dataGridViewPnL.RowTemplate.Height = 24;
            this.dataGridViewPnL.Size = new System.Drawing.Size(1257, 467);
            this.dataGridViewPnL.TabIndex = 0;
            // 
            // TxExplorer
            // 
            this.TxExplorer.Controls.Add(this.dataGridViewTxExplorer);
            this.TxExplorer.Controls.Add(this.labelTxExplorer);
            this.TxExplorer.Location = new System.Drawing.Point(4, 25);
            this.TxExplorer.Name = "TxExplorer";
            this.TxExplorer.Padding = new System.Windows.Forms.Padding(3);
            this.TxExplorer.Size = new System.Drawing.Size(1270, 561);
            this.TxExplorer.TabIndex = 3;
            this.TxExplorer.Text = "TxExplorer";
            this.TxExplorer.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTxExplorer
            // 
            this.dataGridViewTxExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTxExplorer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTxExplorer.Location = new System.Drawing.Point(6, 23);
            this.dataGridViewTxExplorer.Name = "dataGridViewTxExplorer";
            this.dataGridViewTxExplorer.RowHeadersWidth = 51;
            this.dataGridViewTxExplorer.RowTemplate.Height = 24;
            this.dataGridViewTxExplorer.Size = new System.Drawing.Size(1258, 532);
            this.dataGridViewTxExplorer.TabIndex = 1;
            // 
            // labelTxExplorer
            // 
            this.labelTxExplorer.AutoSize = true;
            this.labelTxExplorer.Location = new System.Drawing.Point(6, 3);
            this.labelTxExplorer.Name = "labelTxExplorer";
            this.labelTxExplorer.Size = new System.Drawing.Size(79, 17);
            this.labelTxExplorer.TabIndex = 0;
            this.labelTxExplorer.Text = "Tx Explorer";
            // 
            // OpenOrders
            // 
            this.OpenOrders.Controls.Add(this.dataGridViewOpenSellOrders);
            this.OpenOrders.Controls.Add(this.dataGridViewOpenBuyOrders);
            this.OpenOrders.Controls.Add(this.buttonOpenOrdersShow);
            this.OpenOrders.Controls.Add(this.label5);
            this.OpenOrders.Controls.Add(this.label4);
            this.OpenOrders.Controls.Add(this.comboBoxCcy2);
            this.OpenOrders.Controls.Add(this.comboBoxCcy1);
            this.OpenOrders.Location = new System.Drawing.Point(4, 25);
            this.OpenOrders.Name = "OpenOrders";
            this.OpenOrders.Size = new System.Drawing.Size(1270, 561);
            this.OpenOrders.TabIndex = 4;
            this.OpenOrders.Text = "OpenOrders";
            this.OpenOrders.UseVisualStyleBackColor = true;
            // 
            // dataGridViewOpenSellOrders
            // 
            this.dataGridViewOpenSellOrders.AllowUserToAddRows = false;
            this.dataGridViewOpenSellOrders.AllowUserToDeleteRows = false;
            this.dataGridViewOpenSellOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOpenSellOrders.Location = new System.Drawing.Point(651, 56);
            this.dataGridViewOpenSellOrders.Name = "dataGridViewOpenSellOrders";
            this.dataGridViewOpenSellOrders.ReadOnly = true;
            this.dataGridViewOpenSellOrders.RowHeadersVisible = false;
            this.dataGridViewOpenSellOrders.RowHeadersWidth = 51;
            this.dataGridViewOpenSellOrders.RowTemplate.Height = 24;
            this.dataGridViewOpenSellOrders.Size = new System.Drawing.Size(590, 502);
            this.dataGridViewOpenSellOrders.TabIndex = 6;
            // 
            // dataGridViewOpenBuyOrders
            // 
            this.dataGridViewOpenBuyOrders.AllowUserToAddRows = false;
            this.dataGridViewOpenBuyOrders.AllowUserToDeleteRows = false;
            this.dataGridViewOpenBuyOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOpenBuyOrders.Location = new System.Drawing.Point(30, 56);
            this.dataGridViewOpenBuyOrders.Name = "dataGridViewOpenBuyOrders";
            this.dataGridViewOpenBuyOrders.ReadOnly = true;
            this.dataGridViewOpenBuyOrders.RowHeadersVisible = false;
            this.dataGridViewOpenBuyOrders.RowHeadersWidth = 51;
            this.dataGridViewOpenBuyOrders.RowTemplate.Height = 24;
            this.dataGridViewOpenBuyOrders.Size = new System.Drawing.Size(615, 502);
            this.dataGridViewOpenBuyOrders.TabIndex = 5;
            // 
            // buttonOpenOrdersShow
            // 
            this.buttonOpenOrdersShow.Location = new System.Drawing.Point(323, 12);
            this.buttonOpenOrdersShow.Name = "buttonOpenOrdersShow";
            this.buttonOpenOrdersShow.Size = new System.Drawing.Size(96, 26);
            this.buttonOpenOrdersShow.TabIndex = 4;
            this.buttonOpenOrdersShow.Text = "Show";
            this.buttonOpenOrdersShow.UseVisualStyleBackColor = true;
            this.buttonOpenOrdersShow.Click += new System.EventHandler(this.ButtonOpenOrdersShow_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(169, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 17);
            this.label5.TabIndex = 3;
            this.label5.Text = "Ccy2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Ccy1";
            // 
            // comboBoxCcy2
            // 
            this.comboBoxCcy2.FormattingEnabled = true;
            this.comboBoxCcy2.ItemHeight = 16;
            this.comboBoxCcy2.Location = new System.Drawing.Point(214, 14);
            this.comboBoxCcy2.Name = "comboBoxCcy2";
            this.comboBoxCcy2.Size = new System.Drawing.Size(90, 24);
            this.comboBoxCcy2.TabIndex = 1;
            // 
            // comboBoxCcy1
            // 
            this.comboBoxCcy1.FormattingEnabled = true;
            this.comboBoxCcy1.ItemHeight = 16;
            this.comboBoxCcy1.Location = new System.Drawing.Point(58, 14);
            this.comboBoxCcy1.Name = "comboBoxCcy1";
            this.comboBoxCcy1.Size = new System.Drawing.Size(90, 24);
            this.comboBoxCcy1.TabIndex = 0;
            // 
            // richTextBoxLogger
            // 
            this.richTextBoxLogger.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.richTextBoxLogger.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBoxLogger.ForeColor = System.Drawing.SystemColors.Info;
            this.richTextBoxLogger.Location = new System.Drawing.Point(0, 663);
            this.richTextBoxLogger.Name = "richTextBoxLogger";
            this.richTextBoxLogger.ReadOnly = true;
            this.richTextBoxLogger.Size = new System.Drawing.Size(1293, 130);
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
            this.ClientSize = new System.Drawing.Size(1293, 793);
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
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPnL)).EndInit();
            this.TxExplorer.ResumeLayout(false);
            this.TxExplorer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTxExplorer)).EndInit();
            this.OpenOrders.ResumeLayout(false);
            this.OpenOrders.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenSellOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOpenBuyOrders)).EndInit();
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
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dataGridViewPnL;
        private System.Windows.Forms.Button button1;
        private CustomControls.DateSelectorControl dateSelectorControl1;
        private CustomControls.DateSelectorControl dateSelectorControlGraph;
        private System.Windows.Forms.TabPage TxExplorer;
        private System.Windows.Forms.Label labelTxExplorer;
        private System.Windows.Forms.DataGridView dataGridViewTxExplorer;
        private System.Windows.Forms.TabPage OpenOrders;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxCcy2;
        private System.Windows.Forms.ComboBox comboBoxCcy1;
        private System.Windows.Forms.Button buttonOpenOrdersShow;
        private System.Windows.Forms.DataGridView dataGridViewOpenSellOrders;
        private System.Windows.Forms.DataGridView dataGridViewOpenBuyOrders;
    }
}

