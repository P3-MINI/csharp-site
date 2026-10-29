using System.Net.Sockets;
using MiniKV.Client;
using MiniKV.Protocol;

// Usage: MiniKV.Client [host] [port]
string host = args.Length > 0 ? args[0] : "localhost";
int port = args.Length > 1 ? int.Parse(args[1]) : KvProtocol.DefaultPort;

KvConnection connection;
try
{
    connection = await KvConnection.ConnectAsync(host, port, TimeSpan.FromSeconds(3));
}
catch (Exception e) when (e is SocketException or TimeoutException)
{
    Console.WriteLine($"Cannot connect to {host}:{port}: {e.Message}");
    return 1;
}

using (connection)
{
    // Called from the receive loop (a background thread), possibly while we wait in ReadLine
    connection.NotificationReceived += notification => Console.WriteLine(notification.Value is null
        ? $"[watch] '{notification.Key}' was deleted"
        : $"[watch] '{notification.Key}' = \"{notification.Value}\"");

    Console.WriteLine($"Connected to {host}:{port}. {CommandParser.Usage} | quit");
    while (true)
    {
        Console.Write("> ");
        string? line = Console.ReadLine()?.Trim();
        if (line is null or "quit") // null = end of input (Ctrl+D / Ctrl+Z)
            break;
        if (line.Length == 0)
            continue;

        try
        {
            Request request = CommandParser.Parse(line);
            Response response = await connection.SendAsync(request);
            Console.WriteLine(ResponseFormatter.Format(response));
        }
        catch (FormatException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e) when (e is IOException or InvalidDataException)
        {
            Console.WriteLine($"Connection lost: {e.Message}");
            return 1;
        }
    }
}

Console.WriteLine("Bye!");
return 0;
