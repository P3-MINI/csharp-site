namespace Wrapped;

/// <summary>A track from the music library.</summary>
public record Track(int Id, string Title, int ArtistId, TimeSpan Length);
