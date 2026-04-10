/**
 * Campaign level definitions.
 * Ported from C# PlayerDataModel.InitializeWorld1Levels() and InitializeWorld2Levels().
 *
 * Each level specifies:
 * - Grid dimensions
 * - Generation algorithm
 * - Star thresholds (moves for 2★, time for 3★)
 * - Connections to next levels
 * - Minimum stars required to unlock
 */

import type { CampaignLevel, MazeAlgorithm, MazeShape, LevelReward } from './types';

interface LevelDef {
	num: string;
	w: number;
	h: number;
	type: MazeAlgorithm;
	kind?: 'standard' | 'boss';
	twoStar: number;
	threeStar: number;
	connects?: string[];
	minStars?: number;
	shape?: MazeShape;
	reward?: LevelReward;
	bossFlavor?: string;
}

function buildLevel(def: LevelDef, id: number): CampaignLevel {
	const connects = def.connects ?? [];
	// Auto-connect to next sequential level for non-bonus levels
	if (!def.num.includes('b')) {
		const next = String(parseInt(def.num) + 1);
		if (!connects.includes(next)) connects.push(next);
	}
	return {
		levelId: id,
		levelNumber: def.num,
		levelKind: def.kind ?? 'standard',
		width: def.w,
		height: def.h,
		levelType: def.type,
		mazeShape: def.shape ?? 'rectangular',
		twoStarMoves: def.twoStar,
		threeStarTime: def.threeStar,
		fiveStarMoves: Math.floor(def.twoStar * 0.6),
		fiveStarTime: Math.floor(def.threeStar * 0.6),
		numberOfStars: 0,
		minimumStarsToUnlock: def.minStars ?? 0,
		connectTo1: connects[0] ?? null,
		connectTo2: connects[1] ?? null,
		...(def.reward ? { levelReward: def.reward } : {}),
		completed: false,
		star1: false,
		star2: false,
		star3: false,
		star4: false,
		star5: false,
		bestMoves: 0,
		bestTime: 0,
		bossFlavor: def.bossFlavor
	};
}

