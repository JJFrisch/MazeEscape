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
	return path;
}

export function calculateStars(
	moves: number,
	elapsedSeconds: number,
	twoStarMoves: number,
	threeStarTime: number
): { star1: boolean; star2: boolean; star3: boolean; total: number } {
	const star1 = true; // always earned on completion
	const star2 = moves <= twoStarMoves;
	const star3 = elapsedSeconds <= threeStarTime;
	return { star1, star2, star3, total: (star1 ? 1 : 0) + (star2 ? 1 : 0) + (star3 ? 1 : 0) };
}
