---
title: "Projekt"
weight: 25
---

# Projekt

W ramach zaliczenia możemy realizować dowolny projekt. Liczba punktów zależy od jakości i włożonej pracy.

Poniżej przykładowe projekty:

## 1. Raytracer (Raytracing in One Weekend)

**Opis:** Implementujemy pierwszą książkę "Raytracing in One Weekend" w C#.

**Ważne:** Ograniczamy alokacje pamięci przy renderowaniu (wybór: klasa czy struktura).

**Mile widziane:** Zrównoleglenie pętli, zapis do innego formatu, implementacja dalszych części książki.

**Punkty:** ok. 4

## 2. CityBike - Analiza danych LINQ

**Opis:** Pobieramy dane (np. z https://citibikenyc.com/system-data), wczytujemy je i wykonujemy min. 5 zapytań LINQ.

**Mile widziane:** Łączymy dane z wypożyczalni z danymi pogodowymi.

**Punkty:** ok. 4 (zależnie od skomplikowania zapytań)

## 3. Program do kopii zapasowych

**Opis:** Realizujemy wymagania projektu z SOP, ale zamiast procesów używamy wątków, zadań lub programowania asynchronicznego.

**Punkty:** ok. 4

## 4. Program do segregowania zdjęć

**Opis:** Monitorujemy folder (również rekursywnie i w archiwach ZIP/TAR). Nowe zdjęcia segregujemy względem daty i miejsca. Czytamy EXIF, pobieramy nazwę lokalizacji z koordynatów (np. Nominatim) i zapisujemy w strukturze "rok/miesiąc/dzień/" i "kraj/miejscowość".

**Punkty:** ok. 4

## 5. Gra sieciowa

**Opis:** Tworzymy grę sieciową dowolnego gatunku.

**Punkty:** 4-12 (np. 4 pkt za turową grę konsolową, 8 pkt za klona agar.io w 2D)

**Mile widziane:** Użycie biblioteki do renderowania grafiki 2D/3D, silnika fizyki.

## 6. Gra w Godot lub Unity

**Opis:** Tworzymy grę w wybranym silniku, z C# jako językiem skryptowym.

**Punkty:** Zależnie od zaawansowania 4-8

## 7. Raytracer + P/Invoke

**Opis:** Do Raytracera w C++ (można użyć gotowej implementacji) dodajemy API C. W C# tworzymy odpowiedniki struktur i importujemy funkcje przez P/Invoke. Scenę definiujemy po stronie C# i wywołujemy renderowanie przekazując callback do postępu (postępem może być np. render dla X próbek).

**Punkty:** ok. 8 (ale w ramach planowego projektu)
