---
title: "Udp"
weight: 45
---

# Udp

Protokół `UDP` (*User Datagram Protocol*) to drugi z podstawowych protokołów warstwy transportu. W przeciwieństwie do [TCP]({{< ref "/lectures/10.ipc/tcp" >}}) jest to protokół **bezpołączeniowy**: nie nawiązuje się połączenia, a każda wiadomość – **datagram** – jest wysyłana niezależnie na podany adres i port. UDP dodaje bardzo niewiele do samego protokołu IP – jedynie 8-bajtowy nagłówek z portem źródłowym, portem docelowym, długością i sumą kontrolną. Dzięki temu jest prosty i szybki, ale wszystkie gwarancje pozostawia aplikacji:

- **Granice wiadomości są zachowane** – jedna operacja wysłania tworzy jeden datagram, a jedna operacja odbioru zwraca dokładnie jeden cały datagram. W przeciwieństwie do strumienia bajtów TCP nie trzeba oznaczać granic wiadomości (*framing*).
- **Brak gwarancji dostarczenia** – datagram może zaginąć, a nadawca nie zostanie o tym powiadomiony.
- **Brak gwarancji kolejności** – datagramy mogą dotrzeć w innej kolejności, niż zostały wysłane.
- **Brak ochrony przed duplikatami** – ten sam datagram może zostać dostarczony więcej niż raz.
- **Brak kontroli przepływu i przeciążenia** – nadawca może wysyłać dane tak szybko, jak chce. Jeśli sieć lub odbiorca nie nadążają, datagramy są odrzucane (np. gdy bufor gniazda odbiorcy jest pełny).

Datagram z niepoprawną sumą kontrolną jest po cichu odrzucany, co z punktu widzenia aplikacji oznacza po prostu kolejny zagubiony datagram.

> [!NOTE]
> Maksymalny rozmiar danych w datagramie to **65 507 bajtów** w IPv4 (65 535 bajtów pakietu IP minus 20 bajtów nagłówka IP i 8 bajtów nagłówka UDP). Próba wysłania większego datagramu kończy się wyjątkiem `SocketException` (`SocketError.MessageSize`). W praktyce datagramy powinny być znacznie mniejsze: datagram większy niż MTU (*Maximum Transmission Unit*) ścieżki sieciowej jest dzielony na fragmenty IP, a utrata dowolnego fragmentu oznacza utratę całego datagramu (niektóre sieci w ogóle odrzucają fragmenty). W sieci Ethernet, z MTU równym 1500 bajtów, oznacza to co najwyżej **1472 bajty** danych na datagram – a w internecie bezpieczniej jest pozostać jeszcze poniżej tej wartości.

## Zastosowania

UDP stosuje się tam, gdzie małe opóźnienia są ważniejsze niż niezawodność, gdzie wiadomości są małe i niezależne od siebie albo gdzie aplikacja implementuje własne mechanizmy niezawodności:

- **DNS** – zapytanie i odpowiedź zwykle mieszczą się w pojedynczym datagramie. Jeśli odpowiedź nie nadejdzie, klient po prostu ponawia zapytanie.
- **VoIP, rozmowy wideo, transmisje na żywo** – retransmitowany fragment dźwięku lub obrazu i tak dotarłby za późno, żeby go odtworzyć, więc lepiej go pominąć.
- **Gry sieciowe** – liczy się tylko najnowszy stan gry; zagubiona, nieaktualna już aktualizacja pozycji nie musi być retransmitowana.
- **Wykrywanie usług w sieci lokalnej** – rozgłaszanie (*broadcast*) i rozsyłanie grupowe (*multicast*), wykorzystywane np. przez DHCP czy mDNS, są dostępne tylko w UDP.
- **QUIC** i zbudowany na nim **HTTP/3** – QUIC implementuje niezawodność, kolejność, kontrolę przeciążenia i szyfrowanie w przestrzeni użytkownika, na bazie UDP. Pozwala to uniknąć blokowania czoła kolejki (*head-of-line blocking*) w TCP (w HTTP/2 jeden zagubiony segment TCP wstrzymuje wszystkie żądania multipleksowane w połączeniu) i rozwijać protokół bez zmian w systemach operacyjnych i urządzeniach sieciowych. W .NET `HttpClient` obsługuje HTTP/3, a przestrzeń nazw `System.Net.Quic` udostępnia sam protokół QUIC (oba wymagają natywnej biblioteki `msquic`).

