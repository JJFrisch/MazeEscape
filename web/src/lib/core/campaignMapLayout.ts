/**
 * Campaign map layout definitions.
 *
 * World 1 uses a 24-column × 52-row tile grid at 80px per tile.
 * The path winds down and across the map like a large maze, with:
 *   – Main path connecting levels 1–67 sequentially
 *   – Bonus branches (dashed corridors) to bonus levels (1b–10b)
 *   – 5 star gates (one per area transition)
 *   – 2 key gates for extra locked sections
 *   – 5 fog regions (areas 2–5 start hidden)
 *   – ~15 collectibles (chests, gems, keys, cloaks, powerups)
 */

import type {
	WorldMapLayout,
	MapNode,
	MapPathSegment,
	MapCollectible,
	MapFogRegion,
	MapTile
} from './types';

// ---------------------------------------------------------------------------
// Helper
// ---------------------------------------------------------------------------

function tile(col: number, row: number): MapTile {
	return { col, row };
}

function seg(fc: number, fr: number, tc: number, tr: number, bonus = false): MapPathSegment {
	return { from: tile(fc, fr), to: tile(tc, tr), bonus };
}

// ---------------------------------------------------------------------------
// World 1 Layout  (24 cols × 52 rows, tileSize = 80px)
// ---------------------------------------------------------------------------
//
// Visual map sketch (not to scale):
//   Area 1: Cols 2–22, Rows 1–10   (levels 1–14 + bonus 1b-4b)
//   [STAR GATE 1 @ row 10]
//   Area 2: Cols 2–22, Rows 11–21  (levels 15–28 + bonus 5b-7b)
//   [STAR GATE 2 @ row 21]
//   Area 3: Cols 2–22, Rows 22–31  (levels 29–43 + bonus 8b-9b)
//   [STAR GATE 3 @ row 31]
//   Area 4: Cols 2–22, Rows 32–41  (levels 44–55 + bonus 10b)
//   [STAR GATE 4 @ row 41]
//   Area 5: Cols 2–22, Rows 42–51  (levels 56–67 + portal)
//
// The path snakes: right → down → left → down → right → …

