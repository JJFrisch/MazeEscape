# For Each Person's Machine - Complete Reference

## 📌 TL;DR (30 seconds)

```bash
# Clone
git clone https://github.com/JJFrisch/MazeEscape.git
cd MazeEscape

# Setup (choose one)
bash setup.sh          # macOS/Linux - auto does everything
# OR
setup.bat              # Windows - auto does everything
# OR
dotnet restore && dotnet build  # Manual option

# Run
dotnet run --project MazeEscape.Api

# Test (in another terminal)
curl http://localhost:5000/api/health

# You should see:
# {
#   "status": "healthy",
#   "service": "MazeEscape.Api",
#   "environment": "Development",
#   "timestamp": "..."
# }
```

✅ **You're done!** API is now running on your machine.

---

## 📚 Full Documentation by Use Case

### "I'm new to the team"
**Read these in order** (~30 minutes total):
1. **README.md** - Project overview
2. **ONBOARDING.md** - Step-by-step checklist with boxes to check
3. **LOCAL_DEVELOPMENT.md** - Detailed reference guide

### "I just need to get it running"
1. Run script: `bash setup.sh` (macOS/Linux) or `setup.bat` (Windows)
2. Start API: `dotnet run --project MazeEscape.Api`
3. Done! See output showing "Now listening on: http://localhost:5000"

### "I need to test endpoints"
**See**: LOCAL_DEVELOPMENT.md → Testing the API Locally section
- curl examples (copy-paste ready)
- Postman collection
- VS Code REST Client examples

### "I'm debugging/stuck"
**Check**: LOCAL_DEVELOPMENT.md → Common Issues & Solutions section
Or see troubleshooting section in this file below.

### "I need to understand the project structure"
**See**: API_SETUP_SUMMARY.md → Files Modified/Created section

### "I'm deploying to production"
**See**: DEPLOYMENT_CHECKLIST.md (full guide with verification steps)

---

## 🎯 What Each File Does

| File | Purpose | Read Time |
|------|---------|-----------|
| **setup.sh** | Automated setup (macOS/Linux) | 2 min |
| **setup.bat** | Automated setup (Windows) | 2 min |
| **README.md** | Quick start & overview | 5 min |
| **ONBOARDING.md** | New member checklist | 30 min |
| **LOCAL_DEVELOPMENT.md** | Complete dev reference | 20 min |
| **LOCAL_SETUP_SUMMARY.md** | This file - local setup guide | 10 min |
| **API_SETUP_SUMMARY.md** | What's new in the API | 5 min |
| **DEPLOYMENT_CHECKLIST.md** | Production deployment | 15 min |
| **.env.example** | Environment variables template | 2 min |

---

## 🚀 Setup Paths by Experience

### Path 1: Fastest (5 minutes)
```bash
bash setup.sh                                # Auto does everything
dotnet run --project MazeEscape.Api         # Start API
# Test in another terminal: curl http://localhost:5000/api/health
```

### Path 2: Manual (10 minutes)
```bash
dotnet restore                              # Download packages
dotnet build                                # Compile
dotnet run --project MazeEscape.Api         # Run
```

### Path 3: Thorough (30 minutes)
```bash
# Follow ONBOARDING.md checklist which includes:
# ✓ Checking prerequisites
# ✓ Cloning repo
# ✓ Running setup
# ✓ Testing endpoints
# ✓ Reading documentation
# ✓ Setting up IDE
```

---

## 🔧 Daily Development Routine

### Morning: Start developing
```bash
cd /path/to/MazeEscape
git pull                                    # Latest changes
dotnet run --project MazeEscape.Api         # Start API
# Code in another terminal...
```

### Before lunch: Quick verification
```bash
curl http://localhost:5000/api/health       # Still running?
```

### Before pushing code: Verify everything
```bash
dotnet test                                 # All tests pass?
dotnet build                                # Compiles clean?
curl http://localhost:5000/api/health       # API responds?
```

### Done for the day: Clean shutdown
```bash
# Press Ctrl+C in API terminal
# Check in your code changes
git add .
git commit -m "feat: describe what you did"
git push origin feature/branch-name
```

---

## 🛑 Quick Troubleshooting

### Problem: "dotnet: command not found"
```bash
# Install .NET SDK from https://dotnet.microsoft.com/download/dotnet/9.0
# Or on macOS: brew install dotnet
# Verify: dotnet --version  (should be 9.0+)
```

### Problem: "Port 5000 already in use"
```bash
# Find what's using it:
# macOS/Linux
lsof -i :5000

# Windows
netstat -ano | findstr :5000

# Kill the process or use different port:
dotnet run --project MazeEscape.Api -- --urls="http://localhost:5555"
```

### Problem: "failed to restore packages" or "build failed"
```bash
# Clean everything and try again
dotnet clean
dotnet restore
dotnet build
```

### Problem: "JWT returns 401 Unauthorized"
```bash
# Check you're using the right client secret:
# It's: dev-web-client-secret
# In: MazeEscape.Api/appsettings.json
```

### Problem: "HTTPS certificate not trusted"
```bash
# Generate and trust local certificate (first time only)
dotnet dev-certs https --trust
```

---

## 🎯 Key Concepts

### No Database Needed
- All data stored in `App_Data/saves.json`
- File-based storage
- Auto-created on first run
- Safe to delete (will regenerate)

