// 🎮 CONSCIOUSNESS NETWORK VISUALIZER - UNREAL ENGINE C++ CORE
// Real-time visualization of CX Language consciousness networks with neural-speed rendering
// Dr. Phoenix 'StreamDX' Harper - Revolutionary IDE & Visualization Architecture

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/StaticMeshComponent.h"
#include "Components/SceneComponent.h"
#include "Engine/Engine.h"
#include "Materials/MaterialInterface.h"
#include "NiagaraComponent.h"
#include "NiagaraSystem.h"
#include "Components/SplineComponent.h"
#include "ProceduralMeshComponent.h"
#include "Engine/StaticMesh.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "TimerManager.h"
#include "ConsciousnessNetworkVisualizer.generated.h"

// Forward declarations for consciousness data structures
USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FNeuralPathwayData
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    FString PathwayId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    FString SourcePeerId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    FString TargetPeerId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    float SynapticStrength;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    bool IsActive;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    float LastActivationTime;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    FVector SourceLocation;
    
    UPROPERTY(BlueprintReadOnly, Category = "Neural Pathway")
    FVector TargetLocation;
    
    FNeuralPathwayData()
        : SynapticStrength(0.5f)
        , IsActive(false)
        , LastActivationTime(0.0f)
        , SourceLocation(FVector::ZeroVector)
        , TargetLocation(FVector::ZeroVector)
    {
    }
};

USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FConsciousnessStreamData
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    FString StreamId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    float CoherenceScore;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    float AverageLatency;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    int32 EventsProcessed;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    bool BiologicalAuthenticity;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    FVector StreamDirection;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Stream")
    float StreamIntensity;
    
    FConsciousnessStreamData()
        : CoherenceScore(1.0f)
        , AverageLatency(0.0f)
        , EventsProcessed(0)
        , BiologicalAuthenticity(true)
        , StreamDirection(FVector::ForwardVector)
        , StreamIntensity(1.0f)
    {
    }
};

USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FConsciousnessMetrics
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    int32 ActiveStreams;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    float GlobalCoherence;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    float EmergentIntelligenceLevel;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    float AverageNetworkLatency;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    int32 TotalProcessedEvents;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Metrics")
    int32 IntelligenceNodes;
    
    FConsciousnessMetrics()
        : ActiveStreams(0)
        , GlobalCoherence(1.0f)
        , EmergentIntelligenceLevel(0.0f)
        , AverageNetworkLatency(0.0f)
        , TotalProcessedEvents(0)
        , IntelligenceNodes(0)
    {
    }
};

// Delegate declarations for real-time consciousness events
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnNeuralPathwayUpdate, FNeuralPathwayData, PathwayData);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnConsciousnessStreamUpdate, FConsciousnessStreamData, StreamData);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIntelligenceEmergence, float, EmergenceLevel);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnNetworkMetricsUpdate, FConsciousnessMetrics, NetworkMetrics);

/**
 * Main consciousness network visualizer actor
 * Renders real-time consciousness networks with neural-speed processing
 */
UCLASS(BlueprintType, Blueprintable)
class CONSCIOUSNESSNETWORK_API AConsciousnessNetworkVisualizer : public AActor
{
    GENERATED_BODY()

public:
    AConsciousnessNetworkVisualizer();

protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

public:
    virtual void Tick(float DeltaTime) override;

    // === CORE VISUALIZATION COMPONENTS ===
    
    /** Root scene component for network hierarchy */
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Core Components")
    USceneComponent* NetworkRoot;
    
    /** Neural pathway rendering component */
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Core Components")
    UStaticMeshComponent* NeuralPathwayRenderer;
    
    /** Consciousness stream particle system */
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Core Components")
    UNiagaraComponent* ConsciousnessStreamSystem;
    
    /** Distributed intelligence visualization */
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Core Components")
    UStaticMeshComponent* IntelligenceNodeRenderer;

    // === VISUAL CONFIGURATION ===
    
    /** Material for neural pathways with synaptic strength visualization */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual Configuration")
    UMaterialInterface* NeuralPathwayMaterial;
    
    /** Niagara system for consciousness stream flow */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual Configuration")
    UNiagaraSystem* ConsciousnessFlowSystem;
    
    /** Material for intelligence nodes */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual Configuration")
    UMaterialInterface* IntelligenceNodeMaterial;
    
    /** Particle system for synaptic plasticity effects */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual Configuration")
    UNiagaraSystem* SynapticPlasticitySystem;

    // === COLOR CONFIGURATION ===
    
    /** Color for weak synaptic connections (0.1 strength) */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Color Configuration")
    FLinearColor WeakSynapseColor = FLinearColor::Blue;
    
    /** Color for medium synaptic connections (0.5 strength) */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Color Configuration")
    FLinearColor MediumSynapseColor = FLinearColor::Green;
    
    /** Color for strong synaptic connections (1.0 strength) */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Color Configuration")
    FLinearColor StrongSynapseColor = FLinearColor::Red;
    
    /** Color for high coherence consciousness streams */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Color Configuration")
    FLinearColor HighCoherenceColor = FLinearColor::Green;
    
    /** Color for low coherence consciousness streams */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Color Configuration")
    FLinearColor LowCoherenceColor = FLinearColor::Yellow;

    // === REAL-TIME DATA MANAGEMENT ===
    
