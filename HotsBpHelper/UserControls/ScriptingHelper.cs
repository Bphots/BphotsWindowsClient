using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;

namespace HotsBpHelper.UserControls
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ScriptingHelper
    {
        private readonly UserControl _userControl;

        public ScriptingHelper(UserControl userControl)
        {
            _userControl = userControl;
        }

        public void HideWindow()
        {
            var win = Window.GetWindow(_userControl);
            if (win != null)
            {
                win.Visibility = Visibility.Hidden;
            }
        }
    }
}