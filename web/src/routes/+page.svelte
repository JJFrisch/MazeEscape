<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
</script>

<svelte:head>
	<title>MazeEscape – Procedural Puzzle Mazes</title>
</svelte:head>

<div class="landing">
	<div class="hero">
		<div class="hero-glow"></div>
		<h1 class="hero-title">
			<span class="title-maze">Maze</span><span class="title-escape">Escape</span>
		</h1>
		<p class="hero-subtitle">Navigate procedurally generated labyrinths across worlds, collect stars, and prove your puzzle mastery.</p>

		<div class="hero-actions">
			<a href="{base}/campaign/worlds" class="btn btn-primary btn-large">
				<span class="btn-icon">🗺️</span>
				Play Campaign
			</a>
			<a
				href="{base}/daily"
				class="btn btn-secondary btn-large"
				class:btn-locked={!gameStore.isDailyMazeUnlocked()}
			>
				<span class="btn-icon">📅</span>
				Daily Maze
				{#if !gameStore.isDailyMazeUnlocked()}
					<span class="lock-badge">🔒 Beat Level 10</span>
				{/if}
			</a>
		</div>
	</div>

	<div class="quick-nav">
		<a href="{base}/shop" class="nav-card">
			<span class="nav-card-icon">🛒</span>
			<span class="nav-card-label">Shop</span>
			<span class="nav-card-desc">Powerups & items</span>
		</a>
		<a href="{base}/equip" class="nav-card">
			<span class="nav-card-icon">🎨</span>
			<span class="nav-card-label">Equip</span>
			<span class="nav-card-desc">Skins & style</span>
		</a>
		<a href="{base}/settings" class="nav-card">
			<span class="nav-card-icon">⚙️</span>
			<span class="nav-card-label">Settings</span>
			<span class="nav-card-desc">Customize</span>
		</a>
		<a href="{base}/info" class="nav-card">
			<span class="nav-card-icon">ℹ️</span>
			<span class="nav-card-label">How to Play</span>
			<span class="nav-card-desc">Controls & rules</span>
		</a>
	</div>

	{#if gameStore.player.playerName === 'Player'}
		<div class="welcome-prompt">
			<p>Welcome to MazeEscape! Set your name in <a href="{base}/settings">Settings</a> to get started.</p>
		</div>
	{/if}
</div>

<style>
	.landing {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-12);
		padding-top: var(--space-8);
	}

	.hero {
		text-align: center;
		position: relative;
		max-width: 600px;
	}

	.hero-glow {
		position: absolute;
		top: -80px;
		left: 50%;
		transform: translateX(-50%);
		width: 400px;
		height: 400px;
		background: radial-gradient(circle, rgba(99, 102, 241, 0.15) 0%, transparent 70%);
		pointer-events: none;
		z-index: 0;
	}

	.hero-title {
		position: relative;
		z-index: 1;
		font-family: var(--font-display);
		font-size: clamp(2.5rem, 8vw, 4.5rem);
		font-weight: 700;
		letter-spacing: -0.02em;
		line-height: 1.1;
		margin-bottom: var(--space-4);
	}

	.title-maze {
		color: var(--color-accent-secondary);
	}

	.title-escape {
		color: var(--color-text-primary);
	}

	.hero-subtitle {
		position: relative;
		z-index: 1;
		font-size: var(--text-lg);
		color: var(--color-text-secondary);
		max-width: 480px;
		margin: 0 auto var(--space-8);
		line-height: 1.6;
	}

	.hero-actions {
		position: relative;
		z-index: 1;
		display: flex;
		flex-wrap: wrap;
		gap: var(--space-4);
		justify-content: center;
	}

	/* Buttons */
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
		color: white;
		box-shadow: var(--shadow-glow);
	}

	.btn-primary:hover {
		background: var(--color-accent-glow);
		box-shadow: 0 0 30px rgba(99, 102, 241, 0.5);
		transform: translateY(-2px);
		color: white;
	}

	.btn-secondary {
		background: var(--color-bg-card);
		color: var(--color-text-primary);
		border: 1px solid var(--color-border);
	}

	.btn-secondary:hover {
		background: var(--color-bg-card-hover);
		border-color: var(--color-accent-primary);
		transform: translateY(-2px);
		color: var(--color-text-primary);
	}

	.btn-locked {
		opacity: 0.6;
		cursor: default;
	}

	.btn-locked:hover {
		transform: none;
	}

	.lock-badge {
		font-size: var(--text-xs);
		color: var(--color-accent-red);
		position: absolute;
		bottom: -18px;
		left: 50%;
		transform: translateX(-50%);
		white-space: nowrap;
	}

	/* Quick navigation cards */
	.quick-nav {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
		gap: var(--space-4);
		width: 100%;
		max-width: 800px;
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
	}

	.nav-card:hover {
		background: var(--color-bg-card-hover);
		border-color: var(--color-accent-primary);
		transform: translateY(-4px);
		box-shadow: var(--shadow-glow);
	}

	.nav-card-icon {
		font-size: 2rem;
	}

	.nav-card-label {
		font-family: var(--font-display);
		font-weight: 600;
		color: var(--color-text-primary);
	}

	.nav-card-desc {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
	}

	.welcome-prompt {
		background: var(--color-bg-card);
		border: 1px solid var(--color-accent-primary);
		border-radius: var(--radius-lg);
		padding: var(--space-4) var(--space-6);
		text-align: center;
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
	}

	.welcome-prompt a {
		color: var(--color-accent-secondary);
		text-decoration: underline;
	}

	@media (max-width: 480px) {
		.hero-actions {
			flex-direction: column;
			align-items: stretch;
		}
		.btn-large {
			justify-content: center;
		}
		.quick-nav {
			grid-template-columns: repeat(2, 1fr);
		}
	}
</style>
