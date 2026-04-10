<!--
  CampaignMazeMap — fullscreen scrollable/pannable SVG campaign map.

  Layers (bottom to top):
    1. Background fill
    2. Path corridor tiles (walkable cells)
    3. Bonus branch corridors (dashed)
    4. Map collectibles (chest / gem / key / cloak / powerup icons)
    5. Level nodes (clickable circles)
    6. Gate overlays (star gates and key gates)
    7. Fog of war regions (fade out as areas unlock)
    8. Player marker at furthest unlocked level
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { onMount, untrack } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import type { WorldMapLayout, MapNode, MapCollectible } from '$lib/core/types';
	import MapCollectiblePopup from '$lib/components/MapCollectiblePopup.svelte';
	import { WORLD_THEMES } from '$lib/worldThemes';

	let { worldId, layout }: { worldId: number; layout: WorldMapLayout } = $props();

	const T = $derived(layout.tileSize);
	const svgWidth = $derived(layout.cols * T);
	const svgHeight = $derived(layout.rows * T);

	// Pan / zoom state
	let panX = $state(0);
	let panY = $state(0);
	let scale = $state(1);
	const MIN_SCALE = 0.3;
	const MAX_SCALE = 2.0;

	// Pan gesture tracking
	let autoPanDone = false;
	let isPanning = $state(false);
	let pointerIsDown = false;
	let panStartX = 0;
	let panStartY = 0;
	let panOriginX = 0;
	let panOriginY = 0;
	let touchStartDist = 0;
	let touchStartScale = 1;

	// Container ref for clamping
	let containerEl = $state<HTMLDivElement | null>(null);

	// Tooltip / hover state
	let tooltip = $state<{ text: string; x: number; y: number } | null>(null);

	// Collectible popup
	let activeCollectible = $state<MapCollectible | null>(null);

	// ---------------------------------------------------------------------------
	// Derived: player's furthest area and level nodes by area
	// ---------------------------------------------------------------------------

	const highestArea = $derived(gameStore.getHighestAreaUnlocked(worldId));

	function isLevelUnlocked(levelNumber: string): boolean {
		if (levelNumber === '1') return true;
		for (const node of layout.nodes) {
			if (node.type !== 'level' && node.type !== 'bonus_level') continue;
			const prog = gameStore.getLevelProgress(worldId, node.levelNumber ?? '');
			if (!prog?.completed) continue;
			// Check if this node connects to the requested level via level data
			const worldDef = gameStore.worlds.find((w) => w.worldId === worldId);
			const levelDef = worldDef?.levels.find((l) => l.levelNumber === node.levelNumber);
			if (levelDef && (levelDef.connectTo1 === levelNumber || levelDef.connectTo2 === levelNumber)) {
				return true;
			}
		}
		return false;
	}

	function isGateOpen(node: MapNode): boolean {
		if (node.type === 'star_gate') {
			const stars = gameStore.getWorldStarCount(worldId);
			return stars >= (node.starsRequired ?? 0);
		}
		if (node.type === 'key_gate') {
			return gameStore.hasKey(node.keyItemId ?? '');
		}
		return true;
	}

	function isAreaVisible(area: number): boolean {
		return area <= highestArea;
	}

	// Find the furthest completed level to place the player marker
	const playerMarkerNode = $derived((() => {
		let best: MapNode | null = null;
		for (const node of [...layout.nodes].reverse()) {
			if (node.type !== 'level' && node.type !== 'bonus_level') continue;
			const prog = gameStore.getLevelProgress(worldId, node.levelNumber ?? '');
			if (prog?.completed) { best = node; break; }
		}
		return best;
	})());

	// ---------------------------------------------------------------------------
	// Auto-pan to player position on mount
	// ---------------------------------------------------------------------------

	$effect(() => {
		const el = containerEl;
		const marker = playerMarkerNode;
		if (!el || !marker || autoPanDone) return;
		autoPanDone = true;
		const cw = el.clientWidth;
		const ch = el.clientHeight;
		const targetX = marker.tile.col * T + T / 2;
		const targetY = marker.tile.row * T + T / 2;
		untrack(() => {
			panX = cw / 2 - targetX * scale;
			panY = ch / 2 - targetY * scale;
			clampPan();
		});
	});

	function clampPan() {
		if (!containerEl) return;
		const cw = containerEl.clientWidth;
		const ch = containerEl.clientHeight;
		const scaledW = svgWidth * scale;
		const scaledH = svgHeight * scale;
		panX = Math.min(cw * 0.3, Math.max(cw - scaledW - cw * 0.3, panX));
		panY = Math.min(ch * 0.3, Math.max(ch - scaledH - ch * 0.3, panY));
	}

	// ---------------------------------------------------------------------------
	// Mouse / touch interactions
	// ---------------------------------------------------------------------------

	function onPointerDown(e: PointerEvent) {
		if (e.button !== 0 && e.pointerType !== 'touch') return;
		pointerIsDown = true;
		panStartX = e.clientX;
		panStartY = e.clientY;
		panOriginX = panX;
		panOriginY = panY;
	}

	function onPointerMove(e: PointerEvent) {
		// Only pan when a button is actually held down.
		if (!pointerIsDown) return;
		const dx = e.clientX - panStartX;
		const dy = e.clientY - panStartY;
		// Only commit to panning once we've clearly dragged past the click threshold.
		if (!isPanning) {
			if (Math.hypot(dx, dy) < 6) return;
			isPanning = true;
			(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
		}
		panX = panOriginX + dx;
		panY = panOriginY + dy;
		clampPan();
	}

	function onPointerUp() {
		pointerIsDown = false;
		isPanning = false;
	}

	function onWheel(e: WheelEvent) {
		e.preventDefault();
		const delta = e.deltaY > 0 ? 0.9 : 1.1;
		const newScale = Math.min(MAX_SCALE, Math.max(MIN_SCALE, scale * delta));
		// Zoom towards cursor
		const rect = containerEl!.getBoundingClientRect();
		const cx = e.clientX - rect.left;
		const cy = e.clientY - rect.top;
		panX = cx - (cx - panX) * (newScale / scale);
		panY = cy - (cy - panY) * (newScale / scale);
		scale = newScale;
		clampPan();
	}

	function onTouchStart(e: TouchEvent) {
		if (e.touches.length === 2) {
			const dx = e.touches[0].clientX - e.touches[1].clientX;
			const dy = e.touches[0].clientY - e.touches[1].clientY;
			touchStartDist = Math.hypot(dx, dy);
			touchStartScale = scale;
			isPanning = false;
		} else if (e.touches.length === 1) {
			isPanning = true;
			panStartX = e.touches[0].clientX;
			panStartY = e.touches[0].clientY;
			panOriginX = panX;
			panOriginY = panY;
		}
	}

	function onTouchMove(e: TouchEvent) {
		e.preventDefault();
		if (e.touches.length === 2) {
			const dx = e.touches[0].clientX - e.touches[1].clientX;
			const dy = e.touches[0].clientY - e.touches[1].clientY;
			const dist = Math.hypot(dx, dy);
			scale = Math.min(MAX_SCALE, Math.max(MIN_SCALE, touchStartScale * (dist / touchStartDist)));
			clampPan();
		} else if (e.touches.length === 1 && isPanning) {
			panX = panOriginX + (e.touches[0].clientX - panStartX);
			panY = panOriginY + (e.touches[0].clientY - panStartY);
			clampPan();
		}
	}

	function onTouchEnd() {
		isPanning = false;
	}

	// Attach wheel and touchmove with { passive: false } so e.preventDefault()
	// actually suppresses native scroll/zoom (Svelte 5 inline directives are passive).
	onMount(() => {
		const el = containerEl!;
		el.addEventListener('wheel', onWheel, { passive: false });
		el.addEventListener('touchmove', onTouchMove, { passive: false });
		return () => {
			el.removeEventListener('wheel', onWheel);
			el.removeEventListener('touchmove', onTouchMove);
		};
	});

	// ---------------------------------------------------------------------------
	// Node interaction
	// ---------------------------------------------------------------------------

	function onNodeClick(e: MouseEvent, node: MapNode) {
		// Prevent triggering pan
		if (Math.abs((e.clientX - panStartX)) > 6 || Math.abs(e.clientY - panStartY) > 6) return;

		if (node.type === 'level' || node.type === 'bonus_level') {
			if (!isLevelUnlocked(node.levelNumber ?? '')) return;
			goto(`${base}/campaign/play/${worldId}-${node.levelNumber}`);
		} else if (node.type === 'star_gate') {
			const stars = gameStore.getWorldStarCount(worldId);
			const needed = node.starsRequired ?? 0;
			showTooltip(e, isGateOpen(node) ? 'Gate Open!' : `Requires ${needed} ★ — you have ${stars}`);
		} else if (node.type === 'key_gate') {
			const hasIt = gameStore.hasKey(node.keyItemId ?? '');
			showTooltip(e, hasIt ? 'Gate Open!' : `Requires the ${node.keyItemId?.replace(/_/g, ' ') ?? 'key'}`);
		} else if (node.type === 'portal') {
			const stars = gameStore.getWorldStarCount(worldId);
			if (stars >= 300) showTooltip(e, 'Portal to World 2!');
			else showTooltip(e, `Portal locked — need 300 ★ (you have ${stars})`);
		}
	}

	function onNodeHover(e: MouseEvent, node: MapNode) {
		if (node.type === 'star_gate') {
			const stars = gameStore.getWorldStarCount(worldId);
			showTooltip(e, `Star Gate — ${node.starsRequired} ★ required (you: ${stars})`);
		} else if (node.type === 'key_gate') {
			const name = node.keyItemId?.replace(/_/g, ' ') ?? 'key';
			showTooltip(e, `Key Gate — requires "${name}"`);
		} else if ((node.type === 'level' || node.type === 'bonus_level') && !isLevelUnlocked(node.levelNumber ?? '')) {
			const worldDef = gameStore.worlds.find((w) => w.worldId === worldId);
			const lvl = worldDef?.levels.find((l) => l.levelNumber === node.levelNumber);
			const minStars = lvl?.minimumStarsToUnlock ?? 0;
			const stars = gameStore.getWorldStarCount(worldId);
			showTooltip(e, minStars > 0 && stars < minStars
				? `Level ${node.levelNumber} — needs ${minStars} ★ (you: ${stars})`
				: `Level ${node.levelNumber} — complete previous levels first`);
		} else {
			tooltip = null;
		}
	}

	function onCollectibleClick(e: MouseEvent, item: MapCollectible) {
		if (Math.abs(e.clientX - panStartX) > 6 || Math.abs(e.clientY - panStartY) > 6) return;
		if (gameStore.isMapItemCollected(worldId, item.id)) return;
		activeCollectible = item;
	}

	function onCollectibleCollect(item: MapCollectible) {
		gameStore.collectMapItem(worldId, item.id);
		const r = item.reward;
		if (r.coins) gameStore.addCoins(r.coins);
		if (r.powerup && r.powerupCount) {
			for (let i = 0; i < r.powerupCount; i++) gameStore.addPowerup(r.powerup);
		}
		if (r.skinId != null) gameStore.unlockSkin(r.skinId);
		if (r.keyItemId) gameStore.addKey(r.keyItemId);
		activeCollectible = null;
	}

	function showTooltip(e: MouseEvent, text: string) {
		tooltip = { text, x: e.clientX, y: e.clientY };
		setTimeout(() => { tooltip = null; }, 2200);
	}

	// ---------------------------------------------------------------------------
	// SVG rendering helpers
	// ---------------------------------------------------------------------------

	const CORRIDOR_W = 0.7; // fraction of tileSize for corridor width
	const WALL_COLOR = '#0a1628';
	const PATH_COLOR = '#1a2d4a';
	const BONUS_COLOR = '#1e3a2f';
	const ACCENT_BLUE = '#38bdf8';
	const ACCENT_GOLD = '#fbbf24';
	const worldTheme = $derived(WORLD_THEMES.find(t => t.worldId === worldId) ?? WORLD_THEMES[0]);
	const GATE_RED = '#f87171';
	const GATE_GREEN = '#4ade80';
	const GEM_COLOR = '#a78bfa';
	const CLOAK_COLOR = '#fb923c';

	/** Convert tile col/row to SVG centre-point */
	function cx(col: number) { return col * T + T / 2; }
	function cy(row: number) { return row * T + T / 2; }

	function levelNodeFill(node: MapNode): string {
		const prog = gameStore.getLevelProgress(worldId, node.levelNumber ?? '');
		if (prog?.completed) return node.type === 'bonus_level' ? ACCENT_GOLD : worldTheme.accentColor;
		if (isLevelUnlocked(node.levelNumber ?? '')) return '#0f2a44';
		return '#0a1628';
	}

	function levelNodeStroke(node: MapNode): string {
		const prog = gameStore.getLevelProgress(worldId, node.levelNumber ?? '');
		if (prog?.completed) return node.type === 'bonus_level' ? ACCENT_GOLD : worldTheme.accentColor;
		if (isLevelUnlocked(node.levelNumber ?? '')) return worldTheme.accentColor;
		return '#1e3a5c';
	}

	function starCountForNode(levelNumber: string): number {
		return gameStore.getLevelProgress(worldId, levelNumber)?.numberOfStars ?? 0;
	}

	function collectibleColor(type: MapCollectible['type']): string {
		switch (type) {
			case 'chest': return ACCENT_GOLD;
			case 'key': return '#facc15';
			case 'gem': return GEM_COLOR;
			case 'cloak': return CLOAK_COLOR;
			default: return '#86efac'; // powerups → green
		}
	}
</script>

<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class="map-container"
	bind:this={containerEl}
	onpointerdown={onPointerDown}
	onpointermove={onPointerMove}
	onpointerup={onPointerUp}
	onpointercancel={onPointerUp}
	ontouchstart={onTouchStart}
	ontouchend={onTouchEnd}
	role="application"
	aria-label="Campaign map — pan and zoom to explore"
>
	<div
		class="map-canvas"
		style="transform: translate({panX}px, {panY}px) scale({scale}); transform-origin: 0 0; width: {svgWidth}px; height: {svgHeight}px;"
	>
		<svg
			width={svgWidth}
			height={svgHeight}
			viewBox="0 0 {svgWidth} {svgHeight}"
			xmlns="http://www.w3.org/2000/svg"
			style="display:block;"
		>
			<defs>
				<!-- Fog feather filter -->
				<filter id="fog-blur">
					<feGaussianBlur in="SourceGraphic" stdDeviation="18" />
				</filter>
				<!-- Glow filter for active nodes -->
				<filter id="node-glow" x="-50%" y="-50%" width="200%" height="200%">
					<feGaussianBlur in="SourceGraphic" stdDeviation="4" result="blur" />
					<feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge>
				</filter>
				<!-- Ambient blur for nebula blobs -->
				<filter id="ambient-blur" x="-40%" y="-40%" width="180%" height="180%">
					<feGaussianBlur in="SourceGraphic" stdDeviation="32" />
				</filter>
				<!-- Gate shimmer animation -->
				<radialGradient id="gate-closed-grad" cx="50%" cy="50%" r="50%">
					<stop offset="0%" stop-color="#f87171" stop-opacity="0.5"/>
					<stop offset="100%" stop-color="#7f1d1d" stop-opacity="0.2"/>
				</radialGradient>
				<radialGradient id="gate-open-grad" cx="50%" cy="50%" r="50%">
					<stop offset="0%" stop-color="#4ade80" stop-opacity="0.5"/>
					<stop offset="100%" stop-color="#14532d" stop-opacity="0.1"/>
				</radialGradient>
				<!-- Animated background gradient (world-themed center glow + edge vignette) -->
				<radialGradient id="bg-grad" cx="50%" cy="40%" r="65%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.07">
						<animate attributeName="stop-opacity" values="0.07;0.13;0.07" dur="14s" repeatCount="indefinite"/>
					</stop>
					<stop offset="55%" stop-color="#060e1c" stop-opacity="0"/>
					<stop offset="100%" stop-color="#020610" stop-opacity="1"/>
				</radialGradient>
				<!-- Nebula radial gradients (world-themed accent color) -->
				<radialGradient id="nebula-tl" cx="20%" cy="20%" r="50%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.32"/>
					<stop offset="100%" stop-color={worldTheme.accentColor} stop-opacity="0"/>
				</radialGradient>
				<radialGradient id="nebula-br" cx="80%" cy="80%" r="50%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.26"/>
					<stop offset="100%" stop-color={worldTheme.accentColor} stop-opacity="0"/>
				</radialGradient>
				<radialGradient id="nebula-mid" cx="55%" cy="50%" r="45%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.17"/>
					<stop offset="100%" stop-color={worldTheme.accentColor} stop-opacity="0"/>
				</radialGradient>
				<radialGradient id="nebula-tr" cx="80%" cy="18%" r="40%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.23"/>
					<stop offset="100%" stop-color={worldTheme.accentColor} stop-opacity="0"/>
				</radialGradient>
				<!-- Fifth nebula — lower-left -->
				<radialGradient id="nebula-bl" cx="15%" cy="80%" r="38%">
					<stop offset="0%" stop-color={worldTheme.accentColor} stop-opacity="0.19"/>
					<stop offset="100%" stop-color={worldTheme.accentColor} stop-opacity="0"/>
				</radialGradient>
				<!-- Crosshatch grid pattern (world-tinted lines) -->
				<pattern id="map-grid" x="0" y="0" width={T} height={T} patternUnits="userSpaceOnUse">
					<line x1={T / 2} y1="0" x2={T / 2} y2={T} stroke={worldTheme.accentColor} stroke-width="0.5" opacity="0.07"/>
					<line x1="0" y1={T / 2} x2={T} y2={T / 2} stroke={worldTheme.accentColor} stroke-width="0.5" opacity="0.07"/>
				</pattern>
			</defs>

			<!-- ── Layer 1: Animated background ────────────────────── -->
			<rect width={svgWidth} height={svgHeight} fill="url(#bg-grad)" />

			<!-- ── Layer 1b: Nebula atmosphere blobs ───────────────── -->
			<ellipse
				cx={svgWidth * 0.18} cy={svgHeight * 0.2}
				rx={svgWidth * 0.35} ry={svgHeight * 0.32}
				fill="url(#nebula-tl)" filter="url(#ambient-blur)"
			>
				<animate attributeName="opacity" values="1;0.55;1" dur="9s" begin="0s" repeatCount="indefinite"/>
			</ellipse>
			<ellipse
				cx={svgWidth * 0.82} cy={svgHeight * 0.78}
				rx={svgWidth * 0.38} ry={svgHeight * 0.3}
				fill="url(#nebula-br)" filter="url(#ambient-blur)"
			>
				<animate attributeName="opacity" values="0.7;1;0.7" dur="11s" begin="2s" repeatCount="indefinite"/>
			</ellipse>
			<ellipse
				cx={svgWidth * 0.52} cy={svgHeight * 0.48}
				rx={svgWidth * 0.3} ry={svgHeight * 0.28}
				fill="url(#nebula-mid)" filter="url(#ambient-blur)"
			>
				<animate attributeName="opacity" values="0.6;1;0.6" dur="13s" begin="5s" repeatCount="indefinite"/>
			</ellipse>
			<ellipse
				cx={svgWidth * 0.8} cy={svgHeight * 0.15}
				rx={svgWidth * 0.25} ry={svgHeight * 0.22}
				fill="url(#nebula-tr)" filter="url(#ambient-blur)"
			>
				<animate attributeName="opacity" values="0.8;0.45;0.8" dur="10s" begin="7s" repeatCount="indefinite"/>
			</ellipse>
			<ellipse
				cx={svgWidth * 0.12} cy={svgHeight * 0.82}
				rx={svgWidth * 0.28} ry={svgHeight * 0.2}
				fill="url(#nebula-bl)" filter="url(#ambient-blur)"
			>
				<animate attributeName="opacity" values="0.55;1;0.55" dur="12s" begin="3.5s" repeatCount="indefinite"/>
			</ellipse>

			<!-- ── Layer 2a: Path corridor glow (soft background halo) -->
			{#each layout.pathSegments as seg}
				{#if !seg.bonus}
					{@const x1 = cx(seg.from.col)}
					{@const y1 = cy(seg.from.row)}
					{@const x2 = cx(seg.to.col)}
					{@const y2 = cy(seg.to.row)}
					<line
						{x1} {y1} {x2} {y2}
						stroke={worldTheme.accentColor}
						stroke-width={T * CORRIDOR_W * 1.8}
						stroke-linecap="round"
						opacity="0.045"
					/>
				{/if}
			{/each}

			<!-- ── Layer 2: Path corridors ─────────────────────────── -->
			{#each layout.pathSegments as seg}
				{@const x1 = cx(seg.from.col)}
				{@const y1 = cy(seg.from.row)}
				{@const x2 = cx(seg.to.col)}
				{@const y2 = cy(seg.to.row)}
				{@const strokeW = T * CORRIDOR_W}
				{@const color = seg.bonus ? BONUS_COLOR : PATH_COLOR}
				<line
					{x1} {y1} {x2} {y2}
					stroke={color}
					stroke-width={strokeW}
					stroke-linecap="round"
					stroke-dasharray={seg.bonus ? `${T * 0.5} ${T * 0.25}` : undefined}
					opacity={seg.bonus ? 0.8 : 1}
				/>
			{/each}

			<!-- ── Layer 3: Crosshatch grid texture ─────────────────── -->
			<rect width={svgWidth} height={svgHeight} fill="url(#map-grid)" pointer-events="none"/>

			<!-- ── Layer 3b: Drifting ambient particles ─────────────── -->
			{#each [
				{ col: 2,  row: 3,  dur: 9,  begin: 0   },
				{ col: 5,  row: 7,  dur: 12, begin: 1.5 },
				{ col: 9,  row: 2,  dur: 10, begin: 3   },
				{ col: 13, row: 5,  dur: 8,  begin: 0.7 },
				{ col: 1,  row: 10, dur: 14, begin: 2.2 },
				{ col: 7,  row: 12, dur: 11, begin: 4   },
				{ col: 15, row: 9,  dur: 9,  begin: 5.5 },
				{ col: 4,  row: 15, dur: 13, begin: 1   },
				{ col: 11, row: 1,  dur: 10, begin: 6   },
				{ col: 8,  row: 8,  dur: 12, begin: 3.3 },
				{ col: 3,  row: 18, dur: 9,  begin: 0.4 },
				{ col: 16, row: 14, dur: 11, begin: 7   },
				{ col: 6,  row: 4,  dur: 8,  begin: 2.8 },
				{ col: 12, row: 16, dur: 14, begin: 1.2 },
				{ col: 10, row: 11, dur: 10, begin: 5   },
			] as p}
				{@const px = cx(Math.min(p.col, layout.cols - 1))}
				{@const py = cy(Math.min(p.row, layout.rows - 1))}
				<circle cx={px} cy={py} r="1.5" fill={worldTheme.accentColor} opacity="0.35" pointer-events="none">
					<animate attributeName="cy" values="{py};{py - 55};{py}" dur="{p.dur}s" begin="{p.begin}s" repeatCount="indefinite" calcMode="spline" keyTimes="0;0.5;1" keySplines="0.4 0 0.6 1;0.4 0 0.6 1"/>
					<animate attributeName="opacity" values="0.35;0.0;0.35" dur="{p.dur}s" begin="{p.begin}s" repeatCount="indefinite" calcMode="spline" keyTimes="0;0.5;1" keySplines="0.4 0 0.6 1;0.4 0 0.6 1"/>
				</circle>
			{/each}

			<!-- ── Layer 3c: World-specific motif elements ──────────── -->
			{#if worldTheme.motif === 'space'}
				<!-- Twinkling background stars scattered across the map -->
				{#each [
					{ x: 0.04, y: 0.06, r: 1.1, dur: 3.2, begin: 0.0 },
					{ x: 0.18, y: 0.03, r: 0.8, dur: 2.8, begin: 0.6 },
					{ x: 0.31, y: 0.09, r: 1.0, dur: 4.0, begin: 1.1 },
					{ x: 0.55, y: 0.04, r: 0.7, dur: 3.5, begin: 1.8 },
					{ x: 0.72, y: 0.08, r: 1.2, dur: 2.6, begin: 0.3 },
					{ x: 0.88, y: 0.02, r: 0.9, dur: 3.8, begin: 2.0 },
					{ x: 0.07, y: 0.22, r: 0.6, dur: 4.2, begin: 0.9 },
					{ x: 0.40, y: 0.17, r: 1.0, dur: 3.0, begin: 1.5 },
					{ x: 0.63, y: 0.20, r: 0.8, dur: 3.6, begin: 2.4 },
					{ x: 0.92, y: 0.25, r: 0.7, dur: 2.9, begin: 0.7 },
					{ x: 0.14, y: 0.38, r: 1.1, dur: 4.1, begin: 3.0 },
					{ x: 0.50, y: 0.33, r: 0.8, dur: 3.3, begin: 1.2 },
					{ x: 0.78, y: 0.41, r: 1.0, dur: 2.7, begin: 2.8 },
					{ x: 0.96, y: 0.36, r: 0.6, dur: 4.4, begin: 0.2 },
					{ x: 0.03, y: 0.55, r: 0.9, dur: 3.7, begin: 1.9 },
					{ x: 0.26, y: 0.60, r: 1.2, dur: 3.1, begin: 0.5 },
					{ x: 0.60, y: 0.58, r: 0.7, dur: 4.0, begin: 2.1 },
					{ x: 0.84, y: 0.63, r: 1.0, dur: 2.5, begin: 3.3 },
					{ x: 0.10, y: 0.76, r: 0.8, dur: 3.9, begin: 1.4 },
					{ x: 0.45, y: 0.80, r: 0.6, dur: 3.4, begin: 2.6 },
					{ x: 0.70, y: 0.78, r: 1.1, dur: 3.0, begin: 0.8 },
					{ x: 0.90, y: 0.84, r: 0.9, dur: 4.3, begin: 1.7 },
					{ x: 0.22, y: 0.92, r: 0.7, dur: 2.8, begin: 3.5 },
					{ x: 0.55, y: 0.95, r: 1.0, dur: 3.6, begin: 0.4 },
					{ x: 0.80, y: 0.97, r: 0.8, dur: 2.6, begin: 2.2 },
				] as s}
					<circle
						cx={svgWidth * s.x} cy={svgHeight * s.y}
						r={s.r} fill="white" pointer-events="none"
					>
						<animate attributeName="opacity" values="0.1;0.75;0.1" dur="{s.dur}s" begin="{s.begin}s" repeatCount="indefinite" calcMode="spline" keyTimes="0;0.5;1" keySplines="0.4 0 0.6 1;0.4 0 0.6 1"/>
					</circle>
				{/each}
			{:else if worldTheme.motif === 'tech'}
				<!-- Faint circuit nodes pulsing in the background -->
				{#each [
					{ col: 1,  row: 2  }, { col: 4,  row: 5  }, { col: 7,  row: 1  },
					{ col: 10, row: 4  }, { col: 13, row: 2  }, { col: 2,  row: 8  },
					{ col: 5,  row: 11 }, { col: 9,  row: 9  }, { col: 12, row: 7  },
					{ col: 15, row: 10 }, { col: 3,  row: 14 }, { col: 8,  row: 16 },
					{ col: 11, row: 13 }, { col: 14, row: 15 }, { col: 6,  row: 17 },
					{ col: 0,  row: 6  }, { col: 16, row: 3  }, { col: 1,  row: 18 },
				] as n}
					{@const nx = cx(Math.min(n.col, layout.cols - 1))}
					{@const ny = cy(Math.min(n.row, layout.rows - 1))}
					<circle cx={nx} cy={ny} r={T * 0.11} fill="none" stroke={worldTheme.accentColor} stroke-width="1" opacity="0.13" pointer-events="none">
						<animate attributeName="opacity" values="0.13;0.28;0.13" dur="{6 + (n.col % 4)}s" begin="{(n.row % 5) * 0.7}s" repeatCount="indefinite"/>
					</circle>
					<!-- Short connector dashes radiating from node -->
					<line x1={nx - T * 0.18} y1={ny} x2={nx - T * 0.28} y2={ny} stroke={worldTheme.accentColor} stroke-width="1" opacity="0.10" stroke-dasharray="3 4" pointer-events="none"/>
					<line x1={nx + T * 0.18} y1={ny} x2={nx + T * 0.28} y2={ny} stroke={worldTheme.accentColor} stroke-width="1" opacity="0.10" stroke-dasharray="3 4" pointer-events="none"/>
					<line x1={nx} y1={ny - T * 0.18} x2={nx} y2={ny - T * 0.28} stroke={worldTheme.accentColor} stroke-width="1" opacity="0.10" stroke-dasharray="3 4" pointer-events="none"/>
				{/each}
			{:else if worldTheme.motif === 'elemental'}
				<!-- Slow drifting organic energy wisps -->
				{#each [
					{ cx: 0.22, cy: 0.32, rx: 0.18, ry: 0.07, dur: 15, begin: 0.0 },
					{ cx: 0.65, cy: 0.18, rx: 0.14, ry: 0.06, dur: 18, begin: 4.0 },
					{ cx: 0.45, cy: 0.70, rx: 0.16, ry: 0.07, dur: 12, begin: 7.0 },
					{ cx: 0.80, cy: 0.52, rx: 0.20, ry: 0.08, dur: 16, begin: 2.0 },
					{ cx: 0.10, cy: 0.62, rx: 0.14, ry: 0.06, dur: 14, begin: 5.5 },
					{ cx: 0.50, cy: 0.90, rx: 0.17, ry: 0.07, dur: 19, begin: 1.5 },
				] as w}
					<ellipse
						cx={svgWidth * w.cx} cy={svgHeight * w.cy}
						rx={svgWidth * w.rx} ry={svgHeight * w.ry}
						fill={worldTheme.accentColor} opacity="0" pointer-events="none"
						filter="url(#ambient-blur)"
					>
						<animate attributeName="opacity" values="0;0.20;0;0.13;0" dur="{w.dur}s" begin="{w.begin}s" repeatCount="indefinite" calcMode="spline" keyTimes="0;0.25;0.5;0.75;1" keySplines="0.4 0 0.6 1;0.4 0 0.6 1;0.4 0 0.6 1;0.4 0 0.6 1"/>
					</ellipse>
				{/each}
			{/if}

			<!-- ── Layer 4: Collectibles ────────────────────────────── -->
			{#each layout.collectibles as item}
				{@const collected = gameStore.isMapItemCollected(worldId, item.id)}
				{@const visible = isAreaVisible(item.area)}
				{#if visible}
					{@const px = cx(item.tile.col)}
					{@const py = cy(item.tile.row)}
					{@const color = collected ? '#374151' : collectibleColor(item.type)}
					<!-- svelte-ignore a11y_click_events_have_key_events -->
					<g
						class="collectible"
						class:collected
						onclick={(e) => onCollectibleClick(e, item)}
						role="button"
						aria-label={collected ? `${item.label} (collected)` : item.label}
						tabindex={collected ? undefined : 0}
					>
						<!-- Glow ring (uncollected) -->
						{#if !collected}
							<circle cx={px} cy={py} r={T * 0.38} fill={color} opacity="0.15"/>
						{/if}
						<!-- Icon background -->
						<circle cx={px} cy={py} r={T * 0.28} fill={collected ? '#1f2937' : '#0f1e35'} stroke={color} stroke-width="2" />
						<!-- Icon symbol -->
						{#if item.type === 'chest'}
							<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.28} fill={color}>🪙</text>
						{:else if item.type === 'key'}
							<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.28} fill={color}>🗝️</text>
						{:else if item.type === 'gem'}
							<!-- Diamond shape -->
							<polygon
								points="{px},{py - T*0.18} {px + T*0.14},{py} {px},{py + T*0.18} {px - T*0.14},{py}"
								fill={color} opacity={collected ? 0.3 : 1}
							/>
						{:else if item.type === 'cloak'}
							<!-- Cloak: hood arc -->
							<path
								d="M {px - T*0.15} {py + T*0.12} Q {px} {py - T*0.22} {px + T*0.15} {py + T*0.12} Z"
								fill={color} opacity={collected ? 0.3 : 1}
							/>
						{:else}
							<!-- Powerup: lightning bolt / star -->
							<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.26} fill={color}>⚡</text>
						{/if}
					</g>
				{/if}
			{/each}

			<!-- ── Layer 5: Level nodes ──────────────────────────────── -->
			{#each layout.nodes as node}
				{#if node.type === 'level' || node.type === 'bonus_level'}
					{@const px = cx(node.tile.col)}
					{@const py = cy(node.tile.row)}
					{@const unlocked = isLevelUnlocked(node.levelNumber ?? '')}
					{@const prog = gameStore.getLevelProgress(worldId, node.levelNumber ?? '')}
					{@const stars = starCountForNode(node.levelNumber ?? '')}
					{@const visible = isAreaVisible(node.area)}
					{#if visible}
						<!-- svelte-ignore a11y_click_events_have_key_events -->
						<g
							class="level-node"
							class:unlocked
							class:completed={!!prog?.completed}
							onclick={(e) => onNodeClick(e, node)}
							onmouseenter={(e) => onNodeHover(e, node)}
							onmouseleave={() => { tooltip = null; }}
							role="button"
							aria-label="Level {node.levelNumber}{prog?.completed ? ' (completed)' : unlocked ? ' (unlocked)' : ' (locked)'}"
							tabindex={unlocked ? 0 : undefined}
							filter={unlocked && !prog?.completed ? 'url(#node-glow)' : undefined}
						>
							<!-- Outer ring (completed = filled accent) -->
							<circle
								cx={px} cy={py}
								r={node.type === 'bonus_level' ? T * 0.32 : T * 0.36}
								fill={levelNodeFill(node)}
								stroke={levelNodeStroke(node)}
								stroke-width={prog?.completed ? 0 : 2}
								opacity={unlocked ? 1 : 0.45}
							/>
							<!-- Level number -->
							<text
								x={px} y={py + 5}
								text-anchor="middle"
								font-size={node.levelNumber!.length > 2 ? T * 0.18 : T * 0.22}
								font-weight="700"
								fill={prog?.completed ? '#0a0f1a' : unlocked ? '#e2f4ff' : '#4b6278'}
								font-family="monospace"
							>{node.levelNumber}</text>
							<!-- Lock icon if locked -->
							{#if !unlocked}
								<text x={px} y={py + T * 0.48} text-anchor="middle" font-size={T * 0.2} fill="#4b6278">🔒</text>
							{/if}
							<!-- Star dots (3 small dots below) -->
							{#if unlocked}
								{#each [0, 1, 2] as si}
									<circle
										cx={px + (si - 1) * T * 0.14} cy={py + T * 0.44}
										r={T * 0.07}
										fill={si < stars ? ACCENT_GOLD : 'rgba(30, 58, 92, 0.9)'}
									/>
								{/each}
							{/if}
						</g>
					{/if}
				{/if}
			{/each}

			<!-- ── Layer 6a: Star gates ──────────────────────────────── -->
			{#each layout.nodes as node}
				{#if node.type === 'star_gate'}
					{@const px = cx(node.tile.col)}
					{@const py = cy(node.tile.row)}
					{@const open = isGateOpen(node)}
					{@const visible = isAreaVisible(node.area - 1)}
					{#if visible}
						<!-- svelte-ignore a11y_click_events_have_key_events -->
						<g
							class="gate star-gate"
							class:open
							onclick={(e) => onNodeClick(e, node)}
							onmouseenter={(e) => onNodeHover(e, node)}
							onmouseleave={() => { tooltip = null; }}
							role="button"
							aria-label="Star gate — requires {node.starsRequired} stars"
						>
							<!-- Gate bar -->
							<rect
								x={px - T * 0.45} y={py - T * 0.12}
								width={T * 0.9} height={T * 0.24}
								rx={T * 0.06}
								fill={open ? 'url(#gate-open-grad)' : 'url(#gate-closed-grad)'}
								stroke={open ? GATE_GREEN : GATE_RED}
								stroke-width="2"
								opacity={open ? 0.7 : 1}
							/>
							<!-- Star icon + count -->
							<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.2} fill={open ? GATE_GREEN : '#fca5a5'} font-weight="700">
								★ {node.starsRequired}
							</text>
						</g>
					{/if}
				{/if}
			{/each}

			<!-- ── Layer 6b: Key gates ───────────────────────────────── -->
			{#each layout.nodes as node}
				{#if node.type === 'key_gate'}
					{@const px = cx(node.tile.col)}
					{@const py = cy(node.tile.row)}
					{@const open = isGateOpen(node)}
					{@const visible = isAreaVisible(node.area)}
					{#if visible}
						<!-- svelte-ignore a11y_click_events_have_key_events -->
						<g
							class="gate key-gate"
							class:open
							onclick={(e) => onNodeClick(e, node)}
							onmouseenter={(e) => onNodeHover(e, node)}
							onmouseleave={() => { tooltip = null; }}
							role="button"
							aria-label="Key gate — requires a key"
						>
							<!-- Vertical bars (portcullis look) -->
							{#each [-0.25, 0, 0.25] as xOff}
								<rect
									x={px + xOff * T - T * 0.07} y={py - T * 0.35}
									width={T * 0.14} height={T * 0.7}
									rx={T * 0.04}
									fill={open ? '#1e3a2f' : '#1c0a0a'}
									stroke={open ? GATE_GREEN : '#b45309'}
									stroke-width="2"
									opacity={open ? 0.5 : 1}
								/>
							{/each}
							<!-- Padlock -->
							{#if !open}
								<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.28} fill="#fbbf24">🔑</text>
							{:else}
								<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.28} fill={GATE_GREEN}>✓</text>
							{/if}
						</g>
					{/if}
				{/if}
			{/each}

			<!-- ── Layer 6c: Portal node ─────────────────────────────── -->
			{#each layout.nodes as node}
				{#if node.type === 'portal'}
					{@const px = cx(node.tile.col)}
					{@const py = cy(node.tile.row)}
					{@const stars = gameStore.getWorldStarCount(worldId)}
					{@const unlocked = stars >= 250}
					{@const visible = isAreaVisible(node.area)}
					{#if visible}
						<!-- svelte-ignore a11y_click_events_have_key_events -->
						<g
							class="portal-node"
							onclick={(e) => onNodeClick(e, node)}
							role="button"
							aria-label="World portal"
						>
							<circle cx={px} cy={py} r={T * 0.45}
								fill={unlocked ? '#1a0935' : '#0f0f1a'}
								stroke={unlocked ? '#a855f7' : '#374151'}
								stroke-width="3"
								opacity={unlocked ? 1 : 0.6}
							/>
							{#if unlocked}
								<!-- Animated ring -->
								<circle cx={px} cy={py} r={T * 0.45} fill="none" stroke="#c084fc" stroke-width="2" opacity="0.4">
									<animate attributeName="r" values="{T*0.45};{T*0.55};{T*0.45}" dur="2s" repeatCount="indefinite"/>
									<animate attributeName="opacity" values="0.4;0.1;0.4" dur="2s" repeatCount="indefinite"/>
								</circle>
							{/if}
							<text x={px} y={py + 5} text-anchor="middle" font-size={T * 0.32} fill={unlocked ? '#c084fc' : '#6b7280'}>🌀</text>
						</g>
					{/if}
				{/if}
			{/each}

			<!-- ── Layer 7: Player marker ───────────────────────────── -->
			{#if playerMarkerNode}
				{@const px = cx(playerMarkerNode.tile.col)}
				{@const py = cy(playerMarkerNode.tile.row)}
				<g class="player-marker">
					<!-- Outer pulse -->
					<circle cx={px} cy={py} r={T * 0.55} fill={worldTheme.accentColor} opacity="0.12">
						<animate attributeName="r" values="{T*0.55};{T*0.75};{T*0.55}" dur="1.8s" repeatCount="indefinite"/>
						<animate attributeName="opacity" values="0.12;0.0;0.12" dur="1.8s" repeatCount="indefinite"/>
					</circle>
					<!-- Marker pin -->
					<circle cx={px} cy={py} r={T * 0.16} fill={worldTheme.accentColor} stroke="#fff" stroke-width="2"/>
					<text x={px} y={py - T * 0.22} text-anchor="middle" font-size={T * 0.22} fill={worldTheme.accentColor}>▼</text>
				</g>
			{/if}

			<!-- ── Layer 8: Fog of war ───────────────────────────────── -->
			{#each layout.fogRegions as fog}
				{@const lifted = highestArea >= fog.area}
				{@const x = fog.topLeft.col * T}
				{@const y = fog.topLeft.row * T}
				{@const w = (fog.bottomRight.col - fog.topLeft.col + 1) * T}
				{@const h = (fog.bottomRight.row - fog.topLeft.row + 1) * T}
				<rect
					{x} {y} {w} {h}
					fill="#050d1a"
					opacity={lifted ? 0 : 0.88}
					style="transition: opacity 1.8s ease-out; pointer-events: none;"
				/>
				<!-- Fog texture (only when visible) -->
				{#if !lifted}
					<rect
						{x} {y} {w} {h}
						fill="none"
						stroke="#1e3a5c"
						stroke-width="1"
						opacity="0.3"
						style="pointer-events: none;"
					/>
					<!-- "???" text in fog centre -->
					<text
						x={x + w / 2} y={y + h / 2}
						text-anchor="middle"
						dominant-baseline="middle"
						font-size={T * 0.65}
						fill="#1e3a5c"
						font-weight="700"
						style="pointer-events: none;"
					>???</text>
				{/if}
			{/each}
		</svg>
	</div>

	<!-- Zoom controls -->
	<div class="zoom-controls" role="group" aria-label="Map zoom controls">
		<button class="zoom-btn" onclick={() => { scale = Math.min(MAX_SCALE, scale * 1.25); clampPan(); }} aria-label="Zoom in">+</button>
		<button class="zoom-btn" onclick={() => { scale = Math.max(MIN_SCALE, scale * 0.8); clampPan(); }} aria-label="Zoom out">−</button>
		<button class="zoom-btn" onclick={() => { scale = 1; if (playerMarkerNode) { const cw = containerEl!.clientWidth; const ch = containerEl!.clientHeight; panX = cw/2 - cx(playerMarkerNode.tile.col)*scale; panY = ch/2 - cy(playerMarkerNode.tile.row)*scale; clampPan(); } }} aria-label="Reset view" title="Re-center">⌖</button>
	</div>
</div>

<!-- Tooltip -->
{#if tooltip}
	<div class="map-tooltip" style="left:{tooltip.x + 12}px; top:{tooltip.y - 8}px">
		{tooltip.text}
	</div>
{/if}

<!-- Collectible popup -->
{#if activeCollectible}
	<MapCollectiblePopup
		collectible={activeCollectible}
		onCollect={() => onCollectibleCollect(activeCollectible!)}
		onClose={() => { activeCollectible = null; }}
	/>
{/if}

<style>
	.map-container {
		position: relative;
		width: 100%;
		height: 100%;
		overflow: hidden;
		background: #030810;
		cursor: grab;
		user-select: none;
		touch-action: none;
	}
	.map-container::after {
		content: '';
		position: absolute;
		inset: 0;
		background: radial-gradient(ellipse 80% 80% at 50% 50%, transparent 40%, rgba(2, 5, 14, 0.7) 100%);
		pointer-events: none;
		z-index: 1;
	}
	.map-container:active {
		cursor: grabbing;
	}

	.map-canvas {
		position: absolute;
		top: 0;
		left: 0;
		will-change: transform;
	}

	/* Level nodes */
	:global(.level-node) {
		cursor: default;
	}
	:global(.level-node.unlocked) {
		cursor: pointer;
	}
	:global(.level-node.unlocked:hover circle:first-child) {
		opacity: 0.9;
		filter: brightness(1.15);
	}

	/* Collectibles */
	:global(.collectible) {
		cursor: pointer;
	}
	:global(.collectible.collected) {
		cursor: default;
		opacity: 0.45;
	}

	/* Gates */
	:global(.gate) {
		cursor: pointer;
	}
	:global(.gate.open) {
		opacity: 0.6;
		pointer-events: none;
	}

	/* Portal */
	:global(.portal-node) {
		cursor: pointer;
	}

	/* Zoom controls */
	.zoom-controls {
		position: absolute;
		bottom: 1.5rem;
		right: 1.5rem;
		display: flex;
		flex-direction: column;
		gap: 0.4rem;
		z-index: 10;
	}

	.zoom-btn {
		width: 2.4rem;
		height: 2.4rem;
		border-radius: 50%;
		border: 1px solid rgba(56, 189, 248, 0.3);
		background: rgba(10, 22, 40, 0.9);
		color: #93c5fd;
		font-size: 1.2rem;
		font-weight: 700;
		line-height: 1;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: background 0.15s, border-color 0.15s;
		backdrop-filter: blur(4px);
	}
	.zoom-btn:hover {
		background: rgba(56, 189, 248, 0.15);
		border-color: #38bdf8;
		color: #e0f2fe;
	}

	/* Tooltip */
	.map-tooltip {
		position: fixed;
		z-index: 100;
		background: rgba(10, 22, 40, 0.95);
		border: 1px solid rgba(56, 189, 248, 0.35);
		color: #e2f4ff;
		font-size: 0.8rem;
		padding: 0.4rem 0.7rem;
		border-radius: 6px;
		pointer-events: none;
		backdrop-filter: blur(6px);
		max-width: 220px;
		box-shadow: 0 4px 16px rgba(0,0,0,0.5);
	}
</style>
