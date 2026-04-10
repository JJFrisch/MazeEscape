// Core types for MazeEscape web client
// Ported from C# MazeEscape.Core

export interface MazeCell {
	x: number;
	y: number;
	/** 0=empty, 2=start, 3=end */
	value: number;
	/** Wall to the east (right) */
	east: boolean;
	/** Wall to the north (top) */
	north: boolean;
}

export interface Position {
	x: number;
	y: number;
}

export type Direction = 'up' | 'down' | 'left' | 'right';

export interface MoveResult {
	moved: boolean;
	x: number;
	y: number;
}

export interface MazeData {
	cells: MazeCell[][];
	width: number;
	height: number;
	start: Position;
	end: Position;
	path: Position[];
	pathLength: number;
}

export interface CampaignLevel {
	levelId: number;
	levelNumber: string;
	levelKind?: 'standard' | 'boss';
	width: number;
	height: number;
	levelType: MazeAlgorithm;
	mazeShape: MazeShape;
	twoStarMoves: number;
	threeStarTime: number;
	fiveStarMoves: number;
	fiveStarTime: number;
	numberOfStars: number;
	minimumStarsToUnlock: number;
	connectTo1: string | null;
	connectTo2: string | null;
	/** Optional reward popup shown after completing this level */
	levelReward?: LevelReward;
	completed: boolean;
	star1: boolean;
	star2: boolean;
	star3: boolean;
	star4: boolean;
	star5: boolean;
	bestMoves: number;
	bestTime: number; // seconds
	bossFlavor?: string;
}

export interface CampaignWorld {
	worldId: number;
	worldName: string;
	imageUrl: string;
	numberOfLevels: number;
	highestBeatenLevel: number;
	completed: boolean;
	locked: boolean;
	starCount: number;
	unlockedMazeNumbers: string[];
	unlockedGateNumbers: number[];
	levelConnections: Record<string, string[]>;
	highestAreaUnlocked: number;
	gateStarRequired: number[];
	rewards: RewardDefinition[];
}

export type RewardType = 'chest' | 'portal' | 'skin_unlock' | 'key';

export interface RewardDefinition {
	id: string;
	type: RewardType;
	x: number;
	y: number;
	opened: boolean;
	/** For portal: stars needed. For skin_unlock: skinId. For key: connectsTo. */
	data?: string | number;
}

export interface SkinModel {
	id: number;
	name: string;
	imageUrl: string;
	coinPrice: number;
	gemPrice: number;
	isUnlocked: boolean;
	isEquipped: boolean;
	isSpecialSkin: boolean;
	masteryAlgorithm?: MazeAlgorithm;
}

export interface MasteryRewardClaimState {
	item20: boolean;
	coins80: boolean;
	skin120: boolean;
}

export interface DailyMazeLevel {
	levelId: number;
	shortDate: string;
	date: string; // ISO date string
	monthYear: string;
	width: number;
	height: number;
	levelType: MazeAlgorithm;
	status: DailyMazeStatus;
	timeNeeded: number;
	movesNeeded: number;
	completionTime: number;
	completionMoves: number;
}

export type DailyMazeStatus = 'not_started' | 'completed' | 'completed_late';

export interface PlayerData {
	playerId: string;
	playerName: string;
	coinCount: number;
	gemCount: number;
	hintsOwned: number;
	extraTimesOwned: number;
	extraMovesOwned: number;
	// New consumables
	compassOwned: number;
	hourglassOwned: number;
	blinkScrollsOwned: number;
	streakShieldsOwned: number;
	doubleCoinsTokensOwned: number;
	doubleCoinsActive: boolean;
	currentWorldIndex: number;
	currentSkinId: number;
	unlockedSkinIds: number[];
	wallColor: string; // hex color
	monthPrize1Achieved: boolean;
	monthPrize2Achieved: boolean;
	mostRecentMonth: string;
	// Streak tracking
	currentStreak: number;
	bestStreak: number;
	lastDailyDate: string; // 'M/D/YYYY'
	// Algorithm mastery (completions per algorithm type)
	algoMasteryCount: Partial<Record<MazeAlgorithm, number>>;
	masteryRewardsClaimed: Partial<Record<MazeAlgorithm, MasteryRewardClaimState>>;
	// Lifetime stats
	coinsEarnedLifetime: number;
	// Achievements
	achievements: Record<string, { progress: number; unlocked: boolean; dateUnlocked?: string }>;
	// Crafting
	crystalShards: number;
	// Total mazes completed (campaign + daily)
	mazesCompleted: number;
	// Boss rewards / relic inventory
	specialItemIds: string[];
}

export interface GhostRunData {
	moves: Direction[];
	updatedAt?: number;
}

