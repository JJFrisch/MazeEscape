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
	twoStar: number;
	threeStar: number;
	connects?: string[];
	minStars?: number;
	shape?: MazeShape;
	reward?: LevelReward;
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
		width: def.w,
		height: def.h,
		levelType: def.type,
		mazeShape: def.shape ?? 'rectangular',
		twoStarMoves: def.twoStar,
		threeStarTime: def.threeStar,
		numberOfStars: 0,
		minimumStarsToUnlock: def.minStars ?? 0,
		connectTo1: connects[0] ?? null,
		connectTo2: connects[1] ?? null,
		...(def.reward ? { levelReward: def.reward } : {}),
		completed: false,
		star1: false,
		star2: false,
		star3: false,
		bestMoves: 0,
		bestTime: 0
	};
}

// ---------------------------------------------------------------------------
// World 1: Cybernetic Labyrinths (67 levels)
// ---------------------------------------------------------------------------
const world1Defs: LevelDef[] = [
	// Area 1 (Levels 1–14 + Bonus 1b–4b)
	{ num: '1', w: 3, h: 3, type: 'backtracking', twoStar: 15, threeStar: 15 },
	{ num: '2', w: 3, h: 3, type: 'huntAndKill', twoStar: 15, threeStar: 15, connects: ['3', '1b'] },
	{ num: '1b', w: 3, h: 4, type: 'backtracking', twoStar: 18, threeStar: 20 },
	{ num: '3', w: 4, h: 3, type: 'backtracking', twoStar: 16, threeStar: 15 },
	{ num: '4', w: 4, h: 4, type: 'huntAndKill', twoStar: 22, threeStar: 18, connects: ['5', '2b'] },
	{ num: '2b', w: 4, h: 4, type: 'prims', twoStar: 24, threeStar: 20 },
	{ num: '5', w: 4, h: 4, type: 'growingTree_50_50', twoStar: 24, threeStar: 18 },
	{ num: '6', w: 5, h: 4, type: 'backtracking', twoStar: 28, threeStar: 20 },
	{ num: '7', w: 5, h: 5, type: 'huntAndKill', twoStar: 35, threeStar: 22, connects: ['8', '3b'] },
	{ num: '3b', w: 5, h: 5, type: 'growingTree_75_25', twoStar: 38, threeStar: 25 },
	{ num: '8', w: 5, h: 5, type: 'prims', twoStar: 36, threeStar: 22 },
	{ num: '9', w: 6, h: 5, type: 'backtracking', twoStar: 40, threeStar: 25 },
	{ num: '10', w: 6, h: 6, type: 'kruskals', twoStar: 48, threeStar: 28, connects: ['11', '4b'] },
	{ num: '4b', w: 6, h: 5, type: 'growingTree_25_75', twoStar: 42, threeStar: 25 },
	{ num: '11', w: 6, h: 6, type: 'huntAndKill', twoStar: 50, threeStar: 28 },
	{ num: '12', w: 7, h: 6, type: 'backtracking', twoStar: 55, threeStar: 30 },
	{ num: '13', w: 7, h: 7, type: 'growingTree_50_50', twoStar: 60, threeStar: 32 },
{ num: '14', w: 7, h: 7, type: 'prims', twoStar: 62, threeStar: 32, reward: { type: 'key', label: 'Rusted Gate Key', reward: { keyItemId: 'key_area2' } } },

	// Area 2 (Levels 15–28 + Bonus 5b–7b)
	{ num: '15', w: 8, h: 7, type: 'backtracking', twoStar: 65, threeStar: 35, minStars: 20 },
	{ num: '16', w: 8, h: 8, type: 'huntAndKill', twoStar: 72, threeStar: 38, connects: ['17', '5b'] },
	{ num: '5b', w: 8, h: 7, type: 'kruskals', twoStar: 68, threeStar: 36 },
	{ num: '17', w: 8, h: 8, type: 'growingTree_75_25', twoStar: 75, threeStar: 38 },
	{ num: '18', w: 9, h: 8, type: 'backtracking', twoStar: 80, threeStar: 40 },
	{ num: '19', w: 9, h: 9, type: 'prims', twoStar: 88, threeStar: 42, connects: ['20', '6b'] },
	{ num: '6b', w: 9, h: 8, type: 'growingTree_50_0', twoStar: 82, threeStar: 40 },
	{ num: '20', w: 9, h: 9, type: 'huntAndKill', twoStar: 90, threeStar: 42 },
	{ num: '21', w: 10, h: 9, type: 'backtracking', twoStar: 95, threeStar: 45 },
	{ num: '22', w: 10, h: 10, type: 'growingTree_25_75', twoStar: 105, threeStar: 48, connects: ['23', '7b'] },
	{ num: '7b', w: 10, h: 9, type: 'kruskals', twoStar: 98, threeStar: 45 },
	{ num: '23', w: 10, h: 10, type: 'prims', twoStar: 108, threeStar: 48 },
	{ num: '24', w: 11, h: 10, type: 'backtracking', twoStar: 115, threeStar: 50 },
	{ num: '25', w: 11, h: 11, type: 'huntAndKill', twoStar: 125, threeStar: 52 },
	{ num: '26', w: 11, h: 11, type: 'growingTree_50_50', twoStar: 128, threeStar: 52 },
	{ num: '27', w: 12, h: 11, type: 'backtracking', twoStar: 135, threeStar: 55 },
{ num: '28', w: 12, h: 12, type: 'prims', twoStar: 145, threeStar: 58, reward: { type: 'gem', label: 'Crystal Gem', reward: { coins: 350 } } },

	// Area 3 (Levels 29–43 + Bonus 8b–9b)
	{ num: '29', w: 12, h: 12, type: 'huntAndKill', twoStar: 148, threeStar: 58, minStars: 45 },
	{ num: '30', w: 13, h: 12, type: 'backtracking', twoStar: 155, threeStar: 60 },
	{ num: '31', w: 13, h: 13, type: 'growingTree_75_25', twoStar: 168, threeStar: 62, connects: ['32', '8b'] },
	{ num: '8b', w: 13, h: 12, type: 'kruskals', twoStar: 158, threeStar: 60 },
	{ num: '32', w: 13, h: 13, type: 'prims', twoStar: 170, threeStar: 62 },
	{ num: '33', w: 14, h: 13, type: 'backtracking', twoStar: 178, threeStar: 65 },
	{ num: '34', w: 14, h: 14, type: 'huntAndKill', twoStar: 190, threeStar: 68 },
	{ num: '35', w: 14, h: 14, type: 'growingTree_50_0', twoStar: 192, threeStar: 68 },
	{ num: '36', w: 15, h: 14, type: 'backtracking', twoStar: 200, threeStar: 70 },
	{ num: '37', w: 15, h: 15, type: 'prims', twoStar: 215, threeStar: 72, connects: ['38', '9b'] },
	{ num: '9b', w: 15, h: 14, type: 'growingTree_25_75', twoStar: 205, threeStar: 70 },
	{ num: '38', w: 15, h: 15, type: 'huntAndKill', twoStar: 218, threeStar: 72 },
	{ num: '39', w: 16, h: 15, type: 'backtracking', twoStar: 228, threeStar: 75 },
	{ num: '40', w: 16, h: 16, type: 'growingTree_50_50', twoStar: 245, threeStar: 78 },
	{ num: '41', w: 16, h: 16, type: 'kruskals', twoStar: 248, threeStar: 78 },
	{ num: '42', w: 17, h: 16, type: 'backtracking', twoStar: 258, threeStar: 80 },
{ num: '43', w: 17, h: 17, type: 'prims', twoStar: 275, threeStar: 82, reward: { type: 'key', label: 'Forest Gate Key', reward: { keyItemId: 'key_forest' } } },

	// Area 4 (Levels 44–55 + Bonus 10b–20b)
	{ num: '44', w: 17, h: 17, type: 'huntAndKill', twoStar: 278, threeStar: 82, minStars: 80 },
	{ num: '45', w: 18, h: 17, type: 'backtracking', twoStar: 288, threeStar: 85, connects: ['46', '10b'] },
	{ num: '10b', w: 17, h: 17, type: 'growingTree_75_25', twoStar: 280, threeStar: 82 },
	{ num: '46', w: 18, h: 18, type: 'prims', twoStar: 305, threeStar: 88 },
	{ num: '47', w: 18, h: 18, type: 'growingTree_50_0', twoStar: 308, threeStar: 88 },
	{ num: '48', w: 19, h: 18, type: 'backtracking', twoStar: 318, threeStar: 90 },
	{ num: '49', w: 19, h: 19, type: 'huntAndKill', twoStar: 335, threeStar: 92 },
	{ num: '50', w: 19, h: 19, type: 'kruskals', twoStar: 338, threeStar: 92 },
	{ num: '51', w: 20, h: 19, type: 'backtracking', twoStar: 348, threeStar: 95 },
	{ num: '52', w: 20, h: 20, type: 'prims', twoStar: 368, threeStar: 98 },
	{ num: '53', w: 20, h: 20, type: 'growingTree_50_50', twoStar: 372, threeStar: 98 },
	{ num: '54', w: 21, h: 20, type: 'backtracking', twoStar: 382, threeStar: 100, reward: { type: 'powerup_hint', label: "Navigator's Compass", reward: { powerup: 'hint', powerupCount: 3 } } },
	{ num: '55', w: 21, h: 21, type: 'huntAndKill', twoStar: 402, threeStar: 102, reward: { type: 'cloak', label: 'Shadow Cloak', reward: { skinId: 7 } } },

	// Area 5 (Levels 56–67)
	{ num: '56', w: 21, h: 21, type: 'growingTree_75_25', twoStar: 405, threeStar: 102, minStars: 120 },
	{ num: '57', w: 22, h: 21, type: 'backtracking', twoStar: 415, threeStar: 105 },
	{ num: '58', w: 22, h: 22, type: 'prims', twoStar: 438, threeStar: 108 },
	{ num: '59', w: 22, h: 22, type: 'kruskals', twoStar: 442, threeStar: 108 },
	{ num: '60', w: 23, h: 22, type: 'backtracking', twoStar: 452, threeStar: 110 },
	{ num: '61', w: 23, h: 23, type: 'huntAndKill', twoStar: 472, threeStar: 112 },
	{ num: '62', w: 23, h: 23, type: 'growingTree_25_75', twoStar: 476, threeStar: 112 },
	{ num: '63', w: 24, h: 23, type: 'backtracking', twoStar: 488, threeStar: 115 },
	{ num: '64', w: 24, h: 24, type: 'prims', twoStar: 510, threeStar: 118 },
	{ num: '65', w: 24, h: 24, type: 'growingTree_50_0', twoStar: 514, threeStar: 118 },
	{ num: '66', w: 25, h: 24, type: 'backtracking', twoStar: 525, threeStar: 120 },
	{ num: '67', w: 25, h: 25, type: 'huntAndKill', twoStar: 548, threeStar: 122, connects: [], reward: { type: 'gem', label: 'Cyber Core Gem', reward: { coins: 1000 } } }
];

