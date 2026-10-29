---
title: "Archives"
weight: 30
---

# Archives

Archives are files that act as containers for other files and folders. The C# standard library allows working with archives in `.zip` and `.tar` (from .NET 7) formats through classes in the `System.IO.Compression` namespace.

## ZIP Archives

ZIP archives, in addition to grouping files, also offer data compression by default.

For simple operations, the static `ZipFile` class is used, which allows for quick creation of an archive from a folder (`CreateFromDirectory`) or extracting it (`ExtractToDirectory`).

```csharp
const string sourceDirectory = "my_files_to_zip";
const string zipPath = "archive.zip";
const string extractPath = "extracted_files";

Directory.CreateDirectory(sourceDirectory);
File.WriteAllText(Path.Combine(sourceDirectory, "file1.txt"), "Hello");
Directory.CreateDirectory(Path.Combine(sourceDirectory, "subfolder"));
File.WriteAllText(Path.Combine(sourceDirectory, "subfolder", "file2.txt"), "World");

// Creating .zip archive (override existing)
if (File.Exists(zipPath)) File.Delete(zipPath);
ZipFile.CreateFromDirectory(sourceDirectory, zipPath);

// Extracting archive (override if files exist)
if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
ZipFile.ExtractToDirectory(zipPath, extractPath);
```

For more advanced operations, the `ZipFile.Open` method is used, which returns a `ZipArchive` object. This allows for precise manipulation of archive contents entry by entry. An archive can be opened in one of three modes: `Read`, `Create`, or `Update`.

```csharp
using ZipArchive zip = ZipFile.Open("archive.zip", ZipArchiveMode.Read);

Console.WriteLine("Archive content:");
foreach (ZipArchiveEntry entry in zip.Entries)
{
    Console.WriteLine($"{entry.FullName,-32} Size: {entry.Length} bytes");
}
```

The table below presents selected methods and properties that allow for precise manipulation of archives after opening them.

| Method or Property | Description |
| :--------------------------------- | :-------------------------------------------------------------------------------------------- |
| `ZipArchive.Entries` | Retrieves a collection of all entries (files and folders) within the archive. |
| `ZipArchive.CreateEntry` | Creates a new, empty entry in the archive. |
| `ZipArchive.GetEntry` | Retrieves a specific entry from the archive based on its name. |
| `ZipArchive.CreateEntryFromFile` | (Extension method) Creates a new entry in the archive based on an existing file on disk. |
| `ZipArchive.ExtractToDirectory` | (Extension method) Extracts the entire contents of the archive to a specified folder. |
| `ZipArchiveEntry.Delete` | Deletes the current entry from the archive. |
| `ZipArchiveEntry.Open` | Opens a stream to the entry's content, allowing it to be read or written. |
| `ZipArchiveEntry.ExtractToFile` | (Extension method) Extracts the current entry to a file on disk. |

### Asynchronous API (.NET 10)

Since .NET 10, ZIP archives can also be processed [asynchronously]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}), without blocking a thread during I/O operations: `ZipFile.CreateFromDirectoryAsync`, `ZipFile.ExtractToDirectoryAsync`, `ZipFile.OpenAsync`, `ZipFile.OpenReadAsync`, `ZipArchive.CreateAsync`, `ZipArchiveEntry.OpenAsync`, and the extension methods `CreateEntryFromFileAsync`, `ExtractToFileAsync`, `ExtractToDirectoryAsync`. `ZipArchive` also implements `IAsyncDisposable`.

```csharp
if (File.Exists("archive.zip")) File.Delete("archive.zip");
await ZipFile.CreateFromDirectoryAsync("my_files_to_zip", "archive.zip");

await using ZipArchive zip = await ZipFile.OpenReadAsync("archive.zip");
foreach (ZipArchiveEntry entry in zip.Entries)
{
    await using Stream stream = await entry.OpenAsync();
    using StreamReader reader = new StreamReader(stream);
    Console.WriteLine($"{entry.FullName}: {await reader.ReadToEndAsync()}");
}
```

## TAR Archives (.NET 7)

The TAR (`Tape Archive`) format only groups files into a single container, but **does not apply compression itself**. Its advantage is support for Unix system attributes. In practice, `.tar` archives are often additionally compressed using GZip, Brotli, or BZip2 (not available in the standard library) algorithms, resulting in files with `.tar.gz` (or `.tgz`) and `.tar.br` extensions.

Similar to ZIP, the `TarFile` class provides easy-to-use `CreateFromDirectory` and `ExtractToDirectory` methods.

```csharp
const string sourceDirectory = "my_files_to_tar";
const string tarPath = "archive.tar";

Directory.CreateDirectory(sourceDirectory);
File.WriteAllText(Path.Combine(sourceDirectory, "log.txt"), "Log entry");

// Creating .tar archive
if (File.Exists(tarPath)) File.Delete(tarPath);
TarFile.CreateFromDirectory(sourceDirectory, tarPath);
```

Similarly, the `ExtractToDirectory` method is used to extract archives. Unlike `ZipFile`, it requires the target folder to exist.

```csharp
const string tarPath = "archive.tar";
const string extractPath = "extracted_tar_files";

// Extracting .tar archive
if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
Directory.CreateDirectory(extractPath);
TarFile.ExtractToDirectory(tarPath, extractPath, false);
```

To create a compressed `.tar.gz` archive, instead of providing a path, provide a stream with applied compression when creating the archive:

```csharp
// Creating .tar.gz archive
using FileStream fileStream = new("archive.tar.gz", FileMode.Create);
using GZipStream gzipStream = new(fileStream, CompressionMode.Compress);
TarFile.CreateFromDirectory(sourceDirectory, gzipStream, false);
```

To extract a compressed archive (e.g., `.tar.gz`), use the appropriate decompression stream.

```csharp
const string sourceArchive = "archive.tar.gz";
const string destinationDirectory = "extracted_targz_files";

if (Directory.Exists(destinationDirectory)) Directory.Delete(destinationDirectory, true);
Directory.CreateDirectory(destinationDirectory);

// Extracting .tar.gz archive
using FileStream fileStream = File.OpenRead(sourceArchive);
using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
TarFile.ExtractToDirectory(gzipStream, destinationDirectory, false);
```

### `TarWriter` and `TarReader`

For more control, entry by entry (similar to `ZipArchive`), the `TarWriter` and `TarReader` classes are used. They operate on streams, so they can be combined with compression streams as well:

```csharp
// Creating .tar.gz archive entry by entry
using (FileStream fs = File.Create("logs.tar.gz"))
using (GZipStream gz = new GZipStream(fs, CompressionMode.Compress))
using (TarWriter writer = new TarWriter(gz))
{
    // Entry from a file on disk
    writer.WriteEntry(Path.Combine("my_files_to_tar", "log.txt"), entryName: "logs/log.txt");

    // Entry with data from a stream
    var entry = new PaxTarEntry(TarEntryType.RegularFile, "logs/info.txt")
    {
        DataStream = new MemoryStream("Hello from TarWriter"u8.ToArray())
    };
    writer.WriteEntry(entry);
}

// Reading .tar.gz archive entry by entry
using (FileStream fs = File.OpenRead("logs.tar.gz"))
using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
using (TarReader reader = new TarReader(gz))
{
    while (reader.GetNextEntry() is TarEntry entry)
    {
        Console.WriteLine($"{entry.Name,-16} {entry.Length,3} bytes, mode: {entry.Mode}");
        // entry.DataStream - entry contents, entry.ExtractToFile(...) - extracting to disk
    }
}
```

```text
logs/log.txt       9 bytes, mode: OtherRead, GroupRead, UserWrite, UserRead
logs/info.txt     20 bytes, mode: OtherRead, GroupRead, UserWrite, UserRead
```
