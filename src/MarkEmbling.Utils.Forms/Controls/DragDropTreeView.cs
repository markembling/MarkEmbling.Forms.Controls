using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms.Controls {
    /// <summary>
    /// TreeView with drag and drop item reorganisation.
    /// 
    /// Items can be dragged and placed under any node. An event is fired each time a node is 
    /// dragged over another, which allows prevention of the being-dragged node from becoming 
    /// a child.
    /// 
    /// Most of the code here is taken/derived from 
    /// http://www.codeproject.com/Articles/6184/TreeView-Rearrange.
    /// </summary>
    public class DragDropTreeView : NativeStyleTreeView {
        private const int MapSize = 128;
        private string _nodeMap = "";
        private StringBuilder _newNodeMap = new StringBuilder(MapSize);
        private readonly Timer _autoScrollTimer = new Timer();
        private bool _placeholdersVisible;

        private const int WmVscroll = 0x115;
        private const int SbLineDown = 1;
        private const int SbLineUp = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        enum PlaceholderLocation {
            LeafTop,
            LeafBottom,
            FolderTop,
            FolderAdd,
            FolderBottom,
            None
        }

        /// <summary>
        /// Fired when a node is being dragged over another; can be used to determine 
        /// whether the node being dragged over is able to accept or not. E.g. 'is it a
        /// folder?'
        /// </summary>
        [Category("Drag Drop"), Description("Occurs when a node is being dragged over another.")]
        public event AcceptingDraggedNodeHandler AcceptingDraggedNode;

        /// <summary>
        /// Fired when a node has been moved and dropped in its new location in the tree.
        /// </summary>
        [Category("Drag Drop"), Description("Occurs when a node has been dropped in a new location.")]
        public event DragDropReorganizeFinishedHandler DragDropReorganizeFinished;

        public DragDropTreeView() {
            // Set up events
            MouseDown += DragDropTreeView_MouseDown;
            ItemDrag += DragDropTreeView_ItemDrag;
            DragEnter += DragDropTreeView_DragEnter;
            DragOver += DragDropTreeView_DragOver;
            DragDrop += DragDropTreeView_DragDrop;

            // Set defaults
            AutoScrollEnabled = true;
            AutoScrollInterval = 150;
            AutoScrollThreshold = 12;
        }

        [Category("Behavior"),
         Description("Whether the control should auto-scroll up/down when dragging items near the edge"),
         DefaultValue(true)]
        public bool AutoScrollEnabled { get; set; }

        [Category("Behavior"),
         Description("Interval between auto-scroll triggering"),
         DefaultValue(150)]
        public int AutoScrollInterval {
            get { return _autoScrollTimer.Interval; }
            set { _autoScrollTimer.Interval = value; }
        }

        [Category("Behavior"),
         Description("Distance to the top/bottom which the cursor must be within to trigger auto-scrolling"),
         DefaultValue(12)]
        public int AutoScrollThreshold { get; set; }

        private void DragDropTreeView_MouseDown(object sender, MouseEventArgs e) {
            SelectedNode = GetNodeAt(e.X, e.Y);
        }

        private void DragDropTreeView_ItemDrag(object sender, ItemDragEventArgs e) {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void DragDropTreeView_DragEnter(object sender, DragEventArgs e) {
            //e.Effect = DragDropEffects.Move;
        }

        private void DragDropTreeView_DragOver(object sender, DragEventArgs e) {
            var clientPos = PointToClient(Cursor.Position);

            // Scroll the tree up/down if necessary
            if (AutoScrollEnabled)
                AutoScroll(clientPos);

            var nodeOver = GetNodeAt(clientPos);
            var nodeMoving = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");

            // nodeOver must not be null, and nodeOver must not be the same as nodeMoving UNLESS
            // nodeOver is the last node in the branch (so it is possible to drop below a parent branch)
            if (nodeOver != null && (nodeOver != nodeMoving || (nodeOver.Parent != null && nodeOver.Index == (nodeOver.Parent.Nodes.Count - 1)))) {
                var offsetY = PointToClient(Cursor.Position).Y - nodeOver.Bounds.Top;

                // Prevent paradoxes here and now
                if (CausesParadox(nodeOver, nodeMoving)) {
                    SetPlaceholders(PlaceholderLocation.None, null, null);
                    return;
                }

                // Create a new event args object for the AcceptingDraggedNode event.
                var args = new AcceptingDraggedNodeEventArgs(nodeOver, nodeMoving);

                // Fire the event (if there is a handler attached)
                if (AcceptingDraggedNode != null)
                    AcceptingDraggedNode(this, args);

                if (args.PreventAnyDrop) {
                    SetPlaceholders(PlaceholderLocation.None, null, null);
                    e.Effect = DragDropEffects.None;
                    return;
                }

                e.Effect = DragDropEffects.Move;

                if (args.PreventDropAsChild) {
                    // Node does not accept children (is a leaf)

                    // Collapse so we don't get weirdness when dropping below the node
                    // but apparently above its children nodes.
                    if (nodeOver.IsExpanded)
                        nodeOver.Collapse(true);

                    HandleDraggingOverNonChildAcceptingNode(offsetY, nodeOver);
                } else {
                    // Node accepts children
                    HandleDraggingOverChildAcceptingNode(offsetY, nodeOver, nodeMoving);
                }
            }
        }

        /// <summary>
        /// Trigger automatic scrolling up/down if an item is being dragged very high or low
        /// </summary>
        /// <param name="clientPos"></param>
        private void AutoScroll(Point clientPos) {
            if (clientPos.Y <= AutoScrollThreshold) {
                if (!_autoScrollTimer.Enabled) {
                    _autoScrollTimer.Tick += _autoScrollTimer_Tick_scrollUp;
                    _autoScrollTimer.Start();
                }
            } else if (clientPos.Y >= (Height - AutoScrollThreshold)) {
                if (!_autoScrollTimer.Enabled) {
                    _autoScrollTimer.Tick += _autoScrollTimer_Tick_scrollDown;
                    _autoScrollTimer.Start();
                }
            } else {
                _autoScrollTimer.Tick -= _autoScrollTimer_Tick_scrollUp;
                _autoScrollTimer.Tick -= _autoScrollTimer_Tick_scrollDown;
                _autoScrollTimer.Stop();
            }
        }

        private void HandleDraggingOverChildAcceptingNode(int offsetY, TreeNode nodeOver, TreeNode nodeMoving) {
            if (offsetY < (nodeOver.Bounds.Height / 3)) {
                // Store placeholder info
                SetNewNodeMap(nodeOver, false);
                if (SetMapsEqual()) return;

                SetPlaceholders(PlaceholderLocation.FolderTop, nodeOver, null);
            } else if ((nodeOver.Parent != null && nodeOver.Index == 0) &&
                       (offsetY > (nodeOver.Bounds.Height - (nodeOver.Bounds.Height / 3)))) {
                // Store placeholder info
                SetNewNodeMap(nodeOver, true);
                if (SetMapsEqual()) return;

                SetPlaceholders(PlaceholderLocation.FolderTop, nodeOver, null);
            } else {
                // Expand node if necessary, then return
                if (nodeOver.Nodes.Count > 0 && !nodeOver.IsExpanded) {
                    nodeOver.Expand();
                    return;
                }

                // Prevent the node from being dragged onto itself
                if (nodeMoving == nodeOver) return;


                // ----------------
                TreeNode parentDragDrop = null;
                //if (nodeOver.Parent != null && nodeOver.Index == (nodeOver.Parent.Nodes.Count - 1)) {
                var xPos = PointToClient(Cursor.Position).X;
                if (xPos < nodeOver.Bounds.Left - ImageList.Images[nodeOver.ImageIndex].Size.Width) {
                    parentDragDrop = nodeOver.Parent;

                    /*
                    if (xPos < (parentDragDrop.Bounds.Left - ImageList.Images[parentDragDrop.ImageIndex].Size.Width)) {
                        if (parentDragDrop.Parent != null)
                            parentDragDrop = parentDragDrop.Parent;
                    }
                     */

                    // Put it underneath the node as a sibling, not a child
                    SetNewNodeMap(parentDragDrop ?? nodeOver, true);
                    if (SetMapsEqual()) return;

                    SetPlaceholders(PlaceholderLocation.LeafBottom, nodeOver, parentDragDrop);

                    return;
                }
                //}

                // Put under as a child node
                SetNewNodeMap(nodeOver, false);
                _newNodeMap = _newNodeMap.Insert(_newNodeMap.Length, "|0");
                if (SetMapsEqual()) return;

                SetPlaceholders(PlaceholderLocation.FolderBottom, nodeOver, parentDragDrop);
                // ------------------


                // Store placeholder info
                /*
                SetNewNodeMap(nodeOver, false);
                _newNodeMap = _newNodeMap.Insert(_newNodeMap.Length, "|0");
                if (SetMapsEqual()) return;
                
                //SetPlaceholders(PlaceholderLocation.FolderAdd, nodeOver, null);
                SetPlaceholders(PlaceholderLocation.FolderBottom, nodeOver, null);*/
            }
        }

        private void HandleDraggingOverNonChildAcceptingNode(int offsetY, TreeNode nodeOver) {
            if (offsetY < (nodeOver.Bounds.Height / 2)) {
                // Store placeholder info
                SetNewNodeMap(nodeOver, false);
                if (SetMapsEqual()) return;

                SetPlaceholders(PlaceholderLocation.LeafTop, nodeOver, null);
            } else {
                // Allow drag-drop to parent branches
                TreeNode parentDragDrop = null;
                // If the node the mouse is over is the last node of the branch we should allow
                // the ability to drop the "nodeMoving" node BELOW the parent node
                if (nodeOver.Parent != null && nodeOver.Index == (nodeOver.Parent.Nodes.Count - 1)) {
                    var xPos = PointToClient(Cursor.Position).X;
                    if (xPos < nodeOver.Bounds.Left - ImageList.Images[nodeOver.ImageIndex].Size.Width) {
                        parentDragDrop = nodeOver.Parent;

                        /*
                        if (xPos < (parentDragDrop.Bounds.Left - ImageList.Images[parentDragDrop.ImageIndex].Size.Width)) {
                            if (parentDragDrop.Parent != null)
                                parentDragDrop = parentDragDrop.Parent;
                        }*/
                    }
                }

                // Store placeholder info: since we are in a special case here, use 
                // the parentDragDrop node as the current "nodeover"
                SetNewNodeMap(parentDragDrop ?? nodeOver, true);
                if (SetMapsEqual()) return;

                SetPlaceholders(PlaceholderLocation.LeafBottom, nodeOver, parentDragDrop);
            }
        }

        private void DragDropTreeView_DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false) && _nodeMap != "") {
                var movingNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                var nodeIndexes = _nodeMap.Split('|');
                var insertCollection = Nodes;

                for (var i = 0; i < nodeIndexes.Length - 1; i++) {
                    insertCollection = insertCollection[Int32.Parse(nodeIndexes[i])].Nodes;
                }

                insertCollection.Insert(Int32.Parse(nodeIndexes[nodeIndexes.Length - 1]), (TreeNode)movingNode.Clone());
                SelectedNode = insertCollection[Int32.Parse(nodeIndexes[nodeIndexes.Length - 1])];

                // Remove the old ersion of the node
                movingNode.Remove();

                // Expand if necessary
                if (SelectedNode.Nodes.Count > 0 && !SelectedNode.IsExpanded)
                    SelectedNode.Expand();

                // Fire the event (if there is a handler attached)
                var args = new DragDropReorganizeFinishedEventArgs {Node = SelectedNode};
                if (DragDropReorganizeFinished != null)
                    DragDropReorganizeFinished(this, args);
            }
        }

        /// <summary>
        /// Detect whether the current operation would cause a paradox: if nodeOver is a 
        /// child of nodeMoving. A parent can't move down below one of it's children.
        /// </summary>
        /// <param name="nodeOver">Node we're dragged over</param>
        /// <param name="nodeMoving">Node we're moving</param>
        /// <returns>True if a paradox situation would ensue</returns>
        private bool CausesParadox(TreeNode nodeOver, TreeNode nodeMoving) {
            var tnParadox = nodeOver;
            while (tnParadox.Parent != null) {
                if (tnParadox.Parent == nodeMoving) {
                    _nodeMap = "";
                    return true;
                }

                tnParadox = tnParadox.Parent;
            }

            return false;
        }

        /// <summary>
        /// Removes existing placeholders and draws a new one according to the given location.
        /// </summary>
        /// <param name="location">Location of the placeholder to draw</param>
        /// <param name="node">Node we're over (set null if location is None)</param>
        /// <param name="parent">Parent node (only relevent to leaf bottom location, set 
        /// null otherwise)</param>
        private void SetPlaceholders(PlaceholderLocation location, TreeNode node, TreeNode parent) {
            // Clear placeholders
            if (_placeholdersVisible)
                Refresh();

            // Set placeholder flag
            _placeholdersVisible = location != PlaceholderLocation.None;

            // Draw the proper one
            switch (location) {
                case PlaceholderLocation.LeafTop:
                    DrawLeafTopPlaceholders(node);
                    break;
                case PlaceholderLocation.LeafBottom:
                    DrawLeafBottomPlaceholders(node, parent);
                    break;
                case PlaceholderLocation.FolderTop:
                    DrawFolderTopPlaceholders(node);
                    break;
                case PlaceholderLocation.FolderAdd:
                    DrawAddToFolderPlaceholder(node);
                    break;
                case PlaceholderLocation.FolderBottom:
                    DrawFolderBottomPlaceholders(node);
                    break;
            }
        }

        #region Placeholder draw methods
        private void DrawLeafTopPlaceholders(TreeNode nodeOver) {
            var g = CreateGraphics();

            var nodeOverImageWidth = ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;
            var leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            var rightPos = Width - 4;

            var leftTriangle = new[] {
                new Point(leftPos, nodeOver.Bounds.Top - 4),
                new Point(leftPos, nodeOver.Bounds.Top + 4),
                new Point(leftPos + 4, nodeOver.Bounds.Y),
                new Point(leftPos + 4, nodeOver.Bounds.Top - 1),
                new Point(leftPos, nodeOver.Bounds.Top - 5)
            };

            var rightTriangle = new[] {
                new Point(rightPos, nodeOver.Bounds.Top - 4),
                new Point(rightPos, nodeOver.Bounds.Top + 4),
                new Point(rightPos - 4, nodeOver.Bounds.Y),
                new Point(rightPos - 4, nodeOver.Bounds.Top - 1),
                new Point(rightPos, nodeOver.Bounds.Top - 5)
            };

            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Top),
                       new Point(rightPos, nodeOver.Bounds.Top));

        }

        private void DrawLeafBottomPlaceholders(TreeNode nodeOver, TreeNode parentDragDrop) {
            var g = CreateGraphics();

            var nodeOverImageWidth = ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;
            // Once again, we are not dragging to node over, draw the placeholder using the 
            // ParentDragDrop bounds
            int leftPos;
            if (parentDragDrop != null)
                leftPos = parentDragDrop.Bounds.Left - (ImageList.Images[parentDragDrop.ImageIndex].Size.Width + 8);
            else
                leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            var rightPos = Width - 4;

            var leftTriangle = new[] {
                new Point(leftPos, nodeOver.Bounds.Bottom - 4),
                new Point(leftPos, nodeOver.Bounds.Bottom + 4),
                new Point(leftPos + 4, nodeOver.Bounds.Bottom),
                new Point(leftPos + 4, nodeOver.Bounds.Bottom - 1),
                new Point(leftPos, nodeOver.Bounds.Bottom - 5)
            };

            var rightTriangle = new[] {
                new Point(rightPos, nodeOver.Bounds.Bottom - 4),
                new Point(rightPos, nodeOver.Bounds.Bottom + 4),
                new Point(rightPos - 4, nodeOver.Bounds.Bottom),
                new Point(rightPos - 4, nodeOver.Bounds.Bottom - 1),
                new Point(rightPos, nodeOver.Bounds.Bottom - 5)
            };


            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Bottom),
                       new Point(rightPos, nodeOver.Bounds.Bottom));
        }

        private void DrawFolderTopPlaceholders(TreeNode nodeOver) {
            var g = CreateGraphics();
            var nodeOverImageWidth = ImageList.Images[nodeOver.ImageIndex].Size.Width + 8;

            var leftPos = nodeOver.Bounds.Left - nodeOverImageWidth;
            var rightPos = Width - 4;

            var leftTriangle = new[] {
                new Point(leftPos, nodeOver.Bounds.Top - 4),
                new Point(leftPos, nodeOver.Bounds.Top + 4),
                new Point(leftPos + 4, nodeOver.Bounds.Y),
                new Point(leftPos + 4, nodeOver.Bounds.Top - 1),
                new Point(leftPos, nodeOver.Bounds.Top - 5)
            };

            var rightTriangle = new[] {
                new Point(rightPos, nodeOver.Bounds.Top - 4),
                new Point(rightPos, nodeOver.Bounds.Top + 4),
                new Point(rightPos - 4, nodeOver.Bounds.Y),
                new Point(rightPos - 4, nodeOver.Bounds.Top - 1),
                new Point(rightPos, nodeOver.Bounds.Top - 5)
            };

            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Top),
                       new Point(rightPos, nodeOver.Bounds.Top));
        }

        private void DrawFolderBottomPlaceholders(TreeNode nodeOver) {
            var g = CreateGraphics();
            var leftPos = nodeOver.Bounds.Left - 4;
            var rightPos = Width - 4;

            var leftTriangle = new[] {
                new Point(leftPos, nodeOver.Bounds.Bottom - 4),
                new Point(leftPos, nodeOver.Bounds.Bottom + 4),
                new Point(leftPos + 4, nodeOver.Bounds.Bottom),
                new Point(leftPos + 4, nodeOver.Bounds.Bottom - 1),
                new Point(leftPos, nodeOver.Bounds.Bottom - 5)
            };

            var rightTriangle = new[] {
                new Point(rightPos, nodeOver.Bounds.Bottom - 4),
                new Point(rightPos, nodeOver.Bounds.Bottom + 4),
                new Point(rightPos - 4, nodeOver.Bounds.Bottom),
                new Point(rightPos - 4, nodeOver.Bounds.Bottom - 1),
                new Point(rightPos, nodeOver.Bounds.Bottom - 5)
            };

            g.FillPolygon(Brushes.Black, leftTriangle);
            g.FillPolygon(Brushes.Black, rightTriangle);
            g.DrawLine(new Pen(Color.Black, 2), new Point(leftPos, nodeOver.Bounds.Bottom),
                       new Point(rightPos, nodeOver.Bounds.Bottom));
        }

        private void DrawAddToFolderPlaceholder(TreeNode nodeOver) {
            var g = CreateGraphics();
            var rightPos = nodeOver.Bounds.Right + 6;
            var rightTriangle = new[] {
                new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height/2) + 4),
                new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height/2) + 4),
                new Point(rightPos - 4, nodeOver.Bounds.Y + (nodeOver.Bounds.Height/2)),
                new Point(rightPos - 4, nodeOver.Bounds.Y + (nodeOver.Bounds.Height/2) - 1),
                new Point(rightPos, nodeOver.Bounds.Y + (nodeOver.Bounds.Height/2) - 5)
            };

            Refresh();
            g.FillPolygon(Brushes.Black, rightTriangle);
        }

        #endregion

        #region Auto-scroll timer event handler methods
        private void _autoScrollTimer_Tick_scrollUp(object sender, EventArgs e) {
            SendMessage(Handle, WmVscroll, (IntPtr) SbLineUp, IntPtr.Zero);
        }

        private void _autoScrollTimer_Tick_scrollDown(object sender, EventArgs e) {
            SendMessage(Handle, WmVscroll, (IntPtr) SbLineDown, IntPtr.Zero);
        }
        #endregion

        private void SetNewNodeMap(TreeNode tnNode, bool boolBelowNode) {
            _newNodeMap.Length = 0;

            if (boolBelowNode)
                _newNodeMap.Insert(0, tnNode.Index + 1);
            else
                _newNodeMap.Insert(0, tnNode.Index);
            var tnCurNode = tnNode;

            while (tnCurNode.Parent != null) {
                tnCurNode = tnCurNode.Parent;

                if (_newNodeMap.Length == 0 && boolBelowNode) {
                    _newNodeMap.Insert(0, (tnCurNode.Index + 1) + "|");
                } else {
                    _newNodeMap.Insert(0, tnCurNode.Index + "|");
                }
            }
        }

        private bool SetMapsEqual() {
            if (_newNodeMap.ToString() == _nodeMap)
                return true;
            _nodeMap = _newNodeMap.ToString();
            return false;
        }
    }
}