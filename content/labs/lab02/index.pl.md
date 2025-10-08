---
title: "Lab02"
weight: 10
---

# Laboratorium 2: MSBuild, Unit Tests, .NET CLI

## Task 1: MSBuild

Twoim zadaniem jest zmodyfikowanie pliku [`CppProject.proj`](/labs/lab02/start/CppProject/CppProject.proj) w celu dodania następujących funkcjonalności:

> Jeżeli pracujesz na systemie Windows zacznij od pliku [`CppProject.proj`](/labs/lab02/start/CppProjectWindows/CppProject.proj). Będziesz pracował z kompilatorem `cl.exe`. Wszystkie potrzebne Ci narzędzia są dostępne w konsoli dla deweloperów (`Visual Studio Developer Command Prompt`). Jak otworzyć konsolę dla deweloperów znajdziesz w [dokumentacji Visual Studio](https://learn.microsoft.com/visualstudio/ide/reference/command-prompt-powershell).

#### Kod początkowy

{{< filetree dir="labs/lab02/start" >}}

### 1. Konfiguracja `Debug` i `Release`

Celem jest dodanie do projektu wsparcia dla dwóch konfiguracji budowania: `Debug` i `Release`.
- **Debug**: Konfiguracja deweloperska, powinna zawierać symbole debugowania i mieć wyłączone optymalizacje.
- **Release**: Konfiguracja produkcyjna, powinna być zoptymalizowana pod kątem wydajności i nie zawierać symboli debugowania.

**Wymagania:**
- Dodaj właściwość `Configuration`, która domyślnie będzie ustawiona na `Debug`.
- Użyj warunkowych grup właściwości (`PropertyGroup`), aby zdefiniować różne flagi kompilatora (dla symboli debugowania i optymalizacji) w zależności od aktywnej konfiguracji.
- Zmodyfikuj targety, aby używały zdefiniowanych flag; `-O0` i `-g` (`/Od` `/Zi` dla `cl.exe`) dla konfiguracji `Debug`; `-O2` (`/O2` dla `cl.exe`) dla konfiguracji `Release`.
- Zmodyfikuj `OutputPath`, aby pliki wynikowe dla każdej konfiguracji trafiały do osobnych podkatalogów (np. `build/Debug/` i `build/Release/`).

Gdy skończysz zbuduj z lini poleceń aplikację w konfiguracji deweloperskiej i produkcyjnej.

### 2. Kompilacja przyrostowa (Incremental Builds)

Żeby przyspieszyć proces budowania, często implementuje się tzw. kompilację przyrostową. Oznacza to, że kompilowane powinny być tylko te pliki, które zostały zmienione od ostatniej kompilacji. MSBuild realizuje to zadanie poprzez porównywanie sygnatur czasowych plików zdefiniowanych w atrybutach `Inputs` i `Outputs` danego targetu. Jeżeli wszystkie pliki wyjściowe (`Outputs`) są nowsze niż wszystkie pliki wejściowe (`Inputs`), MSBuild pomija wykonanie danego targetu, oszczędzając czas.

**Wymagania:**
- Wykorzystaj atrybuty `Inputs` i `Outputs` w targetach `Compile` i `Link`.
- Target `Compile` dla danego pliku `.cpp` powinien być uruchamiany tylko wtedy, gdy sam plik `.cpp` lub którykolwiek z plików nagłówkowych (`.h`) w projekcie jest nowszy niż odpowiadający mu plik obiektowy (`.o`).
- Target `Link` powinien być uruchamiany tylko wtedy, gdy którykolwiek z plików obiektowych jest nowszy niż plik wykonywalny.

Wyczyść projekt targetem `Clean`, następnie zbuduj go dwa razy targetem `Build`.

### 3. Tworzenie paczki dystrybucyjnej

Celem jest zautomatyzowanie tworzenia paczki `.zip` zawierającej gotową aplikację oraz dodatkowe pliki.

**Wymagania:**
- Dodaj do projektu nowy target o nazwie `CreateDist`.
- Target `CreateDist` powinien zależeć od targetu `Build`.
- Dodaj właściwość `Version` (np. `1.0.0`), która będzie używana w nazwie paczki.
- Dodaj grupę itemów, w której zdefiniujesz item `DistFiles` zawierający `README.md` i `LICENSE`
- W targecie `CreateDist`:
    - Skopiuj plik wykonywalny oraz pliki dystrybucyjne do tymczasowego katalogu.
    - Użyj taska [`ZipDirectory`](https://learn.microsoft.com/visualstudio/msbuild/zipdirectory-task), aby spakować zawartość katalogu tymczasowego.
    - Nazwa wynikowego pliku archiwum powinna mieć format `$(OutputName)-$(Version).zip`.
    - Usuń folder tymczasowy

Wywołaj target `CreateDist` dla konfiguracji `Release`.

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w pliku [CppProject.proj](/labs/lab02/solution/CppProject/CppProject.proj) lub w pliku [CppProject.proj](/labs/lab02/solution/CppProjectWindows/CppProject.proj) jeżeli pracowałeś na Windowsie.

#### CppProject

{{< filetree dir="labs/lab02/solution/CppProject" >}}

#### CppProjectWindows

{{< filetree dir="labs/lab02/solution/CppProjectWindows" >}}

## Task 2: .NET SDK

W ramach drugiego zadania będziemy pracować z projektami w stylu Sdk. Do grupowania projektów, które są ze soba powiązane służą solucje. Solucje nie mają nic wspólnego z MSBuildem, są plikami Visual Studio, ale są też wspierane przez inne IDE. Po otwarciu solucji otworzą się na w IDE wszystkie będące jej częścią projekty. Pracę z plikami solucji z poziomu lini poleceń umożliwia także narzędzie `dotnet`.

Warto zapoznać się z komendą `dotnet`. Opcja `--help` wypisze nam wszystkie dostępne polecenia - zapoznaj się z nimi. Jeżeli chcesz dowiedzieć się więcej na temat konkretnego polecenia użyj opcji `--help` wraz z tym poleceniem.

```shell
dotnet --help
dotnet [command] --help
```

### 1. Stworzenie solucji i projektów

> To zadanie możesz zrobić na dwa sposoby: z lini poleceń za pomocą komendy `dotnet`, albo używając wybranego przez siebie IDE. Na Windowsach do wyboru jest `Visual Studio` i `Rider`, na Linuksach jest `Rider`.

Zaczniemy od stworzenia solucji i dwóch projektów: biblioteki i konsolowego. Projekt konsolowy będzie interfejsem konsolowym do wspomnianej biblioteki. Będziemy tworzyć aplikację do walidowania haseł. Wymyśl nazwę solucji i projektów. Może to być `PasswordValidator` dla solucji i `PasswordValidator.App`, `PasswordValidator.Lib` dla projektów jeżeli nie masz lepszych pomysłów.

```shell
# Create a solution
dotnet new sln --output <SolutionName>
cd <SolutionName>

# Create two projects inside <SolutionName>
dotnet new console --output <ConsoleProjectName>
dotnet new classlib --output <LibraryProjectName>

# Add projects to the solution:
dotnet sln add <ConsoleProjectName> <LibraryProjectName>

# Generate .gitignore file
dotnet new gitignore
```

Po stworzeniu uruchom aplikację konsolową: albo przez IDE, albo `dotnet run`.

Żeby kod z projektu `PasswordValidatorLib` był widziany w aplikacji konsolowej, należy dodać do niego referencję w projekcie `PasswordValidatorApp`. Można to zrobić na kilka sposobów:

1. `dotnet` CLI: `dotnet add PasswordValidatorApp reference PasswordValidatorLib`
2. Przez IDE:
   * [Visual Studio](https://learn.microsoft.com/visualstudio/ide/how-to-add-or-remove-references-by-using-the-reference-manager)
   * [Rider](https://www.jetbrains.com/help/rider/Extending_Your_Solution.html#project_assembly_references)
3. Ręcznie edytując plik projektu `PasswordValidatorApp.csproj`

Niezależnie od wybranej metody w pliku projektu powinieneś w pliku projektu konsolowego zobaczyć wpis:

```xml
  <ItemGroup>
    <ProjectReference Include="..\PasswordValidatorLib\PasswordValidatorLib.csproj" />
  </ItemGroup>
```

Itemy `ProjectReference` to projekty, z których kodu możemy korzystać w tym projekcie. Zostaną one zbudowane i dołączone do tego projektu.

### 2. Część biblioteczna

Możemy zacząć od usunięcia pliku z templatki `Class1.cs`, stworzymy dwa pliki: 

- `ValidationError.cs` z publicznym wyliczeniem (*enum*) możliwych błędów,
- `PasswordValidator.cs` z publiczną klasą o tej samej nazwie, a w niej metodę `public List<ValidationError> Validate(string password)`

Żeby nie umieszczać całej długiej logiki w jednej metodzie, podzielimy wykrywanie konkretnych cech hasła na oddzielne metody:

- `public bool ValidatePasswordLength(string password)`: sprawdza czy hasło ma co najmniej 8 znaków
- `public bool ValidatePasswordHasLowerCaseLetter(string password)`: sprawdza czy hasło zawiera małą literę
- `public bool ValidateContainsSpecialCharacter(string password)`: czy zawiera jeden znak specjalny ze zbioru: `!@#$%^&*(),.?'";:{}|<>[]`
- ... itd.

Dla każdej metody dodaj odpowiednie wyliczenie do `ValidationError`.

### 3. Część konsolowa

W `Program.cs` stwórz nowy obiekt `PasswordValidator` i w pętli odpytuj się użytkownika o hasło.

- Jeżeli hasło jest poprawne, wyświetl komunikat `"✓ Password is valid and safe!"`
- Jeżeli hasło nie jest poprawne wyświetl komunikat `"✗ Password is invalid:"`
  - Dla każdej niespełnionej reguły w kolejnej linii wypisz jej słowny opis, np. `"Password should contain at least 8 characters"`
  - Dodaj funkcję `string GetErrorMessage(ValidationError error)`, która zwróci tekstowy opis reguły
- Jeżeli użytkownik wpisze `exit`, to przerwij pętlę i zakończ program

### 4. `NuGet`

`NuGet` to oficjalny menedżer pakietów dla platformy .NET. Wyobraź sobie, że budujesz aplikację i potrzebujesz zaimplementować jakąś funkcjonalność, np. kolorowanie tekstu w konsoli, logowanie błędów, albo pracę z plikami JSON. Zamiast pisać cały ten kod od zera, możesz użyć gotowej biblioteki (czyli "pakietu"), którą ktoś już stworzył, przetestował i udostępnił. 

Dostępne paczki można wyszukać na [nuget.org](nuget.org), przez CLI `dotnet package search <search term>` lub przez IDE. Jak to zrobić dla Visual Studio znajdziesz w [dokumentacji NuGeta](https://learn.microsoft.com/nuget/quickstart/install-and-use-a-package-in-visual-studio#nuget-package-manager), a dla Ridera w jego [dokumetntacji](https://www.jetbrains.com/help/rider/Using_NuGet.html).

My do aplikacji konsolowej dodamy kolorowanie wyjścia. Użyjemy gotowej bilbioteki `Pastel` dostępnej w repozytorium `NuGet`. Żeby dodać paczkę do projektu można to zrobić na dwa sposoby przez komendę `dotnet`: `dotnet add PasswordValidatorApp package Pastel`, lub przez IDE.

Po dodaniu w pliku projektu powinieneś zauważyć nowy wpis, który deklaruje zależność projektu od pakietu NuGet. Itemy w `PackageReference` to bibioteki, które zostaną pobrane podczas budowania i będzie ich można użyć w projekcie.

```xml
  <ItemGroup>
    <PackageReference Include="Pastel" Version="7.0.0" />
  </ItemGroup>
```

Następnie dodaj do aplikacji kolorowanie składni.

1. Zaimportuj bibliotekę: `using Pastel;`
2. Wypisywane stringi `"<Text>"` zastąp przez `"<Text>".Pastel(ConsoleColor.<Color>)`, ustaw:
   - zielony kolor, jeżeli hasło było poprawne
   - czerwony, jeżeli było niepoprawne

## Task 3: Testy jednostkowe

Ostatnim rodzajem projektu, z którym będziemy pracować są testy jednostkowe. Projekt z testami jednostkowymi jest budowany tak jak zwykła biblioteka. Taka biblioteka jest później **wejściem** dla *test runnera*, który wyszukuje w takiej bibliotece metody oznaczone atrybutem `[Test]` (czyli testy) i je uruchamia. Zwyczajowo przyjmujemy, że test przeszedł, jeżeli nie rzucił wyjątkiem. Metody z rodziny "Assert", które służą do sprawdzenia warunków, przy niespełnionym warunku rzucają wyjątkiem.

### 0. Czym są testy jednostkowe?

Test jednostkowy to fragment kodu, który w sposób automatyczny sprawdza poprawność działania "jednostki" kodu aplikacji – najczęściej pojedynczej metody lub klasy.

Głównym celem jest upewnienie się, że dany fragment kodu działa dokładnie tak, jak tego oczekujemy, w izolacji od reszty kodu. Pozwala to na wczesne wykrywanie błędów i zabezpiecza przed psuciem istniejących funkcjonalności.

Dobry test jednostkowy jest pisany według prostego schematu **Arrange-Act-Assert (AAA)**:
1. **Arrange:** Przygotowujesz warunki i dane wejściowe.
2. **Act:** Wywołujesz testowaną metodę.
3. **Assert:** Sprawdzasz, czy wynik jest zgodny z oczekiwaniami.

W C# mamy 3 różne biblioteki do testów jednostkowych:

- `MSTest`
- `xUnit`
- `NUnit`

Generalnie my będziemy używać `MSTest`, ale wszystkie działają na tej samej zasadzie, robią to samo, różnią się tylko trochę terminologią, np. `Fact` vs `Test`.

### 1. Tworzenie projektu z testami

Żeby dodać projekt z testami jednostkowymi możemy to zrobić znów na dwa sposoby, przez CLI, albo przez IDE. Dodatkowo projekt z testami powinien mieć referencję na projekt testowany (W naszym przypadku będzie to `PasswordValidatorLib`).

```shell
dotnet new mstest --output PasswordValidatorTests
dotnet sln add PasswordValidatorTests
dotnet add PasswordValidatorTests reference PasswordValidatorLib
```

W nowo wygenerowanym projekcie powinieneś zobaczyć klasę w pliku `Test1.cs`, która na razie zawiera 1 test. Klasa, która może zawierać testy jest oznaczona atrybutem `[TestClass]`, testy jednostkowe (metody) są oznaczone atrybutem `[TestMethod]`.

```csharp
namespace PasswordValidatorTests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void TestMethod1()
    {
    }
}
````

Żeby uruchomić testy, możesz to uruchomić w konsoli `dotnet test`, albo przez w IDE. Jak to zrobić znajdziesz w [dokumentacji Visual Studio](https://learn.microsoft.com/visualstudio/test/run-unit-tests-with-test-explorer), lub [dokumentacji Ridera](https://www.jetbrains.com/help/rider/Getting_Started_with_Unit_Testing.html#step-3-run-the-tests). Jako że metoda jest teraz pusta, to test powinien przejść.

### 2. Testy jednostkowe

Zaczniemy od zmiany nazwy pliku z testami i nazwy klasy testowej na `PasswordValidatorTests`. W nim zmienimy też nazwę istniejącego już testu na `ValidatePassword_ValidPassword_ReturnsEmptyErrorList`. To częsta konwencja nazywania testu, w której zawieramy co testujemy, jakie jest wejście i jakie jest oczekiwane zachowanie. Test ten stworzymy według schematu **Arrange-Act-Assert**:

```csharp
    [TestMethod]
    public void Validate_ValidPassword_ReturnsEmptyErrorList()
    {
        // Arrange
        PasswordValidator validator = new PasswordValidator();
        string password = "Pass123!";
        
        // Act 
        var errorList = validator.Validate(password);
        
        // Assert
        Assert.AreEqual(errorList.Count, 0);
    }
```

W ten sposób, dla metody `Validate` zdefiniowaliśmy zachowanie, jakie powinna ta metoda spełniać. Celem jest napisanie testów dla wszystkich kluczowych scenariuszy: poprawnego działania, obsługi błędów oraz przypadków brzegowych (np. pusty string, `null`, wartości graniczne).

Dobry test jednostkowy powinien być:

* **szybki** - testów w projekcie może być tysiące, chcemy dostać szybki feedback czy nasze zmiany nie powodują regresji.
* **niezależny** - test powinien sprawdzać tylko jedną, konkretną "jednostkę" kodu i być odizolowany od zewnętrznych zależności (baza danych, sieć, UI). Stosowanie zasad [**SOLID**](https://en.wikipedia.org/wiki/SOLID) (a zwłaszcza **zasady odwrócenia zależności**) jest kluczowe do osiągnięcia tej izolacji, ponieważ pozwala na użycie tzw. "mocków" zamiast prawdziwych zależności.
* **powtarzalny** - test musi dawać ten sam wynik za każdym razem, niezależnie od środowiska, w którym jest uruchamiany (np. na maszynie dewelopera, na serwerze CI/CD). Nie powinien zależeć odczynników zewnętrznych, takich jak aktualna data/godzina, losowe wartości czy konfiguracja systemu.
* **prosty** - test powinien być krótki - około 3-5 linijek - samodokumentujący się. Ważne, że w teście definiujemy tylko input, wywołujemy metodę testową i sprawdzamy output, w żadnym wypadku nie piszemy w nim logiki, w szczególności logiki testowanej metody.

Czasami najpierw zaczyna się od pisania testów jednostkowych, czyli definiowania zachowań funkcji, a dopiero później pisze się implementację testowanych metod, aż do przejścia wszystkich testów. Takie podejście nazywamy *Test Driven Development (TDD)*.

Teraz spróbuj sam dopisać testy jednostkowe. Na przykład:

- `Validate_PasswordHasNoSpecialCharacter_ReturnsNoSpecialCharacterError`
- `ValidateLength_EmptyString_ReturnsFalse`
- `ValidateContainsDigit_PasswordWithDigit_ReturnsTrue`
- `ValidateContainsDigit_PasswordWithNoDigit_ReturnsFalse`

Możesz potrzebować innych metod do asercji: `CollectionAssert.Contains`, `Assert.IsFalse`, `Assert.IsTrue`.

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w [PasswordValidator](/labs/lab02/solution/PasswordValidator).

{{< filetree dir="labs/lab02/solution/PasswordValidator" >}}
