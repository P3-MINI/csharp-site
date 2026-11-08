using System.Text.Json.Serialization;

namespace MiniKV.Protocol;

/// <summary>
/// Base type of everything sent over the wire.
/// The "type" property in JSON tells the deserializer which derived record to create.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GetRequest), "get")]
[JsonDerivedType(typeof(SetRequest), "set")]
[JsonDerivedType(typeof(DeleteRequest), "del")]
[JsonDerivedType(typeof(KeysRequest), "keys")]
[JsonDerivedType(typeof(Response), "response")]
public abstract record Message;

// Client -> server

public abstract record Request : Message;

public sealed record GetRequest(string Key) : Request;

public sealed record SetRequest(string Key, string Value) : Request;

public sealed record DeleteRequest(string Key) : Request;

public sealed record KeysRequest : Request;

// Server -> client

public enum Status
{
    Ok,
    NotFound,
    Error,
}

public sealed record Response(
    Status Status,
    string? Value = null,
    IReadOnlyList<string>? Keys = null,
    string? Error = null) : Message
{
    public static Response Ok(string? value = null) => new(Status.Ok, value);

    public static Response WithKeys(IReadOnlyList<string> keys) => new(Status.Ok, Keys: keys);

    public static Response NotFound() => new(Status.NotFound);

    public static Response Fail(string error) => new(Status.Error, Error: error);
}
