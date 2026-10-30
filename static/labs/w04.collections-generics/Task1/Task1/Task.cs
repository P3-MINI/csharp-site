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
// TODO 1 
// =============================================================

// public class...

// =============================================================
// TODO 2 
// =============================================================

// public class...

// =============================================================
// TODO 3 
// =============================================================

public class Shelf<T> /* add missing interfaces and constraints here */ {
    public int Space { get; init; } = 5;
    private int _current = 0;
    private T?[] _items;

    public Shelf(int space) {
        _items = new T?[space];
        Space = space;
    }
}