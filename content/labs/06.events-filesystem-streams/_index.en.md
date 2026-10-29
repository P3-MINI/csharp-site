---
title: "Events, filesystem, streams"
weight: 60
---

## Tutorial 6: Events, Filesystem, Streams

The scope of this laboratory covers working with the filesystem (paths, files, directories, `FileSystemWatcher`), streams (reading and writing files, stream adapters and decorators, compression, ZIP archives), releasing resources with `IDisposable` and `using`, and events (`event`, `EventHandler<T>`, custom `EventArgs`).

As in C++, a C# program talks to the operating system through a library – here it is the `System.IO` namespace. The concepts will feel familiar (`std::filesystem`, `std::fstream`), but there are two important differences. First, C# has no destructors that run deterministically at the end of a scope, so releasing files and other resources is done explicitly with `IDisposable` and `using`. Second, the language has built-in support for the observer pattern: **events**. Before starting the task, we will briefly go through the key concepts.

### Paths and Directories

In C#, paths are ordinary `string`s. To manipulate them, use the static `Path` class instead of concatenating strings by hand – it knows the directory separator of the current operating system (`/` on Linux and macOS, `\` on Windows):

```csharp
string path = Path.Combine("data", "2026", "measurements.csv");
Console.WriteLine(path);                         // data/2026/measurements.csv (data\2026\measurements.csv on Windows)
Console.WriteLine(Path.GetFileName(path));       // measurements.csv
Console.WriteLine(Path.GetExtension(path));      // .csv
Console.WriteLine(Path.GetDirectoryName(path));  // data/2026
Console.WriteLine(Path.GetFullPath(path));       // e.g. /home/student/data/2026/measurements.csv

string root = Path.GetFullPath("data");
Console.WriteLine(Path.GetRelativePath(root, Path.GetFullPath(path))); // 2026/measurements.csv
```

Relative paths are resolved against the **current working directory** of the process – the directory from which you started the program, not the directory of the executable.

Files and directories can be accessed through static classes (`File`, `Directory`) or through objects describing a particular file or directory (`FileInfo`, `DirectoryInfo`). Directory contents can be listed in two ways:

- `GetFiles`, `GetDirectories` – return an array with all entries at once,
- `EnumerateFiles`, `EnumerateDirectories` – return a lazy `IEnumerable<T>`; entries are read from the disk as you iterate over them, so you can start processing the first file before the whole tree is scanned.

More details can be found in the [Filesystem lecture]({{< ref "/lectures/07.filesystem/filesystem" >}}).

### Streams and Releasing Resources

A **stream** (`Stream`) is an abstraction of a sequence of bytes that can be read, written and (sometimes) positioned. The same API is used for files (`FileStream`), memory (`MemoryStream`), network connections, or compressed data. Streams operate on bytes; to work with text, wrap a stream in an **adapter** (`StreamReader`, `StreamWriter`) – similarly to how `std::ifstream` combines a file buffer with formatted text input in C++.

An open file is an operating system resource, which the Garbage Collector knows nothing about. Objects owning such resources implement the `IDisposable` interface, and the `using` statement guarantees that their `Dispose()` method is called when the scope ends – even if an exception is thrown. This is the C# counterpart of RAII:

```csharp
// Count the lines of a (possibly huge) file without loading it into memory
using FileStream stream = File.OpenRead("data/measurements.csv"); // closed when the scope ends
using var reader = new StreamReader(stream);                      // adapter: bytes -> text
int lines = 0;
while (reader.ReadLine() is not null)
{
    lines++;
}
Console.WriteLine(lines); // 1441
```

More details can be found in the [Streams]({{< ref "/lectures/07.filesystem/streams" >}}) and [Disposable]({{< ref "/lectures/05.csharp-features/disposable" >}}) lectures.

### Delegates and Events

A **delegate** is a type-safe reference to a method (together with the object it should be called on) – a counterpart of `std::function` from C++. Delegates are **multicast**: with `+=` you can attach several methods, which are then called one after another.

An **event** is a delegate field exposed with the `event` keyword. From the outside, only `+=` (subscribe) and `-=` (unsubscribe) are allowed; only the class that declares the event can raise it. By convention, events use the `EventHandler<TEventArgs>` delegate, i.e. methods with the signature `void Handler(object? sender, TEventArgs e)`, where `sender` is the object raising the event and `e` carries its data (a class derived from `EventArgs`):

```csharp
var countdown = new Countdown(3);

// Subscribing: a lambda and a method group
countdown.Tick += (sender, e) => Console.WriteLine($"{e.Remaining}...");
countdown.Finished += OnFinished;

countdown.Run(); // prints: 3... 2... 1... Liftoff!

countdown.Finished -= OnFinished; // unsubscribing
// countdown.Finished(countdown, EventArgs.Empty); // error CS0070: only the owner can raise the event

static void OnFinished(object? sender, EventArgs e) => Console.WriteLine("Liftoff!");

public class TickEventArgs(int remaining) : EventArgs
{
    public int Remaining { get; } = remaining;
}

public class Countdown(int start)
{
    public event EventHandler<TickEventArgs>? Tick;
    public event EventHandler? Finished;

    public void Run()
    {
        for (int i = start; i > 0; i--)
        {
            OnTick(new TickEventArgs(i));
        }
        OnFinished(EventArgs.Empty);
    }

    // null when nobody has subscribed - hence ?.Invoke
    protected virtual void OnTick(TickEventArgs e) => Tick?.Invoke(this, e);
    protected virtual void OnFinished(EventArgs e) => Finished?.Invoke(this, e);
}
```

More details can be found in the [Delegates]({{< ref "/lectures/05.csharp-features/delegates" >}}) and [Events]({{< ref "/lectures/05.csharp-features/events" >}}) lectures.

## Task 1 - Snapshot: an Incremental Backup Tool

