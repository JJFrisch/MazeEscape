<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';

	const worlds = getAllWorlds();

	// Live stats for each zone
	const totalStars = $derived(
		worlds.reduce((s, w) => s + gameStore.getWorldStarCount(w.worldId), 0)
	);
	const maxStars = $derived(worlds.reduce((s, w) => s + w.levels.length * 3, 0));

	const dailyStreak = $derived(gameStore.player.currentStreak ?? 0);
	const coins = $derived(gameStore.player.coinCount);

	// Daily locked until level 10
	const dailyLocked = $derived(
		worlds[0]?.levels.find(l => l.levelNumber === '10')
			? !gameStore.getLevelProgress(1, '10')?.completed
			: false
	);

	function handleZoneClick(e: MouseEvent, href: string, locked: boolean) {
		if (locked) return;
		// Don't navigate if user clicked a secondary link
		if ((e.target as HTMLElement).closest('.secondary-link')) return;
		goto(href);
	}

	type SubLink = { label: string; href: string };
	type Zone = {
		id: string;
		name: string;
		subtitle: string;
		glyph: string;
		color: string;
		colorDim: string;
		href: string;
		stat: string;
		description: string;
		secondary?: SubLink[];
		locked?: boolean;
		lockReason?: string;
	};

	const zones: Zone[] = $derived([
		{
			id: 'dungeon',
			name: 'The Dungeon',
			subtitle: 'Campaign',
			glyph: '⚔',
			color: '#38bdf8',
			colorDim: 'rgba(56,189,248,0.13)',
			href: `${base}/campaign/worlds`,
			stat: `${totalStars} / ${maxStars} ★`,
			description: 'Navigate procedural labyrinths across 3 worlds',
		},
		{
			id: 'spire',
			name: 'The Spire',
			subtitle: 'Daily Maze',
			glyph: '◉',
			color: '#c084fc',
			colorDim: 'rgba(192,132,252,0.13)',
			href: `${base}/daily`,
			stat: dailyStreak > 0 ? `🔥 ${dailyStreak} day streak` : 'No streak yet',
			description: 'A fresh labyrinth generated every 24 hours',
			locked: dailyLocked,
			lockReason: 'Complete Level 10 to unlock',
		},
		{
			id: 'arena',
			name: 'The Arena',
			subtitle: 'Leaderboard',
			glyph: '◈',
			color: '#f59e0b',
			colorDim: 'rgba(245,158,11,0.13)',
			href: `${base}/leaderboard`,
			stat: 'Global rankings',
			description: 'Compete for the top of the board',
		},
		{
			id: 'vault',
			name: 'The Vault',
			subtitle: 'Shop & Equip',
			glyph: '◆',
			color: '#34d399',
			colorDim: 'rgba(52,211,153,0.13)',
			href: `${base}/shop`,
			stat: `${coins.toLocaleString()} coins`,
			description: 'Powerups, skins, and trail effects',
			secondary: [{ label: 'Equip', href: `${base}/equip` }],
		},
		{
			id: 'archives',
			name: 'The Archives',
			subtitle: 'Knowledge',
			glyph: '≡',
			color: '#22d3ee',
			colorDim: 'rgba(34,211,238,0.13)',
			href: `${base}/codex`,
			stat: 'Lore & mastery',
			description: 'Codex of worlds, algorithm lore & chronicles',
			secondary: [
				{ label: 'Algorithms', href: `${base}/algorithms` },
				{ label: 'Chronicles', href: `${base}/stats` },
			],
		},
		{
			id: 'sanctum',
			name: 'The Sanctum',
			subtitle: 'Settings & Account',
			glyph: '⬡',
			color: '#a78bfa',
			colorDim: 'rgba(167,139,250,0.13)',
			href: `${base}/settings`,
			stat: authStore.user ? (authStore.user.email?.split('@')[0] ?? 'Signed in') : 'Sign In',
			description: 'Configure your journey and sync your progress',
			secondary: [{ label: 'Account', href: `${base}/auth` }],
		},
	]);

	// 7 corridors connecting the 6 zones
	type Corridor = {
		dir: 'h' | 'v';
		gridCol: number;
		gridRow: number;
		c1: string;
		c2: string;
	};

	const corridors: Corridor[] = $derived([
		{ dir: 'h', gridCol: 2, gridRow: 1, c1: zones[0].color, c2: zones[1].color },
		{ dir: 'h', gridCol: 4, gridRow: 1, c1: zones[1].color, c2: zones[2].color },
		{ dir: 'h', gridCol: 2, gridRow: 3, c1: zones[3].color, c2: zones[4].color },
		{ dir: 'h', gridCol: 4, gridRow: 3, c1: zones[4].color, c2: zones[5].color },
		{ dir: 'v', gridCol: 1, gridRow: 2, c1: zones[0].color, c2: zones[3].color },
		{ dir: 'v', gridCol: 3, gridRow: 2, c1: zones[1].color, c2: zones[4].color },
		{ dir: 'v', gridCol: 5, gridRow: 2, c1: zones[2].color, c2: zones[5].color },
	]);

	const roomPositions = [
		{ gridCol: 1, gridRow: 1 },
		{ gridCol: 3, gridRow: 1 },
		{ gridCol: 5, gridRow: 1 },
		{ gridCol: 1, gridRow: 3 },
		{ gridCol: 3, gridRow: 3 },
		{ gridCol: 5, gridRow: 3 },
	];
