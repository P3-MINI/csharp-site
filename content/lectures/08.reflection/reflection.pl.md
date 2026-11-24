---
title: "Refleksja"
weight: 20
---

# Refleksja

Refleksja to mechanizm, który pozwala programowi na analizowanie swojej własnej struktury w czasie wykonania. Dzięki refleksji można uzyskiwać informację o typach i ich składowych. Można na przykład pobrać listę konstruktorów danej klasy i stworzyć jej instancję, nie znając nazwy tej klasy w czasie kompilacji. Atrybuty wspomniane wcześniej są bezużyteczne, dopóki ktoś ich nie odczyta; refleksja pozwala na ich odczytanie i podjęcie na ich podstawie jakichś działań. Za pomocą refleksji można również przeglądać skompilowany kod metod.

> Refleksja jest formą metaprogramowania. W odróżnieniu od szablonów z C++, refleksja działa w czasie wykonania. C# dostarcza także formę metaprogramowania działającą w czasie kompilacji. Generatory źródeł (*Source Generators*) pozwalają na analizę istniejącego kodu w czasie kompilacji i dodawanie do niego nowych plików źródłowych.

Należy pamiętać, że operacje oparte na refleksji są znacznie wolniejsze od bezpośredniego wywołania kodu. Kod używający refleksji jest często trudny do zrozumienia, a jeszcze trudniejszy do debugowania.

Mimo wszystko, refleksja znajduje swoje zastosowanie. Niektóre problemy znacznie łatwiej rozwiązać za pomocą refleksji:

* **Testy jednostkowe** - możemy znaleźć wszystkie metody oznaczone atrybutem `[Test]` i je uruchomić.
* **Serializacja** - możemy przeanalizować składowe obiektu i na ich podstawie skonwertować je do odpowiedniego formatu, np. JSON.
* **System pluginów** - możemy dynamicznie doczytywać zewnętrzne assembly w poszukiwaniu klas implementujących określony interfejs, np. `IPlugin`.

## Przestrzenie nazw

Mechanizm refleksji jest dostępny za pomocą klas z przestrzeni nazw:

- `System.Reflection`
- `System.Reflection.Emit`

W przestrzeni nazw `System.Reflection` znajdują się klasy, które reprezentują różne elementy kodu i pozwalają na ich inspekcję i interakcję z nimi:

```text
├── Assembly
│   └── Module
│       └── Type (System.Type)
│           └── MemberInfo
│               ├── MethodBase
│               │   ├── ConstructorInfo
│               │   │   └── ParameterInfo
│               │   └── MethodInfo
│               │       └── ParameterInfo
│               ├── PropertyInfo
│               ├── FieldInfo
│               └── EventInfo
└── CustomAttributeData
```

> Istnienie modułów można najczęściej pominąć. W .NET (Core) assembly (zestaw) składa się zawsze z dokładnie jednego modułu (pliku z kodem pośrednim `.dll` lub `.exe`). Wielomodułowe zestawy są obsługiwane wyłącznie przez .NET Framework (a Visual Studio i tak nie wspiera ich tworzenia); bardzo rzadko są też przydatne, na przykład gdy część kodu jednej biblioteki jest napisana w innym języku również kompilowanym do kodu pośredniego. Wtedy wiele modułów oddzielnie kompilowanych jest zebranych w jeden zestaw. Zazwyczaj moduły i assembly są traktowane równoważnie (obydwa jako pliki z kodem pośrednim).

Przestrzeń nazw `System.Reflection.Emit` służy do dynamicznego generowania nowego kodu w czasie wykonania programu. Można tworzyć własne klasy, metody i ich kod i korzystać z nich tak jakby były one częścią oryginalnego skompilowanego programu. Proces ten polega na emitowaniu instrukcji języka pośredniego (*CIL*). Pominiemy tę część refleksji. Dynamiczne generowanie kodu jest używane na przykład do tworzenia *mocków* do testów jednostkowych lub tworzenia skompilowanych wyrażeń regularnych.

## Pobieranie informacji o typie

Punktem wejścia do refleksji jest zazwyczaj pobranie obiektu reprezentującego typ (`System.Type`). Możemy to zrobić na kilka sposobów, dwa z nich poznaliśmy już wcześniej.

Dla klasy `Person`:

