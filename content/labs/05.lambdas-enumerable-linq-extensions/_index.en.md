---
title: "Lambdas, Extension Methods, LINQ"
weight: 50
---

# Quick lecture revision
## Delegates
A delegate is a safer and statically typed equivalent of C++ function pointer. It can be stored in a variable, passed as an argument, returned from a function, and invoked like a method. For example:

```csharp
public delegate int BinaryOp(int a, int b);

int Add(int x, int y) => x + y;
int Mul(int x, int y) => x * y;

BinaryOp op = Add;
Console.WriteLine(op(2, 3));   // 5
op = Mul;
Console.WriteLine(op(2, 3));   // 6
```
### Built-in delegate types
In modern C# you rarely need to declare a custom delegate type, because the built-in sytem generic delegate types cover almost everything:
|Signature|Example|
|---|---|
| `void Action<in T1..T16>` | `Action<string> echo = (s => Console.WriteLine(s));` |
| `TResult Func<in T1..T16, out TResult>` | `Func<int, int> sqr = (x => x * x);` |
| `bool Predicate<in T>` | `Predicate<int> odd = (x => x % 2 != 0);` | 
| `int Comparison<in T>` | `Comparison<int> cmp = ((a, b) => a.CompareTo(b));` |

### Multicast delegates
Delegates can be combined with `+` and removed with `-`. When you invoke a multicast delegate, all wrapped methods run in order.
```csharp
Action log = () => Console.WriteLine("one");
log += () => Console.WriteLine("two");
log += () => Console.WriteLine("three");
log();
// one
// two
// three
```
Only the last method's return value will be returned. Because of that you shouldn't be using multicast for anything that returns a value, but a list of delegates instead.

## Lambda expressions
A lambda is a unnamed function, assignable to a compatible delegate type. Syntax:

    var name = <return type> (parameters) => expression; OR { statement block }

>[!IMPORTANT]
> A lambda captures variables from the enclosing scope. The captured variable is **shared**, **not copied** — the lambda holds a reference to the variable, not its value at capture time.

```csharp
int counter = 0;
Action increment = () => counter++;
increment();
increment();
Console.WriteLine(counter); // 2
```
Internally, the compiler generates a class that stores the captured variable as a field.

> [!WARNING]
> Remeber that `foreach` loops create readonly copies for value types, contrary to other loops:
```csharp
var actions1 = new List<Action>();
foreach (var i in new[] { 1, 2, 3 }) actions1.Add(() => Console.WriteLine(i));
foreach (var a in actions1) a();   // 1, 2, 3

var actions2 = new List<Action>();
for (int i = 0; i < 3; i++) actions2.Add(() => Console.WriteLine(i));
foreach (var a in actions2) a();   // 3, 3, 3
```

>[!IMPORTANT]
> If you want to guarantee no capture (for micro-optimization, or to prevent accidents), since C# 9 you can mark lambdas as static:
```csharp
int captured = 5;
Func<int, int> bad = static x => x + captured;   // compile error 
```

## Extension methods
Extension methods allow you to add methods to existing types without modifying their code. Even though extension methods are `static`, you can call them as if it they were instance methods on a type you don't own.

```csharp
public static class StringExtensions {
    public static string Reverse(this string s) {
        var chars = s.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}

string s = "hello";
string r1 = s.Reverse();                 // called as instance method
string r2 = StringExtensions.Reverse(s); // equivalent, explicit call as static method
```
An extension method:
1. Must be declared in a `static` **non-generic** **non-nested** class.
2. Must be `static`.
3. Must have `this` modifier for the first parameter.

> [!WARNING]
> If the type has an instance method with the same name and compatible signature, the compiler will choose the instance method.

> [!IMPORTANT]
> You can extend the interfaces as well as classes. `Where`, `Select` and other LINQ methods that you will learn in this tutorial are all extension methods on IEnumerable<T>:
```csharp
public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate) { ... }
public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) { ... }
```

### Extension members
Since C# 14 you can add extension properties, static extension members, and extension operators. For example:

