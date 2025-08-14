# Azure Deployment Setup Guide

This guide explains how to set up automated deployment of the AgileCloud.ai website to Azure using GitHub Actions.

## 🔧 Prerequisites

1. **Azure Subscription**: `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`
2. **GitHub Repository**: Write access to configure secrets
3. **Azure CLI**: For initial setup (optional)

## 🚀 Quick Setup

### 1. Create Azure Service Principal

Run the following Azure CLI commands to create a service principal for GitHub Actions:

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb

# Create service principal
az ad sp create-for-rbac \
  --name "github-actions-agilecloud-website" \
  --role "Contributor" \
  --scopes "/subscriptions/0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb" \
  --sdk-auth
```

This will output JSON credentials that look like:
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

### 2. Configure GitHub Secrets

Go to your GitHub repository settings and add the following secrets:

#### Required Secret:
- **Name**: `AZURE_CREDENTIALS`
- **Value**: The entire JSON output from the service principal creation

#### Optional Variables:
- **Name**: `CUSTOM_DOMAIN`
- **Value**: Your custom domain (e.g., `agilecloud.ai`)

### 3. Deploy the Website

The deployment will automatically trigger when you:
- Push changes to the `master` branch in the `website/` directory
- Create a pull request affecting the `website/` directory
- Manually trigger the workflow from GitHub Actions

## 🌐 Azure Resources

The deployment creates the following Azure resources:

### Storage Account
- **Name**: `agilecloudwebsite`
- **Type**: StorageV2 with static website hosting
- **Location**: East US
- **Features**: 
  - Static website hosting enabled
  - `$web` container for website files
  - Custom 404 error page support

### CDN Profile & Endpoint
- **Profile**: `agilecloud-cdn` (Standard Microsoft)
- **Endpoint**: `agilecloud-website`
- **Features**:
  - Global content distribution
  - Automatic cache purging
  - Custom domain support (optional)

### Resource Group
- **Name**: `agilecloud-rg`
- **Location**: East US
- **Contains**: All website-related resources

## 📊 Deployment Process

1. **Trigger**: Push to master or manual workflow dispatch
2. **Build**: 
   - Install Node.js dependencies
   - Minify HTML, CSS, and JavaScript
   - Optimize assets for production
3. **Deploy**:
   - Create Azure resources (if needed)
   - Upload files to Azure Storage `$web` container
   - Configure static website hosting
   - Set up Azure CDN
   - Purge CDN cache
4. **Verify**: Display website URLs and deployment status

## 🔍 Monitoring & Troubleshooting

### Check Deployment Status
- GitHub Actions tab in your repository
- Azure portal for resource health
- CDN endpoint status and metrics

### Common Issues

#### 1. Authentication Failures
```
Error: Azure login failed
```
**Solution**: Verify `AZURE_CREDENTIALS` secret is correctly formatted

#### 2. Resource Creation Failures
```
Error: Storage account name already exists
```
**Solution**: Storage account names must be globally unique. Update the name in the workflow.

#### 3. CDN Issues
```
Warning: CDN endpoint not configured
```
**Solution**: CDN creation is optional and may fail in some regions. The website will still work via storage URL.

### Manual Resource Creation

If automatic creation fails, you can create resources manually:

```bash
# Create resource group
az group create --name agilecloud-rg --location "East US"

# Create storage account
az storage account create \
  --name agilecloudwebsite \
  --resource-group agilecloud-rg \
  --location "East US" \
  --sku Standard_LRS \
  --kind StorageV2

# Enable static website hosting
az storage blob service-properties update \
  --account-name agilecloudwebsite \
  --static-website \
  --404-document 404.html \
  --index-document index.html
```

## 🌍 URLs After Deployment

### Primary Website URL
Format: `https://{storage-account}.z13.web.core.windows.net/`
Example: `https://agilecloudwebsite.z13.web.core.windows.net/`

### CDN URL (if configured)
Format: `https://{cdn-endpoint}.azureedge.net/`
Example: `https://agilecloud-website.azureedge.net/`

### Custom Domain (if configured)
Your configured custom domain will point to the CDN endpoint.

## 🔐 Security Features

- **HTTPS Enforced**: All traffic redirected to HTTPS
- **Security Headers**: XSS protection, content type options, frame options
- **Content Security Policy**: Restricts resource loading for security
- **Static Content**: No server-side code execution

## 💰 Cost Considerations

### Storage Account
- **Hot tier**: ~$0.0184/GB/month
- **Bandwidth**: First 5GB free, then ~$0.09/GB
- **Transactions**: ~$0.0004 per 10,000 transactions

### CDN
- **Standard Microsoft**: ~$0.081/GB for first 10TB
- **HTTP/HTTPS requests**: ~$0.0075 per 10,000 requests

**Estimated monthly cost for typical website**: $1-5 USD

## 📈 Performance Optimization

- **Minified Assets**: HTML, CSS, and JavaScript are minified
- **CDN Distribution**: Global edge locations for fast loading
- **Caching**: Aggressive caching with purge on deployment
- **Compression**: Gzip compression enabled

## 🚀 Advanced Configuration

### Custom Domain Setup
1. Add DNS CNAME record pointing to CDN endpoint
2. Configure custom domain in Azure CDN
3. Enable HTTPS certificate (automatic)

### Multi-Environment Deployment
Create separate workflows for staging/production:
- Use different storage account names
- Separate resource groups
- Branch-based deployment triggers

---

## 📞 Support

For deployment issues:
1. Check GitHub Actions logs
2. Verify Azure portal resource status  
3. Review Azure storage logs
4. Test CDN endpoint functionality

**Repository**: https://github.com/TheManInTheBox/cx
**Documentation**: See `website/README.md` for local development
