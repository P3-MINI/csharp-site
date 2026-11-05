using SnakeArena.Shared.Game;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Client;

/// <summary>
/// A simple computer player: heads for the nearest food while avoiding walls, snakes and dead ends.
/// Useful for testing with many players.
/// </summary>
public static class SnakeBot
{
    /// <summary>
    /// Chooses the direction the given player should take, or returns <see langword="null"/>
    /// if the player is dead, not in the snapshot, or has no safe move.
    /// </summary>
    public static Direction? ChooseDirection(StateSnapshot snapshot, PlayerId self, int width, int height)
    {
        var me = snapshot.Players.FirstOrDefault(p => p.Id == self);
        if (me is null || !me.IsAlive || me.Body.Count == 0)
            return null;

        var blocked = new HashSet<Position>();
        foreach (var player in snapshot.Players.Where(p => p.IsAlive && p.Body.Count > 0))
        {
            // The last segment moves away in the next tick (unless the snake eats).
            for (int i = 0; i < player.Body.Count - 1; i++)
                blocked.Add(player.Body[i]);

            // Avoid cells another head may enter in the next tick (head-to-head crash).
            if (player.Id != self)
            {
                foreach (var direction in Enum.GetValues<Direction>())
                    blocked.Add(player.Body[0].Move(direction));
            }
        }

        int wanted = 2 * me.Body.Count + 8;
        Direction? best = null;
        double bestScore = double.MinValue;
        foreach (var direction in Enum.GetValues<Direction>())
        {
            if (direction == me.Direction.Opposite())
                continue;

            var next = me.Body[0].Move(direction);
            if (!IsInside(next, width, height) || blocked.Contains(next))
                continue;

            int space = ReachableCells(next, blocked, width, height, wanted);
            int distance = snapshot.Food.Count == 0
                ? 0
                : snapshot.Food.Min(f => Math.Abs(f.X - next.X) + Math.Abs(f.Y - next.Y));
            double score = 10.0 * Math.Min(space, wanted) - distance + (direction == me.Direction ? 0.5 : 0.0);
            if (score > bestScore)
            {
                bestScore = score;
                best = direction;
            }
        }
        return best;
    }

    private static bool IsInside(Position p, int width, int height) =>
        p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height;

    // Breadth-first search counting free cells reachable from start (stops at limit).
    private static int ReachableCells(Position start, HashSet<Position> blocked, int width, int height, int limit)
    {
        var visited = new HashSet<Position> { start };
        var queue = new Queue<Position>([start]);
        while (queue.Count > 0 && visited.Count < limit)
        {
            var cell = queue.Dequeue();
            foreach (var direction in Enum.GetValues<Direction>())
            {
                var next = cell.Move(direction);
                if (IsInside(next, width, height) && !blocked.Contains(next) && visited.Add(next))
                    queue.Enqueue(next);
            }
        }
        return visited.Count;
    }
}
