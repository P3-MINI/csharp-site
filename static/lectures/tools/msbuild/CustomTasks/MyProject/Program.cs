namespace MyProject;

class Program
{
    static void Main(string[] args)
    {
        string greeting = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "greeting.txt"));
        Console.WriteLine(greeting);
    }
}