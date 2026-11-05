using SnakeArena.Shared.Game;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

public class DatagramCodecTests
{
    private static readonly StateSnapshot Snapshot = new(
        42,
        [new PlayerSnapshot(new PlayerId(1), "A", true, Direction.Right, 2, [new Position(2, 1), new Position(1, 1)])],
        [new Position(5, 3)]);

    public static TheoryData<Datagram> AllDatagrams => new()
    {
        new DiscoveryProbe(ProtocolConstants.Version, 0x12345678),
        new DiscoveryReply(ProtocolConstants.Version, 7, "Lab 204", 47000, 3, 8),
        new DiscoveryReply(2, uint.MaxValue, "Serwer Łukasza", 1, 0, 255),
        new UdpHello(new PlayerId(5), 0xAABBCCDD),
        new SnapshotDatagram(0xCAFEBABE, Snapshot),
        new SnapshotDatagram(1, new StateSnapshot(0, [], [])),
    };

    [Theory]
    [MemberData(nameof(AllDatagrams))]
    public void RoundTrip_ReturnsEqualDatagram(Datagram datagram)
    {
        byte[] data = DatagramCodec.Encode(datagram);

        Assert.True(DatagramCodec.TryDecode(data, out var decoded));
        Assert.Equal(datagram, decoded);
    }

    [Fact]
    public void Encode_DiscoveryProbe_ExactBytes()
    {
        byte[] expected = [0x53, 0x4E, 0x41, 0x4B, 0x01, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12];

        Assert.Equal(expected, DatagramCodec.Encode(new DiscoveryProbe(1, 0x12345678)));
    }

    [Fact]
    public void Encode_DiscoveryReply_ExactBytes()
    {
        byte[] expected =
        [
            0x53, 0x4E, 0x41, 0x4B, 0x02, // magic, kind
            0x01, 0x00, 0x78, 0x56, 0x34, 0x12, // version, nonce
            0x03, 0x00, 0x4C, 0x61, 0x62, // name "Lab"
            0x98, 0xB7, 0x02, 0x08, // TCP port 47000, 2 of 8 players
        ];

        Assert.Equal(expected, DatagramCodec.Encode(new DiscoveryReply(1, 0x12345678, "Lab", 47000, 2, 8)));
    }

    [Fact]
    public void Encode_UdpHello_ExactBytes()
    {
        byte[] expected = [0x53, 0x4E, 0x41, 0x4B, 0x03, 0x05, 0xDD, 0xCC, 0xBB, 0xAA];

        Assert.Equal(expected, DatagramCodec.Encode(new UdpHello(new PlayerId(5), 0xAABBCCDD)));
    }

    [Fact]
    public void Encode_Snapshot_IsHeaderTokenAndMessageBody()
    {
        byte[] expected = [0x53, 0x4E, 0x41, 0x4B, 0x04, 0xBE, 0xBA, 0xFE, 0xCA, .. MessageCodec.Encode(Snapshot)];

        Assert.Equal(expected, DatagramCodec.Encode(new SnapshotDatagram(0xCAFEBABE, Snapshot)));
    }

    [Theory]
    [MemberData(nameof(AllDatagrams))]
    public void TryDecode_Truncated_ReturnsFalse(Datagram datagram)
    {
        byte[] data = DatagramCodec.Encode(datagram);

        for (int length = 0; length < data.Length; length++)
            Assert.False(DatagramCodec.TryDecode(data.AsSpan(0, length), out _));
    }

    [Theory]
    [MemberData(nameof(AllDatagrams))]
    public void TryDecode_TrailingBytes_ReturnsFalse(Datagram datagram)
    {
        byte[] data = [.. DatagramCodec.Encode(datagram), 0x00];

        Assert.False(DatagramCodec.TryDecode(data, out _));
    }

    [Theory]
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4C, 0x01, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12 })] // wrong magic
    [InlineData(new byte[] { 0x73, 0x6E, 0x61, 0x6B, 0x01, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12 })] // lower-case magic
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4B, 0x00, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12 })] // kind 0
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4B, 0x05, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12 })] // kind 5
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4B, 0x02, 0x01, 0x00, 0x78, 0x56, 0x34, 0x12, 0x01, 0x00, 0xFF, 0x98, 0xB7, 0x02, 0x08 })] // invalid UTF-8
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4B, 0x04, 0x01, 0x00, 0x00, 0x00, 0x84, 0x01, 0x00, 0x2A, 0x02, 0x00, 0x68, 0x69 })] // not a snapshot
    [InlineData(new byte[] { 0x53, 0x4E, 0x41, 0x4B, 0x04, 0x01, 0x00, 0x00, 0x00, 0x83, 0x01, 0x00 })] // malformed snapshot
    [InlineData(new byte[] { 0x47, 0x45, 0x54, 0x20, 0x2F, 0x20, 0x48, 0x54, 0x54, 0x50 })] // "GET / HTTP"
    public void TryDecode_Garbage_ReturnsFalse(byte[] data)
    {
        Assert.False(DatagramCodec.TryDecode(data, out var datagram));
        Assert.Null(datagram);
    }

    [Fact]
    public void TryDecode_RandomData_NeverThrows()
    {
        var random = new Random(3);
        byte[] valid = DatagramCodec.Encode(new SnapshotDatagram(1, Snapshot));
        for (int i = 0; i < 4000; i++)
        {
            byte[] data;
            if (i % 2 == 0)
            {
                // A valid header followed by random bytes.
                data = new byte[random.Next(5, 64)];
                random.NextBytes(data);
                DatagramCodec.Magic.CopyTo(data);
                data[4] = (byte)random.Next(1, 5);
            }
            else
            {
                // A valid snapshot datagram with one corrupted byte.
                data = [.. valid];
                data[random.Next(5, data.Length)] = (byte)random.Next(256);
            }

            DatagramCodec.TryDecode(data, out _); // must not throw
        }
    }
}
