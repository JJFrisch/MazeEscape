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
// Additional generation algorithms (7 new)
// ---------------------------------------------------------------------------

function generateWilsons(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	visited.add(key(startPos.x, startPos.y));

	// Track unvisited as a list for O(1) random pick; remove lazily via the set
	const unvisited: Position[] = [];
	for (let y = 0; y < height; y++)
		for (let x = 0; x < width; x++)
			if (!(x === startPos.x && y === startPos.y)) unvisited.push({ x, y });

	while (unvisited.length > 0) {
		// Pick a random unvisited cell (skip already-visited ones)
		let walkStart: Position | null = null;
		while (unvisited.length > 0) {
			const idx = rng.nextInt(0, unvisited.length);
			const candidate = unvisited[idx];
			if (!visited.has(key(candidate.x, candidate.y))) {
				walkStart = candidate;
				break;
			}
			// lazy removal of visited cells
			unvisited[idx] = unvisited[unvisited.length - 1];
			unvisited.pop();
		}
		if (!walkStart) break;

		// Loop-erased random walk from walkStart until hitting visited
		const pathOrder: Position[] = [{ ...walkStart }];
		const pathIndex = new Map<CellKey, number>();
		pathIndex.set(key(walkStart.x, walkStart.y), 0);
		let current = { ...walkStart };

		while (!visited.has(key(current.x, current.y))) {
			const dirs = [
				[0, -1], [0, 1], [1, 0], [-1, 0]
			].filter(([dx, dy]) => inBounds(current.x + dx, current.y + dy, width, height));
			const [dx, dy] = dirs[rng.nextInt(0, dirs.length)];
			const nx = current.x + dx;
			const ny = current.y + dy;
			const nk = key(nx, ny);

			if (pathIndex.has(nk)) {
				// Erase loop: keep path up to (and including) re-visited cell
				const loopStart = pathIndex.get(nk)! + 1;
				for (let i = loopStart; i < pathOrder.length; i++)
					pathIndex.delete(key(pathOrder[i].x, pathOrder[i].y));
				pathOrder.splice(loopStart);
			} else {
				pathIndex.set(nk, pathOrder.length);
				pathOrder.push({ x: nx, y: ny });
			}
			current = { x: nx, y: ny };
		}

		// Carve path (all except last cell which is already in visited)
		for (let i = 0; i < pathOrder.length - 1; i++) {
			linkCells(cells, pathOrder[i], pathOrder[i + 1]);
			visited.add(key(pathOrder[i].x, pathOrder[i].y));
		}
	}
}

function generateAldousBroder(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	visited.add(key(startPos.x, startPos.y));
	let current = { ...startPos };
	const totalCells = width * height;

	while (visited.size < totalCells) {
		const dirs = [
			[0, -1], [0, 1], [1, 0], [-1, 0]
		].filter(([dx, dy]) => inBounds(current.x + dx, current.y + dy, width, height));
		const [dx, dy] = dirs[rng.nextInt(0, dirs.length)];
		const next = { x: current.x + dx, y: current.y + dy };
		const nk = key(next.x, next.y);

		if (!visited.has(nk)) {
			linkCells(cells, current, next);
			visited.add(nk);
		}
		current = next;
	}
}

function generateBinaryTree(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	_startPos: Position
): void {
	for (let y = 0; y < height; y++) {
		for (let x = 0; x < width; x++) {
			const options: Position[] = [];
			if (y > 0) options.push({ x, y: y - 1 }); // north
			if (x < width - 1) options.push({ x: x + 1, y }); // east
			if (options.length > 0)
				linkCells(cells, { x, y }, options[rng.nextInt(0, options.length)]);
		}
	}
}

function generateSidewinder(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	_startPos: Position
): void {
	// Top row: open all east walls
	for (let x = 0; x < width - 1; x++) linkCells(cells, { x, y: 0 }, { x: x + 1, y: 0 });

	for (let y = 1; y < height; y++) {
		let run: Position[] = [];
		for (let x = 0; x < width; x++) {
			run.push({ x, y });
			const atEast = x === width - 1;
			const closeRun = atEast || rng.nextInt(0, 2) === 0;

			if (closeRun) {
				const northCell = run[rng.nextInt(0, run.length)];
				linkCells(cells, northCell, { x: northCell.x, y: northCell.y - 1 });
				run = [];
			} else {
				linkCells(cells, { x, y }, { x: x + 1, y });
			}
		}
	}
}

