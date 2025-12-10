using System.ComponentModel;
using BigCsvReaders;

namespace BigCSVReader.Tests;

public abstract class TestBase : IDisposable
{
    public enum ReaderType
    {
        StreamReader,
        MmfReader
    }

    protected string TestDir { get; private set; } = null!;
    protected string CsvPath { get; private set; } = null!;

    protected TestBase()
    {
        TestDir = Path.Combine(Path.GetTempPath(), $"BigCsvReaderTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(TestDir);
        CsvPath = Path.Combine(TestDir, "test.csv");
    }

    protected string NewTempCsvPath(string prefix = "test")
    {
        return Path.Combine(TestDir, $"{prefix}_{Guid.NewGuid()}.csv");
    }

    protected BigCsvReader GetReader(ReaderType type, string path, char delimiter=',')
    {
        switch (type)
        {
            case ReaderType.StreamReader:
                return new StreamBigCsvReader(path, delimiter);
            case ReaderType.MmfReader:
                return new MmfBigCsvReader(path, delimiter);

            default:
                throw new InvalidEnumArgumentException("Unknown enum value");
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(TestDir))
            Directory.Delete(TestDir, true);
    }
}
