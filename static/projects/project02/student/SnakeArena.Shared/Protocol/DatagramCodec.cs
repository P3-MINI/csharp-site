using System.Diagnostics.CodeAnalysis;

namespace SnakeArena.Shared.Protocol;

/// <summary>
/// Converts UDP datagrams to bytes and back (Part 4).
/// </summary>
public static class DatagramCodec
{
    /// <summary>"SNAK" in ASCII: the first four bytes of every datagram.</summary>
    public static ReadOnlySpan<byte> Magic => "SNAK"u8;

    public static byte[] Encode(Datagram datagram)
    {
        // TODO (Part 4)
        throw new NotImplementedException();
    }

    /// <summary>
    /// Decodes a datagram. Returns <see langword="false"/> (and never throws) if the data is not a valid datagram:
    /// anyone can send anything to a UDP port.
    /// </summary>
    public static bool TryDecode(ReadOnlySpan<byte> data, [NotNullWhen(true)] out Datagram? datagram)
    {
        // TODO (Part 4)
        throw new NotImplementedException();
    }
}
