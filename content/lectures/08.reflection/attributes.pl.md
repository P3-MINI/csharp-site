---
title: "Atrybuty"
weight: 10
---

# Atrybuty

Atrybuty to "etykiety", które można doczepiać do różnych elementów kodu: klas, metod, właściwości, pól, parametrów itd.

Atrybuty przechowują dodatkowe informacje (czyli metadane) o elemencie do którego są przyczepione.

Atrybuty same w sobie nic nie robią. Jest to pasywna informacja. Narzędzia, kompilator, lub sam program mogą te atrybuty odczytywać i na ich podstawie podejmować jakieś działania.

Atrybuty przyczepiamy przez wylistowanie ich w `[]` przed wybranym elementem:

```csharp
[Obsolete("Use iterator `GetFibonacciIter` method instead.")]
public static int GetFibonacci(int n)
{
    if (n < 0) throw new ArgumentException(nameof(n), "Input must be a non-negative integer.");
    if (n is 0 or 1) return 1;
    return GetFibonacci(n - 1) + GetFibonacci(n - 2);
}
```

> [!INFO]
> Kompilator przy próbie użycia metody oznaczonej `Obsolete` wygeneruje ostrzeżenie `warning CS0618: 'FibUtils.GetFibonacci(int)' is obsolete: 'Use iterator method instead.'`

Atrybut sam w sobie jest klasą. Definicja systemowego atrybutu `Obsolete` wygląda następująco:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
public sealed class ObsoleteAttribute : Attribute
{
  public ObsoleteAttribute();
  public ObsoleteAttribute(string? message);
  public ObsoleteAttribute(string? message, bool error);

  public string? DiagnosticId { get; set; }
  public bool IsError { get; }
  public string? Message { get; }
  public string? UrlFormat { get; set; }
}

```

Atrybuty dziedziczą po `System.Attribute`, i zwyczajowo ich nazwa kończy się `...Attribute` (posługując się atrybutem możemy pomijać ten suffix).

## Parametry atrybutów

Wartości do atrybutów można przekazywać na dwa sposoby:

* **Parametry Pozycyjne**: Odpowiadają parametrom konstruktora klasy atrybutu. Są one przekazywane w nawiasach `()` po nazwie atrybutu, w tej samej kolejności co w sygnaturze konstruktora.
* **Parametry Nazwane**: Odpowiadają publicznym właściwościom lub polom klasy atrybutu. Są one opcjonalne i można je przypisać po parametrach pozycyjnych, używając składni `Nazwa = wartość`.

Jeżeli atrybut nie posiada parametrów, możemy pominąć `()`.

Na przykład, dla atrybutu `Obsolete`:

```csharp
[Obsolete]
public void OldMethod() { }

[Obsolete("This method is deprecated.")]
public void VeryOldMethod() { }

[Obsolete("This method is deprecated. See documentation.", UrlFormat = "https://csharp.mini.pw.edu.pl/")]
public void AnotherVeryOldMethod() { }
```

> [!WARNING]
> **Argumenty atrybutów muszą być stałymi czasu kompilacji.**

## Przyczepianie atrybutu

Atrybut można przyczepić do praktycznie dowolnego elementu kodu: 

- Assembly
- Modułu
- Klasy
- Struktury
- Interfejsu
- Typu wyliczeniowego (*enum*)
- Delegata
- Konstruktora
- Metody
- Właściwości
- Pola
- Zdarzenia
- Wartości zwracanej
- Parametru generycznego

### Przykłady przyczepiania atrybutów

Czasami żeby przyczepić atrybut do tego czego chcemy, należy jawnie podać cel atrybutu.

```csharp
[assembly: Description("Applied to an assembly")]
[module: Description("Applied to a module")]

[Description("Applied to a class")]
public class Stack<[Description("Applied to a generic parameter")] T>
{
    [Description("Applied to a field")]
    private T[] _items = new T[8];
    [field: Description("Applied to a backing field")]
    [Description("Applied to a property")]
    public int Count { get; private set; }

