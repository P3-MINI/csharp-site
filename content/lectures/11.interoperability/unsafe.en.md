---
title: "Unsafe context"
weight: 10
---

# Unsafe context

C# is a safe language. The runtime environment takes care of memory management (*Garbage Collector*) and checks array bounds. However, in certain situations - such as interoperability with unmanaged code (e.g., C/C++ libraries) or the need for performance optimization - it is necessary to bypass these safeguards. The `unsafe` keyword serves this purpose.

Code marked as `unsafe` allows the use of pointers and direct memory manipulation, similar to `C++`. To use the `unsafe` context, the project must be compiled with a special flag. The `AllowUnsafeBlocks` property must be added to the project file:

```xml
<PropertyGroup>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

## The `unsafe` keyword

The `unsafe` keyword changes the context to an unsafe one, where pointers can be used. The `unsafe` modifier can be used in the following contexts:

1.  **Class or struct:**
    ```csharp
    public unsafe struct Node
    {
        public Node* Next;
        public int Value;
    }
    ```

2.  **Method:**
    ```csharp
    public unsafe void ClearMemory(void* memory, int bytes)
    {
        // Unsafe code
    }
    ```

3.  **Code block:**
    ```csharp
    unsafe
    {
        // Unsafe code
    }
    ```

## Pointers

In an `unsafe` context, we can use pointers. Standard operators known from C/C++ are supported:

- `*` - dereference
- `&` - address of
- `->` - pointer member access
- `[]` - pointer indexing

```csharp
public static unsafe float IntBitsToFloat(int val)
{
    int* intPtr = &val;
    float* floatPtr = (float*)intPtr;
    return *floatPtr;
}
```

```csharp
public static unsafe void MemCopy(void* src, void* dst, int bytes)
{
    byte* source = (byte*)src;
    byte* destination = (byte*)dst;
    for (int i = 0; i < bytes; i++)
    {
        destination[i] = source[i];
    }
}
```

> Casting between pointers must be explicit, except for casting to `void*`.

```csharp
public unsafe struct Node
{
    public Node* Next;
    public int Value;

    public Node* Find(Node* head, int value)
    {
        while (head != null)
        {
            if (head->Value == value)
                return head;
            head = head->Next;
        }

        return null;
    }
}
```

> [!WARNING]
> In C#, the pointer symbol (`*`) is an integral part of the pointer *type*, not a modifier of the variable name, as in C/C++.
> 
> ```
> int* p1, p2, p3;     // OK: p1, p2, p3 ARE int*
> int *p1, *p2, *p3;   // Invalid in C#
> ```

## The `fixed` statement

The *Garbage Collector* can move objects in memory at any time to defragment the heap. If we take the address of a managed object (e.g., an array) and the *GC* moves it, the pointer will become invalid.

The `fixed` statement "pins" the object in memory, preventing the GC from moving it for the duration of the block. Pointers obtained in a `fixed` statement are read-only. Object pinning should be as short as possible, as it has a negative impact on runtime performance. Memory should also not be allocated on the heap while objects are pinned.

```csharp
public static unsafe void ConvertToGrayscale(int[,] image)
{
    int length = image.Length;

    fixed (int* imageData = image) // or `fixed (int* imageData = &image[0, 0])`
    {
        byte* ptr = (byte*)imageData;
        
        for (int i = 0; i < length; i++, ptr += 4)
        {
            byte b = ptr[0];
            byte g = ptr[1];
            byte r = ptr[2];

            byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

            ptr[0] = gray;
            ptr[1] = gray;
            ptr[2] = gray;
        }
    }
}
```

The `fixed` statement can also be used to get a pointer to a string:

```csharp
string text = "Hello";
unsafe
{
    fixed (char* ptr = text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            Console.WriteLine(ptr[i]);
        }
    }
}
```

> [!WARNING]
> Yes, strings can be modified this way. However, such modification is unsafe. Strings in C# are interned, meaning all variables and literals with the same content point to the same string in memory.

## `stackalloc`

`stackalloc` is a keyword in C# that allows allocating a block of memory on the stack instead of the heap.

Normally in C#, arrays (e.g., `new int[100]`) are objects created on the heap, which puts load on the *Garbage Collector*. `stackalloc` is extremely fast and does not involve the *GC* at all.

Allocated memory can be assigned to a pointer type `T*` or to a `Span<T>` (since `C# 7.2`). Assignment to `Span<T>` does not require an unsafe context.

