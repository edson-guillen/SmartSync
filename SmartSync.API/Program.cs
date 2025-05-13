using RabbitMQ.Client;
using SmartSync.Infraestructure.Logging;
using SmartSync.Infraestructure.Messaging;
using SmartSync.Infraestructure.Messaging.Config;
using SmartSync.Infraestructure.Messaging.Consumers;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Messaging.Middleware;
using SmartSync.Infraestructure.Messaging.Publishers;
using SmartSync.Infraestructure.Messaging.Queueing;

namespace SmartSync.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
            builder.Services.AddSingleton<IEventBus, EventBusRabbitMq>();
            builder.Services.AddHostedService<AcenderLuzesComodoConsumer>();
            builder.Services.AddSingleton<RetryHandler>();
            builder.Services.AddScoped<IMessageLogger, MessageLogger>();


            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://jzbrpolr:sEJrOHF_O46SJenrAhxBvKbL-IZwvNPP@jackal.rmq.cloudamqp.com/jzbrpolr")
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var configService = new QueueConfigurationService(channel);
            configService.DeclareQueueWithDLQ(new QueueConfiguration
            {
                QueueName = "smart.sync.acender.comodo",
                ExchangeName = "smart.sync.acender.fanout",
                DeadLetterExchange = "smart.sync.acender.fanout.dlx",
                MessageTtl = 30000 // 30 segundos
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            QueueInitializer.RegisterQueues(app.Services);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
