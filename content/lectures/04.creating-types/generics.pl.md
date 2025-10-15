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
static T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;
}
```
