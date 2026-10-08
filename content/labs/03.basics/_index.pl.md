---
title: "Podstawy C#"
weight: 30
---

## Tutorial 3: Podstawy C#

Zakres tego laboratorium obejmuje podstawy języka C# (składnia, system typów, typy podstawowe, tablice, parametry) oraz tworzenie własnych typów (klasy, struktury, interfejsy, dziedziczenie, typy wyliczeniowe). 

Jako że znasz już język C++, wiele konstrukcji w C# wyda Ci się znajomych. Przed rozpoczęciem zadania, omówimy najważniejsze różnice i zagadnienia.

### Przestrzenie nazw

Podobnie jak w C++, w C# używasz przestrzeni nazw, aby grupować powiązany kod i unikać konfliktów nazw. W C# kropki (`.`) używasz do odwoływania się do przestrzeni nazw oraz składowych klas i struktur (np. `System.Console.WriteLine`).

Dyrektywa `using` pozwala importować przestrzeń nazw do obecnego pliku, co pozwala na bezpośrednie korzystanie z jej typów bez podawania pełnej ścieżki (np. `using System;`). W nowszych wersjach C# powszechnie stosuje się przestrzenie nazw o zasięgu pliku (file-scoped namespace), co pozwala uniknąć dodatkowego poziomu wcięć:

```csharp
namespace MyApp.Core;

// All code in this file belongs to MyApp.Core
public class Calculator 
{
    // ...
}
```

### Klasy i Struktury

W C++ różnica między `class` a `struct` sprowadza się jedynie do domyślnej widoczności składowych. W C# ta różnica dotyczy sposobu zarządzania pamięcią.

- **Klasy (`class`) to typy referencyjne.** Są alokowane na stercie, a ich cyklem życia zarządza Garbage Collector. Zmienna przechowująca klasę przechowuje referencję do obiektu. Klasy są przeznaczone dla złożonych obiektów, które posiadają swoją tożsamość, wymagają dłuższego cyklu życia lub korzystają z dziedziczenia.
- **Struktury (`struct`) to typy bezpośrednie.** Są alokowane na stosie lub osadzone bezpośrednio wewnątrz innych obiektów. Przypisanie struktury do innej zmiennej lub przekazanie jej jako argument do metody domyślnie tworzy jej pełną kopię. Struktury stosuje się do małych typów danych (zazwyczaj do ok. 16 bajtów).

### Właściwości (Properties)

Enkapsulacja jest jednym z założeń programowania obiektowego. Aby ukryć pola klasy, często tworzy się metody dostępowe, tzw. gettery i settery (np. `float GetX()`, `void SetX(float v)`). C# wspiera koncepcję enkapsulacji poprzez **Właściwości** (Properties). Z zewnątrz używasz ich jak zwykłych zmiennych, ale pod spodem kompilator sam generuje ukryte metody odczytu i zapisu:

```csharp
public struct Point3
{
    // Private, backing field
    private float x;

    // Classic property
    public float X 
    {
        get { return x; }
        set { x = value; } // 'value' is the implicit setter parameter
    }

    // Auto-property - compiler generates the backing field
    public float Y { get; set; }
    
    // Read-only property from the outside
    public float Z { get; private set; }
}
```

Odwoływanie się do zdefiniowanych właściwości wygląda następująco:

```csharp
Point3 p = new Point3();
p.X = 5.0f;       // Uses setter
float val = p.X;  // Uses getter

// p.Z = 2.0f;    // Error: setter is private
```

### Tablice

W C# tablice są typami referencyjnymi i zawsze znają swój rozmiar (`Length`). C# kontroluje granice tablicy – wyjście poza jej zakres kończy się rzuceniem wyjątku `IndexOutOfRangeException`.

```csharp
// Create an array of 100 elements (initialized to default values, 0 for int)
int[] numbers = new int[100];
int length = numbers.Length; // Array knows its size
```

Oprócz tablic jednowymiarowych (oraz tablic tablic, czyli tzw. tablic poszarpanych/jagged arrays), C# obsługuje też wbudowane, spójne pamięciowo tablice wielowymiarowe:

```csharp
// 2D array [width, height]
float[,] grid = new float[800, 600];
grid[0, 0] = 1.5f; // Access specific element
```

### Typ `object` i metoda `ToString()`

W systemie typów C# **każdy typ** (zarówno referencyjny, jak i bezpośredni) dziedziczy po klasie `System.Object` (w skrócie `object`), co oznacza, że każda instancja posiada pewien standardowy zestaw metod.

Często używaną metodą jest wirtualna metoda `ToString()`, która zwraca tekstową reprezentację obiektu. Domyślnie zwraca nazwę typu, ale zazwyczaj chcesz ją **nadpisać**. Używasz do tego słowa kluczowego `override`:

```csharp
public struct Point3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    // Override the built-in ToString() method from object
    public override string ToString()
    {
        // String interpolation
        return $"[{X}, {Y}, {Z}]";
    }
}
```

Przykładowe użycie tej metody w praktyce:

```csharp
using System;

// Object initialization syntax
Point3 p = new Point3 { X = 1.0f, Y = 2.0f, Z = 3.0f };

// Automatically calls ToString() under the hood
Console.WriteLine(p); // Output: [1, 2, 3]
```

## Zadanie 1 - Śledzenie promieni

W ramach tego zadania stworzysz prostą aplikację do śledzenia promieni. Zadanie to opiera się na materiałach z książki [Raytracing in One Weekend](https://raytracing.github.io/books/RayTracingInOneWeekend.html). Będzie się ona składała z pojedynczego projektu - aplikacji wykonywalnej.

Śledzenie promieni (ang. *ray tracing*) to technika generowania obrazów. W metodzie tej rzucamy promienie od obserwatora (kamery) przez każdy piksel rzutni w głąb sceny. Następnie algorytm sprawdza, z jakimi obiektami przecina się dany promień i na tej podstawie oblicza kolor piksela.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/03.basics/example1" >}})
