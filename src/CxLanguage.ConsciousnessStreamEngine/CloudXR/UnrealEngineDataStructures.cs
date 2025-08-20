using System;
using System.Collections.Generic;

namespace CxLanguage.ConsciousnessStreamEngine.CloudXR
{
    /// <summary>
    /// Unreal Engine data structures for CloudXR consciousness streaming
    /// Optimized for Unreal Engine 5.x Blueprint and C++ integration
    /// </summary>

    // Root packet structure for Unreal Engine consumption
    public class UnrealConsciousnessPacket
    {
        public UnrealPacketHeader Header { get; set; } = new();
        public UnrealConsciousnessData ConsciousnessData { get; set; } = new();
        public UnrealRenderingHints RenderingHints { get; set; } = new();
    }

    public class UnrealPacketHeader
    {
        public string PacketType { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public long FrameId { get; set; }
        public int DataSize { get; set; }
    }

    public class UnrealConsciousnessData
    {
        public List<UnrealEntity> Entities { get; set; } = new();
        public List<UnrealFlow> Flows { get; set; } = new();
        public List<UnrealAura> Auras { get; set; } = new();
        public UnrealGlobalMetrics GlobalMetrics { get; set; } = new();
    }

    public class UnrealRenderingHints
    {
        public string RecommendedLOD { get; set; } = "Medium";
        public int TargetFrameRate { get; set; } = 90;
        public string ParticleQuality { get; set; } = "Medium";
        public bool SpatialAudioEnabled { get; set; } = true;
        public bool HapticFeedbackEnabled { get; set; } = true;
    }

    // Unreal Engine entity representation
    public class UnrealEntity
    {
        public string Id { get; set; } = string.Empty;
        public UnrealTransform Transform { get; set; } = new();
        public UnrealConsciousnessProperties ConsciousnessProperties { get; set; } = new();
        public UnrealVisualizationProperties VisualizationProperties { get; set; } = new();
    }

    public class UnrealTransform
    {
        public UnrealVector3 Location { get; set; } = new();
        public UnrealRotator Rotation { get; set; } = new();
        public UnrealVector3 Scale { get; set; } = new(1, 1, 1);
    }

    public class UnrealConsciousnessProperties
    {
        public float Awareness { get; set; }
        public float Emotion { get; set; }
        public float Energy { get; set; }
        public float Level { get; set; }
    }

    public class UnrealVisualizationProperties
    {
        public float AuraRadius { get; set; }
        public float AuraIntensity { get; set; }
        public int ParticleCount { get; set; }
        public float AnimationSpeed { get; set; }
        public Dictionary<string, object> MaterialParameters { get; set; } = new();
    }

    // Consciousness flow representation for Unreal Engine
    public class UnrealFlow
    {
        public string Id { get; set; } = string.Empty;
        public UnrealVector3 StartLocation { get; set; } = new();
        public UnrealVector3 EndLocation { get; set; } = new();
        public UnrealFlowProperties FlowProperties { get; set; } = new();
    }

    public class UnrealFlowProperties
    {
        public float Strength { get; set; }
        public UnrealLinearColor Color { get; set; } = new();
        public float AnimationSpeed { get; set; }
        public bool ParticleTrail { get; set; }
        public bool BiDirectional { get; set; }
        public UnrealSplineParameters SplineParameters { get; set; } = new();
    }

    public class UnrealSplineParameters
    {
        public float Tension { get; set; } = 0.5f;
        public int Segments { get; set; } = 10;
        public int ParticlesPerSegment { get; set; } = 5;
    }

    // Consciousness aura representation for Unreal Engine
    public class UnrealAura
    {
        public string Id { get; set; } = string.Empty;
        public UnrealVector3 Location { get; set; } = new();
        public UnrealAuraProperties AuraProperties { get; set; } = new();
    }

    public class UnrealAuraProperties
    {
        public float Radius { get; set; }
        public float Intensity { get; set; }
        public UnrealLinearColor Color { get; set; } = new();
        public float PulseFrequency { get; set; }
        public bool Breathing { get; set; }
        public bool VolumetricFog { get; set; }
        public string ParticleSystem { get; set; } = string.Empty;
    }

    // Global consciousness metrics for Unreal Engine
    public class UnrealGlobalMetrics
    {
        public int TotalEntities { get; set; }
        public double AverageAwareness { get; set; }
        public double SystemEnergy { get; set; }
        public float NetworkComplexity { get; set; }
    }

    // Unreal Engine basic data types
    public struct UnrealVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public UnrealVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct UnrealRotator
    {
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }

        public UnrealRotator(float pitch, float yaw, float roll)
        {
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
        }
    }

    public struct UnrealLinearColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public UnrealLinearColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
