<script lang="ts">
	import { page } from '$app/stores';
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import Stars from '$lib/components/Stars.svelte';

	const worldId = $derived(Number($page.params.worldId));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levels = $derived(worldDef?.levels ?? []);
	const starCount = $derived(gameStore.getWorldStarCount(worldId));

	function isLevelUnlocked(levelNum: string): boolean {
		// Level "1" is always unlocked
		if (levelNum === '1') return true;

		// Check if any completed level connects to this one
		for (const level of levels) {
			const progress = gameStore.getLevelProgress(worldId, level.levelNumber);
			if (!progress?.completed) continue;

			if (level.connectTo1 === levelNum || level.connectTo2 === levelNum) {
				return true;
			}
		}
		return false;
	}

	function getLevelStars(levelNum: string): number {
		const progress = gameStore.getLevelProgress(worldId, levelNum);
		return progress?.numberOfStars ?? 0;
	}

	function isLevelCompleted(levelNum: string): boolean {
		const progress = gameStore.getLevelProgress(worldId, levelNum);
		return progress?.completed ?? false;
	}

	// Separate main levels from bonus levels
	const mainLevels = $derived(levels.filter((l) => !l.levelNumber.includes('b')));
	const bonusLevels = $derived(levels.filter((l) => l.levelNumber.includes('b')));
</script>

<svelte:head>
	<title>{worldDef?.worldName ?? 'World'} – MazeEscape</title>
</svelte:head>

{#if worldDef}
	<div class="world-page">
		<div class="world-header">
			<a href="{base}/campaign/worlds" class="back-link">← Worlds</a>
			<div class="world-info">
				<h1 class="world-title">{worldDef.worldName}</h1>
				<div class="world-star-count">⭐ {starCount} stars</div>
			</div>
		</div>

		<!-- Main levels grid -->
		<section class="levels-section">
			<h2 class="section-title">Campaign Levels</h2>
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
						>
							<span class="level-num">{level.levelNumber}</span>
							<Stars count={stars} size="0.8rem" />
							<span class="level-size">{level.width}×{level.height}</span>
						</a>
					{:else}
						<div class="level-card locked" aria-label="Level {level.levelNumber} - Locked">
							<span class="level-num">{level.levelNumber}</span>
							<span class="lock">🔒</span>
						</div>
					{/if}
				{/each}
			</div>
		</section>

		<!-- Bonus levels -->
		{#if bonusLevels.length > 0}
			<section class="levels-section">
				<h2 class="section-title">Bonus Levels</h2>
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
							>
								<span class="level-num">{level.levelNumber}</span>
								<Stars count={stars} size="0.8rem" />
								<span class="level-size">{level.width}×{level.height}</span>
							</a>
						{:else}
							<div class="level-card locked bonus" aria-label="Bonus {level.levelNumber} - Locked">
								<span class="level-num">{level.levelNumber}</span>
								<span class="lock">🔒</span>
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
		max-width: 900px;
		margin: 0 auto;
	}

	.world-header {
		margin-bottom: var(--space-8);
	}

	.back-link {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
		display: inline-block;
		margin-bottom: var(--space-2);
	}

	.back-link:hover {
		color: var(--color-accent-secondary);
	}

	.world-info {
		display: flex;
		align-items: baseline;
		gap: var(--space-4);
		flex-wrap: wrap;
	}

	.world-title {
		font-family: var(--font-display);
		font-size: var(--text-3xl);
	}

	.world-star-count {
		font-size: var(--text-lg);
		color: var(--color-accent-gold);
		font-weight: 600;
	}

	.levels-section {
		margin-bottom: var(--space-10);
	}

	.section-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		color: var(--color-text-secondary);
		margin-bottom: var(--space-4);
	}

	.levels-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(90px, 1fr));
		gap: var(--space-3);
	}

	.level-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: var(--space-1);
		padding: var(--space-3);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		text-decoration: none;
		transition: all var(--transition-fast);
		aspect-ratio: 1;
		min-height: 80px;
	}

	.level-card:not(.locked):hover {
		border-color: var(--color-accent-primary);
		background: var(--color-bg-card-hover);
		transform: translateY(-2px);
		box-shadow: var(--shadow-glow);
	}

	.level-card.completed {
		border-color: rgba(99, 102, 241, 0.3);
		background: rgba(99, 102, 241, 0.05);
	}

	.level-card.locked {
		opacity: 0.4;
		cursor: default;
	}

	.level-card.bonus {
		border-style: dashed;
	}

	.level-num {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		color: var(--color-text-primary);
	}

	.level-size {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
	}

	.lock {
		font-size: var(--text-sm);
	}

	.not-found {
		text-align: center;
		padding: var(--space-16);
	}

	.not-found h1 {
		margin-bottom: var(--space-4);
		color: var(--color-text-muted);
	}
</style>
