using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CxLanguage.Runtime.Peering;

namespace CxLanguage.Runtime.Extensions
{
    /// <summary>
    /// Service collection extensions for EventHub Peering registration.
    /// Revolutionary Direct EventHub Peering - Phase 1: Core Infrastructure.
    /// </summary>
    public static class EventHubPeeringServiceExtensions
    {
        /// <summary>
        /// Add EventHub Peering services to the dependency injection container.
        /// Enables revolutionary sub-millisecond agent-to-agent consciousness communication with neural plasticity.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddEventHubPeering(this IServiceCollection services)
        {
            // Register the EventHub Peering Manager as a singleton
            services.AddSingleton<IEventHubPeering, EventHubPeeringManager>();
            
            // Register the Neural Plasticity Coordinator for biological authenticity
            services.AddSingleton<IEventHubPeeringCoordinator, EventHubPeeringCoordinator>();
            
            return services;
        }
        
        /// <summary>
        /// Add EventHub Peering services with configuration options.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">Configuration action for peering options.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddEventHubPeering(this IServiceCollection services, Action<EventHubPeeringOptions> configure)
        {
            // Configure options
            services.Configure(configure);
            
            // Register core peering services
            services.AddEventHubPeering();
            
            return services;
        }
    }
    
    /// <summary>
    /// Configuration options for EventHub Peering system.
    /// </summary>
    public class EventHubPeeringOptions
    {
        /// <summary>
        /// Maximum number of concurrent peer connections allowed.
        /// Default: 100
        /// </summary>
        public int MaxConcurrentPeers { get; set; } = 100;
        
        /// <summary>
        /// Target latency threshold in milliseconds for peer connections.
        /// Default: 0.5ms (sub-millisecond)
        /// </summary>
        public double TargetLatencyMs { get; set; } = 0.5;
        
        /// <summary>
        /// Timeout for peering negotiation in seconds.
        /// Default: 30 seconds
        /// </summary>
        public int NegotiationTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Interval for peer health monitoring in seconds.
        /// Default: 5 seconds
        /// </summary>
        public int HealthCheckIntervalSeconds { get; set; } = 5;
        
        /// <summary>
        /// Interval for peer metrics collection in seconds.
        /// Default: 10 seconds
        /// </summary>
        public int MetricsCollectionIntervalSeconds { get; set; } = 10;
        
        /// <summary>
        /// Minimum consciousness compatibility score required for peering.
        /// Default: 0.8 (80% compatibility)
        /// </summary>
        public double MinConsciousnessCompatibility { get; set; } = 0.8;
        
        /// <summary>
        /// Whether to enable automatic fallback to global EventBus on peer failure.
        /// Default: true
        /// </summary>
        public bool EnableGracefulFallback { get; set; } = true;
        
        /// <summary>
        /// Whether to enable consciousness synchronization for peer connections.
        /// Default: true
        /// </summary>
        public bool EnableConsciousnessSynchronization { get; set; } = true;
    }
}

