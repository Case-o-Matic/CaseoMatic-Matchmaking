using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    [Serializable]
    public sealed class MatchmakingPresence
    {
        public IPEndPoint externalEndPoint;
        public readonly Guid playerId;
        public readonly MatchmakingSearchSettings personalSearchSettings;

        public MatchmakingPresence(IPEndPoint _externalendpoint, MatchmakingSearchSettings _settings)
        {
            externalEndPoint = _externalendpoint;
            playerId = Guid.NewGuid();
            personalSearchSettings = _settings;
        }
        public MatchmakingPresence(MatchmakingSearchSettings _settings)
        {
            personalSearchSettings = _settings;
        }
        public MatchmakingPresence()
        { }
    }
}
