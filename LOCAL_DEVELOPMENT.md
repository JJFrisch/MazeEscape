# Local Development Setup

Complete guide for running MazeEscape API on any developer's machine.

## Prerequisites

### Required
- **[.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)** (or later)
- **Git**
- **A terminal/command line**

### Optional (for UI development)
- **Visual Studio Code** or Visual Studio
- **REST client** (Postman, Insomnia, or VS Code REST Client)

### macOS Only
- Xcode Command Line Tools (if running the MAUI client)

### Windows Only
- Visual Studio 2022 (for MAUI development)

---

## Quick Start (2 minutes)

```bash
# 1. Clone and navigate to the repo
git clone https://github.com/JJFrisch/MazeEscape.git
cd MazeEscape

# 2. Run the API
dotnet run --project MazeEscape.Api

# 3. In another terminal, test it
curl http://localhost:5000/api/health

# Expected output:
# {
#   "status": "healthy",
#   "service": "MazeEscape.Api",
#   "version": "...",
#   "environment": "Development",
#   "timestamp": "..."
# }
```

✅ **Done!** API is now running locally.

---

## Detailed Setup Steps

### Step 1: Install .NET SDK

**Check if you already have it:**
```bash
dotnet --version
# Should be 9.0 or higher
```

**If not installed:**
- **macOS**: `brew install dotnet`
- **Windows**: Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Linux**: Follow [Microsoft docs](https://learn.microsoft.com/en-us/dotnet/core/install/linux)

### Step 2: Clone the Repository

```bash
git clone https://github.com/JJFrisch/MazeEscape.git
cd MazeEscape
```

### Step 3: Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages required by the project.

### Step 4: Build the Solution

```bash
dotnet build
```

Should complete with `Build succeeded. 0 Warning(s)` message.

### Step 5: Run the API

```bash
dotnet run --project MazeEscape.Api
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

---

## Testing the API Locally

### Option 1: Using curl (command line)

```bash
# Health check (no auth required)
curl http://localhost:5000/api/health

# Get a token
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "playerId": "test-player",
    "clientSecret": "dev-web-client-secret"
  }'

# Get player saves (with token)
curl http://localhost:5000/api/saves/test-player \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Option 2: Using Postman/Insomnia

1. Import this collection:
```json
{
  "info": {
    "name": "MazeEscape API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Health Check",
      "request": {
        "method": "GET",
        "url": "http://localhost:5000/api/health"
      }
    },
    {
      "name": "Get Token",
      "request": {
        "method": "POST",
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "body": {"raw": "{\"playerId\":\"test-player\",\"clientSecret\":\"dev-web-client-secret\"}"},
        "url": "http://localhost:5000/api/auth/token"
      }
    },
    {
      "name": "Get Saves",
      "request": {
        "method": "GET",
        "header": [{"key": "Authorization", "value": "Bearer {{token}}"}],
        "url": "http://localhost:5000/api/saves/test-player"
      }
    }
  ]
}
```

### Option 3: VS Code REST Client

Create `.vscode/rest-client.env`:
```
TOKEN=
API_URL=http://localhost:5000
PLAYER_ID=test-player
CLIENT_SECRET=dev-web-client-secret
```

Create `MazeEscape.Api.requests.http`:
```http
### Health Check
GET {{API_URL}}/api/health

### Get Token
POST {{API_URL}}/api/auth/token
Content-Type: application/json

{
  "playerId": "{{PLAYER_ID}}",
  "clientSecret": "{{CLIENT_SECRET}}"
}

@TOKEN = <paste token from response here>

### Get Player Saves
GET {{API_URL}}/api/saves/{{PLAYER_ID}}
Authorization: Bearer {{TOKEN}}

### Save Player Data
PUT {{API_URL}}/api/saves/{{PLAYER_ID}}
Authorization: Bearer {{TOKEN}}
Content-Type: application/json

{
  "playerId": "{{PLAYER_ID}}",
  "levelProgress": 5,
  "coins": 100
}
```

Then use VS Code's "Send Request" button above each request.

---

## Configuration for Different Environments

### Development (Default - Already Set Up)

Uses `appsettings.Development.json`:
```json
{
  "Jwt": {
    "Issuer": "MazeEscape.Api",
    "Audience": "MazeEscape.Web",
    "SigningKey": "replace-this-with-a-long-random-secret-for-production-2026",
    "TokenLifetimeMinutes": 30,
    "ClientSecret": "dev-web-client-secret"
  }
}
```

