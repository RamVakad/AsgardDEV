/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010 Snow and haha01haha01
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AsgardDEV.Tools
{
    /// <summary>
    ///   Class that contains many tools.
    /// </summary>
    public class Misc
    {
        private static readonly Regex MapleNamingPattern = new Regex("[a-zA-Z0-9]{4,12}");

        public static Int64 RandomLong()
        {
            byte[] buffer = new byte[sizeof (Int64)];
            new Random().NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static string GetRightPaddedString(string str, int padUntil, char padWith)
        {
            String ret = str;
            for (int i = ret.Length; i <= padUntil; i++)
            {
                ret += padWith;
            }
            return ret;
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }

        /// <summary>
        ///   Checks if a character is a hex digit
        /// </summary>
        /// <param name="c"> Char to check </param>
        /// <returns> Char is a hex digit </returns>
        public static bool IsHexDigit(Char c)
        {
            c = Char.ToUpper(c);
            int numChar = Convert.ToInt32(c);
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            if (numChar >= numA && numChar < (numA + 6))
            {
                return true;
            }
            if (numChar >= num1 && numChar < (num1 + 10))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///   Convert a hex string to a byte
        /// </summary>
        /// <param name="hex"> Byte as a hex string </param>
        /// <returns> Byte representation of the string </returns>
        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
            {
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            }
            byte newByte = byte.Parse(hex, NumberStyles.HexNumber);
            return newByte;
        }

        /// <summary>
        ///   Convert a hex string to a byte array
        /// </summary>
        /// <param name="hexString"> byte array as a hex string </param>
        /// <returns> Byte array representation of the string </returns>
        public static byte[] GetBytes(string hexString)
        {
            string newString = string.Empty;
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                {
                    newString += c;
                }
            }
            // if odd number of characters, discard last character
            if (newString.Length%2 != 0)
            {
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length/2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new[] {newString[j], newString[j + 1]});
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }

        /// <summary>
        ///   Convert byte array to ASCII
        /// </summary>
        /// <param name="bytes"> Bytes to convert to ASCII </param>
        /// <returns> The byte array as an ASCII string </returns>
        public static String ToStringFromAscii(byte[] bytes)
        {
            char[] ret = new char[bytes.Length];
            for (int x = 0; x < bytes.Length; x++)
            {
                if (bytes[x] < 32 && bytes[x] >= 0)
                {
                    ret[x] = '.';
                }
                else
                {
                    int chr = (bytes[x]) & 0xFF;
                    ret[x] = (char) chr;
                }
            }
            return new String(ret);
        }

        public static bool IsNameAvaliable(string name)
        {
            return name.Length <= 13 && !name.Contains("admin") && !name.Contains("gm") && MapleNamingPattern.IsMatch(name);
        }
    }
}