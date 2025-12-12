using System.IO.Pipes;
using System.Text;

namespace Server;

public class KvServer(string pipeName)
{
    public async Task Start(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await using var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );
            
                await server.WaitForConnectionAsync(ct);
                await HandleClientAsync(server, ct);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server was cancelled");
        }
    }

    private readonly Dictionary<string, string> _db = new();
    
    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken ct)
    {
        Console.WriteLine("Client connected.");
        using var reader = new StreamReader(pipe, Encoding.UTF8);
        await using var writer = new StreamWriter(pipe, Encoding.UTF8) { AutoFlush = true };

        try
        {
            while (pipe.IsConnected && !ct.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync(ct);
                if (line == null) // Client disconnected
                    break;

                string response = ProcessCommand(line);
                await writer.WriteLineAsync(response);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Problem with client communication");
        }
       
        Console.WriteLine("Client disconnected.");
    }
    
    private string ProcessCommand(string input)
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

    private string HandleSet(string[] parts)
    {
        if (parts.Length < 3)
            return "ERROR Usage: SET key value";

        _db[parts[1]] = parts[2];

        return "OK";
    }

    private string HandleGet(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: GET key";

        return _db.GetValueOrDefault(parts[1], "NOT_FOUND");
    }

    private string HandleDelete(string[] parts)
    {
        if (parts.Length < 2)
            return "ERROR Usage: DELETE key";

        return _db.Remove(parts[1], out _) ? "OK" : "NOT_FOUND";
    }
}

