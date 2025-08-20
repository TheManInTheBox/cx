// ConsciousnessEntity.cpp
// Unreal Engine C++ implementation for consciousness entity

#include "ConsciousnessEntity.h"
#include "Components/StaticMeshComponent.h"
#include "NiagaraComponent.h"
#include "Components/AudioComponent.h"
#include "Engine/Engine.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonReader.h"

AConsciousnessEntity::AConsciousnessEntity()
{
    PrimaryActorTick.bCanEverTick = true;

    // Create consciousness mesh component
    ConsciousnessMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("ConsciousnessMesh"));
    RootComponent = ConsciousnessMesh;

    // Create aura particle system
    AuraParticles = CreateDefaultSubobject<UNiagaraComponent>(TEXT("AuraParticles"));
    AuraParticles->SetupAttachment(ConsciousnessMesh);

    // Create consciousness audio component
    ConsciousnessAudio = CreateDefaultSubobject<UAudioComponent>(TEXT("ConsciousnessAudio"));
    ConsciousnessAudio->SetupAttachment(ConsciousnessMesh);

    // Initialize default consciousness data
    ConsciousnessData = FConsciousnessData();
    VisualizationData = FConsciousnessVisualization();

    // Generate unique entity ID
    EntityId = FGuid::NewGuid().ToString();
}

void AConsciousnessEntity::BeginPlay()
{
    Super::BeginPlay();

    // Initialize visualization based on starting consciousness data
    UpdateVisualization();

    UE_LOG(LogTemp, Log, TEXT("Consciousness Entity %s initialized"), *EntityId);
}

void AConsciousnessEntity::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // Check for consciousness changes and update visualization
    bool bHasChanged = false;

    if (PreviousAwareness != ConsciousnessData.Awareness)
    {
        OnAwarenessChanged(ConsciousnessData.Awareness);
        PreviousAwareness = ConsciousnessData.Awareness;
        bHasChanged = true;
    }

    if (PreviousEmotion != ConsciousnessData.Emotion)
    {
        OnEmotionChanged(ConsciousnessData.Emotion);
        PreviousEmotion = ConsciousnessData.Emotion;
        bHasChanged = true;
    }

    if (PreviousEnergy != ConsciousnessData.Energy)
    {
        OnEnergyChanged(ConsciousnessData.Energy);
        PreviousEnergy = ConsciousnessData.Energy;
        bHasChanged = true;
    }

    if (PreviousConsciousnessLevel != ConsciousnessData.ConsciousnessLevel)
    {
        PreviousConsciousnessLevel = ConsciousnessData.ConsciousnessLevel;
        bHasChanged = true;
    }

    if (bHasChanged)
    {
        UpdateVisualization();
        OnConsciousnessChanged();
    }
}

void AConsciousnessEntity::UpdateConsciousnessData(const FConsciousnessData& NewData)
{
    ConsciousnessData = NewData;

    // Update actor location to match consciousness data
    SetActorLocation(ConsciousnessData.Location);

    // Update visualization parameters based on consciousness state
    VisualizationData.AuraRadius = ConsciousnessData.Awareness * 10.0f;
    VisualizationData.AuraIntensity = ConsciousnessData.Energy;
    VisualizationData.ParticleCount = FMath::RoundToInt(ConsciousnessData.Energy * 100.0f);
    VisualizationData.AnimationSpeed = ConsciousnessData.Emotion * 2.0f;

    // Update aura color based on emotion
    float Hue = ConsciousnessData.Emotion * 360.0f; // Map emotion to hue
    VisualizationData.AuraColor = FLinearColor::MakeFromHSV8(Hue, 255, 255);

    UpdateVisualization();

    UE_LOG(LogTemp, Log, TEXT("Consciousness Entity %s updated: Awareness=%.2f, Emotion=%.2f, Energy=%.2f, Level=%.2f"), 
        *EntityId, 
        ConsciousnessData.Awareness, 
        ConsciousnessData.Emotion, 
        ConsciousnessData.Energy, 
        ConsciousnessData.ConsciousnessLevel);
}

void AConsciousnessEntity::UpdateFromCloudXRData(const FString& JsonData)
{
    // Parse JSON data from CloudXR stream
    TSharedPtr<FJsonObject> JsonObject;
    TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(JsonData);

    if (FJsonSerializer::Deserialize(Reader, JsonObject) && JsonObject.IsValid())
    {
        FConsciousnessData NewData;

        // Parse consciousness properties
        if (JsonObject->HasField(TEXT("consciousnessProperties")))
        {
            TSharedPtr<FJsonObject> PropsObject = JsonObject->GetObjectField(TEXT("consciousnessProperties"));
            
            NewData.Awareness = PropsObject->GetNumberField(TEXT("awareness"));
            NewData.Emotion = PropsObject->GetNumberField(TEXT("emotion"));
            NewData.Energy = PropsObject->GetNumberField(TEXT("energy"));
            NewData.ConsciousnessLevel = PropsObject->GetNumberField(TEXT("level"));
        }

        // Parse transform/location
        if (JsonObject->HasField(TEXT("transform")))
        {
            TSharedPtr<FJsonObject> TransformObject = JsonObject->GetObjectField(TEXT("transform"));
            
            if (TransformObject->HasField(TEXT("location")))
            {
                TSharedPtr<FJsonObject> LocationObject = TransformObject->GetObjectField(TEXT("location"));
                
                NewData.Location.X = LocationObject->GetNumberField(TEXT("x"));
                NewData.Location.Y = LocationObject->GetNumberField(TEXT("y"));
                NewData.Location.Z = LocationObject->GetNumberField(TEXT("z"));
            }
        }

        // Update entity ID if provided
        if (JsonObject->HasField(TEXT("id")))
        {
            EntityId = JsonObject->GetStringField(TEXT("id"));
        }

        // Apply the updated consciousness data
        UpdateConsciousnessData(NewData);
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("Failed to parse CloudXR JSON data for entity %s"), *EntityId);
    }
}

