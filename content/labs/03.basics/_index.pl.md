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
- Indeksator (odpowiednik przeciążonego `operatora[]` z C++), który pozwala na odwoływanie się do obiektu tak jak do tablicy. Posłuży on do wygodnego odczytywania i zapisywania pikseli (np. `image[x, y] = color`):

```csharp
public Vector3 this[int x, int y]
{
    get => Pixels[x, y];
    set => Pixels[x, y] = value;
}
```

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

W pliku `Program.cs` utwórz instancję obrazu o rozdzielczości 1920x1080. Wygeneruj dowolny wzór (na przykład gradient) wypełniając nim piksele, a następnie wypisz obraz na standardowe wyjście. Możesz wykorzystać poniższy przykładowy fragment kodu:

```csharp
namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Image image = new Image(1920, 1080);
        
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                float r = (float) i / image.Width;
                float g = (float) j / image.Height;
                float b = MathF.Sin(2 * MathF.PI * ((float) i * j / image.Width / image.Height));
                
                image[i, j] = new Vector3(r, g, b);
            }
        }
        
        Console.WriteLine(image);
    }
}
```

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
1. Przeciążenia operatorów: unarnego `-` (odwrócenie wektora), dodawania `+` i odejmowania `-` dwóch wektorów oraz mnożenia `*` i dzielenia `/` wektora przez skalar (liczbę `float`).
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
1. Zdefiniuj właściwości i powiąż je z prywatnymi polami o przypisanych wartościach domyślnych: `float AspectRatio` (`16.0f / 9.0f`), `int ImageWidth` (`1280`), `Vector3 Position` (`new Vector3(0,0,0)`), `Vector3 Target` (`new Vector3(0,0,-1)`) oraz `float Fov` (`90.0f`). Każdy akcesor `set` w wymienionych właściwościach musi wywoływać metodę `Recalculate()`.
2. Zadeklaruj prywatne pola przechowujące wyliczone parametry rzutni oraz kamery: `int _height`, `Vector3 _pixelDu`, `Vector3 _pixelDv` oraz `Vector3 _viewportCorner`.
3. Dodaj bezparametrowy konstruktor, który jednorazowo wywoła metodę `Recalculate()` do inicjalizacji obiektu.

Logika metody `Recalculate()` wylicza pozycję rzutni i wektory nawigujące po jej powierzchni. Najpierw, na podstawie pozycji kamery i pozycji rzutni (właściwość Target), wyznaczana jest baza ortonormalna (wektory `u`, `v`, `w` określające kierunki osi lokalnych kamery). Następnie za pomocą kąta widzenia i proporcji obrazu ustalany jest fizyczny rozmiar wirtualnego ekranu. Finalnie wyliczamy współrzędne lewego górnego rogu rzutni (`_viewportCorner`) oraz wektory kroku między poszczególnymi pikselami w pionie i poziomie (`_pixelDu`, `_pixelDv`), tak żeby później łatwo można było po nich iterować.

> [!NOTE]
> Poniżej znajdziesz gotową implementację metody przygotowującej rzutnię. Możesz umieścić ją bezpośrednio w swojej klasie.

```csharp
private void Recalculate()
{
    _height = (int)(ImageWidth / AspectRatio);
    if (_height < 1) 
    {
        _height = 1;
    }

    float theta = Fov * (MathF.PI / 180.0f);
    float h = MathF.Tan(theta / 2.0f);
    float distanceToViewport = (Position - Target).Length();
    float viewportHeight = 2.0f * h * distanceToViewport;
    float viewportWidth = viewportHeight * ((float)ImageWidth / _height);

    Vector3 w = (Position - Target).Normalize();
    Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), w).Normalize();
    Vector3 v = Vector3.Cross(w, u);

    Vector3 viewportU = viewportWidth * u;
    Vector3 viewportV = viewportHeight * -v;

    _pixelDu = viewportU / ImageWidth;
    _pixelDv = viewportV / _height;

    Vector3 viewportUpperLeft = Position - (distanceToViewport * w) - viewportU / 2.0f - viewportV / 2.0f;
    _viewportCorner = viewportUpperLeft + 0.5f * (_pixelDu + _pixelDv);
}
```

### Etap 3: Przecinanie promieni z obiektami

W tym etapie zaimplementujemy wykrywanie przecięć promieni z geometrią na scenie.

#### Informacje o przecięciu (HitInfo)

