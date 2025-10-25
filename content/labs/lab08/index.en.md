---
title: "Lab08"
weight: 10
---

# Laboratory 8: Assembly, Reflection

## Dynamic Object Creation

{{% hint info %}}
**What is reflection?**

Reflection is a mechanism that allows examining and manipulating information about types - their fields, properties, methods, attributes, etc., and also in many cases dynamically invoking methods and creating instances at runtime.

Thanks to reflection, code can operate "on types" that it didn't know at compile time.

**Examples of operations using reflection:**

- Reading the list of all properties for a given type.
- Reading and writing property/field values.
- Getting and invoking constructors (e.g., parameterless or more specialized).
- Reading attributes assigned to a class, property, or method.
- Invoking generic methods.
- Getting interfaces that a given class implements.

**Practical notes:**

- **Performance** — reflection operations are usually slower than direct code. It's worth storing once-retrieved information (`Type`, `PropertyInfo`, `MethodInfo`) in data structures.
- **Security** — reflection can be used to break encapsulation (access to private members).

{{% /hint %}}

### Task Description

Implement a `TypeCrafter` class that, using the `CraftInstance<>` method, can dynamically build an instance of any type `T` at runtime, reading values from the console and assigning them to object properties. Your implementation should intensively use reflection (namespace `System.Reflection`).

> [!TIP] 
> **Starting code** 
> {{< filetree dir="labs/lab08/student/TypeCrafter" >}}
>
> **Output:** [TypeCrafter.txt](/labs/lab08/outputs/TypeCrafter.txt)

```csharp
public static class TypeCrafter
{
    public static T CraftInstance<T>()
    {
        throw new NotImplementedException();
    }
}
```

The `CraftInstance<>` method:

- Creates an instance of type `T`.
- Iterates through all public properties.
- For each property:
  - If the property type is `string` — reads a line from console and sets the value.
  - If the type is parsable (implements `IParsable<>` interface or has a static `TryParse` method) - reads a line from console, tries to parse the text by calling the appropriate `TryParse` method and sets the value. In case of invalid parsing, throws a custom `ParseException`.
  - Otherwise treats the property as a complex object and recursively calls `CraftInstance<>` for the type of that property.
  - Returns the initialized object.

In the `Main` method of the `Program` class, demonstrate calling the `CraftInstance<>` method for example types `Customer` and `Invoice`:

```csharp
public sealed class Customer
{
    public Customer() { }

    public Customer(int id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public override string ToString()
    {
        return $"Customer {Id}: {Name} (Balance: {Balance:C})";
    }
}

public sealed class Invoice
{
    public Invoice() { }

    public Invoice(
        Guid id,
        string description,
        decimal amount,
        Customer customer)
    {
        Id = id;
        Description = description;
        Amount = amount;
        Customer = customer;
    }

    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public Customer Customer { get; set; } = null!;

    public override string ToString()
    {
        return $"Invoice {Id}: '{Description}', Amount: {Amount:C}, Customer: {Customer}";
    }
}
```

{{% hint warning %}}
**Implementation Notes**

- **Reflection:**
  - Use types such as `Type`, `PropertyInfo`, `ConstructorInfo`, and `MethodInfo`.
- **Parsing with `TryParse` method:**
  - Use invocation matching signature `TryParse(string? s, IFormatProvider? provider, out T result)`.
  - Use reflection to search among public static methods.
  - To prepare the output parameter (`out`), use `MakeByRefType()`.
- **Constructor requirement:**
  - The method should require a public parameterless constructor for types creating objects.
  - If parameterless constructor doesn't exist, throw `InvalidOperationException` with a readable message.
- **Parsing errors:**
  - In case of unsuccessful parsing, throw `ParseException` (with description of input and target type).
  - The main application (`Main` method) can catch `ParseException` and ask the user to refill (e.g., re-invoke `CraftInstance<>`).

{{% /hint %}}

{{% hint info %}}
**Helpful Materials:**

