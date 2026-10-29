using System.Globalization;
using Wrapped;

// Format numbers and dates the same way on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var library = MusicLibrary.Load(Path.Combine(AppContext.BaseDirectory, "Data"));

Console.WriteLine($"Loaded {library.Artists.Count} artists, {library.Tracks.Count} tracks and {library.Plays.Count} plays.");
Console.WriteLine($"First play: {library.Plays[0]}");
