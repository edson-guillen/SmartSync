using Microsoft.Extensions.Logging;

namespace SmartSync.Infraestructure.Logging
{
    public class MessageLogger : IMessageLogger
    {
        private readonly ILogger<MessageLogger> _logger;

        public MessageLogger(ILogger<MessageLogger> logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message)
        {
            _logger.LogInformation("[RabbitMQ] " + message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.LogError(ex, "[RabbitMQ] " + message);
        }
    }
}
