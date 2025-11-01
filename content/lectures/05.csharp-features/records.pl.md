---
title: "Rekordy"
---

## Rekordy (C# 9)

Rekord to specjalny rodzaj klasy, przeznaczony do pracy z niezmiennymi danymi. Pozwala w zwięzły sposób zdefiniować typ, unikając pisania standardowego kodu.

```csharp
public record Person(string FirstName, string LastName);
```

Kompilator w miejsce rekordu wygeneruje nam klasę `Person` wraz z właściwościami *init-only*, konstruktorem, dekonstruktorem, operatorami porównania oraz nadpisanymi metodami `Equals`, `GetHashCode` i `ToString`.

{{% details title="Klasa `Person` wygenerowana przez kompilator" open=false %}}
```csharp
[CompilerGenerated]
[NullableContext(1)]
[Nullable(0)]
public class Person : IEquatable<Person>
{
    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string <FirstName>k__BackingField;

    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string <LastName>k__BackingField;

    [CompilerGenerated]
    protected virtual Type EqualityContract
    {
        [CompilerGenerated]
        get
        {
            return typeof(Person);
        }
    }

    public string FirstName
    {
        [CompilerGenerated]
        get
        {
            return <FirstName>k__BackingField;
        }
        [CompilerGenerated]
        init
        {
            <FirstName>k__BackingField = value;
        }
    }

    public string LastName
    {
        [CompilerGenerated]
        get
        {
            return <LastName>k__BackingField;
        }
        [CompilerGenerated]
        init
        {
            <LastName>k__BackingField = value;
        }
    }

    public Person(string FirstName, string LastName)
    {
        <FirstName>k__BackingField = FirstName;
        <LastName>k__BackingField = LastName;
        base..ctor();
    }

    [CompilerGenerated]
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Person");
        stringBuilder.Append(" { ");
        if (PrintMembers(stringBuilder))
        {
            stringBuilder.Append(' ');
        }
        stringBuilder.Append('}');
        return stringBuilder.ToString();
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("FirstName = ");
        builder.Append((object)FirstName);
        builder.Append(", LastName = ");
        builder.Append((object)LastName);
        return true;
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public static bool operator !=(Person left, Person right)
    {
        return !(left == right);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public static bool operator ==(Person left, Person right)
    {
        return (object)left == right || ((object)left != null && left.Equals(right));
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
        return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(<FirstName>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(<LastName>k__BackingField);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public override bool Equals(object obj)
    {
        return Equals(obj as Person);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public virtual bool Equals(Person other)
    {
        return (object)this == other || ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(<FirstName>k__BackingField, other.<FirstName>k__BackingField) && EqualityComparer<string>.Default.Equals(<LastName>k__BackingField, other.<LastName>k__BackingField));
    }

    [CompilerGenerated]
    public virtual Person <Clone>$()
    {
        return new Person(this);
    }

    [CompilerGenerated]
    protected Person(Person original)
    {
        <FirstName>k__BackingField = original.<FirstName>k__BackingField;
        <LastName>k__BackingField = original.<LastName>k__BackingField;
    }

    [CompilerGenerated]
    public void Deconstruct(out string FirstName, out string LastName)
    {
        FirstName = this.FirstName;
        LastName = this.LastName;
    }
}
```
{{% /details %}}

### Semantyka porównania

Rekordy implementują porównywanie przez wartość, podobnie jak krotki. Metoda `Equals` i operatory porównania sprawdzają po kolei czy wszystkie właściwości są sobie równe.

```csharp
public record Person(string FirstName, string LastName, int Age);

var john = new Person("John", "Doe", 30);
var doe = new Person("John", "Doe", 30);

Console.WriteLine($"Person: {john == doe}"); // True
```

### Nieniszcząca mutacja (*Non-destructive mutation*)

Obiekty rekordów są niezmienne. Wyrażenie `with` umożliwia tworzenie nowej instancji rekordu, która jest kopią istniejącej, ale ze zmienionymi wybranymi właściwościami. Pod spodem mechanizm ten opiera się na specjalnym, wygenerowanym przez kompilator konstruktorze kopiującym (np. `protected Person(Person original)`), który tworzy płytką kopię obiektu przed zastosowaniem zmian.

```csharp
var john = new Person("John", "Doe", 30);
var jane = john with { FirstName = "Jane", Age = 0 };

Console.WriteLine(john); // Person { FirstName = John, LastName = Doe, Age = 30 }
Console.WriteLine(jane); // Person { FirstName = Jane, LastName = Doe, Age = 0 }
```

### `struct record` (C# 10)

Od C# 10 możemy również tworzyć rekordy typu bezpośredniego. Kompilator dla `record struct` wygeneruje mutowalną strukturę, a dla `readonly record struct` niemutowalną.

```csharp
public record struct Vector3(double X, double Y, double Z);
public readonly record struct Point2(double X, double Y);
```

> Samo słowo kluczowe `record` jest skrótem dla `record class`.

### Dostosowywanie rekordów

Rekordy w zamyśle służą do przechowywania danych, a do tego zestaw wygenerowany przez kompilator zazwyczaj jest wystarczający. Można w razie potrzeby dodawać także do rekordów swoje własne pola, właściwości i metody. Co więcej, jeśli dostarczymy własną implementację funkcjonalności, którą normalnie generuje kompilator (np. metodę `ToString()`), to nasza wersja zostanie użyta, nadpisując domyślne zachowanie.

