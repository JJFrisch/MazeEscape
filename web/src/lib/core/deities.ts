/**
 * Algorithm Deity catalog.
 * Each maze-generation algorithm is personified as a named deity with
 * lore, visual identity, and mastery reward structure.
 */

import type { MazeAlgorithm, PowerupName } from './types';

export interface AlgorithmDeity {
	/** Matches the MazeAlgorithm key */
	algorithm: MazeAlgorithm;
	/** Deity display name */
	name: string;
	/** Thematic domain */
	domain: string;
	/** CSS/hex accent color */
	color: string;
	/** Dimmed version for backgrounds */
	colorDim: string;
	/** Short philosophy quote */
	quote: string;
	/** Plain-English description of the algorithm's behavior */
	description: string;
	/** Typical maze characteristics (0–5 scale) */
	traits: {
		corridorLength: number;  // 0=short, 5=long winding
		deadEndDensity: number;  // 0=few, 5=many dead ends
		difficulty: number;      // 0=easy, 5=hard
		randomness: number;      // 0=structured, 5=chaotic
	};
	/** SVG path data for the deity's sigil (simple geometric shape) */
	sigilPath: string;
}

export interface DeityMasteryRewardDefinition {
	item20: {
		powerup: PowerupName;
		amount: number;
		label: string;
	};
	coins80: number;
	skin120Id: number;
	skin120Name: string;
}

export interface MasteryRewardUnlock {
	algorithm: MazeAlgorithm;
	milestone: 20 | 80 | 120;
	rewardType: 'powerup' | 'coins' | 'skin';
	powerupName?: PowerupName;
	amount?: number;
	coinAmount?: number;
	skinId?: number;
	skinName?: string;
}

