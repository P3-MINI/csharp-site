using System.Net;
using System.Net.Sockets;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

public class FramingTests
{
    [Fact]
    public async Task WriteFrame_WritesLittleEndianLengthAndBody()
    {
        var stream = new MemoryStream();

        await Framing.WriteFrameAsync(stream, new byte[] { 0xAA, 0xBB, 0xCC });
        await Framing.WriteFrameAsync(stream, new byte[300]);

        byte[] written = stream.ToArray();
        Assert.Equal([0x03, 0x00, 0x00, 0x00, 0xAA, 0xBB, 0xCC, 0x2C, 0x01, 0x00, 0x00], written[..11]);
        Assert.Equal(4 + 3 + 4 + 300, written.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(64)]
    [InlineData(100_000)]
    public async Task RoundTrip_ManyFrames_DeliveredInArbitraryChunks(int maxChunk)
    {
        var random = new Random(maxChunk);
        var frames = Enumerable.Range(0, 200)
            .Select(i => Enumerable.Range(0, random.Next(1, i % 50 == 0 ? 20_000 : 300)).Select(_ => (byte)random.Next(256)).ToArray())
            .ToList();
        var buffer = new MemoryStream();
        foreach (var frame in frames)
            await Framing.WriteFrameAsync(buffer, frame);

        var stream = new ChunkedStream(buffer.ToArray(), maxChunk, seed: maxChunk);
        foreach (var expected in frames)
            Assert.Equal(expected, await Framing.ReadFrameAsync(stream));
        Assert.Null(await Framing.ReadFrameAsync(stream));
    }

    [Fact]
    public async Task ReadFrame_EmptyStream_ReturnsNull()
    {
        Assert.Null(await Framing.ReadFrameAsync(new MemoryStream()));
    }

    [Theory]
    [InlineData(new byte[] { 0x05 })]
    [InlineData(new byte[] { 0x05, 0x00, 0x00 })]
    [InlineData(new byte[] { 0x05, 0x00, 0x00, 0x00 })]
    [InlineData(new byte[] { 0x05, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 })]
    public async Task ReadFrame_StreamEndsInsideFrame_ThrowsEndOfStream(byte[] data)
    {
        var stream = new ChunkedStream(data, maxChunk: 2);

        await Assert.ThrowsAsync<EndOfStreamException>(async () => await Framing.ReadFrameAsync(stream));
    }

    [Theory]
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 })] // length 0
    [InlineData(new byte[] { 0x01, 0x00, 0x01, 0x00 })] // 65 537
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F })] // int.MaxValue
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF })] // uint.MaxValue
    public async Task ReadFrame_InvalidLength_ThrowsProtocolException(byte[] header)
    {
        // Only the header is present: an implementation must not try to read (or allocate) the body.
        var stream = new MemoryStream(header);

        await Assert.ThrowsAsync<ProtocolException>(async () => await Framing.ReadFrameAsync(stream));
    }

    [Fact]
    public async Task ReadFrame_MaxLength_IsAccepted()
    {
        var body = new byte[ProtocolConstants.MaxFrameLength];
        body[^1] = 42;
        var buffer = new MemoryStream();
        await Framing.WriteFrameAsync(buffer, body);

        var read = await Framing.ReadFrameAsync(new ChunkedStream(buffer.ToArray(), maxChunk: 1000));

        Assert.Equal(body, read);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ProtocolConstants.MaxFrameLength + 1)]
    public async Task WriteFrame_InvalidBodyLength_Throws(int length)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(
            async () => await Framing.WriteFrameAsync(new MemoryStream(), new byte[length]));
    }

    [Fact]
    public async Task ReadFrame_Cancellation_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await Framing.ReadFrameAsync(new SilentStream(), cts.Token));
    }

    [Fact]
    public async Task Frames_OverTcp_SentByteByByte()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var sender = new TcpClient { NoDelay = true };
        await sender.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
        using var receiver = await listener.AcceptTcpClientAsync();

        byte[][] frames = [[1], [2, 3, 4, 5, 6], [.. Enumerable.Range(0, 1000).Select(i => (byte)i)]];
        var encoded = new MemoryStream();
        foreach (var frame in frames)
            await Framing.WriteFrameAsync(encoded, frame);

        var sending = Task.Run(async () =>
        {
            var stream = sender.GetStream();
            byte[] data = encoded.ToArray();
            for (int i = 0; i < data.Length; i++)
            {
                await stream.WriteAsync(data.AsMemory(i, 1));
                if (i % 97 == 0)
                    await Task.Delay(1);
            }
            sender.Client.Shutdown(SocketShutdown.Send);
        });

        var receiving = receiver.GetStream();
        foreach (var expected in frames)
            Assert.Equal(expected, await Framing.ReadFrameAsync(receiving));
        Assert.Null(await Framing.ReadFrameAsync(receiving));
        await sending;
    }
}
