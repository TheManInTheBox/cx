# Custom Domain Setup Instructions for AgileCloud.ai
# Based on successful GitHub Actions deployment

## 🌐 AgileCloud.ai Custom Domain Setup

Your website has been successfully deployed to Azure! Here's how to configure the custom domain `agilecloud.ai`:

### 1. Expected Azure Resources
Based on the successful deployment, these resources should exist:
- **Resource Group**: `agilecloud-rg`
- **Storage Account**: `agilecloudwebsite`
- **CDN Profile**: `agilecloud-cdn`
- **CDN Endpoint**: `agilecloud-website`

### 2. Find Your CDN Endpoint URL
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to "Resource Groups" → "agilecloud-rg"
3. Find the CDN endpoint "agilecloud-website"
4. Copy the hostname (something like: `agilecloud-website.azureedge.net`)

### 3. Configure DNS for agilecloud.ai
You need to set up DNS records with your domain registrar:

#### Option A: If your registrar supports CNAME for root domain
```
Type: CNAME
Name: agilecloud.ai (or @)
Value: [your-cdn-endpoint].azureedge.net
TTL: 300

Type: CNAME  
Name: www
Value: [your-cdn-endpoint].azureedge.net
TTL: 300
```

#### Option B: If your registrar requires A records for root domain
```
Type: A
Name: agilecloud.ai (or @)
Value: [IP address of CDN endpoint]
TTL: 300

Type: CNAME
Name: www  
Value: [your-cdn-endpoint].azureedge.net
TTL: 300
```

### 4. Add Custom Domain to Azure CDN
Once DNS is configured, add the custom domain to your CDN:

1. In Azure Portal, go to your CDN endpoint
2. Click "Custom domains" 
3. Click "Add custom domain"
4. Enter: `agilecloud.ai`
5. Click "Add"
6. Repeat for `www.agilecloud.ai`

### 5. Enable HTTPS
1. In the custom domains section
2. Click on your domain
3. Enable "Custom domain HTTPS"
4. Choose "CDN managed" certificate
5. Wait for certificate provisioning (can take up to 6 hours)

### 6. Test Your Website
After DNS propagation (24-48 hours), test:
- https://agilecloud.ai
- https://www.agilecloud.ai

### 7. Current Test URLs
You can test the deployment immediately at:
- **Storage URL**: https://agilecloudwebsite.z13.web.core.windows.net/ (direct storage)
- **CDN URL**: https://agilecloud-website.azureedge.net/ (faster, cached)

### Troubleshooting
- **DNS Check**: Use `nslookup agilecloud.ai` to verify DNS propagation
- **SSL Issues**: CDN-managed certificates can take up to 6 hours to provision
- **Cache Issues**: Use CDN purge if you see old content

## 🚀 Your AgileCloud.ai website is live and ready for custom domain configuration!