function generateEllers(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	_startPos: Position
): void {
	let nextSetId = 0;
	let rowSets: number[] = Array.from({ length: width }, () => nextSetId++);

	for (let y = 0; y < height; y++) {
		const isLastRow = y === height - 1;

		// Step 1: merge adjacent cells from different sets (always on last row)
		for (let x = 0; x < width - 1; x++) {
			if (rowSets[x] !== rowSets[x + 1] && (isLastRow || rng.nextInt(0, 2) === 0)) {
				const mergeFrom = rowSets[x + 1];
				const mergeTo = rowSets[x];
				for (let c = 0; c < width; c++)
					if (rowSets[c] === mergeFrom) rowSets[c] = mergeTo;
				linkCells(cells, { x, y }, { x: x + 1, y });
			}
		}

		if (!isLastRow) {
			// Step 2: for each set ensure at least 1 south connection
			const setColumns = new Map<number, number[]>();
			for (let x = 0; x < width; x++) {
				if (!setColumns.has(rowSets[x])) setColumns.set(rowSets[x], []);
				setColumns.get(rowSets[x])!.push(x);
			}

			const nextRowSets: number[] = new Array(width).fill(-1);

			for (const [setId, cols] of setColumns) {
				const shuffled = [...cols];
				rng.shuffle(shuffled);
				let connectedAny = false;

				for (let i = 0; i < shuffled.length; i++) {
					const x = shuffled[i];
					const isLastInSet = i === shuffled.length - 1;
					const shouldConnect = (!connectedAny && isLastInSet) || rng.nextInt(0, 2) === 0;
					if (shouldConnect) {
						linkCells(cells, { x, y }, { x, y: y + 1 });
						nextRowSets[x] = setId;
						connectedAny = true;
					}
				}
			}

			// Cells without south connection get fresh set IDs
			for (let x = 0; x < width; x++)
				if (nextRowSets[x] === -1) nextRowSets[x] = nextSetId++;

			rowSets = nextRowSets;
		}
	}
}

function generateRecursiveDivision(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	_startPos: Position
): void {
	// Open all interior walls first
	for (let y = 0; y < height; y++)
		for (let x = 0; x < width; x++) {
			if (x < width - 1) cells[y][x].east = false;
			if (y > 0) cells[y][x].north = false;
		}

	function divide(x0: number, x1: number, y0: number, y1: number): void {
		const w = x1 - x0 + 1;
		const h = y1 - y0 + 1;
		if (w < 2 || h < 2) return;

		let horizontal: boolean;
		if (h > w) horizontal = true;
		else if (w > h) horizontal = false;
		else horizontal = rng.nextInt(0, 2) === 0;

		if (horizontal) {
			const wallY = rng.nextInt(y0, y1); // y0 <= wallY < y1
			const gapX = rng.nextInt(x0, x1 + 1);
			for (let x = x0; x <= x1; x++)
				if (x !== gapX) cells[wallY + 1][x].north = true;
			divide(x0, x1, y0, wallY);
			divide(x0, x1, wallY + 1, y1);
		} else {
			const wallX = rng.nextInt(x0, x1); // x0 <= wallX < x1
			const gapY = rng.nextInt(y0, y1 + 1);
			for (let y = y0; y <= y1; y++)
				if (y !== gapY) cells[y][wallX].east = true;
			divide(x0, wallX, y0, y1);
			divide(wallX + 1, x1, y0, y1);
		}
	}

	divide(0, width - 1, 0, height - 1);
}

function generateSpiralBacktracker(
	cells: MazeCell[][],
	width: number,
	height: number,
	rng: SeededRandom,
	startPos: Position
): void {
	const visited = new Set<CellKey>();
	visited.add(key(startPos.x, startPos.y));
	const stack: Position[] = [{ ...startPos }];
	let lastDx = 0;
	let lastDy = 0;
	const SPIRAL_WEIGHT = 70;

	while (stack.length > 0) {
		const current = stack[stack.length - 1];
		const neighbours = adjacentUnvisited(current.x, current.y, visited, width, height);

		if (neighbours.length === 0) {
			stack.pop();
			lastDx = 0;
			lastDy = 0;
			continue;
		}

		let next: Position;
		if (lastDx !== 0 || lastDy !== 0) {
			const px = current.x + lastDx;
			const py = current.y + lastDy;
			const pk = key(px, py);
			if (inBounds(px, py, width, height) && !visited.has(pk) && rng.nextInt(0, 100) < SPIRAL_WEIGHT)
				next = { x: px, y: py };
			else
				next = neighbours[rng.nextInt(0, neighbours.length)];
		} else {
			next = neighbours[rng.nextInt(0, neighbours.length)];
		}

		lastDx = next.x - current.x;
		lastDy = next.y - current.y;
		linkCells(cells, current, next);
		visited.add(key(next.x, next.y));
		stack.push(next);
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
		generateGrowingTree(cells, w, h, rng, start, 50, 0),
	wilsons: generateWilsons,
	aldousBroder: generateAldousBroder,
	binaryTree: generateBinaryTree,
	sidewinder: generateSidewinder,
	ellers: generateEllers,
	recursiveDivision: generateRecursiveDivision,
	spiralBacktracker: generateSpiralBacktracker
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
