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
        private readonly IRealtimeChat _realtimeChat;

        public ModernRealtimeService(ILogger<ModernRealtimeService> logger, IRealtimeChat realtimeChat)
        {
            _logger = logger;
            _realtimeChat = realtimeChat;
        }

        public async Task HandleStreamingResponseAsync(string prompt)
        {
            var history = new ChatHistory();
            history.AddUserMessage(prompt);

            _logger.LogInformation("Starting real-time chat stream...");

            await foreach (var update in _realtimeChat.Stream(history))
            {
                if (update.TextUpdate != null)
                {
                    Console.Write(update.TextUpdate);
                }
            }

            _logger.LogInformation("\nReal-time chat stream finished.");
        }
    }
}
