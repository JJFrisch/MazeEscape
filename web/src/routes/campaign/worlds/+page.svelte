<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import Stars from '$lib/components/Stars.svelte';

	const worlds = getAllWorlds();
</script>

<svelte:head>
	<title>Worlds – MazeEscape</title>
</svelte:head>

<div class="worlds-page">
	<h1 class="page-title">Campaign Worlds</h1>

	<div class="worlds-grid">
		{#each worlds as world, i}
			{@const stars = gameStore.getWorldStarCount(world.worldId)}
			{@const isLocked = world.locked && i > 0 && stars === 0 && gameStore.getWorldStarCount(worlds[i - 1]?.worldId ?? 0) < 250}
			<div class="world-card" class:locked={isLocked}>
				{#if isLocked}
					<div class="lock-overlay">
						<span class="lock-icon">🔒</span>
						<span class="lock-text">Complete previous world to unlock</span>
					</div>
				{/if}
				<div class="world-header">
					<span class="world-number">World {world.worldId}</span>
					<Stars count={Math.min(Math.floor(stars / (world.numberOfLevels || 1)), 3)} max={3} size="1rem" />
				</div>
				<h2 class="world-name">{world.worldName}</h2>
				<div class="world-stats">
					<div class="stat-row">
						<span class="stat-label">Levels</span>
						<span class="stat-value">{world.numberOfLevels}</span>
					</div>
					<div class="stat-row">
						<span class="stat-label">Stars Earned</span>
						<span class="stat-value star-count">⭐ {stars}</span>
					</div>
				</div>
				{#if !isLocked && world.levels.length > 0}
					<a href="{base}/campaign/worlds/{world.worldId}" class="world-play-btn">
						Play World
					</a>
				{:else if world.levels.length === 0}
					<span class="coming-soon">Coming Soon</span>
				{/if}
			</div>
		{/each}
	</div>
</div>

<style>
	.worlds-page {
		max-width: 900px;
		margin: 0 auto;
	}

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-3xl);
		margin-bottom: var(--space-8);
		text-align: center;
	}

	.worlds-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
		gap: var(--space-6);
	}

	.world-card {
		position: relative;
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-xl);
		padding: var(--space-6);
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
		transition: all var(--transition-base);
		overflow: hidden;
	}

	.world-card:not(.locked):hover {
		border-color: var(--color-accent-primary);
		box-shadow: var(--shadow-glow);
		transform: translateY(-4px);
	}

	.world-card.locked {
		opacity: 0.5;
		filter: grayscale(0.5);
	}

	.lock-overlay {
		position: absolute;
		inset: 0;
		z-index: 10;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		background: rgba(0, 0, 0, 0.6);
		border-radius: var(--radius-xl);
	}

	.lock-icon {
		font-size: 2rem;
		margin-bottom: var(--space-2);
	}

	.lock-text {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		text-align: center;
		padding: 0 var(--space-4);
	}

	.world-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
	}

	.world-number {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
		font-weight: 600;
		text-transform: uppercase;
		letter-spacing: 0.05em;
	}

	.world-name {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		color: var(--color-accent-secondary);
	}

	.world-stats {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.stat-row {
		display: flex;
		justify-content: space-between;
		font-size: var(--text-sm);
	}

	.stat-label {
		color: var(--color-text-muted);
	}

	.stat-value {
		color: var(--color-text-primary);
		font-weight: 600;
	}

	.star-count {
		color: var(--color-accent-gold);
	}

	.world-play-btn {
		display: block;
		text-align: center;
		padding: var(--space-3) var(--space-4);
		background: var(--color-accent-primary);
		color: white;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		text-decoration: none;
		transition: all var(--transition-fast);
		margin-top: auto;
	}

	.world-play-btn:hover {
		background: var(--color-accent-glow);
		color: white;
		box-shadow: var(--shadow-glow);
	}

	.coming-soon {
		text-align: center;
		padding: var(--space-3);
		color: var(--color-text-muted);
		font-style: italic;
		font-size: var(--text-sm);
		margin-top: auto;
	}
</style>
