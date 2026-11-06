---
title: "Wyjątki"
---

# Wyjątki

Implementacja wyjątków w C# jest bardzo podobna do tej w C++. Składniowo jest niemalże identyczna, z kilkoma nowymi możliwościami w C#. Główną różnicą jest to, że w C# możemy rzucać jedynie obiektami dziedziczącymi po `System.Exception`. W C++ mogliśmy rzucać w zasadzie dowolnym obiektem, czego się i tak nie robi w praktyce.

## Łapanie wyjątków

Tak jak w C++, żeby złapać wyjątek, otaczamy kod, który może rzucić wyjątkiem, blokiem `try` i dołączamy do niego bloki `catch` z obsługą sytuacji wyjątkowych. W przypadku wyjątku program szuka odpowiedniego bloku `catch` od góry do dołu.

```csharp
try
{
    string text = File.ReadAllText("file.txt");
    int number = int.Parse(text);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid format: {ex}");
}
catch (Exception ex) // This will catch any exception
{
    Console.WriteLine($"Unexpected exception: {ex}");
}
```

## Zgłaszanie wyjątków

Żeby zgłosić wyjątek, używamy instrukcji `throw`, po której następuje obiekt wyjątku, zazwyczaj tworzony w miejscu operatorem `new`. Sam typ wyjątku po części dokumentuje sytuację wyjątkową, ale warto również doprecyzować, co się stało, podając dodatkową wiadomość w parametrze konstruktora.

```csharp
public static void ValidateInput(string input, int maxLength)
{
    if (input is null)
    {
        throw new ArgumentNullException(nameof(input), "Input cannot be null.");
    }

    if (input.Length > maxLength)
    {
        throw new ArgumentOutOfRangeException(nameof(input), $"Input exceeds the maximum length of {maxLength} characters.");
    }
}
```

## Przerzucenie wyjątku

W bloku `catch` możemy przekazać dalej wyjątek, jednocześnie częściowo na niego reagując (np. przez zalogowanie błędu). Służy do tego instrukcja `throw` bez żadnego parametru. 

```csharp
try
{
    ValidateInput("Hello, world", 16);
}
catch (ArgumentNullException e)
{
    Console.WriteLine(e);
}
catch (ArgumentOutOfRangeException e)
{
    Console.WriteLine(e);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
```

> [!WARNING]
> `throw;` ponownie rzuca złapany wyjątek i zachowuje jego oryginalny ślad stosu (*stack trace*). Gdybyśmy zamiast tego napisali `throw e;`, rzucony zostałby ten sam obiekt, ale jego ślad stosu zostałby zresetowany do bieżącego bloku `catch` – stracilibyśmy informację o tym, gdzie faktycznie wystąpił błąd.

## Filtry wyjątków

Do bloków `catch` możemy doklejać klauzulę `when`, w której możemy sprecyzować dodatkowe warunki, które muszą być spełnione:

```csharp
try
{
    throw new HttpRequestException("Resource not found", null, System.Net.HttpStatusCode.NotFound);
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("Resource not found.");
    Console.WriteLine(ex);
}
catch (HttpRequestException ex)
{
    Console.WriteLine(ex);
}
```

## Własne typy wyjątków

Biblioteka standardowa dostarcza całkiem pokaźny wachlarz wbudowanych typów wyjątków. Czasami jednak potrzebujemy bardziej szczegółowych typów wyjątków, lepiej opisujących sytuację. Dzięki temu kod staje się bardziej czytelny, a obsługa błędów bardziej precyzyjna. Zazwyczaj reimplementuje się trzy najczęściej używane konstruktory, ale oczywiście można w takiej klasie dodawać własne konstruktory i pola.

```csharp
public class InvalidOrderException : Exception
{
    public InvalidOrderException() 
    {
    }

    public InvalidOrderException(string message)
        : base(message)
    {
    }

    public InvalidOrderException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
```

## Blok `finally`

Blok `finally` może zostać dołączony do bloku `try-catch`. Kod w takim bloku wykona się zawsze, niezależnie od okoliczności, w jakich wychodzimy z bloku `try` (normalnie, przez `return`/`break` czy z powodu wyjątku). Nie wykona się jedynie przy gwałtownym zakończeniu całego procesu, np. przez `Environment.FailFast` lub przepełnienie stosu. Głównym zastosowaniem bloku `finally` jest zwalnianie zasobów, takich jak strumienie plików, połączenia z bazą danych czy zasoby sieciowe.

```csharp
FileStream? fs = null;
try
{
    fs = new FileStream("file.txt", FileMode.Open);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    fs?.Dispose();
}
```
