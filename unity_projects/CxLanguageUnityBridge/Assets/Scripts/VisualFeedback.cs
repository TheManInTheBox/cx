using UnityEngine;
using System.Collections;

namespace CxLanguage.Unity
{
    /// <summary>
    /// Visual Feedback System for Maya Nakamura's Unity Bridge
    /// Provides real-time visual responses to voice and consciousness processing
    /// </summary>
    public class VisualFeedback : MonoBehaviour
    {
        [Header("Visual Feedback Settings")]
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private Color voiceColor = Color.green;
        [SerializeField] private Color consciousnessColor = Color.blue;
        [SerializeField] private float animationDuration = 2f;
        [SerializeField] private bool enableParticleEffects = true;
        
        private ParticleSystem particles;
        private Renderer visualRenderer;
        
        void Start()
        {
            SetupVisualComponents();
            Debug.Log("👀 Visual feedback system initialized");
        }
        
        private void SetupVisualComponents()
        {
            // Create visual elements if they don't exist
            if (!visualPrefab)
            {
                CreateDefaultVisual();
            }
            
            // Setup particle system
            if (enableParticleEffects)
            {
                particles = GetComponent<ParticleSystem>();
                if (!particles)
                {
                    particles = gameObject.AddComponent<ParticleSystem>();
                    ConfigureParticleSystem();
                }
            }
        }
        
        private void CreateDefaultVisual()
        {
            // Create a simple cube for visual feedback
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 2f;
            
            visualRenderer = visual.GetComponent<Renderer>();
            visualRenderer.material = new Material(Shader.Find("Standard"));
            
            Debug.Log("✨ Default visual component created");
        }
        
        private void ConfigureParticleSystem()
        {
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 5f;
            main.maxParticles = 100;
            
            var emission = particles.emission;
            emission.rateOverTime = 20f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
            
            Debug.Log("✨ Particle system configured");
        }
        
        public void ShowVoiceVisualization()
        {
            Debug.Log("🎨 Showing voice visualization");
            StartCoroutine(AnimateVisual(voiceColor, "voice"));
            
            if (particles)
            {
                var main = particles.main;
                main.startColor = voiceColor;
                particles.Play();
            }
        }
        
        public void ShowConsciousnessVisualization()
        {
            Debug.Log("🧠 Showing consciousness visualization");
            StartCoroutine(AnimateVisual(consciousnessColor, "consciousness"));
            
            if (particles)
            {
                var main = particles.main;
                main.startColor = consciousnessColor;
                particles.Play();
            }
        }
        
        public void ShowCustomVisualization(string data)
        {
            Debug.Log($"✨ Custom visualization: {data}");
            
            // Parse custom visualization data
            Color customColor = Color.white;
            if (data.Contains("green")) customColor = Color.green;
            else if (data.Contains("blue")) customColor = Color.blue;
            else if (data.Contains("red")) customColor = Color.red;
            
            StartCoroutine(AnimateVisual(customColor, "custom"));
        }
        
        private IEnumerator AnimateVisual(Color color, string type)
        {
            if (!visualRenderer) yield break;
            
            // Set initial color
            visualRenderer.material.color = color;
            
            // Animate scale and glow
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.5f;
            
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                
                // Pulse animation
                float pulseScale = 1f + Mathf.Sin(progress * Mathf.PI * 4f) * 0.2f;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, pulseScale);
                
                // Glow effect
                Color glowColor = color;
                glowColor.a = Mathf.Lerp(1f, 0.3f, progress);
                visualRenderer.material.color = glowColor;
                
                yield return null;
            }
            
            // Reset to original state
            transform.localScale = originalScale;
            visualRenderer.material.color = Color.white;
            
            Debug.Log($"✅ {type} visualization complete");
        }
        
        public void ShowProcessingIndicator(bool active)
        {
            if (particles)
            {
                if (active)
                {
                    particles.Play();
                }
                else
                {
                    particles.Stop();
                }
            }
            
            Debug.Log($"⚡ Processing indicator: {(active ? "ON" : "OFF")}");
        }
        
        // Unity event handlers
        public void OnVoiceStart()
        {
            ShowVoiceVisualization();
        }
        
        public void OnVoiceComplete()
        {
            ShowProcessingIndicator(false);
        }
        
        public void OnConsciousnessUpdate()
        {
            ShowConsciousnessVisualization();
        }
    }
}
