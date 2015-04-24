using CaseomaticMatchmakingClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExampleMatchmakingClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press \"Enter\" to get queued.");

            enqueue:
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                Console.WriteLine("Matchmaking-center port (int)> ");
                int port;
                if(!int.TryParse(Console.ReadLine(), out port))
                {
                    Console.WriteLine("Thats no integer!");
                    goto enqueue;
                }

                Console.Write("Search settings: deathmatch (boolean)> ");
                bool deathmatch;
                if (!bool.TryParse(Console.ReadLine(), out deathmatch))
                {
                    Console.WriteLine("Thats no boolean!");
                    goto enqueue;
                }

                MatchmakingManager.Start(new MatchmakingPresence(new MatchmakingSearchSettings(new List<MatchmakingSearchSettingsCondition>()
                {
                    new MatchmakingSearchSettingsCondition(deathmatch)
                })), new IPEndPoint(IPAddress.Loopback, port));
                MatchmakingManager.MatchFound += MatchmakingManager_MatchFound;

                MatchmakingManager.QueueMe();
            }
            else goto enqueue;
        }

        private static void MatchmakingManager_MatchFound(MatchmakingFoundInfo info)
        {
            Console.WriteLine("You found a game!");
            Console.WriteLine("Game-info:\nGame-server endpoint> " + info.gameServerEndPoint.ToString());
            info.allUsers.ForEach((u) => { Console.WriteLine("User> " + u.playerId + ", " + u.externalEndPoint.ToString()); });
        }
    }
}
