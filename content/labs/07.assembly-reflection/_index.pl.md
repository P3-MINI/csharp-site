---
title: "Zestawy, refleksja"
weight: 70
---

## Tutorial 7: Assembly i refleksja

Zakres tego laboratorium obejmuje refleksję – badanie typów i ich składowych w czasie wykonania (`System.Type`, przestrzeń nazw `System.Reflection`), definiowanie i odczytywanie własnych atrybutów, dynamiczne tworzenie obiektów i wywoływanie metod – a także assembly: ich metadane oraz wczytywanie ich w czasie działania programu za pomocą `AssemblyLoadContext`.

Refleksja nie ma prawdziwego odpowiednika w standardowym C++. RTTI (`typeid`, `dynamic_cast`) pozwala jedynie poznać nazwę typu (zależną od implementacji) i porównać typy – nie da się wylistować składowych klasy ani wywołać metody, znając tylko jej nazwę. W .NET kompilator zapisuje w każdym assembly pełny opis wszystkich typów (metadane), a środowisko uruchomieniowe udostępnia go poprzez obiekty takie jak `Type`, `MethodInfo` czy `PropertyInfo`. Na tym mechanizmie zbudowane są frameworki do testów jednostkowych, serializatory (np. `System.Text.Json`), kontenery wstrzykiwania zależności i systemy pluginów.

Przed rozpoczęciem zadania krótko omówimy najważniejsze zagadnienia. Więcej szczegółów znajdziesz w wykładach: [Atrybuty]({{< ref "/lectures/08.reflection/attributes" >}}), [Refleksja]({{< ref "/lectures/08.reflection/reflection" >}}) oraz [Assembly]({{< ref "/lectures/08.reflection/assembly" >}}).

### Typy w czasie wykonania: `System.Type`

Punktem wejścia do refleksji jest obiekt `System.Type` opisujący typ. Można go uzyskać na trzy sposoby:

```csharp
Type listType = typeof(List<int>);                             // known at compile time
Type sameType = new List<int>().GetType();                     // run-time type of an object
Type? builderType = Type.GetType("System.Text.StringBuilder"); // by name, e.g. read from a file

Console.WriteLine(listType == sameType);                                  // True - one Type object per type
Console.WriteLine(listType.Name);                                         // List`1
Console.WriteLine(listType.Namespace);                                    // System.Collections.Generic
Console.WriteLine(listType.GetGenericArguments()[0]);                     // System.Int32
Console.WriteLine(listType.GetGenericTypeDefinition() == typeof(List<>)); // True
```

Nazwa typu generycznego kończy się odwrotnym apostrofem i liczbą jego parametrów typu (``List`1``). `typeof(List<>)` oznacza *otwarty* typ generyczny (samą definicję, bez argumentów typu). Właściwości takie jak `IsClass`, `IsValueType`, `IsEnum`, `IsInterface`, `IsAbstract` czy `IsGenericType` opisują rodzaj typu, a metody `IsAssignableFrom`, `IsSubclassOf` i `GetInterfaces` – relacje między typami. Tak na przykład można sprawdzić, czy typ implementuje generyczny interfejs `IParsable<T>` dla dowolnego `T`:

```csharp
bool parsable = typeof(int).GetInterfaces()
    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));
Console.WriteLine(parsable);                                             // True
Console.WriteLine(typeof(IComparable).IsAssignableFrom(typeof(string))); // True
```

### Składowe i `BindingFlags`

`Type` daje dostęp do składowych typu: `GetFields`, `GetProperties`, `GetMethods`, `GetConstructors`, `GetMembers` oraz ich odpowiedników w liczbie pojedynczej (np. `GetMethod("Name")`). Wywołane bez argumentów zwracają tylko **publiczne** składowe – instancyjne i statyczne, w tym odziedziczone składowe instancyjne. Aby pobrać inne składowe, należy przekazać kombinację flag `BindingFlags`:

```csharp
const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
    | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
FieldInfo[] fields = typeof(Person).GetFields(flags); // also private fields, but not inherited ones
```

> [!WARNING]
> Jeśli przekazujesz jakiekolwiek `BindingFlags`, musisz określić zarówno widoczność (`Public` i/lub `NonPublic`), jak i rodzaj składowych (`Instance` i/lub `Static`). Na przykład `GetFields(BindingFlags.NonPublic)` zawsze zwraca pustą tablicę.

Każdą składową opisuje obiekt klasy pochodnej po `MemberInfo` (`FieldInfo`, `PropertyInfo`, `MethodInfo`, `ConstructorInfo`, `EventInfo`). Za pomocą tych obiektów można dynamicznie korzystać ze składowej (`Person` to przykładowa klasa z Etapu 1 zadania poniżej):

| Składowa | Dynamiczny dostęp |
|---|---|
| `FieldInfo`, `PropertyInfo` | `GetValue(obj)`, `SetValue(obj, value)` |
| `MethodInfo` | `Invoke(obj, arguments)` – `obj` to `null` dla metod statycznych |
| `ConstructorInfo` | `Invoke(arguments)` lub po prostu `Activator.CreateInstance(type, arguments)` |

```csharp
var person = new Person("Alice", 30);
PropertyInfo name = typeof(Person).GetProperty("Name")!;
name.SetValue(person, "Bob");

MethodInfo greet = typeof(Person).GetMethod("Greet")!;
object? greeting = greet.Invoke(person, ["Hi", true]); // arguments and the result are boxed in objects
Console.WriteLine(greeting);                           // HI, BOB!
```

Refleksja operuje na typie `object`: argumenty przekazuje się w tablicy `object?[]` (typy bezpośrednie są pakowane), typy są sprawdzane w czasie wykonania, a wynik trzeba rzutować. Daje to dużą elastyczność, ale jest wolne i podatne na błędy – pomyłka, którą w zwykłym kodzie wykryłby kompilator, staje się wyjątkiem w czasie wykonania.

### Assembly

Assembly (plik `.dll` lub `.exe`) to w .NET jednostka wdrażania i wersjonowania. Zawiera kod pośredni (IL), metadane wszystkich swoich typów oraz manifest (nazwę i wersję assembly oraz listę assembly, od których zależy). W refleksji reprezentuje je klasa `Assembly`: `typeof(Person).Assembly` lub `Assembly.GetExecutingAssembly()` zwracają assembly, a `assembly.GetTypes()` – wszystkie zdefiniowane w nim typy.

Assembly można także wczytywać w trakcie działania programu – podobnie jak `dlopen`/`LoadLibrary` w C/C++. Jednak `dlsym` zwraca jedynie adres wyeksportowanej funkcji, której sygnaturę trzeba znać z góry. Wczytane assembly .NET niesie ze sobą pełną informację o typach, dzięki czemu program może znaleźć np. wszystkie klasy oznaczone danym atrybutem, utworzyć ich instancje i korzystać z nich jak ze zwykłych obiektów. Na tym opierają się systemy pluginów.

## Zadanie 1 - Powłoka poleceń z pluginami

W ramach tego zadania zbudujesz interaktywną powłokę poleceń (REPL – *read-eval-print loop*). Polecenia powłoki to zwykłe metody C# oznaczone atrybutami. Powłoka nie zna ich w czasie kompilacji: odnajduje je za pomocą refleksji, konwertuje wpisane przez użytkownika argumenty na typy parametrów, wywołuje metody i wypisuje ich wyniki. W ostatnich etapach polecenia będą pochodzić z pluginów – bibliotek klas wczytywanych w czasie działania programu z katalogu `plugins`, które można nawet przebudować i wczytać ponownie bez restartowania powłoki.

Końcowa wersja powłoki będzie działać następująco:

```text
> help pow
pow <x> [n=2]
  Raises x to the n-th power.
  Aliases: power
  Parameters:
    x          double
    n          int               the exponent
  Defined in: MathPlugin.MathCommands (MathPlugin)
> pow 2 10
1024
> wc "the quick brown fox"
TextStats
  Characters: 19
  Words: 4
  Vowels: 5
```

