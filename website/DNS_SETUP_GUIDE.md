# DNS Configuration Guide for AgileCloud.ai

## 🌐 **DNS Setup Required for Custom Domain**

The error indicates that Azure cannot find the proper DNS records for `agilecloud.ai` pointing to your CDN endpoint. Here's how to fix it:

---

## 🎯 **Required DNS Records**

### **Step 1: Get Your Azure CDN Endpoint**

First, find your CDN endpoint URL:

```bash
# Get CDN endpoint hostname
az cdn endpoint show \
  --name agilecloud-website \
  --profile-name agilecloud-cdn \
  --resource-group agilecloud-rg \
  --query "hostName" -o tsv
```

**Expected Result**: Something like `agilecloud-website.azureedge.net`

---

## 🔧 **DNS Configuration Steps**

### **Option 1: Using Azure DNS (Recommended)**

```bash
# Create DNS zone in Azure
az network dns zone create \
  --resource-group agilecloud-rg \
  --name agilecloud.ai

# Create CNAME record for www
az network dns record-set cname set-record \
  --resource-group agilecloud-rg \
  --zone-name agilecloud.ai \
  --record-set-name www \
  --cname agilecloud-website.azureedge.net

# Create CNAME record for root domain (using alias)
az network dns record-set cname set-record \
  --resource-group agilecloud-rg \
  --zone-name agilecloud.ai \
  --record-set-name @ \
  --cname agilecloud-website.azureedge.net

# Get name servers for your domain registrar
az network dns zone show \
  --resource-group agilecloud-rg \
  --name agilecloud.ai \
  --query "nameServers" -o table
```

### **Option 2: Using Your Domain Registrar's DNS**

If you prefer to use your current DNS provider, add these records:

| Type  | Name | Value | TTL |
|-------|------|-------|-----|
| CNAME | www  | agilecloud-website.azureedge.net | 300 |
| CNAME | @    | agilecloud-website.azureedge.net | 300 |

**Note**: Some registrars don't allow CNAME for root domain (@). In that case, use:

| Type  | Name | Value | TTL |
|-------|------|-------|-----|
| ALIAS/ANAME | @ | agilecloud-website.azureedge.net | 300 |
| CNAME | www | agilecloud-website.azureedge.net | 300 |

---

## 🛠️ **Updated Deployment Script**

Here's a modified GitHub Actions workflow that handles DNS setup:

```yaml
# Add this step before "Configure Custom Domain" in .github/workflows/deploy-website.yml

- name: Setup DNS Records
  run: |
    # Get CDN endpoint hostname
    CDN_HOSTNAME=$(az cdn endpoint show \
      --name $AZURE_CDN_ENDPOINT \
      --profile-name $AZURE_CDN_PROFILE \
      --resource-group $AZURE_RESOURCE_GROUP \
      --subscription $AZURE_SUBSCRIPTION_ID \
      --query "hostName" -o tsv)
    
    echo "CDN Hostname: $CDN_HOSTNAME"
    
    # Create DNS zone if it doesn't exist
    az network dns zone create \
      --resource-group $AZURE_RESOURCE_GROUP \
      --name $CUSTOM_DOMAIN \
      --subscription $AZURE_SUBSCRIPTION_ID || true
    
    # Create CNAME record for www
    az network dns record-set cname set-record \
      --resource-group $AZURE_RESOURCE_GROUP \
      --zone-name $CUSTOM_DOMAIN \
      --record-set-name www \
      --cname $CDN_HOSTNAME \
      --subscription $AZURE_SUBSCRIPTION_ID || true
    
    # Create ALIAS record for root domain
    az network dns record-set cname set-record \
      --resource-group $AZURE_RESOURCE_GROUP \
      --zone-name $CUSTOM_DOMAIN \
      --record-set-name @ \
      --cname $CDN_HOSTNAME \
      --subscription $AZURE_SUBSCRIPTION_ID || true
    
    # Display name servers
    echo "Configure these name servers with your domain registrar:"
    az network dns zone show \
      --resource-group $AZURE_RESOURCE_GROUP \
      --name $CUSTOM_DOMAIN \
      --subscription $AZURE_SUBSCRIPTION_ID \
      --query "nameServers" -o table

- name: Wait for DNS Propagation
  run: |
    echo "Waiting for DNS propagation..."
    sleep 300  # Wait 5 minutes for DNS to propagate
    
    # Test DNS resolution
    nslookup $CUSTOM_DOMAIN || echo "DNS not yet propagated"
    nslookup www.$CUSTOM_DOMAIN || echo "www DNS not yet propagated"
```

---

## 🔍 **Manual DNS Verification**

### **Check Current DNS Settings**

```bash
# Check if DNS is configured correctly
nslookup agilecloud.ai
nslookup www.agilecloud.ai

# Check CNAME records specifically
dig CNAME agilecloud.ai
dig CNAME www.agilecloud.ai
```

### **Expected Results**
- `agilecloud.ai` should point to `agilecloud-website.azureedge.net`
- `www.agilecloud.ai` should point to `agilecloud-website.azureedge.net`

---

## 🚀 **Quick Fix Deployment**

### **Immediate Action Required**

1. **Get your CDN endpoint**:
   ```bash
   az cdn endpoint list \
     --profile-name agilecloud-cdn \
     --resource-group agilecloud-rg \
     --query "[].{Name:name,HostName:hostName}" -o table
   ```

2. **Configure DNS records** with your domain registrar or use Azure DNS

3. **Wait for propagation** (5-60 minutes)

4. **Re-run deployment** after DNS is configured

### **Alternative: Deploy Without Custom Domain First**

Modify the workflow to skip custom domain configuration temporarily:

```yaml
# Comment out this section in deploy-website.yml
# - name: Configure Custom Domain
#   if: env.CUSTOM_DOMAIN
#   run: |
#     # Add custom domain if CUSTOM_DOMAIN is set
#     az cdn custom-domain create \
#       --endpoint-name $AZURE_CDN_ENDPOINT \
#       --name agilecloud-custom-domain \
#       --profile-name $AZURE_CDN_PROFILE \
#       --resource-group $AZURE_RESOURCE_GROUP \
#       --hostname $CUSTOM_DOMAIN \
#       --subscription $AZURE_SUBSCRIPTION_ID || true
```

---

## 📋 **DNS Configuration Checklist**

- [ ] Get Azure CDN endpoint hostname
- [ ] Create DNS zone (Azure DNS or external)
- [ ] Add CNAME record: `www` → `your-endpoint.azureedge.net`
- [ ] Add CNAME/ALIAS record: `@` → `your-endpoint.azureedge.net`
- [ ] Update name servers (if using Azure DNS)
- [ ] Wait for DNS propagation (5-60 minutes)
- [ ] Verify DNS resolution with `nslookup`
- [ ] Re-run deployment

---

## 🆘 **Troubleshooting**

### **Common Issues**
1. **DNS Propagation**: Can take up to 48 hours globally
2. **CNAME at Root**: Some DNS providers don't support this
3. **TTL Too High**: Lower TTL for faster updates during setup
4. **Mixed Records**: Don't mix A and CNAME records for same name

### **Fallback Options**
- Deploy to `*.azureedge.net` first (works immediately)
- Add custom domain later after DNS is configured
- Use subdomain like `app.agilecloud.ai` if root domain is problematic

---

**Next Step**: Configure the DNS records, then re-run the deployment workflow.
