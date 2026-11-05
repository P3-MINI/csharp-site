using SnakeArena.Shared.Game;

namespace SnakeArena.Tests;

// The game rules are provided; these tests document them (and pass from the start).
public class GameStateTests
{
    private static readonly Dictionary<PlayerId, Direction> NoInput = [];
    private static readonly PlayerId Alice = new(1);

    [Fact]
    public void SameSeed_SameEvolution()
    {
        var a = new GameState(new GameSettings(), seed: 7);
        var b = new GameState(new GameSettings(), seed: 7);
        for (byte id = 1; id <= 4; id++)
        {
            a.AddPlayer(new PlayerId(id), $"p{id}");
            b.AddPlayer(new PlayerId(id), $"p{id}");
        }

        for (int i = 0; i < 200; i++)
        {
            var inputs = new Dictionary<PlayerId, Direction> { [new PlayerId((byte)(i % 4 + 1))] = (Direction)(i % 3) };
            a.Tick(inputs);
            b.Tick(inputs);
            Assert.Equal(a.ToSnapshot(), b.ToSnapshot());
        }
    }

    [Fact]
    public void Snake_MovesOneCellPerTick()
    {
        var game = new GameState(new GameSettings(), seed: 1);
        var snake = game.AddPlayer(Alice, "Alice");
        var head = snake.Head;
        int length = snake.Body.Count;

        game.Tick(NoInput);

        Assert.True(snake.IsAlive);
        Assert.Equal(head.Move(snake.Direction), snake.Head);
        Assert.Equal(1u, game.CurrentTick);
        Assert.True(snake.Body.Count >= length);
    }

    [Fact]
    public void ReverseInput_IsIgnored()
    {
        var game = new GameState(new GameSettings(), seed: 2);
        var snake = game.AddPlayer(Alice, "Alice");
        var direction = snake.Direction;

        game.Tick(new Dictionary<PlayerId, Direction> { [Alice] = direction.Opposite() });

        Assert.Equal(direction, snake.Direction);
    }

    [Fact]
    public void SnakeLeavingTheBoard_Crashes_AndRespawnsLater()
    {
        var settings = new GameSettings { RespawnTicks = 5 };
        var game = new GameState(settings, seed: 3);
        var snake = game.AddPlayer(Alice, "Alice");

        int ticks = 0;
        while (snake.IsAlive && ticks++ < settings.Width + settings.Height)
            game.Tick(NoInput);
        Assert.False(snake.IsAlive);
        Assert.Empty(snake.Body);

        for (int i = 0; i < settings.RespawnTicks; i++)
            game.Tick(NoInput);
        Assert.True(snake.IsAlive);
        Assert.Equal(settings.InitialLength, snake.Body.Count);
    }

    [Fact]
    public void RemovedPlayer_DisappearsFromSnapshot()
    {
        var game = new GameState(new GameSettings(), seed: 4);
        game.AddPlayer(Alice, "Alice");
        game.AddPlayer(new PlayerId(2), "Bob");

        game.RemovePlayer(Alice);

        Assert.Equal([new PlayerId(2)], game.ToSnapshot().Players.Select(p => p.Id));
    }
}
