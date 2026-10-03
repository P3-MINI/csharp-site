---
title: "MSBuild"
weight: 20
---

## Tutorial 2: MSBuild, .NET CLI i testy jednostkowe

Podczas tworzenia większych aplikacji rzadko pracujemy z pojedynczym plikiem źródłowym. Większe aplikacje mogą się składać z kilku projektów, takich jak biblioteki, aplikacje wykonywalne oraz projekty zawierające testy. Potrzebujemy więc narzędzi, które pozwolą nam zarządzać strukturą projektu oraz procesem budowania.

W narzędziach .NET korzystamy między innymi z:
- .NET CLI - narzędzie umożliwiające tworzenie, budowanie, uruchamianie i testowanie projektów
- MSBuild - system odpowiedzialny za proces budowania projektów
- solucja (rozwiązanie) - plik grupujący powiązane ze sobą projekty
- NuGet - menedżer pakietów dla .NET

## Projekty i solucja

Większa aplikacja .NET może składać się z kilku oddzielnych projektów. Każdy projekt możemy potraktować jako pojedynczy element większej aplikacji. Jeden projekt może odpowiadać na przykład za logikę programu, a drugi za testy jednostkowe. Każdy z tych projektów posiada własny plik `.csproj`, który opisuje między innymi sposób budowania projektu oraz jego zależności.

Do pogrupowania projektów należących do jednej aplikacji służy **solucja**. Solucja określa, które projekty są ze sobą logicznie powiązane, ale sama nie definiuje zależności pomiędzy nimi. 
## .NET CLI

Podczas pracy z .NET nie jesteśmy ograniczeni do akcji dostępnych w Visual Studio czy Riderze. Często aplikacje budowane są w środowiskach serwerowych bez interfejsu graficznego (np. w procesach CI/CD (ang. *Continous Integragion and Continous Delivery*)). Z tego powodu musisz wiedzieć, jak wykonać te operacje bezpośrednio z terminala. 

Służy do tego **.NET CLI**. Listę dostępnych poleceń możemy wyświetlić za pomocą polecenia `dotnet --help`. Pomoc do poszczególnych poleceń możemy wyświetlić za pomocą komendy: `dotnet <komenda> --help`

Za jego pomocą możemy wykonywać różne operacje na projekcie. Na przykład możemy zbudować projekt za pomocą `dotnet build`, a za pomocą `dotnet run` możemy zbudować i uruchomić naszą aplikację. Z kolei `dotnet test` buduje projekty testowe i uruchamia znajdujące się w nich testy.

W kolejnych częściach laboratorium wykorzystamy .NET CLI między innymi do utworzenia solucja, dodania do niego kilku projektów, zbudowania aplikacji oraz uruchomienia testów jednostkowych.

### Tworzenie solucji

Nową solucję możemy utworzyć za pomocą polecenia:

```bash
dotnet new sln -n <SolutionName>
```

Polecenie `dotnet new` tworzy nowy element na podstawie jednego z szablonów dostępnych w .NET SDK. W tym przypadku używamy szablonu `sln`, przeznaczonego do tworzenia solucji. 

### Tworzenie biblioteki

Jednym z typów projektów dostępnych w .NET jest **biblioteka**. W przeciwieństwie do aplikacji konsolowej nie jest przeznaczona do samodzielnego uruchomienia. Zawiera kod, który może być wykorzystywany przez inne projekty. Bibliotekę możemy utworzyć w następujący sposób:

```bash
dotnet new classlib -n <LibraryName>.Lib
```

Polecenie utworzy nowy katalog `<LibraryName>.Lib` wraz z plikami:
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

### Dodawanie projektu do Solucji

Utworzenie projektu w tym samym katalogu, co plik rozwiązania, nie powoduje automatycznego dodania go do solucji. 

Projekt trzeba dodać osobno za pomocą komendy:

```bash
dotnet sln add <LibraryName>.Lib
```

Możemy wtedy sprawdzić zawartość solucji:

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

Pamiętaj, żeby dodać ją do solucji:

```bash
dotnet sln add <ProjectName>.App
```

### Referencje między projektami

Jeżeli kod jednego projektu będzie korzystał z klas znajdujących się w innym projekcie, musimy ręcznie zdefiniować taką zależność. 

Aby dodać referencję do innego projektu można skorzystać z:

```bash
dotnet add <Project> reference <ReferencedProject>
```