</script>

<svelte:head>
	<title>Maze Escape: Pathbound — World Map</title>
</svelte:head>

<div class="map-shell">

	<!-- Ambient background -->
	<div class="map-bg" aria-hidden="true">
		<div class="map-bg-grid"></div>
		{#each zones as zone}
			<div
				class="map-bg-bloom zone-bloom-{zone.id}"
				style="--bloom-color: {zone.colorDim};"
			></div>
		{/each}
	</div>

	<!-- Title row -->
	<div class="map-title-row" aria-hidden="true">
		<span class="map-label">WORLD MAP</span>
		<span class="map-divider">◇</span>
		<span class="map-sub">Select a zone to enter</span>
	</div>

	<!-- ── The map ─────────────────────────────────────────────── -->
	<div class="map-grid" role="navigation" aria-label="Game zones">

		{#each zones as zone, i}
			{@const pos = roomPositions[i]}
			<!-- Using div + onclick to avoid nested <a> hydration issues; main link is the enter button -->
			<div
				class="zone-room"
				class:zone-locked={zone.locked}
				style="grid-column:{pos.gridCol};grid-row:{pos.gridRow};--zone-color:{zone.color};--zone-dim:{zone.colorDim};"
				role="button"
				tabindex={zone.locked ? -1 : 0}
				aria-label="{zone.name} — {zone.subtitle}"
				onclick={(e) => handleZoneClick(e, zone.href, zone.locked ?? false)}
				onkeydown={(e) => { if (e.key === 'Enter' || e.key === ' ') handleZoneClick(e as unknown as MouseEvent, zone.href, zone.locked ?? false); }}
			>
				<div class="room-glow" aria-hidden="true"></div>
				<div class="room-corner room-corner-tl" aria-hidden="true"></div>
				<div class="room-corner room-corner-tr" aria-hidden="true"></div>
				<div class="room-corner room-corner-bl" aria-hidden="true"></div>
				<div class="room-corner room-corner-br" aria-hidden="true"></div>

				{#if zone.locked}
					<div class="room-lock-overlay" aria-hidden="true">
						<span class="lock-icon">🔒</span>
						<span class="lock-reason">{zone.lockReason}</span>
					</div>
				{/if}

				<div class="room-content">
					<div class="room-glyph" aria-hidden="true">{zone.glyph}</div>
					<div class="room-name">{zone.name}</div>
					<div class="room-subtitle">{zone.subtitle}</div>
					<div class="room-sep" aria-hidden="true"></div>
					<div class="room-stat">{zone.stat}</div>
					<p class="room-desc">{zone.description}</p>
					{#if zone.secondary}
						<div class="room-secondary" role="group">
							{#each zone.secondary as link}
								<a
									href={link.href}
									class="secondary-link"
									onclick={(e) => e.stopPropagation()}
									aria-label="{link.label} section"
								>{link.label}</a>
							{/each}
						</div>
					{/if}
				</div>

				{#if !zone.locked}
					<div class="room-enter" aria-hidden="true">
						<svg width="12" height="12" viewBox="0 0 16 16" fill="none">
							<path d="M3 8h10M9 4l4 4-4 4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
						</svg>
					</div>
				{/if}
			</div>
		{/each}

		<!-- Corridors -->
		{#each corridors as corr}
			<div
				class="corridor corridor-{corr.dir}"
				style="grid-column:{corr.gridCol};grid-row:{corr.gridRow};--c1:{corr.c1};--c2:{corr.c2};"
				aria-hidden="true"
			>
				<div class="corr-line"></div>
				<div class="corr-dot corr-dot-a"></div>
				<div class="corr-dot corr-dot-b"></div>
				<div class="corr-dot corr-dot-c"></div>
			</div>
		{/each}

	</div>

	<!-- Footer row -->
	<div class="map-footer-row">
		<a href="{base}/download" class="download-cta">
			<svg width="11" height="11" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
				<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
			</svg>
			Desktop App
		</a>
		<span class="map-footer-sep">·</span>
		<a href="{base}/privacy" class="map-footer-link">Privacy</a>
		<span class="map-footer-sep">·</span>
		<a href="{base}/terms" class="map-footer-link">Terms</a>
		<span class="map-footer-credit">© 2026 Jake Frischmann</span>
	</div>

</div>

<style>
	/* ── Shell ────────────────────────────────────────────────── */
	.map-shell {
		position: relative;
		width: 100%;
		min-height: calc(100dvh - var(--header-height));
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		overflow: hidden;
		padding: 16px 20px 10px;
		box-sizing: border-box;
		background: var(--color-bg-primary);
	}

	/* ── Background ───────────────────────────────────────────── */
	.map-bg {
		position: absolute;
		inset: 0;
		pointer-events: none;
		z-index: 0;
	}
	.map-bg-grid {
		position: absolute;
		inset: 0;
		background-image:
			linear-gradient(var(--app-grid-line-color) 1px, transparent 1px),
			linear-gradient(90deg, var(--app-grid-line-color) 1px, transparent 1px);
		background-size: 40px 40px;
		opacity: 0.7;
	}
	.map-bg-bloom {
		position: absolute;
		width: 380px;
		height: 380px;
		border-radius: 50%;
		background: radial-gradient(ellipse, var(--bloom-color) 0%, transparent 70%);
		filter: blur(70px);
		animation: bloom-pulse 7s ease-in-out infinite;
	}
	.zone-bloom-dungeon  { top: -60px;  left: -60px;  animation-delay: 0s; }
	.zone-bloom-spire    { top: -60px;  left: calc(50% - 190px); animation-delay: 1.2s; }
	.zone-bloom-arena    { top: -60px;  right: -60px; animation-delay: 2.4s; }
	.zone-bloom-vault    { bottom: -60px; left: -60px;  animation-delay: 3.6s; }
	.zone-bloom-archives { bottom: -60px; left: calc(50% - 190px); animation-delay: 4.8s; }
	.zone-bloom-sanctum  { bottom: -60px; right: -60px; animation-delay: 6s; }

	@keyframes bloom-pulse {
		0%, 100% { opacity: 0.4; transform: scale(1); }
		50%       { opacity: 0.65; transform: scale(1.08); }
	}

	/* ── Title ──────────────────────────────────────────────────── */
	.map-title-row {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		gap: 10px;
		margin-bottom: 14px;
	}
	.map-label {
		font-family: var(--font-display);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.20em;
		color: var(--color-text-muted);
		text-transform: uppercase;
	}
	.map-divider { color: var(--color-accent-primary); font-size: 9px; opacity: 0.5; }
	.map-sub { font-size: var(--text-xs); color: var(--color-text-muted); }

	/* ── Map grid ─────────────────────────────────────────────── */
	.map-grid {
		position: relative;
		z-index: 1;
		display: grid;
		grid-template-columns: 1fr 72px 1fr 72px 1fr;
		grid-template-rows: 1fr 60px 1fr;
		width: 100%;
		max-width: 1060px;
		flex: 1;
		max-height: 600px;
		min-height: 380px;
	}

	/* ── Zone rooms ───────────────────────────────────────────── */
	.zone-room {
		position: relative;
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		justify-content: flex-end;
		padding: 18px 20px 16px;
		background: rgba(5, 10, 28, 0.93);
		border: 1px solid color-mix(in srgb, var(--zone-color) 20%, transparent);
		border-radius: var(--radius-lg);
		color: var(--color-text-primary);
		overflow: hidden;
		cursor: pointer;
		user-select: none;
		transition: border-color 0.22s ease, box-shadow 0.22s ease, transform 0.2s ease;
		outline: none;
	}
	.zone-room:focus-visible {
		box-shadow: 0 0 0 2px var(--zone-color);
	}
	.zone-room:hover:not(.zone-locked),
	.zone-room:focus-visible:not(.zone-locked) {
		border-color: color-mix(in srgb, var(--zone-color) 52%, transparent);
		box-shadow:
			0 0 42px color-mix(in srgb, var(--zone-color) 16%, transparent),
			inset 0 0 56px color-mix(in srgb, var(--zone-color) 5%, transparent);
		transform: scale(1.012);
	}
	.zone-locked { cursor: not-allowed; opacity: 0.4; pointer-events: none; }

	/* Glow layer */
	.room-glow {
		position: absolute;
		inset: 0;
		background: radial-gradient(ellipse at 40% 85%, var(--zone-dim) 0%, transparent 60%);
		opacity: 0.65;
		transition: opacity 0.22s ease;
		pointer-events: none;
	}
	.zone-room:hover .room-glow { opacity: 1; }

	/* Corner accents */
	.room-corner {
		position: absolute;
		width: 8px;
		height: 8px;
		border-color: color-mix(in srgb, var(--zone-color) 38%, transparent);
		border-style: solid;
		border-width: 0;
		transition: border-color 0.22s ease;
	}
	.zone-room:hover .room-corner {
		border-color: color-mix(in srgb, var(--zone-color) 70%, transparent);
	}
	.room-corner-tl { top: 9px;    left: 9px;   border-top-width: 1.5px; border-left-width: 1.5px;   border-radius: 2px 0 0 0; }
	.room-corner-tr { top: 9px;    right: 9px;  border-top-width: 1.5px; border-right-width: 1.5px;  border-radius: 0 2px 0 0; }
	.room-corner-bl { bottom: 9px; left: 9px;   border-bottom-width: 1.5px; border-left-width: 1.5px;  border-radius: 0 0 0 2px; }
	.room-corner-br { bottom: 9px; right: 9px;  border-bottom-width: 1.5px; border-right-width: 1.5px; border-radius: 0 0 2px 0; }

	/* Lock overlay */
	.room-lock-overlay {
		position: absolute;
		inset: 0;
		z-index: 10;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: 6px;
		background: rgba(3, 6, 18, 0.65);
		border-radius: inherit;
		backdrop-filter: blur(2px);
	}
	.lock-icon  { font-size: 1.5rem; }
	.lock-reason { font-size: var(--text-xs); color: var(--color-text-muted); text-align: center; padding: 0 12px; }

	/* Room content */
	.room-content { position: relative; z-index: 2; width: 100%; }

	.room-glyph {
		font-size: 1.55rem;
		line-height: 1;
		color: var(--zone-color);
		margin-bottom: 9px;
		font-family: var(--font-display);
		filter: drop-shadow(0 0 8px color-mix(in srgb, var(--zone-color) 55%, transparent));
		transition: filter 0.22s ease;
	}
	.zone-room:hover .room-glyph {
		filter: drop-shadow(0 0 16px var(--zone-color));
	}

	.room-name {
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 700;
		color: var(--color-text-primary);
		line-height: 1.1;
	}
	.room-subtitle {
		font-size: 10px;
		font-weight: 600;
		color: var(--zone-color);
		letter-spacing: 0.10em;
		text-transform: uppercase;
		margin-top: 2px;
		opacity: 0.8;
	}
	.room-sep {
		width: 24px;
		height: 1px;
		background: color-mix(in srgb, var(--zone-color) 32%, transparent);
		margin: 9px 0;
	}
	.room-stat {
		font-family: var(--font-display);
		font-size: var(--text-sm);
		font-weight: 600;
		color: var(--color-text-primary);
		margin-bottom: 3px;
	}
	.room-desc {
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
		line-height: 1.45;
		margin: 0 0 9px;
		opacity: 0.75;
	}

	/* Secondary links */
	.room-secondary { display: flex; flex-wrap: wrap; gap: 5px; margin-top: 1px; }
	.secondary-link {
		display: inline-flex;
		align-items: center;
		padding: 2px 8px;
		background: color-mix(in srgb, var(--zone-color) 10%, transparent);
		border: 1px solid color-mix(in srgb, var(--zone-color) 26%, transparent);
		border-radius: var(--radius-full);
		font-size: 10px;
		font-weight: 600;
		color: color-mix(in srgb, var(--zone-color) 80%, white 20%);
		text-decoration: none;
		letter-spacing: 0.03em;
		transition: all 0.14s ease;
		pointer-events: all;
	}
	.secondary-link:hover {
		background: color-mix(in srgb, var(--zone-color) 18%, transparent);
		border-color: color-mix(in srgb, var(--zone-color) 52%, transparent);
	}

	/* Enter arrow */
	.room-enter {
		position: absolute;
		top: 14px;
		right: 14px;
		z-index: 2;
		color: color-mix(in srgb, var(--zone-color) 45%, transparent);
		opacity: 0;
		transform: translateX(-5px);
		transition: opacity 0.18s ease, transform 0.18s ease, color 0.18s ease;
	}
	.zone-room:hover .room-enter {
		opacity: 1;
		transform: translateX(0);
		color: var(--zone-color);
	}

	/* ── Corridors ────────────────────────────────────────────── */
	.corridor {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	.corr-line {
		position: absolute;
		border-radius: 2px;
	}
	.corridor-h .corr-line {
		width: 100%;
		height: 2px;
		background: linear-gradient(90deg,
			color-mix(in srgb, var(--c1) 70%, transparent),
			color-mix(in srgb, var(--c2) 70%, transparent)
		);
		box-shadow: 0 0 6px color-mix(in srgb, var(--c1) 30%, var(--c2) 30%);
		opacity: 0.55;
	}
	.corridor-v .corr-line {
		width: 2px;
		height: 100%;
		background: linear-gradient(180deg,
			color-mix(in srgb, var(--c1) 70%, transparent),
			color-mix(in srgb, var(--c2) 70%, transparent)
		);
		box-shadow: 0 0 6px color-mix(in srgb, var(--c1) 30%, var(--c2) 30%);
		opacity: 0.55;
	}

	/* Animated travel dots */
	.corr-dot {
		position: absolute;
		width: 4px;
		height: 4px;
		border-radius: 50%;
		background: white;
		opacity: 0;
	}
	.corridor-h .corr-dot {
		top: 50%;
		transform: translateY(-50%);
		animation: travel-h 3.5s linear infinite;
	}
	.corridor-v .corr-dot {
		left: 50%;
		transform: translateX(-50%);
		animation: travel-v 3.5s linear infinite;
	}
	.corr-dot-a { animation-delay: 0s; }
	.corr-dot-b { animation-delay: 1.17s; }
	.corr-dot-c { animation-delay: 2.33s; }

	@keyframes travel-h {
		0%   { left: 0%;   opacity: 0; }
		8%   { opacity: 0.65; }
		92%  { opacity: 0.65; }
		100% { left: 100%; opacity: 0; }
	}
	@keyframes travel-v {
		0%   { top: 0%;   opacity: 0; }
		8%   { opacity: 0.65; }
		92%  { opacity: 0.65; }
		100% { top: 100%; opacity: 0; }
	}

	/* ── Map footer ───────────────────────────────────────────── */
	.map-footer-row {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		gap: 10px;
		margin-top: 12px;
		padding-top: 10px;
		border-top: 1px solid var(--color-border-subtle);
		width: 100%;
		max-width: 1060px;
	}
	.download-cta {
		display: inline-flex;
		align-items: center;
		gap: 5px;
		padding: 4px 11px;
		background: rgba(245, 158, 11, 0.09);
		border: 1px solid rgba(245, 158, 11, 0.28);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 600;
		color: var(--color-accent-gold);
		text-decoration: none;
		transition: all 0.14s ease;
	}
	.download-cta:hover {
		background: rgba(245, 158, 11, 0.16);
		border-color: rgba(245, 158, 11, 0.5);
	}
	.map-footer-sep { color: var(--color-text-muted); font-size: var(--text-xs); opacity: 0.35; }
	.map-footer-link {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		text-decoration: none;
		transition: color 0.14s ease;
	}
	.map-footer-link:hover { color: var(--color-accent-primary); }
	.map-footer-credit {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		opacity: 0.45;
		margin-left: auto;
	}

	/* ── Responsive ───────────────────────────────────────────── */
	@media (max-width: 860px) {
		.map-grid {
			grid-template-columns: 1fr 52px 1fr;
			grid-template-rows: repeat(6, minmax(120px, auto));
			max-height: none;
			min-height: 0;
		}
		/* Re-place rooms into 2-column layout (override inline styles) */
		.zone-room:nth-child(1) { grid-column: 1 !important; grid-row: 1 !important; }
		.zone-room:nth-child(2) { grid-column: 3 !important; grid-row: 1 !important; }
		.zone-room:nth-child(3) { grid-column: 1 !important; grid-row: 3 !important; }
		.zone-room:nth-child(4) { grid-column: 3 !important; grid-row: 3 !important; }
		.zone-room:nth-child(5) { grid-column: 1 !important; grid-row: 5 !important; }
		.zone-room:nth-child(6) { grid-column: 3 !important; grid-row: 5 !important; }
		/* Show only the 3 pair-connecting corridors */
		.corridor { display: none !important; }
		/* corridor[0] = dungeon↔spire = nth-child(7), place at row 1 */
		.corridor:nth-child(7)  { display: flex !important; grid-column: 2 !important; grid-row: 1 !important; }
		/* reuse corridor[2] = vault↔archives, repurpose at row 3 (arena↔vault pair) */
		.corridor:nth-child(9)  { display: flex !important; grid-column: 2 !important; grid-row: 3 !important; }
		/* reuse corridor[3] = archives↔sanctum, repurpose at row 5 */
		.corridor:nth-child(10) { display: flex !important; grid-column: 2 !important; grid-row: 5 !important; }
	}

	@media (max-width: 520px) {
		.map-shell { padding: 10px 10px 8px; }
		.map-grid {
			grid-template-columns: 1fr;
			grid-template-rows: auto;
			gap: 8px;
			max-height: none;
		}
		.zone-room { grid-column: 1 !important; }
		.corridor  { display: none !important; }
		.room-glyph { font-size: 1.25rem; margin-bottom: 6px; }
		.map-footer-row { flex-wrap: wrap; gap: 8px; }
		.map-footer-credit { margin-left: 0; }
	}
</style>
