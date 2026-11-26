using SnakeArena.Shared.Game;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Tests;

// Integration tests of Part 2. They talk to a real GameServer over TCP on the loopback interface
// and therefore also depend on your Part 1 (Framing, MessageCodec).
public class ServerTests
{
    [Fact]
    public async Task Join_IsAcceptedWithBoardParameters()
    {
        await using var server = TestServer.Start();

        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;

        Assert.NotEqual(0, accepted.PlayerId.Value);
        Assert.Equal(30, accepted.Width);
        Assert.Equal(16, accepted.Height);
        Assert.Equal(20, accepted.TickIntervalMs);
        Assert.NotEqual(0u, accepted.SessionToken);
    }

    [Fact]
    public async Task Join_PlayersGetDistinctIds()
    {
        await using var server = TestServer.Start();

        var (alice, a) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = alice;
        var (bob, b) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var __ = bob;

        Assert.NotEqual(a.PlayerId, b.PlayerId);
    }

    [Fact]
    public async Task Join_WrongVersion_IsRejectedAndConnectionClosed()
    {
        await using var server = TestServer.Start();
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.SendAsync(new JoinRequest(ProtocolConstants.Version + 1, "Alice"));

        var rejected = Assert.IsType<JoinRejected>(await client.ReceiveAsync());
        Assert.Equal(RejectReason.VersionMismatch, rejected.Reason);
        Assert.Empty(await client.ExpectClosedAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("two words")]
    [InlineData("seventeen_chars_x")]
    [InlineData("*")]
    [InlineData("Zażółć")]
    public async Task Join_InvalidName_IsRejected(string name)
    {
        await using var server = TestServer.Start();
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.SendAsync(new JoinRequest(ProtocolConstants.Version, name));

        var rejected = Assert.IsType<JoinRejected>(await client.ReceiveAsync());
        Assert.Equal(RejectReason.InvalidName, rejected.Reason);
        Assert.Empty(await client.ExpectClosedAsync());
    }

    [Fact]
    public async Task Join_NameTaken_IsRejected_CaseInsensitive()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice-1");
        await using var _a = alice;
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.SendAsync(new JoinRequest(ProtocolConstants.Version, "aLiCe-1"));

        var rejected = Assert.IsType<JoinRejected>(await client.ReceiveAsync());
        Assert.Equal(RejectReason.NameTaken, rejected.Reason);
    }

    [Fact]
    public async Task Join_ServerFull_IsRejected()
    {
        await using var server = TestServer.Start(TestServer.DefaultOptions with { MaxPlayers = 2 });
        var (a, _) = await TestClient.JoinAsync(server.EndPoint, "a");
        await using var _a = a;
        var (b, _) = await TestClient.JoinAsync(server.EndPoint, "b");
        await using var _b = b;
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.SendAsync(new JoinRequest(ProtocolConstants.Version, "c"));

        var rejected = Assert.IsType<JoinRejected>(await client.ReceiveAsync());
        Assert.Equal(RejectReason.ServerFull, rejected.Reason);
    }

