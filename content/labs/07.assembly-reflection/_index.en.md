---
title: "Assembly, reflection"
weight: 70
---

## Tutorial 7: Assemblies and Reflection

The scope of this laboratory covers reflection – inspecting types and their members at run time (`System.Type`, the `System.Reflection` namespace), defining and reading custom attributes, creating objects and invoking methods dynamically – as well as assemblies: their metadata and loading them at run time with `AssemblyLoadContext`.

Reflection has no real counterpart in standard C++. RTTI (`typeid`, `dynamic_cast`) can only give you an implementation-defined name of a type and compare types – you cannot list the members of a class or call a method knowing only its name. In .NET, the compiler stores a complete description of all types (metadata) in every assembly, and the runtime exposes it through objects such as `Type`, `MethodInfo` or `PropertyInfo`. Unit test frameworks, serializers (e.g. `System.Text.Json`), dependency injection containers and plugin systems are all built on this mechanism.

Before starting the task, we will briefly review the key concepts. More details can be found in the lectures: [Attributes]({{< ref "/lectures/08.reflection/attributes" >}}), [Reflection]({{< ref "/lectures/08.reflection/reflection" >}}) and [Assembly]({{< ref "/lectures/08.reflection/assembly" >}}).

### Types at Run Time: `System.Type`

The entry point to reflection is a `System.Type` object describing a type. There are three ways to get it:

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

The name of a generic type ends with a backtick and the number of its type parameters (``List`1``). `typeof(List<>)` denotes an *open* generic type (the definition, without type arguments). Properties such as `IsClass`, `IsValueType`, `IsEnum`, `IsInterface`, `IsAbstract` or `IsGenericType` describe the kind of a type, and methods like `IsAssignableFrom`, `IsSubclassOf` and `GetInterfaces` describe the relations between types. For example, this is how you check whether a type implements the generic interface `IParsable<T>` for any `T`:

```csharp
bool parsable = typeof(int).GetInterfaces()
    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));
Console.WriteLine(parsable);                                             // True
Console.WriteLine(typeof(IComparable).IsAssignableFrom(typeof(string))); // True
```

### Members and `BindingFlags`

A `Type` gives access to the members of the type: `GetFields`, `GetProperties`, `GetMethods`, `GetConstructors`, `GetMembers` and their singular versions (e.g. `GetMethod("Name")`). Called without arguments, they return only **public** members – instance and static ones, including inherited instance members. To get other members, pass a combination of `BindingFlags`:

```csharp
const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
    | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
FieldInfo[] fields = typeof(Person).GetFields(flags); // also private fields, but not inherited ones
```

> [!WARNING]
> Once you pass any `BindingFlags`, you must specify both the visibility (`Public` and/or `NonPublic`) and the kind of members (`Instance` and/or `Static`). For example, `GetFields(BindingFlags.NonPublic)` always returns an empty array.

Every member is described by an object derived from `MemberInfo` (`FieldInfo`, `PropertyInfo`, `MethodInfo`, `ConstructorInfo`, `EventInfo`), and these objects can be used to access the member dynamically (`Person` is the sample class from Stage 1 of the task below):

| Member | Dynamic access |
|---|---|
| `FieldInfo`, `PropertyInfo` | `GetValue(obj)`, `SetValue(obj, value)` |
| `MethodInfo` | `Invoke(obj, arguments)` – `obj` is `null` for static methods |
| `ConstructorInfo` | `Invoke(arguments)`, or simply `Activator.CreateInstance(type, arguments)` |

```csharp
var person = new Person("Alice", 30);
PropertyInfo name = typeof(Person).GetProperty("Name")!;
name.SetValue(person, "Bob");

MethodInfo greet = typeof(Person).GetMethod("Greet")!;
object? greeting = greet.Invoke(person, ["Hi", true]); // arguments and the result are boxed in objects
Console.WriteLine(greeting);                           // HI, BOB!
```

Reflection works on `object`: arguments are passed as an `object?[]` array (value types are boxed), types are checked at run time, and the result has to be cast. This makes it flexible, but also slow and error-prone – a mistake that the compiler would catch in ordinary code becomes an exception at run time.

### Assemblies

An assembly (a `.dll` or `.exe` file) is the unit of deployment and versioning in .NET. It contains the IL code, the metadata of all its types and a manifest (the assembly name, version and the list of referenced assemblies). In reflection, it is represented by the `Assembly` class: `typeof(Person).Assembly` or `Assembly.GetExecutingAssembly()` return an assembly, and `assembly.GetTypes()` lists all the types defined in it.

Assemblies can also be loaded while the program is running – similarly to `dlopen`/`LoadLibrary` in C/C++. However, `dlsym` gives you only the address of an exported function, whose signature you have to know in advance. A loaded .NET assembly comes with full type information, so the program can find, for example, all classes marked with an attribute, create their instances and use them as ordinary objects. This is how plugin systems work.

## Task 1 - Plugin-Based Command Shell

In this task, you will build an interactive command shell (a REPL – *read-eval-print loop*). Shell commands are ordinary C# methods marked with attributes. The shell does not know them at compile time: it discovers them with reflection, converts the arguments typed by the user to the types of the parameters, invokes the methods and prints their results. In the last stages, the commands will come from plugins – class libraries loaded at run time from the `plugins` directory, which can even be rebuilt and reloaded without restarting the shell.

The final version of the shell will work as follows:

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

Start by creating a solution with a console application project:

```bash
dotnet new sln -n ReflectionShell
dotnet new console -n Shell
dotnet sln add Shell
```

### Stage 1: Inspecting Types

Before we write the shell itself, we will build two tools that it will use later: `TypeInspector`, which describes the members of any type (the future `inspect` command), and `ObjectDumper`, which prints the values of any object (we will use it to display the results of commands).

#### Readable Type Names

`Type.Name` is not always what we would write in C#: `typeof(int).Name` is `Int32`, and `typeof(List<string>).Name` is ``List`1``. Create a file `TypeExtensions.cs` with an extension property `FriendlyName` returning a C#-like name of a type. Use an extension block introduced in C# 14 (see the lecture [Extension Methods]({{< ref "/lectures/05.csharp-features/extension-methods" >}})):

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

