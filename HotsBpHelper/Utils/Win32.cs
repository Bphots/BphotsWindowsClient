using System;
using System.Runtime.InteropServices;

namespace HotsBpHelper.Utils
{
    public class Win32
    {
        #region Windows API Functions Declarations

        //This Function is used to get Active Window Title...
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hwnd, string lpString, int cch);

        //This Function is used to get Handle for Active Window...
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        //This Function is used to get Active process ID...
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out Int32 lpdwProcessId);

        #endregion

        #region User-defined Functions

        public static Int32 GetWindowProcessID(IntPtr hwnd)
        {
            //This Function is used to get Active process ID...
            Int32 pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }


        public static string ActiveApplTitle()
        {
            //This method is used to get active application's title using GetWindowText() method present in user32.dll
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd.Equals(IntPtr.Zero)) return "";
            string lpText = new string((char)0, 100);
            int intLength = GetWindowText(hwnd, lpText, lpText.Length);
            if ((intLength <= 0) || (intLength > lpText.Length)) return "unknown";
            return lpText.Trim();
        }

        #endregion
    }
}