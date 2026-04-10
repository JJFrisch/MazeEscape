<!--
  HexMazeRenderer: SVG-based hexagonal maze rendering.
  Flat-top hexagons with neon glow effects.
-->
<script lang="ts">
	import { getMazeThemePalette, type MazeVisualTheme } from '$lib/core/mazeVisualThemes';
	import type { HexCell, HexMazeData } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#38bdf8',
		visualTheme = 'neon' as MazeVisualTheme
	}: {
		maze: HexMazeData;
		playerPos: { col: number; row: number };
		wallColor?: string;
		visualTheme?: MazeVisualTheme;
	} = $props();

	// Hex geometry (flat-top)
	const HEX_SIZE = 28; // radius (center to vertex)
	const HEX_W = HEX_SIZE * 2;
	const HEX_H = Math.sqrt(3) * HEX_SIZE;
	const PADDING = 24;

	const palette = $derived(getMazeThemePalette(visualTheme, wallColor));

	// Convert hex grid coords to pixel center
	function hexCenter(col: number, row: number): { x: number; y: number } {
		const x = PADDING + col * (HEX_W * 0.75) + HEX_SIZE;
		const isOdd = col & 1;
		const y = PADDING + row * HEX_H + HEX_H / 2 + (isOdd ? HEX_H / 2 : 0);
		return { x, y };
	}

	// 6 vertices of a flat-top hex
	function hexPoints(cx: number, cy: number): string {
		const pts: string[] = [];
		for (let i = 0; i < 6; i++) {
			const angle = (Math.PI / 180) * (60 * i);
			pts.push(`${cx + HEX_SIZE * Math.cos(angle)},${cy + HEX_SIZE * Math.sin(angle)}`);
		}
		return pts.join(' ');
	}

	// Wall-edge mapping for flat-top hex:
	// Vertices in SVG (y-down): V0=right(0°), V1=lower-right(60°), V2=lower-left(120°),
	//   V3=left(180°), V4=upper-left(240°), V5=upper-right(300°)
	// Wall indices: [N, NE, SE, S, SW, NW]
	const WALL_EDGES: [number, number][] = [
		[4, 5], // N: V4 to V5 (top edge)
		[5, 0], // NE: V5 to V0 (upper-right edge)
		[0, 1], // SE: V0 to V1 (lower-right edge)
		[1, 2], // S: V1 to V2 (bottom edge)
		[2, 3], // SW: V2 to V3 (lower-left edge)
		[3, 4], // NW: V3 to V4 (upper-left edge)
	];

	function vertexPos(cx: number, cy: number, idx: number): { x: number; y: number } {
		const angle = (Math.PI / 180) * (60 * idx);
		return { x: cx + HEX_SIZE * Math.cos(angle), y: cy + HEX_SIZE * Math.sin(angle) };
	}

	function wallLines(cell: HexCell): Array<{ x1: number; y1: number; x2: number; y2: number }> {
		const { x: cx, y: cy } = hexCenter(cell.col, cell.row);
		const lines: Array<{ x1: number; y1: number; x2: number; y2: number }> = [];
		for (let i = 0; i < 6; i++) {
			if (cell.walls[i]) {
				const [v1, v2] = WALL_EDGES[i];
				const p1 = vertexPos(cx, cy, v1);
				const p2 = vertexPos(cx, cy, v2);
				lines.push({ x1: p1.x, y1: p1.y, x2: p2.x, y2: p2.y });
			}
		}
		return lines;
	}

	const allCells = $derived(Array.from(maze.cells.values()));

	const viewBoxWidth = $derived(maze.cols * (HEX_W * 0.75) + HEX_SIZE + PADDING * 2);
	const viewBoxHeight = $derived(maze.rows * HEX_H + HEX_H / 2 + PADDING * 2);

	const playerCenter = $derived(hexCenter(playerPos.col, playerPos.row));
	const exitCenter = $derived(hexCenter(maze.end.col, maze.end.row));
