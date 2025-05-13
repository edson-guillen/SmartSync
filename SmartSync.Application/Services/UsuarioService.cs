using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class UsuarioService : BaseService<Usuario>, IUsuarioService
    {
        public UsuarioService(IBaseRepository<Usuario> repository) : base(repository)
        {
        }
    }
}
