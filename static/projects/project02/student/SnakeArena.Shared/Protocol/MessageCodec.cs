namespace SnakeArena.Shared.Protocol;

/// <summary>
/// Converts messages to frame bodies and back. A frame body is the message type byte followed by the payload,
/// exactly as described in the specification (section "Protocol").
/// </summary>
public static class MessageCodec
{
    /// <summary>Encodes <paramref name="message"/> into a frame body.</summary>
    /// <exception cref="ArgumentException">The message cannot be represented (e.g. a string is too long).</exception>
    public static byte[] Encode(Message message)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }

    /// <summary>Decodes a complete frame body.</summary>
    /// <exception cref="ProtocolException">The body is malformed. No other exception type may escape.</exception>
    public static Message Decode(ReadOnlySpan<byte> body)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }
}
