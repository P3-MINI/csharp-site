using MiniKV.Protocol;

namespace MiniKV.Client;

public static class ResponseFormatter
{
    public static string Format(Response response) => response switch
    {
        { Status: Status.Ok, Keys: { Count: 0 } } => "(empty)",
        { Status: Status.Ok, Keys: { } keys } => string.Join(Environment.NewLine, keys.Select((key, i) => $"{i + 1}) {key}")),
        { Status: Status.Ok, Value: { } value } => $"\"{value}\"",
        { Status: Status.Ok } => "OK",
        { Status: Status.NotFound } => "(not found)",
        _ => $"(error) {response.Error}",
    };
}
