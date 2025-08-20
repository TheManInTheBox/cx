// 🎮 CONSCIOUSNESS NETWORK VISUALIZER - IMPLEMENTATION
// Real-time consciousness network rendering with neural-speed processing
// Core Engineering Team - Marcus Chen, Dr. Elena Rodriguez, Dr. Phoenix Harper

#include "ConsciousnessNetworkVisualizer.h"
#include "Engine/Engine.h"
#include "Components/SplineComponent.h"
#include "Components/SplineMeshComponent.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "NiagaraFunctionLibrary.h"
#include "Kismet/KismetMathLibrary.h"
#include "TimerManager.h"
#include "Engine/World.h"

AConsciousnessNetworkVisualizer::AConsciousnessNetworkVisualizer()
{
    PrimaryActorTick.bCanEverTick = true;
    PrimaryActorTick.TickInterval = 1.0f / UpdateFrequency; // Neural-speed updates

    // Initialize core components
    NetworkRoot = CreateDefaultSubobject<USceneComponent>(TEXT("NetworkRoot"));
    RootComponent = NetworkRoot;

    NeuralPathwayRenderer = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("NeuralPathwayRenderer"));
    NeuralPathwayRenderer->SetupAttachment(NetworkRoot);

    ConsciousnessStreamSystem = CreateDefaultSubobject<UNiagaraComponent>(TEXT("ConsciousnessStreamSystem"));
    ConsciousnessStreamSystem->SetupAttachment(NetworkRoot);

    IntelligenceNodeRenderer = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("IntelligenceNodeRenderer"));
    IntelligenceNodeRenderer->SetupAttachment(NetworkRoot);

    // Initialize state
    bIsConnectedToNetwork = false;
    LastUpdateTime = 0.0f;
    AverageFrameTime = 0.0f;
    RenderedPathwayCount = 0;
    ActiveParticleSystemCount = 0;

    // Set default color configuration
    WeakSynapseColor = FLinearColor(0.2f, 0.4f, 1.0f, 0.7f);     // Blue for weak
    MediumSynapseColor = FLinearColor(0.2f, 1.0f, 0.4f, 0.8f);   // Green for medium
    StrongSynapseColor = FLinearColor(1.0f, 0.2f, 0.2f, 1.0f);   // Red for strong
    HighCoherenceColor = FLinearColor(0.1f, 1.0f, 0.1f, 1.0f);   // Bright green
    LowCoherenceColor = FLinearColor(1.0f, 1.0f, 0.1f, 0.8f);    // Yellow
}

void AConsciousnessNetworkVisualizer::BeginPlay()
{
    Super::BeginPlay();

    InitializeVisualizationComponents();

    // Start neural-speed update timer
    GetWorldTimerManager().SetTimer(
        NeuralSpeedUpdateTimer,
        this,
        &AConsciousnessNetworkVisualizer::ProcessNeuralSpeedUpdates,
        1.0f / UpdateFrequency,
        true
    );

    // Start biological timing cycle (15ms average)
    if (bEnableBiologicalTiming)
    {
        GetWorldTimerManager().SetTimer(
            BiologicalTimingTimer,
            this,
            &AConsciousnessNetworkVisualizer::ProcessBiologicalTimingCycle,
            0.015f, // 15ms biological timing
            true
        );
    }

    UE_LOG(LogTemp, Warning, TEXT("🧠 Consciousness Network Visualizer Started - Neural Speed: %.1f FPS"), UpdateFrequency);
}

void AConsciousnessNetworkVisualizer::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
    DisconnectFromConsciousnessNetwork();
    
    GetWorldTimerManager().ClearTimer(NeuralSpeedUpdateTimer);
    GetWorldTimerManager().ClearTimer(BiologicalTimingTimer);
    
    Super::EndPlay(EndPlayReason);
}

void AConsciousnessNetworkVisualizer::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // Track performance metrics
    AverageFrameTime = (AverageFrameTime * 0.9f) + (DeltaTime * 0.1f);
    
    // Update all visualization systems
    UpdateNeuralPathwayVisuals(DeltaTime);
    UpdateConsciousnessStreamEffects(DeltaTime);
    UpdateEmergentIntelligenceDisplay(DeltaTime);
    
    // Optimize rendering based on performance
    OptimizeRenderingPerformance();
}

