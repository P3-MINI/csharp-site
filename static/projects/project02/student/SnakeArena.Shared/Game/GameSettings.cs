namespace SnakeArena.Shared.Game;

/// <summary>
/// Parameters of the game rules.
/// </summary>
public sealed record GameSettings
{
    public const int MinWidth = 10, MaxWidth = 100;
    public const int MinHeight = 8, MaxHeight = 50;

    /// <summary>Number of columns of the board.</summary>
    public int Width { get; init; } = 30;

    /// <summary>Number of rows of the board.</summary>
    public int Height { get; init; } = 16;

    /// <summary>Length of a freshly (re)spawned snake.</summary>
    public int InitialLength { get; init; } = 3;

    /// <summary>The game keeps at least this many food items on the board.</summary>
    public int FoodCount { get; init; } = 5;

    /// <summary>Number of ticks a crashed snake waits before it respawns.</summary>
    public int RespawnTicks { get; init; } = 20;

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if any value is out of range.</summary>
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(Width, MinWidth, nameof(Width));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Width, MaxWidth, nameof(Width));
        ArgumentOutOfRangeException.ThrowIfLessThan(Height, MinHeight, nameof(Height));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Height, MaxHeight, nameof(Height));
        ArgumentOutOfRangeException.ThrowIfLessThan(InitialLength, 1, nameof(InitialLength));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(InitialLength, MinHeight / 2, nameof(InitialLength));
        ArgumentOutOfRangeException.ThrowIfNegative(FoodCount, nameof(FoodCount));
        ArgumentOutOfRangeException.ThrowIfNegative(RespawnTicks, nameof(RespawnTicks));
    }
}