## TCP a UDP

|                                | TCP                                                  | UDP                                                  |
|--------------------------------|------------------------------------------------------|------------------------------------------------------|
| Połączenie                     | połączeniowy (nawiązanie połączenia, potem strumień) | bezpołączeniowy                                      |
| Jednostka danych               | strumień bajtów, bez granic wiadomości               | datagram, granice zachowane                          |
| Dostarczenie                   | gwarantowane (potwierdzenia, retransmisje)           | niegwarantowane                                      |
| Kolejność                      | gwarantowana                                         | niegwarantowana                                      |
| Duplikaty                      | eliminowane                                          | możliwe                                              |
| Kontrola przepływu/przeciążenia | tak                                                 | nie                                                  |
| Rozmiar nagłówka               | 20–60 bajtów                                         | 8 bajtów                                             |
| Rozgłaszanie/multicast         | nie                                                  | tak                                                  |
| Klasy .NET                     | `TcpListener`, `TcpClient`, `NetworkStream`          | `UdpClient`                                          |
| Typowe zastosowania            | HTTP/1.1, HTTP/2, SMTP, SSH, przesyłanie plików      | DNS, VoIP, gry sieciowe, wykrywanie usług, QUIC      |

## UdpClient

W .NET komunikacja UDP jest obsługiwana przez klasę `UdpClient` z przestrzeni nazw `System.Net.Sockets`. Ponieważ nie ma połączeń, ta sama klasa jest używana zarówno po stronie serwera, jak i klienta – nie ma ani obiektu nasłuchującego, ani akceptowania klientów:

- Konstruktor `new UdpClient(port)` lub `new UdpClient(new IPEndPoint(address, port))` wiąże gniazdo z lokalnym portem. Potrzebuje tego serwer, bo klienci muszą wiedzieć, dokąd wysyłać żądania. `new UdpClient()` nie wiąże gniazda; system operacyjny przydziela wolny (efemeryczny) port przy wysłaniu pierwszego datagramu.
- `SendAsync(datagram, endPoint)` wysyła pojedynczy datagram do podanego punktu końcowego.
- `ReceiveAsync()` i `ReceiveAsync(CancellationToken)` czekają na pojedynczy datagram i zwracają `UdpReceiveResult` z danymi (`Buffer` – tablica `byte[]` zawierająca dokładnie odebrany datagram) oraz adresem nadawcy (`RemoteEndPoint`), na który można odesłać odpowiedź.
- `Connect(address, port)` – wbrew nazwie niczego nie wysyła. Ustawia jedynie domyślny zdalny punkt końcowy: od tej pory datagramy wysyła się metodą `SendAsync(datagram)` bez podawania adresu (podanie adresu zgłasza wyjątek `InvalidOperationException`), a system operacyjny odrzuca datagramy przychodzące z innych adresów.
- `Client` zwraca gniazdo `Socket`, na którym operuje `UdpClient`, np. żeby ustawić opcje gniazda (`Client.ReceiveBufferSize`) albo odczytać lokalny punkt końcowy (`Client.LocalEndPoint`).

## Serwer

Przykład implementuje ten sam prosty "protokół" co na stronie o TCP – klient wysyła tekstowe komendy (`date`, `time`), a serwer odsyła odpowiednią wiadomość. Warto porównać serwer z serwerem TCP: nie ma `AcceptTcpClientAsync`, osobnego zadania dla każdego klienta ani strumienia – **jedno gniazdo obsługuje wszystkich klientów**. Każdy odebrany datagram to kompletna komenda, a serwer odpowiada, wysyłając datagram do jej nadawcy (`RemoteEndPoint`). Niepotrzebna jest też komenda `exit`: nie ma połączenia do zamknięcia, więc klient po prostu przestaje wysyłać. Warto też zauważyć, że porty TCP i UDP są od siebie niezależne – ten serwer może używać portu 5000, nawet gdy działa serwer TCP.

