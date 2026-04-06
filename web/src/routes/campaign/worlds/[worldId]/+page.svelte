<script lang="ts">
	import { page } from '$app/stores';
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { getWorldTheme } from '$lib/worldThemes';

	const worldId = $derived(Number($page.params.worldId));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levels = $derived(worldDef?.levels ?? []);
	const starCount = $derived(gameStore.getWorldStarCount(worldId));
	const theme = $derived(getWorldTheme(worldId));

	function isLevelUnlocked(levelNum: string): boolean {
		if (levelNum === '1') return true;
		for (const level of levels) {
			const progress = gameStore.getLevelProgress(worldId, level.levelNumber);
			if (!progress?.completed) continue;
			if (level.connectTo1 === levelNum || level.connectTo2 === levelNum) return true;
		}
		return false;
	}

	function getLevelStars(levelNum: string): number {
		return gameStore.getLevelProgress(worldId, levelNum)?.numberOfStars ?? 0;
	}

	function isLevelCompleted(levelNum: string): boolean {
		return gameStore.getLevelProgress(worldId, levelNum)?.completed ?? false;
	}

	const mainLevels = $derived(levels.filter((l) => !l.levelNumber.includes('b')));
	const bonusLevels = $derived(levels.filter((l) => l.levelNumber.includes('b')));
</script>

<svelte:head>
	<title>{worldDef?.worldName ?? 'World'} – MazeEscape</title>
</svelte:head>

