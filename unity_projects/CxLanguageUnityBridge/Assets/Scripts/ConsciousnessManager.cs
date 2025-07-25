using UnityEngine;
using System.Collections.Generic;

namespace CxLanguage.Unity
{
    /// <summary>
    /// Consciousness Manager for Maya Nakamura's Unity Bridge
    /// Handles consciousness-aware processing and adaptive learning patterns
    /// </summary>
    public class ConsciousnessManager : MonoBehaviour
    {
        [Header("Consciousness Settings")]
        [SerializeField] private bool enableConsciousnessLogging = true;
        [SerializeField] private float adaptationThreshold = 0.8f;
        [SerializeField] private int maxConsciousnessStates = 100;
        
        [Header("Learning Settings")]
        [SerializeField] private float learningRate = 0.1f;
        [SerializeField] private bool enableDynamicAdaptation = true;
        
        private Dictionary<string, ConsciousnessState> consciousnessRegistry;
        private Queue<string> recentEvents;
        private float currentConsciousnessLevel;
        
        void Start()
        {
            InitializeConsciousness();
            Debug.Log("🧠 Consciousness Manager initialized with Maya Nakamura's adaptive patterns");
        }
        
        private void InitializeConsciousness()
        {
            consciousnessRegistry = new Dictionary<string, ConsciousnessState>();
            recentEvents = new Queue<string>();
            currentConsciousnessLevel = 1.0f;
            
            RegisterDefaultConsciousnessStates();
        }
        
        private void RegisterDefaultConsciousnessStates()
        {
            RegisterConsciousnessState("voice_processing", new ConsciousnessState
            {
                name = "Voice Processing",
                confidence = 0.9f,
                capabilities = new List<string> { "audio synthesis", "voice recognition", "real-time processing" }
            });
            
            RegisterConsciousnessState("visual_feedback", new ConsciousnessState
            {
                name = "Visual Feedback",
                confidence = 0.85f,
                capabilities = new List<string> { "particle effects", "color animation", "real-time visualization" }
            });
            
            RegisterConsciousnessState("azure_integration", new ConsciousnessState
            {
                name = "Azure Integration",
                confidence = 0.95f,
                capabilities = new List<string> { "realtime api", "websocket management", "voice synthesis" }
            });
            
            Debug.Log($"✅ Registered {consciousnessRegistry.Count} default consciousness states");
        }
        
        public void RegisterConsciousnessState(string id, ConsciousnessState state)
        {
            if (consciousnessRegistry.ContainsKey(id))
            {
                consciousnessRegistry[id] = state;
                LogConsciousness($"🔄 Updated consciousness state: {id}");
            }
            else
            {
                consciousnessRegistry.Add(id, state);
                LogConsciousness($"➕ Added consciousness state: {id}");
            }
            
            // Trigger consciousness update events
            if (GetComponent<CxLanguageBridge>())
            {
                GetComponent<CxLanguageBridge>().EmitEvent("consciousness.state.updated", 
                    $"{{ \"stateId\": \"{id}\", \"confidence\": {state.confidence}, \"name\": \"{state.name}\" }}");
            }
        }
        
        public ConsciousnessState GetConsciousnessState(string id)
        {
            return consciousnessRegistry.ContainsKey(id) ? consciousnessRegistry[id] : null;
        }
        
        public void ProcessConsciousnessEvent(string eventName, string data)
        {
            LogConsciousness($"🎯 Processing consciousness event: {eventName}");
            
            // Add to recent events queue
            recentEvents.Enqueue($"{eventName}:{data}");
            if (recentEvents.Count > 50)
            {
                recentEvents.Dequeue();
            }
            
            // Process different event types
            switch (eventName)
            {
                case "voice.synthesis.start":
                    HandleVoiceSynthesisStart(data);
                    break;
                case "voice.synthesis.complete":
                    HandleVoiceSynthesisComplete(data);
                    break;
                case "consciousness.adaptation.request":
                    HandleAdaptationRequest(data);
                    break;
                case "azure.connection.established":
                    HandleAzureConnection(data);
                    break;
                default:
                    HandleGenericConsciousnessEvent(eventName, data);
                    break;
            }
            
            // Check if adaptation is needed
            if (enableDynamicAdaptation)
            {
                CheckAdaptationNeed();
            }
        }
        
        private void HandleVoiceSynthesisStart(string data)
        {
            LogConsciousness("🎤 Voice synthesis starting - consciousness focused on audio processing");
            UpdateConsciousnessLevel("voice_processing", 0.1f);
            
            // Notify visual feedback
            if (GetComponent<VisualFeedback>())
            {
                GetComponent<VisualFeedback>().OnVoiceStart();
            }
        }
        
        private void HandleVoiceSynthesisComplete(string data)
        {
            LogConsciousness("✅ Voice synthesis complete - consciousness processing results");
            UpdateConsciousnessLevel("voice_processing", 0.05f);
            
            // Notify visual feedback
            if (GetComponent<VisualFeedback>())
            {
                GetComponent<VisualFeedback>().OnVoiceComplete();
            }
        }
        
