---
title: "Disposable"
---

## Interfejs IDisposable

Interfejs `IDisposable` zapewnia mechanizm do zwalniania **niezarządzanych zasobów**. Niezarządzanych, czyli takich, których *Runtime* sam nie zwalnia, np. pliki, gniazda sieciowe, połączenia z zewnętrzną bazą danych, bufor na karcie graficznej.

Interfejs zawiera tylko jedną metodę:

```csharp
public interface IDisposable
{
    void Dispose();
}
```

Implementacja interfejsu polega na zwolnieniu zasobów w obrębie metody `Dispose`.

## Instrukcja `using`

Instrukcja `using` gwarantuje, że metoda `Dispose` zawsze zostanie wywołana na obiekcie, gdy sterowanie opuści zakres działania instrukcji. Instrukcja `using` występuje w dwóch wariantach: z blokiem `{}` i bez niego (ten drugi wariant to tzw. deklaracja `using`, ang. *using declaration*, dostępna od C# 8). Instrukcji `using` możemy użyć ze wszystkim, co implementuje `IDisposable`:

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

Kompilator zamienia instrukcję `using` podczas kompilacji na równoważny blok `try-finally`:

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

> Specjalna optymalizacja kompilatora zapobiega operacji pakowania (*boxing*), jeżeli obiekt `IDisposable` jest strukturą.

## Implementacja `IDisposable`

W metodzie `Dispose` po prostu zwalniamy zasoby. Zazwyczaj sprowadza się to do wywołania `Dispose` wszystkich składowych klasy. Poniższy przykład pokazuje implementację, która jest zgodna z opisaną niżej semantyką.

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

Semantyka obiektów `IDisposable` powinna być następująca:
- Po wywołaniu metody `Dispose` wywoływanie jego pozostałych metod lub właściwości powinno rzucać wyjątek `ObjectDisposedException`.
- Wielokrotne wywołanie metody `Dispose` nie powinno powodować błędu.
- Obiekt zwalnialny (implementujący `IDisposable`) powinien również zwalniać wszystkie zwalnialne obiekty, których jest właścicielem.

## Wywołanie `Dispose` z finalizatora

Czasami klasa zarządza bezpośrednio niezarządzanym zasobem (np. surowym uchwytem do pliku z biblioteki C++). W takim przypadku powinniśmy zaimplementować finalizator jako "ubezpieczenie". Uruchomi się on, jeśli użytkownik zapomni wywołać `Dispose()`, zapobiegając wyciekowi zasobu. Klasa, która posiada jedynie obiekty zarządzane (takie jak powyższy `StreamWriter`), nie powinna mieć finalizatora – te obiekty same dbają o swoje zasoby.

*Garbage collector* jest odpowiedzialny za wywoływanie finalizatora, ale nigdy nie wiemy, kiedy i w jakiej kolejności obiekty będą zwalniane.

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

> W bloku `Dispose(false)` (czyli w ścieżce wywołania z finalizatora) powinniśmy zwalniać **TYLKO** niezarządzane zasoby, to jest takie, których Garbage Collector nie zwalnia, i tylko te, których jesteśmy bezpośrednim właścicielem. Niezarządzanymi zasobami są na przykład pamięć zaalokowana z biblioteki C++ lub surowe (`IntPtr`) uchwyty do plików, gniazd sieciowych czy połączeń z bazą danych. Nie wolno nam tam odwoływać się do innych obiektów zarządzanych (np. wywoływać `sw.Dispose()`) – mogły one zostać już sfinalizowane.

> [!TIP]
> We współczesnym .NET ręczne pisanie finalizatora rzadko jest potrzebne. Zamiast przechowywać surowy uchwyt w polu typu `IntPtr`, lepiej opakować go w klasę dziedziczącą po [`SafeHandle`]({{< ref "/lectures/11.interoperability/pinvoke#safehandle" >}}), która ma własny finalizator. Klasa posiadająca `SafeHandle` potrzebuje wtedy jedynie prostej implementacji `Dispose` pokazanej wcześniej.
