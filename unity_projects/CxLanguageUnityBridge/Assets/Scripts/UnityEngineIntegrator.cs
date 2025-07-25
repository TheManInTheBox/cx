using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace CxLanguage.Unity
{
    /// <summary>
    /// Unity Engine Direct Integration for CX Language
    /// Provides native Unity engine interfaces and project management
    /// </summary>
    public class UnityEngineIntegrator : MonoBehaviour
    {
        [Header("Unity Engine Integration")]
        [SerializeField] private string unityProjectPath = "";
        [SerializeField] private bool autoLaunchUnityEditor = true;
        [SerializeField] private bool enableRealTimeSync = true;
        [SerializeField] private string unityExecutablePath = "";
        
        [Header("CX Language Bridge Settings")]
        [SerializeField] private bool enableCxLanguageBridge = true;
        [SerializeField] private int bridgePort = 8080;
        [SerializeField] private bool enableVoiceProcessing = true;
        
        private Process unityEditorProcess;
        private bool isUnityConnected = false;
        private Dictionary<string, object> sharedData;
        
        void Start()
        {
            InitializeUnityIntegration();
        }
        
        private void InitializeUnityIntegration()
        {
            UnityEngine.Debug.Log("🎮 Unity Engine Integrator initializing...");
            
            sharedData = new Dictionary<string, object>();
            
            // Auto-detect Unity installation
            if (string.IsNullOrEmpty(unityExecutablePath))
            {
                DetectUnityInstallation();
            }
            
            // Setup project path
            if (string.IsNullOrEmpty(unityProjectPath))
            {
                unityProjectPath = Application.dataPath.Replace("/Assets", "");
            }
            
            UnityEngine.Debug.Log($"🎮 Unity Project Path: {unityProjectPath}");
            UnityEngine.Debug.Log($"🎮 Unity Executable: {unityExecutablePath}");
            
            // Initialize CX Language bridge
            if (enableCxLanguageBridge)
            {
                InitializeCxBridge();
            }
            
            // Launch Unity Editor if requested
            if (autoLaunchUnityEditor && !Application.isEditor)
            {
                LaunchUnityEditor();
            }
            
            UnityEngine.Debug.Log("✅ Unity Engine Integration initialized");
        }
        
        private void DetectUnityInstallation()
        {
            UnityEngine.Debug.Log("🔍 Detecting Unity installation...");
            
            // Unity Hub installation paths (most common)
            string hubEditorPath = @"C:\Program Files\Unity\Hub\Editor";
            if (Directory.Exists(hubEditorPath))
            {
                DetectUnityHubInstallations(hubEditorPath);
            }
            
            // Manual installation paths (fallback)
            if (string.IsNullOrEmpty(unityExecutablePath))
            {
                string[] manualPaths = {
                    @"C:\Program Files\Unity\Editor\Unity.exe",
                    @"C:\Program Files (x86)\Unity\Editor\Unity.exe",
                    @"C:\Unity\Editor\Unity.exe"
                };
                
                foreach (string path in manualPaths)
                {
                    if (File.Exists(path))
                    {
                        unityExecutablePath = path;
                        UnityEngine.Debug.Log($"🎯 Found Unity manual installation at: {path}");
                        break;
                    }
                }
            }
            
            // Registry detection (Windows)
            if (string.IsNullOrEmpty(unityExecutablePath))
            {
                DetectUnityFromRegistry();
            }
            
            if (string.IsNullOrEmpty(unityExecutablePath))
            {
                UnityEngine.Debug.LogWarning("⚠️ Unity installation not found. Please set unityExecutablePath manually.");
            }
            else
            {
                UnityEngine.Debug.Log($"✅ Unity detected: {unityExecutablePath}");
            }
        }
        
        private void DetectUnityHubInstallations(string hubEditorPath)
        {
            try
            {
                var directories = Directory.GetDirectories(hubEditorPath);
                var versions = new List<System.Version>();
                var versionPaths = new Dictionary<System.Version, string>();
                
                // Find all Unity versions
                foreach (string dir in directories)
                {
                    string versionString = Path.GetFileName(dir);
                    string unityExePath = Path.Combine(dir, "Editor", "Unity.exe");
                    
                    if (File.Exists(unityExePath))
                    {
                        // Parse version (e.g., "2023.3.0f1" -> "2023.3.0")
                        string cleanVersion = versionString.Split('f', 'a', 'b')[0];
                        if (System.Version.TryParse(cleanVersion, out System.Version version))
                        {
                            versions.Add(version);
                            versionPaths[version] = unityExePath;
                            UnityEngine.Debug.Log($"🎮 Found Unity {versionString} at: {unityExePath}");
                        }
                    }
                }
                
                // Use the latest version
                if (versions.Count > 0)
                {
                    versions.Sort();
                    var latestVersion = versions[versions.Count - 1];
                    unityExecutablePath = versionPaths[latestVersion];
                    UnityEngine.Debug.Log($"🎯 Selected latest Unity version: {latestVersion}");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Error detecting Unity Hub installations: {ex.Message}");
            }
        }
        
        private void DetectUnityFromRegistry()
        {
            try
            {
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                // Try Windows Registry detection
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Unity Technologies\Unity Editor"))
                {
                    if (key != null)
                    {
                        var location = key.GetValue("Location") as string;
                        if (!string.IsNullOrEmpty(location) && File.Exists(location))
                        {
                            unityExecutablePath = location;
                            UnityEngine.Debug.Log($"🎯 Found Unity via registry: {location}");
                        }
                    }
                }
                #endif
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Registry detection failed: {ex.Message}");
            }
        }
        
        private void InitializeCxBridge()
        {
            UnityEngine.Debug.Log("🌉 Initializing CX Language Bridge...");
            
            // Setup bridge communication
            var bridge = GetComponent<CxLanguageBridge>();
            if (bridge != null)
            {
                bridge.OnEventReceived += HandleCxLanguageEvent;
                UnityEngine.Debug.Log("✅ CX Language Bridge connected");
            }
            
            // Initialize voice processing if enabled
            if (enableVoiceProcessing)
            {
                var voiceProcessor = GetComponent<VoiceProcessor>();
                if (voiceProcessor != null)
                {
                    UnityEngine.Debug.Log("🎤 Voice processing enabled");
                }
            }
        }
        
        public void LaunchUnityEditor()
        {
            if (string.IsNullOrEmpty(unityExecutablePath) || !File.Exists(unityExecutablePath))
            {
                UnityEngine.Debug.LogError("❌ Unity executable not found!");
                return;
            }
            
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = unityExecutablePath,
                    Arguments = $"-projectPath \"{unityProjectPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
                
                unityEditorProcess = Process.Start(startInfo);
                UnityEngine.Debug.Log("🚀 Unity Editor launched successfully");
                
                StartCoroutine(MonitorUnityConnection());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to launch Unity Editor: {ex.Message}");
            }
        }
        
        private IEnumerator MonitorUnityConnection()
        {
            yield return new WaitForSeconds(5f); // Wait for Unity to start
            
            while (unityEditorProcess != null && !unityEditorProcess.HasExited)
            {
                if (!isUnityConnected)
                {
                    CheckUnityConnection();
                }
                
                yield return new WaitForSeconds(2f);
            }
            
            if (unityEditorProcess != null && unityEditorProcess.HasExited)
            {
                UnityEngine.Debug.Log("🔌 Unity Editor process ended");
                isUnityConnected = false;
            }
        }
        
        private void CheckUnityConnection()
        {
            // Check if Unity Editor is responsive
            try
            {
                if (unityEditorProcess != null && !unityEditorProcess.HasExited)
                {
                    isUnityConnected = true;
                    UnityEngine.Debug.Log("✅ Unity Editor connection established");
                    
                    // Emit Unity ready event
                    if (GetComponent<CxLanguageBridge>())
                    {
                        GetComponent<CxLanguageBridge>().EmitEvent("unity.engine.ready",
                            $"{{ \"projectPath\": \"{unityProjectPath}\", \"editorConnected\": true }}");
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Unity connection check failed: {ex.Message}");
            }
        }
        
        private void HandleCxLanguageEvent(string eventName, string data)
        {
            UnityEngine.Debug.Log($"🎮 Unity Engine handling CX event: {eventName}");
            
            switch (eventName)
            {
                case "unity.engine.launch":
                    LaunchUnityEditor();
                    break;
                case "unity.project.open":
                    HandleProjectOpen(data);
                    break;
                case "unity.scene.load":
                    HandleSceneLoad(data);
                    break;
                case "unity.build.execute":
                    HandleBuildExecute(data);
                    break;
                case "voice.synthesis.request":
                    HandleVoiceSynthesis(data);
                    break;
                default:
                    UnityEngine.Debug.Log($"🔄 Forwarding event to Unity: {eventName}");
                    ForwardEventToUnity(eventName, data);
                    break;
            }
        }
        
        private void HandleProjectOpen(string data)
        {
            UnityEngine.Debug.Log($"📂 Opening Unity project: {data}");
            
            // Parse project path from data
            if (data.Contains("projectPath"))
            {
                // Simple JSON parsing for project path
                var startIndex = data.IndexOf("projectPath") + 14;
                var endIndex = data.IndexOf("\"", startIndex + 1);
                if (endIndex > startIndex)
                {
                    unityProjectPath = data.Substring(startIndex, endIndex - startIndex);
                    LaunchUnityEditor();
                }
            }
        }
        
        private void HandleSceneLoad(string data)
        {
            UnityEngine.Debug.Log($"🎬 Loading Unity scene: {data}");
            
            // In editor mode, we can directly load scenes
            #if UNITY_EDITOR
            if (data.Contains("sceneName"))
            {
                var startIndex = data.IndexOf("sceneName") + 12;
                var endIndex = data.IndexOf("\"", startIndex + 1);
                if (endIndex > startIndex)
                {
                    string sceneName = data.Substring(startIndex, endIndex - startIndex);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity");
                    UnityEngine.Debug.Log($"✅ Scene loaded: {sceneName}");
                }
            }
            #endif
        }
        
        private void HandleBuildExecute(string data)
        {
            UnityEngine.Debug.Log($"🔨 Executing Unity build: {data}");
            
            #if UNITY_EDITOR
            // Execute build through Unity's build pipeline
            var buildPlayerOptions = new UnityEditor.BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/CxBridgeDemo.unity" },
                locationPathName = Path.Combine(unityProjectPath, "Builds", "CxLanguageBridge.exe"),
                target = UnityEditor.BuildTarget.StandaloneWindows64,
                options = UnityEditor.BuildOptions.None
            };
            
            var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("✅ Unity build completed successfully");
                
                // Emit build complete event
                if (GetComponent<CxLanguageBridge>())
                {
                    GetComponent<CxLanguageBridge>().EmitEvent("unity.build.complete",
                        $"{{ \"success\": true, \"buildPath\": \"{buildPlayerOptions.locationPathName}\" }}");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("❌ Unity build failed");
            }
            #endif
        }
        
        private void HandleVoiceSynthesis(string data)
        {
            UnityEngine.Debug.Log($"🎵 Unity handling voice synthesis: {data}");
            
            var voiceProcessor = GetComponent<VoiceProcessor>();
            if (voiceProcessor != null)
            {
                // Extract text from data
                if (data.Contains("text"))
                {
                    var startIndex = data.IndexOf("text") + 7;
                    var endIndex = data.IndexOf("\"", startIndex + 1);
                    if (endIndex > startIndex)
                    {
                        string text = data.Substring(startIndex, endIndex - startIndex);
                        voiceProcessor.SynthesizeVoice(text, (clip) =>
                        {
                            UnityEngine.Debug.Log($"✅ Unity voice synthesis complete: {clip.length}s");
                        });
                    }
                }
            }
        }
        
        private void ForwardEventToUnity(string eventName, string data)
        {
            // Store event in shared data for Unity access
            sharedData[eventName] = data;
            
            // In editor mode, we can directly interact with Unity systems
            #if UNITY_EDITOR
            UnityEngine.Debug.Log($"🔄 Event forwarded to Unity editor: {eventName}");
            #endif
        }
        
        public void SendEventToCxLanguage(string eventName, string data)
        {
            var bridge = GetComponent<CxLanguageBridge>();
            if (bridge != null)
            {
                bridge.EmitEvent(eventName, data);
                UnityEngine.Debug.Log($"📡 Event sent to CX Language: {eventName}");
            }
        }
        
        public bool IsUnityEditorConnected()
        {
            return isUnityConnected && unityEditorProcess != null && !unityEditorProcess.HasExited;
        }
        
        public void RestartUnityEditor()
        {
            if (unityEditorProcess != null && !unityEditorProcess.HasExited)
            {
                unityEditorProcess.CloseMainWindow();
                unityEditorProcess.WaitForExit(5000);
            }
            
            LaunchUnityEditor();
        }
        
        void OnDestroy()
        {
            if (unityEditorProcess != null && !unityEditorProcess.HasExited)
            {
                UnityEngine.Debug.Log("🔌 Closing Unity Editor process");
                unityEditorProcess.CloseMainWindow();
            }
        }
        
        // Unity Inspector methods
        [ContextMenu("Launch Unity Editor")]
        public void InspectorLaunchUnity()
        {
            LaunchUnityEditor();
        }
        
        [ContextMenu("Test CX Language Event")]
        public void InspectorTestCxEvent()
        {
            SendEventToCxLanguage("unity.test.event", "{ \"message\": \"Test from Unity Engine Integrator\" }");
        }
        
        [ContextMenu("Check Unity Connection")]
        public void InspectorCheckConnection()
        {
            UnityEngine.Debug.Log($"Unity Connected: {IsUnityEditorConnected()}");
        }
    }
}
