<!--
  CircularMazeRenderer: SVG-based circular (polar/theta) maze rendering.
  Concentric rings with arc walls and radial walls.
-->
<script lang="ts">
	import type { CircularMazeData } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#c084fc',
		accentColor = '#c084fc'
	}: {
		maze: CircularMazeData;
		playerPos: { ring: number; sector: number };
		wallColor?: string;
		accentColor?: string;
	} = $props();

	const RING_WIDTH = 30;
	const CENTER_R = 24;
	const PADDING = 20;

	const FALLBACK = '#c084fc';
	const stroke = $derived(!wallColor || wallColor === '#000000' ? FALLBACK : wallColor);

	const outerR = $derived(CENTER_R + maze.numRings * RING_WIDTH);
	const size = $derived(outerR * 2 + PADDING * 2);
	const cx = $derived(size / 2);
	const cy = $derived(size / 2);

	function ringInnerR(ring: number): number {
		if (ring === 0) return 0;
		return CENTER_R + (ring - 1) * RING_WIDTH;
	}
	function ringOuterR(ring: number): number {
		if (ring === 0) return CENTER_R;
		return CENTER_R + ring * RING_WIDTH;
	}

	function sectorAngle(sector: number, numSectors: number): number {
		return (sector / numSectors) * Math.PI * 2 - Math.PI / 2;
	}

	function polarToXY(r: number, angle: number): { x: number; y: number } {
		return { x: cx + r * Math.cos(angle), y: cy + r * Math.sin(angle) };
	}

	function cellCenter(ring: number, sector: number): { x: number; y: number } {
		const numSectors = maze.rings[ring].length;
		const midR = (ringInnerR(ring) + ringOuterR(ring)) / 2;
		const midAngle = sectorAngle(sector + 0.5, numSectors);
		return polarToXY(midR, midAngle);
	}

	// Arc path for SVG (large arc flag = 0 for small sectors)
	function arcPath(r: number, startAngle: number, endAngle: number): string {
		const start = polarToXY(r, startAngle);
		const end = polarToXY(r, endAngle);
		const angleDiff = endAngle - startAngle;
		const largeArc = Math.abs(angleDiff) > Math.PI ? 1 : 0;
		const sweep = angleDiff > 0 ? 1 : 0;
		return `M ${start.x} ${start.y} A ${r} ${r} 0 ${largeArc} ${sweep} ${end.x} ${end.y}`;
	}

	interface WallPath {
		d: string;
		key: string;
	}

	const wallPaths = $derived.by(() => {
		const paths: WallPath[] = [];

		for (let ring = 0; ring < maze.numRings; ring++) {
			const numSectors = maze.rings[ring].length;
			const oR = ringOuterR(ring);

			for (let s = 0; s < numSectors; s++) {
				const cell = maze.rings[ring][s];

				// Clockwise wall (radial line at the end of this sector)
				if (cell.wallCW) {
					const angle = sectorAngle(s + 1, numSectors);
					const inner = polarToXY(ringInnerR(ring), angle);
					const outer = polarToXY(oR, angle);
					if (ring === 0) {
						// Center cell — just draw from center to outer
						paths.push({
							d: `M ${cx} ${cy} L ${outer.x} ${outer.y}`,
							key: `cw-${ring}-${s}`
						});
					} else {
						paths.push({
							d: `M ${inner.x} ${inner.y} L ${outer.x} ${outer.y}`,
							key: `cw-${ring}-${s}`
						});
					}
				}

				// Outward wall (arc along the outer edge of this sector)
				if (cell.wallOut && ring < maze.numRings - 1) {
					const startAngle = sectorAngle(s, numSectors);
					const endAngle = sectorAngle(s + 1, numSectors);
					paths.push({
						d: arcPath(oR, startAngle, endAngle),
						key: `out-${ring}-${s}`
					});
				}
			}
		}

		// Outer boundary of the outermost ring
		const lastRing = maze.numRings - 1;
		const oR = ringOuterR(lastRing);
		paths.push({
			d: `M ${cx + oR} ${cy} A ${oR} ${oR} 0 1 1 ${cx + oR - 0.001} ${cy}`,
			key: 'outer-boundary'
		});

		return paths;
	});

	// Cell backgrounds for start/end
	interface CellArc {
		d: string;
		fill: string;
		key: string;
	}

	const cellArcs = $derived.by(() => {
		const arcs: CellArc[] = [];
		for (let ring = 0; ring < maze.numRings; ring++) {
			const numSectors = maze.rings[ring].length;
			for (let s = 0; s < numSectors; s++) {
				const cell = maze.rings[ring][s];
				if (cell.value !== 2 && cell.value !== 3) continue;

				const iR = ringInnerR(ring);
				const oR = ringOuterR(ring);
				const a1 = sectorAngle(s, numSectors);
				const a2 = sectorAngle(s + 1, numSectors);
				const angleDiff = a2 - a1;
				const largeArc = Math.abs(angleDiff) > Math.PI ? 1 : 0;

				const outerStart = polarToXY(oR, a1);
				const outerEnd = polarToXY(oR, a2);

				let d: string;
				if (ring === 0) {
					// Wedge from center
					d = `M ${cx} ${cy} L ${outerStart.x} ${outerStart.y} A ${oR} ${oR} 0 ${largeArc} 1 ${outerEnd.x} ${outerEnd.y} Z`;
				} else {
					const innerStart = polarToXY(iR, a1);
					const innerEnd = polarToXY(iR, a2);
					d = `M ${innerStart.x} ${innerStart.y} L ${outerStart.x} ${outerStart.y} A ${oR} ${oR} 0 ${largeArc} 1 ${outerEnd.x} ${outerEnd.y} L ${innerEnd.x} ${innerEnd.y} A ${iR} ${iR} 0 ${largeArc} 0 ${innerStart.x} ${innerStart.y} Z`;
				}

				arcs.push({
					d,
					fill: cell.value === 2 ? 'rgba(109,40,217,0.18)' : 'rgba(52,211,153,0.15)',
					key: `cell-${ring}-${s}`
				});
			}
		}
		return arcs;
	});

	const playerXY = $derived(cellCenter(playerPos.ring, playerPos.sector));
	const exitXY = $derived(cellCenter(maze.end.ring, maze.end.sector));