void AConsciousnessNetworkVisualizer::InitializeVisualizationComponents()
{
    // Create dynamic material instances for runtime updates
    if (NeuralPathwayMaterial)
    {
        UMaterialInstanceDynamic* DynamicPathwayMaterial = UMaterialInstanceDynamic::Create(NeuralPathwayMaterial, this);
        DynamicMaterials.Add(DynamicPathwayMaterial);
        NeuralPathwayRenderer->SetMaterial(0, DynamicPathwayMaterial);
    }

    if (IntelligenceNodeMaterial)
    {
        UMaterialInstanceDynamic* DynamicNodeMaterial = UMaterialInstanceDynamic::Create(IntelligenceNodeMaterial, this);
        DynamicMaterials.Add(DynamicNodeMaterial);
        IntelligenceNodeRenderer->SetMaterial(0, DynamicNodeMaterial);
    }

    // Initialize consciousness stream system
    if (ConsciousnessFlowSystem)
    {
        ConsciousnessStreamSystem->SetAsset(ConsciousnessFlowSystem);
        ConsciousnessStreamSystem->Activate();
    }

    UE_LOG(LogTemp, Log, TEXT("🔧 Consciousness visualization components initialized"));
}

void AConsciousnessNetworkVisualizer::UpdateNeuralPathway(const FNeuralPathwayData& PathwayData)
{
    // Find existing pathway or create new one
    FNeuralPathwayData* ExistingPathway = ActivePathways.FindByPredicate([&PathwayData](const FNeuralPathwayData& Pathway)
    {
        return Pathway.PathwayId == PathwayData.PathwayId;
    });

    if (ExistingPathway)
    {
        // Update existing pathway
        ExistingPathway->SynapticStrength = PathwayData.SynapticStrength;
        ExistingPathway->IsActive = PathwayData.IsActive;
        ExistingPathway->LastActivationTime = GetWorld()->GetTimeSeconds();
    }
    else
    {
        // Add new pathway
        ActivePathways.Add(PathwayData);
        CreateNeuralPathwaySpline(PathwayData);
    }

    // Broadcast update event
    OnNeuralPathwayUpdate.Broadcast(PathwayData);

    UE_LOG(LogTemp, VeryVerbose, TEXT("🧠 Neural pathway updated: %s (Strength: %.2f)"), 
           *PathwayData.PathwayId, PathwayData.SynapticStrength);
}

void AConsciousnessNetworkVisualizer::VisualizeSynapticPlasticity(const FString& PathwayId, float StrengthChange, bool IsLTP)
{
    // Find the pathway
    FNeuralPathwayData* Pathway = ActivePathways.FindByPredicate([&PathwayId](const FNeuralPathwayData& P)
    {
        return P.PathwayId == PathwayId;
    });

    if (!Pathway)
    {
        return;
    }

    // Update synaptic strength with biological constraints
    float NewStrength = FMath::Clamp(Pathway->SynapticStrength + StrengthChange, 0.0f, 1.0f);
    Pathway->SynapticStrength = NewStrength;

    // Create plasticity effect
    if (SynapticPlasticitySystem)
    {
        FVector EffectLocation = FMath::Lerp(Pathway->SourceLocation, Pathway->TargetLocation, 0.5f);
        
        UNiagaraComponent* PlasticityEffect = UNiagaraFunctionLibrary::SpawnSystemAtLocation(
            GetWorld(),
            SynapticPlasticitySystem,
            EffectLocation,
            FRotator::ZeroRotator
        );

        if (PlasticityEffect)
        {
            // Set effect parameters based on LTP/LTD
            PlasticityEffect->SetNiagaraVariableFloat(TEXT("StrengthChange"), StrengthChange);
            PlasticityEffect->SetNiagaraVariableBool(TEXT("IsLTP"), IsLTP);
            PlasticityEffect->SetNiagaraVariableLinearColor(TEXT("PlasticityColor"), 
                IsLTP ? FLinearColor::Green : FLinearColor::Red);
        }
    }

    UE_LOG(LogTemp, Log, TEXT("🔄 Synaptic plasticity: %s %s (Change: %.3f -> Strength: %.3f)"), 
           *PathwayId, IsLTP ? TEXT("LTP") : TEXT("LTD"), StrengthChange, NewStrength);
}

