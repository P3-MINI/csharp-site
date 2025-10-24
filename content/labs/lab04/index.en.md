---
title: "Lab04"
weight: 10
---

# Laboratory 4: BCL, Collections, and Lambda Expressions

## Text Processing, Parsing

{{% hint info %}}
**What is a REST API?**

REST (Representational State Transfer) is an architectural style for designing web services that uses standard HTTP operations (`GET`, `POST`, `PUT`, `DELETE`) for client-server communication.

- Resources are identified using URLs (e.g., `https://api/users/123`).
- HTTP methods define the action (e.g., retrieve, create, update).
- Each request contains all the information needed to process it (statelessness).

In practice, almost every modern web or mobile application exposes a REST API.

**Resource Paths**

URL paths define which resources and at what hierarchy we want to refer to on the server. Each segment after the host is either a resource name or its identifier.

- `/v1/users/42`: version `1`, resource `users`, identifier `42`.
- `/v2/countries/100/cities/3`: version `2`, resource `countries` (id `100`), nested resource `cities` (id `3`).

**Query Parameters**

Query parameters, placed after the `?` symbol, allow passing additional data to the server:

- Each parameter is `key=value` (e.g., `lang=pl`).
- Multiple key-value pairs are separated with `&`, e.g., `?tag=new&active=true`.
{{% /hint %}}

### Task Description

Your task is to implement the method:

```csharp
public static (ParsedUrl url, ParsingStatus status) ParseUrl(string url);
```

**The method should:**

- Accept a `string url` as input.
- Return a tuple with two named elements:
  - `url` – an instance of the `ParsedUrl` class containing details of the parsed URL.
  - `status` – a value of the `ParsingStatus` enumeration indicating success or the parsing error.
- If parsing fails, the contents of the `ParsedUrl` object are irrelevant (only `status` matters).

**Helper type definitions:**

```csharp
// Schema
public enum UrlScheme
{
	Http,
	Https,
	Ftp,
	Wss
}

// Resource segment (the name of the resource + identifier)
public sealed class ResourceSegment
{
	public string Name { get; set; } = string.Empty;
	public int Id { get; set; }
}

// Parsing status (whether the parsing was successful or not)
public enum ParsingStatus
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

// The type returned by the method
public sealed class ParsedUrl
{
	public UrlScheme Scheme { get; set; }
	public string Host { get; set; } = string.Empty;
	public int Version { get; set; }
	public List<ResourceSegment> PathSegments { get; set; } = [];
	public Dictionary<string, List<string>> QueryParams { get; set; } = [];
}
```

**Expected URL structure:**

```
[scheme]://[host]/v[version]/[resource1]/[id1]/[resource2]/[id2]/...?[param1]=[valueA]&[param1]=[valueB]&[param2]=[valueC]...
```

where:

- `scheme`: one of the `UrlScheme` enum values.
- `host`: domain name (e.g., `example.com`).
- `version`: integer prefixed with `v` (e.g., `v2`).
- `resourceN`: resource name (string without `/`).
- `idN`: integer resource identifier.
- `paramN`: parameter name.
- `valueN`: parameter value.

{{% hint warning %}}
**Implementation notes**

- **Query parameters:**
  - A parameter name **may appear multiple times**.
  - All values for the same parameter should be collected into a list.
- **String operations:**
  - Do not use `System.Uri`.
  - Use string methods like `Split`, `IndexOf`, `Substring`, `Contains`.
- **Parsing status:**
  - `Success`: parsing succeeded.
  - `InvalidScheme`: scheme is unsupported or malformed.
  - `InvalidHost`: missing host component.
  - `InvalidVersion`: version cannot parse to `int` or is `< 1`.
  - `InvalidPath`: incorrect number of path segments (e.g., missing an identifier).
  - `InvalidId`: resource ID is invalid.
  - `InvalidQuery`: query string format is invalid (e.g., missing `=`).
  - `UnexpectedFormat`: any other error.
- **Debugging:** A good opportunity to get familiar with the debugger.
{{% /hint %}}

{{% hint info %}}
**Helpful resources:**

