# MazeEscape Deployment Checklist

## Pre-Deployment Verification

### Code Quality
- [ ] All unit tests pass: `dotnet test`
- [ ] API builds successfully: `dotnet build MazeEscape.Api/MazeEscape.Api.csproj`
- [ ] No compiler warnings in release builds
- [ ] All code changes reviewed and approved

### API Configuration
- [ ] JWT signing key configured in `appsettings.json` (Jwt:SigningKey)
- [ ] JWT issuer configured (Jwt:Issuer)
- [ ] JWT audience configured (Jwt:Audience)
- [ ] Client secret configured (Jwt:ClientSecret)
- [ ] Database connection string configured if needed
- [ ] CORS settings reviewed for target domain

### Environment Setup
- [ ] Production environment variables set
- [ ] Database migrations applied (if applicable)
- [ ] File storage paths configured and accessible
- [ ] Log file paths configured and writable

## Deployment Steps

### 1. API Endpoint Verification
```bash
# Test health endpoint (no auth required)
curl https://your-api-domain.com/api/health

# Expected response:
# {
#   "status": "healthy",
#   "service": "MazeEscape.Api",
#   "version": "x.x.x.x",
#   "timestamp": "2026-04-05T...",
#   "environment": "production"
# }
```

### 2. Authentication Test
```bash
# Get token
curl -X POST https://your-api-domain.com/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"playerId":"test-player","clientSecret":"your-secret"}'

# Expected: 200 OK with accessToken and expiresAtUtc
```

### 3. Data Endpoint Verification
```bash
# Test with valid token
curl https://your-api-domain.com/api/saves/test-player \
  -H "Authorization: Bearer YOUR_TOKEN"

# Expected: 200 OK (if save exists) or 404 Not Found
```

## Post-Deployment Checks

- [ ] Health endpoint responds: `GET /api/health`
- [ ] Auth endpoint works: `POST /api/auth/token`
- [ ] Saves endpoint accessible: `GET /api/saves/{playerId}`
- [ ] CORS headers present in responses
- [ ] No 5xx errors in logs
- [ ] Response times acceptable (< 200ms)

## Rollback Plan

If deployment fails:

1. [ ] Stop the current deployment
2. [ ] Restore previous version from version control
3. [ ] Database rollback (if schema changed)
4. [ ] Verify rollback health checks pass
5. [ ] Investigate failure logs in `/logs`

## Production URLs

| Endpoint | URL |
|----------|-----|
| **Health Check** | `https://your-api-domain.com/api/health` |
| **Auth Token** | `https://your-api-domain.com/api/auth/token` |
| **Player Saves** | `https://your-api-domain.com/api/saves/{playerId}` |
| **Status** | `https://your-api-domain.com/` |

> **TODO**: Replace `your-api-domain.com` with actual production domain

## Monitoring

After deployment, monitor:
- [ ] Error rates in logs
- [ ] Response time metrics
- [ ] Token validation success rate
- [ ] Database connection pool health
- [ ] Disk space for save files

## Support

- Check logs in production environment
- Review GitHub Actions workflow runs: `.github/workflows/dotnet.yml`
- Run health endpoint regularly as part of monitoring
