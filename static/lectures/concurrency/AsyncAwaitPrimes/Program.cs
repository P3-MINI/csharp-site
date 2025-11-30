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

    static async Task<int> CountPrimesAsync(int start, int end)
    {
        int count = 0;

        for (int i = start; i < end; i++)
        {
            if (await IsPrime(i))
            {
                count++;
            }
        }

        return count;
    }

    static async Task<bool> IsPrime(int number)
    {
        if (number < 2)
            return false;

        return await Task.Run(() => 
        {
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        });
    }
}