In this task, you will build **Snapshot** – a simple console tool for making incremental backups of a directory. It will consist of a single project – an executable application with several commands:

```bash
Snapshot scan <source>                                     # summary of a directory tree
Snapshot backup <source> <repository> [max-file-size-KB]   # make a backup
Snapshot restore <repository> <target> [snapshot-name]     # restore the files
Snapshot watch <source> <repository>                       # back up automatically after every change
```

An **incremental** backup stores only what has changed since the previous backup. Every backup creates a *snapshot*: the first one contains all files, each subsequent one only the new and modified files. Every snapshot also contains a **manifest** – the list of all files with their sizes and SHA-256 hashes. By comparing the manifests, the program knows which files were added, modified or deleted, and it can restore the state of the directory from any moment.

#### Sample Data

The task comes with a small sample directory (a fictional weather station project, ~100 KB), which we will back up:

> [!NOTE]
> #### Sample directory
> {{< filetree dir="labs/w06.events-filesystem-streams/sample" >}}

Create a solution with a console project and extract the sample directory into the solution directory, **next to the project directory**:

```bash
dotnet new sln -n Snapshot
dotnet new console -n Snapshot
dotnet sln add Snapshot/Snapshot.csproj
```

```text
Snapshot/              <- solution directory
├── Snapshot.slnx
├── Snapshot/          <- project directory, run all commands from here
│   └── Snapshot.csproj
└── sample/            <- the sample directory
```

All commands in this tutorial are run from the project directory, so the sample directory is available as `../sample`, and the backups will be stored in `../backups`. To pass arguments to the program, put them after `--`, e.g. `dotnet run -- scan ../sample` (in Rider or Visual Studio, set them as *Program arguments* in the run configuration). During the task, you will modify the sample directory – when you want to start from scratch, simply extract it again.

> [!WARNING]
> Backups and restored files should not be tracked by Git. Remember to add `backups/` and `restored/` to your `.gitignore` file.

### Stage 1: Scanning the Directory Tree

We begin by finding all files in the source directory, together with their sizes. Some files should never be backed up – temporary files, build outputs, caches – so we will skip files matching *ignore patterns*.

> [!IMPORTANT]
> #### `EnumerationOptions`
>
> The `Enumerate*` and `Get*` methods have overloads accepting an `EnumerationOptions` object, which controls the enumeration:
> ```csharp
> var options = new EnumerationOptions
> {
>     RecurseSubdirectories = true, // search the whole tree, not just the top directory
>     IgnoreInaccessible = true,    // skip entries we are not allowed to read (the default)
> };
> foreach (FileInfo file in new DirectoryInfo(path).EnumerateFiles("*", options)) { /* ... */ }
> ```
> By default, `EnumerationOptions` also skips hidden and system files (`AttributesToSkip`). On Linux and macOS, files and directories whose names start with a dot are hidden. Note that the older overloads taking `SearchOption.AllDirectories` do **not** skip hidden files.

#### The FileEntry Record

Create a file named `FileEntry.cs` with a record describing a single file:

```csharp
public sealed record FileEntry(string RelativePath, string FullPath, long Size, DateTime LastWriteTime);
```

`RelativePath` is the path relative to the source directory. We will store it in backups, so it must look the same on every operating system – always use `/` as the separator (e.g. `docs/guide.md`).

#### The DirectoryScanner Class

Create a `DirectoryScanner` class (`DirectoryScanner.cs`) with a primary constructor `DirectoryScanner(string rootDirectory, IReadOnlyList<string> ignorePatterns)`. The class should contain:

1. A read-only property `string RootDirectory`, initialized with the full path of `rootDirectory` (`Path.GetFullPath`).
2. A method `string ToRelativePath(string fullPath)`, converting a full path to a path relative to `RootDirectory` with `/` as the separator. Use `Path.GetRelativePath` and replace `Path.DirectorySeparatorChar` with `'/'` (on Linux, this does nothing).
3. A method `bool IsIgnored(string relativePath)`, returning `true` if **any segment** of the path (a directory or file name) matches **any** ignore pattern. Thanks to that, the pattern `cache` ignores the whole `cache/` directory, and `*.bak` ignores `docs/guide.md.bak`. To match a name against a pattern with `*` and `?` wildcards, use `FileSystemName.MatchesSimpleExpression(pattern, name)` from the `System.IO.Enumeration` namespace.
4. A method `IEnumerable<FileEntry> Scan()`, lazily enumerating all files in the tree. Use `DirectoryInfo.EnumerateFiles` with the options shown above and return the files that are not ignored with `yield return`. `FileInfo` provides the size (`Length`) and the time of the last modification (`LastWriteTime`).

> [!TIP]
> `Scan()` is an iterator method, so its body – including the call to `EnumerateFiles` – runs only when someone starts iterating over the result. An error such as a missing directory surfaces in the `foreach` loop, not at the call to `Scan()`. Note also that `IsIgnored` filters files only after they are found: the enumeration still walks through ignored directories. For huge directories (e.g. `node_modules`), you would write a recursive scan that does not enter them at all.

#### Human-Readable Sizes

In `SizeFormatting.cs`, create a static class with an extension method `string ToHumanSize(this long bytes)`. Divide the size by 1024 as long as it is at least 1024, choosing the unit from `B`, `KB`, `MB`, `GB`, `TB`. Format bytes as an integer (`749 B`) and other units with one decimal place (`52.7 KB`). Use `CultureInfo.InvariantCulture`, so that the decimal separator is always a dot, regardless of the system settings:

```csharp
string.Create(CultureInfo.InvariantCulture, $"{size:0.0} {Units[unit]}")
```

#### The `scan` Command

We will use **list patterns** to recognize the commands in `Program.cs`. In the `Main` method:

