---
title: "Typy zagnieżdżone"
weight: 60
---

# Typy zagnieżdżone

W C# możemy dowolnie zagnieżdżać typy. Jest to przydatne, gdy nie chcemy zaśmiecać przestrzeni nazw, a zagnieżdżane typy są ściśle powiązane z jakimś typem.

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

Domyślnie typy zagnieżdżone mają prywatny modyfikator dostępu, tak jak reszta składowych klas i struktur. Poza tym mogą mieć dowolny modyfikator dostępu. Zagnieżdżony typ ma dostęp do prywatnych składowych otaczającego typu. W przykładzie klasa `Builder` wywołuje prywatny konstruktor klasy `Pizza`:

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

Z zewnątrz klasy stworzenie i odniesienia do typów zagnieżdżonych wymagają podania pełnej klasyfikowanej nazwy:

```csharp
Pizza.Builder largePepperoniBuilder = new Pizza.Builder(40);

largePepperoniBuilder.AddTopping(Pizza.Topping.Pepperoni);
largePepperoniBuilder.AddTopping(Pizza.Topping.Cheese);

Pizza largePepperoni = largePepperoniBuilder.Build();
```
