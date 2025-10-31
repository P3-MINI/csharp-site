//#define TASK04
using FluentAssertions;
using tasks;

namespace tests;

#if TASK04
public class EnumerableExtensionsTests
{
    [Fact]
    public void Fold_ShouldSumNumbers_WhenGivenSumFunction()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var seed = 0;
        Func<int, int, int> accumulator = (acc, x) => acc + x;
        Func<int, int> resultSelector = acc => acc;

        // Act
        var result = numbers.Fold(seed, accumulator, resultSelector);

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public void Fold_ShouldReturnSeedProjection_WhenCollectionIsEmpty()
    {
        // Arrange
        var numbers = Enumerable.Empty<int>();
        var seed = 10;
        Func<int, int, int> accumulator = (acc, x) => acc + x;
        Func<int, string> resultSelector = acc => $"Result: {acc}";

        // Act
        var result = numbers.Fold(seed, accumulator, resultSelector);

        // Assert
        result.Should().Be("Result: 10");
    }

    [Fact]
    public void Fold_ShouldHandleDifferentTypesCorrectly()
    {
        // Arrange
        var words = new[] { "hello", "world", "test" };
        // TSource = string, TAccumulate = int (sum of lengths), TResult = string (final output)
        var seed = 0;
        Func<int, string, int> accumulator = (acc, s) => acc + s.Length;
        Func<int, string> resultSelector = acc => $"Total length: {acc}";

        // Act
        var result = words.Fold(seed, accumulator, resultSelector);

        // Assert
        // "hello" (5) + "world" (5) + "test" (4) = 14
        result.Should().Be("Total length: 14");
    }

    [Fact]
    public void Batch_ShouldCreateFullBatches_WhenItemCountIsMultipleOfSize()
    {
        // Arrange
        var items = Enumerable.Range(1, 10); // [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
        var size = 5;

        // Act
        var batches = items.Batch(size).ToList();

        // Assert
        batches.Should().HaveCount(2);
        batches[0].Should().Equal(1, 2, 3, 4, 5);
        batches[1].Should().Equal(6, 7, 8, 9, 10);
    }

    [Fact]
    public void Batch_ShouldCreatePartialLastBatch_WhenItemCountIsNotMultipleOfSize()
    {
        // Arrange
        var items = Enumerable.Range(1, 8); // [1, 2, 3, 4, 5, 6, 7, 8]
        var size = 3;

        // Act
        var batches = items.Batch(size).ToList();

        // Assert
        batches.Should().HaveCount(3);
        batches[0].Should().Equal(1, 2, 3);
        batches[1].Should().Equal(4, 5, 6);
        batches[2].Should().Equal(7, 8); // Last, partial batch
    }

    [Fact]
    public void Batch_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
    {
        // Arrange
        var items = Enumerable.Empty<int>();

        // Act
        var batches = items.Batch(5);

        // Assert
        batches.Should().BeEmpty();
    }

    [Fact]
    public void Batch_ShouldReturnOneBatch_WhenSizeIsLargerThanCollection()
    {
        // Arrange
        var items = Enumerable.Range(1, 3); // [1, 2, 3]

        // Act
        var batches = items.Batch(100).ToList();

        // Assert
        batches.Should().HaveCount(1);
        batches[0].Should().Equal(1, 2, 3);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Batch_ShouldThrowException_WhenSizeIsLessThanOne(int size)
    {
        // Arrange
        var items = Enumerable.Range(1, 5);

        // Act
        var act = () => items.Batch(size).ToList();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Batch size must be at least 1. (Parameter 'size')")
            .And.ParamName.Should().Be("size");
    }

    [Fact]
    public void SlidingWindow_ShouldReturnOverlappingWindowsCorrectly()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };
        var size = 3;

        // Act
        var windows = items.SlidingWindow(size).ToList();

