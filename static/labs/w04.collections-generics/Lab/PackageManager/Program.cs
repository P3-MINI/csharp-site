namespace PackageManager;

public static class Program {
    private static string SortedString(IEnumerable<string> names) {
        List<string> list = [.. names];
        list.Sort(StringComparer.InvariantCultureIgnoreCase);
        return string.Join(", ", list);
    }

    public static void Main() {
        var rng = new Random(7);
        var registry = new PackageRegistry();

        // =============================================================
        // UNCOMMENT TO CHECK YOUR SOLUTION
        // =============================================================

        //string rootName = "";
        //foreach (var p in PackageGenerator.GeneratePkgs(12, rng)) {
        //    registry.Register(p);
        //    rootName = p.Name;            
        //}

        //Console.WriteLine($"Registered packages: {registry.Count}");
        //Console.WriteLine();

        // =============================================================

        //Console.WriteLine("Packages by priority (SortedSet<Package>, biggest first):");
        //foreach (var p in registry.PackagesByPriority()) {
        //    string line = "  " + p;
        //    if (p.DependsOn.Count > 0) {
        //        line += "  <- " + SortedString(p.DependsOn);
        //    }
        //    Console.WriteLine(line);
        //}
        //Console.WriteLine();

        // =============================================================

        //Console.WriteLine("Indexer check:");
        //var root = registry[rootName];
        //Console.WriteLine($"  registry[\"{rootName}\"] is null: {root is null}");
        //Console.WriteLine($"  registry[\"does-not-exist\"] is null: {registry["does-not-exist"] is null}");
        //Console.WriteLine();

        // =============================================================

        //if (root is not null) {
        //    Console.WriteLine($"EnumerateDependencies for {rootName}:");
        //    foreach (var p in registry.EnumerateDependencies(root)) {
        //        Console.WriteLine("  " + p);
        //    }
        //    Console.WriteLine();
        //}

        // =============================================================

        //string[] allNames = [.. registry.Names];
        //string rootName2 = allNames[allNames.Length - 2];
        //string rootName3 = allNames[allNames.Length - 3];

        //var root2 = registry[rootName2];
        //if (root2 is not null) {
        //    Console.WriteLine($"EnumerateDependencies for {rootName2}:");
        //    foreach (var p in registry.EnumerateDependencies(root2)) {
        //        Console.WriteLine("  " + p);
        //    }
        //    Console.WriteLine();
        //}

        //var root3 = registry[rootName3];
        //if (root3 is not null) {
        //    Console.WriteLine($"EnumerateDependencies for {rootName3}:");
        //    foreach (var p in registry.EnumerateDependencies(root3)) {
        //        Console.WriteLine("  " + p);
        //    }
        //    Console.WriteLine();
        //}

        // =============================================================

        //var required = registry.RequiredPackages(rootName, rootName2);
        //Console.WriteLine($"RequiredPackages(\"{rootName}\", \"{rootName2}\")  [union]");
        //Console.WriteLine($"  count = {required.Count}");
        //Console.WriteLine("  " + (required.Count == 0 ? "(none)" : SortedString(required)));
        //Console.WriteLine();

        //var requiredWithTypo = registry.RequiredPackages(rootName, "does-not-exist");
        //Console.WriteLine($"RequiredPackages(\"{rootName}\", \"does-not-exist\")  [unknown skipped]");
        //Console.WriteLine($"  count = {requiredWithTypo.Count}");
        //Console.WriteLine();

        // =============================================================

        //var shared = registry.SharedDependencies(rootName, rootName2);
        //Console.WriteLine($"SharedDependencies(\"{rootName}\", \"{rootName2}\")  [intersection]");
        //Console.WriteLine($"  count = {shared.Count}");
        //Console.WriteLine("  " + (shared.Count == 0 ? "(none)" : SortedString(shared)));
        //Console.WriteLine();

        // =============================================================

        //var original = registry[rootName]!;

        //Console.WriteLine($"Current version: {registry[original.Name]!.Version}");
        //var olderVersion = new Package(original.Name, "0.9", original.DownloadKb, Array.Empty<string>());
        //var newerVersion = new Package(original.Name, "2.0", original.DownloadKb, Array.Empty<string>());

        //registry.Register(olderVersion);
        //Console.WriteLine($"After registering [{original.Name} 0.9] version = {registry[original.Name]!.Version}");

        //registry.Register(newerVersion);
        //Console.WriteLine($"After registering [{original.Name} 2.0] version = {registry[original.Name]!.Version}");
        //Console.WriteLine();

        // =============================================================
    }
}