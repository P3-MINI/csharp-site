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

Słowo kluczowe `unsafe` zmienia kontekst na niebezpieczny, taki, w którym można używać wskaźników. Modyfikatora `unsafe` można używać w następujących kontekstach:

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

    public static Node* Find(Node* head, int value)
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
> ```csharp
> int* p1, p2, p3;     // OK: p1, p2, p3 ARE int*
> int *p1, *p2, *p3;   // Invalid in C#
> ```

## Instrukcja `fixed`

*Garbage Collector* może w dowolnym momencie przesuwać obiekty w pamięci w celu defragmentacji sterty. Jeśli pobierzemy adres obiektu zarządzanego (np. tablicy), a *GC* go przesunie, wskaźnik stanie się nieprawidłowy.                                                        

Instrukcja `fixed` "przypina" (ang. *pins*) obiekt w pamięci, zapobiegając jego przesuwaniu przez GC na czas trwania bloku. Wskaźnik zadeklarowany w instrukcji `fixed` jest tylko do odczytu (nie można przypisać mu innego adresu, ale można modyfikować pamięć, na którą wskazuje), dlatego w poniższym przykładzie iterujemy osobnym wskaźnikiem. Przypinanie obiektów powinno trwać jak najkrócej, ma ono negatywny wpływ na wydajność środowiska uruchomieniowego. Nie powinno się także alokować pamięci na stercie, gdy obiekty są przypięte.

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
> Tak, można w ten sposób także zmodyfikować łańcuch znaków. Taka modyfikacja jest jednak niebezpieczna: łamie niezmienność łańcuchów znaków, na której polega cała platforma (np. kody skrótu, klucze słowników). Co więcej, literały napisowe w C# są internowane, tj. wszystkie literały o tej samej zawartości wskazują na jeden napis w pamięci - modyfikacja jednego z nich modyfikuje je wszystkie.

## `stackalloc`

`stackalloc` to słowo kluczowe w C#, które pozwala na alokację bloku pamięci na stosie zamiast na stercie.

Normalnie w C# tablice (np. `new int[100]`) są obiektami tworzonymi na stercie, co obciąża *Garbage Collector*. `stackalloc` jest ekstremalnie szybki i w ogóle nie angażuje *GC*.

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
> Ilość pamięci, którą w ten sposób można zaalokować, jest ograniczona rozmiarem stosu. Alternatywnie podobną funkcjonalność dostarcza klasa `ArrayPool<T>`. Klasa ta pozwala na wielokrotne wykorzystanie raz zaalokowanej pamięci ze sterty.
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

### `Span<T>`

`Span<T>` (oraz jego wersja tylko do odczytu `ReadOnlySpan<T>`) to `ref struct` reprezentujący ciągły obszar pamięci: tablicę (lub jej fragment), pamięć zaalokowaną przez `stackalloc` lub pamięć niezarządzaną (zob. `NativeMemory` poniżej). Zapewnia dostęp ze sprawdzaniem zakresu bez kontekstu `unsafe`, dzięki czemu jedna metoda przyjmująca `Span<T>` może działać na dowolnym rodzaju pamięci.

Od C# 14 konwersje z `T[]` do `Span<T>` i `ReadOnlySpan<T>`, ze `Span<T>` do `ReadOnlySpan<T>` oraz z `string` do `ReadOnlySpan<char>` są wbudowane w język (ang. *implicit span conversions*). Wcześniej były to zwykłe operatory konwersji zdefiniowane przez użytkownika, których kompilator nie bierze pod uwagę np. przy wywołaniach metod rozszerzających czy wnioskowaniu typów generycznych. Teraz metody rozszerzające dla `Span<T>` z klasy `MemoryExtensions` można wywoływać bezpośrednio na tablicach:

```csharp
int[] array = [1, 2, 3, 4];
Span<int> onStack = stackalloc int[] { 1, 2, 3, 4 };

Console.WriteLine(Sum(array));   // 10 (int[] -> ReadOnlySpan<int>)
Console.WriteLine(Sum(onStack)); // 10 (Span<int> -> ReadOnlySpan<int>)
Console.WriteLine(array.StartsWith([1, 2])); // True (C# 14)

static int Sum(ReadOnlySpan<int> values)
{
    int sum = 0;
    foreach (int value in values)
        sum += value;
    return sum;
}
```

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

### Tablice inline (C# 12)

Pola tablicowe o stałym rozmiarze mają ograniczenia: wymagają kontekstu `unsafe`, typ elementów musi być typem prostym (np. `int`, `float`, `char`), a indeksowanie nie sprawdza zakresu. Od C# 12 (.NET 8) taki sam układ pamięci można uzyskać w bezpiecznym kodzie za pomocą *tablicy inline* (ang. *inline array*) - struktury oznaczonej atrybutem `[InlineArray(length)]`, która zawiera dokładnie jedno pole. Środowisko uruchomieniowe powtarza to pole `length` razy.

