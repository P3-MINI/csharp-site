---
title: "git"
weight: 10
---

## Tutorial 1 - Git



Przy tworzeniu małych projektów, pracując samodzielnie programiści często zapisywali historię swojego projektu w różnych folderach np. `v.1.0.0`, `v.1.0.1`, `v.1.0.2` itd. Nawet w przypadku takich małych samodzielnych projektów jest to koszmar. Nie da się łatwo sprawdzić, co dokładnie zmieniono między poszczególnymi folderami, a powrót do działającej wersji po zepsuciu czegoś nie jest prosty. 

W przypadku większych projektów takie rozwiązanie jest jednak bardzo niewygodne. Problem staje się jeszcze większy, gdy kilka osób pracuje nad jednym projektem. Dwie osoby mogą jednocześnie zmodyfikować te same pliki, a następnie trzeba ustalić, w jaki sposób połączyć ich zmiany. Ponadto o wiele ciężej przesyłać kod między członkami zespołu - czasem mogłoby to się sprowadzić do przesyłania do siebie kolejnych wersji projektu mailem. 

Oba te problemy zostały rozwiązane poprzez **systemy kontroli wersji** (ang. VCS - Version Control System). Pozwalają one przechowywać historię zmian w projekcie, a także w wygodny sposób współpracować nad projektem.

Jednym z najpopularniejszych systemów kontroli wersji jest **Git**. Pozwala on śledzić zmiany w plikach projektu, zapisywać kolejne etapy jego rozwoju oraz łączyć zmiany wprowadzane przez różnych programistów. Git działa lokalnie na komputerze i nie wymaga do działania GitHuba ani połączenia z Internetem. Nie należy mylić **Gita** z **Githubem**. GitHub to serwis internetowy, który udostępnia interfejs umożliwiający wygodne przechowywanie i współdzielenie repozytoriów Git, a Git to narzędzie służące do kontroli wersji. 

Głównym elementem Gita jest **repozytorium** - baza danych, w której są zapisane różne wersje danego projektu zwane **commitami**, czyli migawki projektu wykonane w określonych momentach. Każdy commit zapisuje stan projektu w danej chwili i można łatwo odtworzyć wcześniejsze wersje.
## Pierwsze kroki z Gitem 

Gita możemy pobrać ze strony: https://git-scm.com

Aby sprawdzić, czy Git jest zainstalowany i w jakiej wersji, używamy polecenia `git --version`.

Następnie musimy ustawić swoją nazwę użytkownika oraz adres e-mail. Możemy tego dokonać za pomocą poleceń: `git config --global user.name <username>` oraz `git config --global user.email <email>`. Dane te są zapisywane przy commitach i pozwalają określić kto jest autorem danej zmiany. 

Jeżeli zastanawiamy się, kto ostatnio zmienił daną linię kodu, możemy użyć komendy `git blame <nazwa_pliku>`. Pokaże to nam dla każdej linii autora, datę oraz commit. Jeśli pojawi się bug, `git blame` pomoże sprawdzić, komu wysłać wiadomość ,,Co tu się wydarzyło?". W praktyce jest to jednak narzędzie do śledzenia historii zmian, a nie do szukania winnych.
### Pomoc w Git

Nie musimy pamiętać wszystkich poleceń Gita ani dostępnych dla nich opcji. Git posiada wbudowany system pomocy, z którego możemy skorzystać bezpośrednio z terminala.

Aby wyświetlić ogólną pomoc oraz listę najczęściej używanych poleceń, możemy użyć komendy `git help`

Jeżeli chcemy uzyskać szczegółowe informacje o konkretnym poleceniu, możemy skorzystać z `git <polecenie> --help`. Na przykład `git commit --help` wyświetli pełną dokumentację polecenia `git commit` wraz z opisem jego działania oraz dostępnych opcji.

Jeżeli potrzebujemy jedynie skróconego opisu opcji danej funkcji, możemy wykorzystać komendę `git <polecenie> -h`.

Wbudowana pomoc jest szczególnie przydatna, gdy nie pamiętamy dokładnej składni polecenia lub chcemy sprawdzić dostępne dla niego opcje.

### Tworzenie repozytorium

