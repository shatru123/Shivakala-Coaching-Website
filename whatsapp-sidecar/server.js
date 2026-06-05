'use strict';
const express = require('express');
const path = require('path');
const fs = require('fs');
const { Client, LocalAuth } = require('whatsapp-web.js');
const QRCode = require('qrcode');

const app = express();
app.use(express.json());

const apiKey = readSetting('WHATSAPP_API_KEY');
const authPath = readSetting('WHATSAPP_AUTH_PATH') || getDefaultAuthPath();
const executablePath = resolveExecutablePath();

let qrBase64 = null;
let isAuthenticated = false;
let lastInitError = '';
const messageQueue = [];
const puppeteerOptions = {
    headless: (process.env.WHATSAPP_PUPPETEER_HEADLESS || 'true') !== 'false',
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-dev-shm-usage']
};

if (executablePath) {
    puppeteerOptions.executablePath = executablePath;
}

const client = new Client({
    authStrategy: new LocalAuth({ dataPath: authPath }),
    puppeteer: puppeteerOptions
});

app.use((req, res, next) => {
    if (req.path === '/healthz') {
        return next();
    }

    if (!apiKey) {
        return next();
    }

    if (req.header('X-Api-Key') !== apiKey) {
        return res.status(401).json({ error: 'Unauthorized' });
    }

    next();
});

client.on('qr', async (qr) => {
    isAuthenticated = false;
    qrBase64 = await QRCode.toDataURL(qr);
    console.log('[WA] QR received — scan with WhatsApp');
});

client.on('ready', () => {
    isAuthenticated = true;
    qrBase64 = null;
    lastInitError = '';
    console.log('[WA] Client ready');
    // Flush queued messages
    messageQueue.splice(0).forEach(({ mobile, message, resolve }) => {
        sendMsg(mobile, message).then(resolve).catch(() => resolve(false));
    });
});

client.on('disconnected', () => {
    isAuthenticated = false;
    console.log('[WA] Disconnected');
});

client.initialize().catch(err => {
    lastInitError = err && err.message ? err.message : String(err);
    console.error('[WA] Init error:', err);
});

async function sendMsg(mobile, message) {
    const number = mobile.replace(/\D/g, '');
    const chatId = number.startsWith('91') ? `${number}@c.us` : `91${number}@c.us`;
    try {
        await client.sendMessage(chatId, message);
        return true;
    } catch (e) {
        console.error('[WA] Send error:', e.message);
        return false;
    }
}

// ── Routes ──────────────────────────────────────────────────────────────────

// GET /qr  → { authenticated, qrBase64 }
app.get('/qr', (req, res) => {
    res.json({ authenticated: isAuthenticated, qrBase64 });
});

// GET /status
app.get('/status', (req, res) => {
    res.json({
        authenticated: isAuthenticated,
        queueLength: messageQueue.length,
        browserConfigured: Boolean(executablePath),
        lastInitError
    });
});

// GET /healthz
app.get('/healthz', (_req, res) => {
    res.json({
        ok: true,
        authenticated: isAuthenticated,
        browserConfigured: Boolean(executablePath)
    });
});

// POST /send  { mobile, message }
app.post('/send', async (req, res) => {
    const { mobile, message } = req.body;
    if (!mobile || !message) return res.status(400).json({ error: 'mobile and message required' });

    if (!isAuthenticated) {
        // Queue for later
        const result = await new Promise(resolve => messageQueue.push({ mobile, message, resolve }));
        return res.json({ success: result, queued: false });
    }

    const success = await sendMsg(mobile, message);
    res.json({ success });
});

// POST /broadcast  { mobiles: [], message }
app.post('/broadcast', async (req, res) => {
    const { mobiles, message } = req.body;
    if (!Array.isArray(mobiles) || !message)
        return res.status(400).json({ error: 'mobiles[] and message required' });

    let sent = 0, failed = 0;
    for (const mobile of mobiles) {
        const ok = await sendMsg(mobile, message);
        ok ? sent++ : failed++;
        await new Promise(r => setTimeout(r, 900)); // polite delay
    }
    res.json({ sent, failed, total: mobiles.length });
});

const PORT = process.env.PORT || 3500;
app.listen(PORT, () => console.log(`[WA Sidecar] listening on :${PORT} | auth path: ${authPath}`));

function readSetting(name) {
    const value = process.env[name];
    if (!value) {
        return '';
    }

    const trimmedValue = value.trim();
    return /^__.+__$/.test(trimmedValue) ? '' : trimmedValue;
}

function resolveExecutablePath() {
    const configuredPath = readSetting('PUPPETEER_EXECUTABLE_PATH');
    if (configuredPath) {
        if (fs.existsSync(configuredPath)) {
            return configuredPath;
        }

        console.warn(`[WA] Ignoring missing browser executable path: ${configuredPath}`);
    }

    const candidatePaths = [
        path.join(__dirname, '.render-browsers', 'chrome', 'linux-138.0.7204.168', 'chrome-linux64', 'chrome'),
        path.join(__dirname, '.render-browsers', 'chrome-headless-shell', 'linux-138.0.7204.168', 'chrome-headless-shell-linux64', 'chrome-headless-shell'),
        path.join(__dirname, '.render-browsers'),
        '/usr/bin/google-chrome-stable',
        '/usr/bin/google-chrome',
        '/usr/bin/chromium-browser',
        '/usr/bin/chromium',
        path.join(__dirname, '.local-chromium', 'chrome-win', 'chrome.exe'),
        path.join(__dirname, '.local-chromium', 'chrome', 'win64-134.0.6998.35', 'chrome-win64', 'chrome.exe'),
        path.join(__dirname, 'chrome-win', 'chrome.exe'),
        path.join(__dirname, 'chrome', 'chrome.exe'),
        path.join(__dirname, 'Chromium', 'chrome.exe')
    ];

    for (const candidatePath of candidatePaths) {
        try {
            if (!fs.existsSync(candidatePath)) {
                continue;
            }

            if (fs.statSync(candidatePath).isDirectory()) {
                const nestedExecutable = findChromiumExecutable(candidatePath);
                if (nestedExecutable) {
                    console.log(`[WA] Using bundled browser at ${nestedExecutable}`);
                    return nestedExecutable;
                }

                continue;
            }

            console.log(`[WA] Using bundled browser at ${candidatePath}`);
            return candidatePath;
        } catch (error) {
            console.warn(`[WA] Skipping browser candidate ${candidatePath}: ${error.message}`);
        }
    }

    return undefined;
}

function getDefaultAuthPath() {
    if (process.env.RENDER && fs.existsSync('/var/data')) {
        return '/var/data/whatsapp-auth';
    }

    return path.join(__dirname, '.wwebjs_auth');
}

function findChromiumExecutable(rootPath) {
    const queue = [rootPath];
    const executableNames = new Set(['chrome', 'chrome.exe', 'chromium', 'chromium.exe', 'chrome-headless-shell']);

    while (queue.length > 0) {
        const currentPath = queue.shift();
        let entries = [];
        try {
            entries = fs.readdirSync(currentPath, { withFileTypes: true });
        } catch {
            continue;
        }

        for (const entry of entries) {
            const entryPath = path.join(currentPath, entry.name);
            if (entry.isDirectory()) {
                queue.push(entryPath);
                continue;
            }

            if (executableNames.has(entry.name)) {
                return entryPath;
            }
        }
    }

    return '';
}
