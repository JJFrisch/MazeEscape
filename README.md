# MazeEscape
![Logo](Images/logo.svg)

MazeEscape is a cross-platform puzzle game built with .NET MAUI. Navigate mazes, collect chests, and unlock new skins and levels across multiple worlds.

**Key Features**

- Polished maze gameplay with collectible chests and skins.

- Campaign and daily maze modes.

- Cross-platform support via .NET MAUI (Windows, macOS, iOS, Android, Mac Catalyst).

**Screenshots**

![Sample Maze Screen](Images/screenshot1.svg)

**Getting Started**

Prerequisites:

- Install the .NET SDK 8 or later. See https://dotnet.microsoft.com/

- Install .NET MAUI workloads for your platform (follow Microsoft's MAUI setup docs).

Clone the repo and run:

```bash
git clone <repo-url>
cd MazeEscape-1
dotnet restore
dotnet build
```

To run on macOS (Mac Catalyst):

```bash
dotnet build -f:net8.0-maccatalyst
open bin/Debug/net8.0-maccatalyst/maccatalyst-arm64/MazeEscape.app
```

To run on Windows (if available):

```bash
dotnet build -f:net8.0-windows10.0.19041.0
```

For Android and iOS, use your IDE (Visual Studio for Mac/Windows) or `dotnet` tooling with appropriate emulators/simulators.

**Project Structure (high level)**

- `App.xaml` / `App.xaml.cs` – App entry and resources.
- `Pages/` – XAML pages for UI (Campaign, Worlds, Shop, Settings).
- `Models/` – Game models: `Maze`, `MazeCell`, `CampaignLevel`, etc.
- `Controls/` – Custom controls and data template selectors.
- `Drawables/` – GraphicsView drawables for rendering the maze and player.
- `Resources/Images/` – Game art assets.
- `Images/` – Documentation images used by this README.

**Assets & Images**

Documentation images are in the `Images` folder. Game assets live under `Resources/Images` and `Resources/Raw` for raw data files.

**Contributing**

- Fork the repository, create a feature branch, and submit a pull request.
- Keep platform-specific changes isolated and provide testing notes for each platform.

**Troubleshooting**

- If you encounter MAUI workload issues, run `dotnet workload install maui`.
- Clear NuGet caches if builds fail with `dotnet nuget locals all --clear`.

**Web Deployment (GitHub Pages)**

- GitHub Pages deployment workflow: `.github/workflows/deploy-pages.yml`
- Deployment checklist: `docs/deployment-checklist.md`
- Default Pages URL for this repo: `https://jjfrisch.github.io/MazeEscape/`

**License**

This project does not include a license file. Add one if you plan to open-source it.

---

If you'd like, I can also add real screenshots from a simulator/device or wire up CI build steps. Want me to commit these changes and create a short commit message?

