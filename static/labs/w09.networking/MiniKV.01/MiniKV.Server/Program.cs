using MiniKV.Protocol;
using MiniKV.Server;

// Stage 1: no network yet - we simulate it by serializing every message to JSON and back.

const string SnapshotPath = "minikv.json";

var store = new KeyValueStore();
var handler = new RequestHandler(store);

Request[] requests =
[
    new SetRequest("user:1", "Ala"),
    new SetRequest("user:2", "Bob"),
    new SetRequest("greeting", "Hello, world!"),
    new GetRequest("user:1"),
    new DeleteRequest("user:2"),
    new GetRequest("user:2"),
    new KeysRequest(),
];

foreach (Request request in requests)
{
    // "Client" side: object -> JSON
    string requestJson = KvProtocol.Serialize(request);

    // "Server" side: JSON -> object -> response -> JSON
    Message? received = KvProtocol.Deserialize(requestJson);
    Response response = received is Request r ? handler.Handle(r) : Response.Fail("Expected a request");
    string responseJson = KvProtocol.Serialize(response);

    Console.WriteLine($">> {requestJson}");
    Console.WriteLine($"   {received} (equal to original: {received == request})");
    Console.WriteLine($"<< {responseJson}");
}

await store.SaveAsync(SnapshotPath);
KeyValueStore loaded = await KeyValueStore.LoadAsync(SnapshotPath);

Console.WriteLine();
Console.WriteLine($"Snapshot '{SnapshotPath}' loaded back: {loaded.Count} key(s) [{string.Join(", ", loaded.Keys())}]");
Console.WriteLine(await File.ReadAllTextAsync(SnapshotPath));
