using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.LocalLLM
{
    /// <summary>
    /// Consciousness-aware local LLM service interface for CX Language
    /// Provides zero-cloud dependency AI capabilities
    /// </summary>
    public interface ILocalLLMService
    {
        /// <summary>
        /// Initializes the consciousness-aware LLM service
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// Loads a specific LLM model for consciousness-aware processing
        /// </summary>
        Task<bool> LoadModelAsync(string modelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unloads the currently loaded model to free memory
        /// </summary>
        Task UnloadModelAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates consciousness-aware text response
        /// </summary>
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams consciousness-aware text generation in real-time
        /// </summary>
        IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a model is currently loaded and ready for inference
        /// </summary>
        bool IsModelLoaded { get; }

        /// <summary>
        /// Gets information about the currently loaded model
        /// </summary>
        ModelInfo? ModelInfo { get; }
    }

    /// <summary>
    /// Information about a loaded LLM model
    /// </summary>
    public record ModelInfo(
        string Name,
        string Version,
        long SizeBytes,
        string Architecture,
        DateTime LoadedAt,
        string? Path = null);
}
