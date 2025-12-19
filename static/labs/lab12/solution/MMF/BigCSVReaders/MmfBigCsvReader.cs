using System.IO.MemoryMappedFiles;


namespace BigCsvReaders;

public class MmfBigCsvReader : BigCsvReader
{
    public MmfBigCsvReader(string path, char delimiter=','): base(path, delimiter)
    {
        _offsetsMmf = MemoryMappedFile.CreateFromFile(OffsetsFile, FileMode.Open);
        _csvMmf = MemoryMappedFile.CreateFromFile(CsvFile, FileMode.Open);

        _offsetsFileLen = new FileInfo(OffsetsFile).Length;
        _csvFileLen = new FileInfo(CsvFile).Length;

        _offsetAccessor = _offsetsMmf.CreateViewAccessor();
        _csvAccessor = _csvMmf.CreateViewAccessor();
    }

    private readonly MemoryMappedFile _offsetsMmf;
    private readonly MemoryMappedViewAccessor _offsetAccessor;

    private readonly MemoryMappedFile _csvMmf;
    private readonly MemoryMappedViewAccessor _csvAccessor;

    private readonly long _offsetsFileLen;
    private readonly long _csvFileLen;


    protected override string ReadRow(int row)
    {
        (long offset, long len) = GetRowOffsetAndLen(row);
        
        var buff = new byte[len];

        if (_csvAccessor.ReadArray(offset, buff, 0, buff.Length) < buff.Length)
            throw new IOException("Error while reading csv file");

        return System.Text.Encoding.UTF8.GetString(buff);
    }


    private (long offset, long len) GetRowOffsetAndLen(int row)
    {
        if (row == RowsCnt)
            return GetLastRowOffsetAndLen();

        long offsetPosition = row * sizeof(long);
        if (offsetPosition > _offsetsFileLen)
            throw new IndexOutOfRangeException();

        long offset = _offsetAccessor.ReadInt64(offsetPosition);
        long nextOffset = _offsetAccessor.ReadInt64(offsetPosition + sizeof(long));
        
        long len = nextOffset - offset - 1;
        
        return (offset, len);
    }


    private (long offset, long len) GetLastRowOffsetAndLen()
    {
        long offsetPosition = RowsCnt * sizeof(long);
        if (offsetPosition > _offsetsFileLen)
            throw new IndexOutOfRangeException();

        long offset = _offsetAccessor.ReadInt64(offsetPosition);
        long len = _csvFileLen - offset;
        
        return (offset, len);
    }


    public override void Dispose()
    {
        _offsetAccessor.Dispose();
        _offsetsMmf.Dispose();
        
        _csvAccessor.Dispose();
        _csvMmf.Dispose();

        base.Dispose();
    }
}
