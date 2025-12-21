---
title: "Tcp"
weight: 40
---

# Tcp

The `TCP` (Transmission Control Protocol) is one of the foundations of the internet. It is a connection-oriented protocol, which means that a connection is established between a client and a server before any data is exchanged. TCP guarantees **reliable and ordered data delivery**, which makes it ideal for applications where data integrity is crucial, such as file transfers or e-mail. In .NET, TCP communication is handled by classes in the `System.Net` and `System.Net.Sockets` namespaces.

Communication using `TCP` in .NET is based on two classes: `TcpListener` on the server side and `TcpClient` on the client side.

## Server

The server listens for incoming connections using `TcpListener`. When a connection is accepted (`AcceptTcpClientAsync`), a `TcpClient` object is created to represent that connection. The server in the example is asynchronous and can handle multiple clients simultaneously. For each client, a separate task (`HandleClient`) is created, which reads commands and sends responses.

After establishing a connection, both sides communicate using a stream (`NetworkStream`), which is "wrapped" in a `StreamReader` and `StreamWriter` to facilitate text operations.

> [!WARNING]
> Please note that `StreamReader` and `StreamWriter` internally buffer data. They work best in protocols where messages are text-based and each message is terminated by a newline character. This can lead to issues if the protocol is more complex. In such cases, `BinaryReader` and `BinaryWriter` might be a better choice, as they provide precise control over reading and writing data to the stream, even for text data.

The defined "protocol" is very simple - the client sends text commands (`date`, `time`, `exit`), and the server sends back the appropriate message. In real-world applications, protocols are often more complex and may be based on formats like `JSON` or binary structures.

```csharp
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Server;

public static class Program
{
    const int Port = 5000;
    public static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
        using var listener = new TcpListener(ipEndPoint);
        
        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Wait any key to exit");
            Console.ReadKey(true);
            cts.Cancel();
        });

        await AcceptClients(listener, cts.Token);
        await waitForAnyKey;
    }

    private static async Task AcceptClients(TcpListener listener, CancellationToken token = default)
    {
        listener.Start(backlog: 10);
        var clients = new List<Task>();
        while (true)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                clients.Add(HandleClient(client, token));
                clients.RemoveAll(task => task.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        listener.Stop();
        await Task.WhenAll(clients);
    }

    private static async Task HandleClient(TcpClient client, CancellationToken token = default)
    {
        Console.WriteLine("New client connected");
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };
            while (await reader.ReadLineAsync(token) is { } command)
            {
                Console.WriteLine($"Received command: {command}");
                switch (command)
                {
                    case "date":
                        string message = DateTime.Now.ToString("d", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "time":
                        message = DateTime.Now.ToString("t", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "exit":
                        return;
                    default:
                        await writer.WriteLineAsync("Unknown command".ToCharArray(), token);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, which is expected during shutdown.
        }
        catch (IOException)
        {
            // Client disconnected abruptly.
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling client: {e.Message}");
        }
        finally
        {
            client.Dispose();
            Console.WriteLine("Client disconnected");
        }
    }
}
```

## Client

The **TCP client** initiates a connection with the server. It uses the `TcpClient` class and its `ConnectAsync` method, providing the server's IP address and port. After connecting, similar to the server, it uses a stream wrapped in a `StreamReader` and `StreamWriter` to send commands and receive responses.

```csharp
using System.Net;
using System.Net.Sockets;

namespace Client;

public static class Program
{
    const int Port = 5000;
    public static async Task Main()
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
        using var client = new TcpClient();
        await client.ConnectAsync(ipEndPoint);
        Console.WriteLine("Client connected");
        
        var reader = new StreamReader(client.GetStream());
        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

        while (Console.ReadLine() is { } line)
        {
            await writer.WriteLineAsync(line);
            var response = await reader.ReadLineAsync();
            if (response is null) break;
            Console.WriteLine(response);
        }
    }
}
```

> [!INFO]
> ## Source Code
> {{< filetree dir="lectures/ipc/Networking" >}}