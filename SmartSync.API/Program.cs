using SmartSync.Infraestructure.Persistence.Context;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using SmartSync.API.Extensions;
using Microsoft.Extensions.Configuration; 
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Messaging.Publisher;
using SmartSync.Infraestructure.Messaging.Subscriber;
using SmartSync.Infraestructure.Messaging;

namespace SmartSync.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .AddOData(opt => opt.Select()
                    .Expand()
                    .SetMaxTop(null)
                    .SkipToken()
                    .OrderBy()
                    .Count()
                    .Filter()
                    .EnableQueryFeatures(1000)
                )
                .AddODataNewtonsoftJson();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddDbContext<SmartSyncDbContext>(options =>
            {
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif
            });

            builder.Services.ConfigureRepositories();
            builder.Services.ConfigureServices();

            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "CorsPolicy",
                policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // RabbitMQ - CloudAMQP
            builder.Services.AddSingleton<RabbitMqOptions>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new RabbitMqOptions
                {
                    ConnectionString = "amqps://jzbrpolr:sEJrOHF_O46SJenrAhxBvKbL-IZwvNPP@jackal.rmq.cloudamqp.com/jzbrpolr"
                };
            });
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddSingleton<IRabbitMqSubscriber, RabbitMqSubscriber>();
            builder.Services.AddHostedService<EntityCreatedSubscriber>();


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<SmartSyncDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocorreu um erro ao atualizar o banco de dados.");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CorsPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}