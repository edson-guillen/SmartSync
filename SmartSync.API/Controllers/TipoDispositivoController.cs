using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class TipoDispositivoController(IBaseService<TipoDispositivo> service) : BaseController<TipoDispositivo>(service)
    {
    }
}
