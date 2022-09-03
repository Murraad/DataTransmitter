using System.Text.Json.Serialization;

namespace DataSender
{
    public class AppSettings
    {
        public string BlobStorageConnectionString { get; set; }
        public string ServiceBusQueueConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string FolderReadPath { get; set; }
        public string ProcessPath { get; set; }
        public string QueueName { get; set; }
    }
}
