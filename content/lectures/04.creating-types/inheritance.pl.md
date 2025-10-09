---
title: "Dziedziczenie"
weight: 20
---

# Dziedziczenie

Dziedziczenie działa podobnie jak w C++. Mamy jeden tryb dziedziczenia, który odpowiadałby publicznemu dziedziczeniu z C++. W C# nie mamy wielodziedziczenia: możemy dziedziczyć tylko po jednej klasie na raz.

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

Tak jakbyśmy się spodziewali `Student`, to także `Person` posiada wszystko to co posiada klasa bazowa.

```csharp
Student alice = new Student() { FirstName = "Alice", 
                                LastName = "Brown", 
                                Age = 25, 
                                StudentID = "X-84355" };
Console.WriteLine($"{alice.FirstName} {alice.LastName}, {alice.Age}, {alice.StudentID}");
```

## Polimorfizm

Referencje są polimorficzne, można je traktować tak jakby były typu bazowego. Możemy przekazać obiekt klasy `Student` do metody, która akceptuje obiekt klasy bazowej - w końcu to jest to samo. Dzięki temu możemy pisać rozszerzalny kod, który operuje na typie ogólnym, nie zastanawiając się o konkretne implementacje.

```csharp
Register(alice);

public void Register(Person person)
{
// ...
}
```

Możemy także niejawnie rzutować typ podklasy, do typu bazowego. W drugą stronę wymaga to jawnego rzutowania i może zakończyć się wyjątkiem `InvalidCastException`.

```csharp
Person person = alice; // Implicit cast
Student student = (Student)person; // Requires explicit cast
```

### Operator `as`

Operator `as` działa podobnie do `dynamic_cast` z C++. Możemy go użyć, żeby wykonać rzutowanie w dół, które zwraca `null` jeżeli się ono nie powiedzie.

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

Są jednak lepsze sposoby na sprawdzenie typu.

### Operator `is`

Generalnie operator `is` sprawdza, czy zmienna pasuje do *wzorca* i zwraca wynik w postaci boola. Jednym ze wzorców który nas interesuje jest wzorzec typu.

```csharp
if (alice is Teacher)
{
    Teacher teacher = (Teacher)alice;
    Console.WriteLine("SAP ID: {teacher.SapID}");
}
```

Dodatkowo we wzorcu typu możemy wprowadzić zmienną tego typu.

```csharp
if (alice is Teacher teacher)
{
    Console.WriteLine("SAP ID: {teacher.SapID}");
}
```

## Metody wirtualne

Funkcje wirtualne działają na tej samej zasadzie co w C++. Pod spodem wykorzystują mechanizm podobny do tablic funkcji wirtualnych: [*Virtual Stub Dispatch*](https://github.com/dotnet/runtime/blob/main/docs/design/coreclr/botr/virtual-stub-dispatch.md). Zasada działania jest podobna, z tą różnicą, że wywołanie metody wirtualnej prowadzi do fragmentu kodu, zwanego `Stub`em, który podobnie jak vtable znajduje adres właściwej metody. Jednak zamiast przekazać sterowanie do metody, jakby to się stało w przypadku vtable, `Stub` podmienia w locie adres metody tak żeby następne wywołanie bezpośrednio wskazywało na znaleziony adres. Kolejne wywołania wywołują bezpośrednio odpowiednią implementację.

W C# wirtualne mogą być nie tylko metody, ale też właściwości, indeksery i zdarzenia.

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
> Jeżeli chodzi o słowo kluczowe `base`, to ma ono tutaj dwa znaczenia. Możemy go użyć do:
>
> - Wywołania do metod nadpisanych
> - Wywołania konstruktora klasy bazowej
>
> Działa analogicznie do słówka `this`, ale dla klasy bazowej.

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
> **Kod źródłowy**
> {{< filetree dir="lectures/creating-types/inheritance/Race" >}}

Całość działa analogicznie jakby to działało w C++. Jeśli `vehicle` jest typu `Car`, to wywoła się `Car.Run`, jeżeli typu `Bike`, to wywoła się `Bike.Run`. Jeżeli typ nie nadpisałby tej metody to wywołałoby się `Vehicle.Run`. Jedyna kosmetyczna różnica jest taka, że w C# słówko `override` jest wymagane, jeżeli nadpisujemy wirtualną metodę.

## Klasy abstrakcyjne

Klasy abstrakcyjne to klasy ze słówkiem kluczowym `abstract`. Nie możemy inicjalizować obiektów tej klasy, możemy w takiej klasie definiować abstrakcyjne składowe - czyli takie, które mają sygnaturę, ale nie mają implementacji. Mogą to być abstrakcyjne metody, właściwości, indeksery i zdarzenia. Jest to analogia do klas z C++, które mają zadeklarowane funkcje czysto wirtualne.

Zamiast dostarczać implementację metody `Vehicle.Run`, możemy zrobić ją abstrakcyjną. Wtedy, każda nieabstrakcyjna subklasa musi dostarczyć własną implementację.

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

## Ukrywanie składowych

## `sealed`

## Konstruktory