Zacznij od utworzenia rozwiązania z projektem aplikacji konsolowej:

```bash
dotnet new sln -n ReflectionShell
dotnet new console -n Shell
dotnet sln add Shell
```

### Etap 1: Badanie typów

Zanim napiszemy samą powłokę, zbudujemy dwa narzędzia, z których będzie ona później korzystać: `TypeInspector`, który opisuje składowe dowolnego typu (przyszłe polecenie `inspect`), oraz `ObjectDumper`, który wypisuje wartości dowolnego obiektu (użyjemy go do wyświetlania wyników poleceń).

#### Czytelne nazwy typów

`Type.Name` nie zawsze odpowiada temu, co napisalibyśmy w C#: `typeof(int).Name` to `Int32`, a `typeof(List<string>).Name` to ``List`1``. Utwórz plik `TypeExtensions.cs` z właściwością rozszerzającą `FriendlyName`, zwracającą nazwę typu w stylu C#. Użyj bloku rozszerzającego wprowadzonego w C# 14 (zob. wykład [Metody rozszerzające]({{< ref "/lectures/05.csharp-features/extension-methods" >}})):

```csharp
public static class TypeExtensions
{
    extension(Type type)
    {
        // C#-like name of a type, e.g. "int", "List<string>", "int?", "double[]"
        public string FriendlyName => type switch
        {
            // ...
            _ => type.Name,
        };
    }
}
```

Obsłuż następujące przypadki:
1. Typy posiadające słowo kluczowe C# (`int`, `string`, `bool`, `double`, `decimal`, `object`, `void`, …) – wyszukaj je w prywatnym statycznym słowniku `Dictionary<Type, string>`.
2. Tablice (`IsArray`) – czytelna nazwa typu elementu (`GetElementType()`), a po niej `[]` (`[,]`, gdy `GetArrayRank()` zwraca 2 itd.).
3. Typy przekazywane przez referencję (`IsByRef`) – to typy parametrów `ref`/`out` (np. `Int32&`); zwróć czytelną nazwę typu elementu.
4. `Nullable<T>` – `Nullable.GetUnderlyingType(type)` zwraca `T` (lub `null`, jeśli typ nie jest nullowalny); zwróć np. `int?`.
5. Typy generyczne (`IsGenericType`) – nazwa do odwrotnego apostrofu i czytelne nazwy `GetGenericArguments()`, np. `Dictionary<string, int>`.
6. W pozostałych przypadkach – `Name`.

#### TypeInspector

Inspektor przetestujemy na przykładowej klasie. Utwórz plik `Samples/Person.cs`:

```csharp
namespace Shell.Samples;

public record Address(string City, string Street);

public class Person(string name, int age) : IComparable<Person>
{
    private readonly List<Person> _friends = [];

    public string Name { get; set; } = name;
    public int Age { get; private set; } = age;
    public Address? Address { get; set; }
    public IReadOnlyList<Person> Friends => _friends;
    public bool IsAdult => Age >= 18;

    public void AddFriend(Person friend) => _friends.Add(friend);

    public void CelebrateBirthday() => Age++;

    public string Greet(string greeting = "Hello", bool loud = false)
    {
        string text = $"{greeting}, {Name}!";
        return loud ? text.ToUpperInvariant() : text;
    }

    public static Person Parse(string text)
    {
        string[] parts = text.Split(',', StringSplitOptions.TrimEntries);
        return new Person(parts[0], int.Parse(parts[1]));
    }

    public int CompareTo(Person? other) => string.Compare(Name, other?.Name, StringComparison.Ordinal);

    public override string ToString() => $"{Name} ({Age})";
}
```

Utwórz statyczną klasę `TypeInspector` (`TypeInspector.cs`) z metodą `public static string Describe(Type type)`, która zwraca opis wszystkich składowych **zadeklarowanych** w danym typie:

1. Nagłówek: rodzaj typu (`class`, `struct`, `interface`, `enum`, `static class`, `abstract class`), jego `FullName`, typ bazowy `BaseType`, interfejsy (`GetInterfaces()`) i nazwa assembly (`type.Assembly.GetName().Name`).
2. Sekcje `Constructors`, `Fields`, `Properties` i `Methods` ze składowymi pobranymi przy użyciu `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly`. Refleksja nie gwarantuje kolejności składowych, dlatego posortuj je po nazwie. Dla pustej sekcji wypisz `(none)`.
3. Modyfikator dostępu każdej składowej: `MethodBase` (klasa bazowa `MethodInfo` i `ConstructorInfo`) oraz `FieldInfo` mają właściwości `IsPublic`, `IsPrivate`, `IsFamily` (`protected`), `IsAssembly` (`internal`) i `IsFamilyOrAssembly` (`protected internal`).
4. Pola: `static` (`IsStatic`), `readonly` (`IsInitOnly`) lub `const` (`IsLiteral`), typ `FieldType` i nazwa.
5. Właściwości: typ `PropertyType`, nazwa i akcesory `GetMethod`/`SetMethod` (`null`, jeśli akcesora brak), z modyfikatorem dostępu akcesora, jeśli różni się od modyfikatora właściwości: `public int Age { get; private set; }`.
6. Metody: pomiń te z flagą `IsSpecialName` (zob. ramkę poniżej). Wypisz `static`, `abstract`, `override` (gdy `GetBaseDefinition().DeclaringType` różni się od `DeclaringType`) lub `virtual` (`IsVirtual && !IsFinal`), typ `ReturnType`, nazwę i parametry.
7. Parametry (`GetParameters()`): typ i nazwa, poprzedzone słowem `params` dla parametru oznaczonego `ParamArrayAttribute` (`parameter.IsDefined(typeof(ParamArrayAttribute))`) lub `ref`/`out` dla parametrów przekazywanych przez referencję (`IsOut`). Jeśli `HasDefaultValue` ma wartość `true`, dopisz ` = ` i `DefaultValue`.
8. Wartości domyślne formatuj tak jak w kodzie C# (`"Hello"`, `false`, `null`) w publicznej metodzie `public static string FormatValue(object? value)` – użyjemy jej ponownie później. Liczby formatuj za pomocą `Convert.ToString(value, CultureInfo.InvariantCulture)`, aby wynik nie zależał od ustawień regionalnych.

> [!IMPORTANT]
> #### Kod widziany przez refleksję
>
> Refleksja pokazuje typy tak, jak wygenerował je kompilator, a nie tak, jak je napisałeś:
> - Właściwość to tylko metadane łączące parę metod `get_Name` i `set_Name`. Te metody akcesorów są zwracane także przez `GetMethods()` i oznaczone flagą `IsSpecialName` (w ten sam sposób oznaczone są operatory, np. `op_Addition`).
> - Każda automatyczna właściwość ma ukryte pole (*backing field*) o nazwie `<Name>k__BackingField`.
> - Adnotacje nullowalnych typów referencyjnych nie są częścią typu: `PropertyType` właściwości `Address? Address` to po prostu `Address`. Adnotacje są zapisane w atrybutach i można je odczytać za pomocą `NullabilityInfoContext`.
> - Klasa statyczna jest w IL klasą `abstract sealed` – zarówno `IsAbstract`, jak i `IsSealed` mają wartość `true`.

W `Program.cs` wypisz opis klasy `Person`:

```csharp
Console.WriteLine(TypeInspector.Describe(typeof(Person)));
```

Oczekiwany wynik:

```text
class Shell.Samples.Person
  Base type: object
  Interfaces: IComparable<Person>
  Assembly: Shell
Constructors:
  public Person(string name, int age)
Fields:
  private readonly List<Person> _friends
  private Address <Address>k__BackingField
  private int <Age>k__BackingField
  private string <Name>k__BackingField
Properties:
  public Address Address { get; set; }
  public int Age { get; private set; }
  public IReadOnlyList<Person> Friends { get; }
  public bool IsAdult { get; }
  public string Name { get; set; }
Methods:
  public void AddFriend(Person friend)
  public void CelebrateBirthday()
  public int CompareTo(Person other)
  public string Greet(string greeting = "Hello", bool loud = false)
  public static Person Parse(string text)
  public override string ToString()
```