export const DEITY_CATALOG: AlgorithmDeity[] = [
	{
		algorithm: 'backtracking',
		name: 'The Winding Serpent',
		domain: 'Memory & Recursion',
		color: '#ef4444',
		colorDim: 'rgba(239,68,68,0.15)',
		quote: 'All paths are valid — until proven dead.',
		description: 'Carves deep winding corridors by always pressing forward, then uncoiling back when stuck. Produces mazes with long rivers and few dead ends — but those dead ends run deep.',
		traits: { corridorLength: 5, deadEndDensity: 2, difficulty: 3, randomness: 4 },
		sigilPath: 'M12 2 C18 2 22 6 22 12 C22 18 18 22 12 22 C6 22 2 18 2 12 C2 6 6 2 12 2 M12 7 L12 17 M8 10 L12 7 L16 10',
	},
	{
		algorithm: 'huntAndKill',
		name: 'The Obsidian Hunter',
		domain: 'Pursuit & Selection',
		color: '#f97316',
		colorDim: 'rgba(249,115,22,0.15)',
		quote: 'Walk until you cannot. Then hunt for what remains.',
		description: 'Walks at random until trapped, then systematically hunts unvisited cells row by row. Creates mazes with moderate texture — neither too open nor too dense.',
		traits: { corridorLength: 3, deadEndDensity: 3, difficulty: 3, randomness: 3 },
		sigilPath: 'M12 3 L21 8 L21 16 L12 21 L3 16 L3 8 Z M12 8 L12 16 M8 12 L16 12',
	},
	{
		algorithm: 'prims',
		name: 'The Greedy Arborist',
		domain: 'Growth from a Seed',
		color: '#84cc16',
		colorDim: 'rgba(132,204,22,0.15)',
		quote: 'Always take the nearest step. Never the farthest.',
		description: 'Grows the maze outward from a seed point, always choosing the closest frontier cell. Produces wide, branchy mazes with many short dead ends — easy to get turned around in.',
		traits: { corridorLength: 2, deadEndDensity: 5, difficulty: 4, randomness: 2 },
		sigilPath: 'M12 3 L12 21 M12 8 L7 13 M12 8 L17 13 M12 13 L9 18 M12 13 L15 18',
	},
	{
		algorithm: 'kruskals',
		name: 'The Blind Weaver',
		domain: 'Random Union',
		color: '#06b6d4',
		colorDim: 'rgba(6,182,212,0.15)',
		quote: 'I know not which threads I join — only that none must loop.',
		description: 'Shuffles all walls randomly and removes each one if it connects two unconnected regions. Produces perfectly uniform mazes — every path equally likely, no bias at all.',
		traits: { corridorLength: 2, deadEndDensity: 4, difficulty: 3, randomness: 5 },
		sigilPath: 'M4 4 L20 20 M20 4 L4 20 M12 4 L12 20 M4 12 L20 12',
	},
	{
		algorithm: 'growingTree_50_50',
		name: 'The Balanced Shaper',
		domain: 'Equilibrium',
		color: '#8b5cf6',
		colorDim: 'rgba(139,92,246,0.15)',
		quote: 'Half memory, half chance. The perfect middle path.',
		description: 'Picks from its cell frontier with equal chance of choosing the newest or a random one. Produces mazes balanced between deep rivers and broad bushiness.',
		traits: { corridorLength: 3, deadEndDensity: 3, difficulty: 3, randomness: 3 },
		sigilPath: 'M12 3 A9 9 0 0 1 21 12 A9 9 0 0 1 12 21 A9 9 0 0 1 3 12 A9 9 0 0 1 12 3 M12 3 L12 12 L18 12',
	},
	{
		algorithm: 'growingTree_75_25',
		name: 'The Favored Elder',
		domain: 'Preference & Habit',
		color: '#c084fc',
		colorDim: 'rgba(192,132,252,0.15)',
		quote: 'Memory guides, but chaos keeps it honest.',
		description: 'Favors the newest cell 75% of the time. Produces mostly river-like corridors with occasional side branches — familiar yet unpredictable.',
		traits: { corridorLength: 4, deadEndDensity: 2, difficulty: 3, randomness: 3 },
		sigilPath: 'M12 3 L12 21 M12 3 L19 7 M12 7 L19 11 M12 11 L17 15',
	},
	{
		algorithm: 'growingTree_25_75',
		name: 'The Wild Gambler',
		domain: 'Entropy & Surprise',
		color: '#f472b6',
		colorDim: 'rgba(244,114,182,0.15)',
		quote: 'Why remember where you\'ve been? Surprise yourself.',
		description: 'Picks randomly 75% of the time, newest only 25%. Produces highly branchy, unpredictable mazes with many dead ends — every run feels different.',
		traits: { corridorLength: 1, deadEndDensity: 5, difficulty: 5, randomness: 5 },
		sigilPath: 'M5 5 L19 5 L19 19 L5 19 Z M5 5 L19 19 M5 19 L19 5 M12 5 L12 19 M5 12 L19 12',
	},
	{
		algorithm: 'growingTree_50_0',
		name: 'The Last One Standing',
		domain: 'Pure Recency',
		color: '#38bdf8',
		colorDim: 'rgba(56,189,248,0.15)',
		quote: 'Always return to the newest. Never look back.',
		description: 'Always picks the most recently added cell (equivalent to backtracking). Produces extremely long corridors with deep, infrequent dead ends.',
		traits: { corridorLength: 5, deadEndDensity: 1, difficulty: 2, randomness: 4 },
		sigilPath: 'M3 12 L21 12 M15 6 L21 12 L15 18',
	},
	{
		algorithm: 'wilsons',
		name: 'The Patient Cartographer',
		domain: 'Loop Erasure',
		color: '#34d399',
		colorDim: 'rgba(52,211,153,0.15)',
		quote: 'Walk at random. Erase your loops. Only truth remains.',
		description: 'Performs random walks from unvisited cells, erasing any loops before adding to the maze. Slow to generate but provably uniform — every spanning tree equally likely.',
		traits: { corridorLength: 2, deadEndDensity: 4, difficulty: 3, randomness: 5 },
		sigilPath: 'M4 12 C4 7 8 4 12 4 C16 4 20 7 20 12 C20 17 16 20 12 20 M12 20 L12 12 L16 8',
	},
	{
		algorithm: 'aldousBroder',
		name: 'The Forgotten Wanderer',
		domain: 'Uniform Randomness',
		color: '#94a3b8',
		colorDim: 'rgba(148,163,184,0.15)',
		quote: 'Every cell is equally worthy. I play no favorites.',
		description: 'Wanders randomly, only carving a passage when it reaches an unvisited cell. Slow but produces the most uniformly random mazes possible.',
		traits: { corridorLength: 2, deadEndDensity: 4, difficulty: 3, randomness: 5 },
		sigilPath: 'M12 12 m-8 0 a8 8 0 1 0 16 0 a8 8 0 1 0-16 0 M12 4 L12 6 M12 18 L12 20 M4 12 L6 12 M18 12 L20 12',
	},
	{
		algorithm: 'binaryTree',
		name: 'The Fork-Tongue Oracle',
		domain: 'Binary Choice',
		color: '#fbbf24',
		colorDim: 'rgba(251,191,36,0.15)',
		quote: 'North or East. Always. Only. Forever.',
		description: 'At each cell, carves either North or East — nothing else. Produces mazes with strong diagonal bias, two open edge corridors, and a very recognizable feel.',
		traits: { corridorLength: 3, deadEndDensity: 3, difficulty: 2, randomness: 2 },
		sigilPath: 'M12 3 L12 21 M12 3 L21 3 M4 12 L12 12 L12 3',
	},
	{
		algorithm: 'sidewinder',
		name: 'The River God',
		domain: 'Horizontal Flow',
		color: '#22d3ee',
		colorDim: 'rgba(34,211,238,0.15)',
		quote: 'I carve rivers, not trees. I move in one direction always.',
		description: 'Works row by row, deciding when to carve North vs extend East. Produces mazes with a single open top corridor and horizontal river-like passages.',
		traits: { corridorLength: 4, deadEndDensity: 2, difficulty: 2, randomness: 3 },
		sigilPath: 'M3 8 Q12 5 21 8 M3 12 Q12 9 21 12 M3 16 Q12 13 21 16',
	},
	{
		algorithm: 'ellers',
		name: 'The Row Stitcher',
		domain: 'Row-by-Row Union',
		color: '#a3e635',
		colorDim: 'rgba(163,230,53,0.15)',
		quote: 'I see only the row before me. I never look down.',
		description: 'Processes the maze one row at a time, merging sets and punching vertical passages. Memory-efficient and produces pleasingly textured mazes.',
		traits: { corridorLength: 3, deadEndDensity: 3, difficulty: 3, randomness: 3 },
		sigilPath: 'M3 6 L21 6 M3 10 L21 10 M3 14 L21 14 M3 18 L21 18 M8 6 L8 18 M16 6 L16 18',
	},
	{
		algorithm: 'recursiveDivision',
		name: 'The Great Divider',
		domain: 'Separation & Recursion',
		color: '#fb923c',
		colorDim: 'rgba(251,146,60,0.15)',
		quote: 'I do not build passage. I carve walls into space.',
		description: 'Starts with an open room and recursively divides it with walls, leaving one gap per wall. Produces mazes with long straight corridors and a very different spatial feel.',
		traits: { corridorLength: 4, deadEndDensity: 2, difficulty: 4, randomness: 2 },
		sigilPath: 'M12 3 L12 21 M3 12 L21 12 M3 3 L21 3 L21 21 L3 21 Z',
	},
	{
		algorithm: 'spiralBacktracker',
		name: 'The Coiling Dreamer',
		domain: 'Spiral Memory',
		color: '#e879f9',
		colorDim: 'rgba(232,121,249,0.15)',
		quote: 'I spiral inward before I reach outward.',
		description: 'A backtracking variant that spirals inward first, creating mazes with a distinctive radial structure — challenging to navigate because familiar paths loop back on themselves.',
		traits: { corridorLength: 5, deadEndDensity: 2, difficulty: 5, randomness: 3 },
		sigilPath: 'M12 12 m-7 0 a7 7 0 0 1 7-7 a5 5 0 0 1 5 5 a3 3 0 0 1-3 3 a1.5 1.5 0 0 1-1.5-1.5',
	},
];

