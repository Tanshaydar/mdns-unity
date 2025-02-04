using System;
using System.Text;

namespace Makaretu.Dns
{
    /// <summary>
    /// Convert from base 16/32
    /// </summary>
    public static class BaseConvert
    {
        private const string base32hexAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        /// <summary>
        /// Convert base 16 string to byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] FromBase16(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new FormatException("hex cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length / 2; ++i)
                arr[i] = (byte)((GetHexVal(hex[i * 2]) << 4) + (GetHexVal(hex[(i * 2) + 1])));

            return arr;
        }

        /// <summary>
        /// Convert a byte array to lowercase base16
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase16Lower(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private static int GetHexVal(int val)
        {
            if (val < 48 || (val > 57 && val < 65) || (val > 70 && val < 97) || val > 102)
                throw new FormatException("Invalid hex character");
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        /// <summary>
        /// Converts a byte array to a base32 hex string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBase32Hex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            int charCount = (int)Math.Ceiling(bytes.Length / 5d) * 8;
            char[] returnArray = new char[charCount];

            byte nextChar = 0, bitsRemaining = 5;
            int arrayIndex = 0;

            foreach (byte b in bytes)
            {
                nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
                returnArray[arrayIndex++] = ValueToChar(nextChar);

                if (bitsRemaining < 4)
                {
                    nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                    returnArray[arrayIndex++] = ValueToChar(nextChar);
                    bitsRemaining += 5;
                }

                bitsRemaining -= 3;
                nextChar = (byte)((b << bitsRemaining) & 31);
            }

            if (arrayIndex != charCount)
            {
                returnArray[arrayIndex++] = ValueToChar(nextChar);
                //while (arrayIndex != charCount)
                //    returnArray[arrayIndex++] = '='; //padding
            }

            return new string(returnArray, 0, arrayIndex);
        }

        /// <summary>
        /// Converts a base32hex string to a byte array
        /// </summary>
        /// <param name="base32"></param>
        /// <returns></returns>
        public static byte[] FromBase32Hex(string base32)
        {
            if (string.IsNullOrEmpty(base32))
            {
                throw new ArgumentNullException(nameof(base32));
            }

            base32 = base32.TrimEnd('=');
            int byteCount = base32.Length * 5 / 8;
            byte[] returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int mask = 0, arrayIndex = 0;

            foreach (char c in base32)
            {
                int cValue = CharToValue(c);

                if (bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            if (arrayIndex != byteCount)
            {
                returnArray[arrayIndex] = curByte;
            }

            return returnArray;
        }

        private static int CharToValue(char c)
        {
            int value = (int)c;

            //uppercase letters
            if (value < 87 && value > 64)
            {
                return value - 55;
            }
            //numbers 0-9
            if (value < 58 && value > 47)
            {
                return value - 48;
            }
            //lowercase letters
            if (value < 119 && value > 96)
            {
                return value - 87;
            }

            throw new ArgumentException("Character is not a Base32 character.", "c");
        }

        private static char ValueToChar(byte b)
        {
            if (b < 10)
            {
                return (char)(b + 48);
            }

            if (b < 32)
            {
                return (char)(b + 55);
            }

            throw new ArgumentException("Byte is not a value Base32 value.", "b");
        }
    }
}
