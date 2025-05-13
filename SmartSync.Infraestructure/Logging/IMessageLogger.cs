namespace SmartSync.Infraestructure.Logging
{
    public interface IMessageLogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
    }
}
