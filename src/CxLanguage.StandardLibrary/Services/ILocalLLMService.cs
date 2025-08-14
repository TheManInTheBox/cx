using System;
using System.Threading;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services
{
    /// <summary>
    /// Interface for local LLM (Large Language Model) services
    /// </summary>
    public interface ILocalLLMService : IDisposable
    {
        /// <summary>
        /// Checks if GPU is available for acceleration
        /// </summary>
        /// <returns>True if GPU is available, false otherwise</returns>
        bool IsGpuAvailable();
        
        /// <summary>
        /// Loads a model from the specified path
        /// </summary>
        /// <param name="modelPath">Path to the model file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task LoadModelAsync(string modelPath, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generates text based on the provided prompt
        /// </summary>
        /// <param name="prompt">Input prompt for text generation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated text</returns>
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
