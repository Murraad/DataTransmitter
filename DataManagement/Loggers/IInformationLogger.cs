namespace DataManagement.Loggers
{
    public interface IInformationLogger
    {
        public void LogInformation(string message);
        public void LogError(string message);
    }
}
