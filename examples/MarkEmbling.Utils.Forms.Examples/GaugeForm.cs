using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms.Examples {
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

        private void gauge1_ValueInRangeChanged(object sender, Controls.ValueInRangeChangedEventArgs e) {
            valueLabel.BackColor = e.InRange ? 
                e.Range.Colour : 
                Color.FromKnownColor(KnownColor.Control);
        }
    }
}
