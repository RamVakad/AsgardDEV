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
using System.Net;
using System.Net.Sockets;
using AsgardDEV.Crypto;

namespace AsgardDEV.Networking
{
    /// <summary>
    ///   A Nework Socket Acceptor (Listener)
    /// </summary>
    public class Acceptor
    {
        public Acceptor(int port)
        {
            this.Port = port;
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public int Port { get; set; }

        /// <summary>
        ///   The listener socket
        /// </summary>
        public Socket Listener { get; set; }

        /// <summary>
        ///   Starts listening and accepting connections
        /// </summary>
        public void StartListening()
        {
            Listener.Bind(new IPEndPoint(IPAddress.Any, Port));
            Listener.Listen(15);
            Listener.BeginAccept(OnClientConnect, null);
        }

        /// <summary>
        ///   Stops listening for connections
        /// </summary>
        public void StopListening()
        {
            Listener.Disconnect(true);
        }

        /// <summary>
        ///   Client connected handler
        /// </summary>
        /// <param name="iar"> The IAsyncResult </param>
        private void OnClientConnect(IAsyncResult iar)
        {
            try
            {
                Socket socket = Listener.EndAccept(iar);
                byte[] ivRecv = {70, 114, 122, 82};
                byte[] ivSend = {82, 48, 120, 115};
                MapleCrypto recvCrypto = new MapleCrypto(ivRecv);
                MapleCrypto sendCrypto = new MapleCrypto(ivSend);
                Session session = new Session(socket, recvCrypto, sendCrypto);
                session.Begin(ivRecv, ivSend);
                Listener.BeginAccept(OnClientConnect, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}