---
title: "Tcp"
weight: 40
---

# Tcp

Protokół `TCP` (*Transmission Control Protocol*) jest jednym z fundamentów internetu. Jest to protokół połączeniowy, co oznacza, że przed wymianą danych nawiązywane jest połączenie między klientem a serwerem. TCP gwarantuje **niezawodne i uporządkowane dostarczanie danych**, co czyni go idealnym do zastosowań, gdzie integralność danych jest kluczowa, jak np. przesyłanie plików, poczta e-mail. W .NET komunikacja TCP jest obsługiwana przez klasy w przestrzeni nazw `System.Net` i `System.Net.Sockets`.

Komunikacja za pomocą `TCP` w .NET opiera się na dwóch klasach: `TcpListener` po stronie serwera i `TcpClient` po stronie klienta.

## Serwer

Serwer nasłuchuje na przychodzące połączenia za pomocą `TcpListener`. Kiedy połączenie jest akceptowane (`AcceptTcpClientAsync`), tworzony jest obiekt `TcpClient` reprezentujący to połączenie. Serwer w przykładzie jest asynchroniczny i może obsługiwać wielu klientów jednocześnie. Dla każdego klienta tworzone jest osobne zadanie (`HandleClient`), które odczytuje komendy i wysyła odpowiedzi.

Po nawiązaniu połączenia, obie strony komunikują się za pomocą strumienia (`NetworkStream`), który jest "opakowany" w `StreamReader` i `StreamWriter` dla ułatwienia operacji tekstowych.

> [!WARNING]
> Należy pamiętać, że `StreamReader` i `StreamWriter` wewnętrznie buforują dane. Najlepiej sprawdzają się w protokołach, w których wiadomości są tekstowe i każda z nich jest zakończona znakiem nowej linii. Może to prowadzić do problemów, jeśli protokół jest bardziej złożony. W takich przypadkach lepszym wyborem może być użycie `BinaryReader` i `BinaryWriter`, które dają precyzyjną kontrolę nad odczytem i zapisem danych do strumienia, nawet jeśli są to dane tekstowe.

Zdefiniowany "protokół" jest bardzo prosty - klient wysyła tekstowe komendy (`date`, `time`, `exit`), a serwer odsyła odpowiednią wiadomość. W rzeczywistych aplikacjach protokoły są często bardziej złożone i mogą opierać się na formatach takich jak `JSON` lub na strukturach binarnych.

```csharp
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Server;

public static class Program
{
    const int Port = 5000;
    public static async Task Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
        using var listener = new TcpListener(ipEndPoint);
        
        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Wait any key to exit");
            Console.ReadKey(true);
            cts.Cancel();
        });

        await AcceptClients(listener, cts.Token);
        await waitForAnyKey;
    }

    private static async Task AcceptClients(TcpListener listener, CancellationToken token = default)
    {
        listener.Start(backlog: 10);
        var clients = new List<Task>();
        while (true)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                clients.Add(HandleClient(client, token));
                clients.RemoveAll(task => task.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        listener.Stop();
        await Task.WhenAll(clients);
    }

    private static async Task HandleClient(TcpClient client, CancellationToken token = default)
    {
        Console.WriteLine("New client connected");
        try
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };
            while (await reader.ReadLineAsync(token) is { } command)
            {
                Console.WriteLine($"Received command: {command}");
                switch (command)
                {
                    case "date":
                        string message = DateTime.Now.ToString("d", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "time":
                        message = DateTime.Now.ToString("t", CultureInfo.InvariantCulture);
                        await writer.WriteLineAsync(message.ToCharArray(), token);
                        break;
                    case "exit":
                        return;
                    default:
                        await writer.WriteLineAsync("Unknown command".ToCharArray(), token);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, which is expected during shutdown.
        }
        catch (IOException)
        {
            // Client disconnected abruptly.
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling client: {e.Message}");
        }
        finally
        {
            client.Dispose();
            Console.WriteLine("Client disconnected");
        }
    }
}
```

## Klient

**Klient TCP** inicjuje połączenie z serwerem. Używa do tego klasy `TcpClient` i jej metody `ConnectAsync`, podając adres IP i port serwera. Po połączeniu, podobnie jak serwer, używa strumienia opakowanego w `StreamReader` i `StreamWriter` do wysyłania poleceń i odbierania odpowiedzi.

```csharp
using System.Net;
using System.Net.Sockets;

namespace Client;

public static class Program
{
    const int Port = 5000;
    public static async Task Main()
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Loopback, Port);
        using var client = new TcpClient();
        await client.ConnectAsync(ipEndPoint);
        Console.WriteLine("Client connected");
        
        var reader = new StreamReader(client.GetStream());
        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

        while (Console.ReadLine() is { } line)
        {
            await writer.WriteLineAsync(line);
            var response = await reader.ReadLineAsync();
            if (response is null) break;
            Console.WriteLine(response);
        }
    }
}
```

> [!INFO]
> ## Kod źródłowy
> {{< filetree dir="lectures/ipc/Networking" >}}
