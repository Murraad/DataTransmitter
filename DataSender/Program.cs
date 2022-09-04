using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement;
using DataManagement.FileConverters;
using DataManagement.Logger;
using System.Text.Json;

namespace DataSender
{
    public class Program
    {
        public static async Task Main()
        {
            SenderManager sender = null;
            try
            {
                var appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appsettings.json"));
                IInformationLogger logger = new ConsoleInformationLogger();
                sender = new SenderManager(logger, appSettings.ServiceBusQueueConnectionString, appSettings.QueueName, appSettings.BlobStorageConnectionString, appSettings.ContainerName);

                logger.LogInformation("Press 'Enter' if you want to start processing");
                Console.ReadLine();

                //check if foler path exists
                PathManager.CreatePath(appSettings.FolderReadPath);

                var directory = new DirectoryInfo(appSettings.FolderReadPath);
                var files = directory.GetFiles();
                List<Task> tasks = new List<Task>();
                foreach (var fileInfo in files)
                {
                    tasks.Add(sender.SendMessageAsync(fileInfo));
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