        // Assert
        windows.Should().HaveCount(3);
        windows[0].Should().Equal(1, 2, 3);
        windows[1].Should().Equal(2, 3, 4);
        windows[2].Should().Equal(3, 4, 5);
    }

    [Fact]
    public void SlidingWindow_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
    {
        // Arrange
        var items = Enumerable.Empty<int>();

        // Act
        var windows = items.SlidingWindow(3);

        // Assert
        windows.Should().BeEmpty();
    }

    [Fact]
    public void SlidingWindow_ShouldReturnEmptyCollection_WhenWindowSizeIsLargerThanCollection()
    {
        // Arrange
        var items = new[] { 1, 2 };

        // Act
        var windows = items.SlidingWindow(3);

        // Assert
        windows.Should().BeEmpty();
    }

    [Fact]
    public void SlidingWindow_ShouldReturnOneWindow_WhenSizeEqualsCollectionSize()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var windows = items.SlidingWindow(3).ToList();

        // Assert
        windows.Should().HaveCount(1);
        windows[0].Should().Equal(1, 2, 3);
    }

    [Fact]
    public void SlidingWindow_ShouldReturnWindowsOfSizeOne_WhenSizeIsOne()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var windows = items.SlidingWindow(1).ToList();

        // Assert
        windows.Should().HaveCount(3);
        windows[0].Should().Equal(1);
        windows[1].Should().Equal(2);
        windows[2].Should().Equal(3);
    }

    [Fact]
    public void SlidingWindow_ShouldThrowException_WhenSizeIsZero()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var act = () => items.SlidingWindow(0).ToList();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Window size must be at least 1. (Parameter 'size')")
            .And.ParamName.Should().Be("size");
    }

    [Fact]
    public void SlidingWindow_ShouldThrowException_WhenSizeIsNegative()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var act = () => items.SlidingWindow(-1).ToList();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("size");
    }

    [Fact]
    public void FindSlidingWindowsWithRisingSum_ShouldFindWindowsWithIncreasingSum()
    {
        // Arrange
        var sequence = new[] { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        var result = sequence.FindSlidingWindowsWithRisingSum().ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Equal(2, 3, 4, 5, 6);
        result[1].Should().Equal(3, 4, 5, 6, 7);
    }

    [Fact]
    public void FindSlidingWindowsWithRisingSum_ShouldReturnEmpty_WhenSumsAreNotRising()
    {
        // Arrange
        var sequence = new[] { 5, 4, 3, 2, 1, 0 };

        // Act
        var result = sequence.FindSlidingWindowsWithRisingSum();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSlidingWindowsWithRisingSum_ShouldReturnEmpty_WhenSequenceIsTooShort()
    {
        // Arrange
        var sequence = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = sequence.FindSlidingWindowsWithRisingSum();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSlidingWindowsWithRisingSum_ShouldReturnEmpty_WhenSequenceIsEmpty()
    {
        // Arrange
        var sequence = Enumerable.Empty<int>();

        // Act
        var result = sequence.FindSlidingWindowsWithRisingSum();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSlidingWindowsWithDuplicates_ShouldFindWindowsContainingDuplicates()
    {
        // Arrange
        // W1: [1,2,3,1] -> distinct=3. 3 < 4 -> True
        // W2: [2,3,1,4] -> distinct=4. 4 < 4 -> False
        // W3: [3,1,4,5] -> distinct=4. 4 < 4 -> False
        // W4: [1,4,5,5] -> distinct=3. 3 < 4 -> True
        var sequence = new[] { 1, 2, 3, 1, 4, 5, 5 };

        // Act
        var result = sequence.FindSlidingWindowsWithDuplicates().ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Equal(1, 2, 3, 1);
        result[1].Should().Equal(1, 4, 5, 5);
    }

    [Fact]
    public void FindSlidingWindowsWithDuplicates_ShouldReturnEmpty_WhenNoDuplicatesExist()
    {
        // Arrange
        var sequence = new[] { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        var result = sequence.FindSlidingWindowsWithDuplicates();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSlidingWindowsWithDuplicates_ShouldReturnEmpty_WhenSequenceIsTooShort()
    {
        // Arrange
        var sequence = new[] { 1, 1, 2 }; // Too short for a 4-element window

        // Act
        var result = sequence.FindSlidingWindowsWithDuplicates();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindSlidingWindowsWithDuplicates_ShouldFindWindowWithAllDuplicates()
    {
        // Arrange
        var sequence = new[] { 1, 1, 1, 1 };

        // Act
        var result = sequence.FindSlidingWindowsWithDuplicates().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Equal(1, 1, 1, 1);
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldFindSingleMostCommonTrigram()
    {
        // Arrange
        // "ababa" -> sanitized: ['a','b','a','b','a']
        // Windows: ["aba"], ["bab"], ["aba"]
        // Groups: ("aba", 2), ("bab", 1)
        // MaxCount: 2
        var text = "ababa";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEquivalentTo("aba");
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldFindMultipleMostCommonTrigrams()
    {
        // Arrange
        // "abcabcabc" -> sanitized: ['a','b','c','a','b','c','a','b','c']
        // Windows: ["abc"], ["bca"], ["cab"], ["abc"], ["bca"], ["cab"], ["abc"]
        // Groups: ("abc", 3), ("bca", 2), ("cab", 2)
        // MaxCount: 3
        var text = "abcabcabc";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEquivalentTo("abc");

        // Arrange 2
        // "abcabcab" -> sanitized: ['a','b','c','a','b','c','a','b']
        // Windows: ["abc"], ["bca"], ["cab"], ["abc"], ["bca"], ["cab"]
        // Groups: ("abc", 2), ("bca", 2), ("cab", 2)
        // MaxCount: 2
        var text2 = "abcabcab";

        // Act 2
        var result2 = text2.FindMostCommonTrigrams();

        // Assert 2
        // Using BeEquivalentTo because order is not guaranteed
        result2.Should().BeEquivalentTo("abc", "bca", "cab");
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldHandleCasingAndNonLetters()
    {
        // Arrange
        // "Hello, Hello... world!" -> sanitized: ['h','e','l','l','o','h','e','l','l','o','w','o','r','l','d']
        // Windows: ["hel"], ["ell"], ["llo"], ["loh"], ["ohe"], ["hel"], ["ell"], ["llo"], ["low"], ["owo"], ["wor"], ["orl"], ["rld"]
        // Groups: ("hel", 2), ("ell", 2), ("llo", 2), ... rest are 1
        // MaxCount: 2
        var text = "Hello, Hello... world!";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEquivalentTo("hel", "ell", "llo");
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldReturnEmpty_WhenTextIsEmpty()
    {
        // Arrange
        var text = "";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldReturnEmpty_WhenTextIsWhitespace()
    {
        // Arrange
        var text = "   \t\n";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldReturnEmpty_WhenTextIsTooShort()
    {
        // Arrange
        var text = "ab";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindMostCommonTrigrams_ShouldReturnOneTrigram_WhenTextIsLength3()
    {
        // Arrange
        var text = "abc";

        // Act
        var result = text.FindMostCommonTrigrams();

        // Assert
        result.Should().BeEquivalentTo("abc");
    }

    [Fact]
    public void LongestSequence_ShouldThrowException_WhenSequenceIsEmpty()
    {
        // Arrange
        var sequence = Enumerable.Empty<int>();

        // Act
        var act = () => sequence.LongestSequence();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LongestSequence_ShouldFindSequence_WhenOnlyOneElement()
    {
        // Arrange
        var sequence = new[] { 5 };

        // Act
        var result = sequence.LongestSequence();

        // Assert
        result.Should().Be((0, 0, 5));
    }

    [Fact]
    public void LongestSequence_ShouldFindLongestAtStart()
    {
        // Arrange
        var sequence = new[] { 1, 1, 1, 2, 3, 3 };

        // Act
        var result = sequence.LongestSequence();

        // Assert
        result.Should().Be((0, 2, 1));
    }

    [Fact]
    public void LongestSequence_ShouldFindLongestInMiddle()
    {
        // Arrange
        var sequence = new[] { 1, 2, 2, 2, 2, 1, 1 };

        // Act
        var result = sequence.LongestSequence();

        // Assert
        result.Should().Be((1, 4, 2));
    }

    [Fact]
    public void LongestSequence_ShouldFindLongestAtEnd()
    {
        // Arrange
        var sequence = new[] { 1, 2, 2, 3, 3, 3, 3 };

        // Act
        var result = sequence.LongestSequence();

        // Assert
        result.Should().Be((3, 6, 3));
    }

    [Fact]
    public void LongestSequence_ShouldReturnFirstSequence_WhenTwoHaveSameLength()
    {
        // Arrange
        // The logic (currentLength > length) favors the first sequence found
        var sequence = new[] { 1, 1, 1, 5, 2, 2, 2, 8 };

        // Act
        var result = sequence.LongestSequence();

        // Assert
        result.Should().Be((0, 2, 1));
    }

    [Fact]
    public void ComputeStatistics_ShouldThrowException_WhenSourceIsNull()
    {
        // Arrange
        IEnumerable<int> source = null!;

        // Act
        var act = () => source.ComputeStatistics();

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("source");
    }

    [Fact]
    public void ComputeStatistics_ShouldThrowException_WhenSourceIsEmpty()
    {
        // Arrange
        var source = Enumerable.Empty<int>();

        // Act
        var act = () => source.ComputeStatistics();

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("source");
    }

    [Fact]
    public void ComputeStatistics_ShouldCalculateCorrectly_ForSingleElement()
    {
        // Arrange
        var source = new[] { 10 };

        // Act
        var result = source.ComputeStatistics();

        // Assert
        result.min.Should().Be(10);
        result.max.Should().Be(10);
        result.average.Should().Be(10.0);
        result.standardDeviation.Should().Be(0.0);
    }

    [Fact]
    public void ComputeStatistics_ShouldCalculateCorrectly_ForMultipleElements()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        // Min: 1, Max: 5, Sum: 15, Count: 5, Avg: 3.0
        // SumOfSq: 1+4+9+16+25 = 55
        // Variance: (55 / 5) - (3.0 * 3.0) = 11 - 9 = 2.0
        // StdDev: Sqrt(2.0) = 1.41421356...

        // Act
        var result = source.ComputeStatistics();

        // Assert
        result.min.Should().Be(1);
        result.max.Should().Be(5);
        result.average.Should().BeApproximately(3.0, 0.0001);
        result.standardDeviation.Should().BeApproximately(1.41421, 0.0001);
    }

    [Fact]
    public void ComputeStatistics_ShouldCalculateCorrectly_ForAllSameElements()
    {
        // Arrange
        var source = new[] { 7, 7, 7, 7 };

        // Act
        var result = source.ComputeStatistics();

        // Assert
        result.min.Should().Be(7);
        result.max.Should().Be(7);
        result.average.Should().Be(7.0);
        result.standardDeviation.Should().Be(0.0);
    }

    [Fact]
    public void ComputeStatistics_ShouldCalculateCorrectly_WithNegativeNumbers()
    {
        // Arrange
        var source = new[] { -1, -2, -3 };
        // Min: -3, Max: -1, Sum: -6, Count: 3, Avg: -2.0
        // SumOfSq: 1+4+9 = 14
        // Variance: (14 / 3) - (-2.0 * -2.0) = 4.666... - 4 = 0.666... (2/3)
        // StdDev: Sqrt(2/3) = 0.816496...

        // Act
        var result = source.ComputeStatistics();

        // Assert
        result.min.Should().Be(-3);
        result.max.Should().Be(-1);
        result.average.Should().BeApproximately(-2.0, 0.0001);
        result.standardDeviation.Should().BeApproximately(0.81649, 0.0001);
    }
}
#endif // TASK04