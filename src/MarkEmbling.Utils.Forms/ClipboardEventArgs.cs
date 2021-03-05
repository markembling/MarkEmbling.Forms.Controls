using System.ComponentModel;

namespace MarkEmbling.Utils.Forms {
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
