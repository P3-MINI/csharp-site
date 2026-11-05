using System.Security.Cryptography;

namespace Snapshot;

public static class FileHasher
{
    private const int BufferSize = 64 * 1024;

    /// <summary>
    /// Computes the SHA-256 hash of a file, reading it in chunks,
    /// so that the whole file never has to fit in memory.
    /// </summary>
    public static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        byte[] buffer = new byte[BufferSize];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            sha256.AppendData(buffer, 0, read);
        }

        return Convert.ToHexStringLower(sha256.GetHashAndReset());
    }
}
