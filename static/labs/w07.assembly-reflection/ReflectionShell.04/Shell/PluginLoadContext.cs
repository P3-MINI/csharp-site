using System.Reflection;
using System.Runtime.Loader;
using Shell.Abstractions;

namespace Shell;

// Every plugin is loaded into its own context, so it can bring its own versions of dependencies
public sealed class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(name: Path.GetFileNameWithoutExtension(pluginPath))
{
    // Finds dependencies of the plugin using its .deps.json file
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // The shared contract must come from the default context (the host's copy).
        // A second copy of Shell.Abstractions would define a second, different CommandAttribute type.
        if (assemblyName.Name == typeof(CommandAttribute).Assembly.GetName().Name)
        {
            return null; // null = let the default context load it
        }

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }
}
