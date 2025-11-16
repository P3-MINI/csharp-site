---
title: "Dyrektywy preprocesora"
---

## Dyrektywy preprocesora

Preprocessor w C# jest znacznie prostszy niż w C/C++.
* Nie ma dyrektywy `#include` (w C# używamy `using` do importowania przestrzeni nazw).
* `#define` nie pozwala na tworzenie makr z wartościami ani makr funkcyjnych. Służy jedynie do definiowania symboli warunkowych.

## Kompilacja warunkowa (#if, #elif, #else, #endif)

To najczęstsze i najważniejsze zastosowanie dyrektyw. Pozwala na kompilowanie bloków kodu tylko wtedy, gdy określony symbol jest zdefiniowany. Symbole można definiować i oddefiniowywać za pomocą dyrektyw `#define` i `#undef`.

Wewnątrz warunków można używać operatorów logicznych `!`, `&&` i `||`.

```csharp
#define TEST
#undef TRACE
#if TEST
    Console.WriteLine("TEST is defined");
#endif
#if !TEST
    Console.WriteLine("TEST is not defined");
#endif
#if TEST && DEBUG
    Console.WriteLine("DEBUG and TEST is defined");
#else
    Console.WriteLine("DEBUG and TEST are not both defined");
#endif
```

Można je także definiować w pliku projektu. Niektóre symbole takie jak `DEBUG` są na przykład automatycznie dołączane przy konfiguracji `Debug`.

```xml
<PropertyGroup>
    <DefineConstants>TRACE;DEMO</DefineConstants>
</PropertyGroup>
```

## Kontekst `nullable`

Za pomocą dyrektywy `#nullable` można włączać i wyłączać kontekst *nullable* w którym kompilator dokonuje analizy statycznej pod względem nullowalności typów.

```csharp
#nullable disable
string str1 = null;
#nullable enable
string str2 = null!;
#nullable restore
string str3 = null!;
```

## Regiony

Regiony same w sobie nic nie robią. Grupują kod w bloki, które można zwijać w IDE.

```csharp
#region REGION_EXPLANATION
private static void Region()
{
    PrintCurrentMethodName();
    Console.WriteLine("""
        region is a preprocessor directive
        that is used to group related pieces
        of code together in a way that can
        be collapsed or expanded in the code editor.
        """);
}
#endregion
```

## Sterowanie ostrzeżeniami

Dyrektywa `#pragma warning` pozwala wyciszać i odciszać ostrzeżenia kompilatora.

```csharp
#pragma warning disable CS0219 // unused variable
string str = "Hello, world!";
#pragma warning restore CS0219
```

> Kody błędów można znaleźć w [repozytorium GenerateCSharpErrors](https://github.com/thomaslevesque/GenerateCSharpErrors/blob/master/CSharpErrorsAndWarnings.md).

Za pomocą dyrektyw można także generować ostrzeżenia i błędy kompilacji, co jest przydatne w połączeniu z warunkową kompilacją:

```csharp
#if !DEMO
    #error DEMO is not defined
    Console.WriteLine("This code will not "+
    "compile if demo is not defined.");
#else
    #warning DEMO is defined
    Console.WriteLine("This code will generate a warning message if DEMO is defined.");
#endif
```