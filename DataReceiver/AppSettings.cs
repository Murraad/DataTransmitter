using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReceiver
{
    public class AppSettings
    {
        public string BlobStorageConnectionString { get; set; }
        public string ServiceBusQueueConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string FolderWritePath { get; set; }
        public string ProcessPath { get; set; }
        public string QueueName { get; set; }
    }
}
