using Microsoft.Extensions.AI;
using System;
using System.Threading.Tasks;

namespace TempChatTest 
{
    public class TestCode
    {
        public async Task TestMethods(IChatClient chatClient)
        {
            // Test what methods are available
            var messages = new[] { new ChatMessage(ChatRole.User, "test") };
            
            // Try different method names
            // var result = await chatClient.CompleteAsync(messages);
            // var result = await chatClient.CompleteChatAsync(messages);
            // var result = await chatClient.SendAsync(messages);
        }
    }
}
