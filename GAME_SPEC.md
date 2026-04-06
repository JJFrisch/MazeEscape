# MazeEscape – Complete Feature Specification

> Extracted from the .NET MAUI codebase. This is the authoritative reference for rebuilding the web (SvelteKit) and iOS (SwiftUI) clients.

---

## 1. Core Data Models

### 1.1 MazeCell
| Property | Type | Description |
|----------|------|-------------|
| X, Y | int | Grid coordinates |
| Value | int | 0=empty, 1=wall, 2=start, 3=end |
| East | bool | Wall to the right (true = wall exists) |
| North | bool | Wall above (true = wall exists) |

> Walls are stored on the **south** and **east** edges of each cell. A cell at (x,y) has:
> - A wall below if `Cells[y+1][x].North == true`
> - A wall to the left if `Cells[y][x-1].East == true`

### 1.2 MazeModel
| Property | Type | Description |
|----------|------|-------------|
| Cells | 2D list of MazeCell | The maze grid |
| Width, Height | int | Maze dimensions |
| Start | (int, int) | Entrance coordinates |
| End | (int, int) | Exit coordinates |
| Path | list of (int, int) | Optimal path from start to end |
| PathLength | int | Length of optimal path |
| Player | PlayerModel | {X, Y} current position |

### 1.3 CampaignLevel
| Property | Type | Description |
|----------|------|-------------|
| LevelID | int | Auto-increment PK |
| LevelNumber | string | Display ID ("1", "2", "1b") |
| Width, Height | int | Maze dimensions for this level |
| LevelType | string | Generation algorithm name |
| TwoStarMoves | int | Move threshold for 2nd star |
| ThreeStarTime | int | Time (seconds) threshold for 3rd star |
| NumberOfStars | int | 0–3 stars earned |
| MinimumStarsToUnlock | int | Cumulative stars needed |
| ConnectTo1, ConnectTo2 | string | Next level(s) unlocked on completion |
| Completed | bool | Level beaten at least once |
| Star1, Star2, Star3 | bool | Individual star flags |
| BestMoves | int | Lowest move count achieved |
| BestTime | TimeSpan | Fastest completion time |

### 1.4 CampaignWorld
| Property | Type | Description |
|----------|------|-------------|
| WorldID | int | 1, 2, or 3 |
| WorldName | string | Display name |
| ImageUrl | string | Thumbnail image |
| NumberOfLevels | int | Total levels |
| HighestBeatenLevel | int | Progress marker |
| Completed | bool | All levels beaten |
| Locked | bool | World locked/unlocked |
| StarCount | int | Total stars in this world |
| UnlockedMazesNumbers | list of string | Level numbers the player can access |
| UnlockedGatesNumbers | list of int | Gate indices the player has passed |
| ChestModels | list of IReward | Chests/portals/keys in the world map |
| LevelConnectsToDictionary | dict | Level → [next levels] map |
| gateStarRequired | list of int | Stars needed per gate |
| HighestAreaUnlocked | int | Current area progression |

### 1.5 SkinModel
| Property | Type | Description |
|----------|------|-------------|
| ID | int | Unique identifier |
| Name | string | Display name |
| ImageUrl | string | Avatar image asset |
| CoinPrice | int | Cost in coins (0 → 9999999 = unpurchasable) |
| GemPrice | int | Cost in gems |
| IsUnlocked | bool | Player owns this skin |
| IsEquipped | bool | Currently active |
| IsSpecialSkin | bool | Can only be unlocked via chests |

### 1.6 DailyMazeLevel
| Property | Type | Description |
|----------|------|-------------|
| LevelID | int | Auto-increment PK |
| ShortDate | string | Date key ("m/d/yyyy") |
| Date | DateTime | Calendar date |
| Month_Year | string | "MM-yyyy" for grouping |
| Width, Height | int | Maze dimensions |
| LevelType | string | Generation algorithm |
| Status | string | "not_started" / "completed" / "completed_late" |
| TimeNeeded | int | Time limit (seconds) |
| MovesNeeded | int | Move limit |
| CompletionTime | double | Actual time |
| CompletionMoves | int | Actual moves |

### 1.7 PlayerDataModel
| Property | Type | Description |
|----------|------|-------------|
| PlayerName | string | Username |
| PlayerId | int | Unique ID |
| CoinCount | int | Soft currency |
| HintsOwned | int | Hint powerup count |
| ExtraTimesOwned | int | Extra Time powerup count |
| ExtraMovesOwned | int | Extra Moves powerup count |
| CurrentWorldIndex | int | Last selected world |
| PlayerCurrentSkin | SkinModel | Equipped skin |
| UnlockedSkins | list of SkinModel | Owned skins |
| WallColor | Color | Custom maze wall color |
| MonthPrize1_achieved | bool | Half-month daily prize claimed |
| MonthPrize2_achieved | bool | Full-month daily prize claimed |
| MostRecentMonth | string | Current tracked month |
| Worlds | list of CampaignWorld | 3 worlds |
| WindowWidth, WindowHeight | int | Display size (400×670 default) |

