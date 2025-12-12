using System.Buffers.Binary;
using System.Text;
using Newtonsoft.Json;


namespace Chat.Common.MessageHandlers;


public class InvalidMessageReceived(string message) : Exception(message) {}


public class MessageReader(Stream stream) : MessageHandler, IDisposable
{
    public async Task<MessageDTO?> ReadMessage(CancellationToken ct)
    {
        var headerBuffer = new byte[HeaderLen];

        int bytesRead = await ReadToBuffer(headerBuffer, ct);
        if (bytesRead == 0) // Disconnection from the server
            return null;

        if (bytesRead < HeaderLen)
            throw new InvalidMessageReceived($"Invalid header len: {bytesRead}");

        Int32 payloadLen = BinaryPrimitives.ReadInt32BigEndian(headerBuffer);

        var payloadBuffer = new byte[payloadLen];

        bytesRead = await ReadToBuffer(payloadBuffer, ct);
        if (bytesRead < payloadLen)
            throw new InvalidMessageReceived("Invalid payload len");

        string json = Encoding.UTF8.GetString(payloadBuffer);

        var message = JsonConvert.DeserializeObject<MessageDTO>(json);
        if (message == null)
            throw new InvalidMessageReceived("Payload cannot be deserialized");

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