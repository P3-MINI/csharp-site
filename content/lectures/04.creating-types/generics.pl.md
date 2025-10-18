---
title: "Generyki"
weight: 80
---

# Generyki

Generyki w C# wyglądają podobnie i służą do tego samego celu co szablony z C++. Również za ich pomocą piszemy kod niezależny od typu, ale jest to mechanizm prostszy i bezpieczniejszy. 

Szablony w C++ działają w czasie kompilacji, dla każdego typu użytego w szablonie kompilator C++ tworzy nową oddzielną kopię klasy lub funkcji. W C# generyki **działają w czasie wykonania**. Kompilator generyki kompiluje zawsze raz do języka pośredniego, gdzie typy na razie pozostają niepodstawione. Dopiero podczas uruchomienia kompilator JIT (*Just-In-Time*) przy pierwszym użyciu generyka tworzy jej wyspecjalizowaną wersję.

W C++ (przynajmniej przed C++20) kompilator nie weryfikuje operacji na typach szablonowych, aż do próby użycia. Często generuje to bardzo nieprzejrzyste i trudne w zrozumieniu błędy kompilacji. W C# typy generyczne są w pełni bezpieczne, żeby wiedzieć jak można użyć typu, należy go ograniczyć. Podobny mechanizm w C++ istnieje od wersji C++20: [Constraints and concepts](https://en.cppreference.com/w/cpp/language/constraints.html).

W C++ ten szablon jest jak najbardziej poprawny, jednak jeżeli go spróbujemy użyć na typie `T`, który nie definiuje operatora porównywania, to dostaniemy trudny do zdebugowania błąd.

```cpp
template <class T> T Max(T a, T b)
{
    return a > b ? a : b;
}
```

W C# żeby wiedzieć, że typy możemy ze sobą porównywać musielibyśmy na przykład ograniczyć `T` do typu implementującego `IComparable<T>`. Dzięki temu, tej generycznej metody możemy używać tylko na typach porównywalnych, przez co nie będzie niespodzianek po podstawieniu.

```csharp
static T Max<T>(T a, T b) where T : IComparisonOperators<T, T, bool>
{
    return a > b ? a : b;
}
```

## Bez generyków

Wyobraźmy sobie, że mamy do zaimplementowania stos, który ma działać z typami: `int`, `float` i `string`. Bez typów generycznych możemy na przykład zaimplementować trzy klasy: `IntStack`, `FloatStack`, `StringStack`. Jest to jednak dużo kodu do utrzymania, w dodatku powtórzonego. Innym rozwiązaniem jest użycie klasy `object`, żeby napisać jedną implementację stosu, która będzie działać z każdym typem:

```csharp
public class ObjectStack
{
    private object[] _items = new object[8];
    public int Count { get; private set; }

    public void Push(object item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public object Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

Jednak próba użycia takiego stosu uwidacznia problemy z tą implementacją. Po pierwsze wrzucanie na stos typów bezpośrednich wymaga pakowania. Po drugie, taki stos nie zapewnia nam bezpieczeństwa typów. Pobranie elementu wiąże się z niebezpiecznym rzutowaniem w dół. Typy generyczne rozwiązują oba te problemy.

```csharp
ObjectStack stack = new ObjectStack();

for (int i = 0; i < 10; i++)
{
    stack.Push(i);
}

int number = (int) stack.Pop();
string str = (string) stack.Pop(); // Runtime error: InvalidCastException
```

## Typy generyczne

Generyczna implementacja wygląda następująco:

```csharp
public class Stack<T>
{
    private T[] _items = new T[8];
    public int Count { get; private set; }

    public void Push(T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

Korzystanie z takiego stosu nie powoduje już operacji pakowania i jest bezpieczne:

```csharp
Stack<int> stack = new Stack<int>();

for (int i = 0; i < 10; i++)
{
    stack.Push(i);
}

int number = stack.Pop();
// string str = stack.Pop(); // Compilation error
```

## Metody generyczne

Metody również mogą wprowadzać parametry generyczne:

```csharp
public static void Swap<T>(ref T a, ref T b)
{
    T temp = a;
    a = b;
    b = temp;
}
```

## Ograniczenia typów generycznych

Normalnie o parametrze generycznym wiemy jedynie, że jest typu `object`, co znaczy, że można na nim wywołać metody dostępne na typie `object`. Nie można zrobić nic więcej.

Ograniczenia pozwalają na przekazanie dodatkowych informacji o typie generycznym. Na przykład, jeśli ograniczysz parametr generyczny do konkretnej klasy lub interfejsu, kompilator pozwoli Ci używać metod z tej klasy/interfejsu.

```csharp
public static T Max<T>(T value, params T[] values) where T : IComparable<T>
{
    var max = value;
    foreach (var t in values)
    {
        if (max.CompareTo(t) < 0)
        {
            max = t;
        }
    }
    return max;
}
```

Możliwe ograniczenia:

* `where T : `*<base class/interface>* - najczęstsze ograniczenie i najbardziej przydatne, pozwala wywołać metody z ograniczanych klas i interfejsów.
* `where T : `*<base class/interface>?* - pozwala wywołać metody z ograniczanych klas i interfejsów, dodatkowo może być `null`owalny
* `where T : new()` - typ musi zawierać konstruktor bezparametrowy, przydatne jeżeli musimy tworzyć nowe instancje
* `where T : class` - typ musi być referencyjny
* `where T : class?` - typ musi być referencyjny, może być `null`owalny
* `where T : struct` - typ musi być bezpośredni
* `where T : allows ref struct` - typ może być "ref strukturą"
* `where T : unmanaged` - typ musi być bezpośredni i rekursywnie składać się z innych typów bezpośrednich lub wskaźnikowych
* `where T : notnull` - nie może być `null`owalny

Na typ generyczny możemy nanosić kilka ograniczeń:

```csharp
class Base {}
class Test<T, U>
    where U : struct
    where T : Base, new()
{}
```

## Samoodnoszące się deklaracje generyczne

W deklaracji typu można używać deklarowanego typu jako parametru generycznego:

```csharp
public class Product : IEquatable<Product>
{
    public string EanCode { get; }
    
    public Product(string eanCode) => EanCode = eanCode;
    
    public bool Equals(Product? other) => EanCode == other?.EanCode;
}
```

To ma sens, komunikujemy w ten sposób, że `Product` jest porównywalny z innymi przedstawicielami swojego typu pod względem równości.

W deklaracji możemy także używać parametru generycznego do jego ograniczenia.

```csharp
public class Finder<T> : where T : IEquatable<T>
{
    public T? Find(Collection<T> collection, T item)
    {
        foreach(var t in collection)
        {
            if (t.Equals(item)) return t;
        }

        return default(T);
    }
}
```

To też ma sens, chcemy szukać obiektów, które są porównywalne ze sobą równościowo, inaczej nie wiedzielibyśmy jak szukać.

Poprawne jest też: `class Foo<Bar> : where Bar : Foo<Bar>`.
