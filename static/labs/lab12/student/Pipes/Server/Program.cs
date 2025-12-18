using Server;

class KeyValueServer
{
    public static void Main()
    {
        Console.WriteLine("Key-Value Store Server started. (press Ctrl+C to exit)");
        Console.WriteLine("Waiting for client...");
        
        using CancellationTokenSource cts = new();
        
        KvServer server = new("kv_pipe");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // prevent process termination; we'll shut down gracefully
            Console.WriteLine("Shutdown requested — stopping acceptance of new clients...");
            cts.Cancel();
        };
        
        server.Start(cts.Token).Wait();

        Console.WriteLine("Server shut down.");
    }
}
