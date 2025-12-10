namespace BigCSVReader.Tests;

public class TestDataGenerator
{
    public static void GenerateSimpleCsv(string path)
    {
        var lines = new[]
        {
            "Name,Age,City",
            "Alice,30,New York",
            "Bob,25,Los Angeles",
            "Charlie,35,Chicago",
            "David,28,Houston",
            "Eve,32,Phoenix"
        };
        
        File.WriteAllLines(path, lines);
    }

    public static void GenerateQuotedCsv(string path)
    {
        var lines = new[]
        {
            "Name,Description,Value",
            "Item1,\"Contains, comma\",100",
            "Item2,\"Contains \"\"quotes\"\"\",200",
            "Item3,Simple,300",
            "Item4,\"Multi,part,value\",400"
        };
        
        File.WriteAllLines(path, lines);
    }

    public static void GenerateWithVariousDelimiters(string path, char delimiter = '|')
    {
        var lines = new[]
        {
            $"Name{delimiter}Age{delimiter}Salary",
            $"Alice{delimiter}30{delimiter}75000",
            $"Bob{delimiter}25{delimiter}65000",
            $"Charlie{delimiter}35{delimiter}85000"
        };
        
        File.WriteAllLines(path, lines);
    }
}
