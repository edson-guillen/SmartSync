using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class DispositivoController(IBaseService<Dispositivo> service, ILogger<Dispositivo> logger) : BaseController<Dispositivo>(service, logger)
    {
    }
}
