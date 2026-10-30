namespace PackageManager;
public class Package : IComparable<Package> {
    public string Name { get; }
    public string Version { get; }
    public int DownloadKb { get; }
    public IReadOnlyList<string> DependsOn { get; }

    public Package(string name, string version, int downloadKb, params string[] dependsOn) {
        Name = name;
        Version = version;
        DownloadKb = downloadKb;
        DependsOn = dependsOn;
    }

    public int CompareTo(Package? other) {
        if (other is null) {
            return 1;
        }
        int bySize = other.DownloadKb.CompareTo(DownloadKb);   // descending
        if (bySize != 0) {
            return bySize;
        }
        return Name.CompareTo(other.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public override string ToString() => $"{Name} {Version} ({DownloadKb} KB)";
}