using SmartSync.Infraestructure.Logging.Interfaces;
using System;

namespace SmartSync.Infraestructure.Logging.Providers
{
    public class MessageLogger : IMessageLogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }

        public void LogError(string message, Exception ex)
        {
            Console.WriteLine($"ERROR: {message}\n{ex}");
        }
    }
}