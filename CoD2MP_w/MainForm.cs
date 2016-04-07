using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CoD2MP_w
{
    public partial class MainForm : Form
    {
        static long mem0 = 0, mem1 = 0;

        public MainForm()
        {
            InitializeComponent();
            Icon = Properties.Resources.cod2_high;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Main.Patch();
            Main.CompatFlags();
            SilDev.Run.App(new ProcessStartInfo()
            {
                Arguments = SilDev.Run.CommandLine(false),
                FileName = Main.ExePath
            }, 0, null);
            Loop.Enabled = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) =>
            Loop.Interval = 1;

        private void MainForm_ResizeEnd(object sender, EventArgs e) =>
            Loop.Interval = 200;

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to continue?", Main.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        }

        private void Loop_Tick(object sender, EventArgs e)
        {
            try
            {
                Process[] gameProc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Main.ExePath));
                bool isRunning = gameProc.Length > 0;
                if (!isRunning)
                {
                    Loop.Enabled = false;
                    Application.Exit();
                    return;
                }
                if (Main.PatchIsRequired && !Main.PatchIsActive)
                    return;
                if (Main.Handle == IntPtr.Zero)
                {
                    foreach (Process p in gameProc)
                    {
                        if (p.HasExited)
                            continue;
                        mem0 = p.PeakWorkingSet64 / 1024 / 1024;
                        if (mem0 < 60 || mem0 != mem1)
                            continue;
                        Main.Handle = SilDev.WinAPI.FindWindowByCaption(Main.Title);
                    }
                }
                if (Main.Handle != IntPtr.Zero)
                {
                    if (!ShowInTaskbar)
                    {
                        Text = Main.Title;
                        ShowInTaskbar = true;
                        WindowState = FormWindowState.Maximized;
                        if (Main.PatchIsActive)
                        {
                            SilDev.WinAPI.RemoveWindowBorders(Main.Handle);
                            SilDev.WinAPI.SetWindowSize(Main.Handle, 320, 240);
                        }
                        SilDev.WinAPI.SafeNativeMethods.SetParent(Main.Handle, GamePanel.Handle);
                    }
                    else
                    {
                        Rectangle curRect = new Rectangle();
                        SilDev.WinAPI.SafeNativeMethods.GetWindowRect(Main.Handle, ref curRect);
                        if (SilDev.WinAPI.GetLastError() > 0)
                            return;
                        if (Main.PatchIsActive)
                        {
                            Rectangle scrRect = RectangleToScreen(ClientRectangle);
                            int borderSize = 10;
                            int titleSize = scrRect.Top - Top;
                            Rectangle newRect = new Rectangle()
                            {
                                X = borderSize * -1,
                                Y = titleSize * -1,
                                Width = GamePanel.Width + (borderSize * 2),
                                Height = GamePanel.Height + titleSize + borderSize
                            };
                            if (curRect != newRect)
                                SilDev.WinAPI.SafeNativeMethods.MoveWindow(Main.Handle, newRect.X, newRect.Y, newRect.Width, newRect.Height, true);
                        }
                        else
                        {
                            if (curRect.Size != GamePanel.Size)
                                SilDev.WinAPI.SafeNativeMethods.MoveWindow(Main.Handle, 0, 0, GamePanel.Width, GamePanel.Height, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            if (Main.Handle == IntPtr.Zero)
                mem1 = mem0;
        }
    }
}
