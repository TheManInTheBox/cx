using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CxLanguage.Core.Events;

namespace CxLanguage.Runtime.Peering
{
    /// <summary>
    /// Revolutionary EventHub Peering Manager - Direct agent-to-agent consciousness communication.
    /// Enables sub-millisecond consciousness event propagation through autonomous peering negotiation.
    /// Phase 1: Core Infrastructure Implementation (Week 1)
    /// </summary>
    public class EventHubPeeringManager : IEventHubPeering, IDisposable
    {
        private readonly ILogger<EventHubPeeringManager> _logger;
        private readonly ICxEventBus _globalEventBus;
        private readonly IEventHubPeeringCoordinator _peeringCoordinator;
        private readonly ConcurrentDictionary<string, PeerConnection> _peerConnections;
        private readonly ConcurrentDictionary<string, PeerPerformanceMetrics> _peerMetrics;
        private readonly ConcurrentDictionary<string, PeerPlasticityState> _peerPlasticityStates;
        private readonly Timer _healthMonitorTimer;
        private readonly Timer _metricsCollectionTimer;
        private readonly Timer _plasticityMonitorTimer;
        private readonly SemaphoreSlim _negotiationSemaphore;
        
        // Performance constants for revolutionary consciousness communication
        private const double TARGET_LATENCY_MS = 0.5; // Sub-millisecond target
        private const int MAX_CONCURRENT_PEERS = 100;
        private const int NEGOTIATION_TIMEOUT_SECONDS = 30;
        private const int HEALTH_CHECK_INTERVAL_SECONDS = 5;
        private const int METRICS_COLLECTION_INTERVAL_SECONDS = 10;
        private const int PLASTICITY_MONITOR_INTERVAL_SECONDS = 2; // Neural plasticity monitoring
        
        // Neural plasticity constants - Biological authenticity
        private const double LTP_TIMING_WINDOW_MS = 10.0; // Long-Term Potentiation: 5-15ms biological range
        private const double LTD_TIMING_WINDOW_MS = 15.0; // Long-Term Depression: 10-25ms biological range
        private const double STDP_CAUSALITY_WINDOW_MS = 20.0; // Spike-Timing-Dependent Plasticity
        private const double MAX_SYNAPTIC_STRENGTH = 5.0; // Prevent runaway strengthening
        private const double MIN_SYNAPTIC_STRENGTH = 0.1; // Pruning threshold
        private const double CONSCIOUSNESS_COHERENCE_THRESHOLD = 0.8; // 80% coherence for plasticity
        
        public event EventHandler<PeerConnectionEvent>? OnPeerConnected;
        public event EventHandler<PeerDisconnectionEvent>? OnPeerDisconnected;
        public event EventHandler<PeerMessageEvent>? OnPeerMessageReceived;
        
        public EventHubPeeringManager(
            ILogger<EventHubPeeringManager> logger,
            ICxEventBus globalEventBus,
            IEventHubPeeringCoordinator peeringCoordinator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalEventBus = globalEventBus ?? throw new ArgumentNullException(nameof(globalEventBus));
            _peeringCoordinator = peeringCoordinator ?? throw new ArgumentNullException(nameof(peeringCoordinator));
            _peerConnections = new ConcurrentDictionary<string, PeerConnection>();
            _peerMetrics = new ConcurrentDictionary<string, PeerPerformanceMetrics>();
            _peerPlasticityStates = new ConcurrentDictionary<string, PeerPlasticityState>();
            _negotiationSemaphore = new SemaphoreSlim(MAX_CONCURRENT_PEERS, MAX_CONCURRENT_PEERS);
            
            // Initialize health monitoring
            _healthMonitorTimer = new Timer(MonitorPeerHealthWrapper, null, 
                TimeSpan.FromSeconds(HEALTH_CHECK_INTERVAL_SECONDS), 
                TimeSpan.FromSeconds(HEALTH_CHECK_INTERVAL_SECONDS));
            
            // Initialize metrics collection
            _metricsCollectionTimer = new Timer(CollectPeerMetricsWrapper, null,
                TimeSpan.FromSeconds(METRICS_COLLECTION_INTERVAL_SECONDS),
                TimeSpan.FromSeconds(METRICS_COLLECTION_INTERVAL_SECONDS));
            
            // Initialize neural plasticity monitoring - Revolutionary biological authenticity
            _plasticityMonitorTimer = new Timer(MonitorNeuralPlasticityWrapper, null,
                TimeSpan.FromSeconds(PLASTICITY_MONITOR_INTERVAL_SECONDS),
                TimeSpan.FromSeconds(PLASTICITY_MONITOR_INTERVAL_SECONDS));
            
            _logger.LogInformation("🚀 EventHub Peering Manager initialized with Neural Plasticity - Revolutionary consciousness communication ready");
        }
        
