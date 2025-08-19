using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;

namespace CxLanguage.StandardLibrary.AI.Realtime
{
    public class ModernRealtimeService
    {
        private readonly ILogger<ModernRealtimeService> _logger;
        private readonly IChatClient _chatClient;

        public ModernRealtimeService(ILogger<ModernRealtimeService> logger, IChatClient chatClient)
        {
            _logger = logger;
            _chatClient = chatClient;
        }

        public async Task HandleStreamingResponseAsync(string prompt)
        {
            var messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, prompt) };

            _logger.LogInformation("Starting streaming chat response...");

            await foreach (var update in _chatClient.GetStreamingResponseAsync(messages))
            {
                if (update?.Text != null)
                {
                    Console.Write(update.Text);
                }
            }

            _logger.LogInformation("\nStreaming chat response finished.");
        }
    }
}

