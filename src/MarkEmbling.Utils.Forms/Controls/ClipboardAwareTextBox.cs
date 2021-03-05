using System.ComponentModel;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms.Controls
{
    /*
     * Thanks to the following for pointers, advice and examples:
     * - http://stackoverflow.com/questions/3446233/hook-on-default-paste-event-of-winforms-textbox-control
     * - http://www.vcskicks.com/clipboard-textbox.php
     */

    /// <summary>
    /// Extended version of the standard TextBox control to expose
    /// clipboard-centric events.
    /// </summary>
    public class ClipboardAwareTextBox : TextBox {
        private const int WmCut = 0x0300;
        private const int WmCopy = 0x0301;
        private const int WmPaste = 0x0302;

        [Category("Clipboard")]
        public event ClipboardEventHandler CutText;

        [Category("Clipboard")]
        public event ClipboardEventHandler CopiedText;

        [Category("Clipboard")]
        public event ClipboardEventHandler PastedText;

        protected override void WndProc(ref Message m) {
            switch (m.Msg) {
                case WmCut:
                    if (CutText != null) {
                        var args = new ClipboardEventArgs(SelectedText);
                        CutText(this, args);
                        if (! args.Cancel) base.WndProc(ref m);
                    }
                    break;
                case WmCopy:
                    if (CopiedText != null) {
                        var args = new ClipboardEventArgs(SelectedText);
                        CopiedText(this, args);
                        if (! args.Cancel) base.WndProc(ref m);
                    }
                    break;
                case WmPaste:
                    if (PastedText != null) {
                        var args = new ClipboardEventArgs(Clipboard.GetText());
                        PastedText(this, args);
                        if (! args.Cancel) base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
