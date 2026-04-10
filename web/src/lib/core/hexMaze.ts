/**
 * Hexagonal maze generator.
 * Uses flat-top hex grid with offset (odd-q) coordinates.
 * Each hex cell has 6 walls: N(0), NE(1), SE(2), S(3), SW(4), NW(5).
 */

import type { HexCell, HexMazeData, HexDirection } from './types';
import { SeededRandom } from './random';

const DIRECTIONS: HexDirection[] = ['n', 'ne', 'se', 's', 'sw', 'nw'];
const OPPOSITE: Record<HexDirection, HexDirection> = {
	n: 's',
	ne: 'sw',
	se: 'nw',
	s: 'n',
	sw: 'ne',
	nw: 'se'
};
const DIR_INDEX: Record<HexDirection, number> = {
	n: 0, ne: 1, se: 2, s: 3, sw: 4, nw: 5
};

function cellKey(col: number, row: number): string {
	return `${col},${row}`;
}

/**
 * Flat-top hex, odd-q offset: odd columns are shifted down by half a row.
 */
function getNeighbor(col: number, row: number, dir: HexDirection): { col: number; row: number } {
	const isOdd = col & 1;
	switch (dir) {
		case 'n':  return { col, row: row - 1 };
		case 'ne': return { col: col + 1, row: isOdd ? row : row - 1 };
		case 'se': return { col: col + 1, row: isOdd ? row + 1 : row };
		case 's':  return { col, row: row + 1 };
		case 'sw': return { col: col - 1, row: isOdd ? row + 1 : row };
		case 'nw': return { col: col - 1, row: isOdd ? row : row - 1 };
	}
}