Handle the following cases:
1. Types with a C# keyword (`int`, `string`, `bool`, `double`, `decimal`, `object`, `void`, …) – look them up in a private static `Dictionary<Type, string>`.
2. Arrays (`IsArray`) – the friendly name of the element type (`GetElementType()`) followed by `[]` (`[,]` if `GetArrayRank()` returns 2, etc.).
3. By-reference types (`IsByRef`) – these are the types of `ref`/`out` parameters (e.g. `Int32&`); return the friendly name of the element type.
4. `Nullable<T>` – `Nullable.GetUnderlyingType(type)` returns `T` (or `null` if the type is not nullable); return e.g. `int?`.
5. Generic types (`IsGenericType`) – the name up to the backtick and the friendly names of `GetGenericArguments()`, e.g. `Dictionary<string, int>`.
6. Otherwise – `Name`.

#### TypeInspector

We will test the inspector on a sample class. Create a file `Samples/Person.cs`:

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

Create a static class `TypeInspector` (`TypeInspector.cs`) with a method `public static string Describe(Type type)` that returns a description of all the members **declared** in the type:

1. A header: the kind of the type (`class`, `struct`, `interface`, `enum`, `static class`, `abstract class`), its `FullName`, the `BaseType`, the interfaces (`GetInterfaces()`) and the assembly name (`type.Assembly.GetName().Name`).
2. Sections `Constructors`, `Fields`, `Properties` and `Methods` with members obtained using `BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly`. Reflection does not guarantee the order of members, so sort them by name. Print `(none)` for an empty section.
3. The access modifier of every member: `MethodBase` (the base of `MethodInfo` and `ConstructorInfo`) and `FieldInfo` have the properties `IsPublic`, `IsPrivate`, `IsFamily` (`protected`), `IsAssembly` (`internal`) and `IsFamilyOrAssembly` (`protected internal`).
4. Fields: `static` (`IsStatic`), `readonly` (`IsInitOnly`) or `const` (`IsLiteral`), the `FieldType` and the name.
5. Properties: the `PropertyType`, the name and the accessors `GetMethod`/`SetMethod` (`null` if missing), with the access modifier of an accessor if it differs from the property's one: `public int Age { get; private set; }`.
6. Methods: skip the ones with `IsSpecialName` (see the box below). Print `static`, `abstract`, `override` (when `GetBaseDefinition().DeclaringType` differs from `DeclaringType`) or `virtual` (`IsVirtual && !IsFinal`), the `ReturnType`, the name and the parameters.
7. Parameters (`GetParameters()`): the type and the name, preceded by `params` for a parameter marked with `ParamArrayAttribute` (`parameter.IsDefined(typeof(ParamArrayAttribute))`) or `ref`/`out` for by-reference ones (`IsOut`). If `HasDefaultValue` is `true`, append ` = ` and the `DefaultValue`.
8. Format default values as in C# code (`"Hello"`, `false`, `null`) in a public method `public static string FormatValue(object? value)` – we will reuse it later. Use `Convert.ToString(value, CultureInfo.InvariantCulture)` for numbers, so that the result does not depend on the regional settings.

> [!IMPORTANT]
> #### Code as Seen by Reflection
>
> Reflection shows types as the compiler emitted them, not as you wrote them:
> - A property is just metadata linking a pair of methods, `get_Name` and `set_Name`. These accessor methods are returned by `GetMethods()` as well, marked with the `IsSpecialName` flag (operators, e.g. `op_Addition`, are marked the same way).
> - Every auto-property has a hidden backing field named `<Name>k__BackingField`.
> - Nullable reference type annotations are not a part of the type: the `PropertyType` of `Address? Address` is simply `Address`. The annotations are stored in attributes and can be read with `NullabilityInfoContext`.
> - A static class is `abstract sealed` in IL – `IsAbstract` and `IsSealed` are both `true`.

In `Program.cs`, print the description of the `Person` class:

```csharp
Console.WriteLine(TypeInspector.Describe(typeof(Person)));
```

The expected output:

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

Now we will do the opposite: instead of describing a type, we will print the values stored in an object of any type. Create a static class `ObjectDumper` with a method `public static string Dump(object? value, int maxDepth = 5)`, which recursively prints the object graph:

1. `null` is printed as `null`.
2. Simple values – primitive types (`IsPrimitive`), enums (`IsEnum`), `string`, `decimal` and other types implementing `IFormattable` (e.g. `DateTime`, `DateOnly`, `Guid`) – are printed in a single line using `TypeInspector.FormatValue`.
3. Collections (objects implementing the non-generic `IEnumerable` interface) are printed as the type name and the number of elements, followed by every element on a separate line, in the form `[index]: value`.
4. Other objects are printed as the type name, followed by every public instance property in the form `Name: value`. Skip indexers – properties with parameters (`GetIndexParameters().Length > 0`), which cannot be read without an index. If a getter throws an exception, `GetValue` throws a `TargetInvocationException`; print `<ExceptionTypeName>` instead of the value.
5. Every nesting level is indented by two spaces. When `maxDepth` levels are exceeded, print just the type name followed by `{ ... }`.
6. Object graphs may contain cycles (Alice is a friend of Bob, and Bob is a friend of Alice). Store the objects on the path from the root to the current object in a `HashSet<object>`; if an object is already on the path, print `<cycle: TypeName>` instead of expanding it again. Remove the object from the set (in a `finally` block) when you are done with it.

> [!WARNING]
> The set of visited objects must compare objects **by reference**, like pointers in C++. Create it with `new HashSet<object>(ReferenceEqualityComparer.Instance)`. The default comparer uses `Equals` and `GetHashCode`, which may be overridden: records compare their values, so two different (but equal) `Address` objects would be taken for the same object, and a custom `Equals` may even throw an exception.

Extend `Program.cs`:

```csharp
var alice = new Person("Alice", 30) { Address = new Address("Warsaw", "Koszykowa 75") };
var bob = new Person("Bob", 17);
alice.AddFriend(bob);
bob.AddFriend(alice); // a cycle: alice -> bob -> alice

Console.WriteLine(ObjectDumper.Dump(alice));
Console.WriteLine(ObjectDumper.Dump(new Dictionary<string, DateOnly> { ["Christmas"] = new(2026, 12, 24) }));
```

The expected output:

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

Note that the `Friends` property is declared as `IReadOnlyList<Person>`, but `GetType()` returns the actual run-time type of the object – `List<Person>`.

> [!NOTE]
> #### Stage 1
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.01" >}}

### Stage 2: Attributes and Command Discovery

The commands of our shell will be ordinary methods marked with attributes. Attributes are passive: they only add information to the metadata. It is our code – a command registry – that reads them with reflection and acts on them.

> [!IMPORTANT]
> #### Defining Attributes
>
> An attribute is a class derived from `System.Attribute`. By convention, its name ends with `Attribute`, and the suffix can be omitted when the attribute is used (`[Command]` means `[CommandAttribute]`). The `[AttributeUsage]` attribute specifies the code elements the attribute can be applied to (`AttributeTargets`), whether it can be applied multiple times to the same element (`AllowMultiple`, `false` by default) and whether it is inherited by derived classes and overriding methods (`Inherited`, `true` by default). The compiler enforces these rules – e.g. a method-only attribute applied to a property causes error CS0592, and a duplicated attribute with `AllowMultiple = false` causes error CS0579.
>
> The constructor parameters of an attribute are its positional arguments, and its public writable properties can be set as named arguments (`[Obsolete("Old", DiagnosticId = "X1")]`). All arguments must be compile-time constants, because they are stored in the metadata. An attribute object is created only when it is read with reflection – each call to `GetCustomAttribute` creates a new instance.
>
> ```csharp
> // Marks a method as a shell command. Without a name, the lower-case method name is used.
> [AttributeUsage(AttributeTargets.Method)]
> public sealed class CommandAttribute(string? name = null) : Attribute
> {
>     public string? Name { get; } = name;
> }
> ```

#### Attributes

Create a directory `Abstractions` with four attributes in the namespace `Shell.Abstractions` (in Stage 4, this directory will become a separate library shared by the shell and its plugins):

1. `CommandModuleAttribute` – applicable to classes, not inherited (`Inherited = false`), without parameters. It marks a class containing commands (a *module*), similarly to `[TestClass]` in unit test frameworks.
2. `CommandAttribute` – applicable to methods, with an optional `Name` (shown above).
3. `AliasAttribute` – applicable to methods, many times (`AllowMultiple = true`), with an alternative name of the command (`string Alias`).
4. `DescriptionAttribute` – applicable to classes, methods and parameters (`AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter`), with a description (`string Text`) displayed by the `help` command.

> [!TIP]
> .NET already contains a `DescriptionAttribute` (in the `System.ComponentModel` namespace). This is not a problem as long as you do not import both namespaces in the same file – otherwise the compiler reports an ambiguous reference (CS0104).

#### Command Modules

Create a directory `Commands` with two modules. Copy the `MathCommands` class – it contains both instance commands (the module has state: the memory used by `m+` and `mr`) and static ones:

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

