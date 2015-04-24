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
        public bool isCurrentlyUsed;
        public IPEndPoint endPoint;

        public long GetPing()
        {
            Ping ping = new Ping();
            return ping.Send(endPoint.Address, 250).RoundtripTime;
        }
    }
}
