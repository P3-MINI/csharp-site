---
title: "Typy podstawowe"
weight: 20
---

# Typy podstawowe

Słowa kluczowe typów wbudowanych (np. `int`, `double`, `bool`) są bezpośrednimi aliasami dla typów zdefiniowanych w przestrzeni nazw `System` (np. `System.Int32`, `System.Double`, `System.Boolean`). Warto pamiętać, że te typy to zwykłe klasy w bibliotece standardowej, w nich są zdefiniowane przydatne stałe i metody, np. `int.MaxValue`, `int.Parse(string)`, `float.NegativeInfinity`.

## Typy numeryczne

| Typ | Nazwa .NET | Rozmiar | Zakres (przybliżony) | Precyzja | Sufiks literału |
|---|---|---|---|---|---|
| **sbyte** | `System.SByte` | 8 bitów | `-128` do `127` | - | - |
| **byte** | `System.Byte` | 8 bitów | `0` do `255` | - | - |
| **short** | `System.Int16` | 16 bitów | `-32 768` do `32 767` | - | - |
| **ushort** | `System.UInt16` | 16 bitów | `0` do `65 535` | - | - |
| **int** | `System.Int32` | 32 bity | `-2,1×10⁹` do `2,1×10⁹` | - | - |
| **uint** | `System.UInt32` | 32 bity | `0` do `4,2×10⁹` | - | `U` lub `u` |
| **long** | `System.Int64` | 64 bity | `-9×10¹⁸` do `9×10¹⁸` | - | `L` lub `l` |
| **ulong** | `System.UInt64` | 64 bity | `0` do `18×10¹⁸` | - | `UL` lub `ul` |
| **float** | `System.Single` | 32 bity | `±1.5×10⁻⁴⁵` do `±3.4×10³⁸` | ~6-9 cyfr | `F` lub `f` |
| **double** | `System.Double` | 64 bity | `±5.0×10⁻³²⁴` do `±1.7×10³⁰⁸` | ~15-17 cyfr | `D` lub `d` |
| **decimal** | `System.Decimal` | 128 bitów | `±1.0×10⁻²⁸` do `±7.9×10²⁸` | 28-29 cyfr | `M` lub `m` |


## Konwersje numeryczne

C# rozróżnia dwa rodzaje konwersji między typami numerycznymi.

### Konwersje niejawne (Implicit)

Są to konwersje bezpieczne, wykonywane automatycznie przez kompilator, gdy nie ma ryzyka utraty danych (z małymi wyjątkami). Konwersja jest możliwa z typu o mniejszym zakresie do typu o większym zakresie.

- **Z całkowitoliczbowych na całkowitoliczbowe**: `sbyte` → `short` → `int` → `long`
- **Z całkowitoliczbowych na zmiennoprzecinkowe**: `int` → `float` (ryzyko utraty precyzji) → `double`
- `long` można niejawnie konwertować na `float` lub `double` (ryzyko utraty precyzji).

```csharp
int i = 100;
long l = i;       // OK
float f = l;      // OK, ale może utracić precyzję dla dużych liczb
double d = f;     // OK
```

### Konwersje jawne (Rzutowanie / Explicit)

Wymagają świadomej decyzji programisty i użycia operatora rzutowania `(typ)`. Stosuje się je, gdy istnieje ryzyko utraty informacji.

Konwersja z `double` lub `float` na typ całkowitoliczbowy powoduje **obcięcie** części ułamkowej.

Dla typów całkowitoliczbowych jeżeli operacja powoduje **przepełnienie** (wartość przekracza zakres typu docelowego), to domyślnym zachowaniem jest wykonanie operacji tak jakby była ona wykonana na typie większym i obcięcie bitów znaczących.

```csharp
double d = 99.9;
int i = (int)d; // i = 99 (część ułamkowa obcięta)

long l = 3_000_000_000L;
int i2 = (int)l; // i2 = -1294967296 (przepełnienie)
```

Do kontroli przepełnienia służą konteksty `checked` i `unchecked`. W kontekście `checked` przepełnienie traktowane jest jako błąd.

```csharp
// W bloku checked w przypadku przepełnienia rzucany jest wyjątek System.OverflowException
try
{
    checked
    {
        int i3 = (int)l;
    }
}
catch (OverflowException ex)
{
    Console.WriteLine(ex.Message);
}
```

## Typ `decimal`

Typu `decimal` należy używać do operacji finansowych i monetarnych, gdzie błędy zaokrągleń są niedopuszczalne.

