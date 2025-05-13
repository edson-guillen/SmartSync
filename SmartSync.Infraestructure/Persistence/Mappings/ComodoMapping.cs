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
    public class ComodoMapping : BaseMapping<Comodo>
    {
        public override void Configure(EntityTypeBuilder<Comodo> builder)
        {
            base.Configure(builder);
            builder.Property(c => c.Nome).IsRequired();
            builder.HasMany(c => c.Dispositivos).WithOne().HasForeignKey(d => d.ComodoId);
        }
    }
}
