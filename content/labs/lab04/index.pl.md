---
title: "Lab04"
date: 2022-02-05T17:26:02+01:00
weight: 10
---

# Laboratorium 4: BCL, Kolekcje i Wyrażenia Lambda

## Przetwarzanie tekstu, parsowanie

{{% hint info %}}
**Co to jest REST API?**

REST (Representational State Transfer) to architektoniczny styl projektowania usług sieciowych, który wykorzystuje standardowe operacje HTTP (`GET`, `POST`, `PUT`, `DELETE`) do komunikacji klient‑serwer.

- Zasoby identyfikowane są przy pomocy URL (np. `https://api/users/123`).
- Metody HTTP określają akcję (np. pobranie, utworzenie, modyfikację).
- Każdy zapytanie zawiera wszystkie informacje potrzebne do jego przetworzenia (bezstanowość ang. _stateless_).

W praktyce niemal każda nowoczesna aplikacja webowa czy mobilna udostępnia REST API.

**Ścieżki do zasobów (ang. _resource paths_)**

Ścieżki w URL określają, do jakich zasobów i w jakiej hierarchii chcemy się odwołać na serwerze. Każdy segment po hoście to albo nazwa zasobu, albo jego identyfikator.

- `/v1/users/42`: wersja `1`, zasób `users`, identyfikator `42`.
- `/v2/countries/100/cities/3`: wersja `2`, zasób `countries` (id `100`), a następnie zagnieżdżony zasób `items` (id `200`).

**Parametry zapytania (ang. _query parameters_)**

Parametry zapytania, umieszczone po znaku `?`, pozwalają przekazywać dodatkowe dane do serwera:

- Każdy parametr to `klucz=wartość` (np. `lang=pl`).
- Poszczególne pary `klucz=wartość` oddzielane są znakiem `&`, np. `?tag=new&active=true`.
  {{% /hint %}}

### Opis zadania

Twoim zadaniem jest zaimplementowanie metody:

```csharp
public static (ParsedUrl url, ParsingStatus status) ParseUrl(string url);
```

**Metoda powinna:**

- Przyjmować jako parametr ciąg znaków `url`.
- Zwracać krotkę o dwóch nazwanych elementach:
  - `url` - obiekt klasy `ParsedUrl` zawierająy szczegóły parsowanego adresu.
  - `status` - wartość typu wyliczeniowego `ParsingStatus` informującą o powodzeniu lub przyczynie błędu parsowania.
- W przypadku nieudanego parsowania, zawartość obiektu `ParsedUrl` jest nieistotna (kluczowy jest wówczas `status`).

