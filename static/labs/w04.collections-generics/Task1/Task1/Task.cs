using System.Numerics;

namespace Task1;

public interface IAdder<in T> {
    public bool Add(T val);
}

public interface ITaker<out T> {
    public T? Take();
}

public class Box<T> {
    public T Value { get; set; }
    public Box(T val) { Value = val; }
    public override string ToString() { return $"Box: {Value}"; }
}

// =============================================================
// TODO 1 - Implement generic NumBox class
// The class should derieve from Box class and work on any number type
// It should contain a constructor and additon operator:
// public static NumBox<T> operator+(NumBox<T> a, NumBox<T> b) that returns a sum of values (a.Value + b.Value)
// =============================================================

// public class...

// =============================================================
// TODO 2 - Implement StringBox class
// The class should derieve from Box class, that will work only on strings
// The class should contain a constructor and additon operator:
// public static StringBox operator +(StringBox a, StringBox b) that return concatanated string (a.Value + ";" + b.Value)
// =============================================================

// public class...

// =============================================================
// TODO 3 - Finish implementing Shelf class
// The class should implement IAdder<T> and ITaker<T> interfaces
// public bool Add(T val) - if shelf is full returns false, otherwise adds box at the end of _items and returns true
// public T? Take() - takes and returns last box from shelf if possible, otherwise returns null
// =============================================================

public class Shelf<T> /* add missing interfaces and constrains here */ {
    public int Space { get; init; } = 5;
    private int _current = 0;
    private T?[] _items;

    public Shelf(int space) {
        _items = new T?[space];
        Space = space;
    }
}