```csharp
public class Person
{
    private string _id;
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public DateTime Birthday { get; set; }
    
    public Person(string firstName, string lastName, DateTime? birthday = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Birthday = birthday ?? DateTime.Now;
        _id = GetId(Birthday);
    }
    
    public bool IsAdult()
    {
        return Birthday.AddYears(18) < DateTime.Today;
    }

    public static string GetId(DateTime birthday)
    {
        return $"{birthday.Year:0000}{birthday.Month:00}{birthday.Day:00}{Random.Shared.Next()%100000:00000}";
    }
}
```

Możemy pobrać obiekt typu `Type` na trzy sposoby:

1. Używając operatora `typeof`:

   ```csharp
   Type personType = typeof(Person);
   ```

   Operator `typeof` działa w czasie kompilacji.

2. Używając metody `GetType` na instancji obiektu:

   ```csharp
   Person person = new Person("Alice", "Smith");
   Type personType = person.GetType();
   ```

3. Używając statycznej metody `Type.GetType(string name)`:

   ```csharp
   Type? personType = Type.GetType("Reflection.Person");
   ```

   Możemy to zrobić, nie znając szukanego typu w czasie kompilacji, wczytując nazwę klasy np. z pliku konfiguracyjnego. Nazwa musi zawierać przestrzeń nazw; dla typów z innych assembly niż wywołujące i `System.Private.CoreLib` wymagana jest nazwa uzupełniona o nazwę assembly (*assembly-qualified name*, np. `"System.Text.Json.JsonSerializer, System.Text.Json"`). Jeżeli typ nie zostanie znaleziony, metoda zwraca `null`.

### Pobieranie informacji o składowych

Niezależnie od sposobu pozyskania obiektu `Type` możemy następnie przejrzeć składowe typu. Możemy przejrzeć wszystkie składowe za pomocą `GetMembers` lub ich szczególny rodzaj: `GetFields`, `GetProperties`, `GetEvents`, `GetMethods`, `GetConstructors`, `GetNestedTypes`.

```csharp
MemberInfo[] members = typeof(Person).GetMembers();
foreach (MemberInfo member in members)
{
    Console.WriteLine($"{member.MemberType,16}: {member}");
}
```

> Refleksja pozwala łamać enkapsulację; za jej pomocą można uzyskać dostęp do prywatnych składowych. Żeby refleksja zwracała prywatne składowe, trzeba przekazać do przeciążonej metody flagi `BindingFlags.NonPublic | BindingFlags.Instance`:
> ```csharp
> typeof(Person).GetMembers(BindingFlags.NonPublic | BindingFlags.Instance);
> ```

Alternatywnie można skorzystać z API `TypeInfo` (`GetTypeInfo()`), którego właściwości `Declared*` zwracają sekwencje `IEnumerable`:

```csharp
IEnumerable<MemberInfo> members = typeof(Person)
    .GetTypeInfo()
    .DeclaredMembers;

foreach (MemberInfo member in members)
{
    Console.WriteLine($"{member.MemberType,16}: {member}");
}
```

> [!INFO]
> `DeclaredMembers` nie listuje odziedziczonych składowych, ale listuje składowe niepubliczne i statyczne.

Jeżeli znamy nazwę składowej, to możemy ją znaleźć używając jednej z metod: `GetMember`, `GetField`, `GetProperty`, `GetEvent`, `GetMethod`, `GetNestedType`. Na podstawie podanej listy typów parametrów możemy też znaleźć odpowiedni konstruktor, używając `GetConstructor`.

### Wywoływanie składowych

Metody, konstruktory oraz *gettery* i *settery* dla właściwości można wywoływać po pobraniu za pomocą refleksji.

1. Wywoływanie metod:
   ```csharp
   Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
   MethodInfo? method = typeof(Person).GetMethod("IsAdult");
   if (method is null)
   {
       Console.WriteLine("Method `IsAdult` not found");
   }
   else
   {
       if (method.GetParameters().Length == 0)
       {
           bool? isAdult = method.Invoke(person, [/*parameters*/]) as bool?;
           Console.WriteLine($"Is Adult: {isAdult}");
       }
       else
       {
           Console.WriteLine("Method `IsAdult` is not parameterless");
       }
   }
   ```
   > [!NOTE]
   > Pierwszy parametr metody `Invoke` określa instancję, na rzecz której ma zostać wywołana metoda. Dla metod statycznych można tam przekazać wartość `null`.
