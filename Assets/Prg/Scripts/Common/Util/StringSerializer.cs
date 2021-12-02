using System;
using System.Collections.Generic;
using System.Text;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Encodes and decodes strings using using Base16k encoding to make them "unreadable".
    /// </summary>
    /// <remarks>
    /// See original implementation: https://github.com/JDanielSmith/Base16k
    /// and article: https://sites.google.com/site/markusicu/unicode/base16k
    /// </remarks>
    public static class StringSerializer
    {
        /// <summary>
        /// Compress a string to a shorter string using Base16k encoding.
        /// </summary>
        /// <param name="plainText">any string</param>
        /// <returns>Encoded and compressed text or exception</returns>
        public static string Encode(string plainText)
        {
            var bytes = Encoding.Unicode.GetBytes(plainText);
            var compressedText = ToBase16KString(bytes);
            return compressedText;
        }

        /// <summary>
        /// Decompress a Base16k decoded string back to its original representation.
        /// </summary>
        /// <param name="compressedText">Base16k encoded string</param>
        /// <returns>Decoded string or exception</returns>
        public static string Decode(string compressedText)
        {
            var bytes = FromBase16KString(compressedText);
            var plainText = Encoding.Unicode.GetString(bytes);
            return plainText;
        }

        // https://github.com/JDanielSmith/Base16k/blob/master/Base16k/Base16k.cs

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation
        /// that is encoded with base-16k digits.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation, in base 16k, of the contents of inArray.</returns>
        public static string ToBase16KString(byte[] inArray)
        {
            if (inArray == null) throw new ArgumentNullException(nameof(inArray));

            int len = inArray.Length;

            var sb = new StringBuilder(len * 6 / 5);
            sb.Append(len);

            int code = 0;

            for (int i = 0; i < len; ++i)
            {
                byte byteValue = inArray[i];
                switch (i % 7)
                {
                    case 0:
                        code = byteValue << 6;
                        break;

                    case 1:
                        code |= byteValue >> 2;
                        code += 0x5000;
                        sb.Append(System.Convert.ToChar(code));
                        code = (byteValue & 3) << 12;
                        break;

                    case 2:
                        code |= byteValue << 4;
                        break;

                    case 3:
                        code |= byteValue >> 4;
                        code += 0x5000;
                        sb.Append(System.Convert.ToChar(code));
                        code = (byteValue & 0xf) << 10;
                        break;

                    case 4:
                        code |= byteValue << 2;
                        break;

                    case 5:
                        code |= byteValue >> 6;
                        code += 0x5000;
                        sb.Append(System.Convert.ToChar(code));
                        code = (byteValue & 0x3f) << 8;
                        break;

                    case 6:
                        code |= byteValue;
                        code += 0x5000;
                        sb.Append(System.Convert.ToChar(code));
                        code = 0;
                        break;
                }
            }

            // emit a character for remaining bits
            if (len % 7 != 0)
            {
                code += 0x5000;
                sb.Append(System.Convert.ToChar(code));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string
        ///  representation that is encoded with base-16k digits. Parameters specify the subset
        ///  as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in inArray.</param>
        /// <param name="length">The number of elements of inArray to convert.</param>
        /// <returns>The string representation in base 16k of length elements of inArray, starting at position offset.</returns>
        public static string ToBase16KString(byte[] inArray, int offset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a subset of an 8-bit unsigned integer array to an equivalent subset
        /// of a Unicode character array encoded with base-16k digits. Parameters specify
        /// the subsets as offsets in the input and output arrays, and the number of elements
        /// in the input array to convert.
        /// </summary>
        /// <param name="inArray">An input array of 8-bit unsigned integers.</param>
        /// <param name="offsetIn">A position within inArray.</param>
        /// <param name="length">The number of elements of inArray to convert.</param>
        /// <param name="outArray">An output array of Unicode characters.</param>
        /// <param name="offsetOut">A position within outArray.</param>
        /// <returns>A 32-bit signed integer containing the number of bytes in outArray.</returns>
        public static int ToBase16KCharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            throw new NotImplementedException();
        }

        static byte[] FromBase16KString_(string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            // read the length
            var lengthEnd = -1;
            for (var l = 0; l < s.Length; l++)
            {
                if (s[l] >= '0' && s[l] <= '9')
                {
                    lengthEnd = l;
                }
                else
                {
                    break;
                }
            }

            if (lengthEnd < 0) throw new FormatException("Unable to find a length value.");

            int length;
            if (!Int32.TryParse(s.Substring(0, lengthEnd + 1), out length))
                throw new FormatException("Unable to parse the length string.");

            var buf = new List<byte>(length);

            int pos = 0; // position in s
            while ((pos < s.Length) && (s[pos] >= '0' && s[pos] <= '9'))
                ++pos;

            // decode characters to bytes
            int i = 0; // byte position modulo 7 (0..6 wrapping around)
            int code = 0;
            byte byteValue = 0;

            while (length-- > 0)
            {
                if (((1 << i) & 0x2b) != 0)
                {
                    // fetch another Han character at i=0, 1, 3, 5
                    if (pos >= s.Length)
                        throw new FormatException("Too few Han characters representing binary data.");

                    code = s[pos++] - 0x5000;
                }

                switch (i % 7)
                {
                    case 0:
                        byteValue = System.Convert.ToByte(code >> 6);
                        buf.Add(byteValue);
                        byteValue = System.Convert.ToByte((code & 0x3f) << 2);
                        break;

                    case 1:
                        byteValue |= System.Convert.ToByte(code >> 12);
                        buf.Add(byteValue);
                        break;

                    case 2:
                        byteValue = System.Convert.ToByte((code >> 4) & 0xff);
                        buf.Add(byteValue);
                        byteValue = System.Convert.ToByte((code & 0xf) << 4);
                        break;

                    case 3:
                        byteValue |= System.Convert.ToByte(code >> 10);
                        buf.Add(byteValue);
                        break;

                    case 4:
                        byteValue = System.Convert.ToByte((code >> 2) & 0xff);
                        buf.Add(byteValue);
                        byteValue = System.Convert.ToByte((code & 3) << 6);
                        break;

                    case 5:
                        byteValue |= System.Convert.ToByte(code >> 8);
                        buf.Add(byteValue);
                        break;

                    case 6:
                        byteValue = System.Convert.ToByte(code & 0xff);
                        buf.Add(byteValue);
                        break;
                }

                // advance to the next byte position
                if (++i == 7)
                    i = 0;
            }

            return buf.ToArray();
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as base-16k digits, to
        /// an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>An array of 8-bit unsigned integers that is equivalent to s.</returns>
        public static byte[] FromBase16KString(string s)
        {
            try
            {
                return FromBase16KString_(s);
            }
            catch (OverflowException oe)
            {
                // Throw FormatException instead as we get an OverflowException from bad string data.
                throw new FormatException("Invalid data.", oe);
            }
        }

        /// <summary>
        /// Converts a subset of a Unicode character array, which encodes binary data as
        /// base-16k digits, to an equivalent 8-bit unsigned integer array. Parameters specify
        /// the subset in the input array and the number of elements to convert.
        /// </summary>
        /// <param name="inArray">A Unicode character array.</param>
        /// <param name="offset">A position within inArray.</param>
        /// <param name="length">The number of elements in inArray to convert.</param>
        /// <returns>An array of 8-bit unsigned integers equivalent to length elements at position offset in inArray.</returns>
        public static byte[] FromBase64CharArray(char[] inArray, int offset, int length)
        {
            throw new NotImplementedException();
        }
    }
}