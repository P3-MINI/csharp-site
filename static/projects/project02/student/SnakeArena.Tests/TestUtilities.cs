using System.Net;
using System.Net.Sockets;
using SnakeArena.Server;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

/// <summary>
/// A read-only stream that returns the data in small chunks of random size (1..maxChunk bytes per read),
/// like a network stream does.
/// </summary>
internal sealed class ChunkedStream(byte[] data, int maxChunk, int seed = 0) : Stream
{
    private readonly Random _random = new(seed);
    private int _position;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer)
    {
        int n = Math.Min(Math.Min(buffer.Length, data.Length - _position), _random.Next(1, maxChunk + 1));
        data.AsSpan(_position, n).CopyTo(buffer);
        _position += n;
        return n;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_random.Next(4) == 0)
            await Task.Yield();
        return Read(buffer.Span);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>A stream whose reads never complete (until cancelled).</summary>
internal sealed class SilentStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return 0;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("Use ReadAsync.");
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>Runs a <see cref="GameServer"/> in the background for the duration of a test.</summary>
internal sealed class TestServer : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private TestServer(GameServer server)
    {
        Server = server;
        Run = server.RunAsync(_cts.Token);
    }

    public GameServer Server { get; }
    public Task Run { get; }

    /// <summary>The TCP endpoint to connect to (always on the loopback interface).</summary>
    public IPEndPoint EndPoint => new(IPAddress.Loopback, Server.LocalEndPoint.Port);

    public static ServerOptions DefaultOptions => new()
    {
        Port = 0,
        Name = "test-server",
        Width = 30,
        Height = 16,
        TickIntervalMs = 20,
        MaxPlayers = 8,
        Seed = 42,
        EnableDiscovery = false,
        DiscoveryPort = 0,
        HandshakeTimeout = TimeSpan.FromSeconds(1),
    };

    public static TestServer Start(ServerOptions? options = null)
    {
        var server = new GameServer(options ?? DefaultOptions);
        server.Start();
        return new TestServer(server);
    }

    /// <summary>Requests a graceful shutdown and waits until the server stops.</summary>
    public async Task StopAsync(TimeSpan timeout)
    {
        _cts.Cancel();
        await Run.WaitAsync(timeout);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            try
            {
                await Run.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // The test result has already been decided.
            }
        }
        _cts.Dispose();
    }
}

/// <summary>A minimal protocol client used by the tests (it relies on your Framing and MessageCodec).</summary>
internal sealed class TestClient : IAsyncDisposable
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;

    private TestClient(TcpClient tcp)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
    }

    public static async Task<TestClient> ConnectAsync(IPEndPoint endPoint)
    {
        var tcp = new TcpClient { NoDelay = true };
        await tcp.ConnectAsync(endPoint);
        return new TestClient(tcp);
    }

    public static async Task<(TestClient Client, JoinAccepted Accepted)> JoinAsync(IPEndPoint endPoint, string name)
    {
        var client = await ConnectAsync(endPoint);
        await client.SendAsync(new JoinRequest(ProtocolConstants.Version, name));
        var reply = await client.ReceiveAsync();
        return (client, Assert.IsType<JoinAccepted>(reply));
    }

    public Task SendAsync(Message message) => SendRawAsync(MessageCodec.Encode(message));

    public Task SendRawAsync(byte[] body) => Framing.WriteFrameAsync(_stream, body).AsTask();

    /// <summary>Returns the next message, or <see langword="null"/> if the server closed the connection.</summary>
    public async Task<Message?> ReceiveAsync(TimeSpan? timeout = null)
    {
        using var cts = new CancellationTokenSource(timeout ?? DefaultTimeout);
        byte[]? body;
        try
        {
            body = await Framing.ReadFrameAsync(_stream, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("No message received in time.");
        }
        catch (IOException)
        {
            return null; // connection reset by the server
        }
        return body is null ? null : MessageCodec.Decode(body);
    }

    /// <summary>Skips other messages until a message of type <typeparamref name="T"/> matching the predicate arrives.</summary>
    public async Task<T> ExpectAsync<T>(Func<T, bool>? predicate = null, TimeSpan? timeout = null) where T : Message
    {
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
        while (true)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
                throw new TimeoutException($"No matching {typeof(T).Name} received in time.");
            var message = await ReceiveAsync(remaining);
            if (message is null)
                throw new IOException($"The server closed the connection while the test was waiting for {typeof(T).Name}.");
            if (message is T typed && (predicate is null || predicate(typed)))
                return typed;
        }
    }

    /// <summary>Skips messages until the server closes the connection; returns the skipped messages.</summary>
    public async Task<List<Message>> ExpectClosedAsync(TimeSpan? timeout = null)
    {
        var skipped = new List<Message>();
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
        while (true)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
                throw new TimeoutException("The server did not close the connection in time.");
            var message = await ReceiveAsync(remaining);
            if (message is null)
                return skipped;
            skipped.Add(message);
        }
    }

    public ValueTask DisposeAsync()
    {
        _tcp.Dispose();
        return ValueTask.CompletedTask;
    }
}
