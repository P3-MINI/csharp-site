---
title: "Podstawy C#"
weight: 30
---

## Tutorial 3: Podstawy C#

Zakres tego laboratorium obejmuje podstawy języka C# (składnia, system typów, typy podstawowe, tablice, parametry) oraz tworzenie własnych typów (klasy, struktury, interfejsy, dziedziczenie, typy wyliczeniowe). 

Jako że znasz już język C++, wiele konstrukcji w C# wyda Ci się znajomych. Przed rozpoczęciem zadania, omówimy najważniejsze różnice i zagadnienia.

### Przestrzenie nazw

Podobnie jak w C++, w C# używasz przestrzeni nazw, aby grupować powiązany kod i unikać konfliktów nazw. W C# kropki (`.`) używasz do odwoływania się do przestrzeni nazw oraz składowych klas i struktur (np. `System.Console.WriteLine`).

Dyrektywa `using` pozwala importować przestrzeń nazw do obecnego pliku, co pozwala na bezpośrednie korzystanie z jej typów bez podawania pełnej ścieżki (np. `using System;`). W nowszych wersjach C# powszechnie stosuje się przestrzenie nazw o zasięgu pliku (file-scoped namespace), co pozwala uniknąć dodatkowego poziomu wcięć:

```csharp
namespace MyApp.Core;

// All code in this file belongs to MyApp.Core
public class Calculator 
{
    // ...
}
```

### Klasy i Struktury

W C++ różnica między `class` a `struct` sprowadza się jedynie do domyślnej widoczności składowych. W C# ta różnica dotyczy sposobu zarządzania pamięcią.

- **Klasy (`class`) to typy referencyjne.** Są alokowane na stercie, a ich cyklem życia zarządza Garbage Collector. Zmienna przechowująca klasę przechowuje referencję do obiektu. Klasy są przeznaczone dla złożonych obiektów, które posiadają swoją tożsamość, wymagają dłuższego cyklu życia lub korzystają z dziedziczenia.
- **Struktury (`struct`) to typy bezpośrednie.** Są alokowane na stosie lub osadzone bezpośrednio wewnątrz innych obiektów. Przypisanie struktury do innej zmiennej lub przekazanie jej jako argument do metody domyślnie tworzy jej pełną kopię. Struktury stosuje się do małych typów danych (zazwyczaj do ok. 16 bajtów).

> [!NOTE]
> W naszym projekcie śledzenia promieni:
> - **Strukturami (`struct`)** będą małe typy matematyczne podlegające ciągłemu przetwarzaniu: `Vector3`, `Ray` oraz `HitInfo`. Dzięki alokacji na stosie unikamy obciążania Garbage Collectora.
> - **Klasami (`class`)** będą elementy posiadające tożsamość, stan złożony lub korzystające z polimorfizmu: `Image`, `Camera`, kształty geometryczne (`Sphere`, `Plane`) oraz materiały (`Material`).

### Właściwości (Properties)

Enkapsulacja jest jednym z założeń programowania obiektowego. Aby ukryć pola klasy, często tworzy się metody dostępowe, tzw. gettery i settery (np. `float GetX()`, `void SetX(float v)`). C# wspiera koncepcję enkapsulacji poprzez **Właściwości** (Properties). Z zewnątrz używasz ich jak zwykłych zmiennych, ale pod spodem kompilator sam generuje ukryte metody odczytu i zapisu:

```csharp
public struct Point3
{
    // Private, backing field
    private float x;

    // Classic property
    public float X 
    {
        get { return x; }
        set { x = value; } // 'value' is the implicit setter parameter
    }

    // Auto-property - compiler generates the backing field
    public float Y { get; set; }
    
    // Read-only property from the outside
    public float Z { get; private set; }
}
```

Odwoływanie się do zdefiniowanych właściwości wygląda następująco:

```csharp
Point3 p = new Point3();
p.X = 5.0f;       // Uses setter
float val = p.X;  // Uses getter

// p.Z = 2.0f;    // Error: setter is private
```

### Tablice

W C# tablice są typami referencyjnymi i zawsze znają swój rozmiar (`Length`). C# kontroluje granice tablicy – wyjście poza jej zakres kończy się rzuceniem wyjątku `IndexOutOfRangeException`.

```csharp
// Create an array of 100 elements (initialized to default values, 0 for int)
int[] numbers = new int[100];
int length = numbers.Length; // Array knows its size
```

Oprócz tablic jednowymiarowych (oraz tablic tablic, czyli tzw. tablic poszarpanych/jagged arrays), C# obsługuje też wbudowane, spójne pamięciowo tablice wielowymiarowe:

```csharp
// 2D array [width, height]
float[,] grid = new float[800, 600];
grid[0, 0] = 1.5f; // Access specific element
```

### Typ `object` i metoda `ToString()`

W systemie typów C# **każdy typ** (zarówno referencyjny, jak i bezpośredni) dziedziczy po klasie `System.Object` (w skrócie `object`), co oznacza, że każda instancja posiada pewien standardowy zestaw metod.

Często używaną metodą jest wirtualna metoda `ToString()`, która zwraca tekstową reprezentację obiektu. Domyślnie zwraca nazwę typu, ale zazwyczaj chcesz ją **nadpisać**. Używasz do tego słowa kluczowego `override`:

```csharp
public struct Point3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    // Override the built-in ToString() method from object
    public override string ToString()
    {
        // String interpolation
        return $"[{X}, {Y}, {Z}]";
    }
}
```

Przykładowe użycie tej metody w praktyce:

```csharp
using System;

// Object initialization syntax
Point3 p = new Point3 { X = 1.0f, Y = 2.0f, Z = 3.0f };

// Automatically calls ToString() under the hood
Console.WriteLine(p); // Output: [1, 2, 3]
```

