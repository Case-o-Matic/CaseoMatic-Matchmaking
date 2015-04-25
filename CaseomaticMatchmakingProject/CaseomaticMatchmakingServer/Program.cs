using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingServer
{
    class Program
    {
        public static MatchmakingCenter mmCenter;

        static void Main(string[] args)
        {
            Console.WriteLine("Case-o-Matic Matchmaking Center (v" + MatchmakingCenter.version + ")");
            Console.WriteLine("Write \"?\" for help");
            Console.WriteLine();

            #region Port selection
            port:
            Console.Write("Select a port> ");
            string inputPort = Console.ReadLine();

            int port;
            if (!int.TryParse(inputPort, out port))
            {
                Console.WriteLine(port + " is no valid integer, retry.");
                goto port;
            }
            #endregion

            #region Initializing
            mmCenter = new MatchmakingCenter(port);
            mmCenter.Start();
            #endregion

            loop:
            try
            {
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
                    else if (commandName == "add_gameserver")
                    {
                        GameServerMirror gameServerMirror = new GameServerMirror() { endPoint = new IPEndPoint(IPAddress.Parse(commandArgs[0]), int.Parse(commandArgs[1])) };
                        mmCenter.RegisterNewGameServer(gameServerMirror);
                    }
                    else if (commandName == "remove_gameserver")
                    {
                        GameServerMirror gameServerMirror = mmCenter.registeredGameServers.Find((g) => { if (g.endPoint.Address == IPAddress.Parse(commandArgs[0]) && g.endPoint.Port == int.Parse(commandArgs[1])) return true; else return false; });
                        mmCenter.UnregisterGameServer(gameServerMirror);
                    }
                    else if (commandName == "print_registeredgameservers")
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
                        int value = int.Parse(commandArgs[0]);
                        if (value > 0 && value <= MatchmakingCenter.maxNeededUserQueueLength)
                        {
                            mmCenter.neededUserQueueLength = value;
                            Console.WriteLine("Set the needed user queue length to " + mmCenter.neededUserQueueLength);
                        }
                        else
                            Console.WriteLine("The needed user queue length must be between 1 and " + MatchmakingCenter.maxNeededUserQueueLength);
                    }
                    else if(commandName == "?")
                    {
                        if (File.Exists("commands.txt"))
                            Process.Start("commands.txt");
                        else
                            Console.WriteLine("commands.txt not found!");
                    }
                    else
                        Console.WriteLine("The command \"" + commandFull + "\" is not valid");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error; " + ex.ToString());
                goto loop;
            }
        }
    }
}
