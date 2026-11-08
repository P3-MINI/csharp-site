using System.Net;
using MiniKV.Protocol;
using MiniKV.Server;

// Usage: MiniKV.Server [port] [name]
int port = args.Length > 0 ? int.Parse(args[0]) : KvProtocol.DefaultPort;
string snapshotPath = $"minikv-{port}.json"; // one snapshot per server
string name = args.Length > 1 ? args[1] : Environment.MachineName;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // do not kill the process - shut down gracefully instead
    cts.Cancel();
};

KeyValueStore store = await KeyValueStore.LoadAsync(snapshotPath);
Console.WriteLine($"Loaded {store.Count} key(s) from '{snapshotPath}'");

var server = new KvServer(new IPEndPoint(IPAddress.Any, port), new RequestHandler(store));
var discovery = new DiscoveryResponder(name, port, store);
await Task.WhenAll(server.RunAsync(cts.Token), discovery.RunAsync(cts.Token));

await store.SaveAsync(snapshotPath);
Console.WriteLine($"Saved {store.Count} key(s) to '{snapshotPath}'. Bye!");