Gdy promień przecina obiekt, musimy zebrać kilka informacji: pozycję uderzenia, wektor normalny powierzchni w miejscu trafienia oraz wartość parametru `T` promienia, dla której nastąpiło trafienie. Informacje te będą nam przydatne później przy obliczaniu koloru promienia.

Utwórz plik `HitInfo.cs` ze strukturą przechowującą wynik uderzenia:
1. Zdefiniuj pola publiczne: `Vector3 Position`, `Vector3 Normal`, `float T` oraz `bool FrontFace`.
2. Zdefiniuj metodę `SetFaceNormal(Ray r, Vector3 outwardNormal)`. Do poprawnego obliczania oświetlenia potrzebujemy, aby wektor normalny w miejscu trafienia był zawsze skierowany w stronę przeciwną do nadlatującego promienia. Metoda ta powinna sprawdzić – na podstawie iloczynu skalarnego kierunku promienia i wektora normalnego powierzchni – czy promień uderza w obiekt z zewnątrz (iloczyn ujemny). Jeżeli uderza od wewnątrz, zapisany wektor normalny musi zostać odwrócony.

```csharp
public void SetFaceNormal(Ray r, Vector3 outwardNormal)
{
    FrontFace = Vector3.Dot(r.Direction, outwardNormal) < 0;
    Normal = FrontFace ? outwardNormal : -outwardNormal;
}
```

#### Interfejs IHittable i modyfikator "out"

Interfejsy to abstrakcyjne typy definiujące kontrakt, czyli zbiór metod i właściwości, które klasa musi zaimplementować. Nie posiadają one własnego stanu (pól) ani implementacji. W naszym programie każdy obiekt, w który może uderzyć promień, będzie implementował wspólny interfejs, co pozwoli na polimorficzną obsługę różnych kształtów na scenie.

Jednym ze sposobów na zwrócenie wielu wartości z metody w C# jest użycie słowa kluczowego `out`. Wymusza ono zainicjowanie przekazanej w ten sposób zmiennej przed opuszczeniem metody (np. metoda sprawdzająca trafienie promienia może zwrócić jako wynik `bool`, a przez argument zwrócić wygenerowaną strukturę `HitInfo`).

