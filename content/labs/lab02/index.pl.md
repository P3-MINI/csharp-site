---
title: "Lab02"
weight: 10
---

# Laboratorium 2: MSBuild, Unit Tests, .NET CLI

## Task 1: MSBuild

Twoim zadaniem jest zmodyfikowanie pliku `CppProject.proj` w celu dodania następujących funkcjonalności:

### 1. Konfiguracja `Debug` i `Release`

Celem jest dodanie do projektu wsparcia dla dwóch konfiguracji budowania: `Debug` i `Release`.
- **Debug**: Konfiguracja deweloperska, powinna zawierać symbole debugowania i mieć wyłączone optymalizacje.
- **Release**: Konfiguracja produkcyjna, powinna być zoptymalizowana pod kątem wydajności i nie zawierać symboli debugowania.

**Wymagania:**
- Dodaj właściwość `Configuration`, która domyślnie będzie ustawiona na `Debug`.
- Użyj warunkowych grup właściwości (`PropertyGroup`), aby zdefiniować różne flagi kompilatora (dla symboli debugowania i optymalizacji) w zależności od aktywnej konfiguracji.
- Zmodyfikuj targety, aby używały zdefiniowanych flag; `-O0` i `-g` dla konfiguracji `Debug`; `-O2` dla konfiguracji `Release`.
- Zmodyfikuj `OutputPath`, aby pliki wynikowe dla każdej konfiguracji trafiały do osobnych podkatalogów (np. `build/Debug/` i `build/Release/`).

Gdy skończysz zbuduj z lini poleceń aplikację w konfiguracji deweloperskiej i produkcyjnej.

### 2. Kompilacja przyrostowa (Incremental Builds)

Żeby przyspieszyć proces budowania, często implementuje się tzw. kompilację przyrostową. Oznacza to, że kompilowane powinny być tylko te pliki, które zostały zmienione od ostatniej kompilacji. MSBuild realizuje to zadanie poprzez porównywanie sygnatur czasowych plików zdefiniowanych w atrybutach `Inputs` i `Outputs` danego targetu. Jeżeli wszystkie pliki wyjściowe (`Outputs`) są nowsze niż wszystkie pliki wejściowe (`Inputs`), MSBuild pomija wykonanie danego targetu, oszczędzając czas.

**Wymagania:**
- Wykorzystaj atrybuty `Inputs` i `Outputs` w targetach `Compile` i `Link`.
- Target `Compile` dla danego pliku `.cpp` powinien być uruchamiany tylko wtedy, gdy sam plik `.cpp` lub którykolwiek z plików nagłówkowych (`.h`) w projekcie jest nowszy niż odpowiadający mu plik obiektowy (`.o`).
- Target `Link` powinien być uruchamiany tylko wtedy, gdy którykolwiek z plików obiektowych jest nowszy niż plik wykonywalny.

Wyczyść projekt targetem `Clean`, następnie zbuduj go dwa razy targetem `Build`.

### 3. Tworzenie paczki dystrybucyjnej

Celem jest zautomatyzowanie tworzenia paczki `.zip` zawierającej gotową aplikację oraz dodatkowe pliki.

**Wymagania:**
- Dodaj do projektu nowy target o nazwie `CreateDist`.
- Target `CreateDist` powinien zależeć od targetu `Build`.
- Dodaj właściwość `Version` (np. `1.0.0`), która będzie używana w nazwie paczki.
- Dodaj grupę itemów, w której zdefiniujesz item `DistFiles` zawierający `README.md` i `LICENSE`
- W targecie `CreateDist`:
    - Skopiuj plik wykonywalny oraz pliki dystrybucyjne do tymczasowego katalogu.
    - Użyj taska (`ZipDirectory`)[https://learn.microsoft.com/visualstudio/msbuild/zipdirectory-task], aby spakować zawartość katalogu tymczasowego.
    - Nazwa wynikowego pliku archiwum powinna mieć format `$(OutputName)-$(Version).zip`.
    - Usuń folder tymczasowy

Wywołaj target `CreateDist` dla konfiguracji `Release`.

### Przykładowe rozwiązanie

Przykładowe rozwiązanie można znaleźć w pliku [CppProject.proj](/labs/lab02/solution/CppProject/CppProject.proj).

## Task 2: .NET SDK

> TODO
