---
title: "Memory Mapping"
weight: 50
---

## Memory-Mapped Files and Shared Memory

Memory-mapped files are an operating system mechanism that allows mapping a file's content directly into a process's address space. This allows you to read and modify the file's content as if it were an array in memory.

The main use cases are:
1.  **Efficient access to large files**: Memory-mapped files allow access to a file's content as if it were entirely loaded into memory, even if it is much larger than the available RAM. The operating system dynamically loads and unloads the appropriate portions of the file as they are needed. This allows for random reading and modification of huge files without having to load them entirely.
2.  **Inter-Process Communication (IPC)**: It enables data sharing between multiple processes by mapping the same file (or memory region) into their address spaces. This is one of the fastest methods of inter-process communication.

## Memory-Mapped Files in C#

In .NET, the `MemoryMappedFile` class from the `System.IO.MemoryMappedFiles` namespace is used to work with memory-mapped files.

### Writing to and Reading from a File

To write to or read from a file, you need to get a "view" object, `ViewAccessor`, from the file. It allows reading and writing primitive types, arrays, and structures in a specific portion of the file.
The following example shows how to create a memory-mapped file, write data to it, and then read it back.

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

For random reads/writes, this way of working with a file is about ~10 times faster than using streams. The situation is reversed for sequential reads/writes.

## Shared Memory

Shared memory is a special use case of memory-mapped files where the goal is not to work with a file on disk, but the **direct exchange of data between processes**. In this scenario, the operating system can create a mapping that is not associated with any physical file.

This is a very efficient form of IPC, as data is not copied between processes—both processes operate on the same block of physical memory.

### IPC Communication Example

This is not a cross-platform mechanism. Windows supports named mappings that create shared memory, which other processes can later reference using a common name. On Linux, to create shared memory, you can create it as a file in `/dev/shm`. This is a temporary file system mounted in RAM.

{{< tabs >}}
{{% tab "Linux" %}}
```csharp
using System.IO.MemoryMappedFiles;

namespace SharedMemory;

public class Program
{
    private const string ShmFile = "/dev/shm/shared_memory.shm";
    private const string MutexName = "Global\shared_memory_mutex";
    
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
    private const string MutexName = "Global\shared_memory_mutex";
    
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
> ## Synchronization
> When multiple processes (or threads) have access to the same memory area, **synchronization** may be necessary. Without it, a race condition can occur, where one process tries to read data that is being modified by another.
> In the example above, the `Mutex` class is used to ensure that only one process can access the shared area at a time.

> [!INFO] 
> ## Source Code
> {{< filetree dir="lectures/ipc/MappingMemory" >}}
