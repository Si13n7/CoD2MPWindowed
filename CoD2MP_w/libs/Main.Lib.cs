using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CoD2MP_w
{
    public static class Main
    {
        public static string Title { get; } = "Call of Duty 2 Multiplayer";
        public static IntPtr Handle { get; set; } = IntPtr.Zero;

        public static string ExePath { get; private set; } = Path.Combine(Application.StartupPath, "CoD2MP_s.exe");
        private static readonly string DllPath = Path.Combine(Application.StartupPath, "gfx_d3d_mp_x86_s.dll");
        private static readonly string newExePath = Path.Combine(Application.StartupPath, "___CoD2MP_s.exe");
        private static readonly string newDllPath = Path.Combine(Application.StartupPath, "___SilDev_D3D_MP.dll");

        public static bool PatchIsRequired { get; } = Environment.OSVersion.Version.Major < 10;
        public static bool PatchIsActive { get; private set; } = false;

        public static void CompatFlags()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor <= 1)
                SilDev.Reg.WriteValue(@"HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", ExePath, "~ WINXPSP2 DISABLEDWM");
            else
                SilDev.Reg.WriteValue(@"HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", ExePath, "~ WINXPSP2");
        }

        public static void Patch()
        {
            Cleanup();
            if (!PatchIsRequired)
                return;
            try
            {
                byte[] ba = File.ReadAllBytes(DllPath);
                foreach (int dllOffset in new int[] { 0x11564, 0x12A8A })
                {
                    if (ba[dllOffset] != 0x74)
                        throw new ArgumentOutOfRangeException("dllOffset");
                    ba[dllOffset] = 0xEB;
                }
                File.WriteAllBytes(newDllPath, ba);
                string s = Path.GetFileName(DllPath).Replace("d3d", "%s");
                int exeOffset = 0x1AE898;
                if (Encoding.ASCII.GetString(File.ReadAllBytes(ExePath).Skip(exeOffset).Take(s.Length).ToArray()).ToLower() != s)
                    throw new ArgumentOutOfRangeException("exeOffset");
                File.Copy(ExePath, newExePath);
                using (BinaryWriter bw = new BinaryWriter(File.Open(newExePath, FileMode.Open)))
                {
                    bw.BaseStream.Position = exeOffset;
                    bw.Write(Encoding.ASCII.GetBytes(Path.GetFileName(newDllPath).Replace("D3D", "%s")));
                    PatchIsActive = true;
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex, true);
                Cleanup();
            }
            if (PatchIsActive)
            {
                ExePath = newExePath;
                SilDev.Data.SetAttributes(newExePath, FileAttributes.Hidden);
                SilDev.Data.SetAttributes(newDllPath, FileAttributes.Hidden);
            }
        }

        private static void Cleanup()
        {
            try
            {
                if (File.Exists(newExePath))
                    File.Delete(newExePath);
                if (File.Exists(newDllPath))
                    File.Delete(newDllPath);
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
        }

        public static void ProcessExit()
        {
            try
            {
                if (Handle != IntPtr.Zero)
                    SilDev.WinAPI.SafeNativeMethods.CloseHandle(Handle);
                foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExePath)))
                {
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(200);
                    }
                    if (!p.HasExited)
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                SilDev.Log.Debug(ex);
            }
            Cleanup();
        }
    }
}
