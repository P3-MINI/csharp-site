---
title: "Typ Wyliczeniowy"
weight: 60
---

# Typ Wyliczeniowy (*Enums*)

Typ wyliczeniowy w C# jest podobny do tego z C++. Jest reprezentowany w pamięci jako typ całkowitoliczbowy i grupuje kilka stałych. W C# jest to typ bezpośredni. Domyślnie typem pod spodem jest `int`.

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

Typ pod spodem może być dowolny typ całkowitoliczbowy, który 'pomieści' wszystkie stałe. Stałe bez jawnie przypisanej wartości są kolejno o 1 większe od poprzedniej.

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
else if (side == BorderSide.Botton) {}
else
{
    throw new ArgumentException($"Enum value out of bonds: {side}")
}
```

## Flagi

Normalnie wartość wyliczenia może przyjąć jedną z wartości. Dodanie atrybutu `Flags` do definicji typu wyliczeniowego powoduje, że pojedyncza wartość wyliczenia może reprezentować kombinację kilku wartości. Żeby to zadziałało poprawnie kolejnym stałym musimy przypisać kolejne potęgi dwójki:

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

Bez atrybutu `Flags`, również można wyliczenia używać w podobny sposób. Atrybut ten powoduje, że metoda `ToString` poprawnie formatuje łączone wartości wyliczenia.

Żeby sprawdzić, czy wartość wyliczenia zawiera którąś z opcji używamy operacji bitowych:

```csharp
BorderSides allButTop = BorderSides.All ^ BorderSides.Top;
Console.WriteLine((allButTop & BorderSides.Left) != 0);
Console.WriteLine((allButTop & BorderSides.Right) != 0);
Console.WriteLine((allButTop & BorderSides.Top) != 0);
Console.WriteLine((allButTop & BorderSides.Bottom) != 0);

Console.WriteLine(allButTop);
```
