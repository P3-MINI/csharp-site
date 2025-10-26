using System.Collections;

namespace tasks;

/// <summary>
/// Interface for an array-based binary tree.
/// </summary>
public interface IArrayBinaryTree<T> : IEnumerable<T>
{
    /// <summary>
    /// Gets the number of nodes in the tree.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the index of the root.
    /// </summary>
    int RootIndex { get; }

    /// <summary>
    /// Sets the value of the root node.
    /// </summary>
    void SetRoot(T value);

    /// <summary>
    /// Returns the indices of the left and right children for a given parent.
    /// </summary>
    (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex);

    /// <summary>
    /// Sets the value of the left child of a given parent.
    /// </summary>
    void SetLeftChild(int parentIndex, T value);

    /// <summary>
    /// Sets the value of the right child of a given parent.
    /// </summary>
    void SetRightChild(int parentIndex, T value);

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    T this[int index] { get; }

    /// <summary>
    /// Checks if a node at the specified index exists.
    /// </summary>
    bool Exists(int index);

    /// <summary>
    /// Clears all nodes from the tree.
    /// </summary>
    void Clear();
}

/// <summary>
/// An implementation of a binary tree based on an array.
/// Indices are calculated as:
/// - Left Child: 2 * i + 1
/// - Right Child: 2 * i + 2
/// - Parent: (i - 1) / 2
/// </summary>
public class ArrayBinaryTree<T> : IArrayBinaryTree<T>
{
    public T this[int index]
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public int Count
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public int RootIndex
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

    public (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void SetLeftChild(int parentIndex, T value)
    {
        throw new NotImplementedException();
    }

    public void SetRightChild(int parentIndex, T value)
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
