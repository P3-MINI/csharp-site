namespace SnakeArena.Shared.Protocol;

/// <summary>
/// Splits a byte stream (e.g. a <see cref="System.Net.Sockets.NetworkStream"/>) into frames:
/// a 4-byte little-endian length followed by that many bytes of body.
/// </summary>
public static class Framing
{
    /// <summary>Writes one frame containing <paramref name="body"/>.</summary>
    /// <exception cref="ArgumentException">The body is empty or longer than <see cref="ProtocolConstants.MaxFrameLength"/>.</exception>
    public static ValueTask WriteFrameAsync(Stream stream, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads one complete frame and returns its body, or <see langword="null"/> if the stream ended cleanly
    /// before the first byte of a frame.
    /// </summary>
    /// <exception cref="EndOfStreamException">The stream ended in the middle of a frame.</exception>
    /// <exception cref="ProtocolException">The length prefix is 0 or greater than <see cref="ProtocolConstants.MaxFrameLength"/>.</exception>
    public static ValueTask<byte[]?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // TODO (Part 1)
        throw new NotImplementedException();
    }
}
