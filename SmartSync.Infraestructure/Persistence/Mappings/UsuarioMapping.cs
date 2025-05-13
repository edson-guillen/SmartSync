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
    public class UsuarioMapping : BaseMapping<Usuario>
    {
        public override void Configure(EntityTypeBuilder<Usuario> builder)
        {
            base.Configure(builder);
            builder.Property(u => u.Nome).IsRequired();
            builder.Property(u => u.Email).IsRequired();
            builder.HasMany(u => u.Residencias).WithOne().HasForeignKey(r => r.UsuarioId);
        }
    }
}
