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

### Etap 1: Reprezentacja kolorów i generowanie obrazu

Naszą pracę zaczniemy od zdefiniowania dwóch typów: struktury opisującej wektor (użyjemy go również do reprezentacji punktów i koloru) oraz klasy reprezentującej obraz.

#### Struktura Vector3

Użyjemy struktury agregującej trzy liczby, której poszczególne składniki (X, Y, Z) mogą odpowiadać kanałom RGB (Red, Green, Blue) lub współrzędnym w przestrzeni. Utwórz plik `Vector3.cs` i zdefiniuj:

1. Publiczne właściwości `X`, `Y`, `Z` typu `float`.
2. Publiczny konstruktor przyjmujący trzy wartości, przypisujący je do właściwości.
3. Nadpisaną metodę `ToString()`, zwracającą składniki wektora oddzielone spacją (np. `"1.5 0.2 0.8"`).

#### Klasa Image i format PPM

Obraz traktujemy jako dwuwymiarową tablicę pikseli. Utwórz klasę `Image` (plik `Image.cs`). Klasa powinna zawierać:
- Publiczne właściwości tylko do odczytu `Width` i `Height` typu `int`.
- Prywatną tablicę dwuwymiarową `Vector3[,] Pixels`.
- Konstruktor przyjmujący szerokość i wysokość, który na ich podstawie zainicjalizuje dwuwymiarową tablicę.
- Indeksator (odpowiednik przeciążonego `operatora[]` z C++), który pozwala na odwoływanie się do obiektu tak jak do tablicy. Posłuży on do wygodnego odczytywania i zapisywania pikseli (np. `image[x, y] = color`):

```csharp
public Vector3 this[int x, int y]
{
    get => Pixels[x, y];
    set => Pixels[x, y] = value;
}
```

Aby wyeksportować wygenerowany obraz, wykorzystamy prosty, tekstowy format **PPM**. Nie będziemy implementować osobnej metody zapisującej obraz do pliku. Zamiast tego zaimplementujemy proces generowania zawartości PPM wewnątrz nadpisanej metody `ToString()`. Wywołanie `Console.WriteLine(image)` wypisze obraz w formie tekstowej na standardowe wyjście, które przekierujemy do pliku.

Struktura formatu PPM wygląda następująco:
1. Magiczny ciąg znaków `P3` (zbieżność z nazwą przedmiotu Programowanie 3 przypadkowa).
2. Szerokość i wysokość obrazka (oddzielone spacją).
3. Maksymalna wartość dla koloru (w naszym przypadku `255`).
4. Całkowitoliczbowe wartości R, G, B pikseli (od lewego górnego rogu). Ich ułożenie w wierszach jest dowolne, ważne jedynie, aby wartości były oddzielone białymi znakami.

Poniżej znajduje się przykładowy plik dla obrazka o wymiarach 3x2 (trzy kolumny, dwa wiersze):

```text
P3
3 2
255
255   0   0     0 255   0     0   0 255
255 255   0   255 255 255     0   0   0
```

Twoim zadaniem jest zaimplementowanie metody `ToString()`. Dodaj do tworzonego tekstu odpowiedni nagłówek, a następnie – za pomocą zagnieżdżonych pętli – przeiteruj po wszystkich pikselach na obrazie. Składowe każdego wektora (`float`) zmapuj na format RGB (zakres `[0, 255]`). Upewnij się, że wartości nie wykraczają poza ten przedział, np. używając `Math.Clamp`.

Ponieważ będziemy generować ciąg tekstowy dla milionów wartości, użycie konkatenacji stringów (operator `+`) w pętli byłoby bardzo niewydajne z uwagi na ciągłą alokację nowych obiektów. Do optymalnego zbudowania wyniku wykorzystaj klasę `StringBuilder` (zdefiniowaną w przestrzeni nazw `System.Text`).

#### Generowanie pierwszego obrazka

W pliku `Program.cs` utwórz instancję obrazu o rozdzielczości 1920x1080. Wygeneruj dowolny wzór (na przykład gradient) wypełniając nim piksele, a następnie wypisz obraz na standardowe wyjście. Możesz wykorzystać poniższy przykładowy fragment kodu:

```csharp
namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Image image = new Image(1920, 1080);
        
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                float r = (float) i / image.Width;
                float g = (float) j / image.Height;
                float b = MathF.Sin(2 * MathF.PI * ((float) i * j / image.Width / image.Height));
                
                image[i, j] = new Vector3(r, g, b);
            }
        }
        
        Console.WriteLine(image);
    }
}
```

Aby zapisać obrazek, przekieruj standardowe wyjście do pliku. Uruchom program poleceniem:

```bash
dotnet run > image.ppm
```

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/03.basics/example1" >}})
