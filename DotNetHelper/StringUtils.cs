using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DotNetHelper.Properties;

namespace DotNetHelper
{
    /// <summary>
    ///     Mostly extensions for common string manipulation.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        ///     Appends the specified text to an existing or new file.
        /// </summary>
        /// <param name="contentToWrite">String content to write to the file.</param>
        /// <param name="fileToWriteTo">FilePath to write to.</param>
        /// <returns>A new FilePath that contains the specified string. Null if writing failed.</returns>
        public static FilePath AppendToFile(this string contentToWrite, FilePath fileToWriteTo)
        {
            try
            {
                File.AppendAllText(fileToWriteTo, contentToWrite);
            }
            catch (Exception exception)
            {
                return null;
            }

            return fileToWriteTo;
        }

        /// <summary>
        ///     Returns the substring contained between the start and end index provided.
        ///     (Like .SubString() but with an end index as opposed to the length)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string Between(this string input, int startIndex, int endIndex)
        {
            return input.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        ///     Returns the substring contained between the first instances of the provided chars
        ///     (Like .SubString() but with an end index as opposed to the length)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>
        ///     The substring in between those chars. If input is null or empty, start is not found, or end is not found
        ///     after start, return empty string.
        /// </returns>
        public static string BetweenChars(this string input, char start, char end)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            int startIndex = input.IndexOf(start) + 1;
            if (startIndex == 0) // compare to 0 as + 1 applied above.
                return string.Empty;

            int endIndex = input.IndexOf(end, startIndex);

            return endIndex == -1 ? string.Empty : input.Between(startIndex, endIndex);
        }

        /// <summary>
        ///     Cut a string into chunks of a specific size.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxSize">The maximum size of each string in the result.</param>
        /// <returns></returns>
        public static IEnumerable<string> Cut(this string input, int maxSize)
        {
            if (maxSize < 1)
                yield break;

            for (int i = 0; i < input.Length; i += maxSize)
                yield return input.Substring(i, Math.Min(maxSize, input.Length - i));
        }

