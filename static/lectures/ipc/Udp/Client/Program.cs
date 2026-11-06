using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

public static class Program
{
    const int Port = 5000;
    const int MaxAttempts = 3;
    static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(1);

    public static async Task Main()
    {
        using var udp = new UdpClient();
        udp.Connect(IPAddress.Loopback, Port); // Only sets the default remote endpoint, nothing is sent
        Console.WriteLine($"Client bound to {udp.Client.LocalEndPoint}");

        while (Console.ReadLine() is { } line && line != "exit")
        {
            string? response = await SendRequest(udp, line);
            Console.WriteLine(response ?? "No response from the server");
        }
    }

    private static async Task<string?> SendRequest(UdpClient udp, string command)
    {
        byte[] request = Encoding.UTF8.GetBytes(command);
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            await udp.SendAsync(request);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(ResponseTimeout);
            try
            {
                UdpReceiveResult result = await udp.ReceiveAsync(cts.Token);
                return Encoding.UTF8.GetString(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Timeout (attempt {attempt}/{MaxAttempts})");
            }
            catch (SocketException e) when (e.SocketErrorCode is SocketError.ConnectionRefused or SocketError.ConnectionReset)
            {
                // ICMP "port unreachable" - nothing listens on the server port
                Console.WriteLine($"Server unreachable (attempt {attempt}/{MaxAttempts})");
                await Task.Delay(ResponseTimeout);
            }
        }
        return null;
    }
}
