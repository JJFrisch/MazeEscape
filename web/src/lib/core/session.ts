/**
 * Game session: manages a single maze playthrough (campaign or daily).
 * Tracks player position, move count, timer, and completion state.
 */

import { generateMaze, canMove, applyMove, findPathToEnd } from './maze';
import type { MazeData, MazeAlgorithm, Position, Direction, MoveResult } from './types';

export interface GameSessionState {
	maze: MazeData;
	playerPos: Position;
	moves: number;
	elapsed: number; // seconds
	hintsUsed: number;
	isComplete: boolean;
	hintPath: Position[] | null;
}

export interface GameSessionConfig {
	width: number;
	height: number;
	algorithm: MazeAlgorithm;
	seed: number;
	twoStarMoves?: number;
	threeStarTime?: number;
}

export function createGameSession(config: GameSessionConfig): GameSessionState {
	const maze = generateMaze(config.width, config.height, config.algorithm, config.seed);
	return {
		maze,
		playerPos: { ...maze.start },
		moves: 0,
		elapsed: 0,
		hintsUsed: 0,
		isComplete: false,
		hintPath: null
	};
}

export function tryMove(state: GameSessionState, direction: Direction): MoveResult {
	if (state.isComplete) return { moved: false, x: state.playerPos.x, y: state.playerPos.y };

	const { maze, playerPos } = state;
	if (!canMove(maze.cells, playerPos, direction, maze.width, maze.height)) {
		return { moved: false, x: playerPos.x, y: playerPos.y };
	}

	const newPos = applyMove(playerPos, direction);
	state.playerPos = newPos;
	state.moves++;
	state.hintPath = null; // Clear hint on move

	if (newPos.x === maze.end.x && newPos.y === maze.end.y) {
		state.isComplete = true;
	}

	return { moved: true, x: newPos.x, y: newPos.y };
}

export function getHint(state: GameSessionState): Position[] {
	const path = findPathToEnd(
		state.maze.cells,
		state.playerPos,
		state.maze.end,
		state.maze.width,
		state.maze.height
	);
	state.hintPath = path;
	state.hintsUsed++;
	return path;
}

/**
 * Compass: show the first `n` cells of the solution path without
 * penalizing hintsUsed (so star4 is unaffected).
 */
export function getCompassPath(state: GameSessionState, n = 4): Position[] {
	const fullPath = findPathToEnd(
		state.maze.cells,
		state.playerPos,
		state.maze.end,
		state.maze.width,
		state.maze.height
	);
	const partial = fullPath.slice(0, n + 1); // include player pos as first cell
	state.hintPath = partial;
	return partial;
}

export function calculateStars(
	moves: number,
	elapsedSeconds: number,
	hintsUsed: number,
	twoStarMoves: number,
	threeStarTime: number,
	fiveStarMoves: number,
	fiveStarTime: number
): { star1: boolean; star2: boolean; star3: boolean; star4: boolean; star5: boolean; total: number } {
	const star1 = true; // always earned on completion
	const star2 = moves <= twoStarMoves;
	const star3 = elapsedSeconds <= threeStarTime;
	const star4 = hintsUsed === 0;
	const star5 = moves <= fiveStarMoves && elapsedSeconds <= fiveStarTime;
	return {
		star1,
		star2,
		star3,
		star4,
		star5,
		total: (star1 ? 1 : 0) + (star2 ? 1 : 0) + (star3 ? 1 : 0) + (star4 ? 1 : 0) + (star5 ? 1 : 0)
	};
}

export function getMoveThresholdsForOptimalPath(optimalPathLength: number): { twoStarMoves: number; fiveStarMoves: number } {
	const optimalMoves = Math.max(1, Math.floor(optimalPathLength));
	return {
		twoStarMoves: optimalMoves + Math.max(2, Math.ceil(optimalMoves * 0.22)),
		fiveStarMoves: optimalMoves + Math.max(0, Math.ceil(optimalMoves * 0.08))
	};
}
