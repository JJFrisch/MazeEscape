# MazeEscape Deployment Checklist

This checklist covers deploying the web client to GitHub Pages and pointing it to a production API.

## 1. GitHub Pages Setup

1. Ensure `.github/workflows/deploy-pages.yml` exists and is on `master`.
2. In GitHub repository settings, open **Pages**.
3. Set **Source** to **GitHub Actions**.
4. Push to `master` (or run the workflow manually from Actions).
5. Confirm site is live at:
   - `https://jjfrisch.github.io/MazeEscape/`

## 2. Production API Setup

1. Deploy `MazeEscape.Api` to your API host (Azure App Service, Render, Fly.io, etc.).
2. Set API configuration values:
   - `Jwt:Issuer`
   - `Jwt:Audience`
   - `Jwt:SigningKey`
   - `Jwt:ClientSecret`
3. Verify API endpoints:
   - `GET /api/health`
   - `GET /api/status`
   - `POST /api/auth/token`
   - `GET /api/saves/{playerId}`
   - `PUT /api/saves/{playerId}`

## 3. Point Web Client to Production API

The web client currently reads API URL from [MazeEscape.Web/wwwroot/appsettings.json](MazeEscape.Web/wwwroot/appsettings.json).

1. Update `SyncApi:BaseUrl` to your deployed API URL.
2. Commit and push to `master` to redeploy Pages.

## 4. Important Security Note

`MazeEscape.Web` is a static client. Any value shipped in web config is publicly visible.

- Do **not** rely on static client secrets for strong security.
- Prefer short-lived JWT issuance via a secure auth flow (OIDC/PKCE) or a backend-for-frontend.

## 5. Smoke Test

1. Open `https://jjfrisch.github.io/MazeEscape/`.
2. Confirm the page loads and local IndexedDB works.
3. Confirm sync can request a token and call API.
4. Confirm API health endpoint responds from the browser/network tools.
