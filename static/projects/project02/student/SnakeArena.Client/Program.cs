using System.Net;
using SnakeArena.Client;

if (args is ["--help"] or ["-h"])
{
    Console.WriteLine(ClientOptions.Usage);
    return 0;
}

ClientOptions options;
try
{
    options = ClientOptions.Parse(args);
}
catch (ArgumentException e)
{
    Console.Error.WriteLine(e.Message);
    Console.Error.WriteLine(ClientOptions.Usage);
    return 2;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // do not kill the process, let the client leave the game gracefully
    cts.Cancel();
};

if (options.Discover)
{
    Console.WriteLine("Looking for servers in the local network...");
    IPEndPoint[] targets =
    [
        new(IPAddress.Broadcast, options.DiscoveryPort),
        new(IPAddress.Loopback, options.DiscoveryPort), // a server on this machine, even without a network
    ];
    var servers = await LanDiscovery.FindServersAsync(targets, TimeSpan.FromSeconds(1), cts.Token);
    if (servers.Count == 0)
    {
        Console.WriteLine("No servers found.");
        return 2;
    }

    for (int i = 0; i < servers.Count; i++)
        Console.WriteLine($"  [{i + 1}] {servers[i]}");

    int choice = 1;
    if (servers.Count > 1 && !options.Bot)
    {
        Console.Write($"Choose a server (1-{servers.Count}): ");
        if (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > servers.Count)
            return 2;
    }

    var chosen = servers[choice - 1];
    options = options with { Host = chosen.EndPoint.Address.ToString(), Port = chosen.EndPoint.Port };
}

return await new GameClient(options).RunAsync(cts.Token);
