---
title: "MSBuild"
weight: 20
---

## MSBuild

Podczas tworzenia większych aplikacji rzadko pracujemy z pojedynczym plikiem źródłowym. Większe aplikacje mogą się składać z kilku projektów, takich jak biblioteki, aplikacje wykonywalne oraz projekty zawierające testy. Potrzebujemy więc narzędzi, które pozwolą nam zarządzać strukturą projektu oraz procesem budowania.

W narzędziach .NET korzystamy między innymi z:
- .NET CLI - narzędzie umożliwiające tworzenie, budowanie, uruchamianie i testowanie projektów
- MSBuild - system odpowiedzialny za proces budowania projektów
- solution - plik grupujący powiązane ze sobą projekty
- NuGet - menedżer pakietów dla .NET

## Projekty i solution

Większa aplikacja .NET może składać się z kilku oddzielnych projektów. Każdy projekt możemy potraktować jako pojedynczy element większej aplikacji. Jeden projekt może odpowiadać na przykład za logikę programu, a drugi za testy jednostkowe. Każdy z tych projektów posiada własny plik `.csproj`, który opisuje między innymi sposób budowania projektu oraz jego zależności.

Do pogrupowania projektów, należących do jednej aplikacji, służy **solution**. Solution określa, które projekty są ze sobą logicznie związane, ale samo nie definiuje zależności pomiędzy ich kodem. 
## .NET CLI

Podczas pracy z .NET nie jesteśmy ograniczeni do przycisków dostępnych w Visual Studio czy Riderze. Czasem nawet pracujemy na serwerze, który nie ma zainstalowanego środowiska graficznego. Z tego powodu musisz wiedzieć jak wykonać te operacje bezpośrednio z terminala. 

Służy do tego **.NET CLI**. Listę dostępnych poleceń możemy wyświetlić za pomocą polecenia `dotnet --help`. Pomoc do poszczególnych poleceń możemy wyświetlić za pomocą komendy: `dotnet <komenda> --help`

Za jego pomocą możemy wykonywać różne operacje na projekcie. Na przykład możemy zbudować projekt za pomocą `dotnet build`, a za pomocą `dotnet run` możemy zbudować i uruchomić naszą aplikację. Z kolei `dotnet test` buduje projekty testowe i uruchamia znajdujące się w nich testy.

W kolejnych częściach laboratorium wykorzystamy .NET CLI między innymi do utworzenia solution, dodania do niego kilku projektów, zbudowania aplikacji oraz uruchomienia testów jednostkowych.

### Tworzenie solution

Nowe solution możemy utworzyć za pomocą polecenia:

```bash
dotnet new sln -n <SolutionName>
```

Polecenie `dotnet new` tworzy nowy element na podstawie jednego z szablonów dostępnych w .NET SDK. W tym przypadku używamy szablonu `sln`, przeznaczonego do tworzenia solution. 

### Tworzenie biblioteki

Jednym z typów projektów dostępnych w .NET jest **biblioteka**. W przeciwieństwie do aplikacji konsolowej nie jest przeznaczona do samodzielnego uruchomienia. Zawiera kod, który może być wykorzystywany przez inne projekty. Bibliotekę możemy utworzyć w następujący sposób:
```bash
dotnet new classlib -n <LibraryName>.Lib
```
Polecenie utworzy nowy katalog `<Libraryname>.Lib` wraz z plikami:
`<LibraryName>.Lib.csproj` oraz `Class1.cs`.
`Class1.cs` jest przykładową klasą wygenerowaną przez szablon.

### Implementacja biblioteki

Biblioteka powinna zawierać kod, który może zostać wykorzystany niezależnie przez inne części aplikacji. 
Utworzony wcześniej projekt biblioteczny zawiera domyślny plik `Class1.cs`. Jest on jedynie elementem szablonu, dlatego możemy go usunąć i zastąpić klasą odpowiadającą potrzebom naszej aplikacji.
Przykładowo możemy utworzyć klasę odpowiedzialną za podstawowe operacje na prostokątach:

