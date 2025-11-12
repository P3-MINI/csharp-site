---
title: "Filesystem"
weight: 20
---

# Filesystem

The `System.IO` namespace provides a rich set of classes for interacting with the filesystem. This includes the static classes `File`, `Directory`, and `Path`, their instance-based counterparts `FileInfo` and `DirectoryInfo`, and the `DriveInfo` class for retrieving information about mounted drives.

## The `Path` Class (static)

In C#, paths are stored in `string` objects. The `Path` class is a static utility for manipulating these path strings. It allows for handling paths in a cross-platform manner.

*   **Path Manipulation**: `Combine`, `IsPathRooted`, `GetPathRoot`, `GetDirectoryName`, `GetFileName`, `GetFullPath`
*   **Working with Extensions**: `HasExtension`, `GetExtension`, `GetFileNameWithoutExtension`, `ChangeExtension`
*   **Cross-platform Paths**: `DirectorySeparatorChar`, `AltDirectorySeparatorChar`, `PathSeparator`, `VolumeSeparatorChar`, `GetInvalidPathChars`, `GetInvalidFileNameChars`
*   **Working with Temporary Files**: `GetTempPath`, `GetRandomFileName`, `GetTempFileName`

```csharp
string path = Path.Combine("Workspace", "csharp-site", "hugo.yaml");

Console.WriteLine($"Path combined: {path}"); // Workspace/csharp-site/hugo.yaml

Console.WriteLine($"File Name: {Path.GetFileName(path)}"); // hugo.yaml
Console.WriteLine($"Name without extension: {Path.GetFileNameWithoutExtension(path)}"); // hugo
Console.WriteLine($"Extension: {Path.GetExtension(path)}");  // .yaml
Console.WriteLine($"Parent Directory: {Path.GetDirectoryName(path)}"); // Workspace/csharp-site
Console.WriteLine($"Full Path: {Path.GetFullPath(path)}"); // /home/tomasz/Workspace/csharp-site/hugo.yaml (resolves relative to the current working directory)
Console.WriteLine($"Directory Separator: {Path.DirectorySeparatorChar}"); // \ (on Windows), / (on Linux)
```

## File Operations

### The `File` Class (static)

*   **File Manipulation**: `Exists`, `Delete`, `Copy`, `Move`, `Replace`, `CreateSymbolicLink`
*   **Attribute Operations**: `GetAttributes`, `SetAttributes`
*   **Timestamp Operations**: `GetCreationTime`, `GetLastAccessTime`, `GetLastWriteTime`, `SetCreationTime`, `SetLastAccessTime`, `SetLastWriteTime`
*   **Permission Operations**: `GetUnixFileMode`, `SetUnixFileMode`

It provides simple, high-level methods for file operations like copying, moving, or deleting.

```csharp
string path = "lorem.txt";
File.WriteAllText(path, "Lorem ipsum");

File.Copy(path, "copy.txt", overwrite: true);
File.Move("copy.txt", "moved.txt", overwrite: true);

if (File.Exists("moved.txt"))
{
    File.Delete("moved.txt");
}
```

### The `FileInfo` Class (instance)

Represents a specific file as an object, providing properties with information about it and instance methods for operations. It contains a similar set of methods to its static counterpart, along with several additional properties like `Name`, `Length`, `Extension`, etc.

```csharp
string path = "lorem.txt";
File.WriteAllText(path, "Lorem ipsum");

var fileInfo = new FileInfo(path);

if (fileInfo.Exists)
{
    Console.WriteLine($"Name: {fileInfo.Name}");
    Console.WriteLine($"Size: {fileInfo.Length} bytes");
    Console.WriteLine($"Extension: {fileInfo.Extension}");
    Console.WriteLine($"Directory: {fileInfo.DirectoryName}");
    Console.WriteLine($"Read-only: {fileInfo.IsReadOnly}");

    // Copy to a new file
    fileInfo.CopyTo("ipsum.txt", overwrite: true);
}
```

## Directory Operations

### The `Directory` Class (static)

*   **Directory Manipulation**: `CreateDirectory`, `Exists`, `Delete`, `Move`, `CreateSymbolicLink`
*   **Getting Contents**: `GetFiles`, `GetDirectories`, `GetFileSystemEntries`,
*   **Getting Contents (Lazily)**: `EnumerateFiles`, `EnumerateDirectories`, `EnumerateFileSystemEntries`
*   **Working with the Current Directory**: `GetCurrentDirectory`, `SetCurrentDirectory`
*   **Timestamp Operations**: `GetCreationTime`, `GetLastAccessTime`, `GetLastWriteTime`, `SetCreationTime`, `SetLastAccessTime`, `SetLastWriteTime`
*   **Other**: `GetParent`, `GetDirectoryRoot`

Used for performing one-off operations on directories.

