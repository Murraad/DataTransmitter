using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement;
using DataManagement.FileConverters;
using System.Text.Json;
using System.Transactions;

namespace DataSender
{
    public class Program
    {
        static ServiceBusClient client;
        static ServiceBusSender sender;
        static AFileConverter converter;
        static BlobContainerClient containerClient;
        static AppSettings appSettings;

        public static async Task Main()
        {
            try
            {
                appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText("appsettings.json"));
                Console.WriteLine("Press 'Enter' if you want to start processing");
                Console.ReadLine();

                //check if foler path exists
                PathManager.CreatePath(appSettings.FolderReadPath);

                //create clients and converter
                client = new ServiceBusClient(appSettings.ServiceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
                sender = client.CreateSender(appSettings.QueueName);
                converter = new SHA256FileConverter();
                containerClient = new BlobContainerClient(appSettings.BlobStorageConnectionString, appSettings.ContainerName);

                var directory = new DirectoryInfo(appSettings.FolderReadPath);
                var files = directory.GetFiles();
                List<Task> tasks = new List<Task>();
                foreach (var fileInfo in files)
                {
                    tasks.Add(SendMessageAsync(fileInfo));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.GetType()}\n{ex.Message}");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
                converter.Dispose();
            }
        }

        //send file bytes to blob container
        //send blob file name to queue
        private static async Task SendMessageAsync(FileInfo fileInfo)
        {
            try
            {
                using(var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    throw new Exception();
                    Console.WriteLine($"Application starts sending data: {fileInfo.Name}.");
                    byte[] body = File.ReadAllBytes(fileInfo.FullName);
                    Header header = converter.GenerateHeaderForFile(body);
                    byte[] fileBytes = converter.GetConvertedFile(converter.GetHeaderBytes<Header>(header), body);
                    string fileName = converter.GetFileName(header);
                    var res = await containerClient.UploadBlobAsync(fileName, new BinaryData(fileBytes));
                    var message = new ServiceBusMessage(fileName);
                    await sender.SendMessageAsync(message);
                    Console.WriteLine($"File {fileInfo.Name} was sent successfully.");

                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File {fileInfo.Name} was not sent.\n{ex.Message}");
            }
        }
    }
}