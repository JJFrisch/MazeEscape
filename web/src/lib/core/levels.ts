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
	encounterTitle?: string;
	flavorText?: string;
	archetype?: string;
	feel?: string;
	targetClearTimeSec?: number;
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
		...(def.encounterTitle ? { encounterTitle: def.encounterTitle } : {}),
		...(def.flavorText ? { flavorText: def.flavorText } : {}),
		width: def.w,
		height: def.h,
		levelType: def.type,
		mazeShape: def.shape ?? 'rectangular',
		...(def.archetype ? { archetype: def.archetype } : {}),
		...(def.feel ? { feel: def.feel } : {}),
		...(def.targetClearTimeSec ? { targetClearTimeSec: def.targetClearTimeSec } : {}),
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
// World 1: Cybernetic Labyrinths (67 levels, 22 bonus levels)
// Sizes, generators, connections and star gates are the native app's
// InitializeWorld1Levels(). The native app derives star thresholds from each
// generated maze's path length; here they are estimated from the grid area.
// ---------------------------------------------------------------------------
const world1Defs: LevelDef[] = [
	// Area 1
	{ num: '1', w: 5, h: 5, type: 'huntAndKill', twoStar: 26, threeStar: 20, connects: ['2'] },
	{ num: '2', w: 6, h: 6, type: 'kruskals', twoStar: 33, threeStar: 23, connects: ['3'] },
	{ num: '3', w: 6, h: 6, type: 'prims', twoStar: 33, threeStar: 23, connects: ['4'] },
	{ num: '4', w: 8, h: 8, type: 'growingTree_50_0', twoStar: 52, threeStar: 31, connects: ['5'] },
	{ num: '5', w: 8, h: 8, type: 'growingTree_50_50', twoStar: 52, threeStar: 31, connects: ['6'] },
	{ num: '6', w: 8, h: 8, type: 'growingTree_75_25', twoStar: 52, threeStar: 31, connects: ['7'] },
	{ num: '7', w: 8, h: 8, type: 'growingTree_25_75', twoStar: 52, threeStar: 31, connects: ['8', '1b'] },
	{ num: '8', w: 10, h: 12, type: 'prims', twoStar: 88, threeStar: 43, connects: ['9'] },
	{ num: '9', w: 12, h: 10, type: 'growingTree_25_75', twoStar: 88, threeStar: 43, connects: ['10'] },
	{ num: '10', w: 11, h: 11, type: 'backtracking', twoStar: 89, threeStar: 43, connects: ['11'] },
	{ num: '11', w: 12, h: 10, type: 'growingTree_50_0', twoStar: 88, threeStar: 43, connects: ['12'] },
	{ num: '12', w: 10, h: 12, type: 'growingTree_75_25', twoStar: 88, threeStar: 43, connects: ['13'] },
	{ num: '13', w: 11, h: 11, type: 'kruskals', twoStar: 89, threeStar: 43, connects: ['14', '4b'] },
	{ num: '14', w: 14, h: 9, type: 'growingTree_50_50', twoStar: 92, threeStar: 44, connects: ['15'] },

	// Area 1 bonus levels
	{ num: '1b', w: 15, h: 12, type: 'growingTree_50_0', twoStar: 127, threeStar: 52, connects: ['2b'], minStars: 20 },
	{ num: '2b', w: 15, h: 15, type: 'backtracking', twoStar: 156, threeStar: 59, connects: ['3b'] },
	{ num: '3b', w: 14, h: 15, type: 'growingTree_75_25', twoStar: 147, threeStar: 57, connects: [] },
	{ num: '4b', w: 18, h: 12, type: 'kruskals', twoStar: 150, threeStar: 57, connects: [], minStars: 45 },

	// Area 2
	{ num: '15', w: 15, h: 15, type: 'kruskals', twoStar: 156, threeStar: 59, connects: ['16'], minStars: 30 },
	{ num: '16', w: 14, h: 16, type: 'growingTree_50_0', twoStar: 156, threeStar: 58, connects: ['17'] },
	{ num: '17', w: 16, h: 14, type: 'huntAndKill', twoStar: 156, threeStar: 58, connects: ['18'] },
	{ num: '18', w: 13, h: 16, type: 'prims', twoStar: 145, threeStar: 56, connects: ['19'] },
	{ num: '19', w: 16, h: 13, type: 'growingTree_50_50', twoStar: 145, threeStar: 56, connects: ['20'] },
	{ num: '20', w: 15, h: 15, type: 'backtracking', twoStar: 156, threeStar: 59, connects: ['21', '5b'] },
	{ num: '21', w: 15, h: 15, type: 'kruskals', twoStar: 156, threeStar: 59, connects: ['22'] },
	{ num: '22', w: 17, h: 13, type: 'growingTree_75_25', twoStar: 154, threeStar: 58, connects: ['23'] },
	{ num: '23', w: 17, h: 17, type: 'growingTree_50_0', twoStar: 198, threeStar: 66, connects: ['24'] },
	{ num: '24', w: 16, h: 18, type: 'prims', twoStar: 197, threeStar: 66, connects: ['25'] },
	{ num: '25', w: 17, h: 16, type: 'growingTree_50_50', twoStar: 187, threeStar: 64, connects: ['26'] },
	{ num: '26', w: 16, h: 18, type: 'huntAndKill', twoStar: 197, threeStar: 66, connects: ['27'] },
	{ num: '27', w: 15, h: 19, type: 'backtracking', twoStar: 195, threeStar: 66, connects: ['28'] },
	{ num: '28', w: 23, h: 15, type: 'huntAndKill', twoStar: 234, threeStar: 72, connects: ['29'] },

	// Area 2 bonus levels
	{ num: '5b', w: 20, h: 12, type: 'huntAndKill', twoStar: 166, threeStar: 60, connects: ['6b'], minStars: 60 },
	{ num: '6b', w: 12, h: 20, type: 'prims', twoStar: 166, threeStar: 60, connects: ['7b'] },
	{ num: '7b', w: 20, h: 16, type: 'kruskals', twoStar: 218, threeStar: 70, connects: [] },

	// Area 3
	{ num: '29', w: 19, h: 19, type: 'growingTree_75_25', twoStar: 245, threeStar: 74, connects: ['30'], minStars: 80 },
	{ num: '30', w: 17, h: 20, type: 'kruskals', twoStar: 231, threeStar: 72, connects: ['31'] },
	{ num: '31', w: 21, h: 20, type: 'prims', twoStar: 283, threeStar: 80, connects: ['32'] },
	{ num: '32', w: 22, h: 20, type: 'growingTree_50_0', twoStar: 296, threeStar: 82, connects: ['33'] },
	{ num: '33', w: 22, h: 20, type: 'backtracking', twoStar: 296, threeStar: 82, connects: ['34'] },
	{ num: '34', w: 23, h: 22, type: 'growingTree_75_25', twoStar: 339, threeStar: 88, connects: ['35'] },
	{ num: '35', w: 24, h: 23, type: 'growingTree_50_50', twoStar: 369, threeStar: 92, connects: ['36'] },
	{ num: '36', w: 25, h: 22, type: 'growingTree_25_75', twoStar: 368, threeStar: 91, connects: ['37'] },
	{ num: '37', w: 21, h: 25, type: 'kruskals', twoStar: 351, threeStar: 89, connects: ['38', '8b'] },
	{ num: '38', w: 22, h: 26, type: 'prims', twoStar: 382, threeStar: 93, connects: ['39'] },
	{ num: '39', w: 23, h: 24, type: 'huntAndKill', twoStar: 369, threeStar: 92, connects: ['40'] },
	{ num: '40', w: 22, h: 18, type: 'growingTree_75_25', twoStar: 267, threeStar: 78, connects: ['41'] },
	{ num: '41', w: 21, h: 25, type: 'growingTree_50_50', twoStar: 351, threeStar: 89, connects: ['42'] },
	{ num: '42', w: 24, h: 23, type: 'huntAndKill', twoStar: 369, threeStar: 92, connects: ['43'] },
	{ num: '43', w: 25, h: 25, type: 'prims', twoStar: 416, threeStar: 98, connects: ['44'] },

	// Area 3 bonus levels
	{ num: '8b', w: 20, h: 12, type: 'kruskals', twoStar: 166, threeStar: 60, connects: ['9b'], minStars: 100 },
	{ num: '9b', w: 20, h: 12, type: 'huntAndKill', twoStar: 166, threeStar: 60, connects: [] },

	// Area 4
	{ num: '44', w: 26, h: 21, type: 'growingTree_25_75', twoStar: 365, threeStar: 91, connects: ['45'], minStars: 120 },
	{ num: '45', w: 26, h: 26, type: 'huntAndKill', twoStar: 449, threeStar: 101, connects: ['46'] },
	{ num: '46', w: 27, h: 26, type: 'prims', twoStar: 466, threeStar: 103, connects: ['47'] },
	{ num: '47', w: 26, h: 27, type: 'growingTree_75_25', twoStar: 466, threeStar: 103, connects: ['48'] },
	{ num: '48', w: 22, h: 20, type: 'kruskals', twoStar: 296, threeStar: 82, connects: ['49'] },
	{ num: '49', w: 25, h: 27, type: 'growingTree_50_50', twoStar: 449, threeStar: 101, connects: ['50'] },
	{ num: '50', w: 25, h: 27, type: 'prims', twoStar: 449, threeStar: 101, connects: ['51'] },
	{ num: '51', w: 26, h: 24, type: 'growingTree_75_25', twoStar: 416, threeStar: 97, connects: ['52'] },
	{ num: '52', w: 27, h: 25, type: 'kruskals', twoStar: 449, threeStar: 101, connects: ['53'] },
	{ num: '53', w: 28, h: 26, type: 'huntAndKill', twoStar: 483, threeStar: 105, connects: ['54'] },
	{ num: '54', w: 27, h: 25, type: 'growingTree_50_50', twoStar: 449, threeStar: 101, connects: ['55'] },
	{ num: '55', w: 28, h: 28, type: 'kruskals', twoStar: 520, threeStar: 109, connects: ['56', '10b'] },

	// Area 4 bonus levels
	{ num: '10b', w: 22, h: 26, type: 'kruskals', twoStar: 382, threeStar: 93, connects: ['11b'], minStars: 150 },
	{ num: '11b', w: 23, h: 23, type: 'growingTree_75_25', twoStar: 354, threeStar: 90, connects: ['12b'] },
	{ num: '12b', w: 24, h: 24, type: 'growingTree_50_50', twoStar: 384, threeStar: 94, connects: ['13b'] },
	{ num: '13b', w: 25, h: 25, type: 'prims', twoStar: 416, threeStar: 98, connects: ['14b'] },
	{ num: '14b', w: 20, h: 25, type: 'growingTree_25_75', twoStar: 335, threeStar: 87, connects: ['15b'] },
	{ num: '15b', w: 26, h: 26, type: 'kruskals', twoStar: 449, threeStar: 101, connects: ['16b'] },
	{ num: '16b', w: 27, h: 24, type: 'backtracking', twoStar: 431, threeStar: 99, connects: ['17b'] },
	{ num: '17b', w: 22, h: 27, type: 'huntAndKill', twoStar: 396, threeStar: 95, connects: ['18b'] },
	{ num: '18b', w: 23, h: 25, type: 'growingTree_50_50', twoStar: 384, threeStar: 94, connects: ['19b'] },
	{ num: '19b', w: 25, h: 27, type: 'prims', twoStar: 449, threeStar: 101, connects: ['20b'] },
	{ num: '20b', w: 4, h: 4, type: 'huntAndKill', twoStar: 20, threeStar: 16, connects: [] },

	// Area 5
	{ num: '56', w: 29, h: 29, type: 'growingTree_75_25', twoStar: 557, threeStar: 113, connects: ['57'], minStars: 200 },
	{ num: '57', w: 29, h: 31, type: 'prims', twoStar: 594, threeStar: 117, connects: ['58'] },
	{ num: '58', w: 30, h: 30, type: 'growingTree_50_50', twoStar: 595, threeStar: 117, connects: ['59'] },
	{ num: '59', w: 30, h: 30, type: 'prims', twoStar: 595, threeStar: 117, connects: ['60'] },
	{ num: '60', w: 30, h: 30, type: 'backtracking', twoStar: 595, threeStar: 117, connects: ['61'] },
	{ num: '61', w: 30, h: 30, type: 'kruskals', twoStar: 595, threeStar: 117, connects: ['62'] },
	{ num: '62', w: 31, h: 31, type: 'growingTree_50_50', twoStar: 635, threeStar: 121, connects: ['63'] },
	{ num: '63', w: 32, h: 32, type: 'kruskals', twoStar: 676, threeStar: 125, connects: ['64'] },
	{ num: '64', w: 33, h: 31, type: 'growingTree_50_50', twoStar: 675, threeStar: 125, connects: ['65'] },
	{ num: '65', w: 32, h: 33, type: 'prims', twoStar: 696, threeStar: 127, connects: ['66', '21b'] },
	{ num: '66', w: 33, h: 33, type: 'kruskals', twoStar: 718, threeStar: 129, connects: ['67', '22b'] },
	{ num: '67', w: 4, h: 4, type: 'huntAndKill', twoStar: 20, threeStar: 16, connects: [] },

	// Area 5 bonus levels
	{ num: '21b', w: 33, h: 33, type: 'growingTree_50_50', twoStar: 718, threeStar: 129, connects: [], minStars: 230 },
	{ num: '22b', w: 34, h: 34, type: 'prims', twoStar: 761, threeStar: 133, connects: [], minStars: 240 },
];

