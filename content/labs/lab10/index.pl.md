---
title: "Lab10"
weight: 10
---

# Laboratorium 10: Programowanie równoległe i asynchroniczne

## Generowanie fraktali

{{% hint info %}}
**Czym jest programowanie równoległe?**

Programowanie równoległe to paradygmat programowania, wykorzystujący architekturę nowoczesnych, wielordzeniowych procesorów (CPU) w celu wykonywania wielu obliczeń równocześnie.

Zamiast przetwarzać dane sekwencyjnie w jednym wątku, rozdzielamy pracę na wiele wątków, które wykonują swoją pracę w tym samym momencie.

{{% /hint %}}

### Opis zadania

Celem zadania jest implementacja oraz porównanie czasu wykonania różnych metod zrównoleglających obliczenia wykonywane podczas generowania [Zbioru Mandelbrota](https://en.wikipedia.org/wiki/Mandelbrot_set).

Kod początkowy zawiera abstrakcyjną klasę `MandelbrotSetGenerator`, która zarządza całym procesem generowania fraktala i zapisywania go do pliku `.png`

Klasa `SingleThreadGenerator` stanowi konkretną jednowątkową implementację generatora. Używa ona prostej, zagnieżdżonej pętli `for` do iteracji po wszystkich pikselach generowanego obrazka. Posłuży jako linia bazowa do pomiaru wydajności.

Należy zaimplementować następujące metody zrównoleglania obliczeń:

- `MultiThreadGenerator`: Metoda wielowątkowa, która ręcznie tworzy i zarządza obiektami `Thread`.
- `TasksGenerator`: Metoda, która zrównolegla pracę przy użyciu puli wątków (`ThreadPool`) poprzez obiekty `Task`.
- `ParallelGenerator`: Metoda, która używa biblioteki TPL (ang. _Task Parallel Library_).

`Program.cs` zawiera logikę mierzenia czasu i uruchamiania każdego generatora po kolei.

Poprawnie wygenerowany fraktal powinien wyglądać następująco:

<div style="text-align: center;">
  <img src="/labs/lab10/mandelbrotset.png" width="400px" alt="mandelbrotset.png" />
</div>

{{% hint warning %}}
**Uwagi implementacyjne**

- **Liczba wątków:**
  - W implementacjach `MultiThreadGenerator` i `TasksGenerator` należy stworzyć `N` jednostek pracy (wątków/zadań), gdzie `N` jest równe liczbie rdzeni procesora. Jest to optymalna liczba dla zadań w 100% obciążających CPU.
- **Podział pracy:**
  - W przypadku generowania fraktala, najprostszą strategią jest podział obrazu na `N` równych, poziomych pasów.
  - Oblicz, ile wierszy przypada na jeden wątek, a następnie w pętli przekaż każdemi wątkowi/zadaniu odpowiedni zakres do przetworzenia.

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: Threads and threading](https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading)
- [Microsoft Learn: Task Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task?view=net-9.0)
- [Microsoft Learn: Write a Simple Parallel.For Loop](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP] 
> **Rozwiązanie** 
> {{< filetree dir="labs/lab10/solution/FractalsGenerator" >}}

## Agregator ofert

{{% hint info %}}
**Czym jest programowanie asynchroniczne?**

{{% /hint %}}

### Opis zadania

{{% hint warning %}}
**Uwagi implementacyjne**

- **:**
  -.

{{% /hint %}}

{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: ]()
- [Microsoft Learn: ]()
- [Microsoft Learn: ]()
- [Microsoft Learn: ]()

{{% /hint %}}

### Przykładowe rozwiązanie
