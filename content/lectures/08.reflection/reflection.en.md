---
title: "Reflection"
weight: 20
---

# Reflection

Reflection is a mechanism that allows a program to analyze its own structure at runtime. Thanks to reflection, you can obtain information about types and their members. For example, you can get a list of a given class's constructors and create an instance of it without knowing the class name at compile time. The attributes mentioned earlier are useless until something reads them; reflection allows you to read them and take action based on them. You can also use reflection to inspect the compiled code of methods.

> Reflection is a form of metaprogramming. Unlike C++ templates, reflection works at runtime. C# also provides a form of compile-time metaprogramming. Source Generators allow you to analyze existing code at compile time and add new source files to it.

It's important to remember that reflection-based operations are significantly slower than direct code calls. Code that uses reflection is often difficult to understand and even more difficult to debug.

Nevertheless, reflection has its uses. Some problems are much easier to solve with reflection:

* **Unit Tests** - we can find all methods marked with a `[Test]` attribute and run them.
* **Serialization** - we can analyze an object's members and, based on them, convert the object to a suitable format, e.g., JSON.
* **Plugin Systems** - we can dynamically load external assemblies to find classes that implement a specific interface, e.g., `IPlugin`.

## Namespaces

The reflection mechanism is available through classes in the following namespaces:

- `System.Reflection`
- `System.Reflection.Emit`

The `System.Reflection` namespace contains classes that represent various code elements and allow you to inspect and interact with them:

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

> The existence of modules can usually be ignored. In 99.99% of cases, an assembly consists of exactly one module (a file with intermediate code, i.e., a `.dll` or `.exe`). Visual Studio does not support creating multi-module assemblies, and it is also rarely useful. One example is when part of a single library's code is written in another language that also compiles to intermediate code. In that case, multiple separately compiled modules are collected into a single assembly. Typically, modules and assemblies are treated as equivalent (both being files with intermediate code).

The `System.Reflection.Emit` namespace is used for dynamically generating new code at runtime. You can create your own classes, methods, and their code and use them as if they were part of the original compiled program. This process involves emitting Common Intermediate Language (CIL) instructions. We will skip this part of reflection. Dynamic code generation is used, for example, to create mocks for unit tests or to create compiled regular expressions.

## Getting Type Information

The entry point for reflection is usually obtaining an object that represents a type (`System.Type`). We can do this in several ways, two of which we have already seen.

For a `Person` class:

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

We can get a `Type` object in three ways:

1. Using the `typeof` operator:

   ```csharp
   Type personType = typeof(Person);
   ```

   The `typeof` operator works at compile time.

2. Using the `GetType` method on an object instance:

   ```csharp
   Person person = new Person("Alice", "Smith");
   Type personType = person.GetType();
   ```

3. Using the static `Type.GetType(string name)` method:

   ```csharp
   Type? personType = Type.GetType("Reflection.Person");
   ```

   We can do this without knowing the desired type at compile time, for instance, by loading the class name from a configuration file.

### Getting Information About Members:

Regardless of how we obtain the `Type` object, we can then inspect the type's members. We can get all members using `GetMembers`, or specific kinds of members: `GetFields`, `GetProperties`, `GetEvents`, `GetMethods`, `GetConstructors`, `GetNestedTypes`.

```csharp
MemberInfo[] members = typeof(Person).GetMembers();
foreach (MemberInfo member in members)
{
    Console.WriteLine($"{member.MemberType,16}: {member}");
}
```

> Reflection allows you to break encapsulation; you can use it to access private members. For reflection to return private members, you must pass the `BindingFlags.NonPublic | BindingFlags.Instance` flags to the overloaded method:
> ```csharp
> typeof(Person).GetMembers(BindingFlags.NonPublic);
> ```

Alternatively, you can use the `GetTypeInfo()` API, which returns an `IEnumerable` sequence:

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
> `DeclaredMembers` does not list inherited members.

If we know the name of a member, we can find it using one of the following methods: `GetMember`, `GetField`, `GetProperty`, `GetEvent`, `GetMethod`, `GetNestedType`. Based on a provided list of arguments, we can also find the appropriate constructor using `GetConstructor`.

### Invoking Members

Methods, constructors, and property getters and setters can be invoked after being obtained via reflection.

1. Invoking methods:
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
   > The first parameter of the `Invoke` method specifies the instance on which the method should be called. For static methods, you can pass `null` here.
2. Invoking constructors:
   ```csharp
   ConstructorInfo? constructor = typeof(Person)
       .GetConstructor([typeof(string), typeof(string), typeof(DateTime?)]);
   Person? person = constructor?.Invoke(["John", "Doe", DateTime.Now.AddYears(-42)]) as Person;
   Console.WriteLine($"Name: {person?.FullName}, Birthday: {person?.Birthday:d}");
   ```
   Alternatively, you can use the much simpler `CreateInstance` method from the static helper class `Activator`:
   ```csharp
   Person? person = Activator.CreateInstance(typeof(Person), "John", "Doe", DateTime.Now.AddYears(-42)) as Person;
   ```
