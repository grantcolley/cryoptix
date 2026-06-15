# Cryoptix

![.NET](https://img.shields.io/badge/.NET-10-blue)
![React](https://img.shields.io/badge/React-19-61dafb)
![TypeScript](https://img.shields.io/badge/TypeScript-5-blue)
![License](https://img.shields.io/badge/license-MIT-green)

A cryptocurrency trading platform built with ASP.NET Core and React for running, monitoring, and visualizing automated trading strategies in real time.

## Features

- Real-time strategy monitoring
- Low-latency real-time trade and market data streaming via SignalR
- Interactive TradingView charts
- Auth0 authentication and authorization
- Role-based access control
- Configurable trading strategy execution

![Cryoptix UI](/readme-images/cryoptix-ui.png?raw=true "Cryoptixm UI")

#### Table of Contents
* [Features](#features)
* [Architecture](#architecture)
  * [Technologies](#technologies)
* [Getting Started](#getting-started)
  * [Prerequisites](#prerequisites)	
  * [Clone the Repository](#clone-the-repository)
  * [Install Dependencies](#install-dependencies)  
  * [Setting up Authentication](#setting-up-authentication)
    * [Create the Auth0 API](#create-the-auth0-api)	
    * [Create the Auth0 Application](#create-the-auth0-application)		
    * [Create an Auth0 Role](#create-an-auth0-role)
    * [Create an Auth0 User](#create-an-auth0-user)	
  * [Securing the ASP.NET Core Web API](#securing-the-aspnet-core-web-api)
  * [Securing the React Web Application](#securing-the-react-web-application)
  * [Running the Application](#running-the-application)
    * [Start the API](#start-the-api)
    * [Start the UI](#start-the-ui)
* [Running a Strategy](#running-a-strategy)
* [Azure Deployment](#azure-deployment)
  * [Deploying the Vite + React Web Application](#deploying-the-vite--react-web-application)
	   * [Azure Static Web Apps Configuration](#azure-static-web-apps-configuration)
	   * [Create an Azure Static Web App](#create-an-azure-static-web-app)
	   * [Add the Azure Static Web App URL to Auth0](#add-the-azure-static-web-app-url-to-auth0)
	   * [Deployment Using GitHub Actions for Azure Static Web App](#deployment-using-github-actions-for-azure-static-web-app)
	     * [GitHub Workflow for Azure Static Web App](#github-workflow-for-azure-static-web-app)
         * [Create GitHub Secrets and Variables for Azure Static Web App](#create-github-secrets-and-variables-for-azure-static-web-app)
         * [Run the Azure Static Web App workflow](#run-the-azure-static-web-apps-workflow)
  * [Deploying the ASP.NET Core Web API](#deploying-the-aspnet-core-web-api)
	   * [Azure App Service Configuration](#azure-app-service-configuration)
	   * [Create an Azure App Service](#create-an-azure-app-service)
	   * [Deployment Using GitHub Actions for Azure App Service](#deployment-using-github-actions-for-azure-app-service)
	     * [GitHub Workflow for Azure App Service](#github-workflow-for-azure-app-service)
         * [Create GitHub Secrets and Variables for Azure App Service](#create-github-secrets-and-variables-for-azure-app-service)
         * [Run the Azure App Service workflow](#run-the-azure-app-service-workflow)
* [Disclaimer](#disclaimer)
* [License](#license)
* [Roadmap](#roadmap)

## Architecture

Cryoptix consists of:

- ASP.NET Core Web API
- React + TypeScript SPA
- SignalR real-time communication
- TradingView chart integration
- Auth0 authentication

### Technologies

- .NET 10
- ASP.NET Core
- SignalR
- React
- TypeScript
- shadcn/ui
- TradingView Charting Library

## Getting Started

This section describes the steps to get Cryoptix running in a local development environment.

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- npm
- Auth0 account
- Exchange API credentials (optional)

### Clone the Repository

```
git clone https://github.com/grantcolley/cryoptix.git
cd cryoptix
```

### Install Dependencies

React UI
```
npm install
```

ASP.NET Web API
```
dotnet restore
```

### Setting up Authentication
Cryoptix uses Auth0 for authentication and authorization. The implementation is based on OAuth 2.0 and can be adapted to other compatible identity providers.

[Auth0](https://auth0.com/) offers a free tier and provides an easy-to-use dashboard for registering applications, managing roles, and creating users.

> [!TIP]
> Read [Securing ASP.Net Minimal Web APIs with Auth0](https://auth0.com/blog/securing-aspnet-minimal-webapis-with-auth0/)

#### Create the Auth0 API
Login to Auth0 and navigate to the Dashboard. In the sidebar select  **Applications > APIs**. Click on the *+ Create API* button and provide the name `Cryoptix.API`.

![Create Custom API](/readme-images/auth0-create-api.png?raw=true "Create Custom API")

In the Permissions tab, create a new permission `read:cryoptix-user`. Users will need to be assigned this permission to access the **Cryoptix API**.

![Create API Permission](/readme-images/auth0-create-api-permission.png?raw=true "Create API Permission")

#### Create the Auth0 Application
In the sidebar select  **Applications > Applications**. Click on the *+ Create Application* button, provide the name `Cryoptix`, and select *Single Page Web Application*.

![Create Custom Application](/readme-images/auth0-create-application.png?raw=true "Create Custom Application")

In the section *Application URIs*, add the default localhost `http://localhost:5173/`.

![Application URIs](/readme-images/auth0-create-application-urls.png?raw=true "Application URIs")

#### Create an Auth0 Role
In the sidebar select  **User Management > Roles**. Click on the *+ Create Role* button. Provide the name `cryoptix-user-read` and description `Cryoptix user read role`.

![Create Role](/readme-images/auth0-create-role.png?raw=true "Create Role")

In the *Permissions* tab, click `Assign Permissions`. Select **Cryoptix.API** from the drop down list, and select `read:cryoptix-user` to assign the permission to the role.

![Assign Permission](/readme-images/auth0-assign-permission.png?raw=true "Assign Permission")

Assign this role to any user who should have access to the Cryoptix application.

#### Create an Auth0 User
In the sidebar select  **User Management > Users**. Click on the *+ Create User* button and select *Create via UI*. Provide an email address and password.

![Create User](/readme-images/auth0-create-user.png?raw=true "Create User")

In the *Permissions* tab, click `Assign Permissions`. Select **Cryoptix.API** from the drop down list, and select `read:cryoptix-user` to assign the permission to the user.

![Assign Permission](/readme-images/auth0-assign-permission.png?raw=true "Assign Permission")

In the *Roles* tab, click `Assign Role`. Select `cryoptix-user-read` from the drop down list to assign the role to the user.

![Assign Role](/readme-images/auth0-assign-role.png?raw=true "Assign Role")

### Securing the ASP.NET Core Web API

In the `Cryoptix.Web.API` project, update `appsettings.json` with `Auth`, and `CorsOrigins` values.

```JSON
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext" ]
  },
  "AllowedHosts": "*",
  "Auth": {
    "Domain": "auth_domain", 👈
    "Audience": "auth_api_audience", 👈
    "Issuer": "auth_issuer", 👈
    "ClientIds": [
      "auth_application_client_id" 👈
    ]
  },
  "Credentials": {
    "AccountName": "your_account_name", 👈
    "ApiKey": "exchange_api_key", 👈
    "ApiSecret": "exchange_api_secret" 👈
  },
  "CorsOrigins": {
    "Policy": "local",
    "Urls": "http://localhost:5173" 👈
  },
  "StrategyChannelOptions": {
    "KlineCapacity": "1000",
    "TradeCapacity": "20000",
    "TradeFullMode": "2",
    "KlineFullMode": "2",
    "KlineBroadcastCapacity": "5000",
    "TradeBroadcastCapacity": "10000",
    "IndicatorsBroadcastCapacity": "5000",
    "SignalBroadcastCapacity": "5000",
    "KlineBroadcastFullMode": "2",
    "TradeBroadcastFullMode": "2",
    "IndicatorsBroadcastFullMode": "2",
    "SignalBroadcastFullMode": "2"
  }
}
```

### Securing the React Web Application

In the Cryoptix UI project, update `.env` with `Auth` configuration values shown below.

```
VITE_AUTH_DOMAIN=auth_domain   👈
VITE_AUTH_CLIENT_ID=auth_application_client_id   👈
VITE_AUTH_AUDIENCE=auth_api_audience   👈
VITE_API_ROUTE_STATUS=api/strategy/status
VITE_API_ROUTE_START=api/strategy/start
VITE_API_ROUTE_STOP=api/strategy/stop
VITE_API_ROUTE_UPDATE=api/strategy/update
VITE_API_ROUTE_SUBSCRIBE=api/strategy/subscribe
```

### Running the Application

#### Start the API

`dotnet run --project Cryoptix.Web.API`

#### Start the UI

`npm run dev`

The application will be available at:

`http://localhost:5173`

# Running a Strategy

Click the login icon in the upper-right corner of the application.

![Launch Page](/readme-images/cryoptix-ui-launch-page.png?raw=true "Launch Page")

You will be redirected to Auth0 to authenticate.

![Auth0 Login](/readme-images/cryoptix-auth0-login.png?raw=true "Auth0 Login")

After successful authentication, the navigation panel is displayed. Select a Strategy from the menu.

Enter the server URL and click the **Connect to Server** button.

![Connect to server](/readme-images/cryoptix-ui-connect-to-server.png?raw=true "Connect to server")

Select a strategy from the dropdown list.

![Select strategy](/readme-images/cryoptix-ui-select-strategy.png?raw=true "Select strategy")

Click **Start Strategy** to begin execution.

![Cryoptix UI](/readme-images/cryoptix-ui.png?raw=true "Cryoptix UI")

# Azure Deployment

Create an Azure account using the free tier.

## Deploying the Vite + React Web Application

### Azure Static Web Apps Configuration

The file `public/staticwebapp.config.json` enables client-side routing for the React application.

Without this configuration, refreshing or directly navigating to a route such as `/strategies/foo` would return a 404 from Azure Static Web Apps. The `navigationFallback` rule ensures that unmatched routes are rewritten to `index.html`, allowing React Router to handle navigation.

```JSON
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "/icons/*", "/favicon.ico", "/favicon.svg", "/manifest.webmanifest"]
  },
  "mimeTypes": {
    ".webmanifest": "application/manifest+json"
  }
}
```

### Create an Azure Static Web App

Create an Azure Static Web App using the following settings:

- Hosting Plan: `Free`
- Deployment Source: `Other`

### Add the Azure Static Web App URL to Auth0

Log in to Auth0 and open the Cryoptix application configuration.

Under Application URIs, add the Azure Static Web App URL.

> [!IMPORTANT]
>
> Failing to add the Azure Static Web App URL will prevent users from authenticating successfully.

### Deployment Using GitHub Actions for Azure Static Web App

#### GitHub Workflow for Azure Static Web App

The workflow file `.github/workflows/azure-static-web-apps.yml` is configured for manual deployment and is automatically available in GitHub Actions.

```YAML
name: Azure Static Web Apps

on:
  workflow_dispatch:

jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    name: Build and deploy
    env:
      VITE_AUTH_DOMAIN: ${{ vars.VITE_AUTH_DOMAIN }}
      VITE_AUTH_CLIENT_ID: ${{ vars.VITE_AUTH_CLIENT_ID }}
      VITE_AUTH_AUDIENCE: ${{ vars.VITE_AUTH_AUDIENCE }}
      VITE_API_ROUTE_STATUS: ${{ vars.VITE_API_ROUTE_STATUS }}
      VITE_API_ROUTE_START: ${{ vars.VITE_API_ROUTE_START }}
      VITE_API_ROUTE_STOP: ${{ vars.VITE_API_ROUTE_STOP }}
      VITE_API_ROUTE_UPDATE: ${{ vars.VITE_API_ROUTE_UPDATE }}
      VITE_API_ROUTE_SUBSCRIBE: ${{ vars.VITE_API_ROUTE_SUBSCRIBE }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Build and deploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          action: upload
          app_location: src/gui
          api_location: ""
          output_location: dist
          app_build_command: npm run build
```

#### Create GitHub Secrets and Variables for Azure Static Web App

Navigate to:

GitHub → Repository → Settings → Secrets and variables → Actions

Create the following repository secret:

| Secret Name | 
| --- |
| ``AZURE_STATIC_WEB_APPS_API_TOKEN`` | 

Copy the Azure Static Web App deployment token into this secret.

Create the following repository variables:

| Variable Name | Value |
| --- | --- |
| ``VITE_AUTH_DOMAIN`` | *your_auth0_domain* |
| ``VITE_AUTH_CLIENT_ID`` | *your_auth0_app_client_id* |
| ``VITE_AUTH_AUDIENCE`` | *your_auth0_api_audience* |
| ``VITE_API_ROUTE_STATUS`` | api/strategy/status |
| ``VITE_API_ROUTE_START`` | api/strategy/start |
| ``VITE_API_ROUTE_STOP`` | api/strategy/stop |
| ``VITE_API_ROUTE_UPDATE`` | api/strategy/update |
| ``VITE_API_ROUTE_SUBSCRIBE`` | api/strategy/subscribe |

#### Run the Azure Static Web Apps workflow

1. Open **GitHub Actions**
2. Select **Azure Static Web Apps**
3. Click **Run workflow**
4. Select the target branch.
5. Click **Run workflow**.

GitHub Actions will build, publish, configure, and deploy the React SPA application to Azure Static Web Apps.

![GitHub Action Azure Static Web App](/readme-images/github-action-azure-static-web-apps.png?raw=true "GitHub Action Azure Static Web App")

> [!TIP]
> 
> How GitHub Actions + Vite interact
> 	
> **1. GitHub Actions starts a workflow**
> 
> GitHub provisions a clean Linux build environment and checks out the repository.
> 
> The environment contains:
> - Node.js
> - Repository source code
> - Repository secrets
> - Repository variables
> 
> **2. GitHub Actions Provides Environment Variables**
> 
> The workflow injects the configured variables into the build environment:
> 
> ```yaml
> env:
>   VITE_AUTH_DOMAIN: ${{ secrets.VITE_AUTH_DOMAIN }}
>   VITE_AUTH_CLIENT_ID: ${{ secrets.VITE_AUTH_CLIENT_ID }}
> ```
> 
> **3. Vite Builds the Application**
> 
> GitHub Actions executes:
> 
> `npm run build`
> 
> During the build, Vite:
> -Reads environment variables
> -Exposes them through `import.meta.env`
> -Executes configuration validation
> -Produces the final static assets in `dist/`
> 
> **4. Azure Receives the Built Application**
> 
> The deployment step uploads the generated `dist/` folder to Azure Static Web Apps.
>
> Azure only receives the final compiled static assets and does not execute the application build process itself.

## Deploying the ASP.NET Core Web API

### Azure App Service Configuration

GitHub Actions must authenticate to Azure before deploying resources. Microsoft recommends using OpenID Connect (OIDC), which allows GitHub Actions to authenticate without storing long-lived client secrets.

The authentication flow is:

```text
GitHub Actions
    ↓
Requests short-lived identity token from GitHub
    ↓
Azure validates the token
    ↓
Azure issues temporary access token
    ↓
Deploys to Azure App Service
```

Before deploying, create:

- Microsoft Entra ID App Registration
- Federated Credential

<details>
<summary>Configure GitHub OIDC for Azure App Service deployments</summary>

### Step 1: Create an App Registration

1. Open the Azure Portal.
2. Navigate to **Microsoft Entra ID**.
3. Select **App registrations**.
4. Click **New registration**.
5. Enter a name, for example:

```text
github-cryoptix-api-deploy
```

6. Leave the account type as:

```text
Accounts in this organizational directory only
```

7. Click **Register**.

After creation, note the following values:

- Application (Client) ID
- Directory (Tenant) ID

These values will be required later.

### Step 2: Create a Federated Credential

Within the App Registration:

1. Select **Certificates & secrets**.
2. Open the **Federated credentials** tab.
3. Click **Add credential**.

Select:

```text
Federated credential scenario:
GitHub Actions deploying Azure resources
```

Provide:

- GitHub Organization/User
- Repository Name
- Entity Type: `Branch`
- Branch: `main`
- Credential Name (for example `github-cryoptix-main`)

Click **Add**.

Azure will now trust workflows running from the specified repository and branch.

### Step 3: Assign Azure Permissions

The App Registration exists but cannot deploy resources until permissions are granted.

Recommended scope: Resource Group

1. Open the Resource Group.
2. Select **Access Control (IAM)**.
3. Click **Add Role Assignment**.
4. Choose the **Contributor** role.
5. Select **User, group, or service principal**.
6. Search for the App Registration created earlier.
7. Complete the assignment.

### Step 4: Obtain the Subscription ID

Navigate to:

```text
Subscriptions → Your Subscription
```

Copy the **Subscription ID**.

</details>

### Create an Azure App Service

Cryoptix is designed to run on Azure App Service using the **B1 (Basic)** pricing tier, which supports:

- Always On
- WebSockets
- Background Hosted Services
- SignalR
- Continuous real-time processing

#### 1. Create the App Service

In the Azure Portal:

- Create a new **App Service**
- Select a Resource Group (for example `Cryoptix-RG`)
- Specify an App Service name (for example `Cryoptix-API`)
- Runtime Stack: `.NET 10`
- Region: Closest to your users
- Pricing Plan: `B1 (Basic)`

Click **Review + Create**, then **Create**.

#### 2. Enable WebSockets

Within the App Service:

```text
Configuration → General Settings → WebSockets
```

Enable WebSockets and restart the application.

> [!IMPORTANT]
>
> WebSockets must be enabled for SignalR and real-time exchange connectivity.

### Deployment Using GitHub Actions for Azure App Service

#### GitHub Workflow for Azure App Service

The workflow file `.github/workflows/azure-app-service.yml` is configured for manual deployment and is automatically available in GitHub Actions.

```YAML
name: Azure App Service

on:
  workflow_dispatch:

permissions:
  contents: read
  id-token: write

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore src/api/Cryoptix.Web.API/Cryoptix.Web.API.csproj

      - name: Build
        run: dotnet build src/api/Cryoptix.Web.API/Cryoptix.Web.API.csproj --configuration Release --no-restore

      - name: Publish
        run: dotnet publish src/api/Cryoptix.Web.API/Cryoptix.Web.API.csproj --configuration Release -o ./publish --no-build

      - name: Login to Azure using OIDC
        uses: azure/login@v2
        with:
          client-id: ${{ vars.AZURE_CLIENT_ID }}
          tenant-id: ${{ vars.AZURE_TENANT_ID }}
          subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}

      - name: Configure App Service application settings
        env:
          AZURE_RESOURCE_GROUP: ${{ vars.AZURE_RESOURCE_GROUP }}
          AZURE_WEBAPP_NAME: ${{ vars.AZURE_WEBAPP_NAME }}
        run: |
          set -e

          # Map configuration keys to App Settings using __ to represent : in IConfiguration
          az webapp config appsettings set --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_WEBAPP_NAME" --settings \
            Auth__Domain="${{ vars.AUTH_DOMAIN }}" \
            Auth__Audience="${{ vars.AUTH_AUDIENCE }}" \
            Auth__Issuer="${{ vars.AUTH_ISSUER }}" \
            Auth__ClientIds__0="${{ secrets.AUTH_CLIENTID_0 }}" \
            Credentials__AccountName="${{ secrets.CREDENTIALS_ACCOUNT_NAME }}" \
            Credentials__ApiKey="${{ secrets.CREDENTIALS_API_KEY }}" \
            Credentials__ApiSecret="${{ secrets.CREDENTIALS_API_SECRET }}" \
            CorsOrigins__Policy="${{ vars.CORS_POLICY }}" \
            CorsOrigins__Urls="${{ vars.CORS_URLS }}"

      - name: Deploy to Azure Web App
        env:
          AZURE_RESOURCE_GROUP: ${{ vars.AZURE_RESOURCE_GROUP }}
          AZURE_WEBAPP_NAME: ${{ vars.AZURE_WEBAPP_NAME }}
        run: |
          az webapp deploy --resource-group "$AZURE_RESOURCE_GROUP" --name "$AZURE_WEBAPP_NAME" --src-path ./publish

      - name: Logout of Azure
        run: az logout || true
```

#### Create GitHub Secrets and Variables for Azure App Service

Navigate to:

GitHub → Repository → Settings → Secrets and variables → Actions

Create the following repository secrets:

| Secret Name | Value | 
| --- | --- |
| ``CREDENTIALS_ACCOUNT_NAME`` | *your_exchange_account_name* |
| ``CREDENTIALS_API_KEY`` | *your_exchange_account_api_key* |
| ``CREDENTIALS_API_SECRET`` | *your_exchange_account_api_secret* |
| ``AUTH_CLIENTID_0`` | *your_auth0_client_ids* |

Create the following repository variables:

| Variable Name | Value |
| --- | --- |
| ``AUTH_DOMAIN`` | *your_auth0_domain* |
| ``AUTH_AUDIENCE`` | *your_auth0_api_audience* |
| ``AUTH_ISSUER`` | *your_auth0_api_issuer* |
| ``AZURE_RESOURCE_GROUP`` | *your_azure_resource_group* |
| ``AZURE_WEBAPP_NAME`` | *your_azure_webb_app_name* |
| ``AZURE_CLIENT_ID`` | *your_azure_client_id* |
| ``AZURE_TENANT_ID`` | *your_azure_tenant_id* |
| ``AZURE_SUBSCRIPTION_ID`` | *your_azure_subscription_id* |
| ``CORS_POLICY`` | *cryoptix-cors-policy* |
| ``CORS_URLS`` | *your_azure_static_web_app_url* |

#### Run the Azure App Service Workflow

1. Open **GitHub Actions**.
2. Select **Azure App Service**.
3. Click **Run workflow**.
4. Select the target branch.
5. Click **Run workflow**.

GitHub Actions will build, publish, configure, and deploy the ASP.NET Core Web API to Azure App Service.

![GitHub Action Azure App Service](/readme-images/github-action-azure-app-service.png?raw=true "GitHub Action Azure App Service")

## Disclaimer

Cryoptix is provided for educational and research purposes only.

Cryptocurrency trading involves substantial financial risk. Users are responsible for evaluating and testing any trading strategies before using them with real funds.

The authors and contributors of Cryoptix are not responsible for any financial losses incurred through the use of this software.

## License

This project is licensed under the MIT License.
See the [LICENSE](https://github.com/grantcolley/cryoptix/blob/main/LICENSE) file for details.

## Roadmap

- Azure deployment
- Support multiple exchanges
- Update strategy parameters in realtime
- Trading analytics
- Order execution
