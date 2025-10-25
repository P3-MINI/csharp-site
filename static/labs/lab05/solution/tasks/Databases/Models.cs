using System.Text.Json.Serialization;

namespace tasks.Databases;

public record Movie(
    int Id, 
    string Title, 
    int Year, 
    [property: JsonConverter(typeof(JsonStringEnumConverter))] 
    Genre Genre, 
    int DurationMinutes
);

public record Actor(
    int Id, 
    string Name
);

public record Rating(
    int Id, 
    int MovieId, 
    int Score, 
    DateTime CreatedAt
);

public record Cast(
    int MovieId, 
    int ActorId, 
    string Role
);

public enum Genre
{
    Comedy,
    Drama,
    Horror,
    Romance,
    Thriller,
    Fantasy,
}