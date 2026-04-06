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
	width: number;
	height: number;
	levelType: MazeAlgorithm;
	mazeShape: MazeShape;
	twoStarMoves: number;
	threeStarTime: number;
	numberOfStars: number;
	minimumStarsToUnlock: number;
	connectTo1: string | null;
	connectTo2: string | null;
	completed: boolean;
	star1: boolean;
	star2: boolean;
	star3: boolean;
	bestMoves: number;
	bestTime: number; // seconds
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
	hintsOwned: number;
	extraTimesOwned: number;
	extraMovesOwned: number;
	currentWorldIndex: number;
	currentSkinId: number;
	unlockedSkinIds: number[];
	wallColor: string; // hex color
	monthPrize1Achieved: boolean;
	monthPrize2Achieved: boolean;
	mostRecentMonth: string;
}

export type MazeAlgorithm =
	| 'backtracking'
	| 'huntAndKill'
	| 'prims'
	| 'kruskals'
	| 'growingTree_50_50'
	| 'growingTree_75_25'
	| 'growingTree_25_75'
	| 'growingTree_50_0';

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
	walls: [boolean, boolean, boolean, boolean, boolean, boolean]; // NE, E, SE, SW, W, NW
}

export type HexDirection = 'ne' | 'e' | 'se' | 'sw' | 'w' | 'nw';

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

export interface PowerupCost {
	name: string;
	displayName: string;
	cost: number;
	icon: string;
}

export const POWERUP_COSTS: PowerupCost[] = [
	{ name: 'hint', displayName: 'Hint', cost: 200, icon: '💡' },
	{ name: 'extraTime', displayName: 'Extra Time', cost: 150, icon: '⏱️' },
	{ name: 'extraMoves', displayName: 'Extra Moves', cost: 50, icon: '👟' }
];
