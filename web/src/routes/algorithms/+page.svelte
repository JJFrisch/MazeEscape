<svelte:head>
	<title>Algorithm Encyclopedia – Maze Escape: Pathbound</title>
</svelte:head>

<script lang="ts">
	import { onMount } from 'svelte';
	import { generateMaze } from '$lib/core/maze';
	import { generateHexMaze, generateHexMazePrims, generateHexMazeKruskals } from '$lib/core/hexMaze';
	import { generateCircularMaze, generateCircularMazePrims } from '$lib/core/circularMaze';
	import { generateTriMaze, generateTriMazeHuntAndKill, generateTriMazeKruskals } from '$lib/core/triMaze';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import HexMazeRenderer from '$lib/components/HexMazeRenderer.svelte';
	import CircularMazeRenderer from '$lib/components/CircularMazeRenderer.svelte';
	import TriMazeRenderer from '$lib/components/TriMazeRenderer.svelte';
	import type { MazeData, HexMazeData, CircularMazeData, TriMazeData, MazeAlgorithm } from '$lib/core/types';
	import type { MazeVisualTheme } from '$lib/components/MazeRenderer.svelte';

	// ---------------------------------------------------------------------------
	// Types
	// ---------------------------------------------------------------------------

	type ShapeType = 'rectangular' | 'hexagonal' | 'circular' | 'triangular';

	interface AlgoEntry {
		id: string;
		mazeAlgo?: MazeAlgorithm;
		name: string;
		shape: ShapeType;
		inventor: string;
		inventorUrl: string | null;
		year: string;
		wikiUrl: string | null;
		description: string;
		strategies: string;
	}

	type Preview =
		| { ready: false }
		| { ready: true; type: 'rectangular'; maze: MazeData }
		| { ready: true; type: 'hexagonal'; maze: HexMazeData }
		| { ready: true; type: 'circular'; maze: CircularMazeData }
		| { ready: true; type: 'triangular'; maze: TriMazeData };

	// ---------------------------------------------------------------------------
	// Algorithm metadata
	// ---------------------------------------------------------------------------

	const ALGORITHMS: AlgoEntry[] = [
		// ── Rectangular ──────────────────────────────────────────────────────────
		{
			id: 'backtracking', mazeAlgo: 'backtracking',
			name: 'Recursive Backtracking',
			shape: 'rectangular',
			inventor: 'Depth-first search folklore',
			inventorUrl: null,
			year: 'pre-1970s',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Recursive_backtracker',
			description: `The recursive backtracker generates a maze using depth-first search, treating the grid as a graph where cells are vertices and walls are edges. Starting from a random cell, it marks the cell visited and repeatedly carves passages to random unvisited neighbors. When all neighbors are already visited, it backtracks to the previous cell and tries again. This continues until every cell is visited.

DFS is equivalent to computing a random spanning tree via a biased random walk — the depth-first bias means it follows one direction all the way to a dead end before exploring alternatives. The result is mazes with long, winding river-like corridors and relatively few branch points. Dead ends are infrequent and the longest solution path often winds through most of the maze.`,
			strategies: 'Follow corridors deeply before backtracking. Dead ends are rare and short. The single dominant path often curves continuously toward the exit — trace the longest uninterrupted corridor first.',
		},
		{
			id: 'huntAndKill', mazeAlgo: 'huntAndKill',
			name: 'Hunt and Kill',
			shape: 'rectangular',
			inventor: 'Walter D. Pullen (attributed)',
			inventorUrl: 'http://www.astrolog.org/labyrnth/algrithm.htm',
			year: '~2000',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Hunt-and-kill',
			description: `Hunt and Kill operates in two alternating phases. In the kill phase, the algorithm walks randomly to any unvisited neighbor and carves a passage. When blocked at a dead end with no unvisited neighbors, it switches to the hunt phase: a systematic row-by-row scan of the grid until it finds an unvisited cell adjacent to an already-visited cell, carves a passage to it, then resumes killing.

Structurally similar to recursive backtracking — long corridors with moderate branching — but without the recursion stack. The systematic hunt scan introduces a subtle row-bias: new branches of the maze tend to originate from the upper-left region, since the scan always starts from row 0.`,
			strategies: 'Trace the main corridors first. Hunt and Kill mazes have one or two dominant paths. Branch clusters created by the hunt phase concentrate in the upper portion of the grid.',
		},
		{
			id: 'prims', mazeAlgo: 'prims',
			name: "Prim's Algorithm",
			shape: 'rectangular',
			inventor: 'Robert C. Prim (MST); maze adaptation is folk',
			inventorUrl: null,
			year: '1957 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Prim%27s_algorithm",
			description: `Prim's algorithm builds a spanning tree by growing a frontier. Starting from a single cell, it collects all unvisited neighbors (the frontier) and repeatedly picks one at random, carving a passage from a random visited neighbor to it. Because the next cell is chosen at random from the entire frontier rather than continuing in one direction, the algorithm spreads outward like a growing organism.

Prim's produces mazes with lots of short dead ends, dense branching, and a "bushy" appearance compared to DFS. The short dead ends mean there are many false paths to explore, which makes the maze challenging despite its random structure. The solution path tends to be shorter on average than DFS.`,
			strategies: 'Many short dead ends — dead-end elimination works well. Identify the general direction of the exit by BFS intuition and treat dense branching zones as likely dead ends.',
		},
		{
			id: 'kruskals', mazeAlgo: 'kruskals',
			name: "Kruskal's Algorithm",
			shape: 'rectangular',
			inventor: 'Joseph B. Kruskal (MST); maze adaptation is folk',
			inventorUrl: null,
			year: '1956 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Kruskal%27s_algorithm",
			description: `Kruskal's algorithm treats the maze as a weighted graph where every wall between two cells is an edge. It shuffles all edges randomly, then iterates through them: a wall is removed if and only if the two cells it connects belong to different connected components — tracked efficiently with a union-find data structure. The result is a provably uniform random spanning tree: every possible perfect maze is equally likely with no directional bias.

Kruskal's looks visually similar to Aldous-Broder and Wilson's — even, balanced branching, consistent dead-end density throughout the grid, and no long dominant corridors.`,
			strategies: 'Dead ends are evenly distributed. No dominant corridor exists. Work systematically from the exit backward, eliminating dead ends as you encounter them.',
		},
		{
			id: 'growingTree_50_50', mazeAlgo: 'growingTree_50_50',
			name: 'Growing Tree (50 / 50)',
			shape: 'rectangular',
			inventor: 'Walter D. Pullen (attributed)',
			inventorUrl: 'http://www.astrolog.org/labyrnth/algrithm.htm',
			year: '~2000',
			wikiUrl: null,
			description: `The Growing Tree algorithm is a parameterized family that unifies Prim's and recursive backtracking. An active cell list is maintained. At each step, a cell is selected from the list: 50% of the time the newest (DFS-like) cell is chosen; 50% of the time a random cell from the entire list (Prim's-like). A random unvisited neighbor of the chosen cell is carved, or if none exist, the cell is removed from the list.

The 50/50 split produces a hybrid character: medium-length corridors punctuated by branching bursts. Neither the stark uniformity of pure DFS nor the extreme busyness of pure Prim's. This is widely considered the most "natural-looking" balance — visually varied mazes that are genuinely challenging to solve.`,
			strategies: 'Expect a mix of long corridors and branching clusters. No single approach dominates — alternate between corridor-following and elimination as the structure shifts.',
		},
		{
			id: 'growingTree_75_25', mazeAlgo: 'growingTree_75_25',
			name: 'Growing Tree (75 / 25)',
			shape: 'rectangular',
			inventor: 'Walter D. Pullen (attributed)',
			inventorUrl: 'http://www.astrolog.org/labyrnth/algrithm.htm',
			year: '~2000',
			wikiUrl: null,
			description: `A variant of the Growing Tree algorithm where the active list selection is 75% newest (DFS-like) and 25% random (Prim's-like). This weight strongly favors continuing in the current direction, producing mazes much closer in character to recursive backtracking — long, winding corridors with occasional bursts of branching injected by the 25% random selection.

The random 25% creates enough interruption that the corridors don't run as far as pure DFS, injecting short tangential dead ends. The result is a slightly more complex version of a DFS maze with irregular branch points.`,
			strategies: 'Trace the dominant corridor first. Branch points are less frequent and shorter than 50/50 but more frequent than pure DFS. Dead ends are mostly short.',
		},
		{
			id: 'growingTree_25_75', mazeAlgo: 'growingTree_25_75',
			name: 'Growing Tree (25 / 75)',
			shape: 'rectangular',
			inventor: 'Walter D. Pullen (attributed)',
			inventorUrl: 'http://www.astrolog.org/labyrnth/algrithm.htm',
			year: '~2000',
			wikiUrl: null,
			description: `A Growing Tree variant biased 75% toward random selection (Prim's-like) and only 25% toward the newest cell (DFS-like). The result is much closer in character to Prim's — lots of short dead ends and dense branching — with occasional longer corridors injected by the 25% newest selection.

This variant is noticeably harder than pure Prim's because the short DFS bursts create slightly longer branches that are harder to dismiss at a glance, forcing more careful exploration.`,
			strategies: 'Expect dense branching with many short dead ends. Systematic elimination works well. The occasional longer corridor is likely the solution path — follow it deeper before dismissing it.',
		},
		{
			id: 'growingTree_50_0', mazeAlgo: 'growingTree_50_0',
			name: 'Growing Tree (50 / 0 / 50)',
			shape: 'rectangular',
			inventor: 'Walter D. Pullen (attributed)',
			inventorUrl: 'http://www.astrolog.org/labyrnth/algrithm.htm',
			year: '~2000',
			wikiUrl: null,
			description: `A three-way Growing Tree variant: 50% random selection (Prim's), 0% newest (DFS), and the remaining 50% selects the oldest cell in the active list. The oldest-selection bias has an interesting effect — it causes the algorithm to backtrack very far, revisiting the earliest explored cells and eventually creating long east-west or north-south striping when space runs out.

The oldest-cell bias produces a more regular, almost tiled structure compared to other variants. The maze can appear to have a subtle grid-like organization overlaid on the random structure.`,
			strategies: 'Look for the structural regularity — slightly more tile-like regions exist. The solution path tends to wind across the grid somewhat systematically rather than in a single deep plunge.',
		},
		{
			id: 'wilsons', mazeAlgo: 'wilsons',
			name: "Wilson's Algorithm",
			shape: 'rectangular',
			inventor: 'David Bruce Wilson',
			inventorUrl: null,
			year: '1996',
			wikiUrl: 'https://en.wikipedia.org/wiki/Loop-erased_random_walk',
			description: `Wilson's algorithm builds a maze through loop-erased random walks. Starting from a random seed cell, it picks any unvisited cell and begins a random walk. Whenever the walk revisits a cell already on its own current path, the resulting loop is erased — only the path from the revisit point onward is kept. When the walk reaches a cell already incorporated into the maze, the surviving path is carved in and the process repeats until all cells are visited.

The loop-erasure step is the key insight: it ensures that every path added is a simple (non-self-intersecting) path. Because every possible perfect maze is produced with equal probability, Wilson's shares its statistical distribution with Kruskal's and Aldous-Broder. Visually: balanced, unbiased branching with even dead-end density and no directional grain.`,
			strategies: 'No structural shortcut. The balanced distribution means no region is simpler than another. Systematic dead-end elimination from the exit back is the most reliable approach.',
		},
		{
			id: 'aldousBroder', mazeAlgo: 'aldousBroder',
			name: 'Aldous-Broder',
			shape: 'rectangular',
			inventor: 'David Aldous and Andrei Broder (independently)',
			inventorUrl: null,
			year: '~1989-1990',
			wikiUrl: 'https://en.wikipedia.org/wiki/Random_walk',
			description: `Aldous-Broder is the simplest uniform spanning tree algorithm: start from a random cell, walk randomly to any neighbor, and if that neighbor is unvisited, carve a passage to it. Move to the neighbor regardless. Continue until all cells are visited.

The algorithm makes zero decisions about direction — it is purely random. This produces exactly the same statistical distribution as Wilson's and Kruskal's, but at the cost of being the slowest of the three: the walk can revisit cells many times before stumbling onto unvisited ones. Output is indistinguishable from Wilson's statistically: balanced, unbiased, evenly dense dead ends throughout.`,
			strategies: 'Statistically identical to Wilson\'s. No structural shortcut. Systematic dead-end elimination from the exit is the most reliable method.',
		},
		{
			id: 'binaryTree', mazeAlgo: 'binaryTree',
			name: 'Binary Tree',
			shape: 'rectangular',
			inventor: 'Folk algorithm; popularized by Jamis Buck',
			inventorUrl: 'http://weblog.jamisbuck.org/2011/2/1/maze-generation-binary-tree-algorithm',
			year: 'folklore',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Simple_algorithms',
			description: `Binary Tree is perhaps the simplest possible maze algorithm: for each cell, flip a coin and carve either north or east — ignoring whichever direction is out of bounds. No visited set, no stack, no bookkeeping beyond the grid. Each cell is processed exactly once in a single pass.

The simplicity imposes a severe structural bias: the top row becomes one unbroken east corridor and the right column becomes one unbroken north shaft. Every passage flows either north or east. The maze has a strong northeast diagonal grain — you can see it clearly in any rendered output. Solving is almost trivial once the bias is recognized.`,
			strategies: 'Move northeast at every opportunity — north whenever possible, east otherwise. The exit is placed by BFS so it may not always be in the top-right corner, but the solution will still trend northeast heavily.',
		},
		{
			id: 'sidewinder', mazeAlgo: 'sidewinder',
			name: 'Sidewinder',
			shape: 'rectangular',
			inventor: 'Folk algorithm; popularized by Jamis Buck',
			inventorUrl: 'http://weblog.jamisbuck.org/2011/2/3/maze-generation-sidewinder-algorithm',
			year: 'folklore',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Simple_algorithms',
			description: `Sidewinder processes the grid row by row, left to right. For each cell it accumulates a "run" of cells along the row. At each step: either extend the run eastward, or close the run by carving north from one randomly chosen cell within the run. The top row is always a full east corridor since no northward carving is possible there.

The run mechanism creates horizontal bands of cells that each share exactly one northern exit. Sidewinder mazes have a distinctive layered texture — passages form left-right clusters with vertical necks connecting layers. It retains a bias toward the top (the top row is always open) but eliminates the right-column shaft of Binary Tree.`,
			strategies: 'Each east-west cluster of cells has exactly one north-opening. Find the run-leader for each band — the single vertical passage — and trace the tree layer by layer from top to bottom.',
		},
		{
			id: 'ellers', mazeAlgo: 'ellers',
			name: "Eller's Algorithm",
			shape: 'rectangular',
			inventor: 'Marlin Eller',
			inventorUrl: null,
			year: '1982',
			wikiUrl: null,
			description: `Eller's algorithm generates a maze one row at a time using only O(width) memory, making it capable of generating mazes of arbitrary height in streaming fashion. Each cell in a row belongs to a numbered connected-component set. Adjacent cells from different sets are randomly merged (east wall removed). Then, for each set, at least one cell must receive a south-opening passage — others may be chosen randomly. Cells without south openings begin fresh sets in the next row. The final row merges all remaining different adjacent sets.

Despite working one row at a time, the output is statistically indistinguishable from a global MST approach: balanced branching, no bias, even dead-end density. Invented by Marlin Eller in 1982; unpublished until rediscovered in his archive decades later.`,
			strategies: 'Structurally identical to Kruskal\'s output. No dominant corridors. Balanced dead-end distribution. Eliminate dead ends methodically from the exit back.',
		},
		{
			id: 'recursiveDivision', mazeAlgo: 'recursiveDivision',
			name: 'Recursive Division',
			shape: 'rectangular',
			inventor: 'BSP concept by Bruce Naylor (1969); maze form is folk',
			inventorUrl: null,
			year: 'folklore',
			wikiUrl: 'https://en.wikipedia.org/wiki/Binary_space_partitioning',
			description: `Recursive Division is the only algorithm in this collection that adds walls rather than removing them. The grid starts as a completely open floor plan. A rectangular region is split by a wall running its full width or height, with exactly one gap left for passage. Both sub-regions are then recursively subdivided — tall regions are split horizontally, wide regions vertically, square regions randomly.

The result is a fractal maze with characteristically long straight walls, rectangular chambers nested within chambers, and a small number of mandatory bottleneck passages. Compared to carving algorithms, the structure is highly organized rather than organic. Visually you can see the room structure clearly.`,
			strategies: 'Identify the gap in each dividing wall. The entire maze is a binary tree of nested rectangular regions; for each region the solution must pass through its single gap. Work from the exit inward, tracing which region it is in and finding the gap leading outward at each level.',
		},
		{
			id: 'spiralBacktracker', mazeAlgo: 'spiralBacktracker',
			name: 'Spiral Backtracker',
			shape: 'rectangular',
			inventor: 'Jake Frischmann',
			inventorUrl: 'https://jakefrischman.me',
			year: '2024',
			wikiUrl: null,
			description: `The Spiral Backtracker is a directionally biased variant of recursive backtracking designed by Jake Frischmann. Standard DFS picks a completely random unvisited neighbor at each step. The Spiral Backtracker instead gives a 70% probability to continuing in the same direction as the previous move — causing the path to spiral outward in long looping corridors rather than wandering erratically. When the preferred direction is blocked or the 30% random chance triggers, a new direction is chosen and becomes the next preference. Backtracking resets the directional preference.

The result is a maze dominated by sweeping curved corridors that loop and wrap around themselves, creating a distinctive pinwheel appearance. The spiraling structure can be visually dramatic — long curves that nearly reach the exit before looping away.`,
			strategies: 'Do not be tricked by visual proximity to the exit. Spiraling corridors may curve past the exit multiple times. Follow spirals outward to their natural endpoints before backtracking.',
		},

		// ── Hexagonal ────────────────────────────────────────────────────────────
		{
			id: 'hexBacktracking',
			name: 'Hexagonal Backtracking',
			shape: 'hexagonal',
			inventor: 'Depth-first search adapted for hex; folk',
			inventorUrl: null,
			year: 'folklore',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Recursive_backtracker',
			description: `Recursive backtracking applied to a flat-top hexagonal grid. Each hex cell has 6 walls (N, NE, SE, S, SW, NW), so there are up to 6 neighbors to explore per cell instead of 4. The DFS-era character persists: long winding corridors, few branch points, low dead-end density. The hexagonal topology gives the corridors a curved, organic quality compared to rectangular DFS.

Navigation is harder than rectangular because "forward" can mean any of three hex directions (NW, N, or NE going upward). Spatial intuition requires recalibration — what feels like a diagonal may be a straight northwest passage.`,
			strategies: 'Trace the single dominant corridor. Hexagonal DFS mazes have one long winding path like rectangular DFS, but its curves are less predictable. Follow the passage that continues in roughly the same compass direction.',
		},
		{
			id: 'hexPrims',
			name: "Hexagonal Prim's",
			shape: 'hexagonal',
			inventor: "Robert C. Prim (MST); hex maze adaptation is folk",
			inventorUrl: null,
			year: '1957 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Prim%27s_algorithm",
			description: `Prim's random-frontier algorithm on a flat-top hexagonal grid. Each cell has 6 potential neighbors instead of 4, so the frontier grows faster and branches more aggressively. The result is even shorter dead ends and denser branching than rectangular Prim's.

Hexagonal Prim's combines the bushy dead-end density of Prim's with the inherent navigational difficulty of hex grids — confusing topology plus frequent branching. Each junction has up to 5 unblocked passages to consider, making it harder to dismiss dead ends at a glance.`,
			strategies: 'Treat the maze as a network of hubs rather than corridors. The high connectivity per cell makes pure dead-end elimination less reliable. Keyboard navigation maps approximate hex directions; think in terms of NW/N/NE for "forward."',
		},
		{
			id: 'hexKruskals',
			name: "Hexagonal Kruskal's",
			shape: 'hexagonal',
			inventor: "Joseph B. Kruskal (MST); hex maze adaptation is folk",
			inventorUrl: null,
			year: '1956 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Kruskal%27s_algorithm",
			description: `Kruskal's union-find spanning tree on a flat-top hexagonal grid. All possible edges between adjacent hex cells are shuffled randomly; walls are removed if connecting cells from different components. The result is a provably uniform random spanning tree on the hex topology — every possible perfect hexagonal maze is equally likely.

The visual output has the same even, balanced, unbiased character as rectangular Kruskal's, but the hex shape creates an organic flowing structure rather than a grid-aligned one. Six-directional navigation makes the uniformly distributed dead ends harder to process mentally.`,
			strategies: 'No structural shortcut. Systematic exploration — marking dead ends and backtracking — is the only reliable method. The lack of directional bias means no region of the maze is easier than another.',
		},

		// ── Circular ─────────────────────────────────────────────────────────────
		{
			id: 'circularBacktracking',
			name: 'Circular (Theta) Backtracking',
			shape: 'circular',
			inventor: 'Polar maze structure by Jamis Buck; DFS adaptation is folk',
			inventorUrl: 'http://weblog.jamisbuck.org/2015/10/31/mazes-in-ruby-for-the-kindle.html',
			year: '~2010s',
			wikiUrl: null,
			description: `Recursive backtracking on a polar (circular) grid. Concentric rings of cells radiate from a center cell; sectors double at certain ring boundaries to keep cells roughly square in shape. In the outward direction, a cell may open onto one or two outer-ring cells; inward, it always maps to one inner cell.

DFS on a circular grid creates long spiraling and radial passages rather than grid-aligned corridors. The maze always starts at the center and the exit is placed in the outermost ring. The circular topology removes the concept of "left/right" as compass directions — only in/out and clockwise/counter-clockwise have consistent meaning.`,
			strategies: 'Navigate: up=inward, down=outward, left=counter-clockwise, right=clockwise. The DFS character means one dominant winding path exists — trace it outward toward the exit ring.',
		},
		{
			id: 'circularPrims',
			name: "Circular (Theta) Prim's",
			shape: 'circular',
			inventor: "Robert C. Prim (MST); polar maze adaptation is folk",
			inventorUrl: null,
			year: '1957 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Prim%27s_algorithm",
			description: `Prim's random-frontier selection on a polar circular grid. The frontier grows outward in all directions — both radially and circumferentially — producing a more branched, even structure than DFS backtracking. Because outer rings have more sectors, the frontier expands rapidly at larger radii, creating a tendency for the tree to spread laterally in the outer rings while maintaining radial arms deeper inside.

Circular Prim's creates a visually striking spiral-and-spoke structure — radial passages from the center fan out and connect to circumferential clusters in the outer rings.`,
			strategies: 'More dead ends and branching than circular DFS. Work outward but eliminate circumferential dead ends ring by ring. The center of the maze is typically where all long paths converge.',
		},

		// ── Triangular ───────────────────────────────────────────────────────────
		{
			id: 'triBacktracking',
			name: 'Triangular (Delta) Backtracking',
			shape: 'triangular',
			inventor: 'Delta grid structure popularized by Jamis Buck; DFS is folk',
			inventorUrl: 'http://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm',
			year: 'folklore',
			wikiUrl: null,
			description: `Recursive backtracking on a triangular (delta) grid of alternating up-pointing and down-pointing triangles. Each triangle has 3 walls — left, right, and base — and at most 3 neighbors. The base wall's neighbor alternates: up-pointing triangles' base is below; down-pointing triangles' base is above. The alternating orientations create an unusually intricate visual texture.

DFS on the triangular grid produces long, winding paths as usual but the alternating up/down orientation means horizontal movement always alternates the orientation, which makes visual corridor tracking harder than rectangular grids.`,
			strategies: 'Left/right navigation is straightforward. Base passage direction depends on triangle orientation: for up-pointing triangles the base goes down; for down-pointing it goes up. The dominant long corridor from DFS exists — trace it.',
		},
		{
			id: 'triHuntAndKill',
			name: 'Triangular (Delta) Hunt and Kill',
			shape: 'triangular',
			inventor: 'Delta grid adaptation; folk',
			inventorUrl: null,
			year: 'folklore',
			wikiUrl: 'https://en.wikipedia.org/wiki/Maze_generation_algorithm#Hunt-and-kill',
			description: `Hunt and Kill adapted for the triangular delta grid. The kill phase walks randomly between adjacent triangles through any open wall. When stuck, the hunt phase row-scans for an unvisited triangle that shares a wall with a visited one, then links them and resumes killing.

The row-scan hunt creates the same upper-region bias as rectangular Hunt and Kill — new branches tend to originate in upper rows. On the triangular grid, this bias couples with the alternating triangle orientations to produce distinctive diagonal textures in the upper portion of the maze.`,
			strategies: 'Similar to rectangular Hunt and Kill. Branches concentrate in the upper rows. Trace horizontally within rows first, then find the vertical passage linking to the next row.',
		},
		{
			id: 'triKruskals',
			name: "Triangular (Delta) Kruskal's",
			shape: 'triangular',
			inventor: "Joseph B. Kruskal (MST); triangular adaptation is folk",
			inventorUrl: null,
			year: '1956 (MST)',
			wikiUrl: "https://en.wikipedia.org/wiki/Kruskal%27s_algorithm",
			description: `Kruskal's union-find spanning tree on the triangular delta grid. Each edge between adjacent triangles is shuffled randomly; walls are removed if connecting cells from different components. The result is a provably uniform random spanning tree — every possible triangular perfect maze is equally likely.

The triangular cell packing creates the most visually intricate maze in this collection. Because each cell has only 3 walls but the alternating orientations create complex visual geometry, the maze structure is harder to read than rectangular or hexagonal Kruskal's. The statistical balance means every region is equally challenging.`,
			strategies: 'The alternating up/down orientations mean each base passage changes vertical direction every step. Navigate carefully — what looks like a descending path can switch to ascending at any triangle. Systematic dead-end elimination is the most reliable approach.',
		},
	];

	// ---------------------------------------------------------------------------
	// State
	// ---------------------------------------------------------------------------

	let previews = $state<Preview[]>(ALGORITHMS.map(() => ({ ready: false })));
	let theme = $state<MazeVisualTheme>('neon');

	const SEED_BASE = 42;
	const PREVIEW_W = 8;
	const PREVIEW_H = 7;
	const HEX_COLS = 5;
	const HEX_ROWS = 4;
	const CIRC_RINGS = 4;
	const TRI_COLS = 8;
	const TRI_ROWS = 6;

	// ---------------------------------------------------------------------------
	// Generate all previews on mount
	// ---------------------------------------------------------------------------

	onMount(() => {
		const result: Preview[] = ALGORITHMS.map((algo, i) => {
			try {
				const seed = SEED_BASE + i * 7;
				switch (algo.shape) {
					case 'rectangular': {
						const maze = generateMaze(PREVIEW_W, PREVIEW_H, algo.mazeAlgo!, seed);
						return { ready: true, type: 'rectangular', maze };
					}
					case 'hexagonal': {
						const maze =
							algo.id === 'hexPrims' ? generateHexMazePrims(HEX_COLS, HEX_ROWS, seed) :
							algo.id === 'hexKruskals' ? generateHexMazeKruskals(HEX_COLS, HEX_ROWS, seed) :
							generateHexMaze(HEX_COLS, HEX_ROWS, seed);
						return { ready: true, type: 'hexagonal', maze };
					}
					case 'circular': {
						const maze =
							algo.id === 'circularPrims' ? generateCircularMazePrims(CIRC_RINGS, seed) :
							generateCircularMaze(CIRC_RINGS, seed);
						return { ready: true, type: 'circular', maze };
					}
					case 'triangular': {
						const maze =
							algo.id === 'triHuntAndKill' ? generateTriMazeHuntAndKill(TRI_COLS, TRI_ROWS, seed) :
							algo.id === 'triKruskals' ? generateTriMazeKruskals(TRI_COLS, TRI_ROWS, seed) :
							generateTriMaze(TRI_COLS, TRI_ROWS, seed);
						return { ready: true, type: 'triangular', maze };
					}
				}
			} catch (e) {
				console.error('Preview generation failed for', algo.id, e);
				return { ready: false };
			}
		});
		previews = result;
	});

	// ---------------------------------------------------------------------------
	// Grouping helper
	// ---------------------------------------------------------------------------

	function algosByShape(shape: ShapeType): { algo: AlgoEntry; index: number }[] {
		return ALGORITHMS
			.map((a, i) => ({ algo: a, index: i }))
			.filter(({ algo }) => algo.shape === shape);
	}

	const SHAPE_META: Record<ShapeType, { label: string; tagline: string; color: string }> = {
		rectangular: {
			label: 'Rectangular',
			tagline: 'Standard grid topology — the foundation of all maze generation theory.',
			color: '#38bdf8',
		},
		hexagonal: {
			label: 'Hexagonal',
			tagline: 'Flat-top hex grids with 6 directions per cell — disorients spatial intuition.',
			color: '#a78bfa',
		},
		circular: {
			label: 'Circular (Theta)',
			tagline: 'Concentric polar rings radiating from a central cell.',
			color: '#c084fc',
		},
		triangular: {
			label: 'Triangular (Delta)',
			tagline: 'Alternating up/down triangles — the most visually intricate topology available.',
			color: '#34d399',
		},
	};

	const SHAPE_ORDER: ShapeType[] = ['rectangular', 'hexagonal', 'circular', 'triangular'];
