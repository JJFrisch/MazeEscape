/**
 * Daily maze logic.
 * Generates a unique maze for each calendar day based on a deterministic seed.
 */

import type { DailyMazeLevel, MazeAlgorithm, MazeShape } from './types';
import { dateSeed, SeededRandom } from './random';
import { getActiveEvent } from './events';

export interface DailyMazeViewport {
	width: number;
	height: number;
}

const ALGORITHMS: MazeAlgorithm[] = [
	'backtracking',
	'huntAndKill',
	'prims',
	'kruskals',
	'growingTree_50_50',
	'growingTree_75_25',
	'growingTree_25_75',
	'growingTree_50_0',
	'wilsons',
	'aldousBroder',
	'binaryTree',
	'sidewinder',
	'ellers',
	'recursiveDivision',
	'spiralBacktracker'
];

function shouldUseWideDailyLayout(viewport?: DailyMazeViewport): boolean {
	if (!viewport) return false;
	if (viewport.width < 1100 || viewport.height < 700) return false;
	return viewport.width / Math.max(viewport.height, 1) >= 1.45;
}

function getDailyMazeDimensions(
	rng: SeededRandom,
	smoothDifficulty: number,
	minSize: number,
	maxSize: number,
	viewport?: DailyMazeViewport
): { width: number; height: number } {
	if (!shouldUseWideDailyLayout(viewport)) {
		return {
			width: rng.nextInt(minSize, maxSize + 1),
			height: rng.nextInt(minSize, maxSize + 1)
		};
	}

	const minHeight = 14 + Math.floor(smoothDifficulty * 18);
	const heightVariance = Math.max(4, 7 - Math.floor(smoothDifficulty * 3));
	const height = rng.nextInt(minHeight, minHeight + heightVariance + 1);
	const aspectRatio = rng.nextInt(18, 21) / 10;
	const width = Math.max(height + 6, Math.round(height * aspectRatio));

	return { width, height };
}

/**
 * Generate the daily maze parameters for a given date.
 * The maze itself is generated from these parameters + the seed.
 */
export function getDailyMazeForDate(date: Date, viewport?: DailyMazeViewport): DailyMazeLevel {
	const shortDate = `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()}`;
	const monthYear = `${String(date.getMonth() + 1).padStart(2, '0')}-${date.getFullYear()}`;
	const activeEvent = getActiveEvent(date);
	const seed = dateSeed(activeEvent ? `${activeEvent.id}:${shortDate}` : shortDate);
	const rng = new SeededRandom(seed);

	// Scale difficulty with day of month (days 1-10 easier, 20-31 harder)
	const dayOfMonth = date.getDate();
	const difficulty = Math.min(dayOfMonth / 31, 1); // 0.0 – 1.0
	const smoothDifficulty = difficulty * difficulty * (3 - 2 * difficulty);

	// Days 5/10/15/20/25 use non-rectangular shapes in rotation
	const NON_RECT: Record<number, MazeShape> = {
		5: 'hexagonal',
		10: 'circular',
		15: 'triangular',
		20: 'hexagonal',
		25: 'circular'
	};
	const mazeShape: MazeShape = NON_RECT[dayOfMonth] ?? 'rectangular';

	let width: number;
	let height: number;

	if (mazeShape === 'hexagonal') {
		width  = 5 + Math.floor(smoothDifficulty * 13) + rng.nextInt(0, 3);
		height = 4 + Math.floor(smoothDifficulty * 11) + rng.nextInt(0, 3);
	} else if (mazeShape === 'circular') {
		const rings = 4 + Math.floor(smoothDifficulty * 8) + rng.nextInt(0, 3);
		width  = rings;
		height = rings;
	} else if (mazeShape === 'triangular') {
		width  = 8  + Math.floor(smoothDifficulty * 16) + rng.nextInt(0, 4);
		height = 7  + Math.floor(smoothDifficulty * 13) + rng.nextInt(0, 4);
	} else {
		const minSize = 20 + Math.floor(smoothDifficulty * 44); // 20–64
		const variance = 10 - Math.floor(smoothDifficulty * 4); // 10–6
		const maxSize = Math.min(70, minSize + variance);
		const dims = getDailyMazeDimensions(rng, smoothDifficulty, minSize, maxSize, viewport);
		width  = dims.width;
		height = dims.height;
	}

	const algorithmPool = activeEvent?.dailyAlgorithmPool?.length ? activeEvent.dailyAlgorithmPool : ALGORITHMS;
	const algorithm = algorithmPool[rng.nextInt(0, algorithmPool.length)];

	// Thresholds scale with maze size (rectangular only; non-rect recalculates after generation)
	const area = width * height;
	const movesNeeded = Math.floor(area * (mazeShape === 'rectangular' ? 1.8 : 1.2));
	const timeNeeded  = Math.floor(area * (mazeShape === 'rectangular' ? 0.6 : 0.5)) + 10;

	return {
		levelId: 0,
		shortDate,
		date: date.toISOString(),
		monthYear,
		width,
		height,
		levelType: algorithm,
		mazeShape,
		status: 'not_started',
		timeNeeded,
		movesNeeded,
		completionTime: 0,
		completionMoves: 0
	};
}

/**
 * Get all daily mazes for a given month.
 */
export function getDailyMazesForMonth(year: number, month: number, viewport?: DailyMazeViewport): DailyMazeLevel[] {
	const daysInMonth = new Date(year, month + 1, 0).getDate();
	const mazes: DailyMazeLevel[] = [];

	for (let day = 1; day <= daysInMonth; day++) {
		mazes.push(getDailyMazeForDate(new Date(year, month, day), viewport));
	}

	return mazes;
}

/**
 * Get the seed for generating a daily maze (used to generate the actual maze).
 */
export function getDailyMazeSeed(date: Date): number {
	const shortDate = `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()}`;
	const activeEvent = getActiveEvent(date);
	return dateSeed(activeEvent ? `${activeEvent.id}:${shortDate}` : shortDate);
}
