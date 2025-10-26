---
title: "Lab08"
weight: 10
---

# Laboratorium 8: Assembly, Refleksja

## Dynamiczne tworzenie obiektów

{{% hint info %}}
**Czym jest refleksja?**

Refleksja to mechanizm, który pozwala badać i manipulować informacjami o typach - ich polach, właściwościach, metodach, atrybutach itp., a także w wielu przypadkach dynamicznie wywoływać metody i tworzyć instancje w czasie wykonania.

Dzięki refleksji kod może działać "na typach", których nie znał w czasie kompilacji.

**Przykłady operacji wykorzystujących refleksję:**

- Odczytanie listy wszystkich właściwości dla danego typu.
- Odczytanie i zapisanie wartości właściwości/pól.
- Pobranie i wywołanie konstruktora (np. bezparametrowego lub bardziej wyspecjalizowanego).
- Odczytanie atrybutów przypisanych do klasy, właściwości lub metody.
- Wywoływanie generycznych metod.
- Pobranie interfejsów, które implementuje dana klasa.

**Uwagi praktyczne:**

- **Wydajność** — operacje refleksji są zwykle wolniejsze niż bezpośredni kod. Warto przechowywać raz pobrane informacje (`Type`, `PropertyInfo`, `MethodInfo`) w strukturach danych.
- **Bezpieczeństwo** — przy pomocy refleksji można łamać enkapsulację (dostęp do prywatnych członków).

{{% /hint %}}

### Opis zadania

Zaimplementuj klasę `TypeCrafter`, która przy pomocy metody `CraftInstance<>` potrafi dynamicznie zbudować instancję dowolnego typu `T` w czasie wykonania, czytając wartości z konsoli i przypisując je do właściwości obiektu. Twoja implementacja powinna intensywnie korzystać z refleksji (przestrzeń nazw `System.Reflection`).

> [!TIP] 
> **Kod początkowy** 
> {{< filetree dir="labs/lab08/student/TypeCrafter" >}}
>
> **Wyjście:** [TypeCrafter.txt](/labs/lab08/outputs/TypeCrafter.txt)

```csharp
public static class TypeCrafter
{
    public static T CraftInstance<T>()
    {
        throw new NotImplementedException();
    }
}
```

Metoda `CraftInstance<>`:

- Tworzy instancję typu `T`.
- Przechodzi po wszystkich publicznych właściwościach.
- Dla każdej właściwości:
  - Jeśli typ właściwości to `string` — pobiera linię z konsoli i ustawia wartość.
  - Jeżeli typ jest parsowalny (implementuje interfejs `IParsable<>` lub posiada statyczną metodę `TryParse`) - pobiera linię z konsoli, próbuje sparsować tekst wywołując odpowiednią metodę `TryParse` i ustawia wartość. W przypadku nieprawidłowego parsowania zgłasza wyjątek customowy wyjątek `ParseException`.
  - W przeciwnym razie traktuje właściwość jako złożony obiekt i rekurencyjnie wywołuje `CraftInstance<>` dla typu tej właściwości.
  - Zwraca zainicjowany obiekt.

W metodzie `Main` klasy `Program` zaprezentuj wywołanie metody `CraftInstance<>` dla przykładowych typów `Customer` oraz `Invoice`:

```csharp
public sealed class Customer
{
    public Customer() { }

    public Customer(int id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public override string ToString()
    {
        return $"Customer {Id}: {Name} (Balance: {Balance:C})";
    }
}

public sealed class Invoice
{
    public Invoice() { }

    public Invoice(
        Guid id,
        string description,
        decimal amount,
        Customer customer)
    {
        Id = id;
        Description = description;
        Amount = amount;
        Customer = customer;
    }

    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public Customer Customer { get; set; } = null!;

    public override string ToString()
    {
        return $"Invoice {Id}: '{Description}', Amount: {Amount:C}, Customer: {Customer}";
    }
}
```

{{% hint warning %}}
**Uwagi implementacyjne**

- **Refleksja:**
  - Wykorzystaj m.in. typy `Type`, `PropertyInfo`, `ConstructorInfo` i `MethodInfo`.
- **Parsowanie metodą `TryParse`:**
  - Wykorzystaj wywołanie zgodne z sygnaturą `TryParse(string? s, IFormatProvider? provider, out T result)`.
  - Przy pomocy refleksji szukaj wśród publicznych metod statycznych.
  - Do przygotowania parametru wyjściowego (`out`) wykorzystaj `MakeByRefType()`.
- **Wymóg konstruktorów:**
  - Metoda powinna wymagać publicznego konstruktora bezparametrowego dla typów tworzących obiekt.
  - Jeżeli konstruktor bezparametrowy nie istnieje, rzuć `InvalidOperationException` z czytelnym komunikatem.
