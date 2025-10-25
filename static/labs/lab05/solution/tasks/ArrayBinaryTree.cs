using System.Collections;

namespace tasks;

public interface IBinaryTree<TKey, TValue> : IEnumerable<TValue>
{
    int Count { get; }

    void SetRoot(TValue value);

    TValue Get(int index);

    TKey GetLeftKey(TKey parentKey);

    TKey GetRightKey(TKey parentKey);

    void SetLeft(TKey parentKey, TValue value);

    void SetRight(TKey parentKey, TValue value);

    bool Exists(TKey key);

    void Clear();
}

public class ArrayBinaryTree<T> : IBinaryTree<int, T>
{
    private T[] _nodes;
    private bool[] _present;
    public int Count { get; private set; }

    public ArrayBinaryTree(int initialCapacity = 8)
    {
        if (initialCapacity < 1)
        {
            initialCapacity = 8;
        }

        _nodes = new T[initialCapacity];
        _present = new bool[initialCapacity];
        
        Count = 0;
    }

    private void EnsureCapacityForKeys(int key)
    {
        if (key < _nodes.Length)
        {
            return;
        }

        var newSize = _nodes.Length;
        
        while (newSize <= key)
        {
            newSize *= 2;
        }

        Array.Resize(ref _nodes, newSize);
        Array.Resize(ref _present, newSize);
    }

    public void SetRoot(T value)
    {
        SetAt(0, value);
    }

    public T Get(int key)
    {
        if (key < 0 || key >= _nodes.Length || !_present[key])
        {
            throw new IndexOutOfRangeException("Node not found.");
        }
        return _nodes[key];
    }

    public void SetLeft(int parentKey, T value)
    {
        if (!Exists(parentKey))
        {
            throw new InvalidOperationException($"Parent with key #{parentKey} not found.");
        }

        var leftIndex = GetLeftKey(parentKey);

        SetAt(leftIndex, value);
    }

    public void SetRight(int parentKey, T value)
    {
        if (!Exists(parentKey))
        {
            throw new InvalidOperationException($"Parent with key #{parentKey} not found.");
        }

        var rightKey = GetRightKey(parentKey);
        
        SetAt(rightKey, value);
    }

    private void SetAt(int key, T value)
    {
        EnsureCapacityForKeys(key);
        if (!_present[key])
        {
            _present[key] = true;
            Count++;
        }
        _nodes[key] = value;
    }

    public int GetLeftKey(int parentKey)
    {
        return 2 * parentKey + 1;
    }

    public int GetRightKey(int parentKey)
    {
        return 2 * parentKey + 2;
    }

    public bool Exists(int key)
    {
        return key >= 0 && key < _present.Length && _present[key];
    }

    public void Clear()
    {
        Array.Clear(_nodes, 0, _nodes.Length);
        Array.Clear(_present, 0, _present.Length);
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return InOrder(0).GetEnumerator();
    }

    private IEnumerable<T> InOrder(int key)
    {
        if (key >= _nodes.Length || !_present[key])
            yield break;

        var left = GetLeftKey(key);
        var right = GetRightKey(key);

        foreach (var v in InOrder(left))
        {
            yield return v;
        }

        yield return _nodes[key];

        foreach (var v in InOrder(right))
        {
            yield return v;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
