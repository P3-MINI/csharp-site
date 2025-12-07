---
title: "Przegląd"
weight: 10
---

# Przegląd

**Komunikacja Międzyprocesowa (_IPC - Inter-Process Communication_)** to zestaw mechanizmów, które umożliwiają procesom wymianę danych i synchronizację ich działań. W środowisku .NET, dostępne są różne metody IPC, pozwalające na tworzenie aplikacji, które składają się z wielu współpracujących procesów.

Między innymi:

- **Łącza (_Pipes_)**: Umożliwiają komunikację strumieniową między procesami na tej samej maszynie. Wyróżniamy łącza nazwane (*Named Pipes*), oraz łącza anonimowe (*Anonymous Pipes*), przeznaczone dla komunikacji między procesem rodzic-dziecko.
- **Sieć**: Wykorzystuje protokoły sieciowe do komunikacji między procesami, niezależnie od tego, czy znajdują się na tej samej maszynie, w sieci lokalnej, czy globalnej.
- **Pliki**: Procesy komunikują się poprzez zapisywanie i odczytywanie danych ze współdzielonych plików na dysku. Jest to prosta metoda, ale może wymagać dodatkowej synchronizacji.
- **Pamięć dzielona (Shared Memory)**: Pozwala procesom na bezpośredni dostęp do wspólnego bloku pamięci, co jest najszybszą formą IPC. Wymaga jednak zaawansowanego zarządzania synchronizacją dostępu do pamięci.