- [Microsoft Learn: Strings and string literals](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/strings/)
- [Microsoft Learn: Extract substrings from a string](https://learn.microsoft.com/en-us/dotnet/standard/base-types/divide-up-strings)
- [Microsoft Learn: How to separate strings using String.Split](https://learn.microsoft.com/en-us/dotnet/csharp/how-to/parse-strings-using-split)
- [Microsoft Learn: Dictionary<TKey,TValue> Class](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-8.0)
{{% /hint %}}

**Examples**

{{% details "Valid URL with one segment and one parameter" false %}}

```csharp
var input1 = "http://example.com/v1/users/42?lang=pl";
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

{{% details "URL with multiple segments and repeated parameters" false %}}

```csharp
var input2 = "https://api.test/v2/orders/100/items/200" +
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

{{% details "Unsupported scheme" false %}}

```csharp
var input3 = "smtp://host/v1/res/1";
var (_, status3) = ParseUrl(input3);
// status3 == ParsingStatus.InvalidScheme
```

{{% /details %}}

{{% details "Odd number of path segments" false %}}

```csharp
var input4 = "https://host/v1/resOnly";
var (_, status4) = ParseUrl(input4);
// status4 == ParsingStatus.InvalidPath
```

{{% /details %}}

{{% details "Invalid query parameter (missing '=')" false %}}

```csharp
var input5 = "http://host/v1/r/10?badparam";
var (_, status5) = ParseUrl(input5);
// status5 == ParsingStatus.InvalidQuery
```

{{% /details %}}

**Bonus Task**

- In your implementation of the `ParseUrl` method, try leveraging the capabilities provided by the `System.Text.RegularExpressions` namespace. You can learn about .NET regular expressions in the Microsoft Learn article [.NET regular expressions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions). A printable PDF reference is also available: [Regular expressions quick reference](https://download.microsoft.com/download/D/2/4/D240EBF6-A9BA-4E4F-A63F-AEB6DA0B921C/Regular%20expressions%20quick%20reference.pdf). For interactive regex development (compatible with .NET syntax), try the popular site [regex101.com](https://regex101.com/).

### Example Solution

An example implementation along with unit tests can be found in the file [`Task01.cs`](/labs/lab04/solution/tasks/Task01.cs).

## Formatting, Date, and Time

{{% hint info %}}
**What characterizes the CSV format?**

CSV (comma-separated values) is a format for storing tabular data in text files (MIME type `text/csv`).

- Records are separated by newline characters `
`.
- Fields are usually separated by commas `,`.
- Sometimes a semicolon `;` is used as the separator (as in our task).
- Only one separator type per file.
- Fields can be quoted if they contain the separator.
- The first line may be a header with field names.

**Learning goals:**

- Working with date and time in C#, reading data from a CSV file containing daily temperature measurements in selected European capitals.
- Working with dates and time intervals (`DateTime`, `TimeSpan`).
- Formatting output according to a culture (`CultureInfo`).
- Using `List<T>.ForEach` and simple lambda expressions to filter and display data.
{{% /hint %}}

### Task Description

Implement the method:

```csharp
public static List<Measurement> ParseMeasurements(string content);
```

**The method should:**

- Accept a `string content` representing the CSV file contents.
- Parse each record and return a list of `Measurement` objects.

Then implement functionality to print `Measurement` objects so that each object’s string representation includes:

- Country and city,
- Date in long format according to the culture specified by the `Code` field,
- Measurement values formatted with the culture’s decimal and thousand separators.

Example for a record with `Code = "pl-PL"`:

```
Location: Poland, Warsaw
Date: June 21, 2025
Temperatures:
   0.50 °C   3.20 °C   7.10 °C   12.45 °C
  16.30 °C  14.10 °C  10.05 °C    1.23 °C
   1.23 °C   4.56 °C   7.89 °C
```

After parsing, print objects that meet the following conditions:

- Measurements from June 8 to September 13 of the current year.
- Measurements from the year 2025.
- Measurements taken only on weekends (Saturday and Sunday).
- Measurements taken in the last 7 days.

**Helper type definitions:**

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

**CSV file format:**

```
Location; Code; Date; Temperatures
Poland  Warsaw  ; pl-PL; 2025-06-21; [  25.9, 18.0  , 9.5 , 24.3  ]
```

Use the following CSV file: `measurements.csv`.

{{% hint warning %}}
**Implementation notes**

- **Reading the CSV:**
  - Use `File.ReadAllText` to read the file.
  - Temperature arrays use `InvariantCulture` (decimal point `.`).
  - Trim excess whitespace.
  - Assume all data is valid.
- **Printing:**
  - Preserve formatting as in the example (date format, spacing, decimals, line width).
  - Use `List<T>.ForEach` and lambda expressions - no explicit loops.
{{% /hint %}}

{{% hint info %}}
**Helpful resources:**

- [Microsoft Learn: Parse strings in .NET](https://learn.microsoft.com/en-us/dotnet/standard/base-types/parsing-strings)
- [Microsoft Learn: Using the StringBuilder Class in .NET](https://learn.microsoft.com/en-us/dotnet/standard/base-types/stringbuilder)
- [Microsoft Learn: DateTime Struct](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-9.0)
- [Microsoft Learn: Standard date and time format strings](http://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)
- [Microsoft Learn: Standard numeric format strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
- [Microsoft Learn: CultureInfo Class](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=net-9.0)
- [Microsoft Learn: List<T>.ForEach(Action<T>) Method](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.foreach?view=net-9.0)
{{% /hint %}}

**Bonus Task**

- Implement CSV data reading using the popular NuGet package [CsvHelper](https://joshclose.github.io/CsvHelper/).
  Installation instructions are available in the Microsoft Learn guide (Visual Studio) [Install and manage packages in Visual Studio using the NuGet Package Manager](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio) and in the JetBrains documentation (Rider) [Consume NuGet packages﻿](https://www.jetbrains.com/help/rider/Using_NuGet.html).

### Example Solution

An example implementation can be found in the file [`Task02.cs`](/labs/lab04/solution/tasks/Task02.cs).

## Random Numbers and Lambda Expressions

{{% hint info %}}
**Lambda expressions**

Lambda expressions are short, anonymous functions using `=>`. They allow passing logic to methods, returning functions, or storing them in collections.

- Lambdas can capture outer variables (closures), retaining access even outside their original scope.
- Used extensively in LINQ for filtering, aggregation, and projection.

**Learning goals:**

- Creating lambda expressions as `Func<T>` delegates.
- Understanding variable capture.
- Generating random numbers and making probability-based decisions.
{{% /hint %}}

### Task Description

Implement the method:

```csharp
public static void Fill(List<int> collection, int length, Func<int> generator);
```

- The method should add `length` elements to `collection`.
- Each element is produced by calling `generator`.

Then, using `Fill`, generate and print:

- 10 elements of an arithmetic sequence starting at 3 with a difference of 8.
- 10 elements of the Fibonacci sequence.
- 10 random integers in the range `[5, 50]`.
- 10 elements of `0` or `1` with a given probability (e.g., `P(1) = 0.3`).
- 10 random elements from the first 10 prime numbers `[2, 3, 5, 7, 11, 13, 17, 19, 23, 29]`.
- A Markov chain of length 20 starting in state `1`, defined by the transition matrix:

|     | `1` | `2` | `3` |
| --- | --- | --- | --- |
| `1` | 0.1 | 0.6 | 0.3 |
| `2` | 0.4 | 0.2 | 0.4 |
| `3` | 0.5 | 0.3 | 0.2 |

> For example, for row 2 and column 1, transition from state `3` to `1` has probability `0.5`.
>
> You can represent the transition table as a dictionary of lists of `(state, probability)` tuples.

{{% hint info %}}
**Resources:**

- [Microsoft Learn: Lambda expressions and anonymous functions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Microsoft Learn: Func<T,TResult> Delegate](https://learn.microsoft.com/en-us/dotnet/api/system.func-2?view=net-9.0)
- [Microsoft Learn: Random Class](https://learn.microsoft.com/en-us/dotnet/api/system.random?view=net-9.0)
{{% /hint %}}

### Example Solution

An example implementation can be found in the file [`Task03.cs`](/labs/lab04/solution/tasks/Task03.cs).

## Regular Expressions

{{% hint info %}}
**What are regular expressions?**

Regular expressions (aka _regex_) are a powerful tool for searching and manipulating text based on character patterns.

- They allow precise matching of character sequences in text.
- They enable grouping and capturing parts of the matched text for further processing.
- They are supported in many programming languages, including C#, where they are provided by the `Regex` class.

Although regex-based processing can be less efficient than dedicated algorithms (e.g., character-by-character parsing), their biggest advantage is the conciseness and clarity of code - the entire matching and extraction logic can be written in a single pattern and executed by the regex engine. This results in cleaner, shorter, and more maintainable code.

**Learning goals:**

- How to create and use regular expressions in C# to parse complex text formats such as server logs.
- How to define named capture groups in your regex to conveniently extract relevant data.
{{% /hint %}}

### Task Description

Write a program that, given the text file [logs.txt](/labs/lab04/logs.txt) containing logs in the following format:

```
[YYYY-MM-DD HH:mm:ss] LEVEL: IP - METHOD /api/RESOURCE/ID - HTTP_CODE HTTP_STATUS[: optional message]
```

Use a regular expression with named capture groups to extract the following fields from each log entry:

- `LEVEL`: the log level,
- `RESOURCE`: the resource name,
- `ID`: the resource identifier,
- `HTTP_CODE`: the HTTP response code,
- `HTTP_STATUS`: the HTTP status text.

Map the extracted information into a collection of `LogEntry` records, then print them to the console in the format:

```
LEVEL: RESOURCE/ID => HTTP_CODE HTTP_STATUS
```

**Helper type definitions:**

```csharp
public record LogEntry(
	string Level,
	string Resource,
	string Id,
	int HttpCode,
	string HttpStatus
);
```

{{% hint warning %}}
**Notes**

- This task is considered optional (due to its higher difficulty and complexity).
{{% /hint %}}

{{% hint info %}}
**Resources:**

- [Microsoft Learn: Regex Class](https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex?view=net-9.0)
- [Microsoft Learn: .NET regular expressions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions)
- [Microsoft Learn: Regular Expression Language - Quick Reference](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)
- [regex101.com](https://regex101.com/)
{{% /hint %}}

### Example Solution

An example implementation can be found in the file [`Task04.cs`](/labs/lab04/solution/tasks/Task04.cs).