        /// <summary>
        ///     Replaces all double quotes with a single quote.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DoubleToSingleQuote(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? input : input.Replace("\"", "'");
        }

        /// <summary>
        ///     Check if a string contains another string with StringComparison supported.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keyword"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string keyword, StringComparison stringComparison)
        {
            if (string.IsNullOrEmpty(source))
                return false;

            return source.IndexOf(keyword, stringComparison) >= 0;

        }

        /// <summary>
        ///     Returns the string decrypted using AES
        /// </summary>
        /// <param name="encrypted">Must be a string previously encrypted with AES</param>
        /// <param name="key">Must be the same key used to encrypt the string</param>
        /// <param name="iv">Must be the same iv used to encrypt the string</param>
        /// <returns></returns>
        public static string FromAes(this string encrypted, string key, string iv)
        {
            byte[] encryptedbytes = Convert.FromBase64String(encrypted);
            var aes = new AesCryptoServiceProvider
                {
                    BlockSize = 128,
                    KeySize = 256,
                    Key = Encoding.ASCII.GetBytes(key),
                    IV = Encoding.ASCII.GetBytes(iv),
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC
                };
            using (ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] plainTextBytes = crypto.TransformFinalBlock(encryptedbytes, 0, encryptedbytes.Length);
                return Encoding.ASCII.GetString(plainTextBytes);
            }
        }

        /// <summary>
        ///     Converts a Base64-encoded string back to a readable string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">The source encoding used for the given base64 string.</param>
        /// <returns></returns>
        public static string FromBase64(this string input, [NotNull] Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return input == null ? null : encoding.GetString(Convert.FromBase64String(input));
        }

        /// <summary>
        ///     Returns a MemoryStream of the given string.
        ///     Please wrap in a 'using' clause in order to dispose of the Stream properly.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Stream GetStream([NotNull] this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var stream = new MemoryStream();
            var stringWriter = new StreamWriter(stream); // Using not needed as MemoryStream will be disposed by caller.

            stringWriter.Write(input);
            stringWriter.Flush();

            stream.Position = 0;
            return stream;
        }

        /// <summary>
        ///     Converts a hex-string representation of a byte array to a byte array.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexToByteArray(this string hexString)
        {
            if (hexString == null)
                return null;

            var bytes = new byte[hexString.Length / 2];
            int[] hexValues =
                {
                    0x00,
                    0x01,
                    0x02,
                    0x03,
                    0x04,
                    0x05,
                    0x06,
                    0x07,
                    0x08,
                    0x09,
                    0x00,
                    0x00,
                    0x00,
                    0x00,
                    0x00,
                    0x00,
                    0x00,
                    0x0A,
                    0x0B,
                    0x0C,
                    0x0D,
                    0x0E,
                    0x0F
                };

            for (int i = 0, j = 0; j < hexString.Length; j += 2, i += 1)
                bytes[i] = (byte)(hexValues[Char.ToUpper(hexString[j + 0]) - '0'] << 4 | hexValues[Char.ToUpper(hexString[j + 1]) - '0']);

            return bytes;
        }

        /// <summary>
        ///     Ordinal case-insensitive compare of two strings.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool IEquals(this string first, string second)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(first, second);
        }

        /// <summary>
        ///     Returns true if the string input is a valid email.
        ///     NOTE:
        ///     Uses complex regex to determine validity, do not use in a large loop.
        ///     Does not support Unicode email addresses.
        ///     If you need a very thorough checker, then use System.Net.MailAddress around a try catch.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>True if the email address is valid according to the RFC</returns>
        public static bool IsValidEmail(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Taken from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
            // Good enough for our purposes
            const string Pattern =
                @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return Regex.IsMatch(input, Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        ///     Returns the index of the bracket matching the first opening bracket of the string
        ///     for "abc(def())ghi(jkl)" it would return 9
        /// </summary>
        /// <param name="input"></param>
        /// <returns>returns -1 if the first bracket is never closed or its index if a match is found</returns>
        public static int MatchingClosingBracketIndex(this string input)
        {
            for (int pos = 0, bracketLevel = 0; pos < input.Length; ++pos)
            {
                if (input[pos] == '(')
                    ++bracketLevel;
                else if (input[pos] == ')' && bracketLevel > 0) // ignore ) that would be before first (
                {
                    --bracketLevel;
                    if (bracketLevel <= 0)
                        return pos;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Removes any characters that are not legal in Xml.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveIllegalXmlChars(this string input)
        {
            return Regex.Replace(input, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", string.Empty);
        }

        /// <summary>
        ///     Remove any combination of newlines. (System.Environment.NewLine might not always work)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveNewLines(this string input)
        {
            return input == null ? null : input.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        /// <summary>
        ///     Removes XML/HTML brackets from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveXmlBrackets(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            return input.Replace("<", string.Empty).Replace("/>", string.Empty).Replace(">", string.Empty);
        }

        /// <summary>
        ///     Replaces all given character occurrences in a given string with another character.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="chars">The chars to replace</param>
        /// <param name="replacementCharacter">What to replace them by</param>
        /// <returns></returns>
        public static string ReplaceAll(this string input, char[] chars, char replacementCharacter)
        {
            return chars.Aggregate(input, (str, cItem) => str.Replace(cItem, replacementCharacter));
        }

        /// <summary>
        ///     Replaces all given string occurrences in a given string with another string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="strings">The strings to replace</param>
        /// <param name="replacementString">What to replace them by.</param>
        /// <returns></returns>
        public static string ReplaceAll(this string input, string[] strings, string replacementString)
        {
            return strings.Aggregate(input, (str, cItem) => str.Replace(cItem, replacementString));
        }

        /// <summary>
        ///     Un-comments an XML/HTML comment-string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ReplaceXmlCommentBrackets(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Replace("<!--/", "</").Replace("<!--", "<").Replace("-->", ">");
        }

        /// <summary>
        ///     Replaces all double quotes with a single quote and vice-versa.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ReverseQuoteStyle(this string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("'");
                        break;
                    case '\'':
                        sb.Append("\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Returns a new array of strings from the input split at each new line.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] SplitOnNewLine(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return new[] { input };

            return input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        /// <summary>
        ///     Return whether the string input starts with a capital letter.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool StartsWithCapital(this string input)
        {
            return !string.IsNullOrEmpty(input) && char.IsUpper(input[0]) && char.ConvertToUtf32(input, 0) < 512;
        }

        /// <summary>
        ///     Removes UTF8 BOM from the start of string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripBOM(this string input)
        {
            string bom = Encoding.Default.GetString(Encoding.UTF8.GetPreamble());
            if (input.StartsWith(bom, StringComparison.Ordinal))
                return input.Remove(0, bom.Length);

            return input;
        }

        /// <summary>
        ///     Removes BOM (or any unwanted chars) from before the start of an otherwise valid XML stream
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripBOMFromXML(this string input)
        {
            int firstIndex = input.IndexOf("<?xml", StringComparison.Ordinal);
            if (firstIndex == -1)
                return input;

            return input.Substring(firstIndex, input.Length - firstIndex);
        }

        /// <summary>
        ///     Returns the string surrounded by square brackets. (i.e. "String" -> "[String]")
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SurroundByBrackets(this string input)
        {
            return input == null ? null : SurroundByChar(input, '[', ']');
        }

        /// <summary>
        ///     Returns the string surrounded by a starting and ending char.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="allowMultiple">If false, we do not allow more one pair of start/end to surround the input.</param>
        /// <returns></returns>
        public static string SurroundByChar(this string input, char start, char end, bool allowMultiple = true)
        {
            if (string.IsNullOrEmpty(input))
                return start.ToString(CultureInfo.InvariantCulture) + end;

            if (!allowMultiple && input.First() == start && input.Last() == end)
                return input;

            return start + input + end;
        }

        /// <summary>
        ///     Returns the string surrounded by parenthesis. (i.e. "String" -> "(String)")
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SurroundByParenthesis(this string input)
        {
            return SurroundByChar(input, '(', ')');
        }

        /// <summary>
        ///     Returns the string surrounded by spaces. (i.e. "String" -> " String ")
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SurroundBySpace(this string input)
        {
            return SurroundByChar(input, Space, Space);
        }

        /// <summary>
        ///     Returns the string surrounded by a starting and ending string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string SurroundByString(this string input, string start, string end)
        {
            return start + input + end;
        }

        /// <summary>
        ///     Returns an encrypted string using AES
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string ToAes(this string text, string key, string iv)
        {
            byte[] plainTextBytes = Encoding.ASCII.GetBytes(text);
            var aes = new AesCryptoServiceProvider
                {
                    BlockSize = 128,
                    KeySize = 256,
                    Key = Encoding.ASCII.GetBytes(key),
                    IV = Encoding.ASCII.GetBytes(iv),
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC
                };
            using (ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] encrypted = crypto.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                return Convert.ToBase64String(encrypted);
            }
        }

        /// <summary>
        ///     Converts a string to Base64.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">The encoding used for converting the bytes to base64.</param>
        /// <returns></returns>
        public static string ToBase64(this string input, [NotNull] Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            return input == null ? null : Convert.ToBase64String(encoding.GetBytes(input));
        }

        /// <summary>
        ///     Returns the hex representation of a byte array as a string.
        ///     It's about 50% faster than using BitConverter and Replace("-", "")
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            var result = new StringBuilder(bytes.Length * 2);
            const string HexAlphabet = "0123456789ABCDEF";

            foreach (var b in bytes)
            {
                result.Append(HexAlphabet[(b >> 4)]);
                result.Append(HexAlphabet[(b & 0xF)]);
            }

            return result.ToString();
        }

        /// <summary>
        ///     Returns a Culture Invariant String if it exists.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToInvariantString([NotNull] this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj is IConvertible
                       ? ((IConvertible)obj).ToString(CultureInfo.InvariantCulture)
                       : obj is IFormattable ? ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture) : obj.ToString();
        }

        /// <summary>
        ///     Same as ToString but does not explode in your face if input is null.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSafeString(this object input)
        {
            return (input ?? string.Empty).ToString();
        }

        /// <summary>
        ///     Same as ToString but does not explode in your face if input is null.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string ToSafeString(this IFormattable input, IFormatProvider provider)
        {
            return input == null ? string.Empty : input.ToString(null, provider);
        }

        /// <summary>
        ///     Same as ToString but does not explode in your face if input is null.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static string ToSafeString(this DateTime? input, string format, CultureInfo culture)
        {
            return input.HasValue ? input.Value.ToString(format, culture) : string.Empty;
        }

        /// <summary>
        ///     Returns the hex representation of the SHA1 hash of a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSha1(this string input)
        {
            if (input == null)
                return null;

            using (var sha1 = new SHA1Managed())
                return sha1.ComputeHash(Encoding.Unicode.GetBytes(input)).ToHexString();
        }

        /// <summary>
        ///     Returns an array of strings, each containing a single character.
        ///     This is similar to ToCharArray() but returns a string array instead.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] ToStringArray(this string input)
        {
            return string.IsNullOrEmpty(input) ? null : input.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        }

        /// <summary>
        ///     Inserts newlines into a string in order to wrap long lines.
        ///     The newlines are added after each whole word. No word is split.
        ///     WARNING: This means that words that are longer than maxChars will be kept in its entirety.
        /// </summary>
        /// <param name="input">The string to apply the Word Wrap to.</param>
        /// <param name="maxChars">
        ///     The maximum number of characters each line should have. (does not respect words that are longer
        ///     than maxChars)
        /// </param>
        /// <returns></returns>
        public static string Wrap(this string input, int maxChars)
        {
            if (maxChars < 0)
                throw new ArgumentOutOfRangeException("maxChars");

            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var output = new StringBuilder(input);
            int lastPos = 0;

            for (int i = maxChars; i < output.Length; i += maxChars)
            {
                while (i < output.Length && output[i] != Space)
                {
                    if (i > lastPos + Environment.NewLine.Length)
                    {
                        i--;
                        continue;
                    }

                    i = lastPos + maxChars;
                    while (i < output.Length && output[i] != Space)
                        i++;
                }

                if (i >= output.Length)
                    break;

                output.Remove(i, 1);
                output.Insert(i, Environment.NewLine);
                lastPos = i;
            }

            return output.ToString();
        }

        /// <summary>
        ///     Inserts newlines after each occurrence of a specified set of chars.
        ///     It also removes any single trailing whitespace chars before and after the newline.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="chars">One or more chars after which the wrapping will be performed.</param>
        /// <returns></returns>
        public static string WrapOnSpecificChars(this string input, params char[] chars)
        {
            var output = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                output.Append(c);

                if (chars.Contains(c))
                    output.Append(Environment.NewLine);
            }

            return output.ToString();
        }

        /// <summary>
        ///     Writes the specified text to a new file.
        /// </summary>
        /// <param name="contentToWrite">String content to write to the file.</param>
        /// <param name="fileToWriteTo">FilePath to write to.</param>
        /// <returns>A new FilePath that contains the specified string. Null if writing failed.</returns>
        public static FilePath WriteToFile(this string contentToWrite, FilePath fileToWriteTo)
        {
            try
            {
                File.WriteAllText(fileToWriteTo, contentToWrite);
            }
            catch (Exception exception)
            {
                return null;
            }

            return fileToWriteTo;
        }
        

        /// <summary>
        ///     Escapes any invalid XML characters from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string XmlEscape(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? input : SecurityElement.Escape(input);
        }

        /// <summary>
        ///     Un-escapes any previously escaped XML characters from a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string XmlUnescape(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var xmlEscapeDic = new Dictionary<string, char> { { "&amp;", '&' }, { "&apos;", '\'' }, { "&quot;", '"' }, { "&gt;", '>' }, { "&lt;", '<' } };
            var output = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c != '&' || i >= input.Length - 3)
                {
                    output.Append(c);
                    continue;
                }

                int replaceCharNb = 0;
                char? replaceChar = null;
                foreach (var escapeString in xmlEscapeDic.Keys)
                {
                    if (replaceCharNb != 0)
                        continue;

                    for (int j = 1; j < escapeString.Length; j++)
                    {
                        char escapeChar = escapeString[j];
                        if ((i + j) >= input.Length || input[i + j] != escapeChar)
                            break;

                        if (j + 1 != escapeString.Length)
                            continue;

                        replaceCharNb = j;
                        replaceChar = xmlEscapeDic[escapeString];
                    }
                }

                if (replaceCharNb == 0)
                    output.Append(c);
                else if (replaceChar != null)
                {
                    output.Append(replaceChar);
                    i += replaceCharNb;
                }
            }

            return output.ToString();
        }
        
        private const char Space = ' ';
    }

    /// <summary>
    ///     Method by which the strings should be matched.
    /// </summary>
    public enum MatchingMethod
    {
        [XmlEnum("Equals")]
        Equals,

        [XmlEnum("IEquals")]
        IEquals,

        [XmlEnum("StartsWith")]
        StartsWith,

        [XmlEnum("EndsWith")]
        EndsWith,

        [XmlEnum("Contains")]
        Contains
    }
}
