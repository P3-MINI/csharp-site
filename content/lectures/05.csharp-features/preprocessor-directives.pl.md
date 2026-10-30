---
title: "Dyrektywy preprocesora"
---

# Dyrektywy preprocesora

Preprocesor w C# jest znacznie prostszy niż w C/C++.
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
    Console.WriteLine("DEBUG and TEST are defined");
#else
    Console.WriteLine("DEBUG and TEST are not both defined");
#endif
```

Można je także definiować w pliku projektu. Niektóre symbole, takie jak `DEBUG`, są automatycznie definiowane, na przykład w konfiguracji `Debug`.

```xml
<PropertyGroup>
    <DefineConstants>TRACE;DEMO</DefineConstants>
</PropertyGroup>
```

## Kontekst `nullable`

Za pomocą dyrektywy `#nullable` można włączać i wyłączać kontekst *nullable*, w którym kompilator dokonuje analizy statycznej pod względem nullowalności typów.

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

Dyrektywa `#pragma warning` pozwala wyciszać i przywracać ostrzeżenia kompilatora.

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
    "compile if DEMO is not defined.");
#else
    #warning DEMO is defined
    Console.WriteLine("This code will generate a warning message if DEMO is defined.");
#endif
```

## Aplikacje jednoplikowe (.NET 10)

Od .NET 10 pojedynczy plik `.cs` można zbudować i uruchomić bez pliku projektu (`dotnet run app.cs`) jako aplikację jednoplikową (*file-based app*). Ustawienia projektu podaje się wtedy w samym pliku, za pomocą dyrektyw `#:` umieszczonych na początku pliku, przed jakimkolwiek kodem:

- `#:package Nazwa@Wersja` - dodaje referencję do pakietu NuGet,
- `#:property Nazwa=Wartość` - ustawia właściwość MSBuild,
- `#:sdk Nazwa` - wybiera SDK projektu (domyślnie `Microsoft.NET.Sdk`),
- `#:project ścieżka` - dodaje referencję do innego projektu.

W systemach uniksowych pierwszą linią pliku może być też *shebang* (`#!`), który pozwala uruchamiać plik bezpośrednio, jak skrypt (plik musi mieć końce linii `LF` i nie może zaczynać się od BOM). Przykładowy plik `hello.cs`:

```csharp
#!/usr/bin/env dotnet
#:package Humanizer@3.0.10
#:property PublishAot=false

using Humanizer;

Console.WriteLine($"Hello, {(args.Length > 0 ? args[0] : "world")}!");
Console.WriteLine(TimeSpan.FromMinutes(90).Humanize(2)); // 1 hour, 30 minutes
```

```bash
dotnet run hello.cs -- Alice  # Hello, Alice!
chmod +x hello.cs
./hello.cs Alice              # Hello, Alice!
```

Te dyrektywy są dozwolone tylko w aplikacjach jednoplikowych - w zwykłym projekcie powodują błąd kompilacji. Polecenie `dotnet project convert hello.cs` zamienia taki plik na zwykły projekt z plikiem `.csproj`. Pracę z aplikacjami jednoplikowymi opisano szerzej w wykładzie [MSBuild]({{< ref "/lectures/02.tools/msbuild" >}}).
