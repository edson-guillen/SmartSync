using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class ResidenciaController(IBaseService<Residencia> service) : BaseController<Residencia>(service)
    {
    }
}