        private void HandleAdaptationRequest(string data)
        {
            LogConsciousness("🧠 Processing consciousness adaptation request");
            
            // Parse adaptation data (simplified JSON parsing)
            if (data.Contains("voice_optimization"))
            {
                AdaptVoiceProcessing();
            }
            else if (data.Contains("visual_enhancement"))
            {
                AdaptVisualFeedback();
            }
            
            // Emit adaptation complete event
            if (GetComponent<CxLanguageBridge>())
            {
                GetComponent<CxLanguageBridge>().EmitEvent("consciousness.adaptation.complete",
                    $"{{ \"adaptationType\": \"dynamic\", \"success\": true }}");
            }
        }
        
        private void HandleAzureConnection(string data)
        {
            LogConsciousness("☁️ Azure connection established - consciousness aware of cloud capabilities");
            UpdateConsciousnessLevel("azure_integration", 0.1f);
        }
        
        private void HandleGenericConsciousnessEvent(string eventName, string data)
        {
            LogConsciousness($"🔄 Processing generic consciousness event: {eventName}");
            currentConsciousnessLevel = Mathf.Clamp01(currentConsciousnessLevel + 0.01f);
        }
        
        private void UpdateConsciousnessLevel(string stateId, float delta)
        {
            if (consciousnessRegistry.ContainsKey(stateId))
            {
                var state = consciousnessRegistry[stateId];
                state.confidence = Mathf.Clamp01(state.confidence + delta);
                consciousnessRegistry[stateId] = state;
                
                LogConsciousness($"📈 Consciousness level updated for {stateId}: {state.confidence:F2}");
            }
            
            currentConsciousnessLevel = Mathf.Clamp01(currentConsciousnessLevel + delta);
        }
        
        private void CheckAdaptationNeed()
        {
            foreach (var state in consciousnessRegistry.Values)
            {
                if (state.confidence < adaptationThreshold)
                {
                    LogConsciousness($"⚠️ Adaptation needed for {state.name} (confidence: {state.confidence:F2})");
                    TriggerAdaptation(state.name);
                }
            }
        }
        
        private void TriggerAdaptation(string stateName)
        {
            LogConsciousness($"🚀 Triggering consciousness adaptation for: {stateName}");
            
            // Emit adaptation event to CX Language
            if (GetComponent<CxLanguageBridge>())
            {
                GetComponent<CxLanguageBridge>().EmitEvent("consciousness.adaptation.triggered",
                    $"{{ \"stateName\": \"{stateName}\", \"threshold\": {adaptationThreshold} }}");
            }
        }
        
        private void AdaptVoiceProcessing()
        {
            LogConsciousness("🎵 Adapting voice processing capabilities");
            
            if (consciousnessRegistry.ContainsKey("voice_processing"))
            {
                var state = consciousnessRegistry["voice_processing"];
                state.capabilities.Add("enhanced clarity");
                state.capabilities.Add("noise reduction");
                state.confidence = Mathf.Min(1.0f, state.confidence + 0.2f);
                consciousnessRegistry["voice_processing"] = state;
            }
        }
        
        private void AdaptVisualFeedback()
        {
            LogConsciousness("✨ Adapting visual feedback capabilities");
            
            if (consciousnessRegistry.ContainsKey("visual_feedback"))
            {
                var state = consciousnessRegistry["visual_feedback"];
                state.capabilities.Add("enhanced particles");
                state.capabilities.Add("dynamic color mapping");
                state.confidence = Mathf.Min(1.0f, state.confidence + 0.15f);
                consciousnessRegistry["visual_feedback"] = state;
            }
        }
        
        public string GetConsciousnessReport()
        {
            string report = "📊 CONSCIOUSNESS STATUS REPORT\n";
            report += $"Current Level: {currentConsciousnessLevel:F2}\n";
            report += $"Active States: {consciousnessRegistry.Count}\n";
            report += $"Recent Events: {recentEvents.Count}\n\n";
            
            foreach (var kvp in consciousnessRegistry)
            {
                var state = kvp.Value;
                report += $"🧠 {state.name}: {state.confidence:F2} confidence\n";
                report += $"   Capabilities: {string.Join(", ", state.capabilities)}\n\n";
            }
            
            return report;
        }
        
        private void LogConsciousness(string message)
        {
            if (enableConsciousnessLogging)
            {
                Debug.Log($"[CONSCIOUSNESS] {message}");
            }
        }
        
        // Public methods for Unity Inspector
        [ContextMenu("Print Consciousness Report")]
        public void PrintConsciousnessReport()
        {
            Debug.Log(GetConsciousnessReport());
        }
        
        [ContextMenu("Trigger Test Adaptation")]
        public void TriggerTestAdaptation()
        {
            ProcessConsciousnessEvent("consciousness.adaptation.request", "{ \"type\": \"voice_optimization\" }");
        }
    }
    
    [System.Serializable]
    public struct ConsciousnessState
    {
        public string name;
        public float confidence;
        public List<string> capabilities;
    }
}
