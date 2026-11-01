---
title: "Współdziałanie"
weight: 100
---

## Tutorial 10: Współdziałanie

Zakres tego laboratorium obejmuje wywoływanie kodu natywnego (niezarządzanego) z C#: Platform Invoke (`DllImport` i `LibraryImport`), marshalling napisów, tablic, struktur i wywołań zwrotnych, opakowywanie zasobów natywnych w `SafeHandle`, a także kod niebezpieczny (wskaźniki, `fixed`, `stackalloc`) i jego wpływ na wydajność.

W C++ użycie biblioteki napisanej w C sprowadza się do dołączenia jej pliku nagłówkowego i zlinkowania – kompilator sprawdza każde wywołanie z deklaracjami z nagłówka. W C# kompilator nigdy nie widzi nagłówka: biblioteka jest ładowana w czasie wykonania, a każdą funkcję i strukturę trzeba **zadeklarować ponownie w C#**. Nikt nie sprawdza, czy te deklaracje zgadzają się z kodem natywnym – zły typ lub brakujący parametr nie powoduje błędu kompilacji, tylko awarię programu albo po cichu uszkodzoną pamięć. Ten tutorial pokazuje, jak pisać takie deklaracje poprawnie. Więcej szczegółów znajdziesz w wykładach [Platform Invoke]({{< ref "/lectures/11.interoperability/pinvoke" >}}) oraz [Kontekst niebezpieczny]({{< ref "/lectures/11.interoperability/unsafe" >}}).

### Kod zarządzany i niezarządzany

Kod C# jest *zarządzany*: działa pod kontrolą środowiska uruchomieniowego, które zarządza pamięcią (Garbage Collector może w dowolnym momencie przesuwać obiekty), sprawdza zakresy tablic i gwarantuje bezpieczeństwo typów. Kod napisany w C lub C++ i skompilowany do kodu maszynowego jest *niezarządzany* (natywny) – pamięcią zarządza się ręcznie i nic nie chroni nas przed wyjściem poza koniec bufora.

Wywołanie funkcji natywnej z C# za pomocą **P/Invoke** (*Platform Invoke*) obejmuje kilka kroków wykonywanych przez środowisko uruchomieniowe:

1. Przy pierwszym wywołaniu ładuje ono bibliotekę (`LoadLibrary` w Windows, `dlopen` w Linuksie/macOS) i wyszukuje funkcję po nazwie (`GetProcAddress`/`dlsym`).
2. Konwertuje argumenty do ich natywnej reprezentacji (**marshalling**) – np. `string` (w .NET w UTF-16) na `char*` w UTF-8.
3. *Przypina* obiekty zarządzane przekazywane przez wskaźnik, ponieważ GC może działać w trakcie wykonywania kodu natywnego i nie może przesunąć pamięci, której ten kod używa.
4. Po wywołaniu konwertuje z powrotem wynik i parametry wyjściowe oraz zwalnia tymczasowe bufory.

W C++ to samo zrobilibyśmy ręcznie:

```cpp
// C++ (Linux): load a library and call a function from it at run time
void* lib = dlopen("libc.so.6", RTLD_NOW);
auto getpid = reinterpret_cast<int (*)()>(dlsym(lib, "getpid"));
int pid = getpid();
```

### `DllImport` i `LibraryImport`

Funkcję natywną deklarujemy w C# jako metodę `static` bez ciała, oznaczoną atrybutem określającym nazwę biblioteki. Istnieją dwa warianty:

```csharp
using System.Runtime.InteropServices;

internal static partial class OsNative
{
    // Classic P/Invoke: the marshalling stub is generated at run time
    [DllImport("libc", EntryPoint = "getpid")]
    internal static extern int GetPidDllImport();

    // Source-generated P/Invoke: the stub is generated at compile time
    [LibraryImport("libc", EntryPoint = "getpid")]
    internal static partial int GetPid();
}
```

- `[DllImport]` (`static extern`) – kod konwertujący argumenty (tzw. *stub*) generuje środowisko uruchomieniowe przy pierwszym wywołaniu. Stosuje ono wiele niejawnych ustawień domyślnych, np. sposób konwersji `string` i `bool`.
- `[LibraryImport]` (`static partial`, .NET 7+) – stub generuje *generator kodu źródłowego* w trakcie kompilacji, dzięki czemu można go obejrzeć i debugować, a program działa z przycinaniem (*trimming*) i Native AOT. Jest to zalecany sposób w nowym kodzie. Wymaga ustawienia `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` w projekcie (wygenerowany kod używa wskaźników) oraz jawnego wyboru sposobu marshallingu dla nietrywialnych typów (np. `StringMarshalling`, `MarshalAs` dla `bool`).
- `EntryPoint` to nazwa funkcji w bibliotece – dzięki niemu metoda C# może mieć nazwę zgodną z konwencjami .NET.

### Wyszukiwanie biblioteki

Nazwę biblioteki podajemy bez przedrostka i rozszerzenia, np. `"imgfilter"`. Środowisko uruchomieniowe próbuje wariantów właściwych dla systemu operacyjnego: `imgfilter.dll` w Windows, `libimgfilter.so` w Linuksie i `libimgfilter.dylib` w macOS. Szuka najpierw w katalogu aplikacji (obok `ImageFilters.dll`), a następnie pozwala systemowi operacyjnemu przeszukać standardowe lokalizacje. Dlatego najprościej jest umieścić bibliotekę natywną w katalogu wyjściowym aplikacji.

Biblioteka jest ładowana dopiero przy pierwszym wywołaniu jednej z jej funkcji. Jeśli nie zostanie znaleziona, rzucany jest wyjątek `DllNotFoundException`; jeśli nie eksportuje funkcji o podanej nazwie – `EntryPointNotFoundException`. Dzięki temu leniwemu ładowaniu deklaracje funkcji z `kernel32.dll` nie przeszkadzają w uruchomieniu programu w Linuksie, dopóki nie zostaną wywołane.

> [!TIP]
> Nazwa `"libc"` jest traktowana przez środowisko uruchomieniowe w szczególny sposób: oznacza standardową bibliotekę C (`libc.so.6` w Linuksie z glibc, `/usr/lib/libc.dylib` w macOS), więc ta sama deklaracja działa w obu systemach.

### Marshalling i odpowiedniki typów

Typy, które mają taką samą reprezentację w pamięci po obu stronach, nazywamy **przekładalnymi** (*blittable*) – są przekazywane bez żadnej konwersji (wskaźniki do nich można przekazać bezpośrednio do kodu natywnego). Są to typy liczbowe, wskaźniki oraz struktury i tablice złożone wyłącznie z nich. `string`, `bool` i `char` są **nieprzekładalne** (*non-blittable*) – wymagają konwersji, a więc kopiowania.

Poniższa tabela pokazuje odpowiedniki popularnych typów C w C#:

| C | C# | Uwagi |
|---|---|---|
| `int32_t`, `int` | `int` | |
| `uint32_t`, `DWORD` | `uint` | |
| `int64_t` | `long` | |
| `long` | `CLong` | 32 bity w Windows, 64 bity w 64-bitowym Linuksie i macOS |
| `uint8_t`, `unsigned char` | `byte` | |
| `float`, `double` | `float`, `double` | |
| `size_t` | `nuint` | rozmiar wskaźnika |
| `void*`, `intptr_t`, uchwyty | `nint` | rozmiar wskaźnika |
| `T*` | `T*`, `ref T`, `out T`, `in T`, `T[]`, `Span<T>` | zależnie od przeznaczenia parametru |
| `const char*` (UTF-8) | `string` + `StringMarshalling.Utf8` | tylko parametry wejściowe, patrz Etap 2 |
| `const wchar_t*` (Windows) | `string` + `StringMarshalling.Utf16` | `wchar_t` ma 4 bajty w Linuksie/macOS |
| `bool` (C99, C++) | `[MarshalAs(UnmanagedType.U1)] bool` | 1 bajt |
| `BOOL` (WinAPI) | `[MarshalAs(UnmanagedType.Bool)] bool` | 4 bajty |
| `struct` | `[StructLayout(LayoutKind.Sequential)] struct` | te same pola w tej samej kolejności |
| wskaźnik na funkcję | `delegate* unmanaged[Cdecl]<...>` | patrz Etap 5 |

## Zadanie 1 - Natywne filtry obrazów

W ramach tego zadania zbudujesz aplikację konsolową przetwarzającą obrazy za pomocą natywnej biblioteki `imgfilter` w C, którą samodzielnie napiszesz i skompilujesz. Następnie zaimplementujemy te same filtry w C# (kod bezpieczny, `Span<T>` i wskaźniki) i porównamy ich wydajność. Obrazy będziemy przechowywać w formacie PPM, który znasz z [Tutorialu 3]({{< ref "/labs/03.basics" >}}).

Potrzebny będzie kompilator C:
- **Linux:** GCC lub Clang (np. `sudo apt install build-essential` w Ubuntu/Debianie).
- **macOS:** Clang z Xcode Command Line Tools (`xcode-select --install`).
- **Windows:** MSVC (`cl.exe`) – zainstaluj Visual Studio z pakietem roboczym *Desktop development with C++*. Kompilator jest dostępny tylko w konsoli ze skonfigurowanym środowiskiem: uruchom z menu Start **x64 Native Tools Command Prompt for VS** (w Windows na ARM: *ARM64 Native Tools Command Prompt*) i wykonuj z niej wszystkie polecenia `dotnet` w tym zadaniu.

Sprawdź, czy kompilator działa, poleceniem `cc --version` (Linux, macOS) lub `cl` (Windows).

### Kod początkowy

Pobierz kod początkowy. Zawiera on aplikację konsolową `ImageFilters` z klasą `Image`:

- `Width`, `Height` i `Pixels` – obraz jest przechowywany w pojedynczej tablicy `byte[]`, wiersz po wierszu, po 3 bajty (R, G, B) na piksel. Piksel `(x, y)` zaczyna się pod indeksem `(y * Width + x) * 3`. Dokładnie takiego układu oczekuje kod w C.
- `Load(path)` – wczytuje obraz PPM: tekstowy (`P3`, jak w Tutorialu 3) lub binarny (`P6` – ten sam nagłówek, a po nim surowe bajty zamiast liczb).
- `Save(path)` – zapisuje obraz w formacie `P6`.
- `Clone()` – tworzy kopię obrazu.
- `CreateTestPattern(width, height)` – generuje obraz testowy z gradientami i ostrymi krawędziami.

`Program.cs` wczytuje obraz podany jako pierwszy argument lub generuje obraz testowy 640x360 i zapisuje go do `input.ppm`. Możesz przetworzyć własne zdjęcie po skonwertowaniu go do PPM, np. programem ImageMagick: `magick photo.jpg photo.ppm`, a następnie `dotnet run -- photo.ppm`. Pliki PPM są ignorowane przez `.gitignore`.

> [!NOTE]
> #### Kod początkowy
> ![Test pattern]({{< relurl "/labs/w10.interoperability/ImageFilters.00/image.jpg" >}})
> {{< filetree dir="labs/w10.interoperability/ImageFilters.00" >}}

### Etap 1: Wywoływanie funkcji systemu operacyjnego

Zaczniemy bez pisania własnego kodu natywnego – wywołamy funkcje udostępniane przez system operacyjny: pobierzemy identyfikator procesu i spróbujemy otworzyć plik. Linux i macOS udostępniają je w standardowej bibliotece C (`libc`), Windows w `kernel32.dll`.

1. Włącz kod niebezpieczny w `ImageFilters.csproj`, dodając `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` do `PropertyGroup`. Bez tego każde użycie `[LibraryImport]` kończy się błędem `SYSLIB1062`.
2. Utwórz plik `Interop/OsNative.cs` z klasą `internal static partial class OsNative` w przestrzeni nazw `ImageFilters.Interop`. Zadeklaruj (słowo kluczowe `partial` jest potrzebne zarówno przy klasie, jak i przy metodach):
   - dla Linuksa/macOS (biblioteka `"libc"`): `GetPid()` (funkcja `getpid`) z `[LibraryImport]` oraz tę samą funkcję jako `GetPidDllImport()` z `[DllImport]`; `Open(string path, int flags)` (funkcja `open`) i `Close(int fd)` (funkcja `close`); stałą `O_RDONLY = 0`;
   - dla Windows (biblioteka `"kernel32.dll"`): `GetCurrentProcessId()` zwracającą `uint` (również w wariancie `DllImport`, jako `GetCurrentProcessIdDllImport()`), `CreateFile` (funkcja `CreateFileW`) zwracającą `nint`, `CloseHandle`; stałe `GENERIC_READ = 0x80000000`, `FILE_SHARE_READ = 1`, `OPEN_EXISTING = 3` oraz pole `INVALID_HANDLE_VALUE = -1` (typu `nint`).

   Funkcje plikowe zgłaszają błędy przez `errno` (Unix) lub `GetLastError()` (Windows), dlatego potrzebują `SetLastError = true`. Do `open` napisy przekazujemy w UTF-8, a do `CreateFileW` (`W` = *wide*) w UTF-16:

   ```csharp
   // int open(const char* path, int flags, ...) - returns -1 and sets errno on failure
   [LibraryImport("libc", EntryPoint = "open", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
   internal static partial int Open(string path, int flags);

   // HANDLE CreateFileW(...) - returns INVALID_HANDLE_VALUE and sets GetLastError() on failure
   [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
   internal static partial nint CreateFile(string fileName, uint desiredAccess, uint shareMode,
       nint securityAttributes, uint creationDisposition, uint flagsAndAttributes, nint templateFile);

   // BOOL is a 4-byte int in WinAPI
   [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   internal static partial bool CloseHandle(nint handle);
   ```

3. W `Program.cs` dodaj statyczną funkcję lokalną `ShowProcessInfo()`, która wypisze identyfikator procesu uzyskany na trzy sposoby: przez `LibraryImport`, `DllImport` i `Environment.ProcessId`. Funkcje właściwe dla bieżącego systemu wybierz za pomocą `OperatingSystem.IsWindows()`.
4. Dodaj funkcję `TryOpenFile(string path)`, która otworzy plik do odczytu (`open` z `O_RDONLY` lub `CreateFile` z `GENERIC_READ`, `FILE_SHARE_READ` i `OPEN_EXISTING`), wypisze deskryptor/uchwyt i zamknie go. W przypadku niepowodzenia powinna wypisać kod błędu i jego opis. Wywołaj ją dla `input.ppm` (istnieje) i `missing.ppm` (nie istnieje).

