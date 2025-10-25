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

Bloki rozszerzające (*extension blocks*) pozwalają na dodawanie nie tylko metod rozszerzających do typu, ale również właściwości, operatorów i statycznych składowych.

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
Console.WriteLine(str * 3);
Console.WriteLine(str.IsEmpty);
Console.WriteLine(str.WordCount());
```

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