The second module, `TextCommands`, is a **static** class marked with `[CommandModule]` and `[Description("Text manipulation commands.")]`. Implement the following commands in it (add descriptions as in the listing below):
- `upper <text>` – converts the text to upper case,
- `reverse <text>` – reverses the text,
- `repeat <text> [times=2] [separator=" "]` – joins `times` copies of the text with the separator (`string.Join` + `Enumerable.Repeat`),
- `wc <text>` (method `WordCount`) – returns a record `public record TextStats(int Characters, int Words, int Vowels)` with the number of characters, words (separated by spaces) and vowels (`aeiouy`, case-insensitive) in the text.

#### Command Registry

Create a class `CommandInfo` (`CommandInfo.cs`) describing a single discovered command. Use a primary constructor `CommandInfo(string name, MethodInfo method, object? target)`, where `target` is the object on which the method will be called (`null` for static methods), and define the read-only properties:
- `Name`, `Method`, `Target`,
- `ParameterInfo[] Parameters` – the parameters of the method,
- `IReadOnlyList<string> Aliases` – the values of all `[Alias]` attributes of the method,
- `string Description` – the text of the `[Description]` attribute of the method, or an empty string,
- `string Usage` – the name followed by the parameters: required ones as `<name>`, optional ones as `[name=default]` (`[name]` if the default value is `null`) and `params` arrays as `[name...]`, e.g. `round <value> [digits=0] [mode=ToEven]`,
- a method `string GetDetails()` returning a detailed description – see the `help power` output below. For enum parameters, list the allowed values (`Enum.GetNames`).

> [!IMPORTANT]
> #### Reading Attributes
>
> Extension methods from `System.Reflection.CustomAttributeExtensions` read attributes of any code element (`Type`, `MethodInfo`, `ParameterInfo`, `Assembly`, …):
> - `GetCustomAttribute<T>()` returns the attribute or `null` if the element does not have it (and throws `AmbiguousMatchException` if there are more of them),
> - `GetCustomAttributes<T>()` returns all of them – use it for attributes with `AllowMultiple = true`,
> - `IsDefined(typeof(T))` only checks whether the attribute is present, without creating its instance.
>
> ```csharp
> string? description = method.GetCustomAttribute<DescriptionAttribute>()?.Text;
> List<string> aliases = [.. method.GetCustomAttributes<AliasAttribute>().Select(a => a.Alias)];
> ```

Then create a class `CommandRegistry` (`CommandRegistry.cs`). It stores the commands in a list, and in a dictionary indexed by names and aliases (case-insensitive: `StringComparer.OrdinalIgnoreCase`). Implement:

1. `IEnumerable<CommandInfo> Commands` – the commands sorted by name.
2. `bool TryFind(string nameOrAlias, [NotNullWhen(true)] out CommandInfo? command)`.
3. `void RegisterAssembly(Assembly assembly)` – registers every class marked with `[CommandModule]` found among `assembly.GetTypes()` using `RegisterModule(Type)`.
4. `void RegisterModule(Type type)` – creates an instance of the module and registers its commands. A static class does not need an instance. If the class is abstract or does not have a public parameterless constructor, print a warning and skip it.
5. `void RegisterModule(object instance)` – registers the commands of an existing object (we will need it in the next stage).
6. Registering commands: for each public method (instance and static) marked with `[Command]`, create a `CommandInfo` named `attribute.Name ?? method.Name.ToLowerInvariant()`. Skip (with a warning) generic methods (`IsGenericMethodDefinition`) and methods with `ref`/`out` parameters (`ParameterType.IsByRef`), as well as commands whose name or alias is already taken.
7. `string GetHelp(string? name = null)` – for a given name, returns `GetDetails()` of the command (or `Unknown command '…'.`); without a name, returns a list of all commands grouped by module (`Method.DeclaringType`). The header of a group is the name of the module class and its `[Description]`.

Print the warnings in yellow (`Console.ForegroundColor = ConsoleColor.Yellow`, then `Console.ResetColor()`).

> [!IMPORTANT]
> #### Creating Objects with `Activator`
>
> `Activator.CreateInstance(type)` creates an object using the public parameterless constructor of the type. If there is no such constructor, it throws a `MissingMethodException`, so it is better to check it beforehand: `type.GetConstructor(Type.EmptyTypes)` returns `null` if the constructor does not exist. Other overloads, such as `Activator.CreateInstance(type, arg1, arg2)`, choose a constructor matching the arguments. A static class cannot be instantiated at all – recognize it by `type.IsAbstract && type.IsSealed`.

In `Program.cs`, register the commands of the current assembly and print the help:

```csharp
var registry = new CommandRegistry();
registry.RegisterAssembly(Assembly.GetExecutingAssembly());

Console.WriteLine(registry.GetHelp());
Console.WriteLine(registry.GetHelp("power"));
```

The expected output:

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
> Check the validation: temporarily add a module whose constructor has a parameter, a command with `ref` parameters and a second command named `add`. You should see warnings such as `Warning: MathCommands.Add: name 'add' is already taken - skipped.`

> [!NOTE]
> #### Stage 2
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.02" >}}

### Stage 3: Dynamic Invocation

Now we will build the shell itself. Each line typed by the user is split into words; the first one is the name of a command, the rest are its arguments. The shell finds the command in the registry, converts the arguments from strings to the types of the parameters, invokes the method and prints the result.

