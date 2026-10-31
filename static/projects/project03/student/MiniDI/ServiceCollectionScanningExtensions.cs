using System.Reflection;

namespace MiniDI;

public static class ServiceCollectionScanningExtensions
{
    /// <summary>
    /// Registers every class of <paramref name="assembly"/> marked with <see cref="ServiceAttribute"/>
    /// (optionally only those accepted by <paramref name="filter"/>).
    /// </summary>
    public static ServiceCollection AddFromAssembly(this ServiceCollection services, Assembly assembly, Func<Type, bool>? filter = null)
    {
        // TODO (Part 3)
        throw new NotImplementedException();
    }
}
