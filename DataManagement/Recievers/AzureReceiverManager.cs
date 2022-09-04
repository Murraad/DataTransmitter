using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement.FileConverters;
using DataManagement.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Recievers
{
    public class AzureReceiverManager: IReceiverManager
    {
        readonly ServiceBusClient client;
        readonly ServiceBusProcessor processor;
        readonly ServiceBusProcessor deadMessagesProcessor;
        readonly ServiceBusSender sender;
        readonly AFileConverter converter;
        readonly HttpClient httpClient;
        readonly BlobContainerClient containerClient;
        readonly IInformationLogger logger;
        readonly string processPath;
        readonly string folderWritePath;

        public AzureReceiverManager(IInformationLogger logger, string serviceBusQueueConnectionString, string blobStorageConnectionString,
            string containerName, string queueName, string processPath, string folderWritePath)
        {
            this.converter = new SHA256FileConverter();
            this.client = new ServiceBusClient(serviceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            this.httpClient = new HttpClient();
            this.containerClient = new BlobContainerClient(blobStorageConnectionString, containerName);

            // create a processor that we can use to process the messages and set options
            // AutoCompleteMessages - false to prevent complete messages when we don't need it
            this.processor = this.client.CreateProcessor(queueName, new ServiceBusProcessorOptions() { AutoCompleteMessages = false });

            //create processor for dead messages
            this.deadMessagesProcessor = this.client.CreateProcessor(queueName, new ServiceBusProcessorOptions() { AutoCompleteMessages = false, SubQueue = SubQueue.DeadLetter });

            this.sender = this.client.CreateSender(queueName);

            this.logger = logger;
            this.processPath = processPath;
            this.folderWritePath = folderWritePath;
        }

        public void RegisterHandlers()
        {
            this.processor.ProcessMessageAsync += MessageHandler;
            this.deadMessagesProcessor.ProcessMessageAsync += DeadMessageHandler;

            this.processor.ProcessErrorAsync += ErrorHandler;
            this.deadMessagesProcessor.ProcessErrorAsync += ErrorHandler;
        }

        public async Task StartProcessingAsync()
        {
            await this.processor.StartProcessingAsync();
            await this.deadMessagesProcessor.StartProcessingAsync();
        }

        public async Task StopProcessingAsync()
        {
            await processor.StopProcessingAsync();
            await deadMessagesProcessor.StopProcessingAsync();
        }


        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var fileName = args.Message.Body.ToString();
            var blobClient = containerClient.GetBlobClient(fileName);
            if (blobClient.Exists())
            {
                this.logger.LogInformation($"Receiving: {fileName}");
                var content = blobClient.DownloadContent();
                if (content != null && content.GetRawResponse().Status == 200)
                {
                    var fileBytes = content.Value.Content.ToArray();
                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new ByteArrayContent(fileBytes), "file", fileName);
                        var postResult = await httpClient.PostAsync(this.processPath, form);
                        if (postResult.IsSuccessStatusCode)
                        {
                            string filePath = $"{this.folderWritePath}\\{fileName}";
                            File.WriteAllBytes(filePath, fileBytes);
                            this.logger.LogInformation($"{filePath} was saved.");
                            await args.CompleteMessageAsync(args.Message);
                            await blobClient.DeleteIfExistsAsync();
                        }
                        else
                        {
                            try
                            {
                                var header = converter.DeserializeHeaderFromFile(fileBytes);
                                var body = converter.GetBodyArrayFromFile(fileBytes);
                                if (converter.AreChecksumsEqual(header.Checksum, converter.GetChecksum(body)))
                                {
                                    this.logger.LogInformation($"Internal error. File {fileName} will be written later.");
                                    return;
                                }
                            }
                            catch (FormatException) { }
                            this.logger.LogError($"File is invalid. {fileName} will be deleted.");
                            await args.DeadLetterMessageAsync(args.Message, Constants.Message.DeadLetterInvalidChecksumReason);
                            await blobClient.DeleteIfExistsAsync();
                        }
                    }
                }
            }
            else
            {
                this.logger.LogError($"Blob Client doesn't exist. {fileName} will be deleted.");
                await args.DeadLetterMessageAsync(args.Message, Constants.Message.DeadLetterInvalidBlobReason);
            }
        }

        //Resend message if checksum is valid
        async Task DeadMessageHandler(ProcessMessageEventArgs args)
        {
            if (args.Message.DeadLetterReason == "MaxDeliveryCountExceeded")
            {
                var message = new ServiceBusMessage(args.Message.Body);
                await sender.SendMessageAsync(message);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            //if process server doesn't respond, we need start it
            if (args.Exception is HttpRequestException)
            {
                this.logger.LogError("Process server doesn't respond.\n Please check it, application will wait for a 30 seconds");
                Thread.Sleep(30000);
                return Task.CompletedTask;
            }
            this.logger.LogError($"ERROR:\n{args.Exception}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await this.processor.DisposeAsync();
            await this.deadMessagesProcessor.DisposeAsync();
            await this.sender.DisposeAsync();
            await this.client.DisposeAsync();
            this.converter.Dispose();
            this.httpClient.Dispose();
        }
    }
}
