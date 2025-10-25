namespace tasks.Databases;

public sealed class SampleMovieDatabase : IMovieDatabase
{
    public List<Movie> Movies { get; } = new List<Movie>
    {
        // Req 1 (Fantasy), Req 8 (Top 3)
        new(101, "The Lord of the Rings: The Fellowship of the Ring", 2001, Genre.Fantasy, 201),
        // Req 3 (High Rated), Req 2 (Drama), Req 8 (Top 3)
        new(102, "The Shawshank Redemption", 1994, Genre.Drama, 142),
        // Req 2 (Romance), Req 4 & 7 (for Tom Hanks)
        new(103, "Forrest Gump", 1994, Genre.Romance, 142),
        // Req 7 (Thriller)
        new(104, "Se7en", 1995, Genre.Thriller, 127),
        // Req 2 (Horror), Req 10 (Versatile)
        new(105, "Alien", 1979, Genre.Horror, 117),
        // Req 2 (Comedy), Req 10 (Versatile)
        new(106, "The Truman Show", 1998, Genre.Comedy, 103),
        // Req 5 (Recent), Req 1 (Fantasy)
        new(107, "Poor Things", 2023, Genre.Fantasy, 141),
        // Req 9 (MoviesWithoutRatings)
        new(108, "Unreleased Project", 2025, Genre.Drama, 120),
        // Req 10 (Versatile - for Jim Carrey)
        new(109, "Eternal Sunshine of the Spotless Mind", 2004, Genre.Drama, 108),
        // Req 4 (Distinct Roles - for Tom Hanks), Req 2 (longest Drama)
        new(110, "Saving Private Ryan", 1998, Genre.Drama, 169)
    };
    public List<Actor> Actors { get; } = new List<Actor>
    {
        new(201, "Ian McKellen"),         // Req 1 (Fantasy)
        new(202, "Tim Robbins"),          // Req 3 (High Rated)
        new(203, "Morgan Freeman"),       // Req 3, Req 7 (played in Thriller), Req 10
        new(204, "Tom Hanks"),            // Req 4, Req 7 (did NOT play in Thriller), Req 10
        new(205, "Brad Pitt"),            // Req 7 (played in Thriller)
        new(206, "Elijah Wood"),          // Req 1 (Fantasy)
        new(207, "Emma Stone"),           // Req 5 (Recent), Req 1 (Fantasy)
        new(208, "Sigourney Weaver"),     // Req 10
        new(209, "Jim Carrey")            // Req 10
    };
    public List<Rating> Ratings { get; } = new List<Rating>
    {
        // Req 3 (High Rated > 8) for M102
        new(301, 102, 9, new DateTime(2024, 1, 1)),
        new(302, 102, 10, new DateTime(2024, 2, 1)), // Avg: 9.5
    
        // Req 5 (Recent) for M107
        new(303, 107, 8, new DateTime(2024, 3, 1)),
    
        // Req 8 (Top 3 by count)
        // M101 (LOTR) - 3 ratings
        new(304, 101, 10, new DateTime(2024, 4, 1)),
        new(305, 101, 9, new DateTime(2024, 5, 1)),
        new(306, 101, 10, new DateTime(2024, 6, 1)),
    
        // M102 (Shawshank) - 2 ratings (see above)
    
        // M104 (Se7en) - 1 rating
        new(307, 104, 8, new DateTime(2024, 7, 1))
    
        // M107 (Poor Things) - 1 rating (see above)
    
        // Other movies (M103, M105, M106, M108, M109, M110) have no ratings (Req 9)
    };
    public List<Cast> Casts { get; } = new List<Cast>
    {
        // Req 1 (Fantasy Actors)
        new(101, 201, "Gandalf"),
        new(101, 206, "Frodo Baggins"),
    
        // Req 3 (High Rated Cast)
        new(102, 202, "Andy Dufresne"),
        new(102, 203, "Ellis Boyd 'Red' Redding"),
    
        // Req 4 (Distinct Roles) & Req 7 (Not in Thriller) - Tom Hanks
        new(103, 204, "Forrest Gump"),
        new(110, 204, "Captain John H. Miller"), // Hanks has 2 distinct roles and played in Romance and Drama
    
        // Req 7 (Thriller Actors)
        new(104, 205, "Detective David Mills"),
        new(104, 203, "Detective Lt. William Somerset"), // Morgan Freeman played in Thriller (and Drama)
    
        // Req 10 (Versatile)
        new(105, 208, "Ellen Ripley"), // Sigourney (Horror)
        new(106, 209, "Truman Burbank"), // Jim Carrey (Comedy)
        new(109, 209, "Joel Barish"), // Jim Carrey (Drama) - has 2 genres
    
        // Req 5 (Recent)
        new(107, 207, "Bella Baxter")
    };
}