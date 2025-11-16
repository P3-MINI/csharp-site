---
title: "Dokumentacja XML"
---

# Dokumentacja XML

Dokumentacja XML w C# to specjalny rodzaj komentarzy, które umieszczasz bezpośrednio w kodzie źródłowym, a które są pisane w formacie XML. Kompilator C# przetwarza te komentarze i na ich podstawie generuje osobny plik XML, który towarzyszy skompilowanemu assembly (.dll lub .exe). Aby kompilator wygenerował plik .xml z dokumentacją, należy włączyć opcję `GenerateDocumentationFile` w ustawieniach projektu.

```xml
<PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Głównym celem dokumentacji XML jest ułatwienie zrozumienia i używania kodu, zarówno przez innych deweloperów, jak i przez narzędzia.

1. **IntelliSense i podpowiedzi w IDE**: Środowiska programistyczne (IDE) takie jak *Visual Studio*, *Visual Studio Code* czy *Rider*, używają dokumentacji XML do wyświetlania podpowiedzi, opisów parametrów itp.
2. **Generowanie dokumentacji**: Plik XML może być użyty przez zewnętrzne narzędzia (np. `DocFX`, `Sandcastle`, `doxygen`) do automatycznego generowania, dokumentacji (np. w formie stron HTML).
3. **Utrzymywalność kodu**: Zachęca deweloperów do dokumentowania publicznych interfejsów swojego kodu, co poprawia jego zrozumienie i ułatwia utrzymanie w przyszłości.

Dokumentację XML pisze się za pomocą `///` (dla komentarzy jednoliniowych) lub `/** ... */` (dla komentarzy wieloliniowych), umieszczonych bezpośrednio nad deklaracją składowej (klasy, metody, właściwości, pola, zdarzenia, elementu enum itp.).

```csharp
/// <summary>
/// Abstract base class representing a mathematical matrix.
/// </summary>
public abstract class Matrix;
```

> [!INFO]
> Przykładowy projekt z dokumentacją:
> {{< filetree dir="lectures/csharp-features/xml-documentation" >}}

## Znaki specjalne XMLa

Tylko 5 znaków wymaga uciekania specjalnymi sekwencjami:

| Znak | Sekwencja |
|:---:|:---:|
| \"   | \&quot;   |
| \'   | \&apos;   |
| \<   | \&lt;     |
| \>   | \&gt;     |
| \&   | \&amp;    |

```csharp
/// <summary>
/// This property always returns a value &lt; 1.
/// </summary>
```

## Ogólne znaczniki

`summary` zawiera krótki, zwięzły opis typu lub składowej:

```csharp
/// <summary>
/// description
/// </summary>
```

`remarks` zawiera bardziej szczegółowe informacje, uwagi, kontekst użycia.:
```csharp
/// <remarks>
/// description
/// </remarks>
```

## Dokumentacja metod

`returns` opisuje wartość zwracaną przez metodę:
```xml
<returns>description</returns>
```

`param` opisuje paramter metody:
```xml
<param name="name">description</param>
```

`exception` opisuje wyjątek, który może zostać rzucony przez metodę.
```xml
<exception cref="ExceptionType">description</exception>
```

## Formatowanie

Znacznik `para` pozwala dzielić dokumentację na paragrafy:

```xml
<remarks>
    <para>
        This is an introductory paragraph.
    </para>
    <para>
        This paragraph contains more details.
    </para>
</remarks>
```

Znacznik `list` pozwala tworzyć listy i tabele:

```xml
<list type="bullet|number|table">
    <listheader>
        <term>term</term>
        <description>description</description>
    </listheader>
    <item>
        <term>Assembly</term>
        <description>
            The library or executable built from a compilation.
        </description>
    </item>
</list>
```

## Przykłady kodu

Znacznik `c` pozwala umieszczać jednolinijkowy kod wpleciony w tekst:

```xml
<c>x = x++;</c>
```

Znacznik `code` pozwala umieszczać kod w bloku w osobnym akapicie:

```xml
<code>
    var index = 5;
    index++;
</code>
```

Znacznik `example` zawiera przykład kodu demonstrujący użycie składowej.

```xml
<example>
    This shows how to increment an integer.
    <code>
        var index = 5;
        index++;
    </code>
</example>
```

## Reużywanie dokumentacji

Dokumentacja elementu może zostać odziedziczona z typu bazowego lub interfejsu przy użyciu znacznika `inheritdoc`:

```xml
<inheritdoc [cref=""] [path=""]/>
```

Lub zapożyczona z innego pliku xml:

```xml
<include file='filename' path='tagpath[@name="id"]' />
```

## Linki

Możemy w tekście dokumentacji zamieszczać linki do innych składowych lub dowolny adres url:

```xml
<see cref="member"/>
<!-- or -->
<see cref="member">Link text</see>
<!-- or -->
<see href="link">Link Text</see>
<!-- or -->
<see langword="keyword"/>
```

> cref = code reference
> href = hyperlink reference (url)

Znacznik `seealso` zostanie umieszczony we własnym akapicie:

```xml
<seealso cref="member"/>
<!-- or -->
<seealso href="link">Link Text</seealso>
```

## Parametry generyczne

Znacznik `typeparam` opisuje parametry generyczne:
```xml
<typeparam name="TResult">description</typeparam>
```

Żeby odnieść się do typu generycznego używamy znaków `{}` zamiast `<>`:
```xml
<see cref="Foo{T,U}"/>
```

## Generowanie dokumentacji

Z wygenerowanego przez kompilatora pliku xml, można dalej generować bardziej przyjazne formaty dokumentacji, takie jak pliki `pdf`, strony `HTML`, pliki pomocy `CHM`, czy strony manuala (*manpages*).

### Doxygen

Doxygen najlepiej działa, gdy wskażemy mu bezpośrednio katalog z plikami źródłowymi `.cs`. Samodzielnie je sparsuje i wyciągnie z nich komentarze XML. To lepsze podejście niż podawanie mu samego pliku `.xml`, `Doxygen` może wtedy zbudować dodatkowe diagramy, np. grafy wywołań funkcji.

Najpierw trzeba wygenerować plik konfiguracyjny `Doxyfile` w katalogu projektu:

```bash
doxygen -g Doxyfile
```

Po skończonej konfiguracji można na jej podstawie wygenerować dokumentację:

```bash
doxygen Doxyfile
```

Alternatywnie można użyć dostarczanego przez `Doxygen` interfejsu graficznego:

```bash
doxywizard
```
