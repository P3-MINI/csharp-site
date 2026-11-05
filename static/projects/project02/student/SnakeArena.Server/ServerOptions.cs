using SnakeArena.Shared.Protocol;

namespace SnakeArena.Server;

public sealed record ServerOptions
{
    public const string Usage =
        """
        Usage: SnakeArena.Server [options]
          --help                show this help
          --port <n>            TCP (and UDP game) port, 0 = any free port     (default 47000)
          --name <text>         server name shown in LAN discovery             (default: machine name)
          --width <n>           board width  (10-100)                          (default 30)
          --height <n>          board height (8-50)                            (default 16)
          --tick <ms>           tick interval in milliseconds (20-1000)        (default 100)
          --max-players <n>     maximum number of players (1-32)               (default 8)
          --seed <n>            seed of the game's random number generator     (default: random)
          --discovery-port <n>  UDP port for LAN discovery, 0 = any free port  (default 47001)
          --no-discovery        do not answer LAN discovery probes
        """;

    /// <summary>TCP port to listen on; the UDP game socket binds the same port number. 0 = any free port.</summary>
    public int Port { get; init; } = ProtocolConstants.DefaultPort;

    public string Name { get; init; } = Environment.MachineName;
    public int Width { get; init; } = 30;
    public int Height { get; init; } = 16;
    public int TickIntervalMs { get; init; } = 100;
    public int MaxPlayers { get; init; } = 8;
    public int? Seed { get; init; }

    public bool EnableDiscovery { get; init; } = true;

    /// <summary>UDP port for LAN discovery. 0 = any free port (used by tests).</summary>
    public int DiscoveryPort { get; init; } = ProtocolConstants.DiscoveryPort;

    /// <summary>A client that does not send <see cref="JoinRequest"/> within this time is disconnected.</summary>
    public TimeSpan HandshakeTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>Maximum number of frames waiting to be sent to one client; a client exceeding it is too slow.</summary>
    public int SendQueueCapacity { get; init; } = 64;

    /// <exception cref="ArgumentException">Unknown option or invalid value.</exception>
    public static ServerOptions Parse(string[] args)
    {
        var options = new ServerOptions();
        for (int i = 0; i < args.Length; i++)
        {
            options = args[i] switch
            {
                "--port" => options with { Port = ParseInt(args, ref i, 0, 65535) },
                "--name" => options with { Name = NextValue(args, ref i) },
                "--width" => options with { Width = ParseInt(args, ref i, 10, 100) },
                "--height" => options with { Height = ParseInt(args, ref i, 8, 50) },
                "--tick" => options with { TickIntervalMs = ParseInt(args, ref i, 20, 1000) },
                "--max-players" => options with { MaxPlayers = ParseInt(args, ref i, 1, 32) },
                "--seed" => options with { Seed = ParseInt(args, ref i, int.MinValue, int.MaxValue) },
                "--discovery-port" => options with { DiscoveryPort = ParseInt(args, ref i, 0, 65535) },
                "--no-discovery" => options with { EnableDiscovery = false },
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

    private static int ParseInt(string[] args, ref int i, int min, int max)
    {
        string option = args[i];
        string value = NextValue(args, ref i);
        if (!int.TryParse(value, out int result) || result < min || result > max)
            throw new ArgumentException($"Option '{option}' expects an integer in range {min}..{max}, got '{value}'.");
        return result;
    }
}
