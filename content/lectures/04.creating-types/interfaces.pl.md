---
title: "Interfejsy"
weight: 50
---

# Interfejsy

Interfejsy są podobne do klas abstrakcyjnych, ale z kluczową różnicą: **definiują kontrakt zachowań i nie mogą definiować stanu (pól instancji)**. Analogiczną strukturą z C++ byłyby klasy ze wszystkimi składowymi będącymi czysto wirtualnymi metodami.

- Składowymi interfejsu mogą być metody, właściwości, zdarzenia i indeksery. Od C# 8 mogą również zawierać składowe statyczne (w tym stałe) oraz domyślne implementacje składowych.
- Interfejsy **nie mogą zawierać pól instancji**.
- Składowe są niejawnie publiczne, a składowe bez implementacji są niejawnie abstrakcyjne.
- Klasy i struktury wspierają implementowanie wielu interfejsów.

Definicja interfejsu wygląda następująco, tutaj przykład interfejsu `IEnumerator` z przestrzeni nazw `System.Collections`:

```csharp
public interface IEnumerator
{
    bool MoveNext();
    object Current { get; }
    void Reset();
}
```

Zwyczajowo, nazwy interfejsów poprzedzamy literą `I`. Implementowanie interfejsu polega na podaniu implementacji wszystkich jego składowych, które nie mają domyślnej implementacji.

```csharp
public class Counter : IEnumerator
{
    public int Count { get; private set; }
    public Counter(int count) => Count = count;
    public bool MoveNext() => Count-- > 0;
    public object Current => Count;
    public void Reset()
    {
        throw new NotSupportedException();
    }
}
```

Referencje są polimorficzne ze wszystkimi interfejsami, które implementuje dany typ.

```csharp
IEnumerator e = new Counter(10);
while (e.MoveNext())
    Console.Write(e.Current);
Console.WriteLine();
```

## Rozszerzanie interfejsów

Interfejsy mogą rozszerzać inne interfejsy, dziedzicząc wszystkie ich składowe.

```csharp
public interface IUndoable
{
    void Undo();
}

public interface IRedoable : IUndoable
{
    void Redo();
}
```

## Jawna implementacja składowych

Składowe interfejsów możemy jawnie implementować, podając, z którego interfejsu ta składowa pochodzi. Jest to przydatne, gdy nazwy składowych różnych interfejsów ze sobą kolidują.

```csharp
public interface IFoo1 { void Bar(); }
public interface IFoo2 { void Bar(); }

public class Foo : IFoo1, IFoo2
{
    public void Bar()
    {
        Console.WriteLine("Implementation of IFoo1.Bar");
    }

    void IFoo2.Bar()
    {
        Console.WriteLine("Implementation of IFoo2.Bar");
    }
}
```

Do jawnie zaimplementowanych składowych możemy odwołać się **tylko** po rzutowaniu typu do tego konkretnego interfejsu.

```csharp
Foo foo = new Foo();
foo.Bar();          // Implementation of IFoo1.Bar
((IFoo1)foo).Bar(); // Implementation of IFoo1.Bar
((IFoo2)foo).Bar(); // Implementation of IFoo2.Bar
```

## Pakowanie (*Boxing*)

Wywoływanie składowych z interfejsu na strukturze nie powoduje operacji pakowania. Natomiast rzutowanie struktury do interfejsu powoduje jej spakowanie (*boxing*), czyli skopiowanie na stertę.

```csharp
public interface IShape
{
    float Area();
}

public struct Rectangle : IShape
{
    public float Width { get; set; }
    public float Height { get; set; }
    
    public Rectangle(float width, float height)
    {
        Width = width;
        Height = height;
    }
    
    public float Area() => Width * Height;
}
```

```csharp
Rectangle rectangle = new Rectangle(4.0f, 5.0f);
float area = rectangle.Area(); // no boxing
IShape shape = rectangle;      // boxing
```

## Domyślne implementacje (C# 8)

Od C# 8 interfejsy mogą dostarczać domyślne implementacje składowych. Wtedy w implementujących typach dostarczenie własnej implementacji staje się opcjonalne.

```csharp
public interface ILogger
{
    void Message(string message)
    {
        Console.WriteLine(message);
    }
}

public class Logger : ILogger
{
    public void Message(string message) // optional
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {message}");
    }
}
```

Jeżeli typ nie dostarcza własnej implementacji, domyślną implementację można wywołać jedynie przez referencję typu interfejsu.

## Statyczne składowe interfejsów (C# 8)

Ponieważ składowe statyczne nie są częścią stanu instancji, interfejsy mogą je definiować.

```csharp
public interface ILogger
{
    public const string DefaultFile = "default.log";
    public static string LogPrefix { get; set; } = "Log: ";

    void Message(string message)
    {
        Console.WriteLine($"{LogPrefix} {message}");
    }
}
```

### Statyczne składowe abstrakcyjne i wirtualne (C# 11)

Normalnie, tak jak w C++, składowe statyczne nie mogą być wirtualne. Od C# 11 statyczne składowe interfejsów możemy oznaczać słówkami `abstract` i `virtual`. Każdy implementujący typ musi wtedy dostarczyć własną implementację składowej `static abstract`, a dla składowej `static virtual` może to zrobić (w przeciwnym razie używana jest domyślna implementacja z interfejsu).

```csharp
public interface IDescriptable
{
    static abstract string Description { get; }
    static virtual string Category => "General";
}

public class Cat : IDescriptable
{
    public static string Description => "It's a cat";
    public static string Category => "Mammal"; // optional
}

public class Book : IDescriptable
{
    public static string Description => "It's a book";
}
```

Do takich składowych nie można odwołać się przez nazwę interfejsu (`IDescriptable.Description` się nie skompiluje), a jedynie przez parametr generyczny ograniczony do tego interfejsu. Na tym mechanizmie opiera się generyczna matematyka (*generic math*), np. ograniczenie `IComparisonOperators<T, T, bool>` użyte na stronie [Generyki]({{< ref "/lectures/04.creating-types/generics" >}}).

```csharp
PrintDescription<Cat>();  // It's a cat (Mammal)
PrintDescription<Book>(); // It's a book (General)

static void PrintDescription<T>() where T : IDescriptable
{
    Console.WriteLine($"{T.Description} ({T.Category})");
}
```