1. Define the ignore patterns: `private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];`.
2. Match the arguments with a `switch` statement and call a method implementing the command. For unknown arguments, print the usage and return `1`:
   ```csharp
   switch (args)
   {
       case ["scan", var source]:
           return Scan(source);
       default:
           PrintUsage();
           return 1;
   }
   ```
3. Wrap the `switch` in a `try`/`catch` block, which prints the message of an `IOException` or `UnauthorizedAccessException` (`catch (Exception e) when (e is IOException or UnauthorizedAccessException)`) and returns `1`.

Implement the `Scan(string source)` method:

1. If the directory does not exist (`Directory.Exists`), print an error and return `1`.
2. Scan the directory and sort the files by `RelativePath` using `StringComparer.Ordinal`. The order of enumeration depends on the filesystem, so without sorting the output could differ between computers.
3. Print every file with its size, then the number of files with the total size (`Sum`), and finally the three largest files (`OrderByDescending`, `Take`).

```bash
dotnet run -- scan ../sample
```

```text
Scanning ../sample
     749 B  README.md
     180 B  data/config.json
   52.7 KB  data/measurements.csv
     504 B  docs/changelog.txt
    2.4 KB  docs/guide.md
   42.7 KB  media/render.jpg
     658 B  scripts/report.py
7 files, 99.8 KB in total
Largest files:
   52.7 KB  data/measurements.csv
   42.7 KB  media/render.jpg
    2.4 KB  docs/guide.md
```

The ignored files `cache/thumbnails.db` and `docs/guide.md.bak` do not appear in the list.

> [!WARNING]
> Never build paths by concatenating strings with a hardcoded separator (`dir + "/" + name`, `dir + "\\" + name`). Such code works on one operating system only. Use `Path.Combine`, `Path.GetRelativePath` and the other methods of the `Path` class.

> [!NOTE]
> #### Stage 1
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.01" >}}

### Stage 2: Hashing Files and the Manifest

To tell whether a file has changed since the previous backup, we will compare **hashes** of its content. SHA-256 maps any data to 32 bytes (64 hexadecimal digits); a change of a single bit in the file gives a completely different hash. Comparing the modification times would be faster, but they are not reliable – they change even if the content stays the same (e.g. a file is saved again without changes), and some tools set them back after changing a file (e.g. when extracting an archive), so a change could go unnoticed.

A file can be larger than the available memory, so we will compute the hash while reading the file in chunks, using the classic stream reading loop.

> [!IMPORTANT]
> #### Reading a Stream in Chunks
>
> `Stream.Read(buffer, offset, count)` reads **at most** `count` bytes and returns the number of bytes actually read – it may be fewer than requested, even before the end of the stream. The end of the stream is signalled by returning `0`. Therefore, always process exactly `read` bytes:
> ```csharp
> byte[] buffer = new byte[64 * 1024];
> int read;
> while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
> {
>     // process buffer[0..read]
> }
> ```

#### FileHasher

In `FileHasher.cs`, create a static class with a method `string ComputeSha256(string path)`:

1. Open the file with `File.OpenRead` in a `using` declaration.
2. Create an `IncrementalHash` object with `IncrementalHash.CreateHash(HashAlgorithmName.SHA256)` (namespace `System.Security.Cryptography`). It also implements `IDisposable`.
3. In the reading loop, pass every chunk to `AppendData(buffer, 0, read)`.
4. Get the hash with `GetHashAndReset()` and convert it to a lowercase hexadecimal string with `Convert.ToHexStringLower`.

> [!TIP]
> The loop is written by hand here to practise working with streams. In practice, `SHA256.HashData(stream)` does the same in one line – it also reads the stream in chunks. You can check your results with the `sha256sum` tool on Linux or `Get-FileHash` in PowerShell.

#### Changes

In `FileChange.cs`, define:

1. An enum `ChangeKind` with the values `Unchanged`, `New`, `Modified`, `Deleted`.
2. A record `FileChange(string Path, ChangeKind Kind)`.
3. A static class `ChangeKindExtensions` with an extension method `char ToSymbol(this ChangeKind kind)`, returning `'+'` for new, `'~'` for modified, `'-'` for deleted, and a space for unchanged files (use a `switch` expression).

#### The Manifest

The manifest is a text file with a header line, followed by one line per file: the hash, the size and the relative path, separated with spaces. The path is the last field, so it may contain spaces itself:

```text
# Snapshot manifest v1
8feb8de0381868e97c0ce5a1b3b6ffa68bd4869cea347a2037cca15929543e72 749 README.md
0b5bac44713986cbedf46cc217fe929e591975d1a336ba5fe5b7a80a104929aa 180 data/config.json
754fd7ae82bde0fb6657986fcad1f2b10a16540553ce281dabff0cebe3a06156 53990 data/measurements.csv
...
```

> [!IMPORTANT]
> #### Writing to and Reading from a Stream, not a File
>
> The manifest will be stored in a regular file now, and inside a ZIP archive in Stage 4. If the methods writing and reading the manifest accept a `Stream` instead of a file path, the same code will work in both cases. The text adapters `StreamWriter` and `StreamReader` by default **close the wrapped stream** when they are disposed. A method that only borrows a stream from its caller should not close it – pass `leaveOpen: true`:
> ```csharp
> using var writer = new StreamWriter(stream, leaveOpen: true);
> ```

In `Manifest.cs`, define a record `ManifestEntry(string Path, long Size, string Hash)` and a `Manifest` class:

