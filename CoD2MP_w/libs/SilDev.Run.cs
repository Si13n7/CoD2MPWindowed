
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Run
    {
        public enum MachineType : ushort
        {
            UNKNOWN = 0x0,
            AM33 = 0x1d3,
            AMD64 = 0x8664,
            ARM = 0x1c0,
            EBC = 0xebc,
            I386 = 0x14c,
            IA64 = 0x200,
            M32R = 0x9041,
            MIPS16 = 0x266,
            MIPSFPU = 0x366,
            MIPSFPU16 = 0x466,
            POWERPC = 0x1f0,
            POWERPCFP = 0x1f1,
            R4000 = 0x166,
            SH3 = 0x1a2,
            SH3DSP = 0x1a3,
            SH4 = 0x1a6,
            SH5 = 0x1a8,
            THUMB = 0x1c2,
            WCEMIPSV2 = 0x169,
        }

        public static MachineType GetPEArchitecture(string path)
        {
            MachineType mt = MachineType.UNKNOWN;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader br = new BinaryReader(fs);
                    fs.Seek(0x3c, SeekOrigin.Begin);
                    fs.Seek(br.ReadInt32(), SeekOrigin.Begin);
                    br.ReadUInt32();
                    mt = (MachineType)br.ReadUInt16();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return mt;
        }

        public static bool Is64Bit(string path)
        {
            MachineType mt = GetPEArchitecture(path);
            return mt == MachineType.AMD64 || mt == MachineType.IA64;
        }

        private static List<string> cmdLineArgs = new List<string>();
        public static List<string> CommandLineArgs(bool sort = true, int skip = 1)
        {
            if (cmdLineArgs.Count != Environment.GetCommandLineArgs().Length - skip)
            {
                List<string> filteredArgs = new List<string>();
                try
                {
                    if (Environment.GetCommandLineArgs().Length > skip)
                    {
                        List<string> defaultArgs = Environment.GetCommandLineArgs().Skip(skip).ToList();
                        if (sort)
                            defaultArgs.Sort();
                        bool debugArg = false;
                        foreach (string arg in defaultArgs)
                        {
                            if (arg.StartsWith("/debug", StringComparison.OrdinalIgnoreCase) || debugArg)
                            {
                                debugArg = !debugArg;
                                continue;
                            }
                            filteredArgs.Add(arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg);
                        }
                        cmdLineArgs = filteredArgs;
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                }
            }
            return cmdLineArgs;
        }

        public static List<string> CommandLineArgs(int skip) =>
            CommandLineArgs(true, skip);

        private static string commandLine = string.Empty;
        public static string CommandLine(bool sort = true, int skip = 1)
        {
            if (CommandLineArgs(sort).Count > 0)
                commandLine = string.Join(" ", CommandLineArgs(sort, skip));
            return commandLine;
        }

        public static string CommandLine(int skip) =>
            CommandLine(true, skip);

        public static string EnvironmentVariableFilter(params string[] paths)
        {
            string path = Path.Combine(paths);
            try
            {
                path = Path.GetInvalidPathChars().Aggregate(path.Trim(), (current, c) => current.Replace(c.ToString(), string.Empty));
                if (path.StartsWith("%") && (path.Contains("%\\") || path.EndsWith("%")))
                {
                    string variable = Regex.Match(path, "%(.+?)%", RegexOptions.IgnoreCase).Groups[1].Value;
                    string varDir = string.Empty;
                    switch (variable.ToLower())
                    {
                        case "commonstartmenu":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                            break;
                        case "commonstartup":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                            break;
                        case "currentdir":
                            varDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
                            break;
                        case "desktopdir":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            break;
                        case "mydocuments":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            break;
                        case "mymusic":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            break;
                        case "mypictures":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                            break;
                        case "myvideos":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                            break;
                        case "sendto":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
                            break;
                        case "startmenu":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                            break;
                        case "startup":
                            varDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                            break;
                        default:
                            varDir = Environment.GetEnvironmentVariable(variable.ToLower());
                            break;
                    }
                    path = path.Replace($"%{variable}%", varDir);
                }
                if (path.Contains("..\\"))
                {
                    try
                    {
                        path = Path.GetFullPath(path);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                }
                while (path.Contains("\\\\"))
                    path = path.Replace("\\\\", "\\");
                path = path.EndsWith("\\") ? path.Substring(0, path.Length - 1) : path;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return path;
        }

        public static string LastStreamOutput { get; private set; }

        public static int App(ProcessStartInfo psi, int? waitForInputIdle, int? waitForExit)
        {
            try
            {
                int pid = -1;
                using (Process p = new Process() { StartInfo = psi })
                {
                    p.StartInfo.FileName = EnvironmentVariableFilter(p.StartInfo.FileName);
                    if (!File.Exists(p.StartInfo.FileName))
                        throw new FileNotFoundException($"File '{p.StartInfo.FileName}' does not exists.");
                    p.StartInfo.WorkingDirectory = EnvironmentVariableFilter(p.StartInfo.WorkingDirectory);
                    if (!Directory.Exists(p.StartInfo.WorkingDirectory))
                        p.StartInfo.WorkingDirectory = Path.GetDirectoryName(p.StartInfo.FileName);
                    if (!p.StartInfo.UseShellExecute && !p.StartInfo.CreateNoWindow && p.StartInfo.WindowStyle == ProcessWindowStyle.Hidden)
                        p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    try
                    {
                        if (!p.StartInfo.UseShellExecute && p.StartInfo.RedirectStandardOutput)
                            LastStreamOutput = p.StandardOutput.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                    try
                    {
                        if (waitForInputIdle != null && !p.HasExited)
                        {
                            if (waitForInputIdle <= 0)
                                waitForInputIdle = -1;
                            p.WaitForInputIdle((int)waitForInputIdle);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                    }
                    if (waitForExit != null && !p.HasExited)
                    {
                        if (waitForExit <= 0)
                            waitForExit = -1;
                        p.WaitForExit((int)waitForExit);
                    }
                    pid = p.Id;
                }
                return pid;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return -1;
        }

        public static int App(ProcessStartInfo psi, int? waitForExit = null) => 
            App(psi, null, waitForExit);

        public static void Cmd(string command, bool runAsAdmin, int? waitForExit = null)
        {
            try
            {
                string cmd = command.TrimStart();
                if (cmd.StartsWith("/K", StringComparison.OrdinalIgnoreCase))
                    cmd = cmd.Substring(2).Trim();
                if (!cmd.StartsWith("/C", StringComparison.OrdinalIgnoreCase))
                    cmd = $"/C {cmd}";
                if (cmd.Length <= 3)
                    throw new ArgumentNullException();
                App(new ProcessStartInfo()
                {
                    Arguments = cmd,
                    FileName = "%WinDir%\\System32\\cmd.exe",
                    Verb = runAsAdmin ? "runas" : string.Empty,
                    WindowStyle = Log.DebugMode < 2 ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                }, waitForExit);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static void Cmd(string command, int? waitForExit = null) => 
            Cmd(command, false, waitForExit);
    }
}

#endregion
