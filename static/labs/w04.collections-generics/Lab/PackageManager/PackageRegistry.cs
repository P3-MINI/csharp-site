namespace PackageManager;

public class PackageRegistry {
    private readonly IDictionary<string, Package> _byName = new Dictionary<string, Package>(StringComparer.Ordinal);
    public int Count => _byName.Count;
    public IEnumerable<string> Names => _byName.Keys;

    public Package? this[string name] => _byName.TryGetValue(name, out var p) ? p : null;

    /* your code here */
}