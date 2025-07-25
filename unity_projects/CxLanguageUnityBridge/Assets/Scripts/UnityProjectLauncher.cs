using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace CxLanguage.Unity
{
    /// <summary>
    /// Unity Project Launcher for CX Language Integration
    /// Handles local Unity installation detection and project launching
    /// </summary>
    public class UnityProjectLauncher : MonoBehaviour
    {
        [Header("Unity Detection Settings")]
        [SerializeField] private bool autoDetectUnity = true;
        [SerializeField] private bool preferLatestVersion = true;
        [SerializeField] private string customUnityPath = "";
        
        [Header("Project Launch Settings")]
        [SerializeField] private bool launchInBatchMode = false;
        [SerializeField] private bool enableUnityLog = true;
        [SerializeField] private string[] additionalArguments = new string[0];
        
        [Header("CX Language Integration")]
        [SerializeField] private bool enableCxIntegration = true;
        [SerializeField] private string cxLanguageRuntimePath = "";
        
        private List<UnityInstallation> detectedInstallations = new List<UnityInstallation>();
        private UnityInstallation selectedInstallation;
        
        [System.Serializable]
        public class UnityInstallation
        {
            public string version;
            public string path;
            public string editorPath;
            public bool isLTS;
            public DateTime installDate;
            
            public override string ToString()
            {
                return $"Unity {version} {(isLTS ? "(LTS)" : "")} - {path}";
            }
        }
        
        void Start()
        {
            InitializeUnityLauncher();
        }
        
        private void InitializeUnityLauncher()
        {
            UnityEngine.Debug.Log("🚀 Unity Project Launcher initializing...");
            
            if (autoDetectUnity)
            {
                DetectAllUnityInstallations();
            }
            
            if (!string.IsNullOrEmpty(customUnityPath))
            {
                AddCustomUnityInstallation();
            }
            
            SelectBestUnityInstallation();
            
            UnityEngine.Debug.Log($"✅ Unity Launcher ready - {detectedInstallations.Count} installations found");
        }
        
        private void DetectAllUnityInstallations()
        {
            UnityEngine.Debug.Log("🔍 Scanning for Unity installations...");
            
            // Unity Hub installations (primary method)
            DetectUnityHubInstallations();
            
            // Manual installations (fallback)
            DetectManualInstallations();
            
            // macOS installations (if applicable)
            DetectMacOSInstallations();
            
            // Linux installations (if applicable)
            DetectLinuxInstallations();
            
            UnityEngine.Debug.Log($"📊 Detection complete: {detectedInstallations.Count} Unity installations found");
        }
        
        private void DetectUnityHubInstallations()
        {
            string hubPath = "";
            
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            hubPath = @"C:\Program Files\Unity\Hub\Editor";
            #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            hubPath = "/Applications/Unity/Hub/Editor";
            #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            hubPath = "~/Unity/Hub/Editor";
            #endif
            
            if (Directory.Exists(hubPath))
            {
                var versionDirs = Directory.GetDirectories(hubPath);
                
                foreach (string versionDir in versionDirs)
                {
                    string version = Path.GetFileName(versionDir);
                    string editorPath = GetUnityEditorPath(versionDir);
                    
                    if (File.Exists(editorPath))
                    {
                        var installation = new UnityInstallation
                        {
                            version = version,
                            path = versionDir,
                            editorPath = editorPath,
                            isLTS = version.Contains("f") && IsLTSVersion(version),
                            installDate = Directory.GetCreationTime(versionDir)
                        };
                        
                        detectedInstallations.Add(installation);
                        UnityEngine.Debug.Log($"🎮 Found Unity Hub installation: {installation}");
                    }
                }
            }
        }
        
        private void DetectManualInstallations()
        {
            string[] manualPaths = {
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                @"C:\Program Files\Unity\Editor\Unity.exe",
                @"C:\Program Files (x86)\Unity\Editor\Unity.exe",
                @"C:\Unity\Editor\Unity.exe"
                #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
                #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                "/opt/unity/Editor/Unity",
                "~/Unity/Editor/Unity"
                #endif
            };
            
            foreach (string path in manualPaths)
            {
                if (File.Exists(path))
                {
                    var installation = new UnityInstallation
                    {
                        version = "Manual Installation",
                        path = Path.GetDirectoryName(path),
                        editorPath = path,
                        isLTS = false,
                        installDate = File.GetCreationTime(path)
                    };
                    
                    detectedInstallations.Add(installation);
                    UnityEngine.Debug.Log($"🎮 Found manual Unity installation: {installation}");
                }
            }
        }
        
        private void DetectMacOSInstallations()
        {
            #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // Add macOS-specific detection logic
            UnityEngine.Debug.Log("🍎 Scanning macOS Unity installations...");
            #endif
        }
        
        private void DetectLinuxInstallations()
        {
            #if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            // Add Linux-specific detection logic
            UnityEngine.Debug.Log("🐧 Scanning Linux Unity installations...");
            #endif
        }
        
        private string GetUnityEditorPath(string unityInstallDir)
        {
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return Path.Combine(unityInstallDir, "Editor", "Unity.exe");
            #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return Path.Combine(unityInstallDir, "Unity.app", "Contents", "MacOS", "Unity");
            #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            return Path.Combine(unityInstallDir, "Editor", "Unity");
            #else
            return "";
            #endif
        }
        
        private bool IsLTSVersion(string version)
        {
            // LTS versions typically end with 'f' and have specific patterns
            return version.Contains("f") && 
                   (version.StartsWith("2022.3") || 
                    version.StartsWith("2023.3") || 
                    version.StartsWith("2024.3"));
        }
        
        private void AddCustomUnityInstallation()
        {
            if (File.Exists(customUnityPath))
            {
                var installation = new UnityInstallation
                {
                    version = "Custom Path",
                    path = Path.GetDirectoryName(customUnityPath),
                    editorPath = customUnityPath,
                    isLTS = false,
                    installDate = File.GetCreationTime(customUnityPath)
                };
                
                detectedInstallations.Add(installation);
                UnityEngine.Debug.Log($"🎮 Added custom Unity installation: {installation}");
            }
        }
        
        private void SelectBestUnityInstallation()
        {
            if (detectedInstallations.Count == 0)
            {
                UnityEngine.Debug.LogError("❌ No Unity installations found!");
                return;
            }
            
            // Prioritize LTS versions if preferLatestVersion is true
            if (preferLatestVersion)
            {
                var ltsInstallations = detectedInstallations.FindAll(i => i.isLTS);
                if (ltsInstallations.Count > 0)
                {
                    selectedInstallation = ltsInstallations[ltsInstallations.Count - 1];
                }
                else
                {
                    selectedInstallation = detectedInstallations[detectedInstallations.Count - 1];
                }
            }
            else
            {
                selectedInstallation = detectedInstallations[0];
            }
            
            UnityEngine.Debug.Log($"🎯 Selected Unity installation: {selectedInstallation}");
        }
        
        public void LaunchUnityProject()
        {
            if (selectedInstallation == null)
            {
                UnityEngine.Debug.LogError("❌ No Unity installation selected!");
                return;
            }
            
            LaunchUnityProject(selectedInstallation);
        }
        
        public void LaunchUnityProject(UnityInstallation installation)
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = installation.editorPath,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            // Build arguments
            var args = new List<string>
            {
                "-projectPath", $"\"{projectPath}\""
            };
            
            if (launchInBatchMode)
            {
                args.Add("-batchmode");
                args.Add("-nographics");
            }
            
            if (enableUnityLog)
            {
                string logPath = Path.Combine(projectPath, "Logs", "unity_editor.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                args.Add("-logFile");
                args.Add($"\"{logPath}\"");
            }
            
            // Add additional arguments
            foreach (string arg in additionalArguments)
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    args.Add(arg);
                }
            }
            
            startInfo.Arguments = string.Join(" ", args);
            
            try
            {
                UnityEngine.Debug.Log($"🚀 Launching Unity: {installation.editorPath}");
                UnityEngine.Debug.Log($"📝 Arguments: {startInfo.Arguments}");
                
                Process unityProcess = Process.Start(startInfo);
                
                if (unityProcess != null)
                {
                    UnityEngine.Debug.Log("✅ Unity launched successfully!");
                    
                    // Emit launch event for CX Language integration
                    if (enableCxIntegration)
                    {
                        var bridge = GetComponent<CxLanguageBridge>();
                        if (bridge != null)
                        {
                            bridge.EmitEvent("unity.project.launched", 
                                $"{{ \"version\": \"{installation.version}\", \"path\": \"{projectPath}\", \"processId\": {unityProcess.Id} }}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to launch Unity: {ex.Message}");
            }
        }
        
        // Public methods for Unity Inspector
        [ContextMenu("Detect Unity Installations")]
        public void InspectorDetectInstallations()
        {
            detectedInstallations.Clear();
            DetectAllUnityInstallations();
            SelectBestUnityInstallation();
        }
        
        [ContextMenu("Launch Unity Project")]
        public void InspectorLaunchProject()
        {
            LaunchUnityProject();
        }
        
        [ContextMenu("List All Installations")]
        public void InspectorListInstallations()
        {
            UnityEngine.Debug.Log($"📋 Unity Installations ({detectedInstallations.Count}):");
            for (int i = 0; i < detectedInstallations.Count; i++)
            {
                string marker = detectedInstallations[i] == selectedInstallation ? "👉 " : "   ";
                UnityEngine.Debug.Log($"{marker}{i + 1}. {detectedInstallations[i]}");
            }
        }
        
        // Public properties for external access
        public List<UnityInstallation> GetDetectedInstallations()
        {
            return new List<UnityInstallation>(detectedInstallations);
        }
        
        public UnityInstallation GetSelectedInstallation()
        {
            return selectedInstallation;
        }
        
        public void SetSelectedInstallation(int index)
        {
            if (index >= 0 && index < detectedInstallations.Count)
            {
                selectedInstallation = detectedInstallations[index];
                UnityEngine.Debug.Log($"🎯 Unity installation changed to: {selectedInstallation}");
            }
        }
    }
}
