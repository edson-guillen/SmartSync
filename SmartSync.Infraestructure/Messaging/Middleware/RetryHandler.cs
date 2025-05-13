using System;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Middleware
{
    public class RetryHandler
    {
        private const int MaxRetries = 3;

        public async Task ExecuteWithRetryAsync(Func<Task> action)
        {
            int attempt = 0;

            while (attempt < MaxRetries)
            {
                try
                {
                    attempt++;
                    await action();
                    return;
                }
                catch (Exception ex) when (attempt < MaxRetries)
                {
                    Console.WriteLine($"Tentativa {attempt} falhou. Retentando... Erro: {ex.Message}");
                }
            }

            throw new InvalidOperationException("Número máximo de tentativas atingido.");
        }
    }
}