const world1Nodes: MapNode[] = [
	// ── Area 1: levels 1–14, bonus 1b–4b ──────────────────────────────────

	// Main path snakes right across row 2, then down, then left across row 5, etc.
	{ id: 'l1',  type: 'level', tile: tile(2,  2),  levelNumber: '1',  area: 1 },
	{ id: 'l2',  type: 'level', tile: tile(5,  2),  levelNumber: '2',  area: 1 },
	{ id: 'l3',  type: 'level', tile: tile(8,  2),  levelNumber: '3',  area: 1 },
	{ id: 'l4',  type: 'level', tile: tile(11, 2),  levelNumber: '4',  area: 1 },
	{ id: 'l5',  type: 'level', tile: tile(14, 2),  levelNumber: '5',  area: 1 },
	{ id: 'l6',  type: 'level', tile: tile(17, 2),  levelNumber: '6',  area: 1 },
	{ id: 'l7',  type: 'level', tile: tile(20, 2),  levelNumber: '7',  area: 1 },
	// turn down
	{ id: 'l8',  type: 'level', tile: tile(20, 5),  levelNumber: '8',  area: 1 },
	// turn left
	{ id: 'l9',  type: 'level', tile: tile(17, 5),  levelNumber: '9',  area: 1 },
	{ id: 'l10', type: 'level', tile: tile(14, 5),  levelNumber: '10', area: 1 },
	{ id: 'l11', type: 'level', tile: tile(11, 5),  levelNumber: '11', area: 1 },
	{ id: 'l12', type: 'level', tile: tile(8,  5),  levelNumber: '12', area: 1 },
	{ id: 'l13', type: 'level', tile: tile(5,  5),  levelNumber: '13', area: 1 },
	{ id: 'l14', type: 'level', tile: tile(2,  5),  levelNumber: '14', area: 1 },

	// Bonus branches off area 1
	{ id: 'l1b', type: 'bonus_level', tile: tile(5,  4),  levelNumber: '1b', area: 1 },
	{ id: 'l2b', type: 'bonus_level', tile: tile(11, 4),  levelNumber: '2b', area: 1 },
	{ id: 'l3b', type: 'bonus_level', tile: tile(20, 4),  levelNumber: '3b', area: 1 },
	{ id: 'l4b', type: 'bonus_level', tile: tile(14, 4),  levelNumber: '4b', area: 1 },

	// Star gate between area 1 and area 2 (20 stars)
	{ id: 'sg1', type: 'star_gate', tile: tile(2, 7), starsRequired: 35, area: 2 },
	{ id: 'boss1', type: 'boss', tile: tile(5, 7), levelNumber: '14-boss', starsRequired: 35, encounterTitle: 'World Boss', bossFlavor: 'The Labyrinth Core bends the corridors into a war machine.', area: 2 },

	// ── Area 2: levels 15–28, bonus 5b–7b ─────────────────────────────────

	{ id: 'l15', type: 'level', tile: tile(2,  9),  levelNumber: '15', area: 2 },
	{ id: 'l16', type: 'level', tile: tile(5,  9),  levelNumber: '16', area: 2 },
	{ id: 'l17', type: 'level', tile: tile(8,  9),  levelNumber: '17', area: 2 },
	{ id: 'l18', type: 'level', tile: tile(11, 9),  levelNumber: '18', area: 2 },
	{ id: 'l19', type: 'level', tile: tile(14, 9),  levelNumber: '19', area: 2 },
	{ id: 'l20', type: 'level', tile: tile(17, 9),  levelNumber: '20', area: 2 },
	{ id: 'l21', type: 'level', tile: tile(20, 9),  levelNumber: '21', area: 2 },
	// turn down
	{ id: 'l22', type: 'level', tile: tile(20, 12), levelNumber: '22', area: 2 },
	// turn left
	{ id: 'l23', type: 'level', tile: tile(17, 12), levelNumber: '23', area: 2 },
	{ id: 'l24', type: 'level', tile: tile(14, 12), levelNumber: '24', area: 2 },
	{ id: 'l25', type: 'level', tile: tile(11, 12), levelNumber: '25', area: 2 },
	{ id: 'l26', type: 'level', tile: tile(8,  12), levelNumber: '26', area: 2 },
	{ id: 'l27', type: 'level', tile: tile(5,  12), levelNumber: '27', area: 2 },
	{ id: 'l28', type: 'level', tile: tile(2,  12), levelNumber: '28', area: 2 },

	// Bonus branches off area 2
	{ id: 'l5b', type: 'bonus_level', tile: tile(5,  11), levelNumber: '5b', area: 2 },
	{ id: 'l6b', type: 'bonus_level', tile: tile(14, 11), levelNumber: '6b', area: 2 },
	{ id: 'l7b', type: 'bonus_level', tile: tile(20, 11), levelNumber: '7b', area: 2 },

	// Key gate between area 2 sub-sections (requires key item 'key_area2')
	{ id: 'kg1', type: 'key_gate', tile: tile(11, 14), keyItemId: 'key_area2', area: 2 },

	// Star gate between area 2 and area 3 (45 stars)
	{ id: 'sg2', type: 'star_gate', tile: tile(2, 15), starsRequired: 80, area: 3 },

	// ── Area 3: levels 29–43, bonus 8b–9b ─────────────────────────────────

	{ id: 'l29', type: 'level', tile: tile(2,  17), levelNumber: '29', area: 3 },
	{ id: 'l30', type: 'level', tile: tile(5,  17), levelNumber: '30', area: 3 },
	{ id: 'l31', type: 'level', tile: tile(8,  17), levelNumber: '31', area: 3 },
	{ id: 'l32', type: 'level', tile: tile(11, 17), levelNumber: '32', area: 3 },
	{ id: 'l33', type: 'level', tile: tile(14, 17), levelNumber: '33', area: 3 },
	{ id: 'l34', type: 'level', tile: tile(17, 17), levelNumber: '34', area: 3 },
	{ id: 'l35', type: 'level', tile: tile(20, 17), levelNumber: '35', area: 3 },
	// turn down
	{ id: 'l36', type: 'level', tile: tile(20, 20), levelNumber: '36', area: 3 },
	// turn left
	{ id: 'l37', type: 'level', tile: tile(17, 20), levelNumber: '37', area: 3 },
	{ id: 'l38', type: 'level', tile: tile(14, 20), levelNumber: '38', area: 3 },
	{ id: 'l39', type: 'level', tile: tile(11, 20), levelNumber: '39', area: 3 },
	{ id: 'l40', type: 'level', tile: tile(8,  20), levelNumber: '40', area: 3 },
	{ id: 'l41', type: 'level', tile: tile(5,  20), levelNumber: '41', area: 3 },
	{ id: 'l42', type: 'level', tile: tile(3,  20), levelNumber: '42', area: 3 },
	{ id: 'l43', type: 'level', tile: tile(2,  20), levelNumber: '43', area: 3 },

	// Bonus branches off area 3
	{ id: 'l8b', type: 'bonus_level', tile: tile(8,  19), levelNumber: '8b', area: 3 },
	{ id: 'l9b', type: 'bonus_level', tile: tile(17, 19), levelNumber: '9b', area: 3 },

	// Key gate for the deep path through area 3 (requires 'key_forest')
	{ id: 'kg2', type: 'key_gate', tile: tile(20, 22), keyItemId: 'key_forest', area: 3 },

	// Star gate between area 3 and area 4 (80 stars)
	{ id: 'sg3', type: 'star_gate', tile: tile(2, 23), starsRequired: 140, area: 4 },

	// ── Area 4: levels 44–55, bonus 10b ───────────────────────────────────

	{ id: 'l44', type: 'level', tile: tile(2,  25), levelNumber: '44', area: 4 },
	{ id: 'l45', type: 'level', tile: tile(5,  25), levelNumber: '45', area: 4 },
	{ id: 'l46', type: 'level', tile: tile(8,  25), levelNumber: '46', area: 4 },
	{ id: 'l47', type: 'level', tile: tile(11, 25), levelNumber: '47', area: 4 },
	{ id: 'l48', type: 'level', tile: tile(14, 25), levelNumber: '48', area: 4 },
	{ id: 'l49', type: 'level', tile: tile(17, 25), levelNumber: '49', area: 4 },
	{ id: 'l50', type: 'level', tile: tile(20, 25), levelNumber: '50', area: 4 },
	// turn down
	{ id: 'l51', type: 'level', tile: tile(20, 28), levelNumber: '51', area: 4 },
	// turn left
	{ id: 'l52', type: 'level', tile: tile(17, 28), levelNumber: '52', area: 4 },
	{ id: 'l53', type: 'level', tile: tile(14, 28), levelNumber: '53', area: 4 },
	{ id: 'l54', type: 'level', tile: tile(11, 28), levelNumber: '54', area: 4 },
	{ id: 'l55', type: 'level', tile: tile(8,  28), levelNumber: '55', area: 4 },

	// Bonus branch off area 4
	{ id: 'l10b', type: 'bonus_level', tile: tile(5, 27), levelNumber: '10b', area: 4 },

	// Star gate between area 4 and area 5 (120 stars)
	{ id: 'sg4', type: 'star_gate', tile: tile(8, 31), starsRequired: 210, area: 5 },

	// ── Area 5: levels 56–67 + portal ─────────────────────────────────────

	{ id: 'l56', type: 'level', tile: tile(8,  33), levelNumber: '56', area: 5 },
	{ id: 'l57', type: 'level', tile: tile(11, 33), levelNumber: '57', area: 5 },
	{ id: 'l58', type: 'level', tile: tile(14, 33), levelNumber: '58', area: 5 },
	{ id: 'l59', type: 'level', tile: tile(17, 33), levelNumber: '59', area: 5 },
	{ id: 'l60', type: 'level', tile: tile(20, 33), levelNumber: '60', area: 5 },
	// turn down
	{ id: 'l61', type: 'level', tile: tile(20, 36), levelNumber: '61', area: 5 },
	// turn left
	{ id: 'l62', type: 'level', tile: tile(17, 36), levelNumber: '62', area: 5 },
	{ id: 'l63', type: 'level', tile: tile(14, 36), levelNumber: '63', area: 5 },
	{ id: 'l64', type: 'level', tile: tile(11, 36), levelNumber: '64', area: 5 },
	{ id: 'l65', type: 'level', tile: tile(8,  36), levelNumber: '65', area: 5 },
	{ id: 'l66', type: 'level', tile: tile(5,  36), levelNumber: '66', area: 5 },
	{ id: 'l67', type: 'level', tile: tile(2,  36), levelNumber: '67', area: 5 },

	// World portal (end of world 1)
	{ id: 'portal1', type: 'portal', tile: tile(2, 39), area: 5 },

	// Final star gate before portal (250 stars)
	{ id: 'sg5', type: 'star_gate', tile: tile(2, 38), starsRequired: 300, area: 5 }
];