</script>

<!-- ═══════════════════════════════════════════════════════ Hero ══ -->
<div class="algo-page">
	<section class="algo-hero">
		<div class="algo-hero-badge">Nerd's Reference</div>
		<h1 class="algo-title">Maze Algorithm Encyclopedia</h1>
		<p class="algo-subtitle">
			All 20 generation algorithms used in Maze Escape: Pathbound — inventor credits, visual
			descriptions, solving strategies, and live seeded previews.
		</p>

		<div class="algo-theme-picker">
			<span class="theme-label">Rectangular preview theme:</span>
			{#each (['neon', 'classic', 'dotmatrix'] as MazeVisualTheme[]) as t}
				<button
					class="theme-btn"
					class:active={theme === t}
					onclick={() => (theme = t)}
				>{t}</button>
			{/each}
		</div>
	</section>

	<!-- ══════════════════════════════════════════════ Shape sections ══ -->
	{#each SHAPE_ORDER as shape}
		{@const meta = SHAPE_META[shape]}
		{@const entries = algosByShape(shape)}
		<section class="algo-section">
			<div class="section-header" style="--accent: {meta.color}">
				<h2 class="section-title">{meta.label}</h2>
				<p class="section-tagline">{meta.tagline}</p>
			</div>

			<div class="algo-grid">
				{#each entries as { algo, index }}
					{@const preview = previews[index]}
					<article class="algo-card">

						<!-- Preview -->
						<div class="algo-preview">
							{#if preview.ready}
								{#if preview.type === 'rectangular'}
									<MazeRenderer
										maze={preview.maze}
										playerPos={preview.maze.start}
										visualTheme={theme}
										wallColor=""
									/>
								{:else if preview.type === 'hexagonal'}
									<HexMazeRenderer
										maze={preview.maze}
										playerPos={preview.maze.start}
									/>
								{:else if preview.type === 'circular'}
									<CircularMazeRenderer
										maze={preview.maze}
										playerPos={preview.maze.start}
									/>
								{:else if preview.type === 'triangular'}
									<TriMazeRenderer
										maze={preview.maze}
										playerPos={preview.maze.start}
									/>
								{/if}
							{:else}
								<div class="preview-placeholder" aria-label="Generating preview…">
									<span class="placeholder-dot"></span>
									<span class="placeholder-dot"></span>
									<span class="placeholder-dot"></span>
								</div>
							{/if}
						</div>

						<!-- Metadata -->
						<div class="algo-body">
							<div class="algo-meta-row">
								<h3 class="algo-name">{algo.name}</h3>
								<span class="algo-shape-badge" style="--accent: {meta.color}">{meta.label}</span>
							</div>

							<div class="algo-credits">
								{#if algo.inventorUrl}
									<a href={algo.inventorUrl} target="_blank" rel="noopener noreferrer" class="credit-link">
										{algo.inventor}
									</a>
								{:else if algo.inventor === 'Jake Frischmann'}
									<a href="https://jakefrischman.me" target="_blank" rel="noopener noreferrer" class="credit-link credit-mine">
										{algo.inventor}
									</a>
								{:else}
									<span class="credit-text">{algo.inventor}</span>
								{/if}
								<span class="algo-year">· {algo.year}</span>
								{#if algo.wikiUrl}
									<a href={algo.wikiUrl} target="_blank" rel="noopener noreferrer" class="wiki-badge" aria-label="Wikipedia article">
										<svg width="12" height="12" viewBox="0 0 24 24" fill="none" aria-hidden="true">
											<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
											<path d="M12 6v6l4 2" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
										</svg>
										Wikipedia
									</a>
								{/if}
							</div>

							<p class="algo-description">{algo.description}</p>

							<div class="algo-strategies">
								<span class="strategies-label">Solving tip</span>
								<p class="strategies-text">{algo.strategies}</p>
							</div>
						</div>

					</article>
				{/each}
			</div>
		</section>
	{/each}

</div>

<style>
	.algo-page {
		max-width: 1200px;
		margin: 0 auto;
		padding: var(--space-6) var(--space-4) var(--space-12);
	}

	/* ── Hero ─────────────────────────────────────────── */
	.algo-hero {
		text-align: center;
		padding: var(--space-10) 0 var(--space-8);
	}
	.algo-hero-badge {
		display: inline-block;
		background: rgba(56, 189, 248, 0.15);
		color: var(--color-accent-primary);
		border: 1px solid rgba(56, 189, 248, 0.3);
		border-radius: 999px;
		padding: 4px 14px;
		font-size: 0.75rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		margin-bottom: var(--space-4);
	}
	.algo-title {
		font-size: clamp(1.8rem, 5vw, 3rem);
		font-weight: 800;
		color: var(--color-text-primary);
		margin: 0 0 var(--space-3);
		line-height: 1.15;
	}
	.algo-subtitle {
		font-size: 1.05rem;
		color: var(--color-text-secondary);
		max-width: 620px;
		margin: 0 auto var(--space-6);
		line-height: 1.7;
	}

	/* ── Theme picker ─────────────────────────────────── */
	.algo-theme-picker {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		flex-wrap: wrap;
	}
	.theme-label {
		font-size: 0.82rem;
		color: var(--color-text-muted, #64748b);
	}
	.theme-btn {
		background: rgba(255,255,255,0.05);
		border: 1px solid rgba(255,255,255,0.1);
		color: var(--color-text-secondary);
		border-radius: 6px;
		padding: 4px 12px;
		font-size: 0.8rem;
		cursor: pointer;
		transition: border-color 0.15s, color 0.15s, background 0.15s;
	}
	.theme-btn:hover {
		border-color: rgba(56,189,248,0.4);
		color: var(--color-accent-primary);
	}
	.theme-btn.active {
		background: rgba(56,189,248,0.15);
		border-color: rgba(56,189,248,0.5);
		color: var(--color-accent-primary);
	}

	/* ── Section ──────────────────────────────────────── */
	.algo-section {
		margin-bottom: var(--space-12);
	}
	.section-header {
		border-left: 3px solid var(--accent);
		padding-left: var(--space-4);
		margin-bottom: var(--space-6);
	}
	.section-title {
		font-size: 1.4rem;
		font-weight: 700;
		color: var(--color-text-primary);
		margin: 0 0 var(--space-1);
	}
	.section-tagline {
		font-size: 0.9rem;
		color: var(--color-text-secondary);
		margin: 0;
	}

	/* ── Card grid ────────────────────────────────────── */
	.algo-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: var(--space-6);
	}
	@media (min-width: 900px) {
		.algo-grid {
			grid-template-columns: 1fr 1fr;
		}
	}

	/* ── Card ─────────────────────────────────────────── */
	.algo-card {
		background: var(--color-surface-2, rgba(255,255,255,0.03));
		border: 1px solid rgba(255,255,255,0.07);
		border-radius: 12px;
		overflow: hidden;
		display: flex;
		flex-direction: column;
	}

	/* ── Preview ──────────────────────────────────────── */
	.algo-preview {
		background: #080e1e;
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 200px;
		overflow: hidden;
		padding: var(--space-2);
	}
	.algo-preview :global(svg) {
		max-width: 100%;
		height: auto;
		display: block;
	}
	.preview-placeholder {
		display: flex;
		gap: 6px;
		align-items: center;
	}
	.placeholder-dot {
		width: 8px;
		height: 8px;
		border-radius: 50%;
		background: rgba(56,189,248,0.4);
		animation: pulse 1.2s ease-in-out infinite;
	}
	.placeholder-dot:nth-child(2) { animation-delay: 0.2s; }
	.placeholder-dot:nth-child(3) { animation-delay: 0.4s; }
	@keyframes pulse {
		0%, 100% { opacity: 0.3; transform: scale(0.8); }
		50% { opacity: 1; transform: scale(1.1); }
	}

	/* ── Body ─────────────────────────────────────────── */
	.algo-body {
		padding: var(--space-5);
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
		flex: 1;
	}

	.algo-meta-row {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: var(--space-2);
		flex-wrap: wrap;
	}
	.algo-name {
		font-size: 1.1rem;
		font-weight: 700;
		color: var(--color-text-primary);
		margin: 0;
	}
	.algo-shape-badge {
		font-size: 0.72rem;
		color: var(--accent);
		border: 1px solid var(--accent);
		border-radius: 999px;
		padding: 2px 10px;
		white-space: nowrap;
		opacity: 0.85;
	}

	.algo-credits {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		flex-wrap: wrap;
		font-size: 0.82rem;
		color: var(--color-text-secondary);
	}
	.credit-link {
		color: var(--color-accent-primary);
		text-decoration: none;
	}
	.credit-link:hover { text-decoration: underline; }
	.credit-mine {
		color: var(--color-accent-gold, #fbbf24);
	}
	.credit-text { color: var(--color-text-secondary); }
	.algo-year { color: var(--color-text-muted, #64748b); }
	.wiki-badge {
		display: inline-flex;
		align-items: center;
		gap: 4px;
		color: var(--color-text-muted, #64748b);
		border: 1px solid rgba(255,255,255,0.1);
		border-radius: 999px;
		padding: 2px 8px;
		font-size: 0.72rem;
		text-decoration: none;
		transition: color 0.15s, border-color 0.15s;
	}
	.wiki-badge:hover {
		color: var(--color-text-primary);
		border-color: rgba(255,255,255,0.25);
	}

	.algo-description {
		font-size: 0.88rem;
		color: var(--color-text-secondary);
		line-height: 1.75;
		margin: 0;
		white-space: pre-line;
	}

	.algo-strategies {
		background: rgba(56,189,248,0.06);
		border: 1px solid rgba(56,189,248,0.14);
		border-radius: 8px;
		padding: var(--space-3);
		margin-top: auto;
	}
	.strategies-label {
		font-size: 0.7rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.07em;
		color: var(--color-accent-primary);
		display: block;
		margin-bottom: var(--space-1);
	}
	.strategies-text {
		font-size: 0.85rem;
		color: var(--color-text-secondary);
		line-height: 1.65;
		margin: 0;
	}
</style>
