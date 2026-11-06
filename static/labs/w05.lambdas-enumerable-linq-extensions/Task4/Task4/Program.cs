using System;
using System.Linq;

namespace Task3; 
public class Program {
    public static void Main(string[] args) {
        DatabaseMovies database = DatabaseMovies.GetInstance(
            "..\\..\\..\\movies.csv",
            "..\\..\\..\\users.csv",
            "..\\..\\..\\ratings.csv");

        // Question 1
        // Count how many users are in the given age groups, sort (ascending) by age.
        Console.WriteLine("Expected:");
        Console.WriteLine($"--------------");
        Console.WriteLine("1 222\r\n18 1103\r\n25 2096\r\n35 1193\r\n45 550\r\n50 496\r\n56 380");
        Console.WriteLine($"--------------");
        Console.WriteLine("Provided:");
        Console.WriteLine($"--------------");

        /*
        var distribs = ... your solution here


        foreach (var dist in distribs) {
            Console.WriteLine($"{dist.Age} {dist.Count}");
        }
        */
        Console.WriteLine($"--------------");

        // Question 2
        // What is the earliest year of a watched movie in the individual groups determined by age and gender — sort the results ascending by the earliest year.
        Console.WriteLine("Expected:");
        Console.WriteLine($"--------------");
        Console.WriteLine("50 F  - 1919\r\n25 M  - 1919\r\n35 F  - 1919\r\n18 M  - 1919\r\n35 M  - 1919\r\n25 F  - 1919\r\n45 M  - 1919\r\n" +
            "45 F  - 1919\r\n50 M  - 1919\r\n56 M  - 1919\r\n56 F  - 1919\r\n1 F  - 1921\r\n18 F  - 1921\r\n1 M  - 1921");
        Console.WriteLine($"--------------");
        Console.WriteLine("Provided:");
        Console.WriteLine($"--------------");

        /*
        var distribs2 = ... your solution here 

        foreach (var dist in distribs2) {
            Console.WriteLine($"{dist.GenderAge.Age} {dist.GenderAge.Gender}  - {dist.MinYear} ");
        }
        */
        Console.WriteLine($"--------------");

        // Question 3
        // Count the number of occurrences of each movie genre. Each genre is counted separately,
        // so for "Horror|Comedy" we have +1 for the Horror genre and +1 for the Comedy genre. Sort ascending by genre.
        Console.WriteLine("Expected:");
        Console.WriteLine($"--------------");
        Console.WriteLine("Action 503\r\nAdventure 283\r\nAnimation 105\r\nChildren's 251\r\nComedy 1200\r\nCrime 211\r\nDocumentary 127\r\n" +
            "Drama 1603\r\nFantasy 68\r\nFilm-Noir 44\r\nHorror 343\r\nMusical 114\r\nMystery 106\r\nRomance 471\r\nSci-Fi 276\r\nThriller 492\r\nWar 143\r\nWestern 68");
        Console.WriteLine($"--------------");
        Console.WriteLine("Provided:");
        Console.WriteLine($"--------------");

        /*
        var genres = ... your solution here 

        foreach (var genre in genres) {
            Console.WriteLine($"{genre.Name} {genre.Count}");
        }
        */
        Console.WriteLine($"--------------");

        // Question 4
        // We define remakes of a single movie as the set of movies whose title is the same, but the production years are different.
        // Your result should contain all movies that are remakes (all movies for which we can find at least one other movie with the same title).
        // The result should be sorted ascending by production year and contain only the first 14 movies.
        Console.WriteLine("Expected:");
        Console.WriteLine($"--------------");
        Console.WriteLine("915 Sabrina 1954 Comedy|Romance\r\n7 Sabrina 1995 Comedy|Romance\r\n2848 Othello 1952 Drama\r\n26 Othello 1995 Drama\r\n" +
            "73 Misérables, Les 1995 Drama|Musical\r\n1873 Misérables, Les 1998 Drama\r\n957 Scarlet Letter, The 1926 Drama\r\n191 Scarlet Letter, The 1995 Drama\r\n" +
            "2398 Miracle on 34th Street 1947 Drama\r\n277 Miracle on 34th Street 1994 Drama\r\n2553 Village of the Damned 1960 Horror|Sci-Fi|Thriller\r\n332 Village of the Damned 1995 Horror|Sci-Fi\r\n" +
            "2078 Jungle Book, The 1967 Animation|Children's|Comedy|Musical\r\n362 Jungle Book, The 1994 Adventure|Children's|Romance");
        Console.WriteLine($"--------------");
        Console.WriteLine("Provided:");
        Console.WriteLine($"--------------");

        /*
        var remakes = ... your solution here 

        foreach (var remake in remakes) {
            Console.WriteLine($"{remake.MovieID} {remake.Title} {remake.Year} {remake.Genres} ");
        }
        */
        Console.WriteLine($"--------------");


        // Question 5 
        // Sort (descending) the movies by the average rating of votes.
        // Take into account only movies that have received at least 100 votes The result should contain just the first 20 movies.
        Console.WriteLine("Expected:");
        Console.WriteLine($"--------------");
        Console.WriteLine("Seven Samurai (The Magnificent Seven) (Shichinin no samurai) 4,56\r\nShawshank Redemption, The 4,55\r\nGodfather, The 4,52\r\n" +
            "Close Shave, A 4,52\r\nUsual Suspects, The 4,52\r\nSchindler's List 4,51\r\nWrong Trousers, The 4,51\r\nSunset Blvd. (a.k.a. Sunset Boulevard) 4,49\r\n" +
            "Raiders of the Lost Ark 4,48\r\nRear Window 4,48\r\nPaths of Glory 4,47\r\nStar Wars: Episode IV - A New Hope 4,45\r\nThird Man, The 4,45\r\n" +
            "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb 4,45\r\nWallace & Gromit: The Best of Aardman Animation 4,43\r\nTo Kill a Mockingbird 4,43\r\n" +
            "Double Indemnity 4,42\r\nCasablanca 4,41\r\nSixth Sense, The 4,41\r\nYojimbo 4,40");
        Console.WriteLine($"--------------");
        Console.WriteLine("Provided:");
        Console.WriteLine($"--------------");

        /*
        var distribs5 = ... your solution here 
        
        foreach (var dist in distribs5) {
            Console.WriteLine($"{dist.Title} {dist.AverageRating}");
        }
        */
        Console.WriteLine($"--------------");

    }
}