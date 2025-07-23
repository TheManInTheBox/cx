using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;
using CxLanguage.StandardLibrary.AI.Chat;
using CxLanguage.StandardLibrary.AI.Embeddings;

namespace CxLanguage.StandardLibrary.Tests
{
    /// <summary>
    /// Test to verify CustomChatClient and CustomEmbeddingGenerator integration with Microsoft.Extensions.AI
    /// </summary>
    public class MicrosoftExtensionsAiIntegrationTest
    {
        public static void TestCustomChatClient()
        {
            Console.WriteLine("🚀 Testing CustomChatClient with Microsoft.Extensions.AI...");

            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            
            // Add Azure OpenAI client (mock for testing)
            var azureClient = new AzureOpenAIClient(
                new Uri("https://test.openai.azure.com/"),
                new AzureKeyCredential("test-key")
            );
            services.AddSingleton(azureClient);
            
            // Add our custom chat client implementing IChatClient
            services.AddSingleton<IChatClient, CustomChatClient>();
            
            // Add our custom embedding generator implementing IEmbeddingGenerator
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, CustomEmbeddingGenerator>();

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                // Test IChatClient interface
                var chatClient = serviceProvider.GetRequiredService<IChatClient>();
                Console.WriteLine("✅ CustomChatClient successfully registered as IChatClient");
                Console.WriteLine($"   Type: {chatClient.GetType().Name}");

                // Test IEmbeddingGenerator interface
                var embeddingGenerator = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
                Console.WriteLine("✅ CustomEmbeddingGenerator successfully registered as IEmbeddingGenerator");
                Console.WriteLine($"   Type: {embeddingGenerator.GetType().Name}");

                Console.WriteLine("🎉 Microsoft.Extensions.AI integration test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Integration test failed: {ex.Message}");
                throw;
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                TestCustomChatClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
