using System.Text.Json;
using System.Text.Json.Serialization;
using tasks.Databases;

namespace tasks;

public static class DatabaseQueries
{
    public static void RunQueries(this IMovieDatabase movieDatabase)
    {
        movieDatabase.ActorsFromFantasyMovies();
        movieDatabase.LongestMovieByGenre();
        movieDatabase.HighRatedMoviesWithCast();
        movieDatabase.DistinctRolesCountPerActor();
        movieDatabase.RecentMoviesWithAverageRating();
        movieDatabase.AverageRatingByGenre();
        movieDatabase.ActorsWhoNeverPlayedInThriller();
        movieDatabase.Top3MoviesByRatingCount();
        movieDatabase.MoviesWithoutRatings();
        movieDatabase.MostVersatileActors();
    }

    public static void ActorsFromFantasyMovies(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = casts
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

        Console.WriteLine("Actors From Fantasy Movies");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void LongestMovieByGenre(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies
            .GroupBy(movie => movie.Genre)
            .Select(group => new
            {
                Genre = group.Key,
                Movie = group.MaxBy(movie => movie.DurationMinutes)
            })
            .ToList();

        Console.WriteLine("Longest Movie By Genre");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void HighRatedMoviesWithCast(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = ratings
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

        Console.WriteLine("High Rated Movies With Cast");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void DistinctRolesCountPerActor(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = actors
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

        Console.WriteLine("Distinct Roles Count Per Actor");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void RecentMoviesWithAverageRating(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies
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

        Console.WriteLine("Recent Movies With Average Rating");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void AverageRatingByGenre(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = ratings
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

        Console.WriteLine("Average Rating By Genre");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void ActorsWhoNeverPlayedInThriller(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var thrillerMovieIds = movies
            .Where(movie => movie.Genre == Genre.Thriller)
            .Select(movie => movie.Id)
            .ToHashSet();

        var thrillerActorIds = casts
            .Where(cast => thrillerMovieIds.Contains(cast.MovieId))
            .Select(cast => cast.ActorId)
            .ToHashSet();

        var queryResult = actors
            .Where(actor => !thrillerActorIds.Contains(actor.Id))
            .ToList();

        Console.WriteLine("Actors Who Never Played In Thriller");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void Top3MoviesByRatingCount(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = ratings
            .GroupBy(rating => rating.MovieId)
            .OrderByDescending(group => group.Count())
            .Take(3)
            .Select(group => movies.First(movie => movie.Id == group.Key))
            .ToList();

        Console.WriteLine("Top 3 Movies By Rating Count");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void MoviesWithoutRatings(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = movies
            .GroupJoin(ratings, movie => movie.Id, rating => rating.MovieId,
                (movie, movieRatings) => new
                {
                    Movie = movie,
                    HasRatings = movieRatings.Any()
                })
            .Where(movie => !movie.HasRatings)
            .Select(movie => movie.Movie)
            .ToList();

        Console.WriteLine("Movies Without Ratings");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void MostVersatileActors(this IMovieDatabase movieDatabase)
    {
        var movies = movieDatabase.Movies;
        var actors = movieDatabase.Actors;
        var ratings = movieDatabase.Ratings;
        var casts = movieDatabase.Casts;

        var queryResult = casts
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

        Console.WriteLine("Most Versatile Actors");
        DisplayQueryResults(queryResult);
        Console.WriteLine();
    }

    public static void DisplayQueryResults<T>(T query)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        var json = JsonSerializer.Serialize(query, options);

        Console.WriteLine(json);
    }
}