#### The Shell Class

Create a class `CommandShell` with a property `CommandRegistry Registry` and the following members:

1. Built-in commands: `help [command]` (returns `Registry.GetHelp(command)`) and `exit` with the alias `quit` (stops the loop). Implement them as methods of `CommandShell` marked with `[Command]`, and register them in the constructor with `Registry.RegisterModule(this)`. Do not mark `CommandShell` with `[CommandModule]` – `RegisterAssembly` would try to create a second instance of the shell.
2. `void Run()` – prints a welcome message and in a loop prints the `> ` prompt, reads a line (`Console.ReadLine()` returns `null` at the end of the input) and executes it. If the input is redirected from a file (`Console.IsInputRedirected`), print the line read – this way `dotnet run --project Shell < commands.txt` produces a readable transcript of a session.
3. `void Execute(string line)` – executes a single command, as described below.

Splitting a line into words, where a text in double quotes forms a single word, is easiest with a regular expression. You can use the following code (the regular expression is compiled at build time by a source generator, so the class must be `partial`):

```csharp
// A word or a text in double quotes
[GeneratedRegex(@"""([^""]*)""|(\S+)")]
private static partial Regex TokenRegex();

private static string[] Tokenize(string line) =>
    [.. TokenRegex().Matches(line).Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)];
```

#### Converting Arguments

Create a static class `ArgumentConverter` with a method `object?[] Bind(ParameterInfo[] parameters, string[] tokens)`, which creates the array of arguments for `Invoke`:
1. The i-th token is converted to the type of the i-th parameter.
2. If there are fewer tokens than parameters, a missing parameter gets its `DefaultValue` if `HasDefaultValue` is `true`; otherwise, throw `ArgumentException("Missing value for parameter '…'.")`.
3. A `params` parameter (always the last one) takes all the remaining tokens. Create an array of the right type with `Array.CreateInstance(elementType, length)` and fill it with `SetValue`.
4. Superfluous tokens cause an `ArgumentException("Too many arguments: …")`.

> [!WARNING]
> For a parameter without a default value, `ParameterInfo.DefaultValue` returns `DBNull.Value`, not `null`. Always check `HasDefaultValue` first.

Converting a single value is done by `object? ConvertValue(string text, Type type)`:
1. `string` – the text itself.
2. `Nullable<T>` – convert to `T`.
3. Enums – `Enum.TryParse(type, text, ignoreCase: true, out object? value)`. Note that `TryParse` also accepts numbers outside the defined values, so additionally check `Enum.IsDefined(type, value)`. Otherwise, throw an `ArgumentException` listing the allowed values (`Enum.GetNames(type)`).
4. Types implementing `IConvertible` (primitive types, `decimal`, `DateTime`) – `Convert.ChangeType(text, type, CultureInfo.InvariantCulture)`. Thanks to the invariant culture, `3.5` is parsed the same way regardless of the regional settings of the system.
5. Other types (e.g. `DateOnly`, `TimeSpan`, `Guid`) usually have a static method `bool TryParse(string s, IFormatProvider provider, out T result)`. Find it and call it with reflection (see below). If there is no such method, the type is not supported.

Convert the `FormatException`, `OverflowException` and `InvalidCastException` exceptions thrown during conversion into an `ArgumentException` with the message `Cannot convert '…' to ….`, so that the shell can handle all invalid arguments in the same way.

> [!IMPORTANT]
> #### `out` Parameters and Reflection
>
> To find an overload with a `ref`/`out` parameter, describe the type of the parameter with `MakeByRefType()` (`int` → `Int32&`). When such a method is called with `Invoke`, the values of `out` parameters are written back to the array of arguments:
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
> Not every type has such a public method – e.g. `bool` and `char` implement `IParsable<T>.TryParse` explicitly. That is why we try `Convert.ChangeType` first.

#### Executing Commands

Implement `Execute(string line)`:
1. Split the line into words. Ignore an empty line.
2. Find the command with `Registry.TryFind`. If it does not exist, print `Error: Unknown command '…'. Type 'help' to list commands.`
3. Create the arguments with `ArgumentConverter.Bind(command.Parameters, tokens[1..])`. If it throws an `ArgumentException`, print `Error: ` followed by its message.
4. Call `command.Method.Invoke(command.Target, arguments)`.
5. If the method does not return `void` (`Method.ReturnType`), print the result: a string as it is, any other object using `ObjectDumper.Dump`.

Print the errors in red.

> [!IMPORTANT]
> #### `TargetInvocationException`
>
> If the invoked method throws an exception, `MethodInfo.Invoke` does not pass it on directly – it wraps it in a `TargetInvocationException`. The original exception is available in the `InnerException` property:
>
> ```csharp
> catch (TargetInvocationException ex) when (ex.InnerException is { } inner)
> {
>     WriteError($"{inner.GetType().Name}: {inner.Message}");
> }
> ```
>
> Alternatively, you can call `Invoke(target, BindingFlags.DoNotWrapExceptions, null, arguments, null)` – the exception is then thrown as it is.

#### The Inspect Command

