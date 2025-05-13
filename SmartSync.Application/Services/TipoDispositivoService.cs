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
    public class TipoDispositivoService : BaseService<TipoDispositivo>, ITipoDispositivoService
    {
        public TipoDispositivoService(IBaseRepository<TipoDispositivo> repository) : base(repository)
        {
        }
    }
}
