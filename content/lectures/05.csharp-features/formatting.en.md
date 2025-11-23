---
title: "Formatting"
---

# Formatting

All types in .NET have a `ToString()` method, but sometimes its basic implementation is insufficient. Sometimes we need a mechanism that allows for precise control over the textual representation of an object-for example, to display a number as currency or to format a date according to a specific pattern.

This is what the formatting system based on the `IFormattable` interface is for.

## The `IFormattable` Interface

It is implemented by all primitive types, numeric types, and `DateTime`. It defines one overload of the `ToString` method:

```csharp
public interface IFormattable
{
    string ToString(string format, IFormatProvider formatProvider);
}
```

*   `format`: This is a **format string**, a set of codes that specify the desired format, e.g., `"C"` for currency, `"X"` for a hexadecimal value, or `"d"` for a short date.
*   `formatProvider`: This is a **format provider**, which supplies region-specific rules, such as the decimal separator, currency symbol, or the names of the days of the week.

The standard library provides classes that manage formatting rules. The most important of these is `CultureInfo`. It provides culture-specific information, such as:
- `NumberFormatInfo`: Information about number formatting (e.g., decimal separator, currency symbol).
- `DateTimeFormatInfo`: Information about date and time formatting (e.g., day names, date patterns).

### Example

```csharp
using System.Globalization;

var price = 1250.50m;

var plCulture = new CultureInfo("pl-PL");
var usCulture = new CultureInfo("en-US");

// The same "C" format string produces different results based on the provider
Console.WriteLine(price.ToString("C", plCulture)); // Output: 1 250,50 zł
Console.WriteLine(price.ToString("C", usCulture)); // Output: $1,250.50
```

The same mechanism also works inside **interpolated strings**. You just need to add a colon and the format string after the variable. The format provider is then `CultureInfo.CurrentCulture`.

```csharp
var price = 1250.50m;
var date = new DateTime(2025, 12, 25);

// Assuming current culture is pl-PL
Console.WriteLine($"Price: {price:C}"); // Output: Price: 1 250,50 zł
Console.WriteLine($"Date: {date:D}");   // Output: Date: 25 grudnia 2025
```

> Starting from .NET 6 we can interpolate `string`s using chosen format provider, using static helper method `string.Create`:
> 
> ```csharp
> string.Create(culture, $"{date,23}{number,20:N3}");
> ```

### `NumberFormatInfo` and `DateTimeFormatInfo`

The `CultureInfo` class itself is actually a container. The actual formatting rules for numbers and dates are stored in two separate objects that can be retrieved from it:
* `CultureInfo.NumberFormat`: Returns a `NumberFormatInfo` object, which defines, among other things, the currency symbol, decimal separator, and thousand-grouping separator.
* `CultureInfo.DateTimeFormat`: Returns a `DateTimeFormatInfo` object, which defines things like the names of the days of the week, months, and date patterns.

You can modify these objects to create custom formatting rules while preserving the rest of the settings from a given culture.

```csharp
using System.Globalization;

var price = 1250.50m;
var pl = new CultureInfo("pl-PL");

// Mutable copy of NumberFormatInfo
var customPl = (NumberFormatInfo)pl.NumberFormat.Clone();
customPl.CurrencySymbol = "PLN";
customPl.CurrencyDecimalSeparator = ".";

Console.WriteLine(price.ToString("C", customPl)); // Output: 1 250.50 PLN
```

### `CultureInfo.InvariantCulture`

There is also a special format provider – `CultureInfo.InvariantCulture`. It is an immutable culture, based on English, which **guarantees identical formatting on every computer**, regardless of its regional settings.

This is useful when data is intended for machine processing rather than for a human. We use it for:
*   Saving data in configuration files, JSON, XML, CSV.
*   Transmitting data over the network to an API.

```csharp
using System.Globalization;

var date = new DateTime(2025, 12, 30, 22, 05, 10);
var invariant = CultureInfo.InvariantCulture;

Console.WriteLine(date.ToString("o", invariant)); // Output: 2025-12-30T22:05:10.0000000
```

> The "o" (round-trip) format combined with `InvariantCulture` ensures that a string saved this way can be parsed back into a `DateTime` object on any system without loss of precision.

## Implementing `IFormattable`

Implementing the `IFormattable` interface for your own types adds the ability to format their values into a `string`, similar to how it works with built-in types.

For example, for a `Temperature` type, you could define the following formats:
* `"C"` - degrees Celsius
* `"F"` - degrees Fahrenheit
* `"K"` - Kelvin 

```csharp
using System.Globalization;

public class Temperature : IFormattable
{
    private decimal temp;

    public Temperature(decimal temperature)
    {
        if (temperature < -273.15m)
            throw new ArgumentOutOfRangeException($"{temperature} is less than absolute zero.");
        this.temp = temperature;
    }

    public decimal Celsius => temp;

    public decimal Fahrenheit => temp * 9 / 5 + 32;

    public decimal Kelvin => temp + 273.15m;

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public string ToString(string format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public string ToString(string format, IFormatProvider provider)
    {
        if (String.IsNullOrEmpty(format)) format = "G";
        if (provider == null) provider = CultureInfo.CurrentCulture;

        switch (format.ToUpperInvariant())
        {
            case "G":
            case "C":
                return temp.ToString("F2", provider) + " °C";
            case "F":
                return Fahrenheit.ToString("F2", provider) + " °F";
            case "K":
                return Kelvin.ToString("F2", provider) + " K";
            default:
                throw new FormatException($"The {format} format string is not supported.");
        }
    }
}
```

