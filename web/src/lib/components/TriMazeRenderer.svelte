<!--
  TriMazeRenderer: SVG-based triangular maze rendering.
  Alternating up/down triangles with neon glow.
-->
<script lang="ts">
	import { getMazeThemePalette, type MazeVisualTheme } from '$lib/core/mazeVisualThemes';
	import type { TriCell, TriMazeData } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#34d399',
		visualTheme = 'neon' as MazeVisualTheme
	}: {
		maze: TriMazeData;
		playerPos: { col: number; row: number };
		wallColor?: string;
		visualTheme?: MazeVisualTheme;
	} = $props();

	const TRI_SIDE = 40; // side length
	const TRI_H = TRI_SIDE * Math.sqrt(3) / 2;
	const PADDING = 20;

	const palette = $derived(getMazeThemePalette(visualTheme, wallColor));

	// Triangle vertices
	function triVertices(col: number, row: number): { x: number; y: number }[] {
		const up = (col + row) % 2 === 0;
		// Each triangle occupies half a TRI_SIDE width
		const halfW = TRI_SIDE / 2;
		const baseX = PADDING + col * halfW;
		const baseY = PADDING + row * TRI_H;

		if (up) {
			return [
				{ x: baseX + halfW, y: baseY },           // top vertex
				{ x: baseX, y: baseY + TRI_H },           // bottom-left
				{ x: baseX + TRI_SIDE, y: baseY + TRI_H } // bottom-right
			];
		} else {
			return [
				{ x: baseX, y: baseY },                    // top-left
				{ x: baseX + TRI_SIDE, y: baseY },         // top-right
				{ x: baseX + halfW, y: baseY + TRI_H }     // bottom vertex
			];
		}
	}

	function triCenter(col: number, row: number): { x: number; y: number } {
		const verts = triVertices(col, row);
		return {
			x: (verts[0].x + verts[1].x + verts[2].x) / 3,
			y: (verts[0].y + verts[1].y + verts[2].y) / 3
		};
	}

	function triPoints(col: number, row: number): string {
		return triVertices(col, row).map(v => `${v.x},${v.y}`).join(' ');
	}

	// Wall segments for a triangle cell
	// Up-pointing: V0=top, V1=bottom-left, V2=bottom-right
	//   left wall = V0-V1, right wall = V0-V2, base(bottom) wall = V1-V2
	// Down-pointing: V0=top-left, V1=top-right, V2=bottom
	//   left wall = V0-V2, right wall = V1-V2, base(top) wall = V0-V1
	function wallLines(cell: TriCell): Array<{ x1: number; y1: number; x2: number; y2: number }> {
		const v = triVertices(cell.col, cell.row);
		const lines: Array<{ x1: number; y1: number; x2: number; y2: number }> = [];

		if (cell.pointsUp) {
			if (cell.wallLeft)  lines.push({ x1: v[0].x, y1: v[0].y, x2: v[1].x, y2: v[1].y });
			if (cell.wallRight) lines.push({ x1: v[0].x, y1: v[0].y, x2: v[2].x, y2: v[2].y });
			if (cell.wallBase)  lines.push({ x1: v[1].x, y1: v[1].y, x2: v[2].x, y2: v[2].y });
		} else {
			if (cell.wallLeft)  lines.push({ x1: v[0].x, y1: v[0].y, x2: v[2].x, y2: v[2].y });
			if (cell.wallRight) lines.push({ x1: v[1].x, y1: v[1].y, x2: v[2].x, y2: v[2].y });
			if (cell.wallBase)  lines.push({ x1: v[0].x, y1: v[0].y, x2: v[1].x, y2: v[1].y });
		}

		return lines;
	}

	const allCells = $derived(maze.cells.flat());

	const viewBoxWidth = $derived(maze.cols * (TRI_SIDE / 2) + TRI_SIDE / 2 + PADDING * 2);
	const viewBoxHeight = $derived(maze.rows * TRI_H + PADDING * 2);

	const playerXY = $derived(triCenter(playerPos.col, playerPos.row));
	const exitXY = $derived(triCenter(maze.end.col, maze.end.row));
</script>