- **Błędy parsowania:**
  - W przypadku nieudanego parsowania zgłoś `ParseException` (z opisem wejścia i typu docelowego).
  - Główna aplikacja (metoda `Main`) może przechwycić `ParseException` i poprosić użytkownika o powtórne wypełnienie (np. ponowne wywołanie `CraftInstance<>`).

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: Reflection](https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/reflection)
- [Microsoft Learn: The typeof operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/type-testing-and-cast#the-typeof-operator)
- [Microsoft Learn: PropertyInfo Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.propertyinfo?view=net-9.0)
- [Microsoft Learn: MethodBase.Invoke Method](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.invoke?view=net-9.0)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP] 
> **Rozwiązanie** 
> {{< filetree dir="labs/lab08/solution/TypeCrafter" >}}
>
> **Wyjście:** [TypeCrafter.txt](/labs/lab08/outputs/TypeCrafter.txt)

## Biblioteka testów jednostkowych

{{% hint info %}}
**Czym są testy jednostkowe?**

Testy jednostkowe (ang. _Unit tests_) to automatyczne, małe i szybkie testy, które sprawdzają pojedyncze jednostki kodu — najczęściej pojedyncze metody lub klasy — w izolacji od reszty systemu. Ich celem jest wczesne wykrycie błędów, udokumentowanie zachowania modułów i ułatwienie refaktoryzacji.

**Dlaczego warto pisać testy jednostkowe?**

- **Wykrywanie regresji**: Testy pomagają szybko stwierdzić, czy zmiana kodu zepsuła istniejącą funkcjonalność;
- **Dokumentacja zachowania**: Testy pokazują, jak klasa/metoda powinna się zachowywać;
- **Ułatwiona refaktoryzacja**: Bezpieczniej zmieniać strukturę kodu mając zestaw automatycznych testów;
- **Szybsze lokalizowanie błędów**: Błąd z reguły lokalizuje się do małego wycinka kodu.

**Popularne biblioteki i narzędzia dla C#**

