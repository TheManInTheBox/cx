namespace CxLanguage.StandardLibrary.Services;

/// <summary>
/// Interface for real-time voice input capture and processing
/// Provides microphone audio capture for voice-controlled programming
/// </summary>
public interface IVoiceInputService : IDisposable
{
    /// <summary>
    /// Start continuous microphone capture
    /// Audio events will be emitted as 'voice.input.captured'
    /// </summary>
    Task StartListeningAsync();
    
    /// <summary>
    /// Stop microphone capture
    /// </summary>
    Task StopListeningAsync();
    
    /// <summary>
    /// Check if currently listening for voice input
    /// </summary>
    bool IsListening { get; }
    
    /// <summary>
    /// Get available audio input devices
    /// </summary>
    Task<string[]> GetAvailableDevicesAsync();
    
    /// <summary>
    /// Set the audio input device to use
    /// </summary>
    Task SetInputDeviceAsync(int deviceIndex);
}
