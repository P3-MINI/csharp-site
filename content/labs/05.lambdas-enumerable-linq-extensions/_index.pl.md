---
title: "Wyrażenia lambda, metody rozszerzające, LINQ"
weight: 50
---

# Szybka powtórka wykładu

## Delegaty
Delegata to bezpieczniejszy i silnie typowany odpowiednik wskaźnika do funkcji z języka C++. Można ją przechowywać w zmiennej, przekazywać jako argument, zwracać z funkcji oraz wywoływać tak samo jak metodę. Na przykład:

```csharp
public delegate int BinaryOp(int a, int b);

int Add(int x, int y) => x + y;
int Mul(int x, int y) => x * y;

BinaryOp op = Add;
Console.WriteLine(op(2, 3)); // 5
op = Mul;
Console.WriteLine(op(2, 3)); // 6
```
### Wbudowane typy delegat
W nowoczesnym języku C# rzadko zachodzi potrzeba definiowania własnych typów delegat, ponieważ wbudowane generyczne typy pokrywają niemal wszystkie przypadki użycia:
|Sygnatura|Przykład|
|---|---|
| `void Action<in T1..T16>` | `Action<string> echo = (s => Console.WriteLine(s));` |
| `TResult Func<in T1..T16, out TResult>` | `Func<int, int> sqr = (x => x * x);` |
| `bool Predicate<in T>` | `Predicate<int> odd = (x => x % 2 != 0);` |
| `int Comparison<in T>` | `Comparison<int> cmp = ((a, b) => a.CompareTo(b));` |

### Delegaty wielokrotne
Delegaty można łączyć za pomocą operatora `+=` oraz rozdzielać za pomocą operatora `-=`. Wywołanie delegaty wielokrotnej powoduje sekwencyjne uruchomienie wszystkich przypisanych do niej metod.
```csharp
Action log = () => Console.WriteLine("one");
log += () => Console.WriteLine("two");
log += () => Console.WriteLine("three");
log();
// one
// two
// three
```
W przypadku wywołania takiej delegaty, otrzymywana jest jedynie wartość zwracana przez ostatnią z przypisanych metod. Z tego powodu nie należy używać tego mechanizmu w przypadku metod zwracających wartość - zamiast tego lepiej skorzystać z listy delegat.

## Wyrażenia lambda
Wyrażenie lambda to nienazwana funkcja, którą można przypisać do kompatybilnego typu delegaty. Składnia:

    var nazwa = <typ zwracany> (parametry) => wyrażenie; LUB { blok instrukcji }

>[!IMPORTANT]
> Lambda przechwytuje zmienne z otaczającego ją zakresu. Przechwycona zmienna jest **współdzielona**, a **nie kopiowana** — lambda przechowuje referencję do zmiennej, a nie jej wartość w momencie przechwycenia.

```csharp
int counter = 0;
Action increment = () => counter++;
increment();
increment();
Console.WriteLine(counter); // 2
```
Pod spodem, kompilator generuje klasę, w której przechwycona zmienna jest przechowywana jako pole.

> [!WARNING]
> Pamiętaj, że pętle `foreach` tworzą kopie tylko do odczytu w przypadku typów wartościowych, w przeciwieństwie do innych rodzajów pętli:
```csharp
var actions1 = new List<Action>();
foreach (var i in new[] { 1, 2, 3 }) actions1.Add(() => Console.WriteLine(i));
foreach (var a in actions1) a(); // 1, 2, 3

var actions2 = new List<Action>();
for (int i = 0; i < 3; i++) actions2.Add(() => Console.WriteLine(i));
foreach (var a in actions2) a(); // 3, 3, 3
```

>[!IMPORTANT]
> Jeśli chcesz mieć pewność, że żadne zmienne nie zostaną przechwycone (w celach mikrooptymalizacji lub aby uniknąć błędów), od wersji C# 9 możesz oznaczyć lambdy słowem kluczowym `static`:
```csharp
int captured = 5;
Func<int, int> bad = static x => x + captured; // compile error
```

## Metody rozszerzające
Metody rozszerzające umożliwiają dodawanie nowych metod do istniejących typów bez modyfikowania ich kodu źródłowego. Choć metody te są statyczne, można je wywoływać tak, jakby były metodami instancyjnymi obiektu, na którym operują (nawet bez dostępu do kodu źródłowego typu). 

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
Metoda rozszerzająca:
1. Musi być zadeklarowana w statycznej klasie (`static`), która nie jest ani generyczna, ani zagnieżdżona.
2. Musi być statyczna (`static`).
3. Pierwszy parametr musi być poprzedzony modyfikatorem `this`.

> [!WARNING]
> Jeśli dany typ posiada metodę instancyjną o tej samej nazwie i zgodnej sygnaturze, kompilator wybierze ją, zamiast rozszerzającej.

