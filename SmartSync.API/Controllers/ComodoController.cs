using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;

namespace SmartSync.API.Controllers
{
    public class ComodoController(IComodoService service, IRabbitMqPublisher publisher, ILogger<Comodo> logger)
        : BaseController<Comodo>(service, logger)
    {
        [HttpPost("{id}/acao")]
        public async Task<IActionResult> EnviarAcao(Guid id, [FromQuery] string acao)
        {
            if (acao != "Ligar" && acao != "Desligar")
                return BadRequest("Ação inválida. Use 'Ligar' ou 'Desligar'.");

            // Envia mensagem para a exchange do cômodo
            await publisher.PublishToFanoutAsync($"comodo.{id}", new AcaoCommand { ComodoId = id, Acao = acao });
            return Ok(new { ComodoId = id, Acao = acao });
        }
    }
}
