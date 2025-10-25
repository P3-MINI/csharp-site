---
title: "String"
weight: 30
---

## The `string` Type

The `string` type is an alias for the `System.String` class. It is a container that stores a contiguous array of characters. Like `char`, the `string` type is encoded using UTF-16. Unlike in C++, a `string` object is *immutable*. This allows the runtime to apply various optimization tricks around this type. Operations that seem to modify the object actually create a new `string` object in memory. The `string` type is a reference type, but comparison using the `==` operator behaves as if it were a value type.

```csharp
string txt = "Hello, ";
string txt2 = txt + "World!"; // "Hello, World!"

// txt2[5] = '?'; // ERROR: String is immutable!
string txt3 = txt2.Replace(',', '?'); // "Hello? World!"

string option = "first";
if (option == "first")
{
   // ...
}

Console.WriteLine(txt2.ToUpper()); // "HELLO, WORLD!"
```

## Constructors and Properties

A string can be created either with a literal, e.g., `"Hello"`, or using the class constructor. As in any civilized language, strings can be concatenated and indexed. However, the indexing property is read-only. Additionally, it has a `Length` property that returns its length.

```csharp
char[] chars = new[] {'w', 'o', 'r', 'l', 'd'};
string fromLiteral = "Hello";
string fromArray = new string(chars);
string fromSubArray = new string(chars, 1, 2); // "or"
string repeatedChar = new string(' ', 4);
string concatenated = fromLiteral + ' ' + fromArray;
Console.WriteLine(concatenated);
Console.WriteLine("string Length: " + concatenated.Length);
char space = concatenated[5];
// concatenated[5] = '_'; // compilation error, read-only
```

## Mutable Strings

Since strings are immutable, every modification operation (e.g., adding new text using the `+` operator in a loop) results in: the creation of a new object; copying the old string's content and concatenation; leaving the old object to be removed by the *Garbage Collector*.

The `StringBuilder` class from the `System.Text` namespace is a mutable version of a string and uses an internal, dynamically expanding buffer. Thanks to this, adding text using the `Append`, `Remove`, `Insert`, `Replace` methods modifies the existing object in the buffer, instead of creating hundreds (or more) of new objects (**especially important when strings are modified in a loop**).

After finishing work with a `StringBuilder`, we can get a regular `string` from it by calling `ToString()`.

```csharp
StringBuilder stringBuilder = new StringBuilder("Hello, ");
stringBuilder.Append("this is ");
stringBuilder.Append("a simple ");
stringBuilder.Append("StringBuilder demo.");
Console.WriteLine(stringBuilder.ToString());
```

## Provided Methods

The standard library provides a whole host of methods for operating on `string` along with its implementation.

```csharp
string example = "Showcasing C# strings";
string sub = example.Substring(11, 2);
Console.WriteLine($"Substring: {sub}");
bool contains = example.Contains("C#");
Console.WriteLine($"Contains 'C#': {contains}");
string replaced = example.Replace("Showcasing", "Demo of");
Console.WriteLine($"Replace: {replaced}");
string upper = example.ToUpper();
Console.WriteLine($"Uppercase: {upper}");
string[] words = example.Split(' ');
Console.WriteLine("Split:");
foreach (string word in words)
{
    Console.WriteLine(word);
}
string joined = string.Join(", ", words);
Console.WriteLine($"Join: {joined}");
```

> A full set of methods can be reviewed in the [documentation](https://learn.microsoft.com/en-us/dotnet/api/system.string).

## Verbatim String Literals

Verbatim string literals allow you to write a string without needing to escape special sequences: newlines, slashes, etc. The only exception is `"`, which is then escaped with a double `""` character. This is particularly useful for writing paths with the `\` character.

```csharp
string filePath = @"C:\Users\scoleridge\Documents\";
//Output: C:\Users\scoleridge\Documents\
string text = @"My pensive SARA ! thy soft cheek reclined
Thus on mine arm, most soothing sweet it is
To sit beside our Cot,...";
/* Output:
My pensive SARA ! thy soft cheek reclined
Thus on mine arm, most soothing sweet it is
To sit beside our Cot,...
*/
string quote = @"Her name was ""Sara.""";
//Output: Her name was "Sara."
```

## Raw String Literals (C# 11)

Raw string literals begin and end with three or more `"` characters. They come in single-line and multi-line versions. In them, you don't need to escape special sequences even more. Subsequent `"` characters are allowed, as long as there are fewer of them than in the opening and closing quotes. In the multi-line version, whitespace after the opening of the string is ignored up to the new line, while whitespace preceding the closing is removed from every line of the string. This is particularly useful when you want to maintain nicely formatted code inside a string.

```csharp
var str1 = """This is a "raw string literal".""";
var str2 = """It can contain characters like \, ' and ".""";
var xml = """
          <element attr="content">
              <body>
              </body>
          </element>
          """;
var str3 = """"
           \'''Raw string literals\''' can start
           and end with more than three
           double-quotes when needed.
           """";
```

## String Interpolation

String interpolation allows you to embed variables, expressions, or values inside a string literal without having to use concatenation (`+`) or the `string.Format()` method. An interpolated `string` is prefixed with a `$` sign. Any variables, expressions, or values to be inserted into the string are placed in curly braces. Optionally, after the expression, you can specify alignment with a comma, and a format specifier after a colon. A positive number aligns to the right, a negative number to the left. You can read more about formatting in the [documentation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types).

```csharp
string author = "George Orwell";
string book = "Nineteen Eighty-Four";
int year = 1949;
decimal price = 19.50m;
string description = $"{author} is the author of {book}. \n" +
                     $"The book price is {price:C}, it was published in {year}.";
Console.WriteLine(description);
Console.WriteLine($"Number 1: {1.0,10:C}");
Console.WriteLine($"Number 2: {12.5,10:C}");
Console.WriteLine($"Number 3: {123.45m,10:C}");
var random = new Random();
Console.WriteLine($"Coin flip: {(random.NextDouble() < 0.5 ? "heads" : "tails")}");
```

> You can combine string interpolation with other literal styles. You can interpolate verbatim literals `$@"..."` and raw literals `$"""..."""`.
