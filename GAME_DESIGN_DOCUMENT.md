# Maze Escape: Pathbound — Game Design Document
### Feature Expansion Plan · D&D × Algorithms Edition

---

## Prologue: The Unifying Creative Lens

Every design decision in this document flows from a single creative thesis:

> **You are an Adventurer descending into dungeons that were not built by hands — they were conjured by ancient Algorithms, each one a god with its own philosophy of space and passage.**

The game already has three distinct worlds (Cybernetic, Galactic, Elemental) and 15+ maze-generation algorithms. The expansion weaves these together: each algorithm becomes a named **Deity of Paths**, each world becomes a **Plane of Existence**, and the player is cast as an **Adventurer** whose codex, inventory, and achievements tell the story of their journey.

The tone is **premium dark fantasy meets computational philosophy** — think a D&D module written by a computer scientist, with beautiful dark UI.

---

## Part 1 · The Rarity System

Before designing any new item, establish a unified rarity tier. Everything the player can own — skins, powerups, trails, collectibles, achievements — belongs to a tier. This creates immediately understood value signaling.

### Tiers

| Tier | Name | Color | Acquisition |
|---|---|---|---|
| 1 | **Common** | `#94a3b8` slate | Coins, early gameplay |
| 2 | **Uncommon** | `#4ade80` green | Coins (higher), map chests |
| 3 | **Rare** | `#38bdf8` blue | Gems, special chests, achievements |
| 4 | **Epic** | `#a78bfa` purple | Algorithm mastery, seasonal |
| 5 | **Legendary** | `#f59e0b` gold | World bosses, 30-day streaks, hidden |
| 6 | **Algorithm** | animated gradient | One per deity — earned only by mastery |

### Implementation Notes
- Every `SkinModel`, `MapCollectible`, and shop item gets a `rarity: Rarity` field
- Rarity color pulses subtly on owned items in inventory
- Legendary/Algorithm items have a shimmer sweep animation on their card border
- The equip screen gains a rarity filter: All / Owned / Rare+

---

## Part 2 · The Algorithm Deities

The game has 15 maze algorithms. Each becomes a named deity with lore, a visual sigil, a color, and a domain philosophy. This is the **world-building spine** of the entire expansion.

### The Pantheon

| Algorithm Key | Deity Name | Domain | Color | Philosophy |
|---|---|---|---|---|
| `backtracking` | **The Winding Serpent** | Memory & recursion | `#ef4444` red | *"All paths are valid — until proven dead."* |
| `huntAndKill` | **The Obsidian Hunter** | Pursuit & selection | `#f97316` orange | *"Walk until you cannot. Then hunt for what remains."* |
| `prims` | **The Greedy Arborist** | Growth from a seed | `#84cc16` lime | *"Always take the nearest step. Never the farthest."* |
| `kruskals` | **The Blind Weaver** | Random union | `#06b6d4` cyan | *"I know not which threads I join — only that none must loop."* |
| `growingTree_50_50` | **The Balanced Shaper** | Equilibrium | `#8b5cf6` violet | *"Half memory, half chance. The perfect middle path."* |
| `growingTree_75_25` | **The Favored Elder** | Preference & habit | `#c084fc` purple | *"Memory guides, but chaos keeps it honest."* |
| `growingTree_25_75` | **The Wild Gambler** | Entropy & surprise | `#f472b6` pink | *"Why remember where you've been? Surprise yourself."* |
| `growingTree_50_0` | **The Last One Standing** | Pure recency | `#38bdf8` sky | *"Always return to the newest. Never look back."* |
| `wilsons` | **The Patient Cartographer** | Loop erasure | `#34d399` emerald | *"Walk at random. Erase your loops. Only truth remains."* |
| `aldousBroder` | **The Forgotten Wanderer** | Uniform randomness | `#94a3b8` slate | *"Every cell is equally worthy. I play no favorites."* |
| `binaryTree` | **The Fork-Tongue Oracle** | Binary choice | `#fbbf24` amber | *"North or East. Always. Only. Forever."* |
| `sidewinder` | **The River God** | Horizontal flow | `#22d3ee` teal | *"I carve rivers, not trees. I move in one direction always."* |
| `ellers` | **The Row Stitcher** | Row-by-row union | `#a3e635` yellow-green | *"I see only the row before me. I never look down."* |
| `recursiveDivision` | **The Great Divider** | Separation & recursion | `#fb923c` orange-red | *"I do not build passage. I carve walls into space."* |
| `spiralBacktracker` | **The Coiling Dreamer** | Spiral memory | `#e879f9` fuchsia | *"I spiral inward before I reach outward."* |