> [!IMPORTANT]
> #### `SetLastError` i kody błędów
>
> Funkcje natywne często zgłaszają przyczynę błędu przez globalną (lokalną dla wątku) zmienną: `errno` w C lub `GetLastError()` w WinAPI. Jednak samo środowisko uruchomieniowe wywołuje funkcje systemowe (np. przy alokacji pamięci), które mogą nadpisać tę wartość, zanim zdążymy ją odczytać. `SetLastError = true` sprawia, że stub zapisuje kod błędu natychmiast po wywołaniu. Odczytujemy go metodą `Marshal.GetLastPInvokeError()` (starsza `Marshal.GetLastWin32Error()` zwraca tę samą wartość, również w systemach Unix), a `Marshal.GetPInvokeErrorMessage(code)` zamienia go na komunikat systemowy:
>
> ```csharp
> static void PrintLastError(string call)
> {
>     // errno (Unix) or GetLastError() (Windows), saved right after the call thanks to SetLastError = true
>     int error = Marshal.GetLastPInvokeError();
>     Console.WriteLine($"{call} failed: error {error} - {Marshal.GetPInvokeErrorMessage(error)}");
> }
> ```

> [!TIP]
> Kod wygenerowany przez `[LibraryImport]` można obejrzeć w IDE (*Dependencies → Analyzers → Microsoft.Interop.LibraryImportGenerator*) lub zapisać do `obj/Debug/net10.0/generated` po dodaniu do projektu `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>`. Dla `Open` wygląda on następująco (w skrócie) – napis jest konwertowany do UTF-8 do bufora na stosie, `errno` jest zapisywane zaraz po wywołaniu, a właściwy import ma wyłącznie parametry przekładalne:
>
> ```csharp
> internal static partial int Open(string path, int flags)
> {
>     int __lastError;
>     byte* __path_native = default;
>     int __retVal = default;
>     scoped Utf8StringMarshaller.ManagedToUnmanagedIn __path_native__marshaller = new();
>     try
>     {
>         __path_native__marshaller.FromManaged(path, stackalloc byte[Utf8StringMarshaller.ManagedToUnmanagedIn.BufferSize]);
>         {
>             __path_native = __path_native__marshaller.ToUnmanaged();
>             Marshal.SetLastSystemError(0);
>             __retVal = __PInvoke(__path_native, flags);
>             __lastError = Marshal.GetLastSystemError();
>         }
>     }
>     finally
>     {
>         __path_native__marshaller.Free();
>     }
>     Marshal.SetLastPInvokeError(__lastError);
>     return __retVal;
>
>     [DllImport("libc", EntryPoint = "open", ExactSpelling = true)]
>     static extern unsafe int __PInvoke(byte* __path_native, int __flags_native);
> }
> ```

Przykładowy wynik w Linuksie i macOS (identyfikatory będą inne):

```text
Input: test pattern (640x360)
=== Operating system ===
Process ID: 48213 (LibraryImport), 48213 (DllImport), 48213 (Environment)
open("input.ppm") succeeded, fd = 31
open("missing.ppm") failed: error 2 - No such file or directory
```

W Windows (komunikat zależy od języka systemu):

```text
Input: test pattern (640x360)
=== Operating system ===
Process ID: 10432 (LibraryImport), 10432 (DllImport), 10432 (Environment)
CreateFileW("input.ppm") succeeded, handle = 0x2F0
CreateFileW("missing.ppm") failed: error 2 - The system cannot find the file specified.
```

> [!NOTE]
> #### Etap 1
> {{< filetree dir="labs/w10.interoperability/ImageFilters.01" >}}

### Etap 2: Własna biblioteka natywna

W tym etapie napiszemy bibliotekę `imgfilter` w C, skompilujemy ją w każdym systemie operacyjnym i wywołamy jej pierwsze funkcje.

> [!IMPORTANT]
> #### Eksportowanie funkcji z biblioteki współdzielonej
>
> Aby funkcję można było wywołać spoza biblioteki współdzielonej, musi ona zostać **wyeksportowana** pod znaną nazwą:
> - Biblioteka jest napisana w C, więc nazwy funkcji nie są zmieniane. W C++ trzeba deklarować funkcje z `extern "C"` – w przeciwnym razie kompilator koduje w nazwie typy parametrów (*name mangling*) i P/Invoke nie znajdzie funkcji.
> - MSVC eksportuje tylko funkcje oznaczone `__declspec(dllexport)`. GCC i Clang domyślnie eksportują wszystkie funkcje, ale z flagą `-fvisibility=hidden` zachowują się jak MSVC: eksportowane są tylko funkcje oznaczone `__attribute__((visibility("default")))`.
>
> Oba przypadki obsługuje makro w pliku nagłówkowym. Ten sam nagłówek może dołączać kod w C korzystający z biblioteki – wtedy `IMGFILTER_EXPORTS` nie jest zdefiniowane i makro rozwija się do `__declspec(dllimport)`:
>
> ```c
> #if defined(_WIN32)
> #  if defined(IMGFILTER_EXPORTS)
> #    define IMGF_API __declspec(dllexport)
> #  else
> #    define IMGF_API __declspec(dllimport)
> #  endif
> #else
> #  define IMGF_API __attribute__((visibility("default")))
> #endif
> ```

1. Utwórz katalog `ImageFilters/native`, a w nim plik nagłówkowy `imgfilter.h` z powyższym makrem `IMGF_API` (oraz standardową ochroną przed wielokrotnym dołączeniem i blokiem `extern "C"` dla użytkowników C++). Zadeklaruj:

   ```c
   IMGF_API int32_t imgf_add(int32_t a, int32_t b);
   /* Returns a static string - the caller must NOT free it */
   IMGF_API const char* imgf_version(void);
   /* Returns a newly allocated string - the caller must free it with imgf_free */
   IMGF_API char* imgf_build_info(void);
   IMGF_API void imgf_free(void* ptr);
   ```

2. Utwórz plik `imgfilter.c`. Zacznij go od `#define IMGFILTER_EXPORTS` umieszczonego **przed** `#include "imgfilter.h"` – budujemy samą bibliotekę. Zaimplementuj:
   - `imgf_add` – zwraca sumę.
   - `imgf_version` – zwraca literał napisowy `"imgfilter 1.0"`.
   - `imgf_build_info` – zwraca napis zaalokowany za pomocą `malloc`, np. `"imgfilter 1.0, GCC 15.3.1, 64-bit"`. Kompilator można rozpoznać po jego predefiniowanych makrach (`__clang__` i `__clang_version__`, `_MSC_VER`, `__GNUC__` i `__VERSION__`), a liczbę bitów po `sizeof(void*)`. Wymaganą długość bufora można uzyskać, wywołując `snprintf(NULL, 0, format, ...)`.
   - `imgf_free` – wywołuje `free`.

3. Skompiluj bibliotekę ręcznie w katalogu `native`, aby upewnić się, że działa:

   ```bash
   # Linux
   cc -std=c11 -O2 -Wall -shared -fPIC -fvisibility=hidden -o libimgfilter.so imgfilter.c
   nm -D --defined-only libimgfilter.so          # list exported functions

   # macOS
   cc -std=c11 -O2 -Wall -shared -fPIC -fvisibility=hidden -o libimgfilter.dylib imgfilter.c
   nm -gU libimgfilter.dylib

   # Windows (x64 Native Tools Command Prompt for VS)
   cl /nologo /O2 /W3 /LD imgfilter.c /Feimgfilter.dll
   dumpbin /exports imgfilter.dll
   ```

   `-shared` (`/LD` w MSVC) tworzy bibliotekę współdzieloną zamiast pliku wykonywalnego, a `-fPIC` generuje kod niezależny od położenia, wymagany w bibliotekach współdzielonych w Linuksie.