> [!IMPORTANT]
> Rozszerzać można zarówno klasy, jak i interfejsy. `Where`, `Select` oraz inne metody z technologii LINQ, z którymi zapoznasz się w tym samouczku, są w rzeczywistości metodami rozszerzającymi interfejs `IEnumerable<T>`:
```csharp
public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate) { ... }
public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) { ... }
```

### Bloki rozszerzające
Począwszy od wersji C# 14, można dodawać właściwości rozszerzające, statyczne pola rozszerzające oraz operatory rozszerzające. Na przykład:

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
> ## Przykładowe zadania z delegat, wyrażeń lambda i metod rozszerzających
{{% details "TASK 1" false %}}
Zadanie składa się z 5 etapów. Etapy zostały zdefiniowane w następujących plikach:
1. BinaryTree.cs

Dodaj metodę `DepthFirstSearch`, która przyjmuje trzy funkcje typu `Action` z parametrem typu `Node` (`preVisit`, `inVisit`, `postVisit`). Metoda powinna rekurencyjnie przeszukiwać drzewo, wywołując odpowiednie akcje w stosownych momentach:
- `preVisit` powinno być wywoływane przed zejściami rekurencyjnymi,
- `inVisit` powinno być wywoływane pomiędzy zejściami rekurencyjnymi,
- `postVisit` powinno być wywoływane po zakończeniu zejść rekurencyjnych. 

Zaimplementuj metodę `PrintInOrder`, korzystając z wcześniej przygotowanej metody:
- metoda ta powinna wypisywać wartości węzłów w porządku *In-Order* z odpowiednimi wcięciami,
- długość wcięcia powinna być proporcjonalna do poziomu węzła w drzewie,
- w metodach `preVisit` oraz `postVisit` można śledzić poziom rekurencji,
- w metodzie `inVisit` należy wypisać wartość węzła z odpowiednim wcięciem.

2. DictionaryExtensions.cs

Zaimplementuj metodę rozszerzającą dla interfejsu `IDictionary<TKey, TValue>` o nazwie `AddOrUpdate`. 
Metoda ta powinna dodawać nowe elementy do słownika, jeśli wartość dla podanego klucza jeszcze nie istnieje,
lub aktualizować istniejącą wartość w oparciu o klucz oraz poprzednią wartość. Metoda powinna przyjmować następujące parametry:
- `TKey key` – klucz, który ma zostać dodany lub zaktualizowany,
- `TValue addValue` – wartość, która ma zostać dodana, jeśli klucz nie istnieje w słowniku,
- `Func<TKey, TValue, TValue> updateValueFactory` – funkcja, która na podstawie klucza oraz poprzedniej wartości określa sposób aktualizacji; przyjmuje ona klucz (`TKey`) oraz poprzednią wartość (`TValue`) i zwraca nową wartość (`TValue`). 

Metoda powinna zwracać nowo dodaną wartość. 
Następnie zaimplementuj metodę `CountWords`, która zlicza wystąpienia słów w sekwencji ciągów stringów, wykorzystując wcześniej zaimplementowaną metodę. Metoda ta powinna zwracać `IDictionary<string, int>`, gdzie kluczami są słowa, a wartościami – liczba ich wystąpień w sekwencji.

3. EnumerableExtensions.cs

Na tym etapie zabronione jest używanie funkcji `Aggregate` z biblioteki **LINQ**. Napisz metodę rozszerzającą `Reduce<TSource, TAccumulate>` dla interfejsu `IEnumerable<TSource>`, która przeprowadzi operację redukcji sekwencji do pojedynczej wartości. 
Metoda powinna przyjmować następujące parametry:
- `TAccumulate seed`: wartość początkowa akumulatora,
- `Func<TAccumulate, TSource, TAccumulate> accumulator`: funkcja implementująca logikę akumulacji, przyjmująca dwie wartości (typu `TAccumulate` oraz `TSource`) i zwracająca wartość typu `TAccumulate`. Metoda powinna iterować po elementach źródłowej sekwencji i aktualizować akumulator za pomocą przekazanej funkcji. 

Wykorzystując metodę `Reduce`, zaimplementuj trzy metody rozszerzające interfejs `IEnumerable<T>`,
z zastrzeżeniem, że typ `T` musi implementować interfejs `INumber<T>`:
- `Sum` – zwraca sumę elementów typu `T` w sekwencji,
- `Min` – zwraca najmniejszy element typu `T` w sekwencji,
- `Max` – zwraca największy element typu `T` w sekwencji.

Wskazówki: `T.CreateTruncating(double.NegativeInfinity)`, `T.CreateTruncating(double.PositiveInfinity)`, `T.Zero`, `T.Min`, `T.Max`.

4. FactoryMethods.cs

Napisz statyczną funkcję o nazwie `RandomValueFunctionFactory`, która zwraca funkcję anonimową. Funkcja ta powinna zwracać (pseudo)losową wartość typu `double` z zakresu określonego przez parametry: `double min` oraz `double max`. 

