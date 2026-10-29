using System.Net.Sockets;
using MiniKV.Protocol;

namespace MiniKV.Client;

/// <summary>
/// An open connection to a MiniKV server.
/// </summary>
public sealed class KvConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly MessageStream _messages;

    private KvConnection(TcpClient client)
    {
        _client = client;
        _messages = new MessageStream(client.GetStream());
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
    /// Sends a request and waits for the response.
    /// </summary>
    /// <exception cref="IOException">The connection was lost.</exception>
    public async Task<Response> SendAsync(Request request, CancellationToken ct = default)
    {
        await _messages.WriteAsync(request, ct);
        return await _messages.ReadAsync(ct) switch
        {
            Response response => response,
            null => throw new IOException("The server closed the connection"),
            var other => throw new InvalidDataException($"Unexpected message: {other}"),
        };
    }

    public void Dispose() => _client.Dispose();
}
