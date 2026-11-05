using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using SnakeArena.Client;
using SnakeArena.Server;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

// Integration tests of Part 4 (they also depend on Parts 1 and 2).
public class UdpTests
{
    private static readonly ServerOptions DiscoverableServer = TestServer.DefaultOptions with
    {
        Name = "Lab 204",
        MaxPlayers = 5,
        EnableDiscovery = true,
        DiscoveryPort = 0,
    };

    private static async Task<Datagram?> ReceiveDatagramAsync(UdpClient udp, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            while (true)
            {
                var result = await udp.ReceiveAsync(cts.Token);
                if (DatagramCodec.TryDecode(result.Buffer, out var datagram))
                    return datagram;
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static IPEndPoint DiscoveryEndPoint(TestServer server)
    {
        var endPoint = server.Server.DiscoveryEndPoint;
        Assert.NotNull(endPoint);
        return new IPEndPoint(IPAddress.Loopback, endPoint.Port);
    }

    [Fact]
    public async Task DiscoveryEndPoint_IsNullWhenDiscoveryDisabled()
    {
        await using var server = TestServer.Start(); // EnableDiscovery = false

        Assert.Null(server.Server.DiscoveryEndPoint);
    }

    [Fact]
    public async Task Discovery_ServerAnswersProbe()
    {
        await using var server = TestServer.Start(DiscoverableServer);
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        await alice.ExpectAsync<StateSnapshot>(s => s.Players.Count == 1);

        // Garbage must be ignored without breaking the responder.
        await udp.SendAsync("hello"u8.ToArray(), DiscoveryEndPoint(server));
        await udp.SendAsync(DatagramCodec.Encode(new DiscoveryProbe(ProtocolConstants.Version, 0xC0FFEE)), DiscoveryEndPoint(server));

        var reply = Assert.IsType<DiscoveryReply>(await ReceiveDatagramAsync(udp, TimeSpan.FromSeconds(2)));
        Assert.Equal(ProtocolConstants.Version, reply.ProtocolVersion);
        Assert.Equal(0xC0FFEEu, reply.Nonce);
        Assert.Equal("Lab 204", reply.ServerName);
        Assert.Equal(server.Server.LocalEndPoint.Port, reply.TcpPort);
        Assert.Equal(1, reply.PlayerCount);
        Assert.Equal(5, reply.MaxPlayers);
    }

    [Fact]
    public async Task LanDiscovery_FindsLocalServer()
    {
        await using var server = TestServer.Start(DiscoverableServer);

        var servers = await LanDiscovery.FindServersAsync([DiscoveryEndPoint(server)], TimeSpan.FromMilliseconds(500));

        var found = Assert.Single(servers);
        Assert.Equal("Lab 204", found.Name);
        Assert.Equal(IPAddress.Loopback, found.EndPoint.Address);
        Assert.Equal(server.Server.LocalEndPoint.Port, found.EndPoint.Port);
        Assert.Equal(0, found.PlayerCount);
        Assert.Equal(5, found.MaxPlayers);
    }

    [Fact]
    public async Task LanDiscovery_NoServer_ReturnsEmptyAfterTimeout()
    {
        int unusedPort;
        using (var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
            unusedPort = ((IPEndPoint)probe.Client.LocalEndPoint!).Port;

        var stopwatch = Stopwatch.StartNew();
        var servers = await LanDiscovery.FindServersAsync([new IPEndPoint(IPAddress.Loopback, unusedPort)], TimeSpan.FromMilliseconds(500));

        Assert.Empty(servers);
        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(400), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task UdpHello_SwitchesSnapshotsToUdp()
    {
        await using var server = TestServer.Start();
        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var gamePort = new IPEndPoint(IPAddress.Loopback, server.Server.LocalEndPoint.Port);

        SnapshotDatagram? first = null;
        for (int attempt = 0; attempt < 10 && first is null; attempt++)
        {
            await udp.SendAsync(DatagramCodec.Encode(new UdpHello(accepted.PlayerId, accepted.SessionToken)), gamePort);
            first = await ReceiveDatagramAsync(udp, TimeSpan.FromMilliseconds(200)) as SnapshotDatagram;
        }

        Assert.NotNull(first);
        Assert.Equal(accepted.SessionToken, first.SessionToken);
        Assert.Contains(first.Snapshot.Players, p => p.Id == accepted.PlayerId);

        // UDP snapshots keep coming...
        var next = Assert.IsType<SnapshotDatagram>(await ReceiveDatagramAsync(udp, TimeSpan.FromSeconds(1)));
        Assert.True(next.Snapshot.Tick > first.Snapshot.Tick);

        // ...and the TCP connection carries no newer snapshots (only those sent before the switch).
        var stopAt = DateTime.UtcNow + TimeSpan.FromMilliseconds(300);
        while (DateTime.UtcNow < stopAt)
        {
            Message? message;
            try
            {
                message = await client.ReceiveAsync(TimeSpan.FromMilliseconds(100));
            }
            catch (TimeoutException)
            {
                continue;
            }
            Assert.NotNull(message);
            if (message is StateSnapshot snapshot)
                Assert.True(snapshot.Tick < first.Snapshot.Tick, $"Tick {snapshot.Tick} was sent over TCP after the switch to UDP.");
        }
    }

    [Fact]
    public async Task NoMoreUdpHellos_ServerFallsBackToTcp()
    {
        await using var server = TestServer.Start();
        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var gamePort = new IPEndPoint(IPAddress.Loopback, server.Server.LocalEndPoint.Port);

        // A single hello that is never refreshed.
        await udp.SendAsync(DatagramCodec.Encode(new UdpHello(accepted.PlayerId, accepted.SessionToken)), gamePort);
        var first = Assert.IsType<SnapshotDatagram>(await ReceiveDatagramAsync(udp, TimeSpan.FromSeconds(1)));
        var stopwatch = Stopwatch.StartNew();

        await client.ExpectAsync<StateSnapshot>(
            s => s.Tick > first.Snapshot.Tick,
            ProtocolConstants.UdpTimeout + TimeSpan.FromSeconds(2));

        Assert.True(stopwatch.Elapsed > ProtocolConstants.UdpTimeout - TimeSpan.FromMilliseconds(500),
            $"The server returned to TCP after {stopwatch.Elapsed.TotalMilliseconds:0} ms, too early.");
    }

    [Fact]
    public async Task UdpHello_WithWrongToken_IsIgnored()
    {
        await using var server = TestServer.Start();
        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var gamePort = new IPEndPoint(IPAddress.Loopback, server.Server.LocalEndPoint.Port);

        await udp.SendAsync(DatagramCodec.Encode(new UdpHello(accepted.PlayerId, accepted.SessionToken + 1)), gamePort);

        Assert.Null(await ReceiveDatagramAsync(udp, TimeSpan.FromMilliseconds(500)));
        var last = await client.ExpectAsync<StateSnapshot>();
        await client.ExpectAsync<StateSnapshot>(s => s.Tick > last.Tick);
    }
}
