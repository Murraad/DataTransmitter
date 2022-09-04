using DataManagement;
using DataManagement.Loggers;
using DataManagement.Senders;
using System.Text.Json;

namespace DataSender
{
    public class Program
    {
        public static async Task Main()
        {
            ISenderManager sender = null;
            try
            {
                var appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appsettings.json"));
                IInformationLogger logger = new ConsoleInformationLogger();
                sender = new AzureSenderManager(logger, appSettings.ServiceBusQueueConnectionString, appSettings.QueueName, appSettings.BlobStorageConnectionString, appSettings.ContainerName);

                logger.LogInformation("Press 'Enter' if you want to start processing");
                Console.ReadLine();

                //check if foler path exists
                PathManager.CreatePath(appSettings.FolderReadPath);

                var directory = new DirectoryInfo(appSettings.FolderReadPath);
                var files = directory.GetFiles();
                List<Task> tasks = new List<Task>();
                foreach (var fileInfo in files)
                {
                    tasks.Add(sender.SendMessageAsync(fileInfo.Name, await File.ReadAllBytesAsync(fileInfo.FullName)));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Exception: {ex.GetType()}\n{ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            finally
            {
                if(sender is not null)
                {
                    await sender.DisposeAsync();
                }
            }
        }
    }
}