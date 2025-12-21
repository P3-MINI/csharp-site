---
title: "Platform Invoke (P/Invoke)"
weight: 20
---

# Platform Invoke (P/Invoke)

Platform Invoke (P/Invoke) to mechanizm pozwalający kodowi zarządzanemu (C#) na wywoływanie funkcji niezarządzanych zaimplementowanych w bibliotekach dynamicznych DLL (biblioteki współdzielone w systemach *Unix*), np. API systemu Windows (WinAPI) lub własne biblioteki napisane w C/C++.

## Atrybut `DllImport`

Przykładowo chcemy wywołać funkcję z `user32.dll` z WinAPI:

```cpp
int MessageBox(HWND hWnd, LPCTSTR lpText, LPCTSTR lpCaption, UINT uType);
```

Tradycyjnym sposobem deklarowania funkcji zewnętrznych jest użycie atrybutu `[DllImport]`. Metoda musi być zadeklarowana jako `static` i `extern`.

```csharp
using System.Runtime.InteropServices;

public class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(nint hWnd, string text, string caption, uint type);
}
```

Wywołanie:

```csharp
NativeMethods.MessageBox(nint.Zero, "Hello world!", "Info", 0);
```

> Na systemach z rodziny *Unix* możesz spróbować wywołać np. `getpid` z `libc`:
> ```csharp
> [DllImport("libc")]
> public static extern int getpid();
> ```

## Atrybut `LibraryImport` (.NET 7+)

Od wersji .NET 7 wprowadzono atrybut `[LibraryImport]`, który wykorzystuje generatory kodu źródłowego (*Source Generators*) zamiast generowania kodu w czasie wykonywania (jak `DllImport`). Jest to bardziej wydajne rozwiązanie. W nowych projektach zaleca się stosowanie `LibraryImport` zamiast `DllImport`.

Metoda musi być statyczna i oznaczona jako `partial`, aby generator mógł dopisać implementację.

```csharp
public partial class NativeMethods
{
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int MessageBox(nint hWnd, string text, string caption, uint type);
}
```

## Wywołanie własnej biblioteki C++

Najważniejsze jest użycie `extern "C"`. Kompilatory `C++` domyślnie zmieniają nazwy funkcji (tzw. *Name Mangling*), dodając do nich informacje o argumentach czy przestrzeniach nazw. `extern "C"` wyłącza to zachowanie, dzięki czemu C# może znaleźć taką funkcję. Makro `EXPORT` sprawia że funkcja staje się widoczna na zewnątrz biblioteki.

```cpp
#if defined(_WIN32) || defined(_WIN64)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

extern "C" EXPORT void Hello();
```

Następnie po stronie `C#` musimy zaimportować tę funkcję używając atrybutu `DllImport` lub `LibraryImport`.

```csharp
[LibraryImport("HelloCpp")]
private static partial void Hello();
```

> [!INFO]
> Przykład:  
> **C++** {{< filetree dir="lectures/interoperability/HelloCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/HelloCSharp" >}}

## Marshalling

Marshalling to proces konwersji typów danych między kodem zarządzanym a niezarządzanym.

### Typy przekładalne (*blittable*) i nieprzekładalne (*non-blittable*)

* **Typy przekładalne** (*blittable*): Typy, które mają taką samą reprezentację w pamięci w kodzie zarządzanym i niezarządzanym (np. `byte`, `int`, `float`, `nint`, tablice typów przekładalnych, typy bezpośrednie składające się z typów przekładalnych). Nie wymagają dodatkowej konwersji.
* **Typy nieprzekładalne** (*non-blittable*): Typy wymagające konwersji (np. `bool`, `string`).

```cpp
extern "C" EXPORT int Foo(unsigned char c, float x, Bar* bar, bool b);
```

```csharp
[LibraryImport("HelloCpp")]
public static partial int Foo(byte c, float x, nint bar, [MarshalAs(UnmanagedType.U1)]bool b);
```

### Marshalling napisów

Napisy w C# (Unicode `UTF-16`) muszą zostać przekonwertowane z/do formatu oczekiwanego przez funkcję natywną (np. `UTF-8`, `UTF-16`).

#### Jako parametr wejściowy

Jeżeli napis przekazywany jest jako parametr i nie będzie modyfikowany po stronie natywnego kodu, to wystarczy po stronie C# przekazać go jako `string`.

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

A w atrybucie `LibraryImport` wyspecyfikować jak napis ma zostać przetworzony przed przekazaniem do funkcji C++.

```csharp
[LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf8)]
public static partial void PrintAnsiString(string str);

[LibraryImport("StringsCpp", StringMarshalling = StringMarshalling.Utf16)]
public static partial void PrintUnicodeString(string str);
```

#### Jako wartość zwracana

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

Wartość zwracaną najprościej przechwycić po stronie C# jako wskaźnik. Wszelkie uchwyty do pamięci reprezentujemy po stronie C# jako `IntPtr` lub od `C# 9` jako `nint` (alias do `System.IntPtr`). Jest to typ reprezentujący wskaźnik o rozmiarze zależnym od architektury procesora.

```csharp
[LibraryImport("StringsCpp")]
public static partial nint GetAnsiString();

[LibraryImport("StringsCpp")]
public static partial nint GetUnicodeString();
```

Następnie, żeby z takiego wskaźnika odzyskać napis możemy użyć metod pomocniczych `Marshal.PtrToStringAnsi` lub `Marshal.PtrToStringUni`.

```csharp
string? ansi = Marshal.PtrToStringAnsi(GetAnsiString());
string? unicode = Marshal.PtrToStringUni(GetUnicodeString());
Console.WriteLine($"Ansi string: {ansi}");
Console.WriteLine($"Unicode string: {unicode}");
```

#### Jako parametr wejściowo-wyjściowy

Jeżeli funkcja C++ modyfikuje przekazany jako parametr napis, to nie możemy po stronie C# przekazać obiektu `string` - jest niezmienialny. 

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

Najprościej jest wtedy przekazać tablicę znaków (`byte[]` lub `char[]` w zależności od kodowania). W poniższym przykładzie zakładamy kodowanie UTF-8 (kompatybilne z ASCII). 

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
> Przykład:  
> **C++** {{< filetree dir="lectures/interoperability/StringsCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/StringsCSharp" >}}

## Marshalling struktur

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

Aby przekazać strukturę z/do kodu natywnego, należy zdefiniować jej odpowiednik w C# i jeżeli to konieczne użyć atrybutu `[StructLayout]`. Atrybut ten ma jeden parametr pozycyjny `LayoutKind`, który może przyjmować jedną z dwóch wartości:
* **Sequential**: Pola są ułożone w pamięci w kolejności deklaracji (zachowanie domyślne dla `struct`).
* **Explicit**: Pozwala ręcznie określić przesunięcie każdego pola za pomocą atrybutu `[FieldOffset]`. Używane do tworzenia odpowiedników unii.
Opcjonalny parametr `Pack` pozwala również wyspecyfikować wyrównanie pól (*alignment*).

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

### Przekazanie struktur

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
> Przykład:  
> **C++** {{< filetree dir="lectures/interoperability/StructsCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/StructsCSharp" >}}

## Przekazywanie funkcji

Możemy przekazywać metody C# do kodu niezarządzanego jako wskaźniki na funkcje lub delegaty.

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
> Garbage Collector nie widzi referencji trzymanej przez kod niezarządzany. Jeśli przekażesz delegat do funkcji natywnej, która użyje go później (asynchronicznie), musisz zadbać o to, by delegat nie został usunięty przez *GC* (np. przypisując go do statycznego pola).

> [!INFO]
> Przykład:  
> **C++** {{< filetree dir="lectures/interoperability/CallbacksCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/CallbacksCSharp" >}}

## SafeHandle

Klasa `SafeHandle` służy do owijania uchwytów do zasobów systemu operacyjnego (pamięć, pliki, gniazda sieciowe, zewnętrzna baza danych) i gwarantuje zwolnienie zasobu nawet w przypadku wystąpienia wyjątku. `SafeHandle` implementuje `IDisposable`, ale jest też w stanie zwolnić sama zasoby w finalizerze.

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

Zamiast używać `IntPtr`/`nint` dla uchwytów, lepiej jest stworzyć klasy dziedziczące po `SafeHandle`, które automatycznie będą zarządzać cyklem życia zewnętrznych obiektów.

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

Użycie w sygnaturze metody:

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
> Przykład:  
> **C++** {{< filetree dir="lectures/interoperability/SafeHandleCpp" >}}
> 
> **C#** {{< filetree dir="lectures/interoperability/SafeHandleCSharp" >}}
