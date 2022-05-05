using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YouTubeLimiter
{
    internal class WindowInfo
    {
        internal IntPtr HWnd { get; }
        internal string Title { get; }
        internal string ProcessName { get; }
        internal string ModulePath { get; }
        internal int PID { get; }
        internal bool IsBrowser { get; }

        public WindowInfo(IntPtr hwnd, string title, string name, string path, int pid)
        {
            HWnd = hwnd;
            Title = title;
            ProcessName = name;
            ModulePath = path;
            PID = pid;
            IsBrowser = name == "chrome.exe" || name == "msedge.exe";
        }

        internal bool Kill()
        {
            var p = Process.GetProcessById(PID);
            return p.CloseMainWindow();
        }

    }
}
