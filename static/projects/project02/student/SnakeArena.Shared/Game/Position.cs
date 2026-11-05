namespace SnakeArena.Shared.Game;

/// <summary>
/// A cell of the board. (0, 0) is the top-left corner, X grows to the right and Y grows downwards.
/// </summary>
public readonly record struct Position(int X, int Y)
{
    /// <summary>Returns the neighbouring cell in the given direction (it may lie outside the board).</summary>
    public Position Move(Direction direction) => direction switch
    {
        Direction.Up => new Position(X, Y - 1),
        Direction.Right => new Position(X + 1, Y),
        Direction.Down => new Position(X, Y + 1),
        Direction.Left => new Position(X - 1, Y),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };

    public override string ToString() => $"({X},{Y})";
}
