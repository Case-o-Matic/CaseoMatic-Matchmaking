using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseomaticMatchmakingClient
{
    public static class MatchmakingLog
    {
        public static bool saveLog { get; set; }
        private static List<string> messages = new List<string>();

        public static void WriteLog(string msg)
        {
            string text = DateTime.Now.ToString() + ": " + msg;

            Console.WriteLine();
            Console.WriteLine(text);
        }
        public static void SaveLog(string path)
        {
            if (!saveLog)
                return;

            messages.Insert(0, "---"); // Give a few informations out?
            File.AppendAllLines(path, messages.ToArray());
        }
    }
}
