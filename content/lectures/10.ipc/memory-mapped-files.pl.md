---
title: "Mapowanie pamięci"
weight: 50
---

## Mapowanie plików i pamięć dzielona

Mapowanie plików (*memory-mapped files*) to mechanizm systemu operacyjnego, który pozwala na mapowanie zawartości pliku bezpośrednio do przestrzeni adresowej procesu. Dzięki temu można odczytywać i modyfikować zawartość pliku tak, jakby był on tablicą w pamięci.

Główne zastosowania to:
1. **Wydajny dostęp do dużych plików**: Pliki mapowane w pamięci pozwalają na dostęp do zawartości pliku tak, jakby był on w całości załadowany do pamięci, nawet jeśli jest znacznie większy niż dostępna pamięć RAM. System operacyjny dynamicznie wczytuje i zwalnia odpowiednie fragmenty pliku, gdy są one potrzebne. Dzięki temu można losowo odczytywać i modyfikować ogromne pliki bez konieczności wczytywania ich w całości.
2. **Komunikacja międzyprocesowa (IPC)**: Umożliwia współdzielenie danych między wieloma procesami poprzez mapowanie tego samego pliku (lub obszaru pamięci) do ich przestrzeni adresowych. Jest to jeden z najszybszych sposobów komunikacji międzyprocesowej.

## Mapowanie plików w C#

W .NET do pracy z plikami mapowanymi w pamięci służy klasa `MemoryMappedFile` z przestrzeni nazw `System.IO.MemoryMappedFiles`.

### Zapis i odczyt pliku

Żeby pisać lub czytać z pliku należy pobrać z pliku obiekt "widoku" `ViewAccessor`. Pozwala on na odczyt i zapis typów podstawowych, tablic i struktur w określonym fragmencie pliku.
Poniższy przykład pokazuje, jak stworzyć plik mapowany w pamięci, zapisać do niego dane, a następnie je odczytać.

```csharp
using System.IO.MemoryMappedFiles;
using System.Text;

long fileSize = 1024;
string filePath = "test.dat";
string message = "Hello, world!";

using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Create, null, fileSize);
using (var accessor = mmf.CreateViewAccessor(0, message.Length * 2, MemoryMappedFileAccess.Write))
{
    byte[] buffer = Encoding.Unicode.GetBytes(message);
    accessor.WriteArray(0, buffer, 0, buffer.Length);
}

using (var accessor = mmf.CreateViewAccessor(0, message.Length * 2, MemoryMappedFileAccess.Read))
{
    byte[] buffer = new byte[message.Length * 2];
    accessor.ReadArray(0, buffer, 0, buffer.Length);
    string readMessage = Encoding.Unicode.GetString(buffer);
    Console.WriteLine(readMessage);
}
```

Przy losowym odczycie/zapisie taki sposób pracy z plikiem jest około ~10 razy szybszy w stosunku do strumieni. Sytuacja jest odwrotna przy sekwencyjnym odczycie/zapisie.

## Pamięć dzielona (*Shared Memory*)

Pamięć dzielona to specjalny przypadek użycia plików mapowanych, gdzie celem nie jest praca z plikiem na dysku, ale **bezpośrednia wymiana danych między procesami**. W tym scenariuszu system operacyjny może utworzyć mapowanie, które nie jest powiązane z żadnym fizycznym plikiem.

Jest to bardzo wydajna forma IPC, dane nie są kopiowane między procesami – oba procesy operują na tym samym bloku fizycznej pamięci.

### Przykład komunikacji IPC

Nie jest to mechanizm wieloplatformowy. Windows wspiera nazwane mapowania, które tworzą pamięć dzieloną, do której później inne procesy mogą się odwoływać za pomocą wspólnej nazwy. Na Linuxie żeby stworzyć pamięć dzieloną można ją stworzyć jako plik w `/dev/shm`. Jest to tymczasowy system plików zamontowany w pamięci RAM.

{{< tabs >}}
{{% tab "Linux" %}} 
```csharp
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
```
{{% /tab %}}
{{% tab "Windows" %}} 
```csharp
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
        using var mmf = MemoryMappedFile.CreateOrOpen(MapName, 2000); // Not supported on Linux
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
```
{{% /tab %}}
{{< /tabs >}}

> [!WARNING]
> ## Synchronizacja
> Kiedy wiele procesów (lub wątków) ma dostęp do tego samego obszaru pamięci, może być potrzebna **synchronizacja**. Bez niej może dojść do sytuacji wyścigu (*race condition*), gdzie jeden proces próbuje odczytać dane, które są w trakcie modyfikacji przez inny.
> W powyższym przykładzie użyto klasy `Mutex` do zapewnienia, że w danym momencie tylko jeden proces ma dostęp do współdzielonego obszaru.

> [!INFO]
> ## Kod źródłowy
> {{< filetree dir="lectures/ipc/MappingMemory" >}}