### Deity Codex (In-Game)
A **Codex** tab in the main nav reveals all 15 deities. Each entry shows:
- Deity name + sigil (decorative SVG icon)
- Quote / philosophy
- Algorithm description in plain English ("This algorithm picks a random unvisited neighbor, carves to it, then backtracks when stuck — producing mazes with long winding corridors")
- Maze characteristics (avg dead-ends, corridor length, difficulty rating)
- Player's mastery progress: "Completed 12 / 30 mazes conjured by this deity"
- **Mastery reward** — unlocked at 10, 20, 30 completions of that algorithm type

This is the algorithm-nerd hook. Players who care about graph theory get something no other puzzle game gives them: the actual maths, beautifully presented.

### Algorithm Mastery Rewards
Each deity grants three rewards at 10 / 80 / 200 completions:
- **10 completions**: The deity's Sigil badge (shown in profile)
- **80 completions**: Rare item (trail, powerup pack, or gems)
- **200 completions**: **Algorithm skin** — a Legendary skin themed after that deity (Epic/Algorithm rarity, earned nowhere else)

The `CampaignLevel` type already has `levelType: MazeAlgorithm`. Daily maze also stores `levelType`. So mastery tracking is purely additive to the existing store.

---

## Part 3 · New Shop Items & Collectibles

### 3a · Consumable Powerups (Shop)

All existing powerups stay. New additions:

**Compass** 🧭 — 75 coins — Common
> *"A gift from the Wanderer. It knows not your path, only your destination's bearing."*
- Highlights the quadrant of the exit (N/E/S/W flash) for 3 seconds. Cheaper than a Hint — gives direction, not path.
- Implementation: compute `exit.x - player.x`, `exit.y - player.y`, display directional arrow overlay on maze for 3s.

**Hourglass** ⌛ — 300 coins — Uncommon  
> *"The Coiling Dreamer slows time for those who ask kindly."*
- Freezes the elapsed timer for 15 seconds. Shown as a glowing frozen timer in the HUD.
- Implementation: set a `frozenUntil` timestamp in session state; timer only increments when `Date.now() > frozenUntil`.

**Blink Scroll** 📜 — 450 coins — Rare  
> *"The Wanderer walked these halls before you. Step where he stepped."*
- Returns player to start WITHOUT ending the run and WITHOUT counting additional moves. Preserves visited cells and move count. Useful when stuck with bad move count.
- Implementation: `playerPos = maze.start`. Keep `moves`, `visitedCells`, `elapsed` unchanged.

**Streak Shield** 🛡️ — 800 coins — Rare  
> *"A pact with the Hunter. Miss a day — this burns in your place."*
- Protects daily streak if you miss exactly one day. Single-use. Auto-consumed. Shows in inventory.
- Implementation: `streakShieldsOwned: number` in `PlayerData`. Check on daily completion.

**Double Coin Token** 🪙🪙 — 600 coins — Uncommon  
> *"The Greedy Arborist's blessing — the next path you carve yields twice the reward."*
- Next maze completion earns 2× coins. Auto-consumed after use. Shows "2×" badge in HUD.
- Implementation: `doubleCoinsActive: boolean` in `PlayerData`. Check in coin-award logic.

**Reveal Shard** 💎 — 400 coins — Rare  
> *"A fragment of the Weaver's loom. It unmasks what fog conceals."*
- On the campaign map, reveals the next fog-of-war region's layout for 30 seconds before earning it. Cosmetic only — doesn't unlock levels.

---

### 3b · Cosmetic Shop Items (Permanent)

**Trail Styles** — replaces plain visited-cell fill with themed trail rendering. Sold for coins or gems. The `MazeRenderer` already renders `visitedCells` — trail styles are a palette addition to `MazeThemePalette`:

| Trail Name | Style | Price | Rarity |
|---|---|---|---|
| Default Haze | Soft fill (existing) | Free | Common |
| Electric Arc | Thin animated line connecting visited cells | 1,200 coins | Uncommon |
| Ember Crumble | Orange-red fading dots | 1,800 coins | Uncommon |
| Ghost Step | Blue-white translucent footprints | 2,500 coins | Rare |
| Arcane Runes | Glowing sigil marks at each visited cell | 800 gems | Epic |
| Algorithm Echo | Trail color matches the current deity's color | 1,200 gems | Algorithm |

Trail data: `trailStyle: TrailStyle` added to `PlayerData`. Renderer checks this when drawing visited cells.

