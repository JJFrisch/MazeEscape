@echo off
REM MazeEscape Local Development Setup Script
REM Usage: setup.bat
REM Requires: PowerShell, Git, and .NET SDK

setlocal enabledelayedexpansion

echo.
echo MazeEscape Development Setup
echo =============================
echo.

REM Check .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [X] .NET SDK not found
    echo Install from: https://dotnet.microsoft.com/download/dotnet/9.0
    exit /b 1
)
for /f "tokens=*" %%A in ('dotnet --version') do set DOTNET_VERSION=%%A
echo [OK] .NET SDK found: %DOTNET_VERSION%

REM Check Git
git --version >nul 2>&1
if errorlevel 1 (
    echo [X] Git not found
    exit /b 1
)
echo [OK] Git installed

echo.
echo Setting up dependencies...

REM Restore packages
echo  - Restoring NuGet packages...
dotnet restore >nul 2>&1
if errorlevel 1 (
    echo [X] Failed to restore packages
    exit /b 1
)
echo [OK] Packages restored

REM Build solution
echo  - Building solution...
dotnet build >nul 2>&1
if errorlevel 1 (
    echo [X] Failed to build solution
    exit /b 1
)
echo [OK] Solution built

REM Create App_Data if needed
echo  - Creating data folders...
if not exist "MazeEscape.Api\App_Data" mkdir "MazeEscape.Api\App_Data"
echo [OK] Folders created

echo.
echo Setup complete!
echo.
echo Next steps:
echo.
echo 1. Run the API:
echo    dotnet run --project MazeEscape.Api
echo.
echo 2. Test health endpoint (in another terminal):
echo    curl http://localhost:5000/api/health
echo.
echo 3. See LOCAL_DEVELOPMENT.md for full guide
echo.
