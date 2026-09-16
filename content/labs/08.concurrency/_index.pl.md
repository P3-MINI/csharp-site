---
title: "Współbieżność"
weight: 80
---

Disclaimer: to wciąż work in progress, brakuje jeszcze faktycznego zadania, a opisy funkcji mogą się zmienić.

# Współbieżność

Do tej pory wszystkie pisane przez nas programy charakteryzowały się jednowątkowością. Kolejne zaprogramowane przez nas zadania były realizowane jedno po drugim. Taki sprosób wykonywania kodu jest wystarczający w większości prostych przypadków, jednak ma on swoje limity. Programowanie współbieżne jest paradygmatem programowania, który pozwala komputerowi realizować wiele zadań jednocześnie, wykorzystując wielowątkową naturę procesorów. Współbieżność może być używana w celu optymalizacji, by rozdzielić zadania pomiędzy wieloma rdzeniami procesora lub w celu zapewnienia równoległego fukcjonowania kilku elementów programu.

## Przydatne funcje, klasy i słowa kluczowe

### async, await
Para słów kluczowych pozwalających na wykonywanie czasochłonnych zadań, bez zawieszania wątku programu. Nie pozwalają one na wykonanie dwóch zadań jednocześnie, ale dbają o to, by w trakcie oczekiwania na realizację czasochłonnego procesu wątek nie był zatrzymany. Wyobraźmy sobie program, który musi pobrać dane z serwera http. Taka operacja może zająć znaczącą z punktu widzenia komputera ilość czasu. Konieczne jest zawarcie połączenia z serwerem, oraz oczekiwanie na pakiety zwrotne. Operacje te są obciążone opóźnieniem niezależnym od komputera użytkownika. W takim przypadku wykożystać możemy słowo kluczowe await. Powoduje ono zwolnienie wątku aż do czasu wykonania zadania po nim określonego.\
```await DownloadDataFromTheInternet();```\
Tak oznaczone wywołanie funkcji spowoduje zwolnienie wątku do czasu jej wykonania. Dopiero potem realizowane będą dalsze linie kodu.\
Słowo kluczowe **async** umieszcza się przed deklaracjami funcji zawierającymi instrukcje używające **await**.
```
async AFunctionUsingAwait()
{
    ...
    await SomeFunction();
}
```

### Task Parallel Library (TPL)
Jest to biblioteka ułatwiająca programowanie współbieżne. Wprowadza klasę Task, która symbolizuje pewne zadanie, które ma być wykonane asynchronicznie. Zadanie definiujemy poprzez przekazanie naszej funkcji do metody Task.Run(). Po wywołaniu jest ono przekazywane do wykonania, nie blokując wątku wywołującego. Wynik działania danego Taska możemy odczytać po jego wykonaniu, zwykle przy użyciu słowa kluczowego await.
```
public int CalculateResult()
{ ... }
static void Main(string[] args)
{
    Task task = Task.Run(CalculateResults);
    
    // Wykonujemy inne operacje
    
    // W pewnym momencie możemy spróbować pobrać rezultat kalkulacji
    int result = await task;
}
```
Jeżeli w momencie próby pobrania rezultatu Task nie zakończył jeszcze działania, wątek zostanie zawieszony, zgodnie ze zwykłym działaniem słowa kluczowego await.\
Częstą praktyką przy uruchamianiu Tasków jest używanie **funkcji lambda** jako argumentów Task.Run(), pozwala to na przekazanie lokalnych zmiennych jako argumentów.
```
Task task = Task.Run(() => CalculateResults(localVariable1, localVariable2));
```

