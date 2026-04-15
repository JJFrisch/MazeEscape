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
// World 1: Cybernetic Labyrinths, Pathbound structure (20 normal, 4 bonus)
// ---------------------------------------------------------------------------
const world1Defs: LevelDef[] = [
	{ num: '1', w: 20, h: 20, type: 'huntAndKill', archetype: 'Grid Intro', feel: 'Welcoming, readable, and confidence-building.', targetClearTimeSec: 45, twoStar: 110, threeStar: 55 },
	{ num: '2', w: 22, h: 20, type: 'kruskals', archetype: 'City Blocks', feel: 'Brisk lanes and tidy decisions.', targetClearTimeSec: 55, twoStar: 120, threeStar: 60 },
	{ num: '3', w: 22, h: 22, type: 'growingTree_50_50', archetype: 'Organic Light', feel: 'Softer flow with light wandering.', targetClearTimeSec: 60, twoStar: 130, threeStar: 65 },
	{ num: '4', w: 24, h: 22, type: 'growingTree_50_0', archetype: 'Banded Horizontal', feel: 'First real choke points and route planning.', targetClearTimeSec: 75, twoStar: 140, threeStar: 75, connects: ['4b'] },
	{ num: '4b', w: 21, h: 21, type: 'kruskals', archetype: 'Donut Reset', feel: 'A celebratory ring-like palate cleanser.', targetClearTimeSec: 65, twoStar: 120, threeStar: 65, shape: 'circular', connects: ['5'] },

	{ num: '5', w: 24, h: 24, type: 'kruskals', archetype: 'Hub and Spoke', feel: 'A center landmark teaches reorientation.', targetClearTimeSec: 85, twoStar: 155, threeStar: 85, minStars: 6 },
	{ num: '6', w: 26, h: 24, type: 'growingTree_25_75', archetype: 'Organic Winding', feel: 'Longer commitments, still fair.', targetClearTimeSec: 95, twoStar: 170, threeStar: 95 },
	{ num: '7', w: 26, h: 26, type: 'prims', archetype: 'Radial Light', feel: 'Circular pull with readable macro-structure.', targetClearTimeSec: 105, twoStar: 185, threeStar: 105 },
	{ num: '8', w: 28, h: 24, type: 'prims', archetype: 'Dense Grid', feel: 'Active decision density without punishment spikes.', targetClearTimeSec: 115, twoStar: 195, threeStar: 115, connects: ['8b'] },
	{ num: '8b', w: 24, h: 22, type: 'growingTree_75_25', archetype: 'Crescent Reset', feel: 'A softer exploratory detour after density.', targetClearTimeSec: 85, twoStar: 155, threeStar: 85, shape: 'triangular', connects: ['9'] },

	{ num: '9', w: 28, h: 26, type: 'huntAndKill', archetype: 'Cavern Rectangle', feel: 'Moodier flow and mild uncertainty.', targetClearTimeSec: 130, twoStar: 215, threeStar: 130, minStars: 15 },
	{ num: '10', w: 30, h: 26, type: 'growingTree_50_0', archetype: 'Banded Vertical', feel: 'Macro-navigation starts to matter.', targetClearTimeSec: 145, twoStar: 230, threeStar: 145 },
	{ num: '11', w: 30, h: 28, type: 'backtracking', archetype: 'Spiral Pressure', feel: 'Draws the player inward, tense but legible.', targetClearTimeSec: 160, twoStar: 245, threeStar: 160 },
	{ num: '12', w: 32, h: 28, type: 'kruskals', archetype: 'Hub plus Ring', feel: 'A mid-world boss structure with multiple good routes.', targetClearTimeSec: 175, twoStar: 260, threeStar: 175, connects: ['12b'] },
	{ num: '12b', w: 27, h: 27, type: 'prims', archetype: 'Clover Reset', feel: 'Strong landmarking and low-friction exploration.', targetClearTimeSec: 105, twoStar: 200, threeStar: 105, shape: 'hexagonal', connects: ['13'] },

	{ num: '13', w: 32, h: 30, type: 'growingTree_50_50', archetype: 'Asymmetric Grid', feel: 'Clever route reading with less symmetry.', targetClearTimeSec: 190, twoStar: 280, threeStar: 190, minStars: 27 },
	{ num: '14', w: 34, h: 30, type: 'kruskals', archetype: 'Radial Hub', feel: 'A strong center with deceptive spokes.', targetClearTimeSec: 205, twoStar: 300, threeStar: 205 },
	{ num: '15', w: 34, h: 32, type: 'growingTree_25_75', archetype: 'Braided Organic', feel: 'Dense but forgiving, with recoverable mistakes.', targetClearTimeSec: 220, twoStar: 320, threeStar: 220 },
	{ num: '16', w: 36, h: 32, type: 'growingTree_75_25', archetype: 'Banded Pockets', feel: 'Layered districts and expedition pacing.', targetClearTimeSec: 235, twoStar: 340, threeStar: 235, connects: ['16b'] },
	{ num: '16b', w: 30, h: 28, type: 'backtracking', archetype: 'Broken Spiral Reset', feel: 'Short spectacle before the final climb.', targetClearTimeSec: 120, twoStar: 250, threeStar: 120, shape: 'circular', connects: ['17'] },

	{ num: '17', w: 36, h: 34, type: 'backtracking', archetype: 'Spiral Capstone', feel: 'Large, dramatic, and still readable by shape.', targetClearTimeSec: 250, twoStar: 360, threeStar: 250, minStars: 42 },
	{ num: '18', w: 38, h: 34, type: 'growingTree_50_50', archetype: 'Hybrid Grid and Organic', feel: 'A mastery test in mixed navigation styles.', targetClearTimeSec: 265, twoStar: 385, threeStar: 265 },
	{ num: '19', w: 40, h: 34, type: 'kruskals', archetype: 'Fortress Hub', feel: 'Sector-based navigation and longer commitments.', targetClearTimeSec: 280, twoStar: 410, threeStar: 280 },
	{ num: '20', w: 42, h: 36, type: 'backtracking', archetype: 'Capstone Hybrid', feel: 'Epic, earned, and clearly structured final climb.', targetClearTimeSec: 300, twoStar: 435, threeStar: 300, connects: [] }
];

