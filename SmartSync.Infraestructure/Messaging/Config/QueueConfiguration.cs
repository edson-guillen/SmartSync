using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Config
{
    public class QueueConfiguration
    {
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; } = string.Empty;
        public string DeadLetterExchange { get; set; }
        public int MessageTtl { get; set; } = 0; // em milissegundos (opcional)
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
    }
}
