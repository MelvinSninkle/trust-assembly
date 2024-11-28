using System.Diagnostics;

namespace Scrape_Headlines.Utilities
{
    public static class Log
    {
        public static void Info(object message)
        {
            var mess = $"{DateTime.Now.ToString("dd HH:mm:ss")} INFO: {message.ToString()}";
            LogForNow(mess);
        }

        public static void Debug(object message)
        {
            var mess = $"{DateTime.Now.ToString("dd HH:mm:ss")} DEBUG: {message.ToString()}";
            LogForNow(mess);
        }

        public static void Error(object message)
        {
            var mess = $"{DateTime.Now.ToString("dd HH:mm:ss")} ERROR: {message.ToString()}";
            LogForNow(mess);
        }

        public static void Warning(object message)
        {
            var mess = $"{DateTime.Now.ToString("dd HH:mm:ss")} WARN: {message.ToString()}";
            LogForNow(mess);
        }

        public static void Warn(object message)
        {
            var mess = $"{DateTime.Now.ToString("dd HH:mm:ss")} WARN: {message.ToString()}";
            LogForNow(mess);
        }

        //TODO: use a proper log framework like serilog!
        public static string log_file { get; set; } = @"C:\temp\scraper_log.txt";

        private static void LogForNow(string message)
        {
            // always trace, console, and file
            Trace.WriteLine(message);
            Console.WriteLine(message);
            File.AppendAllText(log_file, message);
        }
    }
}
