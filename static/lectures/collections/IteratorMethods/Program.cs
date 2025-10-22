namespace IteratorMethods;

class Program
{
    static void Main(string[] args)
    {
        var sequence = Odds(Fibonacci(10));
        Console.WriteLine("Odd elements of the fibonacci sequence:");
        foreach (var item in sequence)
        {
            Console.WriteLine(item);
        }
    }
    
    public static IEnumerable<int> Fibonacci(int n)
    {
        int current = 0, next = 1;
        for (int i = 0; i < n; i++)
        {
            yield return current;
            (current, next) = (next, current + next);
        }
    }

    public static IEnumerable<int> Odds(IEnumerable<int> sequence)
    {
        foreach (var item in sequence)
        {
            if (item % 2 == 1) yield return item;
        }
    }
}
