using FluentAssertions;
using tasks;

namespace tests;

public class PrimeFinderTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-10)]
    public void SieveOfEratosthenes_ShouldReturnEmpty_WhenUpperBoundIsLessThanTwo(int upperBound)
    {
        // Act
        var result = PrimeFinder.SieveOfEratosthenes(upperBound).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    public static IEnumerable<object[]> KnownPrimeSets()
    {
        yield return new object[] { 2, new[] { 2 } };
        yield return new object[] { 3, new[] { 2, 3 } };
        yield return new object[] { 10, new[] { 2, 3, 5, 7 } };
        yield return new object[] { 11, new[] { 2, 3, 5, 7, 11 } };
        yield return new object[] { 30, new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 } };
    }

    [Theory]
    [MemberData(nameof(KnownPrimeSets))]
    public void SieveOfEratosthenes_ShouldReturnCorrectPrimes_ForGivenBounds(int upperBound, IEnumerable<int> expectedPrimes)
    {
        // Act
        var result = PrimeFinder.SieveOfEratosthenes(upperBound);

        // Assert
        result.Should().Equal(expectedPrimes);
    }

    [Fact]
    public void SieveOfEratosthenes_ShouldReturnOnlyTwo_WhenUpperBoundIsTwo()
    {
        // Act
        var result = PrimeFinder.SieveOfEratosthenes(2);

        // Assert
        result.Should().Equal([2]);
    }
}
