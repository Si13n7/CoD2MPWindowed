
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Ini
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int GetPrivateProfileInt([MarshalAs(UnmanagedType.LPStr)]string lpApplicationName, [MarshalAs(UnmanagedType.LPStr)]string lpKeyName, int nDefault, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string nDefault, StringBuilder retVal, int nSize, string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string nDefault, string retVal, int nSize, string lpFileName);

            [DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Ansi)]
            internal static extern int WritePrivateProfileSection([MarshalAs(UnmanagedType.LPStr)]string lpAppName, [MarshalAs(UnmanagedType.LPStr)]string lpString, [MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);
        }

        #region DEFAULT ACCESS FILE

        private static string iniFile = null;

        public static bool File(params string[] paths)
        {
            try
            {
                iniFile = Path.Combine(paths);
                if (System.IO.File.Exists(iniFile))
                    return true;
                string iniDir = Path.GetDirectoryName(iniFile);
                if (!Directory.Exists(iniDir))
                    Directory.CreateDirectory(iniDir);
                System.IO.File.Create(iniFile).Close();
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static string File() => 
            iniFile != null ? iniFile : string.Empty;

        #endregion

        #region SECTION ORDER

        public static List<string> GetSections(string fileOrContent = null, bool sorted = true)
        {
            List<string> output = new List<string>();
            try
            {
                string iniPath = fileOrContent ?? iniFile;
                if (System.IO.File.Exists(iniPath))
                {
                    byte[] buffer = new byte[short.MaxValue];
                    if (SafeNativeMethods.GetPrivateProfileSectionNames(buffer, short.MaxValue, iniPath) != 0)
                        output = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0').ToList();
                }
                else
                {
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetRandomFileName());
                    System.IO.File.WriteAllText(path, iniPath);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetSections(path, sorted);
                        System.IO.File.Delete(path);
                    }
                }
                if (sorted)
                    output.Sort();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static List<string> GetSections(bool sorted) =>
            GetSections(iniFile, sorted);

        #endregion

        #region REMOVE SECTION

        public static bool RemoveSection(string section, string file = null)
        {
            try
            {
                if (!System.IO.File.Exists(file ?? iniFile))
                    throw new FileNotFoundException();
                return SafeNativeMethods.WritePrivateProfileSection(section, null, file ?? iniFile) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        #endregion

        #region KEY ORDER

        public static List<string> GetKeys(string section, string fileOrContent = null, bool sorted = true)
        {
            List<string> output = new List<string>();
            try
            {
                string iniPath = fileOrContent ?? iniFile;
                if (System.IO.File.Exists(iniPath))
                {
                    string tmp = new string(' ', short.MaxValue);
                    if (SafeNativeMethods.GetPrivateProfileString(section, null, string.Empty, tmp, short.MaxValue, iniPath) != 0)
                    {
                        output = new List<string>(tmp.Split('\0'));
                        output.RemoveRange(output.Count - 2, 2);
                    }
                }
                else
                {
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetRandomFileName());
                    System.IO.File.WriteAllText(path, iniPath);
                    if (System.IO.File.Exists(path))
                    {
                        output = GetKeys(section, path, sorted);
                        System.IO.File.Delete(path);
                    }
                }
                if (sorted)
                    output.Sort();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static List<string> GetKeys(string section, bool sorted) =>
            GetKeys(section, iniFile, sorted);

        #endregion

        #region REMOVE KEY

        public static bool RemoveKey(string section, string key, string file = null)
        {
            try
            {
                if (!System.IO.File.Exists(file ?? iniFile))
                    throw new FileNotFoundException();
                return SafeNativeMethods.WritePrivateProfileString(section, key, null, file ?? iniFile) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        #endregion

        #region READ ALL

        public static Dictionary<string, Dictionary<string, string>> ReadAll(string fileOrContent = null, bool sorted = true)
        {
            Dictionary<string, Dictionary<string, string>> output = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                bool isContent = false;
                string path = fileOrContent ?? iniFile;
                if (!System.IO.File.Exists(path))
                {
                    isContent = true;
                    path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetRandomFileName());
                    System.IO.File.WriteAllText(path, fileOrContent);
                }
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                List<string> sections = GetSections(path, sorted);
                if (sections.Count == 0)
                    throw new ArgumentNullException();
                foreach (string section in sections)
                {
                    List<string> keys = GetKeys(section, path, sorted);
                    if (keys.Count == 0)
                        continue;
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    foreach (string key in keys)
                    {
                        string value = Read(section, key, path);
                        if (string.IsNullOrWhiteSpace(value))
                            continue;
                        values.Add(key, value);
                    }
                    if (values.Count == 0)
                        continue;
                    if (!output.ContainsKey(section))
                        output.Add(section, values);
                }
                if (isContent)
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        public static Dictionary<string, Dictionary<string, string>> ReadAll(bool sorted) =>
            ReadAll(iniFile, sorted);

        #endregion

        #region READ VALUE

        public static bool ValueExists(string section, string key, string fileOrContent = null) =>
            !string.IsNullOrWhiteSpace(Read(section, key, fileOrContent ?? iniFile));

        public static string Read(string section, string key, string fileOrContent = null)
        {
            string output = string.Empty;
            try
            {
                string iniPath = fileOrContent ?? iniFile;
                if (System.IO.File.Exists(iniPath))
                {
                    StringBuilder tmp = new StringBuilder(short.MaxValue);
                    if (SafeNativeMethods.GetPrivateProfileString(section, key, string.Empty, tmp, short.MaxValue, iniPath) != 0)
                        output = tmp.ToString();
                }
                else
                {
                    string path = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Path.GetRandomFileName());
                    System.IO.File.WriteAllText(path, iniPath);
                    if (System.IO.File.Exists(path))
                    {
                        output = Read(section, key, path);
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
            return output;
        }

        private enum IniValueKind
        {
            Boolean,
            Byte,
            ByteArray,
            DateTime,
            Double,
            Float,
            Integer,
            Long,
            Short,
            String,
            Version
        }

        private static object ReadObject(string section, string key, object defValue, IniValueKind valkind, string fileOrContent)
        {
            object output = null;
            string value = Read(section, key, fileOrContent);
            switch (valkind)
            {
                case IniValueKind.Boolean:
                    bool boolParser;
                    if (bool.TryParse(Read(section, key, fileOrContent), out boolParser))
                        output = boolParser;
                    break;
                case IniValueKind.Byte:
                    byte byteParser;
                    if (byte.TryParse(Read(section, key, fileOrContent), out byteParser))
                        output = byteParser;
                    break;
                case IniValueKind.ByteArray:
                    byte[] bytesParser = Enumerable.Range(0, value.Length).Where(x => x % 2 == 0).Select(x => System.Convert.ToByte(value.Substring(x, 2), 16)).ToArray();
                    if (bytesParser.Length > 0)
                        output = bytesParser;
                    break;
                case IniValueKind.DateTime:
                    DateTime dateTimeParser;
                    if (DateTime.TryParse(Read(section, key, fileOrContent), out dateTimeParser))
                        output = dateTimeParser;
                    break;
                case IniValueKind.Double:
                    double doubleParser;
                    if (double.TryParse(Read(section, key, fileOrContent), out doubleParser))
                        output = doubleParser;
                    break;
                case IniValueKind.Float:
                    float floatParser;
                    if (float.TryParse(Read(section, key, fileOrContent), out floatParser))
                        output = floatParser;
                    break;
                case IniValueKind.Integer:
                    int intParser;
                    if (int.TryParse(Read(section, key, fileOrContent), out intParser))
                        output = intParser;
                    break;
                case IniValueKind.Long:
                    long longParser;
                    if (long.TryParse(Read(section, key, fileOrContent), out longParser))
                        output = longParser;
                    break;
                case IniValueKind.Short:
                    short shortParser;
                    if (short.TryParse(Read(section, key, fileOrContent), out shortParser))
                        output = shortParser;
                    break;
                case IniValueKind.Version:
                    Version versionParser;
                    if (Version.TryParse(Read(section, key, fileOrContent), out versionParser))
                        output = versionParser;
                    break;
                default:
                    output = Read(section, key, fileOrContent);
                    if (string.IsNullOrWhiteSpace(output as string))
                        output = null;
                    break;
            }

            if (output == null)
                output = defValue;
            return output;
        }


        public static bool ReadBoolean(string section, string key, bool defValue = false, string fileOrContent = null) =>
            System.Convert.ToBoolean(ReadObject(section, key, defValue, IniValueKind.Boolean, fileOrContent ?? iniFile));

        public static bool ReadBoolean(string section, string key, string fileOrContent) =>
            ReadBoolean(section, key, false, fileOrContent);


        public static byte ReadByte(string section, string key, byte defValue = 0x0, string fileOrContent = null) =>
            System.Convert.ToByte(ReadObject(section, key, defValue, IniValueKind.Byte, fileOrContent ?? iniFile));

        public static byte ReadByte(string section, string key, string fileOrContent) =>
            ReadByte(section, key, 0x0, fileOrContent);


        public static byte[] ReadByteArray(string section, string key, byte[] defValue = null, string fileOrContent = null) =>
            ReadObject(section, key, defValue, IniValueKind.ByteArray, fileOrContent ?? iniFile) as byte[];

        public static byte[] ReadByteArray(string section, string key, string fileOrContent) =>
            ReadByteArray(section, key, null, fileOrContent);


        public static DateTime ReadDateTime(string section, string key, DateTime defValue, string fileOrContent = null) =>
            System.Convert.ToDateTime(ReadObject(section, key, defValue, IniValueKind.DateTime, fileOrContent ?? iniFile));

        public static DateTime ReadDateTime(string section, string key, string fileOrContent) =>
            ReadDateTime(section, key, DateTime.Now, fileOrContent);

        public static DateTime ReadDateTime(string section, string key) =>
            ReadDateTime(section, key, DateTime.Now, iniFile);


        public static double ReadDouble(string section, string key, double defValue = 0d, string fileOrContent = null) =>
            System.Convert.ToDouble(ReadObject(section, key, defValue, IniValueKind.Double, fileOrContent ?? iniFile));

        public static double ReadDouble(string section, string key, string fileOrContent) =>
            ReadDouble(section, key, 0d, fileOrContent);


        public static float ReadFloat(string section, string key, float defValue = 0f, string fileOrContent = null) =>
            System.Convert.ToSingle(ReadObject(section, key, defValue, IniValueKind.Float, fileOrContent ?? iniFile));

        public static double ReadFloat(string section, string key, string fileOrContent) =>
            ReadFloat(section, key, 0f, fileOrContent);


        public static int ReadInteger(string section, string key, int defValue = 0, string fileOrContent = null) =>
            System.Convert.ToInt32(ReadObject(section, key, defValue, IniValueKind.Integer, fileOrContent ?? iniFile));

        public static int ReadInteger(string section, string key, string fileOrContent) =>
            ReadInteger(section, key, 0, fileOrContent);


        public static long ReadLong(string section, string key, long defValue = 0, string fileOrContent = null) =>
            System.Convert.ToInt64(ReadObject(section, key, defValue, IniValueKind.Long, fileOrContent ?? iniFile));

        public static long ReadLong(string section, string key, string fileOrContent) =>
            ReadLong(section, key, 0, fileOrContent);


        public static short ReadShort(string section, string key, short defValue = 0, string fileOrContent = null) =>
            System.Convert.ToInt16(ReadObject(section, key, defValue, IniValueKind.Short, fileOrContent ?? iniFile));

        public static short ReadShort(string section, string key, string fileOrContent) =>
            ReadShort(section, key, 0, fileOrContent);


        public static string ReadString(string section, string key, string defValue = "", string fileOrContent = null) =>
            System.Convert.ToString(ReadObject(section, key, defValue, IniValueKind.String, fileOrContent));


        public static Version ReadVersion(string section, string key, Version defValue, string fileOrContent = null) =>
            Version.Parse(ReadObject(section, key, defValue, IniValueKind.Version, fileOrContent ?? iniFile).ToString());

        public static Version ReadVersion(string section, string key, string fileOrContent) =>
            ReadVersion(section, key, Version.Parse("0.0.0.0"), fileOrContent);

        public static Version ReadVersion(string section, string key) =>
            ReadVersion(section, key, Version.Parse("0.0.0.0"), iniFile);

        #endregion

        #region WRITE VALUE

        public static bool Write(string section, string key, object value, string file = null, bool forceOverwrite = true, bool skipExistValue = false)
        {
            try
            {
                string path = file ?? iniFile;
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException();
                if (value == null)
                    return RemoveKey(section, key, path);
                string newValue = value.ToString();
                if (value is byte[])
                    newValue = Convert.ByteArrayToString((byte[])value);
                if (!forceOverwrite || skipExistValue)
                {
                    string curValue = Read(section, key, path);
                    if (!forceOverwrite && curValue == newValue || skipExistValue && !string.IsNullOrWhiteSpace(curValue))
                        return false;
                }
                return SafeNativeMethods.WritePrivateProfileString(section, key, newValue, path) != 0;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return false;
            }
        }

        public static bool Write(string section, string key, object value, bool forceOverwrite, bool skipExistValue = false) =>
            Write(section, key, value, iniFile, forceOverwrite, skipExistValue);

        #endregion
    }
}

#endregion