```csharp
using System.Runtime.CompilerServices;

[InlineArray(16)]
public struct Float16
{
    private float _element0;
}

public struct Matrix4f
{
    // Stored inline, like a fixed-size buffer, but no unsafe code is needed.
    public Float16 Elements;

    public float this[int row, int col]
    {
        get => Elements[row * 4 + col];
        set => Elements[row * 4 + col] = value;
    }
}
```

Tablicę inline można indeksować (ze sprawdzaniem zakresu), iterować po niej pętlą `foreach` oraz konwertować do `Span<T>` lub `ReadOnlySpan<T>`. Typ jej elementów może być dowolny, także referencyjny. Układ pamięci jest taki sam jak w przypadku tablicy w C, więc struktury zawierające tablice inline o elementach typów przekładalnych (*blittable*) można również przekazywać do kodu natywnego (podobnie jak pola tablicowe o stałym rozmiarze).

```csharp
var matrix = new Matrix4f();
matrix[1, 2] = 5;
Span<float> elements = matrix.Elements;
Console.WriteLine(elements.Length); // 16
matrix[4, 0] = 1; // IndexOutOfRangeException
```

## Wskaźniki na funkcje (C# 9.0)

Wskaźnik na funkcję `delegate*` to typ wskaźnikowy, który pozwala przechowywać **surowy adres w pamięci**, pod którym znajduje się kod maszynowy funkcji. W porównaniu do delegatów wskaźniki na funkcję nie powodują alokacji na stercie. Jest to odpowiednik wskaźników na funkcje z `C/C++`. Do wskaźnika na funkcję można przypisać wyłącznie adres statycznej metody. Domyślnie `delegate*` używa zarządzanej konwencji wywołania, więc można go wywołać tylko z C#. Wskaźniki na funkcje przekazywane do kodu natywnego lub z niego otrzymywane deklaruje się jako `delegate* unmanaged` (zob. [P/Invoke]({{< ref "/lectures/11.interoperability/pinvoke" >}})).

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

NativeMemory (wprowadzony w .NET 6) to statyczna klasa służąca do alokacji pamięci na stercie niezarządzanej (czyli takiej, którą nie zarządza Garbage Collector). Jest to warstwa abstrakcji na funkcje do zarządzania pamięcią z biblioteki standardowej C:

- `Alloc` ≈ `malloc`
- `AllocZeroed` ≈ `calloc`
- `Realloc` ≈ `realloc`
- `Free` ≈ `free`
- `Fill`, `Clear` ≈ `memset` (.NET 7)
- `Copy` ≈ `memcpy` (.NET 7)

**Pamięć zaalokowana w ten sposób musi być ręcznie zwolniona.**

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

> W starszych wersjach .NETa można użyć klasy `Marshal` udostępniającej podobną funkcjonalność (`Marshal.AllocHGlobal`, `Marshal.FreeHGlobal`).

## Operator `sizeof`

Normalnie operator `sizeof` działa tylko dla typów prostych. W kontekście niebezpiecznym można go także użyć do pobrania rozmiaru typów niezarządzanych, czyli takich struktur, które składają się z typów prostych lub innych typów niezarządzanych.

```csharp
unsafe
{
    Console.WriteLine($"Sizeof {nameof(Vector3)}: {sizeof(Vector3)}"); // Sizeof Vector3: 12
}

struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
```

## Pola `ref` i `scoped` (C# 11)

Wiele scenariuszy, które kiedyś wymagały wskaźników, można wyrazić za pomocą zarządzanych referencji (`ref`), które są bezpieczne: kompilator pilnuje, aby referencja nie przeżyła danych, na które wskazuje, a *GC* je śledzi. Od C# 11 `ref struct` (zob. [Struktury]({{< ref "/lectures/04.creating-types/structs" >}})) może zawierać pola `ref`. W ten sposób zaimplementowany jest sam `Span<T>` - jako `ref` do pierwszego elementu i długość.

```csharp
int x = 1;
var r = new Ref<int>(ref x);
r.Value = 42;
Console.WriteLine(x); // 42

public ref struct Ref<T>
{
    public ref T Value; // ref field

    public Ref(ref T value) => Value = ref value;
}
```

Modyfikator `scoped` (C# 11) ogranicza czas życia parametru `ref` albo parametru lub zmiennej lokalnej typu `ref struct` do bieżącej metody: nie można go zwrócić ani zapisać w polu. Dzięki temu wywołujący może przekazać referencje do danych lokalnych, np. bufora zaalokowanego przez `stackalloc`:

```csharp
var reader = new Reader("Hello, world!");
Span<char> buffer = stackalloc char[5];
int count = reader.Read(buffer);
Console.WriteLine(buffer[..count].ToString()); // Hello

public ref struct Reader
{
    private ReadOnlySpan<char> _text;

    public Reader(ReadOnlySpan<char> text) => _text = text;

    // Without 'scoped', the call with a stackalloc buffer would not compile (error CS8350),
    // because the method could store 'destination' in the '_text' field.
    public int Read(scoped Span<char> destination)
    {
        int count = Math.Min(destination.Length, _text.Length);
        _text[..count].CopyTo(destination);
        _text = _text[count..];
        return count;
    }
}
```
