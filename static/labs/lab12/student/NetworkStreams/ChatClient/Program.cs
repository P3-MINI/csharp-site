using System.Net.Sockets;
using Chat.Common;
using Chat.Common.MessageHandlers;

namespace ChatClient;

class Program
{
    const string defaultHost = "localhost";
    const int defaultPort = 5000;
    const int connectTimeoutMs = 3000;

    static async Task Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
        {
            PrintUsage();
            return;
        }
        
        string host = defaultHost;
        int port = defaultPort;
        
        if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
            host = args[0];

        if (args.Length >= 2)
        {
            if (!int.TryParse(args[1], out port))
            {
                Console.WriteLine("Invalid port number. Must be an integer.");
                return;
            }
        }

        try
        {
            await RunChatClient(host, port);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected application failure: {ex.Message}");
        }
    }


    static void PrintUsage()
    {
        Console.WriteLine("Usage: ChatClient [host] [port]");
        Console.WriteLine($"Defaults: host={defaultHost}, port={defaultPort}");
    }


    static async Task RunChatClient(string host, int port)
    {
        IProgress<string> progress = new Progress<string>(Console.WriteLine);

        Console.Write("Enter your name: ");
        string? name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
            name = "Anonymous";

        using var client = await Connect(host, port, progress);
        if (client == null)
            return;

        using NetworkStream stream = client.GetStream();
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        
        Task receiver = MessageReceiver(stream, progress, cts);

        using var sender = new MessageWriter(stream);

        while (true)
        {
            string? msg = Console.ReadLine();
            if (msg == null)
                break;

            if (msg.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (ct.IsCancellationRequested) {
                progress.Report("Connection is down, you can only exit the app by typing exit");
                continue;
            }

            var dto = new MessageDTO
            {
                Content = msg,
                Sender = name,
                Time = DateTime.UtcNow
            };

            try
            {
                await sender.WriteMessage(dto, cts.Token);
            }
            catch (TooLongMessageException)
            {
                progress.Report($"[ERROR] Message was too long.");
            }
            
        }

        await cts.CancelAsync();
        try
        {
            await receiver.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (TimeoutException)
        {
            progress.Report("[Error]: timeout while waiting for message receiver.");
        }

        progress.Report("Disconnecting");
    }


    static async Task MessageReceiver(NetworkStream stream, IProgress<string> progress, CancellationTokenSource cts)
    {
        var ct = cts.Token;
        
        using var reader = new MessageReader(stream);

        try {
            while (!ct.IsCancellationRequested)
            {
                MessageDTO? msg = await reader.ReadMessage(ct);
                if (msg == null) {
                    progress.Report("[System] Your peer disconnected.");
                    cts.Cancel();
                    break;
                }

                Console.WriteLine($"[{msg.Time:u}] {msg.Sender}: {msg.Content}");
            }
        }
        catch (InvalidMessageException invMsg)
        {
            progress.Report($"[Error]: invalid message received {invMsg.Message}");
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown due to cancellation; no further action required.
            return;
        }
        catch (Exception e)
        {
            progress.Report($"[Error]: {e.Message}");
        }
    }


    static async Task<TcpClient?> Connect(string host, int port, IProgress<string> progress)
    {
        // TODO
        throw new NotImplementedException();
    }
}
