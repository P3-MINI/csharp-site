using System;
using System.Collections.Generic;
using System.IO;

namespace Task3;

public record class Movie(int MovieID, string Title, string Genres, int Year);

public record class User(int UserID, string Gender, int Age, int Occupation, string ZipCode);

public record class RatingEntry(int UserID, int MovieID, double Rating, DateTime Timestamp); 

internal class DatabaseMovies {

    public List<Movie> Movies { get; set; }
    public List<User> Users { get; set; }
    public List<RatingEntry> Ratings { get; set; }

    static List<Movie> GetMovies(string moviesPath) {
        StreamReader sr = new(moviesPath);  // stream opened in open mode
        List<Movie> movies = [];
        string[] buf;

        while (!sr.EndOfStream) {
            buf = sr.ReadLine().Split('@');
            int movieId = int.Parse(buf[0]);
            string title = buf[1];
            string genres = buf[2];
            int year = int.Parse(buf[3]);
            movies.Add(new Movie(movieId, title, genres, year));
        }
        sr.Close();
        return movies;
    }

    static List<User> GetUsers(string moviesPath) {
        StreamReader sr = new(moviesPath);  // stream opened in open mode
        List<User> users = [];
        string[] buf;

        while (!sr.EndOfStream) {
            buf = sr.ReadLine().Split('@');
            int userId = int.Parse(buf[0]);
            string gender = buf[1];
            int age = int.Parse(buf[2]);
            int occupation = int.Parse(buf[3]);
            string zipcode = buf[4];
            users.Add(new User(userId, gender, age, occupation, zipcode));
        }
        sr.Close();
        return users;
    }

    static List<RatingEntry> GetRatings(string moviesPath) {
        StreamReader sr = new(moviesPath);  // stream opened in open mode
        List<RatingEntry> ratings = [];
        string[] buf;

        while (!sr.EndOfStream) {
            buf = sr.ReadLine().Split('@');
            int userId = int.Parse(buf[0]);
            int movieId = int.Parse(buf[1]);
            double rating = double.Parse(buf[2]);
            int seconds = int.Parse(buf[3]);
            System.DateTime dtDateTime = new(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime timestamp = dtDateTime.AddSeconds(seconds).ToLocalTime();
            ratings.Add(new RatingEntry(userId, movieId, rating, timestamp));
        }
        sr.Close();
        return ratings;
    }

    public static DatabaseMovies GetInstance(string moviesPath, string usersPath, string ratingsPath) {
        List<Movie> movies = GetMovies(moviesPath);
        List<User> users = GetUsers(usersPath);
        List<RatingEntry> ratings = GetRatings(ratingsPath);

        return new DatabaseMovies() {
            Movies = movies,
            Users = users,
            Ratings = ratings
        };
    }
}
