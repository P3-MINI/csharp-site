---
title: "Projekt03"
weight: 30
---

# MiniDI: kontener wstrzykiwania zależności i generator źródeł

**Suma punktów:** 8

> [!NOTE]
> ## Kod początkowy
> {{< filetree dir="projects/project03/student" >}}

## 1. Przegląd
W tym zadaniu zaimplementujesz **MiniDI**, niewielki kontener wstrzykiwania zależności (*dependency injection*, DI) wzorowany na `Microsoft.Extensions.DependencyInjection` (kontenerze używanym przez ASP.NET Core). Usługi (*services*) rejestruje się w `ServiceCollection`; zbudowany z niej `ServiceProvider` tworzy obiekty razem ze wszystkimi zależnościami przekazywanymi przez konstruktor oraz zarządza ich czasem życia (singleton, scoped, transient) i zwalnianiem.

Kontener zbudujesz dwukrotnie: najpierw z użyciem **refleksji** (konstruktory są wyszukiwane i wywoływane w czasie działania programu, a klasy znajdowane przez przeszukanie assembly pod kątem atrybutów), a następnie z użyciem **inkrementalnego generatora źródeł**, który znajduje te same klasy w czasie kompilacji i generuje zwykłe wyrażenia `new`, dzięki czemu w czasie działania refleksja nie jest potrzebna. C++ nie ma refleksji w czasie wykonania, dlatego biblioteki DI dla C++ opierają się na szablonach, czyli działają w czasie kompilacji, podobnie jak część 4.

Powiązane wykłady: [Refleksja]({{< ref "/lectures/08.reflection/reflection" >}}), [Atrybuty]({{< ref "/lectures/08.reflection/attributes" >}}), [Assembly]({{< ref "/lectures/08.reflection/assembly" >}}), [Typy generyczne]({{< ref "/lectures/04.creating-types/generics" >}}), [Disposable]({{< ref "/lectures/05.csharp-features/disposable" >}}), [Generatory źródeł]({{< ref "/lectures/13.others/source-generators" >}}).

Rozwiązanie `MiniDI.slnx` zawiera:

| Projekt | Zawartość |
|---|---|
| `MiniDI` | Biblioteka kontenera (`net10.0`). |
| `MiniDI.Generator` | Inkrementalny generator źródeł (`netstandard2.0`, Roslyn 4.8). |
| `MiniDI.Demo` | Aplikacja konsolowa (mały sklep) korzystająca z kontenera. Odwołuje się do generatora jako analizatora, co jest już skonfigurowane w `MiniDI.Demo.csproj`. |
| `MiniDI.Tests` | Testy xUnit do wszystkich części. Testy generatora kompilują w pamięci małe programy i uruchamiają na nich generator. |

Poniższy kod jest **dostarczony** i nie wolno go zmieniać: `Lifetime`, `ServiceAttribute`, `InjectAttribute`, wyjątki w `Exceptions.cs`, `TypeNameFormatter`, `ISupportRequiredService` i `ServiceProviderExtensions` (generyczne `GetService<T>()`/`GetRequiredService<T>()`); w generatorze: potok w metodzie `Initialize`, `EquatableArray<T>`, `LocationInfo`, `DiagnosticDescriptors`, `IsExternalInit`; demo (poza `Benchmarks.cs`) oraz testy.

Zasady ogólne:
1. Nie zmieniaj publicznego API używanego przez testy i demo. Możesz dodawać dowolne prywatne/wewnętrzne składowe i typy oraz zmieniać konstruktory `internal` klas `ServiceProvider` i `ServiceScope`.
2. Wszystkie testy muszą przechodzić (`dotnet test`). Z kodem początkowym nie przechodzi żaden test poza trzema trywialnymi testami generatora. Testy nie sprawdzają wszystkiego: oceniane są wszystkie poniższe wymagania.
3. Nie używaj żadnej biblioteki DI; projekt `MiniDI` nie może odwoływać się do żadnych pakietów NuGet.

Terminologia: **typ usługi** (*service type*) to to, o co się prosi (`IClock`); **rejestracja** (`ServiceDescriptor`) mówi, jak usługę utworzyć; **dostawca główny** (*root provider*) jest tworzony przez `BuildServiceProvider()`; **zakres** (*scope*) jest tworzony przez `CreateScope()` (np. jedno żądanie HTTP).

