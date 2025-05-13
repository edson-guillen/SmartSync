using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Utils
{
    public static class RetryPolicyHelper
    {
        public static int GetRetryCount(IBasicProperties props)
        {
            if (props.Headers != null && props.Headers.ContainsKey("x-retry"))
            {
                var value = props.Headers["x-retry"] as byte[];
                var retryString = Encoding.UTF8.GetString(value ?? []);
                return int.TryParse(retryString, out var count) ? count : 0;
            }
            return 0;
        }

        public static void AddRetryHeader(IBasicProperties props, int retryCount)
        {
            if (props.Headers == null)
                props.Headers = new Dictionary<string, object>();

            props.Headers["x-retry"] = Encoding.UTF8.GetBytes(retryCount.ToString());
        }
    }
}
