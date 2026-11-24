using System.Reflection;
using System.Runtime.Loader;

namespace Plugin.Host;

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