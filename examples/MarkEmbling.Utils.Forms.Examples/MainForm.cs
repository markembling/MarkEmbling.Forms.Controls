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
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void viewButton_Click(object sender, EventArgs e) {
            switch (examplesDropDown.SelectedIndex) {
                case 0:
                    new DragDropTreeViewForm().Show(this);
                    break;
                case 1:
                    new GaugeForm().Show(this);
                    break;
            }
        }
    }
}
