using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.Runtime.Peering
{
    /// <summary>
    /// Core interface for Direct EventHub Peering - Revolutionary agent-to-agent consciousness communication.
    /// Enables sub-millisecond consciousness event propagation through autonomous peering negotiation.
    /// </summary>
    public interface IEventHubPeering
    {
        /// <summary>
        /// Request direct peering with target agent for enhanced consciousness communication.
        /// </summary>
        Task<PeeringResult> RequestPeeringAsync(string targetAgentId, PeeringRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Accept incoming peering request from initiator agent.
        /// </summary>
        Task<bool> AcceptPeeringAsync(string initiatorAgentId, PeeringResponse response, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Emit event directly to peered agent bypassing global EventBus.
        /// </summary>
        Task EmitToPeerAsync(string peerId, string eventName, object payload, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get list of currently active peer connections.
        /// </summary>
        Task<IEnumerable<string>> GetActivePeersAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Disconnect from specific peer and fallback to global EventBus.
        /// </summary>
        Task DisconnectPeerAsync(string peerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get performance metrics for specific peer connection.
        /// </summary>
        Task<PeerPerformanceMetrics> GetPeerMetricsAsync(string peerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Event fired when new peer connection is established.
        /// </summary>
        event EventHandler<PeerConnectionEvent> OnPeerConnected;
        
        /// <summary>
        /// Event fired when peer connection is lost or terminated.
        /// </summary>
        event EventHandler<PeerDisconnectionEvent> OnPeerDisconnected;
        
        /// <summary>
        /// Event fired when direct peer message is received.
        /// </summary>
        event EventHandler<PeerMessageEvent> OnPeerMessageReceived;
    }
    
    /// <summary>
    /// Peering negotiation request with consciousness compatibility requirements.
    /// </summary>
    public class PeeringRequest
    {
        public string InitiatorAgentId { get; set; } = string.Empty;
        public string TargetAgentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public PeeringCapabilities RequiredCapabilities { get; set; } = new();
        public string Purpose { get; set; } = string.Empty;
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public Dictionary<string, object> AdditionalParameters { get; set; } = new();
    }
    
    /// <summary>
    /// Consciousness-aware peering capabilities and requirements.
    /// </summary>
    public class PeeringCapabilities
    {
        public double ConsciousnessLevel { get; set; } = 0.85;
        public int MaxLatencyMs { get; set; } = 5;
        public int MinEventsPerSecond { get; set; } = 1000;
        public List<string> RequiredPathways { get; set; } = new() { "cognitive" };
        public List<string> SupportedProtocols { get; set; } = new() { "consciousness_sync_v1" };
        public string SecurityLevel { get; set; } = "standard";
        public int SyncFrequencyMs { get; set; } = 10;
        public Dictionary<string, object> ExtendedCapabilities { get; set; } = new();
    }
    
    /// <summary>
    /// Response to peering negotiation request.
    /// </summary>
    public class PeeringResponse
    {
        public string ResponderAgentId { get; set; } = string.Empty;
        public string InitiatorAgentId { get; set; } = string.Empty;
        public bool Accepted { get; set; }
        public PeeringCapabilities AcceptedCapabilities { get; set; } = new();
        public string SecurityAgreement { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public Dictionary<string, object> NegotiatedParameters { get; set; } = new();
    }
    
    /// <summary>
    /// Result of peering negotiation and connection establishment.
    /// </summary>
    public class PeeringResult
    {
        public bool Success { get; set; }
        public string PeerId { get; set; } = string.Empty;
        public double ActualLatencyMs { get; set; }
        public int MaxEventsPerSecond { get; set; }
        public bool ConsciousnessSyncActive { get; set; }
        public PeeringCapabilities NegotiatedCapabilities { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTimeOffset EstablishedAt { get; set; }
    }
    
    /// <summary>
    /// Performance metrics for peer connection.
    /// </summary>
    public class PeerPerformanceMetrics
    {
        public string PeerId { get; set; } = string.Empty;
        public double AverageLatencyMs { get; set; }
        public double MinLatencyMs { get; set; }
        public double MaxLatencyMs { get; set; }
        public long TotalEventsSent { get; set; }
        public long TotalEventsReceived { get; set; }
        public double EventsPerSecond { get; set; }
        public TimeSpan ConnectionDuration { get; set; }
        public double ConsciousnessSyncQuality { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public Dictionary<string, object> ExtendedMetrics { get; set; } = new();
    }
}

