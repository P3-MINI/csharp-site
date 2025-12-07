using System.Net.Sockets;
using Chat.Common;
using Chat.Common.MessageHandlers;


class Program
{
    static bool serverIsActive = true;

    const string defaultHost = "127.0.0.1";
    const int defaultPort = 5000;
    public const int ConnectTimeOutMs = 3000;

    static async Task Main(string[] args)
    {
        if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?"))
        {
            PrintUsage();
            return;
        }

        string host = defaultHost;
        int port = defaultPort;
        if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0])) host = args[0];
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

        using NetworkStream stream = client!.GetStream();
        var cts = new CancellationTokenSource();

        Task receiver = Task.Factory.StartNew(
            () => MessageReceiver(stream, progress, cts.Token),
            TaskCreationOptions.LongRunning
        );

        using var sender = new MessageWriter(stream);

        while (true)
        {
            string? msg = Console.ReadLine();
            if (msg == null)
                break;

            if (msg.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (!serverIsActive) {
                Console.WriteLine("Server is down, you can only exit the app by typing exit");
                continue;
            }

            var dto = new MessageDTO
            {
                Content = msg,
                Sender = name,
                Time = DateTime.UtcNow
            };

            await sender.WriteMessage(dto, cts.Token);
        }

        cts.Cancel();
        receiver.Wait();

        progress.Report("Disconnecting");
    }


    static async Task MessageReceiver(NetworkStream stream, IProgress<string> progress, CancellationToken ct)
    {
        using var reader = new MessageReader(stream);

        try {
            while (!ct.IsCancellationRequested)
            {
                MessageDTO? msg = await reader.ReadMessage(ct);
                if (msg == null) {
                    progress.Report("[System] Your peer disconnected.");
                    serverIsActive = false;
                    break;
                }

                Console.WriteLine($"[{msg.Time:u}] {msg.Sender}: {msg.Content}");
            }
        }
        catch (InvalidMessageReceived invMsg)
        {
            progress.Report($"[Error]: invalid message received {invMsg.Message}");
        }
        catch (OperationCanceledException) {}   // Ignore
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