Wówczas `<Project>` będzie mógł korzystać z kodu znajdującego się w `<ReferencedProject>`.

### Korzystanie z biblioteki w aplikacji konsolowej

Po dodaniu referencji możemy skorzystać w aplikacji konsolowej z publicznych klas znajdujących się w bibliotece.

Na przykład, jeśli biblioteka posiada przestrzeń nazw `GeometryTools.Lib` możemy ją zaimportować na początku naszej aplikacji:

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

Kod źródłowy C# przed uruchomieniem musi zostać skompilowany. Do kompilacji całej solucji służy polecenie `dotnet build`. Jeśli natomiast chcemy skompilować tylko konkretny projekt to możemy skorzystać z `dotnet build <NazwaProjektu>`. 
Podczas budowania uwzględniane są również zależności, dzięki czemu projekty są budowane w odpowiedniej kolejności.

## Zadanie 1 - Konwerter temperatur

W tym zadaniu utworzysz aplikację `TemperatureConverter`, która będzie się składała z 2 projektów: `TemperatureConverter.Lib` oraz `TemperatureConverter.App`. Będzie to aplikacja konwertująca temperaturę z Celsjusza na Fahrenheita.

Wykonaj w tym celu następujące kroki:
- Utwórz solucję `TemperatureConverter` oraz oba projekty i dodaj je do solucji.
- Dodaj odpowiednią referencję do projektu, tak aby aplikacja konsolowa mogła korzystać z biblioteki.
- W bibliotece utwórz klasę `TemperatureUtils` zawierającą publiczną metodę `public static double CelsiusToFahrenheit(double temperature)`, która przelicza temperaturę według wzoru: `F = C * 9/5 + 32`.
- W aplikacji konsolowej wykorzystaj metodę z biblioteki do przeliczenia kilku przykładowych temperatur, np. `-20`, `0`, `20` i `100`.
- Zbuduj całą solucję, a następnie uruchom aplikację konsolową.

>[!Warning]
>Rozwiąż to zadanie zarówno z poziomu terminala, korzystając z .NET CLI, jak i również w wybranym IDE, Takim jak Visual Studio lub Rider.

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

Plik projektu MSBuild może zawierać kilka rodzajów elementów. Najważniejsze z nich to **Properties**, **Items**, **Targets** oraz **Tasks**. 

### Properties

**Properties** przechowują pojedyncze wartości wykorzystywane podczas procesu budowania. Grupujemy je wewnątrz elementu `PropertyGroup`. 

Przykładowo:

```xml
<PropertyGroup>
    <ApplicationName>GeometryTools</ApplicationName>
</PropertyGroup>
```

Do wartości property możemy odwołać się za pomocą `$(NazwaProperty)`.

**Properties** spotkaliśmy już wcześniej w pliku `.csproj`. Na przykład:

```xml
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
</PropertyGroup>
```

### Items

**Items** służą do reprezentowania zbiorów elementów wykorzystywanych podczas budowania. Najczęściej są to na przykład pliki lub zależności projektu.

Do zbioru **Items** odwołujemy się za pomocą:
```
@(NazwaItemu)
```

Skorzystaliśmy już z **Itemów** przy dodawaniu referencji do projektu:
```xml
  <ItemGroup>
    <ProjectReference Include="..\GeometryTools.Lib\GeometryTools.Lib.csproj" />
  </ItemGroup>
```
### Targets

**Target** opisuje konkretny etap procesu wykonywanego przez MSBuild. Projekt może zawierać wiele takich **Targetów**, z których każdy odpowiada za określone zadanie. **Targety** mogą również deklarować zależności między sobą:
```xml
<Target Name="Prepare">
    <Message Text="Preparing..." />
</Target>

<Target Name="BuildApplication" DependsOnTargets="Prepare">
    <Message Text="Building..." />
</Target>
```

Uruchomienie `BuildApplication` spowoduje najpierw wykonanie **Targetu** `Prepare`. Aby uruchomić target możemy skorzystać z komendy `dotnet msbuild -target:BuildApplication`.

### Tasks

Sam **Target** określa jedynie **etap procesu**. Konkretne czynności wykonywane podczas tego etapu nazywane są **Taskami**. Przykładowo, jeśli chcemy utworzyć katalog i wyświetlić informację o zakończeniu tej operacji możemy zrobić to w następujący sposób:
```xml
<Target Name="Prepare">
    <MakeDir Directories="build" />
    <Message Text="Build directory prepared" />
</Target>
```

