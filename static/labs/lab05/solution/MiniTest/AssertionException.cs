namespace MiniTest;

public sealed class AssertionException : Exception
{
    public AssertionException()
    {
    }

    public AssertionException(string? message) : base(message)
    {
    }

    public AssertionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}