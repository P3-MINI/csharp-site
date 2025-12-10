using System.IO.MemoryMappedFiles;


namespace BigCsvReaders;

public class MmfBigCsvReader : BigCsvReader
{
    public MmfBigCsvReader(string path, char delimiter=','): base(path, delimiter)
    {
        // TODO
    }


    protected override string ReadRow(int row)
    {
        // TODO
    }


    public override void Dispose()
    {
        // TODO

        base.Dispose();
    }
}
