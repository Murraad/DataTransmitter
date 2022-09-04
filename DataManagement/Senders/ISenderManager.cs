namespace DataManagement.Senders
{
    public interface ISenderManager: IAsyncDisposable
    {
        Task SendMessageAsync(string fileName, byte[] body);
    }
}
