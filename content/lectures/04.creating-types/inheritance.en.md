---
title: "Inheritance"
weight: 20
---

# Inheritance

Inheritance works similarly to C++. We have one mode of inheritance, which would correspond to public inheritance from C++. In C#, we do not have multiple inheritance: we can only inherit from one class at a time.

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

public class Student : Person
{
    public string StudentID { get; set; }
}
```

As we would expect, a `Student` is also a `Person` and has everything the base class has.

```csharp
Student alice = new Student() { FirstName = "Alice", 
                                LastName = "Brown", 
                                Age = 25, 
                                StudentID = "X-84355" };
Console.WriteLine($"{alice.FirstName} {alice.LastName}, {alice.Age}, {alice.StudentID}");
```

## Polymorphism

References are polymorphic; they can be treated as if they were of the base type. We can pass a `Student` object to a method that accepts a base class object - after all, it's the same thing. This allows us to write extensible code that operates on a general type, without worrying about specific implementations.

```csharp
Register(alice);

public void Register(Person person)
{
// ...
}
```

We can also implicitly cast a subclass type to a base type. The other way around requires an explicit cast and may result in an `InvalidCastException`.

```csharp
Person person = alice; // Implicit cast
Student student = (Student)person; // Requires explicit cast
```

### `as` Operator

The `as` operator works similarly to `dynamic_cast` from C++. We can use it to perform a downcast that returns `null` if it fails.

```csharp
Teacher teacher = alice as Teacher;

if (teacher != null)
{
    Console.WriteLine("SAP ID: {teacher.SapID}");
}

public class Teacher : Person
{
    public int SapID { get; set; }
}
```

However, there are better ways to check the type.

### `is` Operator

In general, the `is` operator checks if a variable matches a *pattern* and returns a boolean result. One of the patterns that interests us is the type pattern.

```csharp
if (alice is Teacher)
{
    Teacher teacher = (Teacher)alice;
    Console.WriteLine("SAP ID: {teacher.SapID}");
}
```

Additionally, in the type pattern, we can introduce a variable of that type.

```csharp
if (alice is Teacher teacher)
{
    Console.WriteLine("SAP ID: {teacher.SapID}");
}
```

## Virtual Methods

Virtual functions work on the same principle as in C++. Under the hood, they use a virtual function tables.

In C#, not only methods can be virtual, but also properties, indexers, and events.

```csharp
public class Vehicle
{
    public float Position { get; protected set; } = 0;
    public virtual float Speed { get; protected set; } = 1.0;
    public string Name { get; }
    
    public Vehicle(string name) => Name = name;
    public virtual float Run(float dt)
    {
        Console.WriteLine($"Vehicle.Run({dt})");
        return (Position = Position + dt * Speed);
    }
}

public class Car : Vehicle
{
    public override float Speed { get; protected set; } = 0.0f;
    public virtual float Acceleration { get; }
    
    public Car(string name, float acceleration) : base(name) => Acceleration = acceleration;
    public override float Run(float dt)
    {
        Console.WriteLine($"Car.Run({dt})");
        Position += dt * Speed;
        Speed += dt * Acceleration;
        return Position;
    }
}

public class Bike : Vehicle
{    
    public Bike(string name) : base(name) {}
    public override float Run(float dt)
    {
        Console.WriteLine($"Bike.Run({dt})"); // We can skip implementation if not for the output.
        return base.Run(dt);
    }
}
```

> [!IMPORTANT]
> ### `base`
> When it comes to the `base` keyword, it has two meanings here. We can use it to:
>
> - Call overridden methods
> - Call the base class constructor
>
> It works analogously to the `this` keyword, but for the base class.

```csharp
List<Vehicle> vehicles = [new Bike("Romet"), new Car("Honda Civic", 1.5f), new Car("Toyota Yaris", 1.0f)];

