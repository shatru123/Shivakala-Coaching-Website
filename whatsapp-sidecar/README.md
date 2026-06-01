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

## Notes
- Keep your phone connected to the internet.
- Do not broadcast spam — WhatsApp may ban the number.
- For high volume consider official WhatsApp Business API.
