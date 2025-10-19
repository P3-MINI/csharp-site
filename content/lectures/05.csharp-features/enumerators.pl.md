---
title: "Enumeratory"
---

# Enumeratory

Enumerator to podobny koncept do iteratorów z C++. Enumerator to kursor działający w trybie "tylko do odczytu", który pozwala na jednokierunkowe przeglądanie sekwencji wartości.

Żeby obiekt można było nazwać enumeratorem to musi posiadać publiczną bezparametrową metodę `bool MoveNext()` i właściwość `Current`. Najczęściej implementuje się interfejs `System.Collections.Generics.IEnumerator<T>` (lub rzadziej `System.Collections.IEnumerator`), który dostarcza wspomnianą metodę i właściwość.

Definicja tych interfejsów wygląda następująco:

```csharp
public interface IEnumerator
{
    bool MoveNext();
    object Current { get; }
    void Reset();
}

public interface IEnumerator<out T> : IDisposable, IEnumerator
{
    T Current { get; }
}
```

Często metody `Reset` się nie implementuje, to jest rzuca ona wyjątek `NotSupportedException`.

## Typ enumerowalny

Typ enumerowalny to z kolei obiekt, który jest w stanie stworzyć nowy enumerator. Musi mieć publiczną metodę `GetEnumerator()`, która zwraca enumerator (może to być metoda rozszerzająca). I tu znów najczęściej implementuje się systemowy interfejs `System.Collections.Generic.IEnumerable<T>` (lub rzadziej `System.Collections.IEnumerable`).

```csharp
public interface IEnumerable
{
    IEnumerator GetEnumerator();
}

public interface IEnumerable<out T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}
```

## Pętla `foreach`

Wszystkie systemowe kolekcje, tablice i stringi są enumerowalne. Wszystko co jest enumerowalne może być użyte wewnątrz pętli `foreach`:

```csharp
List<int> collection = [1, 2, 3, 4, 5, 6, 7, 8];

foreach(int item in collection)
{
    Console.WriteLine(item);
}
```

Pętla `foreach` jest cukierkiem składniowym, kompilator przekształca to wyrażenie następująco:

```csharp
IEnumerator<int> enumerator = collection.GetEnumerator();
try
{
    while (enumerator.MoveNext())
    {
        var item = enumerator.Current;
        Console.WriteLine(item);
    }
}
finally
{
    enumerator?.Dispose();
}
```

## Metoda iterująca

**Korutyna** to specjalny rodzaj funkcji, która **ma zdolność do wstrzymania swojego działania** w dowolnym momencie i **zwrócenia kontroli** do kodu, który ją wywołał, a następnie, po jakimś czasie, **wznowienia działania** dokładnie od miejsca, w którym została wstrzymana.

Metody iterujące to jeden z dwóch przykładów korutyn w C#. Taka metoda musi zwracać `IEnumerable<T>` lub `IEnumerator<T>` (lub `IEnumerable` lub `IEnumerator`) i używać instrukcji `yield`. Taka metoda jest przez kompilator traktowana w szczególny sposób. Kompilator w jej miejsce tworzy maszynę stanów, która zapamiętuje stan iteracji, czyli lokalne zmienne oraz informację o miejscu wstrzymania.

```csharp
public static IEnumerable<int> Fibonacci(int count)
{
    for (int i = 0, prev = 1, curr = 1; i < count; i++)
    {
        yield return prev;
        (prev, curr) = (curr, prev + curr);
    }
}
```

{{% details title="Taka maszyna stanów wygląda następująco dla tej metody:" open=false %}}
```csharp
[CompilerGenerated]
private sealed class <Fibs>d__1 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
{
    private int <>1__state;
    private int <>2__current;
    private int <>l__initialThreadId;
    private int count;
    public int <>3__count;
    private int <i>5__1;
    private int <prev>5__2;
    private int <curr>5__3;

    int IEnumerator<int>.Current
    {
        [DebuggerHidden]
        get
        {
            return <>2__current;
        }
    }

    object IEnumerator.Current
    {
        [DebuggerHidden]
        [return: Nullable(0)]
        get
        {
            return <>2__current;
        }
    }

    [DebuggerHidden]
    public <Fibs>d__1(int <>1__state)
    {
        this.<>1__state = <>1__state;
        <>l__initialThreadId = Environment.CurrentManagedThreadId;
    }

    [DebuggerHidden]
    void IDisposable.Dispose()
    {
    }

    private bool MoveNext()
    {
        int num = <>1__state;
        if (num != 0)
        {
            if (num != 1)
            {
                return false;
            }
            <>1__state = -1;
            int num2 = <curr>5__3;
            int num3 = (<curr>5__3 = <prev>5__2 + <curr>5__3);
            <prev>5__2 = num2;
            <i>5__1++;
        }
        else
        {
            <>1__state = -1;
            <i>5__1 = 0;
            <prev>5__2 = 1;
            <curr>5__3 = 1;
        }
        if (<i>5__1 < count)
        {
            <>2__current = <prev>5__2;
            <>1__state = 1;
            return true;
        }
        return false;
    }

    bool IEnumerator.MoveNext()
    {
        //ILSpy generated this explicit interface implementation from .override directive in MoveNext
        return this.MoveNext();
    }

    [DebuggerHidden]
    void IEnumerator.Reset()
    {
        throw new NotSupportedException();
    }

    [DebuggerHidden]
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
    {
        <Fibs>d__1 <Fibs>d__;
        if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
        {
            <>1__state = 0;
            <Fibs>d__ = this;
        }
        else
        {
            <Fibs>d__ = new <Fibs>d__1(0);
        }
        <Fibs>d__.count = <>3__count;
        return <Fibs>d__;
    }

    [DebuggerHidden]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<int>)this).GetEnumerator();
    }
}
```
{{% /details %}}

