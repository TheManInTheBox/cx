using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace CxLanguage.Azure.Services;

// --- Data Structures and Event Args ---

public class MicrophoneDevice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AudioChunk
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTimeOffset Timestamp { get; set; }
    public int SampleRate { get; set; }
}

public class ProcessedAudioChunk : AudioChunk
{
    public int DurationMs { get; set; }
}

public class AudioCapturedEventArgs : EventArgs
{
    public AudioChunk? AudioChunk { get; set; }
}

public class DeviceChangedEventArgs : EventArgs
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
}

// --- Service Implementation ---

public class ModernMicrophoneService : IDisposable
{
    private readonly ILogger<NAudioMicrophoneService> _logger;
    private WaveInEvent? _waveIn;
    private int _deviceId = 0;
    private bool _disposed;

    public event EventHandler<AudioCapturedEventArgs>? AudioCaptured;
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;
    public event EventHandler<DeviceChangedEventArgs>? DeviceChanged;

    public NAudioMicrophoneService(ILogger<NAudioMicrophoneService> logger, IConfiguration configuration)
    {
        _logger = logger;
    }

    public Task StartCapturingAsync()
    {
        _logger.LogInformation("Starting microphone capture...");
        if (_waveIn != null)
        {
            _logger.LogWarning("Capture is already running.");
            return Task.CompletedTask;
        }

        try
        {
            _waveIn = new WaveInEvent
            {
                DeviceNumber = _deviceId,
                WaveFormat = new WaveFormat(16000, 16, 1), // 16kHz, 16-bit, Mono
                BufferMilliseconds = 100
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
            _waveIn.StartRecording();
            _logger.LogInformation("Microphone capture started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start microphone capture.");
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopCapturingAsync()
    {
        _logger.LogInformation("Stopping microphone capture...");
        _waveIn?.StopRecording();
        return Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded > 0)
        {
            var buffer = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, buffer, e.BytesRecorded);

            var audioChunk = new ProcessedAudioChunk
            {
                Data = buffer,
                Timestamp = DateTimeOffset.UtcNow,
                SampleRate = _waveIn?.WaveFormat.SampleRate ?? 16000,
                DurationMs = (int)((double)e.BytesRecorded / (_waveIn?.WaveFormat.AverageBytesPerSecond ?? 32000) * 1000)
            };

            AudioCaptured?.Invoke(this, new AudioCapturedEventArgs { AudioChunk = audioChunk });
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogInformation("Microphone capture stopped.");
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.Dispose();
            _waveIn = null;
        }
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "Microphone capture stopped with an error.");
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(e.Exception));
        }
    }

    public List<MicrophoneDevice> GetAvailableDevices()
    {
        var devices = new List<MicrophoneDevice>();
        for (int i = 0; i < WaveIn.DeviceCount; i++)
        {
            var caps = WaveIn.GetCapabilities(i);
            devices.Add(new MicrophoneDevice { Id = i, Name = caps.ProductName });
        }
        return devices;
    }

    public Task SetMicrophoneDeviceAsync(int deviceId)
    {
        if (deviceId >= WaveIn.DeviceCount)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId), "Invalid microphone device ID.");
        }
        
        if (_deviceId == deviceId) return Task.CompletedTask;

        _deviceId = deviceId;
        var device = GetCurrentDevice();
        if (device != null)
        {
            _logger.LogInformation($"Microphone device set to: {device.Name}");
            DeviceChanged?.Invoke(this, new DeviceChangedEventArgs { DeviceId = device.Id, DeviceName = device.Name });
        }

        if (_waveIn != null)
        {
            return Task.Run(async () =>
            {
                await StopCapturingAsync();
                await StartCapturingAsync();
            });
        }
        return Task.CompletedTask;
    }

    public MicrophoneDevice? GetCurrentDevice()
    {
        if (_deviceId < WaveIn.DeviceCount)
        {
            var caps = WaveIn.GetCapabilities(_deviceId);
            return new MicrophoneDevice { Id = _deviceId, Name = caps.ProductName };
        }
        return null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            StopCapturingAsync().Wait();
            _waveIn?.Dispose();
        }
    }
}
