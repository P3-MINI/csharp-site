using MiniKV.Protocol;

namespace MiniKV.Client;

public static class CommandParser
{
    public const string Usage = "Commands: get <key> | set <key> <value> | del <key> | keys | watch <key> | unwatch <key>";

    /// <summary>
    /// Turns a line like "set user:1 Ala" into a request.
    /// </summary>
    /// <exception cref="FormatException">The line is not a valid command.</exception>
    public static Request Parse(string line)
    {
        // At most 3 parts, so that the value may contain spaces: "set greeting Hello, world!"
        string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["get", var key] => new GetRequest(key),
            ["set", var key, var value] => new SetRequest(key, value),
            ["del", var key] => new DeleteRequest(key),
            ["keys"] => new KeysRequest(),
            ["watch", var key] => new WatchRequest(key),
            ["unwatch", var key] => new UnwatchRequest(key),
            _ => throw new FormatException($"Invalid command: '{line}'. {Usage}"),
        };
    }
}
