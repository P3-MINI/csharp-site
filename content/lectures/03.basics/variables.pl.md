---
title: "Zmienne"
weight: 80
---

# Zmienne

## Przypisanie niewątpliwe (*Definite assignment*)

Zasada przypisania niewątpliwego (*Definite assignment rule*) jest regułą, którą wymusza kompilator, aby zapewnić że każda zmienna lokalna jest zainicjalizowana przed użyciem. Użycie potencjalnie niezainicjalizowanej zmiennej powoduje błąd kompilacji. Nie dotyczy to elementów tablicy i pól klas, te są domyślnie inicjalizowane zerami (bitowo).

```csharp
int x;
Console.WriteLine(x); // Compilation error

int[] ints = new int[2];
Console.WriteLine(ints[1]); // OK

Test test = new Test();
Console.WriteLine(test.X); // OK

public class Test { public int X; }
```

## Wartości domyślne

Zmienną można jawnie zainicjalizować wartością domyślną za pomocą operatora `default`. Wartością domyślną jest zawsze bitowo wyzerowany obiekt dla typów bezpośrednich, lub `null` dla typów referencyjnych. Zauważmy że sprowadza się to zawsze do wartości `0` dla typów numerycznych.

```csharp
float x = default;
Console.WriteLine(default(float));
```

## Niejawne typowanie zmiennych lokalnych

Jeżeli kompilator jest w stanie wydedukować typ zmiennej na podstawie prawej strony przypisania, to możemy deklarację typu zastąpić słowem kluczowym `var`. Zmienne muszą być zainicjalizowane w tej samej linii w której są deklarowane.

```csharp
var greeting = "Hello class";
var i = 0;
var x = 0.15f;
var list = new List<int>();
var dayOfWeek = DateTime.Now.DayOfWeek;
```