Napisz statyczną funkcję o nazwie `RandomPointFunctionFactory`, która zwraca funkcję anonimową. Funkcja ta powinna generować (pseudo)losowy wektor składający się z dwóch wartości typu `double` (w formie tupli) z zakresu określonego przez parametry: `double xMin`, `double xMax`, `double yMin`, `double yMax`. Skorzystaj z wcześniej zdefiniowanej metody.

Napisz statyczną funkcję o nazwie `MonteCarloPiEstimation`, która zwraca przybliżoną wartość liczby π (typu `double`).
Wykorzystaj wcześniej zdefiniowaną metodę. Funkcja powinna przyjmować parametr określający liczbę próbek (punktów) użytych do przybliżenia – jest to wartość typu `int` (z wartością domyślną 1024).
- Metoda Monte Carlo: <https://en.wikipedia.org/wiki/Monte_Carlo_method>
- Wskazówka: Wygeneruj losowe punkty wewnątrz prostokąta obejmującego (fragment) koła jednostkowego, a następnie sprawdź, czy znajdują się one wewnątrz tego koła (lub jego fragmentu). Oblicz stosunek liczby punktów znajdujących się wewnątrz koła do całkowitej liczby punktów, aby oszacować wartość π.
  
5. Numerics.cs

Zadeklaruj delegatę o nazwie `Function`, która przyjmuje parametr `x` typu `double` i zwraca wartość typu `double`. Zaimplementuj metodę statyczną o nazwie `NewtonRootFinding`, która przyjmuje funkcję `f` (ypu `Function`, funkcję pochodną `df` typu `Function` (z wartością domyślną `null`), punkt startowy `x0` (z wartością domyślną 0) oraz precyzję `eps` (z wartością domyślną wartość 1e-6). Metoda powinna zwracać obliczony pierwiastek równania przy użyciu metody Newtona. Jeśli parametr `df` (funkcja pochodnej) ma wartość `null`, należy go zainicjować na początku metody, korzystając z przybliżenia:

(f(x + eps) - f(x - eps)) / (2 * eps). 
- Metoda Newtona: <https://en.wikipedia.org/wiki/Newton%27s_method>

Następnie zaimplementuj trzy metody bezparametrowe, które zwracają pierwiastki funkcji
przy użyciu metody Newtona:
- `FindLinearRoot` – dla funkcji `f(x) = x - 2`. Przekaż dokładną pochodną.
- `FindQuadraticRoot` – dla funkcji `f(x) = x * x + 2 * x + 1`. Przekaż `null` jako pochodną.
- `FindSinRoot` – dla funkcji `f(x) = sin(x)`. Przekaż dokładną pochodną oraz punkt startowy `x0 = 2`. 
Jako argumenty przekaż funkcje `Math.Sin` oraz `Math.Cos`.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task1" >}}
{{% /details %}}

{{% details "TASK 2" false %}}
Zadanie składa się z 5 etapów. Etapy te zostały zdefiniowane w następujących plikach:
1. BinaryTree.cs

Zaimplementuj metodę [`BreadthFirstSearch`](https://en.wikipedia.org/wiki/Breadth-first_search), która przyjmuje jako parametr funkcję typu `Action` (o nazwie `onVisit`), przyjmującą argument typu `Node`. Metoda powinna przeszukiwać drzewo poziomami (ang. *level order*) i wywoływać akcję `onVisit` przy każdym napotkanym węźle (`Node`). Pseudokod ogólnego algorytmu BFS dla drzewa binarnego:
```
Queue Q = Queue.Empty
Q.Enqueue(Root)
while(Q not empty)
Node N = Q.Dequeue()
if(N.Left) Q.Enqueue(N.Left)
if(N.Right) Q.Enqueue(N.Right)
```
Następnie zaimplementuj metodę `ValueCount`, która przyjmuje jako parametr wartość typu `T`. Metoda ta przeszukuje drzewo przy użyciu wcześniej zaimplementowanej metody `BreadthFirstSearch` i zlicza, ile razy dana wartość występuje w drzewie.

2. Benchmarking.cs

Zaimplementuj metodę `CreateBenchmark`, która tworzy wrapper typu benchmark dla podanej delegaty `Action`, umożliwiając pomiar i raportowanie średniego czasu wykonania w określonej liczbie iteracji. 
Metoda ta przyjmuje następujące parametry:
- delegatę `Action` o nazwie `action`,
- ciąg znaków `name` określający nazwę benchmarku,
- opcjonalny parametr `times` (domyślnie ustawiony na 1000).

Metoda zwraca funkcję anonimową typu `Action`. Po wywołaniu, zwrócony delegat `Action` mierzy średni czas wykonania przekazanego `action`, uruchamiając go w pętli określoną liczbę razy (`times`). Do precyzyjnego pomiaru upływającego czasu wykorzystuje klasę `Stopwatch`. 

Po zakończeniu pętli metoda oblicza i wypisuje średni czas pojedynczego wykonania `action` (w milisekundach). Wynik zawiera nazwę funkcji (`name`) oraz średni czas wykonania w następującym formacie:
```
"Function `name`: `elapsed` ms on average"
```
Następnie, korzystając z wcześniej zaimplementowanej metody, stwórz metodę `CreateBenchmarks`, która przygotuje trzy benchmarki porównujące wydajność trzech metod łączenia ciągów znaków (zdefiniowanych w tym samym pliku): `StringAdd`, `StringBuilder` oraz `StringJoin`. Istotne funkcje i właściwości klasy `Stopwatch`:
- `new Stopwatch()`
- `stopwatch.Start()`
- `stopwatch.Stop()`
- `stopwatch.Elapsed.TotalMilliseconds`

3. DictionaryExtensions.cs

Zaimplementuj metodę `Merge<TKey, TValue>` – metodę rozszerzającą dla interfejsu `IDictionary`, która łączy istniejącą wartość przypisaną do danego klucza w słowniku z podaną wartością. 

Jeśli określony klucz nie jest jeszcze powiązany z żadną wartością lub jest powiązany z wartością `null`, metoda przypisuje mu przekazaną w parametrze wartość (która nie może być `null`). W przeciwnym razie metoda zastępuje istniejącą wartość wynikiem działania funkcji przekształcającej (remapping function) lub usuwa wpis, jeśli wynik tej funkcji to `null`. 

Metoda przyjmuje następujące parametry:
- `TKey` `key` – klucz wpisu, który ma zostać scalony lub dodany,
- `TValue` `value` – wartość (niebędąca `null`), która ma zostać scalona z istniejącą wartością przypisaną do klucza `key` (lub przypisana do niego, jeśli wcześniej nie było tam żadnej wartości bądź była ona równa `null`),
- `Func` `remappingFunction` – funkcja służąca do ponownego obliczenia wartości (jeśli już istnieje), w oparciu o starą wartość oraz nową wartość `value`.

Metoda zwraca nową wartość przypisaną do określonego klucza `key` lub `null`, jeśli po operacji z kluczem nie jest powiązana żadna wartość. 

Następnie zaimplementuj metodę `MergeLogs`, wykorzystując wcześniej zdefiniowaną metodę `Merge` do połączenia dwóch słowników. W przypadku wystąpienia tego samego klucza w obu słownikach, zaktualizuj wartość w słowniku źródłowym, łącząc wartości z obu słowników (użyj `string.Concat`).

4. EnumerableExtensions.cs

Zaimplementuj metodę `Collect<TSource, TAccumulate>` – metodę rozszerzającą dla interfejsu `IEnumerable`, która zapewnia ogólny mechanizm gromadzenia elementów z kolekcji źródłowej w wynikowy obiekt. Pozwala to na elastyczne definiowanie sposobu akumulacji poprzez przekazanie własnej akcji akumulującej.

Metoda przyjmuje następujące parametry:
- `TAccumulate` `collection` – kontener wynikowy. 
- `Action` `accumulator` – asocjatywna, nieingerująca i bezstanowa funkcja służąca do włączania dodatkowego elementu do wyniku; przyjmuje ona argument typu `TAccumulate` oraz argument typu `TSource` i nie zwraca żadnej wartości. 

Metoda zwraca wynik redukcji. 

Zaimplementuj metodę `Filter<T>` – metodę rozszerzającą dla interfejsu `IEnumerable`, która przyjmuje parametr `Predicate` o nazwie `predicate`. Metoda iteruje po każdym elemencie w kolekcji źródłowej i zwraca (poprzez `yield`) tylko te elementy, dla których określony `predicate` przyjmuje wartość `true`. Zwraca ona `IEnumerable<T>` zawierający przefiltrowane elementy. 

Następnie zaimplementuj metodę `ConcatStrings`, która filtruje elementy kolekcji, pozostawiając tylko te, które są typu `string`, wykorzystując w tym celu metodę `Filter`. Przefiltrowane ciągi znaków są następnie łączone w jeden obiekt `string` przy użyciu wcześniej zaimplementowanej metody `Collect` oraz klasy `StringBuilder`. Na koniec metoda zwraca wynikowy, połączony ciąg znaków.

5. Numerics.cs

Zadeklaruj delegat `Function`, który przyjmuje jeden parametr typu `double` (oznaczony jako `x`) i zwraca liczbę typu `double`. 

Zaimplementuj metodę `HalleyRootFinding`, która realizuje metodę Halleya służącą do wyznaczania pierwiastków równania. Przyjmuje ona funkcję `Function f`, opcjonalne pierwszą i drugą pochodną (odpowiednio `Function df` oraz `ddf`, z wartością domyślną `null`), wartość początkową (`x0`, domyślnie `0`) oraz tolerancję (`eps`, domyślnie `1e-6`). Jeśli pierwsza lub druga pochodna nie zostały podane (wartość `null`), należy je zainicjować za pomocą następującego przybliżenia: `df = (f(x + eps) - f(x - eps)) / (2 * eps)`. Pseudokod [metody Halleya](https://en.wikipedia.org/wiki/Halley%27s_method) służącej do wyznaczania pierwiastków równań:
```
Halley(f, df, ddf, x0, eps):
xn = x0
do
x = xn
xn = x - 2 * f(x) * df(x) / (2 * df(x) * df(x) - f(x) * ddf(x))
while |x - xn| > eps
return xn
```
Następnie zaimplementuj funkcje `FindLinearRoot`, `FindQuadraticRoot` oraz `FindLogRoot`, które wyznaczają pierwiastki poniższych funkcji przy użyciu metody Halleya:
- `FindLinearRoot`: `f(x) = 2x - 1`; określ dokładne wartości pierwszej i drugiej pochodnej. Pozostaw domyślny punkt startowy oraz wartość epsilon. 
- `FindQuadraticRoot`: `f(x) = 2x^2 + 3x + 1`; pozostaw wszystkie parametry metody Halleya na wartościach domyślnych. 
- `FindLogRoot`: `f(x) = log(x)`; określ dokładną wartość pierwszej pochodnej (drugą pomiń). Przyjmij `2` jako punkt startowy (`x0`). Pozostaw domyślną wartość epsilon. Przekaż funkcje jako referencje do metod `double.Log` oraz `double.ReciprocalEstimate`.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task2" >}}
{{% /details %}}

## Więcej na ten temat:
- [Delegates (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- [Lambda expressions and anonymous functions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Extension members (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)

## LINQ
**LINQ** (Language-Integrated Query) to zestaw metod rozszerzających dla interfejsów `IEnumerable<T>` (oraz `IQueryable<T>`), które umożliwiają wykonywanie na kolekcjach operacji przypominających zapytania SQL. LINQ oferuje dwie równoważne składnie – *składnię metod* (ang. *method syntax*) oraz *składnię zapytań* (ang. *query syntax*) – kompilowane do tego samego kodu.

> [!NOTE]
> Interfejs `IQueryable<T>` udostępnia ten sam zestaw metod rozszerzających co `IEnumerable<T>`, jednak zamiast tworzyć potok delegat, buduje drzewo wyrażeń (ang. *expression tree*). W przypadku korzystania z frameworków typu [ORM](https://en.wikipedia.org/wiki/Object%E2%80%93relational_mapping), takich jak [Entity Framework](https://en.wikipedia.org/wiki/Entity_Framework), drzewo wyrażeń jest tłumaczone na język [SQL](https://en.wikipedia.org/wiki/SQL) i wykonywane bezpośrednio w bazie danych. Podczas tych warsztatów nie będziemy pracować z bazami danych, ale wiedza ta może okazać się przydatna w trakcie innych przedmiotów w przyszłości.

### Wykonanie odroczone
Większość operatorów LINQ działa w sposób odroczony – nie są one wykonywane w trakcie budowania zapytania, lecz dopiero w momencie zażądania jego wyniku (dotyczy to również zgłaszania wyjątków). Może to znacząco poprawić wydajność w przypadku dużych, złożonych zapytań, pod warunkiem poprawnego wykorzystania tego mechanizmu.
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
Pętle `foreach` również wymuszają wykonanie całego zapytania.

### Przegląd metod
#### Odroczone
| Kategoria | Metoda |
| --- | --- |
| **Filtrowanie** | `Where`, `OfType` |
| **Projekcja** | `Select`, `SelectMany`, `Zip` |
| **Operacje na zbiorach** | `Concat`, `Union`, `UnionBy`, `Intersect`, `IntersectBy`, `Except`, `ExceptBy`, `Distinct`, `DistinctBy` |
| **Partycjonowanie** | `Take`, `TakeLast`, `TakeWhile`, `Skip`, `SkipLast`, `SkipWhile`, `Chunk` |
| **Łączenie** | `Join`, `GroupJoin`, `LeftJoin`, `RightJoin` |
| **Sortowanie** | `Order`, `OrderBy`, `OrderDescending`, `OrderByDescending`, `ThenBy`, `ThenByDescending`, `Reverse`, `Shuffle` |

#### Natychmiastowe
| Kategoria | Metoda |
| --- | --- |
| **Kwantyfikatory** | `All`, `Any`, `Contains` |
| **Wybór elementów** | `First`, `FirstOrDefault`, `Last`, `LastOrDefault`, `Single`, `SingleOrDefault`, `ElementAt`, `ElementAtOrDefault`, `DefaultIfEmpty` |
| **Agregacja** | `Aggregate`, `AggregateBy`, `Average`, `Count`, `LongCount`, `Sum`, `Max`, `Min`, `MinBy`, `MaxBy`, `SequenceEqual` |
| **Generowanie** | `Empty`, `Range`, `Repeat`, `Prepend`, `InfiniteSequence` |

#### Mieszane
| Kategoria | Odroczone | Natychmiastowe |
| --- | --- | --- |
| **Konwersja** | `Cast`, `AsEnumerable`, `AsQueryable` | `ToArray`, `ToList`, `ToDictionary`, `ToHashSet` |
| **Grupowanie** | `GroupBy` | `ToLookup` | 

### Szczegółowe wyjaśnienia poszczególnych metod:
- [Wykład z LINQ](https://csharp.mini.pw.edu.pl/pl/lectures/06.collections/linq/#method-overview)
- [Enumerable Class Methods](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate?view=net-10.0)
- [Standard query operators overview](https://learn.microsoft.com/en-us/dotnet/csharp/linq/standard-query-operators/)

> [!WARNING]
> Nie należy używać operacji wymagających dostępu do całej kolekcji (takich jak `OrderBy`) w przypadku **nieskończonych sekwencji**! Jest to równoważne pętli `while(true)` bez warunku przerwania.

### Składnia metod a składnia zapytań 
Jak wspomniano wcześniej, *składnia metod* oraz *składnia zapytań* są kompilowane do tego samego kodu, więc wybór między nimi zależy wyłącznie od czytelności. Warto zaznaczyć, że nie każda metoda dostępna w *składni metod* posiada swój odpowiednik w *składni zapytań*.

Każde zapytanie w *składni zapytań* rozpoczyna się od:
```csharp
from x in source
```
i kończy klauzulą ​​`select` albo `group`.

| Składnia zapytań | Odpowiednik w składni metod |
| --- | --- |
| `where p`                           | `.Where(x => p)`
| `select e`                          | `.Select(x => e)`
| `orderby k`	                      | `.OrderBy(x => k)`
| `orderby k descending`	          | `.OrderByDescending(x => k)`
| `orderby k1, k2`                    | `.OrderBy(x => k1).ThenBy(x => k2)`
| `group x by k`	                  | `.GroupBy(x => k)`
| `join y in other on k1 equals k2`   | `.Join(other, x => k1, y => k2, ...)`

#### Klauzula `let`
Klauzula `let` przypisuje wartość pośrednią do identyfikatora wewnątrz zapytania, dzięki czemu można z niej korzystać ponownie bez konieczności ponownego obliczania.
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

#### Klauzula `into`
Klauzula `into` umożliwia dodanie kolejnych klauzul po `group`, `select` lub `join`, traktując bieżący wynik jako dane wejściowe dla nowego zapytania. W ten sposób *składnia zapytań* łączy kolejne operacje przekształcania danych, które w *składni metod* wyrażono by za pomocą `.Select(...)`.

```csharp
var queryGroupJoin = from x in source
             join y in other on x.key equals y.key into result
             select new { x.key, Result = result };

var methodGroupJoin  = source.GroupJoin(other, x => x.key, y => y.key, 
    (x, result) => new { x.key, Result = result });   
```

#### Wiele klauzul `from`
Można łączyć ze sobą wiele klauzul `from` w celu spłaszczenia kolekcji.

```csharp
var queryFlatten = from x in source
                   from e in x.list
                   select e;

var methodFlatten = source.SelectMany(x => x.list);
```

#### Łączenie obu składni
Nic nie stoi na przeszkodzie, aby łączyć obie składnie w celu uzyskania lepszej czytelności kodu.

```csharp
var result = (from n in numbers where n > 5 select n).Take(3).ToList();
```

### PLINQ
Metoda `AsParallel()` dzieli zapytanie LINQ pomiędzy wątki i wykonuje je równolegle poprzez pulę wątków:
```csharp
var sum = Enumerable.Range(0, 1_000_000)
    .AsParallel()
    .Where(x => ExpensiveCheck(x))
    .Sum();
```
PLINQ najlepiej sprawdza się w przypadku zadań intensywnie obciążających procesor, wykonywanych na dużych kolekcjach znajdujących się w pamięci (w przypadku operacji I/O, zazwyczaj lepszym rozwiązaniem są `async`/`await` oraz `IAsyncEnumerable<T>`). Równoległe przetwarzanie małych kolekcji jest nieopłacalne, ponadto nie wszystkie operatory (np. `OrderBy`, `GroupBy`, `Reverse`) dobrze nadają się do zrównoleglenia.

> [!IMPORTANT]
> W przypadku zapytań obejmujących wiele sekwencji źródłowych, metodę `AsParallel()` należy zastosować do **każdej z nich** - w przeciwnym razie kompilator zastąpi całe zapytanie wariantem sekwencyjnym.

> [!IMPORTANT]
> Nie należy wywoływać `AsParallel()` wielokrotnie. Wynik jest już obiektem typu `ParallelQuery<T>` i ponowne wywołanie może negatywnie wpłynąć na wydajność.

#### Inne metody
- `AsSequential()` – przekształca `ParallelQuery<T>` z powrotem w `IEnumerable<T>`, co pozwala na wykonywanie części zapytania równolegle, a innych sekwencyjnie (przydatne gdy fragment kodu, z którym współpracuje zapytanie, nie jest bezpieczny wątkowo),
- `AsOrdered()` – porządkuje wyniki po wykonaniu, aby zachować ich pierwotną kolejność, kosztem wydajności (domyślnie fragmenty danych są przetwarzane w różnych wątkach i scalane w miarę kończenia przez nie pracy),
- `AsUnordered()` – anuluje działanie `AsOrdered()`,
- `ForAll(Action<T>)` – wywołuje przekazaną delegatę dla każdego elementu, wykonując to **w wątku roboczym**, który wygenerował dany element.


## Zakresy
> [!NOTE]
> Pojęcia wycinania oraz indeksowania powinny być już dobrze znane. Zostały one omówione podczas [wykładu poświęconego tablicom](https://csharp.mini.pw.edu.pl/pl/lectures/03.basics/arrays/).

Zakres (ang. *range*) pozwala określić wycinek sekwencji – od jednej pozycji do drugiej. W języku C# 8 wprowadzono dwa powiązane typy:
- `Index` – reprezentuje pojedynczą pozycję,
- `Range` – reprezentuje parę wartości `Index` (zapisywaną jako `start..end`) i opisuje wycinek sekwencji.

### Przykłady użycia `Index`:
```csharp
Index first = 0;     
Index second = 1;
Index last = ^1;     
Index secondLast = ^2;
```
Dostępna jest również metoda pomocnicza `Index.GetOffset(int length)`, która przelicza obiekt `Index` na konkretny numeryczny indeks (liczony od zera) w oparciu o długość kolekcji:
```csharp
Index i = ^1;
Console.WriteLine(i.GetOffset(5)); // 4, because ^1 = 5-1 = 4
```

### Przykłady użycia `Range`:
```csharp
Range r1 = 0..3;        // elements 0, 1, 2  
Range r2 = 2..^1;       // from index 2 to one before last
Range r3 = ..3;         // from start to index 3 - same as 0..3
Range r4 = 2..;         // from index 2 to the end
Range r5 = ..;          // whole sequence
```
Jak widać, `start` jest inkluzywny (włącza do zakresu), natomiast `end` jest ekskluzywny (wyłącza z zakresu).

### Wycinanie fragmentów
Wycinanie za pomocą zakresów działa w przypadku typów, które posiadają indeksator przyjmujący `Range`:
- tablice jak `T[]`, `T[][]` itd. (przy czym działa to tylko dla najbardziej zewnętrznego wymiaru), ale już nie tablice wielowymiarowe typu `T[,]`,
- `Span<T>` lub `ReadOnlySpan<T>`,
- `string`,

lub posiadających właściwość `Length` / `Count` oraz metodę `Slice(int start, int length)`, jak na przykład `List<T>`.

```csharp
string s = "hello world";
string hello = s[..5];      // "hello"
string world = s[6..];      // "world"
```
Zatem w przypadku własnych typów należy albo zaimplementować indeksator przyjmujący `Range` (`this[Range]`), albo zapewnić interfejs analogiczny do tego, który posiada `List<T>` - wtedy kompilator automatycznie dokona odpowiedniej transformacji kodu.

### Zakresy a LINQ
Poniższa tabela to powód, dla którego w niniejszym samouczku wracamy do tematu zakresów. Jak już wiemy, LINQ również umożliwia wycinanie fragmentów z wymienionych wcześniej typów, jednak wiąże się to z pewnymi kompromisami:

| Zadanie | LINQ | Zakresy (Ranges) | Porównanie |
|---|---|---|---|
| Pobranie pierwszych 5 elementów tablicy `int[]` | `arr.Take(5).ToArray()` | `arr[..5]` | LINQ tworzy enumerator, przechwycenie delegaty oraz tablicę wynikową; zakresy tworzą wyłącznie tablicę wynikową. |
| Wycięcie fragmentu stringa | `string.Concat(s.Skip(1).Take(3))` | `s[1..4]` | Zakresy są kompilowane do zoptymalizowanego obiektu `Substring`; LINQ powoduje alokację enumeratorów dla każdego operatora, wewnętrznych buforów oraz wynikowego, skonkatenowanego ciągu znaków. |
| Wyciągnięcie określonego zakresu z `List<T>` | `list.Skip(100).Take(50).ToList()` | `list[2..7]` | LINQ powoduje alokację dwóch enumeratorów, powiązanych z nimi przechwyceń, buforów pośrednich oraz wynikowej listy; zakresy wymagają jedynie alokacji listy wynikowej, realizowanej za pomocą jednej operacji `Slice` oraz kopiowania danych. |

Podsumowując, LINQ operuje na interfejsie `IEnumerable<T>` oraz na enumeratorach, a to zawsze wiąże się ze znacznie większym narzutem w porównaniu z metodami bardziej bezpośrednimi. Jeśli Twoje źródło danych jest jednym ze wspomnianych wcześniej typów i wykonujesz operację wycinania fragmentu raz, bez żadnych dalszych przekształceń, skorzystaj z zakresów.

> [!IMPORTANT]
> ## Przykładowe zadania z LINQ
{{% details "TASK 3" false %}}
Celem zadania jest napisanie 5 zapytań przy użyciu LINQ z prosta bazą danych sklepu internetowego.

Zadania 1–4 należy zrealizować w formie pojedynczych zapytań. Zadanie 5 można wykonać za pomocą kilku powiązanych ze sobą zapytań (zapis w formie pojedynczego zapytania również jest możliwy, jednak dość skomplikowany).

Dla większości zapytań należy utworzyć nową klasę anonimową, zawierającą tylko te pola, które są niezbędne na danym etapie. Pola te powinny wynikać z oczekiwanego rezultatu danego zapytania.

W pliku `Database.cs` znajdują się klasy reprezentujące tabele w bazie danych (`Customer`, `LoyaltyCard`, `Product`, `Order`, `OrderProduct`), a klasa `Database` zawiera listy danych odpowiadające wspomnianym tabelom.

**Uwaga**: pojedyncze zamówienie może zawierać kilka produktów - takim przypadku w tabeli `OrderProducts` znajduje się kilka wierszy z tym samym identyfikatorem zamówienia.

Należy modyfikować wyłącznie plik `Program.cs`.

1. Wypisz osoby (w kolejności alfabetycznej według nazwiska, a następnie imienia), które nie posiadają karty lojalnościowej.

2. Na podstawie numeru PESEL wybierz osoby, które mają ukończone 29 lat (biorąc pod uwagę rok urodzenia). Wyświetl te osoby, zaczynając od najstarszej.

3. Wskaż 3 osoby, które korzystają z usług sklepu najdłużej.

4. Wskaż 5 produktów, dla których łączna liczba zamówionych sztuk jest największa.

5. Dla każdej osoby oblicz łączną kwotę wydaną w sklepie (uwzględniając zniżkę przysługującą posiadaczom karty lojalnościowej). Zestawienie powinno być posortowane alfabetycznie według nazwiska, a następnie imienia klienta.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task3" >}}
{{% /details %}}

{{% details "TASK 4" false %}}
Dostępna jest baza danych składająca się z trzech tabel: `database.Movies`, `database.Users` oraz `database.Ratings`. Szczegółowe informacje na temat dostępnych kolumn można znaleźć, analizując klasy `Movie`, `User` oraz `RatingEntry`.

W tabeli `Users` pole `Age` nie oznacza bezpośrednio wieku, lecz identyfikator dolnego przedziału wiekowego (np. wartość 1 odpowiada przedziałowi 1–18 lat). Każdy film posiada unikalny identyfikator `MovieID`, a każdy użytkownik – unikalny identyfikator `UserID`. Tabele `Movies` oraz `Users` są powiązane za pomocą ocen: pojedyncza ocena filmu jest wystawiana przez konkretnego użytkownika.

Należy modyfikować wyłącznie plik `Program.cs`.

1. Oblicz liczbę użytkowników w poszczególnych grupach wiekowych; wyniki posortuj rosnąco według wieku.

2. Wyznacz najwcześniejszy rok produkcji filmu obejrzanego przez użytkowników w grupach określonych według wieku i płci; wyniki posortuj rosnąco według tego roku.

3. Zlicz wystąpienia poszczególnych gatunków filmowych. Każdy gatunek liczony jest osobno (np. w przypadku wpisu `"Horror|Comedy"` doliczamy +1 do gatunku Horror oraz +1 do gatunku Comedy). Wyniki posortuj rosnąco według nazwy gatunku.

4. Remake’i danego filmu definiujemy jako zbiór filmów o tym samym tytule, ale różnych latach produkcji. Zestawienie powinno zawierać wszystkie filmy będące remake’ami (czyli takie, dla których istnieje co najmniej jeden inny film o tym samym tytule). Wyniki posortuj rosnąco według roku produkcji i ogranicz do pierwszych 14 pozycji.

5. Posortuj filmy malejąco według średniej oceny. Uwzględnij tylko te filmy, które otrzymały co najmniej 100 ocen. Zestawienie powinno zawierać pierwsze 20 pozycji.

{{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Task4" >}}
{{% /details %}}

## Więcej na ten temat:
- [Indices and ranges](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes)
- [Language Integrated Query (LINQ)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)

{{% details "**Dodatkowe materiały na warsztaty**" false %}}
Materials: {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/Lab" >}}
Solution: {{< filetree dir="labs/w05.lambdas-enumerable-linq-extensions/LabSolution" >}}
{{% /details %}}