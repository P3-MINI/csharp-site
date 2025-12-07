using System.IO.Pipes;

namespace Server;

class Program
{
    private static async Task Main()
    {
        await using var pipe = new NamedPipeServerStream("p3_pipe", PipeDirection.InOut);
        await pipe.WaitForConnectionAsync();
        
        var reader = new StreamReader(pipe);
        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        
        while (await reader.ReadLineAsync() is { } line)
        {
            await writer.WriteLineAsync(line.ToUpper());
        }
    }
}