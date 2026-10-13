---
title: "Sieć, serializacja"
weight: 90
---

## Sieć, serializacja
Czasy, gdy komputery były używane samodzielnie, w izolacji od innych użądzeń dawno już przeminęły. Dzisiejszy świat polega na szybkiej komunikacji, realizowanej głównie przez internet. Ważne jest, by każdy przyszły inżynier programista rozumiał działanie komunikacji sieciowej oraz umiał ją wykorzystać w tworzonym przez siebie oprogramowaniu. Celem dzisiejszego warsztatu jest wprowadzenie tematu komunikacji sieciowej.

### Protokoły TCP i UDP
UDP (User Datagram Protocol) jest najprostszym, popularnie używanym do komunikacji sieciowej protokołem transportowym. Pozwala on na wysyłanie atomowych wiadomości, bez
zapewnienia potwierdzenia ich dostarczenia, oraz bez zarządzania prędkością transmisji. Charakteryzuje się szybkością i łatwością użycia. Jest dobrym wyborem przy przesyłaniu danych (np. wideo) na żywo, oraz przy prostej komunikacji, nie wymagającej wbudowanych mechanizmów dbania o dostarczenie i integralność wiadomości.

TCP (Transmission Control Protocol) jest protokołem połączeniowym (wymagającym zawarcia tymczasowego połączenia pomiędzy oboma stronami) ma on wbudowane mechanizmy gwarantujące dostarczenie danych, ułożenie ich w kolejności nadania oraz zarządzanie transmisją przy ograniczonej przepustowości łącza.

Adres Ip - w uproszczeniu jest to adres przypisywany indywidualnie do komputera w sieci. Składa się z czterech bajtów i jest zapisywany w postaci czterech liczb w zakresie 0-255 podzielonych kropkami np. 192.168.0.1

Port - jest to dodatkowa liczba pozwalająca na przypisanie danego pakietu danych do konkretnej aplikacji lub wątku. Kiedy aplikacja otwiera nowe połączenie, używa do tego innego portu niż inne aplikacje. Razem z adresem Ip port tworzy gniazdo lub inaczej endpoint. Gniazdo jest przypisane do danego komputera, aplikacji i wątku.