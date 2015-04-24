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
                Console.Write("Matchmaking-center port (int)> ");
                int centerPort;
                if(!int.TryParse(Console.ReadLine(), out centerPort))
                {
                    Console.WriteLine("Thats no integer!");
                    goto enqueue;
                }
                Console.Write("Local port (int)> ");
                int localPort;
                if (!int.TryParse(Console.ReadLine(), out localPort))
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

                MatchmakingManager.Start(localPort, new MatchmakingPresence(new MatchmakingSearchSettings(new List<MatchmakingSearchSettingsCondition>()
                {
                    new MatchmakingSearchSettingsCondition(deathmatch)
                })), new IPEndPoint(IPAddress.Loopback, centerPort));
                MatchmakingManager.OnMatchFound += MatchmakingManager_MatchFound;

                MatchmakingManager.QueueMe();

                while (Console.ReadKey().Key != ConsoleKey.Enter)
                    Console.WriteLine();

                MatchmakingManager.DequeueMe();
            }
            else goto enqueue;
        }

        private static void MatchmakingManager_MatchFound(MatchmakingFoundInfo info)
        {
            Console.WriteLine("You found a game!");
            Console.WriteLine("Game-info:\nGame-server endpoint> " + info.gameServerEndPoint);
            info.allUsers.ForEach((u) => { Console.WriteLine("User> " + u.playerId + ", " + u.externalEndPoint); });
        }
    }
}
