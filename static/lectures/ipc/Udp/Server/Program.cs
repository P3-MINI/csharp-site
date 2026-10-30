using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public static class Program
{
    const int Port = 5000;
    const double LossRate = 0.25; // Probability of a simulated request loss

    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Any, Port));

        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
            cts.Cancel();
        });

        await HandleRequests(udp, cts.Token);
        await waitForAnyKey;
    }

    private static async Task HandleRequests(UdpClient udp, CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult request = await udp.ReceiveAsync(token);
                string command = Encoding.UTF8.GetString(request.Buffer);
                Console.WriteLine($"Received command: {command} from {request.RemoteEndPoint}");

                if (Random.Shared.NextDouble() < LossRate)
                {
                    Console.WriteLine("Request lost (simulated)");
                    continue;
                }

                string response = command switch
                {
                    "date" => DateTime.Now.ToString("d", CultureInfo.InvariantCulture),
                    "time" => DateTime.Now.ToString("t", CultureInfo.InvariantCulture),
                    _ => "Unknown command"
                };
                await udp.SendAsync(Encoding.UTF8.GetBytes(response), request.RemoteEndPoint, token);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, which is expected during shutdown.
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Windows: a previous response was rejected by its recipient (ICMP port unreachable).
            }
        }
    }
}
