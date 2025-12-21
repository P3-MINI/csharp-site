using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Server;

public static class Program
{
    const int Port = 5000;
    public static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
        using var listener = new TcpListener(ipEndPoint);
        
        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Wait any key to exit");
            Console.ReadKey(true);
            cts.Cancel();
        });

        await AcceptClients(listener, cts.Token);
        await waitForAnyKey;
    }

    private static async Task AcceptClients(TcpListener listener, CancellationToken token = default)
    {
        listener.Start(backlog: 10);
        var clients = new List<Task>();
        while (true)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                clients.Add(HandleClient(client, token));
                clients.RemoveAll(task => task.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        listener.Stop();
        await Task.WhenAll(clients);
    }

    private static async Task HandleClient(TcpClient client, CancellationToken token = default)
    {
        Console.WriteLine("New client connected");
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };
            while (await reader.ReadLineAsync(token) is { } command)
            {
                Console.WriteLine($"Received command: {command}");
                switch (command)
                {
                    case "date":
                        string message = DateTime.Now.ToString("d", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "time":
                        message = DateTime.Now.ToString("t", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "exit":
                        return;
                    default:
                        await writer.WriteLineAsync("Unknown command".ToCharArray(), token);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, which is expected during shutdown.
        }
        catch (IOException)
        {
            // Client disconnected abruptly.
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling client: {e.Message}");
        }
        finally
        {
            client.Dispose();
            Console.WriteLine("Client disconnected");
        }
    }
}