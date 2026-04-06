/**
 * Maze generation engine.
 * Ported from C# MazeEscape.Core/Models/Maze.cs
 *
 * Supports 8 generation algorithms:
 * - backtracking (recursive DFS)
 * - huntAndKill
 * - prims
 * - kruskals
 * - growingTree variants (50/50, 75/25, 25/75, 50/0)
 *
 * After generation, OptimizeMaze() uses BFS to find the cell furthest from
 * the start and designates it as the exit, maximizing path length.
 */

import type { MazeCell, MazeData, MazeAlgorithm, Position } from './types';
import { SeededRandom } from './random';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function initializeGrid(width: number, height: number): MazeCell[][] {
	const cells: MazeCell[][] = [];
	for (let y = 0; y < height; y++) {
		const row: MazeCell[] = [];
		for (let x = 0; x < width; x++) {
			row.push({ x, y, value: 0, east: true, north: true });
		}
		cells.push(row);
	}
	return cells;
}

function inBounds(x: number, y: number, width: number, height: number): boolean {
	return x >= 0 && x < width && y >= 0 && y < height;
}

type CellKey = string; // "x,y"
function key(x: number, y: number): CellKey {
	return `${x},${y}`;
}

function adjacentUnvisited(
	x: number,
	y: number,
	visited: Set<CellKey>,
	width: number,
	height: number
): Position[] {
	const neighbours: Position[] = [];
	const dirs = [
		[0, -1],
		[0, 1],
		[1, 0],
		[-1, 0]
	];
	for (const [dx, dy] of dirs) {
		const nx = x + dx;
		const ny = y + dy;
		if (inBounds(nx, ny, width, height) && !visited.has(key(nx, ny))) {
			neighbours.push({ x: nx, y: ny });
		}
	}
	return neighbours;
}

function adjacentVisited(
	x: number,
	y: number,
	visited: Set<CellKey>,
	width: number,
	height: number
): Position[] {
	const neighbours: Position[] = [];
	const dirs = [
		[0, -1],
		[0, 1],
		[1, 0],
		[-1, 0]
	];
	for (const [dx, dy] of dirs) {
		const nx = x + dx;
		const ny = y + dy;
		if (inBounds(nx, ny, width, height) && visited.has(key(nx, ny))) {
			neighbours.push({ x: nx, y: ny });
		}
	}
	return neighbours;
}

/** Remove the wall between two adjacent cells. */
function linkCells(cells: MazeCell[][], a: Position, b: Position): void {
	if (a.x === b.x) {
		// Vertical neighbours – remove the North wall of the lower cell
		const lower = Math.max(a.y, b.y);
		cells[lower][a.x].north = false;
	} else if (a.y === b.y) {
		// Horizontal neighbours – remove the East wall of the left cell
		const left = Math.min(a.x, b.x);
		cells[a.y][left].east = false;
	}
}

// ---------------------------------------------------------------------------
// Generation algorithms
// ---------------------------------------------------------------------------

function generateBacktracking(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	visited.add(key(startPos.x, startPos.y));

	function recurse(cx: number, cy: number): void {
		const neighbours = adjacentUnvisited(cx, cy, visited, width, height);
		rng.shuffle(neighbours);

		for (const n of neighbours) {
			if (visited.has(key(n.x, n.y))) continue;
			visited.add(key(n.x, n.y));
			linkCells(cells, { x: cx, y: cy }, n);
			recurse(n.x, n.y);
		}
	}

	recurse(startPos.x, startPos.y);
}

function generateHuntAndKill(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	let current = { ...startPos };
	visited.add(key(current.x, current.y));
	const totalCells = width * height;

	while (visited.size < totalCells) {
		const moves = adjacentUnvisited(current.x, current.y, visited, width, height);

		if (moves.length > 0) {
			// Kill phase
			const next = moves[rng.nextInt(0, moves.length)];
			linkCells(cells, current, next);
			visited.add(key(next.x, next.y));
			current = next;
		} else {
			// Hunt phase
			let found = false;
			for (let y = 0; y < height && !found; y++) {
				for (let x = 0; x < width && !found; x++) {
					if (visited.has(key(x, y))) continue;
					const adj = adjacentVisited(x, y, visited, width, height);
					if (adj.length === 0) continue;

					const neighbor = adj[rng.nextInt(0, adj.length)];
					linkCells(cells, neighbor, { x, y });
					visited.add(key(x, y));
					current = { x, y };
					found = true;
				}
			}
			if (!found) break;
		}
	}
}

