using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MarkEmbling.Utils.Forms.Controls {
    /// <summary>
    /// Enhanced version of the TreeView control which allows use of the native Explorer-style
    /// appearance.
    /// </summary>
    public class NativeStyleTreeView : TreeView {
        [Category("Appearance"),
         Description("Use the native Windows appearance (like Windows Explorer)."),
         DefaultValue(false)]
        public bool UseNativeAppearance { get; set; }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        protected override void CreateHandle() {
            base.CreateHandle();

            // Apply the 'native' explorer style if required.
            if (UseNativeAppearance)
                SetWindowTheme(Handle, "explorer", null);
        }
    }
}