---
title: "Serializacja"
weight: 60
---

# Serializacja

Serializacja to proces konwersji obiektu (jego aktualnego stanu w pamięci) do formatu, który można łatwo zapisać lub przesłać. Może to być format binarny albo tekstowy, taki jak `JSON` lub `XML`.

Główne zastosowania serializacji to:

1. **Trwałe przechowywanie:** Umożliwia zapisanie obiektu na dysku (w pliku) lub w bazie danych. Po ponownym uruchomieniu aplikacji można wczytać te dane i odtworzyć obiekt w takim samym stanie, w jakim był w momencie zapisu.
2. **Komunikacja:** Pozwala na przesyłanie obiektów. Jedna aplikacja serializuje obiekt i wysyła go, a druga odbiera dane i deserializuje je, czyli odtwarza oryginalny obiekt.

Procesem odwrotnym do serializacji jest deserializacja - czyli odtwarzanie obiektu z jego zapisanej (zserializowanej) formy.

Biblioteka standardowa dostarcza mechanizmy serializacji zarówno do formatu `JSON`, jak i `XML` poprzez klasy `JsonSerializer` i `XmlSerializer`, dostępne odpowiednio w przestrzeniach nazw `System.Text.Json` i `System.Xml.Serialization`. Popularna jest również zewnętrzna implementacja serializacji do formatu `JSON` dostępna w pakiecie NuGet `Newtonsoft.Json`. Jest ona wciąż często używana, oferuje więcej funkcji i mniej restrykcyjne ustawienia domyślne, jednak jest mniej wydajna.

Obydwa podejścia oferują "automagiczny" mechanizm serializacji oparty na **refleksji**. Metoda serializacji przyjmuje tylko obiekt do serializacji i w czasie wykonania odczytuje jego pola i właściwości i na tej podstawie go serializuje. Deserializacja przyjmuje łańcuch znaków i obiekt reprezentujący deserializowany typ, a następnie za pomocą refleksji tworzy jego instancję i ją inicjalizuje. Na proces serializacji i deserializacji można wpływać, ozdabiając klasy i ich składowe atrybutami.

## System.Text.Json

Serializacja odbywa się za pomocą statycznej klasy `JsonSerializer`.

### Serializacja

```csharp
var options = new JsonSerializerOptions { WriteIndented = true };
var serialized = JsonSerializer.Serialize(forecast, options);
```

```json
{
  "Location": "Warsaw",
  "Temperatures": {
    "2025-12-07T22:16:44.6735555+01:00": 3.463462782637311,
    "2025-12-07T23:16:44.6735555+01:00": -19.053981902530726,
    "2025-12-08T00:16:44.6735555+01:00": -1.900355712285556,
    "2025-12-08T01:16:44.6735555+01:00": 0.549260294866361,
    "2025-12-08T02:16:44.6735555+01:00": 8.865189862795464,
    "2025-12-08T03:16:44.6735555+01:00": -18.058228230029254,
    "2025-12-08T04:16:44.6735555+01:00": 13.068063094521058,
    "2025-12-08T05:16:44.6735555+01:00": 27.008561724678614,
    "2025-12-08T06:16:44.6735555+01:00": 7.273621688485271,
    "2025-12-08T07:16:44.6735555+01:00": 27.049612806241868,
    "2025-12-08T08:16:44.6735555+01:00": 23.570338186867424,
    "2025-12-08T09:16:44.6735555+01:00": -11.79202963667603,
    "2025-12-08T10:16:44.6735555+01:00": 19.010310158995928,
    "2025-12-08T11:16:44.6735555+01:00": 24.2806090207253,
    "2025-12-08T12:16:44.6735555+01:00": 15.272542969051628,
    "2025-12-08T13:16:44.6735555+01:00": -15.570638202617841,
    "2025-12-08T14:16:44.6735555+01:00": -13.395053669977223,
    "2025-12-08T15:16:44.6735555+01:00": -18.17296011210254,
    "2025-12-08T16:16:44.6735555+01:00": 12.321830976357617,
    "2025-12-08T17:16:44.6735555+01:00": -18.447507645769615,
    "2025-12-08T18:16:44.6735555+01:00": -13.383437152501326,
    "2025-12-08T19:16:44.6735555+01:00": 28.467406712047328,
    "2025-12-08T20:16:44.6735555+01:00": -16.137085680790847,
    "2025-12-08T21:16:44.6735555+01:00": -6.799952301806021
  },
  "Summary": [
    "Hot",
    "Windy"
  ]
}
```