## Zadanie 1 - Śledzenie promieni

W ramach tego zadania stworzysz prostą aplikację do śledzenia promieni. Zadanie to opiera się na materiałach z książki [Raytracing in One Weekend](https://raytracing.github.io/books/RayTracingInOneWeekend.html). Będzie się ona składała z pojedynczego projektu - aplikacji wykonywalnej.

Śledzenie promieni (ang. *ray tracing*) to technika generowania obrazów. W metodzie tej rzucamy promienie od obserwatora (kamery) przez każdy piksel rzutni w głąb sceny. Następnie algorytm sprawdza, z jakimi obiektami przecina się dany promień i na tej podstawie oblicza kolor piksela.

### Etap 1: Reprezentacja kolorów i generowanie obrazu

Naszą pracę zaczniemy od zdefiniowania dwóch typów: struktury opisującej wektor (użyjemy go również do reprezentacji punktów i koloru) oraz klasy reprezentującej obraz.

#### Struktura Vector3

Użyjemy struktury agregującej trzy liczby, której poszczególne składniki (X, Y, Z) mogą odpowiadać kanałom RGB (Red, Green, Blue) lub współrzędnym w przestrzeni. Utwórz plik `Vector3.cs` i zdefiniuj:

1. Publiczne właściwości `X`, `Y`, `Z` typu `float`.
2. Publiczny konstruktor przyjmujący trzy wartości, przypisujący je do właściwości.
3. Nadpisaną metodę `ToString()`, zwracającą składniki wektora oddzielone spacją (np. `"1.5 0.2 0.8"`).

#### Klasa Image i format PPM

Obraz traktujemy jako dwuwymiarową tablicę pikseli. Utwórz klasę `Image` (plik `Image.cs`). Klasa powinna zawierać:
- Publiczne właściwości tylko do odczytu `Width` i `Height` typu `int`.
- Prywatną tablicę dwuwymiarową `Vector3[,] Pixels`.
- Konstruktor przyjmujący szerokość i wysokość, który na ich podstawie zainicjalizuje dwuwymiarową tablicę.
- Dwuwymiarowy indeksator (`public Vector3 this[int x, int y]`), stanowiący odpowiednik przeciążonego `operatora[]` z C++. W akcesorze `get` powinien zwrócić piksel z tablicy `Pixels[x, y]`, a w akcesorze `set` przypisać do niego przekazaną wartość (`value`). Posłuży on do wygodnego odczytywania i modyfikowania pikseli (np. `image[x, y] = color`).

Aby wyeksportować wygenerowany obraz, wykorzystamy prosty, tekstowy format **PPM**. Nie będziemy implementować osobnej metody zapisującej obraz do pliku. Zamiast tego zaimplementujemy proces generowania zawartości PPM wewnątrz nadpisanej metody `ToString()`. Wywołanie `Console.WriteLine(image)` wypisze obraz w formie tekstowej na standardowe wyjście, które przekierujemy do pliku.

Struktura formatu PPM wygląda następująco:
1. Magiczny ciąg znaków `P3` (zbieżność z nazwą przedmiotu Programowanie 3 przypadkowa).
2. Szerokość i wysokość obrazka (oddzielone spacją).
3. Maksymalna wartość dla koloru (w naszym przypadku `255`).
4. Całkowitoliczbowe wartości R, G, B pikseli (od lewego górnego rogu). Ich ułożenie w wierszach jest dowolne, ważne jedynie, aby wartości były oddzielone białymi znakami.

Poniżej znajduje się przykładowy plik dla obrazka o wymiarach 3x2 (trzy kolumny, dwa wiersze):

```text
P3
3 2
255
255   0   0     0 255   0     0   0 255
255 255   0   255 255 255     0   0   0
```

Twoim zadaniem jest zaimplementowanie metody `ToString()`. Dodaj do tworzonego tekstu odpowiedni nagłówek, a następnie – za pomocą zagnieżdżonych pętli – przeiteruj po wszystkich pikselach na obrazie. Składowe każdego wektora (`float`) zmapuj na format RGB (zakres `[0, 255]`). Upewnij się, że wartości nie wykraczają poza ten przedział, np. używając `Math.Clamp`.

Ponieważ będziemy generować ciąg tekstowy dla milionów wartości, użycie konkatenacji stringów (operator `+`) w pętli byłoby bardzo niewydajne z uwagi na ciągłą alokację nowych obiektów. Do optymalnego zbudowania wyniku wykorzystaj klasę `StringBuilder` (zdefiniowaną w przestrzeni nazw `System.Text`).

#### Generowanie pierwszego obrazka

W metodzie `Main` pliku `Program.cs`:
1. Utwórz instancję obrazu `Image` o rozdzielczości np. 1920x1080.
2. W zagnieżdżonej pętli wypełnij piksele dowolnym wzorem (np. gradientem, gdzie składowa czerwona {{< katex >}}R = i / \text{Width}{{< /katex >}}, zielona {{< katex >}}G = j / \text{Height}{{< /katex >}}, a niebieska to stała lub funkcja sinus).
3. Wypisz obiekt obrazu do konsoli za pomocą `Console.WriteLine(image)`.

Aby zapisać obrazek, przekieruj standardowe wyjście do pliku. Uruchom program poleceniem:

```bash
dotnet run > image.ppm
```

### Etap 2: Kamera i Promienie

W tym etapie rozbudujemy strukturę `Vector3` oraz stworzymy mechanizm wirtualnej kamery i strukturę promienia (`Ray`).

#### Rozbudowa wektora i przeciążanie operatorów

Zanim stworzymy kamerę, nasza struktura `Vector3` wymaga matematycznej rozbudowy. W C# operatory matematyczne można przeciążać, definiując statyczne metody z użyciem słowa kluczowego `operator`. 

Poniżej znajduje się przykład przeciążenia operatora dodawania dwóch wektorów z wykorzystaniem notacji strzałkowej:

```csharp
public static Vector3 operator +(Vector3 u, Vector3 v) => new Vector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
```

W pliku `Vector3.cs` zaimplementuj:
1. Przeciążenia operatorów: unarnego `-` (odwrócenie wektora), dodawania `+` i odejmowania `-` dwóch wektorów, mnożenia `*` i dzielenia `/` wektora przez skalar (`float`), a także mnożenia dwóch wektorów przez siebie `*` (mnożącego odpowiadające sobie składowe: {{< katex >}}u.X \cdot v.X, u.Y \cdot v.Y, u.Z \cdot v.Z{{< /katex >}}).
2. Metody `LengthSquared()` (zwracającą kwadrat długości wektora) oraz `Length()` (obliczającą długość wektora). Do wyciągnięcia pierwiastka kwadratowego użyj statycznej klasy `MathF`, która zawiera metody (np. `MathF.Sqrt()`) zoptymalizowane dla liczb typu `float`.
3. Statyczną metodę `Dot()` do obliczania iloczynu skalarnego.
4. Statyczną metodę `Cross()` do obliczania iloczynu wektorowego.
5. Metodę `Normalize()`, która znormalizuje wektor.

#### Struktura Promienia (Ray)

Każdy wypuszczony z kamery promień to w przestrzeni półprosta, którą można opisać matematyczną funkcją {{< katex >}}P(t) = A + t \cdot b{{< /katex >}}, gdzie {{< katex >}}A{{< /katex >}} to początek promienia (Origin), {{< katex >}}b{{< /katex >}} to jego kierunek (Direction), a parametr {{< katex >}}t{{< /katex >}} oznacza dystans przebyty wzdłuż tego kierunku.

Utwórz nową strukturę `Ray` (plik `Ray.cs`) i zdefiniuj w niej:
- Właściwości tylko do odczytu: `Vector3 Origin` oraz `Vector3 Direction`.
- Konstruktor przyjmujący i przypisujący ich wartości początkowe.
- Metodę `Vector3 At(float t)`, wyznaczającą wartość funkcji {{< katex >}}P(t){{< /katex >}} (pozycję punktu na promieniu w przestrzeni) dla danego parametru {{< katex >}}t{{< /katex >}}.

#### Klasa Kamery

Będziemy posługiwali się modelem kamery, w którym wypuszczamy promienie z wirtualnego punktu (środka projekcji/pozycji obserwatora) prosto w trójwymiarową przestrzeń, przepuszczając je przez piksele wirtualnego ekranu (rzutni) zawieszonego przed kamerą.

![Model Kamery](https://raytracing.github.io/images/fig-1.03-cam-geom.jpg)
*(Źródło: Ray Tracing in One Weekend)*

Każda zmiana parametru fizycznego (pozycja kamery, pozycja rzutni, czy kąt widzenia FoV) wymaga przeliczenia od nowa parametrów siatki rzutni. Z tego powodu, właściwości konfigurujące kamerę oprzemy o prywatne pola klasy (tzw. *backing field*), co pozwoli nam na umieszczenie w bloku `set` wywołania funkcji aktualizującej stan kamery po każdej zmianie.

```csharp
public class Camera
{
    private float _fov = 90.0f; // Private field with default value

    public float Fov 
    { 
        get => _fov; 
        set 
        { 
            _fov = value; 
            Recalculate(); // Recalculate layout after parameter change
        } 
    }
    // ...
}
```

Utwórz plik `Camera.cs` definiujący kamerę:
1. Zdefiniuj właściwości i powiąż je z prywatnymi polami o przypisanych wartościach domyślnych: `float AspectRatio` (`16.0f / 9.0f`), `int ImageWidth` (`1280`), `Vector3 Position` (`new Vector3(0,0,0)`), `Vector3 Target` (`new Vector3(0,0,-1)`) oraz `float Fov` (`90.0f`). Właściwość `Target` określa punkt w przestrzeni, w którym znajduje się środek rzutni kamery. Każdy akcesor `set` w wymienionych właściwościach musi wywoływać metodę `Recalculate()`.
2. Zadeklaruj prywatne pola przechowujące wyliczone parametry rzutni oraz kamery: `int _height`, `Vector3 _pixelDu`, `Vector3 _pixelDv` oraz `Vector3 _viewportCorner`.
3. Dodaj bezparametrowy konstruktor, który jednorazowo wywoła metodę `Recalculate()` do inicjalizacji obiektu.

Zaimplementuj prywatną metodę `Recalculate()`, wyznaczającą geometrię rzutni:
1. **Wymiary obrazu:** Oblicz wysokość `_height = (int)(ImageWidth / AspectRatio)` (zabezpiecz przed wartością mniejszą niż 1).
2. **Fizyczny rozmiar rzutni:** Przelicz kąt `Fov` na radiany ({{< katex >}}\theta = \text{Fov} \cdot \pi / 180{{< /katex >}}) i wyznacz {{< katex >}}h = \tan(\theta / 2){{< /katex >}}. Odległość od obserwatora do rzutni to długość wektora {{< katex >}}(\text{Position} - \text{Target}){{< /katex >}}. Wysokość rzutni wynosi {{< katex >}}2 \cdot h \cdot \text{odległość}{{< /katex >}}, a szerokość to {{< katex >}}\text{wysokość} \cdot (\text{ImageWidth} / \text{\_height}){{< /katex >}}.
3. **Baza wektorów kamery (`front`, `right`, `up`):** 
   - Wektor kierunku patrzenia: {{< katex >}}w = \text{Normalize}(\text{Position} - \text{Target}){{< /katex >}} (zwrócony przeciwnie do kierunku patrzenia zgodnie z układem prawoskrętnym).
   - Wektor w prawo (`right`): {{< katex >}}u = \text{Normalize}(\text{Cross}(\text{Vector3}(0, 1, 0), w)){{< /katex >}}.
   - Wektor w górę (`up`): {{< katex >}}v = \text{Cross}(w, u){{< /katex >}}.
4. **Wektory kroków piksela:**
   - Rozpiętość pozioma rzutni wynosi {{< katex >}}\text{viewportU} = \text{szerokość} \cdot u{{< /katex >}}.
   - Rozpiętość pionowa wynosi {{< katex >}}\text{viewportV} = \text{wysokość} \cdot (-v){{< /katex >}} (skierowana w dół, ponieważ wiersze obrazu liczymy od góry do dołu).
   - Wektory kroku pojedynczego piksela: {{< katex >}}\text{\_pixelDu} = \text{viewportU} / \text{ImageWidth}{{< /katex >}} oraz {{< katex >}}\text{\_pixelDv} = \text{viewportV} / \text{\_height}{{< /katex >}}.
5. **Środek pierwszego piksela (`_viewportCorner`):** Wyznacz lewy górny róg rzutni: {{< katex >}}\text{Position} - (\text{odległość} \cdot w) - \text{viewportU} / 2 - \text{viewportV} / 2{{< /katex >}}, a następnie przesuń go na środek pierwszego piksela dodając {{< katex >}}0.5 \cdot (\text{\_pixelDu} + \text{\_pixelDv}){{< /katex >}}.

### Etap 3: Przecinanie promieni z obiektami

W tym etapie zaimplementujemy wykrywanie przecięć promieni z geometrią na scenie.

#### Informacje o przecięciu (HitInfo)

Gdy promień przecina obiekt, musimy zebrać kilka informacji: pozycję uderzenia, wektor normalny powierzchni w miejscu trafienia oraz wartość parametru `T` promienia, dla której nastąpiło trafienie. Informacje te będą nam przydatne później przy obliczaniu koloru promienia.

Utwórz plik `HitInfo.cs` ze strukturą przechowującą wynik uderzenia:
1. Zdefiniuj pola publiczne: `Vector3 Position`, `Vector3 Normal`, `float T` oraz `bool FrontFace`.
2. Zdefiniuj metodę `SetFaceNormal(Ray r, Vector3 outwardNormal)`. Do poprawnego oświetlenia wektor normalny musi być skierowany przeciwnie do promienia:
   - Sprawdź iloczyn skalarny kierunku promienia i wektora `outwardNormal`.
   - Jeżeli wynik jest ujemny, promień uderza z zewnątrz (`FrontFace = true`), a `Normal` przyjmuje wartość `outwardNormal`.
   - W przeciwnym razie promień trafia od wewnątrz (`FrontFace = false`), a `Normal` należy odwrócić (`-outwardNormal`).

#### Interfejs IHittable i modyfikator "out"

Interfejsy to abstrakcyjne typy definiujące kontrakt, czyli zbiór metod i właściwości, które klasa musi zaimplementować. Nie posiadają one własnego stanu (pól) ani implementacji. W naszym programie każdy obiekt, w który może uderzyć promień, będzie implementował wspólny interfejs, co pozwoli na polimorficzną obsługę różnych kształtów na scenie.

Jednym ze sposobów na zwrócenie wielu wartości z metody w C# jest użycie słowa kluczowego `out`. Wymusza ono zainicjowanie przekazanej w ten sposób zmiennej przed opuszczeniem metody (np. metoda sprawdzająca trafienie promienia może zwrócić jako wynik `bool`, a przez argument zwrócić wygenerowaną strukturę `HitInfo`).

Utwórz plik `IHittable.cs` z definicją interfejsu (w C# nazwy interfejsów zwyczajowo zaczynamy od dużej litery `I`), deklarującego metodę:

```csharp
bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo);
```

> [!NOTE]
> W C# wszystkie składowe zadeklarowane wewnątrz interfejsu są domyślnie publiczne (`public`), dlatego nie wymagają jawnego modyfikatora dostępu w deklaracji.

#### Implementacja IHittable (Sphere i Plane)

Każdy obiekt na naszej scenie (włącznie z samą sceną) będzie implementował interfejs `IHittable`. Kontrakt ten oznacza wprost, że w dany obiekt można "strzelać" promieniami. Dzięki temu wszystkie elementy w świecie będą traktowane polimorficznie – kamera nie musi wiedzieć, czy strzela w pojedynczą kulę, całą scenę, czy trójkąt, o ile dany obiekt implementuje interfejs `IHittable`.

Utwórz definicję klasy sfery (plik `Sphere.cs`):
- Utwórz klasę `Sphere` implementującą interfejs `IHittable` (`class Sphere : IHittable`) i definiującą właściwości `Vector3 Center` oraz `float Radius`.
- Zdefiniuj konstruktor inicjalizujący podane wartości.
- Zaimplementuj metodę `Hit`:
  1. Wyznacz wektor od środka kuli do początku promienia: {{< katex >}}oc = \text{ray.Origin} - \text{Center}{{< /katex >}}.
  2. Wyznacz współczynniki równania kwadratowego:
     - {{< katex >}}a = \|\text{ray.Direction}\|^2{{< /katex >}}
     - {{< katex >}}halfB = oc \cdot \text{ray.Direction}{{< /katex >}}
     - {{< katex >}}c = \|oc\|^2 - \text{Radius}^2{{< /katex >}}
  3. Oblicz wyróżnik równania: {{< katex >}}\Delta = halfB^2 - a \cdot c{{< /katex >}}. Jeśli {{< katex >}}\Delta < 0{{< /katex >}}, brak przecięcia – zwróć `false`.
  4. Znajdź najmniejszy pierwiastek w przedziale {{< katex >}}[tMin, tMax]{{< /katex >}}. Sprawdź najpierw {{< katex >}}(-halfB - \sqrt{\Delta}) / a{{< /katex >}}. Jeśli nie mieści się w zakresie, sprawdź {{< katex >}}(-halfB + \sqrt{\Delta}) / a{{< /katex >}}. Jeśli żaden nie pasuje, zwróć `false`.
  5. Wypełnij `hitInfo`: parametr `T = root`, punkt uderzenia `Position = ray.At(root)`, wyznacz wektor normalny {{< katex >}}(\text{Position} - \text{Center}) / \text{Radius}{{< /katex >}} i przekaż do `SetFaceNormal(ray, outwardNormal)`. Zwróć `true`.
  ([Więcej szczegółów matematycznych znajdziesz w książce](https://raytracing.github.io/books/RayTracingInOneWeekend.html#addingasphere/ray-sphereintersection)).

Utwórz klasę `Plane` (plik `Plane.cs`) implementującą interfejs `IHittable`, reprezentującą płaszczyznę.
- Dodaj właściwości `Vector3 Point` oraz `Vector3 Normal`. Matematycznie płaszczyzna opisana jest za pomocą dowolnego leżącego na niej punktu (właściwość `Point`) oraz wektora do niej prostopadłego (właściwość `Normal`). W konstruktorze zadbaj, by przypisywany wektor normalny został znormalizowany.
- Zaimplementuj metodę `Hit`. Punkt {{< katex >}}P{{< /katex >}} leży na płaszczyźnie, gdy spełnia równanie {{< katex >}}(P - \text{Point}) \cdot \text{Normal} = 0{{< /katex >}}. Podstawiając równanie promienia {{< katex >}}P(t) = \text{Origin} + t \cdot \text{Direction}{{< /katex >}} i wyznaczając parametr {{< katex >}}t{{< /katex >}}, otrzymujemy:
  {{< katex >}}t = \frac{(\text{Point} - \text{Origin}) \cdot \text{Normal}}{\text{Direction} \cdot \text{Normal}}{{< /katex >}}
  
  W metodzie `Hit`:
  1. Oblicz mianownik powyższego ułamka: {{< katex >}}denom = \text{Normal} \cdot \text{ray.Direction}{{< /katex >}}.
  2. Jeśli wartość bezwzględna mianownika jest bliska zeru ({{< katex >}}|denom| < 10^{-6}{{< /katex >}}), promień jest równoległy do płaszczyzny (brak punktu przecięcia) – zwróć `false`.
  3. W przeciwnym razie oblicz wartość parametru {{< katex >}}t{{< /katex >}} ze wzoru.
  4. Jeśli parametr {{< katex >}}t{{< /katex >}} mieści się w przedziale {{< katex >}}(tMin, tMax){{< /katex >}}, uzupełnij strukturę `hitInfo` (parametr `T = t`, punkt przecięcia `Position = ray.At(t)`, wektor normalny przez `SetFaceNormal(ray, Normal)`) i zwróć `true`. W przeciwnym razie zwróć `false`.

#### Scena

Nasza scena będzie się składać ze zbioru obiektów. Do przechowywania elementów o dynamicznym rozmiarze użyjemy generycznej klasy `List<T>` (z przestrzeni nazw `System.Collections.Generic`).

Utwórz klasę `Scene`, implementującą interfejs `IHittable`. Zdefiniuj w niej:
- Publiczną listę: `public List<IHittable> Objects { get; } = new List<IHittable>();`.
- Metodę dodającą obiekt do listy: `public void Add(IHittable obj) => Objects.Add(obj);`.
- Implementację metody `Hit`. Przeiteruj pętlą `foreach` przez wszystkie elementy w `Objects`. Za każdym razem przy zlokalizowaniu trafienia, nadpisuj zmienną określającą górny próg poszukiwań parametru `tMax` wynikiem tego trafienia. Dzięki temu, w wynikowym `hitInfo` ostatecznie znajdzie się struktura z danymi fizycznie najbliższego trafionego obiektu. Zwróć zmienną typu `bool` informującą, czy wystąpiło jakiekolwiek trafienie.

#### Złożenie całości w jedną aplikację

W klasie `Camera` dodaj metodę `Render`, zwracającą gotowy `Image`, a przyjmującą jako argument naszą scenę (`IHittable world`). Wewnątrz zagnieżdżonej pętli iterującej po pikselach obrazu (`i` po szerokości, `j` po wysokości):
1. Oblicz pozycję środka bieżącego piksela na wirtualnej rzutni:
   `Vector3 pixelCenter = _viewportCorner + (i * _pixelDu) + (j * _pixelDv);`
2. Wyznacz kierunek promienia jako wektor od pozycji obserwatora do punktu na rzutni:
   `Vector3 rayDirection = pixelCenter - Position;`
3. Skonstruuj promień `Ray(Position, rayDirection)` i przetestuj trafienie w obiekty sceny (`world.Hit(...)`).

Jeśli promień trafi w obiekt, ustaw kolor piksela na czerwony (`[1, 0, 0]`). W przeciwnym razie ustaw kolor na czarny (`[0, 0, 0]`).

W metodzie `Main` pliku `Program.cs`:
1. Utwórz obiekt `Scene` i dodaj sferę w punkcie {{< katex >}}(0, 0, -1){{< /katex >}} o promieniu 0.5 oraz płaszczyznę w punkcie {{< katex >}}(0, -0.5, 0){{< /katex >}} z wektorem normalnym {{< katex >}}(0, 1, 0){{< /katex >}}.
2. Skonfiguruj kamerę: `Position` na {{< katex >}}(0, 0, 1){{< /katex >}} oraz `Target` na {{< katex >}}(0, 0, -1){{< /katex >}}.
3. Wywołaj `cam.Render(world)` i wypisz wynik do konsoli za pomocą `Console.Write(image)`.

Aby wygenerować obraz, uruchom program, przekierowując standardowe wyjście do pliku:

```bash
dotnet run > image.ppm
```

### Etap 4: Materiały i rekursja

W tym etapie dodamy obsługę materiałów, które zdefiniują zachowanie promieni po uderzeniu w obiekt. Zaimplementujemy również system wielokrotnych odbić przy użyciu rekurencji.

#### Klasy abstrakcyjne

Z klasy abstrakcyjnej nie można bezpośrednio utworzyć instancji. Służy ona jako definicja bazowa dla klas pochodnych. Może zawierać deklaracje metod abstrakcyjnych (odpowiednik funkcji czysto wirtualnych z C++, np. `virtual void Method() = 0;`), które nie posiadają implementacji i wymagają zdefiniowania w klasie pochodnej, oraz metod wirtualnych (posiadających domyślną implementację, którą opcjonalnie można nadpisać).

#### Przekazywanie parametrów przez referencję

Przekazywanie argumentów przez referencję pozwala uniknąć kopiowania struktur w pamięci podczas wywołań metod. W C# służą do tego trzy słowa kluczowe:
- `out` – argument wyjściowy. Przekazywana zmienna nie musi być zainicjalizowana przed wywołaniem, jednak jej inicjalizacja wewnątrz metody przed zakończeniem wykonania jest wymagana przez kompilator.
- `ref` – dwukierunkowa referencja. Zmienna musi zostać zainicjowana przed przekazaniem do metody.
- `in` – referencja tylko do odczytu. Gwarantuje brak modyfikacji argumentu wewnątrz metody.

W pliku `Material.cs` zdefiniuj abstrakcyjną klasę `Material`:
- Zadeklaruj metodę abstrakcyjną: `public abstract bool Scatter(in Ray rIn, ref HitInfo hitInfo, out Vector3 attenuation, out Ray scattered);`. Przyjmuje ona promień wejściowy (`rIn`) oraz informacje o trafieniu (`hitInfo`). Przez argumenty wyjściowe zwraca wektor tłumienia koloru (`attenuation`) oraz promień odbity (`scattered`), a jako wynik działania (typ `bool`) zwraca informację, czy promień uległ odbiciu.
- Zadeklaruj metodę wirtualną `Emit()` zwracającą domyślnie wektor zerowy (`new Vector3(0, 0, 0)`).

#### EmissiveMaterial

W pliku `EmissiveMaterial.cs` utwórz klasę `EmissiveMaterial` dziedziczącą po `Material`. Reprezentuje ona materiał emitujący własne światło. Zdefiniuj w niej:
- Właściwość `Vector3 Color` inicjalizowaną przez konstruktor.
- Metodę `Scatter`, która zwraca `false` (wskazując brak dalszego odbicia promienia) oraz przypisuje domyślne wartości do zmiennych wyjściowych `out`.
- Metodę `Emit()` zwracającą właściwość `Color`.

#### Powiązanie materiałów z geometrią sceny

Zaktualizuj struktury odpowiedzialne za przechowywanie danych o kolizjach tak, aby umożliwiały odczyt właściwości trafionego materiału:
1. W pliku `HitInfo.cs` dodaj do struktury pole `public Material Mat;`.
2. W klasach `Sphere` i `Plane` dodaj właściwość `public Material Mat { get; set; }` i przypisz jej wartość z konstruktora.
3. W metodach `Hit` dla obu kształtów dodaj instrukcję przypisującą materiał do zwracanej struktury wyjściowej (np. `hitInfo.Mat = Mat;`).

#### Rekurencyjne wyliczanie koloru

W klasie `Camera` dodaj właściwość `public int MaxDepth { get; set; } = 50;`. Określa ona dopuszczalny limit wywołań rekurencyjnych. Zabezpiecza to program przed zawieszeniem (np. w sytuacji, gdy promień odbija się w nieskończoność między dwoma lustrami).

Wewnętrzną pętlę w metodzie `Render` zmodyfikuj tak, aby używała rekurencyjnej metody `RayColor`:

```csharp
image[i, j] = RayColor(ray, MaxDepth, world);
```

Zaimplementuj prywatną metodę `RayColor(Ray ray, int depth, IHittable world)`:
1. Warunek stopu: jeśli limit głębokości został wyczerpany (`depth <= 0`), przerwij rekurencję i zwróć kolor czarny `[0, 0, 0]`.
2. Sprawdź trafienie w obiekty sceny za pomocą `world.Hit(ray, 0.001f, float.PositiveInfinity, out HitInfo hitInfo)`. Jeśli brak kolizji, zwróć kolor czarny.
3. Pobierz światło emitowane przez materiał trafionego obiektu (`hitInfo.Mat.Emit()`).
4. Wywołaj metodę `Scatter(...)`. Jeżeli materiał rozprasza promień (`true`), zwróć sumę światła emitowanego oraz rekurencyjnego wywołania `RayColor(scattered, depth - 1, world)` pomnożonego przez `attenuation`.
5. W przeciwnym razie (brak odbicia) zwróć wyłącznie kolor wyemitowany.

> [!NOTE]
> Wartość `tMin` w wywołaniu funkcji `Hit` została ustawiona na `0.001f` w celu uniknięcia błędu precyzji zmiennoprzecinkowej. Zabezpiecza to przed sytuacją, w której wtórny promień wskutek zaokrągleń uderza bezpośrednio w tę samą powierzchnię, z której został wyemitowany (zjawisko *shadow acne*).

#### Weryfikacja etapu czwartego

W pliku `Program.cs` zmodyfikuj inicjalizację obiektów sceny. Utwórz czerwoną sferę, białą płaszczyznę oraz błękitną sferę o promieniu 1000 otaczającą scenę, emitującą światło (tzw. skysphere), po czym wyrenderuj obraz.

```csharp
world.Add(new Sphere(new Vector3(0, 0, 0), 1000.0f, new EmissiveMaterial(new Vector3(0.5f, 0.7f, 1.0f))));
world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new EmissiveMaterial(new Vector3(1, 0, 0)))); 
world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0), new EmissiveMaterial(new Vector3(1, 1, 1))));
```

### Etap 5: Materiał dyfuzyjny i wygładzanie krawędzi

W tym etapie zaimplementujemy materiał rozpraszający światło oraz wygładzanie krawędzi (antyaliasing) z użyciem wielokrotnego próbkowania.

#### Generowanie losowych wektorów

Działanie materiału rozpraszającego polega na odbijaniu uderzającego w niego promienia w losowym kierunku, dlatego struktura `Vector3` wymaga rozbudowy o generator losowych wektorów. Zastosujemy wbudowaną właściwość `System.Random.Shared`, która zapewnia współdzieloną, bezpieczną w kontekście wielowątkowości instancję generatora liczb pseudolosowych.

W pliku `Vector3.cs` zaimplementuj trzy nowe metody:

1. `NearZero()`: Metoda sprawdzająca, czy wektor jest bliski zera we wszystkich wymiarach. Wektor jest bliski zera, gdy wartość bezwzględna każdego z jego trzech wymiarów jest mniejsza niż z góry określony próg (np. `1e-8f`). Będzie to przydatne do eliminacji błędów matematycznych przy wyliczaniu wektora rozproszenia.
2. `RandomInUnitSphere()`: Metoda implementująca próbkowanie z odrzucaniem (rejection sampling). W nieskończonej pętli losuj współrzędne wektora w przedziale `[-1, 1)`. Jeżeli kwadrat długości tego wektora jest mniejszy niż 1 (co oznacza, że wyznaczony punkt znajduje się wewnątrz jednostkowej kuli), przerwij pętlę i zwróć ten wektor. Do losowania wartości bazowych wykorzystaj metodę `System.Random.Shared.NextDouble()`, która zwraca wartość zmiennoprzecinkową w przedziale `[0, 1)`.
3. `RandomUnitVector()`: Metoda zwracająca jednostkowy wektor kierunkowy. Wynikiem działania powinno być bezpośrednie zwrócenie znormalizowanego wektora pobranego z metody `RandomInUnitSphere()`.

#### Lambertian

W pliku `Lambertian.cs` utwórz klasę `Lambertian` dziedziczącą po `Material`. Modeluje ona idealnie matową powierzchnię, która po uderzeniu rozprasza padające światło w różnych kierunkach ze zmienną intensywnością (zależną od kąta). 

Fizycznie, w celu symulacji światła rozproszonego, materiał powinien wygenerować wiele odbitych promieni w różnych kierunkach dla każdego uderzenia. Rozgałęzianie promieni na każdym kroku spowodowałoby jednak wykładniczy wzrost wykonywanych obliczeń (eksplozję promieni). Aby tego uniknąć, śledzenie promieni (path tracing) stosuje model stochastyczny – w miejscu uderzenia metoda `Scatter` generuje zaledwie **jeden** promień odbity, za to w losowym kierunku. Brakujące rozproszone światło uśredni się samoistnie z upływem czasu na skutek wystrzelenia ogromnej puli promieni bazowych startujących z kamery dla jednego piksela (metoda Monte Carlo).

W klasie `Lambertian` zdefiniuj:
- Właściwość `Vector3 Albedo` (określającą współczynnik odbicia, czyli bazowy kolor materiału), inicjalizowaną przez konstruktor.
- Nadpisaną metodę `Scatter`. Wyznacza ona kierunek rozproszenia promienia (`scatterDirection`). Zgodnie z prawem Lamberta, światło rozprasza się z najwyższym prawdopodobieństwem w kierunku prostopadłym do powierzchni. Symuluje się to wyznaczając nowy wektor kierunku będący sumą wektora normalnego powierzchni i losowego wektora jednostkowego (z metody `RandomUnitVector()`).
- Zabezpiecz wyliczony kierunek przy pomocy metody `NearZero()`. Jeżeli wylosowany wektor będzie idealnie przeciwny do wektora normalnego (ich suma jest bliska zeru), kierunek rozproszenia awaryjnie powinien przyjąć wartość wektora normalnego.
- Zwróć promień wtórny oraz przypisz wartość z `Albedo` do zmiennej `attenuation`. Metoda powinna ostatecznie zwrócić `true`.

#### Antyaliasing

Obecnie przez środek każdego piksela rzutni przepuszczany jest dokładnie jeden promień. Ponieważ promień stanowi punktowy wycinek, trafia on w geometrię zawsze zero-jedynkowo. Przy ograniczonej rozdzielczości ekranu skutkuje to ostrym, schodkowym podziałem na krawędziach nachodzących na siebie kształtów (zjawisko aliasingu). Aby to zniwelować, wysyła się wiele promieni lekko przesuniętych losowo w granicach pojedynczego piksela. Ostateczny kolor piksela stanowi uśredniony wynik kolorów zebranych przez wszystkie wystrzelone próbki.

W klasie `Camera` dodaj właściwość `public int SamplesPerPixel { get; set; } = 100;`. Określa ona liczbę promieni wysyłanych dla pojedynczego piksela ekranu.

Zmodyfikuj zawartość wewnętrznej pętli w metodzie `Render`. Zastąp pojedyncze wywołanie `RayColor` pętlą akumulującą kolor. W każdej iteracji:
1. Wylosuj przesunięcie punktu na siatce piksela w osi X oraz Y (zmienne `offsetX` i `offsetY` z przedziału `[-0.5, 0.5)`).
2. Oblicz pozycję wirtualnego punktu rzutni dla aktualnej iteracji próbkowania, dodając wylosowane przesunięcia do standardowych współrzędnych `i` oraz `j` przed wymnożeniem ich przez wektory kroku `_pixelDu` i `_pixelDv`.
3. Skonstruuj promień wychodzący z pozycji kamery przechodzący przez wylosowany punkt i dodaj wynik wywołania `RayColor` do wektora sumy (akumulatora koloru).

Po wykonaniu nowej pętli dla wszystkich próbek, podziel wektor skumulowanego koloru przez ustaloną liczbę `SamplesPerPixel` i przypisz go docelowo do tablicy pikseli obrazu.

#### Efekt końcowy

W pliku `Program.cs` zmodyfikuj kod dodający obiekty do sceny. Utwórz trzy sfery z materiałem `Lambertian` o różnych kolorach obok siebie oraz płaszczyznę w kolorze zielonym. Pozostaw instancję sfery z materiałem `EmissiveMaterial` otaczającą scenę, służącą jako tło/niebo.

```csharp
world.Add(new Sphere(new Vector3(0, 0, 0), 1000.0f, new EmissiveMaterial(new Vector3(0.5f, 0.7f, 1.0f))));
world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Vector3(1, 0, 0)))); 
world.Add(new Sphere(new Vector3(-1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(0, 0, 1)))); 
world.Add(new Sphere(new Vector3(1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(1, 1, 0)))); 
world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0), new Lambertian(new Vector3(0.2f, 0.8f, 0.2f))));
```

Wyrenderuj ostateczny obraz za pomocą polecenia `dotnet run > image.ppm`.

> [!NOTE]
> **Korekcja gamma:** Wypisane wartości kolorów znajdują się w przestrzeni liniowej. Ponieważ monitory komputerowe stosują nieliniową charakterystykę jasności (gamma), wyrenderowany obraz może wydawać się ciemniejszy w partiach cieniowych. Aby uzyskać prawidłowe odwzorowanie tonalne (tzw. przestrzeń *sRGB* / gamma ok. 2.2), przed zapisem wartości do pliku podnosi się każdą składową koloru do potęgi `1 / gamma` (np. w najprostszym przybliżeniu dla gamma = 2 jest to po prostu pierwiastek kwadratowy: `MathF.Sqrt(color)`).

### Kierunki dalszego rozwoju

Napisana w ramach tutoriala aplikacja to dobra podstawa do dalszej rozbudowy. Poniżej zebraliśmy najważniejsze mechanizmy, które warto dodać w kolejnych krokach, aby rozwinąć projekt:

- **Zaawansowane materiały (Metal, Szkło):** Obecnie światło rozprasza się losowo (matowo). Aby uzyskać efekt lustra, należy zaimplementować klasę wyliczającą wektor idealnego odbicia (kąt padania równy kątowi odbicia). Materiały przezroczyste (szkło, woda) wymagają z kolei dodania obsługi załamania światła (prawa Snella) i zależności odbicia od kąta patrzenia (tzw. aproksymacji Schlicka).
- **Obsługa trójkątów i ładowanie modeli 3D:** Większość grafiki 3D opiera się na trójkątach, a nie sferach. Dodanie kodu wyliczającego przecięcie promienia z trójkątem pozwala na ładowanie gotowych siatek wyeksportowanych m.in. z Blendera w powszechnych formatach tekstowych (np. OBJ).
- **Zapis do zewnętrznych formatów obrazu:** Użyty format PPM jest łatwy we wdrożeniu, ale mało optymalny przy zapisie na dysk. W projektach .NET do obsługi obrazów PNG czy JPEG standardowo podłącza się zewnętrzne biblioteki, np. *ImageSharp* lub *SkiaSharp* (odpowiedniki biblioteki *stb_image* znanej z C/C++). 
- **Optymalizacja za pomocą BVH:** Przy tysiącach obiektów na scenie, sprawdzanie kolizji promienia z każdym elementem powoduje drastyczne spowolnienie. Struktura BVH (*Bounding Volume Hierarchy*) organizuje geometrię w drzewo brył otaczających. Jeżeli promień nie trafia w nadrzędną bryłę zbiorczą, algorytm natychmiast odrzuca całą zawartą w niej podgrupę obiektów, co w ogromnym stopniu przyspiesza renderowanie.
- **Obsługa tekstur:** System wspiera na razie tylko jednolite barwy materiałów. Nałożenie na obiekt tekstury polega na przypisaniu do geometrii współrzędnych określających ułożenie obrazka (UV), a następnie odczytywaniu właściwych pikseli z zewnętrznego pliku w momencie trafienia promienia w powierzchnię.

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/03.basics/example1" >}})
