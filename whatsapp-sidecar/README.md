# WhatsApp Sidecar

Free WhatsApp broadcasting powered by [whatsapp-web.js](https://wwebjs.dev/).

## Setup

```bash
cd whatsapp-sidecar
npm install
npm start
```

Open the admin panel → **WhatsApp** → scan the QR with WhatsApp on your phone.

Once authenticated the session is persisted in `.wwebjs_auth/` — you won't need
to re-scan unless you log out on the phone.

### Environment Variables

| Variable | Purpose | Default |
|--------|---------|---------|
| `PORT` | HTTP port | `3500` |
| `WHATSAPP_API_KEY` | Optional shared secret expected in `X-Api-Key` | empty |
| `WHATSAPP_AUTH_PATH` | Persisted WhatsApp login session folder | `.wwebjs_auth` under the app root |
| `PUPPETEER_EXECUTABLE_PATH` | Optional Chrome/Chromium path for hosted environments | auto |
| `WHATSAPP_PUPPETEER_HEADLESS` | Headless browser toggle | `true` |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET  | /qr       | Returns QR PNG (base64) or `{ authenticated: true }` |
| GET  | /status   | Session health |
| POST | /send     | `{ mobile, message }` – single message |
| POST | /broadcast | `{ mobiles[], message }` – bulk send |

## Production

Run with PM2:
```bash
npm install -g pm2
pm2 start server.js --name wa-sidecar
pm2 save && pm2 startup
```

### Render Hosting

Render is the recommended host for this sidecar because it can run headless Chrome.

1. Push this repository to GitHub
2. In Render, create a new **Blueprint** or **Web Service** from the repo
3. Point the service at `whatsapp-sidecar/` or use the repo root `render.yaml`
4. Add `WHATSAPP_API_KEY` in Render environment variables
5. Keep the persistent disk mounted at `/var/data`
6. Deploy once, then open `/qr` with the `X-Api-Key` header and scan the code
7. Set your main app `WhatsApp.BaseUrl` to the Render URL or your Render custom domain

Render-specific details in this repo:

- `render.yaml` creates the Node web service
- `npm run render-build` downloads Chrome into `whatsapp-sidecar/.render-browsers`
- `WHATSAPP_AUTH_PATH` defaults to `/var/data/whatsapp-auth` on Render so the login survives restarts
- `GET /healthz` is unprotected for Render health checks

### SmarterASP.NET Hosting

This repo includes `web.config` for a separate Node.js site on SmarterASP.NET.

Recommended production setup:

1. Keep the ASP.NET MVC site on `shivkalaclasses.com`
2. Create a separate Node.js site or subdomain such as `wa.shivkalaclasses.com`
3. Deploy the contents of `whatsapp-sidecar/` to that Node.js site
4. Set `WhatsApp__BaseUrl=https://wa.shivkalaclasses.com` in the MVC app
5. Set the same `WHATSAPP_API_KEY` value in both the MVC app and the sidecar

The GitHub Actions pipeline can deploy both sites separately by Web Deploy once you add the required secrets, but Render is the preferred option for the WhatsApp sidecar.

## Notes
- Keep your phone connected to the internet.
- Do not broadcast spam — WhatsApp may ban the number.
- For high volume consider official WhatsApp Business API.
