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
    public class DispositivoMapping : BaseMapping<Dispositivo>
    {
        public override void Configure(EntityTypeBuilder<Dispositivo> builder)
        {
            base.Configure(builder);
            builder.Property(d => d.Nome).IsRequired();
            builder.HasOne(d => d.TipoDispositivo).WithMany().HasForeignKey(d => d.TipoDispositivoId);
        }
    }
}
