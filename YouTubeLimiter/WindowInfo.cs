using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace YouTubeLimiter
{
    internal class WindowInfo
    {
        internal string Title { get; }
        internal string ProcessName { get; }
        internal string ModulePath { get; }
        internal int PID { get; }

        public WindowInfo(string title, string name, string path, int pid)
        {
            Title = title;
            ProcessName = name;
            ModulePath = path;
            PID = pid;
        }

        internal bool Kill()
        {
            var p = Process.GetProcessById(PID);
            return p.CloseMainWindow();
        }

    }
}
