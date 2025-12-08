---
title: "Łącza"
weight: 20
---

# Łącza

Łącza (*pipes*) to jeden z mechanizmów komunikacji międzyprocesowej (*Inter-Process Communication*, IPC), który pozwala na przepływ danych między procesami, w sposób jednokierunkowy lub dwukierunkowy. Działają one w sposób podobny do strumieni, gdzie jeden proces zapisuje dane na jednym końcu łącza, a drugi odczytuje je z drugiego końca.

## Łącza nazwane

> [!NOTE]
> Nazewnictwo w .NET może być mylące. Na systemach Unix-podobnych (takich jak Linux czy macOS), `NamedPipeStream` nie jest zaimplementowane jako pliki FIFO (czyli łącza nazwane w POSIX), lecz jako **gniazda lokalne** (*Unix Domain Sockets*). Na Windowsie są to faktycznie łącza nazwane.

Łącza nazwane, jak sama nazwa wskazuje, posiadają unikalną w systemie nazwę. Dzięki temu mogą być używane przez dowolne dwa procesy, nawet jeśli nie są ze sobą spokrewnione. Model komunikacji przypomina architekturę klient-serwer. Jeden proces pełni rolę serwera, który tworzy łącze i czeka na połączenie, a drugi to klient, który się do niego podłącza.

Poniższy kod przedstawia serwer, który tworzy dwukierunkowe łącze (`PipeDirection.InOut`) i czeka na połączenie od klienta za pomocą `WaitForConnectionAsync()`.

```csharp
using System.IO.Pipes;

namespace Server;

class Program
{
    private static async Task Main()
    {
        await using var pipe = new NamedPipeServerStream("p3_pipe", PipeDirection.InOut);
        await pipe.WaitForConnectionAsync();
        
        var reader = new StreamReader(pipe);
        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        
        while (await reader.ReadLineAsync() is { } line)
        {
            await writer.WriteLineAsync(line.ToUpper());
        }
    }
}
```

Poniższy kod przedstawia klienta, który łączy się z serwerem za pomocą tej samej nazwy łącza. Po nawiązaniu połączenia, klient wysyła linie tekstu, a następnie odczytuje i wyświetla przetworzoną odpowiedź odesłaną przez serwer. Cała komunikacja odbywa się przez jedno łącze, skonfigurowane jako dwukierunkowe. Na systemach uniksowych klasyczne łącza nazwane (FIFO) są jednokierunkowe. Dwukierunkowa komunikacja wymagałaby dwóch osobnych łącz. Implementacja w .NET omija to ograniczenie, wykorzystując gniazda lokalne (*Unix Domain Sockets*), które natywnie wspierają komunikację dwukierunkową.

> [!NOTE]
> Czytanie danych z łącza jest blokujące. Jeśli łącze jest puste, program będzie czekał, aż po drugiej stronie pojawią się dane do odczytania lub gdy łącze zostanie zamknięte. Jeśli łącze jest przepełnione, to pisanie może również blokować na dłużej.

```csharp
using System.IO.Pipes;

namespace Client;

class Program
{
    private static async Task Main()
    {
        await using var pipe = new NamedPipeClientStream("p3_pipe");
        await pipe.ConnectAsync();

        var reader = new StreamReader(pipe);
        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;

        await foreach (var line in EnumerateLines("lorem.txt"))
        {
            await writer.WriteLineAsync(line);
            var processedLine = await reader.ReadLineAsync();
            Console.WriteLine(processedLine);
        }
    }

    private static async IAsyncEnumerable<string> EnumerateLines(string path)
    {
        var stream = new StreamReader(path);

        while (await stream.ReadLineAsync() is { } line)
        {
            await Task.Delay(500);
            yield return line;
        }
    }
}
```

## Łącza anonimowe

Łącza anonimowe nie posiadają nazwy w systemie plików i z tego powodu nie mogą być używane przez dowolne, niezależne procesy. Ich głównym zastosowaniem jest komunikacja pomiędzy procesem nadrzędnym (rodzicem) a procesem potomnym, który został przez niego utworzony.

Mechanizm działania polega na tym, że proces nadrzędny tworzy łącze anonimowe i przekazuje uchwyt (*handle*) do jednego z jego końców procesowi potomnemu podczas jego uruchamiania. Uchwyt do potoku jest serializowany do postaci ciągu znaków i przekazywany jako argument wiersza poleceń. Proces potomny używa otrzymanego uchwytu, aby połączyć się ze swoim rodzicem. W poniższym przykładzie ten sam program może pełnić obie role – jego zachowanie zależy od argumentów, z jakimi został uruchomiony.

```csharp
using System.Diagnostics;
using System.IO.Pipes;

namespace Anonymous;

class Program
{
    private static async Task Main(string[] args)
    {
        if (args is [var handle])
        {
            await ChildWork(handle);
        }
        else
        {
            await ParentWork();
        }
    }

    private static async Task ParentWork()
    {
        var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]),
            Arguments = pipe.GetClientHandleAsString()
        };

        using var childProcess = Process.Start(startInfo);
        if (childProcess == null)
        {
            Console.WriteLine("Failed to create child process");
            Environment.Exit(-1);
        }
        Console.WriteLine("Child process started with PID: " + childProcess.Id);

        var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        
        await foreach (var line in EnumerateLines("lorem.txt"))
        {
            await writer.WriteLineAsync(line);
        }

        await pipe.DisposeAsync(); // Try to comment that line

        await childProcess.WaitForExitAsync();
    }

    private static async Task ChildWork(string pipeHandle)
    {
        await using var pipe = new AnonymousPipeClientStream(PipeDirection.In, pipeHandle);

        using var reader = new StreamReader(pipe);
        while (await reader.ReadLineAsync() is { } line)
        {
            Console.WriteLine(line.ToUpper());
        }
    }
    
    private static async IAsyncEnumerable<string> EnumerateLines(string path)
    {
        using var stream = new StreamReader(path);

        while (await stream.ReadLineAsync() is { } line)
        {
            await Task.Delay(500);
            yield return line;
        }
    }
}
```

Ważnym elementem w kodzie rodzica jest linia `await pipe.DisposeAsync()`. Zamknięcie potoku po stronie serwera wysyła do klienta informację o końcu strumienia (*end-of-file*, EOF). Dzięki temu wywołanie `reader.ReadLineAsync()` w procesie potomnym zwraca `null`, co pozwala na naturalne zakończenie pętli i poprawne zamknięcie aplikacji potomnej. Bez tego proces potomny czekałby na dane w nieskończoność, a proces rodzica czekałby na zakończenie dziecka, powodując zakleszczenie (*deadlock*).

> [!INFO]
> ## Kod źródłowy
> {{< filetree dir="lectures/ipc/Pipes" >}}
