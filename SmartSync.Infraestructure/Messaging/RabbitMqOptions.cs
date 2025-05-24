namespace SmartSync.Infraestructure.Messaging
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public string ConnectionString { get; set; } = "amqps://jzbrpolr:sEJrOHF_O46SJenrAhxBvKbL-IZwvNPP@jackal.rmq.cloudamqp.com/jzbrpolr"; // opcional: caso prefira usar connection string do CloudAMQP
    }
}