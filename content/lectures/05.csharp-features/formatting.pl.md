---
title: "Formatowanie"
---

# Formatowanie

Wszystkie typy w .NET posiadają metodę `ToString()`, ale czasami jej podstawowa wersja jest niewystarczająca. Czasami potrzebujemy mechanizmu, który pozwoli precyzyjnie kontrolować tekstową reprezentację obiektu - na przykład, aby wyświetlić liczbę jako walutę, albo sformatować datę według określonego wzorca.

Do tego właśnie służy system formatowania oparty o interfejs `IFormattable`.

## Interfejs `IFormattable`

Jest on zaimplementowany przez wszystkie typy proste, numeryczne, oraz `DateTime`. Definiuje on jedną wersję metody `ToString`:

```csharp
public interface IFormattable
{
    string ToString(string format, IFormatProvider formatProvider);
}
```

* `format`: To **ciąg formatujący**, czyli zestaw kodów, które określają pożądany format, np. `"C"` dla waluty, `"X"` dla wartości szesnastkowej, czy `"d"` dla krótkiej daty.
* `formatProvider`: To **dostawca formatu**, który dostarcza reguł specyficznych dla danego regionu, takich jak separator dziesiętny, symbol waluty, czy nazwy dni tygodnia.

Biblioteka standardowa dostarcza klas, które zarządzają regułami formatowania. Najważniejszą z nich jest `CultureInfo`. Dostarcza ona informacji specyficznych dla danej kultury, takich jak:
- `NumberFormatInfo`: Informacje o formatowaniu liczb (np. separator dziesiętny, symbol waluty).
- `DateTimeFormatInfo`: Informacje o formatowaniu dat i czasu (np. nazwy dni, format daty).

### Przykład

```csharp
using System.Globalization;

var price = 1250.50m;

var plCulture = new CultureInfo("pl-PL");
var usCulture = new CultureInfo("en-US");

// The same "C" format string produces different results based on the provider
Console.WriteLine(price.ToString("C", plCulture)); // Output: 1 250,50 zł
Console.WriteLine(price.ToString("C", usCulture)); // Output: $1,250.50
```

Ten sam mechanizm działa również wewnątrz **interpolowanych stringów**. Po zmiennej wystarczy dodać dwukropek i podać ciąg formatujący. Dostawcą formatu jest wtedy `CultureInfo.CurrentCulture`.

```csharp
var price = 1250.50m;
var date = new DateTime(2025, 12, 25);

// Assuming current culture is pl-PL
Console.WriteLine($"Price: {price:C}"); // Output: Price: 1 250,50 zł
Console.WriteLine($"Date: {date:D}");   // Output: Date: 25 grudnia 2025
```

> Od .NETa 6 możemy też interpolować `string`i przy użyciu wybranego dostawcy formatu, z pomocą statycznej metody `string.Create`:
> 
> ```csharp
> string.Create(culture, $"{date,23}{number,20:N3}");
> ```

### `NumberFormatInfo` i `DateTimeFormatInfo`

Sama klasa `CultureInfo` jest w rzeczywistości kontenerem. Właściwe reguły formatowania dla liczb i dat przechowywane są w dwóch osobnych obiektach, które można z niej pobrać:
* `CultureInfo.NumberFormat`: Zwraca obiekt `NumberFormatInfo`, który definiuje m.in. symbol waluty, separator dziesiętny i grupujący tysiące.
* `CultureInfo.DateTimeFormat`: Zwraca obiekt `DateTimeFormatInfo`, który definiuje np. nazwy dni tygodnia, miesięcy i wzorce dat.

Można modyfikować te obiekty, aby stworzyć niestandardowe reguły formatowania, zachowując resztę ustawień z danej kultury.

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

Istnieje też specjalny dostawca formatu – `CultureInfo.InvariantCulture`. Jest to niezmienna kultura, oparta na języku angielskim, która **gwarantuje identyczne formatowanie na każdym komputerze**, niezależnie od jego ustawień regionalnych.

Jest to przydatne, gdy dane są przeznaczone do przetwarzania maszynowego, a nie dla człowieka. Używamy jej do:
* Zapisywania danych w plikach konfiguracyjnych, JSON, XML, CSV.
* Przesyłania danych przez sieć do API.

```csharp
using System.Globalization;

var date = new DateTime(2025, 12, 30, 22, 05, 10);
var invariant = CultureInfo.InvariantCulture;

Console.WriteLine(date.ToString("o", invariant)); // Output: 2025-12-30T22:05:10.0000000
```

> Format "o" (round-trip) w połączeniu z `InvariantCulture` daje pewność, że tak zapisany `string` może być z powrotem odczytany do obiektu `DateTime` na dowolnym systemie bez utraty precyzji.

## Implementacja `IFormattable`

Implementacja interfejsu `IFormattable` dla własnych typów dodaje do tego typu możliwość formatowania jego wartości do `string`a, podobnie jak ma to miejsce w typach wbudowanych.

Na przykład, dla typu `Temperature` można zdefiniować formaty:
* `"C"` - stopnie Celsjusza
* `"F"` - stopnie Fahrenheita
* `"K"` - Kelwiny 

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

Dla takiego typu można podawać format przy interpolacji stringów:

```csharp
Temperature temp = new Temperature(0.0m);
Console.WriteLine($"The water freezes in {temp:C}");
Console.WriteLine($"The water freezes in {temp:F}");
Console.WriteLine($"The water freezes in {temp:K}");
```