```csharp
// extension methods before
public static class EnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();
}
if (list.IsEmpty()) { ... }
```
```csharp
// extension property now
public static class EnumerableExtensions {
    extension<T>(IEnumerable<T> source) {
        public bool IsEmpty => !source.Any();
    }
}
if (list.IsEmpty) { ... }
```
## Read more
- [Delegates (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- [Lambda expressions and anonymous functions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Extension members (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
> [!IMPORTANT]
> ## Delegates, lambdas and extension methods example tasks
{{% details "TASK 1" false %}}
The task consists of 5 stages. The stages are defined in the files:
1. BinaryTree.cs
   
   Add a `DepthFirstSearch` method that takes three functions of type `Action` with a parameter of type `Node` (`preVisit`, `inVisit`, `postVisit`). The method should recursively traverse the tree, calling the appropriate actions at the appropriate moments:
    - `preVisit` should be called before the recursive descents,
    - `inVisit` should be called between the recursive descents,
    - `postVisit` should be called after the recursive descents.
  
    Add an implementation of the `PrintInOrder` method, using the previously implemented method:
    - this method should print the values of nodes in *In-Order* with appropriate indentations,
    - the length of the indentation is to be proportional to the level of the node in the tree,
    - in the `preVisit` and postVisit methods, the recursion level can be tracked,
    - in the `inVisit` method, the value of the node should be printed with the appropriate indentation.

2. DictionaryExtensions.cs
   
    Implement an extension method for the `IDictionary<TKey, TValue>` interface named `AddOrUpdate`.
    This method should add new elements to the dictionary if the value under the specified key does not exist
    or update the existing value based on the key and the previous value. The method should take the following parameters:
    - `TKey key` - the key to be added or updated,
    - `TValue addValue` - the value to be added if the key does not exist in the dictionary,
    - `Func updateValueFactory` - a function that, based on the previous value and the key, defines how to update the previous value. It should take the old key `TKey` and the previous value `TValue` and return the new value `TValue`.
  
    The method should return the newly added value.
    Then implement a `CountWords` method that counts the number of occurrences of words in a sequence of strings using the previously implemented method. This method is to return `IDictionary<string, int>`, where the keys are words and the values are the number of occurrences in the sequence.

3. EnumerableExtensions.cs
   
    In this stage, it is forbidden to use the `Aggregate` function from **LINQ**. Write an extension method `Reduce<TSource, TAccumulate>` for the `IEnumerable<TSource>` interface, which will perform a reduction operation on a sequence to a single value.
    The method should take the following parameters:
    - `TAccumulate seed`: the initial value of the accumulator,
    - `Func accumulator`: a function that implements the accumulation logic, taking two values of type `TAccumulate` and `TSource`, returning a value of type `TAccumulate`. The method should iterate through the elements of the source sequence and update the resulting accumulator using the provided function.
  
    Using the `Reduce` method, implement three methods extending the `IEnumerable<T>` interface,
    with the constraint that type `T` is limited to the `INumber<T>` interface:
    - `Sum` - returning the sum T of the sequence
    - `Min` - returning the minimum element T of the sequence
    - `Max` - returning the maximum element T of the sequence

    Hints: `T.CreateTruncating(double.NegativeInfinity)`, `T.CreateTruncating(double.PositiveInfinity)`, `T.Zero`, `T.Min`, `T.Max`.

4. FactoryMethods.cs
   
    Write a `static` function `RandomValueFunctionFactory` that returns an anonymous function
    returning a (pseudo)random double value from the range given by the parameters:
    `double min` and `double max`.

    Write a `static` function `RandomPointFunctionFactory` that returns an anonymous function
    returning a (pseudo)random vector of two double values (as a tuple) from the range given by the parameters: `double xMin`, `double xMax`, `double yMin`, `double yMax`. Use the previously defined method.

    Write a static function `MonteCarloPiEstimation` returning an approximation of the number π (of type `double`).
    Use the previously defined method. The method should take the number of samples (points) used for the approximation of type `int` (with a default value of 1024).
    - Monte Carlo method: https://en.wikipedia.org/wiki/Monte_Carlo_method
    - Hint: Generate random points in a rectangle containing (a sector of) the unit circle and check whether they are inside (the sector of) the unit circle. Calculate the ratio of points inside (the sector of) the circle to the total number of points to estimate the value of π.
5. Numerics.cs
   
    Declare a delegate named `Function` that takes a parameter x of type `double` and returns a value of type `double`. Implement a `static` method `NewtonRootFinding` that takes a function f of type `Function`, a derivative function df of type `Function` (with a default value of `null`), a starting point `x0` (with a default value of 0), and a precision `eps` (with a default value of 1e-6). The method should return the computed root of the equation using Newton's method. If the derivative function `df` is `null` in the parameter, initialize it at the beginning of the method using the approximation:

        (f(x + eps) - f(x - eps)) / (2 * eps).
    - Newton's method: https://en.wikipedia.org/wiki/Newton%27s_method

    Then write three parameterless methods returning the roots of functions
    using Newton's method:
    - `FindLinearRoot` - for the function `f(x) = x - 2`. Pass the exact derivative,
    - `FindQuadraticRoot` - for the function `f(x) = x * x + 2 * x + 1`. Pass `null` as the derivative,
    - `FindSinRoot` - for the function `f(x) = sin(x)`. Pass the exact derivative and the starting point `x0 = 2`.
    Pass the `sin` and `cos` functions by reference to the methods: `Math.Sin` and `Math.Cos`.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task1" >}}
{{% /details %}}

{{% details "TASK 2" false %}}
The task consists of 5 stages. The stages are defined in the files:
1. BinaryTree.cs
   
    Implement a method [`BreadthFirstSearch`](https://en.wikipedia.org/wiki/Breadth-first_search), that takes a function of built-in type `Action` (called `onVisit`) with argument of type `Node`, as a parameter. The method traverses the tree in a level order and calls `onVisit` action when it encounters a `Node`.  
    Pseudocode of the generic BFS on a binary tree:
    ```
    Queue Q = Queue.Empty
    Q.Enqueue(Root)
    while(Q not empty)
    Node N = Q.Dequeue()
    if(N.Left) Q.Enqueue(N.Left)
    if(N.Right) Q.Enqueue(N.Right)
    ```
    Then implement a `ValueCount` method that takes a value of type `T` as a parameter. It traverses the tree using a previously implemented `BreadthFirstSearch` method and counts how many times the value appears in a tree.

2. Benchmarking.cs

    Implement a method `CreateBenchmark`, that creates a benchmarking wrapper around a given Action to measure and report the average execution time over a specified number of iterations.
    This method takes the following parameters:
    - an Action delegate `action`
    - a string representing the name of the benchmark `name`
    - and an optional parameter `times` (default set to 1000)

    It returns an anonymous function of an `Action` delegate type. The returned `Action` delegate, when invoked, measures the average execution time of the provided `action` by running it in a loop for the specified number of `times`. It uses the `Stopwatch` class to accurately measure the elapsed time.

    After the loop, the method calculates and prints the average time taken for a single execution of the `action` in milliseconds. The output includes the name of the function `name` and the average time per execution in a format:
    ```
    "Function `name`: `elapsed` ms on average"
    ```
    Then implement method `CreateBenchmarks` using previously implemented method, that create 3 benchmarks comparing performance of the 3 methods of concatenating a string (defined in the same file): `StringAdd`, `StringBuilder`, and `StringJoin`.  
    Relevant `Stopwatch` functions and properties:
    - `new Stopwatch()`
    - `stopwatch.Start()`
    - `stopwatch.Stop()`
    - `stopwatch.Elapsed.TotalMilliseconds`

3. DictionaryExtensions.cs
   
    Implement a method `Merge<TKey, TValue>` - an extension method for the `IDictionary` interface, that merges an existing value associated with a given key in the dictionary with a given value.  

    If the specified key is not already associated with a value or is associated with `null`, the method associates it with the given as parameter non-`null` value. Otherwise, it replaces the associated value with the results of the given remapping function, or removes if the result of remapping is `null`.  

    The method takes the following parameters:
    - `TKey` `key` - The key of the entry to be merged or added,
    - `TValue` `value` - the non-`null` value to be merged with the existing value associated with the `key` or, if no existing value or a `null` value is associated with the key, to be associated with the key,
    - `Func` `remappingFunction` - the function to recompute a value if present, based on old value and `value`  

    It returns the new value associated with the specified `key`, or `null` if no value is associated with the `key`.  

    Then implement the `MergeLogs` method using the previously defined Merge method, which merges two dictionaries. In case of a conflicting key in both dictionaries, update the value in the source dictionary by concatenating values from both dictionaries (use `string.Concat`).

4. EnumerableExtensions.cs
   
    Implement method `Collect<TSource, TAccumulate>` - an extension method for the `IEnumerable` interface, that provides a generic mechanism for accumulating elements from an enumerable source into a result, allowing flexibility in how the accumulation is performed by providing a custom accumulator action.  

    The method takes the following parameters:
    - `TAccumulate` `collection` - a result container.
    - `Action` `accumulator` - an associative, non-interfering, stateless function for incorporating an additional element into a result, that takes an argument of a `TAccumulate` type and an argument of a `TSource` type with no return value.   

    It returns the result of the reduction.  

    Implement a method `Filter<T>` - an extension method for the `IEnumerable` interface, that takes a `Predicate` `predicate` parameter. The method iterates through each element in the source collection and yields only those elements for which the specified `predicate` evaluates to `true`. It returns an `IEnumerable<T>` containing the filtered elements.  

    Then implement a method `ConcatStrings`, that filters the elements of enumeration, keeping only those that are of type `string`, using the `Filter` method. The filtered strings are then concatenated into a single `string` using previously implemented `Collect` method and `StringBuilder`. Finally, the method returns the resulting concatenated string.

5. Numerics.cs
   
    Declare a delegate `Function` that takes one double parameter `x` and returns one double number.

    Implement a method `HalleyRootFinding`, that performs the Halley method for root finding. It takes a `Function f` and optional first and second derivatives (`Function df` and `ddf` respectively with `null` as a default value), an initial guess (`x0`) with a default value `0`, and a tolerance (`eps`) with a default value `1e-6`. If the first or the second derivative was not provided (`null` value) initialize them with the following estimation: `df = (f(x + eps) - f(x - eps)) / (2 * eps)`.

    Pseudocode of the [Halley root finding procedure](https://en.wikipedia.org/wiki/Halley%27s_method):
    ```
    Halley(f, df, ddf, x0, eps):
    xn = x0
    do
        x = xn
        xn = x - 2 * f(x) * df(x) / (2 * df(x) * df(x) - f(x) * ddf(x))
    while |x - xn| > eps
    return xn
    ```
    Then implement `FindLinearRoot`, `FindQuadraticRoot`, and `FindLogRoot` functions that find a root of the following functions using Halley's method:
    - `FindLinearRoot`: `f(x) = 2x - 1`, also specify the exact first and second derivatives. Keep the default starting point and epsilon. 
    - `FindQuadraticRoot`: `f(x) = 2x^2 + 3x + 1`, keep all parameters of Halley's method at default value. 
    - `FindLinearRoot`: `f(x) = log(x)`, specify the exact first derivative, but not the second. Use `2` as a starting point (`x0`). Keep the default epsilon value. Pass the functions as a references to `double.Log` and `double.ReciprocalEstimate` methods.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task2" >}}
{{% /details %}}

## LINQ
**LINQ** (Language-Integrated Query) is a set of extension methods on `IEnumerable<T>` (and `IQueryable<T>`) that let you perform various SQL-like operations on collections. It has two equivalent syntaxes - the method syntax and the query syntax - that compile to the same code.

> [!NOTE]
> `IQueryable<T>` is the same set of extension methods as `IEnumerable<T>`, but instead of building a delegate pipeline, they build an expression tree. When working with [ORM](https://en.wikipedia.org/wiki/Object%E2%80%93relational_mapping) frameworks such as [Entity Framework](https://en.wikipedia.org/wiki/Entity_Framework), the expression tree is translated into [SQL](https://en.wikipedia.org/wiki/SQL) and run directly on the database. We will not be working on database during this workshop, but it may become useful during your future courses.

### Deferred execution
Most LINQ operators are deferred - they don't run when you build the query, but only when you request the result (the same is true for throwing exceptions). This can boost the efficiency in case of large, complex queries, but only if you use it correctly.
```csharp
var query = numbers.Where(x => ExpensiveCheck(x));

// inefficient
if (query.Any())                   // runs the query once
    var first = query.First();     // runs the query again!

// efficient (safes query result to memory)
var materialized = query.ToList(); // runs the query only once
if (materialized.Count > 0)
    var first = materialized[0];
```  
`foreach` loops also force the execution of the entire query.

### Method overview
#### Deffered
| Category | Method |
| --- | --- |
| **Filtering** | `Where`, `OfType` |
| **Projection** | `Select`, `SelectMany`, `Zip` |
| **Set Operations** | `Concat`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy`, `Distinct`, `DistinctBy` |
| **Partitioning** | `Take`, `TakeLast`, `TakeWhile`, `Skip`, `SkipLast`, `SkipWhile`, `Chunk` |
| **Joining** | `Join`, `GroupJoin`, `LeftJoin`, `RightJoin` |
| **Sorting** | `Order`, `OrderBy`, `OrderDescending`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Reverse`, `Shuffle` | 

#### Immediate
| Category | Method |
| --- | --- |
| **Quantifiers** | `All`, `Any`, `Contains` |
| **Element Selection** | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `DefaultIfEmpty` |
| **Aggregation** | `Aggregate`, `AggregateBy`, `Average`, `Count`, `LongCount`, `Sum`, `Max`, `Min`, `MinBy`, `MaxBy`, `SequenceEqual` |
| **Generation** | `Empty`, `Range`, `Repeat`, `Prepend`, `InfiniteSequence` |

#### Mixed
| Category | Deffered | Immediate |
| --- | --- | --- |
| **Conversion** | `Cast`, `AsEnumerable`, `AsQueryable` | `ToArray`, `ToList`, `ToDictionary`, `ToHashSet` |
| **Grouping** | `GroupBy` | `ToLookup` |

### Deeper explanations of each method:
- [LINQ Lecture](https://csharp.mini.pw.edu.pl/en/lectures/06.collections/linq/#method-overview)
- [Enumerable Class Methods](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate?view=net-10.0)
- [Standard query operators overview](https://learn.microsoft.com/en-us/dotnet/csharp/linq/standard-query-operators/)

> [!WARNING]
> Don't use the operations that require the whole collection like `OrderBy` on **infinite sequences**! This is an equivalent to `while(true)` loop without a break clause.

### Method Syntax vs Query Syntax 
As stated before, *method syntax* and *query syntax* compile to the same code, so the choice between them is only a matter of readability. It is important to note, that not every method in *method syntax* has an equivalent in *query syntax*.

Every query in *query syntax* starts with:
```csharp
from x in source	
```
and ends with either `select` or `group` clause.

| Query	syntax | Method syntax equivalent |
| --- | --- | 
| `where p`                           | `.Where(x => p)`
| `select e`                          | `.Select(x => e)`
| `orderby k`	                      | `.OrderBy(x => k)`
| `orderby k descending`	          | `.OrderByDescending(x => k)`
| `orderby k1, k2`                    | `.OrderBy(x => k1).ThenBy(x => k2)`
| `group x by k`	                  | `.GroupBy(x => k)`
| `join y in other on k1 equals k2`   | `.Join(other, x => k1, y => k2, ...)`

#### `let` clause
The `let` clause binds an intermediate value to a name inside the query, so you can reuse it without recomputing. 
```csharp
var queryResult = from x in source	
                  let y = x.prop + 1
                  where y >= 10
                  orderby x.prop
                  select new { x.prop, y };

var methodResult = source
    .Select(x => new { x, y = x.prop + 1 })
    .Where(x => x.y >= 10)
    .OrderBy(x => x.prop)
    .Select(x => new { x.prop, x.y });
```

#### `into` clause
The `into` clause lets you write more clauses after `group`/`select`/`join`, treating the current result as the input to a new query. This is how *query syntax* chains transformations, that *method syntax* would express with `.Select(...)`.
```csharp
var queryGroupJoin = from x in source
             join y in other on x.key equals y.key into result
             select new { x.key, Result = result };

var methodGroupJoin  = source.GroupJoin(other, x => x.key, y => y.key, 
    (x, result) => new { x.key, Result = result });   
```

#### Multiple `from` clauses
You can chain multiple `from` clauses to flatten the collection.

```csharp
var queryFlatten = from x in source
                   from e in x.list
                   select e;

var methodFlatten = source.SelectMany(x => x.list);
```

#### Mixing both syntaxes
Nothing stops you from mixing two syntaxes to achieve better readability.

```csharp
var result = (from n in numbers where n > 5 select n).Take(3).ToList();
```

### PLINQ 
`AsParallel()` partitions LINQ query across threads and executes it in parallel on the thread pool:
```csharp
var sum = Enumerable.Range(0, 1_000_000)
    .AsParallel()
    .Where(x => ExpensiveCheck(x))
    .Sum();
```
PLINQ is most useful for CPU-bound work on large in-memory collections (for I/O-bound work, `async`/`await` and `IAsyncEnumerable<T>` are usually better). Small collections aren't worth parallelizing and not all operators (e.g. `OrderBy`, `GroupBy`, `Reverse`) parallelize well.

> [!IMPORTANT]
> For queries with multiple source sequences, `AsParallel()` must be applied to **every sequence**, otherwise the compiler will pick the sequential overload for the whole query.

> [!IMPORTANT]
> Don't call `AsParallel()` multiple times. The result is already a `ParallelQuery<T>` and calling `AsParallel()` again can hurt the performance.

#### Other methods
- `AsSequential()` - converts a `ParallelQuery<T>` back into an `IEnumerable<T>`, which means you can run some parts of your query in parallel and others sequentially (helpful when part of the code that the query interacts with is not tread-safe),
- `AsOrdered()` - reorders the results at the end to keep the original order at the cost of performance (by default the chunks are processed on different threads and merged as they finish),
- `AsUnordered()` - cancels `AsOrdered()`,
- `ForAll(Action<T>)` - invokes the delegate for each element **on the worker thread** that produced that element.


## Ranges
> [!NOTE]
> Slicing and indexing should be already known to you well. They were explained during the [lecture about arrays](https://csharp.mini.pw.edu.pl/en/lectures/03.basics/arrays/).

A range is a way to describe a slice of a sequence — from one position to another. C# 8 introduced two related types:
- `Index` — a single position,
- `Range`— a pair of `Index` values (`start..end`), describing a slice.

### `Index` examples:
```csharp
Index first = 0;     
Index second = 1;
Index last = ^1;     
Index secondLast = ^2;
```
There is also a helper method `Index.GetOffset(int length)` that converts an `Index` to a concrete zero-based offset given a collection length:
```csharp
Index i = ^1;
Console.WriteLine(i.GetOffset(5)); // 4, because ^1 = 5-1 = 4
```

### `Range` examples:
```csharp
Range r1 = 0..3;        // elements 0, 1, 2  
Range r2 = 2..^1;       // from index 2 to one before last
Range r3 = ..3;         // from start to index 3 - same as 0..3
Range r4 = 2..;         // from index 2 to the end
Range r5 = ..;          // whole sequence
```
As you can see, `start` is inclusive, while `end` is exclusive.

### Slicing
Slicing with ranges works with the types that either have `Range` indexer:
- arrays, such as `T[]`, `T[][]` etc. (only for the most outer dimension), but not `T[,]` and so on,
- `Span<T>` / `ReadOnlySpan<T>`,
- `string`,

or have `Length` / `Count` property and `Slice(int start, int length)` method, such as `List<T>`.

```csharp
string s = "hello world";
string hello = s[..5];      // "hello"
string world = s[6..];      // "world"
```
So, for custom types, you either need to create an indexer accepting `Range` (`this[Range]`) or provide the same interface as `List<T>` does and rely on the compiler, that will rewrite it for you.

### Ranges vs LINQ
This part is why we bring back ranges int this tutorial. As we learned, LINQ also offers slicing capabilities for aforementioned types, but with certain trade-offs:
| Task | LINQ | Ranges | Comparison |
|---|---|---|---|
| Slice the first 5 elements of an `int[]` | `arr.Take(5).ToArray()` | `arr[..5]` | LINQ allocates an enumerator, a delegate closure and the output array; ranges allocate only the result array. |
| Take a part of `string` | `string.Concat(s.Skip(1).Take(3))` | `s[1..4]` | Ranges compile to an optimized `Substring`; LINQ allocates per-operator enumerators, internal buffers, and the concatenated string. |
| Extract a fixed range from a `List<T>` | `list.Skip(100).Take(50).ToList()` | `list[2..7]` | LINQ allocates two enumerators, their closures, intermediate buffers and the output list; ranges allocate only the output list via one `Slice` and copy call. |

Bottom line is, that LINQ works on `IEnumerable<T>` interface and enumerators, so it will always create a much bigger overhead compared to more direct methods. If your source is one of the aforementioned types and you're slicing once without further transformation, use ranges.

## Read more
- [Indices and ranges](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes)
- [Language Integrated Query (LINQ)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)

> [!IMPORTANT]
> ## LINQ example tasks
{{% details "TASK 3" false %}}
The goal of the task is to write 5 queries using LINQ for a simple database of an online store.

Questions 1–4 should be implemented as single query expressions. Question 5 can be implemented as several related query expressions
(as single expressions it's of course also possible, but it will be quite complicated).

For almost every query, you should create a new anonymous class that contains only the fields needed at that stage. Those fields should be inferred by looking at the results of the given query.

In the `Database.cs` file, there are classes representing the tables in the database (`Customer`, `LoyaltyCard`, `Product`, `Order`, `OrderProduct`), while the `Database` class contains the data lists representing the aforementioned tables.

**Note**: a single order can contain several products; in that case there are several rows in the `OrderProducts` table with the id of that order.

Modify only the `Program.cs` file.

1. List people (in alphabetical order by surname, then first name) who do not have a loyalty card.

2. Based on the PESEL number, select people who are more than 29 years old, by birth year. Display these people from oldest to youngest.

3. Determine the 3 people who have been using the store's services the longest.

4. Determine the 5 items with the highest total number of ordered units.

5. For each person, determine the total amount they spent in the store (taking into account the discount available to people with a loyalty card). List the customers in alphabetical order by surname, then first name.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task3" >}} 
{{% /details %}}

{{% details "TASK 4" false %}}
A database is provided with three tables: `database.Movies`, `database.Users`, `database.Ratings`. Exact information about available columns can be found by inspecting the `Movie`, `User`, and `RatingEntry` classes.

In the `User`s table, the `Age` field is not an age itself, but rather a bottom age-range identifier, e.g. 1 means the range 1-18. Each movie has a unique id number `MovieID`, each user has a unique id number in `UserID`. Both tables (`Movies`, `Users`) are connected through ratings: a single vote on a movie is cast by one user on one movie.

Modify only the `Program.cs` file.

1. Count how many users are in the given age groups, sort (ascending) by age.

2. What is the earliest year of a watched movie in the individual groups determined by age and gender — sort the results ascending by the earliest year.

3. Count the number of occurrences of each movie genre. Each genre is counted separately, so for `"Horror|Comedy"` we have +1 for the Horror genre and +1 for the Comedy genre. Sort ascending by genre.

4. We define remakes of a single movie as the set of movies whose title is the same, but the production years are different. Your result should contain all movies that are remakes (all movies for which we can find at least one other movie with the same title). The result should be sorted ascending by production year and contain only the first 14 movies.

5. Sort (descending) the movies by the average rating of votes. Take into account only movies that have received at least 100 votes The result should contain just the first 20 movies.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task4" >}}
{{% /details %}}


{{% details "**Additonal materials for workshop**" false %}}
Materials: {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Lab" >}}
Solution: {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/LabSolution" >}}
{{% /details %}}