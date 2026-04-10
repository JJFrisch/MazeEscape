<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { WORLD_THEMES } from '$lib/worldThemes';

	const worlds = getAllWorlds();
</script>

<svelte:head>
	<title>Campaign Worlds – Maze Escape: Pathbound</title>
</svelte:head>

<div class="worlds-page">
	<div class="page-header">
		<div class="page-eyebrow">
			<span class="eyebrow-dot"></span>
			Campaign Mode
		</div>
		<h1 class="page-title">Campaign Worlds</h1>
		<p class="page-sub">Select a world to begin your journey through the labyrinth.</p>
	</div>

	<div class="worlds-grid">
		{#each worlds as world, i}
			{@const theme = WORLD_THEMES.find((t) => t.worldId === world.worldId)}
			{@const stars = gameStore.getWorldStarCount(world.worldId)}
			{@const isLocked = world.locked && i > 0 && gameStore.getWorldStarCount(worlds[i - 1]?.worldId ?? 0) < 250}

			<div
				class="world-card"
				class:locked={isLocked}
				style="
					--i: {i};
					--world-accent: {theme?.accentColor ?? '#38bdf8'};
					--world-accent-dim: {theme?.accentDim ?? 'rgba(56,189,248,0.18)'};
					{theme?.bgImageFile
						? `background-image: ${theme.overlayGradient}, url('${base}/images/${theme.bgImageFile}');`
						: `background: linear-gradient(135deg, #042014, #064424);`
					}
				"
			>
				{#if isLocked}
					<div class="lock-overlay">
						<img src="{base}/images/lock.png" alt="Locked" class="lock-img" />
						<span class="lock-text">Complete previous world to unlock</span>
					</div>
				{/if}

				<!-- Motif decoration top-right -->
				{#if theme?.motif === 'tech'}
					<div class="motif-badge tech-motif">⬡ CYBER</div>
				{:else if theme?.motif === 'space'}
					<div class="motif-badge space-motif">✦ GALACTIC</div>
				{:else}
					<div class="motif-badge elemental-motif">◈ ELEMENTAL</div>
				{/if}

				<div class="world-body">
					<div class="world-num">World {world.worldId}</div>
					<h2 class="world-name">{world.worldName}</h2>
					{#if theme}
						<p class="world-tagline" style="color: {theme.textAccent}">{theme.tagline}</p>
					{/if}

					<!-- Progress bar -->
					<div class="progress-section">
						<div class="progress-row">
							<span class="progress-label">Stars</span>
							<span class="progress-value" style="color: var(--color-accent-gold)">
								<img src="{base}/images/full_star.png" alt="" class="star-icon-sm" />
								{stars}
							</span>
						</div>
						<div class="progress-bar-track">
							<div
								class="progress-bar-fill"
								style="
									--fill-w: {Math.min((stars / (world.numberOfLevels * 3)) * 100, 100)}%;
									background: {theme?.accentColor ?? '#38bdf8'};
								"
							></div>
						</div>
						<div class="progress-row">
							<span class="progress-label">Levels</span>
							<span class="progress-value">{world.numberOfLevels}</span>
						</div>
					</div>

					{#if !isLocked && world.levels.length > 0}
						<a
							href="{base}/campaign/worlds/{world.worldId}"
							class="play-btn"
							style="
								border-color: {theme?.accentColor ?? '#38bdf8'};
								color: {theme?.accentColor ?? '#38bdf8'};
								box-shadow: 0 0 18px {theme?.accentDim ?? 'rgba(56,189,248,0.18)'};
							"
						>
							Enter World →
						</a>
					{:else if world.levels.length === 0}
						<span class="coming-soon">Coming Soon</span>
					{/if}
				</div>

				<!-- Chest decoration -->
				{#if !isLocked}
					<img
						src="{base}/images/{world.worldId === 2 ? 'world2_chest' : 'chest'}.png"
						alt=""
						class="card-deco-chest"
						aria-hidden="true"
					/>
				{/if}
			</div>
		{/each}
	</div>
</div>

<style>
	.worlds-page {
		max-width: 1000px;
		margin: 0 auto;
	}

	/* ── Page header ─────────────────────────────── */
	.page-header {
		text-align: center;
		margin-bottom: var(--space-10);
		animation: fade-up 0.5s ease both;
	}

	@keyframes fade-up {
		from { opacity: 0; transform: translateY(16px); }
		to   { opacity: 1; transform: translateY(0); }
	}

	.page-eyebrow {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 5px 14px;
		background: rgba(56, 189, 248, 0.08);
		border: 1px solid rgba(56, 189, 248, 0.25);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: #93c5fd;
		margin-bottom: var(--space-4);
	}
	.eyebrow-dot {
		width: 6px;
		height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 6px var(--color-accent-primary);
		animation: pulse 2s ease-in-out infinite;
	}
	@keyframes pulse {
		0%, 100% { opacity: 1; } 50% { opacity: 0.4; }
	}

	.page-title {
		font-family: var(--font-display);
		font-size: clamp(2rem, 5vw, 3rem);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: var(--space-3);
		letter-spacing: -0.02em;
	}

	.page-sub {
		color: var(--color-text-secondary);
		font-size: var(--text-lg);
	}

	/* ── Grid ────────────────────────────────────── */
	.worlds-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
		gap: var(--space-6);
	}

	/* ── World card ──────────────────────────────── */
	@keyframes card-rise {
		from { transform: translateY(28px); opacity: 0; }
		to   { transform: translateY(0);    opacity: 1; }
	}

	.world-card {
		position: relative;
		border-radius: var(--radius-2xl);
		overflow: hidden;
		background-size: cover;
		background-position: center;
		min-height: 380px;
		display: flex;
		flex-direction: column;
		justify-content: flex-end;
		border: 1px solid rgba(255, 255, 255, 0.10);
		transition: transform var(--transition-base), box-shadow var(--transition-base), border-color var(--transition-base);
		animation: card-rise 0.55s cubic-bezier(0.22, 1, 0.36, 1) calc(var(--i, 0) * 130ms) both;
	}

	/* Shimmer sweep */
	.world-card::before {
		content: '';
		position: absolute;
		inset: 0;
		z-index: 2;
		pointer-events: none;
		background: linear-gradient(115deg, transparent 40%, rgba(255,255,255,0.09) 50%, transparent 60%);
		background-size: 200% 100%;
		background-position: -200% 0;
		transition: background-position 0.6s ease;
		border-radius: var(--radius-2xl);
	}

	.world-card:not(.locked):hover::before {
		background-position: 200% 0;
	}

	.world-card:not(.locked):hover {
		transform: translateY(-8px);
		box-shadow: 0 16px 60px var(--world-accent-dim, rgba(56,189,248,0.22));
		border-color: var(--world-accent, #38bdf8);
	}

	.world-card.locked {
		filter: grayscale(0.5);
		opacity: 0.68;
	}

	/* ── Lock overlay ────────────────────────────── */
	.lock-overlay {
		position: absolute;
		inset: 0;
		z-index: 20;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: var(--space-3);
		background: rgba(0, 0, 0, 0.60);
		border-radius: var(--radius-2xl);
	}

	.lock-img {
		width: 52px;
		height: 52px;
		object-fit: contain;
		filter: drop-shadow(0 0 10px rgba(255,255,255,0.25));
		animation: float-y 3s ease-in-out infinite;
	}

	.lock-text {
		font-size: var(--text-sm);
		color: #94a3b8;
		text-align: center;
		padding: 0 var(--space-6);
	}

	/* ── Motif badge ─────────────────────────────── */
	.motif-badge {
		position: absolute;
		top: var(--space-4);
		right: var(--space-4);
		z-index: 5;
		font-size: 0.65rem;
		font-weight: 800;
		letter-spacing: 0.12em;
		padding: 4px var(--space-3);
		border-radius: var(--radius-full);
		backdrop-filter: blur(6px);
		animation: glow-throb 3s ease-in-out infinite;
	}

	.tech-motif {
		background: rgba(56, 189, 248, 0.18);
		border: 1px solid rgba(56, 189, 248, 0.4);
		color: #38bdf8;
	}

	.space-motif {
		background: rgba(192, 132, 252, 0.18);
		border: 1px solid rgba(192, 132, 252, 0.4);
		color: #c084fc;
	}

	.elemental-motif {
		background: rgba(52, 211, 153, 0.18);
		border: 1px solid rgba(52, 211, 153, 0.4);
		color: #34d399;
	}

	/* ── Card body ───────────────────────────────── */
	.world-body {
		position: relative;
		z-index: 5;
		padding: var(--space-6);
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
	}

	.world-num {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: rgba(255, 255, 255, 0.50);
	}

	.world-name {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
		color: #f0f6ff;
		line-height: 1.15;
	}

	.world-tagline {
		font-size: var(--text-sm);
		line-height: 1.45;
		opacity: 0.90;
	}

	/* ── Progress ────────────────────────────────── */
	.progress-section {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
		margin-top: var(--space-1);
	}

	.progress-row {
		display: flex;
		justify-content: space-between;
		align-items: center;
		font-size: var(--text-sm);
	}

	.progress-label {
		color: rgba(255, 255, 255, 0.50);
	}

	.progress-value {
		color: rgba(255, 255, 255, 0.85);
		font-weight: 600;
		display: flex;
		align-items: center;
		gap: 4px;
	}

	.star-icon-sm {
		width: 14px;
		height: 14px;
		object-fit: contain;
	}

	.progress-bar-track {
		height: 5px;
		background: rgba(255, 255, 255, 0.10);
		border-radius: var(--radius-full);
		overflow: hidden;
	}

	@keyframes bar-fill {
		from { width: 0; }
		to   { width: var(--fill-w, 0%); }
	}

	.progress-bar-fill {
		height: 100%;
		border-radius: var(--radius-full);
		width: var(--fill-w, 0%);
		animation: bar-fill 0.9s cubic-bezier(0.22, 1, 0.36, 1) calc(var(--i, 0) * 130ms + 300ms) both;
		box-shadow: 0 0 8px currentColor;
	}

	/* ── Play button ─────────────────────────────── */
	.play-btn {
		display: inline-flex;
		align-items: center;
		align-self: flex-start;
		padding: var(--space-2) var(--space-5);
		background: rgba(255, 255, 255, 0.08);
		border: 1px solid;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		text-decoration: none;
		transition: all var(--transition-fast);
		backdrop-filter: blur(4px);
		margin-top: var(--space-2);
	}

	.play-btn:hover {
		background: rgba(255, 255, 255, 0.16);
		transform: translateX(4px);
		box-shadow: 0 0 16px var(--world-accent-dim, rgba(56,189,248,0.3));
	}

	.coming-soon {
		font-size: var(--text-sm);
		color: rgba(255, 255, 255, 0.38);
		font-style: italic;
		margin-top: var(--space-2);
	}

	/* ── Chest decoration ────────────────────────── */
	.card-deco-chest {
		position: absolute;
		top: var(--space-5);
		left: var(--space-5);
		width: 56px;
		height: 56px;
		object-fit: contain;
		filter: drop-shadow(0 4px 12px rgba(0,0,0,0.5));
		animation: float-y 4s ease-in-out infinite;
		z-index: 5;
	}

	/* ── World 3 gradient fallback ───────────────── */
	.world-card:nth-child(3):not([class*='locked']) {
		background: linear-gradient(135deg, #042014 0%, #064424 50%, #021a10 100%) !important;
	}

	@media (max-width: 640px) {
		.worlds-grid {
			grid-template-columns: 1fr;
		}
		.world-card {
			min-height: 320px;
		}
	}
</style>