- [Microsoft Learn: Reflection](https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/reflection)
- [Microsoft Learn: The typeof operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/type-testing-and-cast#the-typeof-operator)
- [Microsoft Learn: PropertyInfo Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.propertyinfo?view=net-9.0)
- [Microsoft Learn: MethodBase.Invoke Method](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.invoke?view=net-9.0)

{{% /hint %}}

### Example Solution

> [!TIP] 
> **Solution** 
> {{< filetree dir="labs/lab08/solution/TypeCrafter" >}}
>
> **Output:** [TypeCrafter.txt](/labs/lab08/outputs/TypeCrafter.txt)

## Unit Testing Library

{{% hint info %}}
**What are unit tests?**

Unit tests are automatic, small and fast tests that check individual units of code — most often individual methods or classes — in isolation from the rest of the system. Their purpose is early error detection, documenting module behavior and facilitating refactoring.

**Why write unit tests?**

- **Regression detection**: Tests help quickly determine if a code change broke existing functionality;
- **Behavior documentation**: Tests show how a class/method should behave;
- **Facilitated refactoring**: It's safer to change code structure when you have a set of automated tests;
- **Faster error localization**: Errors are usually localized to a small piece of code.

**Popular libraries and tools for C#**

- [xUnit](https://xunit.net/?tabs=cs) - modern, good for .NET Core/.NET 6+, supports parallel tests.
- [NUnit](https://nunit.org/) - stable, feature-rich, useful when migrating from older projects.
- [MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-mstest) - simple and well integrated with Visual Studio (default in some templates).
- [FluentAssertions](https://fluentassertions.com/introduction) - extends assertion readability through "fluent" style and improves error diagnostics through nicer messages.
- [Moq](https://github.com/devlooped/moq/wiki/Quickstart) - very popular framework for creating mocks in C#.

{{% /hint %}}

### Task Description

Your task is to create from scratch your own lightweight unit testing framework.

The project should contain 2 components:

- **`MiniTest` Library** – containing test attributes allowing users to mark classes and methods as test containers and assertion methods.
- **`MiniTestRunner` (executable program)** – application that dynamically loads a collection of assemblies containing tests, searches for test containers, runs found tests and presents results in console.

> [!TIP] > **Starting code**  
> Starting code contains a library with unit tests that can may be used as an input for **MiniTestRunner** program.
> {{< filetree dir="labs/lab08/student/MiniTest" >}}
>
> **Output:** [MiniTestRunner.txt](/labs/lab08/outputs/MiniTestRunner.txt)

#### MiniTest

**Test attributes:**

The library should provide the following attributes for marking classes and methods as test containers and managing test lifecycle:

- `TestClassAttribute` – marks a class as a container for test methods.
- `TestMethodAttribute` – marks a method as a unit test to execute.
- `BeforeEachAttribute` – specifies a method to be run before each test.
- `AfterEachAttribute` – specifies a method to be run after each test.
- `PriorityAttribute` – sets priority (integer) for tests. Lower number means higher priority.
- `DataRowAttribute` – enables parameterized tests by passing data to test methods.
  - accepts an array of objects (`object?[]`) representing test data,
  - optionally accepts a string documenting the test data set.
- `DescriptionAttribute` – allows adding description to a test or test class.

**Assertions:**

The library should also provide methods for verifying test success or failure.
They should be handled by a static `Assert` class containing assertion methods:

- `ThrowsException<TException>(Action action, string message = "")` – checks if a specific exception type is thrown during execution.
- `AreEqual<T>(T? expected, T? actual, string message = "")` – compares expected and actual values.
- `AreNotEqual<T>(T? notExpected, T? actual, string message = "")` – checks if values are different.
- `IsTrue(bool condition, string message = "")` – confirms that a logical condition is true.
- `IsFalse(bool condition, string message = "")` – confirms that a logical condition is false.
- `Fail(string message = "")` – explicitly marks test as failed with given message.

Each method should throw an exception when the condition is not met. All should optionally accept a description (`message`).

**Exception handling:**

You should implement a custom `AssertionException` used exclusively for failed assertions.

Each assertion method should clearly describe the reason for failure, e.g.:

- `ThrowsException`:
  - `Expected exception of type <{typeof(TException)}> but received <{ex.GetType()}>. {message}`
  - `Expected exception of type <{typeof(TException)}> but no exception was thrown. {message}`
- `AreEqual`:
  - `Expected: {expected}. Received: {actual}. {message}`
- `AreNotEqual`:
  - `Expected any value other than: {notExpected}. Received: {actual}. {message}`

#### MiniTestRunner

`MiniTestRunner` is a console application responsible for finding and executing tests and reporting results.

**Input:**

The program accepts paths to assembly files as command line arguments. They should contain test classes and methods marked with `MiniTest` attributes.

```bash
MiniTestRunner path/to/test-assembly1.dll path/to/test-assembly2.dll
```

**Assembly loading:**

- Use `AssemblyLoadContext` to dynamically load test assemblies without affecting the main execution context.
- Contexts should be collectible (`isCollectible`) to efficiently manage memory.

**Test discovery:**

- Search for classes marked with `TestClassAttribute`.
- In each test class:
  - discover methods marked with `TestMethodAttribute`,
  - find parameterized tests (`DataRowAttribute`),
  - identify `BeforeEach` and `AfterEach` methods for setup/cleanup logic.
- Skip test classes without parameterless constructor.
- Ignore invalid configurations (e.g., bad parameters for `DataRow`).
- Print warning to console when configuration is invalid.

**Test execution:**

- Tests should be executed by priority (`PriorityAttribute` – lower number means higher priority).
- Missing attribute means priority `0`.
- With the same priority, execute alphabetically by method names.

For each class and test:

1. Run `BeforeEach` method.
2. Run test.
3. Run `AfterEach` method.
4. Treat parameterized tests (`DataRow`) as separate tests.
5. A test is considered failed if an unhandled exception occurred.

**Results and formatting:**

Individual test results:

- Status: `PASSED` or `FAILED`.
- Failure reason + exception messages.
- Test or class description (if provided).

Summary after each class:

- Number of all tests.
- Number of successful and failed tests.

Summary after each assembly:

- Total number of tests.
- Number of successful and failed tests.

Console coloring:

- `Green` = passed tests,
- `Red` = failed tests,
- `Yellow` = warnings (e.g., missing constructor, invalid configuration).

{{% hint warning %}}
**Implementation Notes**

- **Compatibility with example tests:**
  - The `MiniTest` library implementation should be fully compatible with example tests contained in the starter project.
  - Details of "business logic" that the example unit tests concern are not important in this task.

{{% /hint %}}

{{% hint info %}}
**Helpful Materials:**

- [Microsoft Learn: Attributes](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/)
- [Microsoft Learn: Retrieving Attributes from Class Members](https://learn.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes#retrieving-attributes-from-class-members)
- [Microsoft Learn: Retrieving Information Stored in Attributes](https://learn.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes)
- [Microsoft Learn: Assembly Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly?view=net-9.0)

{{% /hint %}}

### Example Solution

> [!TIP] 
> **Solution** 
> {{< filetree dir="labs/lab08/solution/MiniTest" >}}
>
> **Output:** [MiniTestRunner.txt](/labs/lab08/outputs/MiniTestRunner.txt)