**Exit Portal Skins** — the exit tile currently uses `exitCellColor`/`exitDotColor` in the palette. Wrap this in a `PortalSkin` type:

| Portal Skin | Description | Price | Rarity |
|---|---|---|---|
| Green Glow | Default | Free | Common |
| Void Tear | Black hole with purple ring | 900 coins | Uncommon |
| Golden Gate | Ornate gold arch | 2,000 coins | Rare |
| Digital Door | Cyan holographic outline | 500 gems | Epic |
| Celestial Eye | Animated rotating eye | 1,500 gems | Legendary |

**Wall Texture Packs** — currently players pick a wall color (`wallColor: string`). Expand to `wallStyle: WallStyle`:

| Wall Pack | Description | Price | Rarity |
|---|---|---|---|
| Solid Color | Current behavior | Free | Common |
| Circuit Board | Green trace lines on dark PCB | 1,500 coins | Uncommon |
| Dungeon Stone | Rough grey texture on walls | 2,000 coins | Uncommon |
| Ice Crystal | Pale blue walls with frost edges | 3,000 coins | Rare |
| Plasma Grid | Neon walls with glow animation | 1,000 gems | Epic |
| Void Fragment | Walls that look like tears in space | 2,000 gems | Legendary |

---

### 3c · Map Collectibles (Found on Campaign Map)

These use the existing `MapCollectible` / `MapCollectibleType` infrastructure. Add new types to the enum:

**Lore Scroll** 📜 `lore_scroll`  
- Tap to open a full-screen scroll overlay with world lore text and decorative border  
- 10 scrolls per world, each revealing a chapter of the world's mythology (e.g., World 1: the origin of the Cyber Labyrinths, how the Algorithm Deities came to power)  
- Collected scrolls appear in the Codex tab under "Chronicles"  
- No gameplay reward — pure lore. The completionist hook.

**Crystal Shard** 💠 `crystal_shard`  
- A crafting material. Collect 5 shards → craft one item at the Forge (new UI section in Inventory)  
- Placed in hidden off-path locations on the map, requiring detour  
- Forge recipes: 5 shards = 1 Rare trail, 10 shards = 1 Epic trail, 15 shards + 3 gems = 1 Legendary portal skin

**Area Trophy** 🏆 `area_trophy`  
- One per area (5 per world). Appears only after completing all levels in that area with at least 1 star  
- Opens a cinematic popup: animated trophy with area name, e.g., "The Warden of Circuits — Area 1 Conqueror"  
- Trophies displayed as a wall in the Inventory screen  
- Grants bonus coins on pickup (500–2000 based on area)

**Rune Stone** 🔮 `rune_stone`  
- A deity-specific collectible. Each rune is linked to one Algorithm Deity  
- Collecting it counts as 1 "mastery encounter" with that deity (boosts algorithm mastery progress even without playing that algo type)  
- 3–5 rune stones per world, each showing the deity's sigil

---

## Part 4 · The Codex — Inventory Screen