```csharp
public record Product(string Name, decimal Price)
{
    public int Quantity { get; set; }

    public Product(Product original)
    {
        Name = original.Name;
        Price = original.Price;
        Quantity = 0;
    }

    public override string ToString() => $"{Name}({Quantity}): {Price:C}";
}
```

```csharp
Product apple = new Product("Apple", 1.99m) { Quantity = 5 };
Product copy = apple with {Price = 2.99m};
Console.WriteLine(apple); // Apple(5): $1.99
Console.WriteLine(copy);  // Apple(0): $2.99
```

## Typy anonimowe

Typy anonimowe to proste, małe klasy tworzone "w locie" przez kompilator, bez potrzeby nadawania im jawnej nazwy. Używa się ich do tworzenia obiektów, które mają przechowywać dane tylko tymczasowo, w obrębie jednej metody.

Tworzy się je za pomocą słowa kluczowego `new` i inicjalizatora obiektów:

```csharp
var anon = new { Name = "Alice", Age = 23 };
```

Kompilator automatycznie wygeneruje klasę, która posiada publiczne, niemodyfikowalne właściwości, konstruktor oraz przeciążenia `Equals`, `GetHashCode` i `ToString`.

{{% details title="Klasa anonimowa wygenerowana przez kompilator" open=false %}}
```csharp
[CompilerGenerated]
[DebuggerDisplay("\\{ Name = {Name}, Age = {Age} }", Type = "<Anonymous Type>")]
internal sealed class <>f__AnonymousType0<<Name>j__TPar, <Age>j__TPar>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly <Name>j__TPar <Name>i__Field;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly <Age>j__TPar <Age>i__Field;

    public <Name>j__TPar Name
    {
        get
        {
            return <Name>i__Field;
        }
    }

    public <Age>j__TPar Age
    {
        get
        {
            return <Age>i__Field;
        }
    }

    [DebuggerHidden]
    public <>f__AnonymousType0(<Name>j__TPar Name, <Age>j__TPar Age)
    {
        <Name>i__Field = Name;
        <Age>i__Field = Age;
    }

    [DebuggerHidden]
    public override bool Equals(object value)
    {
        <>f__AnonymousType0<<Name>j__TPar, <Age>j__TPar> anon = value as <>f__AnonymousType0<<Name>j__TPar, <Age>j__TPar>;
        return this == anon || (anon != null && EqualityComparer<<Name>j__TPar>.Default.Equals(<Name>i__Field, anon.<Name>i__Field) && EqualityComparer<<Age>j__TPar>.Default.Equals(<Age>i__Field, anon.<Age>i__Field));
    }

    [DebuggerHidden]
    public override int GetHashCode()
    {
        return (-2097246416 * -1521134295 + EqualityComparer<<Name>j__TPar>.Default.GetHashCode(<Name>i__Field)) * -1521134295 + EqualityComparer<<Age>j__TPar>.Default.GetHashCode(<Age>i__Field);
    }

    [DebuggerHidden]
    [return: Nullable(1)]
    public override string ToString()
    {
        object[] array = new object[2];
        <Name>j__TPar val = <Name>i__Field;
        array[0] = ((val != null) ? val.ToString() : null);
        <Age>j__TPar val2 = <Age>i__Field;
        array[1] = ((val2 != null) ? val2.ToString() : null);
        return string.Format(null, "{{ Name = {0}, Age = {1} }}", array);
    }
}
```
{{% /details %}}

Podobnie jak dla rekordów klasy anonimowe są porównywane przez wartość, oraz wspierają nieniszczącą mutację.

> Najważniejszym ograniczeniem typów anonimowych jest ich zasięg lokalny. Nie można ich użyć jako typu zwracanego przez metodę ani jako parametru, ponieważ nie mają nazwy, do której można by się odwołać. Są one przeznaczone wyłącznie do tymczasowego użytku wewnątrz jednej metody.

### Nazwy właściwości

Możemy jawnie nadać nazwy właściwości. Jeżeli tego nie zrobimy, kompilator spróbuje nazwę wydedukować na podstawie nazwy przekazanego pola lub właściwości.

```csharp
int age = 23;
var anon = new { Name = "Bob", age, age.ToString().Length };

Console.WriteLine($"His {nameof(anon.Name)} is {anon.Name}");
Console.WriteLine($"His {nameof(anon.age)} is {anon.age}");
Console.WriteLine($"His {nameof(anon.Length)} is {anon.Length}");
```

### Typy anonimowe w LINQ

Typy anonimowe są szczególnie przydatne w LINQ, gdy potrzebujemy w locie stworzyć obiekt agregujący jakieś dane.

```csharp
var queryResult = ratings
    .GroupBy(r => r.MovieId)
    .Select(g => new
    {
        MovieId = g.Key,
        Average = g.Average(r => r.Score)
    })
    .Where(x => x.Average > 8)
    .Join(movies,
        rating => rating.MovieId,
        movie => movie.Id,
        (rating, movie) => new
        {
            Movie = movie,
            rating.Average
        })
    .GroupJoin(casts,
        movie => movie.Movie.Id,
        cast => cast.MovieId,
        (movie, movieCasts) => new
        {
            movie.Movie,
            movie.Average,
            CastIds = movieCasts.Select(c => c.ActorId)
        })
    .Select(x => new
    {
        x.Movie,
        x.Average,
        Cast = x.CastIds
            .Join(actors,
                actorId => actorId,
                actor => actor.Id,
                (actorId, actor) => actor)
            .ToList()
    });
```
