using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class Dispositivo : BaseModel
    {
        public required string Nome { get; set; }
        public Guid TipoDispositivoId { get; set; }
        public Guid ComodoId { get; set; }
        public TipoDispositivo? TipoDispositivo { get; set; }
    }
}
