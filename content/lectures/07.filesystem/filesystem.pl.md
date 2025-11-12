---
title: "System plików"
weight: 20
---

# System plików

Przestrzeń nazw `System.IO` dostarcza bogaty zestaw klas do interakcji z systemem plików. Służą do tego statyczne klasy `File`, `Directory` i `Path` oraz ich instancyjne odpowiedniki `FileInfo`, `DirectoryInfo`. Dodatkowo, klasa `DriveInfo` pozwala na uzyskanie informacji o zamontowanych dyskach.

## Klasa `Path` (statyczna)

Ścieżki w C# przechowujemy w obiektach typu **`string`**. Klasa `Path` to statyczne narzędzie do manipulacji ścieżkami (w postaci stringów). Pozwala na operacje na ścieżkach w sposób niezależny od systemu operacyjnego.

* **Manipulacja ścieżek**: **`Combine`**, `IsPathRooted`, `GetPathRoot`, `GetDirectoryName`, `GetFileName`, `GetFullPath`
* **Praca z rozszerzeniami**: `HasExtension`, `GetExtension`, `GetFileNameWithoutExtension`, `ChangeExtension`
* **Ścieżki niezależne od systemu**: `DirectorySeparatorChar`, `AltDirectorySeparatorChar`, `PathSeparator`, `VolumeSeparatorChar`, `GetInvalidPathChars`, `GetInvalidFileNameChars`
* **Praca z tymczasowymi plikami**: `GetTempPath`, `GetRandomFileName`, `GetTempFileName`

```csharp
string path = Path.Combine("Workspace", "csharp-site", "hugo.yaml");

Console.WriteLine($"Path combined: {path}"); // Workspace/csharp-site/hugo.yaml

Console.WriteLine($"File Name: {Path.GetFileName(path)}"); // hugo.yaml
Console.WriteLine($"Name without extension: {Path.GetFileNameWithoutExtension(path)}"); // hugo
Console.WriteLine($"Extension: {Path.GetExtension(path)}");  // .yaml
Console.WriteLine($"Parent Directory: {Path.GetDirectoryName(path)}"); // Workspace/csharp-site
Console.WriteLine($"Full Path: {Path.GetFullPath(path)}"); // /home/tomasz/Workspace/csharp-site/hugo.yaml (resolves relative to the current working directory)
Console.WriteLine($"Directory Separator: {Path.DirectorySeparatorChar}"); // \ (on Windows) / (on Linux)
```

## Operacje na plikach

### Klasa `File` (statyczna)

* **Manipulowanie plikami**: `Exists`, `Delete`, `Copy`, `Move`, `Replace`, `CreateSymbolicLink`
* **Operacje na atrybutach**: `GetAttributes`, `SetAttributes`
* **Operacje na znacznikach czasu**: `GetCreationTime`, `GetLastAccessTime`, `GetLastWriteTime`, `SetCreationTime`, `SetLastAccessTime`, `SetLastWriteTime`
* **Operacje na uprawnieniach**: `GetUnixFileMode`, `SetUnixFileMode`

Umożliwia proste, wysokopoziomowe operacje na plikach, takie jak kopiowanie, przenoszenie czy usuwanie.

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

### Klasa `FileInfo` (instancyjna)

Reprezentuje konkretny plik jako obiekt i udostępnia informacje o nim oraz metody do operacji. Zawiera analogiczny zbiór metod co statyczny odpowiednik oraz kilka dodatkowych właściwości: `Name`, `Length`, `Extension`, etc.

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

## Operacje na katalogach

### Klasa `Directory` (statyczna)

* **Manipulacja katalogami**: `CreateDirectory`, `Exists`, `Delete`, `Move`, `CreateSymbolicLink`
* **Pobieranie zawartości**: `GetFiles`, `GetDirectories`, `GetFileSystemEntries`,
* **Pobieranie zawartości (leniwie)**: `EnumerateFiles`, `EnumerateDirectories`, `EnumerateFileSystemEntries`
* **Praca z obecnym katalogiem**: `GetCurrentDirectory`, `SetCurrentDirectory`
* **Operacje na znacznikach czasu**: `GetCreationTime`, `GetLastAccessTime`, `GetLastWriteTime`, `SetCreationTime`, `SetLastAccessTime`, `SetLastWriteTime`
* **Inne**: `GetParent`, `GetDirectoryRoot`

Służy do wykonywania jednorazowych operacji na folderach.

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

Metody `Get*` i `Enumerate*` są przeciążone i umożliwiają także przekazanie wzorca (`string searchPattern`) oraz dodatkowych opcji (`SearchOption options`). Przekazanie opcji `SearchOption.AllDirectories` spowoduje rekursywne przeszukanie katalogu. Na przykład rekursywne przeszukiwanie wszystkich plików pdf w bieżącym katalogu:

```csharp
IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(".", "*.pdf", SearchOption.AllDirectories);
foreach (var e in entries) Console.WriteLine(e);
```

> [!NOTICE]
> **`EnumerateFiles` vs `GetFiles`**
> Metody `GetFiles` i `GetDirectories` zwracają od razu całą kolekcję (tablicę) nazw. Jeśli katalog zawiera tysiące plików, może to zająć dużo czasu i pamięci. Ich odpowiedniki, `EnumerateFiles` i `EnumerateDirectories`, działają w sposób "leniwy", zwracając `IEnumerable<string>`. Elementy są pobierane w miarę iterowania, co może być bardziej efektywne dla dużych katalogów.

### Klasa `DirectoryInfo` (instancyjna)

Reprezentuje konkretny katalog. Wygodne, gdy wykonujemy na nim wiele operacji.

```csharp
var dirInfo = new DirectoryInfo("."); // Current directory

Console.WriteLine($"Name: {dirInfo.Name}");
Console.WriteLine($"Full Path: {dirInfo.FullName}");
Console.WriteLine($"Parent Folder: {dirInfo.Parent?.FullName}");
Console.WriteLine($"Root: {dirInfo.Root}");

DirectoryInfo subDir = dirInfo.CreateSubdirectory("Subdir");

FileInfo[] filesInDir = dirInfo.GetFiles("*.dll");
Console.WriteLine("DLL files in the directory:");
foreach (var f in filesInDir)
    Console.WriteLine($"    {f.Name}");

Console.WriteLine("Subdirectories in the directory:");
DirectoryInfo[] dirsInDir = dirInfo.GetDirectories();
foreach (var d in dirsInDir)
    Console.WriteLine($"    {d.Name}");
```

### Specjalne katalogi

Metoda `Environment.GetFolderPath` pozwala na pobranie ścieżek do specjalnych, predefiniowanych katalogów systemowych, takich jak katalog domowy, pulpit, itp., w sposób niezależny od systemu operacyjnego.

```csharp
string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
```

## Klasa `DriveInfo`

Klasa `DriveInfo` pozwala uzyskać informacje o zamontowanych dyskach w systemie.

```csharp
const int GB = 1024 * 1024 * 1024;
DriveInfo root = new DriveInfo("/");

Console.WriteLine($"Total size: {root.TotalSize / GB} GB");
Console.WriteLine($"Free size: {root.TotalFreeSpace / GB} GB"); // Ignoring quotas
Console.WriteLine($"Available size: {root.AvailableFreeSpace / GB} GB");

foreach (DriveInfo drive in DriveInfo.GetDrives())
{
    Console.WriteLine(drive.Name);
}
```

## Zdarzenia systemu pliku

Klasa `FileSystemWatcher` pozwala na nasłuchiwanie zdarzeń systemu pliku. Dla określonej ścieżki pozwala na monitorowanie zdarzeń utworzenia, usunięcia, modyfikacji, lub zmiany nazwy pliku lub katalogu. Jest to rozwiązanie pod spodem korzystające z `inotify` na systemie Linux lub innych podobnych w zależności od systemu operacyjnego.

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

> Wewnętrznie `FileSystemWatcher` uruchamia osobny wątek na którym będzie powiadamiać o zdarzeniach. Tym samym obsługa zdarzeń będzie uruchamiana asynchronicznie na osobnym wątku.

> Zdarzenie `Error` służy powiadamianiu o wewnętrznych błędach samego `FileSystemWatcher`a, a nie błędów systemu plików. Może to się stać, gdy na raz zachodzi wiele zmian, która spowodowałaby przepełnienie wewnętrznego bufora używanego do kolejkowania zdarzeń. W takiej sytuacji zdarzeniem `Error` zostaniemy poinformowani, że jakieś zdarzenia mogły się zgubić.