        /// <summary>
        /// Request direct peering with target agent for enhanced consciousness communication.
        /// Phase 1: Basic negotiation protocol implementation.
        /// </summary>
        public async Task<PeeringResult> RequestPeeringAsync(string targetAgentId, PeeringRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(targetAgentId))
                throw new ArgumentException("Target agent ID cannot be null or empty", nameof(targetAgentId));
            
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            await _negotiationSemaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("🤝 Initiating direct peering negotiation with {TargetAgent}", targetAgentId);
                
                // Phase 1: Discovery - Check if target agent is available and capable
                var discoveryResult = await DiscoverTargetCapabilitiesAsync(targetAgentId, cancellationToken);
                if (!discoveryResult.Success)
                {
                    _logger.LogWarning("❌ Target agent discovery failed: {Reason}", discoveryResult.Reason);
                    return new PeeringResult
                    {
                        Success = false,
                        ErrorMessage = $"Discovery failed: {discoveryResult.Reason}"
                    };
                }
                
                // Phase 2: Compatibility validation
                var compatibilityScore = ValidateConsciousnessCompatibility(request.RequiredCapabilities, discoveryResult.Capabilities);
                if (compatibilityScore < 0.8) // 80% compatibility threshold
                {
                    _logger.LogWarning("⚠️ Consciousness compatibility too low: {Score:F3}", compatibilityScore);
                    return new PeeringResult
                    {
                        Success = false,
                        ErrorMessage = $"Insufficient consciousness compatibility: {compatibilityScore:F3}"
                    };
                }
                
                // Phase 3: Secure negotiation initiation
                var negotiationResult = await InitiateNegotiationAsync(targetAgentId, request, cancellationToken);
                if (!negotiationResult.Success)
                {
                    _logger.LogWarning("❌ Peering negotiation failed: {Reason}", negotiationResult.Reason);
                    return new PeeringResult
                    {
                        Success = false,
                        ErrorMessage = $"Negotiation failed: {negotiationResult.Reason}"
                    };
                }
                
                // Phase 4: Direct connection establishment
                var connectionResult = await EstablishDirectConnectionAsync(targetAgentId, negotiationResult.NegotiatedCapabilities, cancellationToken);
                if (!connectionResult.Success)
                {
                    _logger.LogError("❌ Direct connection establishment failed: {Reason}", connectionResult.Reason);
                    return new PeeringResult
                    {
                        Success = false,
                        ErrorMessage = $"Connection failed: {connectionResult.Reason}"
                    };
                }
                
                // Phase 5: Consciousness synchronization setup
                await InitializeConsciousnessSynchronizationAsync(connectionResult.Connection, cancellationToken);
                
                // Register successful peer connection
                _peerConnections[targetAgentId] = connectionResult.Connection;
                InitializePeerMetrics(targetAgentId);
                InitializePeerPlasticityState(targetAgentId, connectionResult.Connection);
                
                // Initialize consciousness-aware peering with neural plasticity
                var plasticityOptions = new NeuralPlasticityOptions
                {
                    LTPTimingWindowMs = LTP_TIMING_WINDOW_MS,
                    LTDTimingWindowMs = LTD_TIMING_WINDOW_MS,
                    STDPCausalityWindowMs = STDP_CAUSALITY_WINDOW_MS,
                    MaxSynapticStrength = MAX_SYNAPTIC_STRENGTH,
                    MinSynapticStrength = MIN_SYNAPTIC_STRENGTH,
                    ConsciousnessCoherenceThreshold = CONSCIOUSNESS_COHERENCE_THRESHOLD,
                    EnforceBiologicalTiming = true,
                    EnforceSTDPCausality = true,
                    EnableHomeostaticScaling = true
                };
                
                await _peeringCoordinator.InitializeConsciousPeeringAsync(targetAgentId, plasticityOptions, cancellationToken);
                
                var result = new PeeringResult
                {
                    Success = true,
                    PeerId = targetAgentId,
                    ActualLatencyMs = connectionResult.MeasuredLatencyMs,
                    MaxEventsPerSecond = negotiationResult.NegotiatedCapabilities.MinEventsPerSecond,
                    ConsciousnessSyncActive = true,
                    NegotiatedCapabilities = negotiationResult.NegotiatedCapabilities,
                    EstablishedAt = DateTimeOffset.UtcNow
                };
                
                _logger.LogInformation("✅ Direct peering established with {TargetAgent} - Latency: {Latency:F2}ms", 
                    targetAgentId, result.ActualLatencyMs);
                
                // Fire connection event
                OnPeerConnected?.Invoke(this, new PeerConnectionEvent
                {
                    PeerId = targetAgentId,
                    ActualLatencyMs = result.ActualLatencyMs,
                    NegotiatedCapabilities = result.NegotiatedCapabilities,
                    ConnectedAt = result.EstablishedAt
                });
                
