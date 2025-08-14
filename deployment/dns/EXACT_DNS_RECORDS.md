# 🎯 EXACT DNS RECORDS FOR AGILECLOUD.AI

## ✅ Solution for "Record data is invalid"

Your registrar doesn't support CNAME for root domains (this is normal). Use these exact records:

### DNS Records to Add:

```
Record Type: A
Name: @ (or leave blank for root)
Value: 13.107.246.40
TTL: 300

Record Type: CNAME
Name: www
Value: agilecloud-website.azureedge.net
TTL: 300
```

### Quick Steps:
1. **Go to your domain registrar's DNS management**
2. **Delete any existing A or CNAME records for @ and www**
3. **Add the A record above for the root domain**
4. **Add the CNAME record above for www**
5. **Save changes**

### What This Does:
- ✅ **agilecloud.ai** → Points to Azure CDN IP address
- ✅ **www.agilecloud.ai** → Points to Azure CDN hostname  
- ✅ **Both URLs will work** after DNS propagation

### Timeline:
- **Now**: Add the DNS records
- **1-6 hours**: DNS starts working
- **24-48 hours**: DNS fully propagated globally

### Test When Ready:
```powershell
.\verify-dns.ps1
```

## 🚀 Your custom domain will be live once you add these two records!
