<!--
  MazeRenderer: SVG-based maze rendering.
  Uses declarative markup instead of imperative canvas drawing so the maze is
  visible reliably on first render across browsers and deploy targets.
-->
<script lang="ts">
	import type { MazeCell, MazeData, Position } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#38bdf8',
		hintPath = null,
		skinEmoji = '🟣',
		showVisited = false,
		visitedCells = new Set<string>()
	}: {
		maze: MazeData;
		playerPos: Position;
		wallColor?: string;
		hintPath?: Position[] | null;
		skinEmoji?: string;
		showVisited?: boolean;
		visitedCells?: Set<string>;
	} = $props();

	const CELL_SIZE = 32;
	const PADDING = 12;
	const FALLBACK_WALL_COLOR = '#38bdf8';

	const strokeColor = $derived(wallColor === '#000000' ? FALLBACK_WALL_COLOR : (wallColor || FALLBACK_WALL_COLOR));
	const viewBoxWidth = $derived(maze.width * CELL_SIZE + PADDING * 2);
	const viewBoxHeight = $derived(maze.height * CELL_SIZE + PADDING * 2);
	const hintPathPoints = $derived(
		(hintPath ?? [])
			.map((point) => `${cellCenterX(point.x)},${cellCenterY(point.y)}`)
			.join(' ')
	);

	function cellLeft(x: number): number {
		return PADDING + x * CELL_SIZE;
	}

	function cellTop(y: number): number {
		return PADDING + y * CELL_SIZE;
	}

	function cellCenterX(x: number): number {
		return cellLeft(x) + CELL_SIZE / 2;
	}

	function cellCenterY(y: number): number {
		return cellTop(y) + CELL_SIZE / 2;
	}

	function innerRect(x: number, y: number) {
		const inset = CELL_SIZE * 0.12;
		return {
			x: cellLeft(x) + inset,
			y: cellTop(y) + inset,
			width: CELL_SIZE - inset * 2,
			height: CELL_SIZE - inset * 2,
			rx: CELL_SIZE * 0.08
		};
	}

	function wallSegments(cell: MazeCell) {
		const left = cellLeft(cell.x);
		const top = cellTop(cell.y);
		const right = left + CELL_SIZE;
		const bottom = top + CELL_SIZE;
		const segments: Array<{ x1: number; y1: number; x2: number; y2: number }> = [];

		if (cell.north) {
			segments.push({ x1: left, y1: top, x2: right, y2: top });
		}

		if (cell.east) {
			segments.push({ x1: right, y1: top, x2: right, y2: bottom });
		}

		if (cell.y === maze.height - 1) {
			segments.push({ x1: left, y1: bottom, x2: right, y2: bottom });
		}

		if (cell.x === 0) {
			segments.push({ x1: left, y1: top, x2: left, y2: bottom });
		}

		return segments;
	}
</script>

<div class="maze-container" role="img" aria-label="Maze grid">
	<svg
		class="maze-svg"
		viewBox={`0 0 ${viewBoxWidth} ${viewBoxHeight}`}
		preserveAspectRatio="xMidYMid meet"
	>
		<rect x="0" y="0" width={viewBoxWidth} height={viewBoxHeight} fill="#0f172a" />

		{#each maze.cells as row}
			{#each row as cell (`${cell.x},${cell.y}`)}
				{#if cell.value === 2}
					{@const rect = innerRect(cell.x, cell.y)}
					<rect {...rect} fill="rgba(99, 102, 241, 0.22)" />
				{:else if cell.value === 3}
					{@const rect = innerRect(cell.x, cell.y)}
					<rect {...rect} fill="rgba(52, 211, 153, 0.25)" />
					<circle
						cx={cellCenterX(cell.x)}
						cy={cellCenterY(cell.y)}
						r={CELL_SIZE * 0.14}
						fill="#34d399"
					/>
				{:else if showVisited && visitedCells.has(`${cell.x},${cell.y}`)}
					{@const rect = innerRect(cell.x, cell.y)}
					<rect {...rect} fill="rgba(167, 139, 250, 0.08)" />
				{/if}
			{/each}
		{/each}

		{#if hintPath && hintPath.length > 1}
			<polyline
				points={hintPathPoints}
				fill="none"
				stroke="rgba(251, 191, 36, 0.55)"
				stroke-width={CELL_SIZE * 0.16}
				stroke-linecap="round"
				stroke-linejoin="round"
			/>
		{/if}

		<g stroke={strokeColor} stroke-width="3" stroke-linecap="round">
			{#each maze.cells as row}
				{#each row as cell (`wall-${cell.x},${cell.y}`)}
					{#each wallSegments(cell) as segment (`${segment.x1}-${segment.y1}-${segment.x2}-${segment.y2}`)}
						<line
							x1={segment.x1}
							y1={segment.y1}
							x2={segment.x2}
							y2={segment.y2}
						/>
					{/each}
				{/each}
			{/each}
		</g>

		<circle
			cx={cellCenterX(playerPos.x)}
			cy={cellCenterY(playerPos.y)}
			r={CELL_SIZE * 0.28}
			fill="#a78bfa"
			fill-opacity="0.95"
		/>
		<text
			x={cellCenterX(playerPos.x)}
			y={cellCenterY(playerPos.y) + 1}
			text-anchor="middle"
			dominant-baseline="middle"
			font-size={CELL_SIZE * 0.44}
			font-family="Apple Color Emoji, Segoe UI Emoji, sans-serif"
		>
			{skinEmoji}
		</text>
	</svg>
</div>

<style>
	.maze-container {
		width: 100%;
		height: 100%;
		min-height: 200px;
		border-radius: var(--radius-lg);
		overflow: hidden;
		background: #0f172a;
	}

	.maze-svg {
		display: block;
		width: 100%;
		height: 100%;
	}
</style>
