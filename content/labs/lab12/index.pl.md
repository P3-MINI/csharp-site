---
title: "Lab12"
weight: 10
---

# Laboratorium 12: Sieć, łącza i mapowanie pamięci


## Sieć

Celem zadania jest stworzenie prostej aplikacji konsolowej, umożliwiającej dwóm użytkownikom wymianę wiadomości przez sieć.

### Kod początkowy

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab12/student/NetworkStreams" >}}

### Opis zadania

#### ChatCommon

Projekt `ChatCommon` zawiera wspólny kod wykorzystywany zarówno przez program serwera, jak i klienta. Zawiera klasę `MessageDTO`, która reprezentuje pojedynczą wiadomość przesyłaną w sieci, oraz folder `MessageHandlers` z klasami odpowiedzialnymi za jej obsługę.

Komunikacja między programami odbywa się według następującego protokołu: każda wiadomość poprzedzona jest 32‑bitowym nagłówkiem — liczbą typu int w zapisie big endian. Nagłówek określa długość wiadomości w bajtach. Bezpośrednio po nim przesyłana jest właściwa treść wiadomości w formacie JSON, zakodowana w UTF‑8.

W ramach tego projektu należy uzupełnić implementacje poniższych metod:

- `ReadMessage` z klasy `MessageReader` – w przypadku błędu odczytu należy zgłosić wyjątek `InvalidMessageReceived` z odpowiednim opisem. Jeśli osiągnięty zostanie koniec strumienia, metoda powinna zwrócić `null`.
- `WriteMessage` z klasy `MessageWriter`.

Do serializacji i deserializacji wiadomości należy wykorzystać bibliotekę `Newtonsoft.Json`.

#### ChatClient

W projekcie `ChatClient` zaimplementuj asynchroniczną metodę `Connect`, która próbuje nawiązać połączenie TCP z serwerem. W przypadku braku połączenia w ciągu trzech sekund metoda powinna zwrócić `null`. Do logowania przebiegu operacji wykorzystaj przekazany obiekt progress.


#### ChatServer

W projekcie `ChatServer` zaimplementuj asynchroniczną metodę `ForwardMessagesAsync`, która aż do zgłoszenia anulowania przez `CancellationToken` odbiera wiadomości od jednego klienta, loguje je na standardowe wyjście, a następnie przekazuje drugiemu klientowi.


{{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: NetworkStream](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=net-9.0)
- [Microsoft Learn: BinaryPrimitives](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.binary.binaryprimitives?view=net-9.0)
- [Microsoft Learn: TcpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-9.0)
- [Newtonsoft.JSON serializacja i deserializacja](https://www.newtonsoft.com/json/help/html/SerializingJSON.htm)

{{% /hint %}}

### Przykładowe rozwiązanie

> [!TIP]
> **Rozwiązanie**
> {{< filetree dir="labs/lab12/solution/NetworkStreams" >}}


## Łącza

{{% hint info %}}
**Bazy danych typu klucz-wartość**

Bazy danych typu klucz-wartość to sposób przechowywania danych, w którym każda informacja zapisywana jest w postaci pary: unikalny klucz oraz odpowiadająca mu wartość.

Zamiast korzystać z rozbudowanych tabel i relacji, dane są organizowane w prostą strukturę przypominającą słownik, co umożliwia bardzo szybkie wyszukiwanie, zapisywanie i odczyt danych na podstawie klucza.

Taki model jest szczególnie przydatny w systemach wymagających dużej wydajności i łatwej skalowalności, na przykład do obsługi pamięci podręcznej, sesji użytkowników czy konfiguracji aplikacji.

{{% /hint %}}

### Kod początkowy

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab12/student/Pipes" >}}

### Opis zadania

Celem zadania jest implementacja prostej bazy danych typu klucz-wartość, która będzie mogła odpowiadać na zapytania z innych procesów działających na tym samym komputerze.

Przyjmujemy następującą składnię zapytań do serwera:
 - utworzenie nowej pary klucz-wartość lub aktualizacja wartości - `SET <key> <value>`, na co serwer w przypadku powodzenia odpowiada `OK`.
 - pobranie wartości - `GET <key>` na co serwer zwraca żądaną wartość lub `NOT_FOUND`, jeśli podany klucz nie istnieje w bazie.
 - usunięcie pary klucz-wartość - `DELETE <key>`, na co serwer odpowiada `OK` przy powodzeniu lub `NOT_FOUND`, jeśli dany klucz nie był obecny.
 - w przypadku nie prawidłowego zapytania serwer wysyła wiadomość `ERROR <msg>`

 Wszystkie wiadomości kodowane są za pomocą UTF-8 i oddzielane od siebie znakiem nowej linii. Z tego powodu żadna z wiadomości nie może zawierać w sobie tego znaku.


#### Client

W projekcie `Client` zaimplementuj następujące fragmenty kodu:
 - w metodzie `Main` utwórz zmienną `client` typu `NamedPipeClientStream` i połącz się z serwerem. Jeśli łączenie będzie trwało więcej niż trzy sekundy - zakończ program z odpowiednim komunikatem.
 - Zaimplementuj metodę `GetResponse`, która wysyła zapytanie do serwer i oczekuje na odpowiedź. W przypadku przerwania połączenia zwracana jest wartość `null`


#### Serwer

W projekcie `Serwer` zaimplementuj następujące fragmenty kodu:
 - W metodzie `Main` utwórz zmienną `server` typu `NamedPipeServerStream` i połącz się z klientem. Łączenie może zostać przerwane przez `CancellationToken`
 - Zaimplementuj metodę `HandleClientAsync`, która w sposób asynchroniczny odczytuje zapytania od klienta i odpowiada na nie. Uwzględnij możliwość przerwania przez `CancellationToken`. Do uzyskania odpowiedzi wykorzystaj metodę `ProcessCommand`.

 {{% hint info %}}
**Materiały pomocnicze:**

- [Microsoft Learn: NamedPipeClientStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeclientstream?view=net-9.0)
- [Microsoft Learn: NamedPipeServerStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream?view=net-9.0)

{{% /hint %}}


### Przykładowe rozwiązanie

> [!TIP]
> **Rozwiązanie**
> {{< filetree dir="labs/lab12/solution/Pipes" >}}