The Inventory is renamed the **Codex** (the adventurer's personal tome). It replaces a simple list with a rich tabbed interface.

### Navigation
A tab bar with 5 sections, accessible from the main nav:

```
[ Satchel ] [ Forge ] [ Trophies ] [ Chronicles ] [ Bestiary ]
```

---

### Tab 1: Satchel (Consumables)
A grid of all owned consumable items. Each card shows:
- Large emoji/icon  
- Item name + rarity color border  
- Quantity badge (e.g., "×3")  
- D&D-flavored description in small italic text  
- "Use" button (greyed out — items are auto-used in-maze; this is informational) or "Discard" option  

Sections: **Powerups** | **Keys** | **Tokens** | **Shields**

Visual: dark cards with rarity-tinted borders, faint deity sigil watermark on relevant items.

---

### Tab 2: Forge (Crafting)
A full-width panel split left/right:

**Left: Crystal Shard Bank**  
Shows current shard count with a progress bar toward each recipe tier. Animated shards float in a decorative container.

**Right: Forge Recipes**  
A list of craftable items. Each recipe shows:
- Resulting item (with rarity glow)  
- Required shards + any gem cost  
- "Craft" button (disabled if insufficient materials)  
- Animated forge effect on craft: particles, brief flash, item materializes

The Forge makes exploration feel meaningful — players who sweep every corner of the map get rewarded with items coin-buyers don't have.

---

### Tab 3: Trophies
A display wall of earned Area Trophies and special achievement badges. Layout:

- World sections with a decorative header (world accent color, world name)
- Each trophy is a 3D-ish card showing: trophy icon, area name, date earned, area conqueror title
- Locked trophies shown as grey silhouettes ("???" until earned)
- Total trophy count in header: "7 / 15 Trophies"

This is the prestige display. Players feel accomplished browsing it.

---

### Tab 4: Chronicles (Lore)
A scroll-style reader for collected Lore Scrolls:

- World selector at top
- Collected chapters listed (e.g., "Chapter 1: The First Algorithm" ✓, "Chapter 2: ???" 🔒)
- Click a collected chapter → full-screen scroll parchment with lore text and decorative borders (world-themed: circuit traces for World 1, star maps for World 2, vine borders for World 3)
- Progress: "3 / 10 Chronicles found in Cybernetic Labyrinths"

---

### Tab 5: Bestiary (Algorithm Codex)
The algorithm deity entries described in Part 2. Each card:

- Deity portrait area (generated SVG sigil based on algorithm properties)
- Deity name, quote, and domain
- Maze characteristics in stat bars: Corridor Length, Dead End Density, Difficulty, Randomness
- Player's mastery progress ring: 0 / 10 / 20 / 30
- Mastery rewards preview (dimmed until earned)
- "Play this deity" link → takes to campaign map with an algorithm filter showing all levels using this algorithm

The Bestiary is the hook for algorithmic-curious players. It's also a backdoor engagement loop: players who like the Wanderer's maze style will seek out Wilsons-type levels.

---

## Part 5 · Achievements System

### Structure: The Adventurer's Guild

Achievements are called **Guild Commendations**. They're organized into **Ranks** (D&D tiers):

| Rank | Name | Requirements |
|---|---|---|
| 1 | Initiate | 1–5 commendations |
| 2 | Journeyman | 6–15 commendations |
| 3 | Adventurer | 16–30 commendations |
| 4 | Veteran | 31–50 commendations |
| 5 | Champion | 51–75 commendations |
| 6 | Grandmaster | 76–100 commendations |
| 7 | Archon of the Labyrinth | All 100+ commendations |

Current Guild Rank shows in profile header with a badge icon.

### Commendation Categories

**The Pathfinder's Code** (Completion milestones)
- "First Step" — Complete your first level
- "Cartographer's Apprentice" — Complete 10 levels
- "Trail Blazer" — Complete 50 levels
- "The Endless Walker" — Complete 100 levels
- "The Labyrinth's Heir" — Complete all campaign levels
- "World Walker" — Complete all levels in any world with 3+ stars per level

**The Perfectionist's Ledger** (Star mastery)
- "Three-Pointed Crown" — Earn 3 stars on any level (all 3: completion + moves + time)
- "Unblemished Run" — Complete a level with 0 hints used, under par moves AND par time
- "Flawless Campaign" — Complete an entire world area with max stars
- "The Gilded Path" — Earn 5 stars on any level (all 5 stars)
- "Star Hoarder" — Earn 500 total stars

**The Deity's Favor** (Algorithm mastery — per deity)
- "Student of [Deity Name]" — 10 completions of that algorithm (×15 achievements)
- "Disciple of [Deity Name]" — 20 completions (×15)
- "Champion of [Deity Name]" — 30 completions (×15)
- "Pantheon Master" — Reach Champion rank with all 15 deities

**The Merchant's Ledger** (Economy)
- "First Purchase" — Buy any shop item
- "Hoarder's Trove" — Collect 10,000 coins total
- "The Gilded Vault" — Collect 100,000 coins total (lifetime, not current balance)
- "Forge Master" — Craft 5 items at the Forge
- "Crystal Collector" — Collect 25 crystal shards

**The Daily Pilgrim** (Streaks & daily maze)
- "Daily Devotee" — Complete 7 daily mazes (any days)
- "The Unbroken Chain" — 7-day streak
- "Pilgrim's Oath" — 30-day streak (grants Legendary Streak Shield as reward)
- "The Eternal Walker" — 100-day streak (grants Legendary skin: "The Pilgrim")
- "Back in Time" — Complete a daily maze from a previous month

**The Collector's Eye** (Collectibles)
- "Treasure Hunter" — Open 5 chests on the campaign map
- "Lorekeeper" — Collect 5 Lore Scrolls
- "Chronicler" — Collect all Lore Scrolls in one world
- "Rune Reader" — Collect all Rune Stones in one world
- "Area Conqueror" — Earn all 5 Area Trophies in a single world

**The Speedrunner's Gauntlet** (Time-based)
- "Flash Step" — Complete any level larger than 10x10 under 10 seconds
- "The Quicksilver Mind" — Complete 5 levels with 20 seconds better than 5-star time 
- "No Time to Think" — Complete a 15×15+ maze in under 30 seconds
- "Ghost Protocol" — Beat your own previous best time on any level by 10+ seconds

**The Tactician's Mark** (Move efficiency)
- "Economical" — Complete any level at exactly par moves (no extra moves)
- "The Optimal Path" — Complete 10 levels at or under par moves
- "Pathfinder Supreme" — Complete 50 levels at or under par moves
- "Zero Waste" — Complete a level with 0 hints, 0 extra moves, under time (all in one run)

**Hidden / Secret Commendations** (Easter eggs — shown as "???" until earned)
- "The Wanderer's Blessing" — Complete 5 Wilsons mazes without using any powerups
- "Against the Grain" — Complete a Binary Tree maze moving only South and West
- "The Recursion Loop" — Complete 5 Backtracking mazes in a row
- "Off the Beaten Path" — Find and collect a hidden Lore Scroll placed in an off-path dead end
- "Archon's Trial" — Complete a World Boss level without using any powerups
- "The Obsidian Run" — Complete 10 levels with 0 moves wasted (exact optimal path)

### Reward Structure
- Each commendation grants: coins (50–5000 based on difficulty) + progress toward Guild Rank
- Milestone Guild Ranks grant: title displayed on profile, exclusive skin or trail, gem bundle
- Rare+ commendations grant: their own unique badge displayed in Trophies tab

---

## Part 6 · Daily Streak & The Tavern Board

### The Tavern Board
Rename the daily maze's calendar UI to the **Tavern Board** — a dungeon tavern notice board where daily contracts are posted.

**Streak Counter** — top of the daily page, prominent:
```
🔥 Current Streak: 12 days
⭐ Best Streak: 23 days
```
At 3, 7, 14, 30, 60, 100 days: milestone pop-up with bonus reward (gems, streak shields, exclusive trail).

**Streak Multiplier** (optional future mechanic):
- 7-day streak = 1.2× coins on daily
- 30-day streak = 1.5× coins
- 100-day streak = 2× coins permanently while streak holds

**Daily Difficulty Rating** — show a difficulty badge on each calendar day:
- ⚡ Easy (small maze, long time limit)
- ⚔️ Medium (standard)
- 💀 Hard (large maze, tight time limit)
- 👑 Legendary (special weekend challenges, larger reward)

**Monthly Challenge Badge** — if you complete every single day of a month:
- Earn "Month Conqueror" commendation
- Exclusive monthly badge/cosmetic
- The existing `monthPrize1Achieved` / `monthPrize2Achieved` fields in `PlayerData` already hint at this mechanic — expand it.

**Streak Recovery** — if streak breaks, the Streak Shield auto-activates. Show an elegant "Shield Consumed" overlay rather than a failure screen.

---

## Part 7 · Adventurer's Chronicle (Stats Page)

A dedicated `/profile` or `/stats` route. Two panels:

### Left: Identity Card
- Player name (editable inline)
- Avatar (current skin, rendered large)
- Guild Rank badge + title (e.g., "Veteran — The Wanderer's Disciple")
- Favorite Deity (algorithm with most completions) — shown with deity color
- Member since date
- Cloud sync status

### Right: The Chronicle
Four stat clusters, each with its own decorative header:

**Paths Carved** (completion stats)
- Total mazes completed
- Campaign levels completed / total
- Daily mazes completed
- Bonus levels completed
- Total moves made (lifetime)
- Total time in mazes (formatted as "14h 32m")

**Stars & Glory**
- Total stars earned
- Average stars per level
- Perfect 5-star runs
- Levels with zero hints used
- Best streak (moves under par): X levels in a row

**The Merchant's Ledger**
- Coins collected (lifetime)
- Coins spent
- Items purchased
- Skins owned / total
- Chests opened

**Deity Records**
- For each algorithm deity: number of mazes completed (shown as mini progress bars)
- Fastest time ever (any maze)
- Fewest moves ever (any maze)
- Most efficient single run (closest to optimal path %)

All stats read directly from `levelProgress`, `dailyResults`, and `player` in `gameStore`. Pure display — no new data collection needed beyond streak/lifetime-coins tracking.

---

## Part 8 · Hall of Fame (Leaderboard)

Supabase is already wired. Add a `leaderboard` table:

```sql
daily_leaderboard:
  user_id, short_date, completion_time, completion_moves, stars, player_name, skin_id
```

### Three Leaderboard Modes

**Today's Gauntlet** — Default view. The current day's daily maze ranked by:
1. Stars earned (desc)
2. Completion time (asc)
3. Moves used (asc)

Shows top 100 with player name + skin avatar. Current player's row highlighted and pinned if outside top 100.

**Weekly Champions** — Best run from each player across the current week's dailies. Encourages consistency.

**Algorithm Duels** — For a selected algorithm deity, the fastest completion of any maze conjured by that deity. This is the nerd leaderboard — people compete to be the fastest solver of Wilsons or Eller's mazes specifically.

### Visual Design
The leaderboard looks like a D&D Hall of Fame plaque — dark background, gold accents, each row has a rank number styled like an engraved medal (#1 = gold, #2 = silver, #3 = bronze). Player names are accompanied by their skin avatar and Guild Rank badge.

---

## Part 9 · Campaign Map — Complete Interaction Reimagining

This is the biggest opportunity. The map is already a beautiful scrollable SVG. The goal is to make it feel **alive, reactive, and deeply interactive** — closer to opening a D&D dungeon module than clicking a level select screen.

### 9a · The Living Map

**Ambient World Effects** — each world's map has its own ambient animation layer rendered beneath the SVG:
- World 1 (Cyber): Scrolling data-stream particles, flickering neon grid lines, occasional lightning arc between nodes
- World 2 (Space): Slow-drifting stars, occasional comet passing, nodes pulse like constellations
- World 3 (Elemental): Drifting leaves/particles, subtle wind-blown path animation, torches flickering at key nodes

These are CSS/canvas animations layered behind the existing SVG. Low performance impact (requestAnimationFrame, reduced motion check).

**Path Lighting** — the main path corridors have a subtle ambient glow in the world's accent color. When a new area is unlocked, the path segment animates lighting up: the glow travels from the gate along the path to the first new level node (0.8s animation).

**Level Node Pulse** — incomplete, unlocked level nodes gently pulse (scale 1 → 1.05 → 1, 3s cycle). Completed nodes are solid and still. The current active level (player's furthest progress) has a slightly stronger pulse.

**Collectible Sparkle** — uncollected chests, gems, and rune stones emit small particle sparkles every few seconds. Collected items have a subtle "claimed" visual (desaturated, checkmark overlay).

**Fog Lift Animation** — when a star gate opens, the fog doesn't just disappear. It tears away in wisps from the gate outward — a 1.5s cinematic moment before the player can interact with the new area.

---

### 9b · Node Interaction — The Encounter Card

Currently: clicking a level node navigates directly to that level's play page.

New behavior: **clicking a level node opens an Encounter Card overlay** — a D&D-style encounter entry that stays on the map without navigating away.

```
┌──────────────────────────────────────────┐
│  ⚔️  LEVEL 12 · The Recursive Pit       │
│  Conjured by: The Winding Serpent        │
│  ──────────────────────────────────────  │
│  Difficulty: ●●●○○   Shape: Rectangular  │
│  Grid: 12×12   Algorithm: Backtracking   │
│                                          │
│  "Long corridors twist back on           │
│   themselves — there is always a way,    │
│   but rarely the obvious one."           │
│                                          │
│  Your Best: ⭐⭐⭐○○  Moves: 34  Time: 28s │
│  Par Moves: 38   Par Time: 45s           │
│                                          │
│  [  Enter Level  ]  [ Challenge Mode ]   │
│  [ Share Seed ]  [ View Algo Codex ]     │
└──────────────────────────────────────────┘
```

The Encounter Card:
- Appears as a slide-up panel from the bottom (mobile-first) or a floating panel near the node (desktop)
- Background uses the world accent color with glassmorphism blur
- The level name is procedurally generated from the algorithm deity + a flavor phrase bank
- Shows player's personal best with star breakdown
- **Challenge Mode** button: starts the level with a random modifier active (see Part 11)
- **Share Seed** button: copies a URL that deep-links to this exact level for friends
- **View Algo Codex**: opens the Bestiary entry for this deity in a modal

This replaces the current direct-navigate behavior. It adds friction in a good way — players pause, read the flavor, feel like they're choosing an encounter rather than clicking a button.

---

### 9c · The Mini-Map

A collapsed mini-map in the corner of the campaign map (bottom-right, 120×80px). Shows:
- The full world map at tiny scale
- A viewport rectangle showing current view area
- Player position marker (small dot)
- Red dots for uncollected collectibles

Click the mini-map to toggle a larger version that fits the full screen. Click again to collapse. This solves the navigation problem of being lost on a 24×52 tile map.

---

### 9d · The Filter Bar

A horizontal pill filter bar above the map:

```
[ All ] [ Incomplete ] [ Bonus ] [ Collectibles ] [ Deities ▾ ]
```

- **All**: Show everything (default)
- **Incomplete**: Dims completed levels, highlights incomplete ones
- **Bonus**: Dims main path levels, highlights bonus branches
- **Collectibles**: Dims levels, highlights uncollected collectibles with pulsing ring
- **Deities ▾**: Dropdown of all 15 deities — selecting one dims all levels except those using that algorithm, and highlights the relevant deity in the world's palette color

This makes the map useful as a planning tool, not just a visual.

---

### 9e · Player Trail & Breadcrumb

Show a **ghost trail** of the last 5 levels the player played, connected by a faint animated line in the player's trail color. This gives a sense of recent activity and helps orient the player when they return.

---

### 9f · Collectible Context Menu

Long-pressing (or right-clicking) a collectible opens a small context card:
- Item name + rarity
- Description of what it grants
- "What is this?" link to Codex entry
- If it's a Lore Scroll: shows chapter title (but not contents until collected)

---

### 9g · World Select → Map Transition

Currently: click world card → navigate to map page (cold load).

New: **Cinematic zoom-in transition**:
1. Player clicks world card
2. The world card expands to fill the screen (scale transform, 400ms)
3. The background image zooms in subtly (Ken Burns effect)
4. A brief "entering world" text fades in
5. The campaign map fades in over the world card background

On exit: reverse. The map zooms out back to the world card.

Implementation: CSS `view-transition-api` (supported in modern browsers) with `::view-transition-old` and `::view-transition-new` keyframes keyed to the world card element.

---

## Part 10 · Ghost Mode & The Rival System

### Your Ghost
After completing a level, the move sequence is stored (`bestMoveSequence: Direction[]` added to `CampaignLevel`). On retry, a **ghost player** replays your best run at 1× speed alongside your current run. Your ghost is slightly transparent, uses your current skin with 40% opacity and no halo glow.

The HUD shows: `You: 34 moves | Ghost: 38 moves` — live comparison.

If you beat the ghost, a small "👻 Ghost beaten!" toast appears.

### The Daily Rival
On the leaderboard, the player ranked directly above you becomes your **Daily Rival**. Their best move sequence for today's daily maze is stored (anonymized as a ghost — no username shown during play, only "The Rival"). After completing, the leaderboard reveals who it was.

---

## Part 11 · World Boss Encounters

The final level of each area's completion — after you hit the star gate requirement — triggers a **Boss Encounter**. This is not a separate mode: it's a special maze with unique properties.

### Boss Encounter Design

**Trigger**: Earning enough stars to pass a star gate opens not just the gate, but a Boss level node on the map, styled differently (larger, glowing, animated sigil of the area's dominant deity).

**The Encounter**: A large maze (20×20+) conjured by combining two algorithm styles (the area's dominant deity + a "challenger deity"). The boss maze has:
- No hints allowed (or hints cost 3× the normal count)
- A par time that is genuinely tight (requires planning)
- A special visual theme: the maze walls use the deity's color, with sigil watermarks in cells
- The intro overlay names the boss: *"You face the Winding Serpent in the Recursive Abyss — its labyrinth has no shortcuts, only truth."*

**Rewards**: Completing the boss grants:
- The area's **Trophy** (also collectible on the map, but boss completion guarantees it)
- A large coin reward (2000–5000 depending on area)
- The boss's **Rune Stone** (counts toward deity mastery)
- A permanent "Slayer of [Deity Name]" notation in the Codex entry

**Boss Failure**: If you exit without completing, a "Retreat" outcome — boss level remains available, no penalty. Encourages retry.

---

## Part 12 · Seasonal Quests

### The Weekly Contract
Each week, the Tavern Board posts a **Weekly Contract** alongside the daily mazes:
- A specific challenge: "Complete 5 mazes conjured by The Winding Serpent this week"
- Or: "Earn 15 stars on bonus levels this week"
- Or: "Complete today's daily maze in under 20 seconds"
- Reward: Uncommon–Rare item, crystals, or gems

The Weekly Contract reuses all existing data — it's purely a goal layer on top of existing actions.

### Seasonal Events (Quarterly)
Four per year, each themed:

**The Algorithm Solstice** (Winter) — All 15 deities are "active." Complete one maze of each deity type to earn the **Solstice Codex** (Legendary trail that cycles through all deity colors).

**The Galactic Drift** (Spring) — World 2 special event. Unique daily maze variants with star-warp visual effects. Exclusive space-themed skin available for 30 event completions.

**The Circuit Wars** (Summer) — Leaderboard competition event. Top 100 players on a special weekly leaderboard earn an exclusive "Circuit Champion" badge.

**The Elemental Rift** (Autumn) — World 3 special event. Maze walls shift between four element themes mid-run. Exclusive elemental trail reward.

---

## Part 13 · Implementation Priority Order

### Phase 1 — Foundation (do first, unlocks everything else)
1. **Rarity System** — add `rarity` field to types; build rarity color utilities
2. **Algorithm Deity data** — add deity metadata object to `mazeVisualThemes.ts` / new file
3. **Algorithm Mastery tracking** — add `algoMasteryCount: Record<MazeAlgorithm, number>` to `PlayerData`
4. **Streak tracking** — add `currentStreak`, `bestStreak`, `lastDailyDate`, `streakShieldsOwned` to `PlayerData`
5. **Lifetime coin tracking** — add `coinsEarnedLifetime` to `PlayerData`

### Phase 2 — Inventory / Codex
6. **Codex route** (`/codex`) with Satchel + Trophies + Bestiary tabs
7. **Algorithm Bestiary** — read deity data, show mastery progress
8. **Trophy display** — show earned area trophies

### Phase 3 — Campaign Map Reimagining
9. **Encounter Card** — replace direct navigate with node-click overlay
10. **Filter Bar** — add filter pills above map
11. **Mini-map** — corner mini-map component
12. **Living Map effects** — ambient animation layer per world

### Phase 4 — New Collectibles & Shop
13. **New powerup types** — Compass, Hourglass, Blink Scroll, Streak Shield, Double Coin
14. **Trail style system** — extend `MazeThemePalette` with `trailStyle`
15. **Lore Scrolls + Crystal Shards** — new `MapCollectibleType` entries + Forge UI

### Phase 5 — Social & Progression
16. **Achievements system** — commendation definitions + tracking + reward dispatch
17. **Daily Streak UI** — streak counter, milestone rewards, shield consumption
18. **Stats/Profile page** — `/stats` route
19. **Leaderboard** — `daily_leaderboard` Supabase table + UI
20. **Ghost Mode** — store `bestMoveSequence`, ghost renderer in maze

### Phase 6 — Endgame
21. **World Boss encounters** — boss node type, combined-algorithm generation, boss UI
22. **Weekly Contracts** — contract definition system + Tavern Board integration
23. **Seasonal Events** — event metadata + exclusive reward dispatch
24. **World Select transition** — View Transitions API cinematic zoom

---

## Appendix A: New `PlayerData` Fields

```typescript
interface PlayerData {
  // ... existing fields ...

  // Economy
  coinsEarnedLifetime: number;
  gemCount: number; // already exists in types.ts but missing from gameStore default

  // Consumables
  compassOwned: number;
  hourglassOwned: number;
  blinkScrollsOwned: number;
  streakShieldsOwned: number;
  doubleCoinsActive: boolean;
  revealShardsOwned: number;

  // Cosmetics
  trailStyle: TrailStyle;
  portalSkinId: number;
  wallStyle: WallStyle;

  // Progression
  currentStreak: number;
  bestStreak: number;
  lastDailyDate: string; // 'MM/DD/YYYY'
  algoMasteryCount: Partial<Record<MazeAlgorithm, number>>;
  collectedScrollIds: string[];
  collectedRuneIds: string[];
  crystalShards: number;
  earnedCommendationIds: string[];

  // Guild
  guildRank: GuildRank;
}
```

## Appendix B: New `MapCollectibleType` Values

```typescript
export type MapCollectibleType =
  | 'chest' | 'key' | 'gem' | 'cloak'
  | 'powerup_hint' | 'powerup_time' | 'powerup_moves'
  | 'lore_scroll'     // NEW: world lore chapter
  | 'crystal_shard'   // NEW: crafting material
  | 'area_trophy'     // NEW: area completion trophy
  | 'rune_stone';     // NEW: deity mastery rune
```

## Appendix C: New Routes

```
/codex               → Adventurer's Codex (Inventory)
/codex/satchel       → Consumables
/codex/forge         → Crafting
/codex/trophies      → Trophies & commendations
/codex/chronicles    → Lore scrolls
/codex/bestiary      → Algorithm deity codex
/stats               → Adventurer's Chronicle (Stats page)
/leaderboard         → Hall of Fame
```
