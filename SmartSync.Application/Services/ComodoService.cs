using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class ComodoService : BaseService<Comodo>, IComodoService
    {
        private readonly IEventBus _eventBus;

        public ComodoService(IBaseRepository<Comodo> repository, IEventBus eventBus)
            : base(repository)
        {
            _eventBus = eventBus;
        }

        public async Task AcenderLuzesAsync(Guid comodoId)
        {
            var evento = new AcenderLuzesComodoEvent { ComodoId = comodoId };
            _eventBus.Publish(evento);
        }
    }
}
