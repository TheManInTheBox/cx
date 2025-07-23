using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace CxLanguage.StandardLibrary.AI.Realtime;

/// <summary>
/// Interface for real-time chat operations
/// Simplified interface compatible with Microsoft.Extensions.AI
/// </summary>
public interface IRealtimeChat
{
    /// <summary>
    /// Stream chat responses in real-time
    /// </summary>
    IAsyncEnumerable<ChatMessage> StreamAsync(IList<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message and get a streaming response
    /// </summary>
    IAsyncEnumerable<ChatMessage> StreamAsync(string message, CancellationToken cancellationToken = default);
}
