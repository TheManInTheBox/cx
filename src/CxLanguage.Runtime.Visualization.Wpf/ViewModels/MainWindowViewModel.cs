using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;
using CxLanguage.Runtime.Visualization.Wpf.Services;

namespace CxLanguage.Runtime.Visualization.Wpf.ViewModels;

/// <summary>
/// ViewModel for the main consciousness peering visualization window
/// Coordinates 3D visualization with real-time consciousness network data
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPeeringDataService _peeringDataService;
    private readonly IConsciousnessNetworkService _networkService;
    private readonly IVisualization3DEngine _visualization3D;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly Timer _refreshTimer;

    private bool _isLoading;
    private string _statusMessage = "Initializing...";
    private DateTime _lastUpdated = DateTime.Now;
    private int _totalPeers;
    private int _activePeers;
    private int _totalConnections;
    private double _networkDensity;

    public ObservableCollection<PeerViewModel> Peers { get; } = new();

    public MainWindowViewModel(
        IPeeringDataService peeringDataService,
        IConsciousnessNetworkService networkService,
        IVisualization3DEngine visualization3D,
        ILogger<MainWindowViewModel> logger)
    {
        _peeringDataService = peeringDataService;
        _networkService = networkService;
        _visualization3D = visualization3D;
        _logger = logger;

        RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

        // Set up auto-refresh timer (every 2 seconds)
        _refreshTimer = new Timer(async _ => await RefreshDataAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    public ICommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    public int TotalPeers
    {
        get => _totalPeers;
        set => SetProperty(ref _totalPeers, value);
    }

    public int ActivePeers
    {
        get => _activePeers;
        set => SetProperty(ref _activePeers, value);
    }

    public int TotalConnections
    {
        get => _totalConnections;
        set => SetProperty(ref _totalConnections, value);
    }

    public double NetworkDensity
    {
        get => _networkDensity;
        set => SetProperty(ref _networkDensity, value);
    }

    public void SetModels3DContainer(ModelVisual3D models3D)
    {
        _visualization3D.Models3D = models3D;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing consciousness peering visualization...");
        StatusMessage = "Initializing consciousness network...";
        
        await _visualization3D.InitializeAsync();
        await RefreshDataAsync();
        
        StatusMessage = "Consciousness network visualization ready";
        _logger.LogInformation("Consciousness peering visualization initialized successfully");
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Refreshing consciousness network data...";

            // Get consciousness peering data
            var peeringData = await _peeringDataService.GetPeeringDataAsync();
            var networkAnalysis = await _networkService.AnalyzeNetworkAsync(peeringData.Peers);

            // Update statistics
            TotalPeers = peeringData.Peers.Count;
            ActivePeers = peeringData.Peers.Count(p => p.IsActive);
            TotalConnections = peeringData.Peers.Sum(p => p.Connections.Count);
            NetworkDensity = networkAnalysis.Density;

            // Update peer list
            Peers.Clear();
            foreach (var peer in peeringData.Peers.OrderBy(p => p.Id))
            {
                Peers.Add(new PeerViewModel
                {
                    Id = peer.Id,
                    IsActive = peer.IsActive,
                    ConnectionCount = peer.Connections.Count,
                    ConnectionStrength = peer.Connections.Any() 
                        ? peer.Connections.Average(c => c.Strength) 
                        : 0.0,
                    StatusIcon = peer.IsActive ? "🟢" : "⚪",
                    ConnectionStrengthBar = CreateStrengthBar(
                        peer.Connections.Any() ? peer.Connections.Average(c => c.Strength) : 0.0)
                });
            }

            // Update 3D visualization
            await _visualization3D.UpdateVisualizationAsync(peeringData.Peers, networkAnalysis);

            LastUpdated = DateTime.Now;
            StatusMessage = $"Network updated - {ActivePeers}/{TotalPeers} peers active";

            _logger.LogInformation("Refreshed consciousness network: {TotalPeers} peers, {TotalConnections} connections", 
                TotalPeers, TotalConnections);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing network: {ex.Message}";
            _logger.LogError(ex, "Error refreshing consciousness network data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string CreateStrengthBar(double strength)
    {
        var barLength = 10;
        var filledLength = (int)(strength * barLength);
        return new string('█', filledLength) + new string('░', barLength - filledLength);
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        _visualization3D?.Dispose();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

/// <summary>
/// ViewModel representing a consciousness peer in the network
/// </summary>
public class PeerViewModel
{
    public string Id { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ConnectionCount { get; set; }
    public double ConnectionStrength { get; set; }
    public string StatusIcon { get; set; } = string.Empty;
    public string ConnectionStrengthBar { get; set; } = string.Empty;
}


