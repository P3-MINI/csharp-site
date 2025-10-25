namespace tasks.Databases;

public interface IMovieDatabase
{
    public List<Movie> Movies { get; }
    public List<Actor> Actors { get; }
    public List<Rating> Ratings { get; }
    public List<Cast> Casts { get; }
}