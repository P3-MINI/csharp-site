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
        Delimiter = delimiter;

        CreateOffsetFile();
    }
    
    protected string CsvFile;
    protected string OffsetsFile;
    protected readonly char Delimiter;

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
            else if (c == Delimiter && !insideQuotes)
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
        
        int byteRead;
        while ((byteRead = csvFileReader.ReadByte()) > 0)
        {
            if (byteRead == '\n')
            {
                RowsCnt++;
                writer.Write(csvFileReader.Position);
            }
        }
    }

    public virtual void Dispose()
    {
        if (File.Exists(OffsetsFile))
            File.Delete(OffsetsFile);
    }
}
