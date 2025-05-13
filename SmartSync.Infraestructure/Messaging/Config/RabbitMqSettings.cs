using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Config
{
    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string ExchangeName { get; set; } = "smartsync.fanout";
        public string DeadLetterExchange { get; set; } = "smartsync.dlq";
        public string RetryExchange { get; set; } = "smartsync.retry";
    }
}
