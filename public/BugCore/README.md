# BUGCORE - Enterprise Defect Tracking & Slack ChatOps Platform

**BUGCORE** (BugCore) is a modern, modular ASP.NET Core MVC defect tracking platform translated from the legacy BugCoreBT architecture. It features a dual-tier Slack ChatOps integration, role-based access control (RBAC), multi-project hierarchy management, and multi-channel webhook dispatching.

---

## 🚀 Quick Start Guide

### Default Demo Credentials

| Role | Username | Password | Purpose |
| :--- | :--- | :--- | :--- |
| **Administrator** | `administrator` | `root_Password123!` | Access to Admin Governance Portal, Workspace OAuth & Channel Routing Matrix |
| **Developer / Employee** | `developer_hamza` | `Developer_123!` | Self-service Slack identity binding, personal alert rules, and issue triage |
| **Manager** | `manager` | `Manager_123!` | Project category configuration and version release milestone management |
| **Reporter** | `reporter` | `Reporter_123!` | Filing bug reports, uploading attachments, and monitoring issue progress |

> **Pro Tip:** In the live application header, use the top-right profile dropdown to switch between **Administrator** and **Developer** roles instantly without re-logging in.

---

## 🛡️ Dual-Tier Slack ChatOps Architecture

BUGCORE isolates **Organization Governance** from **Individual Employee Actions** for maximum security and role isolation.

### 1. ⚡ Employee ChatOps Workbench (`/Slack`)
* **Self-Service Identity Binding**: Link your personal Slack profile Member ID (`U08...`) to your BUGCORE user session.
* **Personal Notification Thresholds**: Mute low-priority defect broadcasts or set minimum severity levels (Immediate, High, Normal).
* **Slash Commands (`/bug`)**:
  * `/bug report [title]` — Opens an interactive modal in Slack to file a defect.
  * `/bug assign [BUG-ID]` — Assigns a defect directly to your linked Slack account.
  * `/bug status [BUG-ID] [Status]` — Transitions issue status (e.g., *Resolved*, *Feedback*).
* **Message Escalation**: Convert any Slack message into a BUGCORE defect ticket via Slack message shortcuts.

### 2. 🔐 Admin Governance Portal (`/Slack/Admin`)
* **Project-to-Channel Routing Matrix**: Map individual BUGCORE projects (*Core Software Suite*, *Mobile Companion App*) to specific Slack channels (`#dev-alerts`, `#mobile-bugs`).
* **Workspace OAuth Bot Scopes**: Verify active bot tokens (`chat:write`, `commands`, `channels:read`, `im:write`, `reactions:read`).
* **Security & HMAC Audit**: Validate inbound Slack webhooks using HMAC-SHA256 signature checks.
* **Test Dispatcher**: Trigger sample Block Kit webhooks to verify channel connectivity with 1-click.

---

## 💻 Running Locally

### Running with .NET 8 SDK (Local Machine)
If you have the .NET 8 SDK installed on your machine:
```bash
cd BugCore
dotnet restore
dotnet build
dotnet run --project src/BugCore.Web
```
The application will launch on `http://localhost:5000`.

### Running in Preview Sandbox (Node.js Engine)
In the preview environment, the application runs on **Port 3000** (`npm start`), serving the complete UI layout, role switcher, Slack ChatOps workbench, and API endpoints.

---

## 🌐 Production Deployment Guide for Organizations

When deploying BUGCORE for production use in your organization, follow these deployment strategies:

### Option 1: Containerized Deployment (Docker / AWS ECS / Google Cloud Run / Azure Container Apps)
BUGCORE is fully containerizable with a standard multi-stage .NET 8 Dockerfile:

```dockerfile
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY BugCore/ .
RUN dotnet restore src/BugCore.Web/BugCore.Web.csproj
RUN dotnet publish src/BugCore.Web/BugCore.Web.csproj -c Release -o /out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BugCore.Web.dll"]
```

### Option 2: Azure App Service or Windows Server IIS
* **Azure App Service**: Deploy `BugCore.Web.csproj` directly via GitHub Actions or Azure DevOps pipeline targeting `.NET 8 (LTS)`.
* **On-Premises IIS**: Install the [.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows Server, configure an IIS Application Pool set to **No Managed Code**, and point the site physical path to the published folder.

### Option 3: Linux VM (Ubuntu / Debian + Nginx Reverse Proxy)
1. Install `.NET 8 Runtime` on your server.
2. Publish output: `dotnet publish -c Release -o /var/www/bugcore`.
3. Configure `systemd` service (`/etc/systemd/system/bugcore.service`):
   ```ini
   [Unit]
   Description=BUGCORE ASP.NET Core Web App
   [Service]
   WorkingDirectory=/var/www/bugcore
   ExecStart=/usr/bin/dotnet /var/www/bugcore/BugCore.Web.dll
   Restart=always
   Environment=ASPNETCORE_ENVIRONMENT=Production
   [Install]
   WantedBy=multi-user.target
   ```
4. Setup **Nginx** reverse proxy to forward HTTPS traffic to `http://localhost:5000` and enable Let's Encrypt SSL.

---

## ⚙️ Organization Environment Variables & Configuration

For production deployment, override default settings in `appsettings.Production.json` or system Environment Variables:

| Environment Variable | Description | Example |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL / SQL Server Connection String | `Server=db.org.com;Database=BugcoreDb;User Id=app;Password=...` |
| `Slack__BotToken` | Organization Central Slack Bot User OAuth Token | `YOUR_SLACK_BOT_TOKEN_HERE` |
| `Slack__SigningSecret` | Slack Signing Secret for HMAC Verification | `e123456789abcdef...` |
| `Slack__AppId` | Organization Slack App ID | `A08SLACKAPPID` |
| `GitHub__WebhookSecret` | HMAC secret for GitHub Commit Linker | `gh_secret_123` |

---

## 🔐 Organization Slack App Setup Checklist

1. **Create Slack App**: Visit [api.slack.com/apps](https://api.slack.com/apps) and click **Create New App** -> **From an app manifest**.
2. **Paste App Manifest**: Copy the generated App Manifest JSON from BUGCORE Admin Governance (`/Slack/Admin`).
3. **Install to Workspace**: Click **Install to Workspace** and grant channel permissions.
4. **Copy Bot Token & Signing Secret**: Save Bot User OAuth Token and Signing Secret into your production deployment environment variables.
5. **Verify HTTPS**: Ensure your deployed URL has a valid SSL certificate (e.g., `https://bugcore.yourcompany.com`) as required by Slack for event subscriptions and Slash commands.

---

## 🤝 Need Help or Support?
* **Report a bug**: Navigate to `/Issue/Create` or type `/bug report` in Slack.
* **Manage channels**: Access `/Slack/Admin` as an Administrator.
* **Link Slack account**: Access `/Slack` as a Developer.
