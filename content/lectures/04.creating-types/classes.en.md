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
- `internal`- classes are by default internal, meaning that they are visible only inside assembly (executable or `.dll`)
- `public` - can be seen from everywhere
- `static` - instances cannot be created, must contain only static members
- `sealed` - the class can be no longer inherited from 
- `abstract`- an object of this class cannot be initialized (equivalent to a class containing a pure virtual method in C++)

In C#, each class member has it's own access modifier

```csharp
class MyClass
{
    public int Member1;
    private string _member2;
    protected float Member3;
}
```

## Fields

A field is a variable that is a member of a class or struct

```csharp
class Student
{
    private string _name; // This is a field
    public int Year; // Still a field
}
```

Field modifiers are similar to those in C++, the main difference lies between `readonly` and `const`. `const` members must be initialized in declaration, and the value has to be known at compile time, while `readonly` members have to be initialized after constructor call finishes.

Private fields should be written in camelCase, starting with an underscore, otherwise in PascalCase. More guidelines can be found in the [documentation](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names).

Uninitialized fields, have their value set to bitwise `0`, this happens before the constructor is called.

## Methods

Methods work in the same way as in C++, but in C# we do not split declaration and implementation into different files, implementation is written inside the class itself.

If a method contains a single expression, such as:

```csharp
int Foo (int x) { return x * 2; }
```

It can be written using a simplified syntax

```csharp
int Foo (int x) => x * 2;
```

Similarly to C++, we can also define local methods (a method within another method)

```csharp
void MyMethod
{
    void PrintInt(int value) => Console.Writeline(value);

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

If there is no user-defined parameterless constructor, one is generated automatically.

## Deconstructors

Deconstructors are used to reverse the assignment of fields back to the variables.

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
(var x, var y) = point;
var (x, y) = point;

Console.WriteLine($"x: {x}, y:{y}"); 
```

> Do not confuse deconstruction with destruction, the object after deconstruction is still valid.

## Object Initializers
An Object initializer can be used to initialize `public` fields or properties, directly after construction.

```csharp
class Hamster
{
    public string Name;
    public bool LikesViolence;

    public Hamster () {};
    public Hamster(string name) => Name = name;
}
```

It can be used as follows:

```csharp
Hamster h1 = new Hamster {Name = "Boo", LikesViolence=true};
Hamster h2 = new Hamster ("Boo")       {LikesViolence=true};
```

The **order** of initialization is:
1. fields
2. constructors
3. initializer lists

## Properties

Properties look like fields, taste like fields, but are actually getter and setter methods in disguise. They can contain additional logic if specified and have the same access modifiers as fields.

```csharp
var book = new ShopItem();
book.Price = 9.99m;
book.Price -= 1.0m;
Console.WriteLine($"Price: {book.Price}$");

class ShopItem
{
    private decimal _price;
    public decimal Price
    {
        get { return _price; }               // get => _price; 
        set { _price = Math.Max(value, 0); } // set => Math.Max(value, 0);
    }
}
```

Both `get` and `set` are optional. If `get` and `set` bodies are omitted, the compiler will automatically generate a corresponding field. This is the most common use case of properties.

```csharp
class ShopItem
{
    public decimal Price { get; set; } = 0.0m; // auto-generated properties can be initialized here
    public string Name { get; set ;}
}
```

> Both `get` and `set` can have their own access modifiers: `public decimal Price {get; private set;}`, otherwise they inherit the access modifier of the corresponding property.

## Init-only setters

The `set` accessor can be replaced with `init` effectively making the read-only properties. Init setters can be initialized in the constructor, object initializer list or inline. If we omit `set` accessor the property can still be initialized in the constructor or inline, and cannot be changes later.

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
Console.WriteLine(sentence[3]);
```

## Finalizers

Finalizers may seem like destructors from C++, but the difference is that we never know when finalizer will be called. After the last reference of an object is out of scope, the object awaits garbage collection. When will that happen? When Garbage Collector feels like it - most of the time when there is memory pressure or at random, periodically.

```csharp
class Class1
{
    ~Class1()
    {
    }
}
```

