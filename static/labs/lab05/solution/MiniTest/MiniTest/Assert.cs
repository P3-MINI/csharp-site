namespace MiniTest;

/// <summary>
/// Provides a set of assertion methods used for validating test conditions in the MiniTest framework.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Asserts that the specified action throws an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of exception expected to be thrown.</typeparam>
    /// <param name="action">The action expected to throw the exception.</param>
    /// <param name="message">An optional message to include in the exception if the assertion fails.</param>
    /// <returns>The thrown exception of type <typeparamref name="TException"/>.</returns>
    /// <exception cref="AssertionException">
    /// Thrown if the action does not throw an exception, or throws an exception of a different type.
    /// </exception>
    public static TException ThrowsException<TException>(Action action, string message = "") where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            message = $"Expected exception type:<{typeof(TException)}>. Actual exception type:<{ex.GetType()}>. {message}";
            throw new AssertionException(message);
        }

        message = $"Expected exception type:<{typeof(TException)}> but no exception was thrown. {message}";
        throw new AssertionException(message);
    }

    /// <summary>
    /// Asserts that two values are equal.
    /// </summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">An optional message to include in the exception if the assertion fails.</param>
    /// <exception cref="AssertionException">Thrown if the values are not equal.</exception>
    public static void AreEqual<T>(T? expected, T? actual, string message = "")
    {
        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (comparer.Equals(expected, actual))
        {
            return;
        }

        message = $"Expected: {expected?.ToString() ?? "null"}. Actual: {actual?.ToString() ?? "null"}. {message}";
        throw new AssertionException(message);
    }

    /// <summary>
    /// Asserts that two values are not equal.
    /// </summary>
    /// <typeparam name="T">The type of the values to compare.</typeparam>
    /// <param name="notExpected">The value that is not expected.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">An optional message to include in the exception if the assertion fails.</param>
    /// <exception cref="AssertionException">Thrown if the values are equal.</exception>
    public static void AreNotEqual<T>(T? notExpected, T? actual, string message = "")
    {
        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (!comparer.Equals(notExpected, actual))
        {
            return;
        }

        message = $"Expected any value except: {notExpected?.ToString() ?? "null"}. Actual: {actual?.ToString() ?? "null"}. {message}";
        throw new AssertionException(message);
    }

    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">An optional message to include in the exception if the assertion fails.</param>
    /// <exception cref="AssertionException">Thrown if the condition is false.</exception>
    public static void IsTrue(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new AssertionException(message);
        }
    }

    /// <summary>
    /// Asserts that a condition is false.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">An optional message to include in the exception if the assertion fails.</param>
    /// <exception cref="AssertionException">Thrown if the condition is true.</exception>
    public static void IsFalse(bool condition, string message = "")
    {
        if (condition)
        {
            throw new AssertionException(message);
        }
    }

    /// <summary>
    /// Fails a test with the specified message.
    /// </summary>
    /// <param name="message">An optional message to include in the exception.</param>
    /// <exception cref="AssertionException">Always thrown to indicate a failed test.</exception>
    public static void Fail(string message = "")
    {
        throw new AssertionException(message);
    }
}