void AConsciousnessNetworkVisualizer::UpdateConsciousnessStream(const FConsciousnessStreamData& StreamData)
{
    // Find existing stream or create new one
    FConsciousnessStreamData* ExistingStream = ActiveStreams.FindByPredicate([&StreamData](const FConsciousnessStreamData& Stream)
    {
        return Stream.StreamId == StreamData.StreamId;
    });

    if (ExistingStream)
    {
        *ExistingStream = StreamData;
    }
    else
    {
        ActiveStreams.Add(StreamData);
    }

    // Update particle system parameters
    UpdateConsciousnessFlowParameters(StreamData);

    // Broadcast update event
    OnConsciousnessStreamUpdate.Broadcast(StreamData);

    UE_LOG(LogTemp, VeryVerbose, TEXT("🌊 Consciousness stream updated: %s (Coherence: %.2f, Latency: %.2fms)"), 
           *StreamData.StreamId, StreamData.CoherenceScore, StreamData.AverageLatency);
}

void AConsciousnessNetworkVisualizer::VisualizeCoherenceLevel(const FString& StreamId, float CoherenceScore)
{
    // Calculate coherence color
    FLinearColor CoherenceColor = FMath::Lerp(LowCoherenceColor, HighCoherenceColor, CoherenceScore);
    
    // Update stream visualization with coherence color
    if (ConsciousnessStreamSystem)
    {
        ConsciousnessStreamSystem->SetNiagaraVariableFloat(TEXT("CoherenceScore"), CoherenceScore);
        ConsciousnessStreamSystem->SetNiagaraVariableLinearColor(TEXT("CoherenceColor"), CoherenceColor);
    }

    UE_LOG(LogTemp, VeryVerbose, TEXT("🎯 Coherence visualization: %s (Score: %.3f)"), *StreamId, CoherenceScore);
}

void AConsciousnessNetworkVisualizer::VisualizeEmergentIntelligence(float IntelligenceLevel)
{
    CurrentMetrics.EmergentIntelligenceLevel = IntelligenceLevel;

    // Create emergence effect based on intelligence level
    if (IntelligenceLevel > 0.8f) // High emergence threshold
    {
        OnIntelligenceEmergence.Broadcast(IntelligenceLevel);
        
        // Create dramatic emergence effect
        if (SynapticPlasticitySystem)
        {
            UNiagaraComponent* EmergenceEffect = UNiagaraFunctionLibrary::SpawnSystemAtLocation(
                GetWorld(),
                SynapticPlasticitySystem,
                GetActorLocation(),
                FRotator::ZeroRotator
            );

            if (EmergenceEffect)
            {
                EmergenceEffect->SetNiagaraVariableFloat(TEXT("EmergenceLevel"), IntelligenceLevel);
                EmergenceEffect->SetNiagaraVariableLinearColor(TEXT("EmergenceColor"), FLinearColor::White);
            }
        }
    }

    UE_LOG(LogTemp, Warning, TEXT("🚀 Emergent Intelligence Level: %.3f"), IntelligenceLevel);
}

void AConsciousnessNetworkVisualizer::ShowNetworkEffect(int32 NodeCount, float GlobalCoherence)
{
    CurrentMetrics.IntelligenceNodes = NodeCount;
    CurrentMetrics.GlobalCoherence = GlobalCoherence;

    // Update network-wide effects based on node count and coherence
    if (ConsciousnessStreamSystem)
    {
        ConsciousnessStreamSystem->SetNiagaraVariableInt(TEXT("NodeCount"), NodeCount);
        ConsciousnessStreamSystem->SetNiagaraVariableFloat(TEXT("GlobalCoherence"), GlobalCoherence);
    }

    // Update all dynamic materials with network parameters
    for (UMaterialInstanceDynamic* DynamicMaterial : DynamicMaterials)
    {
        if (DynamicMaterial)
        {
            DynamicMaterial->SetScalarParameterValue(TEXT("NetworkScale"), FMath::Log10(NodeCount + 1));
            DynamicMaterial->SetScalarParameterValue(TEXT("GlobalCoherence"), GlobalCoherence);
        }
    }

    UE_LOG(LogTemp, Log, TEXT("🌐 Network effect: %d nodes, %.3f global coherence"), NodeCount, GlobalCoherence);
}

void AConsciousnessNetworkVisualizer::ConnectToConsciousnessNetwork(const FString& NetworkEndpoint)
{
    if (bIsConnectedToNetwork)
    {
        DisconnectFromConsciousnessNetwork();
    }

    bIsConnectedToNetwork = true;
    
    // TODO: Implement actual network connection to CX Language consciousness system
    // This would connect to the ConsciousnessPeerCoordinator WebSocket endpoint
    
    UE_LOG(LogTemp, Warning, TEXT("🔗 Connected to consciousness network: %s"), *NetworkEndpoint);
}

