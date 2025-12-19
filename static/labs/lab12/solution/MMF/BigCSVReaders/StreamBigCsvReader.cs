namespace BigCsvReaders;

public class StreamBigCsvReader : BigCsvReader
{
    public StreamBigCsvReader(string path, char delimiter = ','): base(path, delimiter)
    {
        _offsetsFileStream = File.OpenRead(OffsetsFile);
        _offsetsReader = new BinaryReader(_offsetsFileStream);
        
        _csvFileStream = new FileStream(CsvFile, FileMode.Open);
        _csvReader = new StreamReader(_csvFileStream);
    }

    private readonly FileStream _offsetsFileStream;
    private readonly BinaryReader _offsetsReader;

    private readonly FileStream _csvFileStream;
    private readonly StreamReader _csvReader;


    protected override string ReadRow(int row)
    {
        long offsetPosition = row * sizeof(long);   // Position of an offset in the offsets file
        if (offsetPosition > _offsetsFileStream.Length)
            throw new IndexOutOfRangeException();

        _offsetsReader.BaseStream.Seek(offsetPosition, SeekOrigin.Begin);
        long offset = _offsetsReader.ReadInt64();

        _csvReader.BaseStream.Seek(offset, SeekOrigin.Begin);
        string? line = _csvReader.ReadLine();
        if (line == null)
            throw new IOException("Error while reading csv file");

        return line;
    }


    public override void Dispose()
    {
        _offsetsReader.Dispose();
        _offsetsFileStream.Dispose();

        _csvReader.Dispose();
        _csvFileStream.Dispose();

        base.Dispose();
    }
}