1. A private field `SortedDictionary<string, ManifestEntry>`, mapping paths to entries, created with `StringComparer.Ordinal` (entries are kept sorted by path).
2. Properties `IReadOnlyCollection<ManifestEntry> Entries`, `int Count`, `long TotalSize`, and methods `void Add(ManifestEntry entry)` and `bool Contains(string path)`.
3. A method `ChangeKind Classify(ManifestEntry current)`, which compares an entry describing the **current** state of a file with **this** (previous) manifest: `New` if the path is not in the manifest, `Unchanged` if the hashes are equal, `Modified` otherwise.
4. A method `void WriteTo(Stream stream)`, writing the header and the entries with a `StreamWriter`.
5. A static method `Manifest ReadFrom(Stream stream)`, reading the lines with a `StreamReader` (`while (reader.ReadLine() is { } line)`). Skip empty lines and lines starting with `#`, split the others with `line.Split(' ', 3)`, and throw an `InvalidDataException` if a line has an invalid format.
6. Convenience methods `void Save(string path)` and `static Manifest Load(string path)`, which open a file (`File.Create`, `File.OpenRead`) and call `WriteTo`/`ReadFrom`.

> [!WARNING]
> `StreamWriter` collects the text in a buffer and writes it to the stream in larger chunks. If the writer is not disposed (or flushed with `Flush()`), the end of the data never reaches the file – you may end up with an empty manifest. The `using` declaration takes care of it.

#### Where to Store the Backups

The backups will be stored in a separate **repository** directory. It must not be located inside the source directory – otherwise, every backup would also back up the previous ones. In `PathUtils.cs`, create a static method `bool IsInside(string path, string directory)`, which returns `true` if `path` is `directory` itself or is located inside it. Compute `Path.GetRelativePath` between the two full paths: the path is outside if the result is `..`, starts with `..` followed by the directory separator, or is rooted (`Path.IsPathRooted` – e.g. a different drive on Windows).

#### The `backup` Command

Add the `case ["backup", var source, var repository]` to `Program.cs` and implement the `Backup` method:

1. Check that the source directory exists and that the repository is not inside it (`PathUtils.IsInside`).
2. Load the previous manifest from `<repository>/latest.manifest`, if it exists.
3. For each scanned file (sorted by path), compute its hash, add a `ManifestEntry` to the current manifest, and classify it using the previous manifest (or as `New` if there is no previous manifest).
4. Add the files present in the previous manifest, but missing from the current one, as `Deleted`.
5. Print all changes except the unchanged files, and the number of files of each kind.
6. Create the repository directory (`Directory.CreateDirectory` does nothing if it already exists) and save the current manifest as `latest.manifest`, so that it becomes the base for the next backup.

Run the first backup:

```bash
dotnet run -- backup ../sample ../backups
```

```text
No previous manifest - all files are new.
+ README.md
+ data/config.json
+ data/measurements.csv
+ docs/changelog.txt
+ docs/guide.md
+ media/render.jpg
+ scripts/report.py
New: 7, modified: 0, deleted: 0, unchanged: 0
Manifest saved to ../backups/latest.manifest
```

Now modify the sample directory – the following commands work in both Bash and PowerShell (you can also simply edit the files in your editor):

```bash
echo "Clean the sensors before winter." >> ../sample/docs/guide.md
echo "# FAQ" > ../sample/docs/faq.md
rm ../sample/docs/changelog.txt
```

The next backup detects exactly these changes (running it once more reports `New: 0, modified: 0, deleted: 0, unchanged: 7`):

```text
Previous manifest: 7 files, 99.8 KB
+ docs/faq.md
~ docs/guide.md
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
Manifest saved to ../backups/latest.manifest
```

> [!NOTE]
> #### Stage 2
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.02" >}}

### Stage 3: Backup Engine with Events

Currently, the whole backup logic lives in `Program.cs` and writes directly to the console. In this stage, we will move it into a `BackupEngine` class, which does not print anything – instead, it **raises events** describing what is happening. Anybody interested can subscribe to them: a console progress bar, a logger, a window of a graphical application, or (in Stage 5) the automatic watch mode. The engine does not need to know anything about its subscribers.

> [!IMPORTANT]
> #### Declaring and Raising Events
>
> The standard event pattern in .NET consists of:
> - a class derived from `EventArgs`, carrying the data of the event (e.g. `FileProcessedEventArgs`),
> - an event of type `EventHandler<TEventArgs>`, declared as nullable – it is `null` when there are no subscribers,
> - a `protected virtual void OnEventName(TEventArgs e)` method, which raises the event. Only this method raises the event, so derived classes can override it to react to the event (or suppress it).
> ```csharp
> public event EventHandler<FileProcessedEventArgs>? FileProcessed;
>
> protected virtual void OnFileProcessed(FileProcessedEventArgs e) => FileProcessed?.Invoke(this, e);
> ```
> Raising an event calls all subscribers **synchronously**, one after another, on the thread that raised it. When `Invoke` returns, all handlers have finished.

#### Event Data

In `BackupEventArgs.cs`, create the following classes. Their properties should be read-only (initialized from a primary constructor):

1. `FileFoundEventArgs` with the property `FileEntry File`. It derives from `CancelEventArgs` (namespace `System.ComponentModel`, see below).
2. `FileProcessedEventArgs : EventArgs` with the properties `FileEntry File` and `ChangeKind Kind`.
3. `BackupProgressEventArgs : EventArgs` with the properties `int ProcessedFiles`, `int TotalFiles`, `long ProcessedBytes`, `long TotalBytes` and a computed property `double Fraction` – the progress from 0 to 1 based on the bytes (return `1.0` if `TotalBytes` is `0`).
4. `BackupCompletedEventArgs : EventArgs` with the property `BackupSummary Summary`.

In `BackupSummary.cs`, create a `BackupSummary` class with the property `IReadOnlyList<FileChange> Changes`, the properties `New`, `Modified`, `Deleted`, `Unchanged` counting the changes of each kind, and an overridden `ToString()` returning the line `New: 1, modified: 1, deleted: 1, unchanged: 5`.

