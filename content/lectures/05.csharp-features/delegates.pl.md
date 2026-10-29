---
title: "Delegaty"
---

## Delegaty

Delegat to obiekt, który wie jak wywołać metodę - jest to bezpieczny odpowiednik wskaźnika na funkcję z C/C++.

Typ delegata deklarujemy w następujący sposób:

```csharp
delegate double Function(double x);
```

Parametry i typ zwracany delegata określają jakie funkcje delegat może wywoływać. Tak zdefiniowany delegat będzie kompatybilny z metodami, które przyjmują parametr `double` i zwracają `double`. Na przykład:

```csharp
double QuadraticFunction(double x) => x * x - 2 * x + 1;
```

Instancję obiektu delegata można stworzyć przypisując kompatybilną metodę do zmiennej typu delegacji.

```csharp
Function quadratic = QuadraticFunction;
```

> [!NOTE]
> Wszystkie delegaty niejawnie dziedziczą po klasie `System.Delegate`. Przechowują w środku wskazanie na metodę, którą wywołują i opcjonalnie referencję obiektu na rzecz którego wywołują metodę, jeżeli metoda nie jest statyczna. Warto o tym pamiętać, ponieważ delegat przedłuża przez to czas życia obiektów, a nawet może powodować wycieki pamięci.

Delegata można wywołać tak samo jak metodę:

```csharp
double y = quadratic(2.0);
```

> Jest to równoważne wywołaniu `quadratic.Invoke(2.0)`

Pełny przykład wykorzystania delegatów:

```csharp
// Numerics.cs
public static class Numerics
{
    public delegate double Function(double x);

    public static double NewtonRootFinding(Function f, Function df, double x0 = 0, double eps = 1e-6)
    {
        double x;
        double xn = x0;

        do
        {
            x = xn;
            xn = x - f(x) / df(x);
        } while (Math.Abs(x - xn) >= eps);

        return xn;
    }
}

// Quadratic.cs
public class Quadratic
{
    public double A { get; }
    public double B { get; }
    public double C { get; }

    public Quadratic(double a, double b, double c)
    {
        A = a;
        B = b;
        C = c;
    }

    public double Function(double x) => A * x * x + B * x + C;

    public double Derivative(double x) => 2 * A * x + B;

    public override string ToString() => $"f(x) = {A}x^2 + {B}x + {C}";
}

// Program.cs
public static class Program
{
    public static void Main()
    {
        var quadratic = new Quadratic(1.0, -7.0, 10.0);

        Numerics.Function function = quadratic.Function;
        Numerics.Function derivative = quadratic.Derivative;

        double root = Numerics.NewtonRootFinding(function, derivative);

        Console.WriteLine($"Root of {quadratic}: {root:F2}");
    }
}
```

> [!NOTE]
> **Kod źródłowy**
> {{< filetree dir="lectures/csharp-features/delegates/Numerics/" >}}

### Generyczne delegaty

Typ Delegata może być generyczny:

```csharp
Function<double> function = QuadraticFunction;
double result = function(3);
Console.WriteLine(result);

// Compatible method:
double QuadraticFunction(double x) => x * x - 2 * x + 1;

// Delegate type declaration:
delegate T Function<T>(T x);
```

### Systemowe delegaty

Biblioteka standardowa dostarcza dwa rodzaje generycznych delegatów: `Func` i `Action`. **Nie ma potrzeby definiowania własnych typów delegatów**, te wbudowane są na tyle ogólne, że można za ich pomocą przedstawić dowolnego delegata.

```csharp
delegate TResult Func<out TResult>();
delegate TResult Func<in T, out TResult>(T arg);
delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
// ... and so on, up to T16
delegate void Action();
delegate void Action<in T>(T arg);
delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
// ... and so on, up to T16
```

> [!NOTE]
> Parametry generyczne delegatów można oznaczać `in` (wejściowe) i `out` (wyjściowe) tak samo jak parametry generyczne w interfejsach. Ma to podobne znaczenie. Takie parametry generyczne można dzięki temu rzutować w dół (`in`) lub w górę (`out`).

### Delegaty 'multicast' (*Multicast Delegates*)

Delegaty mogą przechowywać w sobie wskazania na kilka metod. Operatory `+=` i `-=` pozwalają do delegata dodawać i usuwać metody. Wywołanie takiego delegata spowoduje wywołanie wszystkich przechowywanych w nim metod w kolejności dodania. Takie delegaty dziedziczą niejawnie po `System.MulticastDelegate`, które dziedziczy po zwykłym `System.Delegate`.

