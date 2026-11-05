namespace SnakeArena.Shared.Game;

/// <summary>
/// Movement direction of a snake. The numeric values are part of the wire protocol.
/// </summary>
public enum Direction : byte
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
}

public static class DirectionExtensions
{
    /// <summary>Returns the direction pointing the other way (Up &lt;-&gt; Down, Left &lt;-&gt; Right).</summary>
    public static Direction Opposite(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Right => Direction.Left,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };
}
