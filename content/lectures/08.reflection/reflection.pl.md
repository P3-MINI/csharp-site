---
title: "Refleksja"
weight: 20
---

# Refleksja

Refleksja to mechanizm, który pozwala programowi na analizowanie swojej własnej struktury w czasie wykonania. Dzięki refleksji można uzyskiwać informację o typach i ich składowych. Można na przykład pobrać listę konstruktorów danej klasy i  stworzyć jej instancję nie znając nazwy tej klasy w czasie kompilacji. Atrybuty wspomniane wcześniej są bezużyteczne, aż ktoś ich nie odczyta, refleksja pozwala na ich odczytanie i podjęcie na ich podstawie jakichś działań. Za pomocą refleksji można również przeglądać skompilowany kod metod.

> Refleksja jest formą metaprogramowania. W odróżnieniu od szablonów z C++, refleksja działa w czasie wykoniania. C# dostarcza także formę metaprogramowania działającą w czasie kompilacji. Generatory źródeł (*Source Generators*) pozwalają na analizę istniejącego kodu w czasie kompilacji i dodawaniu do niego nowych plików źródłowych.

Należy pamiętać, że operacje oparte na refleksji są znacznie wolniejsze od bezpośredniego wywołania kodu. Kod używający refleksję jest często trudny do zrozumienia, jeszcze trudniejszy do zdebugowania.

Mimo wszystko, refleksja znajduje swoje zastosowanie. Niektóre problemy znacznie łatwiej rozwiązać jest refleksją:

* **Testy jednostkowe** - możemy znaleźć wszystkie metody oznaczone atrybutem `[Test]` i je uruchomić.
* **Serializacja** - możemy przeanalizować składowe obiektu i na ich podstawie skonwertować je do odpowiedniego formatu, np JSON.
* **System pluginów** - możemy dynamicznie doczytywać zewnętrzne assembly w poszukiwaniu klas implementujących określony interfejs, np. `IPlugin`.

## Przestrzenie nazw

Mechanizm refleksji jest dostępny za pomocą klas dostępnych w przestrzeniach nazw:

- `System.Reflection`
- `System.Reflection.Emit`

W przestrzeni nazw `System.Reflection` znajdują się klasy, które reprezentują różne elementy kodu i pozwalają na ich inspekcję i interakcję z nimi:

```text
│└── Assembly
│    └── Module
│        └── Type (System.Type)
│            └── MemberInfo
│                ├── MethodBase
│                │   ├── ConstructorInfo
│                │   │   └── ParameterInfo
│                │   └── MethodInfo
│                │       └── ParameterInfo
│                ├── PropertyInfo
│                ├── FieldInfo
│                └── EventInfo
└── CustomAttributeData
```

> Istnienie modułów można najczęściej pominąć. W 99.99% przypadków assembly (zestaw) składa się z dokładnie jednego modułu (pliku z kodem pośrednim `.dll` lub `.exe`). Visual Studio nie wspiera tworzenia wielomodułowych zestawów, bardzo rzadko jest to także przydatne, na przykład gdy część kodu jednej biblioteki jest napisana w innym języku również kompilowanym do kodu pośredniego. Wtedy wiele modułów oddzielnie kompilowanych jest zebranych w jeden zestaw. Zazwyczaj moduły i assembly są traktowane równoważnie (obydwa jako pliki z kodem pośrednim).

Przestrzeń nazw `System.Reflection.Emit` służy do dynamicznego generowania nowego kodu w czasie wykonania programu. Można tworzyć własne klasy, metody i ich kod i korzystać z nich tak jakby były one częścią oryginalnego skompilowanego programu. Proces ten polega na emitowaniu instrukcji języka pośredniego (*CIL*). Pominiemy tą część refleksji. Dynamiczne generowanie kodu jest używane na przykład do tworzenia *mocków* do testów jednostkowych, lub tworzenia skompilowanych wyrażeń regularnych.

## Pobieranie informacji o typie

Punktem wejścia do refleksji jest zazwyczaj pobranie obiektu reprezentujący typ (`System.Type`). Możemy to zrobić na kilka sposobów, dwa z nich poznaliśmy już wcześniej.

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

   Możemy to zrobić nie znając szukanego typu w czasie kompilacji, wczytując nazwę klasy np. z pliku konfiguracyjnego

