using CaseomaticMatchmakingClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingServer
{
    public sealed class MatchmakingCenter
    {
        public const int maxNeededUserQueueLength = 450;

        public readonly IPEndPoint localEndPoint;
        public List<GameServerMirror> registeredGameServers { get; private set; }
        public List<MatchmakingPresence> curentUsersSearchingAMatch { get; private set; }
        public int neededUserQueueLength
        {
            get { return _neededUserQueueLength; }
            set
            {
                if (value > 0 || value <= maxNeededUserQueueLength)
                    _neededUserQueueLength = value;
            }
        }
        private int _neededUserQueueLength;

        private UdpClient client;
        private BackgroundWorker bgWorker;
        private bool isOnline;

        internal Dictionary<MatchmakingSearchSettings, List<MatchmakingPresence>> currentSettingsAndUsers;

        public MatchmakingCenter(int port)
        {
            localEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            registeredGameServers = new List<GameServerMirror>();
            curentUsersSearchingAMatch = new List<MatchmakingPresence>();
            currentSettingsAndUsers = new Dictionary<MatchmakingSearchSettings, List<MatchmakingPresence>>();

            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += DoBackgroundReceiveRoutine;
        }

        public void RegisterNewGameServer(GameServerMirror gameserverendpoint)
        {
            if (!registeredGameServers.Contains(gameserverendpoint))
                registeredGameServers.Add(gameserverendpoint);
            else
                WriteLog("Error; This game server is already in the registered list.");
        }
        public void UnregisterGameServer(GameServerMirror gameserverendpoint)
        {
            if (registeredGameServers.Contains(gameserverendpoint))
                registeredGameServers.Add(gameserverendpoint);
            else
                WriteLog("Error; This game server is not in the registered list.");
        }
        public bool CheckIsOnline()
        {
            return isOnline;
        }

        public void Start()
        {
            isOnline = true;
            client = new UdpClient(localEndPoint.Port, AddressFamily.InterNetwork);
            bgWorker.RunWorkerAsync();

            WriteLog("Started matchmaking center on " + localEndPoint.ToString() + ".");
        }
        public void Stop()
        {
            isOnline = false;
            bgWorker.CancelAsync();
            client.Close();

            WriteLog("Closed matchmaking center");
        }

        private void DoBackgroundReceiveRoutine(object sender, DoWorkEventArgs e)
        {
            while (isOnline)
            {
                IPEndPoint sourceMessageSender = new IPEndPoint(IPAddress.Any, 0);
                byte[] sourceMessage = client.Receive(ref sourceMessageSender);
                // Maybe check here if the sender-port is from a valid matchmaking presence?

                if (sourceMessageSender.Port == 41001)
                {
                    try
                    {
                        MatchmakingRequest mmRequest = MatchmakingRequest.DeserializeToMMRequest(sourceMessage);
                        mmRequest.mmPresence.externalEndPoint = sourceMessageSender;

                        if (mmRequest.requestState == MatchmakingRequestState.QueueMe)
                            QueueIntoMM(mmRequest.mmPresence);
                        else if (mmRequest.requestState == MatchmakingRequestState.DequeueMe)
                            DequeueFromMM(mmRequest.mmPresence);
                        else if (mmRequest.requestState == MatchmakingRequestState.HeartbeatAnswer)
                        { }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("Error; " + ex.ToString());
                    }
                }
                else if (sourceMessageSender.Port == 41002) // The port of game servers
                {
                    string msg = ASCIIEncoding.ASCII.GetString(sourceMessage);
                    if (msg == "msg,ready")
                    {
                        foreach (var gameservermirror in registeredGameServers)
                        {
                            if (gameservermirror.endPoint == sourceMessageSender)
                            {
                                gameservermirror.isCurrentlyUsed = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void SendAnswerToMMPresence(MatchmakingPresence mmp, MatchmakingAnswer answer)
        {
            byte[] answerInBytes = MatchmakingAnswer.SerializeToBytes(answer);
            client.Send(answerInBytes, answerInBytes.Length);
        }
        private void QueueIntoMM(MatchmakingPresence mmp)
        {
            KeyValuePair<MatchmakingSearchSettings, List<MatchmakingPresence>>? foundQueueSettings = null;
            // Go through each settings obj. that already exists, maybe some people already search for that type of game?
            foreach (var settingobj in currentSettingsAndUsers)
            {
                // Go through every condition of both settings obj. if they are equal
                foreach (var condition in settingobj.Key.conditions)
                {
                    foreach (var condition2 in mmp.personalSearchSettings.conditions)
                    {
                        if (!condition.conditionObject.Equals(condition2.conditionObject))
                        {
                            // No, the settings of this specific queue and the current users arent the same. Create a new queue in the dictionary

                            currentSettingsAndUsers.Add(mmp.personalSearchSettings, new List<MatchmakingPresence>() { mmp });
                            return;
                        }
                    }

                    // Found a queue
                    foundQueueSettings = settingobj;
                }
            }

            if (foundQueueSettings != null)
            {
                // Found a game, the foundQueueSettings variable is not null
                foundQueueSettings.Value.Value.Add(mmp);
                // if the game is now full (10 players) send a message to every player with the needed info to connect to the game server and send the message to the game server itself
                if (foundQueueSettings.Value.Value.Count == 10) // Maybe also check if > 10?
                {
                    WriteLog("Created a full queue, sending needed info to all users and the game server.");

                    GameServerMirror gsm = null;
                    foreach (var gameservermirror in registeredGameServers)
                    {
                        if (!gameservermirror.isCurrentlyUsed)
                        {
                            gsm = gameservermirror;
                            break;
                        }
                    }

                    if (gsm == null)
                    {
                        WriteLog("All registered game servers are currently in use. Requeueing users...");
                        return;
                    }
                    else
                    {
                        gsm.isCurrentlyUsed = true;
                    }

                    MatchmakingAnswer answerToAllInList = new MatchmakingAnswer(MatchmakingSuccessState.MatchFound, foundQueueSettings.Value.Value, gsm.endPoint);
                    foreach (var player in foundQueueSettings.Value.Value)
                    {
                        SendAnswerToMMPresence(player, answerToAllInList);
                    }
                    currentSettingsAndUsers.Remove(foundQueueSettings.Value.Key);
                }
            }
        }
        private void DequeueFromMM(MatchmakingPresence mmp)
        {
            List<MatchmakingPresence> queue = null;
            foreach (var queuesnsettings in currentSettingsAndUsers)
            {
                foreach (var user in queuesnsettings.Value)
                {
                    if (user == mmp)
                    {
                        queue = queuesnsettings.Value;
                        break;
                    }
                }
                if(queue != null)
                    break;
            }

            if (queue != null)
                queue.Remove(mmp);
        }

        private void WriteLog(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString() + ": " + msg);
        }
    }
}