function generatePrims(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	const frontier: Position[] = [];

	visited.add(key(startPos.x, startPos.y));

	for (const n of adjacentUnvisited(startPos.x, startPos.y, visited, width, height)) {
		frontier.push(n);
	}

	while (frontier.length > 0) {
		const idx = rng.nextInt(0, frontier.length);
		const cell = frontier[idx];
		frontier.splice(idx, 1);

		if (visited.has(key(cell.x, cell.y))) continue;

		const visitedNeighbours = adjacentVisited(cell.x, cell.y, visited, width, height);
		if (visitedNeighbours.length === 0) continue;

		const link = visitedNeighbours[rng.nextInt(0, visitedNeighbours.length)];
		linkCells(cells, link, cell);
		visited.add(key(cell.x, cell.y));

		for (const n of adjacentUnvisited(cell.x, cell.y, visited, width, height)) {
			if (!frontier.some((f) => f.x === n.x && f.y === n.y)) {
				frontier.push(n);
			}
		}
	}
}

function generateKruskals(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom
): void {
	// Union-Find
	const parent = new Map<CellKey, CellKey>();
	const rank = new Map<CellKey, number>();

	function find(k: CellKey): CellKey {
		let root = k;
		while (parent.get(root) !== root) root = parent.get(root)!;
		let cur = k;
		while (cur !== root) {
			const next = parent.get(cur)!;
			parent.set(cur, root);
			cur = next;
		}
		return root;
	}

	function union(a: CellKey, b: CellKey): boolean {
		const ra = find(a);
		const rb = find(b);
		if (ra === rb) return false;
		const rankA = rank.get(ra) ?? 0;
		const rankB = rank.get(rb) ?? 0;
		if (rankA < rankB) parent.set(ra, rb);
		else if (rankA > rankB) parent.set(rb, ra);
		else {
			parent.set(rb, ra);
			rank.set(ra, rankA + 1);
		}
		return true;
	}

	for (let y = 0; y < height; y++) {
		for (let x = 0; x < width; x++) {
			const k = key(x, y);
			parent.set(k, k);
			rank.set(k, 0);
		}
	}

	// Collect all possible edges
	const edges: [Position, Position][] = [];
	for (let y = 0; y < height; y++) {
		for (let x = 0; x < width; x++) {
			if (x + 1 < width) edges.push([{ x, y }, { x: x + 1, y }]);
			if (y + 1 < height) edges.push([{ x, y }, { x, y: y + 1 }]);
		}
	}

	rng.shuffle(edges);

	for (const [a, b] of edges) {
		if (union(key(a.x, a.y), key(b.x, b.y))) {
			linkCells(cells, a, b);
		}
	}
}

function generateGrowingTree(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position,
	newestWeight: number,
	randomWeight: number
): void {
	const visited = new Set<CellKey>();
	const active: Position[] = [];

	visited.add(key(startPos.x, startPos.y));
	active.push({ ...startPos });

	while (active.length > 0) {
		// Pick cell based on weights
		let idx: number;
		const roll = rng.next() * (newestWeight + randomWeight);
		if (roll < newestWeight) {
			idx = active.length - 1; // newest
		} else {
			idx = rng.nextInt(0, active.length); // random
		}

		const cell = active[idx];
		const neighbours = adjacentUnvisited(cell.x, cell.y, visited, width, height);

		if (neighbours.length === 0) {
			active.splice(idx, 1);
			continue;
		}

		const next = neighbours[rng.nextInt(0, neighbours.length)];
		linkCells(cells, cell, next);
		visited.add(key(next.x, next.y));
		active.push(next);
	}
}

// ---------------------------------------------------------------------------
// Post-generation optimization (BFS for longest path)
// ---------------------------------------------------------------------------

function optimizeMaze(
	cells: MazeCell[][],
	width: number,
	height: number,
	start: Position
): { end: Position; path: Position[]; pathLength: number } {
	const queue: Position[] = [start];
	const cameFrom = new Map<CellKey, CellKey | null>();
	const pathTo = new Map<CellKey, Position[]>();

	cameFrom.set(key(start.x, start.y), null);
	pathTo.set(key(start.x, start.y), [start]);

	while (queue.length > 0) {
		const current = queue.shift()!;
		for (const next of getAccessibleNeighbours(cells, current, width, height)) {
			const nk = key(next.x, next.y);
			if (cameFrom.has(nk)) continue;
			const ck = key(current.x, current.y);
			pathTo.set(nk, [...(pathTo.get(ck) ?? []), next]);
			queue.push(next);
			cameFrom.set(nk, ck);
		}
	}

	// Find furthest cell
	let maxLen = 0;
	let endPos = start;
	let bestPath: Position[] = [start];

	for (const [k, path] of pathTo) {
		if (path.length > maxLen) {
			maxLen = path.length;
			bestPath = path;
			const [xStr, yStr] = k.split(',');
			endPos = { x: parseInt(xStr), y: parseInt(yStr) };
		}
	}

	return { end: endPos, path: bestPath, pathLength: maxLen };
}

