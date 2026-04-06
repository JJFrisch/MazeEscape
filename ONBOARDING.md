# Developer Onboarding Checklist

Complete this checklist when setting up MazeEscape development on your machine.

## ✅ Environment Setup (15 minutes)

- [ ] Install **.NET SDK 9.0** or later
  - Check: `dotnet --version` → should be 9.0+
  - https://dotnet.microsoft.com/download/dotnet/9.0

- [ ] Clone the repository
  ```bash
  git clone https://github.com/JJFrisch/MazeEscape.git
  cd MazeEscape
  ```

- [ ] Run the setup script
  - **macOS/Linux**: `bash setup.sh`
  - **Windows**: `setup.bat`
  
  This automatically:
  - ✓ Checks .NET SDK
  - ✓ Restores NuGet packages
  - ✓ Builds the solution
  - ✓ Creates data folders

- [ ] Verify build succeeded
  ```bash
  dotnet build
  # Should show: "Build succeeded. 0 Warning(s)"
  ```

## ✅ First Run (5 minutes)

- [ ] Start the API server
  ```bash
  dotnet run --project MazeEscape.Api
  # You should see: "Now listening on: http://localhost:5000"
  ```

- [ ] Test health endpoint in another terminal
  ```bash
  curl http://localhost:5000/api/health
  ```
  
  Expected response shows `"status": "healthy"` ✓

## ✅ IDE/Editor Setup (Optional but Recommended)

### VS Code (Recommended)
- [ ] Install **C# extension** (ms-dotnettools.csharp)
- [ ] Open workspace: `code .`
- [ ] Press `F5` to debug (runs API with breakpoints)

### Visual Studio (Windows)
- [ ] Open `MazeEscape.sln`
- [ ] Right-click `MazeEscape.Api` → **Set as Startup Project**
- [ ] Press `F5` to debug

### Visual Studio for Mac
- [ ] Open `MazeEscape.sln`
- [ ] Select **MazeEscape.Api** as startup project
- [ ] Press `⌘↵` to run

## ✅ Configuration (5 minutes)

- [ ] Understand the JWT setup
  - Dev secret: `dev-web-client-secret` (in `appsettings.json`)
  - Never commit real secrets to version control

- [ ] Create `.env.local` for custom settings (optional)
  ```bash
  cp .env.example .env.local
  # Edit .env.local with your settings
  # This file is Git-ignored
  ```

- [ ] Review API settings
  - Location: `MazeEscape.Api/appsettings.json`
  - Token lifetime: 30 minutes (dev)
  - CORS: Allows any origin (dev only)

## ✅ Testing Setup (5 minutes)

Choose your REST client:

### Option 1: curl (No installation needed)
- [ ] Test commands ready to copy-paste
- [ ] See **LOCAL_DEVELOPMENT.md** for examples

### Option 2: Postman
- [ ] Download [Postman](https://www.postman.com/downloads/)
- [ ] Import collection from **LOCAL_DEVELOPMENT.md**
- [ ] Test each endpoint

### Option 3: VS Code REST Client
- [ ] Install **REST Client** extension (humao.rest-client)
- [ ] Create file `MazeEscape.Api.requests.http`
- [ ] Use examples from **LOCAL_DEVELOPMENT.md**

## ✅ First API Test (5 minutes)

- [ ] Get health status
  ```bash
  curl http://localhost:5000/api/health
  ```

- [ ] Request JWT token
  ```bash
  curl -X POST http://localhost:5000/api/auth/token \
    -H "Content-Type: application/json" \
    -d '{
      "playerId": "test-player",
      "clientSecret": "dev-web-client-secret"
    }'
  ```
  
  Response includes: `"accessToken": "eyJ..."` ✓

- [ ] List player saves (copy token from above)
  ```bash
  curl http://localhost:5000/api/saves/test-player \
    -H "Authorization: Bearer <PASTE_TOKEN>"
  ```

## ✅ Git Configuration

- [ ] Configure your Git name/email
  ```bash
  git config --global user.name "Your Name"
  git config --global user.email "your@email.com"
  ```

- [ ] Understand branching
  - **master**: Production-ready code
  - **feature/\***: Feature branches (create your own)
  
- [ ] Practice a feature branch
  ```bash
  git checkout -b feature/test-setup
  # Make a small change
  git add .
  git commit -m "test: verify local setup"
  # Don't push yet - this is just practice
  git checkout master
  ```

## ✅ Knowledge Base

Read these docs (in order):

1. **[LOCAL_DEVELOPMENT.md](LOCAL_DEVELOPMENT.md)** (10 min)
   - Complete setup guide
   - Troubleshooting
   - Common workflows

2. **[API_SETUP_SUMMARY.md](API_SETUP_SUMMARY.md)** (5 min)
   - What's available
   - Key endpoints
   - Project structure

3. **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)** (5 min)
   - For later: deployment steps
   - Production configuration

4. **[docs/index.html](docs/index.html)** (5 min)
   - Interactive API reference
   - Open in browser

## ✅ Running Tests

- [ ] Run all tests
  ```bash
  dotnet test
  ```
  
- [ ] Watch mode (re-run on changes)
  ```bash
  dotnet watch test
  ```

## ✅ Common Operations

Know how to do these:

- [ ] **Build everything**
  ```bash
  dotnet build
  ```

- [ ] **Clean build** (if something is stuck)
  ```bash
  dotnet clean
  dotnet restore
  dotnet build
  ```

- [ ] **Run API with different environment**
  ```bash
  ASPNETCORE_ENVIRONMENT=Local dotnet run --project MazeEscape.Api
  ```

- [ ] **Check what's listening on port 5000**
  ```bash
  # macOS/Linux
  lsof -i :5000
  # Windows
  netstat -ano | findstr :5000
  ```

## ✅ Troubleshooting

### Stuck? Try these:

- [ ] Latest branch: `git pull origin master`
- [ ] Clean packages: `dotnet clean && dotnet restore`
- [ ] Rebuild everything: `dotnet build`
- [ ] Trust HTTPS cert: `dotnet dev-certs https --trust`
- [ ] Check logs: Run with `--verbosity debug`

See **LOCAL_DEVELOPMENT.md** for more troubleshooting.

## ✅ Ready to Develop!

When all items are checked:

1. ✅ You can build the project anytime: `dotnet build`
2. ✅ You can run the API: `dotnet run --project MazeEscape.Api`
3. ✅ You can test it: `curl http://localhost:5000/api/health`
4. ✅ You can create feature branches and commit code
5. ✅ You understand the structure and know where to find docs

## 🎯 Next Steps

- **New feature?** Create a branch: `git checkout -b feature/my-feature`
- **Found a bug?** Create a branch: `git checkout -b fix/bug-name`
- **Need help?** Check **LOCAL_DEVELOPMENT.md** or ask the team

## 📚 Useful Links

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core API Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [JWT Authentication](https://jwt.io/)
- [GitHub Issues](https://github.com/JJFrisch/MazeEscape/issues)

---

**Onboarding Complete Date**: _______________
**Completed By**: _______________
**Questions?** Ask the team or check documentation above.
