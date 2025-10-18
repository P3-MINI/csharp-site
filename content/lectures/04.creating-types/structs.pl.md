---
title: "Struktury"
weight: 30
---

# Struktury

Struktury i klasy są inne pod pewnymi względami:

* są typami bezpośrednimi
* nie wspierają dziedziczenia
* nie mogą zawierać finalizatorów

Poza tym struktury mogą mieć takie same składowe jak klasa.

## Struktura czy klasa

Struktur powinniśmy używać, jeżeli zależy nam na semantyce wartości bezpośrednich. Szczególnie korzystne jest to wtedy, gdy **obiekt jest mały** (<= 64 bajtów), wtedy wartość będzie się kopiowała w najwyżej paru cyklach procesora. Zazwyczaj struktury bardziej pasują do obiektów o **krótkim cyklu życia**, np. typy matematyczne, które przy używaniu operatorów są często tworzone i szybko usuwane - do tego lepszy jest stos. Jeżeli potrzebujemy **dużej ilości obiektów** to warto pamiętać, że tablica struktur stworzy nam te obiekty w jednej alokacji w ciągłym bloku. Dostęp do ciągłej pamięci jest dużo szybszy.

Klasy z kolei powinny być wybierane dla typów które mają **'tożsamość'**, np. `Customer`, `Order`, `Window` i nie zostaną przypadkowo skopiowane. Dla **obiektów dużych** (> 64 bajtów) dużo wydajniejsze będzie kopiowanie referencji (praktycznie zawsze w 1 cyklu). Obiekty o **dłuższym cyklu życia** zazwyczaj bardziej pasuje przechowywać na stercie, to nam gwarantują klasy. Jeżeli musimy zamodelować hierarchię obiektów, które **wymagają dziedziczenia**, to musimy użyć klas.

## Konstruktor domyślny

Struktury zawsze posiadają bezparametrowy konstruktor domyślny, który zeruje wszystkie pola struktury. Nawet jeżeli zdefiniujemy konstruktor bezparametrowy możemy wywołać konstruktor domyślny przez `default`. Poza tym tablica struktur domyślnie jest bitowo wyzerowana, a klasy również bitowo zerują swoje pola. Warto się przed tym zabezpieczyć i zawsze traktować wyzerowaną strukturę jako możliwą poprawną wartość.

```csharp
Point p1 = new Point(1); // 1, 1
Point p2 = new Point();  // 0, 0

public struct Point
{
    public float X { get; set; }
    public float Y { get; set; } = 1;
    
    public Point(x) => X = x;
}
```

## struktury `readonly`

Słówko `readonly` przy definicji struktury wymusza żeby struktura była niezmienialna, to jest pola również były tylko do odczytu, a właściwości mogły mieć tylko akcesor `get`.

```csharp
public readonly struct Point
{
    public readonly float X;
    public float Y { get; } = 1;
    
    public Point(x) => X = x;
}
```

Pojedyncze metody również można oznaczać `readonly`, co sprawia, że nie możemy zmieniać pól struktur w takiej metodzie.

## struktury `ref`

Struktury ze słówkiem kluczowym ref mogą być alokowane jedynie na stosie, każda konstrukcja, która wymuszała by umieszczenie takiej zmiennej na stercie, kończy się błędem kompilacji. Między innymi nie można tworzyć tablic takich struktur, nie mogą być polami klas lub nie `ref` struktur, nie mogą być używane wewnątrz iteratorów, metod asynchronicznych ani lambd, nie mogą implementować interfejsów.

```csharp
public ref struct Point
{
    public float X { get; set; }
    public float Y { get; set; }
}
```