Utwórz plik `IHittable.cs` z definicją interfejsu (w C# nazwy interfejsów zwyczajowo zaczynamy od dużej litery `I`):

```csharp
namespace Raytracing;

public interface IHittable
{
    bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo);
}
```

#### Implementacja IHittable (Sphere i Plane)

Każdy obiekt na naszej scenie (włącznie z samą sceną) będzie implementował interfejs `IHittable`. Kontrakt ten oznacza wprost, że w dany obiekt można "strzelać" promieniami. Dzięki temu wszystkie elementy w świecie będą traktowane polimorficznie – kamera nie musi wiedzieć, czy strzela w pojedynczą kulę, całą scenę, czy trójkąt, o ile dany obiekt implementuje interfejs `IHittable`.

Utwórz definicję klasy sfery (plik `Sphere.cs`):
- Utwórz klasę `Sphere` implementującą interfejs `IHittable` (`class Sphere : IHittable`) i definiującą właściwości `Vector3 Center` oraz `float Radius`.
- Zdefiniuj konstruktor inicjalizujący podane wartości.
- Zaimplementuj metodę `Hit`. Możesz posłużyć się gotowym kodem, wyznaczającym pierwiastki równania kwadratowego dla punktu przecięcia promienia ze sferą ([więcej informacji znajdziesz w książce](https://raytracing.github.io/books/RayTracingInOneWeekend.html#addingasphere/ray-sphereintersection)):

```csharp
public bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo)
{
    hitInfo = new HitInfo();
    Vector3 oc = ray.Origin - Center;
    float a = ray.Direction.LengthSquared();
    float halfB = Vector3.Dot(oc, ray.Direction);
    float c = oc.LengthSquared() - Radius * Radius;

    float discriminant = halfB * halfB - a * c;
    if (discriminant < 0) return false;
    
    float sqrtd = MathF.Sqrt(discriminant);
    float root = (-halfB - sqrtd) / a;
    
    if (root < tMin || tMax < root)
    {
        root = (-halfB + sqrtd) / a;
        if (root < tMin || tMax < root) return false;
    }

    hitInfo.T = root;
    hitInfo.Position = ray.At(hitInfo.T);
    Vector3 outwardNormal = (hitInfo.Position - Center) / Radius;
    hitInfo.SetFaceNormal(ray, outwardNormal);
    
    return true;
}
```

Utwórz klasę `Plane` (plik `Plane.cs`) implementującą interfejs `IHittable`, reprezentującą płaszczyznę.
- Dodaj właściwości `Vector3 Point` oraz `Vector3 Normal`. Matematycznie płaszczyzna opisana jest za pomocą dowolnego leżącego na niej punktu (właściwość `Point`) oraz wektora do niej prostopadłego (właściwość `Normal`). W konstruktorze zadbaj, by przypisywany wektor normalny został znormalizowany.
- Zaimplementuj metodę `Hit`. Z matematycznego punktu widzenia, punkt `P` leży na płaszczyźnie, jeżeli iloczyn skalarny wektora `(P - Point)` oraz wektora normalnego płaszczyzny wynosi zero. Podstawiając do tego równanie promienia `P(t) = Origin + t * Direction`, możemy wyznaczyć parametr `t`. Jeżeli mianownik we wzorze na `t` (iloczyn skalarny kierunku promienia i normalnej płaszczyzny) jest bliski zeru, oznacza to, że promień jest równoległy do płaszczyzny. Poniżej znajduje się implementacja tej logiki:

```csharp
public bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo)
{
    hitInfo = new HitInfo();
    float denom = Vector3.Dot(Normal, ray.Direction);
    
    if (MathF.Abs(denom) > 1e-6f)
    {
        float t = Vector3.Dot(Point - ray.Origin, Normal) / denom;
        if (t > tMin && t < tMax)
        {
            hitInfo.T = t;
            hitInfo.Position = ray.At(t);
            hitInfo.SetFaceNormal(ray, Normal);
            return true;
        }
    }
    return false;
}
```

#### Scena

Nasza scena będzie się składać ze zbioru obiektów. Do przechowywania elementów o dynamicznym rozmiarze użyjemy generycznej klasy `List<T>` (z przestrzeni nazw `System.Collections.Generic`).

Utwórz klasę `Scene`, implementującą interfejs `IHittable`. Zdefiniuj w niej:
- Publiczną listę: `public List<IHittable> Objects { get; } = new List<IHittable>();`.
- Metodę dodającą obiekt do listy: `public void Add(IHittable obj) => Objects.Add(obj);`.
- Implementację metody `Hit`. Przeiteruj pętlą `foreach` przez wszystkie elementy w `Objects`. Za każdym razem przy zlokalizowaniu trafienia, nadpisuj zmienną określającą górny próg poszukiwań parametru `tMax` wynikiem tego trafienia. Dzięki temu, w wynikowym `hitInfo` ostatecznie znajdzie się struktura z danymi fizycznie najbliższego trafionego obiektu. Zwróć zmienną typu `bool` informującą, czy wystąpiło jakiekolwiek trafienie.

#### Złożenie całości w jedną aplikację

W klasie `Camera` dodaj metodę `Render`, zwracającą gotowy `Image`, a przyjmującą jako argument naszą scenę (`IHittable world`). Wewnątrz zagnieżdżonej pętli iterującej po pikselach obrazu, oblicz pozycję danego piksela na wirtualnej rzutni. Skonstruuj promień wychodzący z pozycji kamery i skierowany w wyliczony piksel, a następnie wykonaj metodę `Hit` na świecie. Jeśli promień w cokolwiek uderzy, ustaw jego kolor na czerwony (`[1, 0, 0]`). W przeciwnym razie ustaw kolor na czarny (`[0, 0, 0]`).

Na koniec utwórz instancję sceny oraz kamery w pliku `Program.cs`, wywołaj metodę `Render` i wypisz wynik do konsoli. Gotowy kod znajduje się poniżej:

```csharp
using System;

namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Scene world = new Scene();
        world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f));
        world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0)));

        Camera cam = new Camera();
        cam.Position = new Vector3(0, 0, 1);
        cam.Target = new Vector3(0, 0, -1);

        Image image = cam.Render(world);
        Console.Write(image.ToString());
    }
}
```

Aby wygenerować obraz, uruchom program, przekierowując standardowe wyjście do pliku:

```bash
dotnet run > image.ppm
```

### Etap 4: Materiały i rekursja

W tym etapie dodamy obsługę materiałów, które zdefiniują zachowanie promieni po uderzeniu w obiekt. Zaimplementujemy również system wielokrotnych odbić przy użyciu rekurencji.

#### Klasy abstrakcyjne

Z klasy abstrakcyjnej nie można bezpośrednio utworzyć instancji. Służy ona jako definicja bazowa dla klas pochodnych. Może zawierać deklaracje metod abstrakcyjnych (pozbawionych implementacji, wymagających zdefiniowania w klasie pochodnej) oraz metod wirtualnych (posiadających domyślną implementację, którą opcjonalnie można nadpisać).

#### Przekazywanie parametrów przez referencję

Przekazywanie argumentów przez referencję pozwala uniknąć kopiowania struktur w pamięci podczas wywołań metod. W C# służą do tego trzy słowa kluczowe:
- `out` – argument wyjściowy. Przekazywana zmienna nie musi być zainicjalizowana przed wywołaniem, jednak jej inicjalizacja wewnątrz metody przed zakończeniem wykonania jest wymagana przez kompilator.
- `ref` – dwukierunkowa referencja. Zmienna musi zostać zainicjowana przed przekazaniem do metody.
- `in` – referencja tylko do odczytu. Gwarantuje brak modyfikacji argumentu wewnątrz metody.

W pliku `Material.cs` zdefiniuj abstrakcyjną klasę `Material`:
1. Zadeklaruj metodę abstrakcyjną `Scatter`: `public abstract bool Scatter(in Ray rIn, ref HitInfo hitInfo, out Vector3 attenuation, out Ray scattered);`. Przyjmuje ona promień wejściowy (`rIn`) oraz informacje o trafieniu (`hitInfo`). Przez argumenty wyjściowe zwraca wektor tłumienia koloru (`attenuation`) oraz promień odbity (`scattered`), a jako wynik działania (typ `bool`) zwraca informację, czy promień uległ odbiciu.
2. Zadeklaruj metodę wirtualną `Emit()` zwracającą domyślnie wektor `new Vector3(0, 0, 0)`.

```csharp
namespace Raytracing;

public abstract class Material
{
    public abstract bool Scatter(in Ray rIn, ref HitInfo hitInfo, out Vector3 attenuation, out Ray scattered);
    
    public virtual Vector3 Emit() => new Vector3(0, 0, 0);
}
```

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

Zaimplementuj metodę `RayColor`. Jeżeli zmienna określająca pozostałą głębokość rekurencji (`depth`) wyniesie 0 lub mniej, przerwij obliczenia i zwróć czarny kolor. 

W przeciwnym wypadku sprawdzane jest przecięcie promienia ze sceną (`world.Hit`). Jeżeli promień nie uderzy w żaden obiekt, zwracany jest czarny kolor. W przypadku trafienia w obiekt następuje obliczenie koloru:
1. W pierwszej kolejności z materiału (jeżeli pole `Mat` nie jest równe `null`) pobierana jest wartość emitowanego przez niego światła (`Emit()`).
2. Następnie wywoływana jest funkcja `Scatter` w celu sprawdzenia, czy dany materiał emituje promień wtórny.
3. Jeżeli tak, funkcja zwraca sumę wyemitowanego koloru z punktu pierwszego oraz koloru uzyskanego dla promienia odbitego (poprzez wywołanie rekurencyjne `RayColor` na promieniu `scattered` ze zmniejszoną wartością `depth`, przemnożonego przez `attenuation`).
4. Jeżeli `Scatter` zwróci `false` (np. przy `EmissiveMaterial`), funkcja kończy rekurencję i zwraca wyłącznie wartość koloru wyemitowanego pobraną w punkcie 1.

```csharp
private Vector3 RayColor(Ray ray, int depth, IHittable world)
{
    if (depth <= 0) return new Vector3(0, 0, 0);

    if (world.Hit(ray, 0.001f, float.PositiveInfinity, out HitInfo hitInfo))
    {
        Vector3 emitted = hitInfo.Mat != null ? hitInfo.Mat.Emit() : new Vector3(0, 0, 0);

        if (hitInfo.Mat != null && hitInfo.Mat.Scatter(in ray, ref hitInfo, out Vector3 attenuation, out Ray scattered))
            return emitted + attenuation * RayColor(scattered, depth - 1, world);
        
        return emitted;
    }
    
    return new Vector3(0, 0, 0); 
}
```

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

## Przykładowe zadania

Wykonaj przykładowe zadanie z poprzedniego roku. Jeżeli jesteś w stanie je wykonać w przeciągu 90 minut, oznacza to, że jesteś dobrze przygotowany do zajęć.

- [Zadanie 1]({{< ref "/labs/03.basics/example1" >}})
