<script lang="ts">
	import '$lib/styles/global.css';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { initializeSupabaseAuth } from '$lib/supabase/auth';
	import { authStore } from '$lib/supabase/authStore.svelte';
	import { onMount } from 'svelte';
	import { base } from '$app/paths';

	let { children } = $props();

	onMount(() => {
		gameStore.init();
		const cleanupSupabaseAuth = initializeSupabaseAuth({
			onSignedIn: async (userId) => {
				await gameStore.handleAuthStateChanged(userId);
			},
			onSignedOut: () => {
				gameStore.handleAuthStateChanged(null);
			}
		});

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
			<!-- Inline maze-fragment SVG icon -->
			<svg class="logo-icon" width="28" height="28" viewBox="0 0 28 28" fill="none" aria-hidden="true">
				<rect x="1.5" y="1.5" width="25" height="25" rx="4" stroke="var(--color-accent-primary)" stroke-width="2"/>
				<!-- Maze walls forming an L-spiral -->
				<path d="M7 7h14v5H12v4h9v5H7v-5h5v-4H7z"
					  fill="none" stroke="var(--color-accent-primary)" stroke-width="2"
					  stroke-linejoin="round" stroke-linecap="round"/>
				<!-- Gold centre dot (exit) -->
				<circle cx="19" cy="20" r="2" fill="var(--color-accent-gold)"/>
			</svg>
			<span class="logo-text">MazeEscape</span>
		</a>

		<nav class="header-nav" aria-label="Main navigation">
			<a href="{base}/" class="nav-link">Home</a>
			<a href="{base}/campaign/worlds" class="nav-link">Campaign</a>
			<a href="{base}/daily" class="nav-link">Daily</a>
			<a href="{base}/shop" class="nav-link">Shop</a>
			<a href="{base}/equip" class="nav-link">Equip</a>
			<a href="{base}/settings" class="nav-link">Settings</a>
			<a href="{base}/auth" class="nav-link">{authStore.isAuthenticated ? 'Account' : 'Sign In'}</a>
		</nav>

		<div class="header-stats">
			{#if authStore.user}
				<span class="stat auth-stat" aria-label="Signed in account">
					<!-- cloud sync icon -->
					<svg width="14" height="14" viewBox="0 0 20 20" fill="var(--color-accent-primary)" aria-hidden="true">
						<path d="M16.4 8.2A6 6 0 004.1 9H3a4 4 0 000 8h13a4 4 0 00.4-8z"/>
					</svg>
					<span class="stat-value auth-email">{authStore.user.email}</span>
				</span>
			{/if}
			<span class="stat coin-stat" aria-label="Coins">
				<img src="{base}/images/coin.png" alt="" class="coin-img" aria-hidden="true" />
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

	/* ── Header ─────────────────────────────────── */
	.app-header {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: 0 var(--space-6);
		background: var(--color-bg-secondary);
		border-bottom: 1px solid var(--color-border);
		height: var(--header-height);
		position: sticky;
		top: 0;
		z-index: 100;
		box-shadow: var(--shadow-sm);
	}

	/* ── Logo ───────────────────────────────────── */
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
		transition: opacity var(--transition-fast);
	}

	.logo:hover {
		opacity: 0.78;
		color: var(--color-text-primary);
	}

	.logo-icon {
		flex-shrink: 0;
		filter: drop-shadow(0 0 4px rgba(21, 101, 192, 0.35));
	}

	/* ── Nav ────────────────────────────────────── */
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
		background: var(--color-bg-card-hover);
		color: var(--color-accent-primary);
		border-radius: var(--radius-md);
	}

	/* ── Stats ──────────────────────────────────── */
	.header-stats {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		flex-shrink: 0;
	}

	.auth-email {
		max-width: 180px;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
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
		background: rgba(217, 119, 6, 0.08);
		border: 1px solid rgba(217, 119, 6, 0.22);
		border-radius: var(--radius-full);
		padding: 3px var(--space-3) 3px var(--space-2);
	}

	.coin-img {
		width: 18px;
		height: 18px;
		object-fit: contain;
	}

	/* ── Main ───────────────────────────────────── */
	.app-main {
		flex: 1;
		width: 100%;
		max-width: var(--max-width);
		margin: 0 auto;
		padding: var(--space-6);
	}

	/* ── Responsive ─────────────────────────────── */
	@media (max-width: 640px) {
		.app-header {
			padding: 0 var(--space-3);
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
