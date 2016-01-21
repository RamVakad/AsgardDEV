namespace AsgardDEV.Networking.Packet
{
    public interface IMaplePacketHandler
    {
        void HandlePacket(MaplePacketReader mpr, Session s);
    }
}