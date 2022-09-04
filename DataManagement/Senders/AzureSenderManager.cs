using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement.FileConverters;
using DataManagement.Loggers;

namespace DataManagement.Senders
{
    public class AzureSenderManager: ISenderManager
    {
        readonly ServiceBusClient client;
        readonly ServiceBusSender sender;
        readonly AFileConverter converter;
        readonly BlobContainerClient containerClient;
        readonly IInformationLogger logger;

        public AzureSenderManager(IInformationLogger logger, string serviceBusQueueConnectionString, string queueName, string blobStorageConnectionString, string containerName)
        {
            this.client = new ServiceBusClient(serviceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            this.sender = client.CreateSender(queueName);
            this.converter = new SHA256FileConverter();
            this.containerClient = new BlobContainerClient(blobStorageConnectionString, containerName);
            this.logger = logger;
        }

        //send file bytes to blob container
        //send blob file name to queue
        public async Task SendMessageAsync(string fileName, byte[] body)
        {
            try
            {
                this.logger.LogInformation($"Application starts sending data: {fileName}.");
                Header header = this.converter.GenerateHeaderForFile(body);
                byte[] sentFileBytes = this.converter.GetConvertedFile(this.converter.GetHeaderBytes<Header>(header), body);
                string sentFileName = this.converter.GetFileName(header);
                var content = await this.containerClient.UploadBlobAsync(sentFileName, new BinaryData(sentFileBytes));
                if (content != null && content.GetRawResponse().Status == 201)
                {
                    var message = new ServiceBusMessage(sentFileName);
                    await this.sender.SendMessageAsync(message);
                    this.logger.LogInformation($"File {fileName} was sent successfully.");
                }
                else
                {
                    this.logger.LogInformation($"File {fileName} was not uploaded and was not sent.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"File {fileName} was not sent.\n{ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.sender.DisposeAsync();
            await this.client.DisposeAsync();
            this.converter.Dispose();
        }
    }
}
