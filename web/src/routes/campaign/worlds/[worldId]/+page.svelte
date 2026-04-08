<script lang="ts">
	import { page } from '$app/stores';
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { getWorldTheme } from '$lib/worldThemes';
	import { getWorldMapLayout } from '$lib/core/campaignMapLayout';
	import CampaignMazeMap from '$lib/components/CampaignMazeMap.svelte';

	const worldId = $derived(Number($page.params.worldId));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levels = $derived(worldDef?.levels ?? []);
	const starCount = $derived(gameStore.getWorldStarCount(worldId));
	const theme = $derived(getWorldTheme(worldId));
	const mapLayout = $derived(getWorldMapLayout(worldId));

	// Fallback grid helpers (used for worlds without a maze map yet)
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
	<!-- ── Sticky header strip ───────────────────────────── -->
	<div
		class="world-strip"
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

		<div class="strip-center">
			<span class="world-label">World {worldDef.worldId}</span>
			<h1 class="world-title" style="text-shadow: 0 0 24px {theme.accentColor}44">{worldDef.worldName}</h1>
		</div>

		<div class="strip-end">
			<img src="{base}/images/full_star.png" alt="Stars:" class="star-icon" />
			<span class="star-value" style="color: var(--color-accent-gold)">{starCount}</span>
			<span class="star-denom">/ {worldDef.numberOfLevels * 3}</span>
		</div>

		<div class="accent-bar" style="background: {theme.accentColor}"></div>
	</div>

	{#if mapLayout}
		<!-- ── Maze map view ────────────────────────────────── -->
		<div class="map-wrapper">
			<CampaignMazeMap {worldId} layout={mapLayout} />
		</div>
	{:else}
		<!-- ── Fallback grid (worlds 2 & 3 until their maps are built) ── -->
		<div class="world-page">
			<section class="levels-section">
				<h2 class="section-title">
					<span class="title-dot" style="background: {theme.accentColor}"></span>
					Campaign Levels
				</h2>
				<div class="levels-grid">
					{#each mainLevels as level, i}
						{@const unlocked = isLevelUnlocked(level.levelNumber)}
						{@const completed = isLevelCompleted(level.levelNumber)}
						{@const stars = getLevelStars(level.levelNumber)}

						{#if unlocked}
							<a
								href="{base}/campaign/play/{worldId}-{level.levelNumber}"
								class="level-card"
								class:completed
								style="--accent: {theme.accentColor}; --accent-dim: {theme.accentDim}; --i: {i}"
							>
								<span class="level-num">{level.levelNumber}</span>
								<div class="star-row">
									{#each Array(3) as _, si}
										<img
											src="{base}/images/{si < stars ? 'full_star' : 'empty_star'}.png"
											alt=""
											class="star-img"
											aria-hidden="true"
										/>
									{/each}
								</div>
								<span class="level-size">{level.width}×{level.height}</span>
							</a>
						{:else}
							<div class="level-card locked" style="--i: {i}" aria-label="Level {level.levelNumber} - Locked">
								<span class="level-num">{level.levelNumber}</span>
								<img src="{base}/images/lock.png" alt="Locked" class="lock-sm-img" />
							</div>
						{/if}
					{/each}
				</div>
			</section>

			{#if bonusLevels.length > 0}
				<section class="levels-section">
					<h2 class="section-title">
						<span class="title-dot" style="background: var(--color-accent-gold)"></span>
						Bonus Levels
					</h2>
					<div class="levels-grid">
						{#each bonusLevels as level, i}
							{@const unlocked = isLevelUnlocked(level.levelNumber)}
							{@const completed = isLevelCompleted(level.levelNumber)}
							{@const stars = getLevelStars(level.levelNumber)}

							{#if unlocked}
								<a
									href="{base}/campaign/play/{worldId}-{level.levelNumber}"
									class="level-card bonus"
									class:completed
									style="--accent: {theme.accentColor}; --accent-dim: {theme.accentDim}; --i: {i}"
								>
									<span class="level-num">{level.levelNumber}</span>
									<div class="star-row">
										{#each Array(3) as _, si}
											<img
												src="{base}/images/{si < stars ? 'full_star' : 'empty_star'}.png"
												alt=""
												class="star-img"
												aria-hidden="true"
											/>
										{/each}
									</div>
									<span class="level-size">{level.width}×{level.height}</span>
								</a>
							{:else}
								<div class="level-card locked bonus" style="--i: {i}" aria-label="Bonus {level.levelNumber} - Locked">
									<span class="level-num">{level.levelNumber}</span>
									<img src="{base}/images/lock.png" alt="Locked" class="lock-sm-img" />
								</div>
							{/if}
						{/each}
					</div>
				</section>
			{/if}
		</div>
	{/if}

{:else}
	<div class="not-found">
		<h1>World not found</h1>
		<a href="{base}/campaign/worlds">← Back to worlds</a>
	</div>
{/if}

<style>
	/* ── Sticky header strip ─────────────────────────────── */
	.world-strip {
		position: sticky;
		top: 0;
		z-index: 30;
		background-size: cover;
		background-position: center;
		border-bottom: 1px solid rgba(255, 255, 255, 0.08);
		padding: 0.75rem 1.25rem;
		display: flex;
		align-items: center;
		gap: 1rem;
		min-height: 64px;
	}

	.back-link {
		display: inline-flex;
		align-items: center;
		gap: 0.25rem;
		font-size: 0.8rem;
		font-weight: 500;
		color: rgba(255, 255, 255, 0.65);
		text-decoration: none;
		white-space: nowrap;
		flex-shrink: 0;
		transition: color 0.15s;
	}
	.back-link:hover { color: #fff; }

	.strip-center {
		flex: 1;
		display: flex;
		flex-direction: column;
		gap: 0.1rem;
		min-width: 0;
	}

	.world-label {
		font-size: 0.65rem;
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: #38bdf8;
		text-shadow: 0 0 8px rgba(56, 189, 248, 0.5);
	}

	.world-title {
		font-family: var(--font-display);
		font-size: clamp(0.95rem, 3vw, 1.35rem);
		font-weight: 800;
		color: #f0f6ff;
		line-height: 1.2;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	.strip-end {
		display: flex;
		align-items: center;
		gap: 0.3rem;
		flex-shrink: 0;
		font-size: 0.85rem;
		background: rgba(251, 191, 36, 0.1);
		border: 1px solid rgba(251, 191, 36, 0.3);
		border-radius: 9999px;
		padding: 3px 10px 3px 6px;
	}

	.star-icon { width: 16px; height: 16px; object-fit: contain; }

	.star-value {
		font-weight: 700;
		color: #fbbf24;
	}

	.star-denom {
		color: rgba(251, 191, 36, 0.55);
		font-size: 0.75rem;
	}

	@keyframes accent-pulse {
		0%, 100% { opacity: 0.75; }
		50% { opacity: 1; box-shadow: 0 0 8px currentColor; }
	}

	.accent-bar {
		position: absolute;
		bottom: 0;
		left: 0;
		right: 0;
		height: 3px;
		animation: accent-pulse 2.5s ease-in-out infinite;
	}

	/* ── Maze map wrapper ────────────────────────────────── */
	.map-wrapper {
		/* Subtract header (3.5rem), world-strip (64px), and app-main's
		   top + bottom padding (2 × var(--space-6) = 3rem) so content
		   fits exactly in the viewport with zero page scroll. */
		height: calc(100dvh - var(--header-height) - 64px - 3rem);
		width: 100%;
		overflow: hidden;
	}

	@media (max-width: 640px) {
		/* app-main padding shrinks to var(--space-4) = 1rem per side on mobile */
		.map-wrapper {
			height: calc(100dvh - var(--header-height) - 64px - 2rem);
		}
	}

	/* ── Fallback grid layout ────────────────────────────── */
	.world-page {
		max-width: 960px;
		margin: 0 auto;
		padding: var(--space-6) var(--space-4);
	}

	.levels-section {
		margin-bottom: var(--space-10);
	}

	@keyframes blink-dot {
		0%, 49% { opacity: 1; }
		50%, 100% { opacity: 0; }
	}

	.section-title {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		font-family: 'Courier New', monospace;
		font-size: var(--text-base);
		font-weight: 700;
		color: var(--color-text-secondary);
		text-transform: uppercase;
		letter-spacing: 0.1em;
		margin-bottom: var(--space-5);
	}

	.title-dot {
		width: 8px;
		height: 8px;
		border-radius: 50%;
		flex-shrink: 0;
		animation: blink-dot 1.2s step-end infinite;
	}

	.levels-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(86px, 1fr));
		gap: var(--space-3);
	}

	@keyframes cell-pop {
		from { transform: scale(0.82) translateY(10px); opacity: 0; }
		to   { transform: scale(1) translateY(0);       opacity: 1; }
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
		animation: cell-pop 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) calc(var(--i, 0) * 40ms) both;
	}

	.level-card:not(.locked):hover {
		border-color: var(--accent, var(--color-accent-primary));
		background: var(--color-bg-card-hover);
		transform: translateY(-4px) scale(1.04);
		box-shadow: 0 6px 24px var(--accent-dim, rgba(21,101,192,0.25));
	}

	.level-card.completed {
		border-color: var(--accent, var(--color-accent-primary));
		background: color-mix(in srgb, var(--accent, #1565c0) 10%, transparent);
	}

	.level-card.locked {
		opacity: 0.45;
		cursor: default;
		background: var(--color-bg-primary);
	}

	.level-card.bonus { border-style: dashed; }

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

	.star-img { width: 11px; height: 11px; object-fit: contain; }

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

	/* ── Not found ───────────────────────────────────────── */
	.not-found {
		text-align: center;
		padding: var(--space-16);
	}

	.not-found h1 {
		margin-bottom: var(--space-4);
		color: var(--color-text-muted);
	}

	@media (max-width: 640px) {
		.world-strip {
			padding: 0.6rem 0.9rem;
		}
		.levels-grid {
			grid-template-columns: repeat(auto-fill, minmax(72px, 1fr));
		}
	}
</style>
