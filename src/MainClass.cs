namespace CoD2MP_w
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Properties;
    using SilDev;

    public static class MainClass
    {
        private static readonly string DllPath = PathEx.Combine(PathEx.LocalDir, Resources.DllName);
        private static readonly string NewDllPath = PathEx.Combine(PathEx.LocalDir, Resources.NewDllName);
        private static readonly string NewExePath = PathEx.Combine(PathEx.LocalDir, Resources.NewExeName);

        public static string ExePath { get; private set; } = PathEx.Combine(PathEx.LocalDir, Resources.ExeName);

        public static IntPtr WindowHandle { get; set; } = IntPtr.Zero;

        public static bool PatchIsRequired { get; } = true;

        public static bool PatchIsActive { get; private set; }

        public static void CompatFlags()
        {
            var layers = new AppCompatLayers
            {
                OperatingSystem = AppCompatSystemVersion.WinXPSP2,
                DisableFullscreenOptimizations = true,
                RunAsAdministrator = true
            };
            AppCompat.SetLayers(ExePath, layers);
        }

        public static void Patch()
        {
            Cleanup();
            if (!PatchIsRequired)
                return;
            try
            {
                var dllBytes = File.ReadAllBytes(DllPath);
                var dllOffsets = new[]
                {
                    0x11564,
                    0x12a8a
                };
                foreach (var offset in dllOffsets)
                {
                    if (dllBytes[offset] != 0x74)
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    dllBytes[offset] = 0xeb;
                }
                File.WriteAllBytes(NewDllPath, dllBytes);
                var dllName = Path.GetFileName(DllPath).Replace("d3d", "%s");
                if (string.IsNullOrEmpty(dllName))
                    throw new ArgumentNullException(nameof(dllName));
                const int exeOffset = 0x1ae898;
                var exeBytes = File.ReadAllBytes(ExePath);
                if (!Encoding.ASCII.GetString(exeBytes.Skip(exeOffset).Take(dllName.Length).ToArray()).EqualsEx(dllName))
                    throw new ArgumentOutOfRangeException(nameof(exeOffset));
                File.Copy(ExePath, NewExePath);
                using var exeWriter = new BinaryWriter(File.Open(NewExePath, FileMode.Open));
                exeWriter.BaseStream.Position = exeOffset;
                exeWriter.Write(Encoding.ASCII.GetBytes(Path.GetFileName(NewDllPath).Replace("d3d", "%s")));
                PatchIsActive = true;
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex, true, true);
                Cleanup();
            }
            if (!PatchIsActive)
                return;
            ExePath = NewExePath;
            FileEx.SetAttributes(NewExePath, FileAttributes.Hidden);
            FileEx.SetAttributes(NewDllPath, FileAttributes.Hidden);
        }

        private static void Cleanup()
        {
            try
            {
                FileEx.SetAttributes(NewExePath, FileAttributes.Normal);
                FileEx.Delete(NewExePath);
                FileEx.SetAttributes(NewDllPath, FileAttributes.Normal);
                FileEx.Delete(NewDllPath);
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex, true);
            }
        }

        public static void ProcessExit()
        {
            if (WindowHandle != IntPtr.Zero)
                WinApi.NativeHelper.CloseHandle(WindowHandle);
            ProcessEx.GetInstances(ExePath, true).ForEach(instance =>
            {
                try
                {
                    instance.Kill();
                }
                catch (Exception ex) when (ex.IsCaught())
                {
                    Log.Write(ex);
                }
            });
            Cleanup();
        }
    }
}
