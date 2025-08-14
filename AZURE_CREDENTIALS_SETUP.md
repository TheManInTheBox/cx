# Azure Credentials Setup for GitHub Actions

## Federated Identity Configuration (Recommended)

You have already set up a federated credential for GitHub Actions. This is the most secure approach.

### Your Configuration Details:
- **Tenant ID (Directory ID)**: `7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127`
- **Application ID (Client ID)**: `291122ee-4f43-4b21-a337-c4d6e2382c8e`
- **Federated Credential**: `WebSite`
- **Subscription ID**: `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`

### ⚠️ IMPORTANT: Federated Credential Configuration

The federated credential must be configured with these exact settings:

1. **Issuer**: `https://token.actions.githubusercontent.com`
2. **Subject**: `repo:TheManInTheBox/cx:ref:refs/heads/master`
3. **Audience**: `api://AzureADTokenExchange`

### To verify/update your federated credential:

1. Go to Azure Portal → Azure Active Directory → App registrations
2. Find your application: `291122ee-4f43-4b21-a337-c4d6e2382c8e`
3. Go to **Certificates & secrets** → **Federated credentials**
4. Edit the "WebSite" credential and ensure it has:
   - **Federated credential scenario**: GitHub Actions deploying Azure resources
   - **Organization**: `TheManInTheBox`
   - **Repository**: `cx`
   - **Entity type**: Branch
   - **GitHub branch name**: `master`

### Required GitHub Secrets for Federated Identity:

Instead of AZURE_CREDENTIALS, you need these individual secrets:

### Required GitHub Secrets for Federated Identity:

Instead of AZURE_CREDENTIALS, you need these individual secrets:

1. **AZURE_CLIENT_ID**: `291122ee-4f43-4b21-a337-c4d6e2382c8e`
2. **AZURE_TENANT_ID**: `7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127`
3. **AZURE_SUBSCRIPTION_ID**: `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`

### How to Add GitHub Secrets:

1. Go to your GitHub repository: https://github.com/TheManInTheBox/cx
2. Click on **Settings** → **Secrets and variables** → **Actions**
3. Add these three secrets:

   - **Name**: `AZURE_CLIENT_ID`
     **Value**: `291122ee-4f43-4b21-a337-c4d6e2382c8e`

   - **Name**: `AZURE_TENANT_ID`
     **Value**: `7dd8cf8b-3a69-4cb3-96c9-0a9e63fe6127`

   - **Name**: `AZURE_SUBSCRIPTION_ID`
     **Value**: `0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb`

### Benefits of Federated Identity:
- ✅ **No client secrets to manage** - more secure
- ✅ **Automatic credential rotation** - Azure handles it
- ✅ **Scoped to specific GitHub repo and branch** - more secure
- ✅ **No expiration concerns** - credentials don't expire

### Legacy Method: Service Principal with Client Secret

If you prefer to use the traditional approach with client secrets:

### Step 3: Verify Permissions

Make sure the service principal has the following permissions:
- **Contributor** role on the subscription
- Ability to create resource groups
- Ability to create storage accounts
- Ability to create CDN profiles and endpoints

### Alternative: Use Azure CLI Login

If you prefer, you can also use Azure CLI to login locally and then export the credentials:

```bash
# Login to Azure
az login

# Set the subscription
az account set --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb

# Create the service principal (replace with your GitHub repo)
az ad sp create-for-rbac --name "github-actions-agilecloud" \
  --role contributor \
  --scopes /subscriptions/0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb \
  --sdk-auth
```

### Security Notes

- The service principal should only have the minimum required permissions
- Consider using environment-specific service principals for production vs. development
- Regularly rotate the client secret for security
- Monitor the service principal usage in Azure Active Directory

### Troubleshooting

If the deployment fails with "No subscriptions found" or login errors:

1. **Check Federated Credential Configuration**:
   - Verify the subject claim matches exactly: `repo:TheManInTheBox/cx:ref:refs/heads/master`
   - Ensure the issuer is: `https://token.actions.githubusercontent.com`
   - Confirm the audience is: `api://AzureADTokenExchange`

2. **Verify Service Principal Permissions**:
   ```bash
   # Check if the service principal has access to the subscription
   az role assignment list --assignee 291122ee-4f43-4b21-a337-c4d6e2382c8e --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb
   ```

3. **Add Subscription Access**:
   ```bash
   # Grant Contributor role to the service principal
   az role assignment create \
     --assignee 291122ee-4f43-4b21-a337-c4d6e2382c8e \
     --role Contributor \
     --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb
   ```

4. **Common Issues**:
   - The application must have permission to the specific subscription
   - The federated credential entity type must match (Branch vs Environment)
   - GitHub repository name and organization must be exact matches
   - Branch name in federated credential must match the branch the workflow runs on

### Manual Fix for Current Error:

Run these commands in Azure CLI to fix the permission issue:

```bash
# Login to Azure
az login

# Set the subscription
az account set --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb

# Grant Contributor role to your service principal
az role assignment create \
  --assignee 291122ee-4f43-4b21-a337-c4d6e2382c8e \
  --role Contributor \
  --subscription 0ae2be9a-f470-4dfe-b2e0-b7e9726acdfb
```

### Manual Alternative

If you can't set up the GitHub secret, you can use the PowerShell deployment script locally:

```powershell
.\deploy-website.ps1
```

This will deploy using your local Azure CLI authentication.
