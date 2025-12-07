using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class KeyValueServer
{
    private static readonly ConcurrentDictionary<string, string> store =
        new ConcurrentDictionary<string, string>();

    public static async Task Main()
    {
        Console.WriteLine("Key-Value Store Server started.");
        Console.WriteLine("Waiting for client... (press Ctrl+C to exit)");

        using var cts = new CancellationTokenSource();
        var clientTasks = new ConcurrentDictionary<int, Task>();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // prevent process termination; we'll shut down gracefully
            Console.WriteLine("Shutdown requested — stopping acceptance of new clients...");
            cts.Cancel();
        };

        while (!cts.IsCancellationRequested)
        {
            var server = // TODO

            var task = Task.Factory.StartNew(
                () => HandleClientAsync(server, cts.Token).ContinueWith(
                    t =>  {
                        clientTasks.TryRemove(t.Id, out _);
                        if (t.IsFaulted)
                            Console.WriteLine($"Client handler failed: {t.Exception?.GetBaseException().Message}");
                    },
                    TaskScheduler.Default
                ),
                TaskCreationOptions.LongRunning
            );
            clientTasks[task.Id] = task;
        }

        // Wait for any active client handlers to finish
        try
        {
            await Task.WhenAll(clientTasks.Values.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while waiting for clients to finish: {ex.Message}");
        }

        Console.WriteLine("Server shut down.");
    }


    private static async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }

    private static string ProcessCommand(string input)
    {
        string[] parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "ERROR Empty command";

        string cmd = parts[0].ToUpperInvariant();

        return cmd switch
        {
            "SET" => HandleSet(parts),
            "GET" => HandleGet(parts),
            "DELETE" => HandleDelete(parts),
            _ => "ERROR Unknown command"
        };
    }

    private static string HandleSet(string[] parts)
    {
        if (parts.Length < 3)
            return "ERROR Usage: SET key value";

        store[parts[1]] = parts[2];

        return "OK";
    }

    private static string HandleGet(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: GET key";

        return store.TryGetValue(parts[1], out var value) ? value : "NOT_FOUND";
    }

    private static string HandleDelete(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: DELETE key";

        return store.TryRemove(parts[1], out _) ? "OK" : "NOT_FOUND";
    }
}