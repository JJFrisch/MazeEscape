<!--
  HexMazeRenderer: SVG-based hexagonal maze rendering.
  Flat-top hexagons with neon glow effects.
-->
<script lang="ts">
	import type { HexCell, HexMazeData } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#38bdf8',
		accentColor = '#38bdf8'
	}: {
		maze: HexMazeData;
		playerPos: { col: number; row: number };
		wallColor?: string;
		accentColor?: string;
	} = $props();

	// Hex geometry (flat-top)
	const HEX_SIZE = 28; // radius (center to vertex)
	const HEX_W = HEX_SIZE * 2;
	const HEX_H = Math.sqrt(3) * HEX_SIZE;
	const PADDING = 24;

	const FALLBACK = '#38bdf8';
	const stroke = $derived(!wallColor || wallColor === '#000000' ? FALLBACK : wallColor);

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

	// Wall segments — each wall is an edge of the hex
	// Flat-top: vertex 0 is rightmost, going CCW: 0=right, 1=top-right, 2=top-left, 3=left, 4=bottom-left, 5=bottom-right
	// Our wall indices: NE=0, E=1, SE=2, SW=3, W=4, NW=5
	// Mapping wall index to which hex edge to draw:
	// NE(0) = edge between vertex 1 and 2 (top-right to top-left... no)
	// Let me re-map for flat-top:
	// Vertex angles: 0°, 60°, 120°, 180°, 240°, 300°
	// V0 = (1,0), V1 = (0.5, √3/2), V2 = (-0.5, √3/2), V3 = (-1,0), V4 = (-0.5,-√3/2), V5 = (0.5,-√3/2)
	// So: V0=right, V1=upper-right, V2=upper-left, V3=left, V4=lower-left, V5=lower-right
	// Wall NE = edge from V0 to V1 (right to upper-right)
	// Wall NW = edge from V1 to V2 (upper-right to upper-left)
	// Wall W  = edge from V2 to V3 (upper-left to left)
	// Wall SW = edge from V3 to V4 (left to lower-left)
	// Wall SE = edge from V4 to V5 (lower-left to lower-right)  -- wait this doesn't look right

	// Actually for our coordinate system:
	// NE(0)=top-right edge, E(1)=right edge, SE(2)=bottom-right edge
	// SW(3)=bottom-left edge, W(4)=left edge, NW(5)=top-left edge
	const WALL_EDGES: [number, number][] = [
		[0, 1], // NE: V0 to V1
		[5, 0], // E: V5 to V0
		[4, 5], // SE: V4 to V5
		[3, 4], // SW: V3 to V4
		[2, 3], // W: V2 to V3
		[1, 2], // NW: V1 to V2
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
		<rect x="0" y="0" width={viewBoxWidth} height={viewBoxHeight} fill="#080e1e" rx="8" />

		<!-- Hex cell backgrounds -->
		{#each allCells as cell (`bg-${cell.col},${cell.row}`)}
			{@const center = hexCenter(cell.col, cell.row)}
			<polygon
				points={hexPoints(center.x, center.y)}
				fill={cell.value === 2 ? 'rgba(109,40,217,0.15)' :
					  cell.value === 3 ? 'rgba(52,211,153,0.15)' :
					  'rgba(255,255,255,0.015)'}
				stroke="rgba(255,255,255,0.04)"
				stroke-width="0.5"
			/>
		{/each}

		<!-- Exit marker -->
		<circle
			class="exit-dot"
			cx={exitCenter.x}
			cy={exitCenter.y}
			r={HEX_SIZE * 0.35}
			fill="url(#hgrad-exit)"
			filter="url(#hglow-player)"
		/>

		<!-- Walls — glow layer -->
		<g stroke={stroke} stroke-width="2" stroke-linecap="round" opacity="0.4" filter="url(#hglow-wall)">
			{#each allCells as cell (`wg-${cell.col},${cell.row}`)}
				{#each wallLines(cell) as seg}
					<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
				{/each}
			{/each}
		</g>

		<!-- Walls — solid layer -->
		<g stroke={stroke} stroke-width="1.5" stroke-linecap="round">
			{#each allCells as cell (`ws-${cell.col},${cell.row}`)}
				{#each wallLines(cell) as seg}
					<line x1={seg.x1} y1={seg.y1} x2={seg.x2} y2={seg.y2} />
				{/each}
			{/each}
		</g>

		<!-- Player -->
		<circle
			class="player-halo"
			cx={playerCenter.x}
			cy={playerCenter.y}
			r={HEX_SIZE * 0.55}
			fill="rgba(139,92,246,0.18)"
			filter="url(#hglow-player)"
		/>
		<circle
			cx={playerCenter.x}
			cy={playerCenter.y}
			r={HEX_SIZE * 0.4}
			fill="url(#hgrad-player)"
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
		background: #080e1e;
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
