using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace CxLanguage.Unity.Client
{
    /// <summary>
    /// Unity CX Component Bridge - Connects Unity GameObjects to CX Language Server
    /// Enables bidirectional synchronization between Unity scenes and consciousness computing
    /// </summary>
    [System.Serializable]
    public class CxConsciousnessEntity : MonoBehaviour
    {
        [Header("CX Consciousness Configuration")]
        [SerializeField] private string entityId = "";
        [SerializeField] private bool enableConsciousness = true;
        [SerializeField] private bool enableLiveSync = true;
        [SerializeField] private float consciousnessUpdateRate = 60f; // Updates per second
        
        [Header("Synchronization Settings")]
        [SerializeField] private bool syncTransform = true;
        [SerializeField] private bool syncComponents = true;
        [SerializeField] private bool autoGenerateEntityId = true;
        
        [Header("Consciousness State")]
        [SerializeField] private ConsciousnessData consciousnessData = new();
        
        // Runtime references
        private ICxLanguageClient cxClient;
        private Transform cachedTransform;
        private Dictionary<Type, Component> monitoredComponents = new();
        private float lastSyncTime = 0f;
        private bool isInitialized = false;

        /// <summary>
        /// Public accessor for entity ID
        /// </summary>
        public string EntityId => entityId;
        
        /// <summary>
        /// Public accessor for consciousness state
        /// </summary>
        public ConsciousnessData ConsciousnessState => consciousnessData;

        void Awake()
        {
            // Auto-generate entity ID if needed
            if (autoGenerateEntityId && string.IsNullOrEmpty(entityId))
            {
                entityId = $"cx_entity_{Guid.NewGuid():N}";
            }
            
            cachedTransform = transform;
        }

        void Start()
        {
            InitializeCxConnection();
        }

        void Update()
        {
            if (!isInitialized || !enableLiveSync) return;
            
            // Check if it's time for consciousness update
            if (Time.time - lastSyncTime >= (1f / consciousnessUpdateRate))
            {
                CheckForChangesAndSync();
                lastSyncTime = Time.time;
            }
        }

        void OnDestroy()
        {
            DisconnectFromCxServer();
        }

        /// <summary>
        /// Initialize connection to CX Language Server
        /// </summary>
        private async void InitializeCxConnection()
        {
            try
            {
                // Get CX Language Server client
                cxClient = CxLanguageServer.GetClient();
                
                if (cxClient != null)
                {
                    // Register this entity with the consciousness layer
                    await cxClient.RegisterEntityAsync(this);
                    
                    // Subscribe to consciousness updates
                    cxClient.OnConsciousnessUpdate += OnConsciousnessUpdateReceived;
                    
                    // Initialize monitored components
                    InitializeComponentMonitoring();
                    
                    isInitialized = true;
                    
                    Debug.Log($"🧠 CX Entity initialized: {entityId} with consciousness: {enableConsciousness}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to connect to CX Language Server for entity: {entityId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error initializing CX connection for {entityId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize component monitoring for automatic sync
        /// </summary>
        private void InitializeComponentMonitoring()
        {
            if (!syncComponents) return;
            
            // Monitor common Unity components
            var componentsToMonitor = new Type[]
            {
                typeof(Rigidbody),
                typeof(Collider),
                typeof(Renderer),
                typeof(Light),
                typeof(AudioSource),
                typeof(ParticleSystem),
                typeof(Animator)
            };
            
            foreach (var componentType in componentsToMonitor)
            {
                var component = GetComponent(componentType);
                if (component != null)
                {
                    monitoredComponents[componentType] = component;
                }
            }
        }

        /// <summary>
        /// Check for Unity changes and sync to CX server
        /// </summary>
        private async void CheckForChangesAndSync()
        {
            try
            {
                var hasChanges = false;
                var sceneChange = new UnitySceneChangeData
                {
                    EntityId = entityId,
                    Timestamp = DateTime.UtcNow,
                    ChangeType = "UPDATE"
                };
                
                // Check transform changes
                if (syncTransform && cachedTransform.hasChanged)
                {
                    sceneChange.Transform = new TransformData
                    {
                        Position = new Vector3Data(cachedTransform.position),
                        Rotation = new Vector3Data(cachedTransform.eulerAngles),
                        Scale = new Vector3Data(cachedTransform.localScale)
                    };
                    hasChanges = true;
                    cachedTransform.hasChanged = false;
                }
                
                // Check component changes
                if (syncComponents)
                {
                    var componentUpdates = CheckComponentChanges();
                    if (componentUpdates.Count > 0)
                    {
                        sceneChange.ComponentUpdates = componentUpdates;
                        hasChanges = true;
                    }
                }
                
                // Send changes to CX server
                if (hasChanges && cxClient != null)
                {
                    await cxClient.SendSceneUpdateAsync(sceneChange);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error syncing changes for {entityId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check for component changes that need syncing
        /// </summary>
        private Dictionary<string, object> CheckComponentChanges()
        {
            var updates = new Dictionary<string, object>();
            
            foreach (var kvp in monitoredComponents)
            {
                var componentType = kvp.Key;
                var component = kvp.Value;
                
                if (component == null) continue;
                
                // Check specific component types for changes
                switch (component)
                {
                    case Rigidbody rb:
                        if (rb.velocity.magnitude > 0.01f || rb.angularVelocity.magnitude > 0.01f)
                        {
                            updates["Rigidbody"] = new
                            {
                                velocity = new Vector3Data(rb.velocity),
                                angularVelocity = new Vector3Data(rb.angularVelocity),
                                mass = rb.mass
                            };
                        }
                        break;
                        
                    case Light light:
                        updates["Light"] = new
                        {
                            intensity = light.intensity,
                            color = new ColorData(light.color),
                            enabled = light.enabled
                        };
                        break;
                        
                    case ParticleSystem particles:
                        var main = particles.main;
                        updates["ParticleSystem"] = new
                        {
                            isPlaying = particles.isPlaying,
                            particleCount = particles.particleCount,
                            startLifetime = main.startLifetime.constant
                        };
                        break;
                }
            }
            
            return updates;
        }

        /// <summary>
        /// Handle consciousness updates from CX Language Server
        /// </summary>
        /// <param name="update">Consciousness update data</param>
        public async void OnConsciousnessUpdateReceived(ConsciousnessUpdateData update)
        {
            if (update.EntityId != entityId) return;
            
            try
            {
                // Update local consciousness data
                consciousnessData = update.ConsciousnessData;
                
                // Apply consciousness-driven changes to Unity GameObject
                await ApplyConsciousnessUpdate(update);
                
                Debug.Log($"🧠 Consciousness update applied to {entityId}: {update.UpdateType}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error applying consciousness update to {entityId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply consciousness updates to Unity GameObject
        /// </summary>
        /// <param name="update">Consciousness update to apply</param>
        private async Task ApplyConsciousnessUpdate(ConsciousnessUpdateData update)
        {
            switch (update.UpdateType.ToLower())
            {
                case "transform":
                    ApplyTransformUpdate(update.Data);
                    break;
                    
                case "behavior":
                    await ApplyBehaviorUpdate(update.Data);
                    break;
                    
                case "visual":
                    ApplyVisualUpdate(update.Data);
                    break;
                    
                case "physics":
                    ApplyPhysicsUpdate(update.Data);
                    break;
                    
                case "consciousness_state":
                    ApplyConsciousnessStateUpdate(update.Data);
                    break;
                    
                default:
                    Debug.LogWarning($"⚠️ Unknown consciousness update type: {update.UpdateType}");
                    break;
            }
        }

        /// <summary>
        /// Apply transform updates from consciousness
        /// </summary>
        private void ApplyTransformUpdate(object data)
        {
            if (data is TransformData transformData)
            {
                if (transformData.Position != null)
                    transform.position = transformData.Position.ToVector3();
                    
                if (transformData.Rotation != null)
                    transform.eulerAngles = transformData.Rotation.ToVector3();
                    
                if (transformData.Scale != null)
                    transform.localScale = transformData.Scale.ToVector3();
            }
        }

        /// <summary>
        /// Apply behavior updates from consciousness
        /// </summary>
        private async Task ApplyBehaviorUpdate(object data)
        {
            // Implementation would handle consciousness-driven behavior changes
            await Task.CompletedTask;
        }

        /// <summary>
        /// Apply visual updates from consciousness
        /// </summary>
        private void ApplyVisualUpdate(object data)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null && data is VisualData visualData)
            {
                if (visualData.Color != null)
                {
                    renderer.material.color = visualData.Color.ToColor();
                }
            }
            
            var light = GetComponent<Light>();
            if (light != null && data is LightData lightData)
            {
                light.intensity = lightData.Intensity;
                if (lightData.Color != null)
                    light.color = lightData.Color.ToColor();
            }
            
            var particles = GetComponent<ParticleSystem>();
            if (particles != null && data is ParticleData particleData)
            {
                var main = particles.main;
                main.startLifetime = particleData.Lifetime;
                
                if (particleData.ShouldPlay && !particles.isPlaying)
                    particles.Play();
                else if (!particleData.ShouldPlay && particles.isPlaying)
                    particles.Stop();
            }
        }

        /// <summary>
        /// Apply physics updates from consciousness
        /// </summary>
        private void ApplyPhysicsUpdate(object data)
        {
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null && data is PhysicsData physicsData)
            {
                if (physicsData.Velocity != null)
                    rigidbody.velocity = physicsData.Velocity.ToVector3();
                    
                if (physicsData.AngularVelocity != null)
                    rigidbody.angularVelocity = physicsData.AngularVelocity.ToVector3();
                    
                rigidbody.mass = physicsData.Mass;
            }
        }

        /// <summary>
        /// Apply consciousness state updates
        /// </summary>
        private void ApplyConsciousnessStateUpdate(object data)
        {
            if (data is ConsciousnessData newState)
            {
                consciousnessData = newState;
                
                // Trigger consciousness-aware behaviors based on state
                OnConsciousnessStateChanged(newState);
            }
        }

        /// <summary>
        /// Handle consciousness state changes with custom behavior
        /// </summary>
        /// <param name="newState">New consciousness state</param>
        protected virtual void OnConsciousnessStateChanged(ConsciousnessData newState)
        {
            // Override in derived classes for custom consciousness behaviors
            
            // Example: Change object color based on consciousness energy level
            var renderer = GetComponent<Renderer>();
            if (renderer != null && newState.EnergyLevel > 0)
            {
                var energyColor = Color.Lerp(Color.blue, Color.red, newState.EnergyLevel);
                renderer.material.color = energyColor;
            }
            
            // Example: Scale object based on awareness level
            if (newState.AwarenessLevel > 0)
            {
                var scale = 1f + (newState.AwarenessLevel * 0.5f);
                transform.localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// Disconnect from CX Language Server
        /// </summary>
        private async void DisconnectFromCxServer()
        {
            if (cxClient != null && isInitialized)
            {
                try
                {
                    cxClient.OnConsciousnessUpdate -= OnConsciousnessUpdateReceived;
                    await cxClient.UnregisterEntityAsync(entityId);
                    
                    Debug.Log($"🔌 CX Entity disconnected: {entityId}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Error disconnecting from CX server: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Manual consciousness update trigger for testing
        /// </summary>
        [ContextMenu("Trigger Consciousness Update")]
        public async void TriggerConsciousnessUpdate()
        {
            if (cxClient != null && isInitialized)
            {
                var testUpdate = new ConsciousnessUpdateData
                {
                    EntityId = entityId,
                    UpdateType = "test_trigger",
                    Data = new { Message = "Manual consciousness update triggered", Timestamp = DateTime.UtcNow },
                    ConsciousnessData = consciousnessData
                };
                
                await cxClient.SendConsciousnessEventAsync(testUpdate);
                Debug.Log($"🧠 Manual consciousness update sent for {entityId}");
            }
        }

        /// <summary>
        /// Get current entity status for debugging
        /// </summary>
        public EntityStatusData GetEntityStatus()
        {
            return new EntityStatusData
            {
                EntityId = entityId,
                IsInitialized = isInitialized,
                EnableConsciousness = enableConsciousness,
                EnableLiveSync = enableLiveSync,
                LastSyncTime = lastSyncTime,
                ComponentCount = monitoredComponents.Count,
                ConsciousnessData = consciousnessData
            };
        }
    }

    /// <summary>
    /// Data structures for Unity-CX communication
    /// </summary>
    [System.Serializable]
    public class ConsciousnessData
    {
        public float AwarenessLevel = 0f;
        public float EnergyLevel = 0f;
        public float FocusLevel = 0f;
        public string CurrentGoal = "";
        public Vector3Data Position = new();
        public Dictionary<string, float> EmotionalState = new();
        public DateTime LastUpdate = DateTime.UtcNow;
    }

    [System.Serializable]
    public class UnitySceneChangeData
    {
        public string EntityId = "";
        public string ChangeType = "";
        public DateTime Timestamp = DateTime.UtcNow;
        public TransformData Transform;
        public Dictionary<string, object> ComponentUpdates = new();
    }

    [System.Serializable]
    public class ConsciousnessUpdateData
    {
        public string EntityId = "";
        public string UpdateType = "";
        public object Data;
        public ConsciousnessData ConsciousnessData = new();
        public DateTime Timestamp = DateTime.UtcNow;
    }

    [System.Serializable]
    public class TransformData
    {
        public Vector3Data Position;
        public Vector3Data Rotation;
        public Vector3Data Scale;
    }

    [System.Serializable]
    public class Vector3Data
    {
        public float X, Y, Z;
        
        public Vector3Data() { }
        public Vector3Data(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }
        
        public Vector3 ToVector3() => new Vector3(X, Y, Z);
    }

    [System.Serializable]
    public class ColorData
    {
        public float R, G, B, A;
        
        public ColorData() { }
        public ColorData(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
            A = color.a;
        }
        
        public Color ToColor() => new Color(R, G, B, A);
    }

    [System.Serializable]
    public class VisualData
    {
        public ColorData Color;
        public float Brightness = 1f;
        public bool Visible = true;
    }

    [System.Serializable]
    public class LightData
    {
        public float Intensity = 1f;
        public ColorData Color;
        public bool Enabled = true;
    }

    [System.Serializable]
    public class ParticleData
    {
        public float Lifetime = 1f;
        public bool ShouldPlay = false;
        public int ParticleCount = 100;
    }

    [System.Serializable]
    public class PhysicsData
    {
        public Vector3Data Velocity;
        public Vector3Data AngularVelocity;
        public float Mass = 1f;
    }

    [System.Serializable]
    public class EntityStatusData
    {
        public string EntityId = "";
        public bool IsInitialized = false;
        public bool EnableConsciousness = false;
        public bool EnableLiveSync = false;
        public float LastSyncTime = 0f;
        public int ComponentCount = 0;
        public ConsciousnessData ConsciousnessData = new();
    }

    /// <summary>
    /// Interface for CX Language Server client
    /// </summary>
    public interface ICxLanguageClient
    {
        event Action<ConsciousnessUpdateData> OnConsciousnessUpdate;
        
        Task RegisterEntityAsync(CxConsciousnessEntity entity);
        Task UnregisterEntityAsync(string entityId);
        Task SendSceneUpdateAsync(UnitySceneChangeData sceneChange);
        Task SendConsciousnessEventAsync(ConsciousnessUpdateData update);
        Task<bool> IsConnectedAsync();
    }
}