## 2. Wymagania

### Część 1: API rejestracji i czasy życia [2 Punkty]
**Pliki:** `MiniDI/ServiceDescriptor.cs`, `MiniDI/ServiceCollection.cs`, `MiniDI/ServiceProvider.cs`, `MiniDI/ServiceScope.cs`

1.  **Deskryptory:** `ServiceDescriptor` ma trzy konstruktory: rejestrację typu `(serviceType, implementationType, lifetime)`, instancji `(serviceType, instance)` (zawsze `Singleton`) oraz fabryki `(serviceType, factory, lifetime)`. Ustawiona jest dokładnie jedna z właściwości `ImplementationType`, `ImplementationInstance`, `ImplementationFactory`. Konstruktory sprawdzają argumenty:
    *   argument `null`: `ArgumentNullException`;
    *   typ implementacji, który nie jest nieabstrakcyjną klasą (interfejs, klasa abstrakcyjna lub statyczna, struktura) albo nie jest przypisywalny do typu usługi, lub instancja, która nie jest instancją typu usługi: `ArgumentException`;
    *   **otwarte typy generyczne:** typ usługi będący definicją typu generycznego (`typeof(IRepository<>)`) jest dozwolony tylko w rejestracji typu, której implementacja jest definicją typu generycznego o tej samej liczbie parametrów typu, implementującą typ usługi (lub dziedziczącą po nim) z własnymi parametrami typu w tej samej kolejności, np. `class Repository<T> : IRepository<T>`. Każda inna kombinacja (otwarta usługa z zamkniętą lub niegeneryczną implementacją, zamknięta usługa z otwartą implementacją, instancja lub fabryka dla otwartej usługi) rzuca `ArgumentException`.
2.  **`ServiceCollection`:** Uporządkowana lista deskryptorów (`IReadOnlyList<ServiceDescriptor>`; `Add` jest dostarczona). Zaimplementuj przeciążenia `AddSingleton`/`AddScoped`/`AddTransient` (typ → typ, `<TService, TImplementation>`, rejestracja klasy jako siebie samej `<TService>`, fabryka; dla singletonów także instancja). Każde z nich dodaje jeden deskryptor i zwraca tę samą kolekcję. Ponowna rejestracja typu usługi nie usuwa wcześniejszych rejestracji.
3.  **Budowanie:** `BuildServiceProvider()` tworzy dostawcę głównego z **kopii** (migawki) kolekcji; późniejsze zmiany kolekcji nie wpływają na dostawcę.
4.  **Rozwiązywanie:** `GetService(Type)` zwraca `null`, gdy żadna rejestracja nie pasuje; `GetRequiredService(Type)` rzuca wtedy `ServiceNotRegisteredException`. Jeśli pasuje kilka rejestracji, **wygrywa ostatnia**. W tej części wystarczy tworzyć usługi rejestrowane typem przez publiczny konstruktor bezparametrowy (część 2 to uogólnia).
5.  **Czasy życia:** Instancje są tworzone leniwie, przy pierwszym żądaniu:

    | Czas życia | Instancja | Właściciel (zwalnia ją) |
    |---|---|---|
    | `Singleton` | jedna na dostawcę głównego i rejestrację (dla otwartej rejestracji generycznej: na zamknięty typ), wspólna dla wszystkich zakresów | dostawca główny |
    | `Scoped` | jedna na zakres i rejestrację; rozwiązanie jej z dostawcy głównego rzuca `CaptiveDependencyException` z `ConsumerType == null` | zakres |
    | `Transient` | nowa instancja przy każdym żądaniu | zakres (lub dostawca główny), z którego ją rozwiązano |

    Dwie rejestracje tego samego typu implementacji to dwa osobne singletony.
