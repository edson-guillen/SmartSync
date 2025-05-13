using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging.Middleware;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Handler
{
    public abstract class BaseRetryHandler<TEvent>
    {
        private readonly ILogger _logger;
        private readonly RetryHandler _retryHandler;
        private readonly IModel _channel;
        private readonly string _deadLetterExchange;

        protected BaseRetryHandler(
            ILogger logger,
            RetryHandler retryHandler,
            IModel channel,
            string deadLetterExchange)
        {
            _logger = logger;
            _retryHandler = retryHandler;
            _channel = channel;
            _deadLetterExchange = deadLetterExchange;
        }

        protected async Task ExecuteWithRetryAsync(TEvent evento, Func<Task> action)
        {
            try
            {
                await _retryHandler.ExecuteWithRetryAsync(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Handler] Falha ao processar evento. Enviando para DLQ.");

                // Enviar para DLQ
                var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(evento));
                _channel.BasicPublish(
                    exchange: _deadLetterExchange,
                    routingKey: "",
                    basicProperties: null,
                    body: body
                );

                _logger.LogInformation("[Handler] Evento enviado para Dead Letter Exchange.");
            }
        }
    }
}