// ---------------------------------------------------------------------------
// Path segments (main corridor + bonus branches)
// ---------------------------------------------------------------------------

const world1Paths: MapPathSegment[] = [
	// ── Area 1 main path ──
	// l1 → l2 → l3 → l4 → l5 → l6 → l7  (right along row 2)
	seg(2, 2, 5, 2), seg(5, 2, 8, 2), seg(8, 2, 11, 2), seg(11, 2, 14, 2), seg(14, 2, 17, 2), seg(17, 2, 20, 2),
	// l7 → l8  (down)
	seg(20, 2, 20, 5),
	// l8 → l9 → l10 → l11 → l12 → l13 → l14  (left along row 5)
	seg(20, 5, 17, 5), seg(17, 5, 14, 5), seg(14, 5, 11, 5), seg(11, 5, 8, 5), seg(8, 5, 5, 5), seg(5, 5, 2, 5),
	// l14 → sg1  (down)
	seg(2, 5, 2, 7),
	seg(2, 7, 5, 7, true),
	// sg1 → l15  (down)
	seg(2, 7, 2, 9),

	// ── Area 1 bonus branches (dashed) ──
	seg(5, 2, 5, 4, true),   // l2 → l1b
	seg(11, 2, 11, 4, true), // l4 → l2b
	seg(20, 2, 20, 4, true), // l7 → l3b
	seg(14, 5, 14, 4, true), // l10 → l4b

	// ── Area 2 main path ──
	// l15 → l16 → l17 → l18 → l19 → l20 → l21  (right along row 9)
	seg(2, 9, 5, 9), seg(5, 9, 8, 9), seg(8, 9, 11, 9), seg(11, 9, 14, 9), seg(14, 9, 17, 9), seg(17, 9, 20, 9),
	// l21 → l22  (down)
	seg(20, 9, 20, 12),
	// l22 → l23 → l24 → l25 → l26 → l27 → l28  (left along row 12)
	seg(20, 12, 17, 12), seg(17, 12, 14, 12), seg(14, 12, 11, 12), seg(11, 12, 8, 12), seg(8, 12, 5, 12), seg(5, 12, 2, 12),
	// l28 → sg2  (down)
	seg(2, 12, 2, 15),
	// sg2 → l29  (down)
	seg(2, 15, 2, 17),

	// ── Area 2 bonus branches (dashed) ──
	seg(5, 12, 5, 11, true),   // l27 → l5b
	seg(14, 12, 14, 11, true), // l24 → l6b
	seg(20, 12, 20, 11, true), // l22 → l7b
	// Key gate sub-path (to the right along row 14, guarded)
	seg(2, 12, 11, 14, true), // decorative dead-end branch behind key gate kg1

	// ── Area 3 main path ──
	// l29 → l30 → l31 → l32 → l33 → l34 → l35  (right along row 17)
	seg(2, 17, 5, 17), seg(5, 17, 8, 17), seg(8, 17, 11, 17), seg(11, 17, 14, 17), seg(14, 17, 17, 17), seg(17, 17, 20, 17),
	// l35 → l36  (down)
	seg(20, 17, 20, 20),
	// l36 → l37 → l38 → l39 → l40 → l41 → l42 → l43  (left along row 20)
	seg(20, 20, 17, 20), seg(17, 20, 14, 20), seg(14, 20, 11, 20), seg(11, 20, 8, 20), seg(8, 20, 5, 20), seg(5, 20, 3, 20), seg(3, 20, 2, 20),
	// l43 → sg3  (down)
	seg(2, 20, 2, 23),
	// sg3 → l44  (down)
	seg(2, 23, 2, 25),

	// ── Area 3 bonus branches (dashed) ──
	seg(8, 17, 8, 19, true),   // l31 → l8b
	seg(17, 17, 17, 19, true), // l34 → l9b
	// Key gate sub-path beyond kg2 (leads to extra treasure)
	seg(20, 20, 20, 22, true),

	// ── Area 4 main path ──
	// l44 → l45 → l46 → l47 → l48 → l49 → l50  (right along row 25)
	seg(2, 25, 5, 25), seg(5, 25, 8, 25), seg(8, 25, 11, 25), seg(11, 25, 14, 25), seg(14, 25, 17, 25), seg(17, 25, 20, 25),
	// l50 → l51  (down)
	seg(20, 25, 20, 28),
	// l51 → l52 → l53 → l54 → l55  (left along row 28)
	seg(20, 28, 17, 28), seg(17, 28, 14, 28), seg(14, 28, 11, 28), seg(11, 28, 8, 28),
	// l55 → sg4  (down from l55 col 8 to col 8 row 31)
	seg(8, 28, 8, 31),
	// sg4 → l56  (down)
	seg(8, 31, 8, 33),

	// ── Area 4 bonus branch (dashed) ──
	seg(5, 25, 5, 27, true), // l45 → l10b

	// ── Area 5 main path ──
	// l56 → l57 → l58 → l59 → l60  (right along row 33)
	seg(8, 33, 11, 33), seg(11, 33, 14, 33), seg(14, 33, 17, 33), seg(17, 33, 20, 33),
	// l60 → l61  (down)
	seg(20, 33, 20, 36),
	// l61 → l62 → l63 → l64 → l65 → l66 → l67  (left along row 36)
	seg(20, 36, 17, 36), seg(17, 36, 14, 36), seg(14, 36, 11, 36), seg(11, 36, 8, 36), seg(8, 36, 5, 36), seg(5, 36, 2, 36),
	// l67 → sg5 → portal1
	seg(2, 36, 2, 38), seg(2, 38, 2, 39)
];

// ---------------------------------------------------------------------------
// Fog regions (one per area 2–5; each lifts when that area unlocks)
// ---------------------------------------------------------------------------

const world1FogRegions: MapFogRegion[] = [
	{ area: 2, topLeft: tile(0, 8),  bottomRight: tile(23, 16) },
	{ area: 3, topLeft: tile(0, 16), bottomRight: tile(23, 24) },
	{ area: 4, topLeft: tile(0, 24), bottomRight: tile(23, 32) },
	{ area: 5, topLeft: tile(0, 32), bottomRight: tile(23, 51) }
];

// ---------------------------------------------------------------------------
// Collectibles
// ---------------------------------------------------------------------------