6.  **Fabryki:** Fabryka otrzymuje dostawcę, który tworzy instancję: dostawcę głównego dla singletonów, a w pozostałych przypadkach zakres (lub dostawcę głównego), z którego rozwiązywana jest usługa. Wszystkie zależności singletonu są rozwiązywane z dostawcy głównego (także w części 2), więc ich właścicielem jest dostawca główny.
7.  **Otwarte typy generyczne:** Dla zamkniętego typu, np. `IRepository<User>`, najpierw używane są rejestracje zamknięte (dokładne dopasowanie). Dopiero gdy ich nie ma, ostatnia otwarta rejestracja `IRepository<>` jest domykana za pomocą `MakeGenericType` (`Repository<User>`). Rejestracja, której ograniczeń generycznych argumenty typu nie spełniają, jest pomijana, tak jakby nie istniała.
8.  **Bezpieczeństwo wątkowe:** Dostawca główny może być używany przez wiele wątków jednocześnie. Konstruktor/fabryka singletonu wykonuje się **dokładnie raz**, nawet gdy wiele wątków jednocześnie żąda go po raz pierwszy. Jeśli rzuci wyjątek, nic nie jest zapamiętywane i kolejne żądanie próbuje ponownie. Zakres jest używany przez jeden wątek naraz i nie musi być bezpieczny wątkowo.
9.  **Zwalnianie:** Dostawca/zakres zwalnia instancje `IDisposable`/`IAsyncDisposable`, których jest właścicielem (zob. tabela; instancje przekazane do `AddSingleton(instance)` należą do użytkownika i nigdy nie są zwalniane). `Dispose()`/`DisposeAsync()` zwalniają je w **odwrotnej kolejności utworzenia**. Instancja jest uznawana za utworzoną, gdy jej konstruktor lub fabryka zakończy działanie, więc usługa jest zwalniana przed swoimi zależnościami.
    *   `DisposeAsync()` wywołuje `DisposeAsync()` na instancjach `IAsyncDisposable`, a `Dispose()` na pozostałych. `Dispose()` rzuca `InvalidOperationException`, jeśli posiadana instancja implementuje tylko `IAsyncDisposable`.
    *   Drugie wywołanie nic nie robi. Po zwolnieniu `GetService`, `GetRequiredService` i `CreateScope` rzucają `ObjectDisposedException`. Zwolnienie dostawcy głównego nie zwalnia zakresów.

### Część 2: Rozwiązywanie zależności przez refleksję [2 Punkty]
**Pliki:** `MiniDI/ServiceProvider.cs`, `MiniDI/ServiceScope.cs` (oraz własne typy wewnętrzne)

1.  **Wybór konstruktora** (dla rejestracji typu, stosowany do zamkniętego typu implementacji):
    1.  Kandydatami są tylko **publiczne konstruktory instancyjne**. Jeśli niektóre z nich są oznaczone `[Inject]`, kandydatami są tylko one; więcej niż jeden konstruktor z `[Inject]` rzuca `AmbiguousConstructorException`.
    2.  Parametr jest **rozwiązywalny**, jeśli dla jego typu istnieje pasująca rejestracja (zamknięta lub otwarta generyczna), jeśli jest typu `IEnumerable<T>` albo jeśli ma wartość domyślną (`ParameterInfo.HasDefaultValue`). To płytkie sprawdzenie: nic nie jest tworzone i zależności zależności nie są badane.
    3.  Spośród kandydatów, których wszystkie parametry są rozwiązywalne, wybierz ten z **największą liczbą parametrów**. Jeśli nie ma żadnego, rzuć `NoSuitableConstructorException`; jeśli dwa lub więcej mają tę samą liczbę parametrów, rzuć `AmbiguousConstructorException` (`Candidates` = te konstruktory).
    4.  Wybór zależy tylko od rejestracji: wyznacz go raz dla każdego typu implementacji i zapamiętaj (w sposób bezpieczny wątkowo). Nie przeszukuj konstruktorów przy każdym rozwiązywaniu.
