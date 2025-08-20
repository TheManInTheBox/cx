// ConsciousnessEntity.h
// Unreal Engine C++ header for consciousness entity integration

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/StaticMeshComponent.h"
#include "NiagaraComponent.h"
#include "Components/AudioComponent.h"
#include "Engine/Engine.h"
#include "ConsciousnessEntity.generated.h"

// Consciousness data structure matching CX Language CloudXR format
USTRUCT(BlueprintType)
struct FConsciousnessData
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Consciousness")
    FVector Location = FVector::ZeroVector;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Consciousness", meta = (ClampMin = "0.0", ClampMax = "1.0"))
    float Awareness = 0.5f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Consciousness", meta = (ClampMin = "0.0", ClampMax = "1.0"))
    float Emotion = 0.5f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Consciousness", meta = (ClampMin = "0.0", ClampMax = "1.0"))
    float Energy = 0.5f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Consciousness", meta = (ClampMin = "0.0", ClampMax = "1.0"))
    float ConsciousnessLevel = 0.5f;

    FConsciousnessData()
    {
        Location = FVector::ZeroVector;
        Awareness = 0.5f;
        Emotion = 0.5f;
        Energy = 0.5f;
        ConsciousnessLevel = 0.5f;
    }
};

// Consciousness visualization parameters
USTRUCT(BlueprintType)
struct FConsciousnessVisualization
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Visualization")
    float AuraRadius = 5.0f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Visualization")
    float AuraIntensity = 1.0f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Visualization")
    int32 ParticleCount = 50;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Visualization")
    float AnimationSpeed = 1.0f;

    UPROPERTY(BlueprintReadWrite, EditAnywhere, Category = "Visualization")
    FLinearColor AuraColor = FLinearColor::Blue;
};

// Main consciousness entity actor
UCLASS(BlueprintType, Blueprintable)
class CONSCIOUSNESSXR_API AConsciousnessEntity : public AActor
{
    GENERATED_BODY()

public:
    AConsciousnessEntity();

protected:
    virtual void BeginPlay() override;

public:
    virtual void Tick(float DeltaTime) override;

    // CloudXR integration functions
    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void UpdateConsciousnessData(const FConsciousnessData& NewData);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void UpdateFromCloudXRData(const FString& JsonData);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnConsciousnessChanged();

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnAwarenessChanged(float NewAwareness);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnEmotionChanged(float NewEmotion);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnEnergyChanged(float NewEnergy);

    // Getters
    UFUNCTION(BlueprintPure, Category = "Consciousness")
    FConsciousnessData GetConsciousnessData() const { return ConsciousnessData; }

    UFUNCTION(BlueprintPure, Category = "Consciousness")
    FConsciousnessVisualization GetVisualizationData() const { return VisualizationData; }

    // Setters
    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void SetAwareness(float NewAwareness);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void SetEmotion(float NewEmotion);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void SetEnergy(float NewEnergy);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void SetConsciousnessLevel(float NewLevel);

protected:
    // Core components
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
    UStaticMeshComponent* ConsciousnessMesh;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
    UNiagaraComponent* AuraParticles;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
    UAudioComponent* ConsciousnessAudio;

    // Consciousness data
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    FConsciousnessData ConsciousnessData;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    FConsciousnessVisualization VisualizationData;

    // Entity identification
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    FString EntityId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    bool bIsActive = true;

private:
    // Internal update functions
    void UpdateVisualization();
    void UpdateAuraParticles();
    void UpdateAudioFeedback();
    void UpdateMaterialParameters();

    // Previous values for change detection
    float PreviousAwareness = 0.5f;
    float PreviousEmotion = 0.5f;
    float PreviousEnergy = 0.5f;
    float PreviousConsciousnessLevel = 0.5f;
};
