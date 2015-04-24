using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;


namespace CaseomaticMatchmakingServer
{
    public sealed class GameServerMirror
    {
        public bool isCurrentlyUsed { get; internal set; }
        public IPEndPoint endPoint { get; internal set; }

        public GameServerMirror(IPEndPoint _endpoint)
        {
            isCurrentlyUsed = false;
            endPoint = _endpoint;
        }
        public GameServerMirror()
        { }

        public long GetPing()
        {
            Ping ping = new Ping();
            PingReply pReply = ping.Send(endPoint.Address, 200);

            return pReply.Status == IPStatus.Success ? pReply.RoundtripTime : -1;
        }
    }
}
