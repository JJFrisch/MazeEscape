<!--
  MazeRenderer: SVG-based maze rendering with multiple visual themes.
  Supports: neon (default), classic, dotmatrix
-->
<script lang="ts">
	import type { MazeCell, MazeData, Position } from '$lib/core/types';

	export type MazeVisualTheme = 'neon' | 'classic' | 'dotmatrix';

	let {
		maze,
		playerPos,
		wallColor = '#38bdf8',
		hintPath = null,
		skinEmoji = '🟣',
		showVisited = false,
		visitedCells = new Set<string>(),
		visualTheme = 'neon' as MazeVisualTheme
	}: {
		maze: MazeData;
		playerPos: Position;
		wallColor?: string;
		hintPath?: Position[] | null;
		skinEmoji?: string;
		showVisited?: boolean;
		visitedCells?: Set<string>;
		visualTheme?: MazeVisualTheme;
	} = $props();

	// Internal coordinate space
	const CELL_SIZE = 44;
	const PADDING = 16;
	// Maximum rendered pixel size per cell — prevents tiny mazes from blowing up
	const MAX_CELL_PX = 80;

	const FALLBACK_WALL_COLOR = '#38bdf8';
	const strokeColor = $derived(
		visualTheme === 'classic' ? '#1a1a1a' :
		visualTheme === 'dotmatrix' ? '#ff3333' :
		(!wallColor || wallColor === '#000000' ? FALLBACK_WALL_COLOR : wallColor)
	);

	// Theme-dependent colors
	const bgColor = $derived(
		visualTheme === 'classic' ? '#ffffff' :
		visualTheme === 'dotmatrix' ? '#0a0a0a' :
		'#080e1e'
	);
	const cellBgColor = $derived(
		visualTheme === 'classic' ? '#f8fafc' :
		visualTheme === 'dotmatrix' ? '#111' :
		'rgba(255,255,255,0.015)'
	);
	const startCellColor = $derived(
		visualTheme === 'classic' ? 'rgba(99,102,241,0.12)' :
		visualTheme === 'dotmatrix' ? 'rgba(255,255,255,0.08)' :
		'rgba(109,40,217,0.18)'
	);
	const exitCellColor = $derived(
		visualTheme === 'classic' ? 'rgba(52,211,153,0.15)' :
		visualTheme === 'dotmatrix' ? 'rgba(52,211,153,0.12)' :
		'rgba(52,211,153,0.15)'
	);
	const visitedColor = $derived(
		visualTheme === 'classic' ? 'rgba(99,102,241,0.06)' :
		visualTheme === 'dotmatrix' ? 'rgba(255,80,80,0.06)' :
		'rgba(139,92,246,0.06)'
	);
	const wallWidth = $derived(visualTheme === 'classic' ? 3.5 : 2);
	const wallGlowWidth = $derived(visualTheme === 'classic' ? 0 : 2.5);
	const playerColor = $derived(
		visualTheme === 'classic' ? '#4f46e5' :
		visualTheme === 'dotmatrix' ? '#ff3333' :
		'url(#mgrad-player)'
	);
	const exitDotColor = $derived(
		visualTheme === 'classic' ? '#059669' :
		visualTheme === 'dotmatrix' ? '#22c55e' :
		'url(#mgrad-exit)'
	);

	const viewBoxWidth = $derived(maze.width * CELL_SIZE + PADDING * 2);
	const viewBoxHeight = $derived(maze.height * CELL_SIZE + PADDING * 2);

	// Cap the display size so cells never grow beyond MAX_CELL_PX
	const maxDisplayWidth = $derived(maze.width * MAX_CELL_PX + PADDING * 2);
	const maxDisplayHeight = $derived(maze.height * MAX_CELL_PX + PADDING * 2);

	const hintPathPoints = $derived(
		(hintPath ?? [])
			.map((p) => `${cx(p.x)},${cy(p.y)}`)
			.join(' ')
	);

	// Coordinate helpers
	function cl(x: number) { return PADDING + x * CELL_SIZE; }
	function ct(y: number) { return PADDING + y * CELL_SIZE; }
	function cx(x: number) { return cl(x) + CELL_SIZE / 2; }
	function cy(y: number) { return ct(y) + CELL_SIZE / 2; }

	function innerRect(x: number, y: number) {
		const inset = CELL_SIZE * 0.1;
		return {
			x: cl(x) + inset,
			y: ct(y) + inset,
			width: CELL_SIZE - inset * 2,
			height: CELL_SIZE - inset * 2,
			rx: CELL_SIZE * 0.14
		};
	}

	function wallSegments(cell: MazeCell) {
		const left = cl(cell.x);
		const top = ct(cell.y);
		const right = left + CELL_SIZE;
		const bottom = top + CELL_SIZE;
		const s: Array<{ x1: number; y1: number; x2: number; y2: number }> = [];

		if (cell.north) s.push({ x1: left, y1: top, x2: right, y2: top });
		if (cell.east) s.push({ x1: right, y1: top, x2: right, y2: bottom });
		if (cell.y === maze.height - 1) s.push({ x1: left, y1: bottom, x2: right, y2: bottom });
		if (cell.x === 0) s.push({ x1: left, y1: top, x2: left, y2: bottom });

		return s;
	}
