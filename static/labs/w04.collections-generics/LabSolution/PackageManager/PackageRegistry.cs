namespace PackageManager;

public sealed class PackageRegistry {
    private readonly IDictionary<string, Package> _byName = new Dictionary<string, Package>(StringComparer.Ordinal);
    public int Count => _byName.Count;
    public IEnumerable<string> Names => _byName.Keys;

    public Package? this[string name] => _byName.TryGetValue(name, out var p) ? p : null;

    public void Register(Package p) {
        if (_byName.TryGetValue(p.Name, out var existing) && string.Compare(existing.Version, p.Version) >= 0) {
            return;
        }
        _byName[p.Name] = p;
    }

    public ISet<Package> PackagesByPriority() {
        var set = new SortedSet<Package>();
        foreach (var p in _byName.Values) {
            set.Add(p);
        }
        return set;
    }

    public IEnumerable<Package> EnumerateDependencies(Package root) {
        var visited = new HashSet<string>();
        var stack = new Stack<Package>();

        visited.Add(root.Name);        
        stack.Push(root);

        while (stack.Count > 0) {
            var pkg = stack.Pop();
            
            if (pkg.Name != root.Name) {
                yield return pkg;
            }
         
            foreach (var depName in pkg.DependsOn) {
                var dep = this[depName];
                if (dep is null) {
                    continue;
                }
                if (visited.Add(dep.Name)) {
                    stack.Push(dep);
                }
            }
        }
    }

    public ISet<string> RequiredPackages(params string[] names) {
        if (names.Length == 0) {
            return new HashSet<string>();
        }

        var namesSet = new HashSet<string>(names);
        HashSet<string> result = [];

        foreach (var name in names) {
            var pkg = this[name];
            if (pkg is null) {
                continue;
            }
            foreach (var p in EnumerateDependencies(pkg)) {
                result.Add(p.Name);               
            }
        }

        result.ExceptWith(namesSet);            
        return result;
    }

    public ISet<string> SharedDependencies(params string[] names) {
        if (names.Length == 0) {
            return new HashSet<string>();
        }

        var namesSet = new HashSet<string>(names);
        HashSet<string>? result = null;

        foreach (var name in names) {
            var pkg = this[name];
            if (pkg is null) {
                continue;
            }

            var deps = new HashSet<string>();
            foreach (var p in EnumerateDependencies(pkg)) {
                deps.Add(p.Name);
            }

            deps.ExceptWith(namesSet);
            if (result is null) {
                result = deps;
            } else {
                result.IntersectWith(deps);
            }
        }
        return result ?? new HashSet<string>();
    }
}