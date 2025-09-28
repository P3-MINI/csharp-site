---
title: "String"
weight: 30
---

## Typ `string`

Typ `string` jest aliasem dla klasy `System.String`. Jest to kontener przechowujący ciągłą tablicę znaków. Tak jak i `char` typ `string` jest kodowany za pomocą UTF-16. W przeciwieństwie do C++ obiekt typu `string` jest niezmienialny (*immutable*). Dzięki temu *runtime* może stosować różne sztuczki do optymalizacji w okół tego typu. Operacje, które wydają się, że ten obiekt zmieniają, tak naprawdę tworzą nowy obiekt typu `string` w pamięci. Typ `string` jest typem referencyjnym, ale porównanie za pomocą operatora zachowuje się tak, jakby był to typ bezpośredni.

```csharp
string txt = "Hello, ";
string txt2 = txt + "World!"; // "Hello, World!"

// txt2[5] = '?'; // BŁĄD: String jest niezmienny!
string txt3 = txt2.Replace(',', '?'); // "Hello? World!"

string option = "first";
if (option == "first")
{
   // ...
}

Console.WriteLine(txt2.ToUpper()); // "HELLO, WORLD!"
```

## Konstruktory i właściwości

Stringa można stworzyć albo za pomocą literału, np. `"Hello"`, albo za pomocą konstruktora klasy. Tak jak w każdym cywilizowanym języku stringi można ze sobą dodawać, można je indeksować. Właściwość indeksowania jest jednak tylko do odczytu. Dodatkowo posiada on właściwość `Length`, zwracającą jego długość.

```csharp
char[] chars = new {'w', 'o', 'r', 'l', 'd'};
string fromLiteral = "Hello";
string fromArray = new string(chars);
string fromSubArray = new string(chars, 1, 2);
string repeatedChar = new string(' ', 4);
string concatenated = fromLiteral + ' ' + fromArray;
Console.WriteLine(concatenated);
Console.WriteLine("string Length: " + concatenated.Length);
char space = concatenated[5];
concatenated[5] = '_'; // compilation error, read-only
```

## Modyfikowalne stringi

Jako, że stringi są niezmienialne, każda operacja modyfikacji (np. dodawanie nowego tekstu za pomocą operatora + w pętli) powoduje: powstanie nowego obiektu; skopiowanie starej zawartości stringa i konkatenacja; pozostawienie starego obiektu do usunięcia przez *Garbage Collector*.

Klasa `StringBuilder` z przestrzeni nazw `System.Text` jest zmienną (mutable) wersją stringa i używa wewnętrznego, dynamicznie rozszerzanego bufora. Dzięki temu, dodawanie tekstu za pomocą metod `Append`, `Remove`, `Insert`, `Replace` modyfikuje istniejący obiekt w buforze, zamiast tworzyć setki (lub więcej) nowych obiektów (**szczególnie istotne gdy stringi są modyfikowane w pętli**).

Po skończeniu pracy ze `StringBuilder`em możemy z niego pobrać zwykłego `string`a wywołując na nim `ToString()`.

```csharp
StringBuilder stringBuilder = new StringBuilder("Hello, ");
stringBuilder.Append("this is ");
stringBuilder.Append("a simple ");
stringBuilder.Append("StringBuilder demo.");
Console.WriteLine(stringBuilder.ToString());
```

## Dostarczane metody

Biblioteka standardowa dostarcza razem z implementacją `string`a całą masę metod do operowania na nim.

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

> Pełen zbiór metod do przeglądu w [dokumentacji](https://learn.microsoft.com/en-us/dotnet/api/system.string).

## Dosłowne literały (*Verbatim string literals*)

Dosłowne literały (*Verbatim string literals*) pozwalają zapisać stringa bez potrzeby uciekania sekwencji specjalnych: nowych linii, slashy, itp. Jedynym wyjątkiem jest `"`, który uciekamy wtedy podwójnym znakiem `""`. Szczególnie przydatne do zapisywania ścieżek ze znakiem `\`.

```csharp
string filePath = @"C:\Users\scoleridge\Documents\";
//Output: C:\Users\scoleridge\Documents\
string text = @"My pensive SARA ! thy soft cheek reclined
Thus on mine arm, most soothing sweet it is
To sit beside our Cot,...";
/* Output:
My pensive SARA ! thy soft cheek reclined
Thus on mine arm, most soothing sweet it is
To sit beside our Cot,...*/
string quote = @"Her name was ""Sara.""";
//Output: Her name was "Sara."
```

## Surowe literały (*Raw string literals*) (C# 11)

Surowe literały zaczynają się i kończą trzema lub więcej znakami `"`. Występują w wersji jedno i wielolinijkowej. W nich nie trzeba uciekać sekwencji specjalnych jeszcze bardziej. Dozwolone są kolejne znaki `"`, o ile jest ich mniej niż w otwierających i zamykających ciapkach. W wielolinijkowej wersji białe znaki po otwarciu stringa są ignorowane do nowej lini, natomiast białe znaki poprzedzające zamknięcie są usuwane z każdej lini stringa. Szczególnie przydatne, gdy chcemy utrzymać ładnie sformatowany kod wewnątrz stringa.

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
           """Raw string literals""" can start
           and end with more than three
           double-quotes when needed.
           """";
```

## Interpolacja stringów (*String interpolation*)

Interpolacja stringów pozwala na umieszczanie zmiennych, wyrażeń lub wartości wewnątrz literału stringa bez konieczności stosowania konkatenacji (`+`) czy używania metody `string.Format()`. Interpolowany `string` jest poprzedzony znakiem `$`. Wszelkie zmienne, wyrażenia lub wartości, które mają zostać wstawione do stringa, umieszcza się w nawiasach klamrowych. Opcjonalnie po wyrażeniu można po przecinku podać wyrównanie, a po dwukropku specyfikator formatu. Liczba dodatnia wyrównuje do prawej, ujemna do lewej. O formatowaniu można doczytać w (dokumentacji)[https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types].

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

> Można łączyć interpolację stringów z innymi stylami literałów. Można interpolować literały dosłowne `$@"..."` oraz surowe `$"""..."""`.
