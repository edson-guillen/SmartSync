using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;

namespace SmartSync.API.Controllers
{
    public class ComodoController(IComodoService comodoService) : BaseController<Comodo>(comodoService)
    {
        private readonly IComodoService _comodoService = comodoService;

        [HttpPost("{comodoId}/acender-luzes")]
        public IActionResult AcenderLuzes(Guid comodoId, [FromServices] IEventBus eventBus)
        {
            _comodoService.AcenderLuzesAsync(comodoId);
            return Ok($"Comando enviado para acender as luzes do comodo id {comodoId}");
        }
    }
}