2.  **Argumenty:** Parametr, którego typ jest zarejestrowany (lub jest `IEnumerable<T>`), jest rozwiązywany rekurencyjnie przez tego samego dostawcę/zakres, zgodnie z zasadami czasu życia; w przeciwnym razie używana jest jego wartość domyślna. Parametr opcjonalny, którego typ jest zarejestrowany, jest rozwiązywany, a nie przyjmuje wartości domyślnej.
3.  **Wyjątki:** Wyjątki rzucone przez konstruktory i fabryki propagują się bez zmian, tzn. nie są opakowane w `TargetInvocationException` (wskazówka: `BindingFlags.DoNotWrapExceptions`). `ResolutionException` z zagnieżdżonej zależności również nie jest opakowywany.
4.  **`IEnumerable<T>`:** `GetService(typeof(IEnumerable<T>))` zwraca `T[]` z jednym elementem na każdą pasującą rejestrację (zamkniętą i otwartą generyczną), w kolejności rejestracji, każdy utworzony zgodnie ze swoim czasem życia. Bez rejestracji zwraca pustą tablicę (nigdy `null`).
5.  **Ścieżka rozwiązywania:** Dostawca śledzi łańcuch trwających żądań usług w bieżącym wątku, także żądań wykonywanych z wnętrza fabryk (np. `sp => new A(sp.GetRequiredService<B>())`) oraz żądań `IEnumerable<T>`. Każdy `ResolutionException` otrzymuje ścieżkę: typy żądanych usług od najbardziej zewnętrznego żądania do tego, które się nie powiodło, np. `[IOrderService, PriceCalculator, IDiscountRule]`. Dostarczone klasy wyjątków umieszczają ją w komunikacie (`... Resolution path: A -> B -> A.`).
6.  **Zależności cykliczne:** Żądanie typu usługi, który jest już w łańcuchu, rzuca `CircularDependencyException`; ścieżka kończy się powtórzonym typem (`[A, B, A]`). Cykl nigdy nie może się skończyć `StackOverflowException`.
7.  **Zależności uwięzione (*captive dependencies*):** Żądanie usługi scoped, gdy w łańcuchu jest singleton (również pośrednio, np. singleton → transient → scoped), rzuca `CaptiveDependencyException` z `ConsumerType` = najbliższy singleton w łańcuchu. W przeciwnym razie żądanie usługi scoped z dostawcy głównego rzuca `CaptiveDependencyException` z `ConsumerType == null`. Usługa scoped może zależeć od singletonów.

### Część 3: Atrybuty, przeszukiwanie assembly i walidacja [2 Punkty]
**Pliki:** `MiniDI/ServiceCollectionScanningExtensions.cs`, `MiniDI/ServiceCollection.cs` (oraz własne typy wewnętrzne)

1.  **Atrybut:** `[Service]` (dostarczony) oznacza klasę jako usługę: `[Service(Lifetime.Scoped, As = typeof(IFoo))]`. Domyślny czas życia to `Transient`, a domyślne `As` to sama klasa. Atrybut ma `AllowMultiple = true` (jedna rejestracja na atrybut) i `Inherited = false`.
2.  **Przeszukiwanie:** `AddFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)`:
    *   rejestruje każdą klasę z assembly (publiczną lub nie), na której bezpośrednio zadeklarowano `[Service]` (atrybut nie jest dziedziczony) i którą akceptuje `filter` (jeśli podano);
    *   przetwarza klasy **posortowane według `Type.FullName`** porównaniem porządkowym (`StringComparer.Ordinal`; kolejność atrybutów jednej klasy jest nieokreślona), dzięki czemu kolejność `IEnumerable<T>` jest deterministyczna;
    *   dla każdego atrybutu dodaje rejestrację typu `As ?? klasa` → klasa z podanym czasem życia; dla klasy generycznej jest to otwarta rejestracja generyczna (`[Service(As = typeof(IRepository<>))] class Repository<T>`);
    *   dla niepoprawnych kombinacji (klasa abstrakcyjna, niezaimplementowane `As`, ...) przepuszcza `ArgumentException` z `ServiceDescriptor`;
    *   zwraca tę samą kolekcję.
3.  **Walidacja:** `BuildServiceProvider(validateOnBuild: true)` sprawdza kontener, zanim go zwróci:
    *   Dla każdej rejestracji typu z zamkniętym typem usługi, w kolejności rejestracji, zasymuluj rozwiązywanie **bez tworzenia czegokolwiek** (nie może się wykonać żaden konstruktor ani fabryka): wybór konstruktora, rekurencyjnie wszystkie zależności (`IEnumerable<T>`: każda pasująca rejestracja) oraz wykrywanie zależności cyklicznych i uwięzionych, z tymi samymi zasadami, wyjątkami i ścieżkami co w części 2. Rejestracja jest sprawdzana tak, jakby była rozwiązywana z zakresu (dla singletonu reguła zależności uwięzionych dotyczy jego zależności).
    *   Rejestracje fabryk, instancji i otwarte rejestracje generyczne nie są sprawdzane same w sobie (otwarta rejestracja generyczna jest sprawdzana, gdy domyka ją któraś zależność). Jako zależności rejestracje fabryk i instancji są liśćmi: nie są analizowane, ale liczy się ich czas życia (singleton zależny od rejestracji fabryki scoped jest zależnością uwięzioną).
    *   Weź pierwszy błąd znaleziony dla każdej błędnej rejestracji i rzuć `ContainerValidationException` z tymi błędami w kolejności rejestracji. Jeśli nie ma błędów, zwróć dostawcę.
    *   Bez walidacji (domyślnie) te błędy pojawiają się dopiero podczas rozwiązywania.

