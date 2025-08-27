namespace CxLanguage.StandardLibrary.Core;

// NOTE: ITextGeneration, IChatCompletion, and IRealtimeAPI removed due to redundancy
// These capabilities are now provided by default via AiServiceBase inheritance:
// - this.GenerateAsync() (basic text generation)
// - this.ChatAsync() (basic chat completion)  
// - this.CommunicateAsync(), this.ConnectAsync() (realtime capabilities)

/// <summary>
/// Interface for classes that need text-to-speech capabilities
/// When implemented, provides access to this.SpeakAsync() method
/// </summary>
public interface ITextToSpeech
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need text embedding capabilities
/// When implemented, provides access to this.EmbedAsync() method
/// </summary>
public interface ITextEmbeddings
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need image generation capabilities
/// When implemented, provides access to this.CreateImageAsync() method
/// </summary>
public interface IImageGeneration
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need audio-to-text capabilities
/// When implemented, provides access to this.TranscribeAsync() method
/// </summary>
public interface IAudioToText
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need text-to-audio capabilities
/// When implemented, provides access to this.CreateAudioAsync() method
/// </summary>
public interface ITextToAudio
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need image analysis capabilities
/// When implemented, provides access to this.AnalyzeAsync() method
/// </summary>
public interface IImageAnalysis
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Interface for classes that need vector database capabilities
/// When implemented, provides access to this.SearchAsync() methods
/// </summary>
public interface IVectorDatabase
{
    // Marker interface - actual implementation provided by inheritance
}

/// <summary>
/// Composite interface for classes that need all specialized AI capabilities
/// Provides access to all optional AI services (excludes basic capabilities that are inherited by default)
/// </summary>
public interface IFullAICapabilities : 
    ITextToSpeech, 
    ITextEmbeddings, 
    IImageGeneration, 
    IAudioToText, 
    ITextToAudio, 
    IImageAnalysis, 
    IVectorDatabase
{
    // Composite marker interface for all specialized AI capabilities
    // Basic capabilities (GenerateAsync, ChatAsync, etc.) are inherited by default
}