export interface EventProgress {
	eventId: string;
	progress: number;
	completedMilestones: number[];
	updatedAt?: number;
}

export type MazeAlgorithm =
	| 'backtracking'
	| 'huntAndKill'
	| 'prims'
	| 'kruskals'
	| 'growingTree_50_50'
	| 'growingTree_75_25'
	| 'growingTree_25_75'
	| 'growingTree_50_0'
	| 'wilsons'
	| 'aldousBroder'
	| 'binaryTree'
	| 'sidewinder'
	| 'ellers'
	| 'recursiveDivision'
	| 'spiralBacktracker';

/** Shape of the maze grid topology */
export type MazeShape = 'rectangular' | 'hexagonal' | 'circular' | 'triangular';

// ---------------------------------------------------------------------------
// Hexagonal maze types (flat-top hex grid, offset coordinates)
// ---------------------------------------------------------------------------

/** A hex cell has 6 walls: NE, E, SE, SW, W, NW (clockwise from top-right) */
export interface HexCell {
	col: number;
	row: number;
	/** 0=empty, 2=start, 3=end */
	value: number;
	/** Walls — true = wall present */
	walls: [boolean, boolean, boolean, boolean, boolean, boolean]; // N, NE, SE, S, SW, NW
}

export type HexDirection = 'n' | 'ne' | 'se' | 's' | 'sw' | 'nw';

export interface HexMazeData {
	shape: 'hexagonal';
	cells: Map<string, HexCell>; // key = "col,row"
	cols: number;
	rows: number;
	start: { col: number; row: number };
	end: { col: number; row: number };
}

// ---------------------------------------------------------------------------
// Circular / polar maze types (concentric rings)
// ---------------------------------------------------------------------------

/** A ring cell in a circular maze */
export interface RingCell {
	ring: number;   // 0 = center
	sector: number; // sector index within the ring
	value: number;  // 0=empty, 2=start, 3=end
	/** Walls: [clockwise, outward] — true = wall present */
	wallCW: boolean;   // wall to next sector clockwise
	wallOut: boolean;  // wall to next ring outward
}

export interface CircularMazeData {
	shape: 'circular';
	rings: RingCell[][]; // rings[ring][sector]
	numRings: number;
	start: { ring: number; sector: number };
	end: { ring: number; sector: number };
}

// ---------------------------------------------------------------------------
// Triangular maze types (up/down triangle grid)
// ---------------------------------------------------------------------------

/** A triangle cell — alternates up/down orientation */
export interface TriCell {
	col: number;
	row: number;
	pointsUp: boolean;
	value: number;
	/** Walls for up-pointing: [left, right, base(bottom)]
	 *  Walls for down-pointing: [left, right, base(top)] */
	wallLeft: boolean;
	wallRight: boolean;
	wallBase: boolean;
}

export interface TriMazeData {
	shape: 'triangular';
	cells: TriCell[][];  // [row][col]
	cols: number;
	rows: number;
	start: { col: number; row: number };
	end: { col: number; row: number };
}

export type PowerupName =
	| 'hint'
	| 'extraTime'
	| 'extraMoves'
	| 'compass'
	| 'hourglass'
	| 'blinkScroll'
	| 'streakShield'
	| 'doubleCoinsToken';

export interface PowerupCost {
	name: PowerupName;
	displayName: string;
	cost: number;
	icon: string;
	description: string;
	flavorText: string;
	rarity: 'common' | 'uncommon' | 'rare';
	accentColor: string;
	tags?: string[];
}

