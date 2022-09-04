namespace DataManagement.Loggers
{
    public class ConsoleInformationLogger : IInformationLogger
    {
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void LogInformation(string message) => Console.WriteLine(message);
    }
}
