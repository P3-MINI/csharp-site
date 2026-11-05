namespace SnakeArena.Shared.Game;

/// <summary>
/// Identifier of a player assigned by the server. Valid identifiers are 1..255 (0 is never used).
/// </summary>
public readonly record struct PlayerId(byte Value) : IComparable<PlayerId>
{
    public int CompareTo(PlayerId other) => Value.CompareTo(other.Value);

    public override string ToString() => $"#{Value}";
}
