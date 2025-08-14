#!/bin/bash

# AgileCloud.ai Website Deployment Script
# This script deploys the website to Azure and configures the custom domain

set -e

# Configuration
SUBSCRIPTION_ID="0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb"
RESOURCE_GROUP="agilecloud-rg"
STORAGE_ACCOUNT="agilecloudwebsite"
CDN_PROFILE="agilecloud-cdn"
CDN_ENDPOINT="agilecloud-website"
CUSTOM_DOMAIN="agilecloud.ai"
LOCATION="East US"

echo "🚀 Starting AgileCloud.ai website deployment..."

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed. Please install it first."
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Login check
echo "🔐 Checking Azure login status..."
if ! az account show &> /dev/null; then
    echo "🔑 Please login to Azure CLI:"
    az login
fi

# Set subscription
echo "📋 Setting Azure subscription..."
az account set --subscription $SUBSCRIPTION_ID

# Create resource group
echo "📁 Creating resource group..."
az group create --name $RESOURCE_GROUP --location "$LOCATION" || true

# Create storage account
echo "💾 Creating storage account..."
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location "$LOCATION" \
  --sku Standard_LRS \
  --kind StorageV2 \
  --access-tier Hot || true

# Enable static website hosting
echo "🌐 Enabling static website hosting..."
az storage blob service-properties update \
  --account-name $STORAGE_ACCOUNT \
  --static-website \
  --404-document 404.html \
  --index-document index.html

# Build website (if build tools are available)
echo "🔨 Building website..."
cd website
if [ -f package.json ]; then
    if command -v npm &> /dev/null; then
        echo "📦 Installing dependencies..."
        npm install
        echo "🏗️ Building optimized version..."
        npm run build || echo "⚠️ Build failed, using source files"
    fi
fi

# Determine source directory
if [ -d "dist" ]; then
    SOURCE_DIR="dist"
    echo "📂 Using built files from dist/"
else
    SOURCE_DIR="public"
    echo "📂 Using source files from public/"
fi

# Get storage account key
echo "🔑 Getting storage account key..."
STORAGE_KEY=$(az storage account keys list \
  --account-name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --query "[0].value" -o tsv)

# Upload files
echo "📤 Uploading website files..."
az storage blob upload-batch \
  --destination '$web' \
  --source $SOURCE_DIR \
  --account-name $STORAGE_ACCOUNT \
  --account-key "$STORAGE_KEY" \
  --overwrite true

# Create CDN profile
echo "🌍 Creating CDN profile..."
az cdn profile create \
  --name $CDN_PROFILE \
  --resource-group $RESOURCE_GROUP \
  --sku Standard_Microsoft || true

# Get storage account web endpoint
ORIGIN_URL=$(az storage account show \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --query "primaryEndpoints.web" -o tsv | sed 's|https://||' | sed 's|/$||')

# Create CDN endpoint
echo "⚡ Creating CDN endpoint..."
az cdn endpoint create \
  --name $CDN_ENDPOINT \
  --profile-name $CDN_PROFILE \
  --resource-group $RESOURCE_GROUP \
  --origin $ORIGIN_URL \
  --origin-host-header $ORIGIN_URL || true

# Configure custom domain
echo "🔗 Configuring custom domain..."
az cdn custom-domain create \
  --endpoint-name $CDN_ENDPOINT \
  --name agilecloud-custom-domain \
  --profile-name $CDN_PROFILE \
  --resource-group $RESOURCE_GROUP \
  --hostname $CUSTOM_DOMAIN || echo "⚠️ Custom domain configuration may require manual DNS setup"

# Purge CDN cache
echo "🧹 Purging CDN cache..."
az cdn endpoint purge \
  --name $CDN_ENDPOINT \
  --profile-name $CDN_PROFILE \
  --resource-group $RESOURCE_GROUP \
  --content-paths "/*" || true

# Get URLs
WEBSITE_URL=$(az storage account show \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --query "primaryEndpoints.web" -o tsv)

CDN_URL=$(az cdn endpoint show \
  --name $CDN_ENDPOINT \
  --profile-name $CDN_PROFILE \
  --resource-group $RESOURCE_GROUP \
  --query "hostName" -o tsv 2>/dev/null || echo "not-configured")

echo ""
echo "✅ Deployment completed successfully!"
echo ""
echo "🌐 Website URLs:"
echo "   Primary: $WEBSITE_URL"
if [ "$CDN_URL" != "not-configured" ]; then
    echo "   CDN: https://$CDN_URL"
fi
echo "   Custom Domain: https://$CUSTOM_DOMAIN (requires DNS configuration)"
echo ""
echo "📋 Next Steps:"
echo "1. Configure DNS CNAME record: $CUSTOM_DOMAIN -> $CDN_URL"
echo "2. Enable HTTPS certificate in Azure CDN"
echo "3. Test the website at all URLs"
echo ""
echo "🔧 Azure Resources Created:"
echo "   - Resource Group: $RESOURCE_GROUP"
echo "   - Storage Account: $STORAGE_ACCOUNT"
echo "   - CDN Profile: $CDN_PROFILE"
echo "   - CDN Endpoint: $CDN_ENDPOINT"
