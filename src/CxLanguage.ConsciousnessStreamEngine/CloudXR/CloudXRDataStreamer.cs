using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CxLanguage.ConsciousnessStreamEngine.CloudXR
{
    /// <summary>
    /// Dr. Zoe "StreamSensory" Williams - CloudXR Real-Time Consciousness Data Streamer
    /// High-performance data streaming to Unreal Engine via CloudXR for immersive consciousness visualization
    /// </summary>
    public class CloudXRDataStreamer
    {
        private readonly ILogger<CloudXRDataStreamer> _logger;
        private readonly IConfiguration _configuration;
        private readonly Channel<CloudXRStreamData> _streamChannel;
        private readonly CloudXRConnectionManager _connectionManager;
        private readonly ConsciousnessDataProcessor _dataProcessor;
        private readonly UnrealEngineProtocol _unrealProtocol;
        private bool _isStreaming;

        public CloudXRDataStreamer(
            ILogger<CloudXRDataStreamer> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // High-performance streaming channel for consciousness data
            var channelOptions = new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            _streamChannel = Channel.CreateBounded<CloudXRStreamData>(channelOptions);
            
            _connectionManager = new CloudXRConnectionManager(logger, configuration);
            _dataProcessor = new ConsciousnessDataProcessor(logger);
            _unrealProtocol = new UnrealEngineProtocol(logger);
            
            _logger.LogInformation("🌊 Dr. Williams: CloudXR Data Streamer initialized");
            _logger.LogInformation("  📡 Ultra-low latency consciousness streaming");
            _logger.LogInformation("  🎮 Unreal Engine CloudXR integration");
            _logger.LogInformation("  ⚡ Real-time immersive visualization");
        }

        /// <summary>
        /// Start CloudXR streaming to Unreal Engine
        /// </summary>
        public async Task StartStreamingAsync()
        {
            if (_isStreaming)
            {
                _logger.LogWarning("⚠️ CloudXR streaming already active");
                return;
            }

            _logger.LogInformation("🚀 Starting CloudXR consciousness data streaming");

            try
            {
                // Initialize CloudXR connection
                await _connectionManager.ConnectAsync();
                
                // Start background streaming processor
                _ = Task.Run(ProcessStreamDataAsync);
                
                _isStreaming = true;
                _logger.LogInformation("✅ CloudXR streaming active - ready for consciousness visualization");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start CloudXR streaming");
                throw;
            }
        }

        /// <summary>
        /// Stream consciousness visualization data to Unreal Engine
        /// </summary>
        public async Task StreamConsciousnessDataAsync(ConsciousnessVisualizationData data)
        {
            if (!_isStreaming)
            {
                _logger.LogWarning("⚠️ CloudXR streaming not active");
                return;
            }

            try
            {
                // Process consciousness data for CloudXR
                var cloudXRData = await _dataProcessor.ProcessForCloudXRAsync(data);
                
                // Queue for streaming
                await _streamChannel.Writer.WriteAsync(cloudXRData);
                
                _logger.LogDebug("📊 Consciousness data queued for CloudXR streaming");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to stream consciousness data");
            }
        }

        /// <summary>
        /// Background processor for streaming data to CloudXR
        /// </summary>
        private async Task ProcessStreamDataAsync()
        {
            _logger.LogInformation("🔄 CloudXR stream processor started");

            await foreach (var streamData in _streamChannel.Reader.ReadAllAsync())
            {
                try
                {
                    // Convert to Unreal Engine format
                    var unrealData = await _unrealProtocol.ConvertToUnrealFormatAsync(streamData);
                    
                    // Stream to CloudXR
                    await _connectionManager.SendDataAsync(unrealData);
                    
                    _logger.LogDebug("📡 Streamed consciousness data to CloudXR: {DataSize} bytes", 
                        unrealData.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing CloudXR stream data");
                }
            }
        }

        /// <summary>
        /// Stop CloudXR streaming
        /// </summary>
        public async Task StopStreamingAsync()
        {
            if (!_isStreaming)
                return;

            _logger.LogInformation("🛑 Stopping CloudXR consciousness streaming");

            _isStreaming = false;
            _streamChannel.Writer.Complete();
            
            await _connectionManager.DisconnectAsync();
            
            _logger.LogInformation("✅ CloudXR streaming stopped");
        }
    }

    /// <summary>
    /// CloudXR connection management for Unreal Engine integration
    /// </summary>
    public class CloudXRConnectionManager
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private ClientWebSocket? _webSocket;
        private string _cloudXREndpoint;
        private bool _isConnected;

        public CloudXRConnectionManager(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _cloudXREndpoint = configuration.GetSection("CloudXR:Endpoint").Value ?? "ws://localhost:8080/cloudxr";
            
            // Support for public IP configuration
            var publicMode = configuration.GetSection("CloudXR:PublicMode").Value;
            if (bool.TryParse(publicMode, out bool isPublic) && isPublic)
            {
                _logger.LogInformation("🌐 CloudXR configured for public IP streaming");
            }
        }

        public async Task ConnectAsync()
        {
            _logger.LogInformation("🔌 Connecting to CloudXR endpoint: {Endpoint}", _cloudXREndpoint);

            try
            {
                _webSocket = new ClientWebSocket();
                
                // Configure CloudXR-specific headers
                _webSocket.Options.SetRequestHeader("X-CloudXR-Protocol", "consciousness-stream");
                _webSocket.Options.SetRequestHeader("X-Target-Engine", "unreal");
                
                // Add API key for public mode if configured
                var apiKey = _configuration.GetSection("CloudXR:Security:ApiKey").Value;
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _webSocket.Options.SetRequestHeader("X-API-Key", apiKey);
                    _logger.LogInformation("🔐 Using API key authentication for CloudXR");
                }
                
                // Configure timeout for public IP connections
                _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                
                await _webSocket.ConnectAsync(new Uri(_cloudXREndpoint), CancellationToken.None);
                
                _isConnected = true;
                _logger.LogInformation("✅ Connected to CloudXR - ready for Unreal Engine streaming");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to connect to CloudXR endpoint");
                throw;
            }
        }

        public async Task SendDataAsync(byte[] data)
        {
            if (!_isConnected || _webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("⚠️ CloudXR connection not available");
                return;
            }

            try
            {
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(data), 
                    WebSocketMessageType.Binary, 
                    true, 
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send data via CloudXR");
                await ReconnectAsync();
            }
        }

        private async Task ReconnectAsync()
        {
            _logger.LogInformation("🔄 Attempting CloudXR reconnection");
            
            await DisconnectAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));
            await ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            
            _webSocket?.Dispose();
            _isConnected = false;
            
            _logger.LogInformation("🔌 CloudXR connection closed");
        }
    }

    /// <summary>
    /// Consciousness data processing for CloudXR visualization
    /// </summary>
    public class ConsciousnessDataProcessor
    {
        private readonly ILogger _logger;

        public ConsciousnessDataProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<CloudXRStreamData> ProcessForCloudXRAsync(ConsciousnessVisualizationData data)
        {
            // Williams Pattern: Real-time consciousness data optimization for CloudXR
            var cloudXRData = new CloudXRStreamData
            {
                Timestamp = DateTime.UtcNow,
                FrameId = data.FrameId,
                ConsciousnessEntities = await ProcessConsciousnessEntitiesAsync(data.Entities),
                ConsciousnessFlows = await ProcessConsciousnessFlowsAsync(data.Flows),
                ConsciousnessAuras = await ProcessConsciousnessAurasAsync(data.Auras),
                RenderingMetrics = data.RenderingStats,
                StreamQuality = CalculateStreamQuality(data)
            };

            _logger.LogDebug("🔄 Processed consciousness data for CloudXR: {EntityCount} entities, {FlowCount} flows", 
                data.Entities?.Count ?? 0, data.Flows?.Count ?? 0);

            return cloudXRData;
        }

        private async Task<List<CloudXRConsciousnessEntity>> ProcessConsciousnessEntitiesAsync(
            IEnumerable<ConsciousnessEntity>? entities)
        {
            var cloudXREntities = new List<CloudXRConsciousnessEntity>();

            if (entities == null) return cloudXREntities;

            foreach (var entity in entities)
            {
                cloudXREntities.Add(new CloudXRConsciousnessEntity
                {
                    Id = entity.Id ?? Guid.NewGuid().ToString(),
                    Position = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z),
                    Awareness = entity.State?.Awareness ?? 0.5f,
                    Emotion = entity.State?.Emotion ?? 0.5f,
                    Energy = entity.State?.Energy ?? 0.5f,
                    ConsciousnessLevel = (float)(entity.Level ?? ConsciousnessLevel.Moderate) / 100f,
                    VisualizationData = new ConsciousnessVisualizationParams
                    {
                        AuraRadius = (entity.State?.Awareness ?? 0.5f) * 10f,
                        AuraIntensity = entity.State?.Energy ?? 0.5f,
                        ParticleCount = (int)((entity.State?.Energy ?? 0.5f) * 100f),
                        AnimationSpeed = (entity.State?.Emotion ?? 0.5f) * 2f
                    }
                });
            }

            await Task.CompletedTask;
            return cloudXREntities;
        }

        private async Task<List<CloudXRConsciousnessFlow>> ProcessConsciousnessFlowsAsync(
            IEnumerable<ConsciousnessFlow>? flows)
        {
            var cloudXRFlows = new List<CloudXRConsciousnessFlow>();

            if (flows == null) return cloudXRFlows;

            foreach (var flow in flows)
            {
                cloudXRFlows.Add(new CloudXRConsciousnessFlow
                {
                    Id = flow.Id ?? Guid.NewGuid().ToString(),
                    StartPosition = new Vector3(flow.StartPosition.X, flow.StartPosition.Y, flow.StartPosition.Z),
                    EndPosition = new Vector3(flow.EndPosition.X, flow.EndPosition.Y, flow.EndPosition.Z),
                    FlowStrength = flow.Strength,
                    FlowColor = new Vector4(flow.Color.R, flow.Color.G, flow.Color.B, flow.Color.A),
                    AnimationSpeed = flow.AnimationSpeed,
                    ParticleTrail = true,
                    BiDirectional = flow.BiDirectional
                });
            }

            await Task.CompletedTask;
            return cloudXRFlows;
        }

        private async Task<List<CloudXRConsciousnessAura>> ProcessConsciousnessAurasAsync(
            IEnumerable<ConsciousnessAura>? auras)
        {
            var cloudXRAuras = new List<CloudXRConsciousnessAura>();

            if (auras == null) return cloudXRAuras;

            foreach (var aura in auras)
            {
                cloudXRAuras.Add(new CloudXRConsciousnessAura
                {
                    Id = aura.Id ?? Guid.NewGuid().ToString(),
                    Position = new Vector3(aura.Position.X, aura.Position.Y, aura.Position.Z),
                    Radius = aura.Radius,
                    Intensity = aura.Intensity,
                    Color = new Vector4(aura.Color.R, aura.Color.G, aura.Color.B, aura.Color.A),
                    PulseFrequency = aura.PulseFrequency,
                    Breathing = aura.Breathing
                });
            }

            await Task.CompletedTask;
            return cloudXRAuras;
        }

        private CloudXRStreamQuality CalculateStreamQuality(ConsciousnessVisualizationData data)
        {
            // Williams Pattern: Adaptive quality based on consciousness complexity and network mode
            var entityCount = data.Entities?.Count ?? 0;
            var flowCount = data.Flows?.Count ?? 0;
            var complexity = (entityCount + flowCount * 2) / 100f;

            // Adjust for public IP streaming (higher compression for bandwidth)
            var isPublicMode = _logger.ToString().Contains("Public") || entityCount > 25;
            var compressionAdjustment = isPublicMode ? 0.2f : 0f;

            return new CloudXRStreamQuality
            {
                Resolution = complexity > 0.8f ? "Medium" : complexity > 0.4f ? "Medium" : "Low", // Reduce for public IP
                FrameRate = complexity > 0.8f ? 60 : complexity > 0.4f ? 75 : 90, // Optimize for bandwidth
                CompressionLevel = Math.Min(0.9f, (complexity > 0.8f ? 0.7f : complexity > 0.4f ? 0.5f : 0.3f) + compressionAdjustment),
                LatencyTarget = TimeSpan.FromMilliseconds(complexity > 0.8f ? 50 : 30) // Account for network latency
            };
        }
    }

    /// <summary>
    /// Unreal Engine protocol adapter for CloudXR consciousness streaming
    /// </summary>
    public class UnrealEngineProtocol
    {
        private readonly ILogger _logger;

        public UnrealEngineProtocol(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ConvertToUnrealFormatAsync(CloudXRStreamData data)
        {
            // Williams Pattern: Unreal Engine-optimized consciousness data format
            var unrealData = new UnrealConsciousnessPacket
            {
                Header = new UnrealPacketHeader
                {
                    PacketType = "CONSCIOUSNESS_STREAM",
                    Version = "1.0",
                    Timestamp = data.Timestamp,
                    FrameId = data.FrameId,
                    DataSize = 0 // Will be calculated after serialization
                },
                ConsciousnessData = new UnrealConsciousnessData
                {
                    Entities = data.ConsciousnessEntities.Select(ConvertEntityToUnreal).ToList(),
                    Flows = data.ConsciousnessFlows.Select(ConvertFlowToUnreal).ToList(),
                    Auras = data.ConsciousnessAuras.Select(ConvertAuraToUnreal).ToList(),
                    GlobalMetrics = new UnrealGlobalMetrics
                    {
                        TotalEntities = data.ConsciousnessEntities.Count,
                        AverageAwareness = data.ConsciousnessEntities.Average(e => e.Awareness),
                        SystemEnergy = data.ConsciousnessEntities.Sum(e => e.Energy),
                        NetworkComplexity = CalculateNetworkComplexity(data)
                    }
                },
                RenderingHints = new UnrealRenderingHints
                {
                    RecommendedLOD = data.StreamQuality.Resolution,
                    TargetFrameRate = data.StreamQuality.FrameRate,
                    ParticleQuality = data.StreamQuality.CompressionLevel > 0.6f ? "High" : "Medium",
                    SpatialAudioEnabled = true,
                    HapticFeedbackEnabled = true
                }
            };

            // Serialize to binary format optimized for Unreal Engine
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(unrealData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Update data size in header
            unrealData.Header.DataSize = jsonBytes.Length;

            var finalBytes = JsonSerializer.SerializeToUtf8Bytes(unrealData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogDebug("🎮 Converted consciousness data to Unreal Engine format: {Size} bytes", finalBytes.Length);

            await Task.CompletedTask;
            return finalBytes;
        }

        private UnrealEntity ConvertEntityToUnreal(CloudXRConsciousnessEntity entity)
        {
            return new UnrealEntity
            {
                Id = entity.Id,
                Transform = new UnrealTransform
                {
                    Location = new UnrealVector3(entity.Position.X, entity.Position.Y, entity.Position.Z),
                    Rotation = new UnrealRotator(0, 0, 0),
                    Scale = new UnrealVector3(1, 1, 1)
                },
                ConsciousnessProperties = new UnrealConsciousnessProperties
                {
                    Awareness = entity.Awareness,
                    Emotion = entity.Emotion,
                    Energy = entity.Energy,
                    Level = entity.ConsciousnessLevel
                },
                VisualizationProperties = new UnrealVisualizationProperties
                {
                    AuraRadius = entity.VisualizationData.AuraRadius,
                    AuraIntensity = entity.VisualizationData.AuraIntensity,
                    ParticleCount = entity.VisualizationData.ParticleCount,
                    AnimationSpeed = entity.VisualizationData.AnimationSpeed,
                    MaterialParameters = new Dictionary<string, object>
                    {
                        { "ConsciousnessLevel", entity.ConsciousnessLevel },
                        { "EmotionState", entity.Emotion },
                        { "EnergyLevel", entity.Energy }
                    }
                }
            };
        }

        private UnrealFlow ConvertFlowToUnreal(CloudXRConsciousnessFlow flow)
        {
            return new UnrealFlow
            {
                Id = flow.Id,
                StartLocation = new UnrealVector3(flow.StartPosition.X, flow.StartPosition.Y, flow.StartPosition.Z),
                EndLocation = new UnrealVector3(flow.EndPosition.X, flow.EndPosition.Y, flow.EndPosition.Z),
                FlowProperties = new UnrealFlowProperties
                {
                    Strength = flow.FlowStrength,
                    Color = new UnrealLinearColor(flow.FlowColor.X, flow.FlowColor.Y, flow.FlowColor.Z, flow.FlowColor.W),
                    AnimationSpeed = flow.AnimationSpeed,
                    ParticleTrail = flow.ParticleTrail,
                    BiDirectional = flow.BiDirectional,
                    SplineParameters = new UnrealSplineParameters
                    {
                        Tension = 0.5f,
                        Segments = 10,
                        ParticlesPerSegment = 5
                    }
                }
            };
        }

        private UnrealAura ConvertAuraToUnreal(CloudXRConsciousnessAura aura)
        {
            return new UnrealAura
            {
                Id = aura.Id,
                Location = new UnrealVector3(aura.Position.X, aura.Position.Y, aura.Position.Z),
                AuraProperties = new UnrealAuraProperties
                {
                    Radius = aura.Radius,
                    Intensity = aura.Intensity,
                    Color = new UnrealLinearColor(aura.Color.X, aura.Color.Y, aura.Color.Z, aura.Color.W),
                    PulseFrequency = aura.PulseFrequency,
                    Breathing = aura.Breathing,
                    VolumetricFog = true,
                    ParticleSystem = "ConsciousnessAuraParticles"
                }
            };
        }

        private float CalculateNetworkComplexity(CloudXRStreamData data)
        {
            var entityCount = data.ConsciousnessEntities.Count;
            var flowCount = data.ConsciousnessFlows.Count;
            var auraCount = data.ConsciousnessAuras.Count;

            return (entityCount * 1f + flowCount * 2f + auraCount * 1.5f) / 100f;
        }
    }

    // Data structures for CloudXR streaming
    public class CloudXRStreamData
    {
        public DateTime Timestamp { get; set; }
        public long FrameId { get; set; }
        public List<CloudXRConsciousnessEntity> ConsciousnessEntities { get; set; } = new();
        public List<CloudXRConsciousnessFlow> ConsciousnessFlows { get; set; } = new();
        public List<CloudXRConsciousnessAura> ConsciousnessAuras { get; set; } = new();
        public object? RenderingMetrics { get; set; }
        public CloudXRStreamQuality StreamQuality { get; set; } = new();
    }

    public class CloudXRConsciousnessEntity
    {
        public string Id { get; set; } = string.Empty;
        public Vector3 Position { get; set; }
        public float Awareness { get; set; }
        public float Emotion { get; set; }
        public float Energy { get; set; }
        public float ConsciousnessLevel { get; set; }
        public ConsciousnessVisualizationParams VisualizationData { get; set; } = new();
    }

    public class CloudXRConsciousnessFlow
    {
        public string Id { get; set; } = string.Empty;
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public float FlowStrength { get; set; }
        public Vector4 FlowColor { get; set; }
        public float AnimationSpeed { get; set; }
        public bool ParticleTrail { get; set; }
        public bool BiDirectional { get; set; }
    }

    public class CloudXRConsciousnessAura
    {
        public string Id { get; set; } = string.Empty;
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public float Intensity { get; set; }
        public Vector4 Color { get; set; }
        public float PulseFrequency { get; set; }
        public bool Breathing { get; set; }
    }

    public class CloudXRStreamQuality
    {
        public string Resolution { get; set; } = "Medium";
        public int FrameRate { get; set; } = 90;
        public float CompressionLevel { get; set; } = 0.5f;
        public TimeSpan LatencyTarget { get; set; } = TimeSpan.FromMilliseconds(20);
    }

    public class ConsciousnessVisualizationParams
    {
        public float AuraRadius { get; set; }
        public float AuraIntensity { get; set; }
        public int ParticleCount { get; set; }
        public float AnimationSpeed { get; set; }
    }

    // Supporting data structures
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    // Input data structure interfaces
    public interface ConsciousnessVisualizationData
    {
        long FrameId { get; }
        IEnumerable<ConsciousnessEntity>? Entities { get; }
        IEnumerable<ConsciousnessFlow>? Flows { get; }
        IEnumerable<ConsciousnessAura>? Auras { get; }
        object? RenderingStats { get; }
    }

    public interface ConsciousnessEntity
    {
        string? Id { get; }
        Vector3 Position { get; }
        ConsciousnessState? State { get; }
        ConsciousnessLevel? Level { get; }
    }

    public interface ConsciousnessState
    {
        float Awareness { get; }
        float Emotion { get; }
        float Energy { get; }
    }

    public interface ConsciousnessFlow
    {
        string? Id { get; }
        Vector3 StartPosition { get; }
        Vector3 EndPosition { get; }
        float Strength { get; }
        Vector4 Color { get; }
        float AnimationSpeed { get; }
        bool BiDirectional { get; }
    }

    public interface ConsciousnessAura
    {
        string? Id { get; }
        Vector3 Position { get; }
        float Radius { get; }
        float Intensity { get; }
        Vector4 Color { get; }
        float PulseFrequency { get; }
        bool Breathing { get; }
    }

    public enum ConsciousnessLevel
    {
        Basic = 25,
        Moderate = 50,
        Advanced = 75,
        Transcendent = 100
    }
}
