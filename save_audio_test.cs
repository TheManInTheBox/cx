// Save TTS audio to file so you can hear it!
using System.IO;

// Create a simple test to save TTS audio
using (var audioService = new CxLanguage.StandardLibrary.AI.TextToSpeech.TextToSpeechService())
{
    var result = await audioService.SynthesizeAsync("Congratulations! Phase 4 multi-service configuration is complete and working perfectly with the new model-specific API versions!");
    
    if (result.IsSuccess)
    {
        File.WriteAllBytes("phase4_celebration.wav", result.AudioData);
        Console.WriteLine($"Audio saved! File size: {result.AudioData.Length} bytes");
        Console.WriteLine("You can now play phase4_celebration.wav to hear it speak!");
    }
}
