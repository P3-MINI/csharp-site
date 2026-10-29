using SnakeArena.Shared.Protocol;

namespace SnakeArena.Shared.Game;

/// <summary>
/// Deterministic rules of the game. For the same settings, seed and sequence of calls
/// the state evolves identically.
/// </summary>
/// <remarks>
/// The class is NOT thread-safe: only one thread (the server's game loop) may use an instance.
/// </remarks>
public sealed class GameState
{
    private const int SpawnAttempts = 200;
    private const int SpawnClearance = 3;

    private readonly SortedDictionary<PlayerId, Snake> _snakes = new();
    private readonly HashSet<Position> _food = [];
    private readonly Random _random;

    public GameState(GameSettings settings, int seed)
    {
        settings.Validate();
        Settings = settings;
        _random = new Random(seed);
        TopUpFood();
    }

    public GameSettings Settings { get; }

    /// <summary>Number of ticks simulated so far.</summary>
    public uint CurrentTick { get; private set; }

    /// <summary>All players (alive or waiting for a respawn), ordered by id.</summary>
    public IReadOnlyCollection<Snake> Snakes => _snakes.Values;

    public IReadOnlyCollection<Position> Food => _food;

    public bool ContainsPlayer(PlayerId id) => _snakes.ContainsKey(id);

    /// <summary>
    /// Adds a player and tries to spawn its snake immediately (if the board is too crowded,
    /// the snake spawns in one of the following ticks).
    /// </summary>
    /// <exception cref="ArgumentException">A player with this id already exists.</exception>
    public Snake AddPlayer(PlayerId id, string name)
    {
        var snake = new Snake(id, name);
        if (!_snakes.TryAdd(id, snake))
            throw new ArgumentException($"Player {id} already exists.", nameof(id));
        TrySpawn(snake);
        return snake;
    }

    /// <summary>Removes the player and its snake from the board.</summary>
    public bool RemovePlayer(PlayerId id) => _snakes.Remove(id);

    /// <summary>
    /// Advances the game by one tick.
    /// </summary>
    /// <param name="inputs">
    /// At most one requested direction per player. Players without an entry keep their direction.
    /// Requests to reverse (e.g. Left while moving Right) and unknown players are ignored.
    /// </param>
    public void Tick(IReadOnlyDictionary<PlayerId, Direction> inputs)
    {
        CurrentTick++;
        var alive = _snakes.Values.Where(s => s.IsAlive).ToList();

        // 1. Steering.
        foreach (var snake in alive)
        {
            if (inputs.TryGetValue(snake.Id, out var direction)
                && Enum.IsDefined(direction)
                && direction != snake.Direction.Opposite())
            {
                snake.Direction = direction;
            }
        }

        // 2. Movement: every snake advances by one cell. A snake grows (keeps its tail)
        //    when its new head lands on food.
        var newHeads = new Dictionary<PlayerId, Position>();
        var growing = new HashSet<PlayerId>();
        foreach (var snake in alive)
        {
            var head = snake.Head.Move(snake.Direction);
            newHeads[snake.Id] = head;
            if (_food.Contains(head))
                growing.Add(snake.Id);
        }

        // Count how many segments occupy each cell after the move.
        var occupancy = new Dictionary<Position, int>();
        foreach (var snake in alive)
        {
            Occupy(occupancy, newHeads[snake.Id]);
            int kept = growing.Contains(snake.Id) ? snake.Segments.Count : snake.Segments.Count - 1;
            for (int i = 0; i < kept; i++)
                Occupy(occupancy, snake.Segments[i]);
        }

        // 3. Collisions: a snake crashes when its new head leaves the board or shares a cell
        //    with any other segment (its own body, another snake, or another head).
        var crashed = alive
            .Where(s => !IsInside(newHeads[s.Id]) || occupancy[newHeads[s.Id]] > 1)
            .ToList();

        // 4. Survivors move and eat.
        foreach (var snake in alive.Except(crashed))
        {
            var head = newHeads[snake.Id];
            snake.Segments.Insert(0, head);
            if (growing.Contains(snake.Id))
            {
                _food.Remove(head);
                snake.Score++;
            }
            else
            {
                snake.Segments.RemoveAt(snake.Segments.Count - 1);
            }
        }

        // 5. Respawn players that have been waiting long enough.
        foreach (var snake in _snakes.Values.Where(s => !s.IsAlive))
        {
            if (snake.RespawnIn > 0)
                snake.RespawnIn--;
            if (snake.RespawnIn == 0)
                TrySpawn(snake);
        }

        // 6. Crashed snakes disappear; every second segment of a crashed snake turns into food.
        foreach (var snake in crashed)
        {
            for (int i = 0; i < snake.Segments.Count; i += 2)
            {
                var cell = snake.Segments[i];
                if (!IsOccupiedBySnake(cell))
                    _food.Add(cell);
            }

            snake.Segments.Clear();
            snake.IsAlive = false;
            snake.RespawnIn = Settings.RespawnTicks;
        }

        TopUpFood();
    }

    /// <summary>Creates a snapshot of the current state (players ordered by id, food ordered by row and column).</summary>
    public StateSnapshot ToSnapshot() => new(
        CurrentTick,
        [
            .. _snakes.Values.Select(s => new PlayerSnapshot(
                s.Id,
                s.Name,
                s.IsAlive,
                s.Direction,
                (ushort)Math.Min(s.Score, ushort.MaxValue),
                [.. s.Segments]))
        ],
        [.. _food.OrderBy(p => p.Y).ThenBy(p => p.X)]);

    private static void Occupy(Dictionary<Position, int> occupancy, Position cell) =>
        occupancy[cell] = occupancy.GetValueOrDefault(cell) + 1;

    private bool IsInside(Position p) => p.X >= 0 && p.Y >= 0 && p.X < Settings.Width && p.Y < Settings.Height;

    private bool IsOccupiedBySnake(Position p) => _snakes.Values.Any(s => s.IsAlive && s.Segments.Contains(p));

    private bool IsFree(Position p) => IsInside(p) && !_food.Contains(p) && !IsOccupiedBySnake(p);

    private void TrySpawn(Snake snake)
    {
        for (int attempt = 0; attempt < SpawnAttempts; attempt++)
        {
            var direction = (Direction)_random.Next(4);
            var head = new Position(_random.Next(Settings.Width), _random.Next(Settings.Height));

            // The body extends behind the head...
            var body = new List<Position> { head };
            for (int i = 1; i < Settings.InitialLength; i++)
                body.Add(body[^1].Move(direction.Opposite()));
            if (!body.All(IsFree))
                continue;

            // ...and there must be some free space in front of it.
            var ahead = head;
            bool clear = true;
            for (int i = 0; i < SpawnClearance && clear; i++)
            {
                ahead = ahead.Move(direction);
                clear = IsInside(ahead) && !IsOccupiedBySnake(ahead);
            }
            if (!clear)
                continue;

            snake.Segments.Clear();
            snake.Segments.AddRange(body);
            snake.Direction = direction;
            snake.IsAlive = true;
            snake.RespawnIn = 0;
            return;
        }
        // No room right now: the snake stays dead and the next tick tries again.
    }

    private void TopUpFood()
    {
        for (int attempt = 0; _food.Count < Settings.FoodCount && attempt < SpawnAttempts; attempt++)
        {
            var cell = new Position(_random.Next(Settings.Width), _random.Next(Settings.Height));
            if (IsFree(cell))
                _food.Add(cell);
        }
    }
}
