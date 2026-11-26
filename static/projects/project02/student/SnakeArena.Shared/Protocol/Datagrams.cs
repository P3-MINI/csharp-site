using SnakeArena.Shared.Game;

namespace SnakeArena.Shared.Protocol;

// UDP datagrams (Part 4). The wire format is defined in the project specification (section "UDP datagrams").

public enum DatagramKind : byte
{
    DiscoveryProbe = 0x01,
    DiscoveryReply = 0x02,
    UdpHello = 0x03,
    Snapshot = 0x04,
}

public abstract record Datagram;

/// <summary>Broadcast by a client looking for servers.</summary>
public sealed record DiscoveryProbe(ushort ProtocolVersion, uint Nonce) : Datagram;

/// <summary>A server's answer to <see cref="DiscoveryProbe"/>, sent to the probe's source endpoint.</summary>
public sealed record DiscoveryReply(
    ushort ProtocolVersion,
    uint Nonce,
    string ServerName,
    ushort TcpPort,
    byte PlayerCount,
    byte MaxPlayers) : Datagram;

/// <summary>Sent by a joined client to the server's UDP game port to receive snapshots over UDP.</summary>
public sealed record UdpHello(PlayerId PlayerId, uint SessionToken) : Datagram;

/// <summary>A state snapshot sent by the server over UDP.</summary>
public sealed record SnapshotDatagram(uint SessionToken, StateSnapshot Snapshot) : Datagram;
