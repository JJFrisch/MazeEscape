<script lang="ts">
	import '$lib/styles/global.css';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { initializeSupabaseAuth } from '$lib/supabase/auth';
	import { onMount } from 'svelte';
	import { base } from '$app/paths';

	let { children } = $props();

	onMount(() => {
		gameStore.init();
		const cleanupSupabaseAuth = initializeSupabaseAuth();

		return () => {
			cleanupSupabaseAuth();
		};
	});
</script>

<svelte:head>
	<title>MazeEscape</title>
</svelte:head>

<div class="app-shell">
	<header class="app-header">
		<a href="{base}/" class="logo" aria-label="MazeEscape Home">
			<span class="logo-icon">🔮</span>
			<span class="logo-text">MazeEscape</span>
		</a>
		<nav class="header-nav" aria-label="Main navigation">
			<a href="{base}/" class="nav-link">Home</a>
			<a href="{base}/campaign/worlds" class="nav-link">Campaign</a>
			<a href="{base}/daily" class="nav-link">Daily</a>
			<a href="{base}/shop" class="nav-link">Shop</a>
			<a href="{base}/equip" class="nav-link">Equip</a>
			<a href="{base}/settings" class="nav-link">Settings</a>
		</nav>
		<div class="header-stats">
			<span class="stat coin-stat" aria-label="Coins">
				<span class="stat-icon">🪙</span>
				<span class="stat-value">{gameStore.player.coinCount.toLocaleString()}</span>
			</span>
		</div>
	</header>

	<main class="app-main">
		{@render children()}
	</main>
</div>

<style>
	.app-shell {
		display: flex;
		flex-direction: column;
		min-height: 100dvh;
	}

	.app-header {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-3) var(--space-6);
		background: var(--color-bg-secondary);
		border-bottom: 1px solid var(--color-border);
		height: var(--header-height);
		position: sticky;
		top: 0;
		z-index: 100;
	}

	.logo {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		color: var(--color-text-primary);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		text-decoration: none;
		flex-shrink: 0;
	}

	.logo:hover {
		color: var(--color-accent-secondary);
	}

	.logo-icon {
		font-size: var(--text-xl);
	}

	.header-nav {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		flex: 1;
		overflow-x: auto;
	}

	.nav-link {
		padding: var(--space-1) var(--space-3);
		border-radius: var(--radius-md);
		font-size: var(--text-sm);
		font-weight: 500;
		color: var(--color-text-secondary);
		transition: all var(--transition-fast);
		white-space: nowrap;
	}

	.nav-link:hover {
		background: var(--color-bg-card);
		color: var(--color-text-primary);
	}

	.header-stats {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		flex-shrink: 0;
	}

	.stat {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		font-size: var(--text-sm);
		font-weight: 600;
	}

	.coin-stat {
		color: var(--color-accent-gold);
	}

	.app-main {
		flex: 1;
		width: 100%;
		max-width: var(--max-width);
		margin: 0 auto;
		padding: var(--space-6);
	}

	@media (max-width: 640px) {
		.app-header {
			padding: var(--space-2) var(--space-3);
		}
		.logo-text {
			display: none;
		}
		.header-nav {
			gap: 0;
		}
		.nav-link {
			padding: var(--space-1) var(--space-2);
			font-size: var(--text-xs);
		}
		.app-main {
			padding: var(--space-4);
		}
	}
</style>
