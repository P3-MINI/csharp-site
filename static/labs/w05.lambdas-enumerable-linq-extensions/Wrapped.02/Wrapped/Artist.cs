namespace Wrapped;

/// <summary>A performer from the music library.</summary>
public record Artist(int Id, string Name, IReadOnlyList<string> Genres);
