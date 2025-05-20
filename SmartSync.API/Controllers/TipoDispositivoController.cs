using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class TipoDispositivoController(ITipoDispositivoService service, ILogger<TipoDispositivo> logger) : BaseController<TipoDispositivo>(service, logger)
    {
    }
}
