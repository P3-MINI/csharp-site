---
title: "Assembly"
weight: 30
---

# Assembly

Assembly to skompilowana jednostka kodu w postaci pliku `.dll` lub `.exe`.

## Co zawiera assembly?

Assembly składa się z:

* **Kodu pośredniego** - kod źródłowy przetłumaczony na język pośredni, tłumaczony dalej przez *Runtime* do kodu maszynowego.
* **Metadanych** - opisują wszystko, co znajduje się wewnątrz assembly: wszystkie zdefiniowane typy (klasy, struktury, interfejsy, enumy), ich składowe (metody, właściwości, pola), parametry, atrybuty itd. To właśnie te metadane są odczytywane przez mechanizm refleksji.
* **Manifestu** - zawierające podstawowe informacje o assembly:
  - nazwę i wersję
  - listę assembly od których to assembly zależy, wraz z ich wersjami
  - listę typów zdefiniowanych i eksportowanych przez asssembly
  - klucz publiczny (dla silnie nazwanych assembly)
* **Zasobów** - pliki osadzone wewnątrz assembly.

## Zasoby

W assembly można osadzać dowolne pliki. Wystarczy w pliku projektu dodać je do grupy `EmbeddedResource`.

```xml
<ItemGroup>
    <None Remove="lorem.txt" />
    <EmbeddedResource Include="lorem.txt" />
</ItemGroup>
```

Do tak osadzonych plików można się dobrać za pomocą refleksji, otrzymując strumień:

```csharp
Assembly? assembly = Assembly.GetEntryAssembly();
if (assembly is null) return;

using Stream? stream = assembly.GetManifestResourceStream("EmbeddingResources.lorem.txt");
if (stream is null)
{
    Console.WriteLine("Embedded resource not found!");
    return;
}

using StreamReader reader = new StreamReader(stream);

while (reader.ReadLine() is {} line)
{
    Console.WriteLine(line);
}
```

## Zlokalizowane zasoby

C# wspiera specjalny rodzaj osadzonych plików, pliki `.resx` przechowujące słownik klucz-wartość dla różnych języków, zawierający wersje napisów w różnych językach. Do pobierania zlokalizowanych napisów służy klasa `ResourceManager`. Z takich słowników możemy pobierać wartość przez klucz, dla którego zostanie automatycznie wybrany napis dla kultury ustawionej we właściwości `Thread.CurrentThread.CurrentUICulture`.

```csharp
ResourceManager rm = new ResourceManager("EmbeddingResources.Resources", typeof(Program).Assembly);
string? greeting = rm.GetString("greeting");
string? welcome = rm.GetString("welcome-message");
Console.WriteLine(greeting);
Console.WriteLine(welcome);
```

Dzięki temu mechanizmowi możemy tworzyć lokalizowane aplikacje.

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/reflection/EmbeddingResources" >}}

## `AssemblyLoadingContext`

Assembly można dynamicznie wczytywać w trakcie działania programu. `AssemblyLoadingContext` (*ALC*) to klasa, która tworzy izolowany kontekst do ładowania assembly. Pozwala kontrolować jak assembly i ich zależności są znajdowane, ładowane i zarządzane w pamięci.

Jednym z problemów które on rozwiązuje jest konflikt między wersjami zależności.

Jeżeli nasza aplikacja używa biblioteki `A` w wersji `13.0`, a chcemy wczytać dynamicznie bibliotekę `B`, która zależy od biblioteki `A` w wersji `10.2`, to możemy ją wczytać do nowego kontekstu, niekonfliktując z już wczytaną wersją w głównym kontekście.

Poza tym konteksty pozwalają zwalniać poprzednio wczytane biblioteki, gdy nie są już potrzebne.

Każda aplikacja ma domyślny kontekst `AssemblyLoadContext.Default`, do którego trafiają wszystkie standardowo ładowane assembly.

Dla wybranego assembly, możemy pobrać kontekst, w którym się ono znajduje:

```csharp
Assembly assembly = Assembly.GetExecutingAssembly();
AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly);

Console.WriteLine(context.Name);
foreach (Assembly a in context.Assemblies)
{
    Console.WriteLine(a.FullName);
}
```

Można też tworzyć własne izolowane konteksty. Argument `isCollectible` sprawia, że taki context będzie można potem zwolnić, odzyskując pamięć.

```csharp
AssemblyLoadContext context = new AssemblyLoadContext("Plugins", isCollectible: true);
```