Add a third module, the static class `Commands/DiagnosticsCommands` (`[Description("Diagnostic commands.")]`), with a command `inspect <typeName>` returning `TypeInspector.Describe` of the given type. First, try `Type.GetType(typeName)` – it finds types by the full name in the current assembly and in the core library (e.g. `System.Text.StringBuilder`). Otherwise, search all the loaded assemblies (`AppDomain.CurrentDomain.GetAssemblies()`) for a type with a matching `FullName` or `Name`.

Finally, change `Program.cs`:

```csharp
var shell = new CommandShell();
shell.Registry.RegisterAssembly(Assembly.GetExecutingAssembly());
shell.Run();
```

An example session (the `help` command now lists two more modules: `CommandShell` with the built-in commands and `DiagnosticsCommands`):

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
> Try `inspect TextStats` to see the members the compiler generates for a record: a copy constructor and `<Clone>$` (used by `with`), `EqualityContract`, `PrintMembers`, `Deconstruct`, `Equals`… The `init` accessors are displayed as `set` – for the runtime, `init` is an ordinary setter marked with a required modifier (`IsExternalInit`).

> [!NOTE]
> #### Stage 3
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.03" >}}

### Stage 4: Assemblies and Plugins

So far, all the commands have been compiled into the shell. Now we will split the solution into four projects:
- `Shell.Abstractions` – a class library with the attributes; the shared contract between the host and the plugins,
- `Shell` – the host application, which references only `Shell.Abstractions`,
- `MathPlugin` and `TextPlugin` – plugins with the command modules. They reference only `Shell.Abstractions` and are loaded by the host at run time from the `plugins` directory.

The host does not know the plugins at compile time, and the plugins do not know the host. A new plugin can be added by copying it to the `plugins` directory, without recompiling the shell.

#### New Projects

Create the libraries, add them to the solution and add the reference to the contract:

```bash
dotnet new classlib -n Shell.Abstractions
dotnet new classlib -n MathPlugin
dotnet new classlib -n TextPlugin
dotnet sln add Shell.Abstractions
dotnet sln add MathPlugin
dotnet sln add TextPlugin
dotnet add Shell reference Shell.Abstractions
```

Delete the generated `Class1.cs` files and move the code:
- `Shell/Abstractions/*.cs` → `Shell.Abstractions/` (the namespace stays `Shell.Abstractions`),
- `Shell/Commands/MathCommands.cs` → `MathPlugin/` (namespace `MathPlugin`),
- `Shell/Commands/TextCommands.cs` → `TextPlugin/` (namespace `TextPlugin`).

`DiagnosticsCommands` stays in the host.

> [!TIP]
> Why not `Plugins.Math`? Inside a namespace whose last part is `Math`, the name `Math` refers to that namespace, and `Math.Pow(x, n)` causes error CS0234 – the compiler looks for `Pow` in the namespace `Plugins.Math`, not in the class `System.Math`.

#### Building Plugins

Replace the content of `MathPlugin/MathPlugin.csproj` (and analogously `TextPlugin.csproj`, with version `1.0.0` and its own description):

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

- `EnableDynamicLoading` marks the library as a dynamically loaded component (among others, the NuGet packages it uses are copied to its output directory, next to the plugin).
- `<Private>false</Private>` and `<ExcludeAssets>runtime</ExcludeAssets>` mean that the plugin is compiled against `Shell.Abstractions`, but `Shell.Abstractions.dll` is **not** copied to the output of the plugin. At run time, the plugin must use the host's copy of the contract – we will explain why in a moment.
- The `CopyToShellPlugins` target (see [Tutorial 2]({{< ref "/labs/02.msbuild" >}}) for MSBuild targets) copies the built plugin to `Shell/bin/<Configuration>/net10.0/plugins/<PluginName>/`. Every plugin gets a separate directory, so that the dependencies of different plugins do not mix.

Add references to the plugins to `Shell/Shell.csproj`, with `ReferenceOutputAssembly="false"`:

```xml
<!-- The host does not reference the plugins: these references only make MSBuild build (and copy) them first -->
<ItemGroup>
  <ProjectReference Include="..\MathPlugin\MathPlugin.csproj" ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\TextPlugin\TextPlugin.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
```

The shell still cannot use any type from the plugins in its code, but thanks to these references, `dotnet run --project Shell` always builds the plugins first.

> [!IMPORTANT]
> #### Assembly Metadata
>
> The `Version` and `Description` properties from the project file end up in the manifest of the assembly. During the build, the SDK generates the file `obj/Debug/net10.0/MathPlugin.AssemblyInfo.cs` with assembly-level attributes, such as `[assembly: AssemblyVersion("1.2.0.0")]`, `[assembly: AssemblyInformationalVersion("1.2.0")]` and `[assembly: AssemblyDescription("…")]`. You can read them like any other attributes:
>
> ```csharp
> string? version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
> ```
>
> `assembly.GetName()` returns the identity of the assembly (`Name`, `Version`), and `GetReferencedAssemblies()` lists the assemblies it depends on, as recorded in the manifest.

#### Loading Plugins

Each plugin will be loaded into its own `AssemblyLoadContext` (see the [Assembly]({{< ref "/lectures/08.reflection/assembly" >}}) lecture). Create the class `PluginLoadContext`:

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

