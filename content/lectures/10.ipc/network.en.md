---
title: "Network"
weight: 30
---

# Network

## Network Architecture

The diagram below illustrates the network types in the system library along with the corresponding network layers.

![Network Architecture](/lectures/NetworkArchitecture.svg)

### Transport Layer

The transport layer includes two fundamental protocols for sending and receiving data: `TCP` (Transmission Control Protocol) and `UDP` (User Datagram Protocol). The `TCP` protocol is connection-oriented, used wherever there's a need for a guarantee that data will arrive in its entirety and in the correct order. The `UDP` protocol does not require establishing a connection and does not guarantee data delivery, but it supports broadcasting. It's used where speed is more important than reliability, such as in video streaming or voice calls.

`UDP` and `TCP` allow for data transmission, but they do not define the messages that can be sent with them or any fixed order of their exchange. When using these protocols directly, we must take care of things like authentication or encryption ourselves.

### Application Layer

The application layer defines higher-level protocols—how applications communicate with each other and exchange data. Application layer protocols, such as *HTTP*, *FTP*, *SMTP*, or *DNS*, specify the message format and the rules of interaction between a client and a server. Unlike transport protocols (TCP/UDP), which deal with byte transmission, application protocols focus on the semantics of the data and operations specific to a given service.

Many of the protocols in the application layer are most often `TCP`/`UDP` protocols wrapped in message exchange semantics. *HTTP* up to version 2, *SMTP*, and *FTP* use *TCP*; *HTTP* version 3, *VoIP* (Voice over IP), *BitTorrent*, and *DNS* use *UDP*. In .NET, the `HttpClient` class is the primary way to interact with HTTP-based services.

## Addresses and Ports

For communication between devices and computers on a network, we need addresses. There are two standards for writing addresses: the 32-bit `IPv4` (e.g., `192.168.8.1`) and the 128-bit `IPv6` (e.g., `DEAD:BEEF:2137:6967:0420:0B4D:ADD4:E550`). The `IPAddress` class from the `System.Net` namespace can represent both types of addresses.

```csharp
IPAddress ipv4 = IPAddress.Parse("192.168.0.100");
Console.WriteLine($"{ipv4}, addr family: {ipv4.AddressFamily}");
IPAddress ipv6 = IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
Console.WriteLine($"{ipv6}, addr family: {ipv6.AddressFamily}");
// 192.168.0.100, addr family: InterNetwork
// 2001:db8:85a3::8a2e:370:7334, addr family: InterNetworkV6
```

TCP and UDP protocols divide each IP address into `65536` ports (0-65535), allowing a computer with a single IP address to run multiple applications, each on a separate port. An IP address along with a port is represented by the `IPEndPoint` class.

```csharp
IPAddress ipv4 = IPAddress.Parse("192.168.0.100");
IPEndPoint ipv4Ep = new IPEndPoint(ipv4, 5000); // Port 5000
Console.WriteLine(ipv4Ep);
// 192.168.0.100:5000
```

## DNS

`DNS` is a service that allows for the conversion between IP addresses and human-friendly domain names (e.g., `csharp.mini.pw.edu.pl`).

The `Dns` class allows for two-way conversion:

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