using SnakeArena.Shared.Game;

namespace SnakeArena.Shared.Protocol;

// Messages exchanged over the TCP connection. The wire format of every message is defined
// in the project specification (section "Protocol"); the numeric codes are in MessageType.

/// <summary>Message type codes (the first byte of every frame body).</summary>
public enum MessageType : byte
{
    // client -> server
    JoinRequest = 0x01,
    PlayerInput = 0x02,
    ChatSend = 0x03,
    LeaveRequest = 0x04,

    // server -> client
    JoinAccepted = 0x81,
    JoinRejected = 0x82,
    StateSnapshot = 0x83,
    ChatBroadcast = 0x84,
    ServerGoodbye = 0x85,
}

public enum RejectReason : byte
{
    VersionMismatch = 1,
    ServerFull = 2,
    InvalidName = 3,
    NameTaken = 4,
}

public abstract record Message;

// ---------------------------------------------------------------- client -> server

/// <summary>The first message sent by a client.</summary>
public sealed record JoinRequest(ushort ProtocolVersion, string Name) : Message;

/// <summary>Requests a change of the snake's direction.</summary>
public sealed record PlayerInput(Direction Direction) : Message;

/// <summary>A chat line typed by the player.</summary>
public sealed record ChatSend(string Text) : Message;

/// <summary>The player leaves the game; the server closes the connection.</summary>
public sealed record LeaveRequest : Message;

// ---------------------------------------------------------------- server -> client

/// <summary>Positive answer to <see cref="JoinRequest"/>.</summary>
public sealed record JoinAccepted(
    PlayerId PlayerId,
    byte Width,
    byte Height,
    ushort TickIntervalMs,
    uint SessionToken) : Message;

/// <summary>Negative answer to <see cref="JoinRequest"/>; the server closes the connection afterwards.</summary>
public sealed record JoinRejected(RejectReason Reason, string Details) : Message;

/// <summary>The complete state of the board after a tick.</summary>
public sealed record StateSnapshot(
    uint Tick,
    IReadOnlyList<PlayerSnapshot> Players,
    IReadOnlyList<Position> Food) : Message
{
    // Records compare collections by reference; compare them element by element instead.
    public bool Equals(StateSnapshot? other) =>
        other is not null
        && Tick == other.Tick
        && Players.SequenceEqual(other.Players)
        && Food.SequenceEqual(other.Food);

    public override int GetHashCode() => HashCode.Combine(Tick, Players.Count, Food.Count);
}

/// <summary>One player inside a <see cref="StateSnapshot"/>. <see cref="Body"/> is head first and empty for a dead snake.</summary>
public sealed record PlayerSnapshot(
    PlayerId Id,
    string Name,
    bool IsAlive,
    Direction Direction,
    ushort Score,
    IReadOnlyList<Position> Body)
{
    public bool Equals(PlayerSnapshot? other) =>
        other is not null
        && Id == other.Id
        && Name == other.Name
        && IsAlive == other.IsAlive
        && Direction == other.Direction
        && Score == other.Score
        && Body.SequenceEqual(other.Body);

    public override int GetHashCode() => HashCode.Combine(Id, Name, IsAlive, Direction, Score, Body.Count);
}

/// <summary>A chat line (or a server notice when <see cref="Sender"/> is <see cref="ProtocolConstants.ServerSender"/>).</summary>
public sealed record ChatBroadcast(string Sender, string Text) : Message;

/// <summary>Sent by the server right before it closes the connection (shutdown, slow client, protocol error...).</summary>
public sealed record ServerGoodbye(string Reason) : Message;
