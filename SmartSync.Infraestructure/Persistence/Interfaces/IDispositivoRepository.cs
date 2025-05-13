using SmartSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Interfaces
{
    public interface IDispositivoRepository : IBaseRepository<Dispositivo>
    {
        Task<List<Dispositivo>> ObterDispositivosPorComodoIdAsync(Guid comodoId, string tipo);
    }
}