---

## 2. Maze Generation

### 2.1 Algorithms
Eight procedural generation algorithms, each producing different maze styles:

1. **GenerateBacktracking** – Recursive backtracking (DFS). Long corridors, few dead ends.
2. **GenerateHuntAndKill** – DFS with linear scan fallback. More varied.
3. **GeneratePrims** – Prim's spanning tree. Short passages, many branches.
4. **GenerateKruskals** – Kruskal's spanning tree. Even distribution.
5. **GenerateGrowingTree_50_50** – 50% newest / 50% random cell.
6. **GenerateGrowingTree_75_25** – 75% newest / 25% random cell.
7. **GenerateGrowingTree_25_75** – 25% newest / 75% random cell.
8. **GenerateGrowingTree_50_0** – 50% newest / 50% oldest.

### 2.2 Generation Process
1. Initialize WxH grid with all walls up.
2. Pick random starting cell, mark as `Value=2` (start).
3. Run chosen algorithm to carve passages (remove walls between cells).
4. **OptimizeMaze()**: BFS from start to find the cell with the longest shortest-path. Mark it as `Value=3` (end). This ensures maximum path length.
5. Store the optimal path and its length.

### 2.3 Pathfinding
- **OptimizeMaze()**: BFS to find furthest reachable cell → becomes the exit.
- **FindPathFrom()**: DFS from player's current position to the exit → used for the Hint powerup.

---

## 3. Movement & Game Session

### 3.1 Movement Rules
- Player moves in 4 directions: Up, Down, Left, Right.
- A move is blocked if a wall exists between the current cell and the target cell.
- Wall collision rules:
  - **Up**: blocked if `Cells[y][x].North == true`
  - **Down**: blocked if `Cells[y+1][x].North == true`
  - **Right**: blocked if `Cells[y][x].East == true`
  - **Left**: blocked if `Cells[y][x-1].East == true`
- Out-of-bounds moves are blocked.

### 3.2 Move Queue
- Maximum 2 moves queued at once (`MoveQueueLengthMax = 2`).
- Inputs: keyboard arrows, on-screen buttons, swipe gestures.

### 3.3 Completion
- Level complete when `Player.X == End.X && Player.Y == End.Y`.

---

## 4. Scoring System

### 4.1 Stars (3-star system)
| Star | Condition |
|------|-----------|
| ★ (Star 1) | Complete the level |
| ★★ (Star 2) | Complete with `moves ≤ TwoStarMoves` |
| ★★★ (Star 3) | Complete with `time ≤ ThreeStarTime` seconds |

Stars are cumulative and persistent. Re-completing a level can only add stars, never remove them.

### 4.2 Coin Rewards
- Campaign levels award coins on completion (amount varies).
- Chests award 100–1000 coins (random).

---

## 5. Campaign Structure

### 5.1 Worlds

| World | Name | Levels | Background | Status |
|-------|------|--------|------------|--------|
| 1 | Cybernetic Labyrinths | 67 | background_maze_3.png | Unlocked |
| 2 | Galactic Grids | 110 | space_background11.png | Locked (portal unlock) |
| 3 | Elemental Whispers | 150 | carousel_maze_4.png | Locked |

### 5.2 Areas & Gates
Each world is divided into ~5 areas separated by **star gates**. Gates require cumulative stars to pass.

**World 1 Gates**: [20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250]
**World 2 Gates**: [20, 30, 50, 60, 80, 100, 120, 150, 230, 240, 250, 150, 230, 240, 250, 240, 250]

### 5.3 Level Connections
- Regular levels (e.g., "1", "2") auto-connect to the next number.
- Bonus levels (e.g., "1b", "4b") are branches—don't auto-connect forward.
- `ConnectTo1` and `ConnectTo2` define explicit connections (up to 2 outgoing edges).
- Levels unlock when:
  1. A connected predecessor is completed (has Star1).
  2. The player has enough cumulative stars (≥ MinimumStarsToUnlock).
  3. The relevant gate is unlocked.

### 5.4 Reward Objects on World Map
| Type | Behavior |
|------|----------|
| **ChestModel** | Awards 100–1000 random coins. Wobble animation. |
| **PortalModel** | Unlocks next locked world when opened. Requires N stars. |
| **SkinUnlockModel** | Unlocks a specific skin. |
| **KeyModel** | Unlocks a specific level by adding it to UnlockedMazesNumbers. |

---

## 6. Daily Maze System

### 6.1 Calendar View
- Monthly calendar grid showing one maze per day.
- Each day shows completion status icon.
- Current month tracked via `MostRecentMonth`. Resets when month changes.

### 6.2 Daily Maze Generation
- One maze per calendar day, deterministically generated from date seed.
- Parameters randomized per day: Width, Height, LevelType, TimeNeeded, MovesNeeded.