### JWT Authentication
- Default dev secret: `dev-web-client-secret`
- Token lifetime: 30 minutes
- Used for `/api/saves/*` endpoints
- Get token from `/api/auth/token`

### Local Ports
- API: `http://localhost:5000` (HTTP)
- API: `https://localhost:5001` (HTTPS)
- Use either for local development

### Three Main Endpoints
1. **`GET /api/health`** - Check if API is running (no auth needed)
2. **`POST /api/auth/token`** - Get JWT token (no auth needed)
3. **`GET/PUT /api/saves/{playerId}`** - Player data (JWT required)

---

## ✅ Verification Checklist

After setup, verify these work:

```bash
# 1. API is running
curl http://localhost:5000/api/health
# Should return: "status": "healthy"

# 2. Build works
dotnet build
# Should show: "Build succeeded"

# 3. Tests pass
dotnet test
# Should show tests completing

# 4. Can get token
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"playerId":"test","clientSecret":"dev-web-client-secret"}'
# Should return: "accessToken": "..."
```

If all 4 pass: ✅ You're ready to develop!

---

## 🔐 Security Reminders

**FOR DEVELOPMENT ONLY:**
- The JWT key in `appsettings.json` is intentionally weak
- CORS allows any origin (for ease of testing)
- File-based storage is not production-ready
- Default secrets are hardcoded

**BEFORE PRODUCTION:**
- Change JWT signing key
- Restrict CORS to your domain
- Use proper database
- Use environment variables for secrets
- See `DEPLOYMENT_CHECKLIST.md`

---

## 💡 Pro Tips

### Tip 1: Auto-reload on changes
```bash
dotnet watch run --project MazeEscape.Api
```
API automatically restarts when you save code changes.

### Tip 2: Debug mode
```bash
# In VS Code: Press F5 (with C# extension)
# Or run with debugging in IDE
# Can set breakpoints and step through code
```

### Tip 3: Environment variables
```bash
# Run with custom environment:
ASPNETCORE_ENVIRONMENT=Custom dotnet run --project MazeEscape.Api
# Create appsettings.Custom.json for overrides
```

### Tip 4: Verbose logging
```bash
dotnet run --project MazeEscape.Api --verbosity debug
```
Shows detailed startup and request information.

### Tip 5: REST Client in VS Code
Install "REST Client" extension (humao.rest-client)
Create `.http` files with requests—click "Send Request" to test them.

---

## 📞 Getting Help

1. **Read the docs** - Check the relevant `.md` file first
2. **Check LOCAL_DEVELOPMENT.md** - Has comprehensive troubleshooting
3. **Ask in comments in GitHub** - Create an issue
4. **Ask teammates** - They've probably solved it before

---

## 🎓 After Setup: Next Steps

1. **Understand the code**
   - Open `MazeEscape.Api/Program.cs` - See how API is configured
   - Open `MazeEscape.Api/Services/FileSaveStore.cs` - See how data is stored
   - Check `MazeEscape.Core/Persistence/` - Data models

2. **Make a change**
   - Create feature branch: `git checkout -b feature/my-feature`
   - Modify code
   - Run tests: `dotnet test`
   - Commit: `git add . && git commit -m "..."`

3. **Submit for review**
   - Push: `git push origin feature/my-feature`
   - Create Pull Request on GitHub
   - Address review comments
   - Merge when approved

4. **Deploy later**
   - See `DEPLOYMENT_CHECKLIST.md` when ready for production

---

## 📱 For Different Developers

### Experienced .NET Developer
- Just run: `dotnet restore && dotnet build && dotnet run --project MazeEscape.Api`
- Go! You know the rest.

### Web Developer (Node/Python background)
- Start: README.md → LOCAL_DEVELOPMENT.md
- Test endpoints with curl or Postman
- The JWT/Auth flow is similar to node/python patterns

### Mobile Developer (Swift/Kotlin background)
- Start: README.md → LOCAL_SETUP_SUMMARY.md (this file)
- Focus on testing the API endpoints
- The MAUI client connects to this API

### DevOps Engineer
- Start: DEPLOYMENT_CHECKLIST.md
- Review GitHub Actions workflow: `.github/workflows/pages.yml`
- Health endpoint: `GET /api/health`

---

## 📊 Project Statistics

- **Framework**: ASP.NET Core 9.0
- **Language**: C# with nullable enabled
- **Database**: None (file-based storage)
- **Authentication**: JWT
- **Build time**: ~1 second (clean)
- **Startup time**: ~2 seconds
- **Test count**: Multiple (run with `dotnet test`)
- **Documentation files**: 7+ guides
- **Setup time**: 2-30 minutes depending on method

---

## ✨ Everything You Have Now

- ✅ Health endpoint (`/api/health`) - for monitoring
- ✅ Setup scripts - automates environment setup
- ✅ Complete documentation - guides for every scenario
- ✅ Authentication - JWT tokens for secure access
- ✅ File storage - local JSON-based data persistence
- ✅ GitHub Pages - auto-deploy docs
- ✅ CI/CD workflow - auto-test on push

---

**Status**: ✅ Complete - Ready for team development

**Setup Time**: 2-5 minutes (automated)
**Learning Time**: 10-30 minutes (docs)
**Ready to Code**: Now!

---

See also: `DEPLOYMENT_CHECKLIST.md` (when ready to deploy)
