/**
 * Daily maze logic.
 * Generates a unique maze for each calendar day based on a deterministic seed.
 */

import type { DailyMazeLevel, MazeAlgorithm } from './types';
import { dateSeed, SeededRandom } from './random';

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

/**
 * Generate the daily maze parameters for a given date.
 * The maze itself is generated from these parameters + the seed.
 */
export function getDailyMazeForDate(date: Date): DailyMazeLevel {
	const shortDate = `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()}`;
	const monthYear = `${String(date.getMonth() + 1).padStart(2, '0')}-${date.getFullYear()}`;
	const seed = dateSeed(shortDate);
	const rng = new SeededRandom(seed);

	// Scale difficulty with day of month (days 1-10 easier, 20-31 harder)
	const dayOfMonth = date.getDate();
	const difficulty = Math.min(dayOfMonth / 31, 1); // 0.0 – 1.0
	const smoothDifficulty = difficulty * difficulty * (3 - 2 * difficulty);

	const minSize = 20 + Math.floor(smoothDifficulty * 44); // 20–64
	const variance = 10 - Math.floor(smoothDifficulty * 4); // 10–6
	const maxSize = Math.min(70, minSize + variance); // 30–70 cap with gentler late-month growth
	const width = rng.nextInt(minSize, maxSize + 1);
	const height = rng.nextInt(minSize, maxSize + 1);

	const algorithm = ALGORITHMS[rng.nextInt(0, ALGORITHMS.length)];

	// Thresholds scale with maze size
	const area = width * height;
	const movesNeeded = Math.floor(area * 1.8);
	const timeNeeded = Math.floor(area * 0.6) + 10;

	return {
		levelId: 0,
		shortDate,
		date: date.toISOString(),
		monthYear,
		width,
		height,
		levelType: algorithm,
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
export function getDailyMazesForMonth(year: number, month: number): DailyMazeLevel[] {
	const daysInMonth = new Date(year, month + 1, 0).getDate();
	const mazes: DailyMazeLevel[] = [];

	for (let day = 1; day <= daysInMonth; day++) {
		mazes.push(getDailyMazeForDate(new Date(year, month, day)));
	}

	return mazes;
}

/**
 * Get the seed for generating a daily maze (used to generate the actual maze).
 */
export function getDailyMazeSeed(date: Date): number {
	const shortDate = `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()}`;
	return dateSeed(shortDate);
}