</script>

<div
	class="maze-container"
	role="img"
	aria-label="Maze grid"
	style:max-width="{maxDisplayWidth}px"
	style:max-height="{maxDisplayHeight}px"
	style:background={bgColor}
>
	<svg
		class="maze-svg"
		viewBox={`0 0 ${viewBoxWidth} ${viewBoxHeight}`}
		preserveAspectRatio="xMidYMid meet"
	>
		<defs>
			<!-- Wall neon glow -->
			<filter id="mglow-wall" x="-30%" y="-30%" width="160%" height="160%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="2" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<!-- Player glow -->
			<filter id="mglow-player" x="-60%" y="-60%" width="220%" height="220%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="3.5" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<!-- Exit glow -->
			<filter id="mglow-exit" x="-60%" y="-60%" width="220%" height="220%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="4" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<!-- Player radial gradient -->
			<radialGradient id="mgrad-player" cx="38%" cy="32%" r="65%">
				<stop offset="0%" stop-color="#ede9fe" />
				<stop offset="60%" stop-color="#a78bfa" />
				<stop offset="100%" stop-color="#5b21b6" />
			</radialGradient>
			<!-- Exit radial gradient -->
			<radialGradient id="mgrad-exit" cx="40%" cy="35%" r="65%">
				<stop offset="0%" stop-color="#d1fae5" />
				<stop offset="60%" stop-color="#34d399" />
				<stop offset="100%" stop-color="#065f46" />
			</radialGradient>
		</defs>

		<!-- Background -->
		<rect x="0" y="0" width={viewBoxWidth} height={viewBoxHeight} fill={bgColor} />

		<!-- Subtle grid lines -->
		{#each maze.cells as row}
			{#each row as cell (`grid-${cell.x},${cell.y}`)}
				{#if visualTheme === 'dotmatrix'}
					<!-- Dot matrix: small dot at each cell center -->
					<circle
						cx={cx(cell.x)}
						cy={cy(cell.y)}
						r={CELL_SIZE * 0.06}
						fill="rgba(255,255,255,0.15)"
					/>
				{:else}
					<rect
						x={cl(cell.x) + 0.5}
						y={ct(cell.y) + 0.5}
						width={CELL_SIZE - 1}
						height={CELL_SIZE - 1}
						fill={cellBgColor}
					/>
				{/if}
			{/each}
		{/each}

		<!-- Cell highlights -->
		{#each maze.cells as row}
			{#each row as cell (`hi-${cell.x},${cell.y}`)}
				{#if cell.value === 2}
					{@const r = innerRect(cell.x, cell.y)}
					<rect {...r} fill={startCellColor} stroke={visualTheme === 'classic' ? 'rgba(99,102,241,0.3)' : 'rgba(139,92,246,0.3)'} stroke-width="1" />
				{:else if cell.value === 3}
					{@const r = innerRect(cell.x, cell.y)}
					<rect {...r} fill={exitCellColor} stroke={visualTheme === 'classic' ? 'rgba(52,211,153,0.4)' : 'rgba(52,211,153,0.35)'} stroke-width="1" class="exit-cell" />
				{:else if showVisited && visitedCells.has(`${cell.x},${cell.y}`)}
					{@const r = innerRect(cell.x, cell.y)}
					<rect {...r} fill={visitedColor} />
				{/if}
			{/each}
		{/each}

		<!-- Exit marker (pulsing) -->
		{#each maze.cells as row}
			{#each row as cell (`exit-${cell.x},${cell.y}`)}
				{#if cell.value === 3}
					<circle
						class="exit-dot"
						cx={cx(cell.x)}
						cy={cy(cell.y)}
						r={CELL_SIZE * 0.24}
						fill={exitDotColor}
						filter={visualTheme === 'neon' ? 'url(#mglow-exit)' : undefined}
					/>
				{/if}
			{/each}
		{/each}

		<!-- Hint path -->
		{#if hintPath && hintPath.length > 1}
			<polyline
				points={hintPathPoints}
				fill="none"
				stroke="rgba(251,191,36,0.55)"
				stroke-width={CELL_SIZE * 0.14}
				stroke-linecap="round"
				stroke-linejoin="round"
			/>
		{/if}

		<!-- Walls — glow layer (neon/dotmatrix only) -->
		{#if wallGlowWidth > 0}
			<g stroke={strokeColor} stroke-width={wallGlowWidth} stroke-linecap="round" stroke-linejoin="round" opacity="0.45" filter="url(#mglow-wall)">
				{#each maze.cells as row}
					{#each row as cell (`wg-${cell.x},${cell.y}`)}
						{#each wallSegments(cell) as seg (`${seg.x1}-${seg.y1}-${seg.x2}-${seg.y2}-g`)}
							<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
						{/each}
					{/each}
				{/each}
			</g>
		{/if}

		<!-- Walls — solid layer -->
		<g stroke={strokeColor} stroke-width={wallWidth} stroke-linecap={visualTheme === 'classic' ? 'square' : 'round'} stroke-linejoin={visualTheme === 'classic' ? 'miter' : 'round'}>
			{#each maze.cells as row}
				{#each row as cell (`ws-${cell.x},${cell.y}`)}
					{#each wallSegments(cell) as seg (`${seg.x1}-${seg.y1}-${seg.x2}-${seg.y2}-s`)}
						<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
					{/each}
				{/each}
			{/each}
		</g>

		<!-- Dot-matrix: wall intersection dots -->
		{#if visualTheme === 'dotmatrix'}
			{#each maze.cells as row}
				{#each row as cell (`dot-${cell.x},${cell.y}`)}
					{#each wallSegments(cell) as seg}
						<circle cx={seg.x1} cy={seg.y1} r={CELL_SIZE * 0.05} fill={strokeColor} />
						<circle cx={seg.x2} cy={seg.y2} r={CELL_SIZE * 0.05} fill={strokeColor} />
					{/each}
				{/each}
			{/each}
		{/if}

		<!-- Player glow halo (neon only) -->
		{#if visualTheme === 'neon'}
			<circle
				class="player-halo"
				cx={cx(playerPos.x)}
				cy={cy(playerPos.y)}
				r={CELL_SIZE * 0.42}
				fill="rgba(139,92,246,0.18)"
				filter="url(#mglow-player)"
			/>
		{/if}
		<!-- Player body -->
		<circle
			cx={cx(playerPos.x)}
			cy={cy(playerPos.y)}
			r={CELL_SIZE * 0.3}
			fill={playerColor}
			stroke={visualTheme === 'classic' ? '#312e81' : 'none'}
			stroke-width={visualTheme === 'classic' ? 1.5 : 0}
		/>
		<!-- Player emoji (neon only) -->
		{#if visualTheme === 'neon'}
			<text
				x={cx(playerPos.x)}
				y={cy(playerPos.y) + 1}
				text-anchor="middle"
				dominant-baseline="middle"
				font-size={CELL_SIZE * 0.42}
				font-family="Apple Color Emoji, Segoe UI Emoji, Noto Color Emoji, sans-serif"
			>{skinEmoji}</text>
		{/if}
	</svg>
</div>

<style>
	.maze-container {
		width: 100%;
		height: 100%;
		min-height: 160px;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: var(--radius-lg);
		overflow: hidden;
	}

	.maze-svg {
		display: block;
		width: 100%;
		height: 100%;
	}

	/* Exit cell background subtle pulse */
	.exit-cell {
		animation: exit-bg-pulse 2.4s ease-in-out infinite;
	}

	@keyframes exit-bg-pulse {
		0%, 100% { opacity: 0.7; }
		50%       { opacity: 1; }
	}

	/* Exit dot scale+opacity pulse */
	.exit-dot {
		transform-box: fill-box;
		transform-origin: center;
		animation: exit-dot-pulse 2.4s ease-in-out infinite;
	}

	@keyframes exit-dot-pulse {
		0%, 100% { transform: scale(0.88); opacity: 0.8; }
		50%       { transform: scale(1.12); opacity: 1; }
	}

	/* Player halo breathe */
	.player-halo {
		animation: player-breathe 2s ease-in-out infinite;
	}

	@keyframes player-breathe {
		0%, 100% { opacity: 0.5; }
		50%       { opacity: 1; }
	}
</style>
