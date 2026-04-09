<script lang="ts">
	import '$lib/styles/global.css';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { initializeSupabaseAuth } from '$lib/supabase/auth';
	import { authStore } from '$lib/supabase/authStore.svelte';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import { base } from '$app/paths';

	let { children } = $props();

	const path = $derived($page.url.pathname);
	function isActive(href: string): boolean {
		if (href === `${base}/`) return path === `${base}/` || path === '/';
		return path.startsWith(href);
	}

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
	<title>Maze Escape: Pathbound</title>
</svelte:head>

<div class="app-shell">
	<header class="app-header">
		<a href="{base}/" class="logo" aria-label="Maze Escape: Pathbound Home">
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
			<span class="logo-text">Maze Escape: Pathbound</span>
		</a>

		<nav class="header-nav" aria-label="Main navigation">
			<a href="{base}/" class="nav-link" class:active={isActive(`${base}/`)}>Home</a>
			<a href="{base}/campaign/worlds" class="nav-link" class:active={isActive(`${base}/campaign`)}>Campaign</a>
			<a href="{base}/daily" class="nav-link" class:active={isActive(`${base}/daily`)}>Daily</a>
			<a href="{base}/shop" class="nav-link" class:active={isActive(`${base}/shop`)}>Shop</a>
			<a href="{base}/equip" class="nav-link" class:active={isActive(`${base}/equip`)}>Equip</a>
			<a href="{base}/settings" class="nav-link" class:active={isActive(`${base}/settings`)}>Settings</a>
			<a href="{base}/auth" class="nav-link" class:active={isActive(`${base}/auth`)}>{authStore.isAuthenticated ? 'Account' : 'Sign In'}</a>
			<a href="{base}/download" class="nav-link nav-link-download" class:active={isActive(`${base}/download`)}>Download</a>
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

	<footer class="app-footer">
		<div class="footer-inner">
			<div class="footer-col footer-col-brand">
				<span class="footer-brand-name">Maze Escape: Pathbound</span>
				<p class="footer-brand-desc">A procedural puzzle maze game.</p>
				<p class="footer-credit">
					Designed and built by
					<a href="https://jakefrischman.me" class="footer-link" target="_blank" rel="noopener noreferrer">Jake Frischmann</a>.
				</p>
			</div>
			<div class="footer-col">
				<span class="footer-heading">Play</span>
				<a href="{base}/" class="footer-link">Home</a>
				<a href="{base}/campaign/worlds" class="footer-link">Campaign</a>
				<a href="{base}/daily" class="footer-link">Daily Maze</a>
				<a href="{base}/download" class="footer-link">Download Desktop App</a>
			</div>
			<div class="footer-col">
				<span class="footer-heading">Legal</span>
				<a href="{base}/privacy" class="footer-link">Privacy Policy</a>
				<a href="{base}/terms" class="footer-link">Terms of Service</a>
			</div>
			<div class="footer-col">
				<span class="footer-heading">About</span>
				<a href="https://jakefrischman.me" class="footer-link" target="_blank" rel="noopener noreferrer">jakefrischman.me</a>
				<a href="https://github.com/JJFrisch/MazeEscape" class="footer-link" target="_blank" rel="noopener noreferrer">GitHub</a>
			</div>
		</div>
		<div class="footer-bottom">
			<span>© 2026 Jake Frischmann · <a href="https://jakefrischman.me" class="footer-link-inline" target="_blank" rel="noopener noreferrer">jakefrischman.me</a></span>
			<span>mazeescapepathbound.com</span>
		</div>
	</footer>
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
		background: #0a1628;
		border-bottom: 1px solid rgba(56, 189, 248, 0.15);
		height: var(--header-height);
		position: sticky;
		top: 0;
		z-index: 100;
		box-shadow: 0 1px 12px rgba(0, 0, 0, 0.4);
	}

	/* ── Logo ───────────────────────────────────── */
	.logo {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		color: #f0f6ff;
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		text-decoration: none;
		flex-shrink: 0;
		transition: opacity var(--transition-fast);
	}

	.logo:hover {
		opacity: 0.78;
		color: #f0f6ff;
	}

	.logo-icon {
		flex-shrink: 0;
		filter: drop-shadow(0 0 6px rgba(56, 189, 248, 0.5));
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
		position: relative;
		padding: var(--space-1) var(--space-3);
		border-radius: var(--radius-md);
		font-size: var(--text-sm);
		font-weight: 500;
		color: rgba(255, 255, 255, 0.55);
		text-decoration: none;
		transition: color var(--transition-fast);
		white-space: nowrap;
	}

	.nav-link:hover {
		color: #f0f6ff;
	}

	.nav-link.active {
		color: #38bdf8;
	}

	.nav-link.active::after {
		content: '';
		position: absolute;
		bottom: -2px;
		left: var(--space-3);
		right: var(--space-3);
		height: 2px;
		background: #38bdf8;
		border-radius: 1px;
		box-shadow: 0 0 6px rgba(56, 189, 248, 0.6);
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
		color: rgba(255, 255, 255, 0.7);
	}

	.coin-stat {
		color: #fbbf24;
		background: rgba(251, 191, 36, 0.08);
		border: 1px solid rgba(251, 191, 36, 0.25);
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

	/* ── Footer ─────────────────────────────────── */
	.app-footer {
		background: #060d1a;
		border-top: 1px solid rgba(56, 189, 248, 0.12);
		padding: var(--space-10) var(--space-6) var(--space-6);
		margin-top: auto;
	}

	.footer-inner {
		display: grid;
		grid-template-columns: 2fr 1fr 1fr 1fr;
		gap: var(--space-8);
		max-width: var(--max-width);
		margin: 0 auto var(--space-8);
	}

	.footer-col {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.footer-brand-name {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: #f0f6ff;
		margin-bottom: var(--space-1);
	}

	.footer-brand-desc {
		font-size: var(--text-sm);
		color: rgba(255, 255, 255, 0.4);
		line-height: 1.5;
	}

	.footer-credit {
		font-size: var(--text-sm);
		color: rgba(255, 255, 255, 0.35);
		line-height: 1.5;
		margin-top: var(--space-1);
	}

	.footer-heading {
		font-family: var(--font-display);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: rgba(255, 255, 255, 0.3);
		margin-bottom: var(--space-1);
	}

	.footer-link {
		font-size: var(--text-sm);
		color: rgba(255, 255, 255, 0.5);
		text-decoration: none;
		transition: color var(--transition-fast);
		width: fit-content;
	}

	.footer-link:hover {
		color: #38bdf8;
	}

	.footer-bottom {
		display: flex;
		justify-content: space-between;
		align-items: center;
		max-width: var(--max-width);
		margin: 0 auto;
		padding-top: var(--space-6);
		border-top: 1px solid rgba(255, 255, 255, 0.06);
		font-size: var(--text-xs);
		color: rgba(255, 255, 255, 0.25);
		flex-wrap: wrap;
		gap: var(--space-2);
	}

	.footer-link-inline {
		color: rgba(56, 189, 248, 0.6);
		text-decoration: none;
		transition: color var(--transition-fast);
	}

	.footer-link-inline:hover {
		color: #38bdf8;
	}

	.nav-link-download {
		color: #d97706 !important;
		border: 1px solid rgba(217, 119, 6, 0.35);
		border-radius: var(--radius-md);
	}

	.nav-link-download:hover {
		background: rgba(217, 119, 6, 0.12);
		color: #fbbf24 !important;
	}

	@media (max-width: 768px) {
		.footer-inner {
			grid-template-columns: 1fr 1fr;
		}
		.footer-col-brand {
			grid-column: 1 / -1;
		}
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
