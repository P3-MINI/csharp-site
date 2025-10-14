---
title: "Typ Wyliczeniowy"
weight: 60
---

# Typ Wyliczeniowy (*Enums*)

Typ wyliczeniowy w C# jest podobny do tego z C++. Jest reprezentowany w pamięci jako typ całkowitoliczbowy i grupuje kilka stałych. W C# jest to typ bezpośredni. Domyślnie typem bazowym jest `int`.

Najprostsza definicja wyliczenia wygląda tak:

```csharp
public enum BorderSide 
{
    Left, 
    Right, 
    Top, 
    Bottom
}
```

W tym przypadku do elementów wyliczenia zostaną przypisane stałe typu `int`: 0, 1, 2, 3.

Możemy zmienić zarówno typ pod spodem wyliczenia, jak i przypisane stałe:

```csharp
public enum BorderSide : byte
{
    Left = 1, 
    Right, 
    Top = 65, 
    Bottom
}
```

Typem bazowym może być dowolny typ całkowitoliczbowy, który 'pomieści' wszystkie stałe. Stałe bez jawnie przypisanej wartości są kolejno o 1 większe od poprzedniej.

## Konwersja między typami liczbowymi

Typ wyliczeniowy możemy jawnie zrzutować z i do odpowiadającego mu typu liczbowego. Wartość zero można niejawnie do typu wyliczeniowego przypisać:

```csharp
int i = (int) BorderSide.Top; // requires explicit cast
BorderSide side = (BorderSide) i; // requires explicit cast
BorderSide side = 0; // might assign 0 implicitly
```

## Operacje na typie wyliczeniowym

Typ wyliczeniowy wspiera operacje za pomocą operatorów bitowych (`~`, `^`, `&`, `|`), arytmetycznych (`+`, `-`, `++`, `--`, `+=`, `-=`) i porównania (`!=`, `==`, `<=`, `>=`, `<`, `>`).

Ze względu na konwersję i operacje operatorami, typ wyliczeniowy może wyjść poza swój dozwolony zakres. Dlatego gdy sprawdzamy możliwe wartości powinniśmy spodziewać się niepoprawnej wartości.

```csharp
BorderSide side = BorderSide.Bottom;
side++;

if (side == BorderSide.Right) {}
else if (side == BorderSide.Left) {}
else if (side == BorderSide.Top) {}
else if (side == BorderSide.Bottom) {}
else
{
    throw new ArgumentException($"Enum value out of bounds: {side}")
}
```

## Flagi

Zazwyczaj wartość wyliczenia może przyjąć jedną z wartości. Możemy też traktować wyliczenie w ten sposób, że pojedyncza wartość wyliczenia może reprezentować kombinację kilku wartości. Aby to działało poprawnie, kolejnym stałym musimy przypisać kolejne potęgi dwójki:

```csharp
[Flags]
public enum BorderSides
{
    None = 0, 
    Left = 1, 
    Right = 1 << 1, 
    Top = 1 << 2, 
    Bottom = 1 << 3,
    LeftRight = Left | Right,
    TopBottom = Top | Bottom,
    All = LeftRight | TopBottom
}
```

Takie wyliczenia zwyczajowo oznaczamy atrybutem `Flags`, przede wszystkim aby zaznaczyć intencję użycia wyliczenia. Atrybut ten powoduje także, że metoda `ToString` poprawnie formatuje łączone wartości wyliczenia.

Aby sprawdzić, czy wartość wyliczenia zawiera którąś z opcji, używamy operacji bitowych:

```csharp
BorderSides allButTop = BorderSides.All ^ BorderSides.Top;
Console.WriteLine((allButTop & BorderSides.Left) != 0);
Console.WriteLine((allButTop & BorderSides.Right) != 0);
Console.WriteLine((allButTop & BorderSides.Top) != 0);
Console.WriteLine((allButTop & BorderSides.Bottom) != 0);

Console.WriteLine(allButTop);
```
