---
title: "Lab03"
weight: 10
---

# Laboratory 3 - Example - Classes/Structures, Inheritance, Properties, Access Modifiers

## Introduction

The task for today's laboratory is to create a simple library responsible for matrix operations.

### Note

To proceed to the next stages, you must uncomment the individual lines like `#define STAGE0X` located at the beginning of the `Program.cs` file. No other interference with this file is allowed.

## Starting code

> [!NOTE]
> **Matrices**
> {{< filetree dir="labs/lab03/student/Matrices" >}}

## Stage 1 (4 points)

In the `Matrices` folder, create an abstract class `Matrix` that will have:
 - a constructor that takes the number of rows and columns of the matrix as `int`
 - a public property `Rows`, which returns the number of rows in the matrix
 - a public property `Columns`, which returns the number of columns in the matrix
 - an abstract indexer that takes a row and column number. It allows reading and writing a `float` value to a specific cell of the matrix.
 - an overridden `ToString` method, so that matrices are printed in the same way as in the sample program output

Create the following classes that inherit from `Matrix`: `DenseMatrix` and `SparseMatrix`.

`DenseMatrix` should store elements in the form of a rectangular array.

`SparseMatrix` should store only the non-zero elements of the matrix in a dictionary. This implementation reduces the amount of memory needed when storing sparse matrices, i.e., those in which a significant number of values are zero.

Both classes should have a constructor that accepts the number of rows and columns. Newly initialized matrices should be filled with zeros.

The program should check the validity of the arguments. In case of invalidity, it should throw an `ArgumentException` with an appropriate comment.

### Hints
 - [Microsoft Learn: Indexers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/?redirectedfrom=MSDN)
 - [Microsoft Learn: StringBuilder](https://learn.microsoft.com/pl-pl/dotnet/api/system.text.stringbuilder?view=net-8.0)
 - [Microsoft Learn: String interpolation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)
 - [Microsoft Learn: Standard numeric format strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings?redirectedfrom=MSDN)
  - [Microsoft Learn: Create and throw exceptions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)


 ### Scoring
 2 points for each derived class. 


## Stage 2 (3 points)

To the `Matrix` class, add abstract methods `Identity`, `Transpose`, and `Norm`. Implement them in the derived classes.

The `Identity` method takes nothing and returns nothing. It only fills the matrix to be an identity matrix. In the case of a non-square matrix, the method should throw an `InvalidOperationException`.

The `Transpose` method returns the transposed matrix.

The `Norm` method returns the norm of the matrix defined as
{{< katex display=true >}}
||A|| = \sqrt{\sum_i \sum_j A_{i,j}^2}.
{{< /katex >}}

Implementations in the `SparseMatrix` class should have a linear time complexity with respect to the number of non-zero elements of the matrix.


### Scoring
1 point for each method.

## Stage 3 (3 points)

To the `Matrix` class, add an abstract method `GetInstance` that takes the number of rows and columns and returns an object of type `Matrix`. This method should not be callable from outside this class and its derived classes. Implement this method in the derived classes in such a way that each class returns an instance of its own class with the requested size.

In the `Matrix` class, implement the following operator overloads:
 - matrix addition operator
 - matrix subtraction operator
 - matrix multiplication operator
 - matrix-scalar multiplication operator

In the implementation, use the `GetInstance` method. For functions that take one matrix, it should return a matrix of the same type. For functions that take two matrices, the returned implementation should be the same as the left (first) argument.

### Hints
- [Microsoft Learn: Operator overloading](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading)

### Scoring
1 for the `GetInstance` method, 0.5 points for each overload. 

## Stage 4 (2 points)

In the final stage, we will use the interface we built during the previous stages. You need to create a `MatrixAlgorithms` class, and in it, a static method `PseudoInverse` that takes a `Matrix` and a maximum number of iterations, which has a default value of 1000. Use {{< katex >}}10^{-7}{{< /katex >}} for the value of {{< katex >}}\epsilon{{< /katex >}}.

For the purpose of this lab, we will only deal with square matrices.

Algorithm:

 > {{< katex >}}A{{< /katex >}} - input square matrix
 >
 > {{< katex >}}I{{< /katex >}} - identity matrix
 >
 >
 > {{< katex >}}\alpha = \frac{1}{||A||^2}{{< /katex >}}  
 > {{< katex >}}A1 = \alpha \cdot A^T{{< /katex >}}  
 > {{< katex >}}\text{for } i=1...maxIter:{{< /katex >}}  
 > {{< katex >}}\qquad A2 = A1 + A1 \cdot (I - A\cdot A1){{< /katex >}}  
 > {{< katex >}}\qquad \text{if } ||A2-A1|| < \epsilon:{{< /katex >}}  
 > {{< katex >}}\qquad \qquad \text{break}{{< /katex >}}  
 > {{< katex >}}\qquad A1 = A2{{< /katex >}}  
 > {{< katex >}}\text{return A2}{{< /katex >}}  

### Scoring
Correct implementation of the algorithm - 2 points.
Partial implementation - 1 point

## Example solution

> [!NOTE]
> **Matrices**
> {{< filetree dir="labs/lab03/solution/Matrices" >}}
