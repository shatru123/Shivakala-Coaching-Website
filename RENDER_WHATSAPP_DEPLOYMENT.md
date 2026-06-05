# Render WhatsApp Deployment

Use Render for the WhatsApp sidecar and keep the main MVC site on SmarterASP.NET.

## 1. Create the Render service

1. Open Render
2. Create a new Blueprint from this repository
3. Approve the `render.yaml` service
4. Wait for the first build to complete

This service uses:

- `rootDir`: `whatsapp-sidecar`
- `buildCommand`: `npm install && npm run render-build`
- `startCommand`: `npm start`
- persistent disk: `/var/data`

## 2. Add environment variables in Render

Set these in the Render dashboard:

- `WHATSAPP_API_KEY` = your shared secret
- `WHATSAPP_PUPPETEER_HEADLESS` = `true`

Keep these values as defined by `render.yaml` unless you have a reason to change them:

- `WHATSAPP_AUTH_PATH=/var/data/whatsapp-auth`
- `PUPPETEER_EXECUTABLE_PATH=`

## 3. Attach a custom domain

Add your custom domain in Render, for example:

- `wa.shivkalaclasses.com`

Then update DNS to point the subdomain to Render as instructed in the Render dashboard.

## 4. Connect the main MVC app

Update the main app production settings to the new Render URL:

```json
{
  "WhatsApp": {
    "BaseUrl": "https://wa.shivkalaclasses.com",
    "ApiKey": "thisismywhatsappsecret"
  }
}
```

If you are using GitHub Actions deployment for the MVC site, set:

- `PROD_WHATSAPP_BASE_URL=https://wa.shivkalaclasses.com`
- `PROD_WHATSAPP_API_KEY=thisismywhatsappsecret`

## 5. Verify the sidecar

Check:

- `GET /healthz`
- `GET /status` with header `X-Api-Key`
- `GET /qr` with header `X-Api-Key`

Expected result after first deploy:

- `/healthz` returns `ok: true`
- `/status` shows `browserConfigured: true`
- `/qr` returns a QR code until the phone is linked

## 6. Finish WhatsApp login

1. Open the admin WhatsApp screen in the main app or call the sidecar `/qr` endpoint directly
2. Scan the QR using WhatsApp on the phone
3. Confirm `/status` shows `authenticated: true`

## 7. Important note

Once Render is working, you should stop using SmarterASP for the WhatsApp sidecar. Keep only the main ASP.NET site on SmarterASP.NET.
