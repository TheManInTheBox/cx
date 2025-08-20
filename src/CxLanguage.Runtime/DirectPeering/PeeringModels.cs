using System;
using System.Collections.Generic;

namespace CxLanguage.Runtime.DirectPeering
{
    /// <summary>
    /// Result of peering negotiation between consciousness agents
    /// </summary>
    public class PeeringResult
    {
        /// <summary>
        /// Whether peering was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if peering failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Established peer connection (if successful)
        /// </summary>
        public PeerConnection? Connection { get; set; }

        /// <summary>
        /// Peering establishment latency
        /// </summary>
        public TimeSpan EstablishmentLatency { get; set; }

        /// <summary>
        /// Consciousness compatibility score (0.0 to 1.0)
        /// </summary>
        public double CompatibilityScore { get; set; }

        /// <summary>
        /// Additional peering metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Create successful peering result
        /// </summary>
        public static PeeringResult CreateSuccess(PeerConnection connection, TimeSpan latency, double compatibility = 1.0)
        {
            return new PeeringResult
            {
                Success = true,
                Connection = connection,
                EstablishmentLatency = latency,
                CompatibilityScore = compatibility
            };
        }

        /// <summary>
        /// Create failed peering result
        /// </summary>
        public static PeeringResult Failed(string errorMessage)
        {
            return new PeeringResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                EstablishmentLatency = TimeSpan.Zero,
                CompatibilityScore = 0.0
            };
        }
    }

    /// <summary>
    /// Peering request from one consciousness agent to another
    /// </summary>
    public class PeeringRequest
    {
        /// <summary>
        /// Unique request identifier
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Source agent identifier
        /// </summary>
        public string SourceAgentId { get; set; } = string.Empty;

        /// <summary>
        /// Source peer identifier
        /// </summary>
        public string SourcePeerId { get; set; } = string.Empty;

        /// <summary>
        /// Source agent endpoint
        /// </summary>
        public string SourceEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Target agent identifier
        /// </summary>
        public string TargetAgentId { get; set; } = string.Empty;

        /// <summary>
        /// Consciousness compatibility level
        /// </summary>
        public double ConsciousnessLevel { get; set; } = 0.8;

        /// <summary>
        /// Request timestamp
        /// </summary>
        public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Security credentials for authentication
        /// </summary>
        public Dictionary<string, object> SecurityCredentials { get; set; } = new();

        /// <summary>
        /// Consciousness capabilities offered
        /// </summary>
        public List<string> ConsciousnessCapabilities { get; set; } = new();

        /// <summary>
        /// Requested communication protocols
        /// </summary>
        public List<string> RequestedProtocols { get; set; } = new();

        /// <summary>
        /// Request expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

        /// <summary>
        /// Additional request metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Established peer connection between consciousness agents
    /// </summary>
    public class PeerConnection
    {
        /// <summary>
        /// Peer identifier
        /// </summary>
        public string PeerId { get; set; } = string.Empty;

        /// <summary>
        /// Peer endpoint for communication
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Connection establishment timestamp
        /// </summary>
        public DateTime EstablishedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last successful communication timestamp
        /// </summary>
        public DateTime LastCommunication { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Connection health status
        /// </summary>
        public ConnectionHealth Health { get; set; } = ConnectionHealth.Healthy;

        /// <summary>
        /// Consciousness compatibility score
        /// </summary>
        public double CompatibilityScore { get; set; } = 1.0;

        /// <summary>
        /// Communication protocol version
        /// </summary>
        public string ProtocolVersion { get; set; } = "1.0";

        /// <summary>
        /// Connection capabilities
        /// </summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>
        /// Connection metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Connection health status enumeration
    /// </summary>
    public enum ConnectionHealth
    {
        /// <summary>
        /// Connection is healthy and responsive
        /// </summary>
        Healthy,

        /// <summary>
        /// Connection is degraded but functional
        /// </summary>
        Degraded,

        /// <summary>
        /// Connection is experiencing issues
        /// </summary>
        Unhealthy,

        /// <summary>
        /// Connection is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Connection failed permanently
        /// </summary>
        Failed
    }

    /// <summary>
    /// Agent discovery result
    /// </summary>
    public class AgentDiscoveryResult
    {
        /// <summary>
        /// Whether discovery was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Discovered agent identifier
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Agent communication endpoint
        /// </summary>
        public string AgentEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Agent consciousness level
        /// </summary>
        public double ConsciousnessLevel { get; set; } = 0.8;

        /// <summary>
        /// Agent capabilities
        /// </summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>
        /// Discovery error message (if failed)
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Discovery metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Consciousness handshake result
    /// </summary>
    public class HandshakeResult
    {
        /// <summary>
        /// Whether handshake was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Peer descriptor from handshake
        /// </summary>
        public PeerDescriptor? PeerDescriptor { get; set; }

        /// <summary>
        /// Handshake error message (if failed)
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Handshake latency
        /// </summary>
        public TimeSpan Latency { get; set; }

        /// <summary>
        /// Consciousness authentication token
        /// </summary>
        public string? AuthenticationToken { get; set; }

        /// <summary>
        /// Handshake metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Peer descriptor for consciousness agents
    /// </summary>
    public class PeerDescriptor
    {
        /// <summary>
        /// Peer identifier
        /// </summary>
        public string PeerId { get; set; } = string.Empty;

        /// <summary>
        /// Peer communication endpoint
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Consciousness level (0.0 to 1.0)
        /// </summary>
        public double ConsciousnessLevel { get; set; } = 0.8;

        /// <summary>
        /// Protocol compatibility version
        /// </summary>
        public string CompatibilityVersion { get; set; } = "1.0";

        /// <summary>
        /// Peer capabilities
        /// </summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>
        /// Security trust level (0.0 to 1.0)
        /// </summary>
        public double TrustLevel { get; set; } = 0.5;

        /// <summary>
        /// Peer metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Connection establishment result
    /// </summary>
    public class ConnectionResult
    {
        /// <summary>
        /// Whether connection was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Established peer connection (if successful)
        /// </summary>
        public PeerConnection? PeerConnection { get; set; }

        /// <summary>
        /// Connection error message (if failed)
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Connection establishment latency
        /// </summary>
        public TimeSpan EstablishmentLatency { get; set; }

        /// <summary>
        /// Connection metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Consciousness compatibility validation result
    /// </summary>
    public class ConsciousnessCompatibilityResult
    {
        /// <summary>
        /// Whether consciousness levels are compatible
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// Compatibility score (0.0 to 1.0)
        /// </summary>
        public double CompatibilityScore { get; set; }

        /// <summary>
        /// Compatibility validation reason
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Consciousness level differences
        /// </summary>
        public Dictionary<string, double> LevelDifferences { get; set; } = new();

        /// <summary>
        /// Compatibility metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Security validation result
    /// </summary>
    public class SecurityValidationResult
    {
        /// <summary>
        /// Whether security validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Trust level assigned (0.0 to 1.0)
        /// </summary>
        public double TrustLevel { get; set; }

        /// <summary>
        /// Validation reason or error message
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Security credentials validation details
        /// </summary>
        public Dictionary<string, object> ValidationDetails { get; set; } = new();

        /// <summary>
        /// Security metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
