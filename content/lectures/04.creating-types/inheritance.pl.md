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

## Klasy abstrakcyjne

## Ukrywanie składowych

## `sealed`

## Konstruktory