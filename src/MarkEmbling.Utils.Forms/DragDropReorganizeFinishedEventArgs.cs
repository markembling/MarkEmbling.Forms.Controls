using System;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms {
    public delegate void DragDropReorganizeFinishedHandler(object source, DragDropReorganizeFinishedEventArgs e);

    /// <summary>
    /// Provides data for the DragDropReorganizeFinished event of DragDropTreeView.
    /// </summary>
    public class DragDropReorganizeFinishedEventArgs : EventArgs {
        /// <summary>
        /// The newly moved tree node
        /// </summary>
        public TreeNode Node { get; set; }
    }
}