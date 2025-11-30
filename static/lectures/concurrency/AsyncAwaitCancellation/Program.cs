namespace AsyncAwaitCancellation;

class Program
{
    private static async Task Main()
    {
        var cancellationSource = new CancellationTokenSource(5000);
        try
        {
            List<int> primes = await GetPrimesAsync(2, cancellationSource.Token);
            Console.WriteLine($"Number of primes: {primes.Count}");
            Console.WriteLine($"Last prime: {primes[^1]}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Canceled");
        }
    }
    
    static async Task<List<int>> GetPrimesAsync(int start, CancellationToken token)
    {
        List<int> primes = [];

        for (int i = start; i < int.MaxValue; i++)
        {
            // if (token.IsCancellationRequested) break;
            token.ThrowIfCancellationRequested();
            if (await IsPrime(i, token))
            {
                primes.Add(i);
            }
        }

        return primes;
    }

    static async Task<bool> IsPrime(int number, CancellationToken token)
    {
        if (number < 2)
            return false;
        
        return await Task.Run(() =>
        {
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                // if (token.IsCancellationRequested) break;
                token.ThrowIfCancellationRequested();
                if (number % i == 0)
                    return false;
            }

            return true;
        });
    }
}