### Część 4: Generator źródeł [2 Punkty]
**Pliki:** `MiniDI.Generator/ServiceRegistrationGenerator.cs` (oraz własne typy modelu), `MiniDI.Demo/Benchmarks.cs`, `README.md`

1.  **Potok:** Potok jest dostarczony w `Initialize`: `ForAttributeWithMetadataName("MiniDI.ServiceAttribute")` → `Analyze` dla każdej klasy → `Collect()` → `Emit`. Zachowaj nazwę kroku `ServiceModels` (`WithTrackingName`), której używa jeden z testów.
2.  **Model:** `Analyze` przekształca symbol klasy w model **porównywalny przez wartość** (rozbuduj rekord `ServiceClassModel`; używaj rekordów, napisów, liczb, `EquatableArray<T>`, `LocationInfo`), który nigdy nie przechowuje `ISymbol`, `SyntaxNode`, `Location` ani `Compilation`. Test dodaje metodę do klasy usługi i sprawdza, że modele są `Cached`/`Unchanged`.
3.  **Generowany kod:** Generator zawsze (nawet gdy nie ma żadnych usług) dodaje plik zawierający:
    ```csharp
    namespace MiniDI.Generated
    {
        internal static class GeneratedServices
        {
            public static global::MiniDI.ServiceCollection AddGenerated(this global::MiniDI.ServiceCollection services)
            {
                // one registration per [Service] attribute
                return services;
            }
        }
    }
    ```
    Klasy są przetwarzane w tej samej kolejności co w `AddFromAssembly`: posortowane porównaniem porządkowym według nazw metadanych, które są równe `Type.FullName` (np. `Shop.Outer+Inner`, ``Shop.Repository`1``).
4.  **Fabryki bez refleksji:** Klasa niegeneryczna jest rejestrowana fabryką, np. `new ServiceDescriptor(typeof(global::Shop.IClock), static sp => new global::Shop.FixedClock(), Lifetime.Singleton)`. Konstruktor jest wybierany w czasie kompilacji: ten z `[Inject]`, jeśli istnieje, a w przeciwnym razie jedyny publiczny konstruktor. Argumenty:
    *   parametr bez wartości domyślnej (także `IEnumerable<T>`): `global::MiniDI.ServiceProviderExtensions.GetRequiredService<T>(sp)`;
    *   parametr z wartością domyślną: usługa, jeśli `sp.GetService(typeof(T))` zwraca wartość różną od `null`, w przeciwnym razie wartość domyślna. Wystarczy obsłużyć `null`/`default` oraz stałe typu `bool`, liczbowe, `char`, `string` i wyliczeniowe (pomocne jest `SymbolDisplay.FormatPrimitive`).

    Wygenerowany kod nie może używać refleksji (`Activator`, `GetConstructor`, `Invoke`, ...); `typeof` jest dozwolone. Używaj w pełni kwalifikowanych nazw (`global::...`), aby kod kompilował się niezależnie od dyrektyw `using` użytkownika.
5.  **Powrót do refleksji:** Klasa generyczna (otwarta usługa generyczna) oraz klasa z kilkoma publicznymi konstruktorami bez `[Inject]` (ostrzeżenie MDI005) są rejestrowane typem (np. `typeof(global::Shop.Repository<>)`), czyli tworzone przez refleksję w czasie działania programu.
6.  **Diagnostyki:** Zgłaszaj je przez `SourceProductionContext.ReportDiagnostic` z użyciem dostarczonych `DiagnosticDescriptors`, z lokalizacją na nazwie klasy (`ClassDeclarationSyntax.Identifier`); `{0}` to nazwa klasy (`ISymbol.ToDisplayString()`). Klasa z błędem nie otrzymuje rejestracji.

    | Id | Poziom | Kiedy |
    |---|---|---|
    | MDI001 | Error | klasa nie ma publicznego konstruktora |
    | MDI002 | Error | więcej niż jeden publiczny konstruktor jest oznaczony `[Inject]` |
    | MDI003 | Error | klasa nie implementuje `As` (`{1}`) ani po nim nie dziedziczy; dla otwartych typów generycznych ta sama reguła co w części 1 |
    | MDI004 | Error | klasa jest abstrakcyjna lub statyczna albo ona lub typ ją zawierający nie jest ani publiczny, ani wewnętrzny (`internal`) |
    | MDI005 | Warning | klasa niegeneryczna ma kilka publicznych konstruktorów i żaden nie jest oznaczony `[Inject]` |

7.  **Demo:** `MiniDI.Demo` domyślnie używa wygenerowanych rejestracji, a z argumentem `reflection` używa `AddFromAssembly`. Oba uruchomienia muszą wypisać to samo (poza pierwszym wierszem):
    ```text
    === MiniDI demo (generated): 9 registrations ===
    -- request for alice --
      [OrderService] request #1: order #1 for alice, discounts: weekend -5%
      Receipt #1 (alice, 2026-01-17 10:30)
          1 x keyboard      249.00 PLN
          2 x mouse         199.00 PLN
        total               425.60 PLN
      OrderService of request #1 disposed asynchronously
      RequestContext #1 disposed
    -- request for bob --
      [OrderService] request #2: order #2 for bob, discounts: bulk -10%, weekend -5%
      Receipt #2 (bob, 2026-01-17 10:30)
         12 x cable         180.00 PLN
        total               153.00 PLN
      OrderService of request #2 disposed asynchronously
      RequestContext #2 disposed
    -- 2 orders in the singleton repository --
    CaptiveDependencyException: Cannot resolve scoped service 'IOrderService' from the root provider. Resolution path: IOrderService.
    ```
8.  **Pomiar wydajności:** Zaimplementuj `Benchmarks.RunAsync` (wystarczy `Stopwatch`; dodatkowo możesz użyć BenchmarkDotNet). Dla podanego wariantu (`reflection` lub `generated`) zmierz: (a) rejestrację + `BuildServiceProvider()`, (b) pierwsze rozwiązanie `IOrderService` w nowym zakresie, (c) średni czas jednego rozwiązania `PriceCalculator` z zakresu (co najmniej 1 000 000 rozwiązań po rozgrzewce). Każdy wariant uruchom w osobnym procesie, w konfiguracji Release. W katalogu głównym rozwiązania napisz `README.md` z opisem maszyny i systemu, tabelą wyników i kilkoma zdaniami wyjaśniającymi różnice, w tym pomiary, w których wariant generowany **nie** jest szybszy, i dlaczego.

> [!TIP]
> Testy generatora uruchamiają go w tym samym procesie, więc można go debugować, ustawiając punkty przerwania w testach. Kod wygenerowany dla demo jest zapisywany w `MiniDI.Demo/obj/Debug/net10.0/generated/` (włączone jest `EmitCompilerGeneratedFiles`). IDE może trzymać załadowaną starą wersję generatora; jeśli wygenerowany kod wygląda na nieaktualny, zbuduj projekt z linii poleceń lub uruchom IDE ponownie.

### Co jest oceniane na obronie
*   Wyjaśnianie i modyfikowanie własnego kodu: wybór konstruktora, otwarte typy generyczne (`MakeGenericType`), czasy życia i własność instancji, łańcuch rozwiązywania, wykrywanie cykli i zależności uwięzionych, bezpieczeństwo wątkowe singletonów, kolejność zwalniania.
*   Potok inkrementalny: dlaczego modele muszą być porównywalne przez wartość i co jest (lub nie jest) wykonywane ponownie po edycji.
*   Interpretacja wyników pomiarów wydajności.

## 3. Budowanie

```bash
dotnet build
dotnet test
dotnet run --project MiniDI.Demo                     # generated registrations
dotnet run --project MiniDI.Demo -- reflection       # assembly scanning
dotnet run -c Release --project MiniDI.Demo -- bench reflection
dotnet run -c Release --project MiniDI.Demo -- bench generated
```
