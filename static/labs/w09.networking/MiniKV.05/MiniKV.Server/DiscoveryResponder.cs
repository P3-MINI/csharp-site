using System.Net;
using System.Net.Sockets;
using MiniKV.Protocol;

namespace MiniKV.Server;

/// <summary>
/// Answers UDP discovery probes, so that clients can find the server in the local network.
/// </summary>
public sealed class DiscoveryResponder(string name, int tcpPort, KeyValueStore store)
{
    public async Task RunAsync(CancellationToken ct)
    {
        UdpClient udp;
        try
        {
            udp = new UdpClient(new IPEndPoint(IPAddress.Any, KvProtocol.DiscoveryPort));
        }
        catch (SocketException e)
        {
            // E.g. another server on this machine already uses the port - TCP still works
            Console.WriteLine($"Discovery disabled: {e.Message}");
            return;
        }

        using (udp)
        {
            Console.WriteLine($"Answering discovery probes on UDP port {KvProtocol.DiscoveryPort} as '{name}'");
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult probe = await udp.ReceiveAsync(ct);
                    if (!Discovery.IsProbe(probe.Buffer))
                        continue; // not a MiniKV probe - ignore it

                    Console.WriteLine($"[udp] probe from {probe.RemoteEndPoint}");
                    byte[] reply = Discovery.Encode(new ServerAnnouncement(name, tcpPort, store.Count));
                    await udp.SendAsync(reply, probe.RemoteEndPoint, ct);
                }
                catch (OperationCanceledException)
                {
                    // Ctrl+C
                }
                catch (SocketException e)
                {
                    // One bad datagram must not stop the responder
                    // (e.g. Windows reports an ICMP "port unreachable" from an earlier reply here)
                    Console.WriteLine($"[udp] {e.Message}");
                }
            }
        }
    }
}
