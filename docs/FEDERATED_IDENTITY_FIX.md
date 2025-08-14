# 🔧 Azure Federated Identity Setup - Step by Step Fix

## Current Problem
The GitHub Actions workflow is failing with "Identity not found" because the federated credential is not properly configured in Azure.

## 🚨 Critical Fix Required

### Step 1: Create/Fix the App Registration

1. **Go to Azure Portal**: https://portal.azure.com
2. **Navigate to**: Azure Active Directory → App registrations
3. **Find your app**: `291122ee-4f43-4b21-a337-c4d6e2382c8e`
   - If it doesn't exist, you need to create it first

### Step 2: Configure Federated Credentials (CRITICAL)

1. **In your app registration**, go to: **Certificates & secrets** → **Federated credentials**
2. **Click "Add credential"**
3. **Configure exactly as follows**:

   **Federated credential scenario**: `GitHub Actions deploying Azure resources`
   
   **Organization**: `TheManInTheBox`
   
   **Repository**: `cx`
   
   **Entity type**: `Branch`
   
   **GitHub branch name**: `master`
   
   **Name**: `WebSite`

4. **This will automatically generate**:
   - **Issuer**: `https://token.actions.githubusercontent.com`
   - **Subject**: `repo:TheManInTheBox/cx:ref:refs/heads/master`
   - **Audience**: `api://AzureADTokenExchange`

### Step 3: Grant Subscription Permissions

1. **Go to**: Subscriptions → `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`
2. **Click**: Access control (IAM) → Add → Add role assignment
3. **Role**: Contributor
4. **Assign access to**: User, group, or service principal
5. **Select**: Search for `291122ee-4f43-4b21-a337-c4d6e2382c8e` or the app name
6. **Click**: Select → Review + assign

### Step 4: Alternative - Create New Service Principal

If the above doesn't work, create a new service principal with Azure CLI:

```bash
# Install Azure CLI first if you don't have it
# https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

# Login to Azure
az login

# Create service principal with federated credential
az ad sp create-for-rbac \
  --name "github-actions-agilecloud-website" \
  --role Contributor \
  --scopes /subscriptions/0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb \
  --json-auth

# Note the output - you'll need the clientId, tenantId, and subscriptionId
```

Then add the federated credential:

```bash
# Get the app ID from the previous command output
APP_ID="your-new-app-id"

# Add federated credential
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "WebSite",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:TheManInTheBox/cx:ref:refs/heads/master",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### Step 5: Update GitHub Secrets

If you created a new service principal, update the GitHub secrets:

1. Go to: https://github.com/TheManInTheBox/cx/settings/secrets/actions
2. Update:
   - **AZURE_CLIENT_ID**: `new-app-id`
   - **AZURE_TENANT_ID**: `7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127`
   - **AZURE_SUBSCRIPTION_ID**: `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`

## 🧪 Test the Fix

After completing the setup:

```powershell
gh workflow run deploy-website.yml
```

## 📞 Still Not Working?

If federated identity continues to fail, we can fall back to client secret authentication:

### Create Client Secret (Fallback Option)

1. **In your app registration**: Certificates & secrets → Client secrets
2. **Click "New client secret"**
3. **Description**: "GitHub Actions deployment"
4. **Expires**: 24 months
5. **Copy the secret value** (you won't see it again!)

6. **Create single GitHub secret**:
   - **Name**: `AZURE_CREDENTIALS`
   - **Value**: 
   ```json
   {
     "clientId": "291122ee-4f43-4b21-a337-c4d6e2382c8e",
     "clientSecret": "your-secret-value",
     "subscriptionId": "0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb",
     "tenantId": "7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127"
   }
   ```

7. **Update workflow** to use:
   ```yaml
   - name: Login to Azure
     uses: azure/login@v2
     with:
       creds: ${{ secrets.AZURE_CREDENTIALS }}
   ```

## 🎯 Next Steps

1. **Fix the federated credential configuration first** (recommended)
2. **Test the deployment**
3. **If still failing, use client secret fallback**
4. **Once working, configure DNS for agilecloud.ai domain**

The issue is definitely in the Azure configuration, not the GitHub Actions workflow.
