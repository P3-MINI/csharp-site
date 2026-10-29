namespace Snapshot;

public static class PathUtils
{
    /// <summary>
    /// Returns true if <paramref name="path"/> is <paramref name="directory"/> itself
    /// or is located somewhere inside it.
    /// </summary>
    public static bool IsInside(string path, string directory)
    {
        string relative = Path.GetRelativePath(Path.GetFullPath(directory), Path.GetFullPath(path));

        // Outside: "..", "../something" or a rooted path (e.g. another drive on Windows)
        bool outside = relative == ".."
                       || relative.StartsWith(".." + Path.DirectorySeparatorChar)
                       || Path.IsPathRooted(relative);
        return !outside;
    }
}
