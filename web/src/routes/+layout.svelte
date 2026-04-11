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
	const isHome = $derived(path === `${base}/` || path === '/' || path === `${base}`);
	const isLegalPage = $derived(
		path.includes('/privacy') ||
		path.includes('/terms') ||
		path.includes('/download') ||
		path.includes('/info')
	);

	// Zone label for the breadcrumb shown on inner pages
	const zoneLabel = $derived((() => {
		if (path.includes('/campaign'))   return { icon: '⚔️', name: 'The Dungeon' };
		if (path.includes('/daily'))      return { icon: '🌀', name: 'The Spire' };
		if (path.includes('/leaderboard'))return { icon: '🏆', name: 'The Arena' };
		if (path.includes('/shop'))       return { icon: '💎', name: 'The Vault' };
		if (path.includes('/equip'))      return { icon: '💎', name: 'The Vault' };
		if (path.includes('/codex'))      return { icon: '📜', name: 'The Archives' };
		if (path.includes('/algorithms')) return { icon: '📜', name: 'The Archives' };
		if (path.includes('/stats'))      return { icon: '📜', name: 'The Archives' };
		if (path.includes('/settings'))   return { icon: '🔮', name: 'The Sanctum' };
		if (path.includes('/auth'))       return { icon: '🔮', name: 'The Sanctum' };
		return null;
	})());

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
		return () => { cleanupSupabaseAuth(); };
	});
</script>

<svelte:head>
	<title>Maze Escape: Pathbound</title>
	<meta name="description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />
	<link rel="icon" type="image/x-icon" href="/favicon.ico" />
	<link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
	<link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
	<link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
	<link rel="manifest" href="/site.webmanifest" />
	<meta name="theme-color" content="#0a1628" />
	<meta property="og:type" content="website" />
	<meta property="og:title" content="Maze Escape: Pathbound" />
	<meta property="og:description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />
	<meta property="og:image" content="/android-chrome-512x512.png" />
	<meta name="twitter:card" content="summary" />
	<meta name="twitter:title" content="Maze Escape: Pathbound" />
	<meta name="twitter:description" content="A puzzle maze adventure game. Solve intricate mazes, collect stars, and conquer campaign worlds." />
	<meta name="twitter:image" content="/android-chrome-512x512.png" />
</svelte:head>

