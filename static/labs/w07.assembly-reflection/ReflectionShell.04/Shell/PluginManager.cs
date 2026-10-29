using System.Reflection;

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
                _assemblies.Add(context.LoadFromAssemblyPath(path));
            }
        }
    }
}
