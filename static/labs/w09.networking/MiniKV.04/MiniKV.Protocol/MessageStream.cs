using System.Buffers.Binary;
using System.Text.Json;

namespace MiniKV.Protocol;

/// <summary>
/// Sends and receives whole messages over a byte stream (e.g. a NetworkStream).
/// Every message is sent as a frame:
/// [payload length: 4 bytes, big-endian int32][payload: UTF-8 JSON]
/// </summary>
public sealed class MessageStream(Stream stream)
{
    private const int HeaderSize = sizeof(int);

    public async Task WriteAsync(Message message, CancellationToken ct = default)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(message, KvProtocol.JsonOptions);
        if (payload.Length > KvProtocol.MaxMessageSize)
            throw new InvalidDataException($"Message too large: {payload.Length} bytes");

        // Header + payload in one buffer, so the frame is written with a single call
        byte[] frame = new byte[HeaderSize + payload.Length];
        BinaryPrimitives.WriteInt32BigEndian(frame, payload.Length);
        payload.CopyTo(frame, HeaderSize);

        await stream.WriteAsync(frame, ct);
        await stream.FlushAsync(ct);
    }

    /// <summary>
    /// Reads the next message. Returns null if the other side closed the connection
    /// cleanly (between two messages).
    /// </summary>
    public async Task<Message?> ReadAsync(CancellationToken ct = default)
    {
        byte[] header = new byte[HeaderSize];
        int read = await stream.ReadAtLeastAsync(header, HeaderSize, throwOnEndOfStream: false, ct);
        if (read == 0)
            return null;
        if (read < HeaderSize)
            throw new EndOfStreamException("Connection closed in the middle of a message");

        int length = BinaryPrimitives.ReadInt32BigEndian(header);
        if (length <= 0 || length > KvProtocol.MaxMessageSize)
            throw new InvalidDataException($"Invalid message length: {length}");

        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload, ct); // throws EndOfStreamException if the stream ends early

        try
        {
            return JsonSerializer.Deserialize<Message>(payload, KvProtocol.JsonOptions)
                   ?? throw new InvalidDataException("Message is null");
        }
        catch (Exception e) when (e is JsonException or NotSupportedException)
        {
            throw new InvalidDataException($"Malformed message: {e.Message}", e);
        }
    }
}