2. Wywoływanie konstruktorów:
   ```csharp
   ConstructorInfo? constructor = typeof(Person)
       .GetConstructor([typeof(string), typeof(string), typeof(DateTime?)]);
   Person? person = constructor?.Invoke(["John", "Doe", DateTime.Now.AddYears(-42)]) as Person;
   Console.WriteLine($"Name: {person?.FullName}, Birthday: {person?.Birthday:d}");
   ```
   Alternatywnie można użyć dużo prostszej metody `CreateInstance` ze statycznej klasy pomocniczej `Activator`:
   ```csharp
   Person? person = Activator.CreateInstance(typeof(Person), "John", "Doe", DateTime.Now.AddYears(-42)) as Person;
   ```
3. Wywoływanie właściwości:
   ```csharp
   Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
   PropertyInfo? property = typeof(Person).GetProperty("Birthday");
   if (property is null)
   {
       Console.WriteLine("Property `Birthday` not found");
   }
   else
   {
       DateTime? birthday = property.GetValue(person) as DateTime?;
       property.SetValue(person, birthday?.AddYears(-2));
       Console.WriteLine($"Age: {DateTime.Now.Year - person.Birthday.Year}"); // 23
   }
   ```
4. Ustawianie pól:
   ```csharp
   Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
   FieldInfo? field = typeof(Person).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
   if (field is null)
   {
       Console.WriteLine("Field `_id` not found");
   }
   else
   {
       string? id = field.GetValue(person) as string;
       Console.WriteLine($"Id before: {id}");
       field.SetValue(person, id?.Substring(0, 8));
       Console.WriteLine($"Id after: {field.GetValue(person)}");
   }
   ```

### Późne wiązanie (*late binding*)

Metody pobrane za pomocą refleksji będą działać zauważalnie wolniej. *Runtime* musi sprawdzić, czy argumenty pasują, a jeżeli argumenty są typu bezpośredniego, to następuje ich pakowanie i rozpakowanie, jako że `MethodInfo.Invoke` operuje na tablicy obiektów. Ponadto samo wyszukanie metody jest kosztowne.

Znalezione `MethodInfo` można przypisać do delegata, przez co unikniemy części problemów z wydajnością. Wywołania przez takiego delegata będą zauważalnie szybsze.

```csharp
Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
MethodInfo method = typeof(Person).GetMethod(nameof(Person.IsAdult))!;
// Binding to a delegate:
Func<bool> isAdult = (Func<bool>)Delegate
    .CreateDelegate(typeof(Func<bool>), person, method);
for (int i = 0; i < 1_000_000; i++)
{
    isAdult();
}
```

### `UnsafeAccessor` (.NET 8)

Refleksji często używa się tylko po to, żeby dostać się do prywatnej składowej typu, którego nie możemy zmodyfikować (np. w testach lub serializatorach). Od .NET 8 można to zrobić bez refleksji, za pomocą atrybutu `[UnsafeAccessor]`. Deklarujemy metodę `static extern`, a jej ciało dostarcza *runtime* — odwołuje się ono bezpośrednio do wskazanej składowej, więc takie wywołanie jest tak samo szybkie jak zwykłe:

```csharp
Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
ref string id = ref PersonAccessors.Id(person); // a reference to the private field
id = id.Substring(0, 8);
Console.WriteLine(PersonAccessors.Id(person));

static class PersonAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_id")]
    public static extern ref string Id(Person person);
}
```

Pierwszy parametr określa typ, do którego składowej się odwołujemy (dla składowych instancyjnych jest to obiekt używany jako `this`). Pozostałe rodzaje składowych (`UnsafeAccessorKind`) to `StaticField`, `Method`, `StaticMethod` i `Constructor`. Składowe typów generycznych i metody generyczne są wspierane od .NET 9. W odróżnieniu od refleksji składowa musi być znana w czasie kompilacji, dlatego analiza pod kątem trimmingu i Native AOT ([patrz niżej]({{< ref "#refleksja-a-trimming-i-native-aot" >}})) nie zgłasza dla takiego kodu ostrzeżeń.

