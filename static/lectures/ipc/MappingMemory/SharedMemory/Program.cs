#if OS_LINUX || OS_MAC
using System.IO.MemoryMappedFiles;

namespace SharedMemory;

public class Program
{
    private const string ShmFile = "/dev/shm/shared_memory.shm";
    private const string MutexName = "Global\\shared_memory_mutex";
    
    public static void Main(string[] args)
    {
        if (args is ["writer"])
        {
            Writer();
        }
        else if (args is ["reader"])
        {
            Reader();
        }
        else
        {
            Usage();
        }
    }

    public static void Usage()
    {
        Console.WriteLine($"{Environment.GetCommandLineArgs()[0]} [reader|writer]");
        Environment.Exit(-1);
    }
    
    public static void Writer()
    {
        using Mutex mutex = new Mutex(true, MutexName);
        using var mmf = MemoryMappedFile.CreateFromFile(ShmFile, FileMode.Create, null, 2000);
        using var accessor = mmf.CreateViewAccessor();
        for (int i = 0; i < 500; i++)
        {
            accessor.Write(i * sizeof(int), i * i);
        }
        mutex.ReleaseMutex();
        Console.ReadLine();
    }
    
    public static void Reader()
    {
        Mutex mutex = Mutex.OpenExisting(MutexName);
        mutex.WaitOne();
        using var mmf = MemoryMappedFile.CreateFromFile(ShmFile, FileMode.Open);
        using var accessor = mmf.CreateViewAccessor();
        for (int i = 0; i < 500; i++)
        {
            int num = accessor.ReadInt32(i * sizeof(int));
            Console.WriteLine(num);
        }
        mutex.ReleaseMutex();
    }
}
#elif OS_WINDOWS
using System.IO.MemoryMappedFiles;

namespace SharedMemory;

public class Program
{
    private const string MapName = "SharedMemory";
    private const string MutexName = "Global\\shared_memory_mutex";
    
    public static void Main(string[] args)
    {
        if (args is ["writer"])
        {
            Writer();
        }
        else if (args is ["reader"])
        {
            Reader();
        }
        else
        {
            Usage();
        }
    }

    public static void Usage()
    {
        Console.WriteLine($"{Environment.GetCommandLineArgs()[0]} [reader|writer]");
        Environment.Exit(-1);
    }

    public static void Writer()
    {
        using Mutex mutex = new Mutex(true, MutexName);
        using var mmf = MemoryMappedFile.CreateOrOpen(MapName, 2000);
        using var accessor = mmf.CreateViewAccessor();
        for (int i = 0; i < 500; i++)
        {
            accessor.Write(i * sizeof(int), i * i);
        }
        mutex.ReleaseMutex();
        Console.ReadLine();
    }

    public static void Reader()
    {
        Mutex mutex = Mutex.OpenExisting(MutexName);
        mutex.WaitOne();
        using var mmf = MemoryMappedFile.OpenExisting(MapName);
        using var accessor = mmf.CreateViewAccessor();
        for (int i = 0; i < 500; i++)
        {
            int num = accessor.ReadInt32(i * sizeof(int));
            Console.WriteLine(num);
        }
        mutex.ReleaseMutex();
    }
}
#endif