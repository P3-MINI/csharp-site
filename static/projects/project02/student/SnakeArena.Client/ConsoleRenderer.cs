using SnakeArena.Shared.Game;
using SnakeArena.Shared.Protocol;

namespace SnakeArena.Client;

/// <summary>
/// Draws the board, the scoreboard, a status line and the chat in the console.
/// </summary>
/// <remarks>
/// Not thread-safe: call all methods from one thread. Requires an interactive console
/// (standard output must not be redirected). Every board cell is two characters wide,
/// so that it looks roughly square.
/// </remarks>
public sealed class ConsoleRenderer
{
    public const int ChatLineCount = 3;
    private const int PanelWidth = 17;
    private const int NameWidth = 10;

    private static readonly ConsoleColor[] PlayerColors =
    [
        ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Magenta,
        ConsoleColor.Blue, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan, ConsoleColor.DarkYellow,
    ];

    private readonly int _boardWidth;
    private readonly int _boardHeight;
    private readonly PlayerId _self;
    private readonly char[,] _chars;
    private readonly ConsoleColor[,] _colors;
    private bool _showingSizeWarning;

    public ConsoleRenderer(int boardWidth, int boardHeight, PlayerId self)
    {
        _boardWidth = boardWidth;
        _boardHeight = boardHeight;
        _self = self;
        Width = 2 * boardWidth + 2 + PanelWidth;
        Height = boardHeight + 2 + 1 + ChatLineCount + 1;
        _chars = new char[Height, Width];
        _colors = new ConsoleColor[Height, Width];

        Console.CursorVisible = false;
        Console.Clear();
    }

    /// <summary>Number of console columns used by the renderer.</summary>
    public int Width { get; }

    /// <summary>Number of console rows used by the renderer.</summary>
    public int Height { get; }

    /// <summary>Color used for the given player's snake and score.</summary>
    public static ConsoleColor ColorOf(PlayerId id) => PlayerColors[(id.Value - 1 + PlayerColors.Length) % PlayerColors.Length];

    /// <summary>Redraws the whole screen.</summary>
    /// <param name="snapshot">The state to draw.</param>
    /// <param name="status">A one-line status text (e.g. tick number, connection info).</param>
    /// <param name="chat">Chat history; the last <see cref="ChatLineCount"/> lines are shown.</param>
    /// <param name="chatInput">The chat line being typed, or <see langword="null"/> when not typing.</param>
    public void Render(StateSnapshot snapshot, string status, IReadOnlyList<string> chat, string? chatInput)
    {
        // Writing into the last column/row may scroll the window, hence the extra column and row.
        if (Console.WindowWidth < Width + 1 || Console.WindowHeight < Height + 1)
        {
            Console.Clear();
            Console.ResetColor();
            Console.Write($"Please enlarge the console to at least {Width + 1}x{Height + 1} " +
                          $"(now {Console.WindowWidth}x{Console.WindowHeight}).");
            _showingSizeWarning = true;
            return;
        }

        if (_showingSizeWarning)
        {
            Console.Clear();
            _showingSizeWarning = false;
        }

        Fill(0, 0, Width, Height, ' ', ConsoleColor.Gray);
        DrawBorder();
        DrawFood(snapshot.Food);
        DrawSnakes(snapshot.Players);
        DrawScoreboard(snapshot.Players);

        int row = _boardHeight + 2;
        Text(row++, 0, status, ConsoleColor.Gray);
        for (int i = Math.Max(0, chat.Count - ChatLineCount); i < chat.Count; i++)
            Text(row++, 0, chat[i], ConsoleColor.White);

        row = Height - 1;
        if (chatInput is null)
            Text(row, 0, "Arrows/WASD: move   Enter: chat   Esc/Q: quit", ConsoleColor.DarkGray);
        else
            Text(row, 0, $"Say: {chatInput}_", ConsoleColor.White);

        Flush();
    }

