using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageWriter(Stream stream) : MessageHandler, IDisposable
{
    private Stream stream = stream;


    public async Task WriteMessage(MessageDTO message, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}