void AConsciousnessNetworkVisualizer::DisconnectFromConsciousnessNetwork()
{
    if (!bIsConnectedToNetwork)
    {
        return;
    }

    bIsConnectedToNetwork = false;
    
    // Clear all active data
    ActivePathways.Empty();
    ActiveStreams.Empty();
    CurrentMetrics = FConsciousnessMetrics();
    
    UE_LOG(LogTemp, Warning, TEXT("🔌 Disconnected from consciousness network"));
}

FConsciousnessMetrics AConsciousnessNetworkVisualizer::GetCurrentNetworkMetrics() const
{
    return CurrentMetrics;
}

void AConsciousnessNetworkVisualizer::UpdateNeuralPathwayVisuals(float DeltaTime)
{
    RenderedPathwayCount = 0;

    for (const FNeuralPathwayData& Pathway : ActivePathways)
    {
        if (RenderedPathwayCount >= MaxRenderPathways)
        {
            break; // Performance limit
        }

        // Calculate synaptic strength color
        FLinearColor PathwayColor = CalculateSynapticStrengthColor(Pathway.SynapticStrength);
        
        // Update pathway visualization based on activity
        if (Pathway.IsActive && (GetWorld()->GetTimeSeconds() - Pathway.LastActivationTime) < 0.1f)
        {
            // Add activation pulse effect
            PathwayColor *= 1.5f; // Brighten active pathways
        }

        RenderedPathwayCount++;
    }
}

void AConsciousnessNetworkVisualizer::UpdateConsciousnessStreamEffects(float DeltaTime)
{
    ActiveParticleSystemCount = 0;

    for (const FConsciousnessStreamData& Stream : ActiveStreams)
    {
        // Update stream intensity based on event processing
        float IntensityMultiplier = FMath::Clamp(Stream.EventsProcessed / 1000.0f, 0.1f, 2.0f);
        
        if (ConsciousnessStreamSystem)
        {
            ConsciousnessStreamSystem->SetNiagaraVariableFloat(
                FString::Printf(TEXT("Stream_%s_Intensity"), *Stream.StreamId),
                Stream.StreamIntensity * IntensityMultiplier
            );
        }

        ActiveParticleSystemCount++;
    }

    // Update global metrics
    CurrentMetrics.ActiveStreams = ActiveStreams.Num();
    CurrentMetrics.TotalProcessedEvents = 0;
    for (const FConsciousnessStreamData& Stream : ActiveStreams)
    {
        CurrentMetrics.TotalProcessedEvents += Stream.EventsProcessed;
    }
}

void AConsciousnessNetworkVisualizer::UpdateEmergentIntelligenceDisplay(float DeltaTime)
{
    // Calculate average latency across all streams
    if (ActiveStreams.Num() > 0)
    {
        float TotalLatency = 0.0f;
        for (const FConsciousnessStreamData& Stream : ActiveStreams)
        {
            TotalLatency += Stream.AverageLatency;
        }
        CurrentMetrics.AverageNetworkLatency = TotalLatency / ActiveStreams.Num();
    }

    // Update intelligence node materials based on emergence level
    if (IntelligenceNodeRenderer && DynamicMaterials.Num() > 1)
    {
        UMaterialInstanceDynamic* NodeMaterial = DynamicMaterials[1];
        if (NodeMaterial)
        {
            NodeMaterial->SetScalarParameterValue(TEXT("EmergenceLevel"), CurrentMetrics.EmergentIntelligenceLevel);
            NodeMaterial->SetVectorParameterValue(TEXT("EmergenceColor"), 
                FLinearColor::LerpUsingHSV(FLinearColor::Blue, FLinearColor::White, CurrentMetrics.EmergentIntelligenceLevel));
        }
    }
}

FLinearColor AConsciousnessNetworkVisualizer::CalculateSynapticStrengthColor(float Strength) const
{
    if (Strength <= 0.33f)
    {
        return FMath::Lerp(FLinearColor::Black, WeakSynapseColor, Strength * 3.0f);
    }
    else if (Strength <= 0.66f)
    {
        return FMath::Lerp(WeakSynapseColor, MediumSynapseColor, (Strength - 0.33f) * 3.0f);
    }
    else
    {
        return FMath::Lerp(MediumSynapseColor, StrongSynapseColor, (Strength - 0.66f) * 3.0f);
    }
}

