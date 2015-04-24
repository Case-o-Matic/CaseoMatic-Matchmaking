using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    [Serializable]
    public class MatchmakingRequest
    {
        private static BinaryFormatter bf = new BinaryFormatter();
        public readonly MatchmakingPresence mmPresence;
        public readonly MatchmakingRequestState requestState;

        public MatchmakingRequest(MatchmakingPresence _mmpresence, MatchmakingRequestState _requeststate)
        {
            mmPresence = _mmpresence;
            requestState = _requeststate;
        }

        public static byte[] SerializeToBytes(MatchmakingRequest request)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, request);
                return ms.ToArray();
            }
        }
        public static MatchmakingRequest DeserializeToMMRequest(byte[] request)
        {
            using (MemoryStream ms = new MemoryStream(request))
            {
                return (MatchmakingRequest)bf.Deserialize(ms);
            }
        }
    }

    [Serializable]
    public enum MatchmakingRequestState
    {
        QueueMe,
        DequeueMe,
        HeartbeatAnswer
    }
}
