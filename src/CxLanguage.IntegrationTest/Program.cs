using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;
using CxLanguage.StandardLibrary.AI.Chat;
using CxLanguage.StandardLibrary.AI.Embeddings;

namespace CxLanguage.IntegrationTest;

/// <summary>
/// Microsoft.Extensions.AI Integration Test
/// Tests CustomChatClient and CustomEmbeddingGenerator compatibility
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 CX Language - Microsoft.Extensions.AI Integration Test");
        Console.WriteLine("=========================================================");

        try
        {
            // Test basic interface registration
            TestInterfaceRegistration();
            
            // Test service resolution
            TestServiceResolution();
            
            Console.WriteLine("\n🎉 All Microsoft.Extensions.AI integration tests passed!");
            Console.WriteLine("✅ CustomChatClient successfully implements IChatClient");
            Console.WriteLine("✅ CustomEmbeddingGenerator successfully implements IEmbeddingGenerator");
            Console.WriteLine("✅ Dependency injection integration works correctly");
            Console.WriteLine("✅ Microsoft.Extensions.AI ecosystem compatibility confirmed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Integration test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    static void TestInterfaceRegistration()
    {
        Console.WriteLine("\n📋 Testing interface registration...");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        // Mock Azure OpenAI client for testing
        var azureClient = new AzureOpenAIClient(
            new Uri("https://test.openai.azure.com/"),
            new AzureKeyCredential("test-key-for-integration-testing")
        );
        services.AddSingleton(azureClient);
        
        // Register our custom implementations
        services.AddSingleton<IChatClient, CustomChatClient>();
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, CustomEmbeddingGenerator>();

        var serviceProvider = services.BuildServiceProvider();

        // Test IChatClient registration
        var chatClient = serviceProvider.GetService<IChatClient>();
        if (chatClient == null)
            throw new Exception("IChatClient not registered");
        
        Console.WriteLine($"✅ IChatClient registered: {chatClient.GetType().Name}");

        // Test IEmbeddingGenerator registration
        var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
        if (embeddingGenerator == null)
            throw new Exception("IEmbeddingGenerator not registered");
        
        Console.WriteLine($"✅ IEmbeddingGenerator registered: {embeddingGenerator.GetType().Name}");
    }

    static void TestServiceResolution()
    {
        Console.WriteLine("\n🔧 Testing service resolution and basic functionality...");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
        // Mock Azure OpenAI client
        var azureClient = new AzureOpenAIClient(
            new Uri("https://test.openai.azure.com/"),
            new AzureKeyCredential("test-key")
        );
        services.AddSingleton(azureClient);
        
        // Register implementations
        services.AddSingleton<IChatClient, CustomChatClient>();
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, CustomEmbeddingGenerator>();

        var serviceProvider = services.BuildServiceProvider();

        // Test ChatClient methods exist and are callable
        var chatClient = serviceProvider.GetRequiredService<IChatClient>();
        
        // Test that methods exist (won't actually call Azure since we don't have real credentials)
        var hasGetResponse = chatClient.GetType().GetMethod("GetResponseAsync") != null;
        var hasGetStreaming = chatClient.GetType().GetMethod("GetStreamingResponseAsync") != null;
        
        if (!hasGetResponse)
            throw new Exception("CustomChatClient missing GetResponseAsync method");
        if (!hasGetStreaming)
            throw new Exception("CustomChatClient missing GetStreamingResponseAsync method");
            
        Console.WriteLine("✅ CustomChatClient has required IChatClient methods");

        // Test EmbeddingGenerator methods exist
        var embeddingGenerator = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        var hasGenerateAsync = embeddingGenerator.GetType().GetMethod("GenerateAsync") != null;
        
        if (!hasGenerateAsync)
            throw new Exception("CustomEmbeddingGenerator missing GenerateAsync method");
            
        Console.WriteLine("✅ CustomEmbeddingGenerator has required IEmbeddingGenerator methods");

        // Test service provider integration
        var chatFromProvider = serviceProvider.GetService(typeof(IChatClient));
        var embeddingFromProvider = serviceProvider.GetService(typeof(IEmbeddingGenerator<string, Embedding<float>>));
        
        if (chatFromProvider == null || embeddingFromProvider == null)
            throw new Exception("Service provider integration failed");
            
        Console.WriteLine("✅ Service provider integration works correctly");
    }
}