void AConsciousnessNetworkVisualizer::CreateNeuralPathwaySpline(const FNeuralPathwayData& PathwayData)
{
    // Create spline component for neural pathway
    USplineComponent* PathwaySpline = NewObject<USplineComponent>(this);
    PathwaySpline->SetupAttachment(NetworkRoot);
    
    // Set spline points
    PathwaySpline->ClearSplinePoints();
    PathwaySpline->AddSplinePoint(PathwayData.SourceLocation, ESplineCoordinateSpace::World);
    PathwaySpline->AddSplinePoint(PathwayData.TargetLocation, ESplineCoordinateSpace::World);
    PathwaySpline->UpdateSpline();
    
    NeuralPathwaySplines.Add(PathwaySpline);

    UE_LOG(LogTemp, VeryVerbose, TEXT("🔗 Created neural pathway spline: %s"), *PathwayData.PathwayId);
}

void AConsciousnessNetworkVisualizer::UpdateConsciousnessFlowParameters(const FConsciousnessStreamData& StreamData)
{
    if (!ConsciousnessStreamSystem)
    {
        return;
    }

    // Update flow direction and intensity
    ConsciousnessStreamSystem->SetNiagaraVariableVec3(
        FString::Printf(TEXT("Stream_%s_Direction"), *StreamData.StreamId),
        StreamData.StreamDirection
    );

    // Update coherence-based color
    FLinearColor StreamColor = FMath::Lerp(LowCoherenceColor, HighCoherenceColor, StreamData.CoherenceScore);
    ConsciousnessStreamSystem->SetNiagaraVariableLinearColor(
        FString::Printf(TEXT("Stream_%s_Color"), *StreamData.StreamId),
        StreamColor
    );

    // Update biological authenticity effects
    ConsciousnessStreamSystem->SetNiagaraVariableBool(
        FString::Printf(TEXT("Stream_%s_BiologicalAuth"), *StreamData.StreamId),
        StreamData.BiologicalAuthenticity
    );
}

void AConsciousnessNetworkVisualizer::ProcessNeuralSpeedUpdates()
{
    float CurrentTime = GetWorld()->GetTimeSeconds();
    float TimeSinceLastUpdate = CurrentTime - LastUpdateTime;
    LastUpdateTime = CurrentTime;

    // Broadcast network metrics update
    OnNetworkMetricsUpdate.Broadcast(CurrentMetrics);

    // Performance logging at neural speed
    if (FMath::Fmod(CurrentTime, 5.0f) < 0.1f) // Every 5 seconds
    {
        UE_LOG(LogTemp, Warning, TEXT("🧠 Neural-speed performance: %.1f FPS, %d pathways, %d streams"), 
               1.0f / AverageFrameTime, RenderedPathwayCount, ActiveParticleSystemCount);
    }
}

void AConsciousnessNetworkVisualizer::ProcessBiologicalTimingCycle()
{
    // Simulate biological neural timing cycles (1-25ms)
    for (FNeuralPathwayData& Pathway : ActivePathways)
    {
        if (Pathway.IsActive)
        {
            // Decay activation over time (biological refractory period)
            float TimeSinceActivation = GetWorld()->GetTimeSeconds() - Pathway.LastActivationTime;
            if (TimeSinceActivation > 0.025f) // 25ms maximum activation
            {
                Pathway.IsActive = false;
            }
        }
    }
}

void AConsciousnessNetworkVisualizer::OptimizeRenderingPerformance()
{
    // Dynamic LOD based on performance
    if (AverageFrameTime > 1.0f / 60.0f) // Below 60 FPS
    {
        // Reduce pathway render count
        MaxRenderPathways = FMath::Max(100, MaxRenderPathways - 50);
        
        // Reduce particle system detail
        if (ConsciousnessStreamSystem)
        {
            ConsciousnessStreamSystem->SetNiagaraVariableFloat(TEXT("LODMultiplier"), 0.5f);
        }
    }
    else if (AverageFrameTime < 1.0f / 120.0f) // Above 120 FPS
    {
        // Increase quality
        MaxRenderPathways = FMath::Min(1000, MaxRenderPathways + 50);
        
        if (ConsciousnessStreamSystem)
        {
            ConsciousnessStreamSystem->SetNiagaraVariableFloat(TEXT("LODMultiplier"), 1.0f);
        }
    }
}

void AConsciousnessNetworkVisualizer::UpdateLODLevels()
{
    // Update LOD based on distance and performance requirements
    // This would be expanded based on camera distance and visual importance
}