const world1Collectibles: MapCollectible[] = [
	// ── Area 1 ──
	{
		id: 'chest_a1_1', type: 'chest', tile: tile(14, 3), area: 1,
		label: 'Treasure Chest',
		reward: { coins: 250 }
	},
	{
		id: 'gem_a1_1', type: 'gem', tile: tile(8, 3), area: 1,
		label: 'Glowing Gem',
		reward: { coins: 150 }
	},
	{
		// The key that opens kg1 (key_area2 gate) is found in area 1
		id: 'key_area2_item', type: 'key', tile: tile(17, 3), area: 1,
		label: 'Rusted Gate Key',
		reward: { keyItemId: 'key_area2' }
	},

	// ── Area 2 ──
	{
		id: 'chest_a2_1', type: 'chest', tile: tile(8, 10), area: 2,
		label: 'Treasure Chest',
		reward: { coins: 400 }
	},
	{
		id: 'powerup_a2_hint', type: 'powerup_hint', tile: tile(17, 10), area: 2,
		label: 'Hint Powerup',
		reward: { powerup: 'hint', powerupCount: 2 }
	},
	{
		id: 'cloak_a2_1', type: 'cloak', tile: tile(20, 10), area: 2,
		label: 'Phantom Cloak',
		reward: { skinId: 3 }
	},
	{
		id: 'gem_a2_1', type: 'gem', tile: tile(11, 13), area: 2,
		label: 'Crystal Gem',
		reward: { coins: 300 }
	},

	// ── Area 3 ──
	{
		// Key for forest gate kg2
		id: 'key_forest_item', type: 'key', tile: tile(14, 18), area: 3,
		label: 'Forest Gate Key',
		reward: { keyItemId: 'key_forest' }
	},
	{
		id: 'chest_a3_1', type: 'chest', tile: tile(8, 18), area: 3,
		label: 'Treasure Chest',
		reward: { coins: 600 }
	},
	{
		id: 'gem_a3_1', type: 'gem', tile: tile(20, 19), area: 3,
		label: 'Ember Gem',
		reward: { coins: 450 }
	},
	{
		id: 'powerup_a3_time', type: 'powerup_time', tile: tile(5, 21), area: 3,
		label: 'Extra Time Powerup',
		reward: { powerup: 'extraTime', powerupCount: 2 }
	},

	// ── Area 4 ──
	{
		id: 'chest_a4_1', type: 'chest', tile: tile(14, 26), area: 4,
		label: 'Ornate Chest',
		reward: { coins: 800 }
	},
	{
		id: 'cloak_a4_1', type: 'cloak', tile: tile(20, 26), area: 4,
		label: 'Shadow Cloak',
		reward: { skinId: 7 }
	},
	{
		id: 'gem_a4_1', type: 'gem', tile: tile(17, 29), area: 4,
		label: 'Void Gem',
		reward: { coins: 700 }
	},

	// ── Area 5 ──
	{
		id: 'chest_a5_1', type: 'chest', tile: tile(14, 34), area: 5,
		label: 'Golden Chest',
		reward: { coins: 1000 }
	},
	{
		id: 'gem_a5_1', type: 'gem', tile: tile(20, 34), area: 5,
		label: 'Quantum Gem',
		reward: { coins: 900 }
	},
	{
		id: 'powerup_a5_moves', type: 'powerup_moves', tile: tile(8, 37), area: 5,
		label: 'Extra Moves Powerup',
		reward: { powerup: 'extraMoves', powerupCount: 3 }
	}
];

// ---------------------------------------------------------------------------
// Exported layout
// ---------------------------------------------------------------------------

export const WORLD_1_MAP_LAYOUT: WorldMapLayout = {
	worldId: 1,
	cols: 24,
	rows: 52,
	tileSize: 80,
	nodes: world1Nodes,
	pathSegments: world1Paths,
	collectibles: world1Collectibles,
	fogRegions: world1FogRegions
};

// ---------------------------------------------------------------------------
// World 2 Layout  (24 cols × 54 rows, tileSize = 80px)
//
// 110 levels across 16 snake legs, 7 levels per leg (last leg has 5).
// Levels sit at cols 2,5,8,11,14,17,20 (step 3).
// Odd legs go right (→); even legs go left (←).
// Rows step by 3 per leg: rows 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, 38, 41, 44, 47.
//
// 11 star gates placed on connectors or inline between adjacent levels:
//   sg1  20★  connector (20,8)→(20,11)      before l22
//   sg2  30★  inline row 11 between l22/l23   before l23
//   sg3  50★  inline row 14 between l29/l30   before l30
//   sg4  60★  inline row 17 between l39/l40   before l40
//   sg5  80★  connector (20,20)→(20,23)       before l50
//   sg6 100★  inline row 26 between l59/l60   before l60
//   sg7 120★  inline row 29 between l69/l70   before l70
//   sg8 150★  inline row 35 between l79/l80   before l80
//   sg9 200★  inline row 38 between l89/l90   before l90
//   sg10 230★ connector (2,41)→(2,44)         before l99
//   sg11 250★ connector (20,44)→(20,47)        before l106
//
// Area assignments (5 areas, 4 fog bands):
//   Area 1: legs 1–3  (l1–l21)
//   Area 2: legs 4–6  (l22–l42)
//   Area 3: legs 7–9  (l43–l63)
//   Area 4: legs 10–12 (l64–l84)
//   Area 5: legs 13–16 (l85–l110) + portal
// ---------------------------------------------------------------------------