```csharp
namespace GeometryTools.Lib; 
public static class RectangleUtils 
{ 
	public static double CalculateArea(double width, double height)
	{ 
		return width * height; 
	} 
}
```

Elementy, które mają być dostępne z innych projektów, muszą być odpowiednio udostępnione, np. przez modyfikator `public`.

Poprawność kompilacji biblioteki możemy sprawdzić za pomocą:

```bash
dotnet build GeometryTools.Lib
```

### Dodawanie projektu do Solution

Utworzenie projektu obok pliku solution nie powoduje automatycznego dodania go do solution. 

Projekt trzeba dodać osobno za pomocą komendy:

```bash
dotnet sln add <LibraryName>.Lib
```

Możemy wtedy sprawdzić zawartość solution:

```bash
dotnet sln list
```

Wówczas na liście powinien pojawić się:

```
<LibraryName>.Lib
```

### Tworzenie projektu aplikacji konsolowej

Aby utworzyć aplikację konsolową możesz skorzystać z:

```bash
dotnet new console -n <ProjectName>.App
```

Pamiętaj, żeby dodać ją do solution:

```bash
dotnet sln add <ProjectName>.App
```

### Referencje między projektami

Jeżeli kod jednego projektu będzie korzystał z klas znajdujących się w innym projekcie musimy ręcznie zdefiniować taką zależność. 

Aby dodać referencję pomiędzy tymi projektami można skorzystać z:

```bash
dotnet add <Project> reference <ReferencedProject>
```

Wówczas `<Project>` będzie mógł korzystać z kodu znajdującego się w `<ReferencedProject>`.

### Korzystanie z biblioteki w aplikacji konsolowej

Po dodaniu referencji możemy skorzystać w aplikacji konsolowej z publicznych klas znajdujących się w bibliotece.

Na przykład jeśli biblioteka posiada przestrzeń nazw `GeometryTools.Lib` możemy ją zaimportować na początku naszej aplikacji:

```cs
using GeometryTools.Lib;
```

Możemy następnie skorzystać z metod udostępnianych przez bibliotekę:

```csharp
double area = RectangleUtils.CalculateArea(5,4);
Console.WriteLine(area);
```

Aplikację możemy następnie uruchomić za pomocą:

```bash
dotnet run --project GeometryTools.App
```

### Budowanie projektu

Kod źródłowy C# przed uruchomieniem musi zostać skompilowany. Do kompilacji całego solution służy polecenie `dotnet build`. Jeśli natomiast chcemy skompilować tylko konkretny projekt to możemy skorzystać z `dotnet build <NazwaProjektu>`. 
Podczas budowania uwzględniane są również zależności, dzięki czemu projekty są budowane w odpowiedniej kolejności.

## Zadanie 1 - Konwerter temperatur

W tym zadaniu utworzysz aplikację `TemperatureConverter`, która będzie się składała z 2 projektów: `TemperatureConverter.Lib` oraz `TemperatureConverter.App`. Będzie to aplikacja konwertująca temperaturę z Celsjusza na Fahrenheita.

Wykonaj w tym celu następujące kroki:
- Utwórz solution `TemperatureConverter` oraz oba projekty i dodaj je do solution.
- Dodaj odpowiednią referencję między projektami, tak aby aplikacja konsolowa mogła korzystać z biblioteki.
- W bibliotece utwórz klasę `TemperatureUtils` zawierającą publiczną metodę `public static double CelsiusToFahrenheit(double temperature)`, która przelicza temperaturę według wzoru: `F = C * 9/5 + 32`.
- W aplikacji konsolowej wykorzystaj metodę z biblioteki do przeliczenia kilku przykładowych temperatur, np. `-20`, `0`, `20` i `100`.
- Zbuduj całe solution, a następnie uruchom aplikację konsolową.

## MSBuild i pliki `.csproj`

Do tej pory wykonywaliśmy większość operacji za pomocą poleceń `dotnet`. Warto jednak zrozumieć, gdzie przechowywane są informacje o projekcie i w jaki sposób są one wykorzystywane podczas budowania aplikacji.