W tym przypadku `Prepare` jest targetem, natomiast `MakeDir` i `Message` będą Taskami. 
Oto kilka najpopularniejszych Tasków MSBuild:
```xml
<Message />   <!-- wypisanie komunikatu -->
<MakeDir />   <!-- utworzenie katalogu -->
<Copy />      <!-- kopiowanie plików -->
<Delete />    <!-- usuwanie plików -->
```

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

Aktualnie używana konfiguracja jest dostępna w MSBuild jako property:

```xml
$(Configuration)
```

Atrybut `Condition` pozwala określić, kiedy dany element MSBuild ma zostać użyty.

Na przykład możemy ustawić różne wartości property w zależności od konfiguracji:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <BuildType>Development</BuildType>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <BuildType>Production</BuildType>
</PropertyGroup>
```

### NuGet

Podczas tworzenia oprogramowania rzadko piszemy wszystko od zera. W codziennej pracy programiści często korzystają z gotowych bibliotek rozwiązujących powszechne problemy - takich jak parsowanie plików JSON, logowanie błędów czy komunikacja z bazą danych.

Do zarządzania tymi zewnętrznymi zależnościami służy **NuGet** - oficjalny menedżer pakietów dla platformy .NET. W centralnym repozytorium [nuget.org](https://www.nuget.org/) znajdują się dziesiątki tysięcy gotowych, darmowych pakietów (np. Newtonsoft.Json, Serilog, czy EntityFramework), które możemy w kilka sekund dołączyć do naszego projektu.

Pakiet możemy dodać do projektu za pomocą:
```bash
dotnet add <Project> package <PackageName>
```

Po dodaniu pakietu w pliku `.csproj` pojawi się wpis podobny do:
```xml
<ItemGroup>
    <PackageReference Include="<PackageName>" Version="<PackageVersion>" />
</ItemGroup>
```

`PackageReference` oznacza zależność projektu od zewnętrznego pakietu NuGet.

## Testy jednostkowe

Ostatnim rodzajem projektu, z którym będziemy pracować, jest projekt zawierający **testy jednostkowe**. Testy pozwalają automatycznie sprawdzić, czy poszczególne fragmenty naszego programu działają zgodnie z oczekiwaniami.

Test jednostkowy sprawdza niewielką część aplikacji - najczęściej pojedynczą metodę lub klasę. Dzięki temu po zmianie kodu możemy szybko sprawdzić, czy wcześniej działające funkcjonalności nadal działają poprawnie.

W .NET możemy korzystać z kilku frameworków do tworzenia testów jednostkowych, między innymi:
- MSTest
- NUnit
- xUnit
W tym laboratorium będziemy korzystać z **MSTest.**

Projekt testowy jest budowany podobnie jak zwykła biblioteka. Powstały w ten sposób projekt jest później wejściem dla *test runnera*, który wyszukuje w takiej bibliotece metody oznaczone atrybutem `[TestMethod]` i je uruchamia.

### Tworzenie projektu testowego

Do istniejącej solucji GeometryTools dodamy trzeci projekt, a następnie dodamy go do solucji i utworzymy referencję za pomocą komend:

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
	    //Arrange
	    double width = 5;
	    double height = 4;
    
	    //Act
        double result = RectangleUtils.CalculateArea(width, height);

		//Assert
        Assert.AreEqual(20, result);
    }
}
```
Test można uruchomić poleceniem `dotnet test`

Polecenie najpierw zbuduje odpowiednie projekty, a następnie uruchomi znalezione testy. 

Jeżeli oczekiwana wartość jest zgodna z wynikiem działania metody, test zostanie oznaczony jako zakończony powodzeniem. Jeśli ten warunek nie zostanie spełniony, test zakończy się błędem.

Dobry test jednostkowy jest pisany według prostego schematu **Arrange-Act-Assert (AAA)**:
1. Arrange: Przygotowujesz warunki i dane wejściowe.
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

Rozszerz solucję `TemperatureConverter` z poprzedniego zadania o projekt zawierający testy jednostkowe.

- Utwórz projekt `TemperatureConverter.Tests` wykorzystujący `MSTest` i dodaj go do rozwiązania.
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

>[!Warning]
>Rozwiąż to zadanie zarówno z poziomu terminala, korzystając z .NET CLI, jak i również w wybranym IDE, Takim jak Visual Studio lub Rider.