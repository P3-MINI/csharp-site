using System.Net;
using System.Net.Sockets;
using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// Accepts TCP connections and serves every client in its own task.
/// </summary>
public sealed class KvServer(IPEndPoint endPoint, RequestHandler handler)
{
    public async Task RunAsync(CancellationToken ct)
    {
        using var listener = new TcpListener(endPoint);
        listener.Start();
        Console.WriteLine($"Listening on {listener.LocalEndpoint}. Press Ctrl+C to stop.");

        List<Task> clients = [];
        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(ct);
                clients.Add(HandleClientAsync(client, ct));
                clients.RemoveAll(task => task.IsCompleted);
            }
        }
        catch (OperationCanceledException)
        {
            // Ctrl+C - stop accepting new clients
        }

        listener.Stop();
        await Task.WhenAll(clients);
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        EndPoint? remote = client.Client.RemoteEndPoint;
        Console.WriteLine($"[{remote}] connected");

        using (client)
        {
            try
            {
                var messages = new MessageStream(client.GetStream());
                while (await messages.ReadAsync(ct) is { } message)
                {
                    Console.WriteLine($"[{remote}] {message}");
                    Response response = message is Request request
                        ? handler.Handle(request)
                        : Response.Fail("Expected a request");
                    await messages.WriteAsync(response, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // The server is shutting down
            }
            catch (IOException e)
            {
                Console.WriteLine($"[{remote}] connection error: {e.Message}");
            }
            catch (InvalidDataException e)
            {
                Console.WriteLine($"[{remote}] protocol error: {e.Message}");
            }
        }

        Console.WriteLine($"[{remote}] disconnected");
    }
}
