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
Delegates can be combined with `+=` and removed with `-=`. When you invoke a multicast delegate, all wrapped methods run in order.
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
> You can extend interfaces as well as classes. `Where`, `Select` and other LINQ methods that you will learn in this tutorial are all extension methods on `IEnumerable<T>`:
```csharp
public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate) { ... }
public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) { ... }
```

### Extension members
Since C# 14 you can add extension properties, static extension members, and extension operators. For example:

```csharp
// extension method (before)
public static class EnumerableExtensions {
    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();
}
if (list.IsEmpty()) { ... }
```
```csharp
// extension property (now)
public static class EnumerableExtensions {
    extension<T>(IEnumerable<T> source) {
        public bool IsEmpty => !source.Any();
    }
}
if (list.IsEmpty) { ... }
```

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
   
    Write a static function `RandomValueFunctionFactory` that returns an anonymous function
    returning a (pseudo)random double value from the range given by the parameters:
    `double min` and `double max`.

    Write a static function `RandomPointFunctionFactory` that returns an anonymous function
    returning a (pseudo)random vector of two double values (as a tuple) from the range given by the parameters: `double xMin`, `double xMax`, `double yMin`, `double yMax`. Use the previously defined method.

    Write a static function `MonteCarloPiEstimation` returning an approximation of the number π (of type `double`).
    Use the previously defined method. The method should take the number of samples (points) used for the approximation of type `int` (with a default value of 1024).
    - Monte Carlo method: <https://en.wikipedia.org/wiki/Monte_Carlo_method>
    - Hint: Generate random points in a rectangle containing (a sector of) the unit circle and check whether they are inside (the sector of) the unit circle. Calculate the ratio of points inside (the sector of) the circle to the total number of points to estimate the value of π.
  
5. Numerics.cs
   
    Declare a delegate named `Function` that takes a parameter x of type `double` and returns a value of type `double`. Implement a `static` method `NewtonRootFinding` that takes a function f of type `Function`, a derivative function df of type `Function` (with a default value of `null`), a starting point `x0` (with a default value of 0), and a precision `eps` (with a default value of 1e-6). The method should return the computed root of the equation using Newton's method. If the derivative function `df` is `null` in the parameter, initialize it at the beginning of the method using the approximation:

        (f(x + eps) - f(x - eps)) / (2 * eps).
    - Newton's method: <https://en.wikipedia.org/wiki/Newton%27s_method>

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

    Implement a method `CreateBenchmark`, that creates a benchmarking wrapper around a given `Action` to measure and report the average execution time over a specified number of iterations.
    This method takes the following parameters:
    - an `Action` delegate `action`
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

