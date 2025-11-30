using System.Runtime.CompilerServices;

namespace AsyncAwaitThreads;

class Program
{
    private static async Task Main()
    {
        Thread.CurrentThread.Name = "Main";

        PrintCurrentThread();

        Task<string> task = HelloWorldAsync();

        PrintCurrentThread();

        string hello = await task;
        
        PrintCurrentThread();

        Console.WriteLine(hello);
    }

    static async Task<string> HelloWorldAsync()
    {
        PrintCurrentThread();

        await Task.Delay(3000);

        PrintCurrentThread();
        
        return "Hello, World";
    }

    private static void PrintCurrentThread([CallerLineNumber] int line = -1)
    {
        var current = Thread.CurrentThread;
        Console.WriteLine($"Thread(line {line}): {current.Name} : {current.ManagedThreadId}");
    }
}