**To use this:**
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project MazeEscape.Api
```

### Local Custom Configuration

Create `appsettings.Local.json` in `MazeEscape.Api/` folder:
```json
{
  "Jwt": {
    "Issuer": "MazeEscape.Api",
    "Audience": "MazeEscape.Web",
    "SigningKey": "your-custom-secret-key-here-12345678",
    "TokenLifetimeMinutes": 30,
    "ClientSecret": "your-custom-client-secret"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

**Run with custom config:**
```bash
ASPNETCORE_ENVIRONMENT=Local dotnet run --project MazeEscape.Api
```

### Team Development

If your team uses different ports or secrets, create `.env.local` (ignored by git):

```bash
# .env.local
PORT=5000
JWT_SIGNING_KEY=team-dev-secret-2026
JWT_CLIENT_SECRET=team-dev-client-secret
```

Then add to `MazeEscape.Api/Program.cs` (optional, for flexibility):
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
var signingKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") 
    ?? jwtOptions.SigningKey;
var clientSecret = Environment.GetEnvironmentVariable("JWT_CLIENT_SECRET") 
    ?? "dev-web-client-secret";
```

---

## Folder Structure

The API will create and use:
```
MazeEscape.Api/
├── App_Data/                    # ← Created automatically
│   └── saves.json              # ← Player save files stored here
├── appsettings.json
├── appsettings.Development.json
├── Program.cs                  # ← Main entry point
├── Auth/
│   └── JwtTokenIssuer.cs
└── Services/
    └── FileSaveStore.cs
```

**Important**: `App_Data/` folder is created automatically on first run. It's safe to delete - it will regenerate.

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run only API tests
dotnet test MazeEscape.Api.Tests/

# Run with verbose output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "TestClassName=UnitTest1"
```

---

## Common Issues & Solutions

### Issue: "dotnet: command not found"
**Solution**: Install .NET SDK or add to PATH
```bash
# macOS with Homebrew
brew install dotnet

# Or verify installation
dotnet --version
```

### Issue: "Port 5000 already in use"
**Solution**: Use a different port
```bash
dotnet run --project MazeEscape.Api -- --urls="http://localhost:5555"
```

### Issue: "App_Data permission denied"
**Solution**: Fix folder permissions
```bash
chmod -R 755 MazeEscape.Api/App_Data
```

### Issue: JWT token returns 401 Unauthorized
**Solution**: Verify client secret matches
```json
{
  "playerId": "test-player",
  "clientSecret": "dev-web-client-secret"  // Must match appsettings.json exactly
}
```

### Issue: HTTPS certificate warning
**Solution**: This is normal for local dev. Accept or trust the self-signed cert:
```bash
# Generate local HTTPS certificate (first time only)
dotnet dev-certs https --trust
```

---

## Advanced: Running Multiple Services

Running API + Web app + Mobile app in parallel:

### Terminal 1: API
```bash
cd MazeEscape
dotnet run --project MazeEscape.Api
```

### Terminal 2: Web Interface
```bash
cd MazeEscape/MazeEscape.Web
dotnet run
```

### Terminal 3: Tests (watch mode)
```bash
cd MazeEscape
dotnet watch test
```

---

## Set Up IDE/Editor

### VS Code
1. Install C# extension (ms-dotnettools.csharp)
2. Open workspace: `code .`
3. Press `Ctrl+F5` to run with debugging

### Visual Studio (Windows)
1. Open `MazeEscape.sln`
2. Right-click `MazeEscape.Api` → Set as Startup Project
3. Press `F5` to run

### Docker (Optional - No .NET SDK Needed)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app
COPY . .
EXPOSE 5000 5001
CMD ["dotnet", "run", "--project", "MazeEscape.Api"]
```

Build and run:
```bash
docker build -t mazeescape-api .
docker run -p 5000:5000 -p 5001:5001 mazeescape-api
```

---

## Troubleshooting Environment Setup

```bash
# Verify .NET SDK
dotnet --version
dotnet --list-sdks

# Check project can be built
dotnet build MazeEscape.Api/MazeEscape.Api.csproj

# Verify dependencies
dotnet list package

# Check listening ports
# macOS/Linux
lsof -i :5000
# Windows
netstat -ano | findstr :5000

# View API logs
dotnet run --project MazeEscape.Api --verbosity debug
```

---

## Common Development Workflows

### Daily Development
```bash
# Clone once (first time)
git clone https://github.com/JJFrisch/MazeEscape.git

# Each day
cd MazeEscape
git pull                                    # Get latest changes
dotnet restore                              # Update dependencies
dotnet build                                # Build solution
dotnet run --project MazeEscape.Api         # Start developing
```

### Making Changes
```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes, test
dotnet test                                 # Run tests frequently

# Commit
git add .
git commit -m "Add feature X"

# Push and create pull request
git push origin feature/my-feature
```

### Before Pushing
```bash
# Ensure everything works
dotnet clean
dotnet restore
dotnet build
dotnet test

# Check health endpoint
curl http://localhost:5000/api/health
```

---

## Getting Help

If you run into issues:

1. **Check logs**: Look for error messages in terminal output
2. **Verify setup**: Run `dotnet build` in isolation
3. **Check prerequisites**: `dotnet --version` should be 9.0+
4. **Review configuration**: Check `appsettings.json` credentials
5. **Clear cache**: `dotnet clean && dotnet restore`

See `DEPLOYMENT_CHECKLIST.md` for production-related issues.

---

## Next Steps

Once API is running locally:
- [ ] Run tests: `dotnet test`
- [ ] Test health endpoint: `curl http://localhost:5000/api/health`
- [ ] Get JWT token and test auth flow
- [ ] Read `DEPLOYMENT_CHECKLIST.md` for deployment later
- [ ] Explore code structure in VS Code
