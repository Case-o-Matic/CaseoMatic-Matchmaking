using CaseomaticMatchmakingClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    [Serializable]
    public class MatchmakingAnswer
    {
        private static BinaryFormatter bf = new BinaryFormatter();

        public readonly MatchmakingSuccessState successState;
        public readonly List<MatchmakingPresence> allUsers;
        public readonly IPEndPoint foundGameServer;

        public MatchmakingAnswer(MatchmakingSuccessState _successstate, List<MatchmakingPresence> _allusers, IPEndPoint _foundgameserver)
        {
            successState = _successstate;
            allUsers = _allusers;
            foundGameServer = _foundgameserver;
        }

        public static byte[] SerializeToBytes(MatchmakingAnswer answer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, answer);
                return ms.ToArray();
            }
        }
        public static MatchmakingAnswer DeserializeToMMAnswer(byte[] answer)
        {
            using (MemoryStream ms = new MemoryStream(answer))
            {
                return (MatchmakingAnswer)bf.Deserialize(ms);
            }
        }
    }

    [Serializable]
    public enum MatchmakingSuccessState
    {
        MatchFound,
        Heartbeat
    }
}
