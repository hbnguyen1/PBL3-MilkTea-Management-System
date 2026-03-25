using System;
using System.IO;

namespace PBL3.Core
{
    internal class Logger
    {
        private static string logFile = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "system_log.txt");

        public static void Log(string message)
        {
            string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFile, log);
        }

        public static void Info(string message)
        {
            Log("[INFO] " + message);
        }

        public static void Error(string message)
        {
            Log("[ERROR] " + message);
        }

        public static void Warning(string message)
        {
            Log("[WARNING] " + message);
        }
    }
}