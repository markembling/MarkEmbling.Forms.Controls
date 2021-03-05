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
    /*
     * Remember to set the 'AllowDrop' property of the TreeView to true to enable
     * drag and drop functionality.
     */
    public partial class DragDropTreeViewForm : Form {
        private int _folderCount;
        private int _leafCount;

        public DragDropTreeViewForm() {
            InitializeComponent();
        }

        private void addLeafButton_Click(object sender, EventArgs e) {
            _leafCount += 1;

            // Add a new leaf item at either the top level or under the selected
            // folder, if any.
            var item = new TreeNode("Item " + _leafCount) {
                ImageIndex = 1, 
                SelectedImageIndex = 1
            };

            // Assign the current group (if any)
            var selectedNode = treeView.SelectedNode;
            if (selectedNode != null && selectedNode.ImageIndex == 0) {
                selectedNode.Nodes.Add(item);
                if (!selectedNode.IsExpanded)
                    selectedNode.Expand();
            } else {
                treeView.Nodes.Add(item);
            }
        }

        private void addFolderButton_Click(object sender, EventArgs e) {
            _folderCount += 1;

            // Add a new folder at the top level
            var g = new TreeNode("Folder " + _folderCount);
            g.ImageIndex = g.SelectedImageIndex = 0;
            treeView.Nodes.Add(g);
        }

        private void removeNodeButton_Click(object sender, EventArgs e) {
            var selectedNode = treeView.SelectedNode;
            if (selectedNode != null)
                selectedNode.Remove();
        }

        private void treeView_AcceptingDraggedNode(object source, AcceptingDraggedNodeEventArgs e) {
            /*
             * This example enforces the following:
             * - Folders should only exist at the top level
             * - Leaves can exist at the top level or inside a folder
             * 
             * Determining whether a node is a leaf or a folder is based on ImageIndex.
             * 0 = folder, 1 = leaf. In a real application, this mechanism would probably
             * be replaced with something a litle better (using the Tag property of each 
             * node, for example).
             */

            // Don't allow a folder to become buried
            if (e.NodeMoving.ImageIndex == 0 && e.NodeOver.Level > 0)
                e.PreventAnyDrop = true;

            // Don't allow folders to be dropped as childen under anything
            if (e.NodeMoving.ImageIndex == 0)
                e.PreventDropAsChild = true;

            // Don't allow dropping under non-zero items (non-folders)
            if (e.NodeOver.ImageIndex > 0)
                e.PreventDropAsChild = true;
        }

        private void treeView_DragDropReorganizeFinished(object source, DragDropReorganizeFinishedEventArgs e) {
            // This event is fired when a node is dropped in its new place.
            // e.Node is the node that has been dropped.

            var parentDescription = 
                e.Node.Parent != null
                    ? "under '" + e.Node.Parent.Text + "'"
                    : "at the top level";

            // Update the status bar to show what happened
            statusLabel.Text = string.Format(
                "'{0}' has been positioned at index {1} {2}",
                e.Node.Text,
                e.Node.Index,
                parentDescription);
        }
    }
}
