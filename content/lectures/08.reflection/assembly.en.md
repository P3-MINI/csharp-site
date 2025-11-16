---
title: "Assembly"
weight: 30
---

# Assembly

An assembly is a compiled unit of code in the form of a `.dll` or `.exe` file.

## What does an assembly contain?

An assembly consists of:

* **Intermediate Language (IL)** - source code that has been translated into an intermediate language, which is then further translated by the Runtime into machine code.
* **Metadata** - describes everything inside the assembly: all defined types (classes, structs, interfaces, enums), their members (methods, properties, fields), parameters, attributes, etc. It is this metadata that is read by the reflection mechanism.
* **Manifest** - contains basic information about the assembly:
  - name and version
  - a list of assemblies that this assembly depends on, along with their versions
  - a list of types defined and exported by the assembly
  - a public key (for strongly-named assemblies)
* **Resources** - files embedded within the assembly.

## Resources

You can embed any file within an assembly. You just need to add it to the `EmbeddedResource` item group in the project file.

```xml
<ItemGroup>
    <None Remove="lorem.txt" />
    <EmbeddedResource Include="lorem.txt" />
</ItemGroup>
```

You can access such embedded files using reflection to get a stream:

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

## Localized Resources

C# supports a special type of embedded file, `.resx` files, which store a key-value dictionary for different languages, containing string versions in various languages. The `ResourceManager` class is used to retrieve localized strings. From such dictionaries, we can retrieve a value by its key, for which the string will be automatically selected for the culture set in the `Thread.CurrentThread.CurrentUICulture` property.

```csharp
ResourceManager rm = new ResourceManager("EmbeddingResources.Resources", typeof(Program).Assembly);
string? greeting = rm.GetString("greeting");
string? welcome = rm.GetString("welcome-message");
Console.WriteLine(greeting);
Console.WriteLine(welcome);
```

Thanks to this mechanism, we can create localized applications.

> [!INFO]
> Source code:
> {{< filetree dir="lectures/reflection/EmbeddingResources" >}}

## `AssemblyLoadContext`

Assemblies can be dynamically loaded while a program is running. An `AssemblyLoadContext` (ALC) is a class that creates an isolated context for loading assemblies. It allows you to control how assemblies and their dependencies are found, loaded, and managed in memory.

One of the problems it solves is conflicts between dependency versions.

If our application uses library `A` in version `13.0`, and we want to dynamically load library `B`, which depends on library `A` in version `10.2`, we can load it into a new context, avoiding a conflict with the version already loaded in the main context.

Additionally, contexts allow for unloading previously loaded libraries when they are no longer needed.

Every application has a default context, `AssemblyLoadContext.Default`, where all standardly loaded assemblies go.

For a given assembly, we can get the context in which it resides:

```csharp
Assembly assembly = Assembly.GetExecutingAssembly();
AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly);

Console.WriteLine(context.Name);
foreach (Assembly a in context.Assemblies)
{
    Console.WriteLine(a.FullName);
}
```

You can also create your own isolated contexts. The `isCollectible` argument makes such a context unloadable, allowing memory to be reclaimed later.

```csharp
AssemblyLoadContext context = new AssemblyLoadContext("Plugins", isCollectible: true);
```

### Dynamic Assembly Loading

There are three methods defined on a context for loading assemblies: 

```csharp
public Assembly LoadFromAssemblyPath(string assemblyPath);
public Assembly LoadFromStream(Stream assembly, Stream? assemblySymbols);
public Assembly LoadFromAssemblyName(AssemblyName assemblyName);
```

The `LoadFromAssemblyPath` and `LoadFromStream` methods load an assembly from the specified file or stream.

In the case of the `LoadFromAssemblyName` method, the context must determine the assembly's location. This method is also chosen when resolving the dependencies of other assemblies. The context searches for the assembly in the following way:

1. If an assembly with the exact same fully qualified name is already loaded, the ALC returns the already loaded assembly.
2. Otherwise, the (virtual protected) `Load` method is called, which is responsible for locating the assembly. Custom ALCs can implement any search logic.
3. If the assembly could not be located in step 2 (`Load` returned `null`), the Runtime calls `Load` on the default ALC.
4. If the assembly still could not be located, the `Resolving` event is raised, first on the default ALC, then on the original ALC.
5. If it still fails, a `FileNotFoundException` is thrown.

The default context searches for dependencies by first reading the `[application_name].deps.json` file, which describes dependency locations, and if that file doesn't exist, it searches the application's base directory.

### Custom Context

You can create your own assembly loading context class that provides its own logic for finding assemblies. We provide custom search logic by overriding the `Load` method. Alternatively, we can subscribe to the `Resolving` event of existing ALCs to provide such logic.

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

<<<<<<< HEAD
> `AssemblyDependencyResolver` reads file `[application_name].deps.json` when searching for dependencies.

=======
>>>>>>> master
### Plugin System

Using dynamic assembly loading with an ALC, you can implement a plugin system.

The example will consist of 5 projects:

* **Plugin.Host** - responsible for loading plugins
* **Plugin.Common** - defines the common plugin contract
* **Plugin.Reverse**, **Plugin.Rot13**, **Plugin.Figgle** - plugins that implement the common contract

The common contract will be defined by a simple interface:

```csharp
// Plugin.Common
public interface ITextPlugin
{
    string ApplyOperation(string input);
}
```

After dynamically loading an assembly, we will search it for classes that implement this interface and call the `ApplyOperation` method from those classes.

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
> Source code:
> {{< filetree dir="lectures/reflection/Plugins" >}}