3. Invoking properties:
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
4. Setting fields:
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

### Late Binding

Methods obtained via reflection will run noticeably slower. The runtime must check if the arguments match, and if the arguments are value types, they are boxed and unboxed, as `MethodInfo.Invoke` operates on an array of objects. This is not to mention that finding the method itself is costly.

A found `MethodInfo` can be assigned to a delegate, which helps avoid some of the performance problems. Calls through such a delegate will be significantly faster.

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

### `UnsafeAccessor` (.NET 8)

Reflection is often used only to access a private member of a type that we cannot modify (e.g., in tests or serializers). Since .NET 8, this can be done without reflection, using the `[UnsafeAccessor]` attribute. We declare a `static extern` method, and the runtime provides its body, which accesses the given member directly, so such a call is as fast as a regular one:

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

The first parameter specifies the type whose member is accessed (for instance members, it is the object used as `this`). Other kinds of members (`UnsafeAccessorKind`) are `StaticField`, `Method`, `StaticMethod`, and `Constructor`. Members of generic types and generic methods are supported since .NET 9. Unlike reflection, the accessed member must be known at compile time, so the trimming and Native AOT analysis ([see below]({{< ref "#reflection-trimming-and-native-aot" >}})) does not report warnings for such code.

## Reflection on an Assembly

You can call `GetTypes` or `GetType` on an assembly to get information about the types it contains. An assembly can be obtained using one of four static methods:

* **Assembly.GetEntryAssembly** gets the assembly that contains the `Main` entry point method.
* **Assembly.GetCallingAssembly** gets the assembly from which the currently executing method was called.
* **Assembly.GetExecutingAssembly** gets the assembly in which the currently executing method is defined.
* **Assembly.GetAssembly** gets the assembly that contains the specified type.

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
> Source code:
> {{< filetree dir="lectures/reflection/Reflection" >}}

## Generic Types

`typeof` can also be applied to an unbound (open) generic type, e.g., `typeof(List<>)`. Unlike C++ templates, which are instantiated only at compile time, such a generic type definition can be closed with concrete type arguments at runtime using `MakeGenericType` (and similarly, a generic method using `MethodInfo.MakeGenericMethod`). The number after the backtick in the type's name is the number of its type parameters.

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

Since C# 14, `nameof` also accepts unbound generic types, e.g., `nameof(List<>)` returns `"List"`.

## Custom Attributes

We can define our own attributes—these are classes that inherit from `System.Attribute`. By convention, their names should end with `...Attribute`; the compiler allows you to omit this suffix when using the attribute. The `AttributeUsage` attribute is used to specify what we can do with our attribute. It has three properties:

* **`AttributeTargets`** - specifies which code elements we can attach the attribute to.
* **`AllowMultiple`** (optional) - a flag indicating whether the attribute can be applied multiple times. Defaults to `false`.
* **`Inherited`** - a flag indicating whether the attribute will be inherited by derived classes and overridden members. Defaults to `true`.

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

Attributes attached to code elements can later be retrieved using reflection. The `GetCustomAttributes<TAttribute>` method is defined on every type representing a code element and returns a sequence of attributes. Based on the attribute and its data, you can then take further steps.

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
> Source code:
> {{< filetree dir="lectures/reflection/Benchmark" >}}

### Generic Attributes (C# 11)

Since C# 11, attribute classes can be generic. Instead of passing a type as an argument (`[Validator(typeof(EmailValidator))]`), it can be passed as a type argument, so the compiler can check its constraints:

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

The type argument must be fully known at compile time — e.g., `[Validator<T>]` using a type parameter `T` of the enclosing generic type is an error (`CS8968`).

## Reflection, Trimming, and Native AOT

When an application is published with trimming (`<PublishTrimmed>`) or compiled ahead of time to native code (Native AOT, `<PublishAot>`, .NET 7+), the compiler statically analyzes the code and removes types and members that appear to be unused. Members used only through reflection (e.g., found by a name read from a configuration file) may then be missing at runtime. That's why the trim analyzer reports warnings (`IL2026`, `IL2057`, `IL2075`, …) for reflection code that it cannot analyze. File-based apps (`dotnet run app.cs`, .NET 10) have Native AOT enabled by default, so these warnings show up there as well.

Such code can be annotated with attributes from the `System.Diagnostics.CodeAnalysis` namespace:

* `[DynamicallyAccessedMembers]` - tells the trimmer which members of a type passed as a `Type` (parameter, field, return value) will be accessed through reflection, so that they are preserved;
* `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]` - mark a method as incompatible with trimming / Native AOT; the warning is then reported at its call sites.

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

That's why many libraries replace reflection with [source generators]({{< ref "/lectures/13.others/source-generators" >}}), which generate the needed code at compile time (e.g., `System.Text.Json`, `[GeneratedRegex]`).
