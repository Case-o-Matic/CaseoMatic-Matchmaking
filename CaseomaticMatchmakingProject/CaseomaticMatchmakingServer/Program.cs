using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingServer
{
    class Program
    {
        public const string version = "1.00";
        public static MatchmakingCenter mmCenter;

        static void Main(string[] args)
        {
            Console.WriteLine("Case-o-Matic Matchmaking Center (v" + version + ")");
            Console.Write("Select a port> ");
            string inputPort = Console.ReadLine();

            #region Initializing
            mmCenter = new MatchmakingCenter(int.Parse(inputPort));
            mmCenter.Start();
            #endregion

            while (true)
            {
                Console.Write("Command> ");
                string commandFull = Console.ReadLine();
                string commandName = commandFull;
                string[] commandArgs = new string[0];
                if (commandFull.Contains(":"))
                {
                    string[] commandFullSplitted = commandName.Split(':');
                    commandName = commandFullSplitted[0];
                    commandArgs = commandFullSplitted[1].Split(',');
                }

                if (commandName == "stop")
                {
                    mmCenter.Stop();

                    Console.ReadKey();
                    return;
                }
                else if (commandName == "add_gameserver ")
                {
                    string[] gamemanagerendpoint = commandName.Split(' ')[1].Split(':');

                    GameServerMirror gameServerMirror = new GameServerMirror() { endPoint = new IPEndPoint(IPAddress.Parse(gamemanagerendpoint[0]), int.Parse(gamemanagerendpoint[1])) };
                    mmCenter.RegisterNewGameServer(gameServerMirror);
                }
                else if (commandName == "remove_gameserver ")
                {
                    string[] gamemanagerendpoint = commandName.Split(' ')[1].Split(':');

                    GameServerMirror gameServerMirror = mmCenter.registeredGameServers.Find((g) => { if (g.endPoint.Address == IPAddress.Parse(gamemanagerendpoint[0]) && g.endPoint.Port == int.Parse(gamemanagerendpoint[1])) return true; else return false; });
                    mmCenter.UnregisterGameServer(gameServerMirror);
                }
                else if (commandName == "print_addedgameservers")
                {
                    Console.WriteLine();
                    Console.WriteLine("All added gameservers:");
                    foreach (var gameservermirror in mmCenter.registeredGameServers)
                    {
                        Console.WriteLine(gameservermirror.endPoint.ToString() + " (ping: " + gameservermirror.GetPing().ToString() + "ms)");
                    }
                }
                else if (commandName == "print_currentqueues")
                {
                    foreach (var queue in mmCenter.currentSettingsAndUsers)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Matchmaking-settings ID: " + queue.Key.GetHashCode().ToString());
                        Console.WriteLine("Users searching under this settings:");
                        foreach (var user in queue.Value)
                        {
                            Console.WriteLine("MM-Presence ID: " + user.playerId.ToString() + ", IP-Endpoint: " + user.externalEndPoint.ToString());
                        }
                    }
                }
                else if (commandName == "set_neededuserqueuelength")
                {
                    mmCenter.neededUserQueueLength = int.Parse(commandArgs[0]);
                }
            }
        }
    }
}
