namespace CoD2MP_w
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Properties;
    using SilDev;
    using SilDev.Forms;
    using static MainClass;

    public partial class MainForm : Form
    {
        private static long _mem0, _mem1;

        public MainForm()
        {
            InitializeComponent();
            Icon = Resources.HighQuality;
            ResizeBegin += (s, args) => WindowEnforcement.Interval = 100;
            Resize += (s, args) => GamePanel.Update();
            ResizeEnd += (s, args) => WindowEnforcement.Interval = 400;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            GamePanel.SetDoubleBuffer();
            Patch();
            CompatFlags();
            using (var p = ProcessEx.Start(ExePath, EnvironmentEx.CommandLine(false), false, false))
                if (p?.HasExited == false)
                    p.WaitForInputIdle();
            WindowEnforcement.Enabled = true;
        }

        private void WindowEnforcement_Tick(object sender, EventArgs e)
        {
            try
            {
                var instances = ProcessEx.GetInstances(ExePath, true)?.ToArray();
                if (instances?.Any() != true)
                {
                    WindowEnforcement.Enabled = false;
                    Application.Exit();
                    return;
                }
                if (PatchIsRequired && !PatchIsActive)
                    return;
                if (WindowHandle == IntPtr.Zero)
                    foreach (var instance in instances)
                    {
                        if (instance.HasExited)
                            continue;
                        _mem0 = (long)Math.Round(instance.PeakWorkingSet64 / Math.Pow(1024, 2));
                        if (_mem0 < 96 || _mem0 != _mem1)
                            continue;
                        WindowHandle = WinApi.NativeHelper.FindWindowByCaption(Resources.WindowTitle);
                    }
                if (WindowHandle != IntPtr.Zero)
                    if (!ShowInTaskbar)
                    {
                        Text = Resources.WindowTitle;
                        ShowInTaskbar = true;
                        WindowState = FormWindowState.Maximized;
                        if (PatchIsActive)
                        {
                            WinApi.NativeHelper.RemoveWindowBorders(WindowHandle);
                            WinApi.NativeHelper.SetWindowSize(WindowHandle, 320, 240);
                        }
                        WinApi.NativeHelper.SetParent(WindowHandle, GamePanel.Handle);
                    }
                    else
                    {
                        var curRect = new Rectangle();
                        WinApi.NativeHelper.GetWindowRect(WindowHandle, ref curRect);
                        if (WinApi.NativeHelper.GetLastError() > 0)
                            return;
                        if (Environment.OSVersion.Version.Major < 10)
                        {
                            var scrRect = RectangleToScreen(ClientRectangle);
                            const int borderSize = 10;
                            var titleSize = scrRect.Top - Top;
                            var newRect = new Rectangle
                            {
                                X = borderSize * -1,
                                Y = titleSize * -1,
                                Width = GamePanel.Width + borderSize * 2,
                                Height = GamePanel.Height + titleSize + borderSize
                            };
                            if (curRect != newRect)
                                WinApi.NativeHelper.MoveWindow(WindowHandle, newRect.X, newRect.Y, newRect.Width, newRect.Height, true);
                        }
                        else
                        {
                            if (curRect.Size != GamePanel.Size)
                                WinApi.NativeHelper.MoveWindow(WindowHandle, 0, 0, GamePanel.Width, GamePanel.Height, true);
                        }
                    }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            if (WindowHandle == IntPtr.Zero)
                _mem1 = _mem0;
        }
    }
}
