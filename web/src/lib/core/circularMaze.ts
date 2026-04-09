/**
 * Circular (polar / theta) maze generator.
 * Concentric rings with sectors that double at certain ring boundaries.
 * Ring 0 is the center (1 cell). Outer rings have more sectors.
 */

import type { RingCell, CircularMazeData } from './types';
import { SeededRandom } from './random';

function cellKey(ring: number, sector: number): string {
	return `${ring},${sector}`;
}

function sectorsForRing(ring: number): number {
	if (ring === 0) return 1;
	// Double sectors at rings 1, 3, 7, 15... (roughly every power of 2)
	let sectors = 6;
	let r = 1;
	while (r <= ring) {
		if (r === 1 || r === 3 || r === 7 || r === 15) {
			sectors *= 2;
		}
		r++;
	}
	return Math.min(sectors, 48); // cap at 48 sectors
}

export function generateCircularMaze(numRings: number, seed: number): CircularMazeData {
	const rng = new SeededRandom(seed);
	const rings: RingCell[][] = [];

	// Initialize rings
	for (let r = 0; r < numRings; r++) {
		const numSectors = sectorsForRing(r);
		const ring: RingCell[] = [];
		for (let s = 0; s < numSectors; s++) {
			ring.push({
				ring: r,
				sector: s,
				value: 0,
				wallCW: true,
				wallOut: true
			});
		}
		rings.push(ring);
	}

	// Get all neighbors for a cell
	function getNeighbors(r: number, s: number): { ring: number; sector: number; type: 'cw' | 'ccw' | 'in' | 'out' }[] {
		const neighbors: { ring: number; sector: number; type: 'cw' | 'ccw' | 'in' | 'out' }[] = [];
		const numSectors = rings[r].length;

		// Clockwise neighbor (same ring)
		neighbors.push({ ring: r, sector: (s + 1) % numSectors, type: 'cw' });
		// Counter-clockwise neighbor (same ring)
		neighbors.push({ ring: r, sector: (s - 1 + numSectors) % numSectors, type: 'ccw' });

		// Inward neighbor
		if (r > 0) {
			const innerSectors = rings[r - 1].length;
			const mappedSector = Math.floor(s * innerSectors / numSectors);
			neighbors.push({ ring: r - 1, sector: mappedSector, type: 'in' });
		}

		// Outward neighbors (there may be multiple if outer ring has more sectors)
		if (r < numRings - 1) {
			const outerSectors = rings[r + 1].length;
			const startSector = Math.floor(s * outerSectors / numSectors);
			const endSector = Math.floor((s + 1) * outerSectors / numSectors);
			for (let os = startSector; os < endSector; os++) {
				neighbors.push({ ring: r + 1, sector: os, type: 'out' });
			}
			// If start == end, still include the one sector
			if (startSector === endSector) {
				neighbors.push({ ring: r + 1, sector: startSector, type: 'out' });
			}
		}

		return neighbors;
	}

	// Remove wall between two cells
	function linkCells(r1: number, s1: number, r2: number, s2: number) {
		if (r1 === r2) {
			// Same ring — clockwise wall
			// The wall is on the cell with the smaller sector (mod), specifically
			// the CW wall of cell (r, min_sector)
			if ((s1 + 1) % rings[r1].length === s2) {
				rings[r1][s1].wallCW = false;
			} else if ((s2 + 1) % rings[r1].length === s1) {
				rings[r1][s2].wallCW = false;
			}
		} else {
			// Different rings — the inner cell's outward wall
			const inner = r1 < r2 ? { r: r1, s: s1 } : { r: r2, s: s2 };
			rings[inner.r][inner.s].wallOut = false;
		}
	}

	// Recursive backtracking on the ring structure
	const visited = new Set<string>();
	const stack: { ring: number; sector: number }[] = [];

	// Start from center
	visited.add(cellKey(0, 0));
	stack.push({ ring: 0, sector: 0 });

	while (stack.length > 0) {
		const { ring: cr, sector: cs } = stack[stack.length - 1];
		const neighbors = getNeighbors(cr, cs);
		const unvisited = neighbors.filter(
			(n) => !visited.has(cellKey(n.ring, n.sector))
		);

		if (unvisited.length === 0) {
			stack.pop();
			continue;
		}

		const chosen = unvisited[rng.nextInt(0, unvisited.length)];
		linkCells(cr, cs, chosen.ring, chosen.sector);
		visited.add(cellKey(chosen.ring, chosen.sector));
		stack.push({ ring: chosen.ring, sector: chosen.sector });
	}

	// BFS for exit — furthest from center
	const bfsQueue = [{ ring: 0, sector: 0 }];
	const bfsVisited = new Set<string>([cellKey(0, 0)]);
	let furthest = { ring: 0, sector: 0 };

	while (bfsQueue.length > 0) {
		const curr = bfsQueue.shift()!;
		furthest = curr;

		for (const n of getNeighbors(curr.ring, curr.sector)) {
			const nk = cellKey(n.ring, n.sector);
			if (bfsVisited.has(nk)) continue;

			// Check if there's a passage
			let hasPassage = false;
			if (curr.ring === n.ring) {
				// Same ring — check CW wall
				const s1 = Math.min(curr.sector, n.sector);
				const s2 = Math.max(curr.sector, n.sector);
				if ((s1 + 1) % rings[curr.ring].length === s2) {
					hasPassage = !rings[curr.ring][s1].wallCW;
				} else if ((s2 + 1) % rings[curr.ring].length === s1) {
					hasPassage = !rings[curr.ring][s2].wallCW;
				}
			} else {
				// Different rings
				const innerR = Math.min(curr.ring, n.ring);
				const innerS = curr.ring < n.ring ? curr.sector : n.sector;
				hasPassage = !rings[innerR][innerS].wallOut;
			}

			if (hasPassage) {
				bfsVisited.add(nk);
				bfsQueue.push(n);
			}
		}
	}

	const start = { ring: 0, sector: 0 };
	const end = furthest;
	rings[start.ring][start.sector].value = 2;
	rings[end.ring][end.sector].value = 3;

	return { shape: 'circular', rings, numRings, start, end };
}

