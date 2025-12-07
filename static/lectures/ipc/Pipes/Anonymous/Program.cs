using System.Diagnostics;
using System.IO.Pipes;

namespace Anonymous;

class Program
{
    private static async Task Main(string[] args)
    {
        if (args is [var handle])
        {
            await ChildWork(handle);
        }
        else
        {
            await ParentWork();
        }
    }

    private static async Task ParentWork()
    {
        var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]),
            Arguments = pipe.GetClientHandleAsString()
        };

        using var childProcess = Process.Start(startInfo);
        if (childProcess == null)
        {
            Console.WriteLine("Failed to create child process");
            Environment.Exit(-1);
        }
        Console.WriteLine("Child process started with PID: " + childProcess.Id);

        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        
        await foreach (var line in EnumerateLines("lorem.txt"))
        {
            await writer.WriteLineAsync(line);
        }

        await pipe.DisposeAsync(); // Try to comment that line

        await childProcess.WaitForExitAsync();
    }

    private static async Task ChildWork(string pipeHandle)
    {
        await using var pipe = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);

        using var reader = new StreamReader(pipe);
        while (await reader.ReadLineAsync() is { } line)
        {
            Console.WriteLine(line.ToUpper());
        }
    }
    
    private static async IAsyncEnumerable<string> EnumerateLines(string path)
    {
        using var stream = new StreamReader(path);

        while (await stream.ReadLineAsync() is { } line)
        {
            await Task.Delay(500);
            yield return line;
        }
    }
}