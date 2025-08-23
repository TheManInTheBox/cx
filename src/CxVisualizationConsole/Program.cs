using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CxLanguage.Runtime;
using CxLanguage.Core;

namespace CxVisualizationConsole;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("🧠 CX Language Runtime - Consciousness Peering Visualization Console");
        Console.WriteLine("=====================================================================");
        Console.WriteLine();
        
        var host = CreateHost();
        
        try
        {
            var visualizer = host.Services.GetRequiredService<ConsciousnessPeeringVisualizer>();
            await visualizer.StartVisualizationAsync();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
    
    static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
                
                services.AddSingleton<ConsciousnessPeeringVisualizer>();
                services.AddSingleton<PeeringDataService>();
                services.AddSingleton<ConsciousnessNetworkService>();
            })
            .Build();
    }
}

public class ConsciousnessPeeringVisualizer
{
    private readonly ILogger<ConsciousnessPeeringVisualizer> _logger;
    private readonly PeeringDataService _peeringData;
    private readonly ConsciousnessNetworkService _networkService;
    
    public ConsciousnessPeeringVisualizer(
        ILogger<ConsciousnessPeeringVisualizer> logger,
        PeeringDataService peeringData,
        ConsciousnessNetworkService networkService)
    {
        _logger = logger;
        _peeringData = peeringData;
        _networkService = networkService;
    }
    
    public async Task StartVisualizationAsync()
    {
        _logger.LogInformation("🚀 Starting Consciousness Peering Visualization...");
        
        // Get current peering relationships
        var peers = await _peeringData.GetCurrentPeersAsync();
        var network = await _networkService.AnalyzeNetworkAsync(peers);
        
        Console.WriteLine($"📊 Consciousness Network Analysis:");
        Console.WriteLine($"   Active Peers: {peers.Count}");
        Console.WriteLine($"   Total Connections: {network.TotalConnections}");
        Console.WriteLine($"   Network Density: {network.Density:P2}");
        Console.WriteLine($"   Clustering Coefficient: {network.ClusteringCoefficient:F3}");
        Console.WriteLine();
        
        // Display peer details
        DisplayPeerDetails(peers);
        
        // Monitor real-time changes
        await MonitorRealTimeChangesAsync();
    }
    
    private void DisplayPeerDetails(List<ConsciousnessPeer> peers)
    {
        Console.WriteLine("🧠 Active Consciousness Peers:");
        Console.WriteLine("─────────────────────────────────────────");
        
        if (peers.Count == 0)
        {
            Console.WriteLine("   No active peers found");
        }
        else
        {
            foreach (var peer in peers)
            {
                var stateIcon = peer.State switch
                {
                    "Active" => "🟢",
                    "Processing" => "🟡", 
                    "Idle" => "⚪",
                    _ => "❓"
                };
                
                var strengthBar = GenerateStrengthBar(peer.ConnectionStrength);
                var lastSeenText = GetLastSeenText(peer.LastSeen);
                
                Console.WriteLine($"   {stateIcon} {peer.Name}");
                Console.WriteLine($"      ID: {peer.Id}");
                Console.WriteLine($"      Connection: {strengthBar} ({peer.ConnectionStrength:P0})");
                Console.WriteLine($"      Last Seen: {lastSeenText}");
                Console.WriteLine();
            }
        }
    }
    
    private string GenerateStrengthBar(double strength)
    {
        var barLength = 20;
        var filledLength = (int)(strength * barLength);
        var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
        return bar;
    }
    
    private string GetLastSeenText(DateTime lastSeen)
    {
        var timeDiff = DateTime.Now - lastSeen;
        
        if (timeDiff.TotalMinutes < 1)
            return $"{(int)timeDiff.TotalSeconds} seconds ago";
        else if (timeDiff.TotalHours < 1)
            return $"{(int)timeDiff.TotalMinutes} minutes ago";
        else
            return $"{(int)timeDiff.TotalHours} hours ago";
    }
    
    private async Task MonitorRealTimeChangesAsync()
    {
        Console.WriteLine("🔄 Monitoring real-time consciousness changes...");
        Console.WriteLine("   (Simulated data - press any key to stop)");
        Console.WriteLine();
        
        var random = new Random();
        var updateCount = 0;
        
        while (!Console.KeyAvailable)
        {
            await Task.Delay(2000);
            updateCount++;
            
            // Simulate dynamic updates
            var eventType = random.Next(1, 4) switch
            {
                1 => "🔗 New peer connection established",
                2 => "⚡ Consciousness activity spike detected",
                3 => "🔄 Peer state transition observed",
                _ => "📊 Network topology updated"
            };
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{timestamp}] {eventType}");
            
            if (updateCount >= 5)
            {
                Console.WriteLine("\n💡 Real-time monitoring would continue indefinitely in production...");
                break;
            }
        }
    }
}

public class PeeringDataService
{
    private readonly ILogger<PeeringDataService> _logger;
    
    public PeeringDataService(ILogger<PeeringDataService> logger)
    {
        _logger = logger;
    }
    