Interfejs pętli zwrotnej (*loopback*) praktycznie nigdy nie gubi datagramów, więc żeby zademonstrować zagubiony datagram na jednej maszynie, serwer celowo ignoruje część żądań (`LossRate`).

```csharp
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

public static class Program
{
    const int Port = 5000;
    const double LossRate = 0.25; // Probability of a simulated request loss

    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        using var udp = new UdpClient(new IPEndPoint(IPAddress.Any, Port));

        var waitForAnyKey = Task.Run(() =>
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
            cts.Cancel();
        });

        await HandleRequests(udp, cts.Token);
        await waitForAnyKey;
    }

    private static async Task HandleRequests(UdpClient udp, CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult request = await udp.ReceiveAsync(token);
                string command = Encoding.UTF8.GetString(request.Buffer);
                Console.WriteLine($"Received command: {command} from {request.RemoteEndPoint}");

                if (Random.Shared.NextDouble() < LossRate)
                {
                    Console.WriteLine("Request lost (simulated)");
                    continue;
                }

                string response = command switch
                {
                    "date" => DateTime.Now.ToString("d", CultureInfo.InvariantCulture),
                    "time" => DateTime.Now.ToString("t", CultureInfo.InvariantCulture),
                    _ => "Unknown command"
                };
                await udp.SendAsync(Encoding.UTF8.GetBytes(response), request.RemoteEndPoint, token);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, which is expected during shutdown.
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Windows: a previous response was rejected by its recipient (ICMP port unreachable).
            }
        }
    }
}
```

## Klient

Klient wywołuje `Connect`, żeby ustawić serwer jako domyślny zdalny punkt końcowy, a następnie wczytuje komendy z konsoli i wysyła każdą z nich jako pojedynczy datagram. Ponieważ zagubić może się zarówno żądanie, jak i odpowiedź, klient nie może czekać na odpowiedź w nieskończoność. `ReceiveAsync` otrzymuje `CancellationToken` anulowany po jednej sekundzie (`CancelAfter`); po przekroczeniu limitu czasu żądanie jest wysyłane ponownie, maksymalnie trzy razy.

> [!WARNING]
> Opcja gniazda `Client.ReceiveTimeout` dotyczy tylko synchronicznej metody `Receive`. Operacje asynchroniczne trzeba ograniczać za pomocą `CancellationToken`.

```csharp
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

public static class Program
{
    const int Port = 5000;
    const int MaxAttempts = 3;
    static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(1);

    public static async Task Main()
    {
        using var udp = new UdpClient();
        udp.Connect(IPAddress.Loopback, Port); // Only sets the default remote endpoint, nothing is sent
        Console.WriteLine($"Client bound to {udp.Client.LocalEndPoint}");

        while (Console.ReadLine() is { } line && line != "exit")
        {
            string? response = await SendRequest(udp, line);
            Console.WriteLine(response ?? "No response from the server");
        }
    }

    private static async Task<string?> SendRequest(UdpClient udp, string command)
    {
        byte[] request = Encoding.UTF8.GetBytes(command);
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            await udp.SendAsync(request);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(ResponseTimeout);
            try
            {
                UdpReceiveResult result = await udp.ReceiveAsync(cts.Token);
                return Encoding.UTF8.GetString(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Timeout (attempt {attempt}/{MaxAttempts})");
            }
            catch (SocketException e) when (e.SocketErrorCode is SocketError.ConnectionRefused or SocketError.ConnectionReset)
            {
                // ICMP "port unreachable" - nothing listens on the server port
                Console.WriteLine($"Server unreachable (attempt {attempt}/{MaxAttempts})");
                await Task.Delay(ResponseTimeout);
            }
        }
        return null;
    }
}
```

Przykładowa sesja (wejście użytkownika przeplata się z wyjściem programu), w której jedno żądanie zaginęło:

