using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class ResidenciaService(IResidenciaRepository repository, IRabbitMqPublisher publisher) : BaseService<Residencia>(repository, publisher), IResidenciaService
    {
    }
}