#### ObjectDumper

Teraz zrobimy odwrotnie: zamiast opisywać typ, wypiszemy wartości przechowywane w obiekcie dowolnego typu. Utwórz statyczną klasę `ObjectDumper` z metodą `public static string Dump(object? value, int maxDepth = 5)`, która rekurencyjnie wypisuje graf obiektów:

1. `null` jest wypisywany jako `null`.
2. Wartości proste – typy prymitywne (`IsPrimitive`), typy wyliczeniowe (`IsEnum`), `string`, `decimal` i inne typy implementujące `IFormattable` (np. `DateTime`, `DateOnly`, `Guid`) – są wypisywane w jednej linii za pomocą `TypeInspector.FormatValue`.
3. Kolekcje (obiekty implementujące niegeneryczny interfejs `IEnumerable`) są wypisywane jako nazwa typu i liczba elementów, a po nich każdy element w osobnej linii, w postaci `[indeks]: wartość`.
4. Pozostałe obiekty są wypisywane jako nazwa typu, a po niej każda publiczna właściwość instancyjna w postaci `Nazwa: wartość`. Pomiń indeksatory – właściwości z parametrami (`GetIndexParameters().Length > 0`), których nie da się odczytać bez indeksu. Jeśli getter rzuci wyjątek, `GetValue` rzuca `TargetInvocationException`; zamiast wartości wypisz wtedy `<NazwaTypuWyjątku>`.
5. Każdy poziom zagnieżdżenia jest wcięty o dwie spacje. Po przekroczeniu `maxDepth` poziomów wypisz tylko nazwę typu i `{ ... }`.
6. Grafy obiektów mogą zawierać cykle (Alice jest przyjaciółką Boba, a Bob – Alice). Przechowuj obiekty leżące na ścieżce od korzenia do bieżącego obiektu w zbiorze `HashSet<object>`; jeśli obiekt już się na niej znajduje, zamiast rozwijać go ponownie wypisz `<cycle: NazwaTypu>`. Po zakończeniu przetwarzania obiektu usuń go ze zbioru (w bloku `finally`).

> [!WARNING]
> Zbiór odwiedzonych obiektów musi porównywać obiekty **przez referencję**, jak wskaźniki w C++. Utwórz go za pomocą `new HashSet<object>(ReferenceEqualityComparer.Instance)`. Domyślny komparator używa metod `Equals` i `GetHashCode`, które mogą być nadpisane: rekordy porównują wartości, więc dwa różne (ale równe) obiekty `Address` zostałyby uznane za ten sam obiekt, a własna implementacja `Equals` może nawet rzucić wyjątek.

Rozbuduj `Program.cs`:

```csharp
var alice = new Person("Alice", 30) { Address = new Address("Warsaw", "Koszykowa 75") };
var bob = new Person("Bob", 17);
alice.AddFriend(bob);
bob.AddFriend(alice); // a cycle: alice -> bob -> alice

Console.WriteLine(ObjectDumper.Dump(alice));
Console.WriteLine(ObjectDumper.Dump(new Dictionary<string, DateOnly> { ["Christmas"] = new(2026, 12, 24) }));
```

Oczekiwany wynik:

```text
Person
  Name: "Alice"
  Age: 30
  Address: Address
    City: "Warsaw"
    Street: "Koszykowa 75"
  Friends: List<Person> (Count = 1)
    [0]: Person
      Name: "Bob"
      Age: 17
      Address: null
      Friends: List<Person> (Count = 1)
        [0]: <cycle: Person>
      IsAdult: false
  IsAdult: true

Dictionary<string, DateOnly> (Count = 1)
  [0]: KeyValuePair<string, DateOnly>
    Key: "Christmas"
    Value: 12/24/2026
```

Zwróć uwagę, że właściwość `Friends` jest zadeklarowana jako `IReadOnlyList<Person>`, ale `GetType()` zwraca rzeczywisty typ obiektu w czasie wykonania – `List<Person>`.

> [!NOTE]
> #### Etap 1
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.01" >}}

### Etap 2: Atrybuty i wykrywanie poleceń

Polecenia naszej powłoki będą zwykłymi metodami oznaczonymi atrybutami. Atrybuty są pasywne: jedynie dodają informacje do metadanych. To nasz kod – rejestr poleceń – odczytuje je za pomocą refleksji i na ich podstawie podejmuje działania.

> [!IMPORTANT]
> #### Definiowanie atrybutów
>
> Atrybut to klasa dziedzicząca po `System.Attribute`. Zwyczajowo jej nazwa kończy się na `Attribute`, a przy użyciu atrybutu ten sufiks można pominąć (`[Command]` oznacza `[CommandAttribute]`). Atrybut `[AttributeUsage]` określa, do jakich elementów kodu można przyczepić atrybut (`AttributeTargets`), czy można go przyczepić wielokrotnie do tego samego elementu (`AllowMultiple`, domyślnie `false`) oraz czy jest dziedziczony przez klasy pochodne i nadpisujące metody (`Inherited`, domyślnie `true`). Kompilator pilnuje tych reguł – np. atrybut przeznaczony dla metod przyczepiony do właściwości powoduje błąd CS0592, a powtórzony atrybut z `AllowMultiple = false` – błąd CS0579.
>
> Parametry konstruktora atrybutu to jego argumenty pozycyjne, a jego publiczne właściwości z możliwością zapisu można ustawiać jako argumenty nazwane (`[Obsolete("Old", DiagnosticId = "X1")]`). Wszystkie argumenty muszą być stałymi czasu kompilacji, ponieważ są zapisywane w metadanych. Obiekt atrybutu powstaje dopiero przy jego odczycie za pomocą refleksji – każde wywołanie `GetCustomAttribute` tworzy nową instancję.
>
> ```csharp
> // Marks a method as a shell command. Without a name, the lower-case method name is used.
> [AttributeUsage(AttributeTargets.Method)]
> public sealed class CommandAttribute(string? name = null) : Attribute
> {
>     public string? Name { get; } = name;
> }
> ```

#### Atrybuty

Utwórz katalog `Abstractions` z czterema atrybutami w przestrzeni nazw `Shell.Abstractions` (w Etapie 4 ten katalog stanie się osobną biblioteką współdzieloną przez powłokę i jej pluginy):

1. `CommandModuleAttribute` – dla klas, niedziedziczony (`Inherited = false`), bez parametrów. Oznacza klasę zawierającą polecenia (*moduł*), podobnie jak `[TestClass]` we frameworkach do testów jednostkowych.
2. `CommandAttribute` – dla metod, z opcjonalną nazwą `Name` (pokazany powyżej).
3. `AliasAttribute` – dla metod, wielokrotny (`AllowMultiple = true`), z alternatywną nazwą polecenia (`string Alias`).
4. `DescriptionAttribute` – dla klas, metod i parametrów (`AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter`), z opisem (`string Text`) wyświetlanym przez polecenie `help`.

> [!TIP]
> .NET zawiera już atrybut `DescriptionAttribute` (w przestrzeni nazw `System.ComponentModel`). Nie stanowi to problemu, dopóki nie importujesz obu przestrzeni nazw w tym samym pliku – w przeciwnym razie kompilator zgłosi niejednoznaczne odwołanie (CS0104).

#### Moduły poleceń

Utwórz katalog `Commands` z dwoma modułami. Skopiuj klasę `MathCommands` – zawiera zarówno polecenia instancyjne (moduł ma stan: pamięć używaną przez `m+` i `mr`), jak i statyczne:

```csharp
using Shell.Abstractions;

namespace Shell.Commands;

[CommandModule]
[Description("Arithmetic commands.")]
public class MathCommands
{
    private decimal _memory;

    [Command, Description("Adds two numbers.")]
    public double Add(double a, double b) => a + b;

    [Command("div"), Description("Divides two integers.")]
    public int Divide(int a, int b) => a / b;

    [Command("pow"), Alias("power"), Description("Raises x to the n-th power.")]
    public double Power(double x, [Description("the exponent")] int n = 2) => Math.Pow(x, n);

    [Command, Alias("r"), Description("Rounds a number to the given number of fractional digits.")]
    public decimal Round(
        decimal value,
        [Description("number of fractional digits")] int digits = 0,
        [Description("how to round midpoint values")] MidpointRounding mode = MidpointRounding.ToEven) =>
        Math.Round(value, digits, mode);

    [Command("avg"), Description("Computes the arithmetic mean.")]
    public static double Average(params double[] values) => values.Length > 0 ? values.Average() : 0;

    [Command, Description("Counts the days between two dates.")]
    public static int Days(DateOnly from, DateOnly to) => to.DayNumber - from.DayNumber;

    [Command("m+"), Description("Adds a value to the memory.")]
    public decimal MemoryAdd(decimal value) => _memory += value;

    [Command("mr"), Description("Recalls the value stored in the memory.")]
    public decimal MemoryRecall() => _memory;
}
```

Drugi moduł, `TextCommands`, to klasa **statyczna** oznaczona atrybutami `[CommandModule]` i `[Description("Text manipulation commands.")]`. Zaimplementuj w niej następujące polecenia (z opisami jak na listingu poniżej):
- `upper <text>` – zamienia tekst na wielkie litery,
- `reverse <text>` – odwraca tekst,
- `repeat <text> [times=2] [separator=" "]` – łączy `times` kopii tekstu separatorem (`string.Join` + `Enumerable.Repeat`),
- `wc <text>` (metoda `WordCount`) – zwraca rekord `public record TextStats(int Characters, int Words, int Vowels)` z liczbą znaków, słów (oddzielonych spacjami) i samogłosek (`aeiouy`, bez rozróżniania wielkości liter) w tekście.

#### Rejestr poleceń

Utwórz klasę `CommandInfo` (`CommandInfo.cs`) opisującą pojedyncze wykryte polecenie. Użyj konstruktora głównego `CommandInfo(string name, MethodInfo method, object? target)`, gdzie `target` to obiekt, na którym zostanie wywołana metoda (`null` dla metod statycznych), i zdefiniuj właściwości tylko do odczytu:
- `Name`, `Method`, `Target`,
- `ParameterInfo[] Parameters` – parametry metody,
- `IReadOnlyList<string> Aliases` – wartości wszystkich atrybutów `[Alias]` metody,
- `string Description` – tekst atrybutu `[Description]` metody lub pusty napis,
- `string Usage` – nazwa, a po niej parametry: wymagane jako `<nazwa>`, opcjonalne jako `[nazwa=domyślna]` (`[nazwa]`, jeśli wartość domyślna to `null`) i tablice `params` jako `[nazwa...]`, np. `round <value> [digits=0] [mode=ToEven]`,
- metodę `string GetDetails()` zwracającą szczegółowy opis – zob. wynik `help power` poniżej. Dla parametrów typu wyliczeniowego wypisz dozwolone wartości (`Enum.GetNames`).

> [!IMPORTANT]
> #### Odczytywanie atrybutów
>
> Metody rozszerzające z klasy `System.Reflection.CustomAttributeExtensions` odczytują atrybuty dowolnego elementu kodu (`Type`, `MethodInfo`, `ParameterInfo`, `Assembly`, …):
> - `GetCustomAttribute<T>()` zwraca atrybut lub `null`, jeśli element go nie posiada (a gdy jest ich więcej – rzuca `AmbiguousMatchException`),
> - `GetCustomAttributes<T>()` zwraca wszystkie – używaj jej dla atrybutów z `AllowMultiple = true`,
> - `IsDefined(typeof(T))` tylko sprawdza, czy atrybut jest obecny, bez tworzenia jego instancji.
>
> ```csharp
> string? description = method.GetCustomAttribute<DescriptionAttribute>()?.Text;
> List<string> aliases = [.. method.GetCustomAttributes<AliasAttribute>().Select(a => a.Alias)];
> ```

Następnie utwórz klasę `CommandRegistry` (`CommandRegistry.cs`). Przechowuje ona polecenia na liście oraz w słowniku indeksowanym nazwami i aliasami (bez rozróżniania wielkości liter: `StringComparer.OrdinalIgnoreCase`). Zaimplementuj:

1. `IEnumerable<CommandInfo> Commands` – polecenia posortowane po nazwie.
2. `bool TryFind(string nameOrAlias, [NotNullWhen(true)] out CommandInfo? command)`.
3. `void RegisterAssembly(Assembly assembly)` – rejestruje za pomocą `RegisterModule(Type)` każdą klasę oznaczoną `[CommandModule]` spośród `assembly.GetTypes()`.
4. `void RegisterModule(Type type)` – tworzy instancję modułu i rejestruje jego polecenia. Klasa statyczna nie potrzebuje instancji. Jeśli klasa jest abstrakcyjna lub nie ma publicznego konstruktora bezparametrowego, wypisz ostrzeżenie i ją pomiń.
5. `void RegisterModule(object instance)` – rejestruje polecenia istniejącego obiektu (przyda się w następnym etapie).
6. Rejestrowanie poleceń: dla każdej publicznej metody (instancyjnej i statycznej) oznaczonej `[Command]` utwórz `CommandInfo` o nazwie `attribute.Name ?? method.Name.ToLowerInvariant()`. Pomiń (z ostrzeżeniem) metody generyczne (`IsGenericMethodDefinition`) i metody z parametrami `ref`/`out` (`ParameterType.IsByRef`), a także polecenia, których nazwa lub alias są już zajęte.
7. `string GetHelp(string? name = null)` – dla podanej nazwy zwraca `GetDetails()` polecenia (lub `Unknown command '…'.`); bez nazwy zwraca listę wszystkich poleceń pogrupowanych według modułów (`Method.DeclaringType`). Nagłówkiem grupy jest nazwa klasy modułu i jej `[Description]`.

Ostrzeżenia wypisuj na żółto (`Console.ForegroundColor = ConsoleColor.Yellow`, a potem `Console.ResetColor()`).

> [!IMPORTANT]
> #### Tworzenie obiektów za pomocą `Activator`
>
> `Activator.CreateInstance(type)` tworzy obiekt za pomocą publicznego konstruktora bezparametrowego. Jeśli takiego konstruktora nie ma, rzuca `MissingMethodException`, dlatego lepiej sprawdzić to wcześniej: `type.GetConstructor(Type.EmptyTypes)` zwraca `null`, jeśli konstruktor nie istnieje. Inne przeciążenia, np. `Activator.CreateInstance(type, arg1, arg2)`, wybierają konstruktor pasujący do argumentów. Klasy statycznej nie da się w ogóle utworzyć – rozpoznasz ją po `type.IsAbstract && type.IsSealed`.

W `Program.cs` zarejestruj polecenia bieżącego assembly i wypisz pomoc:

```csharp
var registry = new CommandRegistry();
registry.RegisterAssembly(Assembly.GetExecutingAssembly());

Console.WriteLine(registry.GetHelp());
Console.WriteLine(registry.GetHelp("power"));
```

Oczekiwany wynik:

```text
MathCommands - Arithmetic commands.
  add <a> <b>                              Adds two numbers.
  avg [values...]                          Computes the arithmetic mean.
  days <from> <to>                         Counts the days between two dates.
  div <a> <b>                              Divides two integers.
  m+ <value>                               Adds a value to the memory.
  mr                                       Recalls the value stored in the memory.
  pow <x> [n=2]                            Raises x to the n-th power.
  round <value> [digits=0] [mode=ToEven]   Rounds a number to the given number of fractional digits.
TextCommands - Text manipulation commands.
  repeat <text> [times=2] [separator=" "]  Repeats the text several times.
  reverse <text>                           Reverses the text.
  upper <text>                             Converts the text to upper case.
  wc <text>                                Counts characters, words and vowels.

pow <x> [n=2]
  Raises x to the n-th power.
  Aliases: power
  Parameters:
    x          double
    n          int               the exponent
  Defined in: Shell.Commands.MathCommands (Shell)
```

