using System.IO.Pipes;

namespace Client;

class Program
{
    private static async Task Main()
    {
        await using var pipe = new NamedPipeClientStream("p3_pipe");
        await pipe.ConnectAsync();

        var reader = new StreamReader(pipe);
        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;

        await foreach (var line in EnumerateLines("lorem.txt"))
        {
            await writer.WriteLineAsync(line);
            var processedLine = await reader.ReadLineAsync();
            Console.WriteLine(processedLine);
        }
    }

    private static async IAsyncEnumerable<string> EnumerateLines(string path)
    {
        var stream = new StreamReader(path);

        while (await stream.ReadLineAsync() is { } line)
        {
            await Task.Delay(500);
            yield return line;
        }
    }
}