Metoda iterująca jest wywoływana po wywołaniu `MoveNext` na enumeratorze od ostatniego stanu. Obliczana jest następna wartość, instrukcja `yield return` zwraca obliczoną wartość przez zapis jej do właściwości `Current` i kontrola wraca do wywołującego.

Istnieje także instrukcja `yield break`, która również zwraca kontrolę do wywołującego, ale sprawia że `MoveNext` zwraca `false`, oznaczając brak dalszych elementów.

```csharp
public static IEnumerable<int> Fibonacci(int count)
{
    for (int i = 0, prev = 1, curr = 1; i < count; i++)
    {
        if (i > 45) yield break;
        yield return prev;
        (prev, curr) = (curr, prev + curr);
    }
}
```

## Kompozycja metod iterujących

Metody iterujące można ze sobą składać. Załóżmy, że mamy dwie metody:

```csharp
public static IEnumerable<int> Fibonacci(int n)
{
    int current = 0, next = 1;
    for (int i = 0; i < n; i++)
    {
        yield return current;
        (current, next) = (next, current + next);
    }
}

public static IEnumerable<int> Odds(IEnumerable<int> sequence)
{
    foreach (var item in sequence)
    {
        if (item % 2 == 1) yield return item;
    }
}
```

Można je ze sobą składać w następujący sposób:

```csharp
var sequence = Odds(Fibonacci(10));
Console.WriteLine("Odd elements of the fibonacci sequence:");
foreach (var item in sequence)
{
    Console.WriteLine(item);
}
```

Przepływ kontroli przedstawia diagram sekwencji:

```mermaid
sequenceDiagram
    participant Main
    participant Odds
    participant Fibonacci
    activate Main
    Main->>+Odds: MoveNext()
    deactivate Main
    activate Odds
    Odds->>Fibonacci: MoveNext()
    deactivate Odds
    activate Fibonacci
    Fibonacci->>Odds: 1
    deactivate Fibonacci
    activate Odds
    Odds->>Main: 1
    deactivate Odds
    activate Main
    Main->>Odds: MoveNext()
    deactivate Main
    activate Odds
    Odds->>Fibonacci: MoveNext()
    deactivate Odds
    activate Fibonacci
    Fibonacci->>Odds: 1
    deactivate Fibonacci
    activate Odds
    Odds->>Main: 1
    deactivate Odds
    activate Main
    Main->>Odds: MoveNext()
    deactivate Main
    activate Odds
    Odds->>Fibonacci: MoveNext()
    deactivate Odds
    activate Fibonacci
    Fibonacci->>Odds: 2
    deactivate Fibonacci
    activate Odds
    Odds->>Fibonacci: MoveNext()
    deactivate Odds
    activate Fibonacci
    Fibonacci->>Odds: 3
    deactivate Fibonacci
    activate Odds
    Odds->>Main: 3
    deactivate Odds
    activate Main
    Main->>Odds: ...
    deactivate Main
```

Metoda `Odds` jest filtrem dla sekwencji `Fibonacci`, na tej zasadzie opieraja się większość metod z LINQ.

> [!NOTE]
> **Kod źródłowy - IteratorMethods**
> {{< filetree dir="lectures/csharp-features/IteratorMethods" >}}

## Implementacja własnego typu enumerowalnego

Żeby nasz typ własny typ był enumerowalny, najprościej jest zaimplementować interfejs `IEnumerable`. Najczęściej metodę `GetEnumerator` implementuje się jako iterującą z wykorzystaniem instrukcji `yield`.

```csharp
public class Stack<T> : IEnumerable<T>
{
    private T[] _items = new T[8];
    public int Count { get; private set; }

    public void Push(T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }

    public IEnumerator<T> GetEnumerator()
    {
        int count = Count;
        while (count-- > 0)
        {
            yield return _items[count];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
```

Taki enumerowalny stos z powodzeniem można użyć w instrukcji `foreach`:

```csharp
Stack<string> stack = new Stack<string>();
stack.Push("The");
stack.Push("quick");
stack.Push("brown");
stack.Push("fox");
stack.Push("jumps");
stack.Push("over");
stack.Push("the");
stack.Push("lazy");
stack.Push("dog");

foreach (var str in stack)
{
    Console.WriteLine(str);
}
```

> [!NOTE]
> **Kod źródłowy - EnumerableStack**
> {{< filetree dir="lectures/csharp-features/EnumerableStack" >}}
