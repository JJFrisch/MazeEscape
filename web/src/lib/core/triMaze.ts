/**
 * Triangular maze generator.
 * Grid of alternating up-pointing and down-pointing triangles.
 * Each triangle has 3 walls: left edge, right edge, base (top or bottom).
 */

import type { TriCell, TriMazeData } from './types';
import { SeededRandom } from './random';

function cellKey(col: number, row: number): string {
	return `${col},${row}`;
}

function isUpPointing(col: number, row: number): boolean {
	return (col + row) % 2 === 0;
}

type TriDir = 'left' | 'right' | 'base';

interface Neighbor {
	col: number;
	row: number;
	wallHere: TriDir;
	wallThere: TriDir;
}

function getNeighbors(col: number, row: number, cols: number, rows: number): Neighbor[] {
	const up = isUpPointing(col, row);
	const neighbors: Neighbor[] = [];

	// Left neighbor: share the left wall
	if (col > 0) {
		neighbors.push({
			col: col - 1, row,
			wallHere: 'left',
			wallThere: 'right'
		});
	}

	// Right neighbor: share the right wall
	if (col < cols - 1) {
		neighbors.push({
			col: col + 1, row,
			wallHere: 'right',
			wallThere: 'left'
		});
	}

	// Base neighbor: for up-pointing, base is bottom → neighbor is one row down
	// For down-pointing, base is top → neighbor is one row up
	if (up && row < rows - 1) {
		// Down-pointing triangle directly below shares its base (top) wall
		neighbors.push({
			col, row: row + 1,
			wallHere: 'base',
			wallThere: 'base'
		});
	} else if (!up && row > 0) {
		neighbors.push({
			col, row: row - 1,
			wallHere: 'base',
			wallThere: 'base'
		});
	}

	return neighbors;
}

export function generateTriMaze(cols: number, rows: number, seed: number): TriMazeData {
	const rng = new SeededRandom(seed);
	const cells: TriCell[][] = [];

	// Initialize grid
	for (let r = 0; r < rows; r++) {
		const row: TriCell[] = [];
		for (let c = 0; c < cols; c++) {
			row.push({
				col: c,
				row: r,
				pointsUp: isUpPointing(c, r),
				value: 0,
				wallLeft: true,
				wallRight: true,
				wallBase: true
			});
		}
		cells.push(row);
	}

	function getWall(cell: TriCell, dir: TriDir): boolean {
		switch (dir) {
			case 'left': return cell.wallLeft;
			case 'right': return cell.wallRight;
			case 'base': return cell.wallBase;
		}
	}

	function setWall(cell: TriCell, dir: TriDir, value: boolean) {
		switch (dir) {
			case 'left': cell.wallLeft = value; break;
			case 'right': cell.wallRight = value; break;
			case 'base': cell.wallBase = value; break;
		}
	}

	// Recursive backtracking
	const visited = new Set<string>();
	const stack: { col: number; row: number }[] = [];

	const startCol = rng.nextInt(0, cols);
	const startRow = rng.nextInt(0, rows);
	visited.add(cellKey(startCol, startRow));
	stack.push({ col: startCol, row: startRow });

	while (stack.length > 0) {
		const current = stack[stack.length - 1];
		const neighbors = getNeighbors(current.col, current.row, cols, rows);
		const unvisited = neighbors.filter(n => !visited.has(cellKey(n.col, n.row)));

		if (unvisited.length === 0) {
			stack.pop();
			continue;
		}

		const chosen = unvisited[rng.nextInt(0, unvisited.length)];
		// Remove walls
		setWall(cells[current.row][current.col], chosen.wallHere, false);
		setWall(cells[chosen.row][chosen.col], chosen.wallThere, false);

		visited.add(cellKey(chosen.col, chosen.row));
		stack.push({ col: chosen.col, row: chosen.row });
	}

	// BFS for exit
	const start = { col: startCol, row: startRow };
	const bfsQueue = [start];
	const bfsVisited = new Set<string>([cellKey(start.col, start.row)]);
	let furthest = start;

	while (bfsQueue.length > 0) {
		const curr = bfsQueue.shift()!;
		furthest = curr;
		const cell = cells[curr.row][curr.col];
		const neighbors = getNeighbors(curr.col, curr.row, cols, rows);
		for (const n of neighbors) {
			const nk = cellKey(n.col, n.row);
			if (bfsVisited.has(nk)) continue;
			if (!getWall(cell, n.wallHere)) {
				bfsVisited.add(nk);
				bfsQueue.push({ col: n.col, row: n.row });
			}
		}
	}

	const end = furthest;
	cells[start.row][start.col].value = 2;
	cells[end.row][end.col].value = 3;

	return { shape: 'triangular', cells, cols, rows, start, end };
}

/** Check if movement is possible from a tri cell */
export function canMoveTri(
	cells: TriCell[][],
	col: number,
	row: number,
	dir: 'left' | 'right' | 'base',
	cols: number,
	rows: number
): boolean {
	const cell = cells[row]?.[col];
	if (!cell) return false;

	switch (dir) {
		case 'left': return !cell.wallLeft && col > 0;
		case 'right': return !cell.wallRight && col < cols - 1;
		case 'base': {
			if (!cell.wallBase) {
				if (cell.pointsUp) return row < rows - 1;
				else return row > 0;
			}
			return false;
		}
	}
}

export function applyMoveTri(
	col: number,
	row: number,
	pointsUp: boolean,
	dir: 'left' | 'right' | 'base'
): { col: number; row: number } {
	switch (dir) {
		case 'left': return { col: col - 1, row };
		case 'right': return { col: col + 1, row };
		case 'base': return { col, row: pointsUp ? row + 1 : row - 1 };
	}
}

/** Map arrow keys to tri directions */
export function keyToTriDir(
	pointsUp: boolean,
	key: 'up' | 'down' | 'left' | 'right'
): ('left' | 'right' | 'base')[] {
	switch (key) {
		case 'left': return ['left'];
		case 'right': return ['right'];
		case 'up': return pointsUp ? ['left', 'right'] : ['base'];
		case 'down': return pointsUp ? ['base'] : ['left', 'right'];
	}
}
