---
title: "Classes"
weight: 10
---

# Classes

Classes are defined in the following way:

```csharp
class ClassName
{
}
```

Unlike in C++, classes can have class modifiers preceding the class name:
- `internal` - non-nested classes are internal by default, meaning that they are visible only inside the assembly (executable or `.dll`)
- `public` - can be seen from everywhere
- `static` - instances cannot be created, must contain only static members
- `sealed` - the class cannot be inherited from
- `abstract` - an object of this class cannot be created (equivalent to a class containing a pure virtual method in C++)

In C#, each class member has its own access modifier:

```csharp
class MyClass
{
    public int Member1;
    private string _member2;
    protected float Member3;
}
```

## Fields

A field is a variable that is a member of a class or struct.

```csharp
class Student
{
    private string _name; // This is a field
    public int Year; // Still a field
}
```

Field modifiers are similar to those in C++, the main difference lies between `readonly` and `const`. `const` members must be initialized in declaration, and the value has to be known at compile time, while `readonly` fields can be assigned only in their declaration or in a constructor, and cannot be changed after the constructor finishes.

Private fields should be written in camelCase, starting with an underscore, otherwise in PascalCase. More guidelines can be found in the [documentation](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names).

Uninitialized fields have their value set to bitwise `0` (i.e., `0`, `false`, `null`); this happens before the constructor is called.

## Methods

Methods work in the same way as in C++, but in C# we do not split declaration and implementation into different files, implementation is written inside the class itself.

If a method contains a single expression, such as:

```csharp
int Foo (int x) { return x * 2; }
```

It can be written using a simplified syntax:

```csharp
int Foo (int x) => x * 2;
```

Unlike in C++, we can also define local functions (a method within another method):

```csharp
void MyMethod()
{
    void PrintInt(int value) => Console.WriteLine(value);

    PrintInt(1);
    PrintInt(2);
    PrintInt(3);
}
```

Same as in C++, methods can also be overloaded.

## Constructors

Constructors are defined in the same way as in C++, but *do not contain an initializer list.*

Constructors can call other constructors using `this` keyword.

```csharp
class Book
{
    private string _title;
    private int _year;
    Book(string title) => _title = title;
    Book(string title, int year) : this(title)
    {
        _year = year;
    }
}
```

If a class does not define any constructors, a parameterless constructor is generated automatically.

### Primary Constructors (C# 12)

Since C# 12, constructor parameters can be declared directly after the name of a class or struct. Parameters of such a *primary constructor* are in scope in the whole body of the type: they can be used to initialize fields and properties, or directly in members.

```csharp
public class Account(string owner, decimal balance)
{
    public string Owner { get; } = owner;                     // initializes a property

    public Account(string owner) : this(owner, 0) { }         // must call the primary constructor

    public void Deposit(decimal amount) => balance += amount; // captured parameter
    public override string ToString() => $"{Owner}: {balance}";
}

public class SavingsAccount(string owner, decimal rate) : Account(owner) // calls the base class constructor
{
    public decimal Rate => rate;
}
```

- Every other constructor must call the primary constructor (directly or indirectly) using `this(...)`, and a class with a primary constructor does not get an implicit parameterless constructor.
- The base class constructor is called directly in the type declaration: `: Account(owner)`.
- Unlike in [records]({{< ref "/lectures/05.csharp-features/records" >}}), primary constructor parameters of classes and structs **do not become properties**. If a parameter is used in a member (like `balance` above), the compiler captures it in a hidden private field. It is still a parameter, though: it can be modified, it cannot be accessed through `this`, and it is not visible outside the type.

```csharp
var account = new Account("Alice", 100);
account.Deposit(50);
Console.WriteLine(account);            // Alice: 150
// Console.WriteLine(account.balance); // Compilation error
```

## Deconstructors

A deconstructor (a method named `Deconstruct` with `out` parameters) works in the opposite direction to a constructor: it assigns the fields of an object back to separate variables.

```csharp
class Point
{
    public float x, y;

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void Deconstruct(out float x, out float y)
    {
        x = this.x;
        y = this.y;
    }
}
```

The deconstructor can be called in the following ways:

```csharp
var point = new Point(1, 2);

(float x, float y) = point;
// or: (var x, var y) = point;
// or: var (x, y) = point;

Console.WriteLine($"x: {x}, y: {y}"); // x: 1, y: 2
```

> Do not confuse deconstruction with destruction, the object after deconstruction is still valid.

## Object Initializers
An object initializer can be used to initialize accessible (e.g., `public`) fields or properties directly after construction.

```csharp
class Hamster
{
    public string Name;
    public bool LikesViolence;

    public Hamster() {}
    public Hamster(string name) => Name = name;
}
```

It can be used as follows:

```csharp
Hamster h1 = new Hamster {Name = "Boo", LikesViolence=true};
Hamster h2 = new Hamster ("Boo")       {LikesViolence=true};
```

The **order** of initialization is:
1. field initializers (values assigned in field and property declarations)
2. constructor body
3. object initializer

## Properties

Properties look like fields, taste like fields, but are actually getter and setter methods in disguise. They can contain additional logic if specified and have the same access modifiers as fields.

```csharp
var book = new ShopItem();
book.Price = 9.99m;
book.Price -= 1.0m;
Console.WriteLine($"Price: {book.Price}$"); // Price: 8.99$

class ShopItem
{
    private decimal _price;
    public decimal Price
    {
        get { return _price; }               // get => _price; 
        set { _price = Math.Max(value, 0); } // set => _price = Math.Max(value, 0);
    }
}
```

> The implicit `value` keyword used in the `set` accessor represents the value that is being assigned to the property.

Either `get` or `set` can be omitted. If the bodies of `get` and `set` are omitted, the compiler will automatically generate a hidden backing field (an *auto-implemented property*). This is the most common use case of properties.

```csharp
class ShopItem
{
    public decimal Price { get; set; } = 0.0m; // auto-implemented properties can be initialized here
    public string Name { get; set; }
}
```

Since C# 14, you can use the `field` keyword to access the compiler-generated backing field directly, without having to declare it manually (if the type already has a member named `field`, inside accessors it has to be referred to as `this.field` or `@field`). Only the accessor that needs additional logic has to be written; the other one can remain automatically implemented:

```csharp
class ShopItem
{
    public decimal Price
    {
        get => field; 
        set => field = Math.Max(value, 0); 
    }

    public string Name { get; set => field = value.Trim(); } = ""; // get is auto-implemented
}
```

> One of the accessors (usually `set`) can have its own, more restrictive access modifier: `public decimal Price { get; private set; }`. Otherwise, accessors have the access modifier of the property.

## Init-only setters

The `set` accessor can be replaced with `init` (C# 9), effectively making the property read-only after initialization. Init-only properties can be initialized in the constructor, in an object initializer or inline. If we omit the `set` accessor, the property can still be initialized in the constructor or inline (but not in an object initializer), and cannot be changed later.

## Indexers
Indexers are similar to overloading `operator[]` in C++. To write an indexer, we define a property called `this`, specifying the arguments in square brackets:

```csharp
class Sentence
{
    private string[] Words { get; }
    
    public string this[int i]
    {
        get => Words[i];
        set { Words[i] = value; }
    }
    
    public Sentence(string sentence) => Words = sentence.Split(' ');
}
```

We use indexers the same way as we use an array, except we can define index arguments to be of any type. The type can declare multiple indexers, and the indexers can have multiple parameters.

```csharp
Sentence sentence = new Sentence("The quick brown fox jumps over lazy dog");
sentence[1] = "swift";
Console.WriteLine(sentence[3]); // fox
```

## Finalizers

Finalizers may seem like destructors from C++, but the difference is that we never know when finalizer will be called. After the last reference to an object goes out of scope, the object awaits garbage collection. When will that happen? When the Garbage Collector decides so - most of the time when there is memory pressure. The finalizer may even never be called: .NET does not run finalizers when the application exits. Because of that it is not the most useful construct; resources should be released deterministically with [`IDisposable`]({{< ref "/lectures/05.csharp-features/disposable" >}}).

```csharp
class Class1
{
    ~Class1()
    {
    }
}
```