4. Ręczne budowanie po każdej zmianie i kopiowanie wyniku do `bin/Debug/net10.0` jest uciążliwe. Niech zrobi to MSBuild – dodaj do `ImageFilters.csproj` (właściwości, elementy i cele znasz z [Tutorialu 2]({{< ref "/labs/02.msbuild" >}})):

   ```xml
   <!-- File name of the native library on the current operating system -->
   <PropertyGroup>
     <NativeLibName Condition="$([MSBuild]::IsOSPlatform('Windows'))">imgfilter.dll</NativeLibName>
     <NativeLibName Condition="$([MSBuild]::IsOSPlatform('OSX'))">libimgfilter.dylib</NativeLibName>
     <NativeLibName Condition="'$(NativeLibName)' == ''">libimgfilter.so</NativeLibName>
   </PropertyGroup>

   <ItemGroup>
     <NativeSource Include="native/*.c;native/*.h" />
   </ItemGroup>

   <!-- Compile the C library into the output directory, next to ImageFilters.dll.
        Inputs/Outputs: the target is skipped when the library is up to date. -->
   <Target Name="BuildNative" AfterTargets="Build" Inputs="@(NativeSource)" Outputs="$(OutDir)$(NativeLibName)">
     <Exec Condition="!$([MSBuild]::IsOSPlatform('Windows'))"
           Command="cc -std=c11 -O2 -Wall -shared -fPIC -fvisibility=hidden -o &quot;$(OutDir)$(NativeLibName)&quot; native/imgfilter.c" />
     <!-- Requires cl.exe on PATH: run dotnet build/run from the "x64 Native Tools Command Prompt for VS" -->
     <Exec Condition="$([MSBuild]::IsOSPlatform('Windows'))"
           Command="cl /nologo /O2 /W3 /LD native\imgfilter.c /Fo&quot;$(IntermediateOutputPath)imgfilter.obj&quot; /Fe&quot;$(OutDir)$(NativeLibName)&quot;" />
   </Target>
   ```

   Od teraz `dotnet build` i `dotnet run` (także z `-c Release`) kompilują bibliotekę do katalogu wyjściowego za każdym razem, gdy zmieni się `imgfilter.c` lub `imgfilter.h`. Usuń pliki skompilowane ręcznie w kroku 3.

> [!WARNING]
> - W Windows `cl` jest dostępny tylko w *Native Tools Command Prompt*. Budowanie uruchomione ze zwykłej konsoli lub z IDE kończy się błędem `MSB3073` (kod wyjścia 9009 – nie znaleziono polecenia).
> - Biblioteka musi mieć tę samą architekturę co proces .NET (`RuntimeInformation.ProcessArchitecture`). Próba załadowania 32-bitowej biblioteki DLL zbudowanej w domyślnym *Developer Command Prompt* (x86) lub biblioteki x64 przez proces ARM64 kończy się wyjątkiem `BadImageFormatException` lub `DllNotFoundException`.

5. Utwórz plik `Interop/ImgFilterNative.cs` z klasą `internal static partial class ImgFilterNative` zawierającą surowe deklaracje (`private const string Lib = "imgfilter";`): `Add`, `Version`, `BuildInfo` i `Free`, z `EntryPoint` wskazującym na `imgf_add`, `imgf_version` itd. Napisy natywne zwracamy jako `nint` (patrz niżej).
6. Utwórz publiczną klasę statyczną `ImgFilter` (`ImgFilter.cs`) – zarządzane API ukrywające surowe deklaracje: `Add(int a, int b)` oraz właściwości `Version` i `BuildInfo` zwracające `string`.
7. W `Program.cs` dodaj funkcję `ShowLibraryInfo()`, wypisującą wersję, wynik `ImgFilter.Add(2, 3)` i informacje o kompilacji.

> [!WARNING]
> #### Kto jest właścicielem zwróconej pamięci?
>
> Typ zwracany `string` wygląda kusząco, ale wtedy marshaller nie tylko kopiuje napis natywny, lecz także go **zwalnia** za pomocą `Marshal.FreeCoTaskMem` (`free` w systemach Unix, `CoTaskMemFree` w Windows). W przypadku `imgf_version`, które zwraca literał napisowy, oznacza to zwolnienie pamięci, która nigdy nie została zaalokowana – proces kończy się awarią (w Linuksie komunikatem `free(): invalid size`). Nawet dla `imgf_build_info` działa to w Linuksie tylko przypadkiem: pamięci zaalokowanej przez `malloc` wewnątrz biblioteki DLL nie wolno zwalniać przez `CoTaskMemFree`, a w Windows biblioteka DLL może wręcz używać innej biblioteki uruchomieniowej C (innej sterty) niż reszta procesu.
>
> Zasada jest więc następująca: zwracaj `nint`, kopiuj napis za pomocą `Marshal.PtrToStringUTF8`, a pamięć należącą do wywołującego zwalniaj funkcją dostarczoną przez **tę samą bibliotekę**:
>
> ```csharp
> // We own the string: copy it and give the memory back to the library
> public static string BuildInfo
> {
>     get
>     {
>         nint ptr = ImgFilterNative.BuildInfo();
>         if (ptr == 0)
>             throw new OutOfMemoryException();
>         try
>         {
>             return Marshal.PtrToStringUTF8(ptr)!;
>         }
>         finally
>         {
>             ImgFilterNative.Free(ptr);
>         }
>     }
> }
> ```

> [!TIP]
> Jeśli biblioteki nie można umieścić obok aplikacji lub jej nazwa różni się między systemami, ładowanie biblioteki można przejąć za pomocą `NativeLibrary.SetDllImportResolver`. Resolver trzeba zarejestrować przed pierwszym wywołaniem funkcji z biblioteki:
>
> ```csharp
> NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), (libraryName, assembly, searchPath) =>
> {
>     if (libraryName != "imgfilter")
>         return 0; // 0 = fall back to the default probing
>     string fileName = OperatingSystem.IsWindows() ? "imgfilter.dll"
>         : OperatingSystem.IsMacOS() ? "libimgfilter.dylib"
>         : "libimgfilter.so";
>     return NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory, "native", fileName));
> });
> ```

Oczekiwany wynik (informacja o kompilatorze zależy od systemu – w macOS `Clang ...`, w Windows `MSVC 19xx`):

```text
=== imgfilter library ===
Version:    imgfilter 1.0
2 + 3 =     5
Build info: imgfilter 1.0, GCC 15.3.1 20260722 (Red Hat 15.3.1-1), 64-bit
```

> [!NOTE]
> #### Etap 2
> {{< filetree dir="labs/w10.interoperability/ImageFilters.02" >}}

### Etap 3: Tablice i struktury

W tym etapie zaimplementujemy w C pierwsze filtry obrazów: odwrócenie kolorów (negatyw) i konwersję do skali szarości, a także obliczanie prostych statystyk. Do kodu natywnego przekażemy bufor pikseli oraz struktury opisujące obraz.