{#if worldDef}
	<div class="world-page">

		<!-- ── World header band ───────────────────── -->
		<div
			class="world-banner"
			style="
				{theme.bgImageFile
					? `background-image: ${theme.overlayGradient}, url('${base}/images/${theme.bgImageFile}');`
					: `background: linear-gradient(135deg, #042014, #064424);`
				}
			"
		>
			<a href="{base}/campaign/worlds" class="back-link">
				<svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
					<path d="M10 12L6 8l4-4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
				</svg>
				All Worlds
			</a>

			<div class="banner-content">
				<div class="world-tag-small">World {worldDef.worldId}</div>
				<h1 class="world-title" style="text-shadow: 0 0 30px {theme.accentColor}44">{worldDef.worldName}</h1>
				<div class="world-stars">
					<img src="{base}/images/full_star.png" alt="Stars earned:" class="star-badge-img" />
					<span style="color: var(--color-accent-gold); font-weight: 700">{starCount}</span>
					<span class="star-denom">/ {worldDef.numberOfLevels * 3} possible</span>
				</div>
			</div>

			<!-- Accent line at bottom of banner -->
			<div class="banner-accent-bar" style="background: {theme.accentColor}"></div>
		</div>

		<!-- ── Main levels ─────────────────────────── -->
		<section class="levels-section">
			<h2 class="section-title">
				<span class="title-dot" style="background: {theme.accentColor}"></span>
				Campaign Levels
			</h2>
			<div class="levels-grid">
				{#each mainLevels as level}
					{@const unlocked = isLevelUnlocked(level.levelNumber)}
					{@const completed = isLevelCompleted(level.levelNumber)}
					{@const stars = getLevelStars(level.levelNumber)}

					{#if unlocked}
						<a
							href="{base}/campaign/play/{worldId}-{level.levelNumber}"
							class="level-card"
							class:completed
							style="--accent: {theme.accentColor}; --accent-dim: {theme.accentDim}"
						>
							<span class="level-num">{level.levelNumber}</span>
							<div class="star-row">
								{#each Array(3) as _, i}
									<img
										src="{base}/images/{i < stars ? 'full_star' : 'empty_star'}.png"
										alt=""
										class="star-img"
										aria-hidden="true"
									/>
								{/each}
							</div>
							<span class="level-size">{level.width}×{level.height}</span>
						</a>
					{:else}
						<div class="level-card locked" aria-label="Level {level.levelNumber} - Locked">
							<span class="level-num">{level.levelNumber}</span>
							<img src="{base}/images/lock.png" alt="Locked" class="lock-sm-img" />
						</div>
					{/if}
				{/each}
			</div>
		</section>

		<!-- ── Bonus levels ────────────────────────── -->
		{#if bonusLevels.length > 0}
			<section class="levels-section">
				<h2 class="section-title">
					<span class="title-dot" style="background: var(--color-accent-gold)"></span>
					Bonus Levels
				</h2>
				<div class="levels-grid">
					{#each bonusLevels as level}
						{@const unlocked = isLevelUnlocked(level.levelNumber)}
						{@const completed = isLevelCompleted(level.levelNumber)}
						{@const stars = getLevelStars(level.levelNumber)}

						{#if unlocked}
							<a
								href="{base}/campaign/play/{worldId}-{level.levelNumber}"
								class="level-card bonus"
								class:completed
								style="--accent: {theme.accentColor}; --accent-dim: {theme.accentDim}"
							>
								<span class="level-num">{level.levelNumber}</span>
								<div class="star-row">
									{#each Array(3) as _, i}
										<img
											src="{base}/images/{i < stars ? 'full_star' : 'empty_star'}.png"
											alt=""
											class="star-img"
											aria-hidden="true"
										/>
									{/each}
								</div>
								<span class="level-size">{level.width}×{level.height}</span>
							</a>
						{:else}
							<div class="level-card locked bonus" aria-label="Bonus {level.levelNumber} - Locked">
								<span class="level-num">{level.levelNumber}</span>
								<img src="{base}/images/lock.png" alt="Locked" class="lock-sm-img" />
							</div>
						{/if}
					{/each}
				</div>
			</section>
		{/if}
	</div>
{:else}
	<div class="not-found">
		<h1>World not found</h1>
		<a href="{base}/campaign/worlds">← Back to worlds</a>
	</div>
{/if}

<style>
	.world-page {
		max-width: 960px;
		margin: 0 auto;
	}

	/* ── Banner ──────────────────────────────────── */
	.world-banner {
		position: relative;
		background-size: cover;
		background-position: center;
		border-radius: var(--radius-2xl);
		overflow: hidden;
		padding: var(--space-6) var(--space-8);
		margin-bottom: var(--space-10);
		min-height: 180px;
		display: flex;
		flex-direction: column;
		justify-content: space-between;
		border: 1px solid rgba(255, 255, 255, 0.08);
	}

	.back-link {
		display: inline-flex;
		align-items: center;
		gap: var(--space-1);
		font-size: var(--text-sm);
		font-weight: 500;
		color: rgba(255, 255, 255, 0.65);
		text-decoration: none;
		transition: color var(--transition-fast);
		align-self: flex-start;
	}

	.back-link:hover {
		color: #fff;
	}

	.banner-content {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.world-tag-small {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: rgba(255, 255, 255, 0.48);
	}

	.world-title {
		font-family: var(--font-display);
		font-size: clamp(1.6rem, 4vw, 2.5rem);
		font-weight: 700;
		color: #f0f6ff;
		line-height: 1.15;
	}

	.world-stars {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		font-size: var(--text-base);
	}

	.star-badge-img {
		width: 20px;
		height: 20px;
		object-fit: contain;
	}

	.star-denom {
		color: rgba(255, 255, 255, 0.40);
		font-size: var(--text-sm);
	}

	.banner-accent-bar {
		position: absolute;
		bottom: 0;
		left: 0;
		right: 0;
		height: 3px;
		opacity: 0.80;
	}

	/* ── Level section ───────────────────────────── */
	.levels-section {
		margin-bottom: var(--space-10);
	}

	.section-title {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		font-family: var(--font-display);
		font-size: var(--text-lg);
		font-weight: 700;
		color: var(--color-text-secondary);
		margin-bottom: var(--space-5);
	}

	.title-dot {
		width: 8px;
		height: 8px;
		border-radius: 50%;
		flex-shrink: 0;
	}

	/* ── Level grid ──────────────────────────────── */
	.levels-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(86px, 1fr));
		gap: var(--space-3);
	}

	.level-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: var(--space-1);
		padding: var(--space-3) var(--space-2);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		text-decoration: none;
		transition: all var(--transition-fast);
		aspect-ratio: 1;
		min-height: 84px;
		box-shadow: var(--shadow-sm);
	}

	.level-card:not(.locked):hover {
		border-color: var(--accent, var(--color-accent-primary));
		background: var(--color-bg-card-hover);
		transform: translateY(-3px);
		box-shadow: 0 4px 20px var(--accent-dim, rgba(21,101,192,0.18));
	}

	.level-card.completed {
		border-color: var(--accent, var(--color-accent-primary));
		background: color-mix(in srgb, var(--accent, #1565c0) 6%, white);
	}

	.level-card.locked {
		opacity: 0.45;
		cursor: default;
		background: var(--color-bg-primary);
	}

	.level-card.bonus {
		border-style: dashed;
	}

	.level-num {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		color: var(--color-text-primary);
		line-height: 1;
	}

	.star-row {
		display: flex;
		gap: 2px;
		align-items: center;
	}

	.star-img {
		width: 11px;
		height: 11px;
		object-fit: contain;
	}

	.level-size {
		font-size: 0.65rem;
		color: var(--color-text-muted);
		letter-spacing: 0.02em;
	}

	.lock-sm-img {
		width: 16px;
		height: 16px;
		object-fit: contain;
		opacity: 0.55;
	}

	/* ── Not found ───────────────────────────────── */
	.not-found {
		text-align: center;
		padding: var(--space-16);
	}

	.not-found h1 {
		margin-bottom: var(--space-4);
		color: var(--color-text-muted);
	}

	@media (max-width: 640px) {
		.world-banner {
			padding: var(--space-4) var(--space-5);
			min-height: 150px;
		}
		.levels-grid {
			grid-template-columns: repeat(auto-fill, minmax(72px, 1fr));
		}
	}
</style>
