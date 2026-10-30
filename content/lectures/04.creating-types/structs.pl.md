---
title: "Struktury"
weight: 30
---

# Struktury

Struktury różnią się od klas pod pewnymi względami:

* są typami bezpośrednimi
* nie wspierają dziedziczenia
* nie mogą zawierać finalizatorów

Poza tym struktury mogą mieć takie same składowe jak klasa.

## Struktura czy klasa

Struktur powinniśmy używać, jeżeli zależy nam na semantyce typów bezpośrednich. Szczególnie korzystne jest to wtedy, gdy **obiekt jest mały** (wytyczne projektowe Microsoftu zalecają poniżej 16 bajtów; do około 64 bajtów - jednej linii pamięci podręcznej - kopiowanie wciąż jest tanie), wtedy wartość będzie się kopiowała w najwyżej paru cyklach procesora. Zazwyczaj struktury bardziej pasują do obiektów o **krótkim cyklu życia**, np. typy matematyczne, które przy używaniu operatorów są często tworzone i szybko usuwane - do tego lepszy jest stos. Jeżeli potrzebujemy **dużej liczby obiektów**, to warto pamiętać, że tablica struktur stworzy nam te obiekty w jednej alokacji w ciągłym bloku pamięci. Dostęp do ciągłej pamięci jest dużo szybszy.

Klasy z kolei powinny być wybierane dla typów, które mają **'tożsamość'**, np. `Customer`, `Order`, `Window`, i nie powinny być przypadkowo kopiowane. Dla **obiektów dużych** (> 64 bajtów) dużo wydajniejsze będzie kopiowanie referencji (praktycznie zawsze w 1 cyklu). Obiekty o **dłuższym cyklu życia** zazwyczaj lepiej przechowywać na stercie, co gwarantują nam klasy. Jeżeli musimy zamodelować hierarchię obiektów, które **wymagają dziedziczenia**, to musimy użyć klas.

## Konstruktor domyślny

Struktury zawsze posiadają bezparametrowy konstruktor domyślny, który zeruje wszystkie pola struktury. Nawet jeżeli zdefiniujemy własny konstruktor bezparametrowy (C# 10), wyzerowaną wartość nadal możemy uzyskać przez `default`. Poza tym tablica struktur domyślnie jest bitowo wyzerowana, a klasy również bitowo zerują swoje pola. Warto się przed tym zabezpieczyć i zawsze traktować wyzerowaną strukturę jako możliwą poprawną wartość.

Jeżeli struktura nie deklaruje konstruktora bezparametrowego, to `new Point()` jest równoważne `default`: inicjalizatory pól i właściwości nie są wykonywane, dlatego w poniższym przykładzie `p2.Y` wynosi `0`, a nie `1`.

```csharp
Point p1 = new Point(1); // 1, 1
Point p2 = new Point();  // 0, 0

public struct Point
{
    public float X { get; set; }
    public float Y { get; set; } = 1;
    
    public Point(float x) => X = x;
}
```

## Struktury `readonly`

Słówko `readonly` przy definicji struktury wymusza, żeby struktura była niezmienna, to jest pola również były tylko do odczytu, a właściwości automatyczne mogły mieć tylko akcesor `get` (lub `init`).

```csharp
public readonly struct Point
{
    public readonly float X;
    public float Y { get; } = 1;
    
    public Point(float x) => X = x;
}
```

Pojedyncze metody również można oznaczać `readonly`, co sprawia, że nie możemy zmieniać pól struktur w takiej metodzie.

## Struktury `ref`

Struktury ze słówkiem kluczowym `ref` mogą być alokowane jedynie na stosie, każda konstrukcja, która wymuszałaby umieszczenie takiej zmiennej na stercie, kończy się błędem kompilacji. Między innymi nie można tworzyć tablic takich struktur, nie mogą być polami klas ani struktur innych niż `ref`, nie mogą być pakowane (konwertowane do `object` lub do typu interfejsu), przechwytywane przez lambdy ani funkcje lokalne, ani używane jako argumenty generyczne (chyba że parametr typu jest zadeklarowany z `allows ref struct`, C# 13). Od C# 13 mogą implementować interfejsy i mogą być zmiennymi lokalnymi w iteratorach i metodach asynchronicznych, o ile ich wartość nie musi przetrwać `yield return` ani `await`.

```csharp
public ref struct Point
{
    public float X { get; set; }
    public float Y { get; set; }
}
```
