namespace BigCsvReaders;

public class StreamBigCsvReader : BigCsvReader
{
    public StreamBigCsvReader(string path, char delimiter = ','): base(path, delimiter)
    {
        // TODO
    }


    protected override string ReadRow(int row)
    {
        // TODO
        throw new NotImplementedException();
    }


    public override void Dispose()
    {
        // TODO

        base.Dispose();
    }
}