export function generateHexMaze(cols: number, rows: number, seed: number): HexMazeData {
	const rng = new SeededRandom(seed);
	const cells = new Map<string, HexCell>();

	// Initialize grid — only use cols that keep neighbors in bounds
	for (let r = 0; r < rows; r++) {
		for (let c = 0; c < cols; c++) {
			cells.set(cellKey(c, r), {
				col: c,
				row: r,
				value: 0,
				walls: [true, true, true, true, true, true]
			});
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
		const unvisitedNeighbors: { dir: HexDirection; col: number; row: number }[] = [];

		for (const dir of DIRECTIONS) {
			const n = getNeighbor(current.col, current.row, dir);
			const nk = cellKey(n.col, n.row);
			if (cells.has(nk) && !visited.has(nk)) {
				unvisitedNeighbors.push({ dir, ...n });
			}
		}

		if (unvisitedNeighbors.length === 0) {
			stack.pop();
			continue;
		}

		const chosen = unvisitedNeighbors[rng.nextInt(0, unvisitedNeighbors.length)];
		const currentCell = cells.get(cellKey(current.col, current.row))!;
		const nextCell = cells.get(cellKey(chosen.col, chosen.row))!;

		// Remove walls between current and chosen
		currentCell.walls[DIR_INDEX[chosen.dir]] = false;
		nextCell.walls[DIR_INDEX[OPPOSITE[chosen.dir]]] = false;

		visited.add(cellKey(chosen.col, chosen.row));
		stack.push({ col: chosen.col, row: chosen.row });
	}

	// BFS to find furthest cell from start for exit placement
	const start = { col: startCol, row: startRow };
	const bfsQueue = [start];
	const bfsVisited = new Set<string>([cellKey(start.col, start.row)]);
	let furthest = start;

	while (bfsQueue.length > 0) {
		const curr = bfsQueue.shift()!;
		furthest = curr;
		const cell = cells.get(cellKey(curr.col, curr.row))!;
		for (const dir of DIRECTIONS) {
			if (!cell.walls[DIR_INDEX[dir]]) {
				const n = getNeighbor(curr.col, curr.row, dir);
				const nk = cellKey(n.col, n.row);
				if (!bfsVisited.has(nk) && cells.has(nk)) {
					bfsVisited.add(nk);
					bfsQueue.push(n);
				}
			}
		}
	}

	const end = furthest;
	cells.get(cellKey(start.col, start.row))!.value = 2;
	cells.get(cellKey(end.col, end.row))!.value = 3;

	return { shape: 'hexagonal', cells, cols, rows, start, end };
}

/** Check if movement in a direction is possible from a hex cell */
export function canMoveHex(
	cells: Map<string, HexCell>,
	col: number,
	row: number,
	dir: HexDirection
): boolean {
	const cell = cells.get(cellKey(col, row));
	if (!cell) return false;
	if (cell.walls[DIR_INDEX[dir]]) return false;
	const n = getNeighbor(col, row, dir);
	return cells.has(cellKey(n.col, n.row));
}

/** Get the neighbor position for a hex direction */
export function applyMoveHex(
	col: number,
	row: number,
	dir: HexDirection
): { col: number; row: number } {
	return getNeighbor(col, row, dir);
}

/** Map keyboard directions to hex directions based on position */
export function keyToHexDir(key: 'up' | 'down' | 'left' | 'right'): HexDirection[] {
	switch (key) {
		case 'up':    return ['n', 'nw', 'ne'];
		case 'down':  return ['s', 'sw', 'se'];
		case 'left':  return ['nw', 'sw'];
		case 'right': return ['ne', 'se'];
	}
}

export function getHexOptimalPathLength(data: HexMazeData): number {
	const startKey = cellKey(data.start.col, data.start.row);
	const endKey = cellKey(data.end.col, data.end.row);
	const queue: Array<{ col: number; row: number; dist: number }> = [
		{ col: data.start.col, row: data.start.row, dist: 0 }
	];
	const visited = new Set<string>([startKey]);

	while (queue.length > 0) {
		const current = queue.shift()!;
		if (cellKey(current.col, current.row) === endKey) {
			return current.dist;
		}

		const cell = data.cells.get(cellKey(current.col, current.row));
		if (!cell) continue;

		for (const dir of DIRECTIONS) {
			if (cell.walls[DIR_INDEX[dir]]) continue;
			const next = getNeighbor(current.col, current.row, dir);
			const nextKey = cellKey(next.col, next.row);
			if (!data.cells.has(nextKey) || visited.has(nextKey)) continue;
			visited.add(nextKey);
			queue.push({ col: next.col, row: next.row, dist: current.dist + 1 });
		}
	}

	return 0;
}

// ---------------------------------------------------------------------------
// Internal helper: remove wall between two adjacent hex cells
// ---------------------------------------------------------------------------
function hexLinkCells(cells: Map<string, HexCell>, c1: number, r1: number, c2: number, r2: number): void {
	for (const dir of DIRECTIONS) {
		const n = getNeighbor(c1, r1, dir);
		if (n.col === c2 && n.row === r2) {
			cells.get(cellKey(c1, r1))!.walls[DIR_INDEX[dir]] = false;
			cells.get(cellKey(c2, r2))!.walls[DIR_INDEX[OPPOSITE[dir]]] = false;
			return;
		}
	}
}

function initHexGrid(cols: number, rows: number): Map<string, HexCell> {
	const cells = new Map<string, HexCell>();
	for (let r = 0; r < rows; r++)
		for (let c = 0; c < cols; c++)
			cells.set(cellKey(c, r), { col: c, row: r, value: 0, walls: [true, true, true, true, true, true] });
	return cells;
}

function hexBfsExit(
	cells: Map<string, HexCell>,
	start: { col: number; row: number }
): { col: number; row: number } {
	const bfsQueue = [start];
	const bfsVisited = new Set<string>([cellKey(start.col, start.row)]);
	let furthest = start;
	while (bfsQueue.length > 0) {
		const curr = bfsQueue.shift()!;
		furthest = curr;
		const cell = cells.get(cellKey(curr.col, curr.row))!;
		for (const dir of DIRECTIONS) {
			if (!cell.walls[DIR_INDEX[dir]]) {
				const n = getNeighbor(curr.col, curr.row, dir);
				const nk = cellKey(n.col, n.row);
				if (!bfsVisited.has(nk) && cells.has(nk)) {
					bfsVisited.add(nk);
					bfsQueue.push(n);
				}
			}
		}
	}
	return furthest;
}

// ---------------------------------------------------------------------------
// Prim's on hexagonal grid
// ---------------------------------------------------------------------------
export function generateHexMazePrims(cols: number, rows: number, seed: number): HexMazeData {
	const rng = new SeededRandom(seed);
	const cells = initHexGrid(cols, rows);

	const startCol = rng.nextInt(0, cols);
	const startRow = rng.nextInt(0, rows);
	const visited = new Set<string>([cellKey(startCol, startRow)]);

	const frontier: { col: number; row: number }[] = [];
	for (const dir of DIRECTIONS) {
		const n = getNeighbor(startCol, startRow, dir);
		if (cells.has(cellKey(n.col, n.row))) frontier.push(n);
	}

	while (frontier.length > 0) {
		const idx = rng.nextInt(0, frontier.length);
		const cell = frontier.splice(idx, 1)[0];
		const ck = cellKey(cell.col, cell.row);
		if (visited.has(ck)) continue;

		// Link to a random visited neighbor
		const visitedNeighbors: { col: number; row: number }[] = [];
		for (const dir of DIRECTIONS) {
			const n = getNeighbor(cell.col, cell.row, dir);
			if (cells.has(cellKey(n.col, n.row)) && visited.has(cellKey(n.col, n.row)))
				visitedNeighbors.push(n);
		}
		const link = visitedNeighbors[rng.nextInt(0, visitedNeighbors.length)];
		hexLinkCells(cells, link.col, link.row, cell.col, cell.row);
		visited.add(ck);

		for (const dir of DIRECTIONS) {
			const n = getNeighbor(cell.col, cell.row, dir);
			const nk = cellKey(n.col, n.row);
			if (cells.has(nk) && !visited.has(nk)) frontier.push(n);
		}
	}

	const start = { col: startCol, row: startRow };
	const end = hexBfsExit(cells, start);
	cells.get(cellKey(start.col, start.row))!.value = 2;
	cells.get(cellKey(end.col, end.row))!.value = 3;
	return { shape: 'hexagonal', cells, cols, rows, start, end };
}

// ---------------------------------------------------------------------------
// Kruskal's on hexagonal grid
// ---------------------------------------------------------------------------
export function generateHexMazeKruskals(cols: number, rows: number, seed: number): HexMazeData {
	const rng = new SeededRandom(seed);
	const cells = initHexGrid(cols, rows);

	// Union-Find
	const parent = new Map<string, string>();
	const rank = new Map<string, number>();
	for (const k of cells.keys()) { parent.set(k, k); rank.set(k, 0); }

	function find(k: string): string {
		if (parent.get(k) !== k) parent.set(k, find(parent.get(k)!));
		return parent.get(k)!;
	}
	function union(a: string, b: string): boolean {
		const ra = find(a), rb = find(b);
		if (ra === rb) return false;
		const rankA = rank.get(ra)!, rankB = rank.get(rb)!;
		if (rankA < rankB) parent.set(ra, rb);
		else if (rankA > rankB) parent.set(rb, ra);
		else { parent.set(rb, ra); rank.set(ra, rankA + 1); }
		return true;
	}

	// Collect unique edges via string-sorted key pairs
	const edgeSet = new Set<string>();
	const edges: [number, number, number, number][] = [];
	for (let r = 0; r < rows; r++) {
		for (let c = 0; c < cols; c++) {
			for (const dir of DIRECTIONS) {
				const n = getNeighbor(c, r, dir);
				if (!cells.has(cellKey(n.col, n.row))) continue;
				const k1 = cellKey(c, r);
				const k2 = cellKey(n.col, n.row);
				const ek = k1 < k2 ? `${k1}|${k2}` : `${k2}|${k1}`;
				if (!edgeSet.has(ek)) {
					edgeSet.add(ek);
					edges.push([c, r, n.col, n.row]);
				}
			}
		}
	}
	rng.shuffle(edges);
	for (const [c1, r1, c2, r2] of edges) {
		if (union(cellKey(c1, r1), cellKey(c2, r2)))
			hexLinkCells(cells, c1, r1, c2, r2);
	}

	const startCol = rng.nextInt(0, cols);
	const startRow = rng.nextInt(0, rows);
	const start = { col: startCol, row: startRow };
	const end = hexBfsExit(cells, start);
	cells.get(cellKey(start.col, start.row))!.value = 2;
	cells.get(cellKey(end.col, end.row))!.value = 3;
	return { shape: 'hexagonal', cells, cols, rows, start, end };
}