```csharp
Action<string> writeLog = Console.WriteLine;
writeLog += WriteLogToFile;

writeLog("DEBUG: This is a test entry");

private static void WriteLogToFile(string log)
{
    File.AppendAllText("test.log", $"{log}\n");
}
```

Zadziała również użycie operatora `+=` na delegacie, który jest `null`em. Będzie to równoważne z przypisaniem do delegata wartości.

```csharp
Action<string>? writeLog = null;
writeLog += Console.WriteLine;

writeLog?.Invoke("DEBUG: This is a test entry");
```

> Jeżeli delegat zwraca jakieś wartości to zwracana jest wartość z ostatniej metody na liście wywołań.

## Wyrażenia lambda

Wyrażenie lambda to nienazwana metoda, którą możemy przypisać do kompatybilnego typu delegata.

```csharp    
Numerics.Function function = (x) => x * x - 2 * x + 1;
Numerics.Function derivative = (x) => 2 * x - 2;

double root = Numerics.NewtonRootFinding(function, derivative);
```

Ogólnie składnia lambdy wygląda następująco:

```csharp
(parameters) => expression-or-statement-block
```

```csharp
Func<double, double> square = x => x * x;
Func<char, int, string> repeat = (c, i) => new string(c, i);
Action<string> write = str => Console.Write(str);
Func<string> greet = () => { return "Hello, world"; };
```

- Jeżeli lambda ma tylko jeden parametr możemy pominąć nawiasy
- Jeżeli lambda ma tylko jedno wyrażenie możemy użyć skróconego zapisu i pominąć słówko `return`
- Kompilator sam jest w stanie wydedukować typy parametrów i wartości zwracanej lambdy na podstawie typu lewej strony

Możemy również jawnie sprecyzować typy parametrów oraz, od C# 10, typ zwracany (zapisywany przed listą parametrów). Czasami zwiększa to po prostu czytelność kodu. Jawne typy parametrów pozwalają też kompilatorowi samodzielnie wywnioskować typ delegata, dzięki czemu możemy użyć słowa kluczowego `var` po lewej stronie przypisania.

```csharp
var square = double (double x) => x * x;
var repeat = string (char c, int i) => new string(c, i);
var write = void (string str) => Console.Write(str);
var greet = string () => { return "Hello, world"; };
```

### Typ naturalny (C# 10)

Od C# 10 lambda z jawnie określonymi typami parametrów (lub bez parametrów) ma *typ naturalny* (*natural type*): kompilator dobiera pasujący delegat `Func` lub `Action` (albo syntetyzuje anonimowy typ delegata, jeśli żaden nie pasuje, np. dla parametrów `ref`). Typ zwracany jest wnioskowany na podstawie ciała lambdy – trzeba go podać jawnie tylko wtedy, gdy nie da się go wywnioskować. Lambda z niejawnie typowanymi parametrami nie ma typu naturalnego, więc potrzebuje docelowego typu delegata (np. `Func<int, int>` po lewej stronie). Typ naturalny mają również grupy metod, o ile wskazują na jedną metodę.

```csharp
var cube = (double x) => x * x * x;            // Func<double, double>
var choose = object (bool b) => b ? 1 : "one"; // Func<bool, object>
var read = Console.ReadLine;                   // Func<string?>
Delegate d = (int x) => x + 1;                 // Func<int, int>

var identity = x => x;         // Error CS8917: The delegate type could not be inferred
var print = Console.WriteLine; // Error CS8917 (WriteLine has many overloads)
```

Również od C# 10 lambdy mogą mieć atrybuty – na samej lambdzie, na jej parametrach i na wartości zwracanej (lista parametrów musi wtedy być w nawiasach). Korzystają z nich głównie frameworki, które analizują delegaty za pomocą refleksji, np. *minimal API* w ASP.NET Core.

```csharp
using System.ComponentModel;
using System.Reflection;

var greet = [Description("Greets a person")] (string name) => $"Hello, {name}!";

var attribute = greet.Method.GetCustomAttribute<DescriptionAttribute>();
Console.WriteLine(attribute?.Description); // Greets a person
```

### Parametry domyślne i `params` (C# 12)

Od C# 12 parametry lambdy mogą przyjmować wartości domyślne, a ostatni parametr może być tablicą `params`:

```csharp
var write = (string str = "hello") => Console.WriteLine(str);
write();
write("world");

var sum = (params int[] values) => values.Sum();
Console.WriteLine(sum(1, 2, 3)); // 6
```

