using System.Net;
using System.Net.Sockets;
using MiniKV.Client;
using MiniKV.Protocol;

// Usage: MiniKV.Client [host] [port] - without arguments the server is discovered via UDP
string host;
int port;
if (args.Length > 0)
{
    host = args[0];
    port = args.Length > 1 ? int.Parse(args[1]) : KvProtocol.DefaultPort;
}
else if (await FindServerAsync() is { } found)
{
    host = found.EndPoint.Address.ToString();
    port = found.EndPoint.Port;
}
else
{
    Console.WriteLine("No server found. Start one or pass its address: MiniKV.Client <host> [port]");
    return 1;
}

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

static async Task<DiscoveredServer?> FindServerAsync()
{
    // Broadcast reaches every machine in the LAN; loopback is a fallback for a machine without a network
    IPAddress[] targets = [IPAddress.Broadcast, IPAddress.Loopback];
    foreach (IPAddress target in targets)
    {
        Console.WriteLine($"Looking for servers via {target}...");
        List<DiscoveredServer> servers = await ServerDiscovery.DiscoverAsync(target, TimeSpan.FromMilliseconds(500));
        foreach (DiscoveredServer server in servers)
            Console.WriteLine($"  found '{server.Info.Name}' at {server.EndPoint} ({server.Info.KeyCount} key(s))");

        if (servers.Count > 0)
            return servers[0];
    }

    return null;
}
