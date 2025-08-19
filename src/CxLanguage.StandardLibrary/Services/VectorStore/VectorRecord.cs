using System;
using System.Collections.Generic;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Represents a single record in the vector store, containing the vector and associated metadata.
    /// </summary>
    public class VectorRecord
    {
        /// <summary>
        /// A unique identifier for the vector record.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The vector embedding.
        /// </summary>
        public float[] Vector { get; set; } = Array.Empty<float>();

        /// <summary>
        /// The original content that was vectorized.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Metadata associated with the vector record.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The timestamp when the record was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}

