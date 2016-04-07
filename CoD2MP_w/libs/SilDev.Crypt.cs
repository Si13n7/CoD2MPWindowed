
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Convert"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Crypt
    {
        #region PRIVATE

        private static string BaseEncodeFilters(string input, string prefixMark, string suffixMark, uint lineLength)
        {
            string s = input;
            if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(prefixMark))
            {
                string prefix = prefixMark;
                string suffix = suffixMark;
                if (lineLength > 0)
                {
                    prefix = $"{prefix}{Environment.NewLine}";
                    suffix = $"{Environment.NewLine}{suffix}";
                }
                s = $"{prefix}{s}{suffix}";
            }
            if (lineLength > 1 & s.Length > lineLength)
            {
                int i = 0;
                s = string.Join(Environment.NewLine, s.ToLookup(c => Math.Floor(i++ / (double)lineLength)).Select(e => new string(e.ToArray())));
            }
            return s;
        }

        private static string BaseDecodeFilters(string input, string prefixMark, string suffixMark)
        {
            string s = input;
            if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
            {
                if (s.StartsWith(prefixMark))
                    s = s.Substring(prefixMark.Length);
                if (s.EndsWith(suffixMark))
                    s = s.Substring(0, s.Length - suffixMark.Length);
            }
            if (s.Contains('\r') || s.Contains('\n'))
                s = string.Concat(s.ToCharArray().Where(c => c != '\r' && c != '\n').ToArray());
            return s;
        }

        #endregion

        #region Base64

        public class Base64
        {
            public string PrefixMark = null;
            public string SuffixMark = null;
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    LastEncodedResult = System.Convert.ToBase64String(bytes);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = null, string suffixMark = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    LastDecodedResult = System.Convert.FromBase64String(code);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    byte[] ba = Encoding.UTF8.GetBytes(text);
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, uint lineLength) =>
                EncodeString(text, null, null, lineLength);

            public string DecodeString(string code, string prefixMark = null, string suffixMark = null)
            {
                try
                {
                    byte[] ba = DecodeByteArray(code, prefixMark, suffixMark);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, uint lineLength) =>
                EncodeFile(path, null, null, lineLength);

            public byte[] DecodeFile(string code, string prefixMark = null, string suffixMark = null) =>
                DecodeByteArray(code, prefixMark, suffixMark);
        }

        #endregion

        #region Base85

        public class Base85
        {
            private static uint[] p85 = 
            {
                85 * 85 * 85 * 85,
                85 * 85 * 85,
                85 * 85,
                85,
                1
            };

            private static byte[] encodeBlock = new byte[5];
            private static byte[] decodeBlock = new byte[4];

            public string PrefixMark = "<~";
            public string SuffixMark = "~>";
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    if (prefixMark != "<~" && suffixMark != "~>")
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    StringBuilder sb = new StringBuilder();
                    uint t = 0;
                    int n = 0;
                    foreach (byte b in bytes)
                    {
                        if (n + 1 < decodeBlock.Length)
                        {
                            t |= (uint)(b << (24 - (n * 8)));
                            n++;
                            continue;
                        }
                        t |= b;
                        if (t == 0)
                            sb.Append((char)122);
                        else
                        {
                            for (int i = encodeBlock.Length - 1; i >= 0; i--)
                            {
                                encodeBlock[i] = (byte)((t % 85) + 33);
                                t /= 85;
                            }
                            for (int i = 0; i < encodeBlock.Length; i++)
                                sb.Append((char)encodeBlock[i]);
                        }
                        t = 0;
                        n = 0;
                    }
                    if (n > 0)
                    {
                        for (int i = encodeBlock.Length - 1; i >= 0; i--)
                        {
                            encodeBlock[i] = (byte)((t % 85) + 33);
                            t /= 85;
                        }
                        for (int i = 0; i <= n; i++)
                            sb.Append((char)encodeBlock[i]);
                    }
                    LastEncodedResult = sb.ToString();
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = "<~", string suffixMark = "~>")
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        char[] ca = new char[] 
                        {
                            (char)0, (char)8, (char)9,
                            (char)10, (char)12, (char)13
                        };
                        int n = 0;
                        uint t = 0;
                        foreach (char c in code)
                        {
                            if (c == (char)122)
                            {
                                if (n != 0)
                                    throw new ArgumentException("n");
                                for (int i = 0; i < 4; i++)
                                    decodeBlock[i] = 0;
                                ms.Write(decodeBlock, 0, decodeBlock.Length);
                                continue;
                            }
                            if (ca.Contains(c))
                                continue;
                            if (c < (char)33 || c > (char)117)
                                throw new ArgumentOutOfRangeException("c");
                            t += (uint)((c - 33) * p85[n]);
                            n++;
                            if (n == encodeBlock.Length)
                            {
                                for (int i = 0; i < decodeBlock.Length; i++)
                                    decodeBlock[i] = (byte)(t >> 24 - (i * 8));
                                ms.Write(decodeBlock, 0, decodeBlock.Length);
                                t = 0;
                                n = 0;
                            }
                        }
                        if (n != 0)
                        {
                            if (n == 1)
                                throw new NotSupportedException("n");
                            n--;
                            t += p85[n];
                            for (int i = 0; i < n; i++)
                                decodeBlock[i] = (byte)(t >> 24 - (i * 8));
                            for (int i = 0; i < n; i++)
                                ms.WriteByte(decodeBlock[i]);
                        }
                        LastDecodedResult = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    byte[] ba = Encoding.UTF8.GetBytes(text);
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, uint lineLength) =>
                EncodeString(text, null, null, lineLength);

            public string DecodeString(string code, string prefixMark = "<~", string suffixMark = "~>")
            {
                try
                {
                    byte[] ba = DecodeByteArray(code, prefixMark, suffixMark);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark = "<~", string suffixMark = "~>", uint lineLength = 0)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, uint lineLength) =>
                EncodeFile(path, null, null, lineLength);

            public byte[] DecodeFile(string code, string prefixMark = "<~", string suffixMark = "~>") =>
                DecodeByteArray(code, prefixMark, suffixMark);
        }

        #endregion

        #region Base91

        public class Base91
        {
            private static uint[] a91 = new uint[]
            {
                 65,  66,  67,  68,  69,  70,  71,  72,  73,
                 74,  75,  76,  77,  78,  79,  80,  81,  82,
                 83,  84,  85,  86,  87,  88,  89,  90,  97,
                 98,  99, 100, 101, 102, 103, 104, 105, 106,
                107, 108, 109, 110, 111, 112, 113, 114, 115,
                116, 117, 118, 119, 120, 121, 122,  48,  49,
                 50,  51,  52,  53,  54,  55,  56,  57,  33,
                 35,  36,  37,  38,  40,  41,  42,  43,  44,
                 45,  46,  58,  59,  60,  61,  62,  63,  64,
                 91,  93,  94,  95,  96, 123, 124, 125, 126,
                 34
            };

            public static char[] defaultEncodeTable;

            public char[] DefaultEncodeTable
            {
                get
                {
                    if (defaultEncodeTable == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (uint i in a91)
                            sb.Append((char)i);
                        defaultEncodeTable = sb.ToString().ToCharArray();
                    }
                    return defaultEncodeTable;
                }
            }

            private char[] encodeTable;

            public char[] EncodeTable
            {
                get
                {
                    if (encodeTable == null)
                        encodeTable = DefaultEncodeTable;
                    return encodeTable;
                }
                set
                {
                    try
                    {
                        value = value.Distinct().ToArray();
                        if (value.Length < 91)
                            throw new ArgumentException("value");
                        if (value.Length > 91)
                            value = new List<char>(value).GetRange(0, 91).ToArray();
                        encodeTable = value;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex);
                        encodeTable = DefaultEncodeTable;
                    }
                }
            }

            private Dictionary<byte, int> decodeTable;

            private void InitializeTables()
            {
                if (encodeTable == null)
                    encodeTable = EncodeTable;
                decodeTable = new Dictionary<byte, int>();
                for (int i = 0; i < 255; i++)
                    decodeTable[(byte)i] = -1;
                for (int i = 0; i < encodeTable.Length; i++)
                    decodeTable[(byte)encodeTable[i]] = i;
            }

            public string PrefixMark = null;
            public string SuffixMark = null;
            public uint LineLength = 0;

            public string LastEncodedResult { get; private set; }
            public byte[] LastDecodedResult { get; private set; }

            public string EncodeByteArray(byte[] bytes, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (lineLength > 0)
                        LineLength = lineLength;
                    if (encodeTable != null)
                        EncodeTable = encodeTable;
                    InitializeTables();
                    StringBuilder sb = new StringBuilder();
                    int[] ia = new int[] { 0, 0, 0 };
                    foreach (byte b in bytes)
                    {
                        ia[0] |= b << ia[1];
                        ia[1] += 8;
                        if (ia[1] < 14)
                            continue;
                        ia[2] = ia[0] & 8191;
                        if (ia[2] > 88)
                        {
                            ia[1] -= 13;
                            ia[0] >>= 13;
                        }
                        else
                        {
                            ia[2] = ia[0] & 16383;
                            ia[1] -= 14;
                            ia[0] >>= 14;
                        }
                        sb.Append(this.encodeTable[ia[2] % 91]);
                        sb.Append(this.encodeTable[ia[2] / 91]);
                    }
                    if (ia[1] != 0)
                    {
                        sb.Append(this.encodeTable[ia[0] % 91]);
                        if (ia[1] >= 8 || ia[0] >= 91)
                            sb.Append(this.encodeTable[ia[0] / 91]);
                    }
                    LastEncodedResult = sb.ToString();
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, null, null, LineLength);
                    LastEncodedResult = BaseEncodeFilters(LastEncodedResult, PrefixMark, SuffixMark, (uint)(LineLength > 1 ? 1 : 0));
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastEncodedResult = string.Empty;
                }
                return LastEncodedResult;
            }

            public byte[] DecodeByteArray(string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefixMark) && !string.IsNullOrEmpty(suffixMark))
                    {
                        PrefixMark = prefixMark;
                        SuffixMark = suffixMark;
                    }
                    if (encodeTable != null)
                        EncodeTable = encodeTable;
                    code = BaseDecodeFilters(code, PrefixMark, SuffixMark);
                    InitializeTables();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int[] ia = new int[] { 0, -1, 0, 0 };
                        foreach (char c in code)
                        {
                            if (this.encodeTable.Count(e => e == (byte)c) == 0)
                                throw new ArgumentOutOfRangeException("c");
                            ia[0] = decodeTable[(byte)c];
                            if (ia[0] == -1)
                                continue;
                            if (ia[1] < 0)
                            {
                                ia[1] = ia[0];
                                continue;
                            }
                            ia[1] += ia[0] * 91;
                            ia[2] |= ia[1] << ia[3];
                            ia[3] += (ia[1] & 8191) > 88 ? 13 : 14;
                            do
                            {
                                ms.WriteByte((byte)(ia[2] & 255));
                                ia[2] >>= 8;
                                ia[3] -= 8;
                            }
                            while (ia[3] > 7);
                            ia[1] = -1;
                        }
                        if (ia[1] != -1)
                            ms.WriteByte((byte)((ia[2] | ia[1] << ia[3]) & 255));
                        LastDecodedResult = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    LastDecodedResult = null;
                }
                return LastDecodedResult;
            }

            public string EncodeString(string text, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    byte[] ba = Encoding.UTF8.GetBytes(text);
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength, encodeTable) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeString(string text, string prefixMark, string suffixMark, char[] encodeTable) =>
                EncodeString(text, prefixMark, suffixMark, 0, encodeTable);

            public string EncodeString(string text, uint lineLength, char[] encodeTable) =>
                EncodeString(text, null, null, lineLength, encodeTable);

            public string EncodeString(string text, char[] encodeTable) =>
                EncodeString(text, null, null, 0, encodeTable);

            public string DecodeString(string text, string prefixMark = null, string suffixMark = null, char[] encodeTable = null)
            {
                try
                {
                    byte[] ba = DecodeByteArray(text, prefixMark, suffixMark, encodeTable);
                    return Encoding.UTF8.GetString(ba) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string DecodeString(string text, char[] encodeTable) =>
                DecodeString(text, null, null, encodeTable);

            public string EncodeFile(string path, string prefixMark = null, string suffixMark = null, uint lineLength = 0, char[] encodeTable = null)
            {
                try
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException();
                    byte[] ba = null;
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        ba = new byte[fs.Length];
                        fs.Read(ba, 0, (int)fs.Length);
                    }
                    return EncodeByteArray(ba, prefixMark, suffixMark, lineLength, encodeTable);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public string EncodeFile(string path, string prefixMark, string suffixMark, char[] encodeTable) =>
                EncodeFile(path, prefixMark, suffixMark, 0, encodeTable);

            public string EncodeFile(string path, uint lineLength, char[] encodeTable) =>
                EncodeFile(path, null, null, lineLength, encodeTable);

            public string EncodeFile(string path, char[] encodeTable) =>
                EncodeFile(path, null, null, 0, encodeTable);

            public byte[] DecodeFile(string code, string prefixMark = null, string suffixMark = null, char[] encodeTable = null) =>
                DecodeByteArray(code, prefixMark, suffixMark, encodeTable);

            public byte[] DecodeFile(string code, char[] encodeTable) =>
                DecodeFile(code, null, null, encodeTable);
        }

        #endregion

        #region Message-Digest 5

        public static class MD5
        {
            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.MD5.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.Replace("-", string.Empty).ToLower();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(Encoding.UTF8.GetBytes(text));

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (var csp = new System.Security.Cryptography.MD5CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return Convert.ByteArrayToString(ba);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Secure Hash Algorithm 1

        public static class SHA1
        {
            public static string EncryptStream(Stream stream)
            {
                try
                {
                    byte[] ba;
                    using (var csp = new System.Security.Cryptography.SHA1CryptoServiceProvider())
                        ba = csp.ComputeHash(stream);
                    return Convert.ByteArrayToString(ba);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    string s;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Read(bytes, 0, bytes.Length);
                        s = EncryptStream(ms);
                    }
                    return s;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text)
            {
                try
                {
                    byte[] ba = Encoding.UTF8.GetBytes(text);
                    using (var csp = System.Security.Cryptography.SHA1.Create())
                        ba = csp.ComputeHash(Encoding.UTF8.GetBytes(text));
                    string s = BitConverter.ToString(ba);
                    return s.Replace("-", string.Empty).ToLower();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptFile(string path)
            {
                try
                {
                    string s;
                    using (FileStream fs = File.OpenRead(path))
                        s = EncryptStream(fs);
                    return s;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Secure Hash Algorithm 2

        public static class SHA256
        {
            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA256.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.Replace("-", string.Empty).ToLower();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(Encoding.UTF8.GetBytes(text));

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (var csp = new System.Security.Cryptography.SHA256CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return Convert.ByteArrayToString(ba);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static class SHA384
        {
            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA384.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.Replace("-", string.Empty).ToLower();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(Encoding.UTF8.GetBytes(text));

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (var csp = new System.Security.Cryptography.SHA384CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return Convert.ByteArrayToString(ba);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }
        }

        public static class SHA512
        {
            public static string EncryptByteArray(byte[] bytes)
            {
                try
                {
                    byte[] ba;
                    using (var csp = System.Security.Cryptography.SHA512.Create())
                        ba = csp.ComputeHash(bytes);
                    string s = BitConverter.ToString(ba);
                    return s.Replace("-", string.Empty).ToLower();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }

            public static string EncryptString(string text) =>
                EncryptByteArray(Encoding.UTF8.GetBytes(text));

            public static string EncryptFile(string path)
            {
                try
                {
                    byte[] ba;
                    using (var csp = new System.Security.Cryptography.SHA512CryptoServiceProvider())
                    {
                        using (FileStream fs = File.OpenRead(path))
                            ba = csp.ComputeHash(fs);
                    }
                    return Convert.ByteArrayToString(ba);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return string.Empty;
                }
            }
        }

        #endregion
    }
}

#endregion
