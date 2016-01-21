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

using AsgardDEV.Constants;

namespace AsgardDEV.Crypto
{
    /// <summary>
    ///   Class to manage Encryption and IV generation
    /// </summary>
    public class MapleCrypto
    {
        /// <summary>
        ///   Creates a new MapleCrypto class
        /// </summary>
        /// <param name="iv"> Intializing Vector </param>
        public MapleCrypto(byte[] iv)
        {
            this.IV = iv;
        }

        /// <summary>
        ///   (private) IV used in the packet encryption
        /// </summary>
        public byte[] IV { get; set; }

        /// <summary>
        ///   Updates the current IV
        /// </summary>
        public void UpdateIV()
        {
            IV = GetNewIV(IV);
        }

        /// <summary>
        ///   Encrypts data with AES and updates the IV
        /// </summary>
        /// <param name="data"> The data to crypt </param>
        public void Crypt(byte[] data)
        {
            AESEncryption.AESCrypt(IV, data, data.Length, CryptoConstants.GetTrimmedUserKey());
            UpdateIV();
        }

        /// <summary>
        ///   Get the packet header for a packet being sent to the client
        /// </summary>
        /// <param name="size"> Size of the packet </param>
        /// <returns> The packet header </returns>
        public byte[] GetHeader(int size)
        {
            byte[] header = new byte[4];
            int a = IV[3]*0x100 + IV[2];
            a ^= -(ServerConstants.MapleVersion + 1);
            int b = a ^ size;
            header[0] = (byte) (a%0x100);
            header[1] = (byte) ((a - header[0])/0x100);
            header[2] = (byte) (b ^ 0x100);
            header[3] = (byte) ((b - header[2])/0x100);
            return header;
        }

        /// <summary>
        ///   Generates a new IV
        /// </summary>
        /// <param name="oldIV"> The Old IV used to generate the new IV </param>
        /// <returns> A new IV </returns>
        public static byte[] GetNewIV(byte[] oldIV)
        {
            byte[] start = new byte[4] {0xf2, 0x53, 0x50, 0xc6}; //TODO: ADD GLOBAL VAR BACK
            for (int i = 0; i < 4; i++)
            {
                Shuffle(oldIV[i], start);
            }
            return start;
        }

        /// <summary>
        ///   Shuffle the bytes in the IV
        /// </summary>
        /// <param name="inputByte"> Byte of the old IV </param>
        /// <param name="start"> The Default AES Key </param>
        /// <returns> The shuffled bytes </returns>
        public static byte[] Shuffle(byte inputByte, byte[] start)
        {
            byte a = start[1];
            byte b = a;
            uint c, d;
            b = CryptoConstants.Shuffle[b];
            b -= inputByte;
            start[0] += b;
            b = start[2];
            b ^= CryptoConstants.Shuffle[inputByte];
            a -= b;
            start[1] = a;
            a = start[3];
            b = a;
            a -= start[0];
            b = CryptoConstants.Shuffle[b];
            b += inputByte;
            b ^= start[2];
            start[2] = b;
            a += CryptoConstants.Shuffle[inputByte];
            start[3] = a;

            c = (uint) (start[0] + start[1]*0x100 + start[2]*0x10000 + start[3]*0x1000000);
            d = c;
            c >>= 0x1D;
            d <<= 0x03;
            c |= d;
            start[0] = (byte) (c%0x100);
            c /= 0x100;
            start[1] = (byte) (c%0x100);
            c /= 0x100;
            start[2] = (byte) (c%0x100);
            start[3] = (byte) (c/0x100);

            return start;
        }

        /// <summary>
        ///   Gets the length of a packet from the header
        /// </summary>
        /// <param name="packetHeader"> Header of the packet </param>
        /// <returns> The length of the packet </returns>
        public static int GetPacketLength(byte[] packetHeader)
        {
            if (packetHeader.Length < 4)
            {
                return -1;
            }
            return (packetHeader[0] + (packetHeader[1] << 8)) ^ (packetHeader[2] + (packetHeader[3] << 8));
        }

        /// <summary>
        ///   Multiplies bytes
        /// </summary>
        /// <param name="input"> Bytes to multiply </param>
        /// <param name="count"> Amount of bytes to repeat </param>
        /// <param name="mult"> Times to repeat the packet </param>
        /// <returns> The multiplied bytes </returns>
        public static byte[] MultiplyBytes(byte[] input, int count, int mult)
        {
            byte[] ret = new byte[count*mult];
            for (int x = 0; x < ret.Length; x++)
            {
                ret[x] = input[x%count];
            }
            return ret;
        }
    }
}