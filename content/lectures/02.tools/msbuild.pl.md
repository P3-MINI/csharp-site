---
title: "MSBuild"
---

# Microsoft Build Engine

## Wprowadzenie

MSBuild to narzędzie do budowania aplikacji. Jest częścią platformy .NET i służy do automatyzacji procesów kompilacji kodu, testowania, pakowania i publikowania aplikacji. Jest to silnik, który Visual Studio używa do budowania projektów, ale może być też uruchamiany niezależnie z lini poleceń, co jest szczególnie przydatne w przypadku zautomatyzowanych procesów kompilacji na serwerach (CI/CD).

## Pliki projektu MSBuild

MSBuild używa plików projektów opartych na XML. W tych plikach programista może zdefiniować w jaki sposób ma przebiegać proces budowania. Pliki te zazwyczaj mają rozszerzenie `.csproj`, `.vbproj` lub ogólnie `.proj`.

### Przykład pliku projektu

Rozważmy prostą aplikację konsolową w C++, `main.cpp`:

```cpp
#include <print>

int main() 
{
    std::println("Hello, world!");
    return 0;
}
```

Poniżej znajduje się plik projektu MSBuild, `HelloMSBuild.proj`, który kompiluje tę aplikację:

```xml
<Project DefaultTargets="Build">
  <PropertyGroup>
    <Compiler Condition="'$(Compiler)' == ''">clang++</Compiler>
    <CppVersion Condition="'$(CppVersion)' == ''">c++23</CppVersion>
    <OutputPath>$(SolutionDir)bin/</OutputPath>
    <OutputName>program</OutputName>
  </PropertyGroup>
  
  <ItemGroup>
    <CppSource Include="**/*.cpp" />
  </ItemGroup>
  
  <Target Name="Build" DependsOnTargets="Link">
    <Message Text="Building with $(Compiler)..." Importance="high" />
  </Target>
  
  <Target Name="Compile">
    <MakeDir Directories="$(OutputPath)" Condition="!Exists('$(OutputPath)')" />
    <Message Text="Compiling with $(Compiler)..." Importance="high" />
    <Exec Command="$(Compiler) -c -std=$(CppVersion) -o $(OutputPath)%(CppSource.Filename).o %(CppSource.Identity)" />
  </Target>
  
  <Target Name="Link" DependsOnTargets="Compile">
    <Message Text="Linking with $(Compiler)..." Importance="high" />
    <Exec Command="$(Compiler) @(CppSource->'$(OutputPath)%(filename).o', ' ') -o $(OutputPath)$(OutputName)" />
  </Target>
  
  <Target Name="Clean">
    <Message Text="Cleaning..." Importance="high" />
    <Delete Files="$(OutputPath)/$(OutputName)" />
    <Delete Files="@(CppSource->'$(OutputPath)%(filename).o')" />
    <RemoveDir Directories="$(OutputPath)" />
  </Target>
  
  <Target Name="Rebuild" DependsOnTargets="Clean;Build">
    <Message Text="Building with $(Compiler)..." Importance="high" />
  </Target>
</Project>
```

W tym przykładzie:
- `<PropertyGroup>` definiuje właściwości `Compiler`, `CppVersion`, `OutputPath` oraz `OutputName`.
- `<ItemGroup>` zawiera element `<CppSource>`, który wskazuje wszystkie pliki `.cpp` w projekcie do skompilowania.
- `<Target Name="Build">` definiuje główny target budowania. Zależy on od targetu `Link`, który z kolei zależy od targetu `Compile`. Najpierw target `Compile` po kolei kompiluje każdy z plików z `CppSource`, następnie w targecie `Link` wszystkie pliki obiektów są linkowane w program.
- `<Target Name="Clean">` usuwa skompilowane pliki.
- `<Target Name="Rebuild">` wykonuje najpierw `Clean`, a następnie `Build`.

### Podstawowe elementy pliku projektu

Plik projektu MSBuild składa się z czterech głównych części:

*   **Properties (Właściwości):** Definiowane wewnątrz elementu `<PropertyGroup>`. Właściwości to pary klucz-wartość, które służą do konfiguracji procesu budowania, np. ścieżki do plików, wersje bibliotek, flagi kompilatora. Można je traktować jak zmienne. Właściwości są przetwarzane w kolejności, w jakiej pojawiają się w pliku projektu, a ich wartości mogą być nadpisywane przez ponowne zdefiniowanie.

*   **Items (Elementy):** Definiowane wewnątrz elementu `<ItemGroup>`. Itemy to listy danych wejściowych dla procesu budowania, najczęściej są to pliki.