const world2Nodes: MapNode[] = [
	// ── Area 1: l1–l21 ── (legs 1-3, rows 2-8) ─────────────────────────────

	// Leg 1 (→, row 2)
	{ id: 'w2l1',  type: 'level', tile: tile(2,  2), levelNumber: '1',  area: 1 },
	{ id: 'w2l2',  type: 'level', tile: tile(5,  2), levelNumber: '2',  area: 1 },
	{ id: 'w2l3',  type: 'level', tile: tile(8,  2), levelNumber: '3',  area: 1 },
	{ id: 'w2l4',  type: 'level', tile: tile(11, 2), levelNumber: '4',  area: 1 },
	{ id: 'w2l5',  type: 'level', tile: tile(14, 2), levelNumber: '5',  area: 1 },
	{ id: 'w2l6',  type: 'level', tile: tile(17, 2), levelNumber: '6',  area: 1 },
	{ id: 'w2l7',  type: 'level', tile: tile(20, 2), levelNumber: '7',  area: 1 },
	// Leg 2 (←, row 5)
	{ id: 'w2l8',  type: 'level', tile: tile(20, 5), levelNumber: '8',  area: 1 },
	{ id: 'w2l9',  type: 'level', tile: tile(17, 5), levelNumber: '9',  area: 1 },
	{ id: 'w2l10', type: 'level', tile: tile(14, 5), levelNumber: '10', area: 1 },
	{ id: 'w2l11', type: 'level', tile: tile(11, 5), levelNumber: '11', area: 1 },
	{ id: 'w2l12', type: 'level', tile: tile(8,  5), levelNumber: '12', area: 1 },
	{ id: 'w2l13', type: 'level', tile: tile(5,  5), levelNumber: '13', area: 1 },
	{ id: 'w2l14', type: 'level', tile: tile(2,  5), levelNumber: '14', area: 1 },
	// Leg 3 (→, row 8)
	{ id: 'w2l15', type: 'level', tile: tile(2,  8), levelNumber: '15', area: 1 },
	{ id: 'w2l16', type: 'level', tile: tile(5,  8), levelNumber: '16', area: 1 },
	{ id: 'w2l17', type: 'level', tile: tile(8,  8), levelNumber: '17', area: 1 },
	{ id: 'w2l18', type: 'level', tile: tile(11, 8), levelNumber: '18', area: 1 },
	{ id: 'w2l19', type: 'level', tile: tile(14, 8), levelNumber: '19', area: 1 },
	{ id: 'w2l20', type: 'level', tile: tile(17, 8), levelNumber: '20', area: 1 },
	{ id: 'w2l21', type: 'level', tile: tile(20, 8), levelNumber: '21', area: 1 },

	// sg1: 20★ — gates l22 (minStars:20); on connector between legs 3 and 4
	{ id: 'w2sg1', type: 'star_gate', tile: tile(20, 10), starsRequired: 35, area: 1 },

	// ── Area 2: l22–l42 ── (legs 4-6, rows 11-17) ─────────────────────────

	// Leg 4 (←, row 11)  — l22 starts area 2 section (minStars:20)
	{ id: 'w2l22', type: 'level', tile: tile(20, 11), levelNumber: '22', area: 2 },
	// sg2: 30★ — gates l23 (minStars:30); inline between l22(20,11) and l23(17,11)
	{ id: 'w2sg2', type: 'star_gate', tile: tile(19, 11), starsRequired: 55, area: 2 },
	{ id: 'w2boss1', type: 'boss', tile: tile(19, 13), levelNumber: '22-boss', starsRequired: 55, encounterTitle: 'World Boss', bossFlavor: 'The Starforged Crown guards the second sector with a living lattice.', area: 2 },
	{ id: 'w2l23', type: 'level', tile: tile(17, 11), levelNumber: '23', area: 2 },
	{ id: 'w2l24', type: 'level', tile: tile(14, 11), levelNumber: '24', area: 2 },
	{ id: 'w2l25', type: 'level', tile: tile(11, 11), levelNumber: '25', area: 2 },
	{ id: 'w2l26', type: 'level', tile: tile(8,  11), levelNumber: '26', area: 2 }, // hex
	{ id: 'w2l27', type: 'level', tile: tile(5,  11), levelNumber: '27', area: 2 }, // circular
	{ id: 'w2l28', type: 'level', tile: tile(2,  11), levelNumber: '28', area: 2 }, // triangular
	// Leg 5 (→, row 14)
	{ id: 'w2l29', type: 'level', tile: tile(2,  14), levelNumber: '29', area: 2 }, // hex
	// sg3: 50★ — gates l30 (minStars:50); inline between l29(2,14) and l30(5,14)
	{ id: 'w2sg3', type: 'star_gate', tile: tile(4,  14), starsRequired: 90, area: 2 },
	{ id: 'w2l30', type: 'level', tile: tile(5,  14), levelNumber: '30', area: 2 },
	{ id: 'w2l31', type: 'level', tile: tile(8,  14), levelNumber: '31', area: 2 },
	{ id: 'w2l32', type: 'level', tile: tile(11, 14), levelNumber: '32', area: 2 },
	{ id: 'w2l33', type: 'level', tile: tile(14, 14), levelNumber: '33', area: 2 },
	{ id: 'w2l34', type: 'level', tile: tile(17, 14), levelNumber: '34', area: 2 },
	{ id: 'w2l35', type: 'level', tile: tile(20, 14), levelNumber: '35', area: 2 },
	// Leg 6 (←, row 17)
	{ id: 'w2l36', type: 'level', tile: tile(20, 17), levelNumber: '36', area: 2 },
	{ id: 'w2l37', type: 'level', tile: tile(17, 17), levelNumber: '37', area: 2 },
	{ id: 'w2l38', type: 'level', tile: tile(14, 17), levelNumber: '38', area: 2 },
	{ id: 'w2l39', type: 'level', tile: tile(11, 17), levelNumber: '39', area: 2 },
	// sg4: 60★ — gates l40 (minStars:60); inline between l39(11,17) and l40(8,17)
	{ id: 'w2sg4', type: 'star_gate', tile: tile(10, 17), starsRequired: 110, area: 2 },
	{ id: 'w2l40', type: 'level', tile: tile(8,  17), levelNumber: '40', area: 2 },
	{ id: 'w2l41', type: 'level', tile: tile(5,  17), levelNumber: '41', area: 2 },
	{ id: 'w2l42', type: 'level', tile: tile(2,  17), levelNumber: '42', area: 2 },

	// ── Area 3: l43–l63 ── (legs 7-9, rows 20-26) ─────────────────────────

	// Leg 7 (→, row 20)
	{ id: 'w2l43', type: 'level', tile: tile(2,  20), levelNumber: '43', area: 3 },
	{ id: 'w2l44', type: 'level', tile: tile(5,  20), levelNumber: '44', area: 3 },
	{ id: 'w2l45', type: 'level', tile: tile(8,  20), levelNumber: '45', area: 3 },
	{ id: 'w2l46', type: 'level', tile: tile(11, 20), levelNumber: '46', area: 3 },
	{ id: 'w2l47', type: 'level', tile: tile(14, 20), levelNumber: '47', area: 3 },
	{ id: 'w2l48', type: 'level', tile: tile(17, 20), levelNumber: '48', area: 3 },
	{ id: 'w2l49', type: 'level', tile: tile(20, 20), levelNumber: '49', area: 3 },
	// sg5: 80★ — gates l50 (minStars:80); on connector between legs 7 and 8
	{ id: 'w2sg5', type: 'star_gate', tile: tile(20, 22), starsRequired: 145, area: 3 },
	// Leg 8 (←, row 23)
	{ id: 'w2l50', type: 'level', tile: tile(20, 23), levelNumber: '50', area: 3 },
	{ id: 'w2l51', type: 'level', tile: tile(17, 23), levelNumber: '51', area: 3 },
	{ id: 'w2l52', type: 'level', tile: tile(14, 23), levelNumber: '52', area: 3 },
	{ id: 'w2l53', type: 'level', tile: tile(11, 23), levelNumber: '53', area: 3 },
	{ id: 'w2l54', type: 'level', tile: tile(8,  23), levelNumber: '54', area: 3 },
	{ id: 'w2l55', type: 'level', tile: tile(5,  23), levelNumber: '55', area: 3 },
	{ id: 'w2l56', type: 'level', tile: tile(2,  23), levelNumber: '56', area: 3 },
	// Leg 9 (→, row 26)
	{ id: 'w2l57', type: 'level', tile: tile(2,  26), levelNumber: '57', area: 3 },
	{ id: 'w2l58', type: 'level', tile: tile(5,  26), levelNumber: '58', area: 3 },
	{ id: 'w2l59', type: 'level', tile: tile(8,  26), levelNumber: '59', area: 3 },
	// sg6: 100★ — gates l60 (minStars:100); inline between l59(8,26) and l60(11,26)
	{ id: 'w2sg6', type: 'star_gate', tile: tile(10, 26), starsRequired: 180, area: 3 },
	{ id: 'w2l60', type: 'level', tile: tile(11, 26), levelNumber: '60', area: 3 },
	{ id: 'w2l61', type: 'level', tile: tile(14, 26), levelNumber: '61', area: 3 },
	{ id: 'w2l62', type: 'level', tile: tile(17, 26), levelNumber: '62', area: 3 },
	{ id: 'w2l63', type: 'level', tile: tile(20, 26), levelNumber: '63', area: 3 },

	// ── Area 4: l64–l84 ── (legs 10-12, rows 29-35) ────────────────────────

	// Leg 10 (←, row 29)
	{ id: 'w2l64', type: 'level', tile: tile(20, 29), levelNumber: '64', area: 4 },
	{ id: 'w2l65', type: 'level', tile: tile(17, 29), levelNumber: '65', area: 4 },
	{ id: 'w2l66', type: 'level', tile: tile(14, 29), levelNumber: '66', area: 4 },
	{ id: 'w2l67', type: 'level', tile: tile(11, 29), levelNumber: '67', area: 4 },
	{ id: 'w2l68', type: 'level', tile: tile(8,  29), levelNumber: '68', area: 4 },
	{ id: 'w2l69', type: 'level', tile: tile(5,  29), levelNumber: '69', area: 4 },
	// sg7: 120★ — gates l70 (minStars:120); inline between l69(5,29) and l70(2,29)
	{ id: 'w2sg7', type: 'star_gate', tile: tile(3,  29), starsRequired: 215, area: 4 },
	{ id: 'w2l70', type: 'level', tile: tile(2,  29), levelNumber: '70', area: 4 },
	// Leg 11 (→, row 32)
	{ id: 'w2l71', type: 'level', tile: tile(2,  32), levelNumber: '71', area: 4 },
	{ id: 'w2l72', type: 'level', tile: tile(5,  32), levelNumber: '72', area: 4 },
	{ id: 'w2l73', type: 'level', tile: tile(8,  32), levelNumber: '73', area: 4 },
	{ id: 'w2l74', type: 'level', tile: tile(11, 32), levelNumber: '74', area: 4 },
	{ id: 'w2l75', type: 'level', tile: tile(14, 32), levelNumber: '75', area: 4 },
	{ id: 'w2l76', type: 'level', tile: tile(17, 32), levelNumber: '76', area: 4 },
	{ id: 'w2l77', type: 'level', tile: tile(20, 32), levelNumber: '77', area: 4 },
	// Leg 12 (←, row 35)
	{ id: 'w2l78', type: 'level', tile: tile(20, 35), levelNumber: '78', area: 4 },
	{ id: 'w2l79', type: 'level', tile: tile(17, 35), levelNumber: '79', area: 4 },
	// sg8: 150★ — gates l80 (minStars:150); inline between l79(17,35) and l80(14,35)
	{ id: 'w2sg8', type: 'star_gate', tile: tile(16, 35), starsRequired: 270, area: 4 },
	{ id: 'w2l80', type: 'level', tile: tile(14, 35), levelNumber: '80', area: 4 },
	{ id: 'w2l81', type: 'level', tile: tile(11, 35), levelNumber: '81', area: 4 },
	{ id: 'w2l82', type: 'level', tile: tile(8,  35), levelNumber: '82', area: 4 },
	{ id: 'w2l83', type: 'level', tile: tile(5,  35), levelNumber: '83', area: 4 },
	{ id: 'w2l84', type: 'level', tile: tile(2,  35), levelNumber: '84', area: 4 },

	// ── Area 5: l85–l110 ── (legs 13-16, rows 38-47) ───────────────────────

	// Leg 13 (→, row 38)
	{ id: 'w2l85', type: 'level', tile: tile(2,  38), levelNumber: '85', area: 5 },
	{ id: 'w2l86', type: 'level', tile: tile(5,  38), levelNumber: '86', area: 5 },
	{ id: 'w2l87', type: 'level', tile: tile(8,  38), levelNumber: '87', area: 5 },
	{ id: 'w2l88', type: 'level', tile: tile(11, 38), levelNumber: '88', area: 5 },
	{ id: 'w2l89', type: 'level', tile: tile(14, 38), levelNumber: '89', area: 5 },
	// sg9: 200★ — gates l90 (minStars:200); inline between l89(14,38) and l90(17,38)
	{ id: 'w2sg9', type: 'star_gate', tile: tile(16, 38), starsRequired: 360, area: 5 },
	{ id: 'w2l90', type: 'level', tile: tile(17, 38), levelNumber: '90', area: 5 },
	{ id: 'w2l91', type: 'level', tile: tile(20, 38), levelNumber: '91', area: 5 },
	// Leg 14 (←, row 41)
	{ id: 'w2l92',  type: 'level', tile: tile(20, 41), levelNumber: '92',  area: 5 },
	{ id: 'w2l93',  type: 'level', tile: tile(17, 41), levelNumber: '93',  area: 5 },
	{ id: 'w2l94',  type: 'level', tile: tile(14, 41), levelNumber: '94',  area: 5 },
	{ id: 'w2l95',  type: 'level', tile: tile(11, 41), levelNumber: '95',  area: 5 },
	{ id: 'w2l96',  type: 'level', tile: tile(8,  41), levelNumber: '96',  area: 5 },
	{ id: 'w2l97',  type: 'level', tile: tile(5,  41), levelNumber: '97',  area: 5 },
	{ id: 'w2l98',  type: 'level', tile: tile(2,  41), levelNumber: '98',  area: 5 },
	// sg10: 230★ — on connector between legs 14 and 15
	{ id: 'w2sg10', type: 'star_gate', tile: tile(2,  43), starsRequired: 415, area: 5 },
	// Leg 15 (→, row 44)
	{ id: 'w2l99',  type: 'level', tile: tile(2,  44), levelNumber: '99',  area: 5 },
	{ id: 'w2l100', type: 'level', tile: tile(5,  44), levelNumber: '100', area: 5 },
	{ id: 'w2l101', type: 'level', tile: tile(8,  44), levelNumber: '101', area: 5 },
	{ id: 'w2l102', type: 'level', tile: tile(11, 44), levelNumber: '102', area: 5 },
	{ id: 'w2l103', type: 'level', tile: tile(14, 44), levelNumber: '103', area: 5 },
	{ id: 'w2l104', type: 'level', tile: tile(17, 44), levelNumber: '104', area: 5 },
	{ id: 'w2l105', type: 'level', tile: tile(20, 44), levelNumber: '105', area: 5 },
	// sg11: 250★ — on connector between legs 15 and 16; final gate before portal
	{ id: 'w2sg11', type: 'star_gate', tile: tile(20, 46), starsRequired: 470, area: 5 },
	// Leg 16 (←, row 47) — 5 levels (l106–l110)
	{ id: 'w2l106', type: 'level', tile: tile(20, 47), levelNumber: '106', area: 5 },
	{ id: 'w2l107', type: 'level', tile: tile(17, 47), levelNumber: '107', area: 5 },
	{ id: 'w2l108', type: 'level', tile: tile(14, 47), levelNumber: '108', area: 5 },
	{ id: 'w2l109', type: 'level', tile: tile(11, 47), levelNumber: '109', area: 5 },
	{ id: 'w2l110', type: 'level', tile: tile(8,  47), levelNumber: '110', area: 5 },

	// World 2 portal (end of world 2, unlocks World 3)
	{ id: 'portal2', type: 'portal', tile: tile(8, 50), area: 5 }
];

