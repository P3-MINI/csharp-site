namespace Snapshot;

/// <summary>
/// A single file found in the source directory.
/// </summary>
/// <param name="RelativePath">Path relative to the source directory, always with '/' as the separator.</param>
/// <param name="FullPath">Absolute path of the file on this machine.</param>
/// <param name="Size">File size in bytes.</param>
/// <param name="LastWriteTime">Time of the last modification (local time).</param>
public sealed record FileEntry(string RelativePath, string FullPath, long Size, DateTime LastWriteTime);
