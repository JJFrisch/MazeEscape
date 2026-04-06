# MazeEscape iOS App

SwiftUI native iOS app for MazeEscape.

## Setup

1. Open Xcode 15+ (required for iOS 17 / Canvas API)
2. Create a new iOS App project (`File → New → Project → App`)
   - Product Name: **MazeEscape**
   - Interface: **SwiftUI**
   - Language: **Swift**
   - Save to a temp location
3. Copy the generated `.xcodeproj` file structure OR drag the Swift source files from `ios/MazeEscape/` into the Xcode project navigator
4. Delete the auto-generated `ContentView.swift` and `App.swift` (ours replace them)
5. Build & run on simulator or device

## Architecture

```
MazeEscape/
├── MazeEscapeApp.swift   — App entry point
├── ContentView.swift      — Tab-based navigation
├── Models/
│   └── Models.swift       — All data models (MazeCell, Position, CampaignLevel, PlayerData, etc.)
├── Core/
│   ├── SeededRandom.swift — Deterministic PRNG (Mulberry32)
│   ├── MazeGenerator.swift — All 8 maze algorithms + pathfinding
│   ├── GameSession.swift  — Single-playthrough manager
│   └── LevelDefinitions.swift — Campaign levels + skin catalog
├── ViewModels/
│   └── GameStore.swift    — Central state (UserDefaults persistence)
└── Views/
    ├── LandingView.swift
    ├── WorldsView.swift
    ├── WorldDetailView.swift
    ├── GameplayView.swift   — Maze gameplay + victory sheet
    ├── MazeCanvasView.swift — Canvas-based maze rendering
    ├── DailyMazeView.swift  — Calendar + daily play
    ├── ShopView.swift
    ├── EquipView.swift
    └── SettingsView.swift
```

## Features

- 8 maze generation algorithms (matching web/MAUI versions)
- Deterministic seeded generation (same seed → same maze across platforms)
- Canvas-based maze rendering with player glow
- Swipe + D-pad controls
- Campaign (20+ levels across 2 worlds)
- Daily maze calendar
- Shop (hints, extra time, extra moves)
- Skin equip/purchase
- Settings (name, wall color)
- UserDefaults persistence
- Dark mode