> [!TIP]
> Sprawdź walidację: dodaj tymczasowo moduł, którego konstruktor ma parametr, polecenie z parametrami `ref` oraz drugie polecenie o nazwie `add`. Powinieneś zobaczyć ostrzeżenia takie jak `Warning: MathCommands.Add: name 'add' is already taken - skipped.`

> [!NOTE]
> #### Etap 2
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.02" >}}

### Etap 3: Dynamiczne wywoływanie

Teraz zbudujemy samą powłokę. Każda linia wpisana przez użytkownika jest dzielona na słowa; pierwsze z nich to nazwa polecenia, pozostałe to jego argumenty. Powłoka wyszukuje polecenie w rejestrze, konwertuje argumenty z napisów na typy parametrów, wywołuje metodę i wypisuje wynik.

#### Klasa powłoki

Utwórz klasę `CommandShell` z właściwością `CommandRegistry Registry` oraz następującymi składowymi:

1. Polecenia wbudowane: `help [command]` (zwraca `Registry.GetHelp(command)`) oraz `exit` z aliasem `quit` (kończy pętlę). Zaimplementuj je jako metody klasy `CommandShell` oznaczone `[Command]` i zarejestruj w konstruktorze za pomocą `Registry.RegisterModule(this)`. Nie oznaczaj `CommandShell` atrybutem `[CommandModule]` – `RegisterAssembly` próbowałoby utworzyć drugą instancję powłoki.
2. `void Run()` – wypisuje powitanie, a następnie w pętli wypisuje znak zachęty `> `, wczytuje linię (`Console.ReadLine()` zwraca `null` na końcu wejścia) i ją wykonuje. Jeśli wejście jest przekierowane z pliku (`Console.IsInputRedirected`), wypisz wczytaną linię – dzięki temu `dotnet run --project Shell < commands.txt` daje czytelny zapis sesji.
3. `void Execute(string line)` – wykonuje pojedyncze polecenie, jak opisano poniżej.

Podział linii na słowa, w którym tekst w cudzysłowie stanowi jedno słowo, najłatwiej zrealizować wyrażeniem regularnym. Możesz użyć poniższego kodu (wyrażenie regularne jest kompilowane podczas budowania przez generator źródeł, dlatego klasa musi być `partial`):

```csharp
// A word or a text in double quotes
[GeneratedRegex(@"""([^""]*)""|(\S+)")]
private static partial Regex TokenRegex();

private static string[] Tokenize(string line) =>
    [.. TokenRegex().Matches(line).Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)];
```

#### Konwersja argumentów

Utwórz statyczną klasę `ArgumentConverter` z metodą `object?[] Bind(ParameterInfo[] parameters, string[] tokens)`, która tworzy tablicę argumentów dla `Invoke`:
1. i-te słowo jest konwertowane na typ i-tego parametru.
2. Jeśli słów jest mniej niż parametrów, brakujący parametr otrzymuje `DefaultValue`, o ile `HasDefaultValue` ma wartość `true`; w przeciwnym razie rzuć `ArgumentException("Missing value for parameter '…'.")`.
3. Parametr `params` (zawsze ostatni) przejmuje wszystkie pozostałe słowa. Utwórz tablicę odpowiedniego typu za pomocą `Array.CreateInstance(elementType, length)` i wypełnij ją metodą `SetValue`.
4. Nadmiarowe słowa powodują `ArgumentException("Too many arguments: …")`.

> [!WARNING]
> Dla parametru bez wartości domyślnej `ParameterInfo.DefaultValue` zwraca `DBNull.Value`, a nie `null`. Zawsze najpierw sprawdzaj `HasDefaultValue`.

Konwersją pojedynczej wartości zajmuje się metoda `object? ConvertValue(string text, Type type)`:
1. `string` – sam tekst.
2. `Nullable<T>` – konwersja na `T`.
3. Typy wyliczeniowe – `Enum.TryParse(type, text, ignoreCase: true, out object? value)`. Zwróć uwagę, że `TryParse` akceptuje także liczby spoza zdefiniowanych wartości, dlatego dodatkowo sprawdź `Enum.IsDefined(type, value)`. W przeciwnym razie rzuć `ArgumentException` z listą dozwolonych wartości (`Enum.GetNames(type)`).
4. Typy implementujące `IConvertible` (typy prymitywne, `decimal`, `DateTime`) – `Convert.ChangeType(text, type, CultureInfo.InvariantCulture)`. Dzięki niezmiennej kulturze (*invariant culture*) `3.5` jest parsowane tak samo niezależnie od ustawień regionalnych systemu.
5. Inne typy (np. `DateOnly`, `TimeSpan`, `Guid`) zwykle mają statyczną metodę `bool TryParse(string s, IFormatProvider provider, out T result)`. Znajdź ją i wywołaj za pomocą refleksji (zob. niżej). Jeśli takiej metody nie ma, typ nie jest obsługiwany.

Wyjątki `FormatException`, `OverflowException` i `InvalidCastException` rzucone podczas konwersji zamieniaj na `ArgumentException` z komunikatem `Cannot convert '…' to ….`, aby powłoka mogła obsługiwać wszystkie niepoprawne argumenty w ten sam sposób.

> [!IMPORTANT]
> #### Parametry `out` a refleksja
>
> Aby znaleźć przeciążenie z parametrem `ref`/`out`, opisz typ parametru za pomocą `MakeByRefType()` (`int` → `Int32&`). Gdy taka metoda zostanie wywołana przez `Invoke`, wartości parametrów `out` są zapisywane z powrotem do tablicy argumentów:
>
> ```csharp
> MethodInfo? tryParse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static,
>     [typeof(string), typeof(IFormatProvider), type.MakeByRefType()]);
> if (tryParse is null)
> {
>     throw new ArgumentException($"Parameters of type {type.FriendlyName} are not supported.");
> }
>
> object?[] arguments = [text, CultureInfo.InvariantCulture, null];
> bool parsed = (bool)tryParse.Invoke(null, arguments)!;
> // Invoke stores the values of out parameters back in the arguments array
> return parsed ? arguments[2] : throw new FormatException();
> ```
>
> Nie każdy typ ma taką publiczną metodę – np. `bool` i `char` implementują `IParsable<T>.TryParse` jawnie (*explicit implementation*). Dlatego najpierw próbujemy `Convert.ChangeType`.

#### Wykonywanie poleceń

Zaimplementuj `Execute(string line)`:
1. Podziel linię na słowa. Pustą linię zignoruj.
2. Znajdź polecenie za pomocą `Registry.TryFind`. Jeśli nie istnieje, wypisz `Error: Unknown command '…'. Type 'help' to list commands.`
3. Utwórz argumenty za pomocą `ArgumentConverter.Bind(command.Parameters, tokens[1..])`. Jeśli rzuci `ArgumentException`, wypisz `Error: ` i komunikat wyjątku.
4. Wywołaj `command.Method.Invoke(command.Target, arguments)`.
5. Jeśli metoda nie zwraca `void` (`Method.ReturnType`), wypisz wynik: napis bez zmian, każdy inny obiekt za pomocą `ObjectDumper.Dump`.

Błędy wypisuj na czerwono.

> [!IMPORTANT]
> #### `TargetInvocationException`
>
> Jeśli wywoływana metoda rzuci wyjątek, `MethodInfo.Invoke` nie przekazuje go bezpośrednio – opakowuje go w `TargetInvocationException`. Oryginalny wyjątek jest dostępny we właściwości `InnerException`:
>
> ```csharp
> catch (TargetInvocationException ex) when (ex.InnerException is { } inner)
> {
>     WriteError($"{inner.GetType().Name}: {inner.Message}");
> }
> ```
>
> Alternatywnie możesz wywołać `Invoke(target, BindingFlags.DoNotWrapExceptions, null, arguments, null)` – wyjątek jest wtedy rzucany bez opakowania.

