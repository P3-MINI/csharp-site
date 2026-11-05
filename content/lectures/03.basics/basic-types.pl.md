---
title: "Typy podstawowe"
weight: 20
---

# Typy podstawowe

Słowa kluczowe typów wbudowanych (np. `int`, `double`, `bool`) są bezpośrednimi aliasami dla typów zdefiniowanych w przestrzeni nazw `System` (np. `System.Int32`, `System.Double`, `System.Boolean`). Warto pamiętać, że te typy to zwykłe struktury (typy bezpośrednie) w bibliotece standardowej, w których zdefiniowane są przydatne stałe i metody, np. `int.MaxValue`, `int.Parse(string)`, `float.NegativeInfinity`.

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
| **float** | `System.Single` | 32 bity | `±1,5×10⁻⁴⁵` do `±3,4×10³⁸` | ~6-9 cyfr | `F` lub `f` |
| **double** | `System.Double` | 64 bity | `±5,0×10⁻³²⁴` do `±1,7×10³⁰⁸` | ~15-17 cyfr | `D` lub `d` |
| **decimal** | `System.Decimal` | 128 bitów | `±1,0×10⁻²⁸` do `±7,9×10²⁸` | 28-29 cyfr | `M` lub `m` |


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

Dla typów całkowitoliczbowych, jeżeli operacja powoduje **przepełnienie** (wartość przekracza zakres typu docelowego), to domyślnym zachowaniem jest wykonanie operacji tak, jakby była ona wykonana na typie większym, i odrzucenie najbardziej znaczących bitów, które się nie mieszczą.

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
- Ze 128 bitów, 1 bit reprezentuje znak, 96 bitów reprezentuje samą wartość (mantysę), a 8 bitów reprezentuje potęgę liczby 10, przez którą dzielona jest ta 96-bitowa wartość (od 0 do 28). Pozostałe bity są niewykorzystane.

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

W odróżnieniu od C++ nie istnieją konwersje (ani niejawne, ani jawne rzutowania) między `bool` a typami liczbowymi.

### Operatory porównania

Dla typów bezpośrednich operacja porównania **zazwyczaj** sprawdza czy wartości są identyczne.

```csharp
int a = 5;
int b = 5;
int c = 10;
Console.WriteLine(a == b); // true
Console.WriteLine(a == c); // false
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

> Operatory porównania mogą być przeciążone tak, żeby zwracały dowolny typ. W praktyce nie ma to zbyt wiele sensu.

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

Typ `char` (alias typu `System.Char`) w .NET ma stały rozmiar 2 bajtów - reprezentuje pojedynczą jednostkę kodową (*code unit*) UTF-16. Znaki spoza podstawowej płaszczyzny wielojęzycznej (BMP), np. większość emoji, nie mieszczą się w jednym `char` i są reprezentowane przez dwie wartości `char` (tzw. parę zastępczą, *surrogate pair*).

Typ `char` można niejawnie konwertować na inne typy liczbowe jeżeli ten typ pomieści w sobie `ushort`. W przeciwnym wypadku wymagana jest jawna konwersja.

```csharp
char a = 'A';
char newLine = '\n';
char copyright = '\u00A9';
```

### Sekwencje ucieczki i `\e` (C# 13)

Sekwencje ucieczki w litera\u0142ach `char` i `string` s\u0105 w wi\u0119kszo\u015Bci takie same jak w C++ (`\n`, `\t`, `\0`, `\\`, `\'`, `\"`, ...). Dodatkowo `\uXXXX` oznacza jednostk\u0119 kodow\u0105 UTF-16 podan\u0105 za pomoc\u0105 4 cyfr szesnastkowych. Od C# 13 istnieje r\u00F3wnie\u017C `\e` - znak ESC (U+001B), u\u017Cywany np. w sekwencjach ANSI steruj\u0105cych terminalem (kolory, pogrubienie tekstu). Wcze\u015Bniej trzeba by\u0142o pisa\u0107 `\u001b` lub `\x1b`; ta druga forma jest podatna na b\u0142\u0119dy, poniewa\u017C `\x` poch\u0142ania od 1 do 4 cyfr szesnastkowych.

```csharp
char escape = '\e'; // C# 13, the same as '\u001b'
Console.WriteLine("\e[1mBold\e[0m and normal text"); // ANSI escape codes
Console.WriteLine("\x1bCafe" == "\u1BCAfe"); // True - \x consumed 4 hex digits: \x1bCa
```

## Klasa `Convert`

Klasa `Convert` zawiera mnóstwo przydatnych metod do konwersji między typami. Daje możliwość konwersji między typami podstawowymi i stringami czy konwersji między typem `bool` a typami liczbowymi. Konwersje z użyciem tej klasy zaokrąglają liczby zmiennoprzecinkowe do najbliższej liczby całkowitej (w odróżnieniu od rzutowania, które obcina część ułamkową); wartości leżące dokładnie pośrodku są zaokrąglane do najbliższej liczby parzystej (tzw. *zaokrąglanie bankierskie*).

```csharp
int number = Convert.ToInt32("42");             // 42
int rounded = Convert.ToInt32(3.5);             // 4
int roundedToEven = Convert.ToInt32(2.5);       // 2
bool isTrue = Convert.ToBoolean(1);             // true
int binaryInt = Convert.ToInt32("101010", 2);   // 42
string hex = Convert.ToString(255, 16);         // "ff"
```