### Dynamicznie wczytywanie assembly

Do wczytywania assembly służą trzy metody zdefiniowane na kontekście: 

```csharp
public Assembly LoadFromAssemblyPath(string assemblyPath);
public Assembly LoadFromStream(Stream assembly, Stream? assemblySymbols);
public Assembly LoadFromAssemblyName(AssemblyName assemblyName);
```

Metody `LoadFromAssemblyPath` i `LoadFromStream` wczytują assembly ze wskazanego pliku lub strumienia.

W przypadku metody `LoadFromAssemblyName` kontekst musi ustalić lokalizację assembly. Metoda ta jest także wybierana, gdy rozwiązujemy zależności innych assembly. Kontekst szuka assembly w następujący sposób:

1. Jeżeli dokładnie takie assembly jest już wczytane (co do pełnej kwalifikowanej nazwy assembly), to *ALC* zwraca już wczytane assembly.
2. W przeciwnym przypadku wywoływana jest metoda (virtual protected) `Load`, która ma za zadanie zlokalizować assembly. Własne *ALC* mogą implementować dowolną logikę poszukiwania.
3. Jeżeli nie udało się zlokalizować assembly w kroku 2. (`Load` zwrócił `null`), to *Runtime* wywołuje `Load` na domyślnym *ALC*.
4. Jeżeli nadal nie udało się zlokalizować assembly, to wywoływane jest zdarzenie `Resolving`, najpierw na na domyślnym *ALC*, następnie na oryginalnym *ALC*.
5. Jeżeli nadal się nie udało, to zgłaszany jest wyjątek `FileNotFoundException`.

Domyślny kontekst szuka zależności najpierw czytając plik `[application_name].deps.json` opisujący lokalizację zależności, a jeżeli ten nie istnieje, to przeszukuje katalog bazowy aplikacji.

### Własny kontekst

Można stworzyć własną klasę kontekstu wczytywania assembly, który dostarczy własną logikę szukania assembly. Własną logikę szukania dostarczamy nadpisując metodę `Load`. Alternatywnie możemy zasubskrybować do zdarzenia `Resolving` już istniejących *ALC*, żeby dostarczyć taką logikę.

```csharp
class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath, bool collectible = true)
        : base(name: pluginPath, isCollectible: collectible)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == "Plugin.Common")
        {
            return null;
        }

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}
```

> `AssemblyDependencyResolver` czyta plik `[application_name].deps.json` w poszukiwaniu zależności.

### System pluginów

Przy użyciu dynamicznego wczytywania assembly, za pomocą *ALC*, można zaimplementować system pluginów.

Przykład będzie składał się z 5 projektów:

* **Plugin.Host** - odpowiedzialny za wczytywanie pluginów
* **Plugin.Common** - definiujący wspólny kontrakt pluginów
* **Plugin.Reverse**, **Plugin.Rot13**, **Plugin.Figgle** - pluginy implementujące wspólny kontrakt

Wspólny kontrakt będzie zdefiniowany przez prosty interfejs:

```csharp
// Plugin.Common
public interface ITextPlugin
{
    string ApplyOperation(string input);
}
```

Po dynamicznym wczytaniu assembly, będziemy w nim szukali klas, które implementują ten interfejs i wywoływali z tych klas metodę `ApplyOperation`.

```csharp
// Plugin.Host
class Program
{
    static void Main()
    {
        string hello = "Hello, World!";
        Console.WriteLine(ApplyPluginOperation(hello, "Plugins/Plugin.Rot13.dll"));
        Console.WriteLine(ApplyPluginOperation(hello, "Plugins/Plugin.Reverse.dll"));
        Console.WriteLine(ApplyPluginOperation(hello, "Plugins/Plugin.Figgle.dll"));
    }

    private static string? ApplyPluginOperation(string input, string pluginPath)
    {
        PluginLoadContext context = new PluginLoadContext(pluginPath);
        try
        {
            Assembly assembly = context.LoadFromAssemblyPath(Path.GetFullPath(pluginPath));
            Type pluginType = assembly
                .ExportedTypes
                .Single(t => typeof(ITextPlugin).IsAssignableFrom(t));
            var plugin = Activator.CreateInstance(pluginType) as ITextPlugin;
            return plugin?.ApplyOperation(input);
        }
        finally
        {
            if (context.IsCollectible) context.Unload();
        }
    }
}
```

> [!INFO]
> Kod źródłowy:
> {{< filetree dir="lectures/reflection/Plugins" >}}
