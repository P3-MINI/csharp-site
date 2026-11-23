using System.Buffers.Binary;
using System.Text;

namespace MiniKV.Protocol;

/// <summary>
/// What a server tells about itself in reply to a discovery probe.
/// </summary>
public sealed record ServerAnnouncement(string Name, int TcpPort, int KeyCount);

/// <summary>
/// Binary format of the UDP discovery datagrams (numbers are big-endian):
/// <code>
/// probe:        "MKV?"
/// announcement: "MKV!" | TCP port (uint16) | key count (int32) | name length (uint8) | name (UTF-8)
///  offset:       0        4                  6                   10                    11
/// </code>
/// </summary>
public static class Discovery
{
    private const int HeaderSize = 11; // everything before the name

    private static ReadOnlySpan<byte> ProbeMagic => "MKV?"u8;
    private static ReadOnlySpan<byte> AnnouncementMagic => "MKV!"u8;

    public static byte[] CreateProbe() => ProbeMagic.ToArray();

    public static bool IsProbe(ReadOnlySpan<byte> datagram) => datagram.SequenceEqual(ProbeMagic);

    public static byte[] Encode(ServerAnnouncement announcement)
    {
        byte[] name = Encoding.UTF8.GetBytes(announcement.Name);
        if (name.Length > byte.MaxValue)
            throw new ArgumentException("Server name is too long", nameof(announcement));

        byte[] datagram = new byte[HeaderSize + name.Length];
        Span<byte> span = datagram;
        AnnouncementMagic.CopyTo(span);
        BinaryPrimitives.WriteUInt16BigEndian(span[4..], (ushort)announcement.TcpPort);
        BinaryPrimitives.WriteInt32BigEndian(span[6..], announcement.KeyCount);
        span[10] = (byte)name.Length;
        name.CopyTo(span[HeaderSize..]);
        return datagram;
    }

    /// <summary>
    /// Returns null if the datagram is not a valid announcement.
    /// </summary>
    public static ServerAnnouncement? TryDecode(ReadOnlySpan<byte> datagram)
    {
        if (datagram.Length < HeaderSize || !datagram[..4].SequenceEqual(AnnouncementMagic))
            return null;

        int nameLength = datagram[10];
        if (datagram.Length != HeaderSize + nameLength)
            return null;

        ushort tcpPort = BinaryPrimitives.ReadUInt16BigEndian(datagram[4..]);
        int keyCount = BinaryPrimitives.ReadInt32BigEndian(datagram[6..]);
        string name = Encoding.UTF8.GetString(datagram[HeaderSize..]);
        return new ServerAnnouncement(name, tcpPort, keyCount);
    }
}
