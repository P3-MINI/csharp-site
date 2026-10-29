---
title: "Udp"
weight: 45
---

# Udp

The `UDP` (User Datagram Protocol) is the second fundamental transport-layer protocol. Unlike [TCP]({{< ref "/lectures/10.ipc/tcp" >}}), it is **connectionless**: no connection is established, and each message – a **datagram** – is sent independently to a given address and port. UDP adds very little to the IP protocol itself, just an 8-byte header with the source port, the destination port, the length, and a checksum. This makes it simple and fast, but all guarantees are left to the application:

- **Message boundaries are preserved** – one send operation produces one datagram, and one receive operation returns exactly one whole datagram. Unlike a TCP byte stream, no framing is needed.
- **No delivery guarantee** – a datagram may be lost, and the sender is not notified about it.
- **No ordering guarantee** – datagrams may arrive in a different order than they were sent.
- **No protection against duplicates** – the same datagram may be delivered more than once.
- **No flow or congestion control** – the sender can send as fast as it wants. If the network or the receiver cannot keep up, datagrams are dropped (e.g., when the receiver's socket buffer is full).

A datagram with an invalid checksum is silently discarded, which – from the application's point of view – is just another lost datagram.

> [!NOTE]
> The maximum payload of a datagram is **65,507 bytes** over IPv4 (65,535 bytes of an IP packet minus 20 bytes of the IP header and 8 bytes of the UDP header). Sending a larger datagram fails with a `SocketException` (`SocketError.MessageSize`). In practice, datagrams should be much smaller: a datagram larger than the MTU (*Maximum Transmission Unit*) of the network path is split into IP fragments, and the loss of any fragment means the loss of the whole datagram (some networks drop fragments altogether). On Ethernet, with an MTU of 1500 bytes, this means at most **1472 bytes** of payload per datagram – and over the internet, staying below that is safer still.

## Use Cases

UDP is used where low latency matters more than reliability, where messages are small and independent, or where an application implements its own reliability mechanisms:

- **DNS** – a query and its response usually fit in a single datagram. If the response does not arrive, the client simply repeats the query.
- **VoIP, video calls, live streaming** – a retransmitted audio or video fragment would arrive too late to be played anyway, so it is better to skip it.
- **Online games** – only the latest game state matters; a lost, outdated position update does not need to be retransmitted.
- **Service discovery in local networks** – broadcast and multicast (used, e.g., by DHCP or mDNS) are available only with UDP.
- **QUIC** and **HTTP/3** built on top of it – QUIC implements reliability, ordering, congestion control, and encryption in user space on top of UDP. This avoids TCP's head-of-line blocking (in HTTP/2, a single lost TCP segment stalls all requests multiplexed over the connection) and lets the protocol evolve without changes to operating systems and network devices. In .NET, `HttpClient` supports HTTP/3, and the `System.Net.Quic` namespace exposes QUIC itself (both require the native `msquic` library).

## TCP vs UDP

|                         | TCP                                            | UDP                                                  |
|-------------------------|------------------------------------------------|------------------------------------------------------|
| Connection              | connection-oriented (handshake, then a stream) | connectionless                                       |
| Unit of data            | byte stream, no message boundaries             | datagram, boundaries preserved                       |
| Delivery                | guaranteed (acknowledgements, retransmissions) | not guaranteed                                       |
| Ordering                | guaranteed                                     | not guaranteed                                       |
| Duplicates              | eliminated                                     | possible                                             |
| Flow/congestion control | yes                                            | no                                                   |
| Header size             | 20–60 bytes                                    | 8 bytes                                              |
| Broadcast/multicast     | no                                             | yes                                                  |
| .NET classes            | `TcpListener`, `TcpClient`, `NetworkStream`    | `UdpClient`                                          |
| Typical uses            | HTTP/1.1, HTTP/2, SMTP, SSH, file transfer     | DNS, VoIP, online games, service discovery, QUIC     |

## UdpClient

In .NET, UDP communication is handled by the `UdpClient` class from the `System.Net.Sockets` namespace. Since there are no connections, the same class is used on both the server and the client side – there is no listener and no accepting of clients:

- The constructor `new UdpClient(port)` or `new UdpClient(new IPEndPoint(address, port))` binds the socket to a local port. A server needs this, since clients must know where to send their requests. `new UdpClient()` does not bind the socket; the operating system assigns a free (ephemeral) port when the first datagram is sent.
- `SendAsync(datagram, endPoint)` sends a single datagram to the given endpoint.
- `ReceiveAsync()` and `ReceiveAsync(CancellationToken)` wait for a single datagram and return a `UdpReceiveResult` with the data (`Buffer` – a `byte[]` containing exactly the received datagram) and the sender's address (`RemoteEndPoint`), to which a response can be sent.
- `Connect(address, port)` – despite its name, it sends nothing. It only sets the default remote endpoint: afterward, datagrams are sent with `SendAsync(datagram)` without an endpoint (passing an endpoint throws `InvalidOperationException`), and the operating system discards datagrams coming from other addresses.
- `Client` returns the underlying `Socket`, e.g., to set socket options (`Client.ReceiveBufferSize`) or to read the local endpoint (`Client.LocalEndPoint`).

## Server

The example implements the same simple "protocol" as on the TCP page – the client sends text commands (`date`, `time`), and the server sends back the appropriate message. Compare the server with the TCP one: there is no `AcceptTcpClientAsync`, no task per client and no stream – a **single socket serves all clients**. Each received datagram is a complete command, and the server responds by sending a datagram to its sender (`RemoteEndPoint`). The `exit` command is not needed either: there is no connection to close, so the client simply stops sending. Note also that TCP and UDP ports are independent – this server can use port 5000 even while the TCP server is running.

The loopback interface practically never loses datagrams, so to demonstrate a lost datagram on a single machine, the server deliberately ignores some of the requests (`LossRate`).

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

## Client

The client calls `Connect` to set the server as its default remote endpoint, then reads commands from the console and sends each one as a single datagram. Since either the request or the response may be lost, the client cannot wait for the response indefinitely. `ReceiveAsync` gets a `CancellationToken` that is cancelled after one second (`CancelAfter`); after a timeout, the request is sent again, up to three times.

> [!WARNING]
> The `Client.ReceiveTimeout` socket option applies only to the synchronous `Receive` method. Asynchronous operations must be limited with a `CancellationToken`.

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

A sample session (user input interleaved with the output) in which one request was lost:

```text
Client bound to 127.0.0.1:34853
date
10/29/2026
time
Timeout (attempt 1/3)
18:40
```

When the server is not running, the client usually learns about it quickly: the server's host responds with an ICMP *port unreachable* message, which a connected UDP socket reports as a `SocketException` (`ConnectionRefused` on Linux, `ConnectionReset` on Windows). When a datagram is lost somewhere in the network, however, nobody is notified at all – only the timeout helps.

## Broadcast and Multicast

UDP allows sending a single datagram to many recipients at once. A **broadcast** datagram sent to the address `255.255.255.255` (`IPAddress.Broadcast`) is delivered to all hosts in the local network – routers do not forward it further. This is useful for discovering services when their address is not known in advance. Sending broadcast datagrams must be enabled with `EnableBroadcast` (`UdpClient` sets it automatically only for `IPAddress.Broadcast`; for a raw `Socket` or a subnet broadcast address such as `192.168.0.255`, the send fails without it).

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

**Multicast** delivers a datagram only to the hosts that have joined a given group, identified by an address from the range `224.0.0.0`–`239.255.255.255` (the `239.0.0.0/8` range is intended for use within an organization). A receiver joins the group with `JoinMulticastGroup` (and leaves it with `DropMulticastGroup`); a sender does not need to join the group. Multicast is used, e.g., by mDNS (`224.0.0.251`) and SSDP (`239.255.255.250`) to discover devices in a local network. IPv6 has no broadcast at all – multicast is used instead.

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

`UdpClient` is a thin wrapper over the `Socket` class, which can also be used directly – with `SocketType.Dgram` and `ProtocolType.Udp`. `SendToAsync` sends a datagram to the given endpoint, and `ReceiveFromAsync` receives one into the provided buffer and returns `SocketReceiveFromResult` with the number of received bytes (`ReceivedBytes`) and the sender (`RemoteEndPoint`). Unlike `UdpClient.ReceiveAsync`, which allocates a new array for every datagram, the buffer can be reused. If the buffer is smaller than the datagram, the datagram is truncated – the excess bytes are lost.

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

## Pitfalls

- **No framing, but no reliability either.** Each receive operation returns a whole datagram, so newline delimiters or length prefixes are unnecessary. However, everything TCP does for us – acknowledgements, retransmissions, ordering, eliminating duplicates – must be implemented by the application if it needs it. Typically, each message carries a **sequence number** (to detect lost, duplicated, and reordered messages), the receiver sends back **acknowledgements**, and the sender retransmits messages that have not been acknowledged. If an application needs all of that, TCP (or QUIC) is usually a better choice.
- **A timeout does not mean the request was lost.** The response might have been lost or merely delayed. The server then processes the same request again (so requests should be idempotent, like `date`), and a late response may be mistaken for the response to the next request. Real protocols add an identifier to each request (DNS, for example, uses a 16-bit ID) and discard responses that do not match it. The example client ignores this problem for simplicity.
- **Datagram size.** Datagrams larger than the path MTU are fragmented, which increases the probability of their loss (see the note at the top of the page).
- **No flow control.** A sender that is too fast overflows the receiver's socket buffer (`Client.ReceiveBufferSize`), and the excess datagrams are dropped without any notification.
- **Firewalls and NAT.** Firewalls often block incoming UDP traffic. UDP has no connection whose end a NAT device could observe (as it observes the end of a TCP connection), so NAT devices remove UDP mappings after a short period of inactivity – long-lived UDP communication through NAT requires periodic keep-alive messages.
- **`SocketException` with `ConnectionReset` on Windows.** When a datagram is sent to a port on which nothing is listening, the ICMP *port unreachable* response makes the **next** receive operation on that socket throw a `SocketException` with `SocketError.ConnectionReset` – even on an unconnected server socket. A single client that exits before the server's response arrives could thus break the server's receive loop, which is why the server above catches this exception and continues. On Linux, such errors are reported only on connected sockets (as `ConnectionRefused`).

> [!INFO]
> ## Source Code
> {{< filetree dir="lectures/ipc/Udp" >}}