Repozytorium w gicie możesz utworzyć za pomocą komendy `git init` w katalogu z plikami projektu, który chcesz dodać do kontroli wersji. Zostanie wówczas utworzony podkatalog `.git`, w którym będą znajdowały się wszystkie informacje potrzebne do kontroli wersji. Usunięcie tego folderu to nieodwracalna utrata historii projektu (choć same pliki robocze pozostaną nienaruszone). Robimy to tylko wtedy, gdy celowo chcemy odpiąć projekt od Gita.

## Pliki 

Pliki naszego projektu mogą znajdować się w jednym z czterech głównych stanów:
- `untracked`
- `modified`
- `staged`
- `committed`

Nowo utworzony plik, który nie został dodany do repozytorium, znajduje się w stanie `untracked`. Oznacza to, że Git widzi plik w katalogu projektu, ale jeszcze go nie śledzi.
Po zmodyfikowaniu plików w katalogu roboczym pliki przechodzą do stanu `modified`. Staging area to poczekalnia. Aby zasygnalizować sztywnemu Gitowi, które konkretnie zmiany chcemy zawrzeć w najbliższym commicie, używamy `git add <nazwa pliku>`. Plik wówczas zmieni swój stan na `staged`. Po dodaniu wszystkich plików możemy utworzyć commita w repozytorium za pomocą komendy `git commit -m <nazwa commita>`. Wówczas pliki zostaną zatwierdzone w commicie. Jeśli chcemy przesłać lokalne commity do zdalnego repozytorium możemy skorzystać z komendy `git push`. 
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
## Zdalne repozytoria 

Jeśli chcielibyśmy współpracować z innymi programistami lub utworzyć kopię zapasową w chmurze możemy wykorzystać **Zdalne repozytorium**. Zazwyczaj będzie on na serwisie hostingowym np. GitHubie. 

### Tworzenie kluczy SSH

Zanim wyślemy kod do zdalnego repozytorium, musimy uwierzytelnić się na serwerze (np. GitHubie).
W tym celu możemy skonfigurować uwierzytelnianie za pomocą SSH. 

Najpierw tworzymy klucz za pomocą komendy:
`ssh-keygen -t ed25519 -C "<twój email>"`,
a następnie zatwierdzamy domyślną lokalizację.

Klucz publiczny wyświetlamy komendą:
`cat ~/.ssh/id_ed25519.pub`

A następnie kopiujemy go na Githuba w zakładce Settings -> SSH and GPG keys -> New SSH Key.

### Praca ze zdalnymi repozytoriami

Często pracę zaczynamy od sklonowania repozytorium za pomocą komendy `git clone <link>`. Klonowanie to pobranie istniejącego repozytorium ze zdalnego serwera na nasz komputer. Kopiowane są nie tylko aktualne pliki projektu, ale również historia zmian zapisana w repozytorium. 

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

## Ignorowanie plików za pomocą `.gitignore`

Czasami twój projekt będzie zawierał pliki, które nie powinny być zapisywane w repozytorium. Mogą to być np. pliki binarne i logi generowane przez program lub dane poufne, których nie chcemy udostępniać innym osobom. 

Listę plików, których nie chcemy dodawać do repozytorium możemy zapisać w pliku `.gitignore`.

Możemy ignorować:
- pojedynczy plik np. `06-07.2026.log`
- cały katalog np. `logs/`
- Czy też wszystkie pliki o określonym rozszerzeniu np. `*.log`

Plik `.gitignore` jest częścią projektu i powinien zostać zcommittowany tak samo jak pozostałe pliki. Dzięki temu każdy programista współtworzący dany projekt będzie korzystał z tych samych zasad ignorowania plików.

W praktyce `.gitignore` jest jednym z pierwszych plików, które warto przygotować podczas tworzenia nowego repozytorium.

>[!warning] Uwaga!
>Dodanie pliku do .gitignore zadziała tylko na pliki, których Git jeszcze NIE śledzi. Jeśli zacommitowałeś plik z hasłami, dodanie go do .gitignore nic nie da.

W praktyce plik `.gitignore` nie zawsze musimy pisać ręcznie. W projektach .NET możemy wygenerować gotowy szablon poleceniem `dotnet new gitignore`. Zawiera on typowe reguły dla projektów C#, więc zazwyczaj od razu nadaje się do użycia. 

