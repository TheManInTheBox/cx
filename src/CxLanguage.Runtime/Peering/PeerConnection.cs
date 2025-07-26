using System;
using System.Collections.Generic;

namespace CxLanguage.Runtime.Peering
{
    /// <summary>
    /// Represents a direct peer connection in the EventHub peering system.
    /// Core data structure for agent-to-agent consciousness communication.
    /// </summary>
    public class PeerConnection
    {
        /// <summary>
        /// Unique identifier for this peer connection.
        /// </summary>
        public string PeerId { get; set; } = string.Empty;
        
        /// <summary>
        /// Agent identifier associated with this peer connection.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;
        
        /// <summary>
        /// Negotiated capabilities for this peer connection.
        /// </summary>
        public PeeringCapabilities Capabilities { get; set; } = new();
        
        /// <summary>
        /// Current state of the peer connection.
        /// </summary>
        public PeerConnectionState State { get; set; }
        
        /// <summary>
        /// When this peer connection was established.
        /// </summary>
        public DateTimeOffset EstablishedAt { get; set; }
        
        /// <summary>
        /// Last activity timestamp for health monitoring.
        /// </summary>
        public DateTimeOffset LastActivity { get; set; }
        
        /// <summary>
        /// Custom metadata for this peer connection.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>
        /// Current consciousness synchronization quality (0.0 to 1.0).
        /// </summary>
        public double ConsciousnessSyncQuality { get; set; } = 1.0;
        
        /// <summary>
        /// Whether this peer supports real-time consciousness synchronization.
        /// </summary>
        public bool SupportsConsciousnessSync { get; set; } = true;
    }
    
    /// <summary>
    /// Event data for peer connection establishment.
    /// </summary>
    public class PeerConnectionEvent : EventArgs
    {
        /// <summary>
        /// ID of the newly connected peer.
        /// </summary>
        public string PeerId { get; set; } = string.Empty;
        
        /// <summary>
        /// Measured latency to this peer in milliseconds.
        /// </summary>
        public double ActualLatencyMs { get; set; }
        
        /// <summary>
        /// Negotiated capabilities for this connection.
        /// </summary>
        public PeeringCapabilities NegotiatedCapabilities { get; set; } = new();
        
        /// <summary>
        /// When the connection was established.
        /// </summary>
        public DateTimeOffset ConnectedAt { get; set; }
    }
    
    /// <summary>
    /// Event data for peer disconnection.
    /// </summary>
    public class PeerDisconnectionEvent : EventArgs
    {
        /// <summary>
        /// ID of the disconnected peer.
        /// </summary>
        public string PeerId { get; set; } = string.Empty;
        
        /// <summary>
        /// Reason for disconnection.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this was a graceful disconnection.
        /// </summary>
        public bool WasGraceful { get; set; }
        
        /// <summary>
        /// When the disconnection occurred.
        /// </summary>
        public DateTimeOffset DisconnectedAt { get; set; }
    }
    
    /// <summary>
    /// Event data for peer message reception.
    /// </summary>
    public class PeerMessageEvent : EventArgs
    {
        /// <summary>
        /// ID of the sender peer.
        /// </summary>
        public string SenderId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the received event.
        /// </summary>
        public string EventName { get; set; } = string.Empty;
        
        /// <summary>
        /// Event payload data.
        /// </summary>
        public object? Payload { get; set; }
        
        /// <summary>
        /// Measured latency for this message in milliseconds.
        /// </summary>
        public double LatencyMs { get; set; }
        
        /// <summary>
        /// When the message was received.
        /// </summary>
        public DateTimeOffset ReceivedAt { get; set; }
    }
    
    /// <summary>
    /// Peer connection states.
    /// </summary>
    public enum PeerConnectionState
    {
        Disconnected,
        Negotiating,
        Connecting,
        Connected,
        Synchronizing,
        Active,
        Degraded,
        Reconnecting,
        Failed
    }

    /// <summary>
    /// Information about a peer connection including neural plasticity metrics.
    /// </summary>
    public class PeerConnectionInfo
    {
        public string PeerId { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public PeerConnectionState State { get; set; }
        public DateTimeOffset? ConnectedAt { get; set; }
        public DateTimeOffset? LastActivity { get; set; }
        public double PlasticityStrength { get; set; }
        public double ConsciousnessCoherence { get; set; }
    }
}