    [Fact]
    public async Task Join_NameIsFreeAgainAfterPlayerLeft()
    {
        await using var server = TestServer.Start();
        var (first, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await first.SendAsync(new LeaveRequest());
        await first.ExpectClosedAsync();
        await first.DisposeAsync();

        var (second, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await second.DisposeAsync();
    }

    [Fact]
    public async Task FirstMessageOtherThanJoin_ClosesConnection()
    {
        await using var server = TestServer.Start();
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.SendAsync(new ChatSend("hello?"));

        var messages = await client.ExpectClosedAsync();
        Assert.All(messages, m => Assert.IsType<ServerGoodbye>(m));
    }

    [Fact]
    public async Task NoJoinWithinHandshakeTimeout_ClosesConnection()
    {
        await using var server = TestServer.Start(); // HandshakeTimeout = 1 s
        await using var client = await TestClient.ConnectAsync(server.EndPoint);

        await client.ExpectClosedAsync(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task Snapshots_ArriveAtTickRateWithIncreasingTicks()
    {
        await using var server = TestServer.Start(); // 20 ms per tick
        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;

        var start = DateTime.UtcNow;
        var ticks = new List<uint>();
        while (ticks.Count < 20)
        {
            var snapshot = await client.ExpectAsync<StateSnapshot>();
            ticks.Add(snapshot.Tick);
            Assert.Contains(snapshot.Players, p => p.Id == accepted.PlayerId && p.Name == "Alice");
        }
        var elapsed = DateTime.UtcNow - start;

        Assert.True(ticks.Zip(ticks.Skip(1)).All(pair => pair.First < pair.Second), "Ticks must increase.");
        Assert.InRange(elapsed, TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Snapshots_ContainAllPlayers()
    {
        await using var server = TestServer.Start();
        var (alice, a) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, b) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var _b = bob;

        await alice.ExpectAsync<StateSnapshot>(s =>
            s.Players.Any(p => p.Id == a.PlayerId && p.Name == "Alice") &&
            s.Players.Any(p => p.Id == b.PlayerId && p.Name == "Bob"));
    }

    [Fact]
    public async Task Input_ChangesDirectionOfOwnSnake()
    {
        await using var server = TestServer.Start(TestServer.DefaultOptions with { Width = 100, Height = 50 });
        var (client, accepted) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _ = client;

        // Keep asking for a turn by 90 degrees until the server reports it (the snake may crash and respawn meanwhile).
        Direction? requested = null;
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
        while (true)
        {
            var me = (await client.ExpectAsync<StateSnapshot>()).Players.Single(p => p.Id == accepted.PlayerId);
            if (me.IsAlive && me.Direction == requested)
                break;
            Assert.True(DateTime.UtcNow < deadline, "The snake did not turn.");
            if (me.IsAlive)
            {
                requested = (Direction)(((int)me.Direction + 1) % 4);
                await client.SendAsync(new PlayerInput(requested.Value));
            }
        }
    }

    [Fact]
    public async Task Chat_IsBroadcastToAllPlayers()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, _) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var _b = bob;

        await alice.SendAsync(new ChatSend("Hello, Bob!"));

        await bob.ExpectAsync<ChatBroadcast>(c => c is { Sender: "Alice", Text: "Hello, Bob!" });
        await alice.ExpectAsync<ChatBroadcast>(c => c is { Sender: "Alice", Text: "Hello, Bob!" });
    }

    [Fact]
    public async Task Chat_IsTrimmedAndTruncated()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;

        await alice.SendAsync(new ChatSend("   "));
        await alice.SendAsync(new ChatSend("  hi  "));
        await alice.SendAsync(new ChatSend(new string('x', 300)));

        var first = await alice.ExpectAsync<ChatBroadcast>(c => c.Sender == "Alice");
        var second = await alice.ExpectAsync<ChatBroadcast>(c => c.Sender == "Alice");
        Assert.Equal("hi", first.Text);
        Assert.Equal(new string('x', ProtocolConstants.MaxChatLength), second.Text);
    }

    [Fact]
    public async Task ServerAnnouncesJoinAndLeave()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;

        var (bob, _) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await alice.ExpectAsync<ChatBroadcast>(c => c.Sender == ProtocolConstants.ServerSender && c.Text.Contains("Bob"));
        await bob.DisposeAsync();
        await alice.ExpectAsync<ChatBroadcast>(c => c.Sender == ProtocolConstants.ServerSender && c.Text.Contains("Bob"));
    }

    [Fact]
    public async Task Leave_RemovesPlayerAndClosesConnection()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, b) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var _b = bob;
        await alice.ExpectAsync<StateSnapshot>(s => s.Players.Any(p => p.Id == b.PlayerId));

        await bob.SendAsync(new LeaveRequest());

        await bob.ExpectClosedAsync();
        await alice.ExpectAsync<StateSnapshot>(s => s.Players.All(p => p.Id != b.PlayerId));
    }

    [Fact]
    public async Task Disconnect_RemovesPlayer()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, b) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await alice.ExpectAsync<StateSnapshot>(s => s.Players.Any(p => p.Id == b.PlayerId));

        await bob.DisposeAsync(); // no LeaveRequest, the connection just disappears

        await alice.ExpectAsync<StateSnapshot>(s => s.Players.All(p => p.Id != b.PlayerId));
    }

    [Fact]
    public async Task ProtocolError_DisconnectsOnlyTheOffendingClient()
    {
        await using var server = TestServer.Start();
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, _) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var _b = bob;

        await bob.SendRawAsync([0x7F, 0x01, 0x02]); // unknown message type

        await bob.ExpectClosedAsync();
        var last = await alice.ExpectAsync<StateSnapshot>();
        await alice.ExpectAsync<StateSnapshot>(s => s.Tick >= last.Tick + 10);
    }

    [Fact]
    public async Task Shutdown_SaysGoodbyeAndClosesAllConnections()
    {
        var server = TestServer.Start();
        await using var _ = server;
        var (alice, _) = await TestClient.JoinAsync(server.EndPoint, "Alice");
        await using var _a = alice;
        var (bob, _) = await TestClient.JoinAsync(server.EndPoint, "Bob");
        await using var _b = bob;
        await using var idle = await TestClient.ConnectAsync(server.EndPoint); // connected, not joined yet

        await server.StopAsync(TimeSpan.FromSeconds(5));

        Assert.Contains(await alice.ExpectClosedAsync(), m => m is ServerGoodbye);
        Assert.Contains(await bob.ExpectClosedAsync(), m => m is ServerGoodbye);
        await idle.ExpectClosedAsync();
    }
}