## Zadanie 1

1. Utwórz nowe puste repozytorium o nazwie `Task1` na Githubie, a także folder o tej samej nazwie na swoim komputerze.
2. Utwórz w tym katalogu lokalne repozytorium Git, a następnie utwórz puste pliki `README.md`, oraz `Program.cs`.
3. Sprawdź stan repozytorium za pomocą komendy `git status`.
4. Dodaj oba pliki do staging area i sprawdź, jak zmienił się stan plików.
5. Utwórz pierwszy commit z wiadomością `Initial commit`. 
6. Połącz lokalne repozytorium z repozytorium utworzonym wcześniej na Githubie. Adres repozytorium możesz znaleźć na stronie repozytorium w zakładce Quick Setup, a po spushowaniu czegokolwiek pojawi się w zakładce Code.
7. Otwórz repozytorium i sprawdź czy pojawiły się w nim pliki.
8. W pliku `Program.cs` zapisz dowolny fragment kodu np. pętlę wypisującą liczby od 1 do 10. 
9. Utwórz nowy commit i prześlij zmiany do repozytorium na Githubie. Sprawdź czy plik uległ zmianie.
10. Utwórz w projekcie katalog `logs`, a w nim kilka plików np. `application.log` `err.log` `niedotykac.txt`
11. W głównym katalogu projektu utwórz plik `notes.tmp`
12. Utwórz plik `.gitignore`. Skonfiguruj go tak, aby Git ignorował katalog `logs`, oraz wszystkie pliki z rozszerzeniem, `.tmp`.
13. Dodaj plik do staging area i utwórz commit `Add gitignore`.
14. Zmodyfikuj plik `README.md` i dodaj wszystkie pliki do staging area za pomocą `git add .`, a następnie sprawdź status. Czy ignorowane pliki się tam pojawiają? Utwórz commit i prześlij go na Githuba.
15. Wyświetl historię wykonanych commitów za pomocą: `git log --oneline`.
16. Na koniec sprawdź jakie pliki są w repozytorium na Githubie. 

## Branche 

Podczas pracy nad projektem często chcemy rozwijać nową funkcjonalność bez zmieniania jego głównej, działającej wersji. Git umożliwia w tym celu tworzenie osobnych **branchy**, na których możemy niezależnie wprowadzać zmiany.

Możemy wyobrazić sobie historię projektu jako pewną "drogę". Utworzenie brancha powoduje powstanie nowej odnogi tej drogi. Od tego momentu na obu gałęziach mogą powstawać niezależne commity. Główna gałąź projektu zazwyczaj nazywa się `main` lub `master`.

Na przykład:

```
A---B---C  main
     \
      D---E  feature
```

Tutaj `feature` został utworzony na podstawie commita B i rozwija się niezależnie od `main`.

Za pomocą komendy `git branch` możemy wyświetlić listę wszystkich lokalnych gałęzi. Przy gałęzi, na której obecnie jesteśmy wyświetli się `*`.

Jeżeli chcemy rozpocząć pracę nad nową funkcjonalnością, możemy utworzyć dla niej osobny branch za pomocą komendy `git branch <nazwa>`.

Na nowego brancha możemy się przełączyć za pomocą komendy `git switch <nazwa>`.  

Aby przesłać commita na konkretnego brancha na Githubie korzystamy z `git push -u origin <nazwa_brancha>`. Możemy jednocześnie utworzyć nowego brancha i się na niego przełączyć korzystając z polecenia `git switch -c <nazwa>`.

### Łączenie gałęzi

Załóżmy, że na gałęzi `login` zakończyliśmy implementację logowania. Jeżeli chcemy, aby ta funkcjonalność znalazła się również na głównej gałęzi `main`, musimy połączyć obie gałęzie. 

Komenda `git merge <nazwa gałęzi>` dołącza zmiany z gałęzi, której nazwę wpisaliśmy do gałęzi na której obecnie się znajdujemy. Na przykład jeśli będąc na gałęzi main wykonamy polecenie `git merge login` to wszystkie zmiany z gałęzi login zostaną dołączone do gałęzi main. Przed scaleniem branchy warto sprawdzić, na której gałęzi obecnie się znajdujemy oraz czy zcommitowaliśmy wszystkie zmiany.