- [xUnit](https://xunit.net/?tabs=cs) - nowoczesny, dobry dla .NET Core/.NET 6+, wspiera testy równoległe.
- [NUnit](https://nunit.org/) - stabilny, bogaty w funkcje, przydatny przy migracji ze starszych projektów.
- [MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-mstest) - prosty i dobrze zintegrowany z Visual Studio (domyślny w niektórych szablonach).
- [FluentAssertions](https://fluentassertions.com/introduction) - rozszerza czytelność asercji przez styl "fluent" oraz poprawia diagnostykę błędów poprzez ładniejsze komunikaty.
- [Moq](https://github.com/devlooped/moq/wiki/Quickstart) - bardzo popularny framework do tworzenia mocków w C#.

{{% /hint %}}

### Opis zadania

Twoim zadaniem jest stworzenie od podstaw własnego, lekkiego frameworka do testów jednostkowych.

Projekt powinien zawierać 2 komponenty:

- **Biblioteka `MiniTest`** – zawierająca atrybuty testowe pozwalające użytkownikom oznaczać klasy i metody jako kontenery testów oraz metody asercji.
- **`MiniTestRunner` (program wykonywalny)** – aplikacja, która dynamicznie ładuje kolekcję assembly zawierających testy, wyszukuje kontenery testów, uruchamia znalezione testy i prezentuje wyniki w konsoli.

> [!TIP] > **Kod początkowy**  
> Kod początkowy zawiera bibliotekę z testami jednostkowymi, którą będzie można użyć jako wejście dla programu **MiniTestRunner**.
> {{< filetree dir="labs/lab08/student/MiniTest" >}}
>
> **Wyjście:** [MiniTestRunner.txt](/labs/lab08/outputs/MiniTestRunner.txt)

#### MiniTest

**Atrybuty testowe:**

Biblioteka powinna udostępniać następujące atrybuty do oznaczania klas i metod jako kontenerów testowych oraz zarządzania cyklem życia testów:

- `TestClassAttribute` – oznacza klasę jako kontener metod testowych.
- `TestMethodAttribute` – oznacza metodę jako test jednostkowy do wykonania.
- `BeforeEachAttribute` – określa metodę, która ma być uruchamiana przed każdym testem.
- `AfterEachAttribute` – określa metodę, która ma być uruchamiana po każdym teście.
- `PriorityAttribute` – ustawia priorytet (liczba całkowita) dla testów. Mniejsza liczba oznacza wyższy priorytet.
- `DataRowAttribute` – umożliwia testy parametryzowane przez przekazanie danych do metod testowych.
  - przyjmuje tablicę obiektów (`object?[]`) reprezentujących dane testowe,
  - opcjonalnie przyjmuje napis dokumentujący zestaw danych testowych.
- `DescriptionAttribute` – pozwala dodać opis do testu lub klasy testowej.

**Asercje:**

Biblioteka powinna również udostępniać metody do weryfikacji powodzenia lub porażki testów.
Mają być one obsługiwane przez statyczną klasę `Assert`, zawierającą metody asercji:

- `ThrowsException<TException>(Action action, string message = "")` – sprawdza, czy podczas działania wyrzucany jest określony typ wyjątku.
- `AreEqual<T>(T? expected, T? actual, string message = "")` – porównuje wartości oczekiwaną i aktualną.
- `AreNotEqual<T>(T? notExpected, T? actual, string message = "")` – sprawdza, czy wartości są różne.
- `IsTrue(bool condition, string message = "")` – potwierdza, że warunek logiczny jest prawdziwy.
- `IsFalse(bool condition, string message = "")` – potwierdza, że warunek logiczny jest fałszywy.
- `Fail(string message = "")` – jawnie oznacza test jako nieudany z podanym komunikatem.

Każda z metod powinna wyrzucać wyjątek, gdy warunek nie jest spełniony. Wszystkie powinny opcjonalnie przyjmować opis (`message`).

**Obsługa wyjątków:**

Należy zaimplementować własny wyjątek `AssertionException`, używany wyłącznie dla nieudanych asercji.

Każda metoda asercji powinna jasno opisywać powód niepowodzenia, np.:

- `ThrowsException`:
  - `Oczekiwano wyjątku typu <{typeof(TException)}> ale otrzymano <{ex.GetType()}>. {message}`
  - `Oczekiwano wyjątku typu <{typeof(TException)}> ale żaden wyjątek nie został wyrzucony. {message}`
- `AreEqual`:
  - `Oczekiwano: {expected}. Otrzymano: {actual}. {message}`
- `AreNotEqual`:
  - `Oczekiwano dowolnej wartości poza: {notExpected}. Otrzymano: {actual}. {message}`

#### MiniTestRunner

`MiniTestRunner` to aplikacja konsolowa odpowiedzialna za wyszukiwanie i wykonywanie testów oraz raportowanie wyników.

**Wejście:**

Program przyjmuje ścieżki do plików assembly jako argumenty wiersza poleceń. Powinny one zawierać klasy i metody testowe oznaczone atrybutami `MiniTest`.

```bash
MiniTestRunner path/to/test-assembly1.dll path/to/test-assembly2.dll
```

**Ładowanie assembly:**

- Użyj `AssemblyLoadContext` do dynamicznego ładowania testowych assembly, bez wpływu na główny kontekst uruchomieniowy.
- Konteksty powinny być możliwe do zwolnienia (`isCollectible`), aby efektywnie zarządzać pamięcią.

**Wykrywanie testów:**

- Szukaj w klas oznaczonych atrybutem `TestClassAttribute`.
- W każdej klasie testowej:
  - odkryj metody oznaczone `TestMethodAttribute`,
  - znajdź testy parametryzowane (`DataRowAttribute`),
  - zidentyfikuj metody `BeforeEach` i `AfterEach` do logiki przygotowania/sprzątania.
- Pomijaj klasy testowe bez konstruktora bezparametrowego.
- Ignoruj niepoprawne konfiguracje (np. złe parametry dla `DataRow`).
- Wypisz ostrzeżenie w konsoli, gdy konfiguracja jest niezgodna.

**Wykonywanie testów:**

- Testy powinny być wykonywane według priorytetu (`PriorityAttribute` – niższa liczba oznacza wyższy priorytet).
- Brak atrybutu oznacza priorytet `0`.
- Przy tym samym priorytecie wykonuj alfabetycznie według nazw metod.

Dla każdej klasy i testu:

1. Uruchom metodę `BeforeEach`.
2. Uruchom test.
3. Uruchom metodę `AfterEach`.
4. Testy parametryzowane (`DataRow`) traktuj jako osobne testy.
5. Test uznaje się za nieudany, jeśli wystąpił nieobsłużony wyjątek.

**Wyniki i formatowanie:**

Wyniki pojedynczego testu:

- Status: `PASSED` lub `FAILED`.
- Powód niepowodzenia + komunikaty wyjątków.
- Opis testu lub klasy (jeśli podano).

Podsumowanie po każdej klasie:

- Liczba wszystkich testów.
- Liczba testów zakończonych sukcesem i porażką.

Podsumowanie po każdym assembly:

- Łączna liczba testów.
- Liczba testów udanych i nieudanych.

Kolorowanie konsoli:

- `Zielony` = testy zaliczone,
- `Czerwony` = testy nieudane,
- `Żółty` = ostrzeżenia (np. brak konstruktora, błędna konfiguracja).

{{% hint warning %}}
**Uwagi implementacyjne**

- **Zgodność z przykładowymi testami:**
  - Implementacja biblioteki `MiniTest` powinna być w pełni zgodna z przykładowymi testami zawartymi w projekcie startowym.
  - Szczegóły "logiki biznesowej", której dotyczą przykładowe testy jednostkowe, nie jest istotna w zadaniu.

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: Attributes](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/)
- [Microsoft Learn: Retrieving Attributes from Class Members](https://learn.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes#retrieving-attributes-from-class-members)
- [Microsoft Learn: Retrieving Information Stored in Attributes](https://learn.microsoft.com/en-us/dotnet/standard/attributes/retrieving-information-stored-in-attributes)
- [Microsoft Learn: Assembly Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly?view=net-9.0)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP] 
> **Rozwiązanie** 
> {{< filetree dir="labs/lab08/solution/MiniTest" >}}
>
> **Wyjście:** [MiniTestRunner.txt](/labs/lab08/outputs/MiniTestRunner.txt)
