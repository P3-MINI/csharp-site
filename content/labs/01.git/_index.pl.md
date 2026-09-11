---
title: "git"
weight: 10
---

## Tutorial 1 - Git


Przy tworzeniu małych projektów, gdy pracujemy samodzielnie, możemy zapisywać kolejne etapy w różnych folderach np. `v.1.0.0`, `v.1.0.1`, `v.1.0.2` itd.

W przypadku większych projektów  takie rozwiązanie jest jednak bardzo niewygodne. Problem staje się jeszcze większy, gdy kilka osób pracuje nad jednym projektem. Dwie osoby mogą jednocześnie zmodyfikować te same pliki, a następnie trzeba ustalić, w jaki sposób połączyć ich zmiany. Ponadto o wiele ciężej przesyłać kod między członkami zespołu - czasem mogłoby to się sprowadzić do przesyłania do siebie kolejnych wersji projektu mailem. 

Oba te problemy zostały rozwiązane poprzez **systemy kontroli wersji** (ang. VCS - Version Control System). Pozwalają one przechowywać historię zmian w projekcie, a także w wygodny sposób współpracować nad projektem.

Jednym z najpopularniejszych systemów kontroli wersji jest **Git**. Pozwala on śledzić zmiany w plikach projektu, zapisywać kolejne etapy jego rozwoju oraz łączyć zmiany wprowadzane przez różnych programistów. Git działa lokalnie na komputerze i nie wymaga do działania GitHuba ani połączenia z Internetem. Nie możemy go utożsamiać z **Githubem**, ponieważ GitHub to serwis internetowy, który udostępnia interfejs umożliwiający wygodne przechowywanie i współdzielenie repozytoriów Git, a Git to narzędzie służące do kontroli wersji. 

Głównym elementem Gita jest **repozytorium** - baza danych, w której są zapisane różne wersje danego projektu zwane **commitami**. Każdy commit reprezentuje konkretny etap pracy nad projektem i pozwala łatwo wrócić do wcześniejszych wersji.

## Pierwsze kroki z Gitem 

(Czy tłumaczyć instalację gita?)

Na początku powinniśmy upewnić sie czy Git jest w ogóle zainstalowany możemy tego dokonać za pomocą komendy `git --version`. W ten sposób możemy się upewnić, jaka wersja Gita jest zainstalowana. 

Następnie musimy ustawić swoją nazwę użytkownika oraz adres e-mail. Możemy tego dokonać za pomocą poleceń: `git config --global user.name <username>` oraz `git config --global user.email <email>`. Dane te są zapisywane przy commitach i pozwalają określić kto jest autorem danej zmiany. 


### Tworzenie repozytorium

Repozytorium w gicie możesz utworzyć za pomocą komendy `git init` w katalogu z plikami projektu, który chcesz dodać do kontroli wersji. Zostanie wówczas utworzony podkatalog `.git`, w którym będą znajdowały się wszystkie informacje potrzebne do kontroli wersji. Z tego powodu nie możemy nigdy usunąć folderu `.git`, bo stracilibyśmy dostęp do gita. Usunięcie katalogu `.git` nie usunie plików projektu, ale spowoduje utratę lokalnej historii repozytorium oraz informacji potrzebnych Gitowi do jego obsługi.


### Pliki 

Pliki naszego projektu mogą znajdować się w jednym z trzech głównych stanów:
- `modified`
- `staged`
- `committed`

Po zmodyfikowaniu plików w katalogu roboczym pliki przechodzą do stanu `modified`. Aby pokazać sztywnemu gitowi, że chcemy przygotować dany plik do committa wykorzystujemy komendę `git add <nazwa pliku>`. Plik wówczas zmieni swój stan na `staged`. Po dodaniu wszystkich plików możemy utworzyć commita w repozytorium za pomocą komendy `git commit -m <nazwa commita>`. Wówczas pliki zostaną zatwierdzone w commicie. Jeśli chcemy przesłać lokalne commity do zdalnego repozytorium możemy skorzystać z komendy `git push`. 


