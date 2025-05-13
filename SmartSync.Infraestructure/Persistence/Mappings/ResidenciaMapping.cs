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
    public class ResidenciaMapping : BaseMapping<Residencia>
    {
        public override void Configure(EntityTypeBuilder<Residencia> builder)
        {
            base.Configure(builder);
            builder.Property(r => r.Endereco).IsRequired();
            builder.HasMany(r => r.Comodos).WithOne().HasForeignKey(c => c.ResidenciaId);
        }
    }
}