</script>

<div class="circ-container" role="img" aria-label="Circular maze">
	<svg
		class="circ-svg"
		viewBox={`0 0 ${size} ${size}`}
		preserveAspectRatio="xMidYMid meet"
	>
		<defs>
			<filter id="cglow-wall" x="-30%" y="-30%" width="160%" height="160%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="2" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<filter id="cglow-player" x="-60%" y="-60%" width="220%" height="220%" color-interpolation-filters="sRGB">
				<feGaussianBlur in="SourceGraphic" stdDeviation="3" result="blur" />
				<feComposite in="SourceGraphic" in2="blur" operator="over" />
			</filter>
			<radialGradient id="cgrad-player" cx="38%" cy="32%" r="65%">
				<stop offset="0%" stop-color="#ede9fe" />
				<stop offset="60%" stop-color="#a78bfa" />
				<stop offset="100%" stop-color="#5b21b6" />
			</radialGradient>
			<radialGradient id="cgrad-exit" cx="40%" cy="35%" r="65%">
				<stop offset="0%" stop-color="#d1fae5" />
				<stop offset="60%" stop-color="#34d399" />
				<stop offset="100%" stop-color="#065f46" />
			</radialGradient>
		</defs>

		<!-- Background -->
		<rect x="0" y="0" width={size} height={size} fill="#080e1e" rx="8" />

		<!-- Ring grid lines (subtle) -->
		{#each Array(maze.numRings) as _, ring}
			<circle
				cx={cx} cy={cy}
				r={ringOuterR(ring)}
				fill="none"
				stroke="rgba(255,255,255,0.03)"
				stroke-width="0.5"
			/>
		{/each}

		<!-- Cell highlights -->
		{#each cellArcs as arc (arc.key)}
			<path d={arc.d} fill={arc.fill} />
		{/each}

		<!-- Exit marker -->
		<circle
			class="exit-dot"
			cx={exitXY.x}
			cy={exitXY.y}
			r={RING_WIDTH * 0.32}
			fill="url(#cgrad-exit)"
			filter="url(#cglow-player)"
		/>

		<!-- Walls — glow layer -->
		<g fill="none" stroke={stroke} stroke-width="2" stroke-linecap="round" opacity="0.4" filter="url(#cglow-wall)">
			{#each wallPaths as wall (wall.key)}
				<path d={wall.d} />
			{/each}
		</g>

		<!-- Walls — solid layer -->
		<g fill="none" stroke={stroke} stroke-width="1.5" stroke-linecap="round">
			{#each wallPaths as wall (wall.key)}
				<path d={wall.d} />
			{/each}
		</g>

		<!-- Player -->
		<circle
			class="player-halo"
			cx={playerXY.x}
			cy={playerXY.y}
			r={RING_WIDTH * 0.5}
			fill="rgba(139,92,246,0.18)"
			filter="url(#cglow-player)"
		/>
		<circle
			cx={playerXY.x}
			cy={playerXY.y}
			r={RING_WIDTH * 0.35}
			fill="url(#cgrad-player)"
		/>
	</svg>
</div>

<style>
	.circ-container {
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

	.circ-svg {
		display: block;
		width: 100%;
		height: 100%;
	}

	.exit-dot {
		transform-box: fill-box;
		transform-origin: center;
		animation: circ-exit-pulse 2.4s ease-in-out infinite;
	}

	@keyframes circ-exit-pulse {
		0%, 100% { transform: scale(0.88); opacity: 0.8; }
		50%       { transform: scale(1.12); opacity: 1; }
	}

	.player-halo {
		animation: circ-breathe 2s ease-in-out infinite;
	}

	@keyframes circ-breathe {
		0%, 100% { opacity: 0.5; }
		50%       { opacity: 1; }
	}
</style>