export const POWERUP_COSTS: PowerupCost[] = [
	{
		name: 'hint',
		displayName: 'Hint',
		cost: 200,
		icon: '💡',
		description: 'Reveals the full path to the exit.',
		flavorText: 'A gift from the Patient Cartographer.',
		rarity: 'common',
		accentColor: 'rgba(56,189,248,'
	},
	{
		name: 'extraTime',
		displayName: 'Extra Time',
		cost: 150,
		icon: '⏱️',
		description: 'Adds bonus seconds toward 3-star time.',
		flavorText: 'The Coiling Dreamer bends the clock.',
		rarity: 'common',
		accentColor: 'rgba(167,139,250,'
	},
	{
		name: 'extraMoves',
		displayName: 'Extra Moves',
		cost: 50,
		icon: '👣',
		description: 'Adds bonus moves toward 2-star threshold.',
		flavorText: 'The Wanderer grants you extra steps.',
		rarity: 'common',
		accentColor: 'rgba(52,211,153,'
	},
	{
		name: 'compass',
		displayName: 'Compass',
		cost: 75,
		icon: '🧭',
		description: 'Flashes the exit direction for 3 seconds.',
		flavorText: 'It knows not your path, only your destination\'s bearing.',
		rarity: 'common',
		accentColor: 'rgba(251,191,36,'
	},
	{
		name: 'hourglass',
		displayName: 'Hourglass',
		cost: 300,
		icon: '⌛',
		description: 'Freezes the timer for 15 seconds.',
		flavorText: 'The Coiling Dreamer slows time for those who ask kindly.',
		rarity: 'uncommon',
		accentColor: 'rgba(34,211,238,'
	},
	{
		name: 'blinkScroll',
		displayName: 'Blink Scroll',
		cost: 450,
		icon: '📜',
		description: 'Returns to start without ending the run or costing moves.',
		flavorText: 'The Wanderer walked these halls before you. Step where he stepped.',
		rarity: 'rare',
		accentColor: 'rgba(249,115,22,'
	},
	{
		name: 'streakShield',
		displayName: 'Streak Shield',
		cost: 800,
		icon: '🛡️',
		description: 'Protects your daily streak if you miss one day.',
		flavorText: 'A pact with the Hunter. Miss a day — this burns in your place.',
		rarity: 'rare',
		accentColor: 'rgba(148,163,184,'
	},
	{
		name: 'doubleCoinsToken',
		displayName: 'Double Coins',
		cost: 600,
		icon: '🪙',
		description: 'Your next maze completion earns 2× coins.',
		flavorText: 'The Greedy Arborist\'s blessing — the next path you carve yields twice the reward.',
		rarity: 'uncommon',
		accentColor: 'rgba(245,158,11,'
	},
];

// ---------------------------------------------------------------------------
// Campaign map types — treasure-map-style level select
// ---------------------------------------------------------------------------

/** A tile coordinate on the campaign map grid */
export interface MapTile {
	col: number;
	row: number;
}

/** Types of nodes placed on the campaign map */
export type MapNodeType = 'level' | 'bonus_level' | 'bonus_end' | 'star_gate' | 'key_gate' | 'portal' | 'boss';

/** Collectible type — items the player can pick up on or off the map */
export type MapCollectibleType = 'chest' | 'key' | 'gem' | 'cloak' | 'powerup_hint' | 'powerup_time' | 'powerup_moves' | 'boss_relic';

/** A level or gate node on the campaign map */
export interface MapNode {
	id: string;
	type: MapNodeType;
	tile: MapTile;
	/** For level/bonus_level nodes: the level number string */
	levelNumber?: string;
	/** For star_gate nodes: stars required to pass */
	starsRequired?: number;
	/** For key_gate nodes: id of the key item required */
	keyItemId?: string;
	/** Boss levels reuse the ordinary play page via level number */
	bossLevelNumber?: string;
	/** Optional custom title on the encounter card */
	encounterTitle?: string;
	/** Optional boss flavor text */
	bossFlavor?: string;
	/** Area index (1–5); gates belong to the area they open */
	area: number;
}

/** A segment of path drawn between two tiles */
export interface MapPathSegment {
	from: MapTile;
	to: MapTile;
	/** true = bonus branch (dashed corridor) */
	bonus: boolean;
}

/** A fog-of-war region covering an area of the map */
export interface MapFogRegion {
	/** Area index this fog covers (lifts when player unlocks this area) */
	area: number;
	/** Top-left corner tile */
	topLeft: MapTile;
	/** Bottom-right corner tile (inclusive) */
	bottomRight: MapTile;
}

/** A collectible item placed on the campaign map */
export interface MapCollectible {
	id: string;
	type: MapCollectibleType;
	tile: MapTile;
	area: number;
	/** Human-readable label shown in popup */
	label: string;
	/** Reward payload: coins amount, or skin id, or powerup count */
	reward: MapCollectibleReward;
}

export interface MapCollectibleReward {
	coins?: number;
	skinId?: number;
	powerup?: 'hint' | 'extraTime' | 'extraMoves';
	powerupCount?: number;
	/** If this collectible grants a key, the key item id */
	keyItemId?: string;
	specialItemId?: string;
}

/** Full layout definition for one world's campaign map */
export interface WorldMapLayout {
	worldId: number;
	/** Total tile columns */
	cols: number;
	/** Total tile rows */
	rows: number;
	/** Virtual px per tile (used for SVG sizing) */
	tileSize: number;
	nodes: MapNode[];
	/** Ordered path segments connecting nodes */
	pathSegments: MapPathSegment[];
	collectibles: MapCollectible[];
	fogRegions: MapFogRegion[];
}

/** Optional reward granted to the player upon completing a specific level */
export interface LevelReward {
	type: MapCollectibleType;
	label: string;
	reward: MapCollectibleReward;
}
