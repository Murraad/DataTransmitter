using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using DataManagement;
using DataManagement.FileConverters;

public class Program
{
    static ServiceBusClient client;
    static ServiceBusProcessor processor;
    static ServiceBusProcessor deadMessagesProcessor;
    static ServiceBusSender sender;
    static AFileConverter converter;
    static HttpClient httpClient;
    static BlobContainerClient containerClient;

    static async Task Main()
    {
        //check if foler path exists
        PathManager.CreateWriteFolderPath();

        converter = new SHA256FileConverter();
        client = new ServiceBusClient(Constants.ServiceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
        httpClient = new HttpClient();
        containerClient = new BlobContainerClient(Constants.BlobStorageConnectionString, Constants.ContainerName);

        // create a processor that we can use to process the messages and set options
        // AutoCompleteMessages - false to prevent complete messages when we don't need it
        processor = client.CreateProcessor(Constants.QueueName, new ServiceBusProcessorOptions() { AutoCompleteMessages = false });

        //create processor for dead messages
        deadMessagesProcessor = client.CreateProcessor(Constants.QueueName, new ServiceBusProcessorOptions() { AutoCompleteMessages = false , SubQueue = SubQueue.DeadLetter});

        sender = client.CreateSender(Constants.QueueName);

        try
        {
            Console.WriteLine("Press 'Enter' if you want to stop application.");

            // add handlers to process messages
            processor.ProcessMessageAsync += MessageHandler;
            deadMessagesProcessor.ProcessMessageAsync += DeadMessageHandler;

            // add handlers to process any errors
            processor.ProcessErrorAsync += ErrorHandler;
            deadMessagesProcessor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();
            await deadMessagesProcessor.StartProcessingAsync();

            Console.ReadLine();

            // stop processing 
            Console.WriteLine("\nStopping the receivers...");
            await processor.StopProcessingAsync();
            await deadMessagesProcessor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
        finally
        {
            await processor.DisposeAsync();
            await deadMessagesProcessor.DisposeAsync();
            await sender.DisposeAsync();
            await client.DisposeAsync();
            converter.Dispose();
            httpClient.Dispose();
        }
    }

    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var fileName = args.Message.Body.ToString();
        var blobClient = containerClient.GetBlobClient(fileName);
        if(blobClient.Exists())
        {
            Console.WriteLine($"Receiving: {fileName}");
            var content = blobClient.DownloadContent();
            if(content != null && content.GetRawResponse().Status == 200)
            {
                var fileBytes = content.Value.Content.ToArray();
                using (var form = new MultipartFormDataContent())
                {
                    form.Add(new ByteArrayContent(fileBytes), "file", fileName);
                    var postResult = await httpClient.PostAsync(Constants.ProcessPath, form);
                    if (postResult.IsSuccessStatusCode)
                    {
                        string filePath = $"{Constants.FolderWritePath}\\{fileName}";
                        File.WriteAllBytes(filePath, fileBytes);
                        Console.WriteLine($"{filePath} was saved.");
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
                                Console.WriteLine($"Internal error. File {fileName} will be written later.");
                                return;
                            }
                        }
                        catch(FormatException) { }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File is invalid. {fileName} will be deleted.");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        await args.DeadLetterMessageAsync(args.Message, Constants.Message.DeadLetterInvalidChecksumReason);
                        await blobClient.DeleteIfExistsAsync();
                    }
                }
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Blob Client doesn't exist. {fileName} will be deleted.");
            Console.ForegroundColor = ConsoleColor.Gray;
            await args.DeadLetterMessageAsync(args.Message, Constants.Message.DeadLetterInvalidBlobReason);
        }
    }

    //Resend message if checksum is valid
    static async Task DeadMessageHandler(ProcessMessageEventArgs args)
    {
        if (args.Message.DeadLetterReason == "MaxDeliveryCountExceeded")
        {
            var message = new ServiceBusMessage(args.Message.Body);
            await sender.SendMessageAsync(message);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        //if process server doesn't respond, we need start it and start receiver again
        if (args.Exception is HttpRequestException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Process server doesn't respond. Please check it and start application again.\n" +
                "Please press Enter to close application.");
            Console.ForegroundColor = ConsoleColor.Gray;
            processor.StopProcessingAsync();
            deadMessagesProcessor.StopProcessingAsync();
            return Task.CompletedTask;
        }
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}