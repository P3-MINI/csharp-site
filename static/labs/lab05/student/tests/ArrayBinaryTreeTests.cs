using FluentAssertions;
using tasks;

namespace tests;

public class ArrayBinaryTreeTests
{
    [Fact]
    public void Constructor_Default_InitializesEmptyTree()
    {
        // Arrange & Act
        var tree = new ArrayBinaryTree<int>();

        // Assert
        tree.Count.Should().Be(0);
        tree.Exists(0).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithInvalidCapacity_ShouldNotThrowAndInitializeEmptyTree(int invalidCapacity)
    {
        // Arrange & Act
        // The class should internally set a default capacity
        var tree = new ArrayBinaryTree<int>(invalidCapacity);

        // Assert
        tree.Count.Should().Be(0);
        tree.Exists(0).Should().BeFalse();
    }

    [Fact]
    public void SetRoot_OnEmptyTree_SetsRootAndIncrementsCount()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Act
        tree.SetRoot(10);

        // Assert
        tree.Count.Should().Be(1);
        tree.Get(0).Should().Be(10);
        tree.Exists(0).Should().BeTrue();
    }

    [Fact]
    public void SetRoot_OnExistingTree_UpdatesValueAndCountIsUnchanged()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10); // Initial value

        // Act
        tree.SetRoot(20); // New value

        // Assert
        tree.Count.Should().Be(1); // Count should not change
        tree.Get(0).Should().Be(20);
    }

    [Fact]
    public void SetLeftAndRight_ValidParent_AddsChildrenAndIncrementsCount()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10); // index 0

        // Act
        tree.SetLeft(0, 5);  // index 1
        tree.SetRight(0, 15); // index 2

        // Assert
        tree.Count.Should().Be(3);
        tree.Get(1).Should().Be(5);
        tree.Exists(1).Should().BeTrue();
        tree.Get(2).Should().Be(15);
        tree.Exists(2).Should().BeTrue();
    }

    [Fact]
    public void SetLeft_NonExistentParent_ThrowsInvalidOperationException()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Act
        // Attempting to add to parent 0, which does not exist
        var act = () => tree.SetLeft(0, 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Parent with key #0 not found.");
    }

    [Fact]
    public void SetRight_NonExistentParent_ThrowsInvalidOperationException()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Act
        // Attempting to add to parent 1 (which doesn't exist), even if root (0) exists
        tree.SetRoot(10);
        var act = () => tree.SetRight(1, 5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Parent with key #1 not found.");
    }

    [Fact]
    public void Set_UpdatingExistingChild_UpdatesValueAndCountIsUnchanged()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10);
        tree.SetLeft(0, 5); // Set the left child

        // Assert
        tree.Count.Should().Be(2);
        tree.Get(1).Should().Be(5);

        // Act
        tree.SetLeft(0, 6); // Update the left child

        // Assert
        tree.Count.Should().Be(2); // The count does not change
        tree.Get(1).Should().Be(6); // Value updated
    }

    [Fact]
    public void Get_IndexNegative_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Act
        var act = () => tree.Get(-1);

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Get_IndexOutOfBounds_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10); // Tree has capacity (e.g., 8), but only 1 element

        // Act
        var act = () => tree.Get(100); // 100 > capacity

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Get_IndexInBoundsButNotSet_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10); // Sets _present[0] = true

        // Act
        // Index 1 is within array bounds, but _present[1] = false
        var act = () => tree.Get(1);

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Exists_ShouldReturnCorrectStatus()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10); // index 0
        tree.SetLeft(0, 5);  // index 1

        // Assert
        tree.Exists(0).Should().BeTrue();  // Root
        tree.Exists(1).Should().BeTrue();  // Left child
        tree.Exists(2).Should().BeFalse(); // Right child (not set)
        tree.Exists(3).Should().BeFalse(); // Node "inside" but not set
        tree.Exists(100).Should().BeFalse(); // Out of bounds
        tree.Exists(-1).Should().BeFalse();  // Out of bounds
    }

    [Fact]
    public void Clear_ResetsTreeState()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();
        tree.SetRoot(10);
        tree.SetLeft(0, 5);
        tree.SetRight(0, 15);
        tree.Count.Should().Be(3);

        // Act
        tree.Clear();

        // Assert
        tree.Count.Should().Be(0);
        tree.Exists(0).Should().BeFalse();
        tree.Exists(1).Should().BeFalse();
        tree.Exists(2).Should().BeFalse();

        // Check if Get throws an exception
        var act = () => tree.Get(0);
        act.Should().Throw<IndexOutOfRangeException>();

        // Check the enumerator
        tree.ToList().Should().BeEmpty();
    }

    [Fact]
    public void Set_ShouldResize_WhenIndexExceedsInitialCapacity()
    {
        // Arrange
        // Set a small initial capacity
        var tree = new ArrayBinaryTree<int>(2); // Will hold indices [0] and [1]

        // Act
        tree.SetRoot(10); // index 0
        tree.SetLeft(0, 5); // index 1 (fits)

        // This operation requires index 2 (2*0 + 2), which will force a resize
        var act = () => tree.SetRight(0, 15);

        // Assert
        act.Should().NotThrow(); // Resize must succeed
        tree.Get(2).Should().Be(15);
        tree.Count.Should().Be(3);
        tree.Exists(2).Should().BeTrue();
    }

    [Fact]
    public void GetEnumerator_EmptyTree_ReturnsEmptyCollection()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Act
        var list = tree.ToList();

        // Assert
        list.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ComplexTree_ReturnsCorrectInOrderTraversal()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Building the tree:
        //       10 (0)
        //      /   \
        //     5(1)  15(2)
        //    / \
        //   3(3) 7(4)

        tree.SetRoot(10);
        tree.SetLeft(0, 5);
        tree.SetRight(0, 15);
        tree.SetLeft(1, 3);
        tree.SetRight(1, 7);

        // Expected In-Order traversal: Left, Root, Right
        var expectedOrder = new[] { 3, 5, 7, 10, 15 };

        // Act
        // Using .ToList() triggers the enumerator
        var inOrderList = tree.ToList();

        // Assert
        inOrderList.Should().Equal(expectedOrder);
        tree.Count.Should().Be(5);
    }

    [Fact]
    public void GetEnumerator_TreeWithGaps_ReturnsCorrectInOrderTraversal()
    {
        // Arrange
        var tree = new ArrayBinaryTree<int>();

        // Building a tree with "gaps" (missing root's left child):
        //       10 (0)
        //          \
        //           15(2)
        //          /
        //         12(5)

        tree.SetRoot(10);
        tree.SetRight(0, 15); // index 2, _present[1] is false
        tree.SetLeft(2, 12);  // index 5

        var expectedOrder = new[] { 10, 12, 15 };

        // Act
        var inOrderList = tree.ToList();

        // Assert
        inOrderList.Should().Equal(expectedOrder);
        tree.Count.Should().Be(3);
    }
}
