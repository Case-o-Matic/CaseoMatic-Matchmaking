using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    public struct MatchmakingFoundInfo
    {
        public List<MatchmakingPresence> allUsers { get; private set; }
        public IPEndPoint gameServerEndPoint { get; private set; }

        internal MatchmakingFoundInfo(List<MatchmakingPresence> _allusers, IPEndPoint _gameserverendpoint)
        {
            allUsers = _allusers;
            gameServerEndPoint = _gameserverendpoint;
        }
    }
}
