using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace YouTubeLimiter
{
    internal class TimeRemain
    {
        private const int MaxTime = 30; // Minute
        private System.Timers.Timer _oneMinuteTimer;
        private readonly string _file = Environment.CurrentDirectory + "\\DoNotOpen.txt";
        private int _timeRemains;

        public TimeRemain()
        {
            if (!File.Exists(_file))
            {
                ResetRemainingTime(MaxTime);
            }
            else
            {
                ResetIfAnotherday();
            }
            SetTimer();
        }

        // https://docs.microsoft.com/ja-jp/dotnet/api/system.timers.timer?view=net-6.0
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            _oneMinuteTimer = new System.Timers.Timer(60000);
            // Hook up the Elapsed event for the timer. 
            _oneMinuteTimer.Elapsed += OnTimedEvent;
            _oneMinuteTimer.AutoReset = true;
            _oneMinuteTimer.Enabled = true;
        }

        //internal int GetRemainingTime()
        //{
        //    return _timeRemains;
        //}

        private (DateTime dt, int remains) Read()
        {
            if (File.Exists(_file))
            {
                string[] buf = File.ReadAllText(_file).Split(',');
                return (DateTime.Parse(buf[0]), Convert.ToInt32(buf[1]));
            }
            ResetRemainingTime(_timeRemains);
            return Read();
        }

        private void ResetIfAnotherday()
        {
            var r = Read();
            if (r.dt.ToShortDateString() == DateTime.Now.ToShortDateString())
            {
                _timeRemains = r.remains;
            }
            else
            {
                // Reset if it's not today.
                ResetRemainingTime(MaxTime);
            }
        }

        internal void ResetRemainingTime(int remainingMinute)
        {
            File.WriteAllText(_file, $"{DateTime.Now.ToShortDateString()},{remainingMinute}");
            _timeRemains = remainingMinute;
            ShowRemainingTime();
        }

        internal void MainFunction(bool calledFromTimer, DateTime signalTime)
        {
            // If there is a YouTube tab, decrement the counter.
            var youtubeTabs = GetWindows().FindAll(a => a.IsBrowser && a.Title.Contains("YouTube"));
            if (youtubeTabs != null && youtubeTabs.Count > 0)
            {
                //Console.WriteLine($"Following YouTube tab(s) are found.");
                //foreach (var t in youtubeTabs)
                //{
                //    Console.WriteLine($"\t'{t.Title}' ({t.ProcessName})");
                //}
                if (calledFromTimer)
                {
                    Decrement();
                }
                ShowRemainingTime();
                if (_timeRemains == 3)
                {
                    User32.ActivateTheWindow();
                }
                else if (_timeRemains <= 0)
                {
                    foreach (var tab in youtubeTabs)
                    {
                        tab.Kill();
                    }
                }
            }
            else
            {
                if (_timeRemains == 0)
                {
                    Console.WriteLine($"{signalTime:HH:mm:ss} You ran out of time. See you tomorrow!");
                }
                else
                {
                    Console.WriteLine($"{signalTime:HH:mm:ss} No YouTube tabs are found. Remaining time is kept {_timeRemains} minutes.");
                }
            }
        }

        private void ActivateMe()
        {

        }

        private void Decrement()
        {
            if (_timeRemains > 0)
            {
                _timeRemains--;
            }
            File.WriteAllText(_file, $"{DateTime.Now.ToShortDateString()},{_timeRemains}");
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            ResetIfAnotherday();
            MainFunction(true, e.SignalTime);
        }

        private void ShowRemainingTime()
        {
            if (_timeRemains <= 3)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"WARNING! stop YouTubing NOW!! or your browser will be closed!");
            }
            else if (_timeRemains <= 10)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"You have {_timeRemains} minutes to browse YouTube.");
            Console.ResetColor();
        }

        private List<WindowInfo> GetWindows()
        {
            var collection = new List<WindowInfo>();
            User32.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = User32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (User32.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                {
                    var p = GetProcInfo(hWnd);
                    //Console.WriteLine($"\t\t{strTitle} {p.name}");
                    collection.Add(new WindowInfo(hWnd, strTitle, p.name, p.path, p.pid));
                }
                return true;
            };

            if (User32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                //foreach (var item in collection)
                //{
                //    Console.WriteLine($"{item.Title} {item.ProcessName}");
                //}
            }
            return collection;
        }

        private (string name, string path, int pid) GetProcInfo(IntPtr hWnd)
        {
            uint lpdwProcessId;
            User32.GetWindowThreadProcessId(hWnd, out lpdwProcessId);

            IntPtr hProcess = Kernel32.OpenProcess(0x0410, false, lpdwProcessId);

            StringBuilder name = new StringBuilder(0x1024);
            StringBuilder path = new StringBuilder(0x1024);
            int ret1 = PSAPI.GetModuleBaseName(hProcess, IntPtr.Zero, name, name.Capacity);
            int ret2 = PSAPI.GetModuleFileNameEx(hProcess, IntPtr.Zero, path, path.Capacity);

            Kernel32.CloseHandle(hProcess);
            string exeName = ret1 > 0 ? name.ToString() : "";
            string exePath = ret2 > 0 ? path.ToString() : "";
            int pid = (int)lpdwProcessId;
            return (exeName, exePath, pid);
        }

        internal void PrintTitles(Predicate<WindowInfo> match)
        {
            foreach (var w in GetWindows().FindAll(match))
            {
                Console.WriteLine($"{w.Title} {w.ProcessName}");
            }
        }
    }

}
