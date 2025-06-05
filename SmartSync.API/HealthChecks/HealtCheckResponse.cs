namespace SmartSync.API.HealthChecks
{
    public class HealthCheckResponse
    {
        public string Status { get; set; }
        public string Version { get; set; }
        public Dictionary<string, object> Checks { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}