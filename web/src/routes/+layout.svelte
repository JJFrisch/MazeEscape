<script lang="ts">
	import '$lib/styles/global.css';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { mazeThemeStore } from '$lib/stores/mazeThemeStore.svelte';
	import { themeStore } from '$lib/stores/themeStore.svelte';
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
		themeStore.init();
		mazeThemeStore.init();
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
	<meta name="description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />

	<!-- Favicons -->
	<link rel="icon" type="image/x-icon" href="/favicon.ico" />
	<link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
	<link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
	<link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
	<link rel="manifest" href="/site.webmanifest" />
	<meta name="theme-color" content="#0a1628" />

	<!-- Open Graph / social sharing -->
	<meta property="og:type" content="website" />
	<meta property="og:title" content="Maze Escape: Pathbound" />
	<meta property="og:description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />
	<meta property="og:image" content="/android-chrome-512x512.png" />

	<!-- Twitter card -->
	<meta name="twitter:card" content="summary" />
	<meta name="twitter:title" content="Maze Escape: Pathbound" />
	<meta name="twitter:description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />
	<meta name="twitter:image" content="/android-chrome-512x512.png" />
</svelte:head>

<div class="app-shell">
	<header class="app-header">
		<div class="header-inner">
			<a href="{base}/" class="logo" aria-label="Maze Escape: Pathbound Home">
				<div class="logo-icon-wrap">
					<svg class="logo-icon" width="22" height="22" viewBox="0 0 28 28" fill="none" aria-hidden="true">
						<rect x="1.5" y="1.5" width="25" height="25" rx="4" stroke="currentColor" stroke-width="2"/>
						<path d="M7 7h14v5H12v4h9v5H7v-5h5v-4H7z"
							  fill="none" stroke="currentColor" stroke-width="2"
							  stroke-linejoin="round" stroke-linecap="round"/>
						<circle cx="19" cy="20" r="2" fill="var(--color-accent-gold)"/>
					</svg>
				</div>
				<span class="logo-text">Maze Escape<span class="logo-sub"> Pathbound</span></span>
			</a>

			<nav class="header-nav" aria-label="Main navigation">
				<a href="{base}/" class="nav-link" class:active={isActive(`${base}/`)}>Home</a>
				<a href="{base}/campaign/worlds" class="nav-link" class:active={isActive(`${base}/campaign`)}>Campaign</a>
				<a href="{base}/daily" class="nav-link" class:active={isActive(`${base}/daily`)}>Daily</a>
				<a href="{base}/leaderboard" class="nav-link" class:active={isActive(`${base}/leaderboard`)}>Leaderboard</a>
				<a href="{base}/shop" class="nav-link" class:active={isActive(`${base}/shop`)}>Shop</a>
				<a href="{base}/equip" class="nav-link" class:active={isActive(`${base}/equip`)}>Equip</a>
				<a href="{base}/codex" class="nav-link" class:active={isActive(`${base}/codex`)}>Codex</a>
				<a href="{base}/stats" class="nav-link" class:active={isActive(`${base}/stats`)}>Chronicles</a>
				<a href="{base}/algorithms" class="nav-link" class:active={isActive(`${base}/algorithms`)}>Algorithms</a>
				<a href="{base}/settings" class="nav-link" class:active={isActive(`${base}/settings`)}>Settings</a>
				<a href="{base}/auth" class="nav-link" class:active={isActive(`${base}/auth`)}>{authStore.isAuthenticated ? 'Account' : 'Sign In'}</a>
				<a href="{base}/download" class="nav-cta" class:active={isActive(`${base}/download`)}>
					<svg width="13" height="13" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
						<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
					</svg>
					Download
				</a>
			</nav>

			<div class="header-right">
				{#if authStore.user}
					<span class="auth-chip" aria-label="Signed in">
						<svg width="12" height="12" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
							<path d="M16.4 8.2A6 6 0 004.1 9H3a4 4 0 000 8h13a4 4 0 00.4-8z"/>
						</svg>
						<span class="auth-email">{authStore.user.email}</span>
					</span>
				{/if}
				<a href="{base}/shop" class="coin-chip" aria-label="Coins — go to shop">
					<img src="{base}/images/coin.png" alt="" class="coin-img" aria-hidden="true" />
					<span class="coin-value">{gameStore.player.coinCount.toLocaleString()}</span>
				</a>
				<button
					class="theme-toggle"
					onclick={() => themeStore.toggle()}
					aria-label={themeStore.theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
					title={themeStore.theme === 'dark' ? 'Light mode' : 'Dark mode'}
				>
					{#if themeStore.theme === 'dark'}
						<!-- Sun icon -->
						<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
							<circle cx="12" cy="12" r="4"/>
							<line x1="12" y1="2" x2="12" y2="5"/>
							<line x1="12" y1="19" x2="12" y2="22"/>
							<line x1="4.22" y1="4.22" x2="6.34" y2="6.34"/>
							<line x1="17.66" y1="17.66" x2="19.78" y2="19.78"/>
							<line x1="2" y1="12" x2="5" y2="12"/>
							<line x1="19" y1="12" x2="22" y2="12"/>
							<line x1="4.22" y1="19.78" x2="6.34" y2="17.66"/>
							<line x1="17.66" y1="6.34" x2="19.78" y2="4.22"/>
						</svg>
					{:else}
						<!-- Moon icon -->
						<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
							<path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/>
						</svg>
					{/if}
				</button>
			</div>
		</div>
	</header>

	<main class="app-main">
		{@render children()}
	</main>

	<footer class="app-footer">
		<div class="footer-inner">
			<div class="footer-brand">
				<div class="footer-logo">
					<svg width="18" height="18" viewBox="0 0 28 28" fill="none" aria-hidden="true">
						<rect x="1.5" y="1.5" width="25" height="25" rx="4" stroke="var(--color-accent-primary)" stroke-width="2"/>
						<path d="M7 7h14v5H12v4h9v5H7v-5h5v-4H7z" fill="none" stroke="var(--color-accent-primary)" stroke-width="2" stroke-linejoin="round" stroke-linecap="round"/>
						<circle cx="19" cy="20" r="2" fill="var(--color-accent-gold)"/>
					</svg>
					<span class="footer-brand-name">Maze Escape: Pathbound</span>
				</div>
				<p class="footer-brand-desc">A procedural puzzle maze game.</p>
				<p class="footer-credit">Designed and built by <a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer">Jake Frischmann</a>.</p>
			</div>
			<div class="footer-links">
				<div class="footer-col">
					<span class="footer-heading">Play</span>
					<a href="{base}/" class="footer-link">Home</a>
					<a href="{base}/campaign/worlds" class="footer-link">Campaign</a>
					<a href="{base}/daily" class="footer-link">Daily Maze</a>
					<a href="{base}/leaderboard" class="footer-link">Leaderboard</a>
					<a href="{base}/codex" class="footer-link">Codex</a>
					<a href="{base}/stats" class="footer-link">Chronicles</a>
					<a href="{base}/download" class="footer-link">Desktop App</a>
				</div>
				<div class="footer-col">
					<span class="footer-heading">Legal</span>
					<a href="{base}/privacy" class="footer-link">Privacy Policy</a>
					<a href="{base}/terms" class="footer-link">Terms of Service</a>
				</div>
				<div class="footer-col">
					<span class="footer-heading">About</span>
					<span>Version 1.0.0</span>
					<a href="mailto:contact@mazeescapepathbound.com" class="footer-link">contact@mazeescapepathbound.com</a>
					<a href="https://jakefrischmann.me" class="footer-link" target="_blank" rel="noopener noreferrer">jakefrischman.me</a>
					<a href="https://github.com/JJFrisch/MazeEscape" class="footer-link" target="_blank" rel="noopener noreferrer">GitHub</a>
				</div>
			</div>
		</div>
		<div class="footer-bottom">
			<span>© 2026 Jake Frischmann · <a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer">jakefrischman.me</a></span>
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
		position: sticky;
		top: 0;
		z-index: 200;
		height: var(--header-height);
		background: var(--color-bg-header);
		backdrop-filter: blur(20px) saturate(180%);
		-webkit-backdrop-filter: blur(20px) saturate(180%);
		border-bottom: 1px solid var(--color-border);
		box-shadow: 0 1px 0 var(--color-border), var(--shadow-sm);
	}

	.header-inner {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		height: 100%;
		max-width: var(--max-width);
		margin: 0 auto;
		padding: 0 var(--space-6);
	}

	/* ── Logo ───────────────────────────────────── */
	.logo {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		color: var(--color-text-primary);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		text-decoration: none;
		flex-shrink: 0;
		transition: opacity var(--transition-fast);
	}
	.logo:hover { opacity: 0.8; color: var(--color-text-primary); }

	.logo-icon-wrap {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 32px;
		height: 32px;
		background: var(--color-logo-icon-bg);
		border: 1px solid var(--color-logo-icon-border);
		border-radius: var(--radius-md);
		color: var(--color-accent-primary);
		flex-shrink: 0;
		transition: all var(--transition-base);
	}
	.logo:hover .logo-icon-wrap {
		background: var(--color-border-bright);
		border-color: var(--color-accent-primary);
		box-shadow: var(--shadow-glow);
	}

	.logo-text {
		white-space: nowrap;
	}
	.logo-sub {
		font-weight: 500;
		color: var(--color-accent-gold);
		margin-left: 2px;
	}

	/* ── Nav ────────────────────────────────────── */
	.header-nav {
		display: flex;
		align-items: center;
		gap: 2px;
		flex: 1;
		overflow-x: auto;
		scrollbar-width: none;
	}
	.header-nav::-webkit-scrollbar { display: none; }

	.nav-link {
		position: relative;
		padding: 6px 10px;
		border-radius: var(--radius-md);
		font-size: var(--text-sm);
		font-weight: 500;
		color: var(--color-nav-link);
		text-decoration: none;
		transition: color var(--transition-fast), background var(--transition-fast);
		white-space: nowrap;
	}
	.nav-link:hover {
		color: var(--color-text-primary);
		background: var(--color-nav-link-hover-bg);
	}
	.nav-link.active {
		color: var(--color-accent-primary);
		background: color-mix(in srgb, var(--color-accent-primary) 10%, transparent);
	}
	.nav-link.active::after {
		content: '';
		position: absolute;
		bottom: -1px;
		left: 10px;
		right: 10px;
		height: 2px;
		background: var(--color-accent-primary);
		border-radius: 1px 1px 0 0;
		box-shadow: 0 0 8px var(--color-accent-glow);
	}

	.nav-cta {
		display: inline-flex;
		align-items: center;
		gap: var(--space-1);
		padding: 6px 14px;
		margin-left: var(--space-2);
		background: rgba(245, 158, 11, 0.12);
		border: 1px solid rgba(245, 158, 11, 0.35);
		border-radius: var(--radius-md);
		font-size: var(--text-sm);
		font-weight: 600;
		color: var(--color-accent-gold);
		text-decoration: none;
		transition: all var(--transition-fast);
		white-space: nowrap;
		flex-shrink: 0;
	}
	.nav-cta:hover {
		background: rgba(245, 158, 11, 0.2);
		border-color: rgba(245, 158, 11, 0.6);
		color: #fcd34d;
		box-shadow: 0 0 16px rgba(245, 158, 11, 0.2);
	}

	/* ── Header right ───────────────────────────── */
	.header-right {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		flex-shrink: 0;
	}

	.auth-chip {
		display: flex;
		align-items: center;
		gap: 5px;
		padding: 4px 10px 4px 8px;
		background: color-mix(in srgb, var(--color-accent-primary) 8%, transparent);
		border: 1px solid color-mix(in srgb, var(--color-accent-primary) 22%, transparent);
		border-radius: var(--radius-full);
		font-size: 11px;
		font-weight: 500;
		color: color-mix(in srgb, var(--color-accent-primary) 82%, white 18%);
	}
	.auth-email {
		max-width: 140px;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.coin-chip {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		padding: 5px 12px 5px 8px;
		background: rgba(245, 158, 11, 0.08);
		border: 1px solid rgba(245, 158, 11, 0.25);
		border-radius: var(--radius-full);
		text-decoration: none;
		transition: all var(--transition-fast);
	}
	.coin-chip:hover {
		background: rgba(245, 158, 11, 0.14);
		border-color: rgba(245, 158, 11, 0.45);
	}
	.coin-img {
		width: 16px;
		height: 16px;
		object-fit: contain;
	}
	.coin-value {
		font-size: var(--text-sm);
		font-weight: 700;
		color: var(--color-accent-gold);
		font-family: var(--font-display);
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
		background: var(--color-bg-footer);
		border-top: 1px solid var(--color-border);
		padding: var(--space-12) var(--space-6) var(--space-6);
	}

	.footer-inner {
		display: flex;
		gap: var(--space-12);
		max-width: var(--max-width);
		margin: 0 auto var(--space-8);
		flex-wrap: wrap;
	}

	.footer-brand {
		flex: 2;
		min-width: 200px;
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.footer-logo {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		margin-bottom: var(--space-1);
	}

	.footer-brand-name {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: var(--color-text-primary);
	}

	.footer-brand-desc,
	.footer-credit {
		font-size: var(--text-sm);
		color: var(--color-footer-muted);
		line-height: 1.5;
	}

	.footer-credit a {
		color: var(--color-footer-bottom-link);
	}
	.footer-credit a:hover { color: var(--color-accent-primary); }

	.footer-links {
		display: flex;
		gap: var(--space-10);
		flex-wrap: wrap;
	}

	.footer-col {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
		min-width: 100px;
	}

	.footer-heading {
		font-family: var(--font-display);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: var(--color-footer-heading);
		margin-bottom: var(--space-1);
	}

	.footer-link {
		font-size: var(--text-sm);
		color: var(--color-footer-link);
		text-decoration: none;
		transition: color var(--transition-fast);
		width: fit-content;
	}
	.footer-link:hover { color: var(--color-accent-primary); }

	.footer-bottom {
		display: flex;
		justify-content: space-between;
		align-items: center;
		max-width: var(--max-width);
		margin: 0 auto;
		padding-top: var(--space-6);
		border-top: 1px solid var(--color-border-subtle);
		font-size: var(--text-xs);
		color: var(--color-footer-bottom-text);
		flex-wrap: wrap;
		gap: var(--space-2);
	}
	.footer-bottom a {
		color: var(--color-footer-bottom-link);
	}
	.footer-bottom a:hover { color: var(--color-accent-primary); }

	/* ── Theme toggle ───────────────────────────── */
	.theme-toggle {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 32px;
		height: 32px;
		padding: 0;
		background: var(--color-bg-glass);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		color: var(--color-text-secondary);
		cursor: pointer;
		transition: all var(--transition-fast);
		flex-shrink: 0;
	}
	.theme-toggle:hover {
		background: var(--color-bg-glass-hover);
		border-color: var(--color-border-bright);
		color: var(--color-accent-primary);
	}

	/* ── Responsive ─────────────────────────────── */
	@media (max-width: 900px) {
		.logo-sub { display: none; }
	}
	@media (max-width: 640px) {
		.header-inner { padding: 0 var(--space-3); }
		.logo-text { display: none; }
		.header-nav { gap: 0; }
		.nav-link { padding: 6px 8px; font-size: var(--text-xs); }
		.app-main { padding: var(--space-4); }
		.footer-inner { flex-direction: column; gap: var(--space-8); }
	}
</style>
