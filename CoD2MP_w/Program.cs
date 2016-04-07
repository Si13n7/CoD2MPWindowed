using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CoD2MP_w
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SilDev.Log.AllowDebug();
            try
            {
                bool newInstance = true;
                using (Mutex mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out newInstance))
                {
                    if (!newInstance)
                    {
                        MessageBox.Show("Multiple calls are not supported!", CoD2MP_w.Main.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!File.Exists(CoD2MP_w.Main.ExePath))
                    {
                        MessageBox.Show("Game not found!", CoD2MP_w.Main.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(CoD2MP_w.Main.ExePath)))
                    {
                        if (p.MainWindowHandle != IntPtr.Zero)
                        {
                            p.CloseMainWindow();
                            p.WaitForExit(200);
                        }
                        if (!p.HasExited)
                            p.Kill();
                    }
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => CoD2MP_w.Main.ProcessExit();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }
    }
}
