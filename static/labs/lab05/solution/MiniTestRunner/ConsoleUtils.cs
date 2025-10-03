namespace MiniTestRunner;

/// <summary>
/// Provides a scoped mechanism for temporarily changing console foreground and background colors.
/// Automatically restores the previous colors when disposed.
/// </summary>
public readonly struct ConsoleColoring : IDisposable
{
    /// <summary>
    /// Gets the console's foreground color before the change.
    /// </summary>
    public ConsoleColor PreviousForeground { get; }

    /// <summary>
    /// Gets the console's background color before the change, if available.
    /// </summary>
    public ConsoleColor? PreviousBackground { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleColoring"/> struct and resets the console colors.
    /// </summary>
    public ConsoleColoring()
    {
        this.PreviousForeground = Console.ForegroundColor;
        this.PreviousBackground = Console.BackgroundColor;
        Console.ResetColor();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleColoring"/> struct and sets the console foreground and optionally background color.
    /// </summary>
    /// <param name="foreground">The foreground color to set.</param>
    /// <param name="background">The optional background color to set.</param>
    public ConsoleColoring(ConsoleColor foreground, ConsoleColor? background = null)
    {
        this.PreviousForeground = Console.ForegroundColor;
        Console.ForegroundColor = foreground;
        if (background is { } bgColor)
        {
            this.PreviousBackground = Console.BackgroundColor;
            Console.BackgroundColor = bgColor;
        }
    }

    /// <summary>
    /// Restores the console's foreground and background colors to their previous values.
    /// </summary>
    public void Dispose()
    {
        if (this.PreviousBackground is { } bgColor)
        {
            Console.BackgroundColor = bgColor;
        }
        Console.ForegroundColor = this.PreviousForeground;
    }
}