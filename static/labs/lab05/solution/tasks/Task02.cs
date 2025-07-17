namespace tasks;

public sealed class Task02 : IExecutable
{
    public void Execute(string[] args)
    {
        foreach (var prime in SieveOfEratosthenes(1000))
        {
            Console.WriteLine(prime);
        }
    }

    public static IEnumerable<int> SieveOfEratosthenes(int upperBound)
    {
        if (upperBound < 2)
            yield break;

        var isComposite = new bool[upperBound + 1];

        for (var i = 2; i <= upperBound; i++)
        {
            if (isComposite[i]) continue;

            yield return i;

            for (var j = i * 2; j <= upperBound; j += i)
            {
                isComposite[j] = true;
            }
        }
    }

}