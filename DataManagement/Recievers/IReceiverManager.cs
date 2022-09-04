using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Recievers
{
    public interface IReceiverManager: IAsyncDisposable
    {
        void RegisterHandlers();
        Task StartProcessingAsync();
        Task StopProcessingAsync();
    }
}
