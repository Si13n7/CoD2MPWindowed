
// Copyright(c) 2016 Si13n7 'Roy Schroedel' Developments(r)
// This file is licensed under the MIT License

#region '

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilDev
{
    /// <summary>Requirements:
    /// <para><see cref="SilDev.Crypt"/>.cs</para>
    /// <para><see cref="SilDev.Log"/>.cs</para>
    /// <seealso cref="SilDev"/></summary>
    public static class Convert
    {
        public enum NewLineFormat
        {
            CarriageReturn = '\u000D',
            FormFeed = '\u000C',
            LineFeed = '\u000A',
            LineSeparator = '\u2028',
            NextLine = '\u0085',
            ParagraphSeparator = '\u2029',
            VerticalTab = '\u000B',
            WindowsDefault = -1
        }

        public static string FormatNewLine(string text, NewLineFormat newLineFormat = NewLineFormat.WindowsDefault)
        {
            try
            {
                string[] sa = Enum.GetValues(typeof(NewLineFormat)).Cast<NewLineFormat>().Select(c => (int)c == -1 ? null : $"{(char)c.GetHashCode()}").ToArray();
                string f = (int)newLineFormat == -1 ? Environment.NewLine : $"{(char)newLineFormat.GetHashCode()}";
                string s = text.Replace(Environment.NewLine, $"{(char)NewLineFormat.LineFeed}");
                return string.Join(f, s.Split(sa, StringSplitOptions.None));
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return text;
            }
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string ReverseString(string text)
        {
            try
            {
                StringBuilder sb = new StringBuilder(text.Length);
                for (int i = text.Length - 1; i >= 0; i--)
                    sb.Append(text[i]);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return text;
            }
        }

        public static byte[] ReplaceBytes(byte[] source, byte[] oldValue, byte[] newValue)
        {
            try
            {
                byte[] ba;
                int index = -1;
                int match = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i] == oldValue[match])
                    {
                        if (match == oldValue.Length - 1)
                        {
                            index = i - match;
                            break;
                        }
                        match++;
                    }
                    else
                        match = 0;
                }
                index = match;
                if (index >= 0)
                {
                    ba = new byte[source.Length - oldValue.Length + newValue.Length];
                    Buffer.BlockCopy(source, 0, ba, 0, index);
                    Buffer.BlockCopy(newValue, 0, ba, index, newValue.Length);
                    Buffer.BlockCopy(source, index + oldValue.Length, ba, index + newValue.Length, source.Length - (index + oldValue.Length));
                    return ba;
                }
                throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return source;
            }
        }

        public static string ToBinaryString(string text, bool separator = true)
        {
            try
            {
                byte[] ba = Encoding.UTF8.GetBytes(text);
                string s = separator ? " " : string.Empty;
                s = string.Join(s, ba.Select(b => System.Convert.ToString(b, 2).PadLeft(8, '0')));
                return s;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string FromBinaryString(string bin)
        {
            try
            {
                string s = bin.Replace(" ", string.Empty);
                if (s.Count(c => c != '0' && c != '1') > 0)
                    throw new ArgumentException("s");
                List<byte> bl = new List<byte>();
                for (int i = 0; i < s.Length; i += 8)
                    bl.Add(System.Convert.ToByte(s.Substring(i, 8), 2));
                s = Encoding.UTF8.GetString(bl.ToArray());
                return s;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string ToHexString(string text, bool separator = true)
        {
            try
            {
                string s = ByteArrayToString(Encoding.UTF8.GetBytes(text));
                if (separator)
                {
                    int i = 0;
                    s = string.Join(" ", s.ToLookup(c => Math.Floor(i++ / 2d)).Select(e => new string(e.ToArray())));
                }
                return s;
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string FromHexString(string hex)
        {
            try
            {
                string s = hex.Replace(" ", string.Empty).ToLower();
                if (s.ToCharArray().Count(c => !"0123456789abcdef".Contains(c)) > 0)
                    throw new ArgumentOutOfRangeException();
                byte[] ba = new byte[s.Length / 2];
                for (int i = 0; i < ba.Length; i++)
                    ba[i] = System.Convert.ToByte(s.Substring(i * 2, 2), 16);
                return Encoding.UTF8.GetString(ba);
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return string.Empty;
            }
        }

        public static string[] StringToLogArray(string text)
        {
            try
            {
                int i = 0;
                double b = Math.Floor(Math.Log(text.Length));
                return text.ToLookup(c => Math.Floor(i++ / b)).Select(e => new string(e.ToArray())).ToArray();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
                return null;
            }
        }
    }
}

#endregion
