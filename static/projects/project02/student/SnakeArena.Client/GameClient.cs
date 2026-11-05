namespace SnakeArena.Client;

/// <summary>
/// A connection to the game server driven by the keyboard (or by <see cref="SnakeBot"/> in bot mode).
/// </summary>
public sealed class GameClient
{
    private readonly ClientOptions _options;

    public GameClient(ClientOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Connects to the server, joins the game and plays until the user quits, the server closes
    /// the connection, or <paramref name="cancellationToken"/> is cancelled (Ctrl+C).
    /// </summary>
    /// <returns>
    /// The process exit code: 0 = the user quit, 1 = the connection was lost or closed by the server,
    /// 2 = could not connect or the join request was rejected.
    /// </returns>
    public Task<int> RunAsync(CancellationToken cancellationToken)
    {
        // TODO (Part 3, Part 4)
        throw new NotImplementedException();
    }
}
