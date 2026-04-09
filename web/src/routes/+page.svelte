<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { WORLD_THEMES } from '$lib/worldThemes';

	const worlds = getAllWorlds();
</script>

<svelte:head>
	<title>Maze Escape: Pathbound – Procedural Puzzle Mazes</title>
</svelte:head>

<div class="landing">

	<!-- ── Hero ────────────────────────────────────── -->
	<section
		class="hero"
		style="background-image: url('{base}/images/background_maze_3.png')"
	>
		<div class="hero-overlay"></div>
		<div class="hero-hex-grid"></div>
		<div class="hero-scanlines"></div>
		<div class="hero-content">
			<span class="hero-badge">Procedural Puzzle Game <span class="cursor-blink">|</span></span>
			<h1 class="hero-title">
				<span class="title-maze glitch-text" data-text="Maze">Maze</span><span class="title-escape"> Escape</span>
				<span class="title-pathbound">Pathbound</span>
			</h1>
			<p class="hero-byline">A game by <a href="https://jakefrischman.me" target="_blank" rel="noopener noreferrer" class="hero-byline-link">Jake Frischmann</a></p>
			<p class="hero-subtitle typewriter">
				Navigate procedurally generated labyrinths across worlds, collect stars, and prove your puzzle mastery.
			</p>
			<div class="hero-actions">
				<a href="{base}/campaign/worlds" class="btn btn-primary btn-large">
					Play Campaign
				</a>
				<a
					href="{base}/daily"
					class="btn btn-secondary btn-large"
					class:btn-locked={!gameStore.isDailyMazeUnlocked()}
				>
					Daily Maze
					{#if !gameStore.isDailyMazeUnlocked()}
						<span class="lock-badge">🔒 Beat Level 10</span>
					{/if}
				</a>
			</div>
		</div>
	</section>

	<!-- ── World preview strip ──────────────────────── -->
	<section class="worlds-strip">
		<h2 class="section-heading">Choose Your World</h2>
		<div class="worlds-row">
			{#each WORLD_THEMES as theme, i}
				{@const worldDef = worlds.find((w) => w.worldId === theme.worldId)}
				{@const stars = gameStore.getWorldStarCount(theme.worldId)}
				{@const isLocked = worldDef?.locked ?? true}
				{@const hasLevels = (worldDef?.levels?.length ?? 0) > 0}

				<div
					class="world-card"
					class:locked={isLocked}
					style="
						--world-accent: {theme.accentColor};
						--world-accent-dim: {theme.accentDim};
						--i: {i};
						{theme.bgImageFile
							? `background-image: ${theme.overlayGradient}, url('${base}/images/${theme.bgImageFile}');`
							: `background: ${theme.overlayGradient.replace('linear-gradient', '').replace(/\(.*?\)/, '(135deg, #042014, #064424)').replace('linear-gradient', 'linear-gradient')};`
					}
					"
				>
					{#if isLocked}
						<div class="world-lock-overlay">
							<img src="{base}/images/lock.png" alt="Locked" class="lock-icon-img"/>
							<span class="lock-label">Complete previous world</span>
						</div>
					{/if}

					<div class="world-card-inner">
						<span class="world-tag">World {theme.worldId}</span>
						<h3 class="world-name">{theme.name}</h3>
						<p class="world-tagline" style="color: {theme.textAccent}">{theme.tagline}</p>

						<div class="world-meta">
							<span class="world-stat">
								<img src="{base}/images/full_star.png" alt="Stars" class="mini-star" />
								{stars} stars
							</span>
							{#if worldDef}
								<span class="world-stat">{worldDef.numberOfLevels} levels</span>
							{/if}
						</div>

						{#if !isLocked && hasLevels}
							<a href="{base}/campaign/worlds/{theme.worldId}" class="world-play-btn"
								style="border-color: {theme.accentColor}; color: {theme.accentColor}">
								Enter World →
							</a>
						{:else if !hasLevels}
							<span class="coming-soon-badge">Coming Soon</span>
						{/if}
					</div>
				</div>
			{/each}
		</div>
	</section>

	<!-- ── Quick navigation ─────────────────────────── -->
	<section class="quick-nav">
		<a href="{base}/shop" class="nav-card">
			<img src="{base}/images/chest.png" alt="" class="nav-card-img" aria-hidden="true" />
			<span class="nav-card-label">Shop</span>
			<span class="nav-card-desc">Powerups & items</span>
		</a>
		<a href="{base}/equip" class="nav-card">
			<img src="{base}/images/key1.png" alt="" class="nav-card-img" aria-hidden="true" />
			<span class="nav-card-label">Equip</span>
			<span class="nav-card-desc">Skins & style</span>
		</a>
		<a href="{base}/settings" class="nav-card">
			<!-- Settings gear SVG -->
			<svg class="nav-card-svg" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2"/>
				<path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"
					  stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
			</svg>
			<span class="nav-card-label">Settings</span>
			<span class="nav-card-desc">Customize</span>
		</a>
		<a href="{base}/info" class="nav-card">
			<!-- Info icon SVG -->
			<svg class="nav-card-svg" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
				<path d="M12 8v1M12 11v5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
			</svg>
			<span class="nav-card-label">How to Play</span>
			<span class="nav-card-desc">Controls & rules</span>
		</a>		<a href="{base}/download" class="nav-card">
			<svg class="nav-card-svg" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
				<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
			</svg>
			<span class="nav-card-label">Download</span>
			<span class="nav-card-desc">Desktop app</span>
		</a>	</section>

	{#if gameStore.player.playerName === 'Player'}
		<div class="welcome-prompt">
			<p>Welcome to Maze Escape: Pathbound! Set your name in <a href="{base}/settings">Settings</a> to get started.</p>
		</div>
	{/if}
</div>

<style>
	.landing {
		/* break out of app-main's horizontal padding for full-bleed hero */
		margin: calc(-1 * var(--space-6));
		display: flex;
		flex-direction: column;
	}

	/* ── Keyframes ───────────────────────────────── */
	@keyframes glitch {
		0%, 92%, 100% { clip-path: none; transform: translateX(0); }
		93% { clip-path: polygon(0 20%, 100% 20%, 100% 40%, 0 40%); transform: translateX(-4px); color: #f0abfc; }
		94% { clip-path: polygon(0 60%, 100% 60%, 100% 80%, 0 80%); transform: translateX(4px); color: #67e8f9; }
		95% { clip-path: none; transform: translateX(0); color: #93c5fd; }
	}

	@keyframes type-in {
		from { width: 0; }
		to { width: 100%; }
	}

	@keyframes cursor-blink {
		0%, 49% { opacity: 1; }
		50%, 100% { opacity: 0; }
	}

	@keyframes ring-pulse {
		0% { transform: translate(-50%, -50%) scale(1); opacity: 0.6; }
		100% { transform: translate(-50%, -50%) scale(2.2); opacity: 0; }
	}

	@keyframes card-rise {
		from { transform: translateY(28px); opacity: 0; }
		to { transform: translateY(0); opacity: 1; }
	}

	/* ── Hero ─────────────────────────────────────── */
	.hero {
		position: relative;
		min-height: 520px;
		display: flex;
		align-items: center;
		justify-content: center;
		background-size: cover;
		background-position: center;
		background-repeat: no-repeat;
		padding: var(--space-16) var(--space-6);
		overflow: hidden;
	}

	.hero-overlay {
		position: absolute;
		inset: 0;
		background: linear-gradient(
			160deg,
			rgba(6, 16, 38, 0.85) 0%,
			rgba(3, 10, 26, 0.70) 60%,
			rgba(2, 8, 20, 0.60) 100%
		);
	}

	.hero-hex-grid {
		position: absolute;
		inset: 0;
		z-index: 0;
		background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='56' height='100' viewBox='0 0 56 100'%3E%3Cpath d='M28 0 L56 16 L56 48 L28 64 L0 48 L0 16 Z' fill='none' stroke='rgba(56,189,248,0.15)' stroke-width='0.8'/%3E%3Cpath d='M28 36 L56 52 L56 84 L28 100 L0 84 L0 52 Z' fill='none' stroke='rgba(56,189,248,0.15)' stroke-width='0.8'/%3E%3C/svg%3E");
		background-size: 56px 100px;
		animation: grid-drift 24s linear infinite;
		opacity: 0.5;
		pointer-events: none;
	}

	.hero-scanlines {
		position: absolute;
		inset: 0;
		z-index: 0;
		background: repeating-linear-gradient(
			0deg,
			transparent 0,
			transparent 3px,
			rgba(0, 0, 0, 0.06) 3px,
			rgba(0, 0, 0, 0.06) 4px
		);
		pointer-events: none;
	}

	.hero-content {
		position: relative;
		z-index: 1;
		text-align: center;
		max-width: 640px;
	}

	.hero-badge {
		display: inline-block;
		padding: var(--space-1) var(--space-3);
		background: rgba(56, 189, 248, 0.12);
		border: 1px solid rgba(56, 189, 248, 0.45);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		color: #93c5fd;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		margin-bottom: var(--space-4);
	}

	.cursor-blink {
		animation: cursor-blink 1s step-end infinite;
		font-weight: 400;
		margin-left: 2px;
		color: #38bdf8;
	}

	.hero-title {
		font-family: var(--font-display);
		font-size: clamp(3rem, 9vw, 5.5rem);
		font-weight: 700;
		letter-spacing: -0.03em;
		line-height: 1.05;
		margin-bottom: var(--space-5);
	}

	.title-maze {
		color: #93c5fd;
		text-shadow: 0 0 40px rgba(56, 189, 248, 0.5);
	}

	.glitch-text {
		position: relative;
		display: inline-block;
		animation: glitch 9s ease-in-out infinite;
	}

	.title-escape {
		color: #f8fafc;
		text-shadow: 0 2px 20px rgba(0, 0, 0, 0.6);
	}

	.title-pathbound {
		display: block;
		color: #d97706;
		text-shadow: 0 0 32px rgba(217, 119, 6, 0.5);
		font-size: 0.72em;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		line-height: 1.1;
	}

	.hero-byline {
		font-size: var(--text-sm);
		color: rgba(203, 213, 225, 0.55);
		letter-spacing: 0.04em;
		margin-bottom: var(--space-5);
	}

	.hero-byline-link {
		color: #d97706;
		text-decoration: none;
		transition: color var(--transition-fast);
	}

	.hero-byline-link:hover {
		color: #fbbf24;
		text-decoration: underline;
	}

	.hero-subtitle.typewriter {
		color: #cbd5e1;
		font-size: var(--text-lg);
		max-width: 480px;
		margin: 0 auto var(--space-8);
		line-height: 1.6;
		overflow: hidden;
		white-space: nowrap;
		width: 0;
		animation: type-in 2.2s steps(80, end) 0.5s forwards;
	}

	.hero-actions {
		display: flex;
		flex-wrap: wrap;
		gap: var(--space-4);
		justify-content: center;
	}

	/* ── Buttons ──────────────────────────────────── */
	.btn {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: var(--space-3) var(--space-6);
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-base);
		text-decoration: none;
		transition: all var(--transition-base);
		cursor: pointer;
		border: none;
		position: relative;
		overflow: visible;
	}

	.btn-large {
		padding: var(--space-4) var(--space-8);
		font-size: var(--text-lg);
	}

	.btn-primary {
		background: var(--color-accent-primary);
		color: #fff;
		box-shadow: 0 0 24px rgba(21, 101, 192, 0.45);
	}

	.btn-primary::after {
		content: '';
		position: absolute;
		top: 50%;
		left: 50%;
		width: 100%;
		height: 100%;
		border: 2px solid rgba(56, 189, 248, 0.6);
		border-radius: var(--radius-lg);
		transform: translate(-50%, -50%) scale(1);
		animation: ring-pulse 2.2s ease-out infinite;
		pointer-events: none;
	}

	.btn-primary:hover {
		background: var(--color-accent-secondary);
		box-shadow: 0 0 36px rgba(21, 101, 192, 0.65);
		transform: translateY(-2px);
		color: #fff;
	}

	.btn-secondary {
		background: rgba(255, 255, 255, 0.10);
		color: #f0f6ff;
		border: 1px solid rgba(255, 255, 255, 0.28);
		backdrop-filter: blur(4px);
	}

	.btn-secondary:hover {
		background: rgba(255, 255, 255, 0.18);
		color: #fff;
		transform: translateY(-2px);
	}

	.btn-locked {
		opacity: 0.55;
		cursor: default;
	}
	.btn-locked:hover { transform: none; }

	.lock-badge {
		font-size: var(--text-xs);
		color: #fca5a5;
		position: absolute;
		bottom: -20px;
		left: 50%;
		transform: translateX(-50%);
		white-space: nowrap;
	}

	/* ── Inner sections container ─────────────────── */
	.worlds-strip,
	.quick-nav,
	.welcome-prompt {
		padding: var(--space-10) var(--space-6);
	}

	/* ── Worlds strip ─────────────────────────────── */
	.section-heading {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: var(--space-6);
		text-align: center;
		letter-spacing: 0.04em;
	}

	.worlds-row {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
		gap: var(--space-5);
		max-width: 960px;
		margin: 0 auto;
	}

	.world-card {
		position: relative;
		border-radius: var(--radius-2xl);
		overflow: hidden;
		background-size: cover;
		background-position: center;
		min-height: 300px;
		display: flex;
		flex-direction: column;
		justify-content: flex-end;
		border: 1px solid rgba(255, 255, 255, 0.10);
		transition: transform var(--transition-base), box-shadow var(--transition-base), border-color var(--transition-base);
		animation: card-rise 0.55s cubic-bezier(0.22, 1, 0.36, 1) calc(var(--i, 0) * 120ms) both;
	}

	/* Shimmer sweep on hover */
	.world-card::before {
		content: '';
		position: absolute;
		inset: 0;
		z-index: 2;
		background: linear-gradient(
			115deg,
			rgba(255,255,255,0) 40%,
			rgba(255,255,255,0.09) 50%,
			rgba(255,255,255,0) 60%
		);
		background-size: 200% 100%;
		background-position: -200% 0;
		transition: background-position 0.6s ease;
		pointer-events: none;
		border-radius: var(--radius-2xl);
	}

	.world-card:not(.locked):hover::before {
		background-position: 200% 0;
	}

	.world-card:not(.locked):hover {
		transform: translateY(-8px);
		box-shadow: 0 16px 56px var(--world-accent-dim, rgba(56,189,248,0.22));
		border-color: var(--world-accent, #38bdf8);
	}

	.world-card.locked {
		filter: grayscale(0.4);
		opacity: 0.72;
	}

	.world-lock-overlay {
		position: absolute;
		inset: 0;
		z-index: 10;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		background: rgba(0, 0, 0, 0.55);
		border-radius: var(--radius-2xl);
	}

	.lock-icon-img {
		width: 40px;
		height: 40px;
		object-fit: contain;
		filter: drop-shadow(0 0 6px rgba(255,255,255,0.3));
		animation: float-y 3s ease-in-out infinite;
	}

	.lock-label {
		font-size: var(--text-sm);
		color: #94a3b8;
		text-align: center;
		padding: 0 var(--space-4);
	}

	.world-card-inner {
		position: relative;
		z-index: 3;
		padding: var(--space-6);
		padding-top: var(--space-16);
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.world-tag {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: rgba(255, 255, 255, 0.50);
	}

	.world-name {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: #f0f6ff;
		line-height: 1.2;
	}

	.world-tagline {
		font-size: var(--text-sm);
		line-height: 1.4;
		opacity: 0.9;
	}

	.world-meta {
		display: flex;
		gap: var(--space-3);
		align-items: center;
		margin-top: var(--space-1);
	}

	.world-stat {
		display: flex;
		align-items: center;
		gap: 4px;
		font-size: var(--text-sm);
		color: rgba(255, 255, 255, 0.65);
		font-weight: 500;
	}

	.mini-star {
		width: 14px;
		height: 14px;
		object-fit: contain;
	}

	.world-play-btn {
		display: inline-flex;
		align-items: center;
		margin-top: var(--space-3);
		padding: var(--space-2) var(--space-4);
		background: rgba(255, 255, 255, 0.08);
		border: 1px solid;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		text-decoration: none;
		transition: background var(--transition-fast), transform var(--transition-fast), box-shadow var(--transition-fast);
		backdrop-filter: blur(4px);
		align-self: flex-start;
	}

	.world-play-btn:hover {
		background: rgba(255, 255, 255, 0.16);
		transform: translateX(4px);
		box-shadow: 0 0 14px var(--world-accent-dim, rgba(56,189,248,0.25));
	}

	.coming-soon-badge {
		margin-top: var(--space-3);
		padding: var(--space-2) var(--space-4);
		background: rgba(255, 255, 255, 0.06);
		border: 1px solid rgba(255, 255, 255, 0.14);
		border-radius: var(--radius-lg);
		font-size: var(--text-xs);
		color: rgba(255, 255, 255, 0.45);
		font-style: italic;
		letter-spacing: 0.04em;
		align-self: flex-start;
	}

	/* World 3 fallback gradient (no bg image) */
	.world-card:nth-child(3) {
		background: linear-gradient(135deg, #042014 0%, #064424 50%, #031a10 100%);
	}

	/* ── Quick nav ────────────────────────────────── */
	.quick-nav {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
		gap: var(--space-4);
		max-width: 800px;
		margin: 0 auto;
	}

	.nav-card {
		position: relative;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-2);
		padding: var(--space-6) var(--space-4);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-xl);
		text-decoration: none;
		transition: all var(--transition-base);
		box-shadow: var(--shadow-sm);
		overflow: hidden;
	}

	/* Scan-line underline on hover */
	.nav-card::after {
		content: '';
		position: absolute;
		bottom: 0;
		left: 0;
		right: 0;
		height: 2px;
		background: var(--color-accent-primary);
		transform: scaleX(0);
		transform-origin: left center;
		transition: transform 0.22s ease;
	}

	.nav-card:hover::after {
		transform: scaleX(1);
	}

	.nav-card:hover {
		background: var(--color-bg-card-hover);
		border-color: var(--color-accent-primary);
		transform: translateY(-4px);
		box-shadow: var(--shadow-glow);
	}

	.nav-card-img {
		width: 44px;
		height: 44px;
		object-fit: contain;
		animation: float-y 3s ease-in-out infinite;
	}

	.nav-card-svg {
		width: 40px;
		height: 40px;
		color: var(--color-accent-primary);
		stroke: currentColor;
	}

	.nav-card-label {
		font-family: var(--font-display);
		font-weight: 700;
		color: var(--color-text-primary);
		font-size: var(--text-base);
	}

	.nav-card-desc {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
		text-align: center;
	}

	/* ── Welcome prompt ───────────────────────────── */
	.welcome-prompt {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border-active);
		border-radius: var(--radius-lg);
		text-align: center;
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		max-width: 480px;
		margin: 0 auto var(--space-6);
		padding: var(--space-4) var(--space-6);
	}

	.welcome-prompt a {
		color: var(--color-accent-primary);
		text-decoration: underline;
	}

	/* ── Responsive ─────────────────────────────── */
	@media (max-width: 480px) {
		.hero-subtitle.typewriter {
			white-space: normal;
			width: 100%;
			animation: none;
		}
		.hero-actions {
			flex-direction: column;
			align-items: center;
		}
		.btn-large {
			width: 100%;
			justify-content: center;
		}
		.worlds-strip,
		.quick-nav,
		.welcome-prompt {
			padding: var(--space-6) var(--space-4);
		}
	}
</style>
