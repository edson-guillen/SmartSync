using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Domain.Entities
{
    public class TipoDispositivo : BaseModel
    {
        public required string Nome { get; set; }

        [NotMapped]
        public ICollection<string>? Acoes { get; set; }
    }
}