<div class="app-shell">
	<!-- ── Slim Header ─────────────────────────────────────────────── -->
	<header class="app-header">
		<div class="header-inner">

			<!-- Logo -->
			<a href="{base}/" class="logo" aria-label="Return to Map">
				<div class="logo-icon-wrap">
					<svg class="logo-icon" width="18" height="18" viewBox="0 0 28 28" fill="none" aria-hidden="true">
						<rect x="1.5" y="1.5" width="25" height="25" rx="4" stroke="currentColor" stroke-width="2"/>
						<path d="M7 7h14v5H12v4h9v5H7v-5h5v-4H7z" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" stroke-linecap="round"/>
						<circle cx="19" cy="20" r="2" fill="var(--color-accent-gold)"/>
					</svg>
				</div>
				<span class="logo-text">Maze Escape<span class="logo-sub"> Pathbound</span></span>
			</a>

			<!-- Zone breadcrumb (inner pages only) -->
			{#if !isHome && zoneLabel}
				<div class="zone-breadcrumb">
					<svg width="12" height="12" viewBox="0 0 16 16" fill="none" aria-hidden="true">
						<path d="M6 3l5 5-5 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
					</svg>
					<span class="zone-label">{zoneLabel.icon} {zoneLabel.name}</span>
				</div>
			{/if}

			<div class="header-spacer"></div>

			<!-- Right side -->
			<div class="header-right">
				<!-- Coins -->
				<a href="{base}/shop" class="coin-chip" aria-label="Coins — go to shop">
					<img src="{base}/images/coin.png" alt="" class="coin-img" aria-hidden="true" />
					<span class="coin-value">{gameStore.player.coinCount.toLocaleString()}</span>
				</a>

				<!-- Account -->
				<a
					href="{base}/auth"
					class="account-btn"
					aria-label={authStore.user ? 'Account' : 'Sign In'}
					title={authStore.user ? (authStore.user.email ?? 'Account') : 'Sign In'}
				>
					{#if authStore.user}
						<svg width="14" height="14" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
							<path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd"/>
						</svg>
					{:else}
						<svg width="14" height="14" viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
							<path d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" stroke-linecap="round" stroke-linejoin="round"/>
						</svg>
					{/if}
				</a>

				<!-- Theme toggle -->
				<button
					class="theme-toggle"
					onclick={() => themeStore.toggle()}
					aria-label={themeStore.theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
				>
					{#if themeStore.theme === 'dark'}
						<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
							<circle cx="12" cy="12" r="4"/>
							<line x1="12" y1="2" x2="12" y2="5"/><line x1="12" y1="19" x2="12" y2="22"/>
							<line x1="4.22" y1="4.22" x2="6.34" y2="6.34"/><line x1="17.66" y1="17.66" x2="19.78" y2="19.78"/>
							<line x1="2" y1="12" x2="5" y2="12"/><line x1="19" y1="12" x2="22" y2="12"/>
							<line x1="4.22" y1="19.78" x2="6.34" y2="17.66"/><line x1="17.66" y1="6.34" x2="19.78" y2="4.22"/>
						</svg>
					{:else}
						<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
							<path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/>
						</svg>
					{/if}
				</button>
			</div>

		</div>
	</header>

	<!-- ── Main content ──────────────────────────────────────────── -->
	<main class="app-main" class:app-main-map={isHome}>
		{@render children()}
	</main>

	<!-- ── Footer (legal / info pages only) ─────────────────────── -->
	{#if isLegalPage}
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
						<span class="footer-heading">Legal</span>
						<a href="{base}/privacy" class="footer-link">Privacy Policy</a>
						<a href="{base}/terms" class="footer-link">Terms of Service</a>
					</div>
					<div class="footer-col">
						<span class="footer-heading">About</span>
						<span>Version 1.0.0</span>
						<a href="mailto:contact@mazeescapepathbound.com" class="footer-link">contact@mazeescapepathbound.com</a>
						<a href="https://github.com/JJFrisch/MazeEscape" class="footer-link" target="_blank" rel="noopener noreferrer">GitHub</a>
					</div>
				</div>
			</div>
			<div class="footer-bottom">
				<span>© 2026 Jake Frischmann · <a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer">jakefrischmann.me</a></span>
				<span>mazeescapepathbound.com</span>
			</div>
		</footer>
	{/if}
</div>

<style>
	.app-shell {
		display: flex;
		flex-direction: column;
		min-height: 100dvh;
	}

	/* ── Header ──────────────────────────────────────────────────── */
	.app-header {
		position: sticky;
		top: 0;
		z-index: 200;
		height: var(--header-height);
		background: var(--color-bg-header);
		backdrop-filter: blur(20px) saturate(180%);
		-webkit-backdrop-filter: blur(20px) saturate(180%);
		border-bottom: 1px solid var(--color-border);
	}

	.header-inner {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		height: 100%;
		padding: 0 var(--space-5);
	}

	/* Logo */
	.logo {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		color: var(--color-text-primary);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		text-decoration: none;
		flex-shrink: 0;
		transition: opacity var(--transition-fast);
	}
	.logo:hover { opacity: 0.75; color: var(--color-text-primary); }

	.logo-icon-wrap {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 28px;
		height: 28px;
		background: var(--color-logo-icon-bg);
		border: 1px solid var(--color-logo-icon-border);
		border-radius: var(--radius-md);
		color: var(--color-accent-primary);
		flex-shrink: 0;
		transition: all var(--transition-base);
	}
	.logo:hover .logo-icon-wrap {
		border-color: var(--color-accent-primary);
		box-shadow: var(--shadow-glow);
	}

	.logo-text { white-space: nowrap; }
	.logo-sub {
		font-weight: 500;
		color: var(--color-accent-gold);
		margin-left: 1px;
	}

	/* Zone breadcrumb */
	.zone-breadcrumb {
		display: flex;
		align-items: center;
		gap: 5px;
		color: var(--color-text-muted);
		font-size: var(--text-xs);
		font-weight: 500;
	}
	.zone-label {
		color: var(--color-text-secondary);
		font-family: var(--font-display);
		letter-spacing: 0.02em;
	}

	.header-spacer { flex: 1; }

	/* Right cluster */
	.header-right {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		flex-shrink: 0;
	}

	.coin-chip {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		padding: 4px 10px 4px 7px;
		background: rgba(245, 158, 11, 0.08);
		border: 1px solid rgba(245, 158, 11, 0.25);
		border-radius: var(--radius-full);
		text-decoration: none;
		transition: all var(--transition-fast);
	}
	.coin-chip:hover {
		background: rgba(245, 158, 11, 0.15);
		border-color: rgba(245, 158, 11, 0.45);
	}
	.coin-img { width: 14px; height: 14px; object-fit: contain; }
	.coin-value {
		font-size: var(--text-xs);
		font-weight: 700;
		color: var(--color-accent-gold);
		font-family: var(--font-display);
	}

	.account-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 30px;
		height: 30px;
		background: var(--color-bg-glass);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		color: var(--color-text-secondary);
		text-decoration: none;
		transition: all var(--transition-fast);
	}
	.account-btn:hover {
		background: var(--color-bg-glass-hover);
		border-color: var(--color-accent-primary);
		color: var(--color-accent-primary);
	}

	.theme-toggle {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 30px;
		height: 30px;
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

	/* ── Main ─────────────────────────────────────────────────── */
	.app-main {
		flex: 1;
		width: 100%;
		max-width: var(--max-width);
		margin: 0 auto;
		padding: var(--space-6);
	}
	.app-main-map {
		max-width: 100%;
		padding: 0;
		display: flex;
		flex-direction: column;
	}

	/* ── Footer ──────────────────────────────────────────────── */
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
	.footer-brand-desc, .footer-credit {
		font-size: var(--text-sm);
		color: var(--color-footer-muted);
	}
	.footer-credit a { color: var(--color-footer-bottom-link); }
	.footer-credit a:hover { color: var(--color-accent-primary); }
	.footer-links { display: flex; gap: var(--space-10); flex-wrap: wrap; }
	.footer-col { display: flex; flex-direction: column; gap: var(--space-2); min-width: 100px; }
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
	.footer-bottom a { color: var(--color-footer-bottom-link); }
	.footer-bottom a:hover { color: var(--color-accent-primary); }

	/* ── Responsive ───────────────────────────────────────────── */
	@media (max-width: 640px) {
		.logo-sub { display: none; }
		.zone-breadcrumb { display: none; }
	}
</style>