// ---------------------------------------------------------------------------
// World 2 path segments
// ---------------------------------------------------------------------------

const world2Paths: MapPathSegment[] = [
	// ── Leg 1 (→, row 2) ──
	seg(2,2,5,2), seg(5,2,8,2), seg(8,2,11,2), seg(11,2,14,2), seg(14,2,17,2), seg(17,2,20,2),
	// connector leg 1→2
	seg(20,2,20,5),
	// ── Leg 2 (←, row 5) ──
	seg(20,5,17,5), seg(17,5,14,5), seg(14,5,11,5), seg(11,5,8,5), seg(8,5,5,5), seg(5,5,2,5),
	// connector leg 2→3
	seg(2,5,2,8),
	// ── Leg 3 (→, row 8) ──
	seg(2,8,5,8), seg(5,8,8,8), seg(8,8,11,8), seg(11,8,14,8), seg(14,8,17,8), seg(17,8,20,8),
	// connector leg 3 → sg1 → leg 4 start
	seg(20,8,20,10), seg(20,10,20,11),
	// ── Leg 4 (←, row 11): l22 → sg2 → l23 … l28 ──
	seg(20,11,19,11), seg(19,11,17,11), seg(17,11,14,11), seg(14,11,11,11), seg(11,11,8,11), seg(8,11,5,11), seg(5,11,2,11),
	seg(19,11,19,13,true),
	// connector leg 4→5
	seg(2,11,2,14),
	// ── Leg 5 (→, row 14): l29 → sg3 → l30 … l35 ──
	seg(2,14,4,14), seg(4,14,5,14), seg(5,14,8,14), seg(8,14,11,14), seg(11,14,14,14), seg(14,14,17,14), seg(17,14,20,14),
	// connector leg 5→6
	seg(20,14,20,17),
	// ── Leg 6 (←, row 17): l36 … l39 → sg4 → l40 … l42 ──
	seg(20,17,17,17), seg(17,17,14,17), seg(14,17,11,17), seg(11,17,10,17), seg(10,17,8,17), seg(8,17,5,17), seg(5,17,2,17),
	// connector leg 6→7
	seg(2,17,2,20),
	// ── Leg 7 (→, row 20) ──
	seg(2,20,5,20), seg(5,20,8,20), seg(8,20,11,20), seg(11,20,14,20), seg(14,20,17,20), seg(17,20,20,20),
	// connector leg 7 → sg5 → leg 8 start
	seg(20,20,20,22), seg(20,22,20,23),
	// ── Leg 8 (←, row 23) ──
	seg(20,23,17,23), seg(17,23,14,23), seg(14,23,11,23), seg(11,23,8,23), seg(8,23,5,23), seg(5,23,2,23),
	// connector leg 8→9
	seg(2,23,2,26),
	// ── Leg 9 (→, row 26): l57 … l59 → sg6 → l60 … l63 ──
	seg(2,26,5,26), seg(5,26,8,26), seg(8,26,10,26), seg(10,26,11,26), seg(11,26,14,26), seg(14,26,17,26), seg(17,26,20,26),
	// connector leg 9→10
	seg(20,26,20,29),
	// ── Leg 10 (←, row 29): l64 … l69 → sg7 → l70 ──
	seg(20,29,17,29), seg(17,29,14,29), seg(14,29,11,29), seg(11,29,8,29), seg(8,29,5,29), seg(5,29,3,29), seg(3,29,2,29),
	// connector leg 10→11
	seg(2,29,2,32),
	// ── Leg 11 (→, row 32) ──
	seg(2,32,5,32), seg(5,32,8,32), seg(8,32,11,32), seg(11,32,14,32), seg(14,32,17,32), seg(17,32,20,32),
	// connector leg 11→12
	seg(20,32,20,35),
	// ── Leg 12 (←, row 35): l78 → l79 → sg8 → l80 … l84 ──
	seg(20,35,17,35), seg(17,35,16,35), seg(16,35,14,35), seg(14,35,11,35), seg(11,35,8,35), seg(8,35,5,35), seg(5,35,2,35),
	// connector leg 12→13
	seg(2,35,2,38),
	// ── Leg 13 (→, row 38): l85 … l89 → sg9 → l90 → l91 ──
	seg(2,38,5,38), seg(5,38,8,38), seg(8,38,11,38), seg(11,38,14,38), seg(14,38,16,38), seg(16,38,17,38), seg(17,38,20,38),
	// connector leg 13→14
	seg(20,38,20,41),
	// ── Leg 14 (←, row 41) ──
	seg(20,41,17,41), seg(17,41,14,41), seg(14,41,11,41), seg(11,41,8,41), seg(8,41,5,41), seg(5,41,2,41),
	// connector leg 14 → sg10 → leg 15 start
	seg(2,41,2,43), seg(2,43,2,44),
	// ── Leg 15 (→, row 44) ──
	seg(2,44,5,44), seg(5,44,8,44), seg(8,44,11,44), seg(11,44,14,44), seg(14,44,17,44), seg(17,44,20,44),
	// connector leg 15 → sg11 → leg 16 start
	seg(20,44,20,46), seg(20,46,20,47),
	// ── Leg 16 (←, row 47): l106 … l110 ──
	seg(20,47,17,47), seg(17,47,14,47), seg(14,47,11,47), seg(11,47,8,47),
	// l110 → portal
	seg(8,47,8,50)
];

