using System.Net;

namespace SnakeArena.Server;

/// <summary>
/// The authoritative game server.
/// </summary>
/// <remarks>
/// Usage: <c>new GameServer(options).Start()</c> binds the sockets (so that <see cref="LocalEndPoint"/> is known),
/// then <see cref="RunAsync"/> serves clients until the token is cancelled.
/// </remarks>
public sealed class GameServer
{
    private readonly ServerOptions _options;
    private readonly TextWriter _log;

    public GameServer(ServerOptions options, TextWriter? log = null)
    {
        _options = options;
        _log = log ?? TextWriter.Null;
    }

    /// <summary>The TCP endpoint the server listens on. Valid after <see cref="Start"/>.</summary>
    public IPEndPoint LocalEndPoint => throw new NotImplementedException(); // TODO (Part 2)

    /// <summary>
    /// The UDP endpoint answering discovery probes, or <see langword="null"/> when discovery is disabled.
    /// Valid after <see cref="Start"/>.
    /// </summary>
    public IPEndPoint? DiscoveryEndPoint => throw new NotImplementedException(); // TODO (Part 4)

    /// <summary>
    /// Binds the TCP listener (and the UDP sockets in Part 4) without accepting clients yet.
    /// </summary>
    /// <exception cref="System.Net.Sockets.SocketException">A port is already in use.</exception>
    public void Start()
    {
        // TODO (Part 2, Part 4)
        throw new NotImplementedException();
    }

    /// <summary>
    /// Accepts clients and runs the game loop until <paramref name="cancellationToken"/> is cancelled,
    /// then shuts down gracefully. The returned task completes when all connections are closed.
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken)
    {
        // TODO (Part 2)
        throw new NotImplementedException();
    }
}
