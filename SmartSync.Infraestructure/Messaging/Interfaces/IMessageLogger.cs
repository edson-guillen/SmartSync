namespace SmartSync.Infraestructure.Logging.Interfaces
{
    public interface IMessageLogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
    }
}