The runtime calls `Load` whenever code in the plugin needs another assembly. Returning `null` means that the assembly will be loaded by the default context – the one where the host and its dependencies live.

Then:
1. Create a class `PluginManager` with a primary constructor taking the path to the plugins directory, a property `IReadOnlyList<Assembly> Assemblies` and a method `void LoadAll()`. For every subdirectory of the plugins directory (sorted by name) containing a file `<DirectoryName>.dll`, create a new `PluginLoadContext` and load the assembly with `context.LoadFromAssemblyPath(path)`. If the plugins directory does not exist, do nothing.
2. In `CommandShell`, add a constructor parameter `string pluginsDirectory` and a private method `LoadCommands()`, called from the constructor, which registers the built-in commands (`RegisterModule(this)`), the commands of the host assembly (`RegisterAssembly(typeof(CommandShell).Assembly)`), then loads the plugins and registers the commands of each plugin assembly, printing `Loaded plugin <Name> <Version>`.
3. Add a built-in command `plugins`, which for each plugin prints: the full name of the assembly (`FullName`), the description, the informational version, the location relative to `AppContext.BaseDirectory` (`Path.GetRelativePath`), the name of its load context (`AssemblyLoadContext.GetLoadContext(assembly)`), the referenced assemblies and the names of the commands it provides (commands whose `Method.DeclaringType.Assembly` is the plugin assembly).
4. Simplify `Program.cs` – the plugins are looked for next to the executable, not in the current directory:

```csharp
var shell = new CommandShell(Path.Combine(AppContext.BaseDirectory, "plugins"));
shell.Run();
```

Build the whole solution and run the shell:

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

If your project is in a git repository, the SDK appends the hash of the current commit to the informational version, e.g. `1.2.0+5e0630ba…`. `help` now shows `MathPlugin.MathCommands (MathPlugin)` in the `Defined in` line, and `inspect` works for the types from plugins as well – try `inspect MathCommands`.

> [!IMPORTANT]
> #### Type Identity
>
> For the runtime, a type is identified not only by its full name, but also by the **assembly instance** it was loaded from. If `Shell.Abstractions.dll` is loaded twice – into the default context (by the host) and into the plugin's context – there are two different `CommandAttribute` types with identical names. `GetCustomAttribute<CommandAttribute>()` and `IsDefined` called by the host (with its own `CommandAttribute`) find nothing in the plugin, which silently provides no commands. Casting such an attribute (or an object implementing an interface from the contract) fails with a confusing message:
>
> ```text
> [A]Shell.Abstractions.CommandAttribute cannot be cast to [B]Shell.Abstractions.CommandAttribute.
> Type A originates from 'Shell.Abstractions, Version=1.0.0.0, ...' in the context '"MathPlugin" Shell.PluginLoadContext #0' ...
> Type B originates from 'Shell.Abstractions, Version=1.0.0.0, ...' in the context 'Default' ...
> ```
>
> We have two safeguards against this: the plugin does not ship its own copy of the contract (`Private=false`), and `PluginLoadContext.Load` delegates the contract to the default context. Check it: remove both safeguards (the `<Private>` and `<ExcludeAssets>` elements, and the `if` in `Load`), rebuild, and see what the `plugins` command shows for `MathPlugin`.

> [!NOTE]
> #### Stage 4
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.04" >}}

### Stage 5: Performance and Unloading

In the last stage, we will measure the cost of reflection and learn how to unload plugins, so that they can be rebuilt and reloaded without restarting the shell.

#### The Cost of Reflection

A reflective call is much slower than an ordinary one. Finding a member by name means searching the metadata, and `MethodInfo.Invoke` has to check the number and types of the arguments, box value types into an `object?[]` array and unbox the result. There are two simple ways to reduce the cost:
- **Cache the `MemberInfo` objects** – find a method once and keep its `MethodInfo` (our registry already does that).
- **Turn a method into a delegate** – if the signature is known at compile time, `method.CreateDelegate<Func<int, int>>()` creates a delegate, which is as fast as an ordinary call. If the signature is unknown, `MethodInvoker.Create(method)` (.NET 8+) prepares an invoker which is faster than `Invoke`, although it still works on `object`s.

Add a command `bench [iterations=1000000]` to `DiagnosticsCommands`. It measures (`Stopwatch`) calling a private static method `int Twice(int x) => 2 * x;` (mark it with `[MethodImpl(MethodImplOptions.NoInlining)]`, so that the JIT compiler cannot inline the direct calls) in five ways: a direct call, a delegate from `CreateDelegate`, a `MethodInvoker`, `MethodInfo.Invoke` with a cached `MethodInfo`, and `GetMethod` + `Invoke` in every iteration. The command returns a table with the total time and the time per call. Your results will differ, but the proportions should be similar:

```text
> bench
Direct call               5.4 ms      5.4 ns/call
Delegate                  6.8 ms      6.8 ns/call
MethodInvoker            31.8 ms     31.8 ns/call
MethodInfo.Invoke        53.0 ms     53.0 ns/call
GetMethod + Invoke      116.6 ms    116.6 ns/call
```

Now use a `MethodInvoker` in the shell. Add a property to `CommandInfo` which creates the invoker on first use and then caches it – the C# 14 `field` keyword (see the lecture [Classes]({{< ref "/lectures/04.creating-types/classes" >}})) refers to the compiler-generated backing field:

