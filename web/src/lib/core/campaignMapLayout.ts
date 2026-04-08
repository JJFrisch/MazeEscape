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
	{ id: 'sg1', type: 'star_gate', tile: tile(2, 7), starsRequired: 20, area: 2 },

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
	{ id: 'sg2', type: 'star_gate', tile: tile(2, 15), starsRequired: 45, area: 3 },

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
	{ id: 'sg3', type: 'star_gate', tile: tile(2, 23), starsRequired: 80, area: 4 },

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
	{ id: 'sg4', type: 'star_gate', tile: tile(8, 31), starsRequired: 120, area: 5 },

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
	{ id: 'sg5', type: 'star_gate', tile: tile(2, 38), starsRequired: 250, area: 5 }
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

export function getWorldMapLayout(worldId: number): WorldMapLayout | null {
	if (worldId === 1) return WORLD_1_MAP_LAYOUT;
	return null;
}