export function getDeityByAlgorithm(algorithm: MazeAlgorithm): AlgorithmDeity | undefined {
	return DEITY_CATALOG.find(d => d.algorithm === algorithm);
}

export const DEITY_MASTERY_REWARDS: Record<MazeAlgorithm, DeityMasteryRewardDefinition> = {
	backtracking: {
		item20: { powerup: 'compass', amount: 1, label: 'Serpent Compass' },
		coins80: 600,
		skin120Id: 100,
		skin120Name: 'Serpent Ascendant'
	},
	huntAndKill: {
		item20: { powerup: 'streakShield', amount: 1, label: 'Hunter Ward' },
		coins80: 650,
		skin120Id: 101,
		skin120Name: 'Obsidian Pursuer'
	},
	prims: {
		item20: { powerup: 'doubleCoinsToken', amount: 1, label: 'Arborist Sigil' },
		coins80: 700,
		skin120Id: 102,
		skin120Name: 'Seedforged Bloom'
	},
	kruskals: {
		item20: { powerup: 'blinkScroll', amount: 1, label: 'Weaver Slip' },
		coins80: 700,
		skin120Id: 103,
		skin120Name: 'Threadbound Prism'
	},
	growingTree_50_50: {
		item20: { powerup: 'hourglass', amount: 1, label: 'Shaper Hourglass' },
		coins80: 750,
		skin120Id: 104,
		skin120Name: 'Balanced Reliquary'
	},
	growingTree_75_25: {
		item20: { powerup: 'compass', amount: 2, label: 'Elder Compass' },
		coins80: 750,
		skin120Id: 105,
		skin120Name: 'Favored Remnant'
	},
	growingTree_25_75: {
		item20: { powerup: 'blinkScroll', amount: 1, label: 'Gambler Slip' },
		coins80: 800,
		skin120Id: 106,
		skin120Name: 'Wild Probability'
	},
	growingTree_50_0: {
		item20: { powerup: 'extraMoves', amount: 3, label: 'Relentless Steps' },
		coins80: 650,
		skin120Id: 107,
		skin120Name: 'Lasting Echo'
	},
	wilsons: {
		item20: { powerup: 'hint', amount: 3, label: 'Cartographer Notes' },
		coins80: 650,
		skin120Id: 108,
		skin120Name: 'Erased Atlas'
	},
	aldousBroder: {
		item20: { powerup: 'extraTime', amount: 3, label: 'Wanderer Minutes' },
		coins80: 650,
		skin120Id: 109,
		skin120Name: 'Forgotten Drift'
	},
	binaryTree: {
		item20: { powerup: 'doubleCoinsToken', amount: 1, label: 'Oracle Tithe' },
		coins80: 600,
		skin120Id: 110,
		skin120Name: 'Fork-Tongue Vestment'
	},
	sidewinder: {
		item20: { powerup: 'extraMoves', amount: 2, label: 'River Stride' },
		coins80: 600,
		skin120Id: 111,
		skin120Name: 'Currentborne Flow'
	},
	ellers: {
		item20: { powerup: 'hourglass', amount: 1, label: 'Stitched Hourglass' },
		coins80: 700,
		skin120Id: 112,
		skin120Name: 'Row-Stitched Mantle'
	},
	recursiveDivision: {
		item20: { powerup: 'streakShield', amount: 1, label: 'Divider Bulwark' },
		coins80: 750,
		skin120Id: 113,
		skin120Name: 'Fractured Bastion'
	},
	spiralBacktracker: {
		item20: { powerup: 'hourglass', amount: 1, label: 'Dreamer Coil' },
		coins80: 800,
		skin120Id: 114,
		skin120Name: 'Coiling Reverie'
	}
};