### Parallel
Zbiór funkcji pozwalających na zrównoleglanie operacji. Używane się ich głównie w celu optymalizacji, pozwalają na pełniejsze wykożystanie potencjału procesora poprzez jednoczelne wykonywanie wielu mniejszych części zadania.\
**Parallel.For**     - służy do zrównoleglania tradycyjnych pętli for opartych na indeksach. 
```
int[] dane = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
int[] wyniki = new int[dane.Length];

Parallel.For(0, dane.Length, i =>
{
    wyniki[i] = dane[i] * dane[i];

    Console.WriteLine($"Indeks {i} przetworzony przez wątek {Environment.CurrentManagedThreadId}");
});
```
**Parallel.ForEach** - przetwarza elementy kolekcji (```IEnumerable``` np. listy) w wielu wątkach jednocześnie.
```
var numbers = Enumerable.Range(1, 10000);

Parallel.ForEach(numbers, number =>
{
    ProcessNumber(number);
});

```

### Lock
Korzystanie przez różne wątki ze wspólnych zasobów może doprowadzić do poważnych błędów w funkcjonowaniu programu, rodzi to potrzebę stosowanie synchronizacji. 
Zilustrujmy to na przykładzie programu symulującego magazyn wydający towar kurierom. Wątek kuriera wracającego do magazynu najpierw sprawdza, czy jest tam towar gotowy do 
odebrania, następnie zwiększa swój własny licznik niesionego towaru i zmniejsza licznik magazynowy. Jeżeli nie zastosowaliśmy żadnych narzędzi do synchronizacji, 
to może dość do sytuacji, gdy jeden kurier sprawdzi stan magazynu, znajdując tam jedną paczkę. Następnie, zanim zmniejszy on licznik towaru w magazynie, pojawić się może drugi kurier, 
który również sprawdzi stan towaru. W takim przypadku obaj z nich mogą wykonać instrukcję odebrania paczek i doprowadzić w ten sposób stan towaru w magazynie do wartości -1.
Najprostszą odpowiedzią na ten problem w języku C# jest instrukcja **Lock**. Zapewnia ona wzajemne wykluczenie, dwa wątki nie mogą wejść do ograniczonego przez **Lock** bloku 
kodu jednocześnie. Żeby używać blokady musimy najpierw zdefiniować obiekt ```private readonly System.Threading.Lock _lockObj = new();``` każdy zdefiniowany tak obiekt symbolizuje 
jedną blokadę. Jej użycie wygląda tak:
```
lock(_lockObj)
{
	// operacja wymagająca synchronizacji
}
```
Jeżeli jeden wątek wejdzie do tak oznaczonego bloku, to każdy inny próbujący to zrobić przejdzie w stan uśpienia.

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
  - Oblicz, ile wierszy przypada na jeden wątek, a następnie w pętli przekaż każdemu wątkowi/zadaniu odpowiedni zakres do przetworzenia.

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

## Magazyn i kurierzy

{{% hint info %}}
**Czym jest programowanie równoległe?**

Programowanie równoległe to paradygmat programowania, wykorzystujący architekturę nowoczesnych, wielordzeniowych procesorów (CPU) w celu wykonywania wielu obliczeń równocześnie.

Zamiast przetwarzać dane sekwencyjnie w jednym wątku, rozdzielamy pracę na wiele wątków, które wykonują swoją pracę w tym samym momencie.

{{% /hint %}}

### Opis zadania

Zadanie ma na celu zademonstrowanie konieczności wykorzystania synchronizacji w sytuacji, gdy wiele wątków korzysta ze wspólnych zasobów.
Kod startowy pozwala na uruchomienie prostego symulatora, pokazującego magazyn oraz rozwożących paczki kurierów. Każdy kurier po dotarciu do magazynu podejmuje próbę
odebrania towaru, jeśli ilość towaru jest większa od 0, to odbiera on paczkę i przechodzi do dostawy. W takiej formie program może prowadzić do sytuacji, gdy 
ilość paczek spadnie poniżej 0. Należy uzupełnić plik DeliveryMan.cs o potrzebne instrukcje Lock, które zapobiegną powstawianiu takich sytuacji.
Uwaga: Nie należy edytować klasy Warehouse.
