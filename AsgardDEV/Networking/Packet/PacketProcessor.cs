using System;

namespace AsgardDEV.Networking.Packet
{
    public class PacketProcessor
    {
        private static readonly IMaplePacketHandler[] Handlers;

        static PacketProcessor()
        {
            int maxRecvOp = 0;
            foreach (int op in Enum.GetValues(typeof (RecvOpCode)))
            {
                if (op > maxRecvOp)
                {
                    maxRecvOp = op;
                }
            }
            Handlers = new IMaplePacketHandler[maxRecvOp + 1];
            RegisterAll();
        }

        public static IMaplePacketHandler GetHandler(short packetHeader)
        {
            return packetHeader > Handlers.Length ? null : Handlers[packetHeader];
        }

        public void RegisterHandler(short code, IMaplePacketHandler handler)
        {
            try
            {
                Handlers[code] = handler;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error registering handler - " + code);
                Console.WriteLine("Exception: " + e);
            }
        }

        public static void RegisterAll()
        {
        }
    }
}