```csharp
Span<int> span = stackalloc int[100];
unsafe
{
    int* ptr = stackalloc int[100];
}

```

> [!TIP]
> The `[SkipLocalsInit]` attribute (C# 9.0) can be used on a method to prevent zeroing of memory allocated on the stack, which can improve performance if we intend to overwrite that memory immediately anyway.

> [!INFO]
> The amount of memory that can be allocated this way is limited by the stack size. Alternatively, similar functionality is provided by the `ArrayPool<T>` class. This class allows reusing memory allocated once from the heap.
> 
> ```csharp
> using System.Buffers;
> 
> var pool = ArrayPool<byte>.Shared;
> byte[] buffer = pool.Rent(64 * 1024);
> try
> {
>     // use buffer
> }
> finally
> {
>     pool.Return(buffer, clearArray: false);
> }
> ```

## Fixed-size buffers

The `fixed` keyword can also be used to declare an array directly within a struct, ensuring that the data is embedded linearly in the struct's memory layout, instead of being a reference to a separate object on the heap.

```csharp
public unsafe struct Matrix4f
{
    // Stored inline. No heap allocation for the array.
    public fixed float Elements[16]; 

    public float this[int row, int col]
    {
        get => Elements[row * 4 + col];
        set => Elements[row * 4 + col] = value;
    }
}
```

## Function pointers (C# 9.0)

A function pointer `delegate*` is a pointer type that allows storing a **raw memory address** where the machine code of a function is located. Compared to delegates, function pointers do not cause heap allocations. It is simply the equivalent of function pointers from `C/C++`. Only the address of a static method can be assigned to a function pointer.

```csharp
public unsafe class Program
{
    public static void Main(string[] args)
    {
        delegate* <string, void> callback = &Log;
        callback("Hello World!");
    }

    public static void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now}: {message}");
    }
}
```

## `NativeMemory` (.NET 6)

NativeMemory (introduced in .NET 6) is a static class used for allocating memory on the unmanaged heap (not managed by the Garbage Collector). It is an abstraction layer over memory management functions from the C standard library:

- `Alloc` ≈ `malloc`
- `AllocZeroed` ≈ `calloc`
- `Realloc` ≈ `realloc`
- `Free` ≈ `free`
- `Fill`, `Clear` ≈ `memset`
- `Copy` ≈ `memcpy`

**Memory allocated in this way must be manually freed.**

```csharp
using System.Runtime.InteropServices;

public unsafe class Program
{
    public static void Main(string[] args)
    {
        const int items = 100;
        int* data = (int*)NativeMemory.Alloc(items * sizeof(int));
        try
        {
            Span<int> span = new Span<int>(data, items);
            for (int i = 0; i < items; i++)
            {
                span[i] = i * i;
            }

            Console.WriteLine(span[^1]);
        }
        finally
        {
            NativeMemory.Free(data);
        }
    }
}
```

> In older .NET versions, the `Marshal` class can be used, which provides similar functionality.

## The `sizeof` operator

Normally, the `sizeof` operator works only for simple types. In an unsafe context, it can also be used to get the size of unmanaged types, i.e., structs that consist of simple types or other unmanaged types.

```csharp
unsafe
{
    Console.WriteLine($"Sizeof {nameof(Vector3)}: {sizeof(Vector3)}");
}

struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
```