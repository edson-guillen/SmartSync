using Microsoft.Extensions.Logging;
using SmartSync.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Handler
{
    public class AcenderLuzesComodoEventHandler
    {
        private readonly ILogger<AcenderLuzesComodoEventHandler> _logger;

        public AcenderLuzesComodoEventHandler(ILogger<AcenderLuzesComodoEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(AcenderLuzesComodoEvent evento)
        {
            _logger.LogInformation($"[Handler] Acendendo luzes do cômodo {evento.ComodoId}");
            // Aqui entra a lógica de acender luzes
            return Task.CompletedTask;
        }
    }

}