### Konflikty 

Jeżeli na dwóch branchach zmieniano różne fragmenty projektu, Git połączy te gałęzie bez problemu. Jednakże w sytuacjach, gdy doszło do zmian tej samej linii kodu lub np. jedna gałąź modyfikuje pewien fragment kodu, a druga go usuwa dochodzi do **konfliktu**.

Git w pliku, w którym doszło do konfliktu umieści znaczniki np.:
```
<<<<<<< HEAD 
Console.WriteLine("Witaj!"); 
======= 
Console.WriteLine("Zalogowano!"); 
>>>>>>> login
```

Fragment pod `HEAD` pochodzi z aktualnej gałęzi, `=======` oddziela obie wersje, a poniżej znajduje się zmiana pochodząca z dołączanego brancha.

Aby rozwiązać konflikt, należy ręcznie edytować plik i zdecydować, która wersja ma pozostać. Możemy zachować wersję z dowolnego z tych branchy, albo napisać zupełnie nową wersję. 

Pełny proces może wyglądać np. tak:
```
git merge <branch> 
↓ 
konflikt 
↓ 
poprawiamy plik 
↓ 
git add <plik> 
↓ 
git commit
```

Komenda `git merge --abort` przerywa obecny merge i przywraca repozytorium do stanu sprzed rozpoczęcia operacji. Jest to szczególnie przydatne, gdy konfliktów jest dużo i chcemy wrócić o wcześniejszego stanu.

## Zadanie 2

1. Utwórz nowe puste repozytorium o nazwie `Task2` na GitHubie oraz folder o tej samej nazwie na swoim komputerze.
2. Utwórz w tym katalogu lokalne repozytorium Git.
3. Utwórz plik `README.md` o zawartości:
```md
# Task2

Projekt służący do nauki pracy z branchami w Git.
```
4. Utwórz plik `config.txt` o zawartości:
```text
Nazwa aplikacji: MiniApp
Wersja: 1.0
Język: polski
Tryb: standardowy
```
5. Dodaj oba pliki do staging area i utwórz commit:
```text
Initial commit
```
6. Połącz lokalne repozytorium z pustym repozytorium utworzonym wcześniej na GitHubie i prześlij pierwszy commit.
7. Utwórz nową gałąź:
```text
feature-description
```
i przełącz się na nią.
8. W pliku `README.md` dopisz:
```md
## Opis

MiniApp jest przykładową aplikacją wykorzystywaną podczas nauki Gita.
```
9. Utwórz commit  o treści:
```text
Add project description
```
 i prześlij go na Githuba na gałąź feature-description. 
10. Przełącz się na `main` i sprawdź zawartość pliku `README.md`.
	Czy sekcja dodana na `feature-description` są tutaj widoczne?
11. Połącz z branchem `main` gałąź `feature-description`.
12. Sprawdź historię repozytorium poleceniem:
```bash
git log --oneline --graph --all
```
13. Utwórz nową gałąź:
```text
feature-config
```
14. Na gałęzi `main` zmień w `config.txt`:
```text
Nazwa aplikacji: MiniApp
```
na:
```text
Nazwa aplikacji: MiniGit
```
i utwórz commit:
```text
Rename application
```
15. Przełącz się na `feature-config` i zmień tę samą linię na:
```text
Nazwa aplikacji: GitApp
```
16. Utwórz commit:
```text
Change application name
```
17. Wróć na `main` i spróbuj połączyć z nim gałąź:
```text
feature-config
```
18. Sprawdź stan repozytorium za pomocą:
```bash
git status
```
19. Otwórz `config.txt` i znajdź konflikt.
20. Rozwiąż konflikt tak, aby końcowa linia miała postać:
```text
Nazwa aplikacji: MiniGitApp
```
21. Dodaj poprawiony plik do staging area i zakończ merge.
22. Wyświetl historię wszystkich gałęzi:
```bash
git log --oneline --graph --all
```
23. Prześlij końcową wersję projektu na GitHuba.
24. Sprawdź na GitHubie, czy pliki `README.md` oraz `config.txt` zawierają wszystkie wykonane zmiany.

## Unfucking the repo

