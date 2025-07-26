using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Defines the contract for a consciousness-aware vector store service.
    /// </summary>
    public interface IVectorStoreService
    {
        /// <summary>
        /// Adds or updates a vector record in the store.
        /// </summary>
        /// <param name="record">The vector record to add or update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(VectorRecord record);

        /// <summary>
        /// Searches for similar vectors in the store.
        /// </summary>
        /// <param name="vector">The vector to search for.</param>
        /// <param name="topK">The number of similar records to return.</param>
        /// <returns>A collection of similar vector records.</returns>
        Task<IEnumerable<VectorRecord>> SearchAsync(float[] vector, int topK);

        /// <summary>
        /// Retrieves a vector record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve.</param>
        /// <returns>The vector record, or null if not found.</returns>
        Task<VectorRecord?> GetAsync(string id);
    }
}
