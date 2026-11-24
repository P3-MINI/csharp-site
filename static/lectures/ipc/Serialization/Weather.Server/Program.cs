#define SYSTEM_JSON

using System.Net;
using System.Net.Sockets;
#if SYSTEM_JSON
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif
using Weather.Common;

namespace Weather.Server;

public static class Program
{
    const int Port = 5000;
#if SYSTEM_JSON
    // Reuse the options instance - it caches serialization metadata
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };
#endif
    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
        using var listener = new TcpListener(ipEndPoint);
        
        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Press any key to exit");
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
                Console.WriteLine("New client connected");
                clients.Add(HandleClient(client, token));
                clients.RemoveAll(task => task.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        listener.Stop();
        try
        {
            await Task.WhenAll(clients);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static async Task HandleClient(TcpClient client, CancellationToken token = default)
    {
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
                    case "exit":
                        return;
                    case {} location:
                        Forecast forecast = Forecast.Generate(location);
#if SYSTEM_JSON
                        var message = JsonSerializer.Serialize(forecast, Options);
#else
                        var message = JsonConvert.SerializeObject(forecast, Formatting.None);
#endif
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    default:
                        await writer.WriteLineAsync("Unknown command".ToCharArray(), token);
                        break;
                }
            }
        }
        finally
        {
            client.Dispose();
        }
    }
}