## Refleksja nad assembly

Na assembly można wywołać `GetTypes` lub `GetType`, żeby uzyskać informacje o typach, które się w nim znajdują. Pobrać assembly można za pomocą jednej z czterech statycznych metod:

* **Assembly.GetEntryAssembly** pobiera assembly, które zawiera metodę startową `Main`.
* **Assembly.GetCallingAssembly** pobiera assembly, z którego została wywołana obecnie wykonywana metoda.
* **Assembly.GetExecutingAssembly** pobiera assembly, w którym jest obecnie wykonywana metoda.
* **Assembly.GetAssembly** pobiera assembly, które zawiera wskazany typ.

```csharp
Assembly? assembly;

assembly = Assembly.GetEntryAssembly();
assembly = Assembly.GetCallingAssembly();
assembly = Assembly.GetExecutingAssembly();
assembly = Assembly.GetAssembly(typeof(Person));

if (assembly is null) return;

foreach (var type in assembly.GetTypes())
{
    Console.WriteLine(type.FullName);
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/reflection/Reflection" >}}

## Typy generyczne

Operator `typeof` można też zastosować do niezwiązanego (otwartego) typu generycznego, np. `typeof(List<>)`. W odróżnieniu od szablonów C++, które są konkretyzowane wyłącznie w czasie kompilacji, taką definicję typu generycznego można w czasie wykonania zamknąć konkretnymi argumentami typu za pomocą `MakeGenericType` (a metodę generyczną analogicznie za pomocą `MethodInfo.MakeGenericMethod`). Liczba po znaku `` ` `` w nazwie typu to liczba jego parametrów typu.

```csharp
Type open = typeof(Dictionary<,>);
Console.WriteLine(open.IsGenericTypeDefinition); // True
Console.WriteLine(open.Name);                    // Dictionary`2

Type closed = open.MakeGenericType(typeof(string), typeof(int));
Console.WriteLine(closed == typeof(Dictionary<string, int>)); // True
Console.WriteLine(string.Join(", ", closed.GetGenericArguments().Select(t => t.Name))); // String, Int32
var dict = (IDictionary<string, int>)Activator.CreateInstance(closed)!;

Console.WriteLine(nameof(Dictionary<,>));        // Dictionary (C# 14)
```

Od C# 14 operator `nameof` przyjmuje także niezwiązane typy generyczne, np. `nameof(List<>)` zwraca `"List"`.

## Własne atrybuty

Możemy definiować własne atrybuty - to klasy dziedziczące po `System.Attribute`. Zwyczajowo ich nazwa powinna kończyć się na `...Attribute`; kompilator pozwala omijać ten sufiks podczas używania atrybutu. Atrybut `AttributeUsage` służy do określenia, co możemy później zrobić z atrybutem. Ma on trzy właściwości:

* **`ValidOn`** (parametr konstruktora typu `AttributeTargets`) - do jakich elementów kodu możemy przyczepić atrybut.
* **`AllowMultiple`** (opcjonalny) - flaga określająca, czy atrybut możemy przyczepiać wiele razy. Domyślnie `false`.
* **`Inherited`** - flaga określająca, czy atrybut będzie dziedziczony w klasach pochodnych i nadpisanych składowych. Domyślnie `true`.

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class BenchmarkAttribute : Attribute
{
    public uint Repetitions { get; }

    public BenchmarkAttribute(uint repetitions = 100)
    {
        Repetitions = repetitions;
    }
}
```

Przyczepione do elementów kodu atrybuty możemy później pobierać za pomocą refleksji. Metoda (rozszerzająca) `GetCustomAttributes<TAttribute>` jest dostępna dla każdego typu reprezentującego element kodu i zwraca nam sekwencję atrybutów. Na podstawie atrybutu i jego danych można podejmować następnie kolejne kroki.

