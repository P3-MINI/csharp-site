---
title: "Nullowalność"
---

# Nullowalność

## Nullowalne typy bezpośrednie

Typy bezpośrednie w C# nie mogą przyjmować wartości `null`. 

```csharp
int i = null; // Compilation error
```

Język dostarcza jednak narzędzie, które sprawia, że będą mogły one przyjmować wartość `null`, a przynajmniej będzie to tak wyglądać.

```csharp
int? i = null; // OK
```

Typ `int?` rozwija się do typu `Nullable<int>`. Jest to generyczna struktura, która opakowuje inny typ bezpośredni wraz z flagą informującą czy typ posiada wartość. Definicja tej struktury wygląda mniej więcej następująco:

```csharp
public struct Nullable<T> where T : struct
{
    public T Value { get; }
    public bool HasValue { get; }
    public T GetValueOrDefault();
    public T GetValueOrDefault(T defaultValue);
    //...
}
```

Składnia związana z tym typem jest tylko cukierkiem składniowym. Kompilator zamienia przypisania wartości `null` i porównania na odpowiednie konstrukcje ze struktury `Nullable`:

```csharp
int? i = null;
Console.WriteLine(i == null);

// Equivalent:
// Nullable<int> i = new Nullable<int>();
// Console.WriteLine(!i.HasValue);
```

> Pobranie wartości przez właściwość `Value` rzuca wyjątkiem InvalidOperationException jeżeli właściwość `HasValue` ma wartość `false`.

### Konwersje

Można niejawnie przypisać wartość nienullowalną do zmiennej typu nullowalnego. W drugą stronę wymaga to jawnego rzutowania i może spowodować rzucenie wyjątkiem jeżeli `HasValue` ma wartość `false`.

```csharp
int? i = 5;     // implicit conversion
int j = (int)i; // explicit conversion

// Equivalent:
// int j = i.Value;
```

### Zapożyczanie operatorów

Struktura `Nullable` nie definiuje operatorów, można na niej jednak operatorów używać tak jak na jej parametrze generycznym.

```csharp
int? x = 5;
int? y = 10;
int? z = x + y;
bool b = x < y;
```

Kompilator zapożycza operator w następujący sposób - w zależności czy jest to operator porównania, czy inny operator:

```csharp
int? z = (x.HasValue && y.HasValue) ? (x.Value + y.Value) : null;
bool b = (x.HasValue && y.HasValue) ? (x.Value < y.Value) : false;
```

#### Logika trójwartościowa

Dzięki zapożyczeniu operatorów typ `bool?` wspiera logikę trójwartościową (z dodatkową wartością `null` reprezentującą `nie wiem`)

```csharp
bool? n = null;
bool? f = false;
bool? t = true;

Console.WriteLine(n | n); // null
Console.WriteLine(n | f); // null
Console.WriteLine(n | t); // true
Console.WriteLine(n & n); // null
Console.WriteLine(n & f); // false
Console.WriteLine(n & t); // null
```

## Nullowalne typy referencyjne

Typy referencyjne naturalnie wspierają wartość `null`. Nullowalne typy referencyjne oznaczają co innego. Jest to funkcja języka (od wersji C# 8.0), która poprzez statyczną analizę kodu pomaga uniknąć wyjątków `NullReferenceException`. W tym wypadku specjalne znaczenie mają typy referencyjne nieoznaczone symbolem `?`. Takie zmienne czy pola kompilator będzie sprawdzał czy zawsze mają przypisaną wartość. Jeżeli kompilator wykryje że taka zmienna może przechowywać wartość `null`, to rzuci ostrzeżenie podczas kompilacji.

```csharp
string str = null; // Warning: Converting null literal or possible null value into non-nullable type
```

Jeżeli naszym zamierzeniem jest przechowywanie wartości null, to typ powinniśmy oznaczyć jako nullowalny:

```csharp
string? str = null; // OK
```

Dla zmiennych nullowalnych kompilator dodatkowo pilnuje żebyśmy sprawdzili czy zmienna nie przechowuje wartości `null` przed użyciem:

```csharp
public static void PrintMessageLength(string? message)
{
    Console.WriteLine(message.Length); // Warning: Dereference of a possibly null reference

    if (message != null)
    {
        Console.WriteLine(message.Length); // OK
    }
}
```

Możemy też pominąć sprawdzenie wartości `null`, jeżeli parametr metody zakłada że jest typem nienullowalnym. Przekazanie potencjalnego `null`a również powoduje wtedy ostrzeżenie:

```csharp
string? str = null;
PrintMessageLength(str); // Warning: Possible null reference argument for parameter 'message' in 'Program.PrintMessageLength'

public static void PrintMessageLength(string message)
{
    Console.WriteLine(message.Length);
}
```

> Nie ma żadnej różnicy w czasie wykonania programu między nullowalnym a nienullowalnym typem referencyjnym. Różnica istnieje tylko podczas kompilacji na potrzeby analizy statycznej.

### Kontekst nullable

Możemy wyłączyć analizę statyczną pod względem nullowalności na poziomie projektu, w pliku projektu ustawiając odpowiednio właściwość `Nullable`:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

Można też wyłączyć/włączyć analizę dla fragmentu kodu używając dyrektyw preprocesora:

```csharp
#nullable enable  // enables nullable reference checks from this point on
#nullable disable // disables nullable reference checks from this point on
#nullable restore // resets nullable reference checks to project setting
```

### Null forgiving operator

Można też wyciszyć ostrzeżenia kompilatora operatorem `!`:

```csharp
string s1 = null!;         // `!` Silences the warning
string? s2 = null;
int s2Length = s2!.Length; // `!` Silences the warning
```

## Operatory związane z nullowalnością

C# dostarcza kilka operatorów do pracy z typami nullowalnymi.

### Null-coalescing operator

Null-coalescing operator sprawdza czy lewa strona jest `null`. Jeżeli jest, zwraca wartość z prawej strony; w przeciwnym wypadku zwraca wartość z lewej strony.

```csharp
string s1 = null;
string s2 = s1 ?? "non-null";
Console.WriteLine($"Value of s2: {s2}");
```

Równoważne jest to z zapisem:

```csharp
string s2 = (s1 == null) ? "non-null" : s1;
```

Po prawej stronie operatora można też rzucić wyjątkiem:

```csharp
string s2 = s1 ?? throw new ArgumentNullException();
```

### Null-coalescing assignment operator

To samo ale w wersji z przypisaniem:

```csharp
string? s = null;
s ??= "non-null";
Console.WriteLine($"Value of s: {s}");
```

Równoważnie:

```csharp
s = (s == null) ? "non-null" : s;
```

### Null-conditional operator

Null-conditional operator pozwala na bezpieczny dostęp do składowych lub elementów obiektu, zwracając null zamiast powodować błąd `NullReferenceException`, jeśli obiekt okazałby się `null`em.

```csharp
StringBuilder? sb = null;
string? s = sb?.ToString();
```

Równoważnie:

```csharp
string? s = (sb == null ? null : sb.ToString());
```
