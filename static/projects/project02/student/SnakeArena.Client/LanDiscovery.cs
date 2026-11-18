using System.Net;

namespace SnakeArena.Client;

/// <summary>A server that answered a discovery probe. <see cref="EndPoint"/> is its TCP endpoint.</summary>
public sealed record DiscoveredServer(IPEndPoint EndPoint, string Name, int PlayerCount, int MaxPlayers)
{
    public override string ToString() => $"{Name} at {EndPoint} ({PlayerCount}/{MaxPlayers} players)";
}

public static class LanDiscovery
{
    /// <summary>
    /// Sends one discovery probe to each of <paramref name="targets"/> and collects the replies
    /// that arrive within <paramref name="timeout"/>.
    /// </summary>
    public static Task<IReadOnlyList<DiscoveredServer>> FindServersAsync(
        IReadOnlyList<IPEndPoint> targets,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        // TODO (Part 4)
        throw new NotImplementedException();
    }
}
