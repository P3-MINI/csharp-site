using SnakeArena.Shared.Game;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

public class MessageCodecTests
{
    private static readonly StateSnapshot SmallSnapshot = new(
        7,
        [new PlayerSnapshot(new PlayerId(1), "A", true, Direction.Right, 2, [new Position(2, 1), new Position(1, 1)])],
        [new Position(5, 3)]);

    public static TheoryData<Message> AllMessages => new()
    {
        new JoinRequest(ProtocolConstants.Version, "Alice"),
        new JoinRequest(513, "Zażółć_gęślą"),
        new PlayerInput(Direction.Up),
        new PlayerInput(Direction.Right),
        new PlayerInput(Direction.Down),
        new PlayerInput(Direction.Left),
        new ChatSend("Hello, world!"),
        new ChatSend(""),
        new ChatSend("Pchnąć w tę łódź jeża 🐍"),
        new LeaveRequest(),
        new JoinAccepted(new PlayerId(3), 30, 16, 100, 0xDEADBEEF),
        new JoinRejected(RejectReason.VersionMismatch, "Expected version 1."),
        new JoinRejected(RejectReason.ServerFull, ""),
        new JoinRejected(RejectReason.InvalidName, "Invalid name."),
        new JoinRejected(RejectReason.NameTaken, "Name 'Alice' is already taken."),
        new StateSnapshot(0, [], []),
        SmallSnapshot,
        new StateSnapshot(
            uint.MaxValue,
            [
                new PlayerSnapshot(new PlayerId(1), "Alice", true, Direction.Up, 0, [new Position(0, 0)]),
                new PlayerSnapshot(new PlayerId(200), "Łukasz", false, Direction.Left, ushort.MaxValue, []),
                new PlayerSnapshot(new PlayerId(255), "Bob", true, Direction.Down, 12,
                    [new Position(99, 49), new Position(99, 48), new Position(98, 48)]),
            ],
            [new Position(0, 49), new Position(99, 0), new Position(255, 255)]),
        new ChatBroadcast(ProtocolConstants.ServerSender, "Alice joined the game."),
        new ChatBroadcast("Bob", "gg"),
        new ServerGoodbye("Server is shutting down."),
    };

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void RoundTrip_ReturnsEqualMessage(Message message)
    {
        byte[] body = MessageCodec.Encode(message);

        Assert.Equal(message, MessageCodec.Decode(body));
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void Encode_FirstByteIsMessageType(Message message)
    {
        byte[] body = MessageCodec.Encode(message);

        Assert.Equal(message.GetType().Name, ((MessageType)body[0]).ToString());
    }

    [Fact]
    public void RoundTrip_LargeSnapshot()
    {
        var random = new Random(1);
        var players = Enumerable.Range(1, 16)
            .Select(i => new PlayerSnapshot(
                new PlayerId((byte)i),
                $"player{i}",
                i % 3 != 0,
                (Direction)(i % 4),
                (ushort)(i * 100),
                i % 3 != 0
                    ? Enumerable.Range(0, 300).Select(_ => new Position(random.Next(100), random.Next(50))).ToList()
                    : []))
            .ToList();
        var food = Enumerable.Range(0, 500).Select(_ => new Position(random.Next(100), random.Next(50))).ToList();
        var snapshot = new StateSnapshot(123456, players, food);

        Assert.Equal(snapshot, MessageCodec.Decode(MessageCodec.Encode(snapshot)));
    }

    public static TheoryData<Message, byte[]> GoldenVectors => new()
    {
        { new PlayerInput(Direction.Left), [0x02, 0x03] },
        { new LeaveRequest(), [0x04] },
        { new JoinRequest(1, "Ala"), [0x01, 0x01, 0x00, 0x03, 0x00, 0x41, 0x6C, 0x61] },
        { new ChatSend("żó"), [0x03, 0x04, 0x00, 0xC5, 0xBC, 0xC3, 0xB3] },
        { new JoinAccepted(new PlayerId(2), 30, 16, 100, 0x01020304), [0x81, 0x02, 0x1E, 0x10, 0x64, 0x00, 0x04, 0x03, 0x02, 0x01] },
        { new JoinRejected(RejectReason.ServerFull, ""), [0x82, 0x02, 0x00, 0x00] },
        { new ChatBroadcast("*", "hi"), [0x84, 0x01, 0x00, 0x2A, 0x02, 0x00, 0x68, 0x69] },
        { new ServerGoodbye("bye"), [0x85, 0x03, 0x00, 0x62, 0x79, 0x65] },
        {
            SmallSnapshot,
            [
                0x83, 0x07, 0x00, 0x00, 0x00, // type, tick
                0x01, // player count
                0x01, 0x01, 0x00, 0x41, 0x01, 0x01, 0x02, 0x00, // id, name "A", flags (alive), direction, score
                0x02, 0x00, 0x02, 0x01, 0x01, 0x01, // body: 2 segments (2,1) (1,1)
                0x01, 0x00, 0x05, 0x03, // food: 1 item (5,3)
            ]
        },
        {
            new StateSnapshot(1, [new PlayerSnapshot(new PlayerId(9), "", false, Direction.Down, 258, [])], []),
            [0x83, 0x01, 0x00, 0x00, 0x00, 0x01, 0x09, 0x00, 0x00, 0x00, 0x02, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00]
        },
    };

    [Theory]
    [MemberData(nameof(GoldenVectors))]
    public void Encode_ProducesExactBytes(Message message, byte[] expected)
    {
        Assert.Equal(expected, MessageCodec.Encode(message));
    }

    [Theory]
    [MemberData(nameof(GoldenVectors))]
    public void Decode_ExactBytes(Message expected, byte[] body)
    {
        Assert.Equal(expected, MessageCodec.Decode(body));
    }

    [Fact]
    public void Decode_IgnoresReservedFlagBits()
    {
        byte[] body = [0x83, 0x01, 0x00, 0x00, 0x00, 0x01, 0x09, 0x00, 0x00, 0xFE, 0x02, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00];

        var snapshot = Assert.IsType<StateSnapshot>(MessageCodec.Decode(body));

        Assert.False(snapshot.Players[0].IsAlive);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 0x00 })]
    [InlineData(new byte[] { 0x05 })]
    [InlineData(new byte[] { 0x80 })]
    [InlineData(new byte[] { 0x86 })]
    [InlineData(new byte[] { 0xFF, 0x00 })]
    public void Decode_UnknownOrMissingType_Throws(byte[] body)
    {
        Assert.Throws<ProtocolException>(() => MessageCodec.Decode(body));
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void Decode_TruncatedBody_Throws(Message message)
    {
        byte[] body = MessageCodec.Encode(message);

        for (int length = 0; length < body.Length; length++)
            Assert.Throws<ProtocolException>(() => MessageCodec.Decode(body.AsSpan(0, length)));
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void Decode_TrailingBytes_Throws(Message message)
    {
        byte[] body = [.. MessageCodec.Encode(message), 0x00];

        Assert.Throws<ProtocolException>(() => MessageCodec.Decode(body));
    }

    [Theory]
    [InlineData(new byte[] { 0x02, 0x04 })] // direction 4
    [InlineData(new byte[] { 0x02, 0xFF })] // direction 255
    [InlineData(new byte[] { 0x03, 0x01, 0x00, 0xFF })] // invalid UTF-8
    [InlineData(new byte[] { 0x03, 0x02, 0x00, 0xC5 })] // truncated UTF-8 sequence (and missing byte)
    [InlineData(new byte[] { 0x03, 0x02, 0x00, 0xC5, 0x41 })] // invalid UTF-8 continuation byte
    [InlineData(new byte[] { 0x82, 0x00, 0x00, 0x00 })] // reject reason 0
    [InlineData(new byte[] { 0x82, 0x05, 0x00, 0x00 })] // reject reason 5
    [InlineData(new byte[] { 0x83, 0x01, 0x00, 0x00, 0x00, 0x01, 0x09, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // direction 4
    [InlineData(new byte[] { 0x83, 0x01, 0x00, 0x00, 0x00, 0x02, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })] // 2 players, 1 present
    public void Decode_InvalidField_Throws(byte[] body)
    {
        Assert.Throws<ProtocolException>(() => MessageCodec.Decode(body));
    }

    [Fact]
    public void Encode_TooLongString_Throws()
    {
        var message = new ChatSend(new string('x', ushort.MaxValue + 1));

        Assert.ThrowsAny<ArgumentException>(() => MessageCodec.Encode(message));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 256)]
    public void Encode_PositionOutOfRange_Throws(int x, int y)
    {
        var snapshot = new StateSnapshot(1, [], [new Position(x, y)]);

        Assert.ThrowsAny<ArgumentException>(() => MessageCodec.Encode(snapshot));
    }
}
