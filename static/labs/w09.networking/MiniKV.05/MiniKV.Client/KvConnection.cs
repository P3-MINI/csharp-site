using System.Net.Sockets;
using System.Threading.Channels;
using MiniKV.Protocol;

namespace MiniKV.Client;

/// <summary>
/// An open connection to a MiniKV server.
/// A background receive loop reads all incoming messages: responses are handed over
/// to <see cref="SendAsync"/>, notifications raise <see cref="NotificationReceived"/>.
/// </summary>
public sealed class KvConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly MessageStream _messages;
    private readonly Channel<Response> _responses = Channel.CreateUnbounded<Response>();

    /// <summary>
    /// Raised on a background thread when a watched key changes.
    /// </summary>
    public event Action<KeyChanged>? NotificationReceived;

    private KvConnection(TcpClient client)
    {
        _client = client;
        _messages = new MessageStream(client.GetStream());
        _ = ReceiveLoopAsync(); // runs until the connection is closed
    }

    /// <exception cref="SocketException">The server cannot be reached (refused, unknown host, ...).</exception>
    /// <exception cref="TimeoutException">The server did not answer within <paramref name="timeout"/>.</exception>
    public static async Task<KvConnection> ConnectAsync(string host, int port, TimeSpan timeout)
    {
        var client = new TcpClient();
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await client.ConnectAsync(host, port, cts.Token);
            return new KvConnection(client);
        }
        catch (OperationCanceledException)
        {
            client.Dispose();
            throw new TimeoutException($"No answer within {timeout.TotalSeconds} s");
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Sends a request and waits for the response. Do not call it concurrently.
    /// </summary>
    /// <exception cref="IOException">The connection was lost.</exception>
    public async Task<Response> SendAsync(Request request, CancellationToken ct = default)
    {
        await _messages.WriteAsync(request, ct);
        try
        {
            return await _responses.Reader.ReadAsync(ct);
        }
        catch (ChannelClosedException)
        {
            throw new IOException("The server closed the connection");
        }
    }

    private async Task ReceiveLoopAsync()
    {
        try
        {
            while (await _messages.ReadAsync() is { } message)
            {
                switch (message)
                {
                    case Response response:
                        await _responses.Writer.WriteAsync(response);
                        break;
                    case KeyChanged notification:
                        NotificationReceived?.Invoke(notification);
                        break;
                }
            }
        }
        catch (Exception e) when (e is IOException or InvalidDataException or ObjectDisposedException)
        {
            // Connection lost or closed by Dispose
        }
        finally
        {
            _responses.Writer.TryComplete(); // wakes up SendAsync if it waits for a response
        }
    }

    public void Dispose() => _client.Dispose();
}
