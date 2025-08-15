# AgileCloud.ai DNS Configuration Helper Script (PowerShell)
# Helps resolve the DNS configuration issue for custom domain

Write-Host "🌐 AgileCloud.ai DNS Configuration Helper" -ForegroundColor Cyan
Write-Host "========================================"

# Configuration
$ResourceGroup = "agilecloud-rg"
$CDNProfile = "agilecloud-cdn"
$CDNEndpoint = "agilecloud-website"
$CustomDomain = "agilecloud.ai"
$SubscriptionId = "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"

function Write-Status($message) {
    Write-Host "[INFO] $message" -ForegroundColor Blue
}

function Write-Success($message) {
    Write-Host "[SUCCESS] $message" -ForegroundColor Green
}

function Write-Warning($message) {
    Write-Host "[WARNING] $message" -ForegroundColor Yellow
}

function Write-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

Write-Status "Checking Azure configuration..."

# Check if Azure CLI is available and logged in
try {
    $account = az account show 2>$null | ConvertFrom-Json
    if (-not $account) {
        Write-Error "Azure CLI not logged in. Please run 'az login' first."
        exit 1
    }
    Write-Success "Azure CLI authenticated"
} catch {
    Write-Error "Azure CLI not found or not logged in. Please install Azure CLI and run 'az login'."
    exit 1
}

# Get CDN endpoint hostname
Write-Status "Getting CDN endpoint information..."

try {
    $CDNHostname = az cdn endpoint show `
        --name $CDNEndpoint `
        --profile-name $CDNProfile `
        --resource-group $ResourceGroup `
        --subscription $SubscriptionId `
        --query "hostName" -o tsv 2>$null
    
    if (-not $CDNHostname) {
        Write-Error "Could not find CDN endpoint. Please check if deployment completed successfully."
        
        # List available CDN endpoints
        Write-Status "Available CDN endpoints:"
        az cdn endpoint list `
            --profile-name $CDNProfile `
            --resource-group $ResourceGroup `
            --subscription $SubscriptionId `
            --query "[].{Name:name,HostName:hostName,Status:resourceState}" -o table 2>$null
        exit 1
    }
    
    Write-Success "CDN endpoint found: $CDNHostname"
} catch {
    Write-Error "Failed to get CDN endpoint information"
    exit 1
}

# Check current DNS configuration
Write-Status "Checking current DNS configuration..."

# Check if domain resolves
try {
    $DomainResolve = Resolve-DnsName $CustomDomain -ErrorAction SilentlyContinue
    if ($DomainResolve) {
        $CurrentIP = $DomainResolve | Where-Object {$_.Type -eq "A"} | Select-Object -First 1 -ExpandProperty IPAddress
        Write-Warning "Domain $CustomDomain currently resolves to: $CurrentIP"
    } else {
        Write-Error "Domain $CustomDomain does not resolve"
    }
} catch {
    Write-Error "Could not resolve domain $CustomDomain"
}

# Check www subdomain
try {
    $WWWResolve = Resolve-DnsName "www.$CustomDomain" -ErrorAction SilentlyContinue
    if ($WWWResolve) {
        $WWWIP = $WWWResolve | Where-Object {$_.Type -eq "A"} | Select-Object -First 1 -ExpandProperty IPAddress
        Write-Warning "Domain www.$CustomDomain currently resolves to: $WWWIP"
    } else {
        Write-Error "Domain www.$CustomDomain does not resolve"
    }
} catch {
    Write-Error "Could not resolve domain www.$CustomDomain"
}

# Check for CNAME records
Write-Status "Checking for CNAME records..."

try {
    $RootCNAME = Resolve-DnsName $CustomDomain -Type CNAME -ErrorAction SilentlyContinue
    if ($RootCNAME -and $RootCNAME.NameHost -eq $CDNHostname) {
        Write-Success "Root domain CNAME correctly points to CDN"
    } else {
        Write-Error "Root domain CNAME not configured correctly"
    }
} catch {
    Write-Error "Could not check root domain CNAME"
}

try {
    $WWWCNAME = Resolve-DnsName "www.$CustomDomain" -Type CNAME -ErrorAction SilentlyContinue
    if ($WWWCNAME -and $WWWCNAME.NameHost -eq $CDNHostname) {
        Write-Success "www subdomain CNAME correctly points to CDN"
    } else {
        Write-Error "www subdomain CNAME not configured correctly"
    }
} catch {
    Write-Error "Could not check www subdomain CNAME"
}

Write-Host ""
Write-Host "🔧 REQUIRED DNS CONFIGURATION" -ForegroundColor Cyan
Write-Host "=============================="
Write-Host ""
Write-Host "Configure the following DNS records with your domain registrar:"
Write-Host ""
Write-Host "Record Type: CNAME"
Write-Host "Name: www"
Write-Host "Value: $CDNHostname"
Write-Host "TTL: 300"
Write-Host ""
Write-Host "Record Type: CNAME (or ALIAS/ANAME if CNAME not supported for root)"
Write-Host "Name: @ (root domain)"
Write-Host "Value: $CDNHostname"
Write-Host "TTL: 300"
Write-Host ""

# Option to create Azure DNS zone
$CreateDNS = Read-Host "Would you like to create an Azure DNS zone for automatic management? (y/n)"
if ($CreateDNS -eq "y" -or $CreateDNS -eq "Y") {
    Write-Status "Creating Azure DNS zone..."
    
    # Create DNS zone
    try {
        az network dns zone create `
            --resource-group $ResourceGroup `
            --name $CustomDomain `
            --subscription $SubscriptionId 2>$null
        
        Write-Success "DNS zone created"
    } catch {
        Write-Warning "DNS zone may already exist or creation failed"
    }
    
    # Create CNAME records
    Write-Status "Creating CNAME records..."
    
    # Create www CNAME
    try {
        az network dns record-set cname set-record `
            --resource-group $ResourceGroup `
            --zone-name $CustomDomain `
            --record-set-name www `
            --cname $CDNHostname `
            --subscription $SubscriptionId 2>$null
        
        Write-Success "www CNAME record created"
    } catch {
        Write-Warning "Failed to create www CNAME record"
    }
    
    # Try to create root CNAME (may not work in all cases)
    try {
        az network dns record-set cname set-record `
            --resource-group $ResourceGroup `
            --zone-name $CustomDomain `
            --record-set-name "@" `
            --cname $CDNHostname `
            --subscription $SubscriptionId 2>$null
        
        Write-Success "Root CNAME record created"
    } catch {
        Write-Warning "Could not create root CNAME. You may need to use an ALIAS record."
    }
    
    # Display name servers
    Write-Host ""
    Write-Host "🌐 NAMESERVERS TO CONFIGURE" -ForegroundColor Cyan
    Write-Host "==========================="
    Write-Host ""
    Write-Host "Configure these name servers with your domain registrar:"
    Write-Host ""
    
    try {
        $NameServers = az network dns zone show `
            --resource-group $ResourceGroup `
            --name $CustomDomain `
            --subscription $SubscriptionId `
            --query "nameServers" -o tsv 2>$null
        
        $NameServers.Split("`n") | ForEach-Object {
            if ($_.Trim()) {
                Write-Host "  $($_.Trim())"
            }
        }
    } catch {
        Write-Error "Could not retrieve name servers"
    }
    
    Write-Host ""
    Write-Warning "You must update your domain registrar to use these name servers!"
} else {
    Write-Host ""
    Write-Warning "Manual DNS configuration required with your domain registrar"
}

