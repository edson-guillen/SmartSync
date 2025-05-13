using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Logging.Interfaces;
using SmartSync.Infraestructure.Messaging.Middleware;
using SmartSync.Infraestructure.Messaging.Publishers;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Handler
{
    public class AcenderLuzesComodoEventHandler
    {
        private readonly ILogger<AcenderLuzesComodoEventHandler> _logger;
        private readonly RetryHandler _retryHandler;
        private readonly MessagePublisher _publisher;
        private readonly IModel _channel; // Canal RabbitMQ para DLQ

        private const string ExchangeName = "smart.sync.acender.fanout";
        private const string DeadLetterExchange = "smart.sync.acender.fanout.dlx";

        public AcenderLuzesComodoEventHandler(
            ILogger<AcenderLuzesComodoEventHandler> logger,
            RetryHandler retryHandler,
            MessagePublisher publisher,
            IModel channel)
        {
            _logger = logger;
            _retryHandler = retryHandler;
            _publisher = publisher;
            _channel = channel;
        }

        public async Task HandleAsync(AcenderLuzesComodoEvent evento)
        {
            try
            {
                _logger.LogInformation($"[Handler] Iniciando processo para acender luzes do cômodo {evento.ComodoId}");

                // Retry para envio de mensagem de broadcast para o Fanout Exchange
                await _retryHandler.ExecuteWithRetryAsync(async () =>
                {
                    var mensagemBroadcast = new
                    {
                        ComodoId = evento.ComodoId,
                        Comando = "Acender"
                    };

                    _publisher.Publish(ExchangeName, routingKey: "", mensagemBroadcast);
                    _logger.LogInformation($"[Handler] Broadcast enviado para o Exchange '{ExchangeName}'.");
                });

                // Simulação de envio para dispositivos (apenas replicando a lógica de broadcast para validação)
                // Se necessário, o envio individual pode ser ajustado aqui
                _logger.LogInformation($"[Handler] Mensagem de broadcast processada para o cômodo {evento.ComodoId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Handler] Erro ao processar evento para o cômodo {evento.ComodoId}");

                // Enviar mensagem para Dead Letter Queue (DLQ) após falha
                var bodyDlq = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));
                _channel.BasicPublish(
                    exchange: DeadLetterExchange,
                    routingKey: "",
                    basicProperties: null,
                    body: bodyDlq
                );

                _logger.LogInformation($"[Handler] Evento enviado para Dead Letter Exchange '{DeadLetterExchange}'.");
            }
        }
    }
}