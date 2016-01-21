using AsgardDEV.Networking;

namespace AsgardDEV.Client
{
    public class MapleClient
    {
        public MapleClient(Session session)
        {
            this.Session = session;
        }

        public Session Session { get; set; }

        public void Disconnect()
        {
        }
    }
}