---
title: "Kontekst niebezpieczny"
weight: 10
---

# Kontekst niebezpieczny (Unsafe)

C# jest językiem bezpiecznym. Środowisko uruchomieniowe dba o zarządzanie pamięcią (*Garbage Collector*) i sprawdza zakresy tablic. Jednak w pewnych sytuacjach - takich jak współdziałanie z kodem niezarządzanym (np. bibliotekami C/C++) lub potrzeba optymalizacji wydajności - konieczne jest obejście tych zabezpieczeń. Do tego służy słowo kluczowe `unsafe`.

Kod oznaczony jako `unsafe` pozwala na używanie wskaźników i bezpośrednią manipulację pamięcią, podobnie jak w języku `C++`. Aby używać kontekstu `unsafe`, projekt musi zostać skompilowany ze specjalną flagą. W pliku projektu należy dodać właściwość `AllowUnsafeBlocks`:

```xml
<PropertyGroup>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

## Słowo kluczowe `unsafe`

Słowo kluczowe `unsafe` zmienia kontekst na niebezpieczny, taki w którym można używać wskaźników. Modyfikatora `unsafe` można używać w następujących kontekstach:

1.  **Klasa lub struktura:**
    ```csharp
    public unsafe struct Node
    {
        public Node* Next;
        public int Value;
    }
    ```

2.  **Metoda:**
    ```csharp
    public unsafe void ClearMemory(void* memory, int bytes)
    {
        // Unsafe code
    }
    ```

3.  **Blok kodu:**
    ```csharp
    unsafe
    {
        // Unsafe code
    }
    ```

## Wskaźniki

W kontekście `unsafe` możemy używać wskaźników. Obsługiwane są standardowe operatory znane z C/C++:                                  

- `*` - dereferencja
- `&` - pobranie adresu
- `->` - dostęp do składowych przez wskaźnik
- `[]` - indeksowanie wskaźnika

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

> Rzutowanie między wskaźnikami musi być jawne, z wyjątkiem rzutowania do typu `void*`.

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
> W C# symbol wskaźnika (`*`) jest integralną częścią *typu* wskaźnikowego, a nie modyfikatorem nazwy zmiennej, jak w C/C++.
> 
> ```
> int* p1, p2, p3;     // OK: p1, p2, p3 ARE int*
> int *p1, *p2, *p3;   // Invalid in C#
> ```

## Instrukcja `fixed`

*Garbage Collector* może w dowolnym momencie przesuwać obiekty w pamięci w celu defragmentacji sterty. Jeśli pobierzemy adres obiektu zarządzanego (np. tablicy), a *GC* go przesunie, wskaźnik stanie się nieprawidłowy.                                                        

Instrukcja `fixed` "przypina" (ang. *pins*) obiekt w pamięci, zapobiegając jego przesuwaniu przez GC na czas trwania bloku. Wskaźniki uzyskane w instrukcji `fixed` są tylko do odczytu. Przypinanie obiektów powinno trwać jak najkrócej, ma ono negatywny wpływ na wydajność środowiska uruchomieniowego. Nie powinno się także alokować pamięci na stercie, gdy obiekty są przypięte.

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

Instrukcji `fixed` można także użyć do pobrania wskaźnika na łańcuch znaków:

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
> Tak, można w ten sposób także zmodyfikować łańcuch znaków. Taka modyfikacja jest jednak niebezpieczna. Łańcuchy znaków w C# są internowane, tj. wszystkie zmienne i literały o tej samej zawartości wskazują na jeden napis w pamięci.

## `stackalloc`

`stackalloc` to słowo kluczowe w C#, które pozwala na alokację bloku pamięci na stosie zamiast na stercie.

Normalnie w C#, tablice (np. `new int[100]`) są obiektami tworzonymi na stercie, co obciąża *Garbage Collector*. `stackalloc` jest ekstremalnie szybki i w ogóle nie angażuje *GC*.

Zaalokowaną pamięć można przypisać do typu wskaźnikowego `T*` lub do typu `Span<T>` (od `C# 7.2`). Przypisanie do typu `Span<T>` nie wymaga niebezpiecznego kontekstu.

```csharp
Span<int> span = stackalloc int[100];
unsafe
{
    int* ptr = stackalloc int[100];
}

```

> [!TIP]
> Atrybut `[SkipLocalsInit]` (C# 9.0) może zostać użyty na metodzie, aby zapobiec zerowaniu pamięci alokowanej na stosie, co może poprawić wydajność, jeżeli i tak zaraz chcemy tę pamięć nadpisać.

> [!INFO]
> Ilość pamięci, którą w ten sposób można zaalokować, jest ograniczona rozmiarem stosu. Alternatywnie podobną funkcjonalność dostarcza klasa `ArrayPool<T>`. Klasa ta pozwala na reużywanie jednokrotnie zaalokowanej pamięci ze sterty.
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

## Pola tablicowe o stałym rozmiarze

Słowo kluczowe `fixed` może też być użyte do deklaracji tablicy bezpośrednio wewnątrz struktury, zapewniając, że dane są osadzone liniowo w układzie pamięci struktury, zamiast być referencją do oddzielnego obiektu na stercie.

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

## Wskaźniki na funkcje (C# 9.0)

Wskaźnik na funkcję `delegate*` to typ wskaźnikowy, który pozwala przechowywać **surowy adres w pamięci**, pod którym znajduje się kod maszynowy funkcji. W porównaniu do delegatów wskaźniki na funkcję nie powodują alokacji na stercie. Jest to po prostu odpowiednik wskaźników na funkcje z `C/C++`. Do wskaźnika na funkcje można przypisać wyłącznie adres statycznej metody.

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

NativeMemory (wprowadzony w .NET 6) to statyczna klasa służąca do alokacji pamięci na stercie niezarządzanej (przez Garbage Collector). Jest to warstwa abstrakcji na funkcje do zarządzania pamięcią z biblioteki standardowej C:

- `Alloc` ≈ `malloc`
- `AllocZeroed` ≈ `calloc`
- `Realloc` ≈ `realloc`
- `Free` ≈ `free`
- `Fill`, `Clear` ≈ `memset`
- `Copy` ≈ `memcpy`

**Pamięć zaalokowana w ten sposób musi być manualnie zwolniona.**

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

> W starszych wersjach .NETa można użyć klasy `Marshal` udostępniającą podobną funkcjonalność.

## Operator `sizeof`

Normalnie operator `sizeof` działa tylko dla typów prostych. W kontekście niebezpiecznym można go także użyć do pobrania rozmiaru typów niezarządzanych, czyli takich struktur, które składają się z typów prostych lub innych typów niezarządzanych.

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