Write-Host ""
Write-Host "⏰ DNS PROPAGATION" -ForegroundColor Cyan
Write-Host "=================="
Write-Host ""
Write-Host "After configuring DNS records:"
Write-Host "- Initial propagation: 5-15 minutes"
Write-Host "- Full global propagation: up to 48 hours"
Write-Host "- You can check status with: Resolve-DnsName $CustomDomain"
Write-Host ""

# Option to test DNS and retry deployment
$TestDNS = Read-Host "Would you like to wait and test DNS propagation? (y/n)"
if ($TestDNS -eq "y" -or $TestDNS -eq "Y") {
    Write-Status "Waiting for DNS propagation (checking every 30 seconds)..."
    
    for ($i = 1; $i -le 20; $i++) {
        Write-Host -NoNewline "."
        Start-Sleep 30
        
        try {
            $TestResolve = Resolve-DnsName $CustomDomain -Type CNAME -ErrorAction SilentlyContinue
            if ($TestResolve -and $TestResolve.NameHost -eq $CDNHostname) {
                Write-Host ""
                Write-Success "DNS propagation successful!"
                break
            }
        } catch {
            # Continue checking
        }
        
        if ($i -eq 20) {
            Write-Host ""
            Write-Warning "DNS still propagating. May take longer."
        }
    }
}

Write-Host ""
Write-Host "🚀 NEXT STEPS" -ForegroundColor Cyan
Write-Host "============="
Write-Host ""
Write-Host "1. Configure DNS records as shown above"
Write-Host "2. Wait for DNS propagation (5-60 minutes)"
Write-Host "3. Re-run the GitHub Actions deployment workflow"
Write-Host "4. If issues persist, deploy without custom domain first:"
Write-Host "   - Comment out custom domain configuration in workflow"
Write-Host "   - Deploy to CDN endpoint directly: https://$CDNHostname"
Write-Host "   - Add custom domain later after DNS is confirmed working"
Write-Host ""
Write-Success "DNS configuration helper completed!"

Read-Host "Press Enter to exit"
