---
title: "Platform Invoke (P/Invoke)"
weight: 20
---

# Platform Invoke (P/Invoke)

Platform Invoke (P/Invoke) is a mechanism that allows managed code (C#) to call unmanaged functions implemented in dynamic link libraries (DLLs) (shared libraries on *Unix* systems), such as the Windows API (WinAPI) or custom libraries written in C/C++.

## The `DllImport` Attribute

For example, if we want to call a function from `user32.dll` in WinAPI:

```cpp
int MessageBox(HWND hWnd, LPCTSTR lpText, LPCTSTR lpCaption, UINT uType);
```

The traditional way to declare external functions is by using the `[DllImport]` attribute. The method must be declared as `static` and `extern`.

```csharp
using System.Runtime.InteropServices;

public class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(nint hWnd, string text, string caption, uint type);
}
```

Call:

```csharp
NativeMethods.MessageBox(nint.Zero, "Hello world!", "Info", 0);
```

> On *Unix*-like systems, you can try calling `getpid` from `libc`, for example:
> ```csharp
> [DllImport("libc")]
> public static extern int getpid();
> ```

## The `LibraryImport` Attribute (.NET 7+)

Since .NET 7, the `[LibraryImport]` attribute has been introduced, which uses *Source Generators* instead of runtime code generation (like `DllImport`). This is a more performant solution. Using `LibraryImport` instead of `DllImport` is recommended for new projects.

The method must be `static` and marked as `partial` so the generator can add the implementation.

```csharp
public partial class NativeMethods
{
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int MessageBox(nint hWnd, string text, string caption, uint type);
}
```

## Calling a Custom C++ Library

The most important thing is to use `extern "C"`. C++ compilers perform *Name Mangling* by default, adding information about arguments or namespaces to function names. `extern "C"` disables this behavior, allowing C# to find the function. The `EXPORT` macro makes the function visible outside the library.

```cpp
#if defined(_WIN32) || defined(_WIN64)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

extern "C" EXPORT void Hello();
```

Then, on the `C#` side, we must import this function using the `DllImport` or `LibraryImport` attribute.

```csharp
[LibraryImport("HelloCpp")]
private static partial void Hello();
```

> [!INFO]
> Example:  
> **C++** {{< filetree dir="lectures/interoperability/HelloCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/HelloCSharp" >}}

## Marshalling

Marshalling is the process of converting data types between managed and unmanaged code.

### Blittable and Non-blittable Types

* **Blittable types**: Types that have the same representation in memory in both managed and unmanaged code (e.g., `byte`, `int`, `float`, `nint`, arrays of blittable types, value types consisting of blittable types). They do not require additional conversion.
* **Non-blittable types**: Types that require conversion (e.g., `bool`, `string`).

```cpp
extern "C" EXPORT int Foo(unsigned char c, float x, Bar* bar, bool b);
```

```csharp
[LibraryImport("HelloCpp")]
public static partial int Foo(byte c, float x, nint bar, [MarshalAs(UnmanagedType.U1)]bool b);
```

### String Marshalling

Strings in C# (Unicode `UTF-16`) must be converted from/to the format expected by the native function (e.g., `UTF-8`, `UTF-16`).

#### As an Input Parameter

If a string is passed as a parameter and will not be modified by the native code, passing it as `string` on the C# side is sufficient.

```cpp
extern "C" EXPORT void PrintAnsiString(const char* str);
extern "C" EXPORT void PrintUnicodeString(const char16_t* str);

void PrintAnsiString(const char *str)
{
    std::println("String: '{}'\n", str);
}

void PrintUnicodeString(const char16_t *str)
{
    std::u16string u16string(str);

    std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter;
    std::string utf8String = converter.to_bytes(u16string);

    std::println("UTF-16 String: '{}'\n", utf8String);
}
```

And in the `LibraryImport` attribute, specify how the string should be processed before being passed to the C++ function.

```csharp
[LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf8)]
public static partial void PrintAnsiString(string str);

[LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf16)]
public static partial void PrintUnicodeString(string str);
```

#### As a Return Value

```cpp
extern "C" EXPORT const char* GetAnsiString();
extern "C" EXPORT const char16_t* GetUnicodeString();

const char *GetAnsiString()
{
    static const char *ansiString = "String from C++";
    return ansiString;
}

const char16_t *GetUnicodeString()
{
    static const char16_t *unicodeString = u"UTF-16 string from C++ \U0001F44B";
    return unicodeString;
}
```

The easiest way to capture a return value on the C# side is as a pointer. All memory handles are represented in C# as `IntPtr` or, since `C# 9`, as `nint` (an alias for `System.IntPtr`). This type represents a pointer with a size dependent on the processor architecture.

```csharp
[LibraryImport("StringsCpp")]
public static partial nint GetAnsiString();

[LibraryImport("StringsCpp")]
public static partial nint GetUnicodeString();
```

Then, to recover the string from such a pointer, we can use the helper methods `Marshal.PtrToStringAnsi` or `Marshal.PtrToStringUni`.

```csharp
string? ansi = Marshal.PtrToStringAnsi(GetAnsiString());
string? unicode = Marshal.PtrToStringUni(GetUnicodeString());
Console.WriteLine($"Ansi string: {ansi}");
Console.WriteLine($"Unicode string: {unicode}");
```

#### As an Input/Output Parameter

If the C++ function modifies the string passed as a parameter, we cannot pass a `string` object on the C# side - it is immutable.

```cpp
extern "C" EXPORT void Encode(char* text);

void Encode(char *text)
{
    if (text == nullptr) return;

    while (*text)
    {
        char c = *text;

        if (c >= 'A' && c <= 'Z')
        {
            *text = static_cast<char>(((c - 'A' + 13) % 26) + 'A');
        }
        else if (c >= 'a' && c <= 'z')
        {
            *text = static_cast<char>(((c - 'a' + 13) % 26) + 'a');
        }

        text++;
    }
}
```

The easiest way is to pass a character array (`byte[]` or `char[]` depending on the encoding). In the example below, we assume UTF-8 encoding (compatible with ASCII).

```csharp
[LibraryImport("StringsCpp")]
public static partial void Encode([In, Out] byte[] str);
```

```csharp
byte[] arr = "Initial content"u8.ToArray();
Encode(arr);
Console.WriteLine($"Encoded: {Encoding.UTF8.GetString(arr)}");
```

> [!INFO]
> Example:  
> **C++** {{< filetree dir="lectures/interoperability/StringsCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/StringsCSharp" >}}

## Struct Marshalling

```cpp
struct Color
{
    union
    {
        struct
        {
            uint8_t r;
            uint8_t g;
            uint8_t b;
            uint8_t a;
        };

        uint32_t rgba;
    };
};
```

To pass a structure from/to native code, you must define its equivalent in C# and use the `[StructLayout]` attribute if necessary. This attribute has one positional parameter `LayoutKind`, which can take one of two values:
* **Sequential**: Fields are laid out in memory in the order of declaration (default behavior for `struct`).
* **Explicit**: Allows you to manually specify the offset of each field using the `[FieldOffset]` attribute. Used to create union equivalents.
The optional `Pack` parameter also allows specifying field *alignment*.

```csharp
[StructLayout(LayoutKind.Explicit)]
public struct Color
{
    [FieldOffset(0)]
    public byte R;
    [FieldOffset(1)]
    public byte G;
    [FieldOffset(2)]
    public byte B;
    [FieldOffset(3)]
    public byte A;
    [FieldOffset(0)]
    public uint Rgba;
}
```

### Passing Structures

```cpp
extern "C" EXPORT Color Add(Color a, Color b);
extern "C" EXPORT void Darken(Color *color);
extern "C" EXPORT void PrintHex(Color color);
```

```csharp
[LibraryImport("StructsCpp")]
private static partial Color Add(Color a, Color b);

[LibraryImport("StructsCpp")]
private static partial void Darken(ref Color b);

[LibraryImport("StructsCpp")]
private static partial void PrintHex(Color a);
```

> [!INFO]
> Example:  
> **C++** {{< filetree dir="lectures/interoperability/StructsCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/StructsCSharp" >}}

## Passing Functions

We can pass C# methods to unmanaged code as function pointers or delegates.

```cpp
typedef void (*Callback)(int value);

extern "C" void Count(int from, int to, Callback callback);

void Count(int from, int to, Callback callback)
{
    for (int i = from; i < to; ++i) {
        callback(i);
    }
}
```

```csharp
public delegate void Callback(int value);
    
[LibraryImport("CallbacksCpp")]
private static partial void Count(int from , int to, Callback callback);

[LibraryImport("CallbacksCpp")]
private static unsafe partial void Count(int from, int to, delegate* <int, void> callback);
```

> [!WARNING]
> The Garbage Collector does not see references held by unmanaged code. If you pass a delegate to a native function that uses it later (asynchronously), you must ensure that the delegate is not collected by the *GC* (e.g., by assigning it to a static field).

> [!INFO]
> Example:  
> **C++** {{< filetree dir="lectures/interoperability/CallbacksCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/CallbacksCSharp" >}}

## SafeHandle

The `SafeHandle` class is used to wrap handles to operating system resources (memory, files, network sockets, external databases) and guarantees resource release even in the event of an exception. `SafeHandle` implements `IDisposable`, but it is also capable of releasing resources itself in the finalizer.

```cpp
extern "C" EXPORT const char* CreateString();
extern "C" EXPORT void PrintString(const char* str);
extern "C" EXPORT void DestroyString(const char* str);

const char *CreateString()
{
    std::println("Creating a C string");
    const char str[] = "C string";
    size_t length = std::strlen(str);
    char* dup = new char[length + 1];
    std::strcpy(dup, str);
    return dup;
}

void PrintString(const char *str)
{
    std::println("The C string is: '{}'", str);
}

void DestroyString(const char *str)
{
    std::println("Destroying a C string");
    delete[] str;
}
```

Instead of using `IntPtr`/`nint` for handles, it is better to create classes inheriting from `SafeHandle` that will automatically manage the lifecycle of external objects.

```csharp
public class StringSafeHandle : SafeHandle
{
    public StringSafeHandle() : base(nint.Zero, true) {}

    protected override bool ReleaseHandle()
    {
        NativeString.DestroyString(handle);
        handle = nint.Zero;
        return true;
    }

    public override bool IsInvalid => handle == nint.Zero;
}
```

Usage in the method signature:

```csharp
public static partial class NativeString
{
    [LibraryImport("SafeHandleCpp")]
    public static partial StringSafeHandle CreateString();

    [LibraryImport("SafeHandleCpp")]
    public static partial void PrintString(StringSafeHandle str);

    [LibraryImport("SafeHandleCpp")]
    public static partial void DestroyString(nint str);
}
```

> [!INFO]
> Example:  
> **C++** {{< filetree dir="lectures/interoperability/SafeHandleCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/SafeHandleCSharp" >}}
