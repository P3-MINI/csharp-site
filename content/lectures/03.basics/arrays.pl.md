---
title: "Tablice"
weight: 50
---

# Tablice

Tablice to typ referencyjny reprezentujący stałą liczbę elementów tego samego typu. Elementy w tablicy są przechowywane zawsze w **ciągłym obszarze pamięci**. Typ tablicowy oznaczamy za pomocą typu elementu, po którym następuje para nawiasów kwadratowych. Tak jak w normalnych językach programowania tablice indeksujemy od 0.

Właściwość `Length` dostarcza rozmiar tablicy. Po jej stworzeniu nie może on zostać zmieniony.

Wszystkie typy tablicowe dziedziczą po `System.Array`. W tej klasie znajduje się też kilka użytecznych metod, które pomagają operować na tablicach, np. `Sort`, `Reverse`, `BinarySearch`, `Clear`, `Fill`, `Copy`, `Resize` (tworzy nową tablicę i kopiuje elementy).

Elementy tablicy, jeżeli nie są jawnie podane, są niejawnie inicjalizowane wartościami domyślnymi (zerami); dla tablic typów referencyjnych oznacza to tablicę referencji `null`.

Wszystkie błędy związane z indeksowaniem tablic są sprawdzane w czasie działania programu. Wyjście poza zakres tablicy powoduje rzucenie wyjątku `IndexOutOfRangeException`.

```csharp
int[] primes = new int[] {2, 3, 5, 7, 11};
char[] vowels = {'a', 'e', 'i', 'o', 'u'};
uint[] even = [0, 2, 4, 6, 8]; // C# 12 collection expression
float[] data = new float[10];
Array array = primes;
Console.WriteLine($"Primes array length: {primes.Length}");
for (int i = 0; i < primes.Length; i++)
{
    Console.WriteLine(primes[i]);
}
```

## Indeksy i zakresy

Indeksy pozwalają indeksować tablicę od końca za pomocą operatora `^`. `^1` odnosi się do ostatniego elementu, `^2` do przedostatniego itd. **Uwaga:** `^0` odnosi się do `array.Length` (czyli już poza tablicą).

Zakresy pozwalają natomiast wybrać podtablicę za pomocą operatora `..`. Po lewej stronie tego operatora można wstawić indeks początkowy (włącznie), a po prawej indeks końcowy (wyłącznie) wybieranej podtablicy. Domyślnie, jeżeli się ich nie poda, są zastępowane odpowiednio przez `0` i `^0`, czyli początek i koniec.

```csharp
int[] primes = new int[] {2, 3, 5, 7};
int firstElem = primes[0], secondElem = primes[1];
int lastElem = primes[^1], secondToLastElem = primes[^2];
Index first = 0;
Index last = ^1;
firstElem = primes[first]; lastElem = primes[last];

int[] firstTwo = primes[..2]; //exclusive end
int[] withoutFirst = primes[1..]; // inclusive start
int[] withoutLast = primes[..^1];
int[] withoutFirstAndLast = primes[1..^1];
int[] all = primes[..];
Range lastTwoRange = ^2..;
int[] lastTwo = primes[lastTwoRange];
```

## Wyrażenia kolekcji (C# 12)

Od C# 12 tablicę można również utworzyć za pomocą *wyrażenia kolekcji* (*collection expression*) - listy elementów w nawiasach kwadratowych. W przeciwieństwie do inicjalizatora `{ ... }`, którego można użyć tylko w deklaracji (w innych miejscach potrzebne jest `new[] { ... }`), wyrażenie kolekcji może wystąpić wszędzie tam, gdzie oczekiwana jest tablica, np. jako argument metody lub wartość zwracana. Nie ma ono jednak własnego typu, więc typ docelowy musi być znany (`var x = [1, 2];` się nie skompiluje). *Element rozwijający* (*spread element*) `..` wstawia wszystkie elementy innej kolekcji (czegokolwiek, po czym można iterować pętlą `foreach`). Ta sama składnia działa również dla innych kolekcji, np. `List<int> list = [1, 2, 3];` - więcej na ten temat w wykładzie [Kolekcje]({{< ref "/lectures/06.collections/collections" >}}).

```csharp
int[] empty = [];
int[] small = [1, 2, 3];
int[] combined = [0, ..small, ..small[1..], 4]; // [0, 1, 2, 3, 2, 3, 4]
PrintAll([5, 6, 7]); // collection expression as a method argument
// PrintAll({5, 6, 7}); // Compilation error: new[] {5, 6, 7} would be needed here

static void PrintAll(int[] numbers) => Console.WriteLine(string.Join(", ", numbers));
```

## Tablice prostokątne

Tablice prostokątne deklaruje się używając `,` do wyspecyfikowania każdego z wymiarów. Tak samo jak tablice jednowymiarowe, można je zainicjalizować jawnie podając listę elementów. Metoda `GetLength(int)` zwraca długość tablicy wzdłuż `i`-tego wymiaru (zaczynając od 0).

```csharp
float[,] matrix = new float[,]
{
    {1.0f, 0.0f, 0.0f},
    {0.0f, 1.0f, 0.0f},
    {0.0f, 0.0f, 1.0f}
};

float[,] matrix3x4 = new float[3, 4];
for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        Console.WriteLine($"m[{i}, {j}] = {matrix[i, j]}");
    }
}
```
