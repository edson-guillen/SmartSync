using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class ResidenciaService : BaseService<Residencia>, IResidenciaService
    {
        public ResidenciaService(IBaseRepository<Residencia> repository) : base(repository)
        {
        }
    }
}