type AuthoredLevelMeta = Pick<LevelDef, 'encounterTitle' | 'flavorText' | 'archetype' | 'feel' | 'targetClearTimeSec'>;
type World2Area = 1 | 2 | 3 | 4 | 5;

const WORLD1_META: Record<string, AuthoredLevelMeta> = {
	'1': { encounterTitle: 'Opening Circuit', flavorText: 'The first lanes are bright and forgiving, asking only for clean reads and steady hands.' },
	'2': { encounterTitle: 'Lane Theory', flavorText: 'Orderly blocks turn every choice into a small lesson about momentum and restraint.' },
	'3': { encounterTitle: 'Soft Current', flavorText: 'The route loosens here, inviting curiosity without ever fully hiding the line forward.' },
	'4': { encounterTitle: 'Pressure Bands', flavorText: 'Horizontal tension starts to matter, and the maze asks you to think a turn ahead.' },
	'4b': { encounterTitle: 'Halo Break', flavorText: 'A circular detour clears the head and rewards players who can read the whole ring at once.' },
	'5': { encounterTitle: 'Central Signal', flavorText: 'A strong midpoint gives this floor a landmark rhythm, so every return to center feels earned.' },
	'6': { encounterTitle: 'Green Route', flavorText: 'Longer corridors create commitment, but the punishment for doubt is still fair.' },
	'7': { encounterTitle: 'Lantern Orbit', flavorText: 'The shape nudges the eye around the map, turning orientation into its own quiet challenge.' },
	'8': { encounterTitle: 'Dense Pulse', flavorText: 'Decision density rises fast here, yet the maze stays readable if you trust the broad structure.' },
	'8b': { encounterTitle: 'Crescent Drift', flavorText: 'This angled bonus run relaxes the pacing just enough to make the next climb feel fresh.' },
	'9': { encounterTitle: 'Cavern Static', flavorText: 'The floor darkens in mood, trading certainty for a more cautious, exploratory rhythm.' },
	'10': { encounterTitle: 'Vertical Divide', flavorText: 'Columnar pressure takes over, and route planning starts to happen on a larger scale.' },
	'11': { encounterTitle: 'Spiral Draw', flavorText: 'Everything seems to lean inward, building tension while still leaving enough clues to recover.' },
	'12': { encounterTitle: 'Ring Junction', flavorText: 'Multiple viable routes make this one feel like a summit before the world turns another corner.' },
	'12b': { encounterTitle: 'Clover Skies', flavorText: 'A hex bonus with strong landmarks and light friction lets the player breathe without losing focus.' },
	'13': { encounterTitle: 'Tilted Survey', flavorText: 'Subtle asymmetry keeps the maze clever, rewarding players who read shape before detail.' },
	'14': { encounterTitle: 'Spoke Mirage', flavorText: 'The center promises clarity, then quietly asks whether you can tell a true lane from a lure.' },
	'15': { encounterTitle: 'Braided Run', flavorText: 'The map grows denser, but mistakes remain recoverable if you move with intent.' },
	'16': { encounterTitle: 'Pocket Districts', flavorText: 'Layered neighborhoods turn the floor into an expedition, with each pocket revealing the next.' },
	'16b': { encounterTitle: 'Broken Halo', flavorText: 'A short spectacle of curved motion resets the player before the final push.' },
	'17': { encounterTitle: 'Spiral Crown', flavorText: 'This is where scale becomes drama, but the maze still honors every strong read you make.' },
	'18': { encounterTitle: 'Mixed Terrain', flavorText: 'Rectilinear logic and organic drift collide here, testing whether you can switch styles on command.' },
	'19': { encounterTitle: 'Fortress Sectors', flavorText: 'The route breaks into districts, making commitment and recovery equally important.' },
	'20': { encounterTitle: 'Pathbound Apex', flavorText: 'The world closes with a maze that feels large because of structure, not noise, and victory should feel fully earned.' }
};

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
		gateStarRequired: [6, 15, 27, 42],
		levels: world1Defs.map((d, i) => buildLevel({ ...d, ...WORLD1_META[d.num] }, i + 1))
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
		levels: world2Defs.map((d, i) => buildLevel({ ...d, ...getWorld2Meta(d) }, i + 1))
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
