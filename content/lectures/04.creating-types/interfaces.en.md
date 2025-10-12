---
title: "Interfaces"
---

# Interfaces

Interfaces are similar to abstract classes, but with a key difference: **they define a contract of behaviors and cannot define state (instance fields)**. An analogous structure from C++ would be a class where all members are pure virtual methods.

- Members of an interface can include methods, properties, events, and indexers. Since C# 8, they can also contain static members (including constants) and default method implementations.
- Interfaces **cannot contain instance fields**.
- Members are implicitly public and abstract.
- Classes and structs support implementing multiple interfaces.

A interface definition looks as follows, here is an example of the `IEnumerator` interface from the `System.Collections` namespace:

```csharp
public interface IEnumerator
{
    bool MoveNext();
    object Current { get; }
    void Reset();
}
```

By convention, interface names are prefixed with the letter `I`. Implementing an interface involves providing an implementation for all its members that do not have a default implementation.

```csharp
public class Counter : IEnumerator
{
    public int Count { get; private set; }
    public Counter(int count) => Count = count;
    public bool MoveNext() => Count-- > 0;
    public object Current => Count;
    public void Reset()
    {
        throw new NotSupportedException();
    }
}
```

References are polymorphic with all interfaces that a given type implements.

```csharp
IEnumerator e = new Counter(10);
while (e.MoveNext())
    Console.Write(e.Current);
Console.WriteLine();
```

## Extending Interfaces

Interfaces can extend other interfaces, incorporating all of their members.

```csharp
public interface IUndoable
{
    void Undo();
}

public interface IRedoable : IUndoable
{
    void Redo();
}
```

## Explicit Member Implementation

We can implement interface members explicitly by specifying which interface the member belongs to. This is useful when member names from different interfaces conflict with each other.

```csharp
public interface IFoo1 { void Bar(); }
public interface IFoo2 { void Bar(); }

public class Foo : IFoo1, IFoo2
{
    public void Bar()
    {
        Console.WriteLine("Implementation of IFoo1.Bar");
    }
    
    void IFoo2.Bar()
    {
        Console.WriteLine("Implementation of IFoo2.Bar");
    }
}
```

Explicitly implemented members can be accessed **only** after casting the type to that specific interface.

```csharp
Foo foo = new Foo();
foo.Bar();          // Implementation of IFoo1.Bar
((IFoo1)foo).Bar(); // Implementation of IFoo1.Bar
((IFoo2)foo).Bar(); // Implementation of IFoo2.Bar
```

## Boxing

Calling an interface member on a struct does not cause a boxing operation. However, casting a struct to an interface causes it to be boxed, which means it is copied to the heap.

```csharp
public interface IShape
{
    float Area();
}

public struct Rectangle : IShape
{
    public float Width { get; set; }
    public float Height { get; set; }
    
    public Rectangle(float width, float height)
    {
        Width = width;
        Height = height;
    }
    
    public float Area() => Width * Height;
}
```

```csharp
Rectangle rectangle = new Rectangle(4.0f, 5.0f);
float area = rectangle.Area(); // no boxing
IShape shape = rectangle;      // boxing
```

## Default Implementations (C# 8)

Since C# 8, interfaces can provide default implementations for members. This makes providing a custom implementation in the implementing types optional.

```csharp
public interface ILogger
{
    void Message(string message)
    {
        Console.WriteLine(message);
    }
}

public class Logger : ILogger
{
    public void Message(string message) // optional
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {message}");
    }
}
```

If a type does not provide its own implementation, the default implementation can only be called through a reference of the interface type.

## Static Interface Members (C# 8)

Since static members are not part of an instance's state, interfaces can define them.

```csharp
public interface ILogger
{
    public const string DefaultFile = "default.log";
    public static string LogPrefix { get; set; } = "Log: ";

    void Message(string message)
    {
        Console.WriteLine($"{LogPrefix} {message}");
    }
}
```

### Static inheritance (C# 11)

Starting from C# 11 interfaces allow to define inherit static members - normally as in C++ static members are not inherited. Members in interfaces can be marked `virtual` and `abstract`, then they can be inherited similarly as in instance members in class inheritance.

```csharp
public interface IDescriptable
{
    static abstract string Description { get; }
    static virtual string Category => "General";
}

public class Cat : IDescriptable
{
    public static string Description => "It's a cat";
    public static string Category => "Mammal"; // optional
}

public class Book : IDescriptable
{
    public static string Description => "It's a book";
}
```