type AuthoredLevelMeta = Pick<LevelDef, 'encounterTitle' | 'flavorText' | 'archetype' | 'feel' | 'targetClearTimeSec'>;
type World2Area = 1 | 2 | 3 | 4 | 5;

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

// Bonus levels on World 2's side passages: sizes, generators and connections
// from the native app's InitializeWorld2Levels(), star thresholds estimated from
// the grid area, and gate requirements from this world's gateStarRequired.
const world2BonusDefs: LevelDef[] = [
	{ num: '1b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['2b'], minStars: 50 },
	{ num: '2b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['3b'] },
	{ num: '3b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['4b'] },
	{ num: '4b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['5b'] },
	{ num: '5b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: [] },
	{ num: '6b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['7b'], minStars: 80 },
	{ num: '7b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['8b'] },
	{ num: '8b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '9b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['10b'], minStars: 120 },
	{ num: '10b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['11b'] },
	{ num: '11b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['12b'] },
	{ num: '12b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['13b'] },
	{ num: '13b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['14b'] },
	{ num: '14b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['15b'] },
	{ num: '15b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['16b', '21b'] },
	{ num: '16b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['17b'] },
	{ num: '17b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['18b'] },
	{ num: '18b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['19b'] },
	{ num: '19b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['20b'] },
	{ num: '20b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '21b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: [] },
	{ num: '22b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['23b'], minStars: 150 },
	{ num: '23b', w: 24, h: 23, type: 'growingTree_75_25', twoStar: 369, threeStar: 92, connects: ['24b'] },
	{ num: '24b', w: 28, h: 23, type: 'kruskals', twoStar: 429, threeStar: 99, connects: [] },
	{ num: '25b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [], minStars: 185 },
	{ num: '26b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['27b'], minStars: 260 },
	{ num: '27b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['28b'] },
	{ num: '28b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['29b'] },
	{ num: '29b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['30b'] },
	{ num: '30b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: [] },
	{ num: '31b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['32b'], minStars: 305 },
	{ num: '32b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['33b'] },
	{ num: '33b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['34b'] },
	{ num: '34b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['35b'] },
	{ num: '35b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['36b'] },
	{ num: '36b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['37b'] },
	{ num: '37b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['38b'] },
	{ num: '38b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: [] },
	{ num: '39b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['40b'], minStars: 410 },
	{ num: '40b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['41b'] },
	{ num: '41b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['42b'] },
	{ num: '42b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['43b'] },
	{ num: '43b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['44b'] },
	{ num: '44b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['45b'] },
	{ num: '45b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['46b'] },
	{ num: '46b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '47b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['48b'], minStars: 440 },
	{ num: '48b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['49b'] },
	{ num: '49b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: [] },
	{ num: '50b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['51b'], minStars: 300 },
	{ num: '51b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['52b'] },
	{ num: '52b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['53b'] },
	{ num: '53b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['54b', '63b'] },
	{ num: '54b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['55b'] },
	{ num: '55b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['56b'] },
	{ num: '56b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['57b'] },
	{ num: '57b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['58b'] },
	{ num: '58b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['59b'] },
	{ num: '59b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['60b'] },
	{ num: '60b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['61b'] },
	{ num: '61b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['62b'] },
	{ num: '62b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: [] },
	{ num: '63b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['64b'] },
	{ num: '64b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['65b'] },
	{ num: '65b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: [] },
	{ num: '66b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['67b'], minStars: 435 },
	{ num: '67b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['68b'] },
	{ num: '68b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['69b'] },
	{ num: '69b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['70b', '74b'] },
	{ num: '70b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['71b', '72b'] },
	{ num: '71b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: [] },
	{ num: '72b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['73b'] },
	{ num: '73b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '74b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['75b'] },
	{ num: '75b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['76b'] },
	{ num: '76b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['77b'] },
	{ num: '77b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['78b'] },
	{ num: '78b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['79b'] },
	{ num: '79b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['80b'] },
	{ num: '80b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['81b', '63b'] },
	{ num: '81b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['82b'] },
	{ num: '82b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['83b'] },
	{ num: '83b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['84b'] },
	{ num: '84b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['85b'] },
	{ num: '85b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['86b'] },
	{ num: '86b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '87b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['88b'], minStars: 450 },
	{ num: '88b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['89b'] },
	{ num: '89b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['90b'] },
	{ num: '90b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: [] },
	{ num: '91b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['92b'], minStars: 480 },
	{ num: '92b', w: 25, h: 25, type: 'backtracking', twoStar: 416, threeStar: 98, connects: ['93b'] },
	{ num: '93b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['94b'] },
	{ num: '94b', w: 28, h: 22, type: 'kruskals', twoStar: 410, threeStar: 97, connects: ['95b', '97b'] },
	{ num: '95b', w: 25, h: 22, type: 'growingTree_50_0', twoStar: 368, threeStar: 91, connects: ['96b'] },
	{ num: '96b', w: 5, h: 5, type: 'backtracking', twoStar: 26, threeStar: 20, connects: [] },
	{ num: '97b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['98b'] },
	{ num: '98b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: ['99b'] },
	{ num: '99b', w: 24, h: 25, type: 'growingTree_75_25', twoStar: 400, threeStar: 96, connects: [] },
];

/** The main-path level each bonus passage branches from. */
const WORLD2_BONUS_LINKS: Record<string, string> = {
	'1': '1b', '5': '6b', '13': '9b', '21': '22b', '22': '25b', '31': '26b', '36': '31b',
	'53': '39b', '57': '47b', '58': '50b', '70': '66b', '95': '87b', '96': '91b'
};

const WORLD2_TITLES: Record<number, string[]> = {
	1: [
		'Docking Lines',
		'Starter Relay',
		'Quiet Constellation',
		'Switchback Port',
		'Solar Crossroads',
		'Signal Garden',
		'First Orbit',
		'Prism Rail',
		'Lumen Drift',
		'Comet Lattice',
		'Antenna Bloom',
		'Hollow Beacon',
		'Relay Spine',
		'Static Harbor',
		'Aurora Steps',
		'Startrace Hall',
		'Split Horizon',
		'Wayfinder Array',
		'Zenith Turn',
		'Pulse Divide',
		'Orbital Survey',
		'Launch Seal'
	],
	2: [
		'Debris Choir',
		'Moonwake',
		'Tidal Switch',
		'Hex Drift',
		'Ring of Glass',
		'Shard Slope',
		'Honeycomb Eclipse',
		'Gravity Lock',
		'Satellite Yard',
		'White Noise Run',
		'Meridian Cage',
		'Solar Bridle',
		'Orbital Archive',
		'Helix Switch',
		'Blue Vacuum',
		'Null Garden',
		'Echo Span',
		'Eventide Grid',
		'Rift Compass',
		'Magnetar Lanes',
		'Signal Well',
		'Polaris Fold',
		'Aphelion Gate'
	],
	3: [
		'Deep Sector',
		'Dimming Array',
		'Cold Transit',
		'Belt Survey',
		'Comet Vault',
		'Pressure Spine',
		'Quiet Engine',
		'Blackwell Run',
		'Distant Weave',
		'Ion Bastion',
		'Long Burn',
		'Crownless Relay',
		'Fracture Atlas',
		'Gravity Ledger',
		'Dust Cathedral',
		'Darklane March',
		'Event Horizon',
		'Chain of Lights',
		'Silent Meridian',
		'Starmap Hollow'
	],
	4: [
		'Gateworks',
		'Mirror Freight',
		'Prism Bastille',
		'Bastion Trace',
		'Lockstep Field',
		'Sentinel Drift',
		'Chokepoint Choir',
		'Iron Nebula',
		'Patrol Thread',
		'Aegis Corridor',
		'Latch and Loop',
		'Ember Checkpoint',
		'Spearhead Run',
		'Bastion Bloom',
		'Sealbreaker',
		'Final Watch'
	],
	5: [
		'Blackglass Mile',
		'Crown Current',
		'Hard Vacuum',
		'Last Survey',
		'Furnace Lanes',
		"Warden's Echo",
		'Shiver Array',
		'Rift Crown',
		'Pale Thrusters',
		'Singularity Steps',
		'Zenith Cage',
		'Burnished Grid',
		'Outer Prism',
		'Coldfire Route',
		'Long Meridian',
		'Event Glass',
		'Starless Index',
		'Iron Wake',
		'Binary Crown',
		'Apex Transit',
		'Redline Vault',
		'Null Spire',
		'Final Vector',
		'Crown Spiral',
		'Darklight Steps',
		'Endurance Ring',
		'Astral Static',
		'Horizon Seal',
		'Exit to Dawn'
	]
};

function getWorld2Area(levelNumber: string): { area: World2Area; localIndex: number } | null {
	const value = Number.parseInt(levelNumber, 10);
	if (Number.isNaN(value)) return null;
	if (value <= 22) return { area: 1, localIndex: value };
	if (value <= 45) return { area: 2, localIndex: value - 22 };
	if (value <= 65) return { area: 3, localIndex: value - 45 };
	if (value <= 81) return { area: 4, localIndex: value - 65 };
	return { area: 5, localIndex: value - 81 };
}

function getWorld2Archetype(def: LevelDef, area: World2Area): string {
	if (def.kind === 'boss') return 'Sector Warden';

	if (def.shape === 'hexagonal') return area <= 2 ? 'Hex Drift' : 'Hex Bastion';
	if (def.shape === 'circular') return area <= 2 ? 'Orbital Ring' : 'Gravity Ring';
	if (def.shape === 'triangular') return area <= 2 ? 'Shard Weave' : 'Prism Shards';

	const areaBase = {
		1: 'Launch Grid',
		2: 'Orbital Field',
		3: 'Deep Sector',
		4: 'Gate Bastion',
		5: 'Crownline'
	} as const;

	switch (def.type) {
		case 'recursiveDivision':
		case 'binaryTree':
		case 'sidewinder':
			return `${areaBase[area]} Lanes`;
		case 'prims':
		case 'kruskals':
			return `${areaBase[area]} Mesh`;
		case 'growingTree_50_0':
		case 'growingTree_50_50':
		case 'growingTree_75_25':
		case 'growingTree_25_75':
			return `${areaBase[area]} Weave`;
		default:
			return `${areaBase[area]} Circuit`;
	}
}

function getWorld2Feel(def: LevelDef, area: World2Area): string {
	if (def.kind === 'boss') return 'A deliberate endurance check with a clear sense of escalation.';

	const baseFeel = {
		1: 'Fast, readable, and welcoming.',
		2: 'More varied, with stronger landmarks and sharper pivots.',
		3: 'Longer expeditions with cleaner punishment for doubt.',
		4: 'Fortified routing where structure matters more than speed.',
		5: 'High-pressure navigation with almost no wasted motion.'
	} as const;

	if (def.shape === 'hexagonal') return `${baseFeel[area]} Six-way reads keep the cadence lively.`;
	if (def.shape === 'circular') return `${baseFeel[area]} The ring structure keeps the route legible.`;
	if (def.shape === 'triangular') return `${baseFeel[area]} Angled cells make every correction feel sharper.`;

	return baseFeel[area];
}

function getWorld2Flavor(def: LevelDef, area: World2Area, localIndex: number): string {
	if (def.kind === 'boss') {
		return 'A sector warden folds the star lanes into a single hostile crown, demanding patience before it yields.';
	}

	const areaFlavor = {
		1: 'The early star lanes favor confidence over caution and reward anyone who can keep a clean rhythm.',
		2: 'Orbit begins to bend the route, adding stranger silhouettes without giving up clarity.',
		3: 'Deep space stretches the commitments, turning every landmark into a promise or a warning.',
		4: 'The gateworks feel built to resist intrusion, with lanes that close ranks the moment you hesitate.',
		5: 'Near the crown, the maze hardens into pure intent, asking for efficiency, nerve, and long-range reading.'
	} as const;

	const cadence = [
		'Short commits keep the floor honest.',
		'Landmarks matter more than raw speed.',
		'The true line appears early if you trust the shape.',
		'Every wrong turn teaches something useful.'
	][(localIndex - 1) % 4];

	if (def.shape === 'hexagonal') return `${areaFlavor[area]} ${cadence} Hex angles keep every branch active.`;
	if (def.shape === 'circular') return `${areaFlavor[area]} ${cadence} The circular frame makes orientation part of the puzzle.`;
	if (def.shape === 'triangular') return `${areaFlavor[area]} ${cadence} Triangular pressure makes the floor feel quick and bright.`;

	return `${areaFlavor[area]} ${cadence}`;
}

function getWorld2EncounterTitle(def: LevelDef, area: World2Area, localIndex: number): string {
	if (def.kind === 'boss') return 'Crown of Static';
	return WORLD2_TITLES[area]?.[localIndex - 1] ?? `Sector ${area}, Run ${localIndex}`;
}

function getWorld2Meta(def: LevelDef): AuthoredLevelMeta {
	const areaInfo = getWorld2Area(def.num);
	if (!areaInfo) return {};

	return {
		encounterTitle: getWorld2EncounterTitle(def, areaInfo.area, areaInfo.localIndex),
		flavorText: getWorld2Flavor(def, areaInfo.area, areaInfo.localIndex),
		archetype: def.archetype ?? getWorld2Archetype(def, areaInfo.area),
		feel: def.feel ?? getWorld2Feel(def, areaInfo.area),
		targetClearTimeSec: def.targetClearTimeSec ?? Math.max(10, Math.round(def.threeStar * 0.9))
	};
}

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
		numberOfLevels: world1Defs.length,
		locked: false,
		// Stars that lift the fog over areas 2 to 5 (the native gates before levels 15, 29, 44, 56).
		gateStarRequired: [30, 80, 120, 200],
		levels: world1Defs.map((d, i) => buildLevel(d, i + 1))
	};
}

export function getWorld2(): WorldDefinition {
	return {
		worldId: 2,
		worldName: 'Galactic Grids',
		imageUrl: 'space_background11',
		numberOfLevels: world2Defs.length + world2BonusDefs.length,
		locked: true,
		gateStarRequired: [50, 80, 120, 150, 185, 220, 260, 305, 370, 410, 440, 300, 395, 435, 460, 450, 480],
		levels: [
			...world2Defs.map((d) => {
				const branch = WORLD2_BONUS_LINKS[d.num];
				return { ...d, ...getWorld2Meta(d), ...(branch ? { connects: [branch, ...(d.connects ?? [])] } : {}) };
			}),
			...world2BonusDefs.map((d) => ({ ...d, connects: [...(d.connects ?? [])] }))
		].map((d, i) => buildLevel(d, i + 1))
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