    public async Task<List<ConsciousnessPeer>> GetCurrentPeersAsync()
    {
        _logger.LogInformation("Collecting consciousness peering data...");
        
        // Simulate real peer discovery
        await Task.Delay(500);
        
        return new List<ConsciousnessPeer>
        {
            new() { Id = "cx-agent-1", Name = "Cognitive Agent Alpha", State = "Active", 
                   ConnectionStrength = 0.85, LastSeen = DateTime.Now.AddMinutes(-2) },
            new() { Id = "cx-agent-2", Name = "Neural Processor Beta", State = "Processing", 
                   ConnectionStrength = 0.72, LastSeen = DateTime.Now.AddMinutes(-1) },
            new() { Id = "cx-agent-3", Name = "Consciousness Monitor Gamma", State = "Active", 
                   ConnectionStrength = 0.91, LastSeen = DateTime.Now.AddSeconds(-30) },
            new() { Id = "cx-agent-4", Name = "Event Handler Delta", State = "Idle", 
                   ConnectionStrength = 0.45, LastSeen = DateTime.Now.AddMinutes(-5) },
            new() { Id = "cx-agent-5", Name = "Stream Processor Epsilon", State = "Active", 
                   ConnectionStrength = 0.78, LastSeen = DateTime.Now.AddSeconds(-15) }
        };
    }
}

public class ConsciousnessNetworkService
{
    private readonly ILogger<ConsciousnessNetworkService> _logger;
    
    public ConsciousnessNetworkService(ILogger<ConsciousnessNetworkService> logger)
    {
        _logger = logger;
    }
    
    public async Task<NetworkAnalysis> AnalyzeNetworkAsync(List<ConsciousnessPeer> peers)
    {
        _logger.LogInformation("Analyzing consciousness network topology...");
        
        await Task.Delay(300);
        
        var connections = GenerateConnections(peers);
        var totalPossibleConnections = peers.Count * (peers.Count - 1) / 2;
        var density = totalPossibleConnections > 0 ? (double)connections.Count / totalPossibleConnections : 0;
        
        return new NetworkAnalysis
        {
            TotalConnections = connections.Count,
            Density = density,
            ClusteringCoefficient = CalculateClusteringCoefficient(peers, connections),
            Connections = connections
        };
    }
    
    private List<Connection> GenerateConnections(List<ConsciousnessPeer> peers)
    {
        var connections = new List<Connection>();
        var random = new Random();
        
        for (int i = 0; i < peers.Count; i++)
        {
            for (int j = i + 1; j < peers.Count; j++)
            {
                // Create connections based on peer states and strength
                var strength = CalculateConnectionStrength(peers[i], peers[j], random);
                
                if (strength > 0.3) // Only show meaningful connections
                {
                    connections.Add(new Connection
                    {
                        FromId = peers[i].Id,
                        ToId = peers[j].Id,
                        Strength = strength
                    });
                }
            }
        }
        
        return connections;
    }
    
    private double CalculateConnectionStrength(ConsciousnessPeer peer1, ConsciousnessPeer peer2, Random random)
    {
        // Base strength on peer states and connection strengths
        var baseStrength = (peer1.ConnectionStrength + peer2.ConnectionStrength) / 2;
        
        // Boost for active peers
        if (peer1.State == "Active" && peer2.State == "Active")
            baseStrength *= 1.2;
        
        // Add some randomness for simulation
        var variation = (random.NextDouble() - 0.5) * 0.4;
        
        return Math.Max(0, Math.Min(1, baseStrength + variation));
    }
    
    private double CalculateClusteringCoefficient(List<ConsciousnessPeer> peers, List<Connection> connections)
    {
        if (peers.Count < 3) return 0;
        
        // Simplified clustering coefficient calculation
        var triangles = 0;
        var totalTriples = 0;
        
        for (int i = 0; i < peers.Count; i++)
        {
            var neighborIds = new HashSet<string>();
            
            foreach (var conn in connections)
            {
                if (conn.FromId == peers[i].Id) neighborIds.Add(conn.ToId);
                if (conn.ToId == peers[i].Id) neighborIds.Add(conn.FromId);
            }
            
            if (neighborIds.Count >= 2)
            {
                totalTriples += neighborIds.Count * (neighborIds.Count - 1) / 2;
                
                // Count actual triangles
                foreach (var neighbor1 in neighborIds)
                {
                    foreach (var neighbor2 in neighborIds)
                    {
                        if (neighbor1 != neighbor2 && 
                            connections.Any(c => (c.FromId == neighbor1 && c.ToId == neighbor2) ||
                                               (c.FromId == neighbor2 && c.ToId == neighbor1)))
                        {
                            triangles++;
                        }
                    }
                }
            }
        }
        
        return totalTriples > 0 ? (double)triangles / (2 * totalTriples) : 0;
    }
}

public class ConsciousnessPeer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public double ConnectionStrength { get; set; }
    public DateTime LastSeen { get; set; }
}

public class NetworkAnalysis
{
    public int TotalConnections { get; set; }
    public double Density { get; set; }
    public double ClusteringCoefficient { get; set; }
    public List<Connection> Connections { get; set; } = new();
}

public class Connection
{
    public string FromId { get; set; } = string.Empty;
    public string ToId { get; set; } = string.Empty;
    public double Strength { get; set; }
}
