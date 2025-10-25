namespace TypeCrafter;

public class ParseException : Exception
{
    public ParseException() { }

    public ParseException(string message) 
        : base(message) { }

    public ParseException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public static class TypeCrafter
{
    public static T CraftInstance<T>()
    {
        throw new NotImplementedException();
    }
}