// ---------------------------------------------------------------------------
// World 1: Cybernetic Labyrinths (67 levels)
// ---------------------------------------------------------------------------
const world1Defs: LevelDef[] = [
	// Area 1 (Levels 1–14 + Bonus 1b–4b)
	{ num: '1', w: 3, h: 3, type: 'backtracking', twoStar: 11, threeStar: 12 },
	{ num: '2', w: 3, h: 3, type: 'huntAndKill', twoStar: 11, threeStar: 12, connects: ['3', '1b'] },
	{ num: '1b', w: 3, h: 4, type: 'backtracking', twoStar: 14, threeStar: 16 },
	{ num: '3', w: 4, h: 3, type: 'backtracking', twoStar: 12, threeStar: 12 },
	{ num: '4', w: 4, h: 4, type: 'huntAndKill', twoStar: 17, threeStar: 15, connects: ['5', '2b'] },
	{ num: '2b', w: 4, h: 4, type: 'prims', twoStar: 18, threeStar: 16 },
	{ num: '5', w: 4, h: 4, type: 'growingTree_50_50', twoStar: 18, threeStar: 15 },
	{ num: '6', w: 5, h: 4, type: 'sidewinder', twoStar: 21, threeStar: 16 },
	{ num: '7', w: 5, h: 5, type: 'huntAndKill', twoStar: 26, threeStar: 18, connects: ['8', '3b'] },
	{ num: '3b', w: 5, h: 5, type: 'growingTree_75_25', twoStar: 29, threeStar: 20, connects: ['8'] },
	{ num: '8', w: 5, h: 5, type: 'prims', twoStar: 27, threeStar: 18, connects: ['9'] },
	{ num: '9', w: 6, h: 5, type: 'backtracking', twoStar: 30, threeStar: 20 },
	{ num: '10', w: 6, h: 6, type: 'kruskals', twoStar: 36, threeStar: 23, connects: ['11', '4b'] },
	{ num: '4b', w: 6, h: 5, type: 'growingTree_25_75', twoStar: 32, threeStar: 20 },
	{ num: '11', w: 6, h: 6, type: 'huntAndKill', twoStar: 38, threeStar: 23 },
	{ num: '12', w: 7, h: 6, type: 'binaryTree', twoStar: 41, threeStar: 24 },
	{ num: '13', w: 7, h: 7, type: 'growingTree_50_50', twoStar: 45, threeStar: 26 },
{ num: '14', w: 7, h: 7, type: 'prims', twoStar: 47, threeStar: 26, reward: { type: 'key', label: 'Rusted Gate Key', reward: { keyItemId: 'key_area2' } } },
	{ num: '14-boss', w: 20, h: 20, type: 'recursiveDivision', kind: 'boss', twoStar: 150, threeStar: 70, connects: ['15'], reward: { type: 'boss_relic', label: 'Labyrinth Core', reward: { specialItemId: 'labyrinth_core' } }, bossFlavor: 'A sentinel intellect folds the corridors into a single war chamber.' },

	// Area 2 (Levels 15–28 + Bonus 5b–7b)
	{ num: '15', w: 8, h: 7, type: 'wilsons', twoStar: 49, threeStar: 28, minStars: 20 },
	{ num: '16', w: 8, h: 8, type: 'huntAndKill', twoStar: 54, threeStar: 31, connects: ['17', '5b'] },
	{ num: '5b', w: 8, h: 7, type: 'kruskals', twoStar: 51, threeStar: 29 },
	{ num: '17', w: 8, h: 8, type: 'growingTree_75_25', twoStar: 57, threeStar: 31 },
	{ num: '18', w: 9, h: 8, type: 'backtracking', twoStar: 60, threeStar: 32 },
	{ num: '19', w: 9, h: 9, type: 'prims', twoStar: 66, threeStar: 34, connects: ['20', '6b'] },
	{ num: '6b', w: 9, h: 8, type: 'growingTree_50_0', twoStar: 62, threeStar: 32 },
	{ num: '20', w: 9, h: 9, type: 'huntAndKill', twoStar: 68, threeStar: 34 },
	{ num: '21', w: 10, h: 9, type: 'ellers', twoStar: 72, threeStar: 36 },
	{ num: '22', w: 10, h: 10, type: 'growingTree_25_75', twoStar: 79, threeStar: 39, connects: ['23', '7b'] },
	{ num: '7b', w: 10, h: 9, type: 'kruskals', twoStar: 74, threeStar: 36 },
	{ num: '23', w: 10, h: 10, type: 'prims', twoStar: 81, threeStar: 39 },
	{ num: '24', w: 11, h: 10, type: 'recursiveDivision', twoStar: 87, threeStar: 40 },
	{ num: '25', w: 11, h: 11, type: 'huntAndKill', twoStar: 94, threeStar: 42 },
	{ num: '26', w: 11, h: 11, type: 'growingTree_50_50', twoStar: 96, threeStar: 42 },
	{ num: '27', w: 12, h: 11, type: 'backtracking', twoStar: 102, threeStar: 44 },
{ num: '28', w: 12, h: 12, type: 'prims', twoStar: 109, threeStar: 47, reward: { type: 'gem', label: 'Crystal Gem', reward: { coins: 350 } } },

	// Area 3 (Levels 29–43 + Bonus 8b–9b)
	{ num: '29', w: 12, h: 12, type: 'huntAndKill', twoStar: 111, threeStar: 47, minStars: 45 },
	{ num: '30', w: 13, h: 12, type: 'aldousBroder', twoStar: 117, threeStar: 48 },
	{ num: '31', w: 13, h: 13, type: 'growingTree_75_25', twoStar: 126, threeStar: 50, connects: ['32', '8b'] },
	{ num: '8b', w: 13, h: 12, type: 'kruskals', twoStar: 119, threeStar: 48 },
	{ num: '32', w: 13, h: 13, type: 'prims', twoStar: 128, threeStar: 50 },
	{ num: '33', w: 14, h: 13, type: 'backtracking', twoStar: 134, threeStar: 52 },
	{ num: '34', w: 14, h: 14, type: 'huntAndKill', twoStar: 143, threeStar: 55 },
	{ num: '35', w: 14, h: 14, type: 'growingTree_50_0', twoStar: 144, threeStar: 55 },
	{ num: '36', w: 15, h: 14, type: 'sidewinder', twoStar: 150, threeStar: 56 },
	{ num: '37', w: 15, h: 15, type: 'prims', twoStar: 162, threeStar: 58, connects: ['38', '9b'] },
	{ num: '9b', w: 15, h: 14, type: 'growingTree_25_75', twoStar: 154, threeStar: 56 },
	{ num: '38', w: 15, h: 15, type: 'huntAndKill', twoStar: 164, threeStar: 58 },
	{ num: '39', w: 16, h: 15, type: 'wilsons', twoStar: 171, threeStar: 60 },
	{ num: '40', w: 16, h: 16, type: 'growingTree_50_50', twoStar: 184, threeStar: 63 },
	{ num: '41', w: 16, h: 16, type: 'kruskals', twoStar: 186, threeStar: 63 },
	{ num: '42', w: 17, h: 16, type: 'ellers', twoStar: 194, threeStar: 64 },
{ num: '43', w: 17, h: 17, type: 'prims', twoStar: 207, threeStar: 66, reward: { type: 'key', label: 'Forest Gate Key', reward: { keyItemId: 'key_forest' } } },

	// Area 4 (Levels 44–55 + Bonus 10b–20b)
	{ num: '44', w: 17, h: 17, type: 'huntAndKill', twoStar: 209, threeStar: 66, minStars: 80 },
	{ num: '45', w: 18, h: 17, type: 'backtracking', twoStar: 216, threeStar: 68, connects: ['46', '10b'] },
	{ num: '10b', w: 17, h: 17, type: 'growingTree_75_25', twoStar: 210, threeStar: 66 },
	{ num: '46', w: 18, h: 18, type: 'prims', twoStar: 229, threeStar: 71 },
	{ num: '47', w: 18, h: 18, type: 'growingTree_50_0', twoStar: 231, threeStar: 71 },
	{ num: '48', w: 19, h: 18, type: 'recursiveDivision', twoStar: 239, threeStar: 72 },
	{ num: '49', w: 19, h: 19, type: 'huntAndKill', twoStar: 252, threeStar: 74 },
	{ num: '50', w: 19, h: 19, type: 'kruskals', twoStar: 254, threeStar: 74 },
	{ num: '51', w: 20, h: 19, type: 'aldousBroder', twoStar: 261, threeStar: 76 },
	{ num: '52', w: 20, h: 20, type: 'prims', twoStar: 276, threeStar: 79 },
	{ num: '53', w: 20, h: 20, type: 'growingTree_50_50', twoStar: 279, threeStar: 79 },
	{ num: '54', w: 21, h: 20, type: 'backtracking', twoStar: 287, threeStar: 80, reward: { type: 'powerup_hint', label: "Navigator's Compass", reward: { powerup: 'hint', powerupCount: 3 } } },
	{ num: '55', w: 21, h: 21, type: 'huntAndKill', twoStar: 302, threeStar: 82, reward: { type: 'cloak', label: 'Shadow Cloak', reward: { skinId: 7 } } },

	// Area 5 (Levels 56–67)
	{ num: '56', w: 21, h: 21, type: 'growingTree_75_25', twoStar: 304, threeStar: 82, minStars: 120 },
	{ num: '57', w: 22, h: 21, type: 'binaryTree', twoStar: 312, threeStar: 84 },
	{ num: '58', w: 22, h: 22, type: 'prims', twoStar: 329, threeStar: 87 },
	{ num: '59', w: 22, h: 22, type: 'kruskals', twoStar: 332, threeStar: 87 },
	{ num: '60', w: 23, h: 22, type: 'backtracking', twoStar: 339, threeStar: 88 },
	{ num: '61', w: 23, h: 23, type: 'huntAndKill', twoStar: 354, threeStar: 90 },
	{ num: '62', w: 23, h: 23, type: 'growingTree_25_75', twoStar: 357, threeStar: 90 },
	{ num: '63', w: 24, h: 23, type: 'backtracking', twoStar: 366, threeStar: 92 },
	{ num: '64', w: 24, h: 24, type: 'prims', twoStar: 383, threeStar: 95 },
	{ num: '65', w: 24, h: 24, type: 'growingTree_50_0', twoStar: 386, threeStar: 95 },
	{ num: '66', w: 25, h: 24, type: 'backtracking', twoStar: 394, threeStar: 96 },
	{ num: '67', w: 25, h: 25, type: 'huntAndKill', twoStar: 411, threeStar: 98, connects: [], reward: { type: 'gem', label: 'Cyber Core Gem', reward: { coins: 1000 } } }
];