```text
Client bound to 127.0.0.1:34853
date
10/29/2026
time
Timeout (attempt 1/3)
18:40
```

Gdy serwer nie działa, klient zwykle szybko się o tym dowiaduje: host serwera odpowiada komunikatem ICMP *port unreachable*, który połączone gniazdo UDP zgłasza jako wyjątek `SocketException` (`ConnectionRefused` na Linuxie, `ConnectionReset` na Windowsie). Gdy jednak datagram zaginie gdzieś w sieci, nikt nie zostaje o tym powiadomiony – pomóc może tylko limit czasu.

## Rozgłaszanie i multicast

UDP pozwala wysłać pojedynczy datagram do wielu odbiorców naraz. Datagram **rozgłoszeniowy** (*broadcast*) wysłany na adres `255.255.255.255` (`IPAddress.Broadcast`) jest dostarczany do wszystkich hostów w sieci lokalnej – routery nie przekazują go dalej. Przydaje się to do wykrywania usług, których adres nie jest znany z góry. Wysyłanie datagramów rozgłoszeniowych trzeba włączyć właściwością `EnableBroadcast` (`UdpClient` ustawia ją automatycznie tylko dla `IPAddress.Broadcast`; w przypadku zwykłego `Socket` lub adresu rozgłoszeniowego podsieci, takiego jak `192.168.0.255`, wysłanie bez niej się nie powiedzie).

```csharp
// Server: answer discovery requests
using var udp = new UdpClient(DiscoveryPort);
while (true)
{
    UdpReceiveResult request = await udp.ReceiveAsync();
    if (Encoding.UTF8.GetString(request.Buffer) == "DISCOVER")
        await udp.SendAsync(Encoding.UTF8.GetBytes("HERE"), request.RemoteEndPoint);
}
```

```csharp
// Client: ask all hosts in the local network
using var udp = new UdpClient();
udp.EnableBroadcast = true;
var broadcast = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
await udp.SendAsync(Encoding.UTF8.GetBytes("DISCOVER"), broadcast);

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
try
{
    while (true) // Collect responses until the timeout
    {
        UdpReceiveResult response = await udp.ReceiveAsync(cts.Token);
        Console.WriteLine($"Server found at {response.RemoteEndPoint.Address}");
    }
}
catch (OperationCanceledException) { }
```

**Multicast** (rozsyłanie grupowe) dostarcza datagram tylko do hostów, które dołączyły do danej grupy, identyfikowanej adresem z zakresu `224.0.0.0`–`239.255.255.255` (zakres `239.0.0.0/8` jest przeznaczony do użytku wewnątrz organizacji). Odbiorca dołącza do grupy metodą `JoinMulticastGroup` (a opuszcza ją metodą `DropMulticastGroup`); nadawca nie musi należeć do grupy. Z multicastu korzystają np. mDNS (`224.0.0.251`) i SSDP (`239.255.255.250`) do wykrywania urządzeń w sieci lokalnej. IPv6 w ogóle nie ma rozgłaszania – zamiast niego używa się multicastu.

```csharp
var group = IPAddress.Parse("239.0.0.1");

// Receiver: join the group
using var receiver = new UdpClient(Port);
receiver.JoinMulticastGroup(group);
UdpReceiveResult message = await receiver.ReceiveAsync();
```

```csharp
// Sender: no need to join the group
using var sender = new UdpClient();
await sender.SendAsync(Encoding.UTF8.GetBytes("Hello, group!"), new IPEndPoint(group, Port));
```

## Socket

`UdpClient` to cienka nakładka na klasę `Socket`, której można też używać bezpośrednio – z `SocketType.Dgram` i `ProtocolType.Udp`. `SendToAsync` wysyła datagram do podanego punktu końcowego, a `ReceiveFromAsync` odbiera datagram do przekazanego bufora i zwraca `SocketReceiveFromResult` z liczbą odebranych bajtów (`ReceivedBytes`) i nadawcą (`RemoteEndPoint`). W przeciwieństwie do `UdpClient.ReceiveAsync`, który dla każdego datagramu alokuje nową tablicę, bufor można wykorzystywać wielokrotnie. Jeśli bufor jest mniejszy niż datagram, datagram zostaje obcięty – nadmiarowe bajty przepadają.

