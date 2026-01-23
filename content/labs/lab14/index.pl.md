---
title: "Lab14"
weight: 10
---

# Laboratorium 14: Współdziałanie, Marshalling, Kontekst Niebezpieczny

## Bezpieczne uchwyty

Celem zadania jest stworzenie programu odczytującego pliki z użyciem zewnętrznych funkcji pochodzących z bibliotek systemowych.

### Kod początkowy

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab14/student/SafeHandles" >}}

### Opis zadania

Współpraca z kodem o ręcznym zarządzaniu pamięci - jak np. napisanym w C++ lub C - wymaga ostrożności przy wymianie informacji. Zabezpieczenia które zapewnia nam platforma .NET tracą siłę w momencie przejścia do kodu zewnętrznego. Struktury tworzone w takim kodzie często przekazywane są w postaci wskaźników lub innych identyfikatorów które posiadają szczególne wartości sygnalizujące błąd (często 0 lub -1). Użyj klas dziedziczących po `SafeHandle`, aby upewnić się że zasoby zostały poprawnie przekazane przed ich użyciem.

W projekcie `FileInteraction` znajdziesz częściowo zaimplementowaną klasę ładującą funkcje systemowe obsługujące pliki - zależnie od systemu operacyjnego odpowiednio w pliku `WindowsFile.cs` oraz `UnixFile.cs`. Tylko ta, która odpowiada Twojemu systemowi będzie dołączona do skompilowanego programu. Zwróć uwagę na plik projektu `FileInteraction.csproj`, już za Ciebie użyte w nim zostało warunkowe wykluczenie niepasującego pliku źródłowego. 

Sprawdź jakie wartości zwracane przez funkcję otwierającą plik powinny być interpretowane jako niewłaściwe. Dokończ implementację części odpowiadającej systemowi którego używasz. Dodaj do klasy `MyFile` w pliku `WindowsFile.cs` lub `UnixFile.cs` dziedziczenie po odpowiedniej klasie z rodziny `SafeHandle` i zaimplementuj potrzebne funkcje. W pliku `MyFile.cs` uzupełnij funkcje `Open` oraz `Read`. Program możesz przetestować uruchamiając go.

Po zakończeniu zajęć zachęcamy do próby wykonania zadania na innym systemie operacyjnym.

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: MSBuild conditions](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-conditions)
- [Microsoft Learn: SafeHandles namespace](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.safehandles?view=net-9.0)
- [POSIX Manual: open](https://www.man7.org/linux/man-pages/man3/open.3p.html)
- [Microsoft Learn: CreateFile](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilea)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP]
> **Rozwiązanie**
> {{< filetree dir="labs/lab14/solution/SafeHandles" >}}

## Wiązania oraz Marshalling
Celem zadania jest użycie biblioteki niezarządzanej do stworzenia wzoru graficznego.

### Kod początkowy

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab14/student/Binding" >}}

### Opis zadania

W projekcie `PatternGeneration`, pliku `NativeMethods.cs` zaimportuj za pomocą `LibraryImport` funkcje, które udostępnia plik `pattern.h`. Zadeklaruj również wszystkie potrzebne dla nich struktury i klasy. Następnie zaimplementuj w klasie `Pattern` w `Pattern.cs` funkcje opakowujące, które pozwolą konsumentowi biblioteki wchodzić w interakcję jedynie z zarządzanym kodem.
- Niektóre argumenty wymagają dodania atrybutu `[In]`, aby sprecyzować kierunek wymiany danych. Analogicznie, jeśli jeden z argumentów podległby zmianie wewnątrz funkcji i potrzebne byłoby odzwierciedlenie tych zmian po stronie wywołującego, stosowny byłby atrybut `[Out]`.
- Struktury po stronie zarządzanej muszą mieć określone ułożenie pól ściśle zgodne ze strukturami, aby marshalling dokonywał się automatycznie. Alternatywą jest ręczny marshalling, do którego potrzebne jest zdefiniowanie reguł zmiany danych niezarządzanych w klasę lub strukturę zarządzaną z użyciem atrybutu [`CustomMarshaller`](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/tutorial-custom-marshaller).
- Potraktuj strukturę `pattern_t` jako nieprzejrzystą (ang. opaque). Nie martw się przenoszeniem danych. Wystarczy potraktować wskaźnik tej struktury jako jeden z `SafeHandle` do wywoływania innych funkcji, analogicznie do poprzedniego zadania.

Dystrybutor biblioteki może czasem nie udostępniać całego rozkładu struktury lub przenoszenie wszystich danych przez barierę biblioteka-aplikacja może nie być potrzebne. W funkcji `GetImage` w `Pattern.cs` użyj funkcji `Marshall.Copy`, aby wydobyć dane o wymiarach oraz zawartości tablicy `color_t[] values` ukrytej pod nieprzejrzystem uchwytem. Zwróć uwagę na to, że pola ułożone będą po kolei tak jak w `pattern.h` (a więc kolejno z przesunięciem 0, `sizeof(int)`, `2*sizeof(int)`), a struktura `color_t` odpowiada bezpośrednio strukturze `Rgb24`. Użyj pozyskanych danych do stworzenia obrazu z `Image.LoadPixelData<Rgb24>`.

Uzupełnij funkcję `Main` w projekcie `PatternGenerationDemo` aby tworzyło się co najmniej po jednym obrazie z użyciem fukcji `pattern_enstripen` oraz `pattern_populate`.

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation)
- [Microsoft Learn: Struct layout](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute?view=net-9.0)
- [Microsoft Learn: Type marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling)
- [Microsoft Learn: Built-in type marshalling](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.disableruntimemarshallingattribute?view=net-9.0)


{{% /hint %}}

### Przykładowy rezultat
`populate`
![populate](Data/Image1.png)
`enstripen`
![enstripen](Data/Image2.png)

### Przykładowe rozwiązanie

> [!TIP]
> **Rozwiązanie**
> {{< filetree dir="labs/lab14/solution/Binding" >}}