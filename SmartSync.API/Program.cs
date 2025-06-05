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
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartSync.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

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
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif
            });

            builder.Services.ConfigureRepositories();
            builder.Services.ConfigureServices();

            builder.Services.AddSwaggerGen();

            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database")
                .AddCheck<RabbitMqHealthCheck>("rabbitmq");


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
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

            builder.Services.AddSingleton<IModel>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var factory = new ConnectionFactory { Uri = new Uri(options.ConnectionString) };
                var connection = factory.CreateConnection();
                return connection.CreateModel();
            });
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddSingleton<IRabbitMqSubscriber, RabbitMqSubscriber>();
            builder.Services.AddHostedService<EntityCreatedSubscriber>();
            builder.Services.AddHostedService<DispositivoCommandSubscriber>();
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);

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

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var response = new HealthCheckResponse
                    {
                        Status = report.Status.ToString(),
                        Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                        Checks = report.Entries.ToDictionary(
                            e => e.Key,
                            e => new
                            {
                                Status = e.Value.Status.ToString(),
                                Description = e.Value.Description,
                                Duration = e.Value.Duration
                            } as object
                        ),
                        Duration = report.TotalDuration,
                        Timestamp = DateTime.UtcNow
                    };

                    context.Response.ContentType = "application/json";
                    await JsonSerializer.SerializeAsync(context.Response.Body, response, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
            });

            app.MapControllers();

            app.Run();
        }
    }
}