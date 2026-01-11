---
title: "Signaling"
weight: 40
---

# Signaling

Signaling mechanisms are used for communication between threads. They allow one thread to suspend its execution until it receives a notification (signal) from another thread.

## EventWaitHandle and CountdownEvent

Classes derived from `EventWaitHandle` (`ManualResetEvent`, `AutoResetEvent`) act like gates that can be either open (signaled) or closed (non-signaled). `CountdownEvent` operates on a different principle (reverse counter) and does not inherit from `EventWaitHandle`.

*   **`Set()`**: Opens the gate (sets the state to signaled).
*   **`Reset()`**: Closes the gate (sets the state to non-signaled).
*   **`WaitOne()`**: Waits at the gate. If it's open, the thread proceeds immediately. If closed, the thread is blocked until the gate opens.

> [!NOTE]
> `CountdownEvent` uses `Signal()` (decrement) and `Wait()` (wait for zero) methods.

### Event Types

1.  **`ManualResetEvent(Slim)`**: Once opened (`Set`), it remains open for any number of threads until it is manually closed (`Reset`). It acts like a classic gate.
2.  **`AutoResetEvent`**: When opened, it lets **only one** waiting thread through and immediately closes automatically. It acts like a subway turnstile.
3.  **`CountdownEvent`**: It becomes signaled only when its internal counter reaches zero. Each call to `Signal()` decrements this counter. It is useful when one thread needs to wait for a specific number of other threads to complete their work.

### Example: Producer-Consumer Queue

The following example implements a bounded-capacity queue using `ManualResetEvent` to signal whether the queue is full or empty.

> [!NOTE]
> Notice the `while(true)` loop in the `Enqueue` and `Dequeue` methods. It is necessary because between the `WaitOne()` call (signal: there is space/item) and entering the `lock`, another thread might have taken that space or item. This requires re-checking the condition.

```csharp
public class Queue<T> : IDisposable
{
    private readonly T?[] _array;
    private int _head;
    private int _tail;
    private int _count;

    private readonly ManualResetEvent _notEmpty;
    private readonly ManualResetEvent _notFull;
    private readonly Lock _lock;

    public Queue(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _array = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;

        _notEmpty = new ManualResetEvent(false);
        _notFull = new ManualResetEvent(true);
        _lock = new Lock();
    }

    public void Enqueue(T item)
    {
        while (true)
        {
            _notFull.WaitOne();

            lock (_lock)
            {
                if (_count < _array.Length)
                {
                    _array[_tail] = item;
                    _tail = (_tail + 1) % _array.Length;
                    _count++;

                    if (_count == _array.Length)
                    {
                        _notFull.Reset();
                    }

                    _notEmpty.Set();
                    return;
                }
            }
        }
    }

    public T Dequeue()
    {
        while (true)
        {
            _notEmpty.WaitOne();

            lock (_lock)
            {
                if (_count > 0)
                {
                    T item = _array[_head]!;
                    _array[_head] = default;
                    _head = (_head + 1) % _array.Length;
                    _count--;

                    if (_count == 0)
                    {
                        _notEmpty.Reset();
                    }

                    _notFull.Set();
                    return item;
                }
            }
        }
    }

    public void Dispose()
    {
        _notEmpty.Dispose();
        _notFull.Dispose();
    }
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/EventWaitHandlesExample" >}}

## Barrier

The `Barrier` class is used to synchronize a group of threads that must work in phases. Threads reach the barrier (`SignalAndWait`) and wait until all other threads in the group also arrive. Only when the full set of threads has checked in are they all released to the next phase.

### Example: Dice Game

In this example, 3 threads roll dice. The barrier ensures that all threads perform their roll in the same round before any of them proceeds to the next, and results are printed in a single line.

```csharp
class Program
{
    static void Main(string[] args)
    {
        var barrier = new Barrier(3, _ => Console.WriteLine());
        new Thread(RollDice).Start();
        new Thread(RollDice).Start();
        new Thread(RollDice).Start();
        void RollDice()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{D6()} ");
                barrier.SignalAndWait();
            }
        }
    }

    static int D6() => 1 + Random.Shared.Next(6);
}
```

> [!INFO]
> Source code:
> {{< filetree dir="lectures/synchronization/BarrierExample" >}}

## Monitor.Wait / Monitor.Pulse

This is a condition variable mechanism (associated with `lock`).

*   `Monitor.Wait(obj)`: Releases the lock on the `obj` object and suspends the thread until it receives a notification.
*   `Monitor.Pulse(obj)`: Wakes up one thread waiting on the `obj` object.
*   `Monitor.PulseAll(obj)`: Wakes up all waiting threads.

This is a lower-level mechanism than `EventWaitHandle` and requires being inside a `lock` block. Typically, using `EventWaitHandle` is sufficient and simpler.