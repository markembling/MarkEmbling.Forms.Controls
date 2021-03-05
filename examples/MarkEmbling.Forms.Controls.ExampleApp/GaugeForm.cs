using MarkEmbling.Utils.Forms.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarkEmbling.Forms.Controls.ExampleApp
{
    public partial class GaugeForm : Form {
        public GaugeForm() {
            InitializeComponent();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            gauge1.Value = trackBar1.Value;
        }

        private void gauge1_ValueChanged(object sender, EventArgs e) {
           valueLabel.Text = gauge1.Value.ToString();
        }

        private void gauge1_ValueInRangeChanged(object sender, ValueInRangeChangedEventArgs e) {
            valueLabel.BackColor = e.InRange ? 
                e.Range.Colour : 
                Color.FromKnownColor(KnownColor.Control);
        }
    }
}
