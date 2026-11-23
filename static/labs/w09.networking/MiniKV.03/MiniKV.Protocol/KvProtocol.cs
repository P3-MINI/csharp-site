using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniKV.Protocol;

/// <summary>
/// Settings shared by the server and the client.
/// </summary>
public static class KvProtocol
{
    public const int DefaultPort = 5555;

    /// <summary>Largest accepted message payload (64 KiB).</summary>
    public const int MaxMessageSize = 64 * 1024;

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        // "Key" -> "key"
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Status.NotFound -> "notFound" instead of 1
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        // Do not send "value": null, "keys": null, ...
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Reject messages with missing or null non-nullable members, e.g. {"type":"get"}
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public static string Serialize(Message message) =>
        JsonSerializer.Serialize(message, JsonOptions);

    public static Message? Deserialize(string json) =>
        JsonSerializer.Deserialize<Message>(json, JsonOptions);
}
