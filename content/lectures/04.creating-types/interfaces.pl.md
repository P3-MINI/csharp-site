---
title: "Interfejsy"
---

# Interfejsy

Interfejsy są podobne do klas abstrakcyjnych, ale **definiują tylko zachowania**, a **nie definiują stanu**. Analogiczną strukturą z C++ byłyby klasy ze wszystkimi składowymi będącymi czysto wirtualnymi metodami.

- Składowymi interfejsu mogą być tylko i wyłącznie metody (także właściwości i zdarzenia), nie może zawierać pól.
- Metody w interfejsach są niejawnie abstrakcyjne.
- Klasy i struktury wspierają implementowanie wielu interfesjów.

Definicja interfejsu wygląda następujaco, tutaj przykład interfejsu `IEnumerator` z przestrzeni nazw `System.Collections`:

```csharp
public interface IEnumerator
{
    bool MoveNext();
    object Current { get; }
    void Reset();
}
```

Zwyczajowo, nazwy interfejsów poprzedzamy literą `I`. Składowe są domyślnie publiczne, 