### Deserializacja

```csharp
Forecast? deserialized = JsonSerializer.Deserialize<Forecast>(serialized);
```

### Atrybuty

```csharp
public class Person
{
    [JsonPropertyName("FirstName")]
    public string Name { get; set; }
    
    [JsonInclude] // Needed for fields (not serialized by default) and non-public members
    private DateTime DateOfBirth;
    
    [JsonIgnore]
    public int Age { get; set; }
}
```

### Opcje

Serializator konfiguruje się obiektem `JsonSerializerOptions`. Jego ustawienia domyślne są dość restrykcyjne i różnią się od tych w `Newtonsoft.Json`: nazwy właściwości są zapisywane dokładnie tak, jak zadeklarowano je w C#, podczas deserializacji są dopasowywane **z rozróżnianiem wielkości liter**, pola są pomijane (chyba że ustawiono `IncludeFields`), a komentarze, przecinki po ostatnim elemencie i liczby zapisane jako łańcuchy znaków nie są akceptowane. Właściwości JSON, które nie pasują do żadnej składowej, są po cichu pomijane:

```csharp
public record User(string Name, int Age);
```

```csharp
string json = """{"name":"Ann","age":"25"}""";
User? p1 = JsonSerializer.Deserialize<User>(json);
// User { Name = , Age = 0 }
User? p2 = JsonSerializer.Deserialize<User>(json, JsonSerializerOptions.Web);
// User { Name = Ann, Age = 25 }
Console.WriteLine(JsonSerializer.Serialize(p2, JsonSerializerOptions.Web));
// {"name":"Ann","age":25}
```

Gotowe instancje tylko do odczytu są dostępne jako właściwości statyczne: `JsonSerializerOptions.Default`, `JsonSerializerOptions.Web` (.NET 9; nazwy w notacji camelCase, dopasowywanie bez rozróżniania wielkości liter i liczby odczytywane z łańcuchów znaków – ustawienia domyślne ASP.NET Core) oraz `JsonSerializerOptions.Strict` (.NET 10, opisane poniżej).

> [!TIP]
> Instancja `JsonSerializerOptions` przechowuje w pamięci podręcznej metadane serializowanych typów. Należy ją utworzyć raz i używać wielokrotnie (np. przechowywać w polu `static readonly`), zamiast tworzyć nową instancję przy każdym wywołaniu.

Domyślnie deserializator jest pobłażliwy wobec niekompletnych lub niepoprawnych danych. Nowsze wersje .NET dodały opcje, które czynią go bardziej restrykcyjnym:

- `RespectNullableAnnotations` (.NET 9) – adnotacje dopuszczalności `null` dla typów referencyjnych są egzekwowane: deserializacja `null` do właściwości lub parametru konstruktora, które nie dopuszczają `null` (np. `string`), zgłasza wyjątek `JsonException`; tak samo serializacja wartości `null` z takiej właściwości.
- `RespectRequiredConstructorParameters` (.NET 9) – brak w JSON właściwości odpowiadającej parametrowi konstruktora bez wartości domyślnej zgłasza wyjątek `JsonException` (domyślnie parametr otrzymuje wartość `default`). Składowe oznaczone modyfikatorem `required` lub atrybutem `[JsonRequired]` są wymagane zawsze.
- `AllowDuplicateProperties = false` (.NET 10) – obiekt z powtórzoną nazwą właściwości zgłasza wyjątek `JsonException`. Domyślnie wygrywa ostatnia wartość, co można wykorzystać do ataku, gdy różne części systemu interpretują ten sam dokument w różny sposób.
- `UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow` (.NET 8) – właściwości JSON, które nie pasują do żadnej składowej, zgłaszają wyjątek `JsonException` zamiast być pomijane.

