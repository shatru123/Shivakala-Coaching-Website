# SmarterASP.NET Deployment Guide

This app is now prepared for a low-friction SmarterASP.NET deployment with:

- `shivkalaclasses.com` → ASP.NET Core MVC site
- SmarterASP SQL Server → primary database
- `wa.shivkalaclasses.com` → WhatsApp Node.js sidecar

## Recommended Layout

| Component | Target |
|--------|--------|
| MVC app | SmarterASP.NET ASP.NET Core site |
| Database | SmarterASP SQL Server |
| Uploads | `wwwroot/uploads` inside the MVC site |
| Data protection keys | `App_Data/DataProtection-Keys` |
| WhatsApp sidecar | Separate SmarterASP Node.js site / subdomain |
| WhatsApp auth session | `.wwebjs_auth` inside the sidecar site |

## 1. Create the SmarterASP Resources

1. Create the main ASP.NET Core hosting plan for `shivkalaclasses.com`
2. Create the SQL Server database from the SmarterASP control panel
3. Create a second Node.js site or subdomain for WhatsApp, for example `wa.shivkalaclasses.com`
4. Note the Web Deploy details for both sites:
   - service URL
   - site/application name
   - username
   - password

## 2. Configure the MVC App

Set these values in SmarterASP app settings or in `src/Shivakala.Web/appsettings.Production.json` before publish:

```json
{
  "Database": {
    "Provider": "SqlServer"
  },
  "ConnectionStrings": {
    "SqlServer": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "WhatsApp": {
    "BaseUrl": "https://wa.shivkalaclasses.com",
    "ApiKey": "set-a-strong-shared-secret"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "change-this-before-first-deploy"
  }
}
```

Notes:

- The app defaults to `SqlServer` in `Production`
- Login cookie keys now persist in `App_Data/DataProtection-Keys`, so app pool recycles do not log everyone out
- Uploads remain on the site filesystem; do not delete `wwwroot/uploads` during deploys

## 3. Configure the WhatsApp Sidecar

The sidecar is now designed to run remotely instead of only on `localhost`.

### Required files

Deploy everything under `whatsapp-sidecar/`, including:

- `server.js`
- `package.json`
- `web.config`

### `web.config` placeholders

`whatsapp-sidecar/web.config` ships with placeholders:

- `__WHATSAPP_API_KEY__`
- `__WHATSAPP_AUTH_PATH__`
- `__PUPPETEER_EXECUTABLE_PATH__`

For manual deployment, replace them before upload:

- `__WHATSAPP_API_KEY__` → same secret used in the MVC app
- `__WHATSAPP_AUTH_PATH__` → leave blank to use the default local `.wwebjs_auth` folder, or set an absolute folder if SmarterASP support gives you one
- `__PUPPETEER_EXECUTABLE_PATH__` → leave blank first; only set this if SmarterASP support gives you a working Chrome/Chromium path

The sidecar treats any untouched `__PLACEHOLDER__` value as empty, so it is safe to deploy before final tuning.

## 4. Publish Order

Use this order the first time:

1. Deploy the Node.js WhatsApp sidecar
2. Confirm `https://wa.shivkalaclasses.com/status` responds
3. Deploy the ASP.NET Core MVC site
4. Browse to `https://shivkalaclasses.com/admin/whatsapp`
5. Scan the QR code from the admin screen

## 5. SQL Server Migrations

Before first production traffic, apply SQL Server migrations against the SmarterASP connection string:

```bash
dotnet ef database update --project src/Shivakala.SqlServerMigrations --startup-project src/Shivakala.Web -- --provider=SqlServer
```

If you publish directly from Visual Studio or GitHub Actions, run the migration command from a secure machine against the hosted SQL Server database before browsing the live site.

## 6. GitHub Actions CI/CD

This repo includes `.github/workflows/ci-cd.yml`.

### Required web app secrets

- `SMARTERASP_WEBDEPLOY_SERVICE_URL`
- `SMARTERASP_WEB_SITE_NAME`
- `SMARTERASP_WEBDEPLOY_USERNAME`
- `SMARTERASP_WEBDEPLOY_PASSWORD`

### Required Node sidecar secrets

- `SMARTERASP_NODE_WEBDEPLOY_SERVICE_URL`
- `SMARTERASP_NODE_SITE_NAME`
- `SMARTERASP_NODE_WEBDEPLOY_USERNAME`
- `SMARTERASP_NODE_WEBDEPLOY_PASSWORD`

### Optional sidecar secrets

- `WHATSAPP_API_KEY`
- `WHATSAPP_AUTH_PATH`
- `PUPPETEER_EXECUTABLE_PATH`

If the Web Deploy secrets are missing, the deploy jobs skip automatically and the build job still passes.

## 7. DNS for `shivkalaclasses.com`

Recommended DNS layout:

- `shivkalaclasses.com` → main MVC site
- `www.shivkalaclasses.com` → optional alias to the main site
- `wa.shivkalaclasses.com` → Node.js WhatsApp sidecar

Point each hostname exactly as SmarterASP asks in the domain manager.

## 8. WhatsApp Reliability Notes

The WhatsApp broadcast feature depends on `whatsapp-web.js`, so keep these expectations in mind:

- the phone must stay signed in and internet-connected
- the sidecar must remain deployed as its own Node.js site
- the `.wwebjs_auth` folder must not be deleted during deploys
- if SmarterASP shared Node hosting blocks Chromium/Puppeteer, move only the sidecar to a small VPS while keeping the main site on SmarterASP

That last fallback still works because the MVC app now uses `WhatsApp__BaseUrl` instead of a hardcoded `localhost` URL.