export function canMoveCircular(
	data: CircularMazeData,
	ring: number,
	sector: number,
	direction: 'cw' | 'ccw' | 'in' | 'out'
): { canMove: boolean; ring: number; sector: number } {
	const numSectors = data.rings[ring].length;

	switch (direction) {
		case 'cw': {
			const nextS = (sector + 1) % numSectors;
			const passable = !data.rings[ring][sector].wallCW;
			return { canMove: passable, ring, sector: nextS };
		}
		case 'ccw': {
			const prevS = (sector - 1 + numSectors) % numSectors;
			const passable = !data.rings[ring][prevS].wallCW;
			return { canMove: passable, ring, sector: prevS };
		}
		case 'out': {
			if (ring >= data.numRings - 1) return { canMove: false, ring, sector };
			const passable = !data.rings[ring][sector].wallOut;
			if (!passable) return { canMove: false, ring, sector };
			const outerSectors = data.rings[ring + 1].length;
			const mappedSector = Math.floor(sector * outerSectors / numSectors);
			return { canMove: true, ring: ring + 1, sector: mappedSector };
		}
		case 'in': {
			if (ring <= 0) return { canMove: false, ring, sector };
			const innerSectors = data.rings[ring - 1].length;
			const mappedSector = Math.floor(sector * innerSectors / numSectors);
			const passable = !data.rings[ring - 1][mappedSector].wallOut;
			return { canMove: passable, ring: ring - 1, sector: mappedSector };
		}
	}
}

/** Map arrow keys to circular directions based on sector angle */
export function keyToCircularDir(
	sector: number,
	numSectors: number,
	key: 'up' | 'down' | 'left' | 'right'
): ('cw' | 'ccw' | 'in' | 'out')[] {
	// Player's angular position determines what "up", "left", etc. mean
	// Simplification: up=inward, down=outward, left=ccw, right=cw
	switch (key) {
		case 'up':    return ['in'];
		case 'down':  return ['out'];
		case 'left':  return ['ccw'];
		case 'right': return ['cw'];
	}
}