<div class="tri-container" role="img" aria-label="Triangular maze">
	<svg
		class="tri-svg"
		viewBox={`0 0 ${viewBoxWidth} ${viewBoxHeight}`}
		preserveAspectRatio="xMidYMid meet"
	>
		<defs>
			<filter id="tglow-wall" x="-30%" y="-30%" width="160%" height="160%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="2" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<filter id="tglow-player" x="-60%" y="-60%" width="220%" height="220%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="3" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<radialGradient id="tgrad-player" cx="38%" cy="32%" r="65%">
				<stop offset="0%" stop-color="#ede9fe" />
				<stop offset="60%" stop-color="#a78bfa" />
				<stop offset="100%" stop-color="#5b21b6" />
			</radialGradient>
			<radialGradient id="tgrad-exit" cx="40%" cy="35%" r="65%">
				<stop offset="0%" stop-color="#d1fae5" />
				<stop offset="60%" stop-color="#34d399" />
				<stop offset="100%" stop-color="#065f46" />
			</radialGradient>
		</defs>

		<!-- Background -->
		<rect x="0" y="0" width={viewBoxWidth} height={viewBoxHeight} fill={palette.bgColor} rx="8" />

		<!-- Cell backgrounds -->
		{#each allCells as cell (`bg-${cell.col},${cell.row}`)}
			<polygon
				points={triPoints(cell.col, cell.row)}
				fill={cell.value === 2 ? palette.startCellColor :
					  cell.value === 3 ? palette.exitCellColor :
					  palette.cellBgColor}
				stroke={palette.cellStrokeColor}
				stroke-width="0.3"
			/>
		{/each}

		<!-- Exit marker -->
		<circle
			class="exit-dot"
			cx={exitXY.x}
			cy={exitXY.y}
			r={TRI_SIDE * 0.14}
			fill={palette.exitDotColor}
			filter={palette.exitGlow ? 'url(#tglow-player)' : undefined}
		/>

		<!-- Walls — glow -->
		{#if palette.wallGlowWidth > 0}
			<g stroke={palette.strokeColor} stroke-width={palette.wallGlowWidth} stroke-linecap="round" opacity="0.4" filter="url(#tglow-wall)">
				{#each allCells as cell (`wg-${cell.col},${cell.row}`)}
					{#each wallLines(cell) as seg}
						<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
					{/each}
				{/each}
			</g>
		{/if}

		<!-- Walls — solid -->
		<g stroke={palette.strokeColor} stroke-width={palette.wallWidth} stroke-linecap={palette.wallLineCap} stroke-linejoin={palette.wallLineJoin}>
			{#each allCells as cell (`ws-${cell.col},${cell.row}`)}
				{#each wallLines(cell) as seg}
					<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
				{/each}
			{/each}
		</g>

		<!-- Player -->
		{#if palette.playerGlow}
			<circle
				class="player-halo"
				cx={playerXY.x}
				cy={playerXY.y}
				r={TRI_SIDE * 0.22}
				fill={palette.playerHaloColor}
				filter="url(#tglow-player)"
			/>
		{/if}
		<circle
			cx={playerXY.x}
			cy={playerXY.y}
			r={TRI_SIDE * 0.16}
			fill={palette.playerColor}
			stroke={palette.playerStroke}
			stroke-width={palette.playerStrokeWidth}
		/>
	</svg>
</div>

<style>
	.tri-container {
		width: 100%;
		height: 100%;
		min-height: 160px;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: var(--radius-lg);
		overflow: hidden;
	}

	.tri-svg {
		display: block;
		width: 100%;
		height: 100%;
	}

	.exit-dot {
		transform-box: fill-box;
		transform-origin: center;
		animation: tri-exit-pulse 2.4s ease-in-out infinite;
	}

	@keyframes tri-exit-pulse {
		0%, 100% { transform: scale(0.88); opacity: 0.8; }
		50%       { transform: scale(1.12); opacity: 1; }
	}

	.player-halo {
		animation: tri-breathe 2s ease-in-out infinite;
	}

	@keyframes tri-breathe {
		0%, 100% { opacity: 0.5; }
		50%       { opacity: 1; }
	}
</style>