    [Description("Applied to a method")]
    public void Push([Description("Applied to a parameter")] T item)
    {
        if (_items.Length == Count)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[Count++] = item;
    }

    [return: Description("Applied to a return value")]
    [method: Description("Implicitly applied to a method")]
    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return _items[--Count];
    }
}
```

### Przypinanie wielu atrybutów:

Do jednego elementu możemy przypiąć wiele atrybutów. Można to zrobić na dwa sposoby, w jednym zestawie `[]`:

```csharp
[Description("Multiple attributes applied to a method"), Conditional("DEBUG")]
private static void Log(string message) 
{ 
    Console.WriteLine($"{DateTime.Now}: {message}");
}
```

Lub otwierając nowy zestaw `[]`:

```csharp
[Description("Multiple attributes applied to a method")]
[Conditional("DEBUG")]
private static void Log(string message) 
{ 
    Console.WriteLine($"{DateTime.Now}: {message}");
}
```

> [!INFO]
> Atrybut `Conditional` warunkowo włącza lub wyłącza wywołania metody. Jeśli symbol kompilacji (np. `DEBUG`) nie jest zdefiniowany, to wszystkie wywołania metody oznaczonej tym atrybutem (wraz z jej argumentami) zostaną całkowicie usunięte ze skompilowanego kodu. Metoda nie może nic zwracać.

## Atrybuty 'Caller Info'

Atrybuty *Caller Info* to specjalny zestaw atrybutów, które pozwalają na automatyczne pobieranie informacji o kodzie, który wywołał daną metodę. Atrybuty *Caller Info* stosuje się do opcjonalnych parametrów metody. Parametry te muszą mieć przypisaną wartość domyślną (np. null, 0, ""). Kompilator automatycznie nadpisuje tę wartość domyślną odpowiednią informacją o wywołującym w miejscu, gdzie metoda jest wywoływana.

```csharp
public static void Err(
    string message,
    [CallerMemberName] string memberName = "", // The name of the calling method/property
    [CallerFilePath] string filePath = "",     // The path of the calling file
    [CallerLineNumber] int lineNumber = 0)     // The line number of the call
{
    Console.WriteLine($"{message}");
    Console.WriteLine($"  Called from: {memberName}");
    Console.WriteLine($"  File: {filePath}");
    Console.WriteLine($"  Line: {lineNumber}");
    Environment.Exit(-1);
}
```

Dla wywołania:

```csharp
public static void LoadConfiguration(string configFilePath)
{
    if (!File.Exists(configFilePath))
    {
        Err($"Configuration file not found at: '{configFilePath}'"); // line 24
    }
    Console.WriteLine($"Configuration loaded successfully from: '{configFilePath}'");
}

public static void Main(string[] args)
{
    string config = "appsettings.json"; 
    LoadConfiguration(config);
}
```

Dostaniemy wyjście:

```bash
Configuration file not found at: 'appsettings.json'
  Called from: LoadConfiguration
  File: /home/tomasz/Workspace/CSharp/ConsoleApp/ConsoleApp/Program.cs
  Line: 24

Process finished with exit code 255.
```

W przeciwieństwie do pozostałych atrybutów *Caller Info*, które dostarczają informacji o miejscu wywołania (nazwa metody, plik, linia), `CallerArgumentExpression` pozwala na przechwycenie wyrażenia w kodzie źródłowym, które zostało przekazane jako argument do innego parametru tej samej metody.

```csharp
public static void Assert(bool condition, 
    [CallerArgumentExpression(nameof(condition))] string? message = null)
{
    if (!condition)
    {
        Console.Error.WriteLine($"Assertion failed: {message}");
    }
}
```

```csharp
Assertion.Assert("Hello, World!" is { Length: < 5 });
// Output:
// Assertion failed: "Hello, World!" is { Length: < 5 }
```

> [!INFO]
> Operator `nameof` pozwala na uzyskanie nazwy zmiennej, typu, składowej lub parametru w postaci ciągu znaków. Działa w czasie kompilacji i chroni przed  błędami związanymi z refaktoryzacją nazw. Jeżeli taka nazwa nie jest do niczego przypisana kompilator zgłosi błąd.