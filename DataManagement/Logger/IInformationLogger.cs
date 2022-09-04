namespace DataManagement.Logger
{
    public interface IInformationLogger
    {
        public void LogInformation(string message);
        public void LogError(string message);
    }
}