Każdy projekt .NET posiada własny plik `.csproj`. Jest to plik XML zawierający opis projektu, między innymi jego typ, wykorzystywaną wersję .NET oraz zależności od innych projektów lub pakietów.

Przykładowy plik projektu aplikacji konsolowej może wyglądać następująco:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\GeometryTools.Lib\GeometryTools.Lib.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>

```

Sekcja `ItemGroup` przechowuje elementy powiązane z projektem na przykład `ProjectReference`, natomiast `PropertyGroup` zawiera właściwości projektu. 

Polecenia wykonywane za pomocą .NET CLI często modyfikują właśnie plik `.csproj`. Przykładowo dodanie referencji do innego projektu powoduje dopisanie odpowiedniego `ProjectReference`. 

Za odczytanie pliku `.csproj` i zbudowanie projektu odpowiada **MSBuild**. W praktyce oznacza to, że polecenie `dotnet build` uruchamia MSBuild, który analizuje plik projektu, uwzględnia jego zależności i wykonuje odpowiednie kroki potrzebne do zbudowania aplikacji. 

Plik projektu MSBuild może zawierać kilka rodzajów elementów. Najważniejsze z nich to **properties**, **items**, **targets** oraz **tasks**. 

### Properties

**Properites** przechowują pojedyncze wartości wykorzystywane podczas procesu budowania. Grupujemy je zazwyczaj wewnątrz elementu `PropertyGroup`. 

### Items

### Targets

### Tasks

### Debug i Release

Projekt .NET może być budowany w różnych konfiguracjach. Najczęściej używane są:
`Debug` - służący do pracy nad kodem.
`Release` -  przeznaczony do budowania gotowej wersji programu. 

Projekt możemy zbudować w wybranej konfiguracji za pomocą:
```bash
dotnet build -c Release
```
lub
```bash 
dotnet build -c Debug
```

### NuGet
## Testy jednostkowe

Ostatnim rodzajem projektu, z którym będziemy pracować, jest projekt zawierający **testy jednostkowe**. Testy pozwalają automatycznie sprawdzić, czy poszczególne fragmenty naszego programu działają zgodnie z oczekiwaniami.

Test jednostkowy sprawdza niewielką część aplikacji - najczęściej pojedynczą metodę lub klasę. Dzięki temu po zmianie kodu możemy szybko sprawdzić, czy wcześniej działające funkcjonalności nadal działają poprawnie.

W .NET możemy korzystać z kilku frameworków do tworzenia testów jednostkowych, między innymi:
- MSTest
- NUnit
- xUnit
W tym laboratorium będziemy korzystać z **MSTest.**

Projekt testowy jest budowany podobnie jak zwykła bliblioteka. Powstały w ten sposób projekt jest później wejściem dla *test runnera*, który wyszukuje w takiej bibliotece metody oznaczone atrybutem `[TestMethod]` i je uruchamia.

### Tworzenie projektu testowego

Do istniejącego solution GeometryTools dodamy trzeci projekt, a następnie dodamy go do solution i utworzymy referencję za pomocą komend:

```bash
dotnet new mstest -n GeometryTools.Tests
dotnet sln add GeometryTools.Tests
dotnet add GeometryTools.Tests reference GeometryTools.Lib
```

Zwróć uwagę, że zależność istnieje **od projektu testowego do biblioteki**. Biblioteka nie powinna wiedzieć o istnieniu swoich testów.

### Pierwszy test

W nowo utworzonym projekcie znajdziesz przykładową klasę testową. W MSTest klasa zawierająca testy oznaczana jest atrybutem `[TestClass]`, natomiast poszczególne metody testowe atrybutem `[TestMethod]`.

Możemy utworzyć test dla napisanej wcześniej metody CalculateArea:

```csharp
using GeometryTools.Lib;

namespace GeometryTools.Tests;

