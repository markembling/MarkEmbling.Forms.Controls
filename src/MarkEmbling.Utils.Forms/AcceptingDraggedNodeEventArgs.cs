using System;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms {
    public delegate void AcceptingDraggedNodeHandler(object source, AcceptingDraggedNodeEventArgs e);

    /// <summary>
    /// Provides data for the AcceptingDraggedNode event of DragDropTreeView.
    /// </summary>
    public class AcceptingDraggedNodeEventArgs : EventArgs {
        public AcceptingDraggedNodeEventArgs(TreeNode nodeOver, TreeNode nodeMoving) {
            NodeMoving = nodeMoving;
            NodeOver = nodeOver;
        }

        /// <summary>
        /// The node being moved.
        /// </summary>
        public TreeNode NodeMoving { get; private set; }

        /// <summary>
        /// The node being dragged over.
        /// </summary>
        public TreeNode NodeOver { get; private set; }

        /// <summary>
        /// Set to true to stop the being-moved node from being dropped 
        /// as a child of the node being dragged over.
        /// </summary>
        public bool PreventDropAsChild { get; set; }

        /// <summary>
        /// Set to true to prevent the being-moved node from being dropped
        /// here at all: "this node may not go here". Warning: use and test 
        /// carefully as this behaviour can be jarring in unexpected places.
        /// </summary>
        public bool PreventAnyDrop { get; set; }
    }
}