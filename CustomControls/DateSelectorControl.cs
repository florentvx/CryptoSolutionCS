using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Date;

namespace CustomControls
{
    public partial class DateSelectorControl : UserControl
    {
        private Frequency ControlFreq = Frequency.Day1;

        public DateSelectorControl(string main_title) // default title: Date Selector
        {
            InitializeComponent();
            InputTenorCheckBox.Checked = true;
            this.groupBox1.Text = main_title;
        }

        public void SetInitialInput(string text)
        {
            TenorTextBox.Text = text;
            InputTenorCheckBox_CheckedChanged(null, null);
            TenorTextBox_TextChanged(null, null);
        }

        public DateTime Date { get { return DateTimePicker.Value; } }

        private void InputTenorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TenorTextBox.ReadOnly = !InputTenorCheckBox.Checked;
            DateTimePicker.Enabled = !InputTenorCheckBox.Checked;
        }

        private void TenorTextBox_TextChanged(object sender, EventArgs e)
        {
            Tenor tnr = new Tenor(TenorTextBox.Text);
            if (tnr.IsTenor)
                DateTimePicker.Value = ControlFreq.Adjust(DateTime.UtcNow.AddTenor("-" + TenorTextBox.Text));
        }
    }
}
