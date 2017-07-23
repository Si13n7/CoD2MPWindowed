namespace CoD2MP_w
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using static MainClass;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            using (new Mutex(true, ProcessEx.CurrentName, out bool newInstance))
            {
                if (!newInstance)
                {
                    MessageBox.Show(Resources.MsgMultipleCalls, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!File.Exists(ExePath))
                {
                    MessageBox.Show(Resources.MsgNotFound, Resources.WindowTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                foreach (var instance in ProcessEx.GetInstances(ExePath, true))
                    try
                    {
                        instance.Kill();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                AppDomain.CurrentDomain.ProcessExit += (s, e) => ProcessExit();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