export function getDeityMasteryRewards(algorithm: MazeAlgorithm): DeityMasteryRewardDefinition {
	return DEITY_MASTERY_REWARDS[algorithm];
}

/** Procedurally generate a level encounter name from deity + level number */
const ENCOUNTER_PREFIXES: Record<string, string[]> = {
	backtracking:        ['The Recursing', 'The Coiling', 'The Serpentine', 'The Remembered'],
	huntAndKill:         ['The Hunted', 'The Obsidian', 'The Stalking', 'The Pursued'],
	prims:               ['The Branching', 'The Seeded', 'The Rooted', 'The Growing'],
	kruskals:            ['The Woven', 'The Tangled', 'The Threaded', 'The Blind'],
	growingTree_50_50:   ['The Balanced', 'The Measured', 'The Tempered', 'The Calibrated'],
	growingTree_75_25:   ['The Elder\'s', 'The Favored', 'The Remembered', 'The Ancient'],
	growingTree_25_75:   ['The Chaotic', 'The Scattered', 'The Wild', 'The Frenzied'],
	growingTree_50_0:    ['The Last', 'The Final', 'The Relentless', 'The Unbroken'],
	wilsons:             ['The Patient', 'The Erased', 'The Truthful', 'The Cartographic'],
	aldousBroder:        ['The Forgotten', 'The Wandering', 'The Aimless', 'The Drifting'],
	binaryTree:          ['The Forking', 'The Oracular', 'The Divided', 'The Binary'],
	sidewinder:          ['The Flowing', 'The River', 'The Lateral', 'The Sweeping'],
	ellers:              ['The Stitched', 'The Rowed', 'The Partitioned', 'The Unioned'],
	recursiveDivision:   ['The Divided', 'The Quartered', 'The Cleaved', 'The Sectioned'],
	spiralBacktracker:   ['The Coiled', 'The Dreaming', 'The Spiraling', 'The Inward'],
};

const ENCOUNTER_SUFFIXES = [
	'Chamber', 'Passage', 'Labyrinth', 'Corridor', 'Sanctum',
	'Vault', 'Nexus', 'Hall', 'Rift', 'Crypt',
	'Abyss', 'Expanse', 'Maze', 'Gauntlet', 'Descent',
];

export function generateLevelName(algorithm: MazeAlgorithm, levelNumber: string): string {
	const prefixes = ENCOUNTER_PREFIXES[algorithm] ?? ['The Unknown'];
	const seed = levelNumber.charCodeAt(0) + (levelNumber.length > 1 ? levelNumber.charCodeAt(1) : 0);
	const prefix = prefixes[seed % prefixes.length];
	const suffix = ENCOUNTER_SUFFIXES[(seed * 7 + 3) % ENCOUNTER_SUFFIXES.length];
	return `${prefix} ${suffix}`;
}
