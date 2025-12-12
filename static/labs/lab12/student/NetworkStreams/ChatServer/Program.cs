using System.Net;
using System.Net.Sockets;
using Chat.Common;
using Chat.Common.MessageHandlers;

namespace ChatServer;


class Program
{
    public static async Task Main()
    {
        const int port = 5000;
        using var listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        Console.WriteLine($"Server running on port {port} ...");

        Console.WriteLine("Waiting for a client...");
        using var client1 = await listener.AcceptTcpClientAsync();

        Console.WriteLine("Waiting for a second client...");
        using var client2 = await listener.AcceptTcpClientAsync();
        Console.WriteLine("Both clients connected - starting forwarding messages.");

        await HandleClientsAsync(client1, client2);
    }


    static async Task HandleClientsAsync(TcpClient client1, TcpClient client2)
    {
        await using var stream1 = client1.GetStream();
        await using var stream2 = client2.GetStream();

        using var cts = new CancellationTokenSource();

        using var readerFromClient1 = new MessageReader(stream1);
        using var readerFromClient2 = new MessageReader(stream2);

        using var writerToClient1 = new MessageWriter(stream1);
        using var writerToClient2 = new MessageWriter(stream2);

        var t1 = ForwardMessagesAsync(readerFromClient1, writerToClient2, cts.Token);
        var t2 = ForwardMessagesAsync(readerFromClient2, writerToClient1, cts.Token);

        var first = await Task.WhenAny(t1, t2);

        // Send a server-side disconnection notification to the remaining client
        var disconnectNotification = new MessageDTO
        {
            Content = "PEER DISCONNECTED",
            Sender = "Server",
            Time = DateTime.UtcNow,
        };

        var remainingClientWriter = first == t1 ? writerToClient2 : writerToClient1;

        try
        {
            await remainingClientWriter.WriteMessage(disconnectNotification, cts.Token);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error while sending disconnection notification: {e.Message}");
        }

        if (!cts.IsCancellationRequested)
            await cts.CancelAsync();
        
        await Task.WhenAll(t1, t2);
        
        Console.WriteLine("Clients disconnected, pair handler finished.");
    }


    static async Task ForwardMessagesAsync(MessageReader reader, MessageWriter writer, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}