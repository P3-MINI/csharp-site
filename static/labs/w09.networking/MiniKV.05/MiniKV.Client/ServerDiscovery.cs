using System.Net;
using System.Net.Sockets;
using MiniKV.Protocol;

namespace MiniKV.Client;

public sealed record DiscoveredServer(IPEndPoint EndPoint, ServerAnnouncement Info);

public static class ServerDiscovery
{
    /// <summary>
    /// Sends a discovery probe to <paramref name="target"/> (e.g. the broadcast address)
    /// and collects all answers that arrive within <paramref name="timeout"/>.
    /// UDP is unreliable, so if nobody answers, the probe is sent again (up to <paramref name="attempts"/> times).
    /// </summary>
    public static async Task<List<DiscoveredServer>> DiscoverAsync(IPAddress target, TimeSpan timeout, int attempts = 3)
    {
        using var udp = new UdpClient(AddressFamily.InterNetwork) { EnableBroadcast = true };
        var probeTarget = new IPEndPoint(target, KvProtocol.DiscoveryPort);
        var found = new Dictionary<IPEndPoint, DiscoveredServer>(); // one server may answer several probes

        for (int attempt = 1; attempt <= attempts && found.Count == 0; attempt++)
        {
            try
            {
                await udp.SendAsync(Discovery.CreateProbe(), probeTarget);
            }
            catch (SocketException e)
            {
                // E.g. "Network is unreachable" when sending a broadcast without a network
                Console.WriteLine($"Cannot send a probe to {target}: {e.Message}");
                return [];
            }

            using var cts = new CancellationTokenSource(timeout);
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult answer = await udp.ReceiveAsync(cts.Token);
                    if (Discovery.TryDecode(answer.Buffer) is { } info)
                    {
                        // The server's address is the sender address of the datagram
                        var endPoint = new IPEndPoint(answer.RemoteEndPoint.Address, info.TcpPort);
                        found[endPoint] = new DiscoveredServer(endPoint, info);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout - stop listening for answers
                }
                catch (SocketException)
                {
                    // Windows reports an ICMP "port unreachable" (nobody listens on the target) here - ignore it
                }
            }
        }

        return [.. found.Values];
    }
}
