namespace DataManagement
{
    /// <summary>
    /// Constants helper
    /// </summary>
    public class Constants
    {

        public static string ServiceBusQueueConnectionString = "Endpoint=sb://testgar.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=d9F+Xe/Rr2cTAfYigO8PSN5TlNVJ+xUYzJGMq0pl7rI=";
        public static string BlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=blobtestgar;AccountKey=xwcG5oXgJxelnvKtvrtWM4wJY8gLAE4NHWzQfsBc+46+7i8SEXOU2Ojlk4I0UEjHTdvHG0ZRnlwL+ASt5ymvgA==;EndpointSuffix=core.windows.net";
        public static string QueueName = "test";
        public static string ContainerName = "testqueuecontainer";
        public static string FolderReadPath = @"C:\test\read";
        public static string FolderWritePath = @"C:\test\write";
        public static string ProcessPath = @"https://localhost:5001/Content/process";

        public static class Message
        {
            public static string DeadLetterInvalidBlobReason = "InvalidBlob";
            public static string DeadLetterInvalidChecksumReason = "InvalidBlob";
        }
    }
}
