using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageWriter(Stream stream) : MessageHandler, IDisposable
{
    public async Task WriteMessage(MessageDTO message, CancellationToken ct)
    {
        string json = JsonConvert.SerializeObject(message);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        if (payload.Length > MaxMessageLen)
            throw new TooLongMessageException($"Message is too long: {payload.Length} bytes");
        
        // 4-byte big-endian length prefix
        var header = new byte[HeaderLen];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);

        await stream.WriteAsync(header, ct);
        await stream.WriteAsync(payload, ct);
        await stream.FlushAsync(ct);
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}
