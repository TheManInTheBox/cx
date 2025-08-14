# DNS ### DNS Records to Add

**SOLUTION FOR "Record data is invalid" ERROR:**

Many registrars don't support CNAME for root domains. Use these A and CNAME records instead:

```
Record Type: A
Name: @ (or agilecloud.ai or root)
Value: 13.107.246.40
TTL: 300 seconds

Record Type: CNAME  
Name: www
Value: agilecloud-website.azureedge.net
TTL: 300 seconds
```

**Alternative if your registrar DOES support CNAME for root:**

```
Record Type: CNAME
Name: @ (or agilecloud.ai or root)
Value: agilecloud-website.azureedge.net
TTL: 300 seconds

Record Type: CNAME  
Name: www
Value: agilecloud-website.azureedge.net
TTL: 300 seconds
```or AgileCloud.ai Custom Domain

## 🌐 CRITICAL: DNS Setup Required

I've configured the custom domains in Azure CDN, but you need to set up DNS records with your domain registrar for agilecloud.ai to work.

### DNS Records to Add

**Option A: If your registrar supports CNAME for root domain:**

```
Record Type: CNAME
Name: @ (or agilecloud.ai or root)
Value: agilecloud-website.azureedge.net
TTL: 300 seconds

Record Type: CNAME  
Name: www
Value: agilecloud-website.azureedge.net
TTL: 300 seconds
```

**Option B: If you get "Record data is invalid" (most common):**

First, get the IP address of the CDN endpoint:
```powershell
nslookup agilecloud-website.azureedge.net
```

Then add these records:
```
Record Type: A
Name: @ (or agilecloud.ai or root)
Value: [IP address from nslookup]
TTL: 300 seconds

Record Type: CNAME
Name: www
Value: agilecloud-website.azureedge.net
TTL: 300 seconds
```

### Popular Domain Registrars Instructions

#### **GoDaddy**
1. Go to GoDaddy Domain Manager
2. Click "DNS" for agilecloud.ai
3. Add A record: @ → **13.107.246.40**
4. Add CNAME record: www → agilecloud-website.azureedge.net

#### **Namecheap**
1. Go to Domain List → Manage agilecloud.ai
2. Advanced DNS tab
3. Add A record: @ → **13.107.246.40**
4. Add CNAME: www → agilecloud-website.azureedge.net

#### **Cloudflare**
1. Go to DNS settings for agilecloud.ai
2. Add A record: agilecloud.ai → **13.107.246.40**
3. Add CNAME: www → agilecloud-website.azureedge.net

#### **Route53 (AWS)**
1. Go to Route53 Hosted Zones
2. Select agilecloud.ai zone
3. Create A record: agilecloud.ai → **13.107.246.40**
4. Create CNAME: www → agilecloud-website.azureedge.net

### Verification

After setting up DNS (can take 24-48 hours), verify with:

```bash
# Check DNS propagation
nslookup agilecloud.ai
nslookup www.agilecloud.ai

# Test website
curl -I https://agilecloud.ai
curl -I https://www.agilecloud.ai
```

### Timeline

- **DNS Records**: Set up immediately with your registrar
- **DNS Propagation**: 24-48 hours globally  
- **HTTPS Certificates**: Azure provisions automatically (up to 6 hours)
- **Full Custom Domain**: Available after DNS propagation + certificate provisioning

### Current Status

✅ **Azure CDN**: Custom domains configured  
✅ **HTTPS**: Enabled (certificates provisioning)  
⏳ **DNS**: Waiting for your registrar configuration  
⏳ **Live Domain**: Will work after DNS setup  

## 🚀 Your website will be live at https://agilecloud.ai once DNS is configured!