### Zdalne repozytoria 

Jeśli chcielibyśmy współpracować z innymi programistami lub utworzyć kopię zapasową poza naszą lokalną maszyną możemy wykorzystać **Zdalne repozytorium**. Zazwyczaj będzie on na serwisie hostingowym np. GitHubie. 

#### Tworzenie kluczy SSH

Zanim będziemy mogli wygodnie wysyłać kod do zdalnych repozytoriów, musimy pozwolić serwerowi z usługą zdalnych repozytoriów rozpoznać nasz komputer.
W tym celu możemy skonfigurować uwierzytelnianie za pomocą SSH. 

Najpierw tworzymy klucz za pomocą komendy:
`ssh-keygen -t ed25519 -C "<twój email>"`,
a następnie zatwierdzamy domyślną lokalizację.

Klucz publiczny wyświetlamy komendą:
`cat ~/.ssh/id_ed25519.pub`

A następnie kopiujemy go na Githuba w zakładce Settings -> SSH and GPG keys -> New SSH Key

#### Praca ze zdalnymi repozytoriami

Często pracę zaczynamy od sklonowania repozytorium za pomocą komendy `git clone <link>`. Klonowanie polega na utworzeniu lokalnej kopii istniejącego repozytorium Git na naszym komputerze. Kopiowane są nie tylko aktualne pliki projektu, ale również historia zmian zapisana w repozytorium. 

Jeśli jednak mamy już projekt na komputerze i chcemy od zera zacząć śledzić jego zmiany za pomocą Gita, możemy utworzyć w jego folderze nowe repozytorium poleceniem `git init`

Następnie możemy dodać pliki do repozytorium i utworzyć pierwszy commit za pomocą komend:

```
git add .
git commit -m "Initial commit"
```

Komenda `git add .` dodaje do staging area wszystkie zmiany w plikach w obecnym katalogu, oraz w jego podkatalogach.

Jeżeli chcemy dodatkowo umieścić projekt w zdalnym repozytorium, np. na GitHubie, należy najpierw utworzyć tam puste repozytorium, a następnie połączyć je z lokalnym projektem:

```
git remote add origin <link>
git branch -M main
git push -u origin main
```

Od tego momentu lokalne repozytorium jest połączone ze zdalnym i kolejne zmiany możemy przesyłać za pomocą polecenia `git push`.

### Sprawdzanie stanu repozytorium

Podczas pracy z Gitem bardzo często korzystamy z polecenia `git status`.

Polecenie `git status` pokazuje aktualny stan naszego repozytorium. Dzięki niemu możemy sprawdzić między innymi:

- które pliki zostały zmodyfikowane,
- które pliki nie są jeszcze śledzone przez Gita,
- które zmiany zostały przygotowane do następnego commita,
- na jakiej gałęzi aktualnie pracujemy.

Przykładowo załóżmy, że utworzyliśmy nowy plik `program.py`. Po wykonaniu komendy `git status` Git może poinformować nas, że plik znajduje się w sekcji:

```text
Untracked files:
    program.py
```

Oznacza to, że Git widzi plik, ale jeszcze go nie śledzi.

Możemy przygotować go do następnego commita za pomocą `git add program.py`
Po ponownym wyświetleniu statusu plik powinien znaleźć się w sekcji:

```text
Changes to be committed:
    new file: program.py
```

Oznacza to, że zmiany znajdują się już w **staging area** i zostaną zapisane przy następnym commicie.


`git status` warto wykonywać bardzo często, szczególnie przed użyciem `git add` oraz `git commit`. Pozwala to upewnić się, jakie zmiany znajdują się obecnie w repozytorium i które z nich zostaną zapisane w kolejnym commicie.

### Wyświetlanie historii commitów - `git log`

Każdy utworzony commit zostaje zapisany w historii repozytorium. Historię tę możemy wyświetlić za pomocą polecenia `git log`

Dla każdego commita Git wyświetla między innymi:

- jego identyfikator,
- autora,
- datę utworzenia,
- wiadomość przypisaną do commita.

