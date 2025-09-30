---
title: "Arrays"
weight: 50
---

# Arrays

Arrays are a reference type representing a fixed number of elements of the same type. Elements in an array are always stored in a **contiguous block of memory**. An array type is denoted by the element type followed by square brackets. As in normal programming languages, arrays are indexed from 0.

The `Length` property provides the size of the array. Once created, its size cannot be changed.

All array types inherit from `System.Array`. This class also contains several useful methods that help operate on arrays, e.g., `Sort`, `Reverse`, `BinarySearch`, `Clear`, `Fill`, `Copy`, `Resize` (creates a new array and copies elements).

If not explicitly provided, array elements are implicitly initialized to their default values (zero-like values), which for arrays of reference types means an array of `null` references.

All errors related to array indexing are checked at runtime. Going out of the array's bounds results in an `IndexOutOfRangeException` being thrown.

```csharp
int[] primes = new int[] {2, 3, 5, 7, 11};
char[] vowels = {'a', 'e', 'i', 'o', 'u'};
uint[] even = [0, 2, 4, 6, 8]; // C# 12 collection expression
float[] data = new float[10];
Array array = primes;
Console.WriteLine($"Primes array length: {primes.Length}");
for (int i = 0; i < primes.Length; i++)
{
    Console.WriteLine(primes[i]);
}
```

## Indices and Ranges

Indices allow you to index an array from the end using the `^` operator. `^1` refers to the last element, `^2` to the second to last, and so on. **Note:** `^0` refers to `array.Length` (which is one element past the end of the array).

Ranges, on the other hand, allow you to select a subarray using the `..` operator. On the left side of this operator, you can place the inclusive starting index, and on the right side, the exclusive ending index to select the subarray. By default, if they are not provided, they are replaced by `0` and `^0` respectively, meaning the start and the end.

```csharp
int[] primes = new int[] {2, 3, 5, 7};
int firstElem = primes[0], secondElem = primes[1];
int lastElem = primes[^1], secondToLastElem = primes[^2];
Index first = 0;
Index last = ^1;
firstElem = primes[first]; lastElem = primes[last];

int[] firstTwo = primes[..2]; //exclusive end
int[] withoutFirst = primes[1..]; // inclusive start
int[] withoutLast = primes[..^1];
int[] withoutFirstAndLast = primes[1..^1];
int[] all = primes[..];
Range lastTwoRange = ^2..;
int[] lastTwo = primes[lastTwoRange];
```

## Rectangular Arrays

Rectangular arrays are declared using a `,` to specify each dimension. Just like one-dimensional arrays, they can be initialized by explicitly providing a list of elements. The `GetLength(int)` method returns the length of the array along the `i`-th dimension (starting from 0).

```csharp
float[,] matrix = new float[,]
{
    {1.0f, 0.0f, 0.0f},
    {0.0f, 1.0f, 0.0f},
    {0.0f, 0.0f, 1.0f}
};

float[,] matrix3x4 = new float[3, 4];
for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        Console.WriteLine($"m[{i}, {j}] = {matrix[i, j]}");
    }
}
```
