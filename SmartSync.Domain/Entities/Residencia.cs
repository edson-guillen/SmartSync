using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class Residencia : BaseModel
    {
        public required string Endereco { get; set; }
        public Guid UsuarioId { get; set; }
        public ICollection<Comodo>? Comodos { get; set; }
    }

}
