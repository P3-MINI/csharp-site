---
title: "Współbieżność"
weight: 80
---

Disclaimer: to wciąż work in progress, brakuje jeszcze faktycznego zadania, a opisy funkcji mogą się zmienić.

# Współbieżność

Do tej pory wszystkie pisane przez nas programy charakteryzowały się jednowątkowością. Kolejne instrukcje programu były wykonywane sekwencyjnie, jedna po drugiej. Taki sposób wykonywania kodu jest wystarczający w większości prostych przypadków, jednak w bardziej złożonych programach może prowadzić do nieefektywnego wykorzystania dostępnych zasobów.

**Programowanie współbieżne** jest podejściem, które pozwala programowi obsługiwać wiele zadań w tym samym czasie, przeplatając ich wykonywanie. Zadania te nie muszą być wykonywane dokładnie w tej samej chwili, faktyczne jednoczesne wykonywanie zależy między innymi od liczby dostępnych rdzeni procesora. Współbieżność pozwala więc organizować pracę programu tak, aby wiele niezależnych zadań mogło robić postęp niezależnie od siebie.

Współbieżność może być realizowana między innymi za pomocą **wielu wątków**. Jeżeli dostępnych jest kilka rdzeni procesora, poszczególne zadania mogą być dodatkowo wykonywane **równolegle**, czyli faktycznie w tym samym czasie.

## Przydatne funcje, klasy i słowa kluczowe

### async, await

Para słów kluczowych wykorzystywanych do tworzenia metod asynchronicznych, które pozwalają wykonywać czasochłonne operacje bez blokowania wątku podczas oczekiwania na ich zakończenie.
Asynchroniczność nie oznacza, że program może wykonywać tylko jedno zadanie naraz. Możemy na przykład rozpocząć kilka operacji asynchronicznych, a następnie oczekiwać na ich zakończenie za pomocą `Task.WhenAll`. Dzięki temu operacje mogą być realizowane współbieżnie.

Wyobraźmy sobie program, który musi pobrać dane z serwera HTTP. Taka operacja może zająć znaczącą ilość czasu, ponieważ konieczne jest nawiązanie połączenia z serwerem oraz oczekiwanie na przesłanie danych. Część tego czasu wynika z opóźnień sieciowych, na które komputer użytkownika nie ma bezpośredniego wpływu.

W takim przypadku możemy wykorzystać słowo kluczowe `await`.

```
await DownloadDataFromTheInternet();
```

`await` powoduje oczekiwanie na zakończenie wskazanego zadania. Jeżeli zadanie nie jest jeszcze zakończone, metoda asynchroniczna zostaje w tym miejscu zawieszona, a wątek może kontynuować pracę, zostać wykorzystany do wykonania innych zadań. Gdy oczekiwane zadanie zostanie zakończone, wykonanie metody zostanie wznowione od miejsca, w którym użyto `await`.

Jeżeli oczekiwane zadanie zwraca wynik, `await` pozwala również pobrać ten wynik.

```
string data = await DownloadDataFromTheInternet();
```

Słowo kluczowe `async` umieszcza się przed deklaracją metody, która wykorzystuje `await`. Metoda oznaczona async zwraca zazwyczaj Task lub Task<T>. Task oznacza operację asynchroniczną, która nie zwraca wartości, natomiast Task<T> oznacza operację, której wynikiem będzie wartość typu T.

```
async Task Download()
{
    string data = await DownloadDataFromTheInternet();
}
```
Metoda nie zwraca bezpośrednio wyniku, ale Task reprezentujący wykonywaną operację.

Jeżeli metoda zwraca wynik to definiujemy ją tak:
```
async Task<string> DownloadData()
{
    return await DownloadDataFromTheInternet();
}
```
Sama metoda zwraca obiekt typu Task\<string\>. Dopiero użycie await pozwala otrzymać właściwy wynik typu string.
```
string data = await DownloadData();
```
`async` samo w sobie nie powoduje uruchomienia nowego wątku. Określa, że metoda może wykonywać operacje asynchroniczne i może zostać zawieszona w miejscach oznaczonych słowem `await`.

### Task Parallel Library (TPL)
Jest to biblioteka zapewniająca mechanizmy tworzenia programów współbieżnych i równoległych. Jednym z najważniejszych jej elementów jest klasa Task, która reprezentuje *operację wykonywaną asynchronicznie*. Obiekt Task pozwala nam śledzić stan operacji, oczekiwać na jej zakończenie oraz w przypadku Task<T> uzyskać zwrócony przez nią wynik.

Istnieje wiele sposobów tworzenia i uruchamiania zadań. Jednym z nich jest metoda ```Task.Run()```. Służy ona przede wszystkim do zlecania wykonania synchronicznej, ale kosztownej obliczeniowo operacji.
```
Task<int> task = Task.Run(CalculateResults);
```
Wywołanie Task.Run() zwraca obiekt Task<int> reprezentujący rozpoczętą operację. Wykonanie operacji odbywa się na wątku z puli wątków, dzięki czemu wątek wywołujący może w tym czasie wykonywać inne zadania.

Na zakończenie operacji możemy zaczekać za pomocą await.
```
int result = await task;
```
Jeżeli zadanie nie zostało jeszcze wykonane, to await wstrzyma wykonanie metody bez blokowania wątku. Po zakończeniu zadania wykonywanie metody zostanie wznowione.

Task.Run() nie jest jednak jedynym sposobem tworzenia obiektów Task. W szczególności operacje wejścia/wyjścia, takie jak komunikacja sieciowa lub odczyt plików, mogą same udostępniać asynchroniczne metody zwracające Task lub Task<T>, bez konieczności przenoszenia ich wykonania do osobnego wątku.

Przykład użycia:

```
static int CalculateResults()
{
    // czasochłonne obliczenia
    return result;
}
```
Jeżeli `CalculateResults()` jest kosztownym obliczeniem:

```
static async Task Main(string[] args)
{
    Task<int> task = Task.Run(CalculateResults);

    // Wykonujemy inne operacje

    // W pewnym momencie możemy pobrać rezultat kalkulacji
    int result = await task;
}
```

`Task.Run` uruchamia obliczenie na wątku z puli wątków, a `await task` pozwala później zaczekać na jego zakończenie i pobrać wynik.

Częstą praktyką przy uruchamianiu Tasków jest używanie **funkcji lambda** jako argumentu Task.Run(), pozwala to na przekazanie lokalnych zmiennych jako argumentów.
```
Task task = Task.Run(() => CalculateResults(localVariable1, localVariable2));
```

### Parallel
Klasa zawierająca zbiór funkcji pozwalających na zrównoleglanie operacji. Używane się ich głównie w celu poprawienia wydajności, pozwalają na pełniejsze wykożystanie potencjału procesora poprzez jednoczelne wykonywanie wielu mniejszych części zadania.\
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