> [!IMPORTANT]
> #### Cancellable Events
>
> Event arguments are objects, so a subscriber can also pass information back to the publisher. The `CancelEventArgs` class from the standard library contains a `bool Cancel` property for exactly this purpose: the publisher raises the event, and afterwards checks whether any subscriber has cancelled the operation:
> ```csharp
> var args = new FileFoundEventArgs(file);
> OnFileFound(args);  // all subscribers run here
> if (!args.Cancel)
> {
>     files.Add(file);
> }
> ```
> When an event has several subscribers, they all receive the same arguments object. A subscriber should therefore only set `Cancel = true`, never reset it to `false` – it would overrule the decision of the subscribers called before it.

#### The BackupEngine Class

Create a `BackupEngine` class (`BackupEngine.cs`) with a primary constructor `BackupEngine(string sourceDirectory, string repositoryDirectory, IReadOnlyList<string> ignorePatterns)`, the properties `SourceDirectory` and `RepositoryDirectory`, and four events:

- `EventHandler<FileFoundEventArgs>? FileFound` – raised for every file found, before it is processed; a subscriber can exclude the file from the backup,
- `EventHandler<FileProcessedEventArgs>? FileProcessed` – raised after a file was hashed and classified,
- `EventHandler<BackupProgressEventArgs>? ProgressChanged` – raised after every processed file,
- `EventHandler<BackupCompletedEventArgs>? Completed` – raised at the end of the backup.

For each event, add a `protected virtual` method `OnFileFound`, `OnFileProcessed`, `OnProgressChanged`, `OnCompleted`, which raises it.

Move the backup logic from `Program.Backup` into a `BackupSummary Run()` method:

1. Scan the source directory and sort the files by path. For each file, raise `FileFound` and add the file to a list, unless a subscriber has cancelled it. The list is needed, because the progress requires the total size of all files before we start hashing.
2. For each file in the list: compute the hash, add the entry to the current manifest, classify it, raise `FileProcessed`, and then `ProgressChanged` with the updated counters.
3. Add the deleted files, save the manifest (as in Stage 2), create a `BackupSummary`, raise `Completed` and return the summary.

#### Console Progress Bar

Create a `ConsoleReporter` class (`ConsoleReporter.cs`), which displays the progress of a backup. It implements `IDisposable`:

1. The constructor takes a `BackupEngine` and **subscribes** private handler methods to `FileProcessed`, `ProgressChanged` and `Completed`.
2. `Dispose()` **unsubscribes** them (`-=`). Thanks to that, `using var reporter = new ConsoleReporter(engine);` limits the subscription to a scope.
3. The `FileProcessed` handler prints changed files (e.g. `+ docs/faq.md`) and ignores unchanged ones.
4. The `ProgressChanged` handler draws a progress bar, e.g. `[###############...............]  50% (3/6 files, 1.4 KB)`. To redraw it in place, start the text with `'\r'` (carriage return – moves the cursor to the beginning of the line) and use `Console.Write` instead of `WriteLine`. Pad the line with spaces (`PadRight`) so that a shorter text completely covers the previous one.
5. Since the progress bar stays in the current line, the `FileProcessed` handler should also start its message with `'\r'` and pad it – the message overwrites the bar, which is drawn again on the next `ProgressChanged` event.
6. The `Completed` handler ends the progress bar line, prints the deleted files and the summary.

> [!TIP]
> An event keeps references to all its subscribers. If a short-lived object subscribes to an event of a long-lived one and never unsubscribes, it will not be collected by the Garbage Collector as long as the publisher lives – this is one of the most common causes of memory leaks in .NET. Also note that a lambda can only be unsubscribed if you keep a reference to it in a variable – writing the same lambda again after `-=` creates a different delegate.

#### Using the Engine

In `Program.cs`, rewrite `Backup` to create a `BackupEngine` and a `ConsoleReporter`, and call `Run()`. Then, add support for an optional fourth argument – the maximum file size in kilobytes:

```csharp
case ["backup", var source, var repository, var maxSize] when long.TryParse(maxSize, out long maxSizeKb):
    return Backup(source, repository, maxSizeKb);
```

If the limit is given, subscribe a lambda to `FileFound`, which cancels files larger than the limit and prints which files are skipped. A skipped file is treated exactly like an ignored one – it is not part of the backup at all.

Delete the `../backups` directory, extract a fresh copy of the sample directory, and make a backup with the limit of 50 KB:

```text
Skipping data/measurements.csv (52.7 KB > 50 KB)
+ README.md
+ data/config.json
+ docs/changelog.txt
+ docs/guide.md
+ media/render.jpg
+ scripts/report.py
[##############################] 100% (6/6 files, 47.1 KB)
New: 6, modified: 0, deleted: 0, unchanged: 0
```

Without the limit, the skipped file is backed up as a new one (`New: 1, modified: 0, deleted: 0, unchanged: 6`). After modifying the sample directory as in Stage 2, the output looks as follows:

```text
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
```

> [!NOTE]
> #### Stage 3
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.03" >}}

### Stage 4: ZIP Snapshots and Restoring Files

So far, the "backup" consists of a manifest only. In this stage, every backup will create a **snapshot** – a ZIP archive named after the UTC time of the backup (`snapshot-20261105-101530.zip`), containing:

- `manifest.txt` – the manifest of **all** files at the time of the backup,
- `files/...` – only the files that are **new or modified** since the previous snapshot.

```text
backups/
├── snapshot-20261105-101530.zip   <- manifest.txt + files/ (all 7 files)
└── snapshot-20261105-102012.zip   <- manifest.txt + files/docs/faq.md, files/docs/guide.md
```

The manifest of the newest snapshot becomes the base for the next backup, so `latest.manifest` is no longer needed (you can delete it).