```csharp
public static class Program
{
    public static void Main()
    {
        var methods = typeof(Program).GetMethods(BindingFlags.Public | BindingFlags.Static);
        
        foreach (var method in methods)
        {
            IEnumerable<BenchmarkAttribute> attributes = method.GetCustomAttributes<BenchmarkAttribute>();

            foreach (var attribute in attributes)
            {
                Action? action = (Action?)Delegate.CreateDelegate(typeof(Action), method, false);
                if (action is null)
                {
                    Console.WriteLine($"Method {method.Name} needs to take no parameters, and return void.");
                    continue;
                }

                uint rep = attribute.Repetitions;
                Console.WriteLine($"Found benchmark: {method.Name}");
                Console.WriteLine($"Calling it {attribute.Repetitions} times");
                Stopwatch sw = Stopwatch.StartNew();
                for (uint i = 0; i < rep; i++)
                {
                    action();
                }
                sw.Stop();
                double micro = sw.Elapsed.TotalNanoseconds / rep / 1000;
                Console.WriteLine($"{method.Name} time: {micro:0}μs");
            }
        }
    }

    [Benchmark]
    public static void StringAdd()
    {
        string _ = "";
        for (int i = 0; i < 10_000; i++)
        {
            _ += 'a';
        }
    }
    
    [Benchmark(10000)]
    public static void StringBuilder()
    {
        StringBuilder a = new StringBuilder();
        for (int i = 0; i < 10_000; i++)
        {
            a.Append('a');
        }

        string _ = a.ToString();
    }
    
    [Benchmark()]
    public static void StringJoin()
    {
        string _ = string.Join(string.Empty, Enumerable.Repeat('a', 10_000));
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/reflection/Benchmark" >}}

### Atrybuty generyczne (C# 11)

Od C# 11 klasy atrybutów mogą być generyczne. Zamiast przekazywać typ jako argument (`[Validator(typeof(EmailValidator))]`), można go przekazać jako argument typu, dzięki czemu kompilator może sprawdzić jego ograniczenia:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ValidatorAttribute<T> : Attribute where T : IValidator, new()
{
    public IValidator CreateValidator() => new T();
}

public class User
{
    [Validator<EmailValidator>]
    public string Email { get; set; } = "";
}

public interface IValidator { bool IsValid(object? value); }
public class EmailValidator : IValidator
{
    public bool IsValid(object? value) => value is string s && s.Contains('@');
}
```

Argument typu musi być w pełni znany w czasie kompilacji — np. `[Validator<T>]` z parametrem typu `T` otaczającego typu generycznego jest błędem (`CS8968`).

## Refleksja a trimming i Native AOT

Gdy aplikacja jest publikowana z przycinaniem (*trimming*, `<PublishTrimmed>`) lub kompilowana z wyprzedzeniem do kodu natywnego (Native AOT, `<PublishAot>`, .NET 7+), kompilator statycznie analizuje kod i usuwa typy oraz składowe, które wyglądają na nieużywane. Składowe używane wyłącznie przez refleksję (np. wyszukiwane po nazwie wczytanej z pliku konfiguracyjnego) mogą wtedy nie istnieć w czasie wykonania. Dlatego analizator zgłasza ostrzeżenia (`IL2026`, `IL2057`, `IL2075`, …) dla kodu z refleksją, którego nie potrafi przeanalizować. Aplikacje jednoplikowe (*file-based apps*, `dotnet run app.cs`, .NET 10) mają domyślnie włączone Native AOT, więc te ostrzeżenia pojawiają się także w nich.

Taki kod można oznaczyć atrybutami z przestrzeni nazw `System.Diagnostics.CodeAnalysis`:

* `[DynamicallyAccessedMembers]` - informuje trimmer, które składowe typu przekazanego jako `Type` (parametr, pole, wartość zwracana) będą używane przez refleksję, dzięki czemu zostaną zachowane;
* `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]` - oznaczają metodę jako niekompatybilną z trimmingiem / Native AOT; ostrzeżenie jest wtedy zgłaszane w miejscach jej wywołania.

```csharp
static void PrintMethods(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
{
    foreach (MethodInfo method in type.GetMethods()) // OK: public methods are preserved
    {
        Console.WriteLine(method.Name);
    }
}

PrintMethods(typeof(Person));                     // OK: the trimmer keeps public methods of Person
PrintMethods(Type.GetType(Console.ReadLine()!)!); // warning IL2057: the type is unknown at compile time
```

Dlatego wiele bibliotek zastępuje refleksję [generatorami źródeł]({{< ref "/lectures/13.others/source-generators" >}}), które generują potrzebny kod w czasie kompilacji (np. `System.Text.Json`, `[GeneratedRegex]`).
