---
title: "Sieć"
weight: 30
---

# Sieć

## Architektura sieci

Poniższy diagram przedstawia typy sieciowe w bibliotece systemowej wraz z warstwami sieci, które im odpowiadają.

![Network Architecture](/lectures/NetworkArchitecture.svg)

### Warstwa transportu

W warstwie transportu znajdują się dwa podstawowe protokoły wysyłania i odbierania danych: `TCP` (*Transmission and Control Protocol*) i `UDP` (*Univeral Datagram Protocol*). Protokół `TCP` bazuje na nawiązaniu połączenia. Wykorzystywany wszędzie tam gdzie potrzebna jest gwarancja, że dane dotrą w całości i w dobrej kolejności. Protokół `UDP` nie wymaga nawiązywania połączenia, nie gwarantuje też dostarczenia danych, ale wspierają rozgłaszanie. Wykorzystywane tam gdzie szybkość jest ważniejsza niż niezawodność, np. strumieniowanie wideo, rozmowy głosowe.

`UDP` i `TCP` pozwalają na przesył danych, nie definiują wiadomości jakie można nimi wysyłać ani jakiejś ustalonej kolejności ich wymiany. Używając bezpośrednio tych protokołów musimy sami o to zadbać o takie rzeczy jak autentykacja czy szyfrowanie.

### Warstwa aplikacji

Warstwa aplikacji definiuje protokoły wyższego poziomu - w jaki sposób aplikacje komunikują się ze sobą i wymieniają dane. Protokoły warstwy aplikacji, takie jak *HTTP*, *FTP*, *SMTP*, czy *DNS*, określają format wiadomości oraz zasady interakcji między klientem a serwerem. W przeciwieństwie do protokołów transportowych (TCP/UDP), które zajmują się przesyłaniem bajtów, protokoły aplikacji skupiają się na semantyce danych i operacjach specyficznych dla danej usługi.

Wiele z protokołów w warstwie aplikacji to najczęściej obudowany protokół TCP/UDP w semantykę wymiany wiadomości. *HTTP* do wersji 2, *SMTP* i *FTP* wykorzytują *TCP*, *HTTP* w wersji 3, *VoIP* (*Voice over IP*), *BitTorrent*, *DNS* wykorzystują *UDP*. W .NET, klasa `HttpClient` jest podstawowym sposobem interakcji z usługami opartymi na protokole HTTP.

## Adresy i porty

Do komunikacji między urządzeniami i komputerami w sieci potrzebujemy adresów. Istnieją dwa standardy zapisu adresów: 32-bitowy `IPv4` (np. `192.168.8.1`) i 128-bitowy `IPv6` (np. `DEAD:BEEF:2137:6967:0420:0B4D:ADD4:E550`). Klasa `IPAddress` z przestrzeni nazw `System.Net` jest w stanie reprezentować obydwa adresy.

```csharp
IPAddress ipv4 = IPAddress.Parse("192.168.0.100");
Console.WriteLine($"{ipv4}, addr family: {ipv4.AddressFamily}");
IPAddress ipv6 = IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
Console.WriteLine($"{ipv6}, addr family: {ipv6.AddressFamily}");
// 192.168.0.100, addr family: InterNetwork
// 2001:db8:85a3::8a2e:370:7334, addr family: InterNetworkV6
```

Protokoły TCP i UDP dzielą każdy adress IP na `65536` portów (0-65535), dzięki czemu komputer z pojedynczym adresem IP może uruchamiać wiele aplikacji, każda na osobnym porcie. Adres IP wraz z portem jest reprezentowany przez klasę `IPEndPoint`.

```csharp
IPAddress ipv4 = IPAddress.Parse("192.168.0.100");
IPEndPoint ipv4Ep = new IPEndPoint(ipv4, 5000); // Port 5000
Console.WriteLine(ipv4Ep);
// 192.168.0.100:5000
```

## DNS

`DNS` to usługa, która pozwala na konwersję między adresami IP, a przyjaznymi dla człowieka nazwami domeny (np. `csharp.mini.pw.edu.pl`).

Klasa `Dns` pozwala na konwersję w dwie strony:

```csharp
foreach (IPAddress address in await Dns.GetHostAddressesAsync("pw.edu.pl"))
    Console.WriteLine(address);
// 194.29.151.9
```

```csharp
IPHostEntry entry = await Dns.GetHostEntryAsync("194.29.151.9");
Console.WriteLine(entry.HostName);
// www-virt6.ci.pw.edu.pl
```
