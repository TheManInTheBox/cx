using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services.Document
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Defines the contract for consciousness-aware document ingestion service.
    /// Processes documents into searchable vector chunks for the local vector database.
    /// </summary>
    public interface IDocumentIngestionService
    {
        /// <summary>
        /// Processes a single document into vector chunks.
        /// Supported formats: TXT, MD, JSON
        /// </summary>
        /// <param name="filePath">Path to the document to process</param>
        /// <param name="metadata">Optional metadata to associate with all chunks</param>
        /// <returns>Number of chunks created from the document</returns>
        Task<int> IngestDocumentAsync(string filePath, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Processes multiple documents into vector chunks.
        /// </summary>
        /// <param name="filePaths">Collection of file paths to process</param>
        /// <param name="metadata">Optional metadata to associate with all chunks</param>
        /// <returns>Total number of chunks created from all documents</returns>
        Task<int> IngestDocumentBatchAsync(IEnumerable<string> filePaths, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Processes a directory recursively for supported document formats.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to process</param>
        /// <param name="recursive">Whether to process subdirectories</param>
        /// <param name="metadata">Optional metadata to associate with all chunks</param>
        /// <returns>Total number of chunks created from all documents</returns>
        Task<int> IngestDirectoryAsync(string directoryPath, bool recursive = true, Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Gets the supported file formats for document ingestion.
        /// </summary>
        /// <returns>Collection of supported file extensions</returns>
        IEnumerable<string> GetSupportedFormats();

        /// <summary>
        /// Validates if a file format is supported for ingestion.
        /// </summary>
        /// <param name="filePath">Path to the file to validate</param>
        /// <returns>True if the file format is supported</returns>
        bool IsFormatSupported(string filePath);
    }
}
