---
title: "Archiwa"
weight: 30
---

# Archiwa

Archiwa to pliki będące kontenerami dla innych plików i folderów. Biblioteka standardowa C# pozwala na pracę z archiwami w formacie `.zip` poprzez klasy w przestrzeni nazw `System.IO.Compression` oraz w formacie `.tar` (od .NET 7) poprzez klasy w przestrzeni nazw `System.Formats.Tar`.

## Archiwa ZIP

Archiwa ZIP, oprócz grupowania plików, domyślnie oferują również kompresję danych.

Do prostych operacji służy statyczna klasa `ZipFile`, która pozwala na szybkie tworzenie archiwum z folderu (`CreateFromDirectory`) lub rozpakowywanie go (`ExtractToDirectory`).

```csharp
const string sourceDirectory = "my_files_to_zip";
const string zipPath = "archive.zip";
const string extractPath = "extracted_files";

Directory.CreateDirectory(sourceDirectory);
File.WriteAllText(Path.Combine(sourceDirectory, "file1.txt"), "Hello");
Directory.CreateDirectory(Path.Combine(sourceDirectory, "subfolder"));
File.WriteAllText(Path.Combine(sourceDirectory, "subfolder", "file2.txt"), "World");

// Creating .zip archive (overwrite existing)
if (File.Exists(zipPath)) File.Delete(zipPath);
ZipFile.CreateFromDirectory(sourceDirectory, zipPath);

// Extracting archive (overwrite if files exist)
if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
ZipFile.ExtractToDirectory(zipPath, extractPath);
```

Do bardziej zaawansowanych operacji służy metoda `ZipFile.Open`, która zwraca obiekt `ZipArchive`. Pozwala to na precyzyjną manipulację zawartością archiwum wpis po wpisie. Archiwum można otworzyć w jednym z trzech trybów: `Read` (odczyt), `Create` (tworzenie) lub `Update` (modyfikacja).

```csharp
using ZipArchive zip = ZipFile.Open("archive.zip", ZipArchiveMode.Read);

Console.WriteLine("Archive content:");
foreach (ZipArchiveEntry entry in zip.Entries)
{
    Console.WriteLine($"{entry.FullName,-32} Size: {entry.Length} bytes");
}
```

Poniższa tabela przedstawia wybrane metody i właściwości, które pozwalają na precyzyjną manipulację archiwami po ich otwarciu.

|  Metoda lub właściwość             | Opis                                                                                          |
| :--------------------------------- | :-------------------------------------------------------------------------------------------- |
| `ZipArchive.Entries`               | Pobiera kolekcję wszystkich wpisów (plików i folderów) znajdujących się w archiwum.           |
| `ZipArchive.CreateEntry`           | Tworzy nowy, pusty wpis w archiwum.                                                           |
| `ZipArchive.GetEntry`              | Pobiera konkretny wpis z archiwum na podstawie jego nazwy.                                    |
| `ZipArchive.CreateEntryFromFile`   | (Metoda rozszerzająca) Tworzy nowy wpis w archiwum na podstawie istniejącego pliku na dysku.  |
| `ZipArchive.ExtractToDirectory`    | (Metoda rozszerzająca) Rozpakowuje całą zawartość archiwum do określonego folderu.            |
| `ZipArchiveEntry.Delete`           | Usuwa bieżący wpis z archiwum.                                                                |
| `ZipArchiveEntry.Open`             | Otwiera strumień do zawartości wpisu, umożliwiając jego odczyt lub zapis.                     |
| `ZipArchiveEntry.ExtractToFile`    | (Metoda rozszerzająca) Rozpakowuje bieżący wpis do pliku na dysku.                            |

### Asynchroniczne API (.NET 10)

Od .NET 10 archiwa ZIP można też przetwarzać [asynchronicznie]({{< ref "/lectures/09.concurrency/asynchronous-programming" >}}), bez blokowania wątku podczas operacji wejścia/wyjścia: `ZipFile.CreateFromDirectoryAsync`, `ZipFile.ExtractToDirectoryAsync`, `ZipFile.OpenAsync`, `ZipFile.OpenReadAsync`, `ZipArchive.CreateAsync`, `ZipArchiveEntry.OpenAsync` oraz metody rozszerzające `CreateEntryFromFileAsync`, `ExtractToFileAsync`, `ExtractToDirectoryAsync`. `ZipArchive` implementuje również `IAsyncDisposable`.

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

## Archiwa TAR (.NET 7)

Format TAR (`Tape Archive`) jedynie grupuje pliki w jeden kontener, ale **sam w sobie nie stosuje kompresji**. Jego zaletą jest obsługa atrybutów systemów uniksowych. W praktyce archiwa `.tar` są często dodatkowo kompresowane za pomocą algorytmów GZip, Brotli lub BZip2 (niedostępny w bibliotece standardowej), co daje pliki z rozszerzeniami `.tar.gz` (lub `.tgz`) oraz `.tar.br`.

Podobnie jak w przypadku ZIP, klasa `TarFile` udostępnia proste w użyciu metody `CreateFromDirectory` i `ExtractToDirectory`.

```csharp
const string sourceDirectory = "my_files_to_tar";
const string tarPath = "archive.tar";

Directory.CreateDirectory(sourceDirectory);
File.WriteAllText(Path.Combine(sourceDirectory, "log.txt"), "Log entry");

// Creating .tar archive
if (File.Exists(tarPath)) File.Delete(tarPath);
TarFile.CreateFromDirectory(sourceDirectory, tarPath, false);
```

Podobnie, do rozpakowywania archiwów służy metoda `ExtractToDirectory`. W odróżnieniu od `ZipFile`, wymaga ona, aby folder docelowy istniał.

```csharp
const string tarPath = "archive.tar";
const string extractPath = "extracted_tar_files";

// Extracting .tar archive
if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
Directory.CreateDirectory(extractPath);
TarFile.ExtractToDirectory(tarPath, extractPath, false);
```

Aby utworzyć skompresowane archiwum `.tar.gz`, należy podczas tworzenia archiwum zamiast ścieżki podać strumień z nałożoną kompresją:

```csharp
// Creating .tar.gz archive
using FileStream fileStream = new("archive.tar.gz", FileMode.Create);
using GZipStream gzipStream = new(fileStream, CompressionMode.Compress);
TarFile.CreateFromDirectory(sourceDirectory, gzipStream, false);
```

Aby rozpakować skompresowane archiwum (np. `.tar.gz`), należy użyć odpowiedniego strumienia dekompresującego.

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

### `TarWriter` i `TarReader`

Do bardziej precyzyjnej pracy, wpis po wpisie (podobnie jak w przypadku `ZipArchive`), służą klasy `TarWriter` i `TarReader`. Operują one na strumieniach, więc można je łączyć również ze strumieniami kompresującymi:

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
