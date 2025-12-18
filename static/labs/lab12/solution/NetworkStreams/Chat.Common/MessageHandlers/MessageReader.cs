using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class MessageReader(Stream stream) : MessageHandler, IDisposable
{
    public async Task<MessageDTO?> ReadMessage(CancellationToken ct)
    {
        var headerBuffer = new byte[HeaderLen];

        int bytesRead = await ReadToBuffer(headerBuffer, ct);
        if (bytesRead < HeaderLen) // Disconnection from the server
            return null;

        int payloadLen = BinaryPrimitives.ReadInt32BigEndian(headerBuffer);
        if (payloadLen > MaxMessageLen)
            throw new TooLongMessageException($"Message is too long: {payloadLen} bytes");

        var payloadBuffer = new byte[payloadLen];

        bytesRead = await ReadToBuffer(payloadBuffer, ct);
        if (bytesRead < payloadLen) // Disconnection from the server
            return null;

        string json = Encoding.UTF8.GetString(payloadBuffer);

        var message = JsonConvert.DeserializeObject<MessageDTO>(json);
        if (message == null)
            throw new InvalidMessageException("Payload cannot be deserialized");

        return message;
    }


    private async Task<int> ReadToBuffer(byte[] buffer, CancellationToken ct)
    {
        int bytesRead = 0; int chunkSize = 1;

        while (bytesRead < buffer.Length && chunkSize > 0)
        {
            chunkSize = await stream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, ct);
            bytesRead += chunkSize;
        }

        return bytesRead;
    }


    public void Dispose()
    {
        stream.Dispose();
    }
}