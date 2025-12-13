using Xunit;
using BigCsvReaders;

namespace BigCSVReader.Tests;

public class BigCsvReaderTests : TestBase
{
    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void Constructor_WithValidPath_CreatesInstance(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);
        
        Assert.NotNull(reader);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void Constructor_WithNonExistentPath_ThrowsFileNotFoundException(ReaderType type)
    {
        string nonExistentPath = Path.Combine(TestDir, "nonexistent.csv");

        Assert.Throws<FileNotFoundException>(() => GetReader(type, nonExistentPath));
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void Constructor_CreatesOffsetsFile(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string offsetsPath = Path.ChangeExtension(CsvPath, ".offsets");
        Assert.True(File.Exists(offsetsPath));
    }
    
    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void ReadRow_FirstDataRow_ReturnsCorrectData(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string firstData = reader[1, 1];  // Row 0 is header, row 1 is first data row

        Assert.Equal("30", firstData);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void ReadRow_MiddleRow_ReturnsCorrectData(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string data = reader[3, 0];  // Row 3 is "Charlie"

        Assert.Equal("Charlie", data);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void ReadRow_LastRow_ReturnsCorrectData(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string data = reader[5, 2];  // Row 5 is "Eve", column 2 is "Phoenix"

        Assert.Equal("Phoenix", data);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void IndexAccessor_WithQuotedFields_ParsesCorrectly(ReaderType type)
    {
        TestDataGenerator.GenerateQuotedCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string field = reader[1, 1];  // Row 1 is "Item1"

        Assert.Equal("Contains, comma", field);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void IndexAccessor_WithEscapedQuotes_ParsesCorrectly(ReaderType type)
    {
        TestDataGenerator.GenerateQuotedCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string field = reader[2, 1];  // Row 2 is "Item2"

        Assert.Equal("Contains \"quotes\"", field);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void IndexAccessor_InvalidRow_ThrowsIndexOutOfRangeException(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        Assert.Throws<IndexOutOfRangeException>(() => _ = reader[999, 0]);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void IndexAccessor_CachesRow_ReturnsSameValueOnSecondAccess(ReaderType type)
    {
        TestDataGenerator.GenerateSimpleCsv(CsvPath);

        using var reader = GetReader(type, CsvPath);

        string first = reader[2, 0];   // Row 2 is "Bob"
        string second = reader[2, 1];
        string third = reader[2, 0];

        Assert.Equal("Bob", first);
        Assert.Equal("25", second);
        Assert.Equal("Bob", third);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void IndexAccessor_WithCustomDelimiter_ParsesCorrectly(ReaderType type)
    {
        TestDataGenerator.GenerateWithVariousDelimiters(CsvPath, '|');

        using var reader = GetReader(type, CsvPath, '|');

        string name = reader[1, 0];  // Row 1 is "Alice"
        string age = reader[1, 1];

        Assert.Equal("Alice", name);
        Assert.Equal("30", age);
    }

    [Theory]
    [InlineData(ReaderType.StreamReader)]
    [InlineData(ReaderType.MmfReader)]
    public void ReadRow_FieldWithWhitespace_TrimsCorrectly(ReaderType type)
    {
        var lines = new[]
        {
            "Name, Age , City",
            " Bob , 25 , NYC "
        };
        File.WriteAllLines(CsvPath, lines);

        using var reader = GetReader(type, CsvPath);

        string name = reader[1, 0];
        string age = reader[1, 1];

        Assert.Equal("Bob", name);
        Assert.Equal("25", age);
    }
}