const float dt = 1.0f;
for (float time = 0.0f; time < 4.0f; time += dt)
{
    Console.WriteLine($"====== time: {time,5:F1}s ======");
    foreach (var vehicle in vehicles)
    {
        vehicle.Run(dt);
    }
    foreach (var vehicle in vehicles)
    {
        Console.WriteLine($"Vehicle {vehicle.Name}, Position {vehicle.Position}");
    }
}
```

> [!NOTE]
> **Source code**
> {{< filetree dir="lectures/creating-types/inheritance/Race" >}}

The whole thing works analogously to how it would work in C++. If `vehicle` is of type `Car`, `Car.Run` will be called, if it is of type `Bike`, `Bike.Run` will be called. If the type did not override this method, `Vehicle.Run` would be called. The only cosmetic difference is that in C# the `override` keyword is required if we want to override a virtual method - if we don't do this, we just hide it and generate a compiler warning.

## Abstract Classes

Abstract classes are classes with the `abstract` keyword. We cannot initialize objects of this class, we can define abstract members in such a class - that is, those that have a signature, but no implementation. These can be abstract methods, properties, indexers, and events. This is an analogy to classes from C++ that have declared pure virtual functions.

Instead of providing an implementation of the `Vehicle.Run` method, we can make it abstract. Then, each non-abstract subclass must provide its own implementation.

```csharp
public abstract class Vehicle
{
    public float Position { get; protected set; } = 0;
    public virtual float Speed { get; protected set; } = 1.0f;
    public string Name { get; }
    
    public Vehicle(string name) => Name = name;
    public abstract float Run(float dt);
}
```

## Hiding Members

A subclass can define the same members as the base class.

```csharp
public class Base
{
    public int Member = 0;
    public string Method() => "Base.Method";
    public virtual string VirtualMethod() => "Base.VirtualMethod";
}

public class Hider : Base
{
    public int Member = 1;
    public string Method() => "Hider.Method";
    public string VirtualMethod() => "Hider.VirtualMethod";
}

public class Overrider : Base
{
    public override string VirtualMethod() => "Overrider.VirtualMethod";
}
```

In this example, the fields and methods in `Hider` are *hidden*. This is usually not intentional - the compiler generates a warning in this case.

The consequences of hiding can be seen in the example:

```csharp
Hider hider = new Hider();
Base baseHider = hider;

Overrider overrider = new Overrider();
Base baseOverrider = overrider;

Console.WriteLine(hider.Method()); // Hider.Method
Console.WriteLine(hider.VirtualMethod()); // Hider.VirtualMethod
Console.WriteLine(baseHider.Method()); // Base.Method
Console.WriteLine(baseHider.VirtualMethod()); // Base.VirtualMethod

Console.WriteLine(overrider.Method()); // Base.Method
Console.WriteLine(overrider.VirtualMethod()); // Overrider.VirtualMethod
Console.WriteLine(baseOverrider.Method()); // Base.Method
Console.WriteLine(baseOverrider.VirtualMethod()); // Overrider.VirtualMethod
```
> [!NOTE]
> **Source code**
> {{< filetree dir="lectures/creating-types/inheritance/Hiding" >}}

Hidden methods are not overridden, usually if a method was marked as `virtual`, the intention is to override it. If the intention was actually to hide it, the compiler warnings can be silenced with the `new` keyword. This is the only action of this keyword in this context.

```csharp
public class Hider : Base
{
    public new int Member = 1;
    public new string Method() => "Hider.Method";
    public new string VirtualMethod() => "Hider.VirtualMethod";
}
```

## `sealed`

The `sealed` keyword applied to a method means that it cannot be overridden in a subclass. This does not mean, however, that such a method cannot be hidden. Applied to a class, it means that such a class cannot be inherited from.

## Constructors

Constructors are not inherited. A derived class must define its own set of constructors.

A derived class must also take care of initializing the base class. In the constructor of the derived class, we can use `base` to call one of the constructors of the base class.

```csharp
public class Base
{
    public int X;
    public Base(int x) => X = x;
}

public class Derived : Base
{
    public int Y;
    public Derived(int x, int y) : base(x) => Y = y;
}
```

If the base class provides a parameterless constructor, we can omit the explicit call to the base class constructor, but the parameterless constructor of the base class will be called implicitly.

```csharp
public class Base
{
    public int X;
    public Base() => X = 1;
}

public class Derived : Base
{
    public int Y;
    public Derived() => Y = 1; // Can skip implicit base call
}
```

### Initialization Order

1. Fields and properties of the derived class
2. Fields and properties of the base class
3. Base class constructor
4. Derived class constructor
