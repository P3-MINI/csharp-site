---
title: "Assembly"
weight: 30
---

# Assembly

Assembly to skompilowana jednostka kodu w postaci pliku `.dll` lub `.exe`.

> W .NET (Core) skompilowany kod aplikacji również trafia do pliku `.dll`. Towarzyszący mu plik `.exe` (w systemach Linux i macOS: plik bez rozszerzenia) to jedynie mały natywny program startowy (*apphost*), który uruchamia środowisko uruchomieniowe i wczytuje ten plik `.dll`.

## Co zawiera assembly?

Assembly składa się z:

* **Kodu pośredniego** - kod źródłowy przetłumaczony na język pośredni, tłumaczony dalej przez *Runtime* do kodu maszynowego.
* **Metadanych** - opisują wszystko, co znajduje się wewnątrz assembly: wszystkie zdefiniowane typy (klasy, struktury, interfejsy, enumy), ich składowe (metody, właściwości, pola), parametry, atrybuty itd. To właśnie te metadane są odczytywane przez mechanizm refleksji.
* **Manifestu** - zawierającego podstawowe informacje o assembly:
  - nazwę i wersję
  - listę assembly, od których to assembly zależy, wraz z ich wersjami
  - listę typów zdefiniowanych i eksportowanych przez assembly
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

.NET wspiera specjalny rodzaj osadzonych plików, pliki `.resx` przechowujące słownik klucz-wartość (np. napisy). Dla każdego języka tworzy się osobny plik: `Resources.resx` zawiera wartości domyślne, a `Resources.pl.resx`, `Resources.es.resx` itd. ich tłumaczenia (są one kompilowane do osobnych *assembly satelickich*). Do pobierania zlokalizowanych napisów służy klasa `ResourceManager`. Z takich słowników możemy pobierać wartość przez klucz, dla którego zostanie automatycznie wybrany napis dla kultury ustawionej we właściwości `Thread.CurrentThread.CurrentUICulture`.

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

## `AssemblyLoadContext`

Assembly można dynamicznie wczytywać w trakcie działania programu. `AssemblyLoadContext` (*ALC*) to klasa, która tworzy izolowany kontekst do ładowania assembly. Pozwala kontrolować, jak assembly i ich zależności są znajdowane, ładowane i zarządzane w pamięci.

Jednym z problemów, które rozwiązuje, jest konflikt między wersjami zależności.

Jeżeli nasza aplikacja używa biblioteki `A` w wersji `13.0`, a chcemy wczytać dynamicznie bibliotekę `B`, która zależy od biblioteki `A` w wersji `10.2`, to możemy ją wczytać do nowego kontekstu, unikając konfliktu z wersją już wczytaną w głównym kontekście.

Poza tym konteksty pozwalają zwalniać poprzednio wczytane biblioteki, gdy nie są już potrzebne.

Każda aplikacja ma domyślny kontekst `AssemblyLoadContext.Default`, do którego trafiają wszystkie standardowo ładowane assembly.

Dla wybranego assembly możemy pobrać kontekst, w którym się ono znajduje:

```csharp
Assembly assembly = Assembly.GetExecutingAssembly();
AssemblyLoadContext? context = AssemblyLoadContext.GetLoadContext(assembly);
if (context is null) return;

Console.WriteLine(context.Name);
foreach (Assembly a in context.Assemblies)
{
    Console.WriteLine(a.FullName);
}
```

Można też tworzyć własne izolowane konteksty. Argument `isCollectible` sprawia, że taki kontekst będzie można potem zwolnić (metodą `Unload`), odzyskując pamięć.

```csharp
AssemblyLoadContext context = new AssemblyLoadContext("Plugins", isCollectible: true);
```

### Dynamiczne wczytywanie assembly

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
4. Jeżeli nadal nie udało się zlokalizować assembly, to wywoływane jest zdarzenie `Resolving`, najpierw na domyślnym *ALC*, następnie na oryginalnym *ALC*.
5. Jeżeli nadal się nie udało, to zgłaszany jest wyjątek `FileNotFoundException`.

Domyślny kontekst szuka zależności najpierw czytając plik `[application_name].deps.json` opisujący lokalizację zależności, a jeżeli ten nie istnieje, to przeszukuje katalog bazowy aplikacji.

### Własny kontekst

Można stworzyć własną klasę kontekstu wczytywania assembly, która dostarczy własną logikę szukania assembly. Własną logikę szukania dostarczamy, nadpisując metodę `Load`. Alternatywnie możemy zasubskrybować zdarzenie `Resolving` już istniejących *ALC*, żeby dostarczyć taką logikę.

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
            // Load the shared contract from the default context,
            // so that ITextPlugin is the same type for the host and the plugin
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

> `AssemblyDependencyResolver` czyta plik `[plugin_name].deps.json` pluginu w poszukiwaniu jego zależności.

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

Po dynamicznym wczytaniu assembly będziemy w nim szukali klas, które implementują ten interfejs i wywoływali z tych klas metodę `ApplyOperation`.

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