Błędy chodzą po ludziach - nie każda zmiana w projekcie okazuje się dobrym pomysłem. Na szczęście Git udostępnia kilka narzędzi pozwalających naprawić feralne zmiany.

### Napisałem coś złego i chcę odzyskać plik

Załóżmy, że zmieniliśmy plik, ale uznaliśmy, że poprzednia wersja była jednak lepsza. Jeżeli zmian jeszcze nie zapisaliśmy w commicie możemy wrócić do wersji z poprzedniego commita za pomocą komendy `git restore <plik>`.

### Przypadkiem zrobiłem git add

Czasami wykonamy `git add .` i dopiero wtedy zauważymy, że nie wszystkie pliki powinny znaleźć się w tym commicie. Możemy usunąć plik ze stageing area bez usuwania zmian w pliku za pomocą `git restore --staged <plik>`

### Commit jest dobry, tylko oczywiście zapomniałem jednego pliku

Jeżeli ostatni commit wymaga niewielkiej poprawki, nie zawsze musimy tworzyć commity `fix 1`, `fix 2`, `fix final`, `fix final ostateczny` itp. W przypadku mniejszych błędów takich jak np. literówka we wiadomości commita lub nie dodanie jednego z plików możemy skorzystać z polecenia`git commit --amend`. Należy pamiętać, że to nie edytuje istniejącego commita, a zastępuje go nowym. Nie powinniśmy w ten sposób poprawiać commitów, które zostały już wypchnięte i z których korzystają inne osoby.

### Commit był za wcześnie 

Jeśli nie chcieliśmy jeszcze robić commita, ale nasz kod jest dobry, jednak brakuje jeszcze kawałka kodu możemy za pomocą polecenia `git reset --soft HEAD~1` cofnąć ostatni commit, pozostawiając jego zmiany w staging area.

### Zepsułem wszystko

Jeśli nasz kod do niczego się nie nadaje i chcemy się cofnąć do poprzedniego commita możemy skorzystać z komendy `git reset --hard HEAD`. Polecenie to usuwa lokalne zmiany w śledzonych plikach oraz zmiany znajdujące się w staging area. Pliki `untracked` nie zostaną przez tę komendę usunięte. 

### Wypchnąłem coś złego i ktoś zdążył to już pobrać.

Załóżmy, że historia commitów wygląda następująco:

```
A---B---C
```

Jeżeli `C` zawiera błąd, polecenie `git revert C` utworzy nowy commit `D`, który odwróci działanie commita `C`.


## Zadanie 3.

Utwórz nowe repozytorium `Task3` i połącz je z pustym repozytorium na GitHubie.

Utwórz plik `notes.txt` o zawartości:

```
Pierwsza wersja pliku.
```

Dodaj go do repozytorium i utwórz commit `Initial commit`.

Następnie wykonuj po kolei poniższe sytuacje.

1. Zmień zawartość `notes.txt` na:
```
Ta zmiana była fatalnym pomysłem.
```
Nie twórz commita. Przywróć poprzednią wersję pliku.
2. Utwórz pliki:
```
important.txt
temporary.txt
```
Dodaj oba do staging area, ale następnie usuń `temporary.txt` ze staging area **bez usuwania samego pliku ani jego zawartości**.
3. Dodaj `important.txt` do repozytorium i utwórz commit:
```
Ad imortant file
```
Popraw literówkę w wiadomości ostatniego commita tak, aby brzmiała:
```
Add important file
```
4. Zmień `notes.txt` i utwórz nowy commit. Następnie cofnij ten commit w taki sposób, aby wykonane zmiany pozostały w staging area.
5. Utwórz ponownie commit z tymi zmianami.
6. Zmodyfikuj kilka śledzonych plików, ale nie twórz commita. Przywróć całe repozytorium do stanu ostatniego commita.
7. Prześlij aktualną historię na GitHuba.
8. Utwórz kolejny commit zmieniający `notes.txt` i również prześlij go na GitHuba.
9. Załóż, że commit z poprzedniego punktu zawiera poważny błąd i został już pobrany przez innych członków zespołu. Cofnij jego zmiany **bez usuwania go z historii repozytorium**.
10. Prześlij wynik na GitHuba i wyświetl historię:

```
git log --oneline
```

Materiały dodatkowe:
https://www.youtube.com/watch?v=8JJ101D3knE