```csharp
using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
socket.Bind(new IPEndPoint(IPAddress.Any, Port));

byte[] buffer = new byte[65_507];
while (true)
{
    // The endpoint argument only determines the address family of the sender's address
    SocketReceiveFromResult result = await socket.ReceiveFromAsync(buffer, new IPEndPoint(IPAddress.Any, 0));
    // Echo the datagram back to the sender
    await socket.SendToAsync(buffer.AsMemory(0, result.ReceivedBytes), result.RemoteEndPoint);
}
```

## Pułapki

- **Nie trzeba oznaczać granic wiadomości, ale nie ma też niezawodności.** Każda operacja odbioru zwraca cały datagram, więc separatory w postaci znaku nowej linii czy prefiksy z długością wiadomości są zbędne. Jednak wszystko, co robi za nas TCP – potwierdzenia, retransmisje, zachowanie kolejności, eliminowanie duplikatów – aplikacja musi zaimplementować sama, jeśli tego potrzebuje. Zwykle każda wiadomość niesie **numer sekwencyjny** (żeby wykrywać zagubione, zduplikowane i przestawione wiadomości), odbiorca odsyła **potwierdzenia**, a nadawca retransmituje wiadomości, które nie zostały potwierdzone. Jeśli aplikacja potrzebuje tego wszystkiego, lepszym wyborem jest zwykle TCP (lub QUIC).
- **Przekroczenie limitu czasu nie oznacza, że żądanie zaginęło.** Zaginąć mogła odpowiedź albo mogła się ona jedynie opóźnić. Serwer przetwarza wtedy to samo żądanie ponownie (dlatego żądania powinny być idempotentne, jak `date`), a spóźniona odpowiedź może zostać wzięta za odpowiedź na następne żądanie. Prawdziwe protokoły dodają do każdego żądania identyfikator (DNS używa np. 16-bitowego ID) i odrzucają odpowiedzi, które do niego nie pasują. Przykładowy klient dla uproszczenia pomija ten problem.
- **Rozmiar datagramu.** Datagramy większe niż MTU ścieżki są fragmentowane, co zwiększa prawdopodobieństwo ich utraty (zob. uwagę na początku strony).
- **Brak kontroli przepływu.** Zbyt szybki nadawca przepełnia bufor gniazda odbiorcy (`Client.ReceiveBufferSize`), a nadmiarowe datagramy są odrzucane bez żadnego powiadomienia.
- **Zapory sieciowe i NAT.** Zapory sieciowe (*firewall*) często blokują przychodzący ruch UDP. W UDP nie ma połączenia, którego koniec urządzenie NAT mogłoby zaobserwować (tak jak obserwuje koniec połączenia TCP), więc urządzenia NAT usuwają mapowania UDP już po krótkim okresie bezczynności – długotrwała komunikacja UDP przez NAT wymaga okresowego wysyłania wiadomości podtrzymujących (*keep-alive*).
- **`SocketException` z `ConnectionReset` na Windowsie.** Gdy datagram zostanie wysłany na port, na którym nikt nie nasłuchuje, odpowiedź ICMP *port unreachable* sprawia, że **następna** operacja odbioru na tym gnieździe zgłasza wyjątek `SocketException` z `SocketError.ConnectionReset` – nawet na niepołączonym gnieździe serwera. Pojedynczy klient, który zakończy działanie przed nadejściem odpowiedzi serwera, mógłby w ten sposób przerwać pętlę odbierającą serwera, dlatego powyższy serwer przechwytuje ten wyjątek i kontynuuje pracę. Na Linuxie takie błędy są zgłaszane tylko na połączonych gniazdach (jako `ConnectionRefused`).

> [!INFO]
> ## Kod źródłowy
> {{< filetree dir="lectures/ipc/Udp" >}}
