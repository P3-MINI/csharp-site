---
title: "Klasy"
weight: 10
---

# Klasy

Klasy definiuje się w następujący sposób:

```csharp
class ClassName
{
}
```

W przeciwieństwie do C++, klasy mogą mieć modyfikatory poprzedzające nazwę klasy:
- `internal` - klasy niezagnieżdżone są domyślnie wewnętrzne, co oznacza, że są widoczne tylko wewnątrz *assembly* (pliku wykonywalnego lub `.dll`)
- `public` - widoczne zewsząd
- `static` - nie można tworzyć instancji, musi zawierać tylko statyczne składowe
- `sealed` - z klasy nie można już dziedziczyć
- `abstract` - obiektu tej klasy nie można utworzyć (odpowiednik klasy zawierającej czysto wirtualną metodę w C++)

W C# każda składowa klasy ma swój własny modyfikator dostępu:

```csharp
class MyClass
{
    public int Member1;
    private string _member2;
    protected float Member3;
}
```

## Pola

Pole to zmienna, która jest składową klasy lub struktury.

```csharp
class Student
{
    private string _name; // This is a field
    public int Year; // Still a field
}
```

Modyfikatory pól są podobne do tych w C++, główna różnica leży między `readonly` a `const`. Składowe `const` muszą być inicjalizowane w deklaracji, a wartość musi być znana w czasie kompilacji, natomiast pola `readonly` można przypisać tylko w deklaracji lub w konstruktorze i nie można ich zmienić po zakończeniu konstruktora.

Prywatne pola powinny być pisane w konwencji camelCase, zaczynające się od podkreślenia, w przeciwnym razie w PascalCase. Więcej wskazówek na temat stylu można znaleźć w [dokumentacji](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names).

Niezainicjalizowane pola mają swoją wartość ustawioną na bitowe `0` (czyli `0`, `false`, `null`), dzieje się to przed wywołaniem konstruktora.

## Metody

Metody działają tak samo jak w C++, ale w C# nie dzielimy deklaracji i implementacji na różne pliki, implementacja jest pisana wewnątrz samej klasy.

Jeśli metoda zawiera pojedyncze wyrażenie, takie jak:

```csharp
int Foo (int x) { return x * 2; }
```

Można ją zapisać za pomocą uproszczonej składni:

```csharp
int Foo (int x) => x * 2;
```

W przeciwieństwie do C++ możemy również definiować funkcje lokalne (metodę wewnątrz innej metody):

```csharp
void MyMethod()
{
    void PrintInt(int value) => Console.WriteLine(value);

    PrintInt(1);
    PrintInt(2);
    PrintInt(3);
}
```

Tak samo jak w C++, metody mogą być również przeciążane.

## Konstruktory

Konstruktory są definiowane tak samo jak w C++, ale *nie zawierają listy inicjalizacyjnej.*

Konstruktory mogą wywoływać inne konstruktory za pomocą słowa kluczowego `this`.

```csharp
class Book
{
    private string _title;
    private int _year;
    Book(string title) => _title = title;
    Book(string title, int year) : this(title)
    {
        _year = year;
    }
}
```

Jeśli klasa nie ma zdefiniowanego żadnego konstruktora, to zostanie wygenerowany konstruktor bezparametrowy.

## Dekonstruktory

Dekonstruktor (metoda o nazwie `Deconstruct` z parametrami `out`) działa odwrotnie do konstruktora: przypisuje pola obiektu z powrotem do osobnych zmiennych.

```csharp
class Point
{
    public float x, y;

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void Deconstruct(out float x, out float y)
    {
        x = this.x;
        y = this.y;
    }
}
```

Dekonstruktor można wywołać w następujący sposób:

```csharp
var point = new Point(1, 2);

(float x, float y) = point;
// or: (var x, var y) = point;
// or: var (x, y) = point;

Console.WriteLine($"x: {x}, y: {y}"); // x: 1, y: 2
```

> Nie mylić dekonstrukcji z destrukcją, obiekt po dekonstrukcji jest nadal prawidłowy.

## Inicjalizatory obiektów
Inicjalizator obiektu może być użyty do inicjalizacji dostępnych (np. publicznych) pól lub właściwości bezpośrednio po wywołaniu konstruktora.

```csharp
class Hamster
{
    public string Name;
    public bool LikesViolence;

    public Hamster() {}
    public Hamster(string name) => Name = name;
}
```

Może być użyty w następujący sposób:

```csharp
Hamster h1 = new Hamster {Name = "Boo", LikesViolence=true};
Hamster h2 = new Hamster ("Boo")       {LikesViolence=true};
```

**Kolejność inicjalizacji** to:
1. inicjalizatory pól (wartości przypisane przy deklaracjach pól i właściwości)
2. ciało konstruktora
3. inicjalizator obiektu

## Właściwości

Właściwości wyglądają jak pola, smakują jak pola, ale w rzeczywistości są ukrytymi metodami get i set. Mogą zawierać dodatkową logikę, jeśli jest określona, i mają takie same modyfikatory dostępu jak pola.

```csharp
var book = new ShopItem();
book.Price = 9.99m;
book.Price -= 1.0m;
Console.WriteLine($"Price: {book.Price}$"); // Price: 8.99$

class ShopItem
{
    private decimal _price;
    public decimal Price
    {
        get { return _price; }               // get => _price;
        set { _price = Math.Max(value, 0); } // set => _price = Math.Max(value, 0);
    }
}
```

> Niejawne słowo kluczowe `value` używane w akcesorze `set` reprezentuje wartość, która jest przypisywana do właściwości.

Można pominąć `get` albo `set`. Jeśli ciała `get` i `set` zostaną pominięte, kompilator automatycznie wygeneruje ukryte pole (*backing field*), a właściwość nazywamy wtedy automatyczną (*auto-implemented property*). Jest to najczęstszy przypadek użycia właściwości.

```csharp
class ShopItem
{
    public decimal Price { get; set; } = 0.0m; // auto-implemented properties can be initialized here
    public string Name { get; set; }
}
```

Od wersji C# 14 możesz użyć słowa kluczowego `field`, aby uzyskać bezpośredni dostęp do wygenerowanego przez kompilator ukrytego pola (backing field), bez konieczności jego ręcznego deklarowania:

```csharp
class ShopItem
{
    public decimal Price
    {
        get => field; 
        set => field = Math.Max(value, 0); 
    }
}
```

> Jeden z akcesorów (zwykle `set`) może mieć własny, bardziej restrykcyjny modyfikator dostępu: `public decimal Price { get; private set; }`. W przeciwnym razie akcesory mają modyfikator dostępu właściwości.

## Init-only setter

Akcesor `set` można zastąpić `init` (C# 9), co czyni właściwość tylko do odczytu po inicjalizacji. Właściwość `init` można inicjalizować w konstruktorze, w inicjalizatorze obiektu lub przy deklaracji. Jeśli pominiemy akcesor `set`, właściwość nadal może być inicjalizowana w konstruktorze lub w deklaracji (ale nie w inicjalizatorze obiektu) i nie może być później zmieniana.

## Indeksery
Indeksery są podobne do przeciążania `operator[]` w C++. Aby napisać indekser, definiujemy właściwość o nazwie `this`, określając argumenty w nawiasach kwadratowych:

```csharp
class Sentence
{
    private string[] Words { get; }

    public string this[int i]
    {
        get => Words[i];
        set { Words[i] = value; }
    }

    public Sentence(string sentence) => Words = sentence.Split(' ');
}
```

Używamy indekserów w ten sam sposób, w jaki używamy tablicy, z tą różnicą, że możemy zdefiniować argumenty indeksu dowolnego typu. Typ może deklarować wiele indekserów, a indeksery mogą mieć wiele parametrów.

```csharp
Sentence sentence = new Sentence("The quick brown fox jumps over lazy dog");
sentence[1] = "swift";
Console.WriteLine(sentence[3]); // fox
```

## Finalizatory

Finalizatory mogą wydawać się destruktorami z C++, ale różnica polega na tym, że nigdy nie wiemy, kiedy finalizator zostanie wywołany. Po tym, jak ostatnia referencja do obiektu zginie, obiekt czeka na odśmiecanie pamięci. Kiedy to nastąpi? Kiedy Garbage Collector uzna to za stosowne - najczęściej, gdy jest presja na pamięć. Finalizator może nawet nigdy nie zostać wywołany: .NET nie uruchamia finalizatorów przy zamykaniu aplikacji. Przez to nie jest to zbyt przydatna konstrukcja; zasoby należy zwalniać deterministycznie za pomocą [`IDisposable`]({{< ref "/lectures/05.csharp-features/disposable" >}}).

```csharp
class Class1
{
    ~Class1()
    {
    }
}
```
