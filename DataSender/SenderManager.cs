using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement;
using DataManagement.FileConverters;
using DataManagement.Logger;

namespace DataSender
{
    public class SenderManager: IAsyncDisposable
    {
        ServiceBusClient client;
        ServiceBusSender sender;
        AFileConverter converter;
        BlobContainerClient containerClient;
        IInformationLogger logger;

        public SenderManager(IInformationLogger logger, string serviceBusQueueConnectionString, string queueName, string blobStorageConnectionString, string containerName)
        {
            this.client = new ServiceBusClient(serviceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            this.sender = client.CreateSender(queueName);
            this.converter = new SHA256FileConverter();
            this.containerClient = new BlobContainerClient(blobStorageConnectionString, containerName);
            this.logger = logger;
        }

        //send file bytes to blob container
        //send blob file name to queue
        public async Task SendMessageAsync(FileInfo fileInfo)
        {
            try
            {
                this.logger.LogInformation($"Application starts sending data: {fileInfo.Name}.");
                byte[] body = File.ReadAllBytes(fileInfo.FullName);
                Header header = converter.GenerateHeaderForFile(body);
                byte[] fileBytes = converter.GetConvertedFile(converter.GetHeaderBytes<Header>(header), body);
                string fileName = converter.GetFileName(header);
                var content = await containerClient.UploadBlobAsync(fileName, new BinaryData(fileBytes));
                if (content != null && content.GetRawResponse().Status == 201)
                {
                    var message = new ServiceBusMessage(fileName);
                    await sender.SendMessageAsync(message);
                    this.logger.LogInformation($"File {fileInfo.Name} was sent successfully.");
                }
                else
                {
                    this.logger.LogInformation($"File {fileInfo.Name} was not uploaded and was not sent.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"File {fileInfo.Name} was not sent.\n{ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
            converter.Dispose();
        }
    }
}
