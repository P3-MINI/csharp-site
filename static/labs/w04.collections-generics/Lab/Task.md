## Task 1: `Package`

1. Implement public constructor for `Package` class:
```csharp
public Package(string name, string version, int downloadKb, params string[] dependsOn) 
```
2. `Package` should implement `IComparable<Package>` interface. Add the missing interface and implement `CompareTo` method:
- bigger packages should come before smaller
- if packages have the same size order by `Name` (use `StringComparison.InvariantCultureIgnoreCase`)

## Task 2: `Package registry`
1. Add `PackagesByPriority` that returns a set with packages sorted by size and name.
```csharp
public ISet<Package> PackagesByPriority() 
```
2. Add `Register` method that's adds a new package to registry or if present updates package's version.
```csharp
 public void Register(Package p)
```
3. Add `EnumerateDependencies` method, that enumerates all dependencies for given package, Depth-First Search order. Use `yield return`.
```csharp
public IEnumerable<Package> EnumerateDependencies(Package root)
```
4. Add `RequiredPackages` method, that returns all dependencies for given package names without duplicates. The given package names should not appear in result.
```csharp
public ISet<string> RequiredPackages(params string[] names)
```
5. Add `SharedDependencies` method, that returns only shared dependencies for given package names. The given package names should not appear in result.
```csharp
public ISet<string> SharedDependencies(params string[] names)
```