using System.Reflection;
using System.Runtime.Loader;

namespace MiniTestRunner;

/// <summary>
/// Provides a custom <see cref="AssemblyLoadContext"/> for loading test assemblies in isolation.
/// Supports resolving managed and unmanaged dependencies using <see cref="AssemblyDependencyResolver"/>.
/// </summary>
/// <param name="pluginPath">The path to the plugin or test assembly to load.</param>
/// <param name="collectible">Indicates whether the load context is collectible (can be unloaded).</param>
sealed class TestLoadContext(string pluginPath, bool collectible = true)
    : AssemblyLoadContext(name: pluginPath, isCollectible: collectible)
{
    private readonly AssemblyDependencyResolver resolver = new(pluginPath);

    /// <summary>
    /// Resolves and loads managed assemblies from the specified plugin path.
    /// Falls back to the default context if the assembly is already loaded.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load.</param>
    /// <returns>The loaded <see cref="Assembly"/>, or <c>null</c> if not found.</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyInDefaultContext = Default.Assemblies
            .FirstOrDefault(assembly => assembly.FullName == assemblyName.FullName);

        if (assemblyInDefaultContext is not null)
        {
            return assemblyInDefaultContext;
        }

        var assemblyPath = this.resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return this.LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    /// <summary>
    /// Resolves and loads unmanaged DLLs from the specified plugin path.
    /// </summary>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL to load.</param>
    /// <returns>A pointer to the loaded unmanaged DLL, or <see cref="IntPtr.Zero"/> if not found.</returns>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return this.LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}