// ---------------------------------------------------------------------------
// Accessible neighbours (respecting walls)
// ---------------------------------------------------------------------------

export function getAccessibleNeighbours(
	cells: MazeCell[][],
	pos: Position,
	width: number,
	height: number
): Position[] {
	const result: Position[] = [];
	const { x, y } = pos;

	// Up: blocked if current cell has north wall
	if (y > 0 && !cells[y][x].north) result.push({ x, y: y - 1 });
	// Down: blocked if cell below has north wall
	if (y < height - 1 && !cells[y + 1][x].north) result.push({ x, y: y + 1 });
	// Right: blocked if current cell has east wall
	if (x < width - 1 && !cells[y][x].east) result.push({ x: x + 1, y });
	// Left: blocked if cell to the left has east wall
	if (x > 0 && !cells[y][x - 1].east) result.push({ x: x - 1, y });

	return result;
}

// ---------------------------------------------------------------------------
// Find path from current position to end (for Hint powerup)
// ---------------------------------------------------------------------------

export function findPathToEnd(
	cells: MazeCell[][],
	from: Position,
	end: Position,
	width: number,
	height: number
): Position[] {
	const visited = new Set<CellKey>();
	visited.add(key(from.x, from.y));

	function dfs(pos: Position): Position[] | null {
		if (pos.x === end.x && pos.y === end.y) return [pos];

		for (const n of getAccessibleNeighbours(cells, pos, width, height)) {
			const nk = key(n.x, n.y);
			if (visited.has(nk)) continue;
			visited.add(nk);
			const path = dfs(n);
			if (path) {
				path.unshift(pos);
				return path;
			}
		}
		return null;
	}

	return dfs(from) ?? [];
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

const algorithmMap: Record<
	MazeAlgorithm,
	(
		cells: MazeCell[][],
		w: number,
		h: number,
		rng: SeededRandom,
		start: Position
	) => void
> = {
	backtracking: generateBacktracking,
	huntAndKill: generateHuntAndKill,
	prims: generatePrims,
	kruskals: (cells, w, h, rng, _start) => generateKruskals(cells, w, h, rng),
	growingTree_50_50: (cells, w, h, rng, start) =>
		generateGrowingTree(cells, w, h, rng, start, 50, 50),
	growingTree_75_25: (cells, w, h, rng, start) =>
		generateGrowingTree(cells, w, h, rng, start, 75, 25),
	growingTree_25_75: (cells, w, h, rng, start) =>
		generateGrowingTree(cells, w, h, rng, start, 25, 75),
	growingTree_50_0: (cells, w, h, rng, start) =>
		generateGrowingTree(cells, w, h, rng, start, 50, 0)
};

/**
 * Generate a complete maze with optimal exit placement.
 *
 * @param width  Number of columns
 * @param height Number of rows
 * @param algorithm Generation algorithm to use
 * @param seed  Integer seed for RNG (deterministic)
 */
export function generateMaze(
	width: number,
	height: number,
	algorithm: MazeAlgorithm,
	seed: number
): MazeData {
	const rng = new SeededRandom(seed);
	const cells = initializeGrid(width, height);

	// Random starting cell
	const start: Position = {
		x: rng.nextInt(0, width),
		y: rng.nextInt(0, height)
	};
	cells[start.y][start.x].value = 2;

	// Generate passages
	const gen = algorithmMap[algorithm];
	gen(cells, width, height, rng, start);

	// Find optimal exit
	const { end, path, pathLength } = optimizeMaze(cells, width, height, start);
	cells[end.y][end.x].value = 3;

	return { cells, width, height, start, end, path, pathLength };
}

// ---------------------------------------------------------------------------
// Movement
// ---------------------------------------------------------------------------

export function canMove(
	cells: MazeCell[][],
	pos: Position,
	direction: 'up' | 'down' | 'left' | 'right',
	width: number,
	height: number
): boolean {
	const { x, y } = pos;
	switch (direction) {
		case 'up':
			return y > 0 && !cells[y][x].north;
		case 'down':
			return y < height - 1 && !cells[y + 1][x].north;
		case 'right':
			return x < width - 1 && !cells[y][x].east;
		case 'left':
			return x > 0 && !cells[y][x - 1].east;
	}
}

export function applyMove(
	pos: Position,
	direction: 'up' | 'down' | 'left' | 'right'
): Position {
	switch (direction) {
		case 'up':
			return { x: pos.x, y: pos.y - 1 };
		case 'down':
			return { x: pos.x, y: pos.y + 1 };
		case 'right':
			return { x: pos.x + 1, y: pos.y };
		case 'left':
			return { x: pos.x - 1, y: pos.y };
	}
}
