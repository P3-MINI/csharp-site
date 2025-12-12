using System.Text;

namespace BigCsvReaders;

public abstract class BigCsvReader: IDisposable
{
    public string this[int row, int col]
    {
        get
        {
            if (row == _cachedRowIndex)
                return _cachedRow![col];
         
            if (row > RowsCnt)
                throw new IndexOutOfRangeException();
                
            UpdateCachedRow(row);
            return _cachedRow![col];
        }
    }
    
    
    public BigCsvReader(string path, char delimiter = ',')
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File {path} does not exists");

        CsvFile = path;
        OffsetsFile = Path.ChangeExtension(CsvFile, ".offsets");
        _delimiter = delimiter;

        CreateOffsetFile();
    }
    
    protected readonly string CsvFile;
    protected readonly string OffsetsFile;
    private readonly char _delimiter;

    protected int RowsCnt;

    private List<string>? _cachedRow = null;
    private int _cachedRowIndex = -1;
    
    protected abstract string ReadRow(int row);

    private void UpdateCachedRow(int row)
    {
        string line = ReadRow(row);
        _cachedRow = ParseRow(line);
        _cachedRowIndex = row;
    }
    
    private List<string> ParseRow(string row)
    {
        var result = new List<string>();
        bool insideQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < row.Length; i++)
        {
            char c = row[i];

            if (c == '"')
            {
                // Check for escaped quote ("")
                if (insideQuotes && i + 1 < row.Length && row[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // skip next quote
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (c == _delimiter && !insideQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        // Add the last field
        result.Add(current.ToString().Trim());

        return result;
    }
    
    private void CreateOffsetFile()
    {
        using FileStream offsetFileStream = File.Create(OffsetsFile);
        using BinaryWriter writer = new BinaryWriter(offsetFileStream);
        
        using FileStream csvFileReader = File.OpenRead(CsvFile);

        RowsCnt = 0;
        writer.Write(0L);
        
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = csvFileReader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == '\n')
                {
                    RowsCnt++;
                    long offset = csvFileReader.Position - (bytesRead - i - 1);
                    writer.Write(offset);
                }
            }
        }
    }

    public virtual void Dispose()
    {
        if (File.Exists(OffsetsFile))
            File.Delete(OffsetsFile);
    }
}