void AConsciousnessEntity::SetAwareness(float NewAwareness)
{
    ConsciousnessData.Awareness = FMath::Clamp(NewAwareness, 0.0f, 1.0f);
    UpdateVisualization();
}

void AConsciousnessEntity::SetEmotion(float NewEmotion)
{
    ConsciousnessData.Emotion = FMath::Clamp(NewEmotion, 0.0f, 1.0f);
    UpdateVisualization();
}

void AConsciousnessEntity::SetEnergy(float NewEnergy)
{
    ConsciousnessData.Energy = FMath::Clamp(NewEnergy, 0.0f, 1.0f);
    UpdateVisualization();
}

void AConsciousnessEntity::SetConsciousnessLevel(float NewLevel)
{
    ConsciousnessData.ConsciousnessLevel = FMath::Clamp(NewLevel, 0.0f, 1.0f);
    UpdateVisualization();
}

void AConsciousnessEntity::UpdateVisualization()
{
    UpdateAuraParticles();
    UpdateAudioFeedback();
    UpdateMaterialParameters();
}

void AConsciousnessEntity::UpdateAuraParticles()
{
    if (AuraParticles && AuraParticles->GetAsset())
    {
        // Update particle system parameters based on consciousness state
        AuraParticles->SetFloatParameter(TEXT("AuraRadius"), VisualizationData.AuraRadius);
        AuraParticles->SetFloatParameter(TEXT("AuraIntensity"), VisualizationData.AuraIntensity);
        AuraParticles->SetFloatParameter(TEXT("ParticleCount"), VisualizationData.ParticleCount);
        AuraParticles->SetFloatParameter(TEXT("AnimationSpeed"), VisualizationData.AnimationSpeed);
        AuraParticles->SetColorParameter(TEXT("AuraColor"), VisualizationData.AuraColor);
        
        // Consciousness-specific parameters
        AuraParticles->SetFloatParameter(TEXT("Awareness"), ConsciousnessData.Awareness);
        AuraParticles->SetFloatParameter(TEXT("Emotion"), ConsciousnessData.Emotion);
        AuraParticles->SetFloatParameter(TEXT("Energy"), ConsciousnessData.Energy);
        AuraParticles->SetFloatParameter(TEXT("ConsciousnessLevel"), ConsciousnessData.ConsciousnessLevel);

        // Enable/disable particle system based on energy level
        if (ConsciousnessData.Energy > 0.1f)
        {
            AuraParticles->SetVisibility(true);
            AuraParticles->Activate();
        }
        else
        {
            AuraParticles->SetVisibility(false);
            AuraParticles->Deactivate();
        }
    }
}

void AConsciousnessEntity::UpdateAudioFeedback()
{
    if (ConsciousnessAudio)
    {
        // Adjust audio volume based on consciousness level
        float Volume = ConsciousnessData.ConsciousnessLevel * ConsciousnessData.Energy;
        ConsciousnessAudio->SetVolumeMultiplier(Volume);

        // Adjust pitch based on awareness
        float Pitch = 0.5f + (ConsciousnessData.Awareness * 0.5f);
        ConsciousnessAudio->SetPitchMultiplier(Pitch);

        // Enable/disable audio based on energy
        if (ConsciousnessData.Energy > 0.2f && bIsActive)
        {
            if (!ConsciousnessAudio->IsPlaying())
            {
                ConsciousnessAudio->Play();
            }
        }
        else
        {
            if (ConsciousnessAudio->IsPlaying())
            {
                ConsciousnessAudio->Stop();
            }
        }
    }
}

void AConsciousnessEntity::UpdateMaterialParameters()
{
    if (ConsciousnessMesh && ConsciousnessMesh->GetMaterial(0))
    {
        // Create dynamic material instance if needed
        UMaterialInstanceDynamic* DynamicMaterial = ConsciousnessMesh->CreateAndSetMaterialInstanceDynamic(0);

        if (DynamicMaterial)
        {
            // Update material parameters based on consciousness state
            DynamicMaterial->SetScalarParameterValue(TEXT("Awareness"), ConsciousnessData.Awareness);
            DynamicMaterial->SetScalarParameterValue(TEXT("Emotion"), ConsciousnessData.Emotion);
            DynamicMaterial->SetScalarParameterValue(TEXT("Energy"), ConsciousnessData.Energy);
            DynamicMaterial->SetScalarParameterValue(TEXT("ConsciousnessLevel"), ConsciousnessData.ConsciousnessLevel);
            
            // Set aura color
            DynamicMaterial->SetVectorParameterValue(TEXT("AuraColor"), VisualizationData.AuraColor);
            
            // Set aura radius and intensity
            DynamicMaterial->SetScalarParameterValue(TEXT("AuraRadius"), VisualizationData.AuraRadius);
            DynamicMaterial->SetScalarParameterValue(TEXT("AuraIntensity"), VisualizationData.AuraIntensity);

            // Opacity based on awareness (more aware = more visible)
            DynamicMaterial->SetScalarParameterValue(TEXT("Opacity"), 0.3f + (ConsciousnessData.Awareness * 0.7f));

            // Emissive intensity based on energy
            DynamicMaterial->SetScalarParameterValue(TEXT("EmissiveIntensity"), ConsciousnessData.Energy * 2.0f);
        }
    }

    // Update actor scale based on consciousness level
    float Scale = 0.5f + (ConsciousnessData.ConsciousnessLevel * 1.5f);
    SetActorScale3D(FVector(Scale, Scale, Scale));
}
