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
using System.Net.Sockets;
using AsgardDEV.Client;
using AsgardDEV.Constants;
using AsgardDEV.Crypto;
using AsgardDEV.Networking.Packet;
using AsgardDEV.Tools;

namespace AsgardDEV.Networking
{
    /// <summary>
    ///   Class to a network session socket
    /// </summary>
    public class Session
    {
        /// <summary>
        ///   Creates a new instance of a Session
        /// </summary>
        /// <param name="socket"> Socket connection of the session </param>
        /// <param name="recv"> Recive crypto of the session </param>
        /// <param name="send"> Send crypto of the session </param>
        public Session(Socket socket, MapleCrypto recv, MapleCrypto send)
        {
            this.Socket = socket;
            this.ReceiveCrypto = recv;
            this.SendCrypto = send;
            this.Client = new MapleClient(this);
        }

        public MapleClient Client { get; set; }

        /// <summary>
        ///   The Session's socket
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        ///   The Recieved packet crypto manager
        /// </summary>
        public MapleCrypto ReceiveCrypto { get; set; }

        /// <summary>
        ///   The Sent packet crypto manager
        /// </summary>
        public MapleCrypto SendCrypto { get; set; }

        /// <summary>
        ///   Begins the session by sending the hello packet
        /// </summary>
        /// <param name="ivRecv"> Initial Receive Initilization Vector </param>
        /// <param name="ivSend"> Initial Send Initilization Vector </param>
        public void Begin(byte[] ivRecv, byte[] ivSend)
        {
            Console.WriteLine("Session from " + Socket.RemoteEndPoint + " opened.");
            SendRawPacket(GetHelloPacket(ivRecv, ivSend).ToArray());
            WaitForData(new ReceiveManager());
        }


        /// <summary>
        ///   Waits for more data to arrive
        /// </summary>
        /// <param name="manager"> Info about data to be received </param>
        private void WaitForData(ReceiveManager manager)
        {
            try
            {
                Socket.BeginReceive(manager.Buffer,
                                    manager.Index,
                                    manager.Buffer.Length - manager.Index,
                                    SocketFlags.None,
                                    DataReceived,
                                    manager);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        ///   Data received event handler
        /// </summary>
        /// <param name="iar"> IAsyncResult of the data received event </param>
        private void DataReceived(IAsyncResult iar)
        {
            ReceiveManager manager = (ReceiveManager) iar.AsyncState;
            try
            {
                int received = Socket.EndReceive(iar);
                manager.Index += received;
                if (manager.Index == manager.Buffer.Length)
                {
                    byte[] data = manager.Buffer;
                    switch (manager.State)
                    {
                        case ReceiveState.Header:
                            int packetLength = MapleCrypto.GetPacketLength(data);
                            manager.State = ReceiveState.Content;
                            manager.Buffer = new byte[packetLength];
                            manager.Index = 0;
                            WaitForData(manager);
                            break;
                        case ReceiveState.Content:
                            ReceiveCrypto.Crypt(data);
                            MapleCustomEncryption.Decrypt(data);
                            PacketReceived(new MaplePacketReader(data));
                            WaitForData(new ReceiveManager());
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("[Warning] Not enough data");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Close("Exception while receiving data.");
            }
        }

        private void Close(string reason)
        {
            lock (this)
            {
                if (Socket == null)
                {
                    return;
                }
                Client.Disconnect();
                Client = null;
                Console.WriteLine("Session from " + Socket.RemoteEndPoint + " closed.");
                Console.WriteLine("REASON: " + reason);
                Socket.Close();
                Socket = null;
            }
        }

        private void PacketReceived(MaplePacketReader mpr)
        {
            String packet = Misc.ByteArrayToHexString(mpr.ToArray());
            Console.WriteLine("Packet Received: (" + mpr.ToArray().Length + ") - " + packet);
            short packetHeader = mpr.ReadShort();
            IMaplePacketHandler handler = PacketProcessor.GetHandler(packetHeader);

            if (handler != null)
            {
                try
                {
                    handler.HandlePacket(mpr, this);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[" + handler + "] Exception: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    Close("Exception during packet handling.");
                }
            }
            else
            {
                Console.WriteLine("No handler found for packet: " + packet.Substring(0, 5));
            }
        }

        /// <summary>
        ///   Encrypts the packet, adds header, then send it to the client.
        /// </summary>
        /// <param name="input"> The byte array to be sent. </param>
        public void SendPacket(byte[] input)
        {
            byte[] cryptData = input;
            byte[] sendData = new byte[cryptData.Length + 4];
            byte[] header = SendCrypto.GetHeader(cryptData.Length);

            MapleCustomEncryption.Encrypt(cryptData);
            SendCrypto.Crypt(cryptData);

            Buffer.BlockCopy(header, 0, sendData, 0, 4); //header
            Buffer.BlockCopy(cryptData, 0, sendData, 4, cryptData.Length); //content
            SendRawPacket(sendData);
        }

        /// <summary>
        ///   Sends a raw buffer to the client.
        /// </summary>
        /// <param name="buffer"> The buffer to be sent. </param>
        public void SendRawPacket(byte[] buffer)
        {
            if (Socket == null)
            {
                return;
            }
            Socket.Send(buffer);
        }

        public static MaplePacket GetHelloPacket(byte[] ivRecv, byte[] ivSend)
        {
            MaplePacketWriter pw = new MaplePacketWriter();
            pw.WriteShort(0x0E);
            pw.WriteShort(ServerConstants.MapleVersion);
            pw.WriteMapleString(ServerConstants.Patch);
            pw.WriteBytes(ivRecv);
            pw.WriteBytes(ivSend);
            pw.WriteByte(8);
            return pw;
        }
    }
}