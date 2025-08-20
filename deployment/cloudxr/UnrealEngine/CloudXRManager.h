// CloudXRManager.h
// Unreal Engine CloudXR manager for consciousness streaming integration

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Engine/Engine.h"
#include "Dom/JsonObject.h"
#include "CloudXRManager.generated.h"

// Forward declarations
class AConsciousnessEntity;
class AConsciousnessFlow;
class AConsciousnessAura;

// CloudXR connection status
UENUM(BlueprintType)
enum class ECloudXRConnectionStatus : uint8
{
    Disconnected UMETA(DisplayName = "Disconnected"),
    Connecting UMETA(DisplayName = "Connecting"),
    Connected UMETA(DisplayName = "Connected"),
    Error UMETA(DisplayName = "Error")
};

// CloudXR configuration
USTRUCT(BlueprintType)
struct FCloudXRConfiguration
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR")
    FString WebSocketURL = TEXT("ws://127.0.0.1:8080/cloudxr");

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR")
    bool bAutoConnect = true;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR")
    float ReconnectInterval = 5.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR")
    int32 MaxReconnectAttempts = 10;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR")
    bool bEnableDebugLogging = true;
};

// Main CloudXR manager class
UCLASS(BlueprintType, Blueprintable)
class CONSCIOUSNESSXR_API ACloudXRManager : public AActor
{
    GENERATED_BODY()

public:
    ACloudXRManager();

protected:
    virtual void BeginPlay() override;
    virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

public:
    virtual void Tick(float DeltaTime) override;

    // CloudXR connection management
    UFUNCTION(BlueprintCallable, Category = "CloudXR")
    void ConnectToCloudXR();

    UFUNCTION(BlueprintCallable, Category = "CloudXR")
    void DisconnectFromCloudXR();

    UFUNCTION(BlueprintPure, Category = "CloudXR")
    ECloudXRConnectionStatus GetConnectionStatus() const { return ConnectionStatus; }

    UFUNCTION(BlueprintPure, Category = "CloudXR")
    bool IsConnected() const { return ConnectionStatus == ECloudXRConnectionStatus::Connected; }

    // Consciousness data handling
    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void ProcessConsciousnessData(const FString& JsonData);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    AConsciousnessEntity* CreateConsciousnessEntity(const FString& EntityId, const FVector& Location);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void UpdateConsciousnessEntity(const FString& EntityId, const FString& JsonData);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void RemoveConsciousnessEntity(const FString& EntityId);

    UFUNCTION(BlueprintCallable, Category = "Consciousness")
    void ClearAllConsciousnessEntities();

    // Entity management
    UFUNCTION(BlueprintPure, Category = "Consciousness")
    AConsciousnessEntity* GetConsciousnessEntity(const FString& EntityId);

    UFUNCTION(BlueprintPure, Category = "Consciousness")
    TArray<AConsciousnessEntity*> GetAllConsciousnessEntities();

    UFUNCTION(BlueprintPure, Category = "Consciousness")
    int32 GetConsciousnessEntityCount() const { return ConsciousnessEntities.Num(); }

    // Blueprint events
    UFUNCTION(BlueprintImplementableEvent, Category = "CloudXR")
    void OnCloudXRConnected();

    UFUNCTION(BlueprintImplementableEvent, Category = "CloudXR")
    void OnCloudXRDisconnected();

    UFUNCTION(BlueprintImplementableEvent, Category = "CloudXR")
    void OnCloudXRConnectionError(const FString& ErrorMessage);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnConsciousnessDataReceived(const FString& JsonData);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnConsciousnessEntityCreated(AConsciousnessEntity* Entity);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnConsciousnessEntityUpdated(AConsciousnessEntity* Entity);

    UFUNCTION(BlueprintImplementableEvent, Category = "Consciousness")
    void OnConsciousnessEntityRemoved(const FString& EntityId);

protected:
    // Configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "CloudXR", meta = (AllowPrivateAccess = "true"))
    FCloudXRConfiguration CloudXRConfig;

    // Connection status
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "CloudXR", meta = (AllowPrivateAccess = "true"))
    ECloudXRConnectionStatus ConnectionStatus = ECloudXRConnectionStatus::Disconnected;

    // Entity management
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    TMap<FString, AConsciousnessEntity*> ConsciousnessEntities;

    // Entity spawning
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    TSubclassOf<AConsciousnessEntity> ConsciousnessEntityClass;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    TSubclassOf<AConsciousnessFlow> ConsciousnessFlowClass;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Consciousness", meta = (AllowPrivateAccess = "true"))
    TSubclassOf<AConsciousnessAura> ConsciousnessAuraClass;

private:
    // Internal connection management
    void AttemptConnection();
    void HandleConnectionSuccess();
    void HandleConnectionFailure(const FString& ErrorMessage);
    void StartHeartbeat();
    void StopHeartbeat();
    void SendHeartbeat();

    // Data processing
    void ProcessConsciousnessPacket(TSharedPtr<FJsonObject> JsonObject);
    void ProcessEntityData(TSharedPtr<FJsonObject> EntityData);
    void ProcessFlowData(TSharedPtr<FJsonObject> FlowData);
    void ProcessAuraData(TSharedPtr<FJsonObject> AuraData);

    // Utility functions
    FVector JsonToVector3(TSharedPtr<FJsonObject> VectorObject);
    FLinearColor JsonToColor(TSharedPtr<FJsonObject> ColorObject);

    // Connection state
    int32 ReconnectAttempts = 0;
    FTimerHandle ReconnectTimerHandle;
    FTimerHandle HeartbeatTimerHandle;
    bool bIsConnecting = false;

    // Performance tracking
    float LastDataReceiveTime = 0.0f;
    int32 PacketsReceived = 0;
    int32 EntitiesProcessed = 0;
};
