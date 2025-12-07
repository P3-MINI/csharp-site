---
title: "Pipes"
weight: 20
---

# Pipes

Pipes are a mechanism for inter-process communication (IPC) that allows data to flow between processes, either unidirectionally or bidirectionally. They function similarly to streams, where one process writes data to one end of the pipe, and another process reads it from the other end.

## Named Pipes

> [!NOTE]
> Naming conventions in .NET can be misleading. On Unix-like systems (such as Linux or macOS), `NamedPipeStream` is not implemented as FIFO files (which are POSIX named pipes), but rather as **Unix Domain Sockets**. On Windows, these are indeed named pipes.

Named pipes, as their name suggests, have a unique name within the system. This allows them to be used by any two processes, even if they are not related. The communication model resembles a client-server architecture. One process acts as a server, which creates the pipe and waits for a connection, while the other is a client that connects to it.

The following code demonstrates a server that creates a bidirectional pipe (`PipeDirection.InOut`) and waits for a client connection using `WaitForConnectionAsync()`.

```csharp
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
```

The following code demonstrates a client that connects to the server using the same pipe name. After establishing a connection, the client sends lines of text, then reads and displays the processed response sent back by the server. All communication occurs over a single pipe, configured as bidirectional. On Unix systems, classic named pipes (FIFO) are unidirectional. Bidirectional communication would require two separate pipes. The .NET implementation bypasses this limitation by using Unix Domain Sockets, which natively support bidirectional communication.

> [!NOTE]
> Reading data from a pipe is blocking. If the pipe is empty, the program will wait until data appears on the other end to be read or until the pipe is closed.

```csharp
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
```

## Anonymous Pipes

Anonymous pipes do not have a name in the file system and therefore cannot be used by arbitrary, independent processes. Their primary use is for communication between a parent process and a child process created by it.

The mechanism involves the parent process creating an anonymous pipe and passing a handle to one of its ends to the child process during its startup. The pipe handle is serialized into a string and passed as a command-line argument. The child process uses the received handle to connect to its parent. In the example below, the same program can play both roles – its behavior depends on the arguments with which it was launched.

```csharp
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
```

An important element in the parent's code is the line `await pipe.DisposeAsync()`. Closing the pipe on the server side sends an end-of-file (EOF) signal to the client. This causes the `reader.ReadLineAsync()` call in the child process to return `null`, allowing the loop to terminate naturally and the child application to close correctly. Without this, the child process would wait indefinitely for data, and the parent process would wait for the child to finish, leading to a deadlock.

> [!INFO]
> ## Source Code
> {{< filetree dir="lectures/ipc/Pipes" >}}
