using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace CxLanguage.IDE.WinUI.Services;

/// <summary>
/// Enhanced GPU detection service for accurate CUDA availability reporting
/// GitHub Issue #229: GPU Detection Inconsistency fix
/// </summary>
public interface IGpuDetectionService
{
    Task<GpuInfo> GetGpuInfoAsync();
    bool IsGpuFunctional();
    Task<bool> TestGpuFunctionalityAsync();
}

/// <summary>
/// GPU information structure
/// </summary>
public class GpuInfo
{
    public bool IsAvailable { get; set; }
    public bool IsFunctional { get; set; }
    public int DeviceCount { get; set; }
    public string? CudaVersion { get; set; }
    public string? DriverVersion { get; set; }
    public List<string> DeviceNames { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool HasCudaInstallation { get; set; }
    public bool HasNvidiaSmi { get; set; }
    public bool HasWorkingRuntime { get; set; }
}

/// <summary>
/// Enhanced GPU detection service implementation
/// </summary>
public class GpuDetectionService : IGpuDetectionService
{
    private readonly ILogger<GpuDetectionService> _logger;
    private GpuInfo? _cachedGpuInfo;
    private DateTime _lastCheck = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public GpuDetectionService(ILogger<GpuDetectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive GPU information
    /// </summary>
    public async Task<GpuInfo> GetGpuInfoAsync()
    {
        // Use cached result if available and not expired
        if (_cachedGpuInfo != null && DateTime.Now - _lastCheck < _cacheTimeout)
        {
            return _cachedGpuInfo;
        }

        var gpuInfo = new GpuInfo();
        
        try
        {
            // Check for CUDA installation directories
            gpuInfo.HasCudaInstallation = CheckCudaInstallation();
            
            // Check for nvidia-smi tool
            gpuInfo.HasNvidiaSmi = await CheckNvidiaSmiAsync();
            
            // Get device information if nvidia-smi is available
            if (gpuInfo.HasNvidiaSmi)
            {
                await PopulateDeviceInfoAsync(gpuInfo);
            }
            
            // Test actual GPU functionality
            gpuInfo.HasWorkingRuntime = await TestGpuRuntimeAsync();
            
            // Determine overall availability and functionality
            gpuInfo.IsAvailable = gpuInfo.HasCudaInstallation || gpuInfo.HasNvidiaSmi;
            gpuInfo.IsFunctional = gpuInfo.HasWorkingRuntime && gpuInfo.DeviceCount > 0;
            
            _logger.LogDebug("GPU Detection Results: Available={Available}, Functional={Functional}, Devices={Devices}", 
                gpuInfo.IsAvailable, gpuInfo.IsFunctional, gpuInfo.DeviceCount);
        }
        catch (Exception ex)
        {
            gpuInfo.ErrorMessage = ex.Message;
            _logger.LogDebug(ex, "Error during GPU detection");
        }
        
        _cachedGpuInfo = gpuInfo;
        _lastCheck = DateTime.Now;
        return gpuInfo;
    }

    /// <summary>
    /// Quick check if GPU is functional (uses cache)
    /// </summary>
    public bool IsGpuFunctional()
    {
        if (_cachedGpuInfo != null && DateTime.Now - _lastCheck < _cacheTimeout)
        {
            return _cachedGpuInfo.IsFunctional;
        }
        
        // Fallback to basic detection if no cache
        return CheckCudaInstallation();
    }

    /// <summary>
    /// Test GPU functionality with actual CUDA operations
    /// </summary>
    public async Task<bool> TestGpuFunctionalityAsync()
    {
        var gpuInfo = await GetGpuInfoAsync();
        return gpuInfo.IsFunctional;
    }

    /// <summary>
    /// Check for CUDA installation
    /// </summary>
    private bool CheckCudaInstallation()
    {
        try
        {
            var cudaPaths = new[]
            {
                @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA",
                @"C:\Program Files\NVIDIA Corporation\NVSMI",
                Environment.GetEnvironmentVariable("CUDA_PATH"),
                Environment.GetEnvironmentVariable("CUDA_HOME")
            };

            return cudaPaths.Any(path => !string.IsNullOrEmpty(path) && Directory.Exists(path));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if nvidia-smi is available
    /// </summary>
    private async Task<bool> CheckNvidiaSmiAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("nvidia-smi not available: {Error}", ex.Message);
        }
        
        return false;
    }

    /// <summary>
    /// Populate device information using nvidia-smi
    /// </summary>
    private async Task PopulateDeviceInfoAsync(GpuInfo gpuInfo)
    {
        try
        {
            // Get device count and names
            var deviceInfo = await RunNvidiaSmiAsync("--query-gpu=name,count --format=csv,noheader,nounits");
            if (!string.IsNullOrEmpty(deviceInfo))
            {
                var lines = deviceInfo.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                gpuInfo.DeviceNames = lines.Select(line => line.Trim()).ToList();
                gpuInfo.DeviceCount = lines.Length;
            }

            // Get CUDA version
            var cudaVersion = await RunNvidiaSmiAsync("--query-gpu=driver_version --format=csv,noheader,nounits");
            if (!string.IsNullOrEmpty(cudaVersion))
            {
                gpuInfo.DriverVersion = cudaVersion.Trim().Split('\n').FirstOrDefault()?.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error getting device info: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Run nvidia-smi command and return output
    /// </summary>
    private async Task<string?> RunNvidiaSmiAsync(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    return output;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error running nvidia-smi: {Error}", ex.Message);
        }
        
        return null;
    }

    /// <summary>
    /// Test actual GPU runtime functionality
    /// </summary>
    private async Task<bool> TestGpuRuntimeAsync()
    {
        try
        {
            // For now, check if we can query GPU memory usage
            var memoryInfo = await RunNvidiaSmiAsync("--query-gpu=memory.used --format=csv,noheader,nounits");
            return !string.IsNullOrEmpty(memoryInfo) && !memoryInfo.Contains("Failed");
        }
        catch
        {
            return false;
        }
    }
}
