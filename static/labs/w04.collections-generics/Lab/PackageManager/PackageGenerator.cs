using System.Text;

namespace PackageManager;

public static class PackageGenerator {
    /// <summary>
    /// Lazily yields <c>count</c> packages. A package may only depend on packages
    /// that have already been emitted, so the result is always acyclic.
    /// Dependencies are picked with rejection sampling so there are no duplicates.
    /// </summary>
    public static IEnumerable<Package> GeneratePkgs(int count, Random rng, string sufix = "pkg") {
        var names = new List<string>(count);

        for (int i = 0; i < count; i++) {
            var sb = new StringBuilder(4);
            for (int k = 0; k < 4; k++) {
                sb.Append((char)('A' + rng.Next(26)));
            }

            var name = $"{sb}.{sufix}";

            int depCount = 0;
            if (names.Count > 0) {
                depCount = rng.Next(1, Math.Min(3, names.Count) + 1);
            }

            var deps = new string[depCount];
            var chosenIdx = new HashSet<int>();
            int j = 0;
            while (j < depCount) {
                int idx = rng.Next(0, names.Count);
                if (chosenIdx.Add(idx)) {
                    deps[j] = names[idx];
                    j++;
                }
            }

            yield return new Package(name, "1.0", rng.Next(5, 200), deps);
            names.Add(name);
        }
    }
}