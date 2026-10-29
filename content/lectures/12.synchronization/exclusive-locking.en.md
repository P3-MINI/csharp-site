---
title: "Exclusive Locking"
weight: 20
---

# Exclusive Locking

Exclusive locking is a synchronization mechanism ensuring that only one thread has access to a protected resource (critical section) at any given time. Other threads attempting to access the resource are suspended until the lock is released.

## The `lock` Statement

The `lock` keyword is the simplest and most commonly used method of synchronization in C#. It is syntactic sugar for the `System.Threading.Monitor` class. The lock object can be any reference type.

```csharp
lock(lockObject)
{
    statement(s);
}
```

The `lock` statement is translated as follows:

```csharp
Monitor.Enter(lockObject);
try
{
    statement(s);
}
finally { Monitor.Exit(lockObject); }
```

It is a very fast mechanism if there is no contention. In case of contention, the thread is blocked by the operating system. The `lock` statement works only within a single process. Furthermore, as mentioned in the introduction, a memory barrier is inserted before and after the `lock` block.

```csharp
int counter = 0, times = 1_000_000;
object locker = new object();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        lock(locker) counter++;
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
        lock(locker) counter--;
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/LocksDemo" >}}

> [!NOTE] 
> Starting from C# 13, it is recommended to use the `Lock` object in the `lock` statement, or use the `Lock` type directly.
> If the lock object is of type `Lock`, the `lock` statement translates as follows:
> ```csharp
> Lock.Scope scope = lockObject.EnterScope();
> try
> {
>     statement(s);
> }
> finally { scope.Dispose(); }
> ```
> The `Lock` object can also be used directly:
> ```csharp
> int counter = 0, times = 1_000_000;
> var locker = new Lock();
> 
> var increment = Task.Run(() =>
> {
>     for (int i = 0; i < times; i++)
>         using(locker.EnterScope()) counter++;
> });
> 
> var decrement = Task.Run(() =>
> {
>     for (int i = 0; i < times; i++)
>         using(locker.EnterScope()) counter--;
> });
> 
> await Task.WhenAll(increment, decrement);
> Console.WriteLine($"Counter Value: {counter}");
> ```

> [!WARNING]
> The compiler uses `Lock.EnterScope()` only when it knows statically that the expression is of type `Lock`. If a `Lock` object is converted to another type (e.g., `object`), the `lock` statement falls back to `Monitor` - the compiler reports this with warning CS9216 at the conversion:
> ```csharp
> Lock locker = new();
> object o = locker; // warning CS9216
> lock (o) { }       // Monitor.Enter/Exit, not Lock.EnterScope()
> ```
> Moreover, `Lock` has no equivalent of `Monitor.Wait`/`Monitor.Pulse` (see [Signaling]({{< ref "/lectures/12.synchronization/signaling" >}})), so a condition variable still requires locking on an ordinary object. Like `Monitor`, `Lock` is reentrant and has thread affinity: it must be released by the thread that acquired it. That is why `await` is not allowed inside a `lock` block, and a `Lock.Scope` (a `ref struct`) cannot be preserved across `await` (error CS4007).

## Mutex

The `System.Threading.Mutex` class is a synchronization primitive operating at the operating system kernel level.

It can be named, which allows for thread synchronization between different processes (e.g., preventing a second instance of an application from starting). It is significantly slower than the `lock` statement (up to 50x slower) due to the need for context switching to the kernel.

```csharp
int counter = 0, times = 1_000_000;
using var mutex = new Mutex();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        mutex.WaitOne();
        try
        {
            counter++;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
});
var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        mutex.WaitOne();
        try
        {
            counter--;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/MutexDemo" >}}

## SpinLock

`System.Threading.SpinLock` is a lightweight structure that implements locking via active waiting (*busy waiting*). Instead of blocking the thread (which costs time for context switching), the thread actively waits by checking the lock availability.

`SpinLock` is ideal for very short critical sections where the wait time is shorter than the thread context switch time. **Busy waiting consumes 100% of the CPU time on a given core while waiting.**

> [!WARNING]
> Improper use of `SpinLock` (e.g., in long critical sections) can drastically reduce program performance.

```csharp
int counter = 0, times = 1_000_000;
SpinLock spinLock = new SpinLock();

var increment = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        bool lockTaken = false;
        try
        {
            spinLock.Enter(ref lockTaken);
            counter++;
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
        }
    }
});

var decrement = Task.Run(() =>
{
    for (int i = 0; i < times; i++)
    {
        bool lockTaken = false;
        try
        {
            spinLock.Enter(ref lockTaken);
            counter--;
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
        }
    }
});

await Task.WhenAll(increment, decrement);
Console.WriteLine($"Counter Value: {counter}");
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/SpinLockDemo" >}}
