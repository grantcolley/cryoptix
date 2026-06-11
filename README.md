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
	   * [Deployment Options](#deployment-options)
	     * [Manual Deployment Using Azure CLI](#manual-deployment-using-azure-cli)
	     * [Deployment Using GitHub Actions](#deployment-using-github-actions)
	       * [GitHub Workflow](#github-workflow)
        * [Create GitHub Secrets and Variables](#create-github-secrets-and-variables)
        * [Run the Azure Static Web Apps workflow](#run-the-azure-static-web-apps-workflow)
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

> [!NOTE]
> 
>This section covers deployment of the React UI to Azure Static Web Apps.
Deployment guidance for the ASP.NET Core Web API will be added in a future release.

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

> [!WARNING]
>
> Failing to add the Azure Static Web App URL will prevent users from authenticating successfully.

### Deployment Options

#### Manual Deployment Using Azure CLI

Documentation for manual deployment using the Azure CLI will be added in a future release.

#### Deployment Using GitHub Actions

##### GitHub Workflow

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

##### Create GitHub Secrets and Variables

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

**How GitHub Actions + Vite interact:**

**1. GitHub Actions starts a workflow**

GitHub provisions a clean Linux build environment and checks out the repository.

The environment contains:
- Node.js
- Repository source code
- Repository secrets
- Repository variables

**2. GitHub Actions Provides Environment Variables**

The workflow injects the configured variables into the build environment:

```yaml
env:
  VITE_AUTH_DOMAIN: ${{ secrets.VITE_AUTH_DOMAIN }}
  VITE_AUTH_CLIENT_ID: ${{ secrets.VITE_AUTH_CLIENT_ID }}
```

**3. Vite Builds the Application**

GitHub Actions executes:

`npm run build`

During the build, Vite:
-Reads environment variables
-Exposes them through `import.meta.env`
-Executes configuration validation
-Produces the final static assets in `dist/`

**4. Azure Receives the Built Application**

The deployment step uploads the generated `dist/` folder to Azure Static Web Apps.

Azure only receives the final compiled static assets and does not execute the application build process itself.

##### Run the Azure Static Web Apps workflow

Open GitHub Actions, select Azure Static Web Apps, and click Run workflow.

![GitHub Action Azure Static Web App](/readme-images/github-action-azure-static-web-apps.png?raw=true "GitHub Action Azure Static Web App")
 
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