// ---------------------------------------------------------------------------
// Prim's on circular (polar) grid
// ---------------------------------------------------------------------------
export function generateCircularMazePrims(numRings: number, seed: number): CircularMazeData {
	const rng = new SeededRandom(seed);
	const rings: RingCell[][] = [];

	for (let r = 0; r < numRings; r++) {
		const numSectors = sectorsForRing(r);
		const ring: RingCell[] = [];
		for (let s = 0; s < numSectors; s++)
			ring.push({ ring: r, sector: s, value: 0, wallCW: true, wallOut: true });
		rings.push(ring);
	}

	function getNeighbors(r: number, s: number): { ring: number; sector: number; type: 'cw' | 'ccw' | 'in' | 'out' }[] {
		const neighbors: { ring: number; sector: number; type: 'cw' | 'ccw' | 'in' | 'out' }[] = [];
		const numSectors = rings[r].length;
		neighbors.push({ ring: r, sector: (s + 1) % numSectors, type: 'cw' });
		neighbors.push({ ring: r, sector: (s - 1 + numSectors) % numSectors, type: 'ccw' });
		if (r > 0) {
			const innerSectors = rings[r - 1].length;
			neighbors.push({ ring: r - 1, sector: Math.floor(s * innerSectors / numSectors), type: 'in' });
		}
		if (r < numRings - 1) {
			const outerSectors = rings[r + 1].length;
			const startSector = Math.floor(s * outerSectors / numSectors);
			const endSector = Math.floor((s + 1) * outerSectors / numSectors);
			for (let os = startSector; os < endSector; os++)
				neighbors.push({ ring: r + 1, sector: os, type: 'out' });
			if (startSector === endSector)
				neighbors.push({ ring: r + 1, sector: startSector, type: 'out' });
		}
		return neighbors;
	}

	function linkCells(r1: number, s1: number, r2: number, s2: number): void {
		if (r1 === r2) {
			if ((s1 + 1) % rings[r1].length === s2) rings[r1][s1].wallCW = false;
			else if ((s2 + 1) % rings[r1].length === s1) rings[r1][s2].wallCW = false;
		} else {
			const inner = r1 < r2 ? { r: r1, s: s1 } : { r: r2, s: s2 };
			rings[inner.r][inner.s].wallOut = false;
		}
	}

	const ck = (r: number, s: number) => `${r},${s}`;
	const visited = new Set<string>([ck(0, 0)]);

	type FrontierCell = { ring: number; sector: number };
	const frontier: FrontierCell[] = [];
	for (const n of getNeighbors(0, 0)) frontier.push({ ring: n.ring, sector: n.sector });

	while (frontier.length > 0) {
		const idx = rng.nextInt(0, frontier.length);
		const cell = frontier.splice(idx, 1)[0];
		if (visited.has(ck(cell.ring, cell.sector))) continue;

		// Link to a random visited neighbor
		const visitedNeighbors = getNeighbors(cell.ring, cell.sector)
			.filter(n => visited.has(ck(n.ring, n.sector)));
		if (visitedNeighbors.length === 0) continue;
		const link = visitedNeighbors[rng.nextInt(0, visitedNeighbors.length)];
		linkCells(link.ring, link.sector, cell.ring, cell.sector);
		visited.add(ck(cell.ring, cell.sector));

		for (const n of getNeighbors(cell.ring, cell.sector))
			if (!visited.has(ck(n.ring, n.sector)))
				frontier.push({ ring: n.ring, sector: n.sector });
	}

	// BFS for exit from center
	const bfsQueue = [{ ring: 0, sector: 0 }];
	const bfsVisited = new Set<string>([ck(0, 0)]);
	let furthest = { ring: 0, sector: 0 };
	while (bfsQueue.length > 0) {
		const curr = bfsQueue.shift()!;
		furthest = curr;
		for (const n of getNeighbors(curr.ring, curr.sector)) {
			if (bfsVisited.has(ck(n.ring, n.sector))) continue;
			let hasPassage = false;
			if (curr.ring === n.ring) {
				const s1 = curr.sector, s2 = n.sector;
				if ((s1 + 1) % rings[curr.ring].length === s2) hasPassage = !rings[curr.ring][s1].wallCW;
				else if ((s2 + 1) % rings[curr.ring].length === s1) hasPassage = !rings[curr.ring][s2].wallCW;
			} else {
				const innerR = Math.min(curr.ring, n.ring);
				const innerS = curr.ring < n.ring ? curr.sector : n.sector;
				hasPassage = !rings[innerR][innerS].wallOut;
			}
			if (hasPassage) { bfsVisited.add(ck(n.ring, n.sector)); bfsQueue.push(n); }
		}
	}

	const start = { ring: 0, sector: 0 };
	const end = furthest;
	rings[start.ring][start.sector].value = 2;
	rings[end.ring][end.sector].value = 3;
	return { shape: 'circular', rings, numRings, start, end };
}
