using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class TipoDispositivo : BaseModel
    {
        public required string Nome { get; set; }
        public ICollection<string>? Acoes { get; set; } = new List<string>();
    }
}
