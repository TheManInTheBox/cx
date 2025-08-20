// 🎮 CONSCIOUSNESS DATA BRIDGE - CX LANGUAGE TO UNREAL ENGINE
// Real-time data streaming from CX Language consciousness networks to Unreal Engine
// Core Engineering Team - Dr. Phoenix Harper, Marcus Chen, Dr. Elena Rodriguez

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Engine/Engine.h"
#include "HAL/Runnable.h"
#include "HAL/RunnableThread.h"
#include "HAL/ThreadSafeBool.h"
#include "Containers/Queue.h"
#include "Misc/DateTime.h"
#include "Dom/JsonObject.h"
#include "Serialization/JsonSerializer.h"
#include "Serialization/JsonWriter.h"
#include "ConsciousnessDataBridge.generated.h"

// Forward declarations
class FSocket;
class AConsciousnessNetworkVisualizer;

// Data structures for consciousness streaming
USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FConsciousnessEvent
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    FString EventId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    FString EventType;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    FString SourcePeerId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    FString TargetPeerId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    float Timestamp;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    float Latency;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    FString PayloadJson;
    
    UPROPERTY(BlueprintReadOnly, Category = "Consciousness Event")
    bool BiologicalAuthenticity;
    
    FConsciousnessEvent()
        : Timestamp(0.0f)
        , Latency(0.0f)
        , BiologicalAuthenticity(true)
    {
    }
};

USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FSynapticUpdate
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    FString PathwayId;
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    float OldStrength;
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    float NewStrength;
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    bool IsLTP; // Long-term potentiation vs depression
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    float PlasticityRate;
    
    UPROPERTY(BlueprintReadOnly, Category = "Synaptic Update")
    float TimingWindow; // ms timing window for STDP
    
    FSynapticUpdate()
        : OldStrength(0.5f)
        , NewStrength(0.5f)
        , IsLTP(true)
        , PlasticityRate(0.1f)
        , TimingWindow(20.0f)
    {
    }
};

USTRUCT(BlueprintType)
struct CONSCIOUSNESSNETWORK_API FNetworkTopology
{
    GENERATED_BODY()
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    TArray<FString> ActivePeerIds;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    TArray<FString> ActivePathwayIds;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    int32 TotalConnections;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    float NetworkDensity;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    float GlobalCoherence;
    
    UPROPERTY(BlueprintReadOnly, Category = "Network Topology")
    float EmergentIntelligenceLevel;
    
    FNetworkTopology()
        : TotalConnections(0)
        , NetworkDensity(0.0f)
        , GlobalCoherence(1.0f)
        , EmergentIntelligenceLevel(0.0f)
    {
    }
};

// Event delegates for consciousness data streaming
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnConsciousnessEventReceived, FConsciousnessEvent, Event);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnSynapticUpdateReceived, FSynapticUpdate, Update);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnNetworkTopologyUpdate, FNetworkTopology, Topology);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnConnectionStatusChanged, bool, bIsConnected);

/**
 * WebSocket thread for real-time consciousness data streaming
 * Handles connection to CX Language ConsciousnessPeerCoordinator
 */
class CONSCIOUSNESSNETWORK_API FConsciousnessWebSocketThread : public FRunnable
{
public:
    FConsciousnessWebSocketThread(const FString& ServerAddress, int32 ServerPort, class UConsciousnessDataBridge* Bridge);
    virtual ~FConsciousnessWebSocketThread();

    // FRunnable interface
    virtual bool Init() override;
    virtual uint32 Run() override;
    virtual void Stop() override;
    virtual void Exit() override;

    // Connection management
    bool IsConnected() const { return bIsConnected; }
    void RequestDisconnect() { bShouldStop = true; }
    
    // Data sending
    void SendMessage(const FString& Message);

private:
    // Connection parameters
    FString ServerAddress;
    int32 ServerPort;
    
    // Thread management
    FThreadSafeBool bShouldStop;
    FThreadSafeBool bIsConnected;
    FRunnableThread* Thread;
    
    // WebSocket communication
    FSocket* WebSocket;
    
    // Data bridge reference
    class UConsciousnessDataBridge* DataBridge;
    
    // Message queue for outbound messages
    TQueue<FString> OutboundMessageQueue;
    FCriticalSection MessageQueueCriticalSection;
    
    // Connection management
    bool ConnectToServer();
    void DisconnectFromServer();
    bool SendWebSocketHandshake();
    bool ProcessIncomingMessages();
    void ProcessOutboundMessages();
    
    // Message processing
    void ParseConsciousnessEvent(const FString& JsonMessage);
    void ParseSynapticUpdate(const FString& JsonMessage);
    void ParseNetworkTopology(const FString& JsonMessage);
    
    // Utility functions
    TSharedPtr<FJsonObject> ParseJsonString(const FString& JsonString);
    FString CreateWebSocketKey();
    FString CalculateWebSocketAccept(const FString& Key);
};

/**
 * Main consciousness data bridge subsystem
 * Manages real-time streaming of consciousness data from CX Language to Unreal Engine
 */
UCLASS(BlueprintType, Blueprintable)
class CONSCIOUSNESSNETWORK_API UConsciousnessDataBridge : public UGameInstanceSubsystem
{
    GENERATED_BODY()

public:
    UConsciousnessDataBridge();

