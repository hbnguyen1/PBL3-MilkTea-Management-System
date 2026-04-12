using System;
using System.IO;

namespace PBL3.Core
{
    internal class Logger
    {
        private static readonly string logDir = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
            "Logs"
        );

        private static string GetLogFile()
        {
            string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
            return Path.Combine(logDir, fileName);
        }

        public static void Log(string level, string message)
        {
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}\n";

                File.AppendAllText(GetLogFile(), log);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi log: " + ex.Message);
            }
        }

        public static void Info(string message) => Log("INFO", message);
        public static void Error(string message) => Log("ERROR", message);
        public static void Warning(string message) => Log("WARNING", message);
    }
}