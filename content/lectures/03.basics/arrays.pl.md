---
title: "Tablice"
weight: 50
---

# Tablice

Tablice to typ referencyjny reprezentujący stałą liczbę elementów tego samego typu. Elementy w tablicy są przechowywane zawsze w **ciągłym obszarze pamięci**. Typ tablicowy oznaczamy za pomocą typu i par nawiasów kwadratowych. Tak jak w normalnych językach programowania tablice indeksujemy od 0.

Właściwość `Length` dostarcza rozmiar tablicy. Po jej stworzeniu nie może on zostać zmieniony.

Wszystkie typy tablicowe dziedziczą po `System.Array`. W tej klasie znajduje się też kilka użytecznych metod które pomagają operować na tablicach, np. `Sort`, `Reverse`, `BinarySearch`, `Clear`, `Fill`, `Copy`, `Resize` (tworzy nową tablicę i kopiuje elementy).

Elementy tablicy, jeżeli nie są jawnie podane, to są niejawnie inicjalizowane zerami, dla tablic typów referencyjnych oznacza to tablicę `null` referencji.

Wszystkie błędy związane z indeksowaniem tablic są sprawdzane w czasie działania programu. Wyjście poza zakres tablicy powoduje rzucenie wyjątku `IndexOutOfRangeException`.

```csharp
int[] primes = new int[] {2, 3, 5, 7, 11};
char[] vowels = {'a', 'e', 'i', 'o', 'u'};
uint[] even = [0, 2, 4, 6, 8]; // C# 12
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

Zakresy pozwalają natomiast wybrać podtablicę za pomocą operatora `..`. Po lewej stronie tego operatora można wstawić indeks od którego elementu włącznie, a po prawej indeks do którego elementu wyłącznie wybrać podtablicę. Domyślnie jeżeli się ich nie poda to są zastępowane odpowiednio przez `0` i `^0`, czyli początek i koniec.

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
for (int i = 0; i < matrix.Length(0); i++)
{
    for (int j = 0; j < matrix.Length(1); j++)
    {
        Console.WriteLine($"m[{i}, {j}] = {matrix[i, j]}");
    }
}
```