// ---------------------------------------------------------------------------
// World 2: Galactic Grids (110 main levels, fully defined)
// ---------------------------------------------------------------------------
const world2Defs: LevelDef[] = [
	// Area 1 (Levels 1–22)
	{ num: '1', w: 3, h: 3, type: 'huntAndKill', twoStar: 11, threeStar: 12 },
	{ num: '2', w: 3, h: 4, type: 'backtracking', twoStar: 13, threeStar: 13 },
	{ num: '3', w: 4, h: 4, type: 'prims', twoStar: 17, threeStar: 15 },
	{ num: '4', w: 4, h: 4, type: 'growingTree_50_0', twoStar: 18, threeStar: 15 },
	{ num: '5', w: 5, h: 4, type: 'sidewinder', twoStar: 21, threeStar: 16 },
	{ num: '6', w: 5, h: 5, type: 'huntAndKill', twoStar: 26, threeStar: 18 },
	{ num: '7', w: 5, h: 5, type: 'kruskals', twoStar: 27, threeStar: 18 },
	{ num: '8', w: 6, h: 5, type: 'binaryTree', twoStar: 30, threeStar: 20 },
	{ num: '9', w: 6, h: 6, type: 'growingTree_75_25', twoStar: 36, threeStar: 23 },
	{ num: '10', w: 6, h: 6, type: 'prims', twoStar: 38, threeStar: 23 },
	{ num: '11', w: 7, h: 6, type: 'wilsons', twoStar: 42, threeStar: 24 },
	{ num: '12', w: 7, h: 7, type: 'huntAndKill', twoStar: 47, threeStar: 26 },
	{ num: '13', w: 7, h: 7, type: 'growingTree_25_75', twoStar: 48, threeStar: 26 },
	{ num: '14', w: 8, h: 7, type: 'aldousBroder', twoStar: 51, threeStar: 28 },
	{ num: '15', w: 8, h: 8, type: 'prims', twoStar: 57, threeStar: 31 },
	{ num: '16', w: 8, h: 8, type: 'kruskals', twoStar: 59, threeStar: 31 },
	{ num: '17', w: 9, h: 8, type: 'ellers', twoStar: 62, threeStar: 32 },
	{ num: '18', w: 9, h: 9, type: 'huntAndKill', twoStar: 69, threeStar: 34 },
	{ num: '19', w: 9, h: 9, type: 'growingTree_50_50', twoStar: 71, threeStar: 34 },
	{ num: '20', w: 10, h: 9, type: 'recursiveDivision', twoStar: 74, threeStar: 36 },
	{ num: '21', w: 10, h: 10, type: 'prims', twoStar: 81, threeStar: 39 },
	{ num: '22', w: 10, h: 10, type: 'growingTree_50_0', twoStar: 83, threeStar: 39, minStars: 20 },
	{ num: '22-boss', w: 24, h: 24, type: 'recursiveDivision', kind: 'boss', twoStar: 220, threeStar: 92, connects: ['23'], reward: { type: 'boss_relic', label: 'Starforged Crown', reward: { specialItemId: 'starforged_crown' } }, bossFlavor: 'A celestial war engine seals the next sector behind a living lattice.' },

	// Area 2 (Levels 23–45)
	{ num: '23', w: 11, h: 10, type: 'recursiveDivision', twoStar: 87, threeStar: 40, minStars: 30 },
	{ num: '24', w: 11, h: 11, type: 'huntAndKill', twoStar: 94, threeStar: 42 },
	{ num: '25', w: 11, h: 11, type: 'kruskals', twoStar: 96, threeStar: 42 },
	// Hex, circular & triangular maze shapes introduced
	{ num: '26', w: 7, h: 6, type: 'backtracking', twoStar: 38, threeStar: 24, shape: 'hexagonal' },
	{ num: '27', w: 5, h: 5, type: 'backtracking', twoStar: 23, threeStar: 20, shape: 'circular' },
	{ num: '28', w: 8, h: 6, type: 'backtracking', twoStar: 42, threeStar: 26, shape: 'triangular' },
	{ num: '29', w: 9, h: 7, type: 'backtracking', twoStar: 49, threeStar: 31, shape: 'hexagonal' },
	{ num: '30', w: 13, h: 12, type: 'sidewinder', twoStar: 117, threeStar: 48, minStars: 50 },
	{ num: '31', w: 13, h: 13, type: 'huntAndKill', twoStar: 123, threeStar: 50 },
	{ num: '32', w: 14, h: 13, type: 'kruskals', twoStar: 130, threeStar: 52 },
	{ num: '33', w: 14, h: 14, type: 'prims', twoStar: 137, threeStar: 52 },
	{ num: '34', w: 14, h: 14, type: 'growingTree_50_0', twoStar: 144, threeStar: 54 },
	{ num: '35', w: 15, h: 14, type: 'growingTree_50_50', twoStar: 150, threeStar: 56 },
	{ num: '36', w: 15, h: 15, type: 'growingTree_75_25', twoStar: 157, threeStar: 57 },
	{ num: '37', w: 15, h: 15, type: 'growingTree_25_75', twoStar: 164, threeStar: 58 },
	{ num: '38', w: 16, h: 15, type: 'wilsons', twoStar: 171, threeStar: 60 },
	{ num: '39', w: 16, h: 15, type: 'prims', twoStar: 177, threeStar: 61 },
	{ num: '40', w: 16, h: 16, type: 'growingTree_50_50', twoStar: 184, threeStar: 63, minStars: 60 },
	{ num: '41', w: 16, h: 16, type: 'huntAndKill', twoStar: 191, threeStar: 64 },
	{ num: '42', w: 17, h: 16, type: 'kruskals', twoStar: 198, threeStar: 65 },
	{ num: '43', w: 17, h: 17, type: 'prims', twoStar: 204, threeStar: 66 },
	{ num: '44', w: 17, h: 17, type: 'growingTree_50_0', twoStar: 211, threeStar: 68 },
	{ num: '45', w: 18, h: 17, type: 'growingTree_50_50', twoStar: 218, threeStar: 68 },

	// Area 3 (Levels 46–65)
	{ num: '46', w: 18, h: 18, type: 'growingTree_75_25', twoStar: 225, threeStar: 70 },
	{ num: '47', w: 18, h: 18, type: 'growingTree_25_75', twoStar: 231, threeStar: 71 },
	{ num: '48', w: 19, h: 18, type: 'binaryTree', twoStar: 238, threeStar: 72 },
	{ num: '49', w: 19, h: 18, type: 'prims', twoStar: 245, threeStar: 73 },
	{ num: '50', w: 19, h: 19, type: 'kruskals', twoStar: 254, threeStar: 74, minStars: 80 },
	{ num: '51', w: 20, h: 19, type: 'huntAndKill', twoStar: 262, threeStar: 76 },
	{ num: '52', w: 20, h: 20, type: 'kruskals', twoStar: 271, threeStar: 77 },
	{ num: '53', w: 20, h: 20, type: 'prims', twoStar: 279, threeStar: 78 },
	{ num: '54', w: 21, h: 20, type: 'growingTree_50_0', twoStar: 288, threeStar: 80 },
	{ num: '55', w: 21, h: 21, type: 'growingTree_50_50', twoStar: 297, threeStar: 81 },
	{ num: '56', w: 22, h: 21, type: 'growingTree_75_25', twoStar: 305, threeStar: 83 },
	{ num: '57', w: 22, h: 21, type: 'growingTree_25_75', twoStar: 313, threeStar: 84 },
	{ num: '58', w: 22, h: 22, type: 'ellers', twoStar: 322, threeStar: 86 },
	{ num: '59', w: 23, h: 22, type: 'prims', twoStar: 330, threeStar: 87 },
	{ num: '60', w: 23, h: 22, type: 'aldousBroder', twoStar: 339, threeStar: 88, minStars: 100 },
	{ num: '61', w: 23, h: 23, type: 'huntAndKill', twoStar: 347, threeStar: 89 },
	{ num: '62', w: 23, h: 23, type: 'kruskals', twoStar: 354, threeStar: 90 },
	{ num: '63', w: 24, h: 23, type: 'prims', twoStar: 361, threeStar: 92 },
	{ num: '64', w: 24, h: 24, type: 'growingTree_50_0', twoStar: 368, threeStar: 92 },
	{ num: '65', w: 24, h: 24, type: 'growingTree_50_50', twoStar: 375, threeStar: 93 },

	// Area 4 (Levels 66–81)
	{ num: '66', w: 25, h: 24, type: 'growingTree_75_25', twoStar: 383, threeStar: 94 },
	{ num: '67', w: 25, h: 24, type: 'growingTree_25_75', twoStar: 390, threeStar: 95 },
	{ num: '68', w: 25, h: 25, type: 'recursiveDivision', twoStar: 397, threeStar: 96 },
	{ num: '69', w: 25, h: 25, type: 'prims', twoStar: 404, threeStar: 97 },
	{ num: '70', w: 25, h: 25, type: 'huntAndKill', twoStar: 411, threeStar: 98, minStars: 120 },
	{ num: '71', w: 25, h: 25, type: 'huntAndKill', twoStar: 419, threeStar: 99 },
	{ num: '72', w: 26, h: 25, type: 'kruskals', twoStar: 426, threeStar: 100 },
	{ num: '73', w: 26, h: 26, type: 'prims', twoStar: 434, threeStar: 101 },
	{ num: '74', w: 26, h: 26, type: 'growingTree_50_0', twoStar: 442, threeStar: 102 },
	{ num: '75', w: 27, h: 26, type: 'growingTree_50_50', twoStar: 450, threeStar: 103 },
	{ num: '76', w: 27, h: 26, type: 'growingTree_75_25', twoStar: 457, threeStar: 104 },
	{ num: '77', w: 27, h: 27, type: 'growingTree_25_75', twoStar: 465, threeStar: 105 },
	{ num: '78', w: 28, h: 27, type: 'sidewinder', twoStar: 473, threeStar: 106 },
	{ num: '79', w: 28, h: 27, type: 'prims', twoStar: 480, threeStar: 107 },
	{ num: '80', w: 28, h: 27, type: 'prims', twoStar: 488, threeStar: 108, minStars: 150 },
	{ num: '81', w: 28, h: 28, type: 'huntAndKill', twoStar: 498, threeStar: 110 },

	// Area 5 (Levels 82–110)
	{ num: '82', w: 28, h: 28, type: 'kruskals', twoStar: 507, threeStar: 111 },
	{ num: '83', w: 29, h: 28, type: 'prims', twoStar: 517, threeStar: 112 },
	{ num: '84', w: 29, h: 29, type: 'growingTree_50_0', twoStar: 527, threeStar: 113 },
	{ num: '85', w: 29, h: 29, type: 'growingTree_50_50', twoStar: 537, threeStar: 115 },
	{ num: '86', w: 30, h: 29, type: 'growingTree_75_25', twoStar: 546, threeStar: 116 },
	{ num: '87', w: 30, h: 29, type: 'growingTree_25_75', twoStar: 556, threeStar: 117 },
	{ num: '88', w: 30, h: 30, type: 'wilsons', twoStar: 566, threeStar: 118 },
	{ num: '89', w: 30, h: 30, type: 'prims', twoStar: 576, threeStar: 120 },
	{ num: '90', w: 30, h: 30, type: 'growingTree_75_25', twoStar: 585, threeStar: 120, minStars: 200 },
	{ num: '91', w: 30, h: 30, type: 'huntAndKill', twoStar: 595, threeStar: 122 },
	{ num: '92', w: 31, h: 30, type: 'kruskals', twoStar: 605, threeStar: 124 },
	{ num: '93', w: 31, h: 31, type: 'prims', twoStar: 615, threeStar: 125 },
	{ num: '94', w: 31, h: 31, type: 'growingTree_50_0', twoStar: 624, threeStar: 127 },
	{ num: '95', w: 32, h: 31, type: 'growingTree_50_50', twoStar: 634, threeStar: 128 },
	{ num: '96', w: 32, h: 31, type: 'growingTree_75_25', twoStar: 644, threeStar: 130 },
	{ num: '97', w: 32, h: 32, type: 'growingTree_25_75', twoStar: 654, threeStar: 132 },
	{ num: '98', w: 32, h: 32, type: 'ellers', twoStar: 663, threeStar: 133 },
	{ num: '99', w: 33, h: 32, type: 'prims', twoStar: 673, threeStar: 135 },
	{ num: '100', w: 32, h: 32, type: 'binaryTree', twoStar: 683, threeStar: 136, minStars: 230 },
	{ num: '101', w: 33, h: 33, type: 'huntAndKill', twoStar: 696, threeStar: 139 },
	{ num: '102', w: 33, h: 33, type: 'kruskals', twoStar: 708, threeStar: 141 },
	{ num: '103', w: 34, h: 33, type: 'prims', twoStar: 721, threeStar: 144 },
	{ num: '104', w: 34, h: 34, type: 'growingTree_50_0', twoStar: 734, threeStar: 146 },
	{ num: '105', w: 34, h: 34, type: 'growingTree_50_50', twoStar: 747, threeStar: 148 },
	{ num: '106', w: 35, h: 34, type: 'growingTree_75_25', twoStar: 759, threeStar: 151 },
	{ num: '107', w: 35, h: 34, type: 'growingTree_25_75', twoStar: 772, threeStar: 153 },
	{ num: '108', w: 35, h: 35, type: 'aldousBroder', twoStar: 785, threeStar: 156 },
	{ num: '109', w: 35, h: 35, type: 'prims', twoStar: 798, threeStar: 158 },
	{ num: '110', w: 35, h: 35, type: 'huntAndKill', twoStar: 810, threeStar: 160, minStars: 250, connects: [] }
];

