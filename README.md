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
* [Deployment](#deployment)	
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

## Deployment

Deployment documentation will be added in a future release.

## Disclaimer

Cryoptix is provided for educational and research purposes only.

Cryptocurrency trading involves substantial financial risk. Users are responsible for evaluating and testing any trading strategies before using them with real funds.

The authors and contributors of Cryoptix are not responsible for any financial losses incurred through the use of this software.

## License

This project is licensed under the MIT License.
See the [LICENSE](https://github.com/grantcolley/cryoptix/blob/main/LICENSE) file for details.

## Roadmap

- Azure deployment
- Multiple exchanges
- Trading analytics
