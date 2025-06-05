using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;

namespace SmartSync.API.Controllers
{
    public class ResidenciaController(IResidenciaService service, IRabbitMqPublisher publisher, ILogger<Residencia> logger)
        : BaseController<Residencia>(service, logger)
    {
        [HttpPost("{id}/acao")]
        public async Task<IActionResult> EnviarAcao(Guid id, [FromQuery] string acao)
        {
            if (acao != "Ligar" && acao != "Desligar")
                return BadRequest("Ação inválida. Use 'Ligar' ou 'Desligar'.");

            // Envia mensagem para a exchange da residência
            await publisher.PublishToFanoutAsync($"residencia.{id}", new AcaoCommand { ResidenciaId = id, Acao = acao });
            return Ok(new { ResidenciaId = id, Acao = acao });
        }
    }
}
