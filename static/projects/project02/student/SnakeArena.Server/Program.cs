using SnakeArena.Server;

if (args is ["--help"] or ["-h"])
{
    Console.WriteLine(ServerOptions.Usage);
    return 0;
}

ServerOptions options;
try
{
    options = ServerOptions.Parse(args);
}
catch (ArgumentException e)
{
    Console.Error.WriteLine(e.Message);
    Console.Error.WriteLine(ServerOptions.Usage);
    return 2;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // do not kill the process, shut down gracefully instead
    cts.Cancel();
};

var server = new GameServer(options, Console.Out);
server.Start();
Console.WriteLine($"Snake Arena server '{options.Name}' listening on {server.LocalEndPoint}. Press Ctrl+C to stop.");

await server.RunAsync(cts.Token);
Console.WriteLine("Server stopped.");
return 0;
