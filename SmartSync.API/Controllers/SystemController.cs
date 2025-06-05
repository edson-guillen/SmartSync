using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Runtime.InteropServices;
using SmartSync.Infraestructure.Persistence.Context;
using System.Diagnostics;

namespace SmartSync.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SystemController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        SmartSyncDbContext dbContext) : ControllerBase
    {
        private static readonly DateTime StartTime = DateTime.UtcNow;
        private readonly IConfiguration _configuration = configuration;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly SmartSyncDbContext _dbContext = dbContext;

        [HttpGet("info")]
        public async Task<ActionResult<SystemInfo>> GetSystemInfo()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var process = Process.GetCurrentProcess();

            var info = new SystemInfo
            {
                ApiVersion = assembly.GetName().Version?.ToString() ?? "1.0.0",
                Environment = _environment.EnvironmentName,
                ServerTime = DateTime.Now,
                ServerTimeUtc = DateTime.UtcNow,
                TimeZone = TimeZoneInfo.Local.DisplayName,

                Resources = new SystemResources
                {
                    CpuUsage = process.TotalProcessorTime.TotalSeconds / (Environment.ProcessorCount * (DateTime.UtcNow - StartTime).TotalSeconds) * 100,
                    MemoryUsage = process.WorkingSet64,
                    TotalMemory = GC.GetTotalMemory(false),
                    DotNetVersion = Environment.Version.ToString(),
                    Uptime = DateTime.UtcNow - StartTime
                },

                RuntimeInfo = new Dictionary<string, string>
                {
                    ["Framework"] = RuntimeInformation.FrameworkDescription,
                    ["OS"] = RuntimeInformation.OSDescription,
                    ["ProcessorArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
                    ["RuntimeIdentifier"] = RuntimeInformation.RuntimeIdentifier
                },

                EnvironmentInfo = new Dictionary<string, string>
                {
                    ["MachineName"] = Environment.MachineName,
                    ["UserName"] = Environment.UserName,
                    ["ProcessorCount"] = Environment.ProcessorCount.ToString(),
                    ["CurrentDirectory"] = Environment.CurrentDirectory
                },

                Services = new Dictionary<string, bool>()
            };

            // Verificar status dos serviços
            try
            {
                await _dbContext.Database.CanConnectAsync();
                info.Services["Database"] = true;
            }
            catch
            {
                info.Services["Database"] = false;
            }

            // Verificar RabbitMQ (se estiver usando)
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_configuration.GetValue<string>("RabbitMq:ConnectionString"))
                };
                using var connection = factory.CreateConnection();
                info.Services["RabbitMQ"] = connection.IsOpen;
            }
            catch
            {
                info.Services["RabbitMQ"] = false;
            }

            return Ok(info);
        }

        [HttpGet("config")]
        public ActionResult<Dictionary<string, string>> GetConfigurationInfo()
        {
            // Apenas configurações não sensíveis
            var config = new Dictionary<string, string>
            {
                ["Environment"] = _environment.EnvironmentName,
                ["ApplicationName"] = _environment.ApplicationName,
                ["ContentRootPath"] = _environment.ContentRootPath,
                ["WebRootPath"] = _environment.WebRootPath ?? "Not Available"
            };

            return Ok(config);
        }
    }

    public class SystemInfo
    {
        public string ApiVersion { get; set; }
        public string Environment { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime ServerTimeUtc { get; set; }
        public string TimeZone { get; set; }
        public SystemResources Resources { get; set; }
        public Dictionary<string, string> RuntimeInfo { get; set; }
        public Dictionary<string, string> EnvironmentInfo { get; set; }
        public Dictionary<string, bool> Services { get; set; }
    }

    public class SystemResources
    {
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public long TotalMemory { get; set; }
        public string DotNetVersion { get; set; }
        public TimeSpan Uptime { get; set; }
    }
}