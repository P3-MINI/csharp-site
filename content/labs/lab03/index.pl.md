---
title: "Lab03"
weight: 10
---

# Laboratorium 3 - Przykładowe - Klasy/Struktury, Dziedzicznie, Właściwości, Modyfikatory dostępu

## Wprowadzenie

Zadaniem na dzisiejszych laboratoriach jest stworzenie prostej biblioteki odpowiedzialnej za działania na macierzach.

### Uwaga

W celu przechodzenia do następnych etapów należy odkomentowywać poszczególne linijki typu `#define STAGE0X` znajdujące się na początku pliku `Program.cs`. Inna ingerencja w ten plik jest zakazana.

## Kod początkowy

> [!NOTE]
> **Matrices**
> {{< filetree dir="labs/lab03/student/Matrices" >}}

## Etap 1 (4 pkt)

W folderze `Matrix` stwórz abstrakcyjną klasę `Matrix`, która będzie posiadała:
 - konstruktor przyjmujący ilość wierszy i kolumn macierzy jako `int`
 - publiczną właściwość `Rows`, zwracająca ilość wierszy w macierzy
 - publiczną właściwość `Columns`, zwracająca ilość kolumn w macierzy
 - abstrakcyjny indeksator, który przyjmuje numer wiersza i kolumny. Pozwala na odczyt i zapis wartości `float` w konkretną komórkę macierzy.
 - przeciążoną metodę `ToString`, w taki sposób, aby macierze były drukowane tak samo jak w przykładowym wyjściu programu

Stwórz następujące klasy dziedziczące po `Matrix`: `DenseMatrix` i `SparseMatrix`.

`DenseMatrix` powinna przechowywać elementy w formie tablicy prostokątnej.

`SparseMatrix` powinna przechowywać w słowniku jedynie niezerowe elementy macierzy. Taka implementacja zmniejsza ilość potrzebnej pamięci w przypadku przechowywania macierzy rzadkich, czyli takich w których znaczna ilość wartości jest równa zero.

Obie klasy powinny posiadać konstruktor przyjmujący liczbę wierszy i kolumn. Nowo zainicjalizowane macierze powinny być wypełnione zerami.

Program powinien sprawdzać poprawność argumentów. W razie niepoprawności powinien generować wyjątek typu `ArgumentException` z odpowiednim komentarzem.

### Wskazówki
 - [Microsoft Learn: Indexers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/?redirectedfrom=MSDN)
 - [Microsoft Learn: StringBuilder](https://learn.microsoft.com/pl-pl/dotnet/api/system.text.stringbuilder?view=net-8.0)
 - [Microsoft Learn: String interpolation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)
 - [Microsoft Learn: Standard numeric format strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings?redirectedfrom=MSDN)
  - [Microsoft Learn: Create and throw exceptions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)


 ### Punktacja
 Po 2 punkty za każdą klasę pochodną.


## Etap 2 (3 pkt)

Do klasy `Matrix` dodaj abstrakcyjne metody `Identity`, `Transpose` i `Norm`. Zaimplementuje w klasach pochodnych.

Metoda `Identity` niczego nie przyjmuje, ani niczego nie zwraca. Wypełnia jedynie macierz tak, aby była identycznościowa. W przypadku macierzy niekwadratowej metoda powinna generować wyjątek typu `InvalidOperationException`.

Metoda `Transpose` zwraca macierz po transpozycji.

Metoda `Norm` zwraca normę macierzy definiowaną jako
{{< katex display=true >}}
||A|| = \sqrt{\sum_i \sum_j A_{i,j}^2}.
{{< /katex >}}

Implementacje w klasie `SparseMatrix` powinny mieć liniową zależność czasową względem ilości niezerowych elementów macierzy.


### Punktacja
Po 1 pkt za każdą z metod.

## Etap 3 (3 pkt)

Do klasy `Matrix` dodaj abstrakcyjną metodę `GetInstance`, która przyjmuje liczbę wierszy i kolumn i zwraca obiekt typu `Matrix`. Metoda ta nie powinna być możliwa do wywołania spoza tej klasy i klas pochodnych. Zaimplementuj tę metodę w klasach pochodnych w taki sposób, że każda klasa zwraca instancję swojej klasy z żądaną wielkością.

W klasie `Matrix` zaimplementuje następujące przeciążenia operatorów:
 - operator dodawania macierzy
 - operator odejmowania macierzy
 - operator mnożenia macierzy
 - operator mnożenia macierzy przez skalar

W implementacji wykorzystaj metodę `GetInstance`. Dla funkcji przyjmujących jedną macierz powinna zwracać macierz tego samego typu. Dla funkcji przyjmujących dwie macierze zwracana implementacja powinna być taka sama jak lewego (pierwszego) argumentu.

### Wskazówki
- [Microsoft Learn: Operator overloading](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading)

### Punktacja
1 za metodę `GetInstance`, po 0.5 pkt za każde przeciążenie. 

## Etap 4 (2 pkt)

W ostatnim etapie wykorzystamy interfejs, który zbudowaliśmy podczas poprzednich etapów. Należy stworzyć klasę `MatrixAlgorithms`, a w niej statyczną metodę `PseudoInverse` przyjmującą `Matrix` oraz maksymalną liczbę iteracji, która ma domyślą wartość 1000. Za wartość {{< katex >}}\epsilon{{< /katex >}} przyjmij {{< katex >}}10^{-7}{{< /katex >}}.

Na potrzeby laboratorium będziemy się zajmować tylko kwadratowymi macierzami.

Algorytm:

 > {{< katex >}}A{{< /katex >}} - input square matrix
 >
 > {{< katex >}}I{{< /katex >}} - identity matrix
 >
 >
 > {{< katex >}}\alpha = \frac{1}{||A||^2}{{< /katex >}}  
 > {{< katex >}}A1 = \alpha \cdot A^T{{< /katex >}}  
 > {{< katex >}}\text{for } i=1...maxIter:{{< /katex >}}  
 > {{< katex >}}\qquad A2 = A1 + A1 \cdot (I - A\cdot A1){{< /katex >}}  
 > {{< katex >}}\qquad \text{if } ||A2-A1|| < \epsilon:{{< /katex >}}  
 > {{< katex >}}\qquad \qquad \text{break}{{< /katex >}}  
 > {{< katex >}}\qquad A1 = A2{{< /katex >}}  
 > {{< katex >}}\text{return A2}{{< /katex >}}  

### Punktacja
Poprawna implementacja algorytmu - 2 pkt.
Częściowa implementacja - 1 pkt

## Przykładowe rozwiązanie

> [!NOTE]
> **Matrices**
> {{< filetree dir="labs/lab03/solution/Matrices" >}}
