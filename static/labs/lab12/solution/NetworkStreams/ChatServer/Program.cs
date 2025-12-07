using System.Net;
using System.Net.Sockets;
using Chat.Common;
using Chat.Common.MessageHandlers;


class Program
{
    public static async Task Main()
    {
        const int port = 5000;
        using var listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        Console.WriteLine($"Server running on port {port} ...");

        Console.WriteLine("Waiting for a client...");
        var client1 = await listener.AcceptTcpClientAsync();

        Console.WriteLine("Waiting for a second client...");
        var client2 = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Both clients connected - starting forwarding messages.");

        await HandleClientsAsync(client1, client2);
    }


    static async Task HandleClientsAsync(TcpClient client1, TcpClient client2)
    {
        using var stream1 = client1.GetStream();
        using var stream2 = client2.GetStream();

        using var cts = new CancellationTokenSource();

        using var readerFromClient1 = new MessageReader(stream1);
        using var readerFromClient2 = new MessageReader(stream2);

        using var writerToClient1 = new MessageWriter(stream1);
        using var writerToCLient2 = new MessageWriter(stream2);

        var t1 = ForwardMessagesAsync(readerFromClient1, writerToCLient2, cts.Token);
        var t2 = ForwardMessagesAsync(readerFromClient2, writerToClient1, cts.Token);

        var first = await Task.WhenAny(t1, t2);

        // Send a server-side disconnection notification to the remaining client
        var disconnectNotification = new MessageDTO
        {
            Content = "PEER DISCONNECTED",
            Sender = "Server",
            Time = DateTime.UtcNow,
        };

        if (first == t1)
        {
            // client1 finished -> notify client2
            try { await writerToCLient2.WriteMessage(disconnectNotification, cts.Token); } catch { }
        }
        else
        {
            // client2 finished -> notify client1
            try { await writerToClient1.WriteMessage(disconnectNotification, cts.Token); } catch { }
        }

        // when one side finishes (disconnect or exit), cancel the other
        cts.Cancel();

        try
        {
            await Task.WhenAll(t1, t2);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine("Relay error: " + ex.Message);
        }

        try { client1.Close(); } catch { }
        try { client2.Close(); } catch { }
        Console.WriteLine("Clients disconnected, pair handler finished.");
    }


    static async Task ForwardMessagesAsync(MessageReader reader, MessageWriter writer, CancellationToken ct)
    {
        try {
            while (!ct.IsCancellationRequested)
            {
                var msg = await reader.ReadMessage(ct);
                if (msg == null)    // client disconnected
                    break;

                Console.WriteLine($"{msg.Sender}: {msg.Content}");

                await writer.WriteMessage(msg, ct);
            }        
        }
        catch (OperationCanceledException) { }  // Ignore exception       }
    }
}