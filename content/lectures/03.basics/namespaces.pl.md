---
title: "Przestrzenie nazw"
weight: 100
---

# Przestrzenie nazw

Przestrzenie nazw działają na tej samej zasadzie co w C++. Różnicą jest sposób wyłuskiwania elementów z przestrzeni nazw, w C# robi się to za pomocą operatora `.`. Poza tym mamy kilka sposobów definicji przestrzeni nazw.

> Wyjątkiem jest globalna przestrzeń nazw z której wyłuskuje się elementy poprzez operator `global::`. W praktyce jest to jednak rzadko przydatne (wcale).

```csharp
Outer.Middle.Inner.ClassA a;

namespace Outer
{
    namespace Middle
    {
        namespace Inner
        {
            class ClassA {}
        }
    }
}

namespace Outer.Middle.Inner
{
    class ClassB {}
}

namespace Outer.Middle.Inner; // Obowiązuje do końca pliku

class classC {}
```

## Dyrektywa `using`

Dyrektywa `using` pozwala na importowanie przestrzeni nazw do bieżącego pliku, dzięki czemu nie trzeba używać pełnej kwalifikacji nazw dla typów w nich zawartych.

```csharp
using Outer.Middle.Inner;

Outer.Middle.Inner.ClassB b1;
ClassB b2; // Można też tak

namespace Outer.Middle.Inner
{
    class ClassB {}
}
```

## Statyczne `using`

Statyczne `using` **importuje typy**, a nie przestrzenie nazw. Efektem tego jest możliwość używania wszystkich statycznych członków klas, a w przypadku wyliczeń jego elementów, bez podawania pełnego kwalifikatora.

```csharp
using static System.Console; // class
using static System.DayOfWeek; // enum

WriteLine("Hello, world!");
WriteLine(Monday); // without using, it would be `DayOfWeek.Monday`
```

## Globalne `using`

Globalne `using` propaguje się na wszystkie pliki projektu. Musi być umieszczone przed innymi dyrektywami `using`.

```csharp
global using System;
global using System.IO;
```

## Niejawne, globalne `using` (*Implicit global using*)

Od .NETa 6.0, projekty w stylu SDK posiadają opcję, która w momencie budowania programu dodaje do naszego programu plik z globalnymi dyrektywami `using`.

Dyrektywy te zależą od wybranego SDK, dla "Microsoft.NET.Sdk" są to:

- System
- System.Collections.Generic
- System.IO
- System.Linq
- System.Net.Http
- System.Threading
- System.Threading.Tasks

Można tę opcję wyłączyć ustawiając właściwość `ImplicitUsings` na `false` w pliku projektu.

## Aliasy typów

Dyrektywy `using` można też używać do wprowadzania aliasu pojedynczego typu lub przestrzeni nazw, zamiast importowania wszystkiego z przestrzeni nazw. Przydatne wtedy gdy dwa typy kolidują w dwóch różnych przestrzeniach nazw, a chcemy używać obydwu, np. `Vector3` z `System.Numerics` i `Vector3` z zewnętrznej biblioteki. Od C# 12 możemy aliasować praktycznie dowolny typ, w tym tablice i krotki.

```csharp
using Vec3 = System.Numerics.Vector3;
using Refl = System.Reflection;
using NumberList = double[];
using PersonInfo = (string Name, int Age);

Vec3 vector;
Refl.PropertyInfo propInfo;
NumberList numbers = { -0.5, 0.5 };
PersonInfo person = ("Alice", 42);
```
