using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JaszCore.Utils
{
    public static class StringUtils
    {

        public static string NotNull(this string val)
        {
            return val ?? string.Empty;
        }

        public static bool IsDefined(this string val)
        {
            return !val.IsEmpty();
        }

        public static bool IsEmpty(this string val)
        {
            return string.IsNullOrEmpty(val) || val.Trim().Equals(string.Empty);
        }

        public static bool SafeEquals(this string s1, string s2)
        {
            if (s1 == s2)
            {
                return true;
            }
            return s1 != null ? s1.Equals(s2) : false;
        }

        public static bool ContainsAnyIgnoreCase(this string text, params string[] parts)
        {
            if (text.IsEmpty() || parts.IsEmpty())
            {
                return false;
            }
            text = text.ToLower();

            foreach (string s in parts)
            {
                if (s == null)
                {
                    continue;
                }
                if (text.IndexOf(s.ToLower()) >= 0)
                {
                    return true;
                }
            }
            return false;
        }


        public static string FileNameSafeMd5Hash(this string input)
        {
            return FileNameSafeStringifyByteHash(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        private static string FileNameSafeStringifyByteHash(byte[] byteHash)
        {
            return Convert.ToBase64String(byteHash).Replace('/', '-');
        }

        public static string ToFileName(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).Replace('+', '-').Replace('/', '_').Replace('\\', '_').Replace("=", string.Empty);
        }

        public static bool Matches(this string value, string pattern)
        {
            return Regex.IsMatch(value, pattern);
        }

        public static string Repeat(this string value, int times)
        {
            return Repeat(value, null, times);
        }
        public static string Repeat(this string value, string separator, int times)
        {
            string result = string.Empty;
            for (int i = 0; i < times; i++)
            {
                result += value;
                if (i < times - 1 && separator != null)
                {
                    result += separator;
                }
            }
            return result;
        }


        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input.First().ToString().ToUpper() + string.Join("", input.Skip(1)).ToLower();
        }

        public static string ToTitleCase(this string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            if (string.IsNullOrEmpty(input))
                return "";
            return textInfo.ToTitleCase(input.ToLower());
        }

        public static string TrimStringToSize(string input, int maxSize = 100)
        {
            var trimSize = input.Length > maxSize ? input.Substring(0, maxSize) : input;
            return trimSize;
        }

        public static int GetStringBuilderIndexOf(StringBuilder sb, string value, int startIndex, bool ignoreCase)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                            ++index;
                        if (index == length)
                            return i;
                    }
                }
                return -1;
            }
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (sb[i + index] == value[index]))
                        ++index;
                    if (index == length)
                        return i;
                }
            }
            return -1;
        }
    }
}