For such a type, you can provide the format during string interpolation:

```csharp
Temperature temp = new Temperature(0.0m);
Console.WriteLine($"The water freezes in {temp:C}");
Console.WriteLine($"The water freezes in {temp:F}");
Console.WriteLine($"The water freezes in {temp:K}");
```

## Number Formatting

To format numbers, we can use standard format specifiers (a single character) or create our own custom patterns.

### Standard Numeric Formats

These are the most commonly used formats that cover most typical use cases.

| Specifier   | Name            | Example                      | Result (`en-US`) |
| :---------- | :-------------- | :--------------------------- | :--------------- |
| `C`         | Currency        | `(1234.567).ToString("C2")`  | `$1,234.57`      |
| `D`         | Decimal         | `(123).ToString("D5")`       | `00123`          |
| `F`         | Fixed-point     | `(1234.567).ToString("F2")`  | `1234.57`        |
| `N`         | Number          | `(1234.567).ToString("N2")`  | `1,234.57`       |
| `P`         | Percent         | `(0.123).ToString("P1")`     | `12.3 %`         |
| `X`         | Hexadecimal     | `(255).ToString("X")`        | `FF`             |

> [!NOTE]
> These are only the most important specifiers. A full, comprehensive list can be found in the [official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings).

### Custom Numeric Formats

You can also create your own formats by combining special characters.

| Specifier   | Description                                         | Example                         | Result (`en-US`) |
| :---------- | :-------------------------------------------------- | :------------------------------ | :--------------- |
| `0`         | Displays a digit or a zero if the digit is absent.  | `(123.45).ToString("0000.000")` | `0123.450`       |
| `#`         | Displays a digit or nothing if the digit is absent. | `(123.45).ToString("####.###")` | `123.45`         |
| `.`         | Decimal separator.                                  | `(1234).ToString("0.00")`       | `1234.00`        |
| `,`         | Thousands separator.                                | `(1234567).ToString("#,##0")`   | `1,234,567`      |
| `%`         | Multiplies the number by 100 and adds a `%` symbol. | `(0.123).ToString("0.0 %")`     | `12.3 %`         |

> [!NOTE]
> These are only the most important specifiers. A full, comprehensive list can be found in the [official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings).

## Date and Time Formatting

Similar to numbers, we have standard and custom formats at our disposal.

### Standard Date and Time Formats

| Specifier   | Name            | Example (for `2025-11-23 14:30:00`) | Result (`en-US`)              |
| :---------- | :-------------- | :---------------------------------- | :---------------------------- |
| `d`         | Short date      | `date.ToString("d")`                | `11/23/2025`                  |
| `D`         | Long date       | `date.ToString("D")`                | `Sunday, November 23, 2025`   |
| `t`         | Short time      | `date.ToString("t")`                | `2:30 PM`                     |
| `T`         | Long time       | `date.ToString("T")`                | `2:30:00 PM`                  |
| `g`         | General (short) | `date.ToString("g")`                | `11/23/2025 2:30 PM`          |
| `o`         | Round-trip (ISO)| `date.ToString("o")`                | `2025-11-23T14:30:00.0000000` |

> [!NOTE]
> These are only the most important specifiers. A full, comprehensive list can be found in the [official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings).

### Custom Date and Time Formats

| Specifier   | Description                       | Example (for `2025-11-23 14:30:05`) | Result            |
| :---------- | :-------------------------------- | :---------------------------------- | :---------------- |
| `dd`        | Day of the month (01-31).         | `date.ToString("dd")`               | `23`              |
| `dddd`      | Full name of the day of the week. | `date.ToString("dddd")`             | `Sunday`          |
| `MM`        | Month (01-12).                    | `date.ToString("MM")`               | `11`              |
| `MMMM`      | Full name of the month.           | `date.ToString("MMMM")`             | `November`        |
| `yyyy`      | Year (four digits).               | `date.ToString("yyyy")`             | `2025`            |
| `HH`        | Hour (00-23).                     | `date.ToString("HH")`               | `14`              |
| `mm`        | Minutes (00-59).                  | `date.ToString("mm")`               | `30`              |
| `ss`        | Seconds (00-59).                  | `date.ToString("ss")`               | `05`              |
| `:`         | Time separator.                   | `date.ToString("HH:mm:ss")`         | `14:30:05`        |
| `/`         | Date separator.                   | `date.ToString("MM/dd/yyyy")`       | `11/23/2025`      |

```csharp
DateTime date = new DateTime(2025, 11, 23, 14, 30, 5);
Console.WriteLine(date.ToString("dddd, dd MMMM yyyy")); // Output: Sunday, 23 November 2025
Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss")); // Output: 2025-11-23 14:30:05
```

> [!NOTE]
> These are only the most important specifiers. A full, comprehensive list can be found in the [official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).
