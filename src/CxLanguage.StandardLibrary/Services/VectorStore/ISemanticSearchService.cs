using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CxLanguage.StandardLibrary.Services.VectorStore
{
    /// <summary>
    /// Dr. Marcus "MemoryLayer" Sterling - Memory Layer Vector Index Architect
    /// Issue #258: Semantic Search Service Interface
    /// 
    /// Defines the contract for consciousness-aware semantic search operations
    /// with natural language query processing and intelligent result ranking.
    /// </summary>
    public interface ISemanticSearchService
    {
        /// <summary>
        /// Performs semantic search using natural language queries.
        /// Core functionality for Issue #258 with sub-200ms performance target.
        /// </summary>
        /// <param name="query">Natural language search query</param>
        /// <param name="options">Search options including topK, threshold, context</param>
        /// <returns>Ranked semantic search results</returns>
        Task<SemanticSearchResult> SearchAsync(string query, SemanticSearchOptions? options = null);

        /// <summary>
        /// Agent-specific consciousness-aware semantic search.
        /// Includes agent context and consciousness state in search processing.
        /// </summary>
        /// <param name="query">Natural language search query</param>
        /// <param name="agentContext">Agent consciousness context and state</param>
        /// <param name="options">Search options</param>
        /// <returns>Context-aware semantic search results</returns>
        Task<SemanticSearchResult> SearchWithContextAsync(string query, AgentContext agentContext, SemanticSearchOptions? options = null);

        /// <summary>
        /// Generate text snippets from search results for human-readable responses.
        /// Creates meaningful text excerpts from document chunks.
        /// </summary>
        /// <param name="results">Search results to process</param>
        /// <param name="query">Original search query for context</param>
        /// <param name="snippetLength">Target length for generated snippets</param>
        /// <returns>Generated text snippets with relevance scores</returns>
        Task<IEnumerable<SearchSnippet>> GenerateSnippetsAsync(IEnumerable<VectorRecord> results, string query, int snippetLength = 200);

        /// <summary>
        /// Get semantic search performance metrics for consciousness monitoring.
        /// </summary>
        /// <returns>Performance metrics and search statistics</returns>
        Task<Dictionary<string, object>> GetSearchMetricsAsync();
    }

    /// <summary>
    /// Comprehensive semantic search result with ranking and metadata.
    /// </summary>
    public class SemanticSearchResult
    {
        /// <summary>
        /// Ranked search results with similarity scores
        /// </summary>
        public IEnumerable<RankedVectorRecord> Results { get; set; } = new List<RankedVectorRecord>();

        /// <summary>
        /// Original search query
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Total processing time in milliseconds
        /// </summary>
        public double ProcessingTimeMs { get; set; }

        /// <summary>
        /// Total number of records searched
        /// </summary>
        public int TotalRecordsSearched { get; set; }

        /// <summary>
        /// Number of results returned
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        /// Search metadata and consciousness context
        /// </summary>
        public Dictionary<string, object> SearchMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Generated human-readable response summary
        /// </summary>
        public string ResponseSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vector record with similarity ranking and additional search metadata.
    /// </summary>
    public class RankedVectorRecord
    {
        /// <summary>
        /// Original vector record
        /// </summary>
        public VectorRecord Record { get; set; } = new VectorRecord();

        /// <summary>
        /// Similarity score (0.0 to 1.0)
        /// </summary>
        public double SimilarityScore { get; set; }

        /// <summary>
        /// Relevance rank (1 = most relevant)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Generated snippet from content
        /// </summary>
        public string Snippet { get; set; } = string.Empty;

        /// <summary>
        /// Search-specific metadata
        /// </summary>
        public Dictionary<string, object> SearchMetadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Options for configuring semantic search behavior.
    /// </summary>
    public class SemanticSearchOptions
    {
        /// <summary>
        /// Maximum number of results to return (default: 5)
        /// </summary>
        public int TopK { get; set; } = 5;

        /// <summary>
        /// Minimum similarity threshold for results (default: 0.3)
        /// </summary>
        public double SimilarityThreshold { get; set; } = 0.3;

        /// <summary>
        /// Include metadata in results (default: true)
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Generate snippets for results (default: true)
        /// </summary>
        public bool GenerateSnippets { get; set; } = true;

        /// <summary>
        /// Maximum snippet length in characters (default: 200)
        /// </summary>
        public int SnippetLength { get; set; } = 200;

        /// <summary>
        /// Collection names to search (null = search all)
        /// </summary>
        public IEnumerable<string>? Collections { get; set; }

        /// <summary>
        /// Additional search filters
        /// </summary>
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Agent consciousness context for awareness-based search.
    /// </summary>
    public class AgentContext
    {
        /// <summary>
        /// Agent identifier
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Current consciousness state
        /// </summary>
        public string ConsciousnessState { get; set; } = string.Empty;

        /// <summary>
        /// Agent's current objectives or focus
        /// </summary>
        public IEnumerable<string> CurrentObjectives { get; set; } = new List<string>();

        /// <summary>
        /// Agent's memory context
        /// </summary>
        public Dictionary<string, object> MemoryContext { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Agent-specific metadata
        /// </summary>
        public Dictionary<string, object> AgentMetadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Generated text snippet with relevance information.
    /// </summary>
    public class SearchSnippet
    {
        /// <summary>
        /// Generated snippet text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Source vector record ID
        /// </summary>
        public string SourceRecordId { get; set; } = string.Empty;

        /// <summary>
        /// Relevance score for this snippet
        /// </summary>
        public double RelevanceScore { get; set; }

        /// <summary>
        /// Highlighted keywords or phrases
        /// </summary>
        public IEnumerable<string> HighlightedTerms { get; set; } = new List<string>();

        /// <summary>
        /// Character position in original content
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// Length of snippet in original content
        /// </summary>
        public int Length { get; set; }
    }
}
