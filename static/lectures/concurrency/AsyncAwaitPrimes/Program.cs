namespace AyncAwaitPrimes;

class Program
{
    private static async Task Main()
    {
        Task<int> task1 = CountPrimesAsync(0, 1000);
        Task<int> task2 = CountPrimesAsync(1000, 2000);
        Console.WriteLine($"Primes(0-1000): {await task1}");
        Console.WriteLine($"Primes(1000-2000): {await task2}");
    }

    private static async Task<int> CountPrimesAsync(int start, int end)
    {
        return await Task.Run(() =>
        {
            int count = 0;
            
            for (int i = start; i < end; i++)
            {
                if (IsPrime(i))
                {
                    count++;
                }
            }
            
            return count;
        });
    }
    
    static bool IsPrime(int number)
    {
        if (number < 2)
            return false;
        
        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0)
                return false;
        }
        
        return true;
    }
}