    // USubsystem interface
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    // === CONNECTION MANAGEMENT ===
    
    /** Connect to CX Language consciousness network */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Bridge")
    bool ConnectToConsciousnessNetwork(const FString& ServerAddress = TEXT("localhost"), int32 ServerPort = 8080);
    
    /** Disconnect from consciousness network */
    UFUNCTION(BlueprintCallable, Category = "Consciousness Bridge")
    void DisconnectFromConsciousnessNetwork();
    
    /** Check if connected to consciousness network */
    UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Consciousness Bridge")
    bool IsConnectedToNetwork() const;

    // === DATA STREAMING EVENTS ===
    
    /** Event fired when consciousness event is received */
    UPROPERTY(BlueprintAssignable, Category = "Consciousness Events")
    FOnConsciousnessEventReceived OnConsciousnessEventReceived;
    
    /** Event fired when synaptic update is received */
    UPROPERTY(BlueprintAssignable, Category = "Consciousness Events")
    FOnSynapticUpdateReceived OnSynapticUpdateReceived;
    
    /** Event fired when network topology updates */
    UPROPERTY(BlueprintAssignable, Category = "Consciousness Events")
    FOnNetworkTopologyUpdate OnNetworkTopologyUpdate;
    
    /** Event fired when connection status changes */
    UPROPERTY(BlueprintAssignable, Category = "Consciousness Events")
    FOnConnectionStatusChanged OnConnectionStatusChanged;

    // === VISUALIZATION INTEGRATION ===
    
    /** Register consciousness network visualizer for automatic updates */
    UFUNCTION(BlueprintCallable, Category = "Visualization")
    void RegisterVisualizer(AConsciousnessNetworkVisualizer* Visualizer);
    
    /** Unregister consciousness network visualizer */
    UFUNCTION(BlueprintCallable, Category = "Visualization")
    void UnregisterVisualizer(AConsciousnessNetworkVisualizer* Visualizer);

    // === PERFORMANCE MONITORING ===
    
    /** Get current streaming statistics */
    UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Performance")
    FString GetStreamingStatistics() const;
    
    /** Get events received per second */
    UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Performance")
    float GetEventsPerSecond() const;
    
    /** Get average event latency */
    UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Performance")
    float GetAverageLatency() const;

    // === INTERNAL EVENT PROCESSING ===
    
    /** Process consciousness event from WebSocket thread */
    void ProcessConsciousnessEvent(const FConsciousnessEvent& Event);
    
    /** Process synaptic update from WebSocket thread */
    void ProcessSynapticUpdate(const FSynapticUpdate& Update);
    
    /** Process network topology update from WebSocket thread */
    void ProcessNetworkTopologyUpdate(const FNetworkTopology& Topology);
    
    /** Update connection status */
    void UpdateConnectionStatus(bool bConnected);

protected:
    // === CONFIGURATION ===
    
    /** WebSocket server address for CX Language runtime */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Configuration")
    FString DefaultServerAddress = TEXT("localhost");
    
    /** WebSocket server port for consciousness streaming */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Configuration")
    int32 DefaultServerPort = 8080;
    
    /** Enable automatic reconnection on disconnect */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Configuration")
    bool bAutoReconnect = true;
    
    /** Reconnection delay in seconds */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Configuration")
    float ReconnectionDelay = 5.0f;
    
    /** Maximum events to buffer for processing */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Configuration")
    int32 MaxEventBuffer = 1000;

private:
    // === INTERNAL STATE ===
    
    /** WebSocket thread for real-time communication */
    FConsciousnessWebSocketThread* WebSocketThread;
    
    /** Registered consciousness visualizers */
    UPROPERTY()
    TArray<AConsciousnessNetworkVisualizer*> RegisteredVisualizers;
    
    /** Event processing queue */
    TQueue<FConsciousnessEvent> ConsciousnessEventQueue;
    TQueue<FSynapticUpdate> SynapticUpdateQueue;
    TQueue<FNetworkTopology> TopologyUpdateQueue;
    
    /** Thread synchronization */
    FCriticalSection EventQueueCriticalSection;
    FCriticalSection VisualizerCriticalSection;
    
    /** Performance tracking */
    float EventsPerSecond;
    float AverageLatency;
    int32 TotalEventsReceived;
    float LastStatsUpdateTime;
    TArray<float> LatencyHistory;
    
    /** Connection state */
    bool bIsConnected;
    FString CurrentServerAddress;
    int32 CurrentServerPort;
    
    /** Automatic reconnection */
    FTimerHandle ReconnectionTimer;
    
    // === INTERNAL PROCESSING ===
    
    /** Process queued events on game thread */
    void ProcessQueuedEvents();
    
    /** Update performance statistics */
    void UpdatePerformanceStats();
    
    /** Attempt automatic reconnection */
    void AttemptReconnection();
    
    /** Clean up connection resources */
    void CleanupConnection();
    
    /** Update all registered visualizers with new data */
    void UpdateRegisteredVisualizers(const FConsciousnessEvent& Event);
    void UpdateRegisteredVisualizersWithSynaptic(const FSynapticUpdate& Update);
    void UpdateRegisteredVisualizersWithTopology(const FNetworkTopology& Topology);
};