</script>

<div class="hex-container" role="img" aria-label="Hexagonal maze">
	<svg
		class="hex-svg"
		viewBox={`0 0 ${viewBoxWidth} ${viewBoxHeight}`}
		preserveAspectRatio="xMidYMid meet"
	>
		<defs>
			<filter id="hglow-wall" x="-30%" y="-30%" width="160%" height="160%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="2" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<filter id="hglow-player" x="-60%" y="-60%" width="220%" height="220%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="3" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<radialGradient id="hgrad-player" cx="38%" cy="32%" r="65%">
				<stop offset="0%" stop-color="#ede9fe" />
				<stop offset="60%" stop-color="#a78bfa" />
				<stop offset="100%" stop-color="#5b21b6" />
			</radialGradient>
			<radialGradient id="hgrad-exit" cx="40%" cy="35%" r="65%">
				<stop offset="0%" stop-color="#d1fae5" />
				<stop offset="60%" stop-color="#34d399" />
				<stop offset="100%" stop-color="#065f46" />
			</radialGradient>
		</defs>

		<!-- Background -->
		<rect x="0" y="0" width={viewBoxWidth} height={viewBoxHeight} fill={palette.bgColor} rx="8" />

		<!-- Hex cell backgrounds -->
		{#each allCells as cell (`bg-${cell.col},${cell.row}`)}
			{@const center = hexCenter(cell.col, cell.row)}
			<polygon
				points={hexPoints(center.x, center.y)}
				fill={cell.value === 2 ? palette.startCellColor :
					  cell.value === 3 ? palette.exitCellColor :
					  palette.cellBgColor}
				stroke={palette.cellStrokeColor}
				stroke-width="0.5"
			/>
		{/each}

		<!-- Exit marker -->
		<circle
			class="exit-dot"
			cx={exitCenter.x}
			cy={exitCenter.y}
			r={HEX_SIZE * 0.35}
			fill={palette.exitDotColor}
			filter={palette.exitGlow ? 'url(#hglow-player)' : undefined}
		/>

		<!-- Walls — glow layer -->
		{#if palette.wallGlowWidth > 0}
			<g stroke={palette.strokeColor} stroke-width={palette.wallGlowWidth} stroke-linecap="round" opacity="0.4" filter="url(#hglow-wall)">
				{#each allCells as cell (`wg-${cell.col},${cell.row}`)}
					{#each wallLines(cell) as seg}
						<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
					{/each}
				{/each}
			</g>
		{/if}

		<!-- Walls — solid layer -->
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
				cx={playerCenter.x}
				cy={playerCenter.y}
				r={HEX_SIZE * 0.55}
				fill={palette.playerHaloColor}
				filter="url(#hglow-player)"
			/>
		{/if}
		<circle
			cx={playerCenter.x}
			cy={playerCenter.y}
			r={HEX_SIZE * 0.4}
			fill={palette.playerColor}
			stroke={palette.playerStroke}
			stroke-width={palette.playerStrokeWidth}
		/>
	</svg>
</div>

<style>
	.hex-container {
		width: 100%;
		height: 100%;
		min-height: 160px;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: var(--radius-lg);
		overflow: hidden;
	}

	.hex-svg {
		display: block;
		width: 100%;
		height: 100%;
	}

	.exit-dot {
		transform-box: fill-box;
		transform-origin: center;
		animation: hex-exit-pulse 2.4s ease-in-out infinite;
	}

	@keyframes hex-exit-pulse {
		0%, 100% { transform: scale(0.88); opacity: 0.8; }
		50%       { transform: scale(1.12); opacity: 1; }
	}

	.player-halo {
		animation: hex-player-breathe 2s ease-in-out infinite;
	}

	@keyframes hex-player-breathe {
		0%, 100% { opacity: 0.5; }
		50%       { opacity: 1; }
	}
</style>