// ---------------------------------------------------------------------------
// World definitions (gate structures, reward types)
// ---------------------------------------------------------------------------

export interface WorldDefinition {
	worldId: number;
	worldName: string;
	imageUrl: string;
	numberOfLevels: number;
	locked: boolean;
	gateStarRequired: number[];
	levels: CampaignLevel[];
}

export function getWorld1(): WorldDefinition {
	return {
		worldId: 1,
		worldName: 'Cybernetic Labyrinths',
		imageUrl: 'background_maze_3',
		numberOfLevels: 67,
		locked: false,
		gateStarRequired: [50, 80, 65, 120, 160, 195, 225, 265, 305, 335, 355, 370],
		levels: world1Defs.map((d, i) => buildLevel(d, i + 1))
	};
}

export function getWorld2(): WorldDefinition {
	return {
		worldId: 2,
		worldName: 'Galactic Grids',
		imageUrl: 'space_background11',
		numberOfLevels: world2Defs.length,
		locked: true,
		gateStarRequired: [50, 80, 120, 150, 185, 220, 260, 305, 370, 410, 440, 300, 395, 435, 460, 450, 480],
		levels: world2Defs.map((d, i) => buildLevel(d, i + 1))
	};
}

export function getWorld3(): WorldDefinition {
	return {
		worldId: 3,
		worldName: 'Elemental Whispers',
		imageUrl: 'carousel_maze_4',
		numberOfLevels: 150,
		locked: true,
		gateStarRequired: [],
		levels: [] // Coming soon
	};
}

export function getAllWorlds(): WorldDefinition[] {
	return [getWorld1(), getWorld2(), getWorld3()];
}

export function getLevelByNumber(world: WorldDefinition, levelNumber: string): CampaignLevel | undefined {
	return world.levels.find((l) => l.levelNumber === levelNumber);
}
