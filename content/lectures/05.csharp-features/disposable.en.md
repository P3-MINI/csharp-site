---
title: "Disposable"
---

## The IDisposable Interface

The `IDisposable` interface provides a mechanism for releasing **unmanaged resources**. Unmanaged resources are those that the *Runtime* does not release on its own, for example, files, network sockets, external database connections, or a buffer on a graphics card.

The interface contains only one method:

```csharp
public interface IDisposable
{
    void Dispose();
}
```

Implementing the interface involves releasing the resources within the `Dispose` method.

## The `using` statement

The `using` statement ensures that the `Dispose` method is always called on an object when control leaves the scope of the statement. The `using` statement comes in two variants: with a `{}` block and without it (the latter is called a *using declaration* and is available since C# 8). We can use the `using` statement with anything that implements `IDisposable`:

```csharp
using (Stream fs = new FileStream("file.txt", FileMode.Open))
{
    // ...
} // Automatically calls Dispose() method on fs
```

```csharp
using Stream fs = new FileStream ("file.txt", FileMode.Open);
// ...
// Automatically calls Dispose() when fs goes out of scope
```

The compiler transforms the `using` statement during compilation into an equivalent `try-finally` block:

```csharp
var fs = new FileStream ("file.txt", FileMode.Open);
try
{
    // ...
}
finally
{
    if (fs != null) ((IDisposable)fs).Dispose();
}
```

> A special compiler optimization prevents a boxing operation if the `IDisposable` object is a struct.

## Implementing `IDisposable`

In the `Dispose` method, we simply release the resources. This usually comes down to calling `Dispose` on all class members. The following example shows an implementation that is consistent with the semantics described below.

```csharp
public class Logger : IDisposable
{
    private StreamWriter sw;
    private bool disposed = false;

    public Logger(string path)
    {
        sw = new StreamWriter(path, append: true);
    }

    public void Log(string message)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger), "Logger has been disposed.");
        }
        sw.WriteLine(message);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        sw.Dispose();
        disposed = true;
    }
}
```

The semantics of `IDisposable` objects should be as follows:
- After the `Dispose` method is called, calling its other methods or properties should throw an `ObjectDisposedException`.
- Calling the `Dispose` method multiple times should not cause an error.
- A disposable object (one that implements `IDisposable`) should also dispose of all disposable objects it owns.

## Calling `Dispose` from a finalizer

Sometimes a class directly manages an unmanaged resource (e.g., a raw file handle from a C++ library). In such a case, we should implement a finalizer as a "safeguard". It will run if the user forgets to call `Dispose()`, preventing a resource leak. A class that owns only managed objects (such as the `StreamWriter` above) should not have a finalizer – these objects take care of their own resources.

The *Garbage collector* is responsible for calling the finalizer, but we never know when or in what order objects will be released.

```csharp
using System.Runtime.InteropServices;

public class Logger : IDisposable
{
    private StreamWriter sw;
    private IntPtr buffer; // Unmanaged memory owned directly by this object
    private bool disposed = false;

    public Logger(string path)
    {
        sw = new StreamWriter(path, append: true);
        buffer = Marshal.AllocHGlobal(1024);
    }

    public void Log(string message) 
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger), "Logger has been disposed.");
        }
        sw.WriteLine(message);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Prevents running finalizer
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            // Call Dispose() on objects owned by this instance.
            sw?.Dispose();
        }

        // Release unmanaged resources owned by just this object.
        Marshal.FreeHGlobal(buffer);
        buffer = IntPtr.Zero;
        disposed = true;
    }

    ~Logger() => Dispose(false);
}
```

> In the `Dispose(false)` block (i.e., in the call path from the finalizer), we should **ONLY** release unmanaged resources, that is, those that the Garbage Collector does not release and only those that we are the direct owner of. Unmanaged resources include, for example, memory allocated from a C++ library or raw (`IntPtr`) handles to files, sockets or database connections. We must not touch other managed objects there (e.g., by calling `sw.Dispose()`) – they may have already been finalized.

> [!TIP]
> In modern .NET, writing a finalizer by hand is rarely necessary. Instead of storing a raw handle in an `IntPtr` field, it is better to wrap it in a class derived from [`SafeHandle`]({{< ref "/lectures/11.interoperability/pinvoke#safehandle" >}}), which has its own finalizer. A class that owns a `SafeHandle` then only needs the simple `Dispose` implementation shown earlier.
