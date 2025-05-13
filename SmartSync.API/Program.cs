using RabbitMQ.Client;
using SmartSync.Infraestructure.Logging.Interfaces;
using SmartSync.Infraestructure.Logging.Providers;
using SmartSync.Infraestructure.Messaging.Config;
using SmartSync.Infraestructure.Messaging.Consumers;
using SmartSync.Infraestructure.Messaging.Handler;
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

            //// Add services to the container.
            //builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

            //// Register RabbitMQ Connection and Channel
            //builder.Services.AddSingleton(sp =>
            //{
            //    var configuration = sp.GetRequiredService<IConfiguration>();
            //    var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
            //    var factory = new ConnectionFactory
            //    {
            //        HostName = rabbitMqSettings.HostName,
            //        UserName = rabbitMqSettings.UserName,
            //        Password = rabbitMqSettings.Password,
            //        VirtualHost = rabbitMqSettings.VirtualHost,
            //        DispatchConsumersAsync = true
            //    };
            //    var connection = factory.CreateConnection();
            //    return connection.CreateModel();
            //});

            //// IMPORTANTE: Todos os servi�os que interagem com o RabbitMQ devem ser Singleton
            //// para garantir que eles usem a mesma inst�ncia do canal

            //// Register Logging (sempre injetar como Singleton)
            //builder.Services.AddSingleton<IMessageLogger, MessageLogger>();

            //// Register Queue Configuration Service
            //builder.Services.AddSingleton<QueueConfigurationService>();

            //// Register RetryHandler
            //builder.Services.AddSingleton<RetryHandler>();

            //// Register Message Publisher
            //builder.Services.AddSingleton<MessagePublisher>();

            //// Register Event Bus
            //builder.Services.AddSingleton<IEventBus, EventBusRabbitMq>();

            //// IMPORTANTE: Ordem de inicializa��o dos servi�os
            //// 1. QueueInitializer: deve ser iniciado primeiro para criar todas as filas
            //// 2. Consumidores: devem ser iniciados depois para consumir mensagens das filas existentes

            //// Initialize Queues at Startup - PRIMEIRO SERVI�O A SER INICIALIZADO
            //builder.Services.AddSingleton<IHostedService, QueueInitializer>();

            //// Register Consumers DEPOIS do QueueInitializer
            //builder.Services.AddSingleton<AcenderLuzesComodoConsumer>();
            //builder.Services.AddHostedService(sp => sp.GetRequiredService<AcenderLuzesComodoConsumer>());

            //// Register Handlers como Singleton tamb�m para evitar problemas com DI
            //builder.Services.AddSingleton<AcenderLuzesComodoEventHandler>();

            ////var factory = new ConnectionFactory
            ////{
            ////    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMq"))
            ////};

            ////using var connection = factory.CreateConnection();
            ////using var channel = connection.CreateModel();

            ////var configService = new QueueConfigurationService(channel);
            ////configService.DeclareQueueWithDLQ(new QueueConfiguration
            ////{
            ////    QueueName = "smart.sync.acender.comodo",
            ////    ExchangeName = "smart.sync.acender.fanout",
            ////    DeadLetterExchange = "smart.sync.acender.fanout.dlx",
            ////    MessageTtl = 30000
            ////});

            // Configure Services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure Middleware
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