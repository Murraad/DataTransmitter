using Azure.Messaging.ServiceBus;
using DataManagement;
using DataManagement.Loggers;
using DataManagement.Recievers;
using System.Text.Json;

namespace DataReceiver
{
    public class Program
    {
        static async Task Main()
        {
            IInformationLogger logger = new ConsoleInformationLogger();
            IReceiverManager receiver = null;
            try
            {
                var appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appsettings.json"));
                receiver = new AzureReceiverManager(logger, appSettings.ServiceBusQueueConnectionString, appSettings.BlobStorageConnectionString,
                    appSettings.ContainerName, appSettings.QueueName, appSettings.ProcessPath, appSettings.FolderWritePath);

                //check if foler path exists
                PathManager.CreatePath(appSettings.FolderWritePath);

                logger.LogInformation("Press 'Enter' if you want to stop application.");

                // register handlers
                receiver.RegisterHandlers();

                // start processing 
                await receiver.StartProcessingAsync();

                Console.ReadLine();

                // stop processing 
                logger.LogInformation("\nStopping the receivers...");
                await receiver.StopProcessingAsync();
                logger.LogInformation("Stopped receiving messages");
            }
            catch (Exception ex)
            {
                logger.LogError($"Unhandled exception: {ex.GetType()}\n{ex.Message}");
            }
            finally
            {
                if(receiver is not null)
                {
                    await receiver.DisposeAsync();
                }
            }
        }
    }
}