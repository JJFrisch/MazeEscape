<script lang="ts">
	import { base } from '$app/paths';
	import { BOSS_RELICS } from '$lib/core/bossRelics';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { WORLD_THEMES } from '$lib/worldThemes';

	const worlds = getAllWorlds();
	const totalRelics = BOSS_RELICS.length;
	const ownedRelicCount = $derived(gameStore.player.specialItemIds.length);
	const latestRelic = $derived(
		BOSS_RELICS.find((relic) => relic.id === gameStore.player.latestSpecialItemId) ?? null
	);
</script>

<svelte:head>
	<title>Maze Escape: Pathbound – Procedural Puzzle Mazes</title>
</svelte:head>

<div class="landing">

	<!-- ── Hero ─────────────────────────────────── -->
	<section
		class="hero"
		style="background-image: url('{base}/images/background_maze_3.png')"
	>
		<!-- Layered atmosphere -->
		<div class="hero-overlay"></div>
		<div class="hero-nebula hero-nebula-1"></div>
		<div class="hero-nebula hero-nebula-2"></div>
		<div class="hero-hex-grid"></div>
		<div class="hero-scanlines"></div>

		<div class="hero-content">
			<div class="hero-badge">
				<span class="badge-dot"></span>
				Procedural Puzzle Game
			</div>

			<h1 class="hero-title">
				<span class="title-maze glitch-text" data-text="Maze">Maze</span><span class="title-escape">Escape</span>
				<span class="title-pathbound">Pathbound</span>
			</h1>

			<p class="hero-byline">A game by <a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer" class="hero-author">Jake Frischmann</a></p>

			<p class="hero-subtitle">
				Navigate procedurally generated labyrinths across worlds — collect stars, master algorithms, and prove your puzzle mastery.
			</p>

			<div class="hero-actions">
				<a href="{base}/campaign/worlds" class="btn-hero-primary">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<polygon points="6,3 20,12 6,21" fill="currentColor"/>
					</svg>
					Play Campaign
				</a>
				<a
					href="{base}/daily"
					class="btn-hero-secondary"
					class:btn-locked={!gameStore.isDailyMazeUnlocked()}
				>
					Daily Maze
					{#if !gameStore.isDailyMazeUnlocked()}
						<span class="lock-hint">🔒 Beat Level 10</span>
					{/if}
				</a>
			</div>

			<!-- Mini stats row -->
			<div class="hero-stats">
				<div class="hero-stat">
					<span class="hero-stat-val">{worlds.length}</span>
					<span class="hero-stat-lbl">Worlds</span>
				</div>
				<div class="hero-stat-divider"></div>
				<div class="hero-stat">
					<span class="hero-stat-val">327</span>
					<span class="hero-stat-lbl">Levels</span>
				</div>
				<div class="hero-stat-divider"></div>
				<div class="hero-stat">
					<span class="hero-stat-val">∞</span>
					<span class="hero-stat-lbl">Procedural</span>
				</div>
				<div class="hero-stat-divider"></div>
				<div class="hero-stat">
					<span class="hero-stat-val">{ownedRelicCount}/{totalRelics}</span>
					<span class="hero-stat-lbl">Relics</span>
				</div>
			</div>
		</div>

		<!-- Scroll indicator -->
		<div class="hero-scroll-hint" aria-hidden="true">
			<svg width="20" height="20" viewBox="0 0 24 24" fill="none">
				<path d="M7 10l5 5 5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
			</svg>
		</div>
	</section>

	<!-- ── World Cards ──────────────────────────── -->
	<section class="worlds-section">
		<div class="section-header">
			<h2 class="section-title">Choose Your World</h2>
			<p class="section-sub">Three worlds. Hundreds of levels. Infinite replay.</p>
		</div>

		<div class="worlds-grid">
			{#each WORLD_THEMES as theme, i}
				{@const worldDef = worlds.find((w) => w.worldId === theme.worldId)}
				{@const stars = gameStore.getWorldStarCount(theme.worldId)}
				{@const isLocked = worldDef?.locked ?? true}
				{@const hasLevels = (worldDef?.levels?.length ?? 0) > 0}

				<div
					class="world-card"
					class:locked={isLocked}
					style="
						--accent: {theme.accentColor};
						--accent-dim: {theme.accentDim};
						--i: {i};
						{theme.bgImageFile
							? `background-image: ${theme.overlayGradient}, url('${base}/images/${theme.bgImageFile}');`
							: `background: linear-gradient(135deg, #031410, #052a1a);`}
					"
				>
					<!-- Shimmer sweep -->
					<div class="card-shimmer" aria-hidden="true"></div>

					{#if isLocked}
						<div class="world-lock">
							<img src="{base}/images/lock.png" alt="Locked" class="lock-img"/>
							<span class="lock-text">Complete previous world</span>
						</div>
					{/if}

					<div class="world-card-body">
						<div class="world-number-tag">World {theme.worldId}</div>
						<h3 class="world-name">{theme.name}</h3>
						<p class="world-tagline" style="color: {theme.textAccent}">{theme.tagline}</p>

						<div class="world-stats">
							<span class="wstat">
								<img src="{base}/images/full_star.png" alt="" class="wstat-star"/>
								{stars} stars
							</span>
							{#if worldDef}
								<span class="wstat">{worldDef.numberOfLevels} levels</span>
							{/if}
						</div>

						{#if !isLocked && hasLevels}
							<a
								href="{base}/campaign/worlds/{theme.worldId}"
								class="world-btn"
								style="--btn-color: {theme.accentColor};"
							>
								Enter World
								<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
									<path d="M5 12h14M13 6l6 6-6 6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
								</svg>
							</a>
						{:else if !hasLevels}
							<span class="coming-badge">Coming Soon</span>
						{/if}
					</div>

					<!-- Bottom accent line -->
					<div class="card-accent-line" style="background: {theme.accentColor};" aria-hidden="true"></div>
				</div>
			{/each}
		</div>
	</section>

	<!-- ── Quick Nav ────────────────────────────── -->
	<section class="quick-section">
		<div class="quick-grid">
			<a href="{base}/stats#relic-vault" class="quick-card quick-card-relic">
				<div class="quick-icon-wrap quick-icon-svg quick-icon-relic">
					<svg viewBox="0 0 120 120" fill="none" aria-hidden="true">
						<path d="M60 18 90 36 90 84 60 102 30 84 30 36Z" stroke="currentColor" stroke-width="5" opacity="0.35" stroke-linejoin="round"/>
						<path d="M60 30 78 40 78 80 60 90 42 80 42 40Z" stroke="currentColor" stroke-width="5" opacity="0.72" stroke-linejoin="round"/>
						<path d="M60 36V84M42 40H78M42 80H78M48 52H72M48 68H72M50 52V68H70V52" stroke="currentColor" stroke-width="5" stroke-linecap="round" stroke-linejoin="round"/>
					</svg>
				</div>
				<span class="quick-label">Relic Vault</span>
				<span class="quick-desc">{ownedRelicCount} of {totalRelics} boss relics archived</span>
				{#if latestRelic}
					<span class="quick-note">Latest: {latestRelic.name}</span>
				{/if}
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
			<a href="{base}/shop" class="quick-card">
				<div class="quick-icon-wrap">
					<img src="{base}/images/chest.png" alt="" class="quick-img" aria-hidden="true" />
				</div>
				<span class="quick-label">Shop</span>
				<span class="quick-desc">Powerups & items</span>
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
			<a href="{base}/equip" class="quick-card">
				<div class="quick-icon-wrap">
					<img src="{base}/images/key1.png" alt="" class="quick-img" aria-hidden="true" />
				</div>
				<span class="quick-label">Equip</span>
				<span class="quick-desc">Skins & style</span>
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
			<a href="{base}/settings" class="quick-card">
				<div class="quick-icon-wrap quick-icon-svg">
					<svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2"/>
						<path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41"
							  stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
					</svg>
				</div>
				<span class="quick-label">Settings</span>
				<span class="quick-desc">Customize</span>
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
			<a href="{base}/info" class="quick-card">
				<div class="quick-icon-wrap quick-icon-svg">
					<svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
						<path d="M12 8v1M12 11v5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
					</svg>
				</div>
				<span class="quick-label">How to Play</span>
				<span class="quick-desc">Controls & rules</span>
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
			<a href="{base}/download" class="quick-card quick-card-highlight">
				<div class="quick-icon-wrap quick-icon-svg">
					<svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
						<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
					</svg>
				</div>
				<span class="quick-label">Download</span>
				<span class="quick-desc">Desktop app</span>
				<div class="quick-arrow" aria-hidden="true">→</div>
			</a>
		</div>
	</section>

	{#if gameStore.player.playerName === 'Player'}
		<div class="welcome-prompt">
			<svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
				<path d="M12 8v1M12 11v5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
			</svg>
			Welcome! Set your name in <a href="{base}/settings">Settings</a> to get started.
		</div>
	{/if}
</div>

<style>
	.landing {
		margin: calc(-1 * var(--space-6));
		display: flex;
		flex-direction: column;
	}

	/* ── Keyframes ─────────────────────────────── */
	@keyframes glitch {
		0%, 91%, 100% { clip-path: none; transform: translateX(0); color: #93c5fd; }
		92% { clip-path: polygon(0 20%, 100% 20%, 100% 45%, 0 45%); transform: translateX(-5px); color: #f0abfc; }
		93% { clip-path: polygon(0 55%, 100% 55%, 100% 75%, 0 75%); transform: translateX(5px); color: #67e8f9; }
		94% { clip-path: none; transform: translateX(0); color: #93c5fd; }
	}

	@keyframes badge-pulse {
		0%, 100% { opacity: 1; transform: scale(1); }
		50% { opacity: 0.4; transform: scale(0.85); }
	}

	@keyframes ring-pulse {
		0% { transform: translate(-50%, -50%) scale(1); opacity: 0.5; }
		100% { transform: translate(-50%, -50%) scale(2.4); opacity: 0; }
	}

	@keyframes card-rise {
		from { transform: translateY(32px); opacity: 0; }
		to   { transform: translateY(0); opacity: 1; }
	}

	@keyframes scroll-bob {
		0%, 100% { transform: translateY(0); opacity: 0.5; }
		50% { transform: translateY(6px); opacity: 1; }
	}

	/* ── Hero ───────────────────────────────────── */
	.hero {
		position: relative;
		min-height: 560px;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		background-size: cover;
		background-position: center;
		background-repeat: no-repeat;
		padding: 80px var(--space-6) 64px;
		overflow: hidden;
	}

	.hero-overlay {
		position: absolute;
		inset: 0;
		background:
			linear-gradient(180deg, rgba(6,13,26,0.3) 0%, rgba(6,13,26,0.6) 60%, rgba(6,13,26,0.95) 100%),
			linear-gradient(135deg, rgba(6,13,26,0.7) 0%, rgba(6,13,26,0.3) 100%);
	}

	.hero-nebula {
		position: absolute;
		border-radius: 50%;
		filter: blur(80px);
		pointer-events: none;
	}
	.hero-nebula-1 {
		width: 600px;
		height: 400px;
		top: -100px;
		left: -100px;
		background: radial-gradient(ellipse, rgba(56,189,248,0.12) 0%, transparent 70%);
		animation: nebula-drift 20s ease-in-out infinite;
	}
	.hero-nebula-2 {
		width: 500px;
		height: 350px;
		bottom: -80px;
		right: -80px;
		background: radial-gradient(ellipse, rgba(245,158,11,0.10) 0%, transparent 70%);
		animation: nebula-drift 25s ease-in-out infinite reverse;
	}

	.hero-hex-grid {
		position: absolute;
		inset: 0;
		background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='56' height='100' viewBox='0 0 56 100'%3E%3Cpath d='M28 0 L56 16 L56 48 L28 64 L0 48 L0 16 Z' fill='none' stroke='rgba(56,189,248,0.12)' stroke-width='0.8'/%3E%3Cpath d='M28 36 L56 52 L56 84 L28 100 L0 84 L0 52 Z' fill='none' stroke='rgba(56,189,248,0.12)' stroke-width='0.8'/%3E%3C/svg%3E");
		background-size: 56px 100px;
		animation: grid-drift 30s linear infinite;
		opacity: 0.6;
		pointer-events: none;
	}

	.hero-scanlines {
		position: absolute;
		inset: 0;
		background: repeating-linear-gradient(0deg, transparent 0, transparent 3px, rgba(0,0,0,0.04) 3px, rgba(0,0,0,0.04) 4px);
		pointer-events: none;
	}

	.hero-content {
		position: relative;
		z-index: 1;
		text-align: center;
		max-width: 680px;
		animation: fade-up 0.7s ease both;
	}

	/* Badge */
	.hero-badge {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 6px 16px;
		background: rgba(56, 189, 248, 0.08);
		border: 1px solid rgba(56, 189, 248, 0.35);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		color: #93c5fd;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		margin-bottom: var(--space-5);
	}
	.badge-dot {
		width: 6px;
		height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 8px var(--color-accent-primary);
		animation: badge-pulse 2s ease-in-out infinite;
	}

	/* Title */
	.hero-title {
		font-family: var(--font-display);
		font-size: clamp(3.2rem, 9vw, 6rem);
		font-weight: 700;
		letter-spacing: -0.03em;
		line-height: 1;
		margin-bottom: var(--space-4);
	}
	.title-maze {
		color: #93c5fd;
		text-shadow: 0 0 48px rgba(56, 189, 248, 0.45);
		animation: glitch 10s ease-in-out infinite;
	}
	.title-escape {
		color: #f8fafc;
		text-shadow: 0 2px 24px rgba(0,0,0,0.6);
	}
	.title-pathbound {
		display: block;
		color: var(--color-accent-gold);
		text-shadow: 0 0 40px rgba(245, 158, 11, 0.5);
		font-size: 0.68em;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		line-height: 1.2;
		margin-top: var(--space-1);
	}

	.hero-byline {
		font-size: var(--text-sm);
		color: rgba(203, 213, 225, 0.5);
		letter-spacing: 0.04em;
		margin-bottom: var(--space-4);
	}
	.hero-author {
		color: var(--color-accent-gold);
		text-decoration: none;
		transition: color var(--transition-fast);
	}
	.hero-author:hover { color: #fcd34d; }

	.hero-subtitle {
		color: #94a3b8;
		font-size: var(--text-lg);
		line-height: 1.65;
		max-width: 520px;
		margin: 0 auto var(--space-8);
	}

	/* CTA Buttons */
	.hero-actions {
		display: flex;
		flex-wrap: wrap;
		gap: var(--space-3);
		justify-content: center;
		margin-bottom: var(--space-10);
	}

	.btn-hero-primary {
		position: relative;
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 14px 32px;
		background: var(--color-accent-primary);
		color: #fff;
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		text-decoration: none;
		border-radius: var(--radius-lg);
		transition: all var(--transition-base);
		box-shadow: 0 0 32px rgba(56, 189, 248, 0.35);
		overflow: visible;
	}
	.btn-hero-primary::before {
		content: '';
		position: absolute;
		top: 50%;
		left: 50%;
		width: 100%;
		height: 100%;
		border: 2px solid rgba(56, 189, 248, 0.5);
		border-radius: var(--radius-lg);
		animation: ring-pulse 2.5s ease-out infinite;
		pointer-events: none;
	}
	.btn-hero-primary:hover {
		background: #0ea5e9;
		box-shadow: 0 0 48px rgba(56, 189, 248, 0.55);
		transform: translateY(-2px);
		color: #fff;
	}

	.btn-hero-secondary {
		position: relative;
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 14px 32px;
		background: rgba(255, 255, 255, 0.06);
		color: #f0f6ff;
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-lg);
		text-decoration: none;
		border: 1px solid rgba(255, 255, 255, 0.2);
		border-radius: var(--radius-lg);
		backdrop-filter: blur(8px);
		transition: all var(--transition-base);
	}
	.btn-hero-secondary:hover {
		background: rgba(255, 255, 255, 0.12);
		border-color: rgba(255, 255, 255, 0.4);
		transform: translateY(-2px);
		color: #fff;
	}
	.btn-locked { opacity: 0.5; pointer-events: none; }

	.lock-hint {
		position: absolute;
		bottom: -22px;
		left: 50%;
		transform: translateX(-50%);
		font-size: var(--text-xs);
		color: #fca5a5;
		white-space: nowrap;
	}

	/* Hero stats strip */
	.hero-stats {
		display: inline-flex;
		align-items: center;
		gap: var(--space-4);
		padding: 12px 28px;
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		backdrop-filter: blur(8px);
	}
	.hero-stat {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2px;
	}
	.hero-stat-val {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-accent-primary);
		line-height: 1;
	}
	.hero-stat-lbl {
		font-size: 10px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: rgba(255,255,255,0.35);
	}
	.hero-stat-divider {
		width: 1px;
		height: 28px;
		background: rgba(255,255,255,0.1);
	}

	/* Scroll hint */
	.hero-scroll-hint {
		position: absolute;
		bottom: var(--space-6);
		left: 50%;
		transform: translateX(-50%);
		color: rgba(255,255,255,0.3);
		animation: scroll-bob 2s ease-in-out infinite;
	}

	/* ── Worlds section ─────────────────────────── */
	.worlds-section {
		padding: var(--space-16) var(--space-6);
	}

	.section-header {
		text-align: center;
		margin-bottom: var(--space-10);
		animation: fade-up 0.5s ease both;
	}
	.section-title {
		font-family: var(--font-display);
		font-size: clamp(1.5rem, 3.5vw, 2.2rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		margin-bottom: var(--space-2);
	}
	.section-sub {
		font-size: var(--text-base);
		color: var(--color-text-secondary);
	}

	.worlds-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
		gap: var(--space-5);
		max-width: 1000px;
		margin: 0 auto;
	}

	.world-card {
		position: relative;
		border-radius: var(--radius-2xl);
		overflow: hidden;
		background-size: cover;
		background-position: center;
		min-height: 320px;
		display: flex;
		flex-direction: column;
		justify-content: flex-end;
		border: 1px solid color-mix(in srgb, var(--accent, var(--color-accent-primary)) 22%, rgba(255, 255, 255, 0.1));
		transition: transform var(--transition-base), box-shadow var(--transition-base), border-color var(--transition-base);
		animation: card-rise 0.55s cubic-bezier(0.22, 1, 0.36, 1) calc(var(--i, 0) * 120ms) both;
		cursor: default;
		box-shadow: 0 18px 40px rgba(0,0,0,0.3), inset 0 -140px 120px rgba(6,13,26,0.55);
	}
	.world-card::after {
		content: '';
		position: absolute;
		inset: 0;
		background:
			radial-gradient(circle at top right, color-mix(in srgb, var(--accent, #38bdf8) 22%, transparent) 0%, transparent 38%),
			linear-gradient(180deg, rgba(6,13,26,0.02) 0%, rgba(6,13,26,0.55) 100%);
		pointer-events: none;
	}

	.card-shimmer {
		position: absolute;
		inset: 0;
		z-index: 2;
		background: linear-gradient(115deg, rgba(255,255,255,0) 40%, rgba(255,255,255,0.06) 50%, rgba(255,255,255,0) 60%);
		background-size: 200% 100%;
		background-position: -200% 0;
		transition: background-position 0.7s ease;
		pointer-events: none;
		border-radius: var(--radius-2xl);
	}
	.world-card:not(.locked):hover .card-shimmer { background-position: 200% 0; }

	.world-card:not(.locked):hover {
		transform: translateY(-10px);
		box-shadow: 0 24px 64px rgba(0,0,0,0.5), 0 0 0 1px var(--accent, #38bdf8), 0 0 48px rgba(0,0,0,0.3);
		border-color: var(--accent, #38bdf8);
	}

	.world-card.locked {
		opacity: 0.65;
		filter: grayscale(0.3);
	}

	.world-lock {
		position: absolute;
		inset: 0;
		z-index: 10;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		background: rgba(0,0,0,0.55);
		backdrop-filter: blur(2px);
		border-radius: var(--radius-2xl);
	}
	.lock-img {
		width: 44px;
		height: 44px;
		object-fit: contain;
		filter: drop-shadow(0 0 8px rgba(255,255,255,0.2));
		animation: float-y 3.5s ease-in-out infinite;
	}
	.lock-text {
		font-size: var(--text-sm);
		color: rgba(255,255,255,0.55);
		font-weight: 500;
	}

	.world-card-body {
		position: relative;
		z-index: 3;
		padding: var(--space-6);
		padding-top: var(--space-16);
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
		background: linear-gradient(180deg, transparent 0%, color-mix(in srgb, var(--accent, #38bdf8) 8%, rgba(6,13,26,0.72)) 100%);
	}

	.world-number-tag {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: rgba(255,255,255,0.4);
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
		line-height: 1.45;
		opacity: 0.9;
	}
	.world-stats {
		display: flex;
		gap: var(--space-3);
		align-items: center;
		margin-top: var(--space-1);
	}
	.wstat {
		display: flex;
		align-items: center;
		gap: 4px;
		font-size: var(--text-sm);
		color: rgba(255,255,255,0.6);
		font-weight: 500;
	}
	.wstat-star {
		width: 13px;
		height: 13px;
		object-fit: contain;
	}

	.world-btn {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		margin-top: var(--space-3);
		padding: 9px 18px;
		background: rgba(255,255,255,0.07);
		border: 1px solid var(--btn-color, #38bdf8);
		border-radius: var(--radius-lg);
		color: var(--btn-color, #38bdf8);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		text-decoration: none;
		transition: all var(--transition-fast);
		backdrop-filter: blur(4px);
		align-self: flex-start;
	}
	.world-btn:hover {
		background: rgba(255,255,255,0.13);
		transform: translateX(4px);
		box-shadow: 0 0 18px rgba(0,0,0,0.3);
	}

	.coming-badge {
		margin-top: var(--space-3);
		padding: 8px 16px;
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.1);
		border-radius: var(--radius-lg);
		font-size: var(--text-xs);
		color: rgba(255,255,255,0.35);
		font-style: italic;
		align-self: flex-start;
	}

	.card-accent-line {
		position: absolute;
		bottom: 0;
		left: 0;
		right: 0;
		height: 2px;
		opacity: 0;
		transition: opacity var(--transition-base);
		box-shadow: 0 0 12px currentColor;
	}
	.world-card:not(.locked):hover .card-accent-line { opacity: 1; }

	/* ── Quick Nav ──────────────────────────────── */
	.quick-section {
		padding: 0 var(--space-6) var(--space-16);
	}

	.quick-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
		gap: var(--space-3);
		max-width: 900px;
		margin: 0 auto;
	}

	.quick-card {
		position: relative;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-2);
		padding: var(--space-6) var(--space-4) var(--space-5);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border-subtle);
		border-radius: var(--radius-xl);
		text-decoration: none;
		transition: all var(--transition-base);
		box-shadow: var(--shadow-card);
		overflow: hidden;
		backdrop-filter: blur(12px);
		background-image:
			radial-gradient(circle at top center, color-mix(in srgb, var(--color-accent-primary) 12%, transparent) 0%, transparent 58%),
			linear-gradient(180deg, color-mix(in srgb, var(--color-accent-primary) 3%, transparent) 0%, transparent 100%);
	}
	.quick-card::before {
		content: '';
		position: absolute;
		inset: 0;
		background: linear-gradient(135deg, color-mix(in srgb, var(--color-accent-primary) 10%, transparent) 0%, transparent 60%);
		opacity: 0;
		transition: opacity var(--transition-base);
		border-radius: var(--radius-xl);
	}
	.quick-card:hover::before { opacity: 1; }
	.quick-card:hover {
		border-color: color-mix(in srgb, var(--color-accent-primary) 34%, transparent);
		transform: translateY(-4px);
		box-shadow: var(--shadow-card), 0 0 28px color-mix(in srgb, var(--color-accent-primary) 20%, transparent);
		background: var(--color-bg-card-hover);
	}

	.quick-card-highlight {
		border-color: rgba(245, 158, 11, 0.2);
	}
	.quick-card-highlight::before {
		background: linear-gradient(135deg, rgba(245,158,11,0.05) 0%, transparent 60%);
	}
	.quick-card-highlight:hover {
		border-color: rgba(245, 158, 11, 0.4);
		box-shadow: var(--shadow-card), 0 0 28px rgba(245, 158, 11, 0.12);
	}

	.quick-card-relic {
		border-color: rgba(56, 189, 248, 0.22);
		background-image:
			radial-gradient(circle at top center, rgba(56, 189, 248, 0.16) 0%, transparent 60%),
			linear-gradient(180deg, rgba(56, 189, 248, 0.06) 0%, transparent 100%);
	}

	.quick-card-relic::before {
		background: linear-gradient(135deg, rgba(56, 189, 248, 0.14) 0%, transparent 60%);
	}

	.quick-card-relic:hover {
		border-color: rgba(56, 189, 248, 0.4);
		box-shadow: var(--shadow-card), 0 0 28px rgba(56, 189, 248, 0.16);
	}

	.quick-icon-wrap {
		width: 52px;
		height: 52px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: color-mix(in srgb, var(--color-accent-primary) 10%, transparent);
		border: 1px solid color-mix(in srgb, var(--color-accent-primary) 18%, transparent);
		border-radius: var(--radius-xl);
		margin-bottom: var(--space-1);
		transition: all var(--transition-base);
	}
	.quick-card:hover .quick-icon-wrap {
		background: color-mix(in srgb, var(--color-accent-primary) 16%, transparent);
		border-color: color-mix(in srgb, var(--color-accent-primary) 34%, transparent);
		transform: scale(1.05);
	}
	.quick-card-highlight .quick-icon-wrap {
		background: rgba(245, 158, 11, 0.08);
		border-color: rgba(245, 158, 11, 0.15);
		color: var(--color-accent-gold);
	}
	.quick-card-highlight:hover .quick-icon-wrap {
		background: rgba(245, 158, 11, 0.14);
		border-color: rgba(245, 158, 11, 0.3);
	}

	.quick-img {
		width: 30px;
		height: 30px;
		object-fit: contain;
	}
	.quick-icon-svg svg {
		width: 24px;
		height: 24px;
		color: var(--color-accent-primary);
		stroke: currentColor;
	}

	.quick-icon-relic svg {
		width: 30px;
		height: 30px;
		color: #7dd3fc;
	}

	.quick-label {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: var(--color-text-primary);
	}
	.quick-desc {
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
	}

	.quick-note {
		font-size: 11px;
		font-weight: 600;
		color: #7dd3fc;
	}
	.quick-arrow {
		position: absolute;
		top: var(--space-4);
		right: var(--space-4);
		font-size: var(--text-sm);
		color: rgba(255,255,255,0.15);
		transition: all var(--transition-fast);
	}
	.quick-card:hover .quick-arrow {
		color: var(--color-accent-primary);
		transform: translateX(3px);
	}

	/* ── Welcome prompt ─────────────────────────── */
	.welcome-prompt {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		margin: 0 var(--space-6) var(--space-8);
		padding: var(--space-3) var(--space-6);
		background: color-mix(in srgb, var(--color-accent-primary) 8%, transparent);
		border: 1px solid color-mix(in srgb, var(--color-accent-primary) 24%, transparent);
		border-radius: var(--radius-lg);
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		max-width: 480px;
		margin-left: auto;
		margin-right: auto;
	}
	.welcome-prompt svg { color: var(--color-accent-primary); flex-shrink: 0; }
	.welcome-prompt a { color: var(--color-accent-primary); font-weight: 600; }

	/* ── Responsive ─────────────────────────────── */
	@media (max-width: 480px) {
		.hero { min-height: 480px; padding: 60px var(--space-4) 48px; }
		.hero-stats { flex-wrap: wrap; justify-content: center; }
		.hero-actions { flex-direction: column; align-items: center; }
		.btn-hero-primary, .btn-hero-secondary { width: 100%; justify-content: center; }
		.worlds-section, .quick-section { padding-left: var(--space-4); padding-right: var(--space-4); }
	}
</style>
