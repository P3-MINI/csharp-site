---
title: "Nested Types"
weight: 60
---

# Nested Types

In C#, you can define types within other types. These are called **nested types**. This is useful when a type is tightly coupled with its containing type, and you want to avoid cluttering the namespace.

```csharp
public class Car
{
    public class Engine 
    {
        public int HorsePower { get; set; }
        public EngineType Type { get; set; }
    }
    
    public enum EngineType { Gasoline, Diesel, Electric }
}
```

By default, nested types have a `private` access modifier, just like other members of classes and structs. However, you can specify any access modifier. A nested type has access to all members of its containing type, including private ones. In the example, the `Builder` class calls the private constructor of the `Pizza` class:

```csharp
public class Pizza
{
    public int SizeCm { get; }
    public IReadOnlyList<Topping> Toppings { get; }
    
    private Pizza(Builder builder)
    {
        SizeCm = builder.SizeCm;
        Toppings = new List<Topping>(builder.Toppings);
    }
    
    public enum Topping { Pepperoni, Sausage, Mushrooms, Cheese, Onions }
    
    public class Builder
    {
        public int SizeCm { get; }
        public List<Topping> Toppings { get; } = new List<Topping>();
        
        public Builder(int sizeCm) => SizeCm = sizeCm;
        
        public Builder AddTopping(Topping topping)
        {
            Toppings.Add(topping);
            return this;
        }
        
        public Pizza Build() => new Pizza(this);
    }
}
```

From outside the containing class, creating and referencing a nested type requires using its full, qualified name:

```csharp
Pizza.Builder largePepperoniBuilder = new Pizza.Builder(40);

largePepperoniBuilder.AddTopping(Pizza.Topping.Pepperoni);
largePepperoniBuilder.AddTopping(Pizza.Topping.Cheese);

Pizza largePepperoni = largePepperoniBuilder.Build();
```
