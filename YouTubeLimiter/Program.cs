using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using YouTubeLimiter;

internal class Program
{
    #region Trap application termination
    // https://stackoverflow.com/questions/474679/capture-console-exit-c-sharp
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    private delegate bool EventHandler(CtrlType sig);
    static EventHandler _handler;

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool Handler(CtrlType sig)
    {
        switch (sig)
        {
            case CtrlType.CTRL_C_EVENT:
                Console.WriteLine($"Ouch! ({sig})");
                break;
            default:
                // Restart itself.
                RestartMe();
                break;
        }
        return true;
    }

    private static void RestartMe()
    {
        Console.WriteLine("RestartMe!");
        string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var psi = new ProcessStartInfo();
        psi.FileName = path;
        psi.WindowStyle = ProcessWindowStyle.Normal;
        psi.UseShellExecute = true;
        var fi = new FileInfo(path);
        psi.WorkingDirectory = fi.Directory.FullName;
        Process.Start(psi);
    }
    #endregion

    public void Start()
    {
        // start a thread and start doing some processing
        //Console.WriteLine("Thread started, processing..");
        Console.Title = "YouTube limiter";
        Console.WriteLine($"{DateTime.Now} YouTube limiter is started.");
        User32.DeleteMenu();
        var _timer = new TimeRemain();

        while (true)
        {
            string command = Console.ReadLine();
            if (string.IsNullOrEmpty(command))
            {
                _timer.MainFunction(false, DateTime.Now);
            }
            else
            {
                switch (command)
                {
                    case "all":
                        _timer.PrintTitles(a => a.PID > 0);
                        break;
                    case "tabs":
                        _timer.PrintTitles(a => a.IsBrowser);
                        break;
                    default:
                        Console.WriteLine("Enter password:");
                        string pw = ReadLineMasked();
                        if (pw.Equals("dadlovesyou"))
                        {
                            Console.WriteLine($"Accepted.\n");
                            int minute;
                            if (int.TryParse(command, out minute))
                            {
                                _timer.ResetRemainingTime(minute);
                            }
                            else if (command.Equals("quit"))
                            {
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Wrong password.\n");
                            _timer.ResetRemainingTime(0);
                        }
                        break;
                }
            }
        }
    }

    // https://stackoverflow.com/questions/3404421/password-masking-console-application
    static string ReadLineMasked(char mask = '*')
    {
        var sb = new StringBuilder();
        ConsoleKeyInfo keyInfo;
        while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (!char.IsControl(keyInfo.KeyChar))
            {
                sb.Append(keyInfo.KeyChar);
                Console.Write(mask);
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);

                if (Console.CursorLeft == 0)
                {
                    Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                    Console.Write(' ');
                    Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                }
                else Console.Write("\b \b");
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }

    static void Main(string[] args)
    {
        // Doesn't work
        //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

        // Some biolerplate to react to close window event, CTRL-C, kill, etc
        _handler += new EventHandler(Handler);
        SetConsoleCtrlHandler(_handler, true);

        //start your multi threaded program here
        Program p = new Program();
        p.Start();
    }

}