```csharp
// Created on first use and then reused - faster than MethodInfo.Invoke
public MethodInvoker Invoker => field ??= MethodInvoker.Create(Method);
```

In `CommandShell.Execute`, replace `command.Method.Invoke(...)` with `command.Invoker.Invoke(command.Target, arguments)`. Unlike `MethodInfo.Invoke`, `MethodInvoker` does **not** wrap exceptions in a `TargetInvocationException`, so catch `Exception` directly.

#### Unloading Plugins

> [!IMPORTANT]
> #### Collectible Load Contexts
>
> A context created with `isCollectible: true` can be unloaded together with all the assemblies loaded into it. However, `Unload()` only starts the process – unloading is cooperative. The garbage collector frees the context when there are no more references to it, nor to anything coming from its assemblies: objects of their types, `Type` and `MethodInfo` objects, delegates, values stored in static fields of the host, threads executing plugin code… A `WeakReference` to the context lets you check whether it has really been freed: call `GC.Collect()` and `GC.WaitForPendingFinalizers()` until `weakReference.IsAlive` becomes `false`.
>
> Avoid keeping references in local variables of the method that waits for the GC: in Debug builds, the JIT extends the lifetime of local variables to the end of the method. Do the unloading in a separate method marked with `[MethodImpl(MethodImplOptions.NoInlining)]`.

Implement the `reload` command:
1. Make `PluginLoadContext` collectible: pass `isCollectible: true` to the base constructor.
2. In `PluginManager.LoadAll`, replace `LoadFromAssemblyPath` with `context.LoadFromStream(stream)` (`using FileStream stream = File.OpenRead(path);`). The runtime then works on a copy in memory, and the file is neither locked (on Windows) nor mapped into memory, so the build can overwrite it while the shell is running. A side effect: `Assembly.Location` is an empty string – show `(loaded from memory)` in the `plugins` command instead.
3. Add `List<WeakReference> UnloadAll()` to `PluginManager` (marked with `NoInlining`): for each plugin assembly, get its context (`AssemblyLoadContext.GetLoadContext`), call `Unload()` and remember a `WeakReference` to the context; then clear the list of assemblies.
4. Add `Clear()` to `CommandRegistry` – the registry must be cleared before unloading, because each `CommandInfo` references a plugin's `MethodInfo` and module instance.
5. Add a built-in command `reload`: clear the registry and unload the plugins (in a separate method marked with `NoInlining`), call `GC.Collect()` and `GC.WaitForPendingFinalizers()` in a loop (at most 10 times) until all the weak references are dead, print `Unloaded N plugins.` (or how many are still in memory) and call `LoadCommands()` again.

After the reload, the state of the modules is lost – they are brand new objects from new assemblies:

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

Now try reloading modified code. Leave the shell running, add a command `sqrt <x>` to `MathCommands`, and build only the plugin in another terminal: `dotnet build MathPlugin` (do not build the whole solution – on Windows, `Shell.dll` is locked by the running process). Then type `reload` in the shell – `sqrt 2` should return `1.4142135623730951`.

> [!WARNING]
> Any reference to plugin code kept by the host prevents unloading. For example, if the shell stored the result of the last command in a static field and the last command was `wc` (which returns a `TextStats` object defined in `TextPlugin`), `reload` would report `1 of 2 plugins are still in memory.` Typical culprits are static caches keyed by `Type` (e.g. a cache of `PropertyInfo[]` in `ObjectDumper`), event handlers and running threads or timers.

> [!NOTE]
> #### Stage 5
> {{< filetree dir="labs/w07.assembly-reflection/ReflectionShell.05" >}}

### Next Steps and Further Enhancements

The shell built in this tutorial can be developed further in many directions. Below are some ideas worth exploring:

- **Named arguments:** Support options such as `repeat ab --times 3`, matched with parameters by `ParameterInfo.Name` – this is what command-line parsing libraries do.
- **Dependency injection:** Instead of requiring a parameterless constructor, find a constructor whose parameters the shell can provide (e.g. the registry or a `TextWriter` for the output) and call it with `ConstructorInfo.Invoke` – this is how DI containers create objects.
- **Asynchronous commands:** Allow commands returning `Task` or `Task<T>`: await the returned task and read its `Result` property with reflection.
- **Automatic reload:** Watch the `plugins` directory with `FileSystemWatcher` and reload the plugins whenever a file changes.
- **Plugins with dependencies:** Add a plugin using a NuGet package (e.g. *Figgle* from the lecture). `AssemblyDependencyResolver` loads the package from the plugin's directory into the plugin's context, so different plugins can even use different versions of the same package.
- **Compile-time discovery:** Reflection does not work well with trimming and Native AOT. *Source generators* can find the `[Command]` methods during compilation and generate the registration code, so no reflection is needed at run time.

## Example Tasks

Complete an example task from previous years. If you are able to finish it within 90 minutes, you are well-prepared for class.

- [Task 1]({{< ref "/labs/07.assembly-reflection/example1" >}})
- [Task 2]({{< ref "/labs/07.assembly-reflection/example2" >}})