**Definicje typów pomocniczych:**

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
public sealed class ResourceSegment
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
public sealed class ParsedUrl
{
    public ParsingStatus Status { get; set; }
    public UrlScheme Scheme { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Version { get; set; }
    public List<ResourceSegment> PathSegments { get; set; } = [];
    public Dictionary<string, List<string>> QueryParams { get; set; } = [];
}
```

**Struktura oczekiwanego URL**

```
[scheme]://[host]/v[version]/[resource1]/[id1]/[resource2]/[id2]/...?[param1]=[valueA]&[param1]=[valueB]&[param2]=[valueC]...
```

gdzie:

- `scheme`: jeden z elementów typu wyliczeniowego `UrlScheme`,
- `host`: nazwa domeny (np. `example.com`),
- `version`: liczba całkowita (prefiks `v`, np. `v2`),
- `resourceN`: nazwa zasobu (ciąg znaków bez `/`),
- `idN`: całkowitoliczbowy identyfikator zasobu,
- `paramN`: nazwa parametru,
- `valueN`: wartość parametru.

{{% hint warning %}}
**Uwagi implementacyjne**

- **Parametry zapytania:**
  - W zadaniu przyjmujemy, że nazwa parametru **może się pojawić na liście parametrów wielokrotnie**.
  - Wszystkie wartości dla danego parametru agregowane są w postaci listy.
- **Operacje na klasie `string`:**
  - Nie korzystaj z `System.Uri`.
  - Używaj gotowych metod klasy `string` (np. `Split`, `IndexOf`, `Substring`, `Contains`).
- **Status parsowania:**
  - `Success`: parsowanie bez błędów,
  - `InvalidScheme`: niezgodny lub nieobsługiwany _scheme_,
  - `InvalidHost`: brak części odpowiadającej za hosta,
  - `InvalidVersion`: wersja nie parsuje się na `int` lub jest `< 1`,
  - `InvalidPath`: błędna liczba segmentów (np. brak identyfikatora dla ostatniego zasobu),
  - `InvalidId`: identyfikator jest niepoprawny (zasady te same jak dla wersji),
  - `InvalidQuery`: błędny format _query_ (np. brak `=`),
  - `UnexpectedFormat`: dowolny inny błąd.
- **Debuggowanie:** - Zadanie jest dobrą okazją do zaznajomienia się z działaniem debuggera (preferowane IDE to [Visual Studio](https://visualstudio.microsoft.com/pl/)).
  {{% /hint %}}

**Przykłady**

{{% details "Poprawny URL z jednym segmentem i jednym parametrem" false %}}

```csharp
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
```

{{% /details %}}
{{% details "URL z wieloma segmentami i wielokrotnymi parametrami" false %}}

```csharp
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
```

{{% /details %}}
{{% details "Nieobsługiwany scheme" false %}}

```csharp
string input3 = "smtp://host/v1/res/1";
var (_, status3) = ParseUrl(input3);
// status3 == ParsingStatus.InvalidScheme
```

{{% /details %}}
{{% details "Nieparzysta liczba segmentów w ścieżce" false %}}

```csharp
string input4 = "https://host/v1/resOnly";
var (_, status4) = ParseUrl(input4);
// status4 == ParsingStatus.InvalidPath
```

{{% /details %}}
{{% details "Błędny parametr zapytania (brak '=')" false %}}

```csharp
string input5 = "http://host/v1/r/10?badparam";
var (_, status5) = ParseUrl(input5);
// status5 == ParsingStatus.InvalidQuery
```

{{% /details %}}

### Przykładowe rozwiązanie

## Formatowanie, data i czas

{{% hint info %}}
**Czym charakteryzuje się format CSV?**

CSV (ang. _comma-separated values_) to format przechowywania danych w plikach tekstowych (odpowiadający mu typ MIME to `text/csv`).

- Poszczególne rekordy oddzielone są znakami końca linii `\n`.
- Wartości pól standardowo oddzielone są przecinkami `,`.
- Jako separator bywa stosowany znak średnika `;` (tak będzie właśnie w naszym zadaniu).
- W jednym pliku może być użyty tylko jeden rodzaj separatora.
- Wartości pól mogą być ujęte w cudzysłowy (w przypadku wartości zawierających znak separatora jest to wymagane).
- Pierwsza linia może stanowić nagłówek zawierający nazwy pól rekordów.

**Czego się nauczysz?**

- Pracy z datą i czasem w języku `C#`, odczytując dane z pliku CSV, zawierającego dzienne pomiary temperatury w wybranych europejskich stolicach.
- Operacji na datach i interwałach czasowych (klasy `DateTime` i `Timespan`).
- Formatowaniem wyjścia zgodnie z ustawieniami kulturowymi (`CultureInfo`).
- Użyciem metody `ForEach` standardowej kolekcji `List<T>` oraz konstruowania prostych wyrażeń lambda do filtrowania i wyświetlania danych.
  {{% /hint %}}

### Opis zadania

Twoim zadaniem jest zaimplementowanie metody:

```csharp
public static List<Measurement> ParseMeasurements(string content);
```

**Metoda powinna:**

- Przyjmować jako parametr ciąg znaków `content`, będący zawartością pliku CSV.
- Parsować kolejne rekordy pliku i zwrócić listę obiektów typu `Measurement`.

Następnie zaimplementuj funkcjonalność wypisywania obiektów klasy `Measurement`, tak aby ciąg znaków odpowiadający pojedynczemu obiektowi zawierał:

- Kraj i miasto,
- Datę w formacie długim zgodnie z kulturą określoną polem `Code`,
- Wartości pomiarów sformatowane z separatorami dziesiętnymi i tysięcznymi właściwymi dla kultury.

Przykład wypisania dla rekordu z `Code = "pl-PL"`:

```
Location: Poland, Warsaw
Date: 21 czerwca 2025
Temperatures:
   0,50 °C   3,20 °C   7,10 °C   12,45 °C
  16,30 °C  14,10 °C  10,05 °C    1,23 °C
   1,23 °C   4,56 °C   7,89 °C
```

Po sparsowaniu zawartości pliku wypisz obiekty spełniające następujące warunki:

- Data pomiarów przeprowadzonych w przedziale od 8 czerwca do 13 września bieżącego roku.
- Data pomiarów przeprowadzonych w roku 2025.
- Data pomiarów przeprowadzonych tylko w weekendy (sobota i niedziela).
- Data pomiarów przeprowadzonych w ciągu ostatnich 7 dni.

**Definicje typów pomocniczych:**

```csharp
public sealed class Measurement
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double[] Temperatures { get; set; } = [];
}
```

**Format pliku CSV**

Plik CSV wygląda w następujący sposób:

```
Location; Code; Date; Temperatures
Poland  Warsaw  ; pl-PL; 2025-06-21; [  25.9, 18.0  , 9.5 , 24.3  ]
```

Skorzystaj z następującego pliku CSV: [measurements.csv](./src/measurements.csv).

{{% hint warning %}}
**Uwagi implementacyjne**

- **Parsowanie pliku CSV:**
  - Do odczytania zawartości pliku skorzystaj z metody [`File.ReadAllText`](https://learn.microsoft.com/pl-pl/dotnet/api/system.io.file.readalltext?view=net-9.0).
  - Tablica pomiarów zawiera wartości zapisane przy użyciu `CultureInfo.InvariantCulture`, które są rozdzielone za pomocą znaku przecinka `,`.
  - Poszczególne pola rekordów mogą zawierać nadmiarowe białe znaki, których należy się pozbyć.
  - Dla ułatwienia można założyć, że wszystkie rekordy i pola zawierają poprawne dane (np. `Date` zawiera poprawnie zapisaną datę, a `Temperatures` poprawnie zapisane liczby zmiennoprzecinkowe).
- **Wypisywanie obiektów**:
  - Należy zachować formatowanie z przykładu (format daty, szerokość wypisywania wyrównanie, liczba miejsc po przecinku itp.).
  - Do filtrowania obiektów wykorzystaj metodę `ForEach` oraz wyrażenia lambda. Nie należy korzystać z jawnej pętli.
    {{% /hint %}}

### Przykładowe rozwiązanie
