#!/bin/bash

# AgileCloud.ai DNS Configuration Helper Script
# Helps resolve the DNS configuration issue for custom domain

set -e

echo "🌐 AgileCloud.ai DNS Configuration Helper"
echo "========================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Configuration
RESOURCE_GROUP="agilecloud-rg"
CDN_PROFILE="agilecloud-cdn"
CDN_ENDPOINT="agilecloud-website"
CUSTOM_DOMAIN="agilecloud.ai"
SUBSCRIPTION_ID="0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"

print_status "Checking Azure configuration..."

# Check if Azure CLI is logged in
if ! az account show &>/dev/null; then
    print_error "Azure CLI not logged in. Please run 'az login' first."
    exit 1
fi

print_success "Azure CLI authenticated"

# Get CDN endpoint hostname
print_status "Getting CDN endpoint information..."

CDN_HOSTNAME=$(az cdn endpoint show \
    --name $CDN_ENDPOINT \
    --profile-name $CDN_PROFILE \
    --resource-group $RESOURCE_GROUP \
    --subscription $SUBSCRIPTION_ID \
    --query "hostName" -o tsv 2>/dev/null || echo "")

if [ -z "$CDN_HOSTNAME" ]; then
    print_error "Could not find CDN endpoint. Please check if deployment completed successfully."
    
    # List available CDN endpoints
    print_status "Available CDN endpoints:"
    az cdn endpoint list \
        --profile-name $CDN_PROFILE \
        --resource-group $RESOURCE_GROUP \
        --subscription $SUBSCRIPTION_ID \
        --query "[].{Name:name,HostName:hostName,Status:resourceState}" -o table 2>/dev/null || true
    exit 1
fi

print_success "CDN endpoint found: $CDN_HOSTNAME"

# Check current DNS configuration
print_status "Checking current DNS configuration..."

# Check if domain resolves
if nslookup $CUSTOM_DOMAIN &>/dev/null; then
    CURRENT_IP=$(nslookup $CUSTOM_DOMAIN | grep "Address:" | tail -1 | awk '{print $2}')
    print_warning "Domain $CUSTOM_DOMAIN currently resolves to: $CURRENT_IP"
else
    print_error "Domain $CUSTOM_DOMAIN does not resolve"
fi

# Check www subdomain
if nslookup www.$CUSTOM_DOMAIN &>/dev/null; then
    WWW_IP=$(nslookup www.$CUSTOM_DOMAIN | grep "Address:" | tail -1 | awk '{print $2}')
    print_warning "Domain www.$CUSTOM_DOMAIN currently resolves to: $WWW_IP"
else
    print_error "Domain www.$CUSTOM_DOMAIN does not resolve"
fi

# Check for CNAME records
print_status "Checking for CNAME records..."

if dig CNAME $CUSTOM_DOMAIN +short | grep -q "$CDN_HOSTNAME"; then
    print_success "Root domain CNAME correctly points to CDN"
else
    print_error "Root domain CNAME not configured correctly"
fi

if dig CNAME www.$CUSTOM_DOMAIN +short | grep -q "$CDN_HOSTNAME"; then
    print_success "www subdomain CNAME correctly points to CDN"
else
    print_error "www subdomain CNAME not configured correctly"
fi

echo ""
echo "🔧 REQUIRED DNS CONFIGURATION"
echo "=============================="
echo ""
echo "Configure the following DNS records with your domain registrar:"
echo ""
echo "Record Type: CNAME"
echo "Name: www"
echo "Value: $CDN_HOSTNAME"
echo "TTL: 300"
echo ""
echo "Record Type: CNAME (or ALIAS/ANAME if CNAME not supported for root)"
echo "Name: @ (root domain)"
echo "Value: $CDN_HOSTNAME"
echo "TTL: 300"
echo ""

# Option to create Azure DNS zone
read -p "Would you like to create an Azure DNS zone for automatic management? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Creating Azure DNS zone..."
    
    # Create DNS zone
    az network dns zone create \
        --resource-group $RESOURCE_GROUP \
        --name $CUSTOM_DOMAIN \
        --subscription $SUBSCRIPTION_ID || true
    
    print_success "DNS zone created"
    
    # Create CNAME records
    print_status "Creating CNAME records..."
    
    # Create www CNAME
    az network dns record-set cname set-record \
        --resource-group $RESOURCE_GROUP \
        --zone-name $CUSTOM_DOMAIN \
        --record-set-name www \
        --cname $CDN_HOSTNAME \
        --subscription $SUBSCRIPTION_ID || true
    
    # Try to create root CNAME (may not work in all cases)
    az network dns record-set cname set-record \
        --resource-group $RESOURCE_GROUP \
        --zone-name $CUSTOM_DOMAIN \
        --record-set-name @ \
        --cname $CDN_HOSTNAME \
        --subscription $SUBSCRIPTION_ID || print_warning "Could not create root CNAME. You may need to use an ALIAS record."
    
    print_success "CNAME records created"
    
    # Display name servers
    echo ""
    echo "🌐 NAMESERVERS TO CONFIGURE"
    echo "==========================="
    echo ""
    echo "Configure these name servers with your domain registrar:"
    echo ""
    az network dns zone show \
        --resource-group $RESOURCE_GROUP \
        --name $CUSTOM_DOMAIN \
        --subscription $SUBSCRIPTION_ID \
        --query "nameServers" -o tsv | while read ns; do
        echo "  $ns"
    done
    echo ""
    print_warning "You must update your domain registrar to use these name servers!"
else
    echo ""
    print_warning "Manual DNS configuration required with your domain registrar"
fi

echo ""
echo "⏰ DNS PROPAGATION"
echo "=================="
echo ""
echo "After configuring DNS records:"
echo "- Initial propagation: 5-15 minutes"
echo "- Full global propagation: up to 48 hours"
echo "- You can check status with: nslookup $CUSTOM_DOMAIN"
echo ""

# Option to test DNS and retry deployment
read -p "Would you like to wait and test DNS propagation? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Waiting for DNS propagation (checking every 30 seconds)..."
    
    for i in {1..20}; do
        echo -n "."
        sleep 30
        
        if nslookup $CUSTOM_DOMAIN | grep -q "$CDN_HOSTNAME"; then
            print_success "DNS propagation successful!"
            break
        fi
        
        if [ $i -eq 20 ]; then
            print_warning "DNS still propagating. May take longer."
        fi
    done
fi

echo ""
echo "🚀 NEXT STEPS"
echo "============="
echo ""
echo "1. Configure DNS records as shown above"
echo "2. Wait for DNS propagation (5-60 minutes)"
echo "3. Re-run the GitHub Actions deployment workflow"
echo "4. If issues persist, deploy without custom domain first:"
echo "   - Comment out custom domain configuration in workflow"
echo "   - Deploy to CDN endpoint directly: https://$CDN_HOSTNAME"
echo "   - Add custom domain later after DNS is confirmed working"
echo ""
print_success "DNS configuration helper completed!"
