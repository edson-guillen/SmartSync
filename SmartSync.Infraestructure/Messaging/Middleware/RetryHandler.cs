using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Middleware
{
    public class RetryHandler
    {
        private readonly ILogger<RetryHandler> _logger;

        public RetryHandler(ILogger<RetryHandler> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteWithRetry(Func<Task> action, int maxRetries = 3)
        {
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning($"Tentativa {retryCount} falhou: {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "Erro após número máximo de tentativas.");
                        throw;
                    }

                    await Task.Delay(2000);
                }
            }
        }
    }

}
