using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// One connected client. Everything sent to the client (responses and notifications)
/// goes through the outbox queue and is written by a single writer task,
/// so two frames are never written to the stream at the same time.
/// </summary>
public sealed class ClientSession(TcpClient client)
{
    private readonly MessageStream _messages = new(client.GetStream());
    private readonly Channel<Message> _outbox = Channel.CreateUnbounded<Message>();

    public EndPoint? RemoteEndPoint { get; } = client.Client.RemoteEndPoint;

    /// <summary>
    /// Queues a message for this client. Thread-safe and never blocks.
    /// </summary>
    public void Send(Message message) => _outbox.Writer.TryWrite(message);

    public async Task RunAsync(RequestHandler handler, CancellationToken ct)
    {
        Task writer = WriteLoopAsync(ct);
        try
        {
            while (await _messages.ReadAsync(ct) is { } message)
            {
                Console.WriteLine($"[{RemoteEndPoint}] {message}");
                Send(message is Request request
                    ? handler.Handle(request, this)
                    : Response.Fail("Expected a request"));
            }
        }
        finally
        {
            _outbox.Writer.TryComplete(); // no more messages - the writer loop ends after sending the rest
            await writer;
        }
    }

    private async Task WriteLoopAsync(CancellationToken ct)
    {
        try
        {
            await foreach (Message message in _outbox.Reader.ReadAllAsync(ct))
                await _messages.WriteAsync(message, ct);
        }
        catch (Exception e) when (e is OperationCanceledException or IOException)
        {
            // Server shutdown or the client is gone - the read loop will notice it as well
        }
    }
}
