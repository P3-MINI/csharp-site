namespace SnakeArena.Shared.Game;

/// <summary>
/// A player's snake as seen by the game rules. Only <see cref="GameState"/> modifies it.
/// </summary>
public sealed class Snake
{
    // Head first.
    internal readonly List<Position> Segments = [];

    internal Snake(PlayerId id, string name)
    {
        Id = id;
        Name = name;
    }

    public PlayerId Id { get; }
    public string Name { get; }
    public bool IsAlive { get; internal set; }
    public Direction Direction { get; internal set; }

    /// <summary>Number of food items eaten since the player joined (it is kept after a crash).</summary>
    public int Score { get; internal set; }

    /// <summary>Ticks left until the next respawn attempt (only meaningful when the snake is dead).</summary>
    public int RespawnIn { get; internal set; }

    /// <summary>Cells occupied by the snake, head first. Empty when the snake is dead.</summary>
    public IReadOnlyList<Position> Body => Segments;

    public Position Head => Segments[0];
}
