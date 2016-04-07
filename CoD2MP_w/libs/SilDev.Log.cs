
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Log
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        }

        public static string ConsoleTitle { get; } = $"Debug Console ('{Application.ProductName}')";

        public static int DebugMode { get; private set; } = 0;

        private static bool IsRunning = false, FirstCall = false, FirstEntry = false;
        private static IntPtr stdHandle = IntPtr.Zero;
        private static SafeFileHandle sfh = null;
        private static FileStream fs = null;
        private static StreamWriter sw = null;

        public static string FileName { get; private set; } = $"{Application.ProductName}_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
        public static string FileLocation { get; set; } = Environment.GetEnvironmentVariable("TEMP");
        public static string FilePath { get; private set; } = Path.Combine(FileLocation, FileName);

        public static void ActivateDebug(int mode = 1)
        {
            DebugMode = mode;
            if (!FirstCall)
            {
                FirstCall = true;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => Debug(e.Exception, true);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => Debug(new ApplicationException(), true);
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
                try
                {
                    if (!Directory.Exists(FileLocation))
                        Directory.CreateDirectory(FileLocation);
                    Path.GetFullPath(FileLocation);
                    FilePath = Path.Combine(FileLocation, FileName);
                }
                catch
                {
                    FileName = $"{Application.ProductName}.log";
                    FileLocation = Environment.GetEnvironmentVariable("TEMP");
                    FilePath = Path.Combine(FileLocation, FileName);
                }
            }
        }

        public static void AllowDebug()
        {
            int mode = 0;
            if (new Regex("/debug [0-2]|/debug \"[0-2]\"").IsMatch(Environment.CommandLine))
            {
                if (!int.TryParse(new Regex("/debug ([0-2]?)").Match(Environment.CommandLine.Replace("\"", string.Empty)).Groups[1].ToString(), out mode))
                    mode = 0;
            }
            ActivateDebug(mode);
        }

        public static void Debug(string exMsg, string exTra = null)
        {
            if (!FirstCall || DebugMode < 1 || string.IsNullOrEmpty(exMsg))
                return;

            if (!FirstEntry)
            {
                FirstEntry = true;
                if (!File.Exists(FilePath))
                {
                    try
                    {
                        if (!Directory.Exists(FileLocation))
                            Directory.CreateDirectory(FileLocation);
                        File.Create(FilePath).Close();
                    }
                    catch (Exception ex)
                    {
                        if (DebugMode > 1)
                        {
                            DebugMode = 3;
                            Debug(ex);
                        }
                    }
                }
                Debug("***Logging has been started***", $"'{Environment.OSVersion}' - '{Application.ProductName}' - '{Application.ProductVersion}' - '{FilePath}'");
            }
            if (!File.Exists(FilePath) && DebugMode < 1)
                return;

            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz");
            string exmsg = $"Time:  {date}\r\nMsg:   {Filter(exMsg)}\r\n";
            if (!string.IsNullOrWhiteSpace(exTra))
            {
                string extra = Filter(exTra);
                extra = extra.Replace("\r\n", " - ");
                exmsg += $"Trace: {extra}\r\n";
            }

            if (DebugMode < 3 && File.Exists(FilePath))
            {
                try
                {
                    File.AppendAllText(FilePath, $"{exmsg}\r\n");
                }
                catch (Exception ex)
                {
                    try
                    {
                        string exFileName = $"{Application.ProductName}_{DateTime.Now.ToString("yyyy-MM-dd_fffffff")}.log";
                        string exFilePath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), exFileName);
                        exmsg += $"Msg2:  {ex.Message}\r\n";
                        File.AppendAllText(exFilePath, exmsg);
                    }
                    catch (Exception exc)
                    {
                        if (DebugMode > 1)
                        {
                            exmsg += $"Msg3:  {exc.Message}\r\n";
                            MessageBox.Show(exmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            if (DebugMode > 1)
            {
                try
                {
                    if (!IsRunning)
                    {
                        SafeNativeMethods.AllocConsole();
                        SafeNativeMethods.DeleteMenu(SafeNativeMethods.GetSystemMenu(SafeNativeMethods.GetConsoleWindow(), false), 0xF060, 0x0);
                        stdHandle = SafeNativeMethods.GetStdHandle(-11);
                        sfh = new SafeFileHandle(stdHandle, true);
                        fs = new FileStream(sfh, FileAccess.Write);
                        if (Console.Title != ConsoleTitle)
                        {
                            Console.Title = ConsoleTitle;
                            Console.BufferHeight = short.MaxValue - 1;
                            Console.BufferWidth = Console.WindowWidth;
                            Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(new Crypt.Base85().DecodeString(string.Concat(new string[]
                            {
                                "<~+<V", "e6?XI", "/I?XI", "/I/mh", "s.+CA", "J_?Q`", "]_?XI", "/I?XF",
                                "ou+<V", "dL+<Y", "#u?XI", "/I?XI", ".I+<W", "<[+<Y", "#u?XI", ".nHs^",
                                "6.04,", "hE+FI", "F$?XI", "/I+<X", "o3+<Y", "#u?XI", "&F?XI", "/I?Q^",
                                "Ir$6U", "Hr?XI", "/I?Q^", "Ir+FG", ":SHm!", "eZ+<Z", "%S+C?", "O(?Q^",
                                "IR+<W", "<[+<V", "e3+<V", "d[+<V", "dL0+&", "gE0-D", "A[+<V", "dL+C'",
                                "::+FG", ";Z+<V", "eS+>4", "i[+<V", "dL+C'", "::+<Z", "%S+C$", "$B+<V",
                                "dL0+&", "gT?XI", "/I?XI", "._+>8", "+L?[N", "uD?XI", "/f04/", "'n?XI",
                                "._+>8", "+L?XJ", "1'+>5", "BT?XI", ".n$6U", "H6+<V", "dL+<X", "oB+<V",
                                "dL+<V", "dL+<V", "dL+<V", "dL+<X", "oB+<V", "dL+<X", "oB~>"
                            })));
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("           D E B U G    C O N S O L E");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        IsRunning = true;
                    }
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(new string('-', Console.BufferWidth - 1));
                    foreach (string line in exmsg.Split(new string[] { "\r\n" }, StringSplitOptions.None))
                    {
                        string[] sa = line.Split(' ');
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(sa[0]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($" {string.Join(" ", sa.Skip(1).ToArray())}");
                    }
                    Console.ResetColor();
                    sw = new StreamWriter(fs, Encoding.ASCII) { AutoFlush = true };
                    Console.SetOut(sw);
                }
                catch (Exception ex)
                {
                    DebugMode = 1;
                    Debug(ex);
                }
            }
        }

        public static void Debug(Exception ex, bool forceLogging = false)
        {
            if (DebugMode < 1)
            {
                if (!forceLogging)
                    return;
                DebugMode = 1;
            }
            Debug(ex.Message, ex.StackTrace);
        }

        private static string Filter(string input)
        {
            try
            {
                string s = string.Join(" - ", input.Split(new string[] { "\r\n" }, StringSplitOptions.None));
                s = Regex.Replace(s.Trim(), " {2,}", " ", RegexOptions.Singleline);
                s = $"{char.ToUpper(s[0])}{s.Substring(1)}";
                return s;
            }
            catch
            {
                return input;
            }
        }

        private static void Close()
        {
            try
            {
                if (sfh != null && !sfh.IsClosed)
                    sfh.Close();
                if (stdHandle != null)
                    SafeNativeMethods.CloseHandle(stdHandle);
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
            try
            {
                foreach (string file in Directory.GetFiles(FileLocation, $"{Application.ProductName}*.log", SearchOption.TopDirectoryOnly))
                {
                    if (FilePath == file)
                        continue;
                    if ((DateTime.Now - new FileInfo(file).LastWriteTime).TotalDays >= 7d)
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
        }
    }
}

#endregion