- `decimal` to **typ zmiennoprzecinkowy o podstawie 10**. W przeciwieństwie do `float` i `double` (podstawa 2), `decimal` dokładnie reprezentuje ułamki dziesiętne (np. 0.1, 0.2).
- W pamięci jest przechowywany jako 96-bitowa mantysa (liczby całkowitej), bit znaku i 31 bitów wykładnika (potęgi 10, określającej pozycję przecinka).

Konwersje między `decimal` a `float`/`double` zawsze muszą być jawne.

Mimo swojej precyzji, `decimal` nie jest uniwersalnym rozwiązaniem i ma istotne wady w porównaniu do `float` i `double`:

-   **Wydajność**: Operacje na `decimal` są znacznie wolniejsze. Arytmetyka dla `float` i `double` jest wykonywana bezpośrednio przez procesor (w jednostce FPU), podczas gdy operacje na `decimal` są najczęściej realizowane programowo, co wiąże się z większym narzutem.
-   **Mniejszy zakres**: `decimal` ma znacznie mniejszy zakres wartości niż `double`. Nie nadaje się do obliczeń naukowych, gdzie operuje się na bardzo dużych lub bardzo małych liczbach.
-   **Zużycie pamięci**: Zajmuje 16 bajtów, czyli dwa razy więcej niż `double` (8 bajtów) i cztery razy więcej niż `float` (4 bajty).

## Typ `bool`

Typ `bool` (alias typu `System.Boolean`) reprezentuje wartości `true` i `false`. W pamięci zajmuje 1 bajt.

```csharp
int x = 1;
bool flag = x > 0;
if (flag)
{
    // ...
    flag = false;
}
```

W odróżnieniu od C++ nie istnieją konwersje między `bool` a typami liczbowymi.

### Operatory porównania

Dla typów bezpośrednich operacja porównania **domyślnie** sprawdza czy obiekty są identyczne pole po polu.

```csharp
Point p1 = new Point {X = 5, Y = 3};
Point p2 = p1; p2.X = 0;
Point p3 = new Point {X = -1, Y = 1};
Point p4 = new Point {X = -1, Y = 1};
Console.WriteLine(p1 == p2); // false
Console.WriteLine(p3 == p4); // true

public struct Point { public float X, Y; }
```

Dla typów referencyjnych operacja porównania **domyślnie** sprawdza czy referencje wskazują na ten sam obiekt.

```csharp
Person p1 = new Person {Name = "Alice", Age = 30};
Person p2 = new Person {Name = "Alice", Age = 30};
Person p3 = new Person {Name = "Bob", Age = 25};
Person p4 = p3; p4.Age = 26;
Console.WriteLine(p1 == p2); // false
Console.WriteLine(p3 == p4); // true

public class Person { public string Name; public int Age; }
```

> Operatory porównania mogą być nadpisane, tak, żeby zwracały dowolny typ. W praktyce nie ma to zbyt wiele sensu.

### Operacje skrócone (*Short-Circuiting*)

Operatory logiczne `&&` i `||` wykonują tzw. operacje skrócone (*Short-Circuiting*). 
Na przykład, jeżeli lewa strona operatora `&&` zewaluuje się do `false`, to jego prawa strona nie będzie ewaluowana.
Dla odróżnienia operatory `&` i `|` powodują ewaluację obu stron operatora zawsze.

```csharp
if (user != null && user.HasPermission("admin"))
{
    Console.WriteLine("Użytkownik ma uprawnienia admina.");
}
```

W tym przykładzie dzięki temu mechanizmowi unikamy `NullReferenceException`. Jeżeli użytkownik jest `null`em, to nie wywoła się na nim metoda.

## Typ `char`

Typ `char` (alias typu `System.Char`) w .NET ma stały rozmiar 2 bajtów - to znak kodowany w UTF-16.

Typ `char` można niejawnie konwertować na inne typy liczbowe jeżeli ten typ pomieści w sobie `ushort`. W przeciwnym wypadku wymagana jest jawna konwersja.

```csharp
char a = 'A';
char newLine = '\n';
char copyright = '\u00A9';
```

## Klasa `Convert`

Klasa Convert zawiera mnóstwo przydatnych metod do konwersji między typami. Daje możliwość konwersji między typami podstawowymi i stringami, czy konwersje między typem `bool` i liczbowymi. Konwersje z użyciem tej klasy zaokrąglają liczby zmiennoprzecinkowe przy konwersji do liczb całkowitoliczbowych.

```csharp
int number = Convert.ToInt32("42");
int rounded = Convert.ToInt32(3.5);
bool isTrue = Convert.ToBoolean(1);
int binaryInt = Convert.ToInt32("101010", 2);
string hex = Convert.ToString(255, 16);
```
