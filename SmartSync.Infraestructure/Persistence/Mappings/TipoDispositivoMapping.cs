using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Mappings
{
    public class TipoDispositivoMapping : BaseMapping<TipoDispositivo>
    {
        public override void Configure(EntityTypeBuilder<TipoDispositivo> builder)
        {
            base.Configure(builder);
            builder.Property(t => t.Nome).IsRequired();
        }
    }
}
