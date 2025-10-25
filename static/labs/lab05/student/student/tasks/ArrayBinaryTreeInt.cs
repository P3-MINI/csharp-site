using System.Collections;
using System.Xml.Linq;

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

/// <summary>
/// Here you should implement a binary tree using an array-based representation for integers.
/// </summary>
public class ArrayBinaryTreeInt : IBinaryTree<int>
{
    public ArrayBinaryTreeInt(int initialCapacity = 8)
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

    public bool Exists(int index)
    {
        throw new NotImplementedException();
    }

    public int Get(int index)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<int> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void SetLeft(int parentIndex, int value)
    {
        throw new NotImplementedException();
    }

    public void SetRight(int parentIndex, int value)
    {
        throw new NotImplementedException();
    }

    public void SetRoot(int value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
