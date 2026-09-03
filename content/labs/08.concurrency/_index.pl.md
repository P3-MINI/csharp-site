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