[TestClass]
public sealed class RectangleUtilsTests
{
    [TestMethod]
    public void CalculateArea_ValidDimensions_ReturnsCorrectArea()
    {
	    double width = 5;
	    double height = 4;
    
        double result = RectangleUtils.CalculateArea(width, height);

        Assert.AreEqual(20, result);
    }
}
```
Test można uruchomić poleceniem `dotnet test`

Polecenie najpierw zbuduje odpowiednie projekty, a następnie uruchamia znalezione testy. 

Jeżeli oczekiwana wartość jest zgodna z wynikiem działania metody, test zostanie oznaczony jako zakończony powodzeniem. Jeśli ten warunek nie zostanie spełniony, test zakończy się błędem.

Dobry test jednostkowy jest pisany według prostego schematu **Arrange-Act-Assert (AAA)**:
1. Arrange: Przechowujesz warunki i dane wejściowe.
2. Act: Wywołujesz testowaną metodę.
3. Assert: Sprawdzasz, czy wynik jest zgodny z oczekiwaniami.

### Asercje

Do sprawdzania wyniku testu służą **asercje**. Przykładowo:
```csharp
Assert.AreEqual(expected, actual);
Assert.IsTrue(condition);
Assert.IsFalse(condition);
Assert.IsNull(value);
Assert.IsNotNull(value);
```

Jeżeli sprawdzany warunek nie jest spełniony, asercja powoduje niepowodzenie testu.

Warto testować nie tylko typowe przypadki, ale również **przypadki brzegowe**, takie jak 0, puste kolekcje, czy wartości znajdujące się na granicy dopuszczalnego zakresu. 

Nazwa testu powinna możliwie dokładnie opisywać sprawdzany przypadek. Jedną z popularnych konwencji jest:

```text
NazwaMetody_Scenariusz_OczekiwanyWynik
```

Na przykład:
```text
CalculateArea_ValidDimensions_ReturnsCorrectArea
CalculateArea_OneSideIsZero_ReturnsZero
```

Dzięki temu już na podstawie wyniku `dotnet test` możemy łatwo zorientować się, jaki przypadek zakończył się niepowodzeniem.

### Cechy dobrego testu

Dobry test jednostkowy powinien być:
- szybki - testów w projekcie mogą być tysiące, dlatego powinny wykonywać się możliwie szybko,
- niezależny - jeden test nie powinien zależeć od wyników innego testu, 
- powtarzalny - wielokrotne uruchomienie testu dla tych samych warunków powinno zawsze dawać ten sam rezultat,
- prosty - test powinien jasno pokazywać dane wejściowe, wykonywaną operację oraz oczekiwany wynik.

Test jednostkowy nie powinien również powielać logiki testowanej metody. Jeśli na przykład sprawdzamy metodę obliczającą pole prostokąta, nie powinniśmy w teście implementować drugiego algorytmu robiącego dokładnie to samo.

Czasami najpierw zaczyna się od pisania testów jednostkowych, czyli definiowania zachowań funkcji, a dopiero później pisze się implementację testowanych metod, aż do przejścia wszystkich testów. Takie podejście nazywamy _Test Driven Development (TDD)_.

## Zadanie 2 - Testowanie konwertera temperatur

Rozszerz solution `TemperatureConverter` z poprzedniego zadania o projekt zawierający testy jednostkowe.

- Utwórz projekt `TemperatureConverter.Tests` wykorzystujący `MSTest` i dodaj go do solution.
- Dodaj odpowiednią referencję, aby projekt testowy mógł korzystać z `TemperatureConverter.Lib`.
- Utwórz klasę `TemperatureUtilsTests`.
- Napisz testy metody `CelsiusToFahrenheit` dla kilku charakterystycznych temperatur.
- Sprawdź między innymi, czy:
    - `0°C` daje `32°F`,
    - `100°C` daje `212°F`,
    - `-40°C` daje `-40°F`.
- Uruchom wszystkie testy za pomocą `dotnet test`.
- Celowo zmień implementację `CelsiusToFahrenheit`, tak aby była niepoprawna, i sprawdź wynik ponownego uruchomienia testów.
- Przywróć poprawną implementację i upewnij się, że wszystkie testy ponownie przechodzą.