#### Polecenie inspect

Dodaj trzeci moduł, statyczną klasę `Commands/DiagnosticsCommands` (`[Description("Diagnostic commands.")]`), z poleceniem `inspect <typeName>` zwracającym `TypeInspector.Describe` dla podanego typu. Najpierw spróbuj `Type.GetType(typeName)` – znajduje typy po pełnej nazwie w bieżącym assembly i w bibliotece podstawowej (np. `System.Text.StringBuilder`). W przeciwnym razie przeszukaj wszystkie wczytane assembly (`AppDomain.CurrentDomain.GetAssemblies()`) w poszukiwaniu typu o pasującym `FullName` lub `Name`.

Na koniec zmień `Program.cs`:

```csharp
var shell = new CommandShell();
shell.Registry.RegisterAssembly(Assembly.GetExecutingAssembly());
shell.Run();
```

Przykładowa sesja (polecenie `help` wypisuje teraz dwa kolejne moduły: `CommandShell` z poleceniami wbudowanymi i `DiagnosticsCommands`):

```text
Reflection shell. Type 'help' to list commands, 'exit' to quit.
> add 2 3.5
5.5
> power 3
9
> round 2.5
2
> r 2.5 0 AwayFromZero
3
> round 3.14159 2 up
Error: 'up' is not one of: ToEven, AwayFromZero, ToZero, ToNegativeInfinity, ToPositiveInfinity.
> avg 1 2 3 4
2.5
> days 2026-01-01 2026-12-24
357
> div 7 0
Error: DivideByZeroException: Attempted to divide by zero.
> add 2
Error: Missing value for parameter 'b'.
> add two 3
Error: Cannot convert 'two' to double.
> repeat ab 3 -
ab-ab-ab
> wc "the quick brown fox"
TextStats
  Characters: 19
  Words: 4
  Vowels: 5
> m+ 5
5
> foo
Error: Unknown command 'foo'. Type 'help' to list commands.
> exit
```

> [!TIP]
> Wypróbuj `inspect TextStats`, aby zobaczyć składowe generowane przez kompilator dla rekordu: konstruktor kopiujący i `<Clone>$` (używane przez `with`), `EqualityContract`, `PrintMembers`, `Deconstruct`, `Equals`… Akcesory `init` są wyświetlane jako `set` – dla środowiska uruchomieniowego `init` to zwykły setter oznaczony wymaganym modyfikatorem (`IsExternalInit`).

> [!NOTE]
> #### Etap 3
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.03" >}}

### Etap 4: Assembly i pluginy

Dotychczas wszystkie polecenia były wkompilowane w powłokę. Teraz podzielimy rozwiązanie na cztery projekty:
- `Shell.Abstractions` – biblioteka klas z atrybutami; wspólny kontrakt między hostem a pluginami,
- `Shell` – aplikacja hosta, odwołująca się tylko do `Shell.Abstractions`,
- `MathPlugin` i `TextPlugin` – pluginy z modułami poleceń. Odwołują się tylko do `Shell.Abstractions` i są wczytywane przez hosta w czasie działania z katalogu `plugins`.

Host nie zna pluginów w czasie kompilacji, a pluginy nie znają hosta. Nowy plugin można dodać, kopiując go do katalogu `plugins`, bez ponownej kompilacji powłoki.

#### Nowe projekty

Utwórz biblioteki, dodaj je do rozwiązania i dodaj referencję do kontraktu:

```bash
dotnet new classlib -n Shell.Abstractions
dotnet new classlib -n MathPlugin
dotnet new classlib -n TextPlugin
dotnet sln add Shell.Abstractions
dotnet sln add MathPlugin
dotnet sln add TextPlugin
dotnet add Shell reference Shell.Abstractions
```

Usuń wygenerowane pliki `Class1.cs` i przenieś kod:
- `Shell/Abstractions/*.cs` → `Shell.Abstractions/` (przestrzeń nazw pozostaje `Shell.Abstractions`),
- `Shell/Commands/MathCommands.cs` → `MathPlugin/` (przestrzeń nazw `MathPlugin`),
- `Shell/Commands/TextCommands.cs` → `TextPlugin/` (przestrzeń nazw `TextPlugin`).

`DiagnosticsCommands` pozostaje w hoście.

> [!TIP]
> Dlaczego nie `Plugins.Math`? Wewnątrz przestrzeni nazw, której ostatni człon to `Math`, nazwa `Math` oznacza tę przestrzeń nazw, a `Math.Pow(x, n)` powoduje błąd CS0234 – kompilator szuka `Pow` w przestrzeni nazw `Plugins.Math`, a nie w klasie `System.Math`.

#### Budowanie pluginów

Zastąp zawartość pliku `MathPlugin/MathPlugin.csproj` (i analogicznie `TextPlugin.csproj`, z wersją `1.0.0` i własnym opisem):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <Version>1.2.0</Version>
    <Description>Arithmetic commands for the reflection shell.</Description>
  </PropertyGroup>

  <ItemGroup>
    <!-- Compile against the shared contract, but do not copy it to the output: the host provides it at run time -->
    <ProjectReference Include="..\Shell.Abstractions\Shell.Abstractions.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>

  <!-- After each build copy the plugin to the host's output directory: plugins/MathPlugin/ -->
  <Target Name="CopyToShellPlugins" AfterTargets="Build">
    <ItemGroup>
      <PluginFiles Include="$(OutDir)**\*" />
    </ItemGroup>
    <Copy SourceFiles="@(PluginFiles)"
          DestinationFolder="$(MSBuildThisFileDirectory)..\Shell\bin\$(Configuration)\$(TargetFramework)\plugins\$(MSBuildProjectName)\%(RecursiveDir)"
          SkipUnchangedFiles="true" />
  </Target>

</Project>
```

- `EnableDynamicLoading` oznacza bibliotekę jako komponent wczytywany dynamicznie (m.in. używane przez nią pakiety NuGet są kopiowane do jej katalogu wyjściowego, obok pluginu).
- `<Private>false</Private>` i `<ExcludeAssets>runtime</ExcludeAssets>` sprawiają, że plugin jest kompilowany z użyciem `Shell.Abstractions`, ale plik `Shell.Abstractions.dll` **nie** jest kopiowany do katalogu wyjściowego pluginu. W czasie działania plugin musi korzystać z kopii kontraktu należącej do hosta – za chwilę wyjaśnimy dlaczego.
- Target `CopyToShellPlugins` (o targetach MSBuild przeczytasz w [Tutorialu 2]({{< ref "/labs/02.msbuild" >}})) kopiuje zbudowany plugin do `Shell/bin/<Konfiguracja>/net10.0/plugins/<NazwaPluginu>/`. Każdy plugin dostaje osobny katalog, aby zależności różnych pluginów się nie mieszały.

Dodaj do `Shell/Shell.csproj` referencje do pluginów z `ReferenceOutputAssembly="false"`:

```xml
<!-- The host does not reference the plugins: these references only make MSBuild build (and copy) them first -->
<ItemGroup>
  <ProjectReference Include="..\MathPlugin\MathPlugin.csproj" ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\TextPlugin\TextPlugin.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