```csharp
string newDir = "NewDirectory";

// Create a directory (and all parent directories if they don't exist)
Directory.CreateDirectory(newDir);

if (Directory.Exists(newDir))
{
    Console.WriteLine("Directory exists.");
}

// Listing contents
Console.WriteLine("\nFiles in the current directory:");
string[] files = Directory.GetFiles(".");
foreach (var file in files) Console.WriteLine(file);

Console.WriteLine("\nDirectories in the current directory:");
string[] dirs = Directory.GetDirectories(".");
foreach (var d in dirs) Console.WriteLine(d);

Console.WriteLine("\nAll entries in the current directory:");
string[] entries = Directory.GetFileSystemEntries(".");
foreach (var e in entries) Console.WriteLine(e);

Directory.Delete(newDir, recursive: true);
```

The `Get*` and `Enumerate*` methods are overloaded to accept a search pattern (`string searchPattern`) and additional options (`SearchOption options`). Passing `SearchOption.AllDirectories` will cause the search to be recursive. For example, to recursively search for all PDF files in the current directory:

```csharp
IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(".", "*.pdf", SearchOption.AllDirectories);
foreach (var e in entries) Console.WriteLine(e);
```

> [!NOTICE]
> **`EnumerateFiles` vs `GetFiles`**
> The `GetFiles` and `GetDirectories` methods return the entire collection (an array) of names at once. If a directory contains thousands of files, this can consume a lot of time and memory. Their counterparts, `EnumerateFiles` and `EnumerateDirectories`, work "lazily" by returning an `IEnumerable<string>`. Items are retrieved as you iterate through them, which can be more efficient for large directories.

### The `DirectoryInfo` Class (instance)

Represents a specific directory. It's convenient when you need to perform multiple operations on the same directory.

```csharp
var dirInfo = new DirectoryInfo("."); // Current directory

Console.WriteLine($"Name: {dirInfo.Name}");
Console.WriteLine($"Full Path: {dirInfo.FullName}");
Console.WriteLine($"Parent Folder: {dirInfo.Parent?.FullName}");
Console.WriteLine($"Root: {dirInfo.Root}");

DirectoryInfo subDir = dirInfo.CreateSubdirectory("Subdir");

FileInfo[] filesInDir = dirInfo.GetFiles("*.dll");
Console.WriteLine("\nDLL files in the directory:");
foreach (var f in filesInDir)
    Console.WriteLine($"    {f.Name}");

Console.WriteLine("\nSubdirectories in the directory:");
DirectoryInfo[] dirsInDir = dirInfo.GetDirectories();
foreach (var d in dirsInDir)
    Console.WriteLine($"    {d.Name}");
```

### Special Folders

The `Environment.GetFolderPath` method allows you to retrieve paths to special, predefined system folders, such as the user's home directory or desktop, in a cross-platform way.

```csharp
string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
```

## The `DriveInfo` Class

The `DriveInfo` class allows you to get information about the mounted drives in the system.

```csharp
const long GB = 1024 * 1024 * 1024;
DriveInfo root = new DriveInfo("/");

Console.WriteLine($"Total size: {root.TotalSize / GB} GB");
Console.WriteLine($"Free size: {root.TotalFreeSpace / GB} GB"); // Ignoring quotas
Console.WriteLine($"Available size: {root.AvailableFreeSpace / GB} GB");

foreach (DriveInfo drive in DriveInfo.GetDrives())
{
    Console.WriteLine(drive.Name);
}
```

## Filesystem Events

The `FileSystemWatcher` class allows you to listen for filesystem events. For a given path, it can monitor for the creation, deletion, modification, or renaming of files and directories. Under the hood, it uses solutions like `inotify` on Linux or similar OS-specific APIs.

```csharp
namespace FileSystemEvents;

class Program
{
    static void Main(string[] args)
    {
        Watch(".", "*", false);
    }
    
    static void Watch(string path, string filter, bool includeSubDirs)
    {
        using var watcher = new FileSystemWatcher(path, filter);
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;
        watcher.IncludeSubdirectories = includeSubDirs;
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Listening for events - press <enter> to finish");
        Console.ReadLine();
    }
    
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"Changed: {e.FullPath}");
    }

    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        Console.WriteLine(value);
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"Deleted: {e.FullPath}");
    }

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Renamed:");
        Console.WriteLine($"    Old: {e.OldFullPath}");
        Console.WriteLine($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine(e.GetException());
    }
}
```

> Internally, `FileSystemWatcher` starts a separate thread to raise events. Therefore, the event handlers will be executed asynchronously on a background thread.

> The `Error` event notifies about internal errors within the `FileSystemWatcher` itself, not filesystem errors. This can happen if many changes occur at once, causing an overflow of the internal buffer used to queue events. In such a situation, the `Error` event informs us that some events may have been lost.