// ---------------------------------------------------------------------------
// World 2 fog regions
// ---------------------------------------------------------------------------

const world2FogRegions: MapFogRegion[] = [
	{ area: 2, topLeft: tile(0, 9),  bottomRight: tile(23, 19) },
	{ area: 3, topLeft: tile(0, 19), bottomRight: tile(23, 28) },
	{ area: 4, topLeft: tile(0, 28), bottomRight: tile(23, 37) },
	{ area: 5, topLeft: tile(0, 37), bottomRight: tile(23, 53) }
];

// ---------------------------------------------------------------------------
// World 2 collectibles
// ---------------------------------------------------------------------------

const world2Collectibles: MapCollectible[] = [
	// ── Area 1 ──
	{
		id: 'w2_chest_a1_1', type: 'chest', tile: tile(8, 3), area: 1,
		label: 'Asteroid Chest',
		reward: { coins: 200 }
	},
	{
		id: 'w2_gem_a1_1', type: 'gem', tile: tile(14, 3), area: 1,
		label: 'Quasar Gem',
		reward: { coins: 150 }
	},
	{
		id: 'w2_powerup_a1_hint', type: 'powerup_hint', tile: tile(17, 6), area: 1,
		label: 'Navigator Beacon',
		reward: { powerup: 'hint', powerupCount: 2 }
	},

	// ── Area 2 ──
	{
		id: 'w2_chest_a2_skin', type: 'cloak', tile: tile(11, 12), area: 2,
		label: 'Space Maze Fragment',
		reward: { skinId: 13 }
	},
	{
		id: 'w2_gem_a2_1', type: 'gem', tile: tile(17, 12), area: 2,
		label: 'Nebula Gem',
		reward: { coins: 300 }
	},
	{
		id: 'w2_powerup_a2_time', type: 'powerup_time', tile: tile(8, 15), area: 2,
		label: 'Warp Drive',
		reward: { powerup: 'extraTime', powerupCount: 2 }
	},
	{
		id: 'w2_chest_a2_2', type: 'chest', tile: tile(14, 18), area: 2,
		label: 'Ion Chest',
		reward: { coins: 500 }
	},

	// ── Area 3 ──
	{
		id: 'w2_gem_a3_1', type: 'gem', tile: tile(5, 21), area: 3,
		label: 'Pulsar Gem',
		reward: { coins: 400 }
	},
	{
		id: 'w2_chest_a3_1', type: 'chest', tile: tile(14, 24), area: 3,
		label: 'Galactic Chest',
		reward: { coins: 650 }
	},
	{
		id: 'w2_powerup_a3_moves', type: 'powerup_moves', tile: tile(17, 27), area: 3,
		label: 'Thruster Boost',
		reward: { powerup: 'extraMoves', powerupCount: 3 }
	},

	// ── Area 4 ──
	{
		id: 'w2_chest_a4_skin', type: 'cloak', tile: tile(8, 30), area: 4,
		label: 'Void Fragment',
		reward: { skinId: 15 }
	},
	{
		id: 'w2_gem_a4_1', type: 'gem', tile: tile(17, 33), area: 4,
		label: 'Dark Matter Gem',
		reward: { coins: 700 }
	},
	{
		id: 'w2_powerup_a4_hint', type: 'powerup_hint', tile: tile(5, 36), area: 4,
		label: 'Stellar Compass',
		reward: { powerup: 'hint', powerupCount: 2 }
	},

	// ── Area 5 ──
	{
		id: 'w2_chest_a5_skin', type: 'cloak', tile: tile(11, 39), area: 5,
		label: 'Butler\'s Crest',
		reward: { skinId: 17 }
	},
	{
		id: 'w2_gem_a5_1', type: 'gem', tile: tile(17, 42), area: 5,
		label: 'Supernova Gem',
		reward: { coins: 800 }
	},
	{
		id: 'w2_powerup_a5_moves', type: 'powerup_moves', tile: tile(5, 45), area: 5,
		label: 'Gravity Boots',
		reward: { powerup: 'extraMoves', powerupCount: 3 }
	},
	{
		id: 'w2_chest_a5_2', type: 'chest', tile: tile(14, 48), area: 5,
		label: 'Cosmic Vault',
		reward: { coins: 1000 }
	}
];

// ---------------------------------------------------------------------------
// Exported layout
// ---------------------------------------------------------------------------

export const WORLD_2_MAP_LAYOUT: WorldMapLayout = {
	worldId: 2,
	cols: 24,
	rows: 54,
	tileSize: 80,
	nodes: world2Nodes,
	pathSegments: world2Paths,
	collectibles: world2Collectibles,
	fogRegions: world2FogRegions
};

export function getWorldMapLayout(worldId: number): WorldMapLayout | null {
	if (worldId === 1) return WORLD_1_MAP_LAYOUT;
	if (worldId === 2) return WORLD_2_MAP_LAYOUT;
	return null;
}
