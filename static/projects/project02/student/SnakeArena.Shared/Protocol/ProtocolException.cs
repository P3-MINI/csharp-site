namespace SnakeArena.Shared.Protocol;

/// <summary>
/// Thrown when received data does not conform to the protocol (malformed frame, unknown message type,
/// truncated payload, invalid field value, ...).
/// </summary>
public sealed class ProtocolException : Exception
{
    public ProtocolException(string message) : base(message)
    {
    }

    public ProtocolException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
