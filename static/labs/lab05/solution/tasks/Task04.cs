using System.Text.Json;

namespace tasks;

public sealed class Task04 : IExecutable
{
    public void Execute(string[] args)
    {
        var movies = new List<Movie>();
        var actors = new List<Actor>();
        var ratings = new List<Rating>();
        var casts = new List<Cast>();

        // Query 1:
        var fantasyActors = casts
            .Join(movies.Where(m => m.Genre == Genre.Fantasy),
                cast => cast.MovieId,
                movie => movie.Id,
                (cast, movie) => cast.ActorId)
            .Distinct()
            .Join(actors,
                actorId => actorId,
                actor => actor.Id,
                (actorId, actor) => actor)
            .ToList();

        DisplayQueryResults(fantasyActors);

        // Query 2:
        var longestMoviesByGenre = movies
            .GroupBy(movie => movie.Genre)
            .Select(group => new
            {
                Genre = group.Key,
                Movie = group.MaxBy(movie => movie.DurationMinutes)
            })
            .ToList();

        DisplayQueryResults(longestMoviesByGenre);

        // Query 3:
        var topRatedMoviesWithCast = ratings
            .GroupBy(r => r.MovieId)
            .Select(g => new
            {
                MovieId = g.Key,
                Average = g.Average(r => r.Score)
            })
            .Where(x => x.Average > 8)
            .Join(movies,
                rating => rating.MovieId,
                movie => movie.Id,
                (rating, movie) => new
                {
                    Movie = movie,
                    rating.Average
                })
            .GroupJoin(casts,
                movie => movie.Movie.Id,
                cast => cast.MovieId,
                (movie, movieCasts) => new
                {
                    movie.Movie,
                    movie.Average,
                    CastIds = movieCasts.Select(c => c.ActorId)
                })
            .Select(x => new
            {
                x.Movie,
                x.Average,
                Cast = x.CastIds
                    .Join(actors,
                          actorId => actorId,
                          actor => actor.Id,
                          (actorId, actor) => actor)
                    .ToList()
            })
            .ToList();

        DisplayQueryResults(topRatedMoviesWithCast);

        // Query 4:
        var actorsWithRoleCount = actors
            .GroupJoin(
                casts,
                actor => actor.Id,
                cast => cast.ActorId,
                (actor, actorCasts) => new
                {
                    Actor = actor,
                    Roles = actorCasts
                        .Select(c => c.Role)
                        .Distinct()
                        .Count()
                }
            )
            .OrderByDescending(x => x.Roles)
            .ToList();


        DisplayQueryResults(actorsWithRoleCount);

        // Query 5:
        var recentTopRatedMovies = movies
            .Where(m => m.Year > DateTime.Now.Year - 5)
            .GroupJoin(
                ratings,
                movie => movie.Id,
                rating => rating.MovieId,
                (movie, movieRatings) => new
                {
                    Movie = movie,
                    AverageScore = movieRatings.Any()
                        ? movieRatings.Average(r => r.Score)
                        : 0.0
                }
            )
            .OrderByDescending(x => x.AverageScore)
            .ToList();

        DisplayQueryResults(recentTopRatedMovies);

        // Query 6:
        var averageRatingByGenre = ratings
            .Join(movies, rating => rating.MovieId, movie => movie.Id,
                (rating, movie) => new
                {
                    movie.Genre,
                    rating.Score
                })
            .GroupBy(genreScore => genreScore.Genre)
            .Select(group => new
            {
                Genre = group.Key,
                AverageScore = group
                    .Select(genreScore => genreScore.Score)
                    .Average()
            })
            .ToList();

        DisplayQueryResults(averageRatingByGenre);

        // Query 7:
        var thrillerMovieIds = movies
            .Where(movie => movie.Genre == Genre.Thriller)
            .Select(movie => movie.Id)
            .ToHashSet();

        var thrillerActorIds = casts
            .Where(cast => thrillerMovieIds.Contains(cast.MovieId))
            .Select(cast => cast.ActorId)
            .ToHashSet();

        var actorsNotInThriller = actors
            .Where(actor => !thrillerActorIds.Contains(actor.Id))
            .ToList();

        DisplayQueryResults(actorsNotInThriller);

        // Query 8:
        var top3RatedMovies = ratings
            .GroupBy(rating => rating.MovieId)
            .OrderByDescending(group => group.Count())
            .Take(3)
            .Select(group => movies.First(movie => movie.Id == group.Key))
            .ToList();

        DisplayQueryResults(top3RatedMovies);

        // Query 9:
        var moviesWithoutRatings = movies
            .GroupJoin(ratings, movie => movie.Id, rating => rating.MovieId,
                (movie, movieRatings) => new
                {
                    Movie = movie,
                    HasRatings = movieRatings.Any()
                })
            .Where(movie => !movie.HasRatings)
            .Select(movie => movie.Movie)
            .ToList();

        DisplayQueryResults(moviesWithoutRatings);

        // Query 10:
        var versatileActors = casts
            .Join(
                movies,
                cast => cast.MovieId,
                movie => movie.Id,
                (cast, movie) => new
                {
                    cast.ActorId,
                    movie.Genre
                }
            )
            .GroupBy(movie => movie.ActorId)
            .Select(group => new
            {
                ActorId = group.Key,
                GenreCount = group
                    .Select(x => x.Genre)
                    .Distinct()
                    .Count()
            })
            .Join(
                actors,
                x => x.ActorId,
                actor => actor.Id,
                (x, actor) => new
                {
                    Actor = actor,
                    x.GenreCount
                }
            )
            .OrderByDescending(x => x.GenreCount)
            .ToList();
    }

    public static void DisplayQueryResults<T>(IEnumerable<T> query)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(query, options);

        Console.WriteLine(json);
    }
}

public record Movie(int Id, string Title, int Year, Genre Genre, int DurationMinutes);

public record Actor(int Id, string Name);

public record Rating(int Id, int MovieId, int Score, DateTime CreatedAt);

public record Cast(int MovieId, int ActorId, string Role);

public enum Genre
{
    Comedy,
    Drama,
    Horror,
    Romance,
    Thriller,
    Fantasy,
}