using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageReader(Stream stream) : MessageHandler, IDisposable
{
    public async Task<MessageDTO?> ReadMessage(CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}