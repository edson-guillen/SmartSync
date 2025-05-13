using RabbitMQ.Client;
using SmartSync.Infraestructure.Logging.Interfaces;
using System;
using System.Text;
using System.Text.Json;

namespace SmartSync.Infraestructure.Messaging.Publishers
{
    public class MessagePublisher
    {
        private readonly IModel _channel;
        private readonly IMessageLogger _logger;

        public MessagePublisher(IModel channel, IMessageLogger logger)
        {
            _channel = channel;
            _logger = logger;
        }

        public void Publish(string exchange, string routingKey, object message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                _channel.BasicPublish(exchange, routingKey, null, body);
                _logger.LogInfo($"[Publisher] Mensagem publicada no exchange '{exchange}' com routing key '{routingKey}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Publisher] Erro ao publicar mensagem no exchange '{exchange}'.", ex);
                throw;
            }
        }
    }
}