### 6.3 Completion Statuses
| Status | Meaning |
|--------|---------|
| not_started | Not attempted |
| completed | Completed within time/move limits |
| completed_late | Completed but exceeded limits |

### 6.4 Monthly Prizes
- **Prize 1**: Awarded after completing half the days in the month.
- **Prize 2**: Awarded after completing all days in the month.
- Tracked via `MonthPrize1_achieved` and `MonthPrize2_achieved`.

### 6.5 Unlock Requirement
Daily Mazes unlock when the player beats **level 10** in World 1 (Star1 on level "10").

---

## 7. Shop & Economy

### 7.1 Powerups
| Powerup | Cost | Effect |
|---------|------|--------|
| Hint | 200 coins | Shows next step on optimal path |
| Extra Time | 150 coins | Adds time during gameplay |
| Extra Moves | 50 coins | Adds moves during gameplay |

### 7.2 Skin Catalog (Purchasable)
| ID | Name | Coin Price | Image |
|----|------|-----------|-------|
| 0 | Maze Solver | FREE | player_image0 |
| 1 | Cool Lion | 500 | player_image1 |
| 2 | Sunset Swirl | 1,000 | player_image2 |
| 3 | Pink Sunset | 2,000 | player_image3 |
| 4 | Diskes | 5,000 | player_image4 |
| 5 | Fireball | 22,000 | player_image5 |
| 6 | Galaxy Ball | 50,000 | player_image6 |
| 7 | Elemental Mixer | 6,500 | player_image7 |
| 8 | Ivy Eye | 3,400 | player_image8 |
| 9 | Ruffles | 7,120 | player_image9 |
| 10 | Rocco | 3,000 | player_image10 |
| 11 | Pugsley | 4,375 | player_image11 |
| 12 | Kowalski | 9,000 | player_image12 |
| 14 | Brain | 19,000 | player_image14 |
| 16 | Fire Elemental | 8,500 | player_image16 |

### 7.3 Special Skins (Chest Unlocks Only)
| ID | Name | Source | Image |
|----|------|--------|-------|
| 13 | Space Maze | W2 Area 2 chest | player_image13 |
| 15 | Chucky | W2 Area 4 chest | player_image15 |
| 17 | Da Butler | W2 Area 5 chest | player_image17 |

---

## 8. Settings
| Setting | Type | Description |
|---------|------|-------------|
| Player Name | string | Editable username |
| Wall Color | Color | Custom maze wall color |

---

## 9. Navigation Flow
```
LandingPage (splash + "Play" button)
  └─→ MainPage (hub)
        ├─→ WorldsPage (horizontal world carousel)
        │     ├─→ CampaignPage (World 1 scrollable level map)
        │     │     └─→ CampaignLevelPage (gameplay)
        │     │           ├─→ CampaignMazeFinishedPopupPage (results)
        │     │           └─→ CampaignChestOpenedPopupPage (reward)
        │     └─→ World2CampaignPage (World 2 scrollable level map)
        │           └─→ World2CampaignLevelPage (gameplay)
        │                 └─→ World2CampaignMazeFinishedPopupPage (results)
        ├─→ DailyMazePage (calendar + gameplay)
        ├─→ ShopPage (powerups)
        ├─→ EquipPage (skins grid + purchase popup)
        ├─→ SettingsPage (username, wall color)
        └─→ InfoPage (help images)
```

---

## 10. Persistence Architecture

### 10.1 Local Storage
- **Mobile (MAUI)**: SQLite databases per world + daily mazes. JSON file for PlayerDataModel.
- **Web (Blazor)**: Browser IndexedDB via JS bridge.

### 10.2 Cloud Storage
- **API**: ASP.NET Core at `/api/saves/{playerId}` with JWT auth.
- **Format**: `SaveDocument` with `PayloadJson` containing serialized `SaveGamePayload`.
- **Concurrency**: Optimistic locking via `ConcurrencyToken` (GUID).
- **File backend**: `App_Data/saves.json` (dictionary keyed by playerId).

### 10.3 Sync Flow
1. On app pause → save checkpoint to API.
2. On app resume → process offline queue.
3. Offline fallback → queue saves to JSON file, retry on reconnect.
4. Conflict resolution → server wins, client pulls latest.

---

## 11. API Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/auth/token | No | Get JWT (playerId + clientSecret) |
| GET | /api/saves/{playerId} | JWT | Load save |
| PUT | /api/saves/{playerId} | JWT | Upsert save (with concurrency check) |
| GET | /api/health | No | Health check |

---

## 12. Key Constants
- Default window: 400×670
- Move queue max: 2
- Timer interval: 10ms
- Chest coin range: 100–1000
- JWT lifetime: 30 minutes
- API retry: 3 attempts, exponential backoff (500ms, 1s, 2s)
- Daily maze unlock: World 1 Level 10 completed
