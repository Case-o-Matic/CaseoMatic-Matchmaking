using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    public static class MatchmakingManager
    {
        public const string version = "1.0.0-alpha.3";

        public delegate void MatchFoundHandler(MatchmakingFoundInfo info);
        public static event MatchFoundHandler OnMatchFound;

        public static MatchmakingPresence matchmakingPresence { get; private set; }

        private static IPEndPoint usedMatchmakingCenterEndPoint;
        private static IPEndPoint localEndPoint;
        private static UdpClient client;
        private static Thread receiveThread;
        private static bool isConnected;
        private static bool isCurrentlyQueueing;

        public static void Start(int port, MatchmakingPresence mmpresence, IPEndPoint matchmakingcenterendpoint)
        {
            try
            {
                isConnected = true;
                matchmakingPresence = mmpresence;

                usedMatchmakingCenterEndPoint = matchmakingcenterendpoint;
                localEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                client = new UdpClient(port, AddressFamily.InterNetwork);

                receiveThread = new Thread(DoReceiveMessageRoutine);
                receiveThread.IsBackground = true;
            }
            catch (Exception ex)
            {
                MatchmakingLog.WriteLog("Error; " + ex.ToString());
            }
        }
        public static void Stop()
        {
            if (isConnected)
            {
                // Do not 

                isConnected = false;
                client.Close();

                MatchmakingLog.SaveLog("mmclient.log");
            }
        }

        public static void QueueMe()
        {
            if (!isCurrentlyQueueing)
            {
                SendMessage(new MatchmakingRequest(matchmakingPresence, MatchmakingRequestState.EnqueueMe));
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
            try
            {
                byte[] mmrequestInBytes = MatchmakingRequest.SerializeToBytes(mmrequest);
                client.Send(mmrequestInBytes, mmrequestInBytes.Length, usedMatchmakingCenterEndPoint);
            }
            catch (Exception ex)
            {
                MatchmakingLog.WriteLog("Error; " + ex.ToString());
            }
        }

        private static void DoReceiveMessageRoutine()
        {
            try
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
                            if (OnMatchFound != null)
                                OnMatchFound(new MatchmakingFoundInfo(answer.allUsers, answer.foundGameServer));
                            isCurrentlyQueueing = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MatchmakingLog.WriteLog("Error; " + ex.ToString());
            }
        }
    }
}