## Formatowanie liczb

Do formatowania liczb możemy używać specyfikatorów standardowych (jeden znak) lub tworzyć własne, niestandardowe wzorce.

### Standardowe formaty liczbowe

To najczęściej używane formaty, które pokrywają większość typowych zastosowań.

| Specyfikator | Nazwa            | Przykład                    | Wynik (`en-US`) |
| :----------- | :--------------- | :-------------------------- | :-------------- |
| `C`          | Waluta           | `(1234.567).ToString("C2")` | `$1,234.57`     |
| `D`          | Dziesiętny       | `(123).ToString("D5")`      | `00123`         |
| `F`          | Stałoprzecinkowy | `(1234.567).ToString("F2")` | `1234.57`       |
| `N`          | Liczba           | `(1234.567).ToString("N2")` | `1,234.57`      |
| `P`          | Procent          | `(0.123).ToString("P1")`    | `12.3 %`        |
| `X`          | Szesnastkowy     | `(255).ToString("X")`       | `FF`            |

> [!NOTE]
> To tylko najważniejsze specyfikatory. Pełna, wyczerpująca lista znajduje się w [oficjalnej dokumentacji Microsoftu](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings).

### Własne formaty liczbowe

Można również tworzyć własne formaty, łącząc ze sobą specjalne znaki.

| Specyfikator | Opis                                             | Przykład                        | Wynik (`en-US`) |
| :----------- | :----------------------------------------------- | :------------------------------ | :-------------- |
| `0`          | Wyświetla cyfrę lub zero, jeśli jej brak.        | `(123.45).ToString("0000.000")` | `0123.450`      |
| `#`          | Wyświetla cyfrę lub nic, jeśli jej brak.         | `(123.45).ToString("####.###")` | `123.45`        |
| `.`          | Separator dziesiętny.                            | `(1234).ToString("0.00")`       | `1234.00`       |
| `,`          | Separator tysięcy.                               | `(1234567).ToString("#,##0")`   | `1,234,567`     |
| `%`          | Mnoży liczbę przez 100 i dodaje symbol `%`.      | `(0.123).ToString("0.0 %")`     | `12.3 %`        |

> [!NOTE]
> To tylko najważniejsze specyfikatory. Pełna, wyczerpująca lista znajduje się w [oficjalnej dokumentacji Microsoftu](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings).

## Formatowanie daty i czasu

Podobnie jak przy liczbach, mamy do dyspozycji formaty standardowe i niestandardowe.

### Standardowe formaty daty i czasu

| Specyfikator | Nazwa            | Przykład (dla `2025-11-23 14:30:00`) | Wynik (`en-US`)               |
| :----------- | :--------------- | :----------------------------------- | :---------------------------- |
| `d`          | Krótka data      | `date.ToString("d")`                 | `11/23/2025`                  |
| `D`          | Długa data       | `date.ToString("D")`                 | `Sunday, November 23, 2025`   |
| `t`          | Krótki czas      | `date.ToString("t")`                 | `2:30 PM`                     |
| `T`          | Długi czas       | `date.ToString("T")`                 | `2:30:00 PM`                  |
| `g`          | Ogólny (krótki)  | `date.ToString("g")`                 | `11/23/2025 2:30 PM`          |
| `o`          | Round-trip (ISO) | `date.ToString("o")`                 | `2025-11-23T14:30:00.0000000` |

> [!NOTE]
> To tylko najważniejsze specyfikatory. Pełna, wyczerpująca lista znajduje się w [oficjalnej dokumentacji Microsoftu](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings).

### Własne formaty daty i czasu

| Specyfikator | Opis                          | Przykład (dla `2025-11-23 14:30:05`)  | Wynik             |
| :----------- | :---------------------------- | :------------------------------------ | :---------------- |
| `dd`         | Dzień miesiąca (01-31).       | `date.ToString("dd")`                 | `23`              |
| `dddd`       | Pełna nazwa dnia tygodnia.    | `date.ToString("dddd")`               | `Sunday`          |
| `MM`         | Miesiąc (01-12).              | `date.ToString("MM")`                 | `11`              |
| `MMMM`       | Pełna nazwa miesiąca.         | `date.ToString("MMMM")`               | `November`        |
| `yyyy`       | Rok (cztery cyfry).           | `date.ToString("yyyy")`               | `2025`            |
| `HH`         | Godzina (00-23).              | `date.ToString("HH")`                 | `14`              |
| `mm`         | Minuty (00-59).               | `date.ToString("mm")`                 | `30`              |
| `ss`         | Sekundy (00-59).              | `date.ToString("ss")`                 | `05`              |
| `:`          | Separator czasu.              | `date.ToString("HH:mm:ss")`           | `14:30:05`        |
| `/`          | Separator daty.               | `date.ToString("MM/dd/yyyy")`         | `11/23/2025`      |

```csharp
DateTime date = new DateTime(2025, 11, 23, 14, 30, 5);
Console.WriteLine(date.ToString("dddd, dd MMMM yyyy")); // Output: Sunday, 23 November 2025
Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss")); // Output: 2025-11-23 14:30:05
```

> [!NOTE]
> To tylko najważniejsze specyfikatory. Pełna, wyczerpująca lista znajduje się w [oficjalnej dokumentacji Microsoftu](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).