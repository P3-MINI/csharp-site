using System.Net;

namespace ChatServer;


class Program
{
    public static async Task Main()
    {
        const int port = 5000;
        
        Console.WriteLine("Press Ctrl+C to stop the server.");
        
        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("Shutdown requested — stopping acceptance of new clients...");
            cts.Cancel();
        };

        var server = new ChatServer(IPAddress.Any, port);
        await server.Run(cts.Token);
    }
}