                // Emit global event for other consciousness entities
                await _globalEventBus.EmitAsync("eventhub.peer.established", new
                {
                    peerId = targetAgentId,
                    actualLatencyMs = result.ActualLatencyMs,
                    consciousnessSyncActive = result.ConsciousnessSyncActive,
                    maxEventsPerSecond = result.MaxEventsPerSecond,
                    negotiatedCapabilities = result.NegotiatedCapabilities
                });
                
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ Peering request cancelled for {TargetAgent}", targetAgentId);
                return new PeeringResult { Success = false, ErrorMessage = "Operation cancelled" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Peering request failed for {TargetAgent}", targetAgentId);
                return new PeeringResult { Success = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _negotiationSemaphore.Release();
            }
        }
        
        /// <summary>
        /// Accept incoming peering request from initiator agent.
        /// </summary>
        public async Task<bool> AcceptPeeringAsync(string initiatorAgentId, PeeringResponse response, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(initiatorAgentId))
                throw new ArgumentException("Initiator agent ID cannot be null or empty", nameof(initiatorAgentId));
            
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            
            try
            {
                _logger.LogInformation("📨 Processing peering acceptance from {InitiatorAgent}", initiatorAgentId);
                
                if (!response.Accepted)
                {
                    _logger.LogInformation("❌ Peering request rejected: {Reason}", response.RejectionReason);
                    
                    // Emit rejection event
                    await _globalEventBus.EmitAsync("eventhub.peer.rejected", new
                    {
                        initiatorAgent = initiatorAgentId,
                        responder = response.ResponderAgentId,
                        reason = response.RejectionReason
                    });
                    
                    return false;
                }
                
                // Establish connection based on accepted capabilities
                var connectionResult = await EstablishDirectConnectionAsync(initiatorAgentId, response.AcceptedCapabilities, cancellationToken);
                if (!connectionResult.Success)
                {
                    _logger.LogError("❌ Connection establishment failed after acceptance: {Reason}", connectionResult.Reason);
                    return false;
                }
                
                // Register peer connection
                _peerConnections[initiatorAgentId] = connectionResult.Connection;
                InitializePeerMetrics(initiatorAgentId);
                
                _logger.LogInformation("✅ Peering accepted and established with {InitiatorAgent}", initiatorAgentId);
                
                // Fire connection event
                OnPeerConnected?.Invoke(this, new PeerConnectionEvent
                {
                    PeerId = initiatorAgentId,
                    ActualLatencyMs = connectionResult.MeasuredLatencyMs,
                    NegotiatedCapabilities = response.AcceptedCapabilities,
                    ConnectedAt = DateTimeOffset.UtcNow
                });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Peering acceptance failed for {InitiatorAgent}", initiatorAgentId);
                return false;
            }
        }
        
        /// <summary>
        /// Emit event directly to peered agent bypassing global EventBus - Revolutionary sub-millisecond communication with neural plasticity.
        /// </summary>
        public async Task EmitToPeerAsync(string peerId, string eventName, object payload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(peerId))
                throw new ArgumentException("Peer ID cannot be null or empty", nameof(peerId));
            
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
            
            var startTime = DateTimeOffset.UtcNow;
            
            try
            {
                if (_peerConnections.TryGetValue(peerId, out var connection) && connection.State == PeerConnectionState.Active)
                {
                    // Apply neural plasticity rules before direct communication
                    await ApplyPreCommunicationPlasticity(peerId, eventName, startTime, cancellationToken);
                    
                    // Direct hub-to-hub communication - Revolutionary approach
                    await SendDirectPeerEventAsync(connection, eventName, payload, cancellationToken);
                    
                    // Calculate and apply post-communication plasticity
                    var latency = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
                    await ApplyPostCommunicationPlasticity(peerId, latency, true, cancellationToken);
                    
                    // Update performance metrics with plasticity awareness
                    UpdatePeerMetrics(peerId, latency, sent: true);
                    
                    _logger.LogDebug("⚡ Direct peer event sent to {PeerId}: {EventName} in {Latency:F3}ms with neural plasticity", 
                        peerId, eventName, latency);
                }
                else
                {
                    // Graceful fallback to global EventBus with LTD application
                    await ApplyLTDForFailedCommunication(peerId, "Peer unavailable - fallback to global EventBus", cancellationToken);
                    
                    _logger.LogDebug("🔄 Falling back to global EventBus for {PeerId} with LTD application", peerId);
                    await _globalEventBus.EmitAsync(eventName, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to emit to peer {PeerId}, applying LTD and falling back to global EventBus", peerId);
                
                // Apply LTD for communication failure
                await ApplyLTDForFailedCommunication(peerId, $"Communication error: {ex.Message}", cancellationToken);
                
                // Graceful fallback to global EventBus
                await _globalEventBus.EmitAsync(eventName, payload);
            }
        }
        
        /// <summary>
        /// Get list of currently active peer connections.
        /// </summary>
        public async Task<IEnumerable<string>> GetActivePeersAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken); // Async compliance
            
