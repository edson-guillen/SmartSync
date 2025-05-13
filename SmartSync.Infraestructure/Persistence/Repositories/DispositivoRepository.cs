using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Repositories
{
    public class DispositivoRepository : BaseRepository<Dispositivo>
    {
        public DispositivoRepository(SmartSyncDbContext context) : base(context) { }
    }
}
