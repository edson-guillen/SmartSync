using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class Comodo : BaseModel
    {
        public required string Nome { get; set; }
        public Guid ResidenciaId { get; set; }
        public ICollection<Dispositivo>? Dispositivos { get; set; }
    }
}