> [!IMPORTANT]
> #### Struktury przekładalne
>
> Struktura przekazywana do kodu natywnego musi mieć w pamięci dokładnie taki sam układ jak jej odpowiednik w C: te same pola, o tych samych rozmiarach, w tej samej kolejności. `[StructLayout(LayoutKind.Sequential)]` (domyślne dla struktur w C#) gwarantuje, że pola są ułożone w kolejności deklaracji, z takimi samymi zasadami wyrównania jak w C. W poniższej strukturze `imgf_stats` kompilator wstawia 6 bajtów wypełnienia (*padding*) po polu `max`, aby `mean` zaczynało się od przesunięcia podzielnego przez 8 – struktura w C# otrzymuje dokładnie takie samo wypełnienie i obie mają 16 bajtów (można to sprawdzić za pomocą `Unsafe.SizeOf<ImageStats>()`). Struktura złożona wyłącznie z pól przekładalnych sama jest przekładalna, więc jest przekazywana bez kopiowania.
>
> ```c
> typedef struct imgf_info
> {
>     int32_t width;
>     int32_t height;
>     int32_t channels;
> } imgf_info;
>
> typedef struct imgf_stats
> {
>     uint8_t min;
>     uint8_t max;
>     double mean;
> } imgf_stats;
> ```
>
> ```csharp
> [StructLayout(LayoutKind.Sequential)]
> public readonly struct ImageInfo(int width, int height, int channels)
> {
>     public readonly int Width = width;
>     public readonly int Height = height;
>     public readonly int Channels = channels;
> }
> ```

> [!IMPORTANT]
> #### Przekazywanie buforów i referencji
>
> Funkcję w C przyjmującą `T*` można zadeklarować w C# na kilka sposobów:
> - `T[]`, `Span<T>` (dla `const T*`: `ReadOnlySpan<T>`) – w przypadku tablic typów przekładalnych stub przypina pamięć na czas wywołania i przekazuje wskaźnik na pierwszy element, bez kopiowania. Kod natywny modyfikuje więc bezpośrednio tablicę C#.
> - `ref T` – wskaźnik na zmienną, którą kod natywny odczytuje i modyfikuje, `in T` – wskaźnik na dane, które kod natywny tylko odczytuje (`const T*`), `out T` – wskaźnik na zmienną, którą kod natywny tylko zapisuje.
> - `T*` – surowy wskaźnik, wymaga kontekstu `unsafe`.
>
> Kod natywny nic nie wie o `Length` – otrzymuje tylko adres. **Rozmiar bufora trzeba zawsze przekazać jawnie**, a kod natywny musi go sprawdzić. Jeśli zapisze dane za końcem bufora, nie zostanie rzucony wyjątek `IndexOutOfRangeException` – pamięć zostanie po cichu uszkodzona, a program może ulec awarii znacznie później, w zupełnie niezwiązanym miejscu.

1. W `imgfilter.h` dodaj pokazane wyżej struktury, kody błędów (`IMGF_OK = 0`, `IMGF_ERR_NULL_POINTER = -1`, `IMGF_ERR_INVALID_ARGUMENT = -2`, `IMGF_ERR_BUFFER_TOO_SMALL = -3`, `IMGF_ERR_CHANNELS = -4`, `IMGF_ERR_OUT_OF_MEMORY = -5`) oraz funkcje:

   ```c
   /* Returns a static description of an error code */
   IMGF_API const char* imgf_error_message(int32_t code);

   IMGF_API int32_t imgf_invert(uint8_t* pixels, size_t length, const imgf_info* info);
   IMGF_API int32_t imgf_grayscale(uint8_t* pixels, size_t length, const imgf_info* info);
   IMGF_API int32_t imgf_compute_stats(const uint8_t* pixels, size_t length, const imgf_info* info,
                                       imgf_stats* stats);
   ```

2. Zaimplementuj je w `imgfilter.c`. Wszystkie filtry działają w miejscu i zwracają `IMGF_OK` lub kod błędu:
   - Najpierw zweryfikuj argumenty (pomocnicza funkcja `static`): wskaźniki `NULL`, niedodatnie wymiary, liczbę kanałów spoza zakresu `1..4` oraz `length` mniejsze niż `width * height * channels`.
   - `imgf_invert` zamienia każdy bajt `v` na `255 - v`.
   - `imgf_grayscale` (wymaga co najmniej 3 kanałów) oblicza jasność każdego piksela z całkowitoliczbowymi wagami `gray = (77 * R + 150 * G + 29 * B) >> 8` (wagi 0.299, 0.587, 0.114 pomnożone przez 256) i zapisuje ją do wszystkich trzech kanałów.
   - `imgf_compute_stats` oblicza minimum, maksimum i średnią wszystkich bajtów i zapisuje je do `*stats`.
   - `imgf_error_message` zwraca literał napisowy dla każdego kodu błędu (np. `"pixel buffer is too small"`).
3. W C# utwórz struktury `ImageInfo` i `ImageStats` (`Interop/ImageInfo.cs`, `Interop/ImageStats.cs`) i dodaj do klasy `Image` właściwość `public ImageInfo Info => new(Width, Height, Channels);`. Zadeklaruj nowe funkcje w `ImgFilterNative`:

   ```csharp
   [LibraryImport(Lib, EntryPoint = "imgf_invert")]
   internal static partial int Invert(Span<byte> pixels, nuint length, in ImageInfo info);

   [LibraryImport(Lib, EntryPoint = "imgf_compute_stats")]
   internal static partial int ComputeStats(ReadOnlySpan<byte> pixels, nuint length, in ImageInfo info,
       out ImageStats stats);
   ```

4. W `ImgFilter` dodaj `ErrorMessage(int code)` oraz metody `Invert(Image)`, `Grayscale(Image)` i `ComputeStats(Image)`. Kody błędów zamieniaj na wyjątki za pomocą metody pomocniczej `Check(int code)`, która rzuca `InvalidOperationException` z komunikatem z `imgf_error_message`, gdy kod jest różny od `0`.
5. W `Program.cs` dodaj funkcję `RunNativeFilters(Image input)`, która wypisze statystyki obrazu wejściowego, zapisze jego kopię z odwróconymi kolorami do `inverted.ppm` i kopię w skali szarości do `grayscale.ppm` (wypisując również ich statystyki), a na koniec wywoła bezpośrednio `ImgFilterNative.Invert` z 10-bajtową tablicą i wymiarami obrazu wejściowego, aby przekonać się, że kod natywny wykrywa zbyt mały bufor.

Oczekiwany wynik:

```text
=== Native filters ===
Input stats:     min = 0, max = 255, mean = 121.40
Inverted stats:  min = 0, max = 255, mean = 133.60
Grayscale stats: min = 0, max = 255, mean = 117.31
Invert with a 10-byte buffer: error -3 - pixel buffer is too small
```

> [!NOTE]
> #### Etap 3
> ![Stage 3: input, inverted, grayscale]({{< relurl "/labs/w10.interoperability/ImageFilters.03/image.jpg" >}})
> {{< filetree dir="labs/w10.interoperability/ImageFilters.03" >}}

### Etap 4: Kod niebezpieczny i wydajność

Czy warto wywoływać kod w C, aby program działał szybciej? W tym etapie zaimplementujemy te same filtry w C# na trzy sposoby, dodamy rozmycie uśredniające 3x3 (*box blur*) i zmierzymy wydajność wszystkich wariantów.

> [!IMPORTANT]
> #### Wskaźniki i `fixed`
>
> W kontekście niebezpiecznym (metoda, klasa lub blok `unsafe`) C# pozwala używać wskaźników z tymi samymi operatorami co C++: `*`, `&`, `->`, `[]` oraz arytmetyką wskaźników. Adresu obiektu zarządzanego (np. tablicy) nie można jednak po prostu pobrać, ponieważ GC może w każdej chwili przesunąć obiekt. Instrukcja `fixed` *przypina* obiekt na czas trwania bloku i daje wskaźnik na jego pierwszy element. Działa ona również ze `Span<T>`:
>
> ```csharp
> // 3. Unsafe code: pointer arithmetic, no bounds checks
> public static unsafe void GrayscaleUnsafe(Span<byte> pixels)
> {
>     fixed (byte* start = pixels)
>     {
>         byte* end = start + pixels.Length - pixels.Length % 3;
>         for (byte* p = start; p < end; p += 3)
>         {
>             // ...
>         }
>     }
> }
> ```

> [!IMPORTANT]
> #### `stackalloc`
>
> `stackalloc` alokuje mały bufor na stosie – jak tablica zadeklarowana jako zmienna lokalna w C++. Taka pamięć nie obciąża GC i jest zwalniana automatycznie po powrocie z metody. Przypisana do `Span<T>` nie wymaga nawet kontekstu `unsafe`. Pamięć na stosie nigdy się nie przesuwa, więc można ją też przekazać do kodu natywnego bez przypinania. Używaj jej tylko dla małych buforów (do kilku kilobajtów), ponieważ stos jest niewielki (zwykle 1 MB w Windows, 8 MB w Linuksie).

1. Utwórz strukturę `Rgb24` (`Rgb24.cs`) – jeden piksel z trzema polami typu `byte`: `[StructLayout(LayoutKind.Sequential)] public record struct Rgb24(byte R, byte G, byte B);`.
2. Utwórz klasę statyczną `ManagedFilters` (`ManagedFilters.cs`) i zaimplementuj konwersję do skali szarości (z tym samym wzorem co w C) na trzy sposoby:
   - `GrayscaleSafe(byte[] pixels)` – pętla po tablicy z indeksowaniem: `pixels[i]`, `pixels[i + 1]`, `pixels[i + 2]`.
   - `GrayscaleSpan(Span<byte> pixels)` – `MemoryMarshal.Cast<byte, Rgb24>(pixels)` reinterpretuje bajty jako piksele bez kopiowania (podobnie jak `reinterpret_cast` w C++), a `foreach (ref Rgb24 p in ...)` iteruje po nich przez referencję, pozwalając na modyfikację.
   - `GrayscaleUnsafe(Span<byte> pixels)` – arytmetyka wskaźników wewnątrz `fixed` (patrz fragment powyżej).
3. Zaimplementuj splot 3x3 w dwóch wariantach: `Convolve3x3Safe` (indeksowanie `byte[]`) i `Convolve3x3Unsafe` (wskaźniki na piksele źródłowe i docelowe oraz na jądro, uzyskane za pomocą `fixed`), oba z sygnaturą `(Image source, Image destination, ReadOnlySpan<int> kernel, int divisor)`:
   - Dla każdego piksela i każdego kanału oblicz sumę 9 sąsiednich wartości pomnożonych przez wagi jądra (*kernel*) (`kernel[0]` dla lewego górnego sąsiada, `kernel[4]` dla samego piksela, `kernel[8]` dla prawego dolnego).
   - Na krawędziach obrazu użyj najbliższego piksela wewnątrz obrazu (ogranicz współrzędne sąsiadów do zakresów `0..Width-1` i `0..Height-1`).
   - Wynikiem jest `sum / divisor` ograniczone do zakresu `0..255` (`Math.Clamp`).
   - Napisz oba warianty w ten sam sposób (np. przesunięcia wiersza powyżej i poniżej oraz kolumny po lewej i po prawej obliczaj raz na wiersz/piksel), tak aby jedyną różnicą było indeksowanie zamiast wskaźników. Przed wariantem niebezpiecznym zweryfikuj argumenty (te same rozmiary obrazów, 9 elementów jądra, niezerowy dzielnik) – niżej nie ma już żadnej siatki bezpieczeństwa.
4. Dodaj metodę `BoxBlur(Image source, bool useUnsafe)`, która tworzy jądro na stosie i zwraca nowy, rozmyty obraz:

   ```csharp
   // Small, short-lived buffer: allocated on the stack, not on the GC heap
   Span<int> kernel = stackalloc int[9];
   kernel.Fill(1);
   ```

5. W `Program.cs` dodaj funkcję `RunManagedFilters(Image input)`, która porówna wynik każdego zarządzanego wariantu konwersji do skali szarości z wynikiem natywnym (`a.Pixels.AsSpan().SequenceEqual(b.Pixels)`), porówna oba warianty rozmycia i zapisze rozmyty obraz do `blur.ppm`.
6. Utwórz klasę statyczną `Benchmark` (`Benchmark.cs`) z metodą `Run()`, wywoływaną na końcu `Program.cs`. Na obrazie testowym 1920x1080 zmierz średni czas: każdego wariantu konwersji do skali szarości (20 iteracji; ponowna konwersja obrazu, który jest już szary, kosztuje tyle samo), każdego wariantu rozmycia (5 iteracji) oraz 10 milionów wywołań zarządzanej metody `Add` oznaczonej `[MethodImpl(MethodImplOptions.NoInlining)]` w porównaniu z 10 milionami wywołań natywnej funkcji `imgf_add` (3 iteracje). Użyj metody pomocniczej:

   ```csharp
   /// <summary>Returns the average time of one iteration in milliseconds.</summary>
   private static double Measure(string name, int iterations, Action action)
   {
       // Warm-up: JIT compilation, loading the native library, filling CPU caches
       action();

       var stopwatch = Stopwatch.StartNew();
       for (int i = 0; i < iterations; i++)
           action();
       stopwatch.Stop();
       // ...
   }
   ```

Uruchom program w konfiguracji Release – w Debug JIT nie optymalizuje kodu, więc pomiary nie mają sensu (wypisz ostrzeżenie w `#if DEBUG`):

```bash
dotnet run -c Release
```

Przykładowy wynik (Linux x64, GCC 15 `-O2`; Twoje czasy będą inne):

```text
=== Managed filters ===
Grayscale same as native: safe True, Span True, unsafe True
Box blur: safe same as unsafe: True
=== Benchmark ===
Image: 1920x1080
Grayscale: safe (byte[])           2.79 ms
Grayscale: Span<Rgb24>             2.63 ms
Grayscale: unsafe (pointers)       1.93 ms
Grayscale: native (C)              2.18 ms
Box blur: safe (byte[])           39.05 ms
Box blur: unsafe (pointers)       27.88 ms
10M calls: managed Add            19.29 ms
10M calls: native imgf_add        23.83 ms
One call: managed 1.93 ns, native 2.38 ns
```

Co pokazują te liczby?
- **Sprawdzanie zakresów.** W bezpiecznym splocie każdy z 9 odczytów z `src[...]` (oraz z `kernel[...]`) jest poprzedzony sprawdzeniem zakresu. JIT usuwa takie sprawdzenia tylko wtedy, gdy potrafi udowodnić, że indeks mieści się w zakresie – typowo w pętli `for (int i = 0; i < array.Length; i++)`. Indeksy takie jak `above + left + c` są sprawdzane za każdym razem, dlatego wariant bezpieczny jest o około 40% wolniejszy od wariantu ze wskaźnikami. W prostej konwersji do skali szarości różnica jest znacznie mniejsza.
- **Kod natywny to nie magia.** C# skompilowany przez JIT i C skompilowane przez GCC mogą dać bardzo podobny kod maszynowy – niebezpieczny wariant w C# jest tak samo szybki jak C. Wywoływanie kodu natywnego opłaca się, gdy biblioteka już istnieje lub korzysta z czegoś niedostępnego w .NET, a nie tylko po to, „żeby było szybciej”. Większe zyski dają lepsze algorytmy i wektoryzacja (`Vector128<T>`, `Vector<T>`).
- **Koszt przejścia.** Wywołanie P/Invoke z argumentami przekładalnymi kosztuje zaledwie kilka nanosekund, podobnie jak zwykłe wywołanie metody – ale takiej metody nie da się wstawić w miejscu wywołania (*inlining*), a każdy argument nieprzekładalny (`string`, `bool[]`) dokłada konwersję i kopiowanie. Jedno wywołanie na obraz jest pomijalne; jedno wywołanie na piksel (2 miliony wywołań) trwałoby dłużej niż cały filtr skali szarości. Natywne API powinno więc być *gruboziarniste* – jedno wywołanie przetwarza cały bufor.

> [!TIP]
> `Stopwatch` z rozgrzewką wystarcza do zgrubnego porównania. Do wiarygodnych pomiarów (wiele uruchomień, statystyki, alokacje pamięci) użyj biblioteki [BenchmarkDotNet](https://benchmarkdotnet.org/).

> [!NOTE]
> #### Etap 4
> ![Stage 4: input and box blur, zoomed in]({{< relurl "/labs/w10.interoperability/ImageFilters.04/image.jpg" >}})
> {{< filetree dir="labs/w10.interoperability/ImageFilters.04" >}}

### Etap 5: Wywołania zwrotne i uchwyty

Dotychczas pamięć zawsze należała do C#. Teraz biblioteka sama zaalokuje obraz i zwróci wskaźnik na obiekt, którego układ jest przed nami ukryty (*typ nieprzejrzysty*, ang. *opaque type*), a długo trwający splot będzie raportował postęp, wywołując funkcję C#.

> [!IMPORTANT]
> #### `SafeHandle`
>
> Wskaźnik na obiekt natywny można by przechowywać jako `nint`, ale wtedy musimy pamiętać o jego zwolnieniu – dokładnie raz, również w przypadku wyjątku, i nigdy w chwili, gdy inny wątek wciąż go używa. Klasa dziedzicząca po `SafeHandle` rozwiązuje te problemy:
> - jej metoda `ReleaseHandle()` jest wywoływana dokładnie raz – z `Dispose()` lub, jeśli o nim zapomnimy, z finalizatora,
> - gdy jest parametrem funkcji P/Invoke, jej licznik referencji jest zwiększany na czas wywołania, więc uchwyt nie może zostać w międzyczasie zwolniony,
> - może być typem zwracanym przez importowaną funkcję – marshaller tworzy obiekt (za pomocą **publicznego** konstruktora bezparametrowego, w przeciwnym razie budowanie kończy się błędem `SYSLIB1051`) i zapisuje w nim wskaźnik natywny.
>
> `SafeHandleZeroOrMinusOneIsInvalid` to klasa bazowa dla uchwytów, dla których `0` (`NULL`) i `-1` oznaczają wartość nieprawidłową:
>
> ```csharp
> public sealed class SafeImageHandle : SafeHandleZeroOrMinusOneIsInvalid
> {
>     // Called by the marshaller when a native function returns imgf_image*
>     public SafeImageHandle() : base(ownsHandle: true)
>     {
>     }
>
>     protected override bool ReleaseHandle()
>     {
>         ImgFilterNative.ImageDestroy(handle);
>         return true;
>     }
> }
> ```

> [!IMPORTANT]
> #### Wskaźniki na funkcje i `[UnmanagedCallersOnly]`
>
> Wskaźnikowi na funkcję w C `void (*)(int32_t, int32_t)` odpowiada w C# niezarządzany wskaźnik na funkcję `delegate* unmanaged[Cdecl]<int, int, void>` (ostatni argument typu to typ zwracany). Może on wskazywać na metodę statyczną oznaczoną `[UnmanagedCallersOnly]` – JIT generuje dla niej punkt wejścia, który można wywołać bezpośrednio z kodu natywnego. Taka metoda:
> - musi być `static` i może mieć wyłącznie przekładalne parametry i typ zwracany,
> - nie może być wywołana z C# (błąd `CS8901`) – pobieramy jedynie jej adres operatorem `&`,
> - **nie może rzucać wyjątków** – wyjątek nie może przejść przez kod natywny, więc proces zostaje zakończony (`try`/`catch` wokół wywołania P/Invoke nie pomoże).
>
> ```csharp
> [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
> private static void ReportProgress(int doneRows, int totalRows) { /* ... */ }
>
> delegate* unmanaged[Cdecl]<int, int, void> progress = &ReportProgress;
> ```

1. W `imgfilter.h` zadeklaruj typ nieprzejrzysty (pola `struct imgf_image` są zdefiniowane tylko w `imgfilter.c`), typ wywołania zwrotnego oraz funkcje:

   ```c
   typedef struct imgf_image imgf_image;

   /* Called after each processed row */
   typedef void (*imgf_progress_fn)(int32_t done_rows, int32_t total_rows);

   IMGF_API imgf_image* imgf_image_create(int32_t width, int32_t height, int32_t channels);
   IMGF_API void imgf_image_destroy(imgf_image* image);
   IMGF_API imgf_info imgf_image_info(const imgf_image* image);
   IMGF_API uint8_t* imgf_image_pixels(imgf_image* image);
   IMGF_API int32_t imgf_image_convolve3x3(imgf_image* image, const int32_t kernel[9], int32_t divisor,
                                           imgf_progress_fn progress);
   /* Number of images created and not yet destroyed */
   IMGF_API int32_t imgf_live_images(void);
   ```

2. Zaimplementuj je w `imgfilter.c`. `struct imgf_image` zawiera `imgf_info` i wskaźnik na piksele. `imgf_image_create` zwraca `NULL` dla nieprawidłowych argumentów lub gdy nie uda się zaalokować pamięci, oraz zwiększa statyczny licznik istniejących obrazów (`imgf_image_destroy` go zmniejsza). `imgf_image_info` zwraca strukturę **przez wartość**. `imgf_image_convolve3x3` implementuje w miejscu ten sam algorytm co `Convolve3x3Safe`: najpierw kopiuje piksele do tymczasowego bufora (każdy piksel wynikowy potrzebuje oryginalnych sąsiadów), a po każdym wierszu wywołuje `progress(y + 1, height)`, jeśli `progress` nie jest `NULL`.
3. W C# utwórz klasę `SafeImageHandle` (`Interop/SafeImageHandle.cs`, jak wyżej) i zadeklaruj nowe funkcje w `ImgFilterNative`. Klasa wymaga teraz modyfikatora `unsafe`, ponieważ niektóre deklaracje używają wskaźników:

   ```csharp
   [LibraryImport(Lib, EntryPoint = "imgf_image_create")]
   internal static partial SafeImageHandle ImageCreate(int width, int height, int channels);

   // Called only from SafeImageHandle.ReleaseHandle, hence the raw handle
   [LibraryImport(Lib, EntryPoint = "imgf_image_destroy")]
   internal static partial void ImageDestroy(nint image);

   [LibraryImport(Lib, EntryPoint = "imgf_image_pixels")]
   internal static partial byte* ImageGetPixels(SafeImageHandle image);

   [LibraryImport(Lib, EntryPoint = "imgf_image_convolve3x3")]
   internal static partial int ImageConvolve3x3(SafeImageHandle image, ReadOnlySpan<int> kernel, int divisor,
       delegate* unmanaged[Cdecl]<int, int, void> progress);
   ```

   Zadeklaruj również `ImageGetInfo` (zwracającą `ImageInfo`) i `LiveImages` oraz dodaj właściwość `ImgFilter.LiveImages`.
4. Utwórz klasę `public sealed unsafe class NativeImage : IDisposable` (`NativeImage.cs`), która opakowuje `SafeImageHandle`:
   - Konstruktor `(int width, int height, int channels)` tworzy obraz natywny i rzuca `ArgumentException`, jeśli uchwyt jest nieprawidłowy (`IsInvalid`). Strukturę `ImageInfo` zwróconą przez `ImageGetInfo` zapisuje we właściwości `Info`.
   - Właściwość `Pixels` zwraca `new Span<byte>(pointer, length)` – widok na pamięć natywną, bez kopiowania. Rzuć `ObjectDisposedException`, gdy uchwyt jest już zamknięty (`ObjectDisposedException.ThrowIf(_handle.IsClosed, this)`).
   - `static NativeImage FromImage(Image image)` i `Image ToImage()` kopiują piksele między pamięcią zarządzaną a natywną (`Span<T>.CopyTo`).
   - `Convolve3x3(ReadOnlySpan<int> kernel, int divisor, bool reportProgress = false)` wywołuje natywny splot, przekazując `&ReportProgress` lub `null`. `ReportProgress` wypisuje linię co 25% wierszy: `int step = Math.Max(1, totalRows / 4);` i wypisuje, gdy `doneRows % step == 0 || doneRows == totalRows`.
   - `Dispose()` zwalnia uchwyt.
5. W `Program.cs` dodaj funkcję `RunNativeImage(Image input)`, która:
   - w bloku `using` tworzy `NativeImage` z obrazu wejściowego, wypisuje jego `Info` i liczbę istniejących obrazów natywnych, rozmywa go z raportowaniem postępu (jądro można podać jako wyrażenie kolekcji `[1, 1, 1, 1, 1, 1, 1, 1, 1]` – dla parametru `ReadOnlySpan<int>` kompilator nie alokuje tablicy) i porównuje wynik z `ManagedFilters.BoxBlur`; następnie ponownie kopiuje do niego obraz wejściowy, wyostrza go jądrem `[0, -1, 0, -1, 5, -1, 0, -1, 0]` z dzielnikiem `1` i zapisuje wynik do `sharpen.ppm`;
   - po bloku `using` ponownie wypisuje liczbę istniejących obrazów;
   - wywołuje pomocniczą funkcję `CreateAndForget()` (oznaczoną `[MethodImpl(MethodImplOptions.NoInlining)]`), która tworzy `NativeImage` i go nie zwalnia, wypisuje liczbę istniejących obrazów, wywołuje `GC.Collect()` i `GC.WaitForPendingFinalizers()`, i wypisuje ją jeszcze raz – finalizator `SafeHandle` zwalnia zapomniany obraz.
6. W klasie `Benchmark` zmierz również natywne rozmycie.

> [!WARNING]
> `Span<byte>` wskazujący na pamięć natywną nie jest śledzony przez GC – po `Dispose()` wcześniej uzyskany widok wskazuje na zwolnioną pamięć (*use after free*), dokładnie jak wiszący wskaźnik w C++. Używaj takich widoków krótko, a jeśli dane są potrzebne dłużej, skopiuj je (`ToImage()`). Alternatywą bez kodu `unsafe` jest `Marshal.Copy(nint source, byte[] destination, int startIndex, int length)`.

> [!TIP]
> Zamiast wskaźnika na funkcję można też przekazać delegat. Może to być metoda instancyjna lub lambda przechwytująca zmienne lokalne, na co `[UnmanagedCallersOnly]` nie pozwala. `Marshal.GetFunctionPointerForDelegate` tworzy dla niego natywny punkt wejścia – jest on jednak ważny tylko tak długo, jak żyje obiekt delegatu. GC nie widzi referencji przechowywanych przez kod natywny, więc delegat musi pozostać żywy, dopóki kod natywny używa wskaźnika (`GC.KeepAlive` lub pole, jeśli kod natywny zapamiętuje wskaźnik na później):
>
> ```csharp
> [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
> delegate void ProgressCallback(int doneRows, int totalRows);
>
> var callback = new ProgressCallback((done, total) => Console.WriteLine($"{done}/{total}"));
> nint pointer = Marshal.GetFunctionPointerForDelegate(callback);
> ImgFilterNative.ImageConvolve3x3(handle, kernel, 9, (delegate* unmanaged[Cdecl]<int, int, void>)pointer);
> GC.KeepAlive(callback); // the delegate must not be collected before this line
> ```

Oczekiwany wynik:

```text
=== Native image ===
Created native image 640x360x3, live native images: 1
Box blur (native):
  progress:  25% (90/360 rows)
  progress:  50% (180/360 rows)
  progress:  75% (270/360 rows)
  progress: 100% (360/360 rows)
Same as managed box blur: True
After Dispose, live native images: 0
Without Dispose, live native images: 1
After GC, live native images: 0
```

oraz w pomiarach wydajności (Release):

```text
Box blur: native (C)              26.85 ms
```

> [!NOTE]
> #### Etap 5
> ![Stage 5: input, box blur and sharpen, zoomed in]({{< relurl "/labs/w10.interoperability/ImageFilters.05/image.jpg" >}})
> {{< filetree dir="labs/w10.interoperability/ImageFilters.05" >}}

### Kierunki dalszego rozwoju

Aplikację napisaną w ramach tutoriala można rozwijać w wielu kierunkach:

- **Wektoryzacja (SIMD):** Zaimplementuj filtry za pomocą `Vector128<T>`/`Vector256<T>` lub `System.Numerics.Tensors.TensorPrimitives` i porównaj je z kodem C skompilowanym z `-O3 -march=native`. Wektoryzacja zwykle daje znacznie większe przyspieszenie niż przejście na C.
- **Równoległość:** Przetwarzaj wiersze równolegle za pomocą `Parallel.For` w C# (lub OpenMP w C). Zwróć uwagę, że wywołanie zwrotne raportujące postęp byłoby wtedy wywoływane z wielu wątków jednocześnie.
- **Kontekst wywołania zwrotnego:** Prawdziwe biblioteki C przekazują do wywołań zwrotnych dodatkowy argument `void* user_data`. Przekaż przez niego `GCHandle` obiektu zarządzanego (`GCHandle.ToIntPtr`), aby skierować wywołanie do metody instancyjnej lub do `IProgress<T>`.
- **Własny marshalling:** Struktury zawierające napisy lub `bool` nie są przekładalne. Zdefiniuj sposób ich konwersji za pomocą atrybutów `[NativeMarshalling]` i `[CustomMarshaller]`.
- **Wieloplatformowe budowanie:** Zastąp polecenia kompilatora w pliku `.csproj` CMake'iem i spakuj bibliotekę dla kilku systemów w pakiet NuGet (`runtimes/<rid>/native/`).
- **Biblioteka w C++:** Przepisz `imgfilter` w C++. Pamiętaj o `extern "C"` oraz o tym, że wyjątki C++ nie mogą wydostać się poza eksportowane funkcje.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/10.interoperability/example1" >}})
