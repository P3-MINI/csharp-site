---
title: "Lab04"
date: 2022-02-05T17:26:02+01:00
weight: 10
---

# Laboratorium 4: BCL, Kolekcje i Wyrażenia Lambda

## Przetwarzanie tekstu, klasa `string`

### Opis zadania

Celem zadania jest zaimplementowanie metody `ParseUrl`, która sparsuje URL o określonej strukturze. Metoda powinna zwracać krotkę, zawierającą na pierwszej pozycji sparsowany obiekt klasy `ParsedUrl`, a na drugiej pozycji wartość typu wyliczeniowego `ParsingStatus`. W przypadku błędu parsowania, dokładna zawartość pierwszej pozycji krotki nie jest istotna.

1. **Sygnatura metody**

```csharp
public static (ParsedUrl url, ParsingStatus status) ParseUrl(string url)
```

2. **Definicje typów pomocniczych**

```csharp
// Schemat protokołu
public enum UrlScheme
{
    Http,
    Https,
    Ftp,
    Wss
}

// Segment ścieżki (nazwa zasobu + identyfikator)
public class ResourceSegment
{
    public string Name { get; set; }
    public id Id { get; set; }
}

// Status parsowania (informuje o powodzeniu lub błędzie)
public enum ParseResult
{
    UnexpectedFormat,
    Success,
    InvalidScheme,
    InvalidHost,
    InvalidVersion,
    InvalidPath,
    InvalidId,
    InvalidQuery,
}

// Typ zwracany przez metodę
public class ParsedUrl
{
    public ParsingStatus Status { get; set; }
    public UrlScheme Scheme { get; set; }
    public string Host { get; set; } = string.empty;
    public int Version { get; set; }
    public List<ResourceSegment> PathSegments { get; set; } = [];
    public Dictionary<string, List<string>> QueryParams { get; set; } = [];
}
```

3. **Struktura URL**

```
[scheme]://[host]/v[version]/[resource1]/[id1]/[resource2]/[id2]/...?[param1]=[valueA]&[param1]=[valueB]&[param2]=[valueC]...
```

gdzie:

- `scheme`: jeden z elementów `UrlScheme`,
- `host`: dowolny ciąg domeny,
- `version`: liczba całkowita (prefiks `v`),
- `resourceN`: nazwa segmentu zasobu (ciąg znaków bez `/`),
- `idN`: identyfikator będący liczbą całkowitą,
- `paramN`: nazwa parametru,
- `valueN`: wartość parametru.

W zadaniu przyjmujemy, że nazwa parametru **może się pojawić na liście parametrów wielokrotnie**. Wszystkie wartości dla danego parametru agregowane są w postaci listy.

4. **Wymagania implementacyjne**

- Operacje na `string`:
  - Używaj metod `Split`, `IndexOf`, `Substring` itp.
  - Nie korzystaj z `System.Uri`.
- Parametry zapytania:
  - Rozbij ciąg znaków najpierw po `&`, następnie po `=`.
  - Agreguj różne wartości parametru do osobnej listy.
- Status parsowania:
  - `Success`: parsowanie bez błędów,
  - `InvalidScheme`: niezgodny lub nieobsługiwany scheme,
  - `InvalidHost`: brak części odpowiadającej za hosta,
  - `InvalidVersion`: wersja nie parsuje się na `int` lub jest `< 1`,
  - `InvalidPath`: błędna liczba segmentów (np. brak identyfikatora dla ostatniego zasobu),
  - `InvalidId`: identyfikator jest niepoprawny (zasady te same jak dla wersji),
  - `InvalidQuery`: błędny format query (np. brak `=`),
  - `UnexpectedFormat`: dowolny inny błąd.

5. Przykłady

```csharp
// 1. Poprawny URL z jednym segmentem i jednym parametrem
string input1 = "http://example.com/v1/users/42?lang=pl";
var (parsed1, status1) = ParseUrl(input1);
/*
status1              == ParsingStatus.Success
parsed1.Scheme       == UrlScheme.Http
parsed1.Host         == "example.com"
parsed1.Version      == 1
parsed1.PathSegments == [ { Name = "users", Id = 42 } ]
parsed1.QueryParams  == { "lang": ["pl"] }
*/

// 2. URL z wieloma segmentami i wielokrotnymi parametrami
string input2 = "https://api.test/v2/orders/100/items/200" +
                "?tag=new&tag=discount&active=true";
var (parsed2, status2) = ParseUrl(input2);
/*
status2              == ParsingStatus.Success
parsed2.Scheme       == UrlScheme.Https
parsed2.Host         == "api.test"
parsed2.Version      == 2
parsed2.PathSegments == [
    { Name = "orders", Id = 100 },
    { Name = "items",  Id = 200 }
]
parsed2.QueryParams  == {
    "tag":    ["new","discount"],
    "active": ["true"]
}
*/

// 3. Nieobsługiwany scheme
string input3 = "smtp://host/v1/res/1";
var (_, status3) = ParseUrl(input3);
// status3 == ParsingStatus.InvalidScheme

// 4. Nieparzysta liczba segmentów w ścieżce
string input4 = "https://host/v1/resOnly";
var (_, status4) = ParseUrl(input4);
// status4 == ParsingStatus.InvalidPath

// 5. Błędny parametr zapytania (brak '=')
string input5 = "http://host/v1/r/10?badparam";
var (_, status5) = ParseUrl(input5);
// status5 == ParsingStatus.InvalidQuery
```

6. Uwagi

- Zadanie jest dobrą okazją do zaznajomienia się z działaniem debuggera (preferowane IDE to [Visual Studio](https://visualstudio.microsoft.com/pl/)).

### Przykładowe rozwiązanie

## Zadanie 2
