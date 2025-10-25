using System.Collections;

namespace tasks;

public interface IBinaryTree<T> : IEnumerable<T>
{
    int Count { get; }

    void SetRoot(T value);

    T Get(int index);

    void SetLeft(int parentIndex, T value);

    void SetRight(int parentIndex, T value);

    bool Exists(int index);

    void Clear();
}

public class ArrayBinaryTreeInt : IBinaryTree<int>
{
    private int[] _nodes;
    private bool[] _present;
    public int Count { get; private set; }

    public ArrayBinaryTreeInt(int initialCapacity = 8)
    {
        if (initialCapacity < 1)
        {
            initialCapacity = 8;
        }

        _nodes = new int[initialCapacity];
        _present = new bool[initialCapacity];
        
        Count = 0;
    }

    private void EnsureCapacityForIndex(int index)
    {
        if (index < _nodes.Length)
        {
            return;
        }

        var newSize = _nodes.Length;
        
        while (newSize <= index)
        {
            newSize *= 2;
        }

        Array.Resize(ref _nodes, newSize);
        Array.Resize(ref _present, newSize);
    }

    public void SetRoot(int value)
    {
        SetAt(0, value);
    }

    public int Get(int index)
    {
        if (index < 0 || index >= _nodes.Length || !_present[index])
        {
            throw new IndexOutOfRangeException("Node not found.");
        }
        return _nodes[index];
    }

    public void SetLeft(int parentIndex, int value)
    {
        if (!Exists(parentIndex))
        {
            throw new InvalidOperationException($"Parent with index #{parentIndex} not found.");
        }

        var leftIndex = 2 * parentIndex + 1;

        SetAt(leftIndex, value);
    }

    public void SetRight(int parentIndex, int value)
    {
        if (!Exists(parentIndex))
        {
            throw new InvalidOperationException($"Parent with index #{parentIndex} not found.");
        }

        var rightIndex = 2 * parentIndex + 2;
        
        SetAt(rightIndex, value);
    }

    private void SetAt(int index, int value)
    {
        EnsureCapacityForIndex(index);
        if (!_present[index])
        {
            _present[index] = true;
            Count++;
        }
        _nodes[index] = value;
    }

    public bool Exists(int index)
    {
        return index >= 0 && index < _present.Length && _present[index];
    }

    public void Clear()
    {
        Array.Clear(_nodes, 0, _nodes.Length);
        Array.Clear(_present, 0, _present.Length);
        Count = 0;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return InOrder(0).GetEnumerator();
    }

    private IEnumerable<int> InOrder(int index)
    {
        if (index >= _nodes.Length || !_present[index])
            yield break;

        var left = 2 * index + 1;
        var right = 2 * index + 2;

        foreach (var v in InOrder(left))
        {
            yield return v;
        }

        yield return _nodes[index];

        foreach (var v in InOrder(right))
        {
            yield return v;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
