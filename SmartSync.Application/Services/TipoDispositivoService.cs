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
    public class TipoDispositivoService(ITipoDispositivoRepository repository, IRabbitMqPublisher publisher) : BaseService<TipoDispositivo>(repository, publisher), ITipoDispositivoService
    {
    }
}