*   **Tasks (Zadania):** Taski to jednostki kodu wykonywalnego, które MSBuild używa do przeprowadzenia operacji budowania. Przykłady zadań to `Csc` (uruchomienie kompilatora C#), `Copy` (kopiowanie plików), `Message` (wyświetlanie komunikatu).

*   **Targets (Cele):** Definiowane za pomocą elementu `<Target>`. Targety grupują zadania w logiczne sekwencje. Polecenie `msbuild -targets` wyświetla listę wszystkich targetów dostępnych w projekcie. Warto wspomnieć, że targety posiadają atrybuty `Inputs` i `Outputs`. Służą one do implementacji tzw. buildów przyrostowych - MSBuild porównuje daty modyfikacji plików wejściowych i wyjściowych, aby zdecydować, czy ponowne wykonanie targetu jest konieczne.

### Odwoływanie się do właściwości i elementów

W plikach MSBuild, aby odwołać się do wartości zdefiniowanych właściwości i elementów, używa się specjalnej składni:

*   **`$()` do właściwości (Properties):** Aby uzyskać wartość właściwości, należy użyć jej nazwy wewnątrz nawiasów `$(NazwaWlasciwosci)`. Na przykład, `$(OutputName)` w powyższym przykładzie zostanie zastąpione przez `program`.

*   **`@()` do elementów (Items):** Aby uzyskać listę wartości z elementów, należy użyć nazwy grupy elementów wewnątrz nawiasów `@(NazwaGrupyElementow)`. Na przykład, `@(CppSource)` zostanie zastąpione listą wszystkich plików (np. `main.cpp;log.cpp`).

### Metadane i transformacje itemów

Każdy item w MSBuild, oprócz swojej wartości (np. ścieżki do pliku), może posiadać również **metadane**. Metadane to dodatkowe informacje powiązane z danym itemem, które można definiować i wykorzystywać w procesie budowania.

#### Składnia metadanych: `%()`

Aby odwołać się do metadanych itemu, używa się składni `%(NazwaMetadanej)`. Jeśli odwołujemy się do metadanych wewnątrz targetu, w którym przetwarzana jest lista itemów (tzw. "batching"), MSBuild iteruje po każdym itemie i udostępnia jego metadane.

**Przykład:**

Załóżmy, że mamy listę plików C++ i chcemy dla każdego z nich zdefiniować inny standard języka.

```xml
<ItemGroup>
  <CppSource Include="main.cpp">
    <LanguageStandard>c++20</LanguageStandard>
  </CppSource>
  <CppSource Include="log.cpp">
    <LanguageStandard>c++20</LanguageStandard>
  </CppSource>
  <CppSource Include="legacy.cpp">
    <LanguageStandard>c++11</LanguageStandard>
  </CppSource>
</ItemGroup>

<Target Name="Compile">
  <Message Text="Kompilowanie @(CppSource) przy użyciu standardu %(CppSource.LanguageStandard)..." />
</Target>
```

W tym przykładzie, `LanguageStandard` to niestandardowa metadana. Po uruchomieniu targetu `Compile`, MSBuild wyświetli:

```
Kompilowanie main.cpp;log.cpp przy użyciu standardu c++20...
Kompilowanie legacy.cpp przy użyciu standardu c++11...
```

#### Predefiniowane metadane (Predefined metadata)

Każdy item posiada zestaw predefiniowanych metadanych, niezależnie od tego, czy zostały zdefiniowane jawnie. Oto niektóre z nich:

*   **`%(Identity)`**: Wartość samego itemu (np. `main.cpp`).
*   **`%(Filename)`**: Nazwa pliku bez rozszerzenia (np. `main`).
*   **`%(Extension)`**: Rozszerzenie pliku (np. `.cpp`).
*   **`%(FullPath)`**: Pełna, absolutna ścieżka do pliku.
*   **`%(RelativeDir)`**: Ścieżka względna do katalogu, w którym znajduje się plik.

Pełną listę można znaleźć w [dokumentacji](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata).

#### Transformacje itemów (Item transformations)

Transformacje pozwalają na konwersję jednej listy itemów na inną, z użyciem metadanych. Składnia transformacji to `'@(NazwaGrupy -> '%(Metadana)')'`. Opcjonalnie możemy jeszcze podać alternatywny znak separatora (domyślnie jest to ';'): `'@(NazwaGrupy -> '%(Metadana)', '_')'`.

**Przykład:**

Załóżmy, że chcemy przekształcić listę plików źródłowych `CppSource` na listę plików obiektowych `.o`.

```xml
<ItemGroup>
  <CppSource Include="main.cpp;utils.cpp" />
</ItemGroup>

<Target Name="ListObjectFiles">
  <Message Text="Pliki obiektowe: @(CppSource -> '%(Filename).o')" />
</Target>
```

W tym przypadku:

1.  `@(CppSource -> '%(Filename).o')` bierze każdy item z `CppSource`.
2.  Dla każdego itemu pobiera metadaną `%(Filename)` (np. `main`, `utils`).
3.  Dołącza do niej `.o`, tworząc nową listę: `main.o;utils.o`.

Target `ListObjectFiles` wyświetli: `Pliki obiektowe: main.o;utils.o`.

### Podstawowe polecenia

*   **Budowanie projektu:**
    ```bash
    msbuild <nazwa_pliku_projektu>
    dotnet build <nazwa_pliku_projektu>
    ```
    Jeśli w katalogu znajduje się tylko jeden plik projektu, można pominąć jego nazwę.

*   **Wybór konkretnego targetu:**
    ```bash
    msbuild <nazwa_pliku_projektu> /t:<nazwa_targetu>
    ```
    Polecenie `dotnet build` nie ma bezpośredniego przełącznika do uruchamiania niestandardowych targetów. Jednakże, można w tym celu użyć polecenia `dotnet msbuild`, które jest częścią .NET SDK i działa analogicznie do `msbuild`.
    ```bash
    msbuild <nazwa_pliku_projektu> /t:<nazwa_targetu>
    dotnet msbuild <nazwa_pliku_projektu> /t:<nazwa_targetu>
    ```
    Dla standardowych operacji, takich jak `clean` czy `publish`, zaleca się używanie dedykowanych poleceń `dotnet`:
    ```bash
    dotnet clean
    dotnet publish
    ```

*   **Przekazywanie właściwości:**
    Właściwości można przekazywać do `msbuild` i `dotnet build` za pomocą przełącznika `/p` (lub `-p` i `--property` dla `dotnet`).
    ```bash
    msbuild /p:Configuration=Release
    dotnet build -p:Configuration=Release
    dotnet build --property:Configuration=Release
    ```
    Wiele popularnych właściwości, takich jak `Configuration`, ma swoje krótsze odpowiedniki w `dotnet`:
    ```bash
    dotnet build -c Release
    ```
    Wszystkie powyższe polecenia zbudują projekt w konfiguracji `Release`.

### Kolejność wykonywania targetów

MSBuild określa kolejność wykonywania targetów na podstawie zdefiniowanych zasad.

Kolejność jest następująca:

1.  **Atrybut `InitialTargets`:** Targety zdefiniowane w tym atrybucie elementu `<Project>` są uruchamiane jako pierwsze, nawet jeśli inne targety zostały podane w linii poleceń lub w atrybucie `DefaultTargets`.

2.  **Targety z linii poleceń:** Jeśli uruchamiasz MSBuild z przełącznikiem `/t` (lub `dotnet msbuild /t`), podane targety zostaną wykonane po tych z `InitialTargets`.

3.  **Atrybut `DefaultTargets`:** Jeśli w linii poleceń nie podano żadnych targetów, MSBuild uruchomi targety zdefiniowane w tym atrybucie elementu `<Project>`.

4.  **Pierwszy target w pliku:** Jeśli nie zdefiniowano `InitialTargets`, `DefaultTargets` i nie podano targetów w linii poleceń, MSBuild wykona pierwszy napotkany target w pliku projektu.

Po ustaleniu targetów początkowych, MSBuild używa następujących atrybutów do rekursywnego budowania drzewa zależności i określenia ostatecznej kolejności:

*   **`DependsOnTargets`:** Atrybut ten określa, że dany target zależy od innych. MSBuild wykona wszystkie targety z listy `DependsOnTargets` **przed** wykonaniem targetu, który je deklaruje.

*   **`BeforeTargets` i `AfterTargets`:** Te atrybuty pozwalają odpalić target przed lub po innym, bez modyfikowania go.

Warto pamiętać, że **każdy target jest wykonywany tylko raz** w trakcie jednego budowania. Nawet jeśli wiele targetów deklaruje zależność od tego samego targetu, zostanie on uruchomiony tylko przy pierwszym wywołaniu.

Dodatkowo, atrybut **`Condition`** na targecie może spowodować jego pominięcie, jeśli warunek nie zostanie spełniony.

### Atrybut `Condition`

Atrybut `Condition` pozwala na warunkowe wykonywanie tasków/targetów lub warunkową definicję właściwości/itemów. **Można go dołączyć do niemal każdego węzła**, w tym:

*   `<PropertyGroup>` i `<Property>`
*   `<ItemGroup>` i poszczególnych `<Item>`
*   `<Target>`
*   `<Task>`
*   `<Import>`

Na przykład:

```xml
<PropertyGroup>
  <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
</PropertyGroup>
```

W tym przypadku, jeśli właściwość `Configuration` nie zostanie przekazana z zewnątrz (np. z linii poleceń), zostanie jej przypisana wartość `Debug`.

### Predefiniowane zadania (Tasks)

MSBuild dostarcza mnóstwo wbudowanych zadań, które można używać w swoich targetach. Kilka przykładowych:

*   **`Message`**: Wyświetla komunikat w logach budowania.
*   **`Copy`**: Kopiuje pliki z jednego miejsca do drugiego.
*   **`Delete`**: Usuwa pliki.
*   **`MakeDir`**: Tworzy katalogi.
*   **`Exec`**: Uruchamia zewnętrzne polecenie lub skrypt.
*   **`Csc`**: Uruchamia kompilator C#.
*   **`MSBuild`**: Uruchamia inne projekty MSBuild, co pozwala na budowanie zależności.

Pełną listę wbudowanych zadań wraz z dokumentacją można znaleźć tutaj: [MSBuild tasks](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-task-reference).

Oprócz wbudowanych zadań, można również **tworzyć własne, niestandardowe zadania (custom tasks)**. Pozwala to na rozszerzenie MSBuild o dowolną logikę, która jest potrzebna w procesie budowania. Jak zdefiniować własne zadania można doczytać w [dokumentacji](https://learn.microsoft.com/en-us/visualstudio/msbuild/task-writing).

### Importowanie innych plików

MSBuild pozwala na dzielenie logiki budowania na wiele plików za pomocą elementu `<Import>`. Jest to kluczowe dla utrzymania porządku w dużych projektach i jest podstawą działania projektów w stylu SDK.

```xml
<Project ...>
  ...
  <Import Project="Common.targets" />
</Project>
```

### Projekty w stylu SDK (SDK-style projects)

Nowoczesne projekty .NET (od .NET Core) używają uproszczonego formatu, znanego jako projekty w stylu SDK. Atrybut `Sdk` w elemencie `<Project>` automatycznie importuje odpowiednie pliki `.props` i `.targets`, które zawierają całą logikę budowania.

W praktyce, atrybut `Sdk` jest "skrótem składniowym" dla dwóch importów. Zapis:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  ...
</Project>
```

Jest logicznie równoważny z ręcznym importowaniem plików `.props` i `.targets` z SDK:

```xml
<Project>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />

  ...

  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
```

Pierwszy import (`Sdk.props`) znajduje się na początku pliku i ładuje domyślne właściwości, a drugi (`Sdk.targets`) na końcu, aby załadować targety i logikę budowania.

Poniższy przykład pokazuje typowy plik projektu, stworzony poleceniem `dotnet new console -o ConsoleProject`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

Ten plik, mimo że jest znacznie krótszy, zawiera całą potrzebną logikę do zbudowania prostej aplikacji konsolowej - importuje ją z `Microsoft.NET.Sdk`. Te importowane pliki można znaleźć w katalogu instalacyjnym .NET SDK. W systemie Windows jest to zazwyczaj `C:\Program Files\dotnet\sdk\[wersja]\Sdks\`, a na Linuksie `/usr/share/dotnet/sdk/[wersja]/Sdks/`.

### Predefiniowane itemy z SDK

Projekty w stylu SDK automatycznie definiują wiele elementów, które ułatwiają pracę. Oto niektóre z nich:

*   **`Compile`**: Pliki z kodem źródłowym do skompilowania (domyślnie wszystkie pliki `.cs` w projekcie).
*   **`EmbeddedResource`**: Pliki, które mają zostać osadzone w wynikowym assembly.
*   **`Content`**: Pliki, które nie są kompilowane, ale mają zostać skopiowane do katalogu wyjściowego (np. pliki konfiguracyjne, zasoby).
*   **`None`**: Pliki, które są częścią projektu, ale nie biorą udziału w procesie budowania (np. `README.md`).
*   **`ProjectReference`**: Odwołania do innych projektów.
*   **`PackageReference`**: Odwołania do pakietów NuGet.

Pełną listę popularnych elementów można znaleźć tutaj: [Common MSBuild project items](https://learn.microsoft.com/visualstudio/msbuild/common-msbuild-project-items).

### Logowanie i diagnozowanie problemów

MSBuild oferuje zaawansowane opcje logowania, które są nieocenione przy diagnozowaniu problemów z budowaniem.

*   **Szczegółowość logów:**
    ```bash
    msbuild /v:detailed
    dotnet build --verbosity detailed
    ```
    Możliwe wartości to `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]` i `diag[nostic]`.

*   **Logowanie do pliku:**
    ```bash
    msbuild /flp:LogFile=build.log;Verbosity=diagnostic
    dotnet build /flp:LogFile=build.log;Verbosity=diagnostic
    ```
    To polecenie zapisze logi do pliku `build.log`.
