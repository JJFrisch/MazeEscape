<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { WORLD_THEMES } from '$lib/worldThemes';

	const worlds = getAllWorlds();
</script>

<svelte:head>
	<title>MazeEscape – Procedural Puzzle Mazes</title>
</svelte:head>

<div class="landing">

	<!-- ── Hero ────────────────────────────────────── -->
	<section
		class="hero"
		style="background-image: url('{base}/images/background_maze_3.png')"
	>
		<div class="hero-overlay"></div>
		<div class="hero-content">
			<span class="hero-badge">Procedural Puzzle Game</span>
			<h1 class="hero-title">
				<span class="title-maze">Maze</span><span class="title-escape">Escape</span>
			</h1>
			<p class="hero-subtitle">
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
			{#each WORLD_THEMES as theme}
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
		</a>
	</section>

	{#if gameStore.player.playerName === 'Player'}
		<div class="welcome-prompt">
			<p>Welcome to MazeEscape! Set your name in <a href="{base}/settings">Settings</a> to get started.</p>
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

	/* ── Hero ─────────────────────────────────────── */
	.hero {
		position: relative;
		min-height: 480px;
		display: flex;
		align-items: center;
		justify-content: center;
		background-size: cover;
		background-position: center;
		background-repeat: no-repeat;
		padding: var(--space-16) var(--space-6);
	}

	.hero-overlay {
		position: absolute;
		inset: 0;
		background: linear-gradient(
			160deg,
			rgba(6, 16, 38, 0.80) 0%,
			rgba(3, 10, 26, 0.65) 60%,
			rgba(2, 8, 20, 0.55) 100%
		);
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
		background: rgba(56, 189, 248, 0.18);
		border: 1px solid rgba(56, 189, 248, 0.45);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		color: #93c5fd;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		margin-bottom: var(--space-4);
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

	.title-escape {
		color: #f8fafc;
		text-shadow: 0 2px 20px rgba(0, 0, 0, 0.6);
	}

	.hero-subtitle {
		color: #cbd5e1;
		font-size: var(--text-lg);
		max-width: 480px;
		margin: 0 auto var(--space-8);
		line-height: 1.6;
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
		min-height: 280px;
		display: flex;
		flex-direction: column;
		justify-content: flex-end;
		border: 1px solid rgba(255, 255, 255, 0.10);
		transition: transform var(--transition-base), box-shadow var(--transition-base);
	}

	.world-card:not(.locked):hover {
		transform: translateY(-6px);
		box-shadow: 0 12px 48px var(--world-accent-dim, rgba(56,189,248,0.22));
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
	}

	.lock-label {
		font-size: var(--text-sm);
		color: #94a3b8;
		text-align: center;
		padding: 0 var(--space-4);
	}

	.world-card-inner {
		position: relative;
		z-index: 1;
		padding: var(--space-6);
		padding-top: var(--space-16);
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.world-tag {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: rgba(255, 255, 255, 0.55);
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
		transition: background var(--transition-fast), transform var(--transition-fast);
		backdrop-filter: blur(4px);
		align-self: flex-start;
	}

	.world-play-btn:hover {
		background: rgba(255, 255, 255, 0.16);
		transform: translateX(4px);
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
