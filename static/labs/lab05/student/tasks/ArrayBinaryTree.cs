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

/// <summary>
/// Here you should implement a binary tree using an array-based representation for integers.
/// </summary>
public class ArrayBinaryTree<T> : IBinaryTree<int, T>
{
    public ArrayBinaryTree(int initialCapacity = 8)
    {

    }

    public int Count
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Exists(int key)
    {
        throw new NotImplementedException();
    }

    public T Get(int index)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int GetLeftKey(int parentKey)
    {
        throw new NotImplementedException();
    }

    public int GetRightKey(int parentKey)
    {
        throw new NotImplementedException();
    }

    public void SetLeft(int parentKey, T value)
    {
        throw new NotImplementedException();
    }

    public void SetRight(int parentKey, T value)
    {
        throw new NotImplementedException();
    }

    public void SetRoot(T value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
