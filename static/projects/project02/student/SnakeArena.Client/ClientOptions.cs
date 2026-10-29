using SnakeArena.Shared.Protocol;

namespace SnakeArena.Client;

public sealed record ClientOptions
{
    public const string Usage =
        """
        Usage: SnakeArena.Client [options]
          --help                show this help
          --host <address>      server address or host name                     (default 127.0.0.1)
          --port <n>            server TCP port                                 (default 47000)
          --name <text>         player name (1-16 letters, digits, '_' or '-')  (default: random)
          --discover            look for servers in the LAN and pick one (replaces --host/--port)
          --discovery-port <n>  UDP port used for LAN discovery                 (default 47001)
          --bot                 the built-in bot plays: no keyboard, no board, status lines only
          --tcp-only            receive snapshots over TCP only (do not use UDP)
          --stall-after <s>     debugging: stop reading from the TCP connection after <s> seconds
                                (simulates a client that is too slow; use with --tcp-only)
        """;

    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = ProtocolConstants.DefaultPort;
    public string Name { get; init; } = $"player{Random.Shared.Next(100, 1000)}";
    public bool Discover { get; init; }
    public int DiscoveryPort { get; init; } = ProtocolConstants.DiscoveryPort;
    public bool Bot { get; init; }
    public bool TcpOnly { get; init; }

    /// <summary>Debugging: stop reading from the TCP connection after this time (null = never).</summary>
    public TimeSpan? StallAfter { get; init; }

    /// <exception cref="ArgumentException">Unknown option or invalid value.</exception>
    public static ClientOptions Parse(string[] args)
    {
        var options = new ClientOptions();
        for (int i = 0; i < args.Length; i++)
        {
            options = args[i] switch
            {
                "--host" => options with { Host = NextValue(args, ref i) },
                "--port" => options with { Port = ParsePort(args, ref i) },
                "--name" => options with { Name = NextValue(args, ref i) },
                "--discover" => options with { Discover = true },
                "--discovery-port" => options with { DiscoveryPort = ParsePort(args, ref i) },
                "--bot" => options with { Bot = true },
                "--tcp-only" => options with { TcpOnly = true },
                "--stall-after" => options with { StallAfter = TimeSpan.FromSeconds(ParseSeconds(args, ref i)) },
                _ => throw new ArgumentException($"Unknown option '{args[i]}'."),
            };
        }
        return options;
    }

    private static string NextValue(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option '{args[i]}' requires a value.");
        return args[++i];
    }

    private static int ParseSeconds(string[] args, ref int i)
    {
        string option = args[i];
        string value = NextValue(args, ref i);
        if (!int.TryParse(value, out int seconds) || seconds < 0)
            throw new ArgumentException($"Option '{option}' expects a number of seconds, got '{value}'.");
        return seconds;
    }

    private static int ParsePort(string[] args, ref int i)
    {
        string option = args[i];
        string value = NextValue(args, ref i);
        if (!int.TryParse(value, out int port) || port is < 1 or > 65535)
            throw new ArgumentException($"Option '{option}' expects a port number (1-65535), got '{value}'.");
        return port;
    }
}