Takich lambd nie da się przedstawić za pomocą `Func` ani `Action`, więc ich typem naturalnym jest typ delegata wygenerowany przez kompilator.

### Modyfikatory parametrów bez typów (C# 14)

Parametry z modyfikatorami takimi jak `ref`, `out`, `in`, `ref readonly` czy `scoped` wymagały dotąd jawnego podania typu. Od C# 14 typy takich parametrów również mogą być wywnioskowane na podstawie docelowego typu delegata:

```csharp
TryParse<int> parse = (text, out result) => int.TryParse(text, out result);
// Before C# 14: (string text, out int result) => int.TryParse(text, out result)

if (parse("42", out int number))
{
    Console.WriteLine(number); // 42
}

delegate bool TryParse<T>(string text, out T result);
```

### Przechwytywanie zmiennych

W wyrażeniach lambda możemy odwoływać się do zmiennych na zewnątrz. Mówimy że takie zmienne sa przechwytywane. To co się dzieje kiedy przechwytujemy zmienną, to kompilator generuje specjalną klasę w której przechowywane są przechwycone zmienne. Jeśli wiele lambd odwołuje się do tego samego pola, to wszystkie będą się odwoływać do tej samej instancji wygenerowanej klasy i tego samego pola. Przechwycone zmienne żyją na stosie jako część wygenerowanej klasy. Należy pamiętać, że przechwytywanie zmiennych wiąże się z alokacją obiektu na stercie.

Zmienna `i` zostanie przechwycona i zostanie umieszczona na stercie, będzie współdzielona przez wszystkie lambdy:

```csharp
Action[] actions = new Action[3];
for (int i = 0; i < 3; i++)
{
    actions[i] = () => Console.Write(i);
}
foreach(Action action in actions) 
{
    action();
}
```

{{% details title="Kompilator wygeneruje następujący kod:" open=false %}}
```csharp
[CompilerGenerated]
private sealed class <>c__DisplayClass0_0
{
    public int i;

    internal void <<Main>$>b__0()
    {
        Console.Write(i);
    }
}

private static void <Main>$(string[] args)
{
    Action[] array = new Action[3];
    <>c__DisplayClass0_0 <>c__DisplayClass0_ = new <>c__DisplayClass0_0();
    <>c__DisplayClass0_.i = 0;
    while (<>c__DisplayClass0_.i < 3)
    {
        array[<>c__DisplayClass0_.i] = new Action(<>c__DisplayClass0_.<<Main>$>b__0);
        <>c__DisplayClass0_.i++;
    }
    Action[] array2 = array;
    int num = 0;
    while (num < array2.Length)
    {
        Action action = array2[num];
        action();
        num++;
    }
}
```
{{% /details %}}

Jeżeli lambda nie powinna niczego przechwytywać (np. aby uniknąć przypadkowych alokacji na stercie), możemy oznaczyć ją modyfikatorem `static` (C# 9). Odwołanie się w takiej lambdzie do zmiennej lokalnej, parametru lub `this` jest błędem kompilacji:

```csharp
int factor = 3;
Func<int, int> triple = static x => x * 3;
Func<int, int> multiply = static x => x * factor; // Error CS8820: A static anonymous function cannot contain a reference to 'factor'
```

## Metody anonimowe

Metody anonimowe zostały wprowadzone w C# 2.0 i zostały całkowicie wyparte przez wyrażenia lambda, wprowadzone w następnej wersji. Nie ma żadnych korzyści z korzystania z metod anonimowych zamiast z wyrażeń lambda. Wszelkie udogodnienia w następnych wersjach języka dotykały wyłącznie wyrażeń lambda. Nie uświadczymy tu żadnych wodotrysków. W przeciwieństwie do wyrażeń lambda, treść metody anonimowej musi być zawsze blokiem kodu. Można natomiast pominąć listę parametrów, jeśli nie są używane.

```csharp
Numerics.Function function = delegate (double x) { return x * x - 2 * x + 1; };
Numerics.Function derivative = delegate (double x) { return 2 * x - 2; };

double root = Numerics.NewtonRootFinding(function, derivative);
```

Jedyną przydatną funkcją metod anonimowych jest możliwość stworzenia pustej metody z pominięciem parametrów, taką metodę anonimową możemy przypisać do dowolnego delegata nie zwracającego wartości. Jest to alternatywa od inicjowania delegata `null`em, dzięki temu unikamy wyjątków `NullReferenceException` przy próbie wywołania 'pustego' delegata.

```csharp
Action<string> writeLog = delegate {};

writeLog("DEBUG: This is a test entry");
```
