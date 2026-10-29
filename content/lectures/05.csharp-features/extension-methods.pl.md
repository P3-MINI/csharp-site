---
title: "Metody rozszerzające"
---

# Metody rozszerzające

Metody rozszerzające pozwalają na dodawanie metod do istniejących typów bez modyfikowania ich kodu. Jest to szczególnie przydatne, jeżeli chcemy rozszerzyć funkcjonalność typu, do którego nie mamy dostępu. Pozwalają one też podzielić i pogrupować funkcjonalności obiektu na kilka klas.

Metody rozszerzające muszą być zdefiniowane w statycznej klasie. Pierwszy parametr określa typ, który jest rozszerzany i musi być poprzedzony słówkiem `this`.

```csharp
public static class StringExtensions
{
    public static int WordCount(this string str)
    {
        return str.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
```

Mimo że metody rozszerzające są zdefiniowane jako metody statyczne, to można je wywoływać tak jakby były metodami instancyjnymi dla rozszerzanego typu.

```csharp
string greeting = "Quick fox jumps over the lazy dog.";
int j = greeting.WordCount();
```

Szczególnie przydatne jest rozszerzanie interfejsów, wtedy takie metody stają się dostępne dla wszystkich typów implementujących dany interfejs. Na tej zasadzie działa LINQ (*Language INtegrated Query*) - definiuje metody rozszerzające dla typu `IEnumerable<T>`, który z kolei implementują wszystkie kolekcje. Dzięki temu na kolekcjach mamy zdefiniowane takie metody jak `Where`, `Select`, `GroupBy`, itp. Jako że te metody w większości również zwracają `IEnumerable<T>`, to można te metody ze sobą łączyć w jednym łańcuchu wywołań (*fluent syntax*).

```csharp
List<int> numbers = [5, 10, 3, 8, 1];
IEnumerable<int> query = numbers
                            .Where(n => n % 2 == 0)
                            .OrderBy(n => n)
                            .Select(n => n * n);
foreach (int n in query)
{
    Console.WriteLine(n);
}
```

## Bloki rozszerzające (C# 14)

C# 14 wprowadza *składowe rozszerzające* (*extension members*), deklarowane w blokach `extension` wewnątrz statycznej klasy. Parametr bloku (poniżej `string s`) to tzw. *odbiorca* (*receiver*): określa rozszerzany typ i jest dostępny we wszystkich składowych instancyjnych bloku. Bloki rozszerzające (*extension blocks*) pozwalają na dodawanie nie tylko metod rozszerzających do typu, ale również właściwości, operatorów i statycznych składowych.

```csharp
using System.Text;

public static class StringExtensions
{
    extension(string s)
    {
        public static string operator *(string str, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            }
            if (count == 0 || string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        public bool IsEmpty => s.Length == 0;

        public int WordCount()
        {
            return s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
```

Użycie:

```csharp
string str = "Quick brown fox jumps over the lazy dog.";
Console.WriteLine("ab" * 3);        // ababab
Console.WriteLine(str.IsEmpty);     // False
Console.WriteLine(str.WordCount()); // 8
```

Operatory są składowymi statycznymi, dlatego `operator *` nie korzysta z odbiorcy `s` – oba operandy są jego parametrami i co najmniej jeden z nich musi być rozszerzanego typu.

> [!NOTE]
> Składowe rozszerzające nie mogą dodać do typu stanu: blok rozszerzający nie może zawierać pól ani automatycznie implementowanych właściwości. Właściwości rozszerzające muszą więc być wyliczane na podstawie odbiorcy, tak jak `IsEmpty` powyżej.

### Statyczne składowe rozszerzające

Statyczne składowe rozszerzające wywołujemy na samym rozszerzanym typie, tak jakby były jego własnymi składowymi statycznymi. Jeżeli blok zawiera wyłącznie składowe statyczne, odbiorca nie musi mieć nazwy:

```csharp
public static class DateTimeExtensions
{
    extension(DateTime)
    {
        public static DateTime Tomorrow => DateTime.Today.AddDays(1);

        public static DateTime FromUnixSeconds(long seconds) => DateTime.UnixEpoch.AddSeconds(seconds);
    }
}
```

```csharp
DateTime deadline = DateTime.Tomorrow;
Console.WriteLine(DateTime.FromUnixSeconds(1_000_000_000).ToString("o")); // 2001-09-09T01:46:40.0000000Z
```

### Generyczne bloki rozszerzające

Blok rozszerzający może być generyczny. Jego parametry typu są wnioskowane na podstawie odbiorcy, tak samo jak w przypadku generycznych metod rozszerzających, więc poniższe składowe są dostępne dla dowolnej sekwencji:

```csharp
public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public bool IsEmpty => !source.Any();

        public IEnumerable<T> WhereNot(Func<T, bool> predicate) => source.Where(x => !predicate(x));
    }
}
```

```csharp
List<int> numbers = [5, 10, 3, 8, 1];
Console.WriteLine(numbers.IsEmpty);                                 // False
Console.WriteLine(string.Join(", ", numbers.WhereNot(n => n > 4))); // 3, 1
```

### Odbiorca przekazywany przez referencję

Jeżeli rozszerzany typ jest typem wartościowym, odbiorcę można przekazać przez referencję (`ref`, `in` lub `ref readonly`), tak samo jak pierwszy parametr klasycznej metody rozszerzającej `this ref`. Odbiorca `ref` pozwala modyfikować oryginalną zmienną zamiast jej kopii:

```csharp
public static class CounterExtensions
{
    extension(ref int number)
    {
        public void Increment() => number++;
    }
}
```

```csharp
int counter = 5;
counter.Increment();
Console.WriteLine(counter); // 6
```

### Zgodność i reguły wyszukiwania

Metoda rozszerzająca zadeklarowana w bloku kompiluje się do takiego samego kodu IL jak klasyczna metoda z `this`. Wywołujący nie są w stanie ich odróżnić, a metodę wciąż można wywołać jak zwykłą metodę statyczną, np. `StringExtensions.WordCount(str)`. Istniejące metody rozszerzające można więc przepisać na nową składnię bez utraty zgodności na poziomie kodu źródłowego i binarnym, a oba style można mieszać w jednej statycznej klasie.

Składowe rozszerzające obu rodzajów są brane pod uwagę tylko wtedy, gdy sam rozszerzany typ nie ma pasującej składowej o danej nazwie – **składowe instancyjne zawsze mają pierwszeństwo**. Na przykład właściwość rozszerzająca `Length` zadeklarowana dla `string` nigdy nie zostanie użyta, ponieważ `string` ma już własną właściwość `Length` (kompilator nawet o tym nie ostrzega). Ponadto, tak jak w przypadku klasycznych metod rozszerzających, składowe rozszerzające są widoczne tylko wtedy, gdy przestrzeń nazw ich statycznej klasy jest w zasięgu (np. zaimportowana dyrektywą `using`).

## Ciekawostki

Możemy rozszerzać typy nieiterowalne o metodę zwracającą iterator.

```csharp
public static class IntExtensions
{
    public static IEnumerator<int> GetEnumerator(this int i)
    {
        while (i-- > 0)
        {
            yield return i;
        }
    }
}
```

Dzięki temu będzie można ich użyć w pętli `foreach`.

```csharp
foreach (int i in 10)
{
    Console.WriteLine(i);
}
```

Tak samo możemy do typu dodać możliwość użycia go z operatorem `await`: [await anything](https://devblogs.microsoft.com/dotnet/await-anything/).