Przykładowy fragment wyniku może wyglądać następująco:

```text
commit 12ab34cd56ef...
Author: Jan Kowalski <jan@example.com>
Date:   Mon Sep 7 15:20:00 2026 +0200

    Dodanie obsługi logowania
```

Przy większej liczbie commitów pełny wynik `git log` może być mało czytelny. Możemy wtedy użyć `git log --oneline`

Wówczas każdy commit zostanie przedstawiony w jednym wierszu, na przykład:

```text
a53f761 Dodanie obsługi logowania
87bc120 Naprawa błędu w formularzu
19ab452 Initial commit
```

Pierwsza część każdego wiersza to skrócony identyfikator commita, a druga to jego wiadomość.

`git log` jest szczególnie przydatny, gdy chcemy:

- sprawdzić, jakie zmiany były wcześniej wykonywane w projekcie,
- znaleźć konkretny commit,
- sprawdzić kolejność commitów,
- odnaleźć identyfikator wcześniejszej wersji projektu.

### Ignorowanie plików za pomocą `.gitignore`

Czasami twój projekt będzie zawierał pliki, które nie powinny być zapisywane w repozytorium. Mogą to być np. pliki binarne i logi generowane przez program lub dane poufne, których nie chcemy udostępniać innym osobom. 

Listę plików, których nie chcemy dodawać do repozytorium możemy zapisać w pliku `.gitignore`.

Możemy ignorować:
- pojedynczy plik np. `06-07.2026.log`
- cały katalog np. `logs/`
- Czy też wszystkie pliki o określonym rozszerzeniu np. `*.log`

Plik `.gitignore` jest częścią projektu i powinien zostać zcommittowany tak samo jak pozostałe pliki. Dzięki temu każdy programista współtworzący dany projekt będzie korzystał z tych samych zasad ignorowania plików.

W praktyce `.gitignore` jest jednym z pierwszych plików, które warto przygotować podczas tworzenia nowego repozytorium.

### Zadanie 1

1. Utwórz nowe puste repozytorium o nazwie `Task1` na Githubie, a także folder o tej samej nazwie na swoim komputerze.
2. Utwórz w tym katalogu lokalne repozytorium Git, a następnie utwórz puste pliki `README.txt`, oraz `Program.cs`.
3. Sprawdź stan repozytorium za pomocą komendy `git status`.
4. Dodaj oba pliki do staging area i sprawdź, jak zmienił się stan plików.
5. Utwórz pierwszy commit z wiadomością `Initial commit`. 
6. Połącz lokalne repozytorium z repozytorium utworzonym wcześniej na Githubie. Adres repozytorium możesz znaleźć tutaj:
7. Otwórz repozytorium i sprawdź czy pojawiły się w nim pliki.
8. W pliku program.cs zapisz dowolny fragment kodu np. pętlę wypisującą liczby od 1 do 10. 
9. Utwórz nowy commit i prześlij zmiany do repozytorium na Githubie. Sprawdź czy plik uległ zmianie.
10. Utwórz w projekcie katalog `logs`, a w nim kilka plików np. `application.log` `err.log` `niedotykac.txt`
11. W głównym katalogu projektu utwórz plik `notes.tmp`
12. Utwórz plik `.gitignore`. Skonfiguruj go tak, aby Git ignorował katalog `logs`, oraz wszystkie pliki z rozszerzenie, `.tmp`.
13. Dodaj plik do staging area i utwórz commit `Add gitignore`.
14. Zmoyfikuj plik `README.txt` i dodaj wszystkie pliki do staging area za pomocą `git add .`, a następnie sprawdź status. Czy ignorowane pliki się tam pojawiają? Utwórz commit i prześlij go na Githuba
15. Wyświetl historię wykonanych commitów za pomocą: `git log --oneline`.
16. Na koniec sprawdź jakie pliki są w repozytorium na Githubie. 

### Branche 

Materiały dodatkowe:
https://www.youtube.com/watch?v=8JJ101D3knE