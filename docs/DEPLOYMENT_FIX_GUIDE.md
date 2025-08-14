# 🚀 AgileCloud.ai Deployment Fix Guide

## Current Issue
The GitHub Actions workflow is failing with "No subscriptions found" error, indicating the service principal doesn't have proper access to the Azure subscription.

## ✅ What's Already Configured
- ✅ GitHub secrets are set: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- ✅ Federated credential exists: "WebSite" 
- ✅ Application ID: 291122ee-4f43-4b21-a337-c4d6e2382c8e
- ✅ Tenant ID: 7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127
- ✅ Subscription ID: 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb

## 🔧 Required Fixes

### 1. Fix Federated Credential Configuration
Go to Azure Portal and ensure your federated credential has these EXACT settings:

**Azure Portal** → **Azure Active Directory** → **App registrations** → **Your App (291122ee-4f43-4b21-a337-c4d6e2382c8e)** → **Certificates & secrets** → **Federated credentials** → **WebSite**

Update the credential with:
- **Federated credential scenario**: GitHub Actions deploying Azure resources
- **Organization**: `TheManInTheBox`
- **Repository**: `cx`
- **Entity type**: `Branch`
- **GitHub branch name**: `master`

This will generate:
- **Issuer**: `https://token.actions.githubusercontent.com`
- **Subject**: `repo:TheManInTheBox/cx:ref:refs/heads/master`
- **Audience**: `api://AzureADTokenExchange`

### 2. Grant Subscription Permissions
The service principal needs Contributor access to your subscription.

**Option A: Via Azure Portal**
1. Go to **Subscriptions** → **0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb** → **Access control (IAM)**
2. Click **+ Add** → **Add role assignment**
3. Select **Contributor** role
4. Select **User, group, or service principal**
5. Search for your app: `291122ee-4f43-4b21-a337-c4d6e2382c8e`
6. Select it and click **Save**

**Option B: Via Azure CLI** (if you have it installed)
```bash
az login
az account set --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb
az role assignment create \
  --assignee 291122ee-4f43-4b21-a337-c4d6e2382c8e \
  --role Contributor \
  --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb
```

### 3. Test the Deployment
After making these changes, trigger the workflow again:

```powershell
gh workflow run deploy-website.yml
```

## 🌐 Expected Result
Once the permissions are fixed, the workflow should:

1. ✅ Login to Azure successfully
2. ✅ Create resource group: `agilecloud-rg`
3. ✅ Create storage account: `agilecloudwebsite`
4. ✅ Enable static website hosting
5. ✅ Upload website files
6. ✅ Create CDN profile and endpoint
7. ✅ Configure custom domain: `agilecloud.ai`

## 📋 Next Steps After Deployment
1. Configure DNS CNAME record: `agilecloud.ai` → CDN endpoint
2. Enable HTTPS certificate in Azure CDN
3. Test the website at https://agilecloud.ai

## 🆘 Alternative: Manual Deployment
If the GitHub Actions continues to fail, you can deploy manually using:

```powershell
# Install Azure CLI first: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
.\deploy-website.ps1
```

## 📞 Support
If you continue having issues:
1. Check the GitHub Actions logs for specific error messages
2. Verify the federated credential configuration matches exactly
3. Ensure you have Owner or User Access Administrator role on the subscription to grant permissions
