using tasks.Databases;

namespace tasks;

public sealed class Program
{
    static void Main(string[] args)
    {
        var database = new SampleMovieDatabase();
        database.RunQueries();
        PressEnterToContinue();
    }

    private  static void PressEnterToContinue()
    {
        Console.WriteLine("Press ENTER key to continue...");
        Console.ReadLine();
    }
}