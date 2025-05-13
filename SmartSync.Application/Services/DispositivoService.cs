using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class DispositivoService : BaseService<Dispositivo>, IDispositivoService
    {
        public DispositivoService(IBaseRepository<Dispositivo> repository) : base(repository)
        {
        }
    }
}