    /// <summary>Restores the console (colors, cursor) and moves the cursor below the drawn area.</summary>
    public void Restore()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        Console.SetCursorPosition(0, Math.Min(Height, Console.BufferHeight - 1));
        Console.WriteLine();
    }

    private void DrawBorder()
    {
        int right = 2 * _boardWidth + 1;
        int bottom = _boardHeight + 1;
        Fill(0, 0, right + 1, 1, '-', ConsoleColor.DarkGray);
        Fill(0, bottom, right + 1, 1, '-', ConsoleColor.DarkGray);
        Fill(0, 0, 1, bottom + 1, '|', ConsoleColor.DarkGray);
        Fill(right, 0, 1, bottom + 1, '|', ConsoleColor.DarkGray);
        foreach (var (x, y) in new[] { (0, 0), (right, 0), (0, bottom), (right, bottom) })
            Put(y, x, '+', ConsoleColor.DarkGray);
    }

    private void DrawFood(IReadOnlyList<Position> food)
    {
        foreach (var cell in food)
            Cell(cell, "()", ConsoleColor.Red);
    }

    private void DrawSnakes(IReadOnlyList<PlayerSnapshot> players)
    {
        foreach (var player in players.Where(p => p.IsAlive && p.Body.Count > 0))
        {
            var color = ColorOf(player.Id);
            for (int i = player.Body.Count - 1; i > 0; i--)
                Cell(player.Body[i], player.Id == _self ? "[]" : "##", color);

            string head = player.Direction switch
            {
                Direction.Up => "^^",
                Direction.Down => "vv",
                Direction.Left => "<<",
                _ => ">>",
            };
            Cell(player.Body[0], head, player.Id == _self ? ConsoleColor.White : color);
        }
    }

    private void DrawScoreboard(IReadOnlyList<PlayerSnapshot> players)
    {
        int column = 2 * _boardWidth + 3;
        Text(0, column, "Players", ConsoleColor.Gray);
        int row = 1;
        foreach (var player in players.OrderByDescending(p => p.Score).ThenBy(p => p.Id))
        {
            if (row >= _boardHeight + 2)
                break;
            string marker = player.Id == _self ? ">" : " ";
            string name = player.Name.Length > NameWidth ? player.Name[..NameWidth] : player.Name;
            var color = player.IsAlive ? ColorOf(player.Id) : ConsoleColor.DarkGray;
            Text(row++, column, $"{marker}{name,-NameWidth}{player.Score,5}", color);
        }
    }

    private void Cell(Position cell, string text, ConsoleColor color)
    {
        if (cell.X < 0 || cell.Y < 0 || cell.X >= _boardWidth || cell.Y >= _boardHeight)
            return;
        Put(cell.Y + 1, 2 * cell.X + 1, text[0], color);
        Put(cell.Y + 1, 2 * cell.X + 2, text[1], color);
    }

    private void Text(int row, int column, string text, ConsoleColor color)
    {
        for (int i = 0; i < text.Length && column + i < Width; i++)
            Put(row, column + i, char.IsControl(text[i]) ? ' ' : text[i], color);
    }

    private void Fill(int x, int y, int width, int height, char c, ConsoleColor color)
    {
        for (int row = y; row < y + height; row++)
            for (int column = x; column < x + width; column++)
                Put(row, column, c, color);
    }

    private void Put(int row, int column, char c, ConsoleColor color)
    {
        _chars[row, column] = c;
        _colors[row, column] = color;
    }

    private void Flush()
    {
        var line = new char[Width];
        for (int row = 0; row < Height; row++)
        {
            for (int column = 0; column < Width; column++)
                line[column] = _chars[row, column];

            Console.SetCursorPosition(0, row);
            int start = 0;
            while (start < Width)
            {
                // Write runs of characters of the same color at once.
                int end = start;
                while (end < Width && _colors[row, end] == _colors[row, start])
                    end++;
                Console.ForegroundColor = _colors[row, start];
                Console.Out.Write(line, start, end - start);
                start = end;
            }
        }
        Console.ResetColor();
    }
}
