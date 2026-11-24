namespace PackageManager;

public class Package /* add missing interface */ {
    public string Name { get; }
    public string Version { get; }
    public int DownloadKb { get; }
    public IReadOnlyList<string> DependsOn { get; }

    /* your code here */

    public override string ToString() => $"{Name} {Version} ({DownloadKb} KB)";
}