> [!IMPORTANT]
> #### ZIP Archives
>
> `ZipFile.Open(path, mode)` (namespace `System.IO.Compression`) opens an archive as a `ZipArchive` object in one of three modes: `Read`, `Create`, `Update`. `ZipFile.OpenRead(path)` is a shortcut for the `Read` mode. `ZipArchive` implements `IDisposable` – the archive is completed (e.g. its table of contents is written) and closed only when it is disposed.
> - `archive.CreateEntry(name, CompressionLevel.Optimal)` adds a new entry (the name may contain `/` to create directories inside the archive), and `entry.Open()` returns a stream for writing its content.
> - `archive.Entries` and `archive.GetEntry(name)` give access to the existing entries; `entry.Open()` returns a stream for reading, and the extension method `entry.ExtractToFile(path, overwrite)` extracts the entry to a file.
>
> In the `Create` mode, only one entry can be open at a time – dispose the stream of an entry before creating the next one. More details can be found in the [Archives lecture]({{< ref "/lectures/07.filesystem/archives" >}}).

> [!IMPORTANT]
> #### Stream Decorators
>
> A stream can wrap another stream and add a feature to it: buffering (`BufferedStream`), compression (`GZipStream`, `DeflateStream`, `BrotliStream`), encryption (`CryptoStream`). Such a **decorator** is a `Stream` itself, so decorators can be chained, and an adapter (`StreamReader`, `StreamWriter`) usually closes the chain:
> ```csharp
> // Writing: StreamWriter -> GZipStream -> FileStream
> using (FileStream file = File.Create("notes.txt.gz"))
> using (var gzip = new GZipStream(file, CompressionLevel.Optimal))
> using (var writer = new StreamWriter(gzip))
> {
>     writer.WriteLine("Hello, compressed world!");
> } // disposed in reverse order: writer (flushes text), gzip (writes the footer), file (closes the handle)
>
> // Reading: disposing the adapter disposes the whole chain
> using var reader = new StreamReader(new GZipStream(File.OpenRead("notes.txt.gz"), CompressionMode.Decompress));
> Console.WriteLine(reader.ReadToEnd()); // Hello, compressed world!
> ```
> The stream returned by `ZipArchiveEntry.Open()` works the same way – it compresses the data written to it (or decompresses the data read from it) on the fly. Because `Manifest.WriteTo` and `Manifest.ReadFrom` accept any `Stream`, they work on top of it without any change.

#### The SnapshotArchive Class

In `SnapshotArchive.cs`, create a static class `SnapshotArchive` with the constants `ManifestEntryName = "manifest.txt"` and `FilesPrefix = "files/"`, and the following methods:

1. `List<string> List(string repository)` – returns the paths of all `snapshot-*.zip` files in the repository, sorted by name with `StringComparer.Ordinal` (the names contain the UTC time, so this sorts them from the oldest to the newest). If the repository does not exist, return an empty list.
2. `string Create(string repository, Manifest manifest, IEnumerable<FileEntry> filesToStore)` – creates the repository directory and a new archive `snapshot-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip` with `ZipFile.Open(path, ZipArchiveMode.Create)` (this mode throws an exception if the file already exists, so an older snapshot is never overwritten). For every file, create an entry `files/<relative path>`, set its `LastWriteTime`, and copy the content of the file to the entry stream with `CopyTo`. Finally, create the `manifest.txt` entry and write the manifest to its stream. Return the path of the archive.
3. `Manifest ReadManifest(string snapshotPath)` – opens the archive for reading, finds the `manifest.txt` entry (throw an `InvalidDataException` if it is missing) and reads the manifest from its stream.

The key part of `Create`:

```csharp
foreach (FileEntry file in filesToStore)
{
    ZipArchiveEntry entry = archive.CreateEntry(FilesPrefix + file.RelativePath, CompressionLevel.Optimal);
    entry.LastWriteTime = file.LastWriteTime;

    // Only one entry can be open at a time, so the entry stream is disposed
    // at the end of each iteration, before the next entry is created
    using Stream entryStream = entry.Open();
    using FileStream source = File.OpenRead(file.FullPath);
    source.CopyTo(entryStream);
}
```

#### Updating the Engine

Modify `BackupEngine.Run`:

1. Take the previous manifest from the newest snapshot (`SnapshotArchive.List(...).LastOrDefault()`, `SnapshotArchive.ReadManifest`) instead of `latest.manifest`.
2. While processing the files, collect the new and modified ones in a separate list.
3. Instead of saving `latest.manifest`, create a snapshot with `SnapshotArchive.Create` – but only if there is at least one change (new, modified or deleted file). A backup with deleted files only creates a snapshot containing just the manifest.
4. Add a property `string? SnapshotPath` to `BackupSummary` (passed to the constructor; `null` if no snapshot was created). In `ConsoleReporter`, print the path and the size of the created snapshot, or the message `No changes - snapshot not created.`.

#### Restoring Files

Restoring the state from the moment snapshot *N* was made requires all snapshots up to *N*: the first one contains all files, and each subsequent one contains the newer versions of the changed files. In `SnapshotArchive`, add a record `RestoreResult(string SnapshotName, int FileCount, long TotalSize, int ChecksumErrors)` and a method:

```csharp
public static RestoreResult Restore(string repository, string target, string? snapshotName,
    Action<string>? log = null)
```

1. Find the index of the chosen snapshot – by name without the extension (`Path.GetFileNameWithoutExtension`), or the newest one if `snapshotName` is `null`. If there are no snapshots or the name is not found, throw a `FileNotFoundException`.
2. Require the target directory to be empty or not to exist (otherwise, throw an `IOException`), and create it.
3. Open the snapshots from the oldest to the chosen one. Extract each entry starting with `files/` to the target directory (`entry.ExtractToFile(destination, overwrite: true)`), creating the missing directories first. Newer versions of files overwrite older ones. After each snapshot, report its name and the number of extracted files with the `log` callback.
4. Read the manifest of the chosen snapshot and delete all files in the target directory that are not in the manifest – they were deleted before the snapshot was made.
5. Verify the restored files: compute the hash of each file from the manifest and count the files that are missing or have a different hash.

> [!WARNING]
> #### Zip Slip
> Never trust the paths stored in an archive. A specially prepared archive may contain an entry named `files/../../.bashrc` – extracting it naively would overwrite a file outside the target directory. Before extracting an entry, compute its full path (`Path.GetFullPath`) and check that it is inside the target directory – `PathUtils.IsInside` from Stage 2 does exactly that. If it is not, throw an `InvalidDataException`.

> [!WARNING]
> In step 4, you delete files while iterating over the directory. Do not modify a directory while it is being lazily enumerated with `EnumerateFiles` – the result is unpredictable. Use `Directory.GetFiles`, which reads all entries into an array before the loop starts.

In `Program.cs`, add the `restore` command with an optional snapshot name, and add `InvalidDataException` to the exception filter in `Main`:

```csharp
case ["restore", var repository, var target]:
    return Restore(repository, target, snapshotName: null);
case ["restore", var repository, var target, var snapshotName]:
    return Restore(repository, target, snapshotName);
```

The `Restore` method passes a lambda printing the messages as the `log` callback, and prints the result.

> [!TIP]
> Why is `log` a delegate parameter and not an event? An event is meant for an **arbitrary number** of independent subscribers that the publisher does not know about, and it lives as long as the object. Here, the restore operation has exactly one caller, which wants to be notified only during this one call – a callback parameter (`Action<string>`) is simpler.

Start from a fresh copy of the sample directory and an empty repository (delete `../backups`). Make a backup – the first snapshot contains all 7 files (`Snapshot created: ../backups/snapshot-20261105-101530.zip (52.5 KB)`). Then modify the sample directory as in Stage 2 and make a backup again (the names of the snapshots on your computer will differ):

```text
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
New: 1, modified: 1, deleted: 1, unchanged: 5
Snapshot created: ../backups/snapshot-20261105-102012.zip (1.8 KB)
```

Now restore the newest state and the state from the first snapshot:

```bash
dotnet run -- restore ../backups ../restored
dotnet run -- restore ../backups ../restored-old snapshot-20261105-101530
```

```text
  Applied snapshot-20261105-101530.zip (7 files)
  Applied snapshot-20261105-102012.zip (2 files)
  Removed docs/changelog.txt
Restored snapshot-20261105-102012: 7 files, 99.4 KB, all checksums OK.
```

```text
  Applied snapshot-20261105-101530.zip (7 files)
Restored snapshot-20261105-101530: 7 files, 99.8 KB, all checksums OK.
```

Open the snapshots in any archive manager and check their contents. The sizes of the archives may slightly differ depending on the version of the compression library.

> [!NOTE]
> #### Stage 4
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.04" >}}

### Stage 5: Watching for Changes

In the last stage, we will add the `watch` command, which makes a backup automatically whenever something changes in the source directory.

> [!IMPORTANT]
> #### FileSystemWatcher
>
> The `FileSystemWatcher` class uses operating system mechanisms (e.g. `inotify` on Linux) to raise events when files in a directory change: `Created`, `Changed`, `Deleted`, `Renamed` (with `RenamedEventArgs`, containing also the old name), and `Error`. The most important properties:
> - `Path`, `Filter` – which directory and which files to watch,
> - `IncludeSubdirectories` – whether to watch the whole tree,
> - `NotifyFilter` – which kinds of changes raise events (`FileName`, `DirectoryName`, `LastWrite`, `Size`, …),
> - `EnableRaisingEvents` – events are raised only after it is set to `true`.
>
> `FileSystemWatcher` implements `IDisposable` – disposing it stops watching. `FileSystemEventArgs.Name` contains the path of the changed file relative to the watched directory.

A single "save" in an editor often produces a whole burst of events: the file is truncated, written in several chunks, sometimes written to a temporary file and renamed. Making a backup after each of them would be wasteful. Instead, we will use **debouncing**: every event (re)starts a countdown, and the backup starts only when the countdown reaches zero – i.e. when nothing has changed for a while (the *quiet period*, e.g. 2 seconds):

```text
events:   x  x x x                 x   x
time:     ------------------------------------------------------->
                  |<-- 2 s -->|          |<-- 2 s -->|
                              backup                 backup
```

> [!IMPORTANT]
> #### Events Raised on Other Threads
>
> Up to now, all events were raised synchronously, on the thread that called `Run()`. `FileSystemWatcher` raises its events on a **background thread** (a thread-pool thread on Windows, a dedicated watcher thread on Linux), and `System.Threading.Timer` calls its callback on a **thread-pool thread** when the countdown elapses. Consequences:
> - handlers may run at the same time as the main thread and as each other,
> - an exception thrown from such a handler is not caught by the `try`/`catch` in `Main` – it **terminates the whole process**. Catch exceptions in such handlers yourself.
>
> When several threads access shared state, guard it with a `lock` statement. Since .NET 9, the dedicated `System.Threading.Lock` type is used as the lock object. Only one thread at a time can execute the code inside `lock (obj) { ... }` – it is a counterpart of `std::lock_guard<std::mutex>` in C++. Threads and synchronization will be covered in detail later in the course.

#### The DebouncedWatcher Class

Create a sealed class `DebouncedWatcher` (`DebouncedWatcher.cs`) implementing `IDisposable`:

1. Two events: `EventHandler<FileSystemEventArgs>? ChangeDetected`, raised for every relevant change, and `EventHandler? ChangesSettled`, raised once the quiet period passes without changes.
2. A constructor `DebouncedWatcher(string directory, TimeSpan quietPeriod, Func<string, bool> isIgnored)`. The `isIgnored` delegate decides which paths are irrelevant – we will pass the `DirectoryScanner.IsIgnored` method, so that e.g. changes of `*.tmp` files do not trigger backups.
3. In the constructor, create a stopped one-shot timer: `new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite)`, and a `FileSystemWatcher` watching the whole tree (`IncludeSubdirectories = true`) with `NotifyFilter` set to `FileName | DirectoryName | LastWrite | Size`. Subscribe a single handler `OnFileSystemChanged(object sender, FileSystemEventArgs e)` to `Created`, `Changed`, `Deleted` and `Renamed`. This works for `Renamed` too, although its delegate expects `RenamedEventArgs` – a method accepting a more general parameter type is compatible with the delegate (contravariance).
4. A method `void Start()`, which sets `EnableRaisingEvents = true`.
5. `OnFileSystemChanged`: convert `e.Name` to a path with `/` separators, return if the path is ignored, raise `ChangeDetected`, and restart the countdown with `_timer.Change(_quietPeriod, Timeout.InfiniteTimeSpan)`.
6. A handler of the `Error` event (raised e.g. when the internal buffer of the watcher overflows and some events are lost) – it just restarts the countdown. The backup rescans the whole directory anyway, so lost events do not matter.
7. `OnTimerElapsed(object? state)`: raises `ChangesSettled`.
8. `Dispose()`: disposes the watcher and the timer (only once).

Guard the timer and the `_disposed` flag with a single `Lock _lock`. Thanks to that, two backups never run at the same time, a timer callback that arrives after `Dispose()` does nothing, and `Dispose()` waits for a backup that is currently running:

```csharp
private void OnTimerElapsed(object? state)
{
    lock (_lock)
    {
        if (!_disposed)
        {
            ChangesSettled?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

> [!TIP]
> The class is `sealed`, so it does not need `protected virtual OnX` methods – nobody can derive from it, and the events can be raised directly.

#### The `watch` Command

Add `case ["watch", var source, var repository]` to `Program.cs` and implement the `Watch` method:

1. Check the directories, create a `BackupEngine` and a `ConsoleReporter` (as in `Backup`), and run a backup immediately, to bring the repository up to date.
2. Create a `DebouncedWatcher` with a quiet period of 2 seconds, passing `IsIgnored` of a `DirectoryScanner` as the method group: `new DebouncedWatcher(source, TimeSpan.FromSeconds(2), scanner.IsIgnored)`.
3. Subscribe to `ChangeDetected` a lambda, printing the kind of the change and the path (for `RenamedEventArgs`, print the old and the new name – use the `is` pattern).
4. Subscribe to `ChangesSettled` a lambda which runs `engine.Run()` inside a `try`/`catch` block, printing the errors.
5. Start the watcher and wait for the user to press Enter (`Console.ReadLine()`). The `using` declarations dispose the watcher and unsubscribe the reporter at the end of the method.

Extract a fresh copy of the sample directory, delete `../backups`, make a backup with the `backup` command, and start `dotnet run -- watch ../sample ../backups`. While the program is running, modify the files: append a line to `docs/guide.md`, create `docs/faq.md`, create a temporary file `docs/faq.md.tmp`, and wait for the backup. Then rename `scripts/report.py` to `scripts/daily_report.py` and delete `docs/changelog.txt`:

```text
[##############################] 100% (7/7 files, 99.8 KB)
New: 0, modified: 0, deleted: 0, unchanged: 7
No changes - snapshot not created.
Watching ../sample - press Enter to stop.
Changed: docs/guide.md
Created: docs/faq.md
Changed: docs/faq.md
Changes settled - starting backup
+ docs/faq.md
~ docs/guide.md
[##############################] 100% (8/8 files, 99.9 KB)
New: 1, modified: 1, deleted: 0, unchanged: 6
Snapshot created: ../backups/snapshot-20261105-103348.zip (1.9 KB)
Renamed: scripts/report.py -> scripts/daily_report.py
Deleted: docs/changelog.txt
Changes settled - starting backup
+ scripts/daily_report.py
[##############################] 100% (7/7 files, 99.4 KB)
- docs/changelog.txt
- scripts/report.py
New: 1, modified: 0, deleted: 2, unchanged: 6
Snapshot created: ../backups/snapshot-20261105-103402.zip (1.0 KB)
```

The change of the ignored `faq.md.tmp` file was not reported at all. The exact sequence of raw events depends on the operating system and the editor (e.g. on Windows, you may also see `Changed` events for directories, or two `Changed` events for a single save), but thanks to debouncing, each burst of changes results in a single backup.

> [!NOTE]
> #### Stage 5
> {{< filetree dir="labs/w06.events-filesystem-streams/Snapshot.05" >}}

### Next Steps and Further Enhancements

The application built in this tutorial provides a solid foundation for further expansion. Below are key features to explore next:

- **Quick change detection:** Hashing every file on every backup is the most reliable, but the slowest method. Tools like `rsync` or Git first compare the size and the modification time stored in the manifest, and compute the hash only for files where these differ.
- **Deduplication:** A renamed file is currently stored again, as a deleted and a new file. If the files are stored in the repository under the names derived from their hashes (*content-addressable storage*, as Git does), every content is stored only once, and renames cost nothing.
- **Retention and consolidation:** The chain of snapshots grows forever. Old snapshots can be merged into a single full snapshot, so that the restore does not need to apply all of them, and the oldest ones can be deleted.
- **Encryption:** Since stream decorators can be chained, encrypting the snapshots is only a matter of adding a `CryptoStream` (e.g. with the `Aes` algorithm) between the archive and the file stream.
- **Asynchronous I/O and cancellation:** Most stream operations have asynchronous counterparts (`ReadAsync`, `CopyToAsync`, and, since .NET 10, also asynchronous ZIP APIs). Together with `CancellationToken`, they allow building a responsive application, e.g. one that can cancel a long backup.
- **Configuration:** The ignore patterns could be read from a `.snapshotignore` file in the source directory, similarly to `.gitignore`.

## Example Tasks

Complete an example task from previous years. If you are able to finish it within 90 minutes, you are well-prepared for class.

- [Task 1]({{< ref "/labs/06.events-filesystem-streams/example1" >}})
