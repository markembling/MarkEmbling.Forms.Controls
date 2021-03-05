using System.ComponentModel;

namespace MarkEmbling.Forms.Controls.Events {
    /// <summary>
    /// Provides data for clipboard-centric events.
    /// </summary>
    public class ClipboardEventArgs : CancelEventArgs {
        public string Text { get; private set; }

        public ClipboardEventArgs(string text) {
            Text = text;
        }
    }

    public delegate void ClipboardEventHandler(object sender, ClipboardEventArgs e);
}
