namespace MyProject;

class Program
{
    static void Main(string[] args)
    {
        string greeting = File.ReadAllText("greeting.txt");
        Console.WriteLine(greeting);
    }
}