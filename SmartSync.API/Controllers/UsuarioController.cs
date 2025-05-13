using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;

namespace SmartSync.API.Controllers
{
    public class UsuarioController(IBaseService<Usuario> service) : BaseController<Usuario>(service)
    {
    }
}
