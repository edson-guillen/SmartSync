using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class Usuario : BaseModel
    {
        public required string Nome { get; set; }
        public string? Email { get; set; }
        public ICollection<Residencia>? Residencias { get; set; }
    }
}
