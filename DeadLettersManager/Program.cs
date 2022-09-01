using Azure.Messaging.ServiceBus;
using DataManagement;
using DataManagement.FileConverters;

public static class Program
{
    static ServiceBusClient client;
    static ServiceBusProcessor processor;
    static AFileConverter converter;

    public static async Task Main()
    {
        try
        {
            client = new ServiceBusClient(Constants.ServiceBusQueueConnectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
            processor = client.CreateProcessor(Constants.QueueName, new ServiceBusProcessorOptions() { SubQueue = SubQueue.DeadLetter });
            var reciever = client.CreateReceiver(Constants.QueueName);
            reciever.
            converter = new SHA256FileConverter();

            bool exit = false;
            while(!exit)
            {

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.GetType()}\n{ex.Message}");
        }
        finally
        {
            await processor.DisposeAsync();
            await client.DisposeAsync();
            converter.Dispose();
        }
    }

    private static void MainMenu()
    {
        processor.St
        Console.WriteLine("Id\tName\tReasons");
        Console.WriteLine()
    }

}
