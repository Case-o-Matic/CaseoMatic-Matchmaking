using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    public static class MatchmakingManager
    {
        internal const int clientPort = 41001;

        public delegate void MatchFoundHandler(MatchmakingFoundInfo info);
        public static event MatchFoundHandler MatchFound;

        public static MatchmakingPresence matchmakingPresence { get; private set; }

        private static IPEndPoint usedMatchmakingCenterEndPoint;
        private static IPEndPoint localEndPoint;
        private static UdpClient client;
        private static BackgroundWorker bgWorker;
        private static bool isConnected;
        private static bool isCurrentlyQueueing;

        public static void Start(MatchmakingPresence mmpresence, IPEndPoint matchmakingcenterendpoint)
        {
            isConnected = true;
            matchmakingPresence = mmpresence;

            usedMatchmakingCenterEndPoint = matchmakingcenterendpoint;
            localEndPoint = new IPEndPoint(IPAddress.Loopback, clientPort);
            client = new UdpClient(usedMatchmakingCenterEndPoint);

            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += DoReceiveMessageRoutine;
            bgWorker.RunWorkerAsync();
        }
        public static void Stop()
        {
            if (isConnected)
            {
                client.Close();
            }
        }

        public static void QueueMe()
        {
            if (!isCurrentlyQueueing)
            {
                SendMessage(new MatchmakingRequest(matchmakingPresence, MatchmakingRequestState.QueueMe));
                isCurrentlyQueueing = true;
            }
        }
        public static void DequeueMe()
        {
            if (isCurrentlyQueueing)
            {
                SendMessage(new MatchmakingRequest(matchmakingPresence, MatchmakingRequestState.DequeueMe));
                isCurrentlyQueueing = false;
            }
        }

        private static void SendMessage(MatchmakingRequest mmrequest)
        {
            byte[] mmrequestInBytes = MatchmakingRequest.SerializeToBytes(mmrequest);
            client.Send(mmrequestInBytes, mmrequestInBytes.Length);
        }

        private static void DoReceiveMessageRoutine(object sender, DoWorkEventArgs e)
        {
            while (isConnected)
            {
                IPEndPoint messageSenderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] answerSourceMessage = client.Receive(ref messageSenderEndPoint);
                if (messageSenderEndPoint == usedMatchmakingCenterEndPoint)
                {
                    MatchmakingAnswer answer = MatchmakingAnswer.DeserializeToMMAnswer(answerSourceMessage);
                    if (answer.successState == MatchmakingSuccessState.Heartbeat)
                    {
                        SendMessage(new MatchmakingRequest(matchmakingPresence, MatchmakingRequestState.HeartbeatAnswer));
                    }
                    else if (answer.successState == MatchmakingSuccessState.MatchFound)
                    {
                        if (MatchFound != null)
                            MatchFound(new MatchmakingFoundInfo(answer.allUsers, answer.foundGameServer));
                    }
                }
            }
        }
    }
}
