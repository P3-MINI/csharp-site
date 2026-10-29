using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Shell;

public sealed class PluginManager(string pluginsDirectory)
{
    private readonly List<Assembly> _assemblies = [];

    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    // Every plugin lives in its own directory: plugins/<Name>/<Name>.dll
    public void LoadAll()
    {
        if (!Directory.Exists(pluginsDirectory))
        {
            return;
        }

        foreach (string directory in Directory.GetDirectories(pluginsDirectory).Order())
        {
            string path = Path.Combine(directory, Path.GetFileName(directory) + ".dll");
            if (File.Exists(path))
            {
                var context = new PluginLoadContext(path);
                // Load a copy from memory - the file is not locked, so the plugin can be rebuilt while the shell runs
                using FileStream stream = File.OpenRead(path);
                _assemblies.Add(context.LoadFromStream(stream));
            }
        }
    }

    // Starts unloading all plugins. The returned weak references tell when the contexts are really gone.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public List<WeakReference> UnloadAll()
    {
        List<WeakReference> contexts = [];
        foreach (Assembly assembly in _assemblies)
        {
            AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly)!;
            context.Unload();
            contexts.Add(new WeakReference(context));
        }
        _assemblies.Clear();
        return contexts;
    }
}