```

Powłoka nadal nie może używać w swoim kodzie żadnego typu z pluginów, ale dzięki tym referencjom `dotnet run --project Shell` zawsze najpierw buduje pluginy.

> [!IMPORTANT]
> #### Metadane assembly
>
> Właściwości `Version` i `Description` z pliku projektu trafiają do manifestu assembly. Podczas budowania SDK generuje plik `obj/Debug/net10.0/MathPlugin.AssemblyInfo.cs` z atrybutami na poziomie assembly, takimi jak `[assembly: AssemblyVersion("1.2.0.0")]`, `[assembly: AssemblyInformationalVersion("1.2.0")]` i `[assembly: AssemblyDescription("…")]`. Można je odczytać jak każde inne atrybuty:
>
> ```csharp
> string? version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
> ```
>
> `assembly.GetName()` zwraca tożsamość assembly (`Name`, `Version`), a `GetReferencedAssemblies()` – listę assembly, od których ono zależy, zapisaną w manifeście.

#### Wczytywanie pluginów

Każdy plugin zostanie wczytany do własnego `AssemblyLoadContext` (zob. wykład [Assembly]({{< ref "/lectures/08.reflection/assembly" >}})). Utwórz klasę `PluginLoadContext`:

```csharp
// Every plugin is loaded into its own context, so it can bring its own versions of dependencies
public sealed class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(name: Path.GetFileNameWithoutExtension(pluginPath))
{
    // Finds dependencies of the plugin using its .deps.json file
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // The shared contract must come from the default context (the host's copy).
        // A second copy of Shell.Abstractions would define a second, different CommandAttribute type.
        if (assemblyName.Name == typeof(CommandAttribute).Assembly.GetName().Name)
        {
            return null; // null = let the default context load it
        }

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }
}
```

Środowisko uruchomieniowe wywołuje `Load` za każdym razem, gdy kod pluginu potrzebuje innego assembly. Zwrócenie `null` oznacza, że assembly zostanie wczytane przez kontekst domyślny – ten, w którym znajduje się host i jego zależności.

Następnie:
1. Utwórz klasę `PluginManager` z konstruktorem głównym przyjmującym ścieżkę do katalogu pluginów, właściwością `IReadOnlyList<Assembly> Assemblies` i metodą `void LoadAll()`. Dla każdego podkatalogu katalogu pluginów (posortowanych po nazwie) zawierającego plik `<NazwaKatalogu>.dll` utwórz nowy `PluginLoadContext` i wczytaj assembly za pomocą `context.LoadFromAssemblyPath(path)`. Jeśli katalog pluginów nie istnieje, nic nie rób.
2. W `CommandShell` dodaj parametr konstruktora `string pluginsDirectory` oraz prywatną metodę `LoadCommands()`, wywoływaną z konstruktora, która rejestruje polecenia wbudowane (`RegisterModule(this)`), polecenia z assembly hosta (`RegisterAssembly(typeof(CommandShell).Assembly)`), a następnie wczytuje pluginy i rejestruje polecenia z każdego assembly pluginu, wypisując `Loaded plugin <Nazwa> <Wersja>`.
3. Dodaj polecenie wbudowane `plugins`, które dla każdego pluginu wypisuje: pełną nazwę assembly (`FullName`), opis, wersję informacyjną, lokalizację względem `AppContext.BaseDirectory` (`Path.GetRelativePath`), nazwę jego kontekstu (`AssemblyLoadContext.GetLoadContext(assembly)`), assembly, do których się odwołuje, oraz nazwy dostarczanych poleceń (poleceń, dla których `Method.DeclaringType.Assembly` to assembly pluginu).
4. Uprość `Program.cs` – pluginy są wyszukiwane obok pliku wykonywalnego, a nie w bieżącym katalogu:

```csharp
var shell = new CommandShell(Path.Combine(AppContext.BaseDirectory, "plugins"));
shell.Run();
```

Zbuduj całe rozwiązanie i uruchom powłokę:

```text
Loaded plugin MathPlugin 1.2.0.0
Loaded plugin TextPlugin 1.0.0.0
Reflection shell. Type 'help' to list commands, 'exit' to quit.
> plugins
MathPlugin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
  Description:     Arithmetic commands for the reflection shell.
  Product version: 1.2.0
  Location:        plugins/MathPlugin/MathPlugin.dll
  Load context:    MathPlugin
  References:      System.Runtime 10.0.0.0, Shell.Abstractions 1.0.0.0, System.Linq 10.0.0.0
  Commands:        add, avg, days, div, m+, mr, pow, round
TextPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  ...
```

Jeśli Twój projekt znajduje się w repozytorium git, SDK dopisuje do wersji informacyjnej skrót bieżącego commita, np. `1.2.0+5e0630ba…`. `help` pokazuje teraz w linii `Defined in` wartość `MathPlugin.MathCommands (MathPlugin)`, a `inspect` działa także dla typów z pluginów – wypróbuj `inspect MathCommands`.

> [!IMPORTANT]
> #### Tożsamość typów
>
> Dla środowiska uruchomieniowego typ jest identyfikowany nie tylko przez pełną nazwę, ale także przez **instancję assembly**, z której został wczytany. Jeśli `Shell.Abstractions.dll` zostanie wczytane dwukrotnie – do kontekstu domyślnego (przez hosta) i do kontekstu pluginu – powstaną dwa różne typy `CommandAttribute` o identycznych nazwach. `GetCustomAttribute<CommandAttribute>()` i `IsDefined` wywołane przez hosta (z jego własnym `CommandAttribute`) nie znajdą w pluginie niczego, a plugin po cichu nie dostarczy żadnych poleceń. Rzutowanie takiego atrybutu (lub obiektu implementującego interfejs z kontraktu) kończy się mylącym komunikatem:
>
> ```text
> [A]Shell.Abstractions.CommandAttribute cannot be cast to [B]Shell.Abstractions.CommandAttribute.
> Type A originates from 'Shell.Abstractions, Version=1.0.0.0, ...' in the context '"MathPlugin" Shell.PluginLoadContext #0' ...
> Type B originates from 'Shell.Abstractions, Version=1.0.0.0, ...' in the context 'Default' ...
> ```
>
> Mamy przed tym dwa zabezpieczenia: plugin nie dostarcza własnej kopii kontraktu (`Private=false`), a `PluginLoadContext.Load` przekazuje wczytanie kontraktu do kontekstu domyślnego. Sprawdź to: usuń oba zabezpieczenia (elementy `<Private>` i `<ExcludeAssets>` oraz instrukcję `if` w `Load`), przebuduj rozwiązanie i zobacz, co polecenie `plugins` pokazuje dla `MathPlugin`.

> [!NOTE]
> #### Etap 4
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.04" >}}

### Etap 5: Wydajność i zwalnianie pluginów

W ostatnim etapie zmierzymy koszt refleksji i nauczymy się zwalniać pluginy, aby można je było przebudować i wczytać ponownie bez restartowania powłoki.

#### Koszt refleksji

Wywołanie przez refleksję jest dużo wolniejsze od zwykłego. Wyszukanie składowej po nazwie oznacza przeszukiwanie metadanych, a `MethodInfo.Invoke` musi sprawdzić liczbę i typy argumentów, spakować typy bezpośrednie do tablicy `object?[]` i rozpakować wynik. Są dwa proste sposoby na zmniejszenie tego kosztu:
- **Zapamiętywanie obiektów `MemberInfo`** – wyszukaj metodę raz i przechowuj jej `MethodInfo` (nasz rejestr już to robi).
- **Zamiana metody na delegat** – jeśli sygnatura jest znana w czasie kompilacji, `method.CreateDelegate<Func<int, int>>()` tworzy delegat, który jest tak szybki jak zwykłe wywołanie. Jeśli sygnatura nie jest znana, `MethodInvoker.Create(method)` (.NET 8+) przygotowuje obiekt wywołujący, który jest szybszy od `Invoke`, choć nadal operuje na typie `object`.

Dodaj do `DiagnosticsCommands` polecenie `bench [iterations=1000000]`. Mierzy ono (`Stopwatch`) wywoływanie prywatnej statycznej metody `int Twice(int x) => 2 * x;` (oznacz ją `[MethodImpl(MethodImplOptions.NoInlining)]`, aby kompilator JIT nie mógł wstawić jej kodu w miejsce bezpośrednich wywołań – tzw. *inlining*) na pięć sposobów: bezpośrednie wywołanie, delegat z `CreateDelegate`, `MethodInvoker`, `MethodInfo.Invoke` z zapamiętanym `MethodInfo` oraz `GetMethod` + `Invoke` w każdej iteracji. Polecenie zwraca tabelę z całkowitym czasem i czasem jednego wywołania. Twoje wyniki będą inne, ale proporcje powinny być podobne:

```text
> bench
Direct call               5.4 ms      5.4 ns/call
Delegate                  6.8 ms      6.8 ns/call
MethodInvoker            31.8 ms     31.8 ns/call
MethodInfo.Invoke        53.0 ms     53.0 ns/call
GetMethod + Invoke      116.6 ms    116.6 ns/call
```

Teraz użyj `MethodInvoker` w powłoce. Dodaj do `CommandInfo` właściwość, która tworzy obiekt wywołujący przy pierwszym użyciu, a potem go przechowuje – słowo kluczowe `field` z C# 14 (zob. wykład [Klasy]({{< ref "/lectures/04.creating-types/classes" >}})) odwołuje się do pola wygenerowanego przez kompilator:

```csharp
// Created on first use and then reused - faster than MethodInfo.Invoke
public MethodInvoker Invoker => field ??= MethodInvoker.Create(Method);
```

W `CommandShell.Execute` zastąp `command.Method.Invoke(...)` wywołaniem `command.Invoker.Invoke(command.Target, arguments)`. W odróżnieniu od `MethodInfo.Invoke`, `MethodInvoker` **nie** opakowuje wyjątków w `TargetInvocationException`, dlatego przechwytuj bezpośrednio `Exception`.

#### Zwalnianie pluginów

> [!IMPORTANT]
> #### Konteksty z możliwością zwolnienia
>
> Kontekst utworzony z `isCollectible: true` można zwolnić razem ze wszystkimi wczytanymi do niego assembly. Jednak `Unload()` jedynie rozpoczyna ten proces – zwalnianie odbywa się na zasadzie współpracy. Garbage Collector zwalnia kontekst dopiero wtedy, gdy nie ma już referencji ani do niego, ani do niczego, co pochodzi z jego assembly: obiektów ich typów, obiektów `Type` i `MethodInfo`, delegatów, wartości przechowywanych w polach statycznych hosta, wątków wykonujących kod pluginu… Słaba referencja `WeakReference` do kontekstu pozwala sprawdzić, czy został on rzeczywiście zwolniony: wywołuj `GC.Collect()` i `GC.WaitForPendingFinalizers()`, dopóki `weakReference.IsAlive` nie zmieni wartości na `false`.
>
> Unikaj przechowywania referencji w zmiennych lokalnych metody, która czeka na GC: w konfiguracji Debug kompilator JIT wydłuża czas życia zmiennych lokalnych do końca metody. Zwalnianie przeprowadź w osobnej metodzie oznaczonej `[MethodImpl(MethodImplOptions.NoInlining)]`.

Zaimplementuj polecenie `reload`:
1. Spraw, aby `PluginLoadContext` dało się zwolnić: przekaż `isCollectible: true` do konstruktora bazowego.
2. W `PluginManager.LoadAll` zastąp `LoadFromAssemblyPath` wywołaniem `context.LoadFromStream(stream)` (`using FileStream stream = File.OpenRead(path);`). Środowisko uruchomieniowe działa wtedy na kopii w pamięci, a plik nie jest ani blokowany (w systemie Windows), ani mapowany do pamięci, więc proces budowania może go nadpisać w trakcie działania powłoki. Efekt uboczny: `Assembly.Location` jest pustym napisem – w poleceniu `plugins` wypisz zamiast tego `(loaded from memory)`.
3. Dodaj do `PluginManager` metodę `List<WeakReference> UnloadAll()` (oznaczoną `NoInlining`): dla każdego assembly pluginu pobierz jego kontekst (`AssemblyLoadContext.GetLoadContext`), wywołaj `Unload()` i zapamiętaj `WeakReference` do kontekstu; na koniec wyczyść listę assembly.
4. Dodaj do `CommandRegistry` metodę `Clear()` – rejestr musi zostać wyczyszczony przed zwolnieniem pluginów, ponieważ każde `CommandInfo` odwołuje się do `MethodInfo` z pluginu i do instancji jego modułu.
5. Dodaj polecenie wbudowane `reload`: wyczyść rejestr i zwolnij pluginy (w osobnej metodzie oznaczonej `NoInlining`), wywołuj w pętli `GC.Collect()` i `GC.WaitForPendingFinalizers()` (co najwyżej 10 razy), dopóki wszystkie słabe referencje nie przestaną być aktywne, wypisz `Unloaded N plugins.` (lub ile z nich nadal jest w pamięci) i ponownie wywołaj `LoadCommands()`.

Po ponownym wczytaniu stan modułów jest tracony – to zupełnie nowe obiekty z nowych assembly:

```text
> m+ 5
5
> m+ 2.5
7.5
> mr
7.5
> reload
Unloaded 2 plugins.
Loaded plugin MathPlugin 1.2.0.0
Loaded plugin TextPlugin 1.0.0.0
> mr
0
```

Teraz spróbuj wczytać zmodyfikowany kod. Pozostaw uruchomioną powłokę, dodaj do `MathCommands` polecenie `sqrt <x>` i zbuduj w drugim terminalu tylko plugin: `dotnet build MathPlugin` (nie buduj całego rozwiązania – w systemie Windows plik `Shell.dll` jest zablokowany przez działający proces). Następnie wpisz w powłoce `reload` – `sqrt 2` powinno zwrócić `1.4142135623730951`.

> [!WARNING]
> Każda referencja do kodu pluginu przechowywana przez hosta uniemożliwia jego zwolnienie. Na przykład gdyby powłoka zapamiętywała wynik ostatniego polecenia w polu statycznym, a ostatnim poleceniem było `wc` (zwracające obiekt `TextStats` zdefiniowany w `TextPlugin`), `reload` zgłosiłoby `1 of 2 plugins are still in memory.` Typowi winowajcy to statyczne pamięci podręczne indeksowane typem `Type` (np. pamięć podręczna tablic `PropertyInfo[]` w `ObjectDumper`), procedury obsługi zdarzeń oraz działające wątki i timery.

> [!NOTE]
> #### Etap 5
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.05" >}}

### Kierunki dalszego rozwoju

Powłokę napisaną w ramach tutoriala można rozwijać w wielu kierunkach. Poniżej zebraliśmy kilka pomysłów wartych uwagi:

- **Argumenty nazwane:** Obsłuż opcje takie jak `repeat ab --times 3`, dopasowywane do parametrów po `ParameterInfo.Name` – tak działają biblioteki do parsowania argumentów wiersza poleceń.
- **Wstrzykiwanie zależności:** Zamiast wymagać konstruktora bezparametrowego, znajdź konstruktor, którego parametry powłoka potrafi dostarczyć (np. rejestr lub `TextWriter` do wypisywania wyników), i wywołaj go przez `ConstructorInfo.Invoke` – tak obiekty tworzą kontenery DI.
- **Polecenia asynchroniczne:** Pozwól na polecenia zwracające `Task` lub `Task<T>`: poczekaj na zwrócone zadanie (`await`) i odczytaj jego właściwość `Result` za pomocą refleksji.
- **Automatyczne przeładowanie:** Obserwuj katalog `plugins` za pomocą `FileSystemWatcher` i wczytuj pluginy ponownie po każdej zmianie pliku.
- **Pluginy z zależnościami:** Dodaj plugin korzystający z pakietu NuGet (np. *Figgle* z wykładu). `AssemblyDependencyResolver` wczytuje pakiet z katalogu pluginu do jego kontekstu, więc różne pluginy mogą nawet używać różnych wersji tego samego pakietu.
- **Wykrywanie w czasie kompilacji:** Refleksja słabo współpracuje z przycinaniem (*trimming*) i Native AOT. *Generatory źródeł* mogą znaleźć metody `[Command]` podczas kompilacji i wygenerować kod rejestrujący, dzięki czemu refleksja w czasie wykonania nie jest potrzebna.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/07.assembly-reflection/example1" >}})
- [Zadanie 2]({{< ref "/labs/07.assembly-reflection/example2" >}})