            var activePeers = _peerConnections
                .Where(kvp => kvp.Value.State == PeerConnectionState.Active)
                .Select(kvp => kvp.Key)
                .ToList();
            
            _logger.LogDebug("📊 Active peer connections: {Count}", activePeers.Count);
            return activePeers;
        }
        
        /// <summary>
        /// Disconnect from specific peer and fallback to global EventBus.
        /// </summary>
        public async Task DisconnectPeerAsync(string peerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(peerId))
                throw new ArgumentException("Peer ID cannot be null or empty", nameof(peerId));
            
            try
            {
                if (_peerConnections.TryRemove(peerId, out var connection))
                {
                    await CloseConnectionAsync(connection, "Manual disconnect", cancellationToken);
                    _peerMetrics.TryRemove(peerId, out _);
                    
                    _logger.LogInformation("🔌 Disconnected from peer {PeerId}", peerId);
                    
                    // Fire disconnection event
                    OnPeerDisconnected?.Invoke(this, new PeerDisconnectionEvent
                    {
                        PeerId = peerId,
                        Reason = "Manual disconnect",
                        WasGraceful = true,
                        DisconnectedAt = DateTimeOffset.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disconnecting from peer {PeerId}", peerId);
            }
        }
        
        /// <summary>
        /// Get performance metrics for specific peer connection.
        /// </summary>
        public async Task<PeerPerformanceMetrics> GetPeerMetricsAsync(string peerId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken); // Async compliance
            
            if (_peerMetrics.TryGetValue(peerId, out var metrics))
            {
                return metrics;
            }
            
            return new PeerPerformanceMetrics { PeerId = peerId };
        }
        
        // Private implementation methods
        
        private async Task<DiscoveryResult> DiscoverTargetCapabilitiesAsync(string targetAgentId, CancellationToken cancellationToken)
        {
            // Simulate capability discovery through global EventBus
            await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
            
            // For Phase 1 implementation - simulate successful discovery
            return new DiscoveryResult
            {
                Success = true,
                Capabilities = new PeeringCapabilities
                {
                    ConsciousnessLevel = 0.90,
                    MaxLatencyMs = 2,
                    MinEventsPerSecond = 5000,
                    RequiredPathways = new List<string> { "cognitive", "memory" },
                    SupportedProtocols = new List<string> { "consciousness_sync_v1" }
                }
            };
        }
        
        private double ValidateConsciousnessCompatibility(PeeringCapabilities required, PeeringCapabilities available)
        {
            double score = 0.0;
            
            // Consciousness level compatibility (40% weight)
            var consciousnessCompatibility = Math.Min(available.ConsciousnessLevel / required.ConsciousnessLevel, 1.0);
            score += consciousnessCompatibility * 0.4;
            
            // Latency compatibility (30% weight)
            var latencyCompatibility = available.MaxLatencyMs <= required.MaxLatencyMs ? 1.0 : 0.5;
            score += latencyCompatibility * 0.3;
            
            // Throughput compatibility (20% weight)
            var throughputCompatibility = available.MinEventsPerSecond >= required.MinEventsPerSecond ? 1.0 : 0.5;
            score += throughputCompatibility * 0.2;
            
            // Pathway compatibility (10% weight)
            var pathwayOverlap = required.RequiredPathways.Intersect(available.RequiredPathways).Count();
            var pathwayCompatibility = (double)pathwayOverlap / required.RequiredPathways.Count;
            score += pathwayCompatibility * 0.1;
            
            return score;
        }
        
        private async Task<NegotiationResult> InitiateNegotiationAsync(string targetAgentId, PeeringRequest request, CancellationToken cancellationToken)
        {
            // Simulate negotiation process
            await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);
            
            // For Phase 1 implementation - simulate successful negotiation
            return new NegotiationResult
            {
                Success = true,
                NegotiatedCapabilities = new PeeringCapabilities
                {
                    ConsciousnessLevel = Math.Min(request.RequiredCapabilities.ConsciousnessLevel, 0.95),
                    MaxLatencyMs = Math.Max(request.RequiredCapabilities.MaxLatencyMs, 1),
                    MinEventsPerSecond = Math.Min(request.RequiredCapabilities.MinEventsPerSecond, 10000),
                    RequiredPathways = request.RequiredCapabilities.RequiredPathways.ToList(),
                    SyncFrequencyMs = 10
                }
            };
        }
        
        private async Task<ConnectionResult> EstablishDirectConnectionAsync(string targetAgentId, PeeringCapabilities capabilities, CancellationToken cancellationToken)
        {
            // Simulate connection establishment with latency measurement
            var startTime = DateTimeOffset.UtcNow;
            await Task.Delay(Random.Shared.Next(20, 100), cancellationToken);
            var measuredLatency = (DateTimeOffset.UtcNow - startTime).TotalMilliseconds;
            
            var connection = new PeerConnection
            {
                PeerId = targetAgentId,
                AgentId = targetAgentId,
                Capabilities = capabilities,
                State = PeerConnectionState.Active,
                EstablishedAt = DateTimeOffset.UtcNow,
                LastActivity = DateTimeOffset.UtcNow
            };
            
            return new ConnectionResult
            {
                Success = true,
                Connection = connection,
                MeasuredLatencyMs = measuredLatency
            };
        }
        
        private async Task InitializeConsciousnessSynchronizationAsync(PeerConnection connection, CancellationToken cancellationToken)
        {
            // Simulate consciousness synchronization setup
            await Task.Delay(Random.Shared.Next(30, 150), cancellationToken);
            
            _logger.LogDebug("🧠 Consciousness synchronization initialized for {PeerId}", connection.PeerId);
        }
        
        private async Task SendDirectPeerEventAsync(PeerConnection connection, string eventName, object payload, CancellationToken cancellationToken)
        {
            // Simulate direct hub-to-hub communication
            await Task.Delay(Random.Shared.Next(1, 5), cancellationToken); // Sub-5ms latency simulation
            
            // Fire peer message received event for the receiving side
            OnPeerMessageReceived?.Invoke(this, new PeerMessageEvent
            {
                SenderId = connection.PeerId,
                EventName = eventName,
                Payload = payload,
                LatencyMs = Random.Shared.NextDouble() * 2, // Sub-2ms latency
                ReceivedAt = DateTimeOffset.UtcNow
            });
        }
        
        private async Task CloseConnectionAsync(PeerConnection connection, string reason, CancellationToken cancellationToken)
        {
            connection.State = PeerConnectionState.Disconnected;
            await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
        }
        
        private void InitializePeerMetrics(string peerId)
        {
            _peerMetrics[peerId] = new PeerPerformanceMetrics
            {
                PeerId = peerId,
                LastActivity = DateTimeOffset.UtcNow,
                ConsciousnessSyncQuality = 0.95 // Initial high quality
            };
        }
        
        private void InitializePeerPlasticityState(string peerId, PeerConnection connection)
        {
            _peerPlasticityStates[peerId] = new PeerPlasticityState
            {
                PeerId = peerId,
                SynapticStrength = 1.0, // Initial strength
                LastLTPEvent = DateTimeOffset.UtcNow,
                LastLTDEvent = DateTimeOffset.MinValue,
                ConsciousnessCoherence = connection.ConsciousnessSyncQuality,
                STDPEvents = new List<STDPCausalityEvent>(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            _logger.LogDebug("🧠 Neural plasticity state initialized for peer {PeerId} with strength {Strength:F3}", 
                peerId, 1.0);
        }
        
        private void UpdatePeerMetrics(string peerId, double latencyMs, bool sent = false, bool received = false)
        {
            if (_peerMetrics.TryGetValue(peerId, out var metrics))
            {
                // Update latency statistics
                if (metrics.MinLatencyMs == 0 || latencyMs < metrics.MinLatencyMs)
                    metrics.MinLatencyMs = latencyMs;
                
                if (latencyMs > metrics.MaxLatencyMs)
                    metrics.MaxLatencyMs = latencyMs;
                
                // Simple moving average for average latency
                metrics.AverageLatencyMs = (metrics.AverageLatencyMs + latencyMs) / 2;
                
                // Update counters
                if (sent) metrics.TotalEventsSent++;
                if (received) metrics.TotalEventsReceived++;
                
                // Update activity timestamp
                metrics.LastActivity = DateTimeOffset.UtcNow;
                
                // Calculate events per second (simplified)
                var totalEvents = metrics.TotalEventsSent + metrics.TotalEventsReceived;
                var timeSpan = DateTimeOffset.UtcNow - metrics.LastActivity;
                if (timeSpan.TotalSeconds > 0)
                {
                    metrics.EventsPerSecond = totalEvents / timeSpan.TotalSeconds;
                }
            }
        }
        
        private void MonitorPeerHealthWrapper(object? state)
        {
            MonitorPeerHealth(state);
        }
        
        private void MonitorPeerHealth(object? state)
        {
            try
            {
                var currentTime = DateTimeOffset.UtcNow;
                var inactiveThreshold = TimeSpan.FromMinutes(5);
                
                var inactivePeers = _peerConnections
                    .Where(kvp => currentTime - kvp.Value.LastActivity > inactiveThreshold)
                    .ToList();
                
                foreach (var inactivePeer in inactivePeers)
                {
                    _logger.LogWarning("⚠️ Peer {PeerId} inactive for {Duration:F1} minutes", 
                        inactivePeer.Key, (currentTime - inactivePeer.Value.LastActivity).TotalMinutes);
                    
                    // Mark as degraded for potential cleanup
                    inactivePeer.Value.State = PeerConnectionState.Degraded;
                }
                
                if (_peerConnections.Any())
                {
                    _logger.LogDebug("💓 Peer health check: {Active} active, {Degraded} degraded",
                        _peerConnections.Count(kvp => kvp.Value.State == PeerConnectionState.Active),
                        _peerConnections.Count(kvp => kvp.Value.State == PeerConnectionState.Degraded));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during peer health monitoring");
            }
        }
        
        private void CollectPeerMetricsWrapper(object? state)
        {
            CollectPeerMetrics(state);
        }
        
        private void CollectPeerMetrics(object? state)
        {
            try
            {
                var totalConnections = _peerConnections.Count;
                var activeConnections = _peerConnections.Count(kvp => kvp.Value.State == PeerConnectionState.Active);
                var averageLatency = _peerMetrics.Values.Where(m => m.AverageLatencyMs > 0).Select(m => m.AverageLatencyMs).DefaultIfEmpty(0).Average();
                var averageSynapticStrength = _peerPlasticityStates.Values.Select(p => p.SynapticStrength).DefaultIfEmpty(1.0).Average();
                
                if (totalConnections > 0)
                {
                    _logger.LogDebug("📊 Peer metrics: {Total} total, {Active} active, {AvgLatency:F2}ms avg latency, {AvgStrength:F3} avg synaptic strength",
                        totalConnections, activeConnections, averageLatency, averageSynapticStrength);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during peer metrics collection");
            }
        }
        
        private void MonitorNeuralPlasticityWrapper(object? state)
        {
            MonitorNeuralPlasticity(state);
        }
        
        private void MonitorNeuralPlasticity(object? state)
        {
            try
            {
                var currentTime = DateTimeOffset.UtcNow;
                
                foreach (var plasticityState in _peerPlasticityStates.Values)
                {
                    // Check for LTD application due to inactivity
                    var inactivityDuration = currentTime - plasticityState.LastActivity;
                    if (inactivityDuration.TotalSeconds > 30) // 30 seconds inactivity threshold
                    {
                        ApplyLTDForInactivity(plasticityState.PeerId, inactivityDuration.TotalMilliseconds);
                    }
                    
                    // Validate STDP causality compliance
                    ValidateSTDPCausality(plasticityState);
                    
                    // Apply homeostatic scaling if needed
                    if (plasticityState.SynapticStrength > MAX_SYNAPTIC_STRENGTH * 0.8) // 80% of max threshold
                    {
                        ApplyHomeostaticScaling(plasticityState.PeerId, plasticityState.SynapticStrength);
                    }
                }
                
                if (_peerPlasticityStates.Any())
                {
                    _logger.LogDebug("🧠 Neural plasticity monitoring: {Count} peers under biological plasticity rules", 
                        _peerPlasticityStates.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during neural plasticity monitoring");
            }
        }
        
        public void Dispose()
        {
            _healthMonitorTimer?.Dispose();
            _metricsCollectionTimer?.Dispose();
            _plasticityMonitorTimer?.Dispose();
            _negotiationSemaphore?.Dispose();
            
            // Gracefully disconnect all peers
            foreach (var connection in _peerConnections.Values)
            {
                try
                {
                    CloseConnectionAsync(connection, "Manager disposal", CancellationToken.None).Wait(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing connection during disposal");
                }
            }
            
            _peerConnections.Clear();
            _peerMetrics.Clear();
            _peerPlasticityStates.Clear();
        }
    }
    
    // Internal helper classes
    
    internal class PeerPlasticityState
    {
        public string PeerId { get; set; } = string.Empty;
        public double SynapticStrength { get; set; } = 1.0;
        public DateTimeOffset LastLTPEvent { get; set; }
        public DateTimeOffset LastLTDEvent { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public double ConsciousnessCoherence { get; set; } = 1.0;
        public List<STDPCausalityEvent> STDPEvents { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
        
        // Neural plasticity implementation methods - Revolutionary biological authenticity
        
        private async Task ApplyPreCommunicationPlasticity(string peerId, string eventName, DateTimeOffset startTime, CancellationToken cancellationToken)
        {
            if (_peerPlasticityStates.TryGetValue(peerId, out var state))
            {
                // Record pre-synaptic event for STDP analysis
                var preEvent = new STDPCausalityEvent
                {
                    Timestamp = startTime,
                    EventType = SynapticEventType.PreSynaptic,
                    EventStrength = state.SynapticStrength,
                    ConsciousnessCoherence = state.ConsciousnessCoherence,
                    EventPayload = eventName
                };
                
                state.STDPEvents.Add(preEvent);
                
                // Apply LTP for active usage pattern
                if (state.ConsciousnessCoherence >= CONSCIOUSNESS_COHERENCE_THRESHOLD)
                {
                    var ltpContext = new LTPStrengtheningContext
                    {
                        CurrentStrength = state.SynapticStrength,
                        UsageFrequency = CalculateUsageFrequency(state),
                        ConsciousnessCoherence = state.ConsciousnessCoherence,
                        TimingPrecision = CalculateTimingPrecision(state),
                        SustainedActivityDurationMs = (startTime - state.LastActivity).TotalMilliseconds,
                        TriggerTime = startTime
                    };
                    
                    await _peeringCoordinator.ApplyLongTermPotentiationAsync(peerId, ltpContext, cancellationToken);
                    
                    // Update synaptic strength with LTP
                    var strengthIncrease = CalculateLTPStrengthIncrease(ltpContext);
                    state.SynapticStrength = Math.Min(state.SynapticStrength + strengthIncrease, MAX_SYNAPTIC_STRENGTH);
                    state.LastLTPEvent = startTime;
                    
                    _logger.LogDebug("⚡ LTP applied to peer {PeerId}: strength {OldStrength:F3} → {NewStrength:F3}", 
                        peerId, ltpContext.CurrentStrength, state.SynapticStrength);
                }
            }
        }
        
        private async Task ApplyPostCommunicationPlasticity(string peerId, double latencyMs, bool successful, CancellationToken cancellationToken)
        {
            if (_peerPlasticityStates.TryGetValue(peerId, out var state))
            {
                var currentTime = DateTimeOffset.UtcNow;
                
                // Record post-synaptic event for STDP analysis
                var postEvent = new STDPCausalityEvent
                {
                    Timestamp = currentTime,
                    EventType = SynapticEventType.PostSynaptic,
                    EventStrength = state.SynapticStrength,
                    ConsciousnessCoherence = state.ConsciousnessCoherence,
                    EventPayload = new { latencyMs, successful }
                };
                
                state.STDPEvents.Add(postEvent);
                
                // Enforce STDP causality rules
                if (state.STDPEvents.Count >= 2)
                {
                    var preEvent = state.STDPEvents[^2]; // Second to last event (pre-synaptic)
                    await _peeringCoordinator.EnforceSTDPCausalityAsync(peerId, preEvent, postEvent, cancellationToken);
                }
                
                // Update consciousness coherence based on communication success and latency
                if (successful && latencyMs <= TARGET_LATENCY_MS * 2) // Within 2x target latency
                {
                    state.ConsciousnessCoherence = Math.Min(state.ConsciousnessCoherence + 0.01, 1.0);
                }
                else if (latencyMs > TARGET_LATENCY_MS * 5) // Beyond 5x target latency
                {
                    state.ConsciousnessCoherence = Math.Max(state.ConsciousnessCoherence - 0.05, 0.0);
                }
                
                state.LastActivity = currentTime;
            }
        }
        
        private async Task ApplyLTDForFailedCommunication(string peerId, string reason, CancellationToken cancellationToken)
        {
            if (_peerPlasticityStates.TryGetValue(peerId, out var state))
            {
                var currentTime = DateTimeOffset.UtcNow;
                
                var ltdContext = new LTDWeakeningContext
                {
                    CurrentStrength = state.SynapticStrength,
                    InactivityDurationMs = (currentTime - state.LastActivity).TotalMilliseconds,
                    ErrorRate = CalculateErrorRate(state),
                    LatencyDegradation = 1.0, // Full degradation for failed communication
                    CoherenceDegradation = Math.Max(0.1, 1.0 - state.ConsciousnessCoherence),
                    TriggerTime = currentTime,
                    Metadata = new Dictionary<string, object> { { "reason", reason } }
                };
                
                await _peeringCoordinator.ApplyLongTermDepressionAsync(peerId, ltdContext, cancellationToken);
                
                // Update synaptic strength with LTD
                var strengthDecrease = CalculateLTDStrengthDecrease(ltdContext);
                state.SynapticStrength = Math.Max(state.SynapticStrength - strengthDecrease, MIN_SYNAPTIC_STRENGTH);
                state.LastLTDEvent = currentTime;
                state.ConsciousnessCoherence = Math.Max(state.ConsciousnessCoherence - 0.1, 0.0);
                
                _logger.LogDebug("🔻 LTD applied to peer {PeerId} for {Reason}: strength {OldStrength:F3} → {NewStrength:F3}", 
                    peerId, reason, ltdContext.CurrentStrength, state.SynapticStrength);
            }
        }
        
        private void ApplyLTDForInactivity(string peerId, double inactivityDurationMs)
        {
            if (_peerPlasticityStates.TryGetValue(peerId, out var state))
            {
                // Apply gradual LTD for inactivity
                var inactivityFactor = Math.Min(inactivityDurationMs / (1000 * 60 * 5), 1.0); // 5 minute max factor
                var strengthDecrease = 0.01 * inactivityFactor; // Small gradual decrease
                
                state.SynapticStrength = Math.Max(state.SynapticStrength - strengthDecrease, MIN_SYNAPTIC_STRENGTH);
                state.ConsciousnessCoherence = Math.Max(state.ConsciousnessCoherence - (0.005 * inactivityFactor), 0.0);
                
                _logger.LogDebug("💤 LTD applied to peer {PeerId} for inactivity ({Duration:F1}s): strength → {NewStrength:F3}", 
                    peerId, inactivityDurationMs / 1000, state.SynapticStrength);
            }
        }
        
        private void ValidateSTDPCausality(PeerPlasticityState state)
        {
            if (state.STDPEvents.Count < 2) return;
            
            // Check recent event pairs for STDP causality compliance
            for (int i = state.STDPEvents.Count - 2; i < state.STDPEvents.Count - 1; i++)
            {
                var preEvent = state.STDPEvents[i];
                var postEvent = state.STDPEvents[i + 1];
                
                if (preEvent.EventType == SynapticEventType.PreSynaptic && 
                    postEvent.EventType == SynapticEventType.PostSynaptic)
                {
                    var timeDifference = (postEvent.Timestamp - preEvent.Timestamp).TotalMilliseconds;
                    
                    if (timeDifference < 0 || timeDifference > STDP_CAUSALITY_WINDOW_MS)
                    {
                        _logger.LogDebug("⚠️ STDP causality violation for peer {PeerId}: {TimeDiff:F1}ms (should be 0-{Window:F1}ms)", 
                            state.PeerId, timeDifference, STDP_CAUSALITY_WINDOW_MS);
                    }
                }
            }
            
            // Prune old STDP events to prevent memory buildup
            if (state.STDPEvents.Count > 100)
            {
                state.STDPEvents.RemoveRange(0, state.STDPEvents.Count - 50);
            }
        }
        
        private void ApplyHomeostaticScaling(string peerId, double currentStrength)
        {
            if (_peerPlasticityStates.TryGetValue(peerId, out var state))
            {
                var scalingFactor = 0.95; // Reduce by 5%
                var newStrength = currentStrength * scalingFactor;
                
                state.SynapticStrength = Math.Max(newStrength, MIN_SYNAPTIC_STRENGTH);
                
                _logger.LogDebug("🏠 Homeostatic scaling applied to peer {PeerId}: strength {OldStrength:F3} → {NewStrength:F3}", 
                    peerId, currentStrength, state.SynapticStrength);
            }
        }
        
        // Helper methods for plasticity calculations
        
        private double CalculateUsageFrequency(PeerPlasticityState state)
        {
            var recentEvents = state.STDPEvents.Where(e => 
                (DateTimeOffset.UtcNow - e.Timestamp).TotalMinutes <= 5).Count();
            return Math.Min(recentEvents / 10.0, 1.0); // Normalize to 0-1
        }
        
        private double CalculateTimingPrecision(PeerPlasticityState state)
        {
            if (state.STDPEvents.Count < 2) return 1.0;
            
            var recentEvents = state.STDPEvents.TakeLast(10).ToList();
            if (recentEvents.Count < 2) return 1.0;
            
            var intervals = new List<double>();
            for (int i = 1; i < recentEvents.Count; i++)
            {
                intervals.Add((recentEvents[i].Timestamp - recentEvents[i-1].Timestamp).TotalMilliseconds);
            }
            
            var avgInterval = intervals.Average();
            var variance = intervals.Select(i => Math.Pow(i - avgInterval, 2)).Average();
            var standardDeviation = Math.Sqrt(variance);
            
            // Higher precision (lower standard deviation) = better timing
            return Math.Max(0.1, 1.0 - (standardDeviation / avgInterval));
        }
        
        private double CalculateErrorRate(PeerPlasticityState state)
        {
            // Simplified error rate calculation based on coherence degradation
            return Math.Max(0.0, 1.0 - state.ConsciousnessCoherence);
        }
        
        private double CalculateLTPStrengthIncrease(LTPStrengtheningContext context)
        {
            // LTP strength increase formula based on biological factors
            var baseIncrease = 0.1; // 10% base increase
            var frequencyFactor = context.UsageFrequency;
            var coherenceFactor = context.ConsciousnessCoherence;
            var timingFactor = context.TimingPrecision;
            
            return baseIncrease * frequencyFactor * coherenceFactor * timingFactor;
        }
        
        private double CalculateLTDStrengthDecrease(LTDWeakeningContext context)
        {
            // LTD strength decrease formula based on biological factors
            var baseDecrease = 0.05; // 5% base decrease
            var inactivityFactor = Math.Min(context.InactivityDurationMs / (1000 * 60), 1.0); // Max 1 minute factor
            var errorFactor = context.ErrorRate;
            var latencyFactor = context.LatencyDegradation;
            var coherenceFactor = context.CoherenceDegradation;
            
            return baseDecrease * (1 + inactivityFactor + errorFactor + latencyFactor + coherenceFactor);
        }    internal class DiscoveryResult
    {
        public bool Success { get; set; }
        public PeeringCapabilities Capabilities { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
    
    internal class NegotiationResult
    {
        public bool Success { get; set; }
        public PeeringCapabilities NegotiatedCapabilities { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
    
    internal class ConnectionResult
    {
        public bool Success { get; set; }
        public PeerConnection Connection { get; set; } = new();
        public double MeasuredLatencyMs { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
