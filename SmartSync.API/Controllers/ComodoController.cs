using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class ComodoController(IComodoService service, ILogger<Comodo> logger) : BaseController<Comodo>(service, logger)
    {
        //private readonly IComodoService _comodoService = comodoService;

        //[HttpPost("{comodoId}/acender-luzes")]
        //public IActionResult AcenderLuzes(Guid comodoId, [FromServices] IEventBus eventBus)
        //{
        //    _comodoService.AcenderLuzesAsync(comodoId);
        //    return Ok($"Comando enviado para acender as luzes do comodo id {comodoId}");
        //}
    }
}