// ---------------------------------------------------------------------------
// World 2: Galactic Grids (110 main levels, fully defined)
// ---------------------------------------------------------------------------
const world2Defs: LevelDef[] = [
	// Area 1 (Levels 1–22)
	{ num: '1', w: 3, h: 3, type: 'huntAndKill', twoStar: 14, threeStar: 14 },
	{ num: '2', w: 3, h: 4, type: 'backtracking', twoStar: 17, threeStar: 16 },
	{ num: '3', w: 4, h: 4, type: 'prims', twoStar: 22, threeStar: 18 },
	{ num: '4', w: 4, h: 4, type: 'growingTree_50_0', twoStar: 24, threeStar: 18 },
	{ num: '5', w: 5, h: 4, type: 'backtracking', twoStar: 28, threeStar: 20 },
	{ num: '6', w: 5, h: 5, type: 'huntAndKill', twoStar: 35, threeStar: 22 },
	{ num: '7', w: 5, h: 5, type: 'kruskals', twoStar: 36, threeStar: 22 },
	{ num: '8', w: 6, h: 5, type: 'backtracking', twoStar: 40, threeStar: 25 },
	{ num: '9', w: 6, h: 6, type: 'growingTree_75_25', twoStar: 48, threeStar: 28 },
	{ num: '10', w: 6, h: 6, type: 'prims', twoStar: 50, threeStar: 28 },
	{ num: '11', w: 7, h: 6, type: 'backtracking', twoStar: 55, threeStar: 30 },
	{ num: '12', w: 7, h: 7, type: 'huntAndKill', twoStar: 62, threeStar: 32 },
	{ num: '13', w: 7, h: 7, type: 'growingTree_25_75', twoStar: 64, threeStar: 32 },
	{ num: '14', w: 8, h: 7, type: 'backtracking', twoStar: 68, threeStar: 35 },
	{ num: '15', w: 8, h: 8, type: 'prims', twoStar: 76, threeStar: 38 },
	{ num: '16', w: 8, h: 8, type: 'kruskals', twoStar: 78, threeStar: 38 },
	{ num: '17', w: 9, h: 8, type: 'backtracking', twoStar: 82, threeStar: 40 },
	{ num: '18', w: 9, h: 9, type: 'huntAndKill', twoStar: 92, threeStar: 42 },
	{ num: '19', w: 9, h: 9, type: 'growingTree_50_50', twoStar: 94, threeStar: 42 },
	{ num: '20', w: 10, h: 9, type: 'backtracking', twoStar: 98, threeStar: 45 },
	{ num: '21', w: 10, h: 10, type: 'prims', twoStar: 108, threeStar: 48 },
	{ num: '22', w: 10, h: 10, type: 'growingTree_50_0', twoStar: 110, threeStar: 48, minStars: 20 },

	// Area 2 (Levels 23–45)
	{ num: '23', w: 11, h: 10, type: 'backtracking', twoStar: 115, threeStar: 50, minStars: 30 },
	{ num: '24', w: 11, h: 11, type: 'huntAndKill', twoStar: 125, threeStar: 52 },
	{ num: '25', w: 11, h: 11, type: 'kruskals', twoStar: 128, threeStar: 52 },
	// Hex, circular & triangular maze shapes introduced
	{ num: '26', w: 7, h: 6, type: 'backtracking', twoStar: 50, threeStar: 30, shape: 'hexagonal' },
	{ num: '27', w: 5, h: 5, type: 'backtracking', twoStar: 30, threeStar: 25, shape: 'circular' },
	{ num: '28', w: 8, h: 6, type: 'backtracking', twoStar: 55, threeStar: 32, shape: 'triangular' },
	{ num: '29', w: 9, h: 7, type: 'backtracking', twoStar: 65, threeStar: 38, shape: 'hexagonal' },
	{ num: '30', w: 13, h: 12, type: 'backtracking', twoStar: 155, threeStar: 60, minStars: 50 },
	{ num: '31', w: 13, h: 13, type: 'huntAndKill', twoStar: 164, threeStar: 62 },
	{ num: '32', w: 14, h: 13, type: 'kruskals', twoStar: 173, threeStar: 64 },
	{ num: '33', w: 14, h: 14, type: 'prims', twoStar: 182, threeStar: 65 },
	{ num: '34', w: 14, h: 14, type: 'growingTree_50_0', twoStar: 191, threeStar: 67 },
	{ num: '35', w: 15, h: 14, type: 'growingTree_50_50', twoStar: 200, threeStar: 69 },
	{ num: '36', w: 15, h: 15, type: 'growingTree_75_25', twoStar: 209, threeStar: 71 },
	{ num: '37', w: 15, h: 15, type: 'growingTree_25_75', twoStar: 218, threeStar: 72 },
	{ num: '38', w: 16, h: 15, type: 'backtracking', twoStar: 227, threeStar: 74 },
	{ num: '39', w: 16, h: 15, type: 'prims', twoStar: 236, threeStar: 76 },
	{ num: '40', w: 16, h: 16, type: 'growingTree_50_50', twoStar: 245, threeStar: 78, minStars: 60 },
	{ num: '41', w: 16, h: 16, type: 'huntAndKill', twoStar: 254, threeStar: 79 },
	{ num: '42', w: 17, h: 16, type: 'kruskals', twoStar: 263, threeStar: 81 },
	{ num: '43', w: 17, h: 17, type: 'prims', twoStar: 272, threeStar: 82 },
	{ num: '44', w: 17, h: 17, type: 'growingTree_50_0', twoStar: 281, threeStar: 84 },
	{ num: '45', w: 18, h: 17, type: 'growingTree_50_50', twoStar: 290, threeStar: 85 },

	// Area 3 (Levels 46–65)
	{ num: '46', w: 18, h: 18, type: 'growingTree_75_25', twoStar: 299, threeStar: 87 },
	{ num: '47', w: 18, h: 18, type: 'growingTree_25_75', twoStar: 308, threeStar: 88 },
	{ num: '48', w: 19, h: 18, type: 'backtracking', twoStar: 317, threeStar: 90 },
	{ num: '49', w: 19, h: 18, type: 'prims', twoStar: 326, threeStar: 91 },
	{ num: '50', w: 19, h: 19, type: 'kruskals', twoStar: 338, threeStar: 92, minStars: 80 },
	{ num: '51', w: 20, h: 19, type: 'huntAndKill', twoStar: 349, threeStar: 94 },
	{ num: '52', w: 20, h: 20, type: 'kruskals', twoStar: 361, threeStar: 96 },
	{ num: '53', w: 20, h: 20, type: 'prims', twoStar: 372, threeStar: 97 },
	{ num: '54', w: 21, h: 20, type: 'growingTree_50_0', twoStar: 383, threeStar: 99 },
	{ num: '55', w: 21, h: 21, type: 'growingTree_50_50', twoStar: 395, threeStar: 101 },
	{ num: '56', w: 22, h: 21, type: 'growingTree_75_25', twoStar: 406, threeStar: 103 },
	{ num: '57', w: 22, h: 21, type: 'growingTree_25_75', twoStar: 417, threeStar: 105 },
	{ num: '58', w: 22, h: 22, type: 'backtracking', twoStar: 429, threeStar: 107 },
	{ num: '59', w: 23, h: 22, type: 'prims', twoStar: 440, threeStar: 108 },
	{ num: '60', w: 23, h: 22, type: 'backtracking', twoStar: 452, threeStar: 110, minStars: 100 },
	{ num: '61', w: 23, h: 23, type: 'huntAndKill', twoStar: 462, threeStar: 111 },
	{ num: '62', w: 23, h: 23, type: 'kruskals', twoStar: 471, threeStar: 112 },
	{ num: '63', w: 24, h: 23, type: 'prims', twoStar: 481, threeStar: 114 },
	{ num: '64', w: 24, h: 24, type: 'growingTree_50_0', twoStar: 490, threeStar: 115 },
	{ num: '65', w: 24, h: 24, type: 'growingTree_50_50', twoStar: 500, threeStar: 116 },

	// Area 4 (Levels 66–81)
	{ num: '66', w: 25, h: 24, type: 'growingTree_75_25', twoStar: 510, threeStar: 117 },
	{ num: '67', w: 25, h: 24, type: 'growingTree_25_75', twoStar: 519, threeStar: 118 },
	{ num: '68', w: 25, h: 25, type: 'backtracking', twoStar: 529, threeStar: 120 },
	{ num: '69', w: 25, h: 25, type: 'prims', twoStar: 538, threeStar: 121 },
	{ num: '70', w: 25, h: 25, type: 'huntAndKill', twoStar: 548, threeStar: 122, minStars: 120 },
	{ num: '71', w: 25, h: 25, type: 'huntAndKill', twoStar: 558, threeStar: 123 },
	{ num: '72', w: 26, h: 25, type: 'kruskals', twoStar: 568, threeStar: 124 },
	{ num: '73', w: 26, h: 26, type: 'prims', twoStar: 578, threeStar: 126 },
	{ num: '74', w: 26, h: 26, type: 'growingTree_50_0', twoStar: 589, threeStar: 127 },
	{ num: '75', w: 27, h: 26, type: 'growingTree_50_50', twoStar: 599, threeStar: 128 },
	{ num: '76', w: 27, h: 26, type: 'growingTree_75_25', twoStar: 609, threeStar: 129 },
	{ num: '77', w: 27, h: 27, type: 'growingTree_25_75', twoStar: 619, threeStar: 131 },
	{ num: '78', w: 28, h: 27, type: 'backtracking', twoStar: 630, threeStar: 132 },
	{ num: '79', w: 28, h: 27, type: 'prims', twoStar: 640, threeStar: 133 },
	{ num: '80', w: 28, h: 27, type: 'prims', twoStar: 650, threeStar: 135, minStars: 150 },
	{ num: '81', w: 28, h: 28, type: 'huntAndKill', twoStar: 663, threeStar: 137 },

	// Area 5 (Levels 82–110)
	{ num: '82', w: 28, h: 28, type: 'kruskals', twoStar: 676, threeStar: 138 },
	{ num: '83', w: 29, h: 28, type: 'prims', twoStar: 689, threeStar: 140 },
	{ num: '84', w: 29, h: 29, type: 'growingTree_50_0', twoStar: 702, threeStar: 141 },
	{ num: '85', w: 29, h: 29, type: 'growingTree_50_50', twoStar: 715, threeStar: 143 },
	{ num: '86', w: 30, h: 29, type: 'growingTree_75_25', twoStar: 728, threeStar: 144 },
	{ num: '87', w: 30, h: 29, type: 'growingTree_25_75', twoStar: 741, threeStar: 146 },
	{ num: '88', w: 30, h: 30, type: 'backtracking', twoStar: 754, threeStar: 147 },
	{ num: '89', w: 30, h: 30, type: 'prims', twoStar: 767, threeStar: 149 },
	{ num: '90', w: 30, h: 30, type: 'growingTree_75_25', twoStar: 780, threeStar: 150, minStars: 200 },
	{ num: '91', w: 30, h: 30, type: 'huntAndKill', twoStar: 793, threeStar: 152 },
	{ num: '92', w: 31, h: 30, type: 'kruskals', twoStar: 806, threeStar: 154 },
	{ num: '93', w: 31, h: 31, type: 'prims', twoStar: 819, threeStar: 156 },
	{ num: '94', w: 31, h: 31, type: 'growingTree_50_0', twoStar: 832, threeStar: 158 },
	{ num: '95', w: 32, h: 31, type: 'growingTree_50_50', twoStar: 845, threeStar: 160 },
	{ num: '96', w: 32, h: 31, type: 'growingTree_75_25', twoStar: 858, threeStar: 162 },
	{ num: '97', w: 32, h: 32, type: 'growingTree_25_75', twoStar: 871, threeStar: 164 },
	{ num: '98', w: 32, h: 32, type: 'backtracking', twoStar: 884, threeStar: 166 },
	{ num: '99', w: 33, h: 32, type: 'prims', twoStar: 897, threeStar: 168 },
	{ num: '100', w: 32, h: 32, type: 'backtracking', twoStar: 910, threeStar: 170, minStars: 230 },
	{ num: '101', w: 33, h: 33, type: 'huntAndKill', twoStar: 927, threeStar: 173 },
	{ num: '102', w: 33, h: 33, type: 'kruskals', twoStar: 944, threeStar: 176 },
	{ num: '103', w: 34, h: 33, type: 'prims', twoStar: 961, threeStar: 179 },
	{ num: '104', w: 34, h: 34, type: 'growingTree_50_0', twoStar: 978, threeStar: 182 },
	{ num: '105', w: 34, h: 34, type: 'growingTree_50_50', twoStar: 995, threeStar: 185 },
	{ num: '106', w: 35, h: 34, type: 'growingTree_75_25', twoStar: 1012, threeStar: 188 },
	{ num: '107', w: 35, h: 34, type: 'growingTree_25_75', twoStar: 1029, threeStar: 191 },
	{ num: '108', w: 35, h: 35, type: 'backtracking', twoStar: 1046, threeStar: 194 },
	{ num: '109', w: 35, h: 35, type: 'prims', twoStar: 1063, threeStar: 197 },
	{ num: '110', w: 35, h: 35, type: 'huntAndKill', twoStar: 1080, threeStar: 200, minStars: 250, connects: [] }
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
		gateStarRequired: [20, 45, 30, 60, 80, 100, 120, 150, 200, 230, 240, 250],
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
		gateStarRequired: [20, 30, 50, 60, 80, 100, 120, 150, 230, 240, 250, 150, 230, 240, 250, 240, 250],
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
