using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;

namespace SmartSync.API.Controllers
{
    public class DispositivoController(IDispositivoService service, IRabbitMqPublisher publisher, ILogger<Dispositivo> logger)
        : BaseController<Dispositivo>(service, logger)
    {
        [HttpPost("{id}/acao")]
        public async Task<IActionResult> EnviarAcao(Guid id, [FromQuery] string acao)
        {
            if (acao != "Ligar" && acao != "Desligar")
                return BadRequest("Ação inválida. Use 'Ligar' ou 'Desligar'.");

            // Envia mensagem para a fila do dispositivo
            await publisher.PublishAsync($"dispositivo.{id}", new AcaoCommand { DispositivoId = id, Acao = acao });

            return Ok(new { DispositivoId = id, Acao = acao });
        }
    }
}
