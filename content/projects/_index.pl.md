---
title: "Projekt"
weight: 25
---

# Projekt

W ramach zaliczenia można realizować dowolny projekt w C#. Liczba punktów zależy od jakości i włożonej pracy.

> [!IMPORTANT]
> **Zasady** (skrót z [regulaminu]({{< ref "/regulations" >}})):
> - Projekty są opcjonalne i nie ma limitu punktów za nie.
> - Każdy projekt trzeba wykonać samodzielnie.
> - Obrona projektu jest obowiązkowa. Niezaliczenie obrony skutkuje karą -10 punktów nałożoną na wszystkie projekty.

> [!TIP]
> - Temat i zakres projektu uzgodnij z prowadzącym laboratorium, zanim zaczniesz. Podane niżej punkty są orientacyjne - ostateczna liczba zależy od jakości i włożonej pracy.
> - Trzymaj projekt w repozytorium git od samego początku i regularnie twórz commity.
> - Przygotuj się do obrony: trzeba umieć wyjaśnić dowolny fragment swojego kodu i stojące za nim decyzje projektowe.

Poniżej proponowane projekty:

## 1. Raytracer (Raytracing in One Weekend)

**Opis:** Implementujemy raytracer na CPU na podstawie pierwszej książki *"Ray Tracing in One Weekend"* Petera Shirleya (dostępnej bezpłatnie pod adresem <https://raytracing.github.io/books/RayTracingInOneWeekend.html>) w C#: promienie, sfery i obiekty trafialne (`Hittable`), konfigurowalna kamera (z głębią ostrości / rozmyciem ogniskowym), materiały rozpraszające (lambertowskie), metaliczne i dielektryczne (szkło) oraz wyrenderowanie końcowej sceny z wieloma kulami do pliku graficznego (np. PPM lub PNG).

**Ważne:** Ograniczamy alokacje pamięci przy renderowaniu: świadomie decydujemy, które typy powinny być bezpośrednie (`struct` / `readonly struct`), a które referencyjne (`class`), unikamy zbędnych alokacji w wewnętrznej pętli przecięć promieni i weryfikujemy alokacje profilerem lub w BenchmarkDotNet.

**Mile widziane:** Zrównoleglenie pętli renderującej (`Parallel.For` po liniach lub kafelkach z lokalnymi dla wątku generatorami liczb losowych), zapis do PNG/JPEG (np. z użyciem ImageSharp lub SkiaSharp), okno podglądu na żywo, hierarchia brył ograniczających (BVH) lub dalsze rozdziały z drugiej książki (*The Next Week*).

**Punkty:** ok. 4

## 2. CityBike - Analiza danych LINQ

**Opis:** Pobieramy rzeczywiste zbiory danych (np. o przejazdach z <https://citibikenyc.com/system-data> lub dowolną inną publiczną bazę danych), parsujemy i wczytujemy dane do silnie typowanych rekordów w C#, a następnie wykonujemy co najmniej 5 nietrywialnych zapytań LINQ odpowiadających na ciekawe pytania analityczne (np. dla Citi Bike: najpopularniejsze stacje i trasy, godziny szczytu w dni robocze vs. weekendy, rozkład czasu przejazdów, zachowania użytkowników abonamentowych vs. okazjonalnych czy wykorzystanie typów rowerów). Wyniki prezentujemy czytelnie w konsoli (np. za pomocą tabel/wykresów Spectre.Console) lub eksportujemy do pliku.

**Ważne:** Efektywnie obsługujemy duże pliki CSV strumieniowo (`StreamReader` / `CsvHelper` lub `IAsyncEnumerable<T>`) zamiast wczytywania ogromnych plików naraz jako surowych napisów; poprawnie obsługujemy brakujące lub błędne pola; wykorzystujemy czyste zapytania LINQ (grupowanie, złączenia, agregacje) zamiast ręcznych pętli imperatywnych.

**Mile widziane:** Połączenie danych z wypożyczalni z historycznymi danymi pogodowymi (np. temperatura, opady) w celu analizy wpływu pogody na liczbę przejazdów, porównanie wielu miesięcy/lat, interaktywne filtry w CLI albo porównanie wydajności zwykłego LINQ i PLINQ.

**Punkty:** ok. 4 (zależnie od skomplikowania zapytań)

## 3. Program do kopii zapasowych

**Opis:** Tworzymy narzędzie do ciągłego tworzenia kopii zapasowych i synchronizacji katalogów wzorowane na klasycznym projekcie z przedmiotu SOP (Systemy Operacyjne), ale zaimplementowane w C# z użyciem wątków, zadań (`Task`) lub programowania asynchronicznego (`async`/`await`) zamiast procesów systemowych (`fork`). Program zarządza jedną lub wieloma parami katalogów (katalog źródłowy -> katalog docelowy): wykonuje początkową kopię, monitoruje katalog źródłowy pod kątem zmian (tworzenie, modyfikacja, usuwanie, zmiana nazwy plików i podkatalogów) za pomocą `FileSystemWatcher` i odzwierciedla je w kopii. Obsługuje również przywracanie kopii do katalogu źródłowego, listowanie aktywnych zadań oraz czyste zamykanie po naciśnięciu Ctrl+C (`Console.CancelKeyPress` / `CancellationToken`).

**Ważne:** Nie blokujemy wątków aktywnym czekaniem ani odpytywaniem w pętli z `Thread.Sleep`; poprawnie obsługujemy specyfikę `FileSystemWatcher` (przepełnienie bufora, zduplikowane lub szybkie serie zdarzeń z użyciem debouncingu, zablokowane pliki); bezpiecznie obsługujemy podkatalogi i dowiązania symboliczne bez nieskończonej rekurencji; chronimy współdzielony stan (aktywne kopie, operacje na plikach) przed wyścigami.

**Mile widziane:** Synchronizacja przyrostowa oparta na sumach kontrolnych (kopiowanie tylko zmienionych plików), wersjonowane migawki lub kompresja (ZIP/TAR), ustrukturyzowane logowanie, czytelny interfejs konsolowy (np. Spectre.Console) pokazujący aktywne zadania i postęp albo testy jednostkowe/integracyjne dla przypadków brzegowych.

**Punkty:** ok. 4

## 4. Program do segregowania zdjęć

**Opis:** Program monitorujący wskazany folder (rekursywnie, również wewnątrz archiwów `.zip` i `.tar`) w poszukiwaniu nowych zdjęć i automatycznie porządkujący je według daty oraz lokalizacji geograficznej. Odczytuje metadane EXIF (datę wykonania i współrzędne GPS, np. za pomocą `MetadataExtractor`), wykonuje odwrotne geokodowanie przez zewnętrzne API (np. OpenStreetMap Nominatim), aby zamienić współrzędne na kraj i miejscowość, a następnie zapisuje/kopiuje zdjęcia w strukturze katalogów `"rok/miesiąc/dzień/"` oraz `"kraj/miejscowość/"`.

**Ważne:** Respektujemy limity zapytań zewnętrznego API (Nominatim wymaga własnego nagłówka `User-Agent` i maksymalnie 1 żądania na sekundę) oraz lokalnie buforujemy wyniki geokodowania, aby pobliskie zdjęcia nie wysyłały zbędnych zapytań; używamy jednej współdzielonej instancji `HttpClient` z `async`/`await`; poprawnie obsługujemy zdjęcia bez daty EXIF lub współrzędnych GPS (np. fallback do daty modyfikacji pliku lub katalogu `"Nieznane"`) oraz unikamy wielokrotnego przetwarzania i kolizji nazw.

**Mile widziane:** Deduplikacja plików po sumie kontrolnej (SHA-256), tryb `dry-run` pokazujący planowane operacje bez modyfikacji dysku, użycie dowiązań twardych/symbolicznych zamiast duplikowania plików między dwiema strukturami, obsługa formatów RAW lub plik konfiguracyjny.

**Punkty:** ok. 4

## 5. Gra sieciowa

**Opis:** Tworzymy wieloosobową grę sieciową dowolnego gatunku z serwerem i klientami komunikującymi się przez sieć (TCP, UDP lub WebSockets/SignalR). Przykłady to od turowej gry konsolowej (szachy, statki, gra karciana) po grę 2D czasu rzeczywistego (np. klon *agar.io* lub *Snake*). Projektujemy czytelny protokół komunikacyjny (ramkowanie wiadomości, serializacja, synchronizacja stanu gry, lobby/dobór graczy oraz obsługa rozłączeń).

**Ważne:** Asynchroniczna komunikacja sieciowa (`async`/`await`, bez blokowania osobnego wątku na każdego klienta); wyraźne rozdzielenie logiki/symulacji gry od sieci i renderowania; walidacja stanu po stronie serwera (server-authoritative), aby klienci nie mogli łatwo oszukiwać; bezpieczna obsługa częściowych odczytów TCP, nagłych rozłączeń klientów i limitów czasu bez wywracania serwera.

**Mile widziane:** Użycie biblioteki do renderowania grafiki 2D/3D lub silnika fizyki (np. Raylib-cs, MonoGame, Silk.NET, Avalonia), predykcja po stronie klienta / interpolacja w grach czasu rzeczywistego, tryb obserwatora, czat w grze lub testy automatyczne protokołu i logiki gry.

**Punkty:** 4-12 (np. 4 pkt za turową grę konsolową, 8 pkt za klona *agar.io* w 2D, do 12 pkt za bardziej zaawansowane gry 2D/3D czasu rzeczywistego)

## 6. Gra w Godot lub Unity

**Opis:** Tworzymy kompletną, grywalną grę w silniku Godot (4.x z .NET/C#) lub Unity z C# jako językiem skryptowym. Gra powinna mieć wyraźną pętlę rozgrywki, menu/interfejs (ekran startowy, pauza, ekran wygranej/przegranej), oprawę audiowizualną oraz co najmniej jedną nietrywialną mechanikę (np. proceduralne generowanie poziomów, AI/wyszukiwanie ścieżek przeciwników, zagadki fizyczne, system ekwipunku/craftingu lub zapis i odczyt stanu gry).

**Ważne:** Projekt jest oceniany przede wszystkim za **jakość i architekturę kodu w C#**, a nie same zasoby graficzne: oddzielamy logikę gry od węzłów silnika / `MonoBehaviour`, gdzie to możliwe; czysto używamy zdarzeń/sygnałów C#; unikamy alokacji w każdej klatce (`Update`/`_Process`) powodujących skoki GC oraz utrzymujemy porządek w skryptach bez gigantycznych klas typu „god class”.

**Mile widziane:** Testy jednostkowe dla czystej logiki gry w C# (poza pętlą silnika), własne narzędzia/inspektory w edytorze, efekty shaderowe, generowanie proceduralne lub tryb wieloosobowy lokalny/sieciowy.

**Punkty:** 4-8 (zależnie od zaawansowania i jakości kodu C#)

## 7. Raytracer + P/Invoke

**Opis:** Implementujemy wysokowydajną warstwę komunikacji między natywnym silnikiem ray tracingu w C++ ("Ray Tracing in One Weekend", z progresywnym rendererem w `camera.h`) a zarządzaną aplikacją w C#, wykorzystując dostarczony kod startowy i gotową bibliotekę okienkową `Windowing` opartą na Avalonia UI:

1. **Warstwa eksportu natywnego (C++, `RayTracing/native/export.cpp`) [1 pkt]:** Tworzymy ABI zgodne z C z nieprzezroczystymi uchwytami (`CreateScene`, `CreateMaterial` itp.) i odpowiadającymi funkcjami `Destroy...`; implementujemy funkcję `RenderScene`, która przyjmuje strukturę `CameraConfig` przez wartość, tworzy lokalny obiekt `camera` wypełniony danymi konfiguracyjnymi i wywołuje jego metodę `render`, przekazując świat sceny, wskaźnik do bufora wyjściowego RGBA oraz wskaźnik na funkcję zwrotną (`typedef void (*RenderCallback)(int samples, uint8_t* buffer);`); udostępniamy funkcję `SavePng` korzystającą z `stb_image_write` do zapisu bufora RGBA do pliku.
2. **Niskopoziomowe wiązania (C#, `RayTracing/NativeMethods.cs`) [1 pkt]:** Wiążemy wyeksportowane funkcje C przez P/Invoke (zalecane `[LibraryImport]`), implementujemy klasy `SceneSafeHandle` i `MaterialSafeHandle` (pochodne `SafeHandle` automatycznie wywołujące natywne funkcje `Destroy...`), definiujemy delegat `RenderCallback` zgodny z sygnaturą w C++ oraz obsługujemy marshaling ścieżki pliku w `SavePng`.
3. **Wysokopoziomowe bezpieczne API (C#, `RayTracing/*.cs`) [1 pkt]:** Tworzymy idiomatyczny, obiektowy wrapper w C# implementujący wzorzec `IDisposable` do zarządzania czasem życia natywnych uchwytów, który nigdy nie ujawnia `IntPtr`/`nint` ani wskaźników w publicznym API.
4. **Demo i integracja (`RayTracingDemo/Program.cs`) [1 pkt]:** Generujemy proceduralnie kultową scenę z okładki pierwszej książki, uruchamiamy okno podglądu przez `Windowing.Viewer.Show`, aktualizujemy w czasie rzeczywistym tekst statusu okna (liczba próbek/czas) oraz bufor obrazu na podstawie danych z callbacku, a po zakończeniu zapisujemy finalny obraz jako `output.png` za pomocą `SavePng`.

**Ważne:** Nie ujawniamy układu klas C++, typów `IntPtr`/`nint` ani surowych wskaźników w publicznym API C#; dbamy o deterministyczne zwalnianie każdego zasobu natywnego przez `SafeHandle` i `IDisposable`; utrzymujemy referencję do delegatu `RenderCallback` przez cały czas trwania `RenderScene`, aby GC nie zebrał go w trakcie działania kodu natywnego. Projekt wykorzystuje CMake zintegrowany z MSBuild (`dotnet build -c Release` oraz `dotnet run -c Release --project RayTracingDemo/RayTracingDemo.csproj`).

> [!NOTE]
> **Kod początkowy:**
> {{< filetree dir="projects/project01/student" >}}

**Punkty:** 4 (po 1 punkcie za każdą z 4 części)

## 8. Interpreter prostego języka (Crafting Interpreters)

**Opis:** Implementujemy interpreter języka Lox oparty na przechodzeniu drzewa składniowego z części II książki "Crafting Interpreters" Roberta Nystroma (dostępnej bezpłatnie pod adresem <https://craftinginterpreters.com>): skaner, parser rekurencyjnie zstępujący, drzewo składniowe (AST), ewaluator i REPL. Interpreter uruchamia programy w Loxie z plików i zgłasza błędy z numerami linii.

**Ważne:** Nie tłumaczymy kodu Javy z książki linijka po linijce - korzystamy z idiomów C#: AST jako hierarchia rekordów, ewaluacja za pomocą wyrażeń `switch` i dopasowywania wzorców zamiast wzorca Odwiedzający (lub obie wersje, dla porównania), włączone nullowalne typy referencyjne bez ostrzeżeń.

**Mile widziane:** Funkcje natywne napisane w C# i rejestrowane za pomocą atrybutów i refleksji, zestaw testów uruchamiających przykładowe programy w Loxie i porównujących ich wyjście z oczekiwanym, maszyna wirtualna z kodem bajtowym z części III książki.

**Punkty:** ok. 6 (rozdziały 4-10, do funkcji i domknięć włącznie), ok. 8 (cała część II, łącznie z klasami i dziedziczeniem)

## 9. Mini framework do testów jednostkowych

**Opis:** Piszemy prosty framework do testów jednostkowych (w duchu NUnit lub xUnit) i konsolowy program uruchamiający testy. Framework udostępnia atrybuty (`[Test]`, `[TestCase(...)]` z argumentami, `[SetUp]`/`[TearDown]`, `[Ignore("reason")]`) oraz klasę `Assert` (`Equal`, `True`, `Throws<TException>`, ...), której komunikaty o błędach zawierają sprawdzane wyrażenie (`[CallerArgumentExpression]`). Program uruchamiający przyjmuje ścieżkę do skompilowanego assembly z testami, ładuje je do osobnego `AssemblyLoadContext`, wyszukuje testy za pomocą refleksji, dla każdego testu tworzy nową instancję klasy testowej, uruchamia testy synchroniczne i asynchroniczne (`async Task`) oraz wypisuje podsumowanie: testy zaliczone/niezaliczone/pominięte, czasy wykonania, komunikaty błędów ze śladem stosu.

**Ważne:** Test, który rzuca wyjątek lub się zawiesza, nie może wywrócić ani zablokować programu uruchamiającego (rozpakowujemy `TargetInvocationException`, stosujemy limity czasu). Uwaga na tożsamość typów: assembly frameworka musi być współdzielone przez program uruchamiający i załadowane testy - w przeciwnym razie `TestAttribute` widziany przez program uruchamiający i ten użyty w testach mogą być dwoma różnymi typami.

**Mile widziane:** Równoległe uruchamianie klas testowych, filtrowanie testów po nazwie lub kategorii z linii poleceń, raport w formacie JUnit XML, tryb obserwowania (ponowne uruchamianie testów po rekompilacji i zwalnianie poprzedniego kontekstu, utworzonego z `isCollectible: true`).

**Punkty:** ok. 6

## 10. Współbieżny crawler stron WWW

**Opis:** Tworzymy narzędzie konsolowe, które zaczynając od podanego adresu URL przegląda strony jednej witryny (do zadanej głębokości lub liczby stron), wyciąga z nich linki (np. za pomocą AngleSharp lub HtmlAgilityPack) i generuje raport: niedziałające linki (odpowiedzi 4xx/5xx), przekierowania, najwolniejsze strony oraz graf linków (np. w formacie Graphviz DOT). Pracę organizujemy jako potok etapów (pobieranie, parsowanie, zbieranie wyników) połączonych ograniczonymi kolejkami `Channel<T>`, z konfigurowalną liczbą równoczesnych żądań i anulowaniem przez Ctrl+C (częściowy raport i tak zostaje zapisany).

**Ważne:** Asynchroniczne operacje wejścia-wyjścia z jedną współdzieloną instancją `HttpClient`, bez wywołań blokujących (`.Result`, `.Wait()`); każdy URL odwiedzamy co najwyżej raz, nawet gdy wiele zadań pobiera strony współbieżnie; respektujemy `robots.txt` i ograniczamy częstotliwość żądań wysyłanych do każdego hosta. Testujemy na witrynie, którą kontrolujemy (np. na lokalnym serwerze), a nie na cudzych serwerach.

**Mile widziane:** Ponawianie żądań z wykładniczym wydłużaniem odstępów (exponential backoff), statystyki postępu na żywo, zapis kopii witryny do przeglądania offline, generowanie `sitemap.xml`, udostępnianie wyników jako `IAsyncEnumerable<T>`.

**Punkty:** ok. 4-6 (zależnie od raportu i potoku)

## 11. Symulacja N ciał z SIMD

**Opis:** Symulujemy oddziaływanie grawitacyjne wielu ciał (od tysięcy do setek tysięcy), np. zderzenie dwóch galaktyk. Zaczynamy od prostej, skalarnej wersji O(n^2) i optymalizujemy ją krok po kroku: układ danych (tablica struktur czy struktura tablic), wektoryzacja za pomocą `System.Numerics` (`Vector<T>`, `Vector3`) lub `System.Runtime.Intrinsics` (`Vector128<T>`, `Vector256<T>`) oraz zrównoleglenie (`Parallel.For` z podziałem na zakresy). Symulację wizualizujemy w czasie rzeczywistym (np. za pomocą Raylib-cs) albo renderujemy klatki do plików graficznych.

**Ważne:** Każdą optymalizację mierzymy za pomocą BenchmarkDotNet i przedstawiamy wyniki (przyspieszenie względem wersji skalarnej, skalowanie z liczbą rdzeni). Poprawność sprawdzamy, porównując wyniki z wersją skalarną (z dopuszczalną tolerancją) i monitorując całkowitą energię układu. Brak alokacji w głównej pętli symulacji.

**Mile widziane:** Algorytm Barnesa-Huta (O(n log n)), wersja na GPU (np. compute shadery OpenGL przez Silk.NET), interaktywna kamera i parametry symulacji.

**Punkty:** ok. 6 (do 8 z algorytmem Barnesa-Huta i wizualizacją w czasie rzeczywistym)

## 12. Analizator zajętości dysku w Avalonii

**Opis:** Tworzymy aplikację desktopową (Avalonia UI, wzorzec MVVM), która pokazuje, co zajmuje miejsce w wybranym katalogu: drzewo katalogów i plików posortowane według rozmiaru (z procentowym udziałem w katalogu nadrzędnym) oraz mapę drzewa (treemap) - prostokąty o polach proporcjonalnych do rozmiarów, rysowane przez własną kontrolkę. Skanowanie odbywa się w tle: interfejs pozostaje responsywny, pokazuje postęp (liczbę plików, przeskanowane bajty), a skanowanie można anulować. Zaznaczenie elementu w drzewie wyróżnia go na mapie drzewa i odwrotnie.

**Ważne:** Rozdzielenie widoku i logiki: modele widoku nie odwołują się do kontrolek, a widok komunikuje się z nimi wyłącznie przez wiązanie danych (data binding) i komendy (np. `[ObservableProperty]` i `[RelayCommand]` z CommunityToolkit.Mvvm); interfejs aktualizujemy tylko z wątku UI (`IProgress<T>`, `Dispatcher.UIThread`); obsługujemy niedostępne katalogi i dowiązania symboliczne (bez nieskończonych pętli i podwójnego liczenia).

**Mile widziane:** Wyszukiwanie duplikatów plików (grupowanie po rozmiarze, potem haszowanie), statystyki według typu pliku (LINQ), równoległe skanowanie, usuwanie plików z potwierdzeniem, testy jednostkowe modeli widoku.

**Punkty:** ok. 6

## 13. Czat w sieci lokalnej z przesyłaniem plików

**Opis:** Tworzymy czat peer-to-peer dla sieci lokalnej, bez centralnego serwera. Węzły wykrywają się nawzajem przez rozgłaszanie UDP (broadcast lub multicast) - okresowe ogłoszenia z nazwą użytkownika i portem TCP; węzły, które przestają się ogłaszać, znikają z listy. Wiadomości i pliki przesyłamy przez TCP z użyciem własnego protokołu ramkowania (długość wiadomości + typ + treść serializowana np. za pomocą `System.Text.Json`). Wymagane funkcje: wiadomości do pojedynczego węzła i do wszystkich, wysyłanie plików z raportowaniem postępu i weryfikacją integralności (SHA-256), kilka transferów jednocześnie, anulowanie transferu oraz interfejs konsolowy (np. Spectre.Console) lub graficzny.

**Ważne:** Asynchroniczna komunikacja sieciowa (`async`/`await`, bez wątku blokowanego na każde połączenie); TCP to strumień bajtów - obsługujemy częściowe odczyty i kilka wiadomości w jednym odczycie; duże pliki przesyłamy strumieniowo, nigdy nie wczytując ich w całości do pamięci; rozłączenie się węzła nie wywraca aplikacji.

**Mile widziane:** Wznawianie przerwanych transferów, szyfrowanie (np. `SslStream`), lokalnie zapisywana historia wiadomości, interfejs graficzny w Avalonii.

**Punkty:** ok. 6

## 14. Serwer klucz-wartość zgodny z Redisem

**Opis:** Tworzymy serwer bazy danych klucz-wartość działający w pamięci, który obsługuje podzbiór protokołu Redisa (RESP), dzięki czemu może się z nim połączyć standardowy klient `redis-cli`. Wymagane polecenia: `PING`, `ECHO`, `SET` (z wygasaniem `EX`/`PX`), `GET`, `DEL`, `EXISTS`, `INCR`, `EXPIRE`, `TTL`, `KEYS` ze wzorcem glob oraz polecenia na listach `LPUSH`, `RPUSH`, `LPOP`, `LRANGE`. Serwer obsługuje wielu klientów współbieżnie (asynchroniczne TCP), wspiera pipelining (wiele poleceń wysłanych naraz), usuwa wygasłe klucze zarówno leniwie, jak i okresowo, a dane utrwala w pliku typu append-only, odtwarzanym przy starcie.

**Ważne:** Sednem projektu jest bezpieczeństwo wątkowe magazynu danych - wybieramy strategię synchronizacji (jedna blokada, `ReaderWriterLockSlim`, podział blokad na segmenty (lock striping), `ConcurrentDictionary` albo jeden wątek wykonujący polecenia z `Channel<T>`, tak jak robi to sam Redis), uzasadniamy ją i testujemy pod obciążeniem (np. `redis-benchmark -t set,get,incr`). Operacje takie jak `INCR` muszą być atomowe. Parser musi obsługiwać polecenia podzielone między kilka odczytów oraz kilka poleceń w jednym odczycie.

**Mile widziane:** `PUBLISH`/`SUBSCRIBE`, transakcje (`MULTI`/`EXEC`), blokujące polecenie `BLPOP` z limitem czasu, parsowanie za pomocą `System.IO.Pipelines`, kompaktowanie pliku append-only w tle.

**Punkty:** ok. 8

## 15. Bezpieczna nakładka .NET na SQLite

**Opis:** Tworzymy bibliotekę opakowującą API C SQLite (natywną bibliotekę `sqlite3`) bez użycia gotowych wiązań (Microsoft.Data.Sqlite, SQLitePCLRaw). Warstwa niskopoziomowa deklaruje funkcje natywne (`sqlite3_open_v2`, `sqlite3_prepare_v2`, `sqlite3_bind_*`, `sqlite3_step`, `sqlite3_column_*`, `sqlite3_finalize`, `sqlite3_close_v2`, `sqlite3_errmsg`, ...) za pomocą `[LibraryImport]` i reprezentuje połączenia oraz przygotowane zapytania jako klasy pochodne `SafeHandle`. Warstwa wysokopoziomowa to idiomatyczne API w C#: połączenia i zapytania implementujące `IDisposable`, wiązanie parametrów, transakcje, wyjątki niosące komunikat błędu SQLite, odczyt wierszy (np. mapowanych na rekordy). Bibliotekę prezentujemy w małej aplikacji (np. menedżer zadań albo import pliku CSV i wykonywanie na nim zapytań).

**Ważne:** Publiczne API nie udostępnia wskaźników ani `nint`; napisy przekazujemy jako UTF-8; poprawnie obsługujemy wartości `NULL` i BLOB (`ReadOnlySpan<byte>`); każdy zasób natywny jest zwalniany, także gdy zostanie rzucony wyjątek. Na Linuksie SQLite jest zwykle dostępne jako systemowa biblioteka `libsqlite3`; na Windowsie skompilowany plik `sqlite3.dll` można pobrać ze strony <https://sqlite.org/download.html>.

**Mile widziane:** Funkcje SQL definiowane przez użytkownika napisane w C# (`sqlite3_create_function_v2` z callbackami `[UnmanagedCallersOnly]`), mapowanie wierszy na obiekty za pomocą refleksji lub generatora źródeł, benchmark w porównaniu z Microsoft.Data.Sqlite. Po uzgodnieniu z prowadzącym można zamiast tego opakować inną bibliotekę C (np. zlib z API opartym na `Stream`).

**Punkty:** ok. 6

## 16. Generator źródeł dla powtarzalnego kodu

**Opis:** Tworzymy przyrostowy generator źródeł (`IIncrementalGenerator`), który dla typów oznaczonych atrybutami generuje co najmniej dwie z poniższych rzeczy: (a) dla typów wyliczeniowych: `ToStringFast()`, `TryParse`, `GetValues()` i `IsDefined` zaimplementowane za pomocą `switch` zamiast refleksji (również dla wyliczeń `[Flags]`); (b) implementację `INotifyPropertyChanged` dla częściowych właściwości oznaczonych `[Notify]`, łącznie z powiadomieniami o właściwościach zależnych; (c) metody mapujące między dwoma typami (np. encją i DTO), z diagnostyką dla właściwości, których nie da się zmapować. Dołączamy projekt demonstracyjny, testy jednostkowe generatora (uruchamianie go przez `CSharpGeneratorDriver` i sprawdzanie wygenerowanego kodu oraz diagnostyk, np. jako testy migawkowe (snapshot) z Verify.SourceGenerators) oraz benchmark (BenchmarkDotNet) porównujący wygenerowany kod z odpowiednikiem opartym na refleksji.

**Ważne:** Generator zgłasza czytelne diagnostyki (np. gdy typ nie jest `partial`) zamiast generować kod, który się nie kompiluje. Potok jest rzeczywiście przyrostowy: modele przekazywane między jego etapami są porównywalne (nie przechowują `ISymbol` ani `SyntaxNode`, kolekcje są porównywane po wartościach). Wygenerowany kod kompiluje się bez ostrzeżeń przy włączonych nullowalnych typach referencyjnych.

**Mile widziane:** Analizator z poprawką kodu (code fix, np. "dodaj modyfikator `partial`"), test sprawdzający, że niezmienione wejście jest obsługiwane z pamięci podręcznej generatora, obsługa typów zagnieżdżonych i generycznych, publikacja generatora jako pakietu NuGet.

**Punkty:** ok. 6

## 17. Narzędzie konsolowe kompilowane z Native AOT

**Opis:** Tworzymy przydatne narzędzie konsolowe publikowane jako pojedynczy natywny plik wykonywalny za pomocą Native AOT (`<PublishAot>true</PublishAot>`), np. analizator logów serwera WWW (najczęstsze adresy klientów, kody odpowiedzi, liczba żądań na minutę), narzędzie do badania zajętości dysku w stylu `du`/`tree`, konwerter CSV do JSON albo proste narzędzie do odpytywania JSON-a w stylu `jq`. Wymagane: podpolecenia, opcje i `--help` (za pomocą biblioteki System.CommandLine lub własnego parsera), czytanie z plików lub ze standardowego wejścia (tak aby narzędzie działało w potokach powłoki), sensowne kody wyjścia, błędy wypisywane na standardowe wyjście błędów, obsługa JSON-a za pomocą generatora źródeł (`JsonSerializerContext`).

**Ważne:** Publikacja nie może zgłaszać żadnych ostrzeżeń dotyczących przycinania (trimming) ani AOT (IL2xxx/IL3xxx) - kod nie może polegać na nieograniczonej refleksji. Porównujemy czas uruchamiania, rozmiar pliku wykonywalnego i zużycie pamięci ze zwykłą wersją (JIT) i przedstawiamy wyniki na obronie. Native AOT wymaga narzędzi C/C++ dla danej platformy (zob. wymagania wstępne w dokumentacji .NET).

**Mile widziane:** Strumieniowe przetwarzanie dużych plików z niewielką liczbą alokacji (`Span<T>`, `ArrayPool<T>`, `System.IO.Pipelines`), zbudowanie części funkcjonalności jako natywnej biblioteki współdzielonej (`<NativeLib>Shared</NativeLib>`) z funkcjami eksportowanymi przez `[UnmanagedCallersOnly(EntryPoint = "...")]` i wywołanie jej z C lub Pythona.

**Punkty:** ok. 4
