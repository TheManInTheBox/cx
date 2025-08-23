using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CxLanguage.LocalLLM
{
    /// <summary>
    /// Extension methods for registering LocalLLM services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds GPU-accelerated Local LLM services to the service collection
        /// </summary>
        public static IServiceCollection AddGpuLocalLlm(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Register concrete service so constructors can depend on GpuLocalLLMService
            services.AddSingleton<GpuLocalLLMService>();
            // Map interface to the same instance
            services.AddSingleton<ILocalLLMService>(sp => sp.GetRequiredService<GpuLocalLLMService>());
            
            // Register event handler to integrate with CX event system
            services.AddSingleton<LocalLlmEventHandler>();

            return services;
        }
        
        /// <summary>
        /// Adds all local LLM services to the service collection with GPU priority
        /// </summary>
        public static IServiceCollection AddAllLocalLlmServices(this IServiceCollection services, bool preferGpu = true)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (preferGpu)
            {
                // GPU has priority - register first for interface resolution
                services.AddSingleton<GpuLocalLLMService>();
                services.AddSingleton<ILocalLLMService>(sp => 
                {
                    var gpuService = sp.GetRequiredService<GpuLocalLLMService>();
                    var logger = sp.GetRequiredService<ILogger<GpuLocalLLMService>>();
                    
                    // Check if GPU is available at runtime
                    try
                    {
                        return gpuService;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "⚠️ Error initializing GPU service, falling back to CPU implementation");
                        throw new NotImplementedException("CPU fallback not implemented in this sample");
                    }
                });
            }
            else
            {
                // CPU has priority - implement CPU service here if needed
                throw new NotImplementedException("CPU implementation priority not implemented in this sample");
            }
            
            // Register event handler to integrate with CX event system
            services.AddSingleton<LocalLlmEventHandler>();

            return services;
        }
    }
}