Gotowa instancja `JsonSerializerOptions.Strict` (.NET 10) włącza wszystkie powyższe opcje (i zachowuje rozróżnianie wielkości liter). Każde z poniższych wywołań kończy się sukcesem przy opcjach domyślnych, ale ze `Strict` zgłasza wyjątek `JsonException`:

```csharp
var strict = JsonSerializerOptions.Strict;
JsonSerializer.Deserialize<User>("""{"Name":"Ann"}""", strict);
// JSON deserialization for type 'User' was missing required properties including: 'Age'.
JsonSerializer.Deserialize<User>("""{"Name":null,"Age":25}""", strict);
// The constructor parameter 'Name' on type 'User' doesn't allow null values. (...)
JsonSerializer.Deserialize<User>("""{"Name":"Ann","Name":"Bob","Age":25}""", strict);
// Duplicate property 'Name' encountered during deserialization of type 'User'.
JsonSerializer.Deserialize<User>("""{"Name":"Ann","Age":25,"Email":"ann@example.com"}""", strict);
// The JSON property 'Email' could not be mapped to any .NET member contained in type 'User'.
```

### Polimorfizm

Domyślnie obiekt jest serializowany zgodnie ze swoim zadeklarowanym typem (argumentem typu lub typem właściwości), a nie typem rzeczywistym. Serializacja obiektu klasy pochodnej przez referencję do klasy bazowej gubi właściwości klasy pochodnej – podobnie jak wycinanie obiektu (*object slicing*) w C++ – a typu abstrakcyjnego nie da się w ogóle zdeserializować, bo serializator nie wie, której klasy instancję utworzyć. Od .NET 7 typ bazowy może wskazać dozwolone typy pochodne atrybutem `[JsonDerivedType]`. Serializator zapisuje wtedy **dyskryminator typu** (domyślnie właściwość `$type`; atrybut `[JsonPolymorphic]` pozwala zmienić jej nazwę) i korzysta z niego podczas deserializacji:

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Circle), "circle")]
[JsonDerivedType(typeof(Rectangle), "rectangle")]
public abstract record Shape;
public record Circle(double Radius) : Shape;
public record Rectangle(double Width, double Height) : Shape;
```

```csharp
Shape[] shapes = [new Circle(1.5), new Rectangle(2, 3)];
string json = JsonSerializer.Serialize(shapes);
// [{"kind":"circle","Radius":1.5},{"kind":"rectangle","Width":2,"Height":3}]
Shape[]? deserialized = JsonSerializer.Deserialize<Shape[]>(json);
// deserialized[0] is Circle { Radius = 1.5 }
```

Dozwolone są tylko wymienione typy: serializacja niewymienionego typu pochodnego zgłasza wyjątek `NotSupportedException`, a nieznana wartość dyskryminatora – wyjątek `JsonException`. Dyskryminator musi być pierwszą właściwością obiektu JSON, chyba że włączono opcję `AllowOutOfOrderMetadataProperties` (.NET 9). Jawna lista dozwolonych typów to celowy mechanizm bezpieczeństwa – `Newtonsoft.Json` oferuje `TypeNameHandling`, które zapisuje w JSON pełne nazwy typów .NET; w połączeniu z niezaufanymi danymi pozwala to atakującemu zmusić deserializator do utworzenia nieoczekiwanych typów, co jest dobrze znanym źródłem podatności.

### Generowanie kodu źródłowego

Serializacja oparta na refleksji analizuje typy w czasie wykonania, co kosztuje czas przy uruchomieniu i pamięć, a do tego jest niezgodne z przycinaniem (*trimming*) i kompilacją Native AOT – w takich aplikacjach (w tym w aplikacjach jednoplikowych .NET 10, `dotnet run app.cs`, które domyślnie włączają `PublishAot`) jest całkowicie wyłączona. Alternatywą jest **generator kodu źródłowego** (*source generator*): klasa częściowa (`partial`) dziedzicząca po `JsonSerializerContext` wymienia serializowane typy w atrybutach `[JsonSerializable]`, a kompilator generuje dla nich metadane i kod serializacji w czasie budowania:

```csharp
[JsonSerializable(typeof(Forecast))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class ForecastContext : JsonSerializerContext;
```

```csharp
string json = JsonSerializer.Serialize(forecast, ForecastContext.Default.Forecast);
Forecast? deserialized = JsonSerializer.Deserialize(json, ForecastContext.Default.Forecast);
```

`ForecastContext.Default.Forecast` to wygenerowany obiekt `JsonTypeInfo<Forecast>`. Kontekst można też przypisać do `JsonSerializerOptions.TypeInfoResolver`, dzięki czemu również wywołania przyjmujące opcje działają bez refleksji.

## Newtonsoft.Json

Serializacja odbywa się za pomocą statycznej klasy `JsonConvert`.

### Serializacja

```csharp
var serialized = JsonConvert.SerializeObject(forecast, Formatting.Indented);
```

```json
{
  "Location": "Warsaw",
  "Temperatures": {
    "2025-12-07T22:15:35.4733681+01:00": -17.084698423041804,
    "2025-12-07T23:15:35.4733681+01:00": -15.132029763809939,
    "2025-12-08T00:15:35.4733681+01:00": 5.772196161399023,
    "2025-12-08T01:15:35.4733681+01:00": 8.832614803522482,
    "2025-12-08T02:15:35.4733681+01:00": -19.629052158164615,
    "2025-12-08T03:15:35.4733681+01:00": 7.6065850260430565,
    "2025-12-08T04:15:35.4733681+01:00": -19.710601981383896,
    "2025-12-08T05:15:35.4733681+01:00": 13.65822297707701,
    "2025-12-08T06:15:35.4733681+01:00": 7.43438191592119,
    "2025-12-08T07:15:35.4733681+01:00": -18.847585808274825,
    "2025-12-08T08:15:35.4733681+01:00": 19.497623200834,
    "2025-12-08T09:15:35.4733681+01:00": 1.3729643304821089,
    "2025-12-08T10:15:35.4733681+01:00": -4.353138125059385,
    "2025-12-08T11:15:35.4733681+01:00": 14.690275394792117,
    "2025-12-08T12:15:35.4733681+01:00": 14.823212353915117,
    "2025-12-08T13:15:35.4733681+01:00": 1.272628008894607,
    "2025-12-08T14:15:35.4733681+01:00": 29.740090341863343,
    "2025-12-08T15:15:35.4733681+01:00": 13.979668721485552,
    "2025-12-08T16:15:35.4733681+01:00": 23.9485046073197,
    "2025-12-08T17:15:35.4733681+01:00": -14.917400243602472,
    "2025-12-08T18:15:35.4733681+01:00": 22.090026162918193,
    "2025-12-08T19:15:35.4733681+01:00": 3.2913333012331627,
    "2025-12-08T20:15:35.4733681+01:00": 14.130944452865734,
    "2025-12-08T21:15:35.4733681+01:00": -10.767305294541261
  },
  "Summary": [
    "Rain",
    "Cold",
    "Humid"
  ]
}
```

### Deserializacja

```csharp
Forecast? deserialized = JsonConvert.DeserializeObject<Forecast>(serialized);
```

### Atrybuty

```csharp
public class Person
{
    [JsonProperty("FirstName")]
    public string Name { get; set; }

    [JsonProperty] // Needed for non-public members (unlike System.Text.Json, public fields are serialized by default)
    private DateTime DateOfBirth;

    [JsonIgnore]
    public int Age { get; set; }
}
```

> [!INFO]
> ## Przykład
> {{< filetree dir="lectures/ipc/Serialization" >}}
