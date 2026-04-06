# GitHub Pages Configuration Guide

To enable GitHub Pages for this repository:

## Setup Instructions

1. **Go to Repository Settings**
   - Navigate to: `https://github.com/JJFrisch/MazeEscape/settings/pages`

2. **Configure GitHub Pages**
   - **Source**: Deploy from a branch
   - **Branch**: `master` (or `main`)
   - **Folder**: `/ (root)` or `/docs` (depending on your setup)

3. **Select the docs folder**
   - For this project, the docs are in the `/docs` directory
   - Set: Branch = `master`, Folder = `/docs`

4. **Save and Wait**
   - GitHub will automatically build and deploy
   - Your site will be available at: `https://JJFrisch.github.io/MazeEscape/`

## What's Deployed

- **API Documentation**: Interactive guide for all endpoints
- **Deployment Checklist**: Step-by-step deployment instructions
- **Health Check Guide**: How to monitor the API
- **Troubleshooting**: Common issues and solutions

## GitHub Pages Workflow

The `.github/workflows/pages.yml` file automatically:
1. Triggers on changes to `/docs` or `DEPLOYMENT_CHECKLIST.md`
2. Copies the deployment checklist to the docs folder
3. Deploys everything to GitHub Pages

## Manual Deploy

You can also manually trigger the deployment:
1. Go to **Actions** tab
2. Select **Deploy to GitHub Pages** workflow
3. Click **Run workflow** → **Run workflow**

## Local Testing

To test the documentation locally:

```bash
# Start a simple HTTP server in the docs directory
cd docs
python3 -m http.server 8000
# Visit http://localhost:8000
```

## Updating Documentation

- Edit `docs/index.html` for the main API documentation
- Edit `DEPLOYMENT_CHECKLIST.md` for deployment instructions
- Push to `master` to automatically deploy via GitHub Actions

---

**Your production documentation URL:**
```
https://JJFrisch.github.io/MazeEscape/
```

Point your API clients to:
```
https://your-production-api-domain.com/api/health
```
