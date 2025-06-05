using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Context
{
    public class SmartSyncDbContext(DbContextOptions<SmartSyncDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        private readonly IConfiguration _configuration;

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Residencia> Residencias { get; set; }
        public DbSet<Comodo> Comodos { get; set; }
        public DbSet<Dispositivo> Dispositivos { get; set; }
        public DbSet<TipoDispositivo> TipoDispositivos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ComodoMapping());
            modelBuilder.ApplyConfiguration(new UsuarioMapping());
            modelBuilder.ApplyConfiguration(new ResidenciaMapping());
            modelBuilder.ApplyConfiguration(new DispositivoMapping());
            modelBuilder.ApplyConfiguration(new TipoDispositivoMapping());
        }
    }
}
