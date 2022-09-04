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
            IInformationLogger logger = new ConsoleInformationLogger();
            ISenderManager sender = null;
            try
            {
                var appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appsettings.json"));
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
                logger.LogError($"Exception: {ex.GetType()}\n{ex.Message}");
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