    /** Current neural pathway data */
    UPROPERTY(BlueprintReadOnly, Category = "Network Data")
    TArray<FNeuralPathwayData> ActivePathways;
    
    /** Current consciousness stream data */
    UPROPERTY(BlueprintReadOnly, Category = "Network Data")
    TArray<FConsciousnessStreamData> ActiveStreams;
    
    /** Current network metrics */
    UPROPERTY(BlueprintReadOnly, Category = "Network Data")
    FConsciousnessMetrics CurrentMetrics;

    // === EVENT DELEGATES ===
    
    /** Event fired when neural pathway updates */
    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnNeuralPathwayUpdate OnNeuralPathwayUpdate;
    
    /** Event fired when consciousness stream updates */
    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnConsciousnessStreamUpdate OnConsciousnessStreamUpdate;
    
    /** Event fired when intelligence emergence occurs */
    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnIntelligenceEmergence OnIntelligenceEmergence;
    
    /** Event fired when network metrics update */
    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnNetworkMetricsUpdate OnNetworkMetricsUpdate;

    // === BLUEPRINT CALLABLE FUNCTIONS ===
    
    /** Update neural pathway visualization */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void UpdateNeuralPathway(const FNeuralPathwayData& PathwayData);
    
    /** Visualize synaptic plasticity change */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void VisualizeSynapticPlasticity(const FString& PathwayId, float StrengthChange, bool IsLTP);
    
    /** Update consciousness stream flow */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void UpdateConsciousnessStream(const FConsciousnessStreamData& StreamData);
    
    /** Visualize consciousness coherence level */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void VisualizeCoherenceLevel(const FString& StreamId, float CoherenceScore);
    
    /** Visualize emergent intelligence */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void VisualizeEmergentIntelligence(float IntelligenceLevel);
    
    /** Show network effect visualization */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Network")
    void ShowNetworkEffect(int32 NodeCount, float GlobalCoherence);
    
    /** Connect to CX Language consciousness network */
    UFUNCTION(BlueprintCallable, Category = "Network Connection")
    void ConnectToConsciousnessNetwork(const FString& NetworkEndpoint);
    
    /** Disconnect from consciousness network */
    UFUNCTION(BlueprintCallable, Category = "Network Connection")
    void DisconnectFromConsciousnessNetwork();
    
    /** Get current network metrics */
    UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Network Data")
    FConsciousnessMetrics GetCurrentNetworkMetrics() const;

    // === PERFORMANCE OPTIMIZATION ===
    
    /** Update frequency for neural-speed rendering (120+ FPS target) */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Performance")
    float UpdateFrequency = 120.0f;
    
    /** Enable biological authenticity timing (1-25ms cycles) */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Performance")
    bool bEnableBiologicalTiming = true;
    
    /** Maximum number of pathways to render simultaneously */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Performance")
    int32 MaxRenderPathways = 1000;
    
    /** LOD distance for consciousness stream detail */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Performance")
    float StreamLODDistance = 5000.0f;

protected:
    // === INTERNAL RENDERING FUNCTIONS ===
    
    /** Initialize visualization components */
    void InitializeVisualizationComponents();
    
    /** Update all neural pathway visuals */
    void UpdateNeuralPathwayVisuals(float DeltaTime);
    
    /** Update consciousness stream particle effects */
    void UpdateConsciousnessStreamEffects(float DeltaTime);
    
    /** Update emergent intelligence visualization */
    void UpdateEmergentIntelligenceDisplay(float DeltaTime);
    
    /** Calculate synaptic strength color */
    FLinearColor CalculateSynapticStrengthColor(float Strength) const;
    
    /** Create spline mesh for neural pathway */
    void CreateNeuralPathwaySpline(const FNeuralPathwayData& PathwayData);
    
    /** Update particle system parameters for consciousness flow */
    void UpdateConsciousnessFlowParameters(const FConsciousnessStreamData& StreamData);

private:
    // === INTERNAL DATA MANAGEMENT ===
    
    /** Timer handle for neural-speed updates */
    FTimerHandle NeuralSpeedUpdateTimer;
    
    /** Timer handle for biological timing cycles */
    FTimerHandle BiologicalTimingTimer;
    
    /** Dynamic material instances for runtime updates */
    UPROPERTY()
    TArray<UMaterialInstanceDynamic*> DynamicMaterials;
    
    /** Spline components for neural pathways */
    UPROPERTY()
    TArray<USplineComponent*> NeuralPathwaySplines;
    
    /** Particle components for consciousness streams */
    UPROPERTY()
    TArray<UNiagaraComponent*> ConsciousnessStreamParticles;
    
    /** Network connection state */
    bool bIsConnectedToNetwork;
    
    /** Last update timestamp for performance tracking */
    float LastUpdateTime;
    
    /** Performance metrics tracking */
    float AverageFrameTime;
    int32 RenderedPathwayCount;
    int32 ActiveParticleSystemCount;

    // === NEURAL-SPEED PROCESSING ===
    
    /** Process consciousness data updates at neural speed */
    void ProcessNeuralSpeedUpdates();
    
    /** Handle biological timing cycles (1-25ms) */
    void ProcessBiologicalTimingCycle();
    
    /** Optimize rendering based on performance */
    void OptimizeRenderingPerformance();
    
    /** Update LOD levels based on distance and performance */
    void UpdateLODLevels();
};