### Pobieranie informacji o składowych:

Niezależnie od sposobu pozyskania obiektu `Type` następnie możemy przejrzeć składowe typu. Możemy przejrzeć wszystkie składowe za pomocą `GetMembers`, lub ich szczególny rodzaj: `GetFields`, `GetProperties`, `GetEvents`, `GetMethods`, `GetConstructors` `GetNestedTypes`.

```csharp
MemberInfo[] members = typeof(Person).GetMembers();
foreach (MemberInfo member in members)
{
    Console.WriteLine($"{member.MemberType,16}: {member}");
}
```

> Refleksja pozwala łamać enkapsulację, za jej pomocą można uzyskać dostęp do prywatnych składowych. Żeby refleksja zwracała prywatne składowe trzeba przekazać do przeciążonej metody flagę `BindingFlags.NonPublic | BindingFlags.Instance`:
> ```csharp
> typeof(Person).GetMembers(BindingFlags.NonPublic);
> ```

Alternatywnie można skorzystać z API `GetTypeInfo()`, zwracające sekwencje `IEnumerable`:

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
> `DeclaredMembers` nie listuje odziedziczonych składowych.

Jeżeli znamy nazwę składowej, to możemy ją znaleźć używając jednej z metod: `GetMember`, `GetField`, `GetProperty`, `GetEvent`, `GetMethod`, `GetNestedType`. Na podstawie podanej listy argumentów możemy też znaleźć odpowiedni konstruktor używając `GetConstructor`.

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
   > Pierwszy parameter metody `Invoke` określa instancję, na rzecz której metoda ma zostać wywołana metoda. Dla metod statycznych można tam przekazać wartość `null`.
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
       Console.WriteLine($"Age: {DateTime.Now.Year - birthday?.Year}");
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

Metody pobrane za pomocą refleksji będą działać zauważalnie wolniej. *Runtime* musi sprawdzić czy argumenty pasują, jeżeli argumenty są typu bezpośredniego, to następuje ich pakowanie i rozpakowanie, jako że `MethodInfo.Invoke` operuje na tablicy obiektów, nie licząc już że samo wyszukanie metody jest kosztowne.

Znalezione `MethodInfo` można przypisać do delegaty, przez co unikniemy części problemów z wydajnością. Wywołania przez taką delegację będą zauważalnie szybsze.

```csharp
Person person = new Person("Alice", "Smith", DateTime.Now.AddYears(-21));
MethodInfo? method = typeof(Person).GetMethod(nameof(Person.IsAdult));
// Binding to a delegate:
Func<bool> isAdult = (Func<bool>)Delegate
    .CreateDelegate(typeof(Func<bool>), person, method);
for (int i = 0; i < 1_000_000; i++)
{
    isAdult();
}
```

## Refleksja nad assembly

Na assembly można wywołać `GetTypes` lub `GetType`, żeby uzyskać informację o typach w nim się znajdujących. Pobrać assembly można za pomocą jednej z czterech statycznych metod:

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

## Własne atrybuty

Możemy definiować własne atrybuty - to klasy dziedziczące po `System.Attribute`. Zwyczajowo ich nazwa powinna kończyć się na `..Attribute`, kompilator pozwala omijać ten suffix podczas używania atrybutu. Atrybut `AttributeUsage` służy do oznaczania co możemy zrobić później z atrybutem, am on trzy właściwości:

* **`AttributeTargets`** - do jakich elementów kodu możemy przyczepić atrybut.
* **`AllowMultiple`** (opcjonalny) - flaga określająca czy atrybut możemy przyczepiać wiele razy. Domyślnie `false`.
* **`Inherited`** - flaga określająca czy atrybut będzie dziedziczony w klasach pochodnych i nadpisanych składowych. Domyślnie `true`.

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class BenchmarkAttribute : Attribute
{
    public int Repetitions { get; }

    public BenchmarkAttribute(int repetitions = 10)
    {
        Repetitions = repetitions;
    }
}
```

Przyczepione do elementów kodu atrybutu możemy później pobierać za pomocą refleksji. Metoda `GetCustomAttributes<TAttribute>` zdefiniowana jest na każdym typie reprezentującym element kodu i zwraca nam sekwencję atrybutów. Na podstawie atrybutu i jego danych można podejmować następnie kolejne kroki.

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
