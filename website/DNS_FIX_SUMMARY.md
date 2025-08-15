# 🚨 DNS Configuration Fix for AgileCloud.ai

## **Problem Identified**
The deployment is failing because Azure cannot find DNS records for `agilecloud.ai` pointing to your CDN endpoint.

**Error**: `We couldn't find a DNS record for custom domain that points to endpoint`

---

## 🎯 **Immediate Solution Options**

### **Option 1: Quick Deploy Without Custom Domain (Recommended First)**

I've temporarily disabled the custom domain configuration in the GitHub workflow. You can now deploy immediately:

```bash
git add .
git commit -m "fix: temporarily disable custom domain for initial deployment"
git push origin main
```

**This will deploy to**: 
- Primary: Your Azure Storage endpoint  
- CDN: `https://agilecloud-website.azureedge.net`

### **Option 2: Configure DNS First (Complete Solution)**

1. **Get your CDN endpoint**:
   ```powershell
   # Run the DNS checker script
   .\website\check-dns.ps1
   ```

2. **Configure DNS records** with your domain registrar:
   - **CNAME Record**: `www` → `agilecloud-website.azureedge.net`
   - **CNAME/ALIAS Record**: `@` → `agilecloud-website.azureedge.net`

3. **Wait for DNS propagation** (5-60 minutes)

4. **Re-enable custom domain** in workflow and deploy again

---

## 🛠️ **Detailed DNS Configuration**

### **Using Your Domain Registrar**

Add these DNS records where you purchased `agilecloud.ai`:

| Record Type | Name | Value | TTL |
|-------------|------|-------|-----|
| CNAME | www | agilecloud-website.azureedge.net | 300 |
| CNAME | @ | agilecloud-website.azureedge.net | 300 |

**Note**: If your registrar doesn't support CNAME for root domain (@), use ALIAS or ANAME instead.

### **Using Azure DNS (Alternative)**

```bash
# Create Azure DNS zone
az network dns zone create \
  --resource-group agilecloud-rg \
  --name agilecloud.ai

# Create CNAME records
az network dns record-set cname set-record \
  --resource-group agilecloud-rg \
  --zone-name agilecloud.ai \
  --record-set-name www \
  --cname agilecloud-website.azureedge.net

# Get name servers to configure with your registrar
az network dns zone show \
  --resource-group agilecloud-rg \
  --name agilecloud.ai \
  --query "nameServers" -o table
```

---

## 🚀 **Step-by-Step Fix Process**

### **Phase 1: Deploy Without Custom Domain (5 minutes)**

1. **Deploy immediately** (custom domain disabled):
   ```bash
   git add .
   git commit -m "fix: deploy without custom domain - DNS configuration needed"
   git push origin main
   ```

2. **Access your site** at the CDN endpoint while configuring DNS

3. **Verify everything works** except custom domain

### **Phase 2: Configure DNS (15-60 minutes)**

1. **Run DNS checker**:
   ```powershell
   # Windows
   .\website\check-dns.ps1
   
   # Linux/Mac
   chmod +x website/check-dns.sh
   ./website/check-dns.sh
   ```

2. **Configure DNS records** as shown by the script

3. **Wait for propagation** and verify with:
   ```bash
   nslookup agilecloud.ai
   nslookup www.agilecloud.ai
   ```

### **Phase 3: Enable Custom Domain (5 minutes)**

1. **Re-enable custom domain** in workflow:
   ```yaml
   # Change this line in .github/workflows/deploy-website.yml
   if: false  # Change to: if: env.CUSTOM_DOMAIN
   ```

2. **Deploy with custom domain**:
   ```bash
   git add .
   git commit -m "feat: enable custom domain - DNS configured"
   git push origin main
   ```

3. **Verify** at https://agilecloud.ai

---

## 🔍 **Verification Commands**

### **Check DNS Configuration**
```bash
# Check if domain points to CDN
nslookup agilecloud.ai
nslookup www.agilecloud.ai

# Check CNAME records specifically  
dig CNAME agilecloud.ai
dig CNAME www.agilecloud.ai
```

### **Check Azure Resources**
```bash
# Get CDN endpoint
az cdn endpoint show \
  --name agilecloud-website \
  --profile-name agilecloud-cdn \
  --resource-group agilecloud-rg \
  --query "hostName" -o tsv

# Check custom domain status
az cdn custom-domain list \
  --endpoint-name agilecloud-website \
  --profile-name agilecloud-cdn \
  --resource-group agilecloud-rg -o table
```

---

## 📋 **Files Created for You**

- **`DNS_SETUP_GUIDE.md`**: Complete DNS configuration guide
- **`check-dns.ps1`**: PowerShell script to check and configure DNS
- **`check-dns.sh`**: Bash script for Linux/Mac
- **Modified workflow**: Temporarily disabled custom domain

---

## ⚡ **Quick Start (Recommended)**

**Right now, you can deploy immediately:**

```bash
git add .
git commit -m "fix: deploy without custom domain - will configure DNS separately"
git push origin main
```

**Then configure DNS in parallel while the site is live on the CDN endpoint.**

---

## 🎯 **Expected Timeline**

- **Immediate**: Deploy to CDN endpoint (working site)
- **15-60 minutes**: DNS propagation after configuration
- **Total**: 1-2 hours for complete custom domain setup

**Your consciousness computing platform will be live and accessible throughout this process!** 🚀
