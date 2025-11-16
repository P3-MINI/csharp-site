using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace EmbeddingResources;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("es");
        WorkingWithEmbeddedResources();
        WorkingWithLocalizedResources();
    }

    public static void WorkingWithEmbeddedResources()
    {
        PrintCurrentMethodName();
        Assembly? assembly = Assembly.GetEntryAssembly();
        if (assembly is null) return;
        using Stream? stream = assembly.GetManifestResourceStream("EmbeddingResources.lorem.txt");
        if (stream is null)
        {
            Console.WriteLine(Resources.embedded_resource_not_found);
            return;
        }
        using StreamReader reader = new StreamReader(stream);

        while (reader.ReadLine() is {} line)
        {
            Console.WriteLine(line);
        }
    }

    public static void WorkingWithLocalizedResources()
    {
        PrintCurrentMethodName();
        ResourceManager rm = new ResourceManager("EmbeddingResources.Resources", typeof(Program).Assembly);
        string? greeting = rm.GetString("greeting");
        string? welcome = rm.GetString("welcome-message");
        Console.WriteLine(greeting);
        Console.WriteLine(welcome);
    }
    
    private static void PrintCurrentMethodName([CallerMemberName] string caller = "")
    {
        Console.WriteLine("***************************************");
        Console.WriteLine($"* Method: {caller,27} *");
        Console.WriteLine("***************************************");
    }
}