## Read more
- [Delegates (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- [Lambda expressions and anonymous functions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Extension members (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)

## LINQ
**LINQ** (Language-Integrated Query) is a set of extension methods on `IEnumerable<T>` (and `IQueryable<T>`) that let you perform various SQL-like operations on collections. It has two equivalent syntaxes - the *method syntax* and the *query syntax* - that compile to the same code.

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
The `let` clause binds an intermediate value to an identifier inside the query, so you can reuse it without recomputing. 
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
There is also a helper method `Index.GetOffset(int length)` that converts an `Index` to a concrete nuemrical zero-based offset given a collection length:
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

> [!IMPORTANT]
> ## LINQ example tasks
{{% details "TASK 3" false %}}
The goal of the task is to write 5 queries using LINQ for a simple database of an online store.

Questions 1–4 should be implemented as single query expressions. Question 5 can be implemented as several related query expressions
(as single expressions it's of course also possible, but it will be quite complicated).

For almost every query, you should create a new anonymous class that contains only the fields needed at that stage. Those fields should be inferred by looking at the results of the given query.

In the `Database.cs` file, there are classes representing the tables in the database (`Customer`, `LoyaltyCard`, `Product`, `Order`, `OrderProduct`), while the `Database` class contains the data lists representing the aforementioned tables.

**Note**: a single order can contain several products - in that case there are several rows in the `OrderProducts` table with the id of that order.

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

In the `User`s table, the `Age` field is not an age itself, but rather a bottom age-range identifier (e.g. 1 means the range 1-18). Each movie has a unique id number `MovieID`, each user has a unique id number in `UserID`. Both tables (`Movies`, `Users`) are connected through ratings: a single vote on a movie is cast by one user on one movie.

Modify only the `Program.cs` file.

1. Count how many users are in the given age groups, sort (ascending) by age.

2. What is the earliest year of a watched movie in the individual groups determined by age and gender — sort the results ascending by the earliest year.

3. Count the number of occurrences of each movie genre. Each genre is counted separately, so for `"Horror|Comedy"` we have +1 for the Horror genre and +1 for the Comedy genre. Sort ascending by genre.

4. We define remakes of a single movie as the set of movies whose title is the same, but the production years are different. Your result should contain all movies that are remakes (all movies for which we can find at least one other movie with the same title). The result should be sorted ascending by production year and contain only the first 14 movies.

5. Sort (descending) the movies by the average rating of votes. Take into account only movies that have received at least 100 votes The result should contain just the first 20 movies.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task4" >}}
{{% /details %}}

## Read more
- [Indices and ranges](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes)
- [Language Integrated Query (LINQ)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)

{{% details "**Materials for workshop**" false %}}
Thanks to **Simon and Claudius** for providing the task.

## Task 1 - Wrapped: Listening History Analyzer

Every December, music streaming services present their users with a personalized summary of the year: minutes listened, favorite artists, tracks and genres. In this task, you will build such a summary (we will call it *Wrapped*) for a listening history stored in CSV files. The application will consist of a single project – a console application.

We start with code that only loads the data. Then, stage by stage, you will write filters based on delegates, your own iterators and extension methods, LINQ queries, and finally combine them into the yearly report.

> [!NOTE]
> #### Starting code
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.00" >}}

The `Data` directory contains a generated listening history of one (fictional) user in 2025:

- `artists.csv` – 40 artists: `id,name,genres` (genres are separated with `;`),
- `tracks.csv` – 268 tracks from the user's library: `id,title,artist_id,length_s`,
- `plays.csv` – 3351 plays sorted by time: `timestamp,track_id,ms_played`.

```text
timestamp,track_id,ms_played
2025-01-02T07:37:43,247,211000
2025-01-02T07:41:16,103,1819
```

The starting code contains:

- The records `Artist`, `Track` and `Play`. `Play` has an additional computed property `Duration` (the time the track was played, as a `TimeSpan`).
- The `MusicLibrary` class with read-only lists `Artists`, `Tracks` and `Plays`, and the static method `Load(string directory)` loading the data. Note the private method `ReadCsv<T>(string path, Func<string[], T> parse)` – it already uses a delegate: the caller decides how a row is converted into an object by passing a lambda expression.
- `Program.cs`, which loads the data and prints the number of loaded objects and the first play. The line `CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;` makes numbers and dates formatted identically on every machine, so that your output can be compared with the expected outputs below.

Data files are copied to the output directory during the build (see `Wrapped.csproj`). Run the program (`dotnet run` in the `Wrapped` directory):

```text
Loaded 40 artists, 268 tracks and 3351 plays.
First play: Play { Timestamp = 01/02/2025 07:37:43, TrackId = 247, MsPlayed = 211000, Duration = 00:03:31 }
```

> [!TIP]
> The compiler generates a `ToString()` method for records, which prints the values of all public properties. That is why `Console.WriteLine` prints a readable description of a `Play` object.

### Stage 1: Filters – Delegates and Lambdas

In this stage, we will create a set of reusable *filters* – functions of type `Func<Play, bool>` that decide whether a play satisfies a given condition – and learn the different ways of creating delegates. Do not use LINQ yet.

> [!IMPORTANT]
> #### Method Groups, Lambdas and Closures
>
> A delegate can be created from:
> - a **method group** – the name of an existing method, e.g. `Func<Play, bool> f = Filters.IsSkip;`,
> - a **lambda expression**, e.g. `Func<Play, bool> f = p => p.MsPlayed < 1000;`.
>
> A lambda that uses variables or parameters from the enclosing scope is called a **closure**. Methods returning closures act as *delegate factories* – each call creates a new function "configured" with the given arguments:
> ```csharp
> public static Func<Play, bool> InMonth(int month) =>
>     play => play.Timestamp.Month == month; // captures the parameter 'month'
> ```
> If a lambda should not capture anything, mark it with the `static` modifier. The compiler then reports an error (CS8820 or CS8821) whenever the lambda tries to use a local variable, a parameter or `this`, which protects you against accidental captures (and the hidden heap allocations that come with them):
> ```csharp
> public static readonly Func<Play, bool> AtNight =
>     static play => play.Timestamp.Hour is >= 22 or < 6;
> ```

#### The Filters Class

Create a file named `Filters.cs` with a static class `Filters` containing:

1. A field `public static readonly TimeSpan SkipThreshold` equal to 30 seconds. Streaming services count a play as a *stream* only if it lasted at least 30 seconds; shorter plays are *skips*.
2. Ordinary static methods `bool IsSkip(Play play)` (play shorter than `SkipThreshold`) and `bool IsWeekend(Play play)` (play on Saturday or Sunday).
3. The `AtNight` field shown above – a `static` lambda stored in a `Func<Play, bool>` field (plays between 22:00 and 6:00).
4. Factory methods returning closures:
   - `Func<Play, bool> InMonth(int month)` (shown above),
   - `Func<Play, bool> LongerThan(TimeSpan duration)` – plays longer than `duration`,
   - `Func<Play, bool> ByArtist(MusicLibrary library, string artistName)` – plays of the artist's tracks. Before returning the lambda, collect the identifiers of the artist's tracks into a `HashSet<int>` using loops. The returned lambda captures the set, so it is computed only once, and each check is just `trackIds.Contains(play.TrackId)`.
5. Combinators creating new filters from existing ones:
   - `Func<Play, bool> Not(Func<Play, bool> filter)` – negation,
   - `Func<Play, bool> All(params Func<Play, bool>[] filters)` – true if all filters are satisfied,
   - `Func<Play, bool> Any(params Func<Play, bool>[] filters)` – true if at least one filter is satisfied.

#### Counting Plays

In `Program.cs`:

1. Add a static local function `int Count(IEnumerable<Play> plays, Func<Play, bool> predicate)` which counts the plays satisfying the predicate with a `foreach` loop. (In Stage 3 we will replace it with the LINQ `Count` method.)
2. Create a lambda `Action<string, int> print` printing a label and a value in aligned columns, e.g. `$"{label,-30}{value,6}"`.
3. Using `Count`, `print` and the filters, print the number of: all plays, skips, weekend plays, night plays, weekend nights (`All`), night streams (`All` + `Not`), plays longer than 6 minutes, plays of *Ada & The Lambdas*, and plays at 7 a.m. (pass an inline lambda to `Count`).

> [!TIP]
> #### `Func<Play, bool>` vs `Predicate<Play>`
>
> `List<T>.RemoveAll` expects a `Predicate<T>`. Although `Predicate<Play>` and `Func<Play, bool>` have identical signatures, they are **different, incompatible types**. A method group or a lambda can be converted to any delegate type with a compatible signature, but an existing delegate object cannot:
> ```csharp
> List<Play> streams = [.. plays];
> int removed = streams.RemoveAll(Filters.IsSkip);         // OK - method group
> // streams.RemoveAll(Filters.AtNight);                   // error CS1503: cannot convert Func<Play, bool> to Predicate<Play>
> streams.RemoveAll(new Predicate<Play>(Filters.AtNight)); // OK - explicit conversion
> ```

4. Copy the plays into a `List<Play>` (as shown above), remove skips and then night plays from it using `RemoveAll`, and print the number of removed skips and the number of remaining (day) streams.

#### The Captured Loop Variable

Finally, let us count the plays in each month. Create a list of 12 filters in a `for` loop and print the result of `Count` for each of them in one line (use the `{date:MMM}` format to get abbreviated month names):

```csharp
var monthFilters = new List<Func<Play, bool>>();
for (int month = 1; month <= 12; month++)
{
    monthFilters.Add(p => p.Timestamp.Month == month);
}
```

All counts are equal to 0. Why?

> [!WARNING]
> #### Capturing a Loop Variable
>
> All twelve lambdas capture **the same** variable `month` – the variable itself, not its value at the moment the lambda was created. The lambdas are executed after the loop has finished, when `month` is already equal to 13. In a `for` loop, there is only one loop variable for the whole loop. (A `foreach` loop creates a new variable in every iteration, so it does not have this problem since C# 5.)
>
> The fix is to capture a variable that is created anew in each iteration: copy the loop variable to a local variable declared inside the loop body (`int m = month;`), or – even better – use our factory `Filters.InMonth(month)`, whose parameter is a separate variable for every call.

Fix the loop with `Filters.InMonth` and print the counts again. Expected output:

```text
All plays                       3351
Skips                            446
Weekend plays                    955
Night plays                      875
Weekend nights                   324
Night streams                    760
Longer than 6 minutes            155
Ada & The Lambdas                201
Plays at 7 a.m.                  368
Removed skips                    446
Day streams                     2145
Plays per month (bug):
 Jan 0 Feb 0 Mar 0 Apr 0 May 0 Jun 0 Jul 0 Aug 0 Sep 0 Oct 0 Nov 0 Dec 0
Plays per month (fixed):
 Jan 273 Feb 298 Mar 299 Apr 277 May 233 Jun 301 Jul 274 Aug 262 Sep 285 Oct 294 Nov 326 Dec 229
```

> [!NOTE]
> #### Stage 1
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.01" >}}

### Stage 2: Custom Iterators and Extension Members

In this stage, we will write our own LINQ-like operators as extension methods implemented with iterators, observe how deferred execution works, and add extension members using the new C# 14 syntax.

#### The EnumerableExtensions Class

Create a file named `EnumerableExtensions.cs` with a static class `EnumerableExtensions`. All methods are generic extension methods for `IEnumerable<T>`. The first one – our own counterpart of `Where` – looks as follows:

```csharp
public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (var item in source)
    {
        if (predicate(item))
        {
            yield return item;
        }
    }
}
```

Implement analogously:

1. `IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)` – our counterpart of `Select`, returning `selector(item)` for each element.
2. `IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)` – calls `action` for each element and passes the element on unchanged. It will let us observe (and count) the elements flowing through a query.
3. `IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)` – returns pairs of consecutive elements: for `a, b, c, d` it returns `(a, b), (b, c), (c, d)`. For a sequence with fewer than two elements, it returns an empty sequence.

> [!IMPORTANT]
> #### Using an Enumerator Directly
>
> A `foreach` loop is syntactic sugar: it calls `GetEnumerator()`, and then calls `MoveNext()` and reads `Current` until `MoveNext()` returns `false`. Sometimes it is more convenient to drive the enumerator manually, e.g. to handle the first element differently:
> ```csharp
> using var enumerator = source.GetEnumerator(); // IEnumerator<T> is IDisposable
> if (!enumerator.MoveNext())
> {
>     yield break; // empty sequence - end the iterator
> }
> T previous = enumerator.Current;
> // ... continue with a while (enumerator.MoveNext()) loop
> ```
> `yield break` ends the iteration (`MoveNext()` of the generated enumerator returns `false`).

Implement `Pairwise` using an enumerator directly, as shown above.

> [!TIP]
> #### Argument Validation in Iterators
>
> The whole body of an iterator method – including argument checks at its beginning – runs only when the enumeration starts, not when the method is called. If an error should be reported immediately, check the arguments in an ordinary method and move the `yield` part to a local iterator function. For example, a sliding window (a generalization of `Pairwise`):
> ```csharp
> public static IEnumerable<T[]> Window<T>(this IEnumerable<T> source, int size)
> {
>     ArgumentOutOfRangeException.ThrowIfLessThan(size, 1); // checked immediately
>     return Iterator();
>
>     IEnumerable<T[]> Iterator() // a local iterator function
>     {
>         var window = new Queue<T>(size);
>         foreach (var item in source)
>         {
>             window.Enqueue(item);
>             if (window.Count > size)
>             {
>                 window.Dequeue();
>             }
>             if (window.Count == size)
>             {
>                 yield return window.ToArray();
>             }
>         }
>     }
> }
> ```

#### Extension Members

> [!IMPORTANT]
> #### Extension Blocks (C# 14)
>
> An extension block `extension(Receiver receiver) { ... }`, placed inside a static class, groups extension members for one receiver type. Besides methods, it can contain extension **properties**, static members and operators – things that cannot be expressed with the classic `this` parameter syntax. The receiver can be any type, including a constructed generic interface such as `IEnumerable<Play>`:
> ```csharp
> public static class ListeningExtensions
> {
>     extension(IEnumerable<Play> plays)
>     {
>         // Extension property
>         public TimeSpan TotalListeningTime
>         {
>             get
>             {
>                 var total = TimeSpan.Zero;
>                 foreach (var play in plays)
>                 {
>                     total += play.Duration;
>                 }
>                 return total;
>             }
>         }
>     }
> }
> ```
> Both syntaxes produce ordinary static methods and can be freely mixed – the classic syntax is still the most common one in existing code.

Create a file named `ListeningExtensions.cs` with a static class `ListeningExtensions` containing two extension blocks:

1. For `IEnumerable<Play>`:
   - the property `TimeSpan TotalListeningTime` – the sum of `Duration` of all plays (shown above),
   - the property `int StreamCount` – the number of plays which are not skips (use your `Filter` method and the filters from Stage 1).
2. For `TimeSpan`: the method `string ToHoursAndMinutes()`, which formats the time span as e.g. `"12 h 05 min"` (whole hours from `TotalHours` and minutes from `Minutes`).

#### Deferred Execution in Practice

Replace the contents of `Program.cs` (after loading the library) with three experiments.

**1. Lazy evaluation.** Build a query and enumerate only its first two elements:

```csharp
Console.WriteLine("Building the query...");
var longPlays = library.Plays
    .Tap(p => Console.WriteLine($"  pulled {p.Timestamp:MM-dd HH:mm} ({p.Duration:m\\:ss})"))
    .Filter(Filters.LongerThan(TimeSpan.FromMinutes(3)))
    .Map(p => $"{p.Timestamp:MM-dd HH:mm}");

Console.WriteLine("Enumerating...");
int taken = 0;
foreach (string time in longPlays)
{
    Console.WriteLine($"  got    {time}");
    if (++taken == 2)
    {
        break;
    }
}
```

Notice in the output that nothing happens while the query is being built, that the elements flow through `Tap`, `Filter` and `Map` one by one, and that only three out of 3351 plays are ever read.

**2. Multiple enumeration.** Count how many elements are pulled from the source by a query which is used twice:

```csharp
int pulled = 0;
var nightPlays = library.Plays.Tap(_ => pulled++).Filter(Filters.AtNight);
```

Print `nightPlays.TotalListeningTime.ToHoursAndMinutes()` and `nightPlays.StreamCount`, and then the value of `pulled`. Then reset `pulled` to 0, store the results of the query in a list (`new List<Play>(nightPlays)`), print both properties for the list and the value of `pulled` again.

> [!WARNING]
> #### Every Enumeration Runs the Query Again
>
> A variable holding a query does not hold its results – it holds a *recipe* for computing them. Each `foreach` (and each operator that consumes the sequence, such as our extension properties) runs the whole pipeline from the beginning. In our example, every play is pulled from the source twice. If the source is expensive (reading a file, a database query, a complex join) or non-deterministic (e.g. random numbers), enumerate the query once and store its results in a collection.

**3. Pairwise.** Using `Pairwise` (combined with `Filter`), count how many times the same track was played twice in a row. Then find the longest break in listening – the largest gap between the end of a play (`Timestamp + Duration`) and the start of the next one – and print it together with its start and end. Deconstruct the pairs directly in the loop: `foreach (var (previous, current) in library.Plays.Pairwise())`.

Expected output:

```text
Building the query...
Enumerating...
  pulled 01-02 07:37 (3:31)
  got    01-02 07:37
  pulled 01-02 07:41 (0:01)
  pulled 01-02 07:41 (3:29)
  got    01-02 07:41

Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 6702
Night listening: 49 h 51 min, 760 streams
Plays pulled from the source: 3351

Same track twice in a row: 276 times
Longest break: 252 h 11 min (2025-08-03 19:45 - 2025-08-14 07:57)
```

> [!NOTE]
> #### Stage 2
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.02" >}}

### Stage 3: First LINQ Queries

`Filter` and `Map` from the previous stage are simplified versions of LINQ's `Where` and `Select` (the real implementations only add optimizations, e.g. for arrays and lists). From now on, we will use LINQ. Replace the contents of `Program.cs` (after loading the library) with the queries described below.

#### Simple Aggregates

Our filters are ordinary `Func<Play, bool>` delegates, so they can be passed directly to LINQ operators. Define the sequence of streams (it is a deferred query, which we will reuse several times):

```csharp
var plays = library.Plays;
var streams = plays.Where(Filters.Not(Filters.IsSkip));
```

Print, using one LINQ expression for each value:

1. The number of plays, streams and skips (`Count()`, `Count(predicate)`).
2. Minutes listened – the sum of `MsPlayed` divided by 60 000 (`Sum`). Cast `MsPlayed` to `long` in the selector – `Sum` uses checked arithmetic and would throw `OverflowException` for larger data sets.
3. The average play duration in seconds (`Average`).
4. The number of distinct tracks streamed (`Select` + `Distinct` + `Count`) and the number of listening days (distinct `DateOnly.FromDateTime(p.Timestamp)`).
5. The time of the first and the last play (`Min`, `Max`).
6. Whether there was any play at 4 a.m. (`Any`) and whether all plays are from 2025 (`All`).
7. The longest play (`MaxBy`) with the title of its track (`library.Tracks.First(...)`).

> [!TIP]
> `MaxBy`/`MinBy` return the whole element with the largest/smallest key, while `Max`/`Min` return only the key. `First` throws an exception if no element matches, while `FirstOrDefault` returns `default` (`null` for reference types) in that case.

#### Top 10 Tracks

> [!IMPORTANT]
> #### Query Syntax
>
> The query syntax is translated by the compiler into calls of LINQ methods: `where` → `Where`, `select` → `Select`, `orderby a descending, b` → `OrderByDescending(...).ThenBy(...)`. The `let` clause introduces a helper variable – under the hood, the compiler projects each element into an anonymous object holding the element and the computed value. Operators without a keyword (e.g. `Take`, `Count`, `MaxBy`) must be called as methods:
> ```csharp
> var topTracksQuery =
>     (from t in library.Tracks
>      let count = streams.Count(p => p.TrackId == t.Id)
>      orderby count descending, t.Title
>      select new { Track = t, Streams = count })
>     .Take(10);
> ```
> `new { Track = t, Streams = count }` creates an object of an **anonymous type** – an immutable class generated by the compiler, with read-only properties and value-based equality.

Write the same query in method syntax: project each track into an anonymous object with the number of streams (`Select`), sort it descending by the number of streams and then by title (`OrderByDescending`, `ThenBy`), and take the first 10 elements (`Take`). Print the result with positions and artist names (find the artist with `library.Artists.First(...)`). To get the position of each element, use the `Index()` method (.NET 9), which returns `(int Index, T Item)` tuples:

```csharp
foreach (var (i, x) in topTracks.Index())
{
    // ...
}
```

Then check that both queries give the same result with `SequenceEqual` (it works thanks to the value equality of anonymous types).

#### Top Artist

Find the artist with the most streams: for each artist, sum (`Sum`) the numbers of streams of their tracks (`Where`), and choose the maximum with `MaxBy`.

> [!WARNING]
> #### Hidden Quadratic Complexity
>
> The queries above are concise, but `streams.Count(...)` inside `Select` runs over all plays **for every track** – 268 × 3351 ≈ 900 000 predicate calls, and each of them also re-runs the deferred `Where`. For our data, it still takes a fraction of a second, but such queries become a problem for larger data. In Stage 5 we will compute the same results in a single pass using grouping.

Expected output:

```text
Plays:            3351 (2905 streams, 446 skips)
Minutes listened: 11465
Average play:     205.3 s
Distinct tracks:  239
Listening days:   270
First and last:   2025-01-02 07:37, 2025-12-30 23:08
Any play at 4 am: False
All in 2025:      True
Longest play:     Little Hours (8:00) on 2025-01-14

Top 10 tracks:
  1. Winter Letters - Static Bloom                 134
  2. Sleepless Roads - Static Bloom                 89
  3. Lazy Satellites - Pixel Monsoon                83
  4. Analog Memories - Lena Marlowe                 73
  5. Burning Horizons - Concrete Gardens            63
  6. Crystal River - Static Bloom                   59
  7. Cosmic Cities - Pixel Monsoon                  57
  8. Lazy Evaluation - Ada & The Lambdas            54
  9. Digital Hours - Mira Volta                     53
 10. Faded Horizons - Static Bloom                  52
Query syntax gives the same result: True
Top artist: Static Bloom (430 streams)
```

> [!NOTE]
> #### Stage 3
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.03" >}}

### Stage 4: Joining Data

Plays contain only track identifiers, and tracks contain only artist identifiers. So far, we have looked up related objects with `First`, which scans the whole list for every element. In this stage, we will join the three collections, as in a relational database.

> [!IMPORTANT]
> #### Join
>
> `Join` combines elements of two sequences with equal keys (like `INNER JOIN` in SQL). It takes the inner sequence, two key selectors and a function creating the result from a matching pair:
> ```csharp
> var playsWithTracks = library.Plays.Join(
>     library.Tracks,               // inner sequence
>     play => play.TrackId,         // key of the outer element
>     track => track.Id,            // key of the inner element
>     (play, track) => (play, track));
> ```
> In query syntax, the same join is written as `join track in library.Tracks on play.TrackId equals track.Id`. `Join` first builds a hash table of the inner sequence, so the whole join takes linear time instead of the quadratic time of nested searches. Elements without a match are skipped.

#### Joined Plays

1. Create a file named `PlayInfo.cs` with the record `PlayInfo(Play Play, Track Track, Artist Artist)`.
2. In `MusicLibrary`, add the method `IEnumerable<PlayInfo> GetPlayInfos()`, which joins `Plays` with `Tracks` and then with `Artists`. Write it in query syntax (with two `join` clauses) – for multiple joins, it is much more readable than method syntax.
3. In `Program.cs`, store the joined plays in a variable (`var infos = library.GetPlayInfos();`) and print the first three of them (`Take`) in the format shown in the expected output.
4. Write the same join in method syntax (two chained `Join` calls) and check with `SequenceEqual` that both versions give the same result. Records, like anonymous types, have value-based equality.

#### Genres

> [!IMPORTANT]
> #### SelectMany
>
> `SelectMany` flattens a sequence of sequences: for each element it gets a sequence and concatenates all of them into one (similarly to `flatMap` in other languages). In query syntax, it corresponds to a second `from` clause:
> ```csharp
> var artistGenres =
>     from artist in library.Artists
>     from genre in artist.Genres
>     select (artist.Name, genre); // one element for every (artist, genre) pair
> ```

1. Print all genres in the library in alphabetical order (`SelectMany` + `Distinct` + `Order`) as a comma-separated list (`string.Join`).
2. Print the names of jazz artists – use query syntax with two `from` clauses and a `where` clause.
3. Print the total time of jazz listening: filter the joined plays by the artist's genres (`Contains`), take the plays (`Select(i => i.Play)`) and use the `TotalListeningTime` extension property from Stage 2.

#### What Was Never Played?

> [!IMPORTANT]
> #### GroupJoin and LeftJoin
>
> `GroupJoin` pairs every element of the outer sequence with a sequence of *all* matching inner elements – possibly empty. It is the basis of a left outer join (in query syntax: `join ... into group`).
>
> .NET 10 introduced `LeftJoin` (and `RightJoin`), which directly implements a `LEFT JOIN`: every outer element appears in the result, and if it has no match, the inner element passed to the result selector is `null` (`default`):
> ```csharp
> var result = library.Artists.LeftJoin(
>     infos,
>     artist => artist.Id,
>     info => info.Artist.Id,
>     (artist, info) => new { artist.Name, Info = info }); // info is null for artists without plays
> ```
> Before .NET 10, the same effect required `GroupJoin` + `SelectMany` + `DefaultIfEmpty`.

1. Using `GroupJoin` of `library.Tracks` with `library.Plays`, compute the number of plays of every track, select the titles of tracks which were never played, and store them in a list (`ToList`). Print their number and the first three titles.
2. Using `LeftJoin` of `library.Artists` with the joined plays, find the artists that were never played.

> [!TIP]
> The tracks that were never played can also be found with a set operation: `library.Tracks.ExceptBy(library.Plays.Select(p => p.TrackId), t => t.Id)`.

Expected output:

```text
First plays:
  2025-01-02 07:37  Paper Satellites - Thunder Index (3:31)
  2025-01-02 07:41  Sleepless Streets - Ruby Fieldnotes (0:01)
  2025-01-02 07:41  Lunar Hours - Kasia Wren (3:29)
Method syntax gives the same result: True

Genres: ambient, classical, electronic, folk, hip-hop, indie, jazz, metal, pop, r&b, rock, synthwave
Jazz artists: Northern Lights Quartet, Blue Hour Trio, Moss & Marble
Jazz listening time: 5 h 26 min

Tracks never played: 26 (Sweet Dreams, Velvet Heart, Distant Oceans, ...)
Artists never played: The Null References
```

> [!NOTE]
> #### Stage 4
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.04" >}}

### Stage 5: Grouping and Aggregation

In this stage, we will write the core statistics of our summary. To make them reusable (in the next stage we will combine them into a report), we will define them as extension members of `IEnumerable<PlayInfo>`.

> [!IMPORTANT]
> #### GroupBy
>
> `GroupBy` splits a sequence into groups of elements with equal keys. Each group is an `IGrouping<TKey, TElement>` – a sequence of elements with an additional `Key` property, so all LINQ operators (and our own extension methods) can be applied to it:
> ```csharp
> var streamsPerArtist = infos
>     .GroupBy(i => i.Artist)
>     .Select(g => (Artist: g.Key, Streams: g.Count()));
> ```
> Records can be used as keys thanks to their value-based equality. In query syntax, grouping is written as `group info by key into g`, after which the query continues with the groups.

> [!IMPORTANT]
> #### CountBy and AggregateBy (.NET 9)
>
> When you only need one number per key, the groups do not have to be created at all. `CountBy` returns the number of elements for each key, and `AggregateBy` computes any aggregate for each key, starting from a seed value. Both return a sequence of `KeyValuePair<TKey, TValue>`:
> ```csharp
> var streamsPerTrack = infos.CountBy(i => i.Track);     // KeyValuePair<Track, int>
> var timePerMonth = infos.AggregateBy(
>     i => i.Play.Timestamp.Month,                       // key
>     TimeSpan.Zero,                                     // seed
>     (total, i) => total + i.Play.Duration);            // KeyValuePair<int, TimeSpan>
> ```

#### The Stats Class

Create a file named `Stats.cs` with a static class `Stats` containing an extension block `extension(IEnumerable<PlayInfo> plays)` with the following members:

1. The property `IEnumerable<PlayInfo> Streams` – the plays that are not skips (`Where`).
2. `IEnumerable<(Artist Artist, int Streams, TimeSpan Time)> TopArtists(int count)` – the artists with the most streams: group `Streams` by artist (`GroupBy`), compute the number of streams and their total time (reuse `TotalListeningTime`), sort descending by the number of streams and then by name, and take `count` elements.
3. `IEnumerable<(Track Track, Artist Artist, int Streams)> TopTracks(int count)` – the tracks with the most streams. Use `CountBy` with a tuple key `(i.Track, i.Artist)`; sort descending by the count and then by title.
4. `IEnumerable<(string Genre, int Streams)> TopGenres(int count)` – the genres with the most streams (`SelectMany` + `CountBy`); sort descending by the count and then by name. A stream of an artist with two genres counts for both of them.
5. `Dictionary<int, TimeSpan> TimeByMonth()` – the listening time in every month, computed with `AggregateBy` (see above) and converted to a dictionary with `ToDictionary()`.
6. `IEnumerable<(int Month, Artist Artist, int Streams)> TopArtistByMonth()` – the most streamed artist of each month, sorted by month. Group the streams by month and reuse `TopArtists` on each group – this is where defining the queries as extension members pays off:
   ```csharp
   from info in plays.Streams
   group info by info.Play.Timestamp.Month into month
   orderby month.Key
   let top = month.TopArtists(1).First()
   select (month.Key, top.Artist, top.Streams);
   ```
7. `ILookup<int, PlayInfo> ByHour()` – the plays grouped by the hour of the day (`ToLookup`).

> [!IMPORTANT]
> #### ToDictionary and ToLookup
>
> Both methods execute the query immediately and build a collection indexed by a key. `ToDictionary` requires unique keys (a duplicate key throws an exception), and the indexer throws `KeyNotFoundException` for a missing key (use `TryGetValue` or `GetValueOrDefault` instead). `ToLookup` allows many elements per key: its indexer returns a sequence of all elements with the given key – **an empty sequence** if there are none. Unlike `GroupBy`, a lookup is computed only once and supports fast access by key.

#### Printing the Statistics

In `Program.cs`, get the joined plays (`var infos = library.GetPlayInfos();`) and print:

1. The top 5 artists (with the number of streams and listening time), the top 5 tracks and the top 3 genres. Positions can be obtained with `Index()`, and the tuples can be deconstructed directly in the loop: `foreach (var (i, (artist, streams, time)) in infos.TopArtists(5).Index())`. Compare the top tracks with the result of Stage 3 – they should be identical.
2. A table of months: the listening time, a bar of `#` characters proportional to the time (the longest bar has 20 characters; dividing a `TimeSpan` by a `TimeSpan` gives a `double`), and the top artist of the month. Convert the result of `TopArtistByMonth()` into a dictionary indexed by month (`ToDictionary(x => x.Month)`).
3. A histogram of plays by the hour of the day for hours 0–23 (the longest bar has 40 characters). Thanks to `ToLookup`, `byHour[hour].Count()` simply returns 0 for hours without plays.

Expected output:

```text
Top 5 artists:
  1. Static Bloom                              430   25 h 36 min
  2. Pixel Monsoon                             232   14 h 33 min
  3. Ada & The Lambdas                         177   10 h 18 min
  4. Lena Marlowe                              174   11 h 18 min
  5. Mira Volta                                154    9 h 55 min
Top 5 tracks:
  1. Winter Letters - Static Bloom             134
  2. Sleepless Roads - Static Bloom             89
  3. Lazy Satellites - Pixel Monsoon            83
  4. Analog Memories - Lena Marlowe             73
  5. Burning Horizons - Concrete Gardens        63
Top genres: electronic (1135), pop (952), indie (821)

Jan  16 h 26 min #################    Static Bloom (38)
Feb  17 h 49 min ###################  Lena Marlowe (41)
Mar  17 h 11 min ##################   Kasia Wren (45)
Apr  15 h 38 min ################     Static Bloom (32)
May  12 h 39 min #############        Concrete Gardens (41)
Jun  17 h 01 min ##################   Static Bloom (42)
Jul  15 h 42 min ################     Lena Marlowe (46)
Aug  14 h 55 min ################     Static Bloom (43)
Sep  16 h 03 min #################    Static Bloom (38)
Oct  16 h 07 min #################    Pixel Monsoon (68)
Nov  18 h 31 min #################### Mira Volta (46)
Dec  12 h 59 min ##############       Static Bloom (30)

00:00 #####                                     66
01:00                                            8
02:00                                            0
03:00                                            0
04:00                                            0
05:00                                            0
06:00                                            0
07:00 ############################             368
08:00 #########################                332
09:00                                            0
10:00 ####                                      61
11:00 ###########                              146
12:00 #####                                     72
13:00 #####                                     75
14:00 #######                                   91
15:00 ###                                       50
16:00 ####                                      61
17:00                                           10
18:00 ######                                    90
19:00 ###########################              356
20:00 ###########################              359
21:00 ###############################          405
22:00 ######################                   286
23:00 ######################################## 515
```

> [!NOTE]
> #### Stage 5
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.05" >}}

### Stage 6: Your Wrapped

In the last stage, we will compose the yearly report out of the pieces written so far and see why it matters *when* a query is executed.

#### One More Iterator: SplitWhen

Listening streaks (consecutive days with music) and listening sessions (plays separated by short gaps) have one thing in common: we need to split a sorted sequence into groups of consecutive elements. In `EnumerableExtensions`, add the method:

```csharp
public static IEnumerable<List<T>> SplitWhen<T>(this IEnumerable<T> source, Func<T, T, bool> startsNewGroup)
```

It collects the elements into a list; before adding an element, it calls `startsNewGroup(last element of the current group, element)`, and if the result is `true`, it returns the current group (`yield return`) and starts a new one. Do not forget to return the last group after the loop (unless the sequence was empty). For example, for `1, 2, 3, 7, 8, 10` and `(a, b) => b - a > 1` it returns `[1, 2, 3]`, `[7, 8]`, `[10]`.

#### More Statistics

Add to the extension block in `Stats`:

1. `(DateOnly First, DateOnly Last) LongestStreak()` – the longest run of consecutive days with at least one play. Get the distinct days of plays, sort them (`Order`), split them with `SplitWhen` whenever the difference of `DayNumber` of neighboring days is greater than 1, and choose the longest group (`MaxBy`).
2. `(DateTime Start, DateTime End, int Plays) LongestSession(TimeSpan maxGap)` – the longest listening session: sort the plays by time and split them whenever the gap between the end of a play and the start of the next one exceeds `maxGap`. The length of a session is the time between the start of its first play and the end of its last play.
3. `(DateTime From, DateTime To) LongestBreak()` – the longest break in listening. This time, implement it as a single LINQ expression: sort the plays, use `Pairwise` from Stage 2, project the pairs into `(From, To)` tuples and choose the longest one with `MaxBy`.
4. `(Track Track, Artist Artist, DateOnly Day, int Streams) OnRepeat()` – the track streamed most times during a single day (`CountBy` with a tuple key `(i.Track, i.Artist, Day)`, then `MaxBy`).

#### The Report

Create a file named `WrappedReport.cs` with a static class `WrappedReport` and the method `string Build(IEnumerable<PlayInfo> history, int year)`. It filters the history to the given year and builds the report text with a `StringBuilder`. The report contains (see the expected output below):

- minutes listened (whole minutes, formatted with `N0`) and the total time,
- the number of streams, and the number of distinct tracks and artists streamed (`DistinctBy`),
- the top 5 artists and the top 5 tracks,
- the top 3 genres,
- the busiest month (the maximum of `TimeByMonth()`; format the month with `{date:MMMM}`),
- the peak hour (the largest group of `ByHour()`) and the listener type, determined with a `switch` expression with relational patterns: 5–11 – *an early bird*, 12–17 – *an afternoon listener*, 18–21 – *an evening listener*, otherwise *a night owl*,
- the longest streak, the longest session (with `maxGap` equal to 15 minutes), the longest break (in days, `TotalDays`) and the track on repeat.

> [!IMPORTANT]
> #### Materialization
>
> *Materialization* means executing a query and storing its results in a collection: `ToList()`, `ToArray()`, `ToDictionary()`, `ToLookup()`, `ToHashSet()`. A materialized collection is a snapshot – enumerating it many times is cheap and always gives the same elements. On the other hand, materializing too early wastes memory, prevents processing infinite sequences and loses the benefits of lazy evaluation (e.g. `First` stopping after the first match). A good rule of thumb: keep queries deferred while composing them, and materialize when the result is going to be enumerated many times.

#### Deferred vs Materialized

Each statistic in the report enumerates its input once, so `Build` enumerates the history about a dozen times. Let us check what this means for a deferred input. In `Program.cs`:

```csharp
// Deferred: every query in the report runs the join again
int created = 0;
IEnumerable<PlayInfo> deferred = library.GetPlayInfos().Tap(_ => created++);
string report = WrappedReport.Build(deferred, 2025);
int createdDeferred = created;

// Materialized: the join runs once, the report reads a list
created = 0;
List<PlayInfo> materialized = library.GetPlayInfos().Tap(_ => created++).ToList();
string sameReport = WrappedReport.Build(materialized, 2025);
```

Print the report, the number of `PlayInfo` objects created by the join in both cases, and whether both reports are identical. With a deferred input, the join – including building its hash tables – is executed again by every statistic. For our data set, it is still fast, but it is easy to imagine a source for which every enumeration means reading a file or querying a database. If you want, measure the time of both variants with `System.Diagnostics.Stopwatch`.

> [!TIP]
> A method that has to enumerate its input many times can protect itself: materialize the argument at the beginning, or declare the parameter as `IReadOnlyCollection<T>`/`IReadOnlyList<T>`, so that the caller has to pass a ready collection. IDEs (e.g. Rider) warn about *possible multiple enumeration* of an `IEnumerable<T>`.

Expected output:

```text
============================================================
                     YOUR 2025 WRAPPED
============================================================
Minutes listened   11,465 (191 h 05 min)
Streams            2,905 streams of 239 tracks by 39 artists

Top artists
  1. Static Bloom                                  430 streams
  2. Pixel Monsoon                                 232 streams
  3. Ada & The Lambdas                             177 streams
  4. Lena Marlowe                                  174 streams
  5. Mira Volta                                    154 streams
Top tracks
  1. Winter Letters - Static Bloom                 134 streams
  2. Sleepless Roads - Static Bloom                 89 streams
  3. Lazy Satellites - Pixel Monsoon                83 streams
  4. Analog Memories - Lena Marlowe                 73 streams
  5. Burning Horizons - Concrete Gardens            63 streams

Top genres         electronic, pop, indie
Busiest month      November (18 h 31 min)
Listening clock    peak at 23:00 - you are a night owl
Longest streak     17 days (2025-09-13 - 2025-09-29)
Longest session    2 h 07 min on 2025-10-11 (34 plays)
Longest break      10.5 days (2025-08-03 - 2025-08-14)
On repeat          Paper Bridges - Lena Marlowe, 5 times on 2025-02-09
============================================================

PlayInfo objects created: deferred 43,563, materialized 3,351
Identical reports: True
```

> [!NOTE]
> #### Stage 6
> {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Wrapped.06" >}}

### Next Steps and Further Enhancements

The application built in this tutorial can be extended in many directions. Below are some ideas worth exploring:

- **More statistics:** The completion rate of tracks (`Play.Duration` compared with `Track.Length`) and the most skipped track, the first play of each artist (*discoveries* of the year), or the most intense 7-day period computed with a sliding `Window` iterator.
- **Real data:** Streaming services allow you to download your own listening history (usually as JSON files). Load it with `System.Text.Json` and generate your own Wrapped.
- **Streaming large files:** Instead of loading all data into memory, read the file lazily with an iterator (`File.ReadLines` + `yield return`) or asynchronously with `IAsyncEnumerable<T>` – since .NET 10, LINQ operators for asynchronous sequences are built into the framework (`System.Linq.AsyncEnumerable`).
- **Unit tests:** Cover the iterators with tests (see [Tutorial 2]({{< ref "/labs/02.msbuild" >}})), especially the edge cases: empty sequences, sequences with one element, and argument validation.
- **PLINQ:** Parallelize the queries with `AsParallel()` and measure whether it pays off for a data set of this size.
- **`IQueryable<T>` and expression trees:** The same LINQ queries written against a database with Entity Framework Core are not executed in memory – they are translated into SQL. Lambdas are then compiled into expression trees (`Expression<Func<T, bool>>`) instead of delegates.
{{% /details %}}

## Example Tasks
Complete an example task from previous years. If you are able to finish it within 90 minutes, you are well-prepared for the class.
- [Task 1]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example1" >}})
- [Task 2]({{< ref "/labs/05.lambdas-enumerable-linq-extensions/example2" >}})