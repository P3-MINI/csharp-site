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