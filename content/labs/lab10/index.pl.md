---
title: "Lab10"
weight: 10
---

# Laboratorium 10: Programowanie równoległe i asynchroniczne

## Kod początkowy

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab10/student" >}}

## Generowanie fraktali

{{% hint info %}}
**Czym jest programowanie równoległe?**

Programowanie równoległe to paradygmat programowania, wykorzystujący architekturę nowoczesnych, wielordzeniowych procesorów (CPU) w celu wykonywania wielu obliczeń równocześnie.

Zamiast przetwarzać dane sekwencyjnie w jednym wątku, rozdzielamy pracę na wiele wątków, które wykonują swoją pracę w tym samym momencie.

{{% /hint %}}

### Opis zadania

Celem zadania jest implementacja oraz porównanie czasu wykonania różnych metod zrównoleglających obliczenia wykonywane podczas generowania [Zbioru Mandelbrota](https://en.wikipedia.org/wiki/Mandelbrot_set).

Kod początkowy zawiera abstrakcyjną klasę `MandelbrotSetGenerator`, która zarządza całym procesem generowania fraktala i zapisywania go do pliku `.png`

Klasa `SingleThreadGenerator` stanowi konkretną jednowątkową implementację generatora. Używa ona prostej, zagnieżdżonej pętli `for` do iteracji po wszystkich pikselach generowanego obrazka. Posłuży jako linia bazowa do pomiaru wydajności.

Należy zaimplementować następujące metody zrównoleglania obliczeń:

- `MultiThreadGenerator`: Metoda wielowątkowa, która ręcznie tworzy i zarządza obiektami `Thread`.
- `TasksGenerator`: Metoda, która używa klasy `Task` z biblioteki TPL (ang. _Task Parallel Library_) do zarządzania pracą równoległą w puli wątków (`ThreadPool`).
- `ParallelGenerator`: Metoda, która używa wysokopoziomowej klasy `Parallel` z biblioteki TPL.

`Program.cs` zawiera logikę mierzenia czasu i uruchamiania każdego generatora po kolei.

Poprawnie wygenerowany fraktal powinien wyglądać następująco:

<div style="text-align: center;">
  <img src="/labs/lab10/mandelbrotset.png" width="400px" alt="mandelbrotset.png" />
</div>

{{% hint warning %}}
**Uwagi implementacyjne**

- **Liczba wątków:**
  - W implementacjach `MultiThreadGenerator` i `TasksGenerator` należy stworzyć `N` jednostek pracy (wątków/zadań), gdzie `N` jest równe liczbie rdzeni procesora. Jest to optymalna liczba dla zadań w 100% obciążających CPU.
- **Podział pracy:**
  - W przypadku generowania fraktala, najprostszą strategią jest podział obrazu na `N` równych, poziomych pasów.
  - Oblicz, ile wierszy przypada na jeden wątek, a następnie w pętli przekaż każdemi wątkowi/zadaniu odpowiedni zakres do przetworzenia.

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: Threads and threading](https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)
- [Microsoft Learn: Write a Simple Parallel.For Loop](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP] 
> **Rozwiązanie** 
> {{< filetree dir="labs/lab10/solution/FractalsGenerator" >}}

## Agregator ofert

{{% hint info %}}
**Czym jest programowanie asynchroniczne?**

Programowanie asynchroniczne służy do nieblokowania wątku wykonawczego podczas oczekiwania na operacje, które są poza naszą kontrolą.

Używamy go między innymi do takich zadań jak oczekiwanie na odpowiedź z sieci (API), wysyłanie zapytań do bazy danych, czy odczytywanie zawartości dużego pliku z dysku.

W tym przypadku, celem nie jest szybsze liczenie, a lepsze zarządzanie czasem oczekiwania i responsywnością aplikacji.

{{% /hint %}}

### Opis zadania

Celem zadania jest zbudowanie asynchronicznego klienta konsolowego (w projekcie `FlightScanner.Client`), który będzie agregował oferty lotnicze z wielu serwisów jednocześnie. Klient będzie komunikował się z lokalnie uruchomionym REST-owym API (projekt `FlightScanner.API`).

API, działające pod adresem `http://localhost:5222` (adres można znaleźć w pliku [launchSettings.json](/labs/lab10/solution/FlightScanner.API/Properties/launchSettings.json)), jest w pełni zaimplementowane i nie wymaga modyfikacji. Należy je uruchomić przed uruchomieniem aplikacji konsolowej.

> [!NOTE]
> Na Linuxie uruchom projekt `FlightScanner.API` przez dostarczony skrypt `run.sh`. Pobierze on brakujące środowisko uruchomieniowe `ASP.NET` i uruchomi projekt.

Aplikacja kliencka powinna:

- Jednokrotnie odpytać "centralny" endpoint API (`/api/providers`), aby pobrać listę dostępnych linii lotniczych.
- Dla każdej linii lotniczej z pobranej listy, aplikacja musi równocześnie (współbieżnie) wysłać zapytanie do jej dedykowanego endpointu (np. `/api/flights/reliable-air`), aby pobrać konkretne oferty lotów.
- Wszystkie pobrane oferty z różnych linii muszą zostać zebrane, odfiltrowane z błędów i zagregowane w jedną, "płaską" listę.
- Na koniec, aplikacja ma wyświetlić 10 najtańszych znalezionych ofert.

#### Modele Danych

API operuje na następujących obiektach DTO (ang. _Data Transfer Objects_). Znajdują się one we współdzielonej bibliotece klas `FlightScanner.Common`.

```csharp
// Received from /api/providers
public record PartnerAirlineDto(
    string Id,
    string Name,
    string Endpoint
);

// Received from airline providers
public record ProviderResponseDto(
    string ProviderName,
    List<FlightOfferDto> Flights
);

// Part of ProviderResponseDto
public record FlightOfferDto(
    string FlightId,
    string Origin,
    string Destination,
    decimal Price
);
```

{{% hint warning %}}
**Uwagi implementacyjne**

- **Współbieżność:**
  - Odpytywanie wszystkich linii lotniczych musi odbywać się w tym samym czasie, a nie sekwencyjnie.
- **Globalny _timeout_:**
  - Cała operacja zbierania ofert musi mieć globalny limit czasu (np. 3 sekundy), narzucony przez `CancellationTokenSource`.
  - Jeśli którykolwiek dostawca nie odpowie w tym czasie, jego oferta jest pomijana.
- **Raportowanie postępu:**
  - Aplikacja musi raportować postęp w czasie rzeczywistym, używając interfejsu `IProgress<T>`.
- **Odporność na Błędy:**
  - Jeśli endpoint jednego dostawcy zwróci błąd HTTP (np. `500`, `404`) lub przekroczy limit czasu, aplikacja nie może się zatrzymać.
  - Powinna zignorować tego dostawcę i kontynuować pracę z pozostałymi.

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: Cancellation in Managed Threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Microsoft Learn: Make HTTP requests in a .NET](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient)
- [Microsoft Learn: IProgress<T> Interface](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1?view=net-9.0)
- [Microsoft Learn: Task.WhenAll Method](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=net-9.0)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP] 
> **Rozwiązanie** 
> {{< filetree dir="labs/lab10/solution/FlightScanner.Client" >}}
>
> **Wyjście:** > [FlightScanner.Client.txt](/labs/lab10/outputs/FlightScanner.Client.txt) > [FlightScanner.API.txt](/labs/lab10/outputs/FlightScanner.API.txt)
