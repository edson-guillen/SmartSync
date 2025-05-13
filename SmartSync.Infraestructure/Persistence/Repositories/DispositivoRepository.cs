using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Context;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Repositories
{
    public class DispositivoRepository : BaseRepository<Dispositivo>, IDispositivoRepository
    {
        public DispositivoRepository(SmartSyncDbContext context) : base(context) { }

        public async Task<List<Dispositivo>> ObterDispositivosPorComodoIdAsync(Guid comodoId, string tipo)
        {
            return await Task.FromResult(
                _context.Dispositivos
                    .Where(d => d.ComodoId == comodoId && d.TipoDispositivo.Nome == tipo)
                    .ToList()
            );
        }
    }
}
