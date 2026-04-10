<script lang="ts">
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import type { PowerupName } from '$lib/core/types';

	let buyMsg = $state('');
	let buyTimeout: ReturnType<typeof setTimeout>;

	function getOwned(name: PowerupName): number {
		const p = gameStore.player;
		switch (name) {
			case 'hint':             return p.hintsOwned;
			case 'extraTime':        return p.extraTimesOwned;
			case 'extraMoves':       return p.extraMovesOwned;
			case 'compass':          return p.compassOwned ?? 0;
			case 'hourglass':        return p.hourglassOwned ?? 0;
			case 'blinkScroll':      return p.blinkScrollsOwned ?? 0;
			case 'streakShield':     return p.streakShieldsOwned ?? 0;
			case 'doubleCoinsToken': return p.doubleCoinsTokensOwned ?? 0;
		}
	}

	function buyItem(name: PowerupName, cost: number) {
		if (gameStore.spendCoins(cost)) {
			gameStore.addPowerup(name);
			const purchased = gameStore.shopCatalog.find((item) => item.name === name);
			if (purchased?.tags?.includes('event') && gameStore.activeEvent) {
				gameStore.incrementEventProgress(gameStore.activeEvent.id, 1);
			}
			clearTimeout(buyTimeout);
			buyMsg = `Purchased!`;
			buyTimeout = setTimeout(() => (buyMsg = ''), 2000);
		}
	}

	const RARITY_STYLES: Record<string, { accent: string; border: string; glow: string; label: string }> = {
		common: {
			accent: 'rgba(148,163,184,0.10)',
			border: 'rgba(148,163,184,0.28)',
			glow:   'rgba(148,163,184,0.18)',
			label:  '#94a3b8',
		},
		uncommon: {
			accent: 'rgba(52,211,153,0.10)',
			border: 'rgba(52,211,153,0.30)',
			glow:   'rgba(52,211,153,0.18)',
			label:  '#34d399',
		},
		rare: {
			accent: 'rgba(56,189,248,0.12)',
			border: 'rgba(56,189,248,0.35)',
			glow:   'rgba(56,189,248,0.22)',
			label:  '#38bdf8',
		},
	};
</script>

<svelte:head>
	<title>Shop – Maze Escape: Pathbound</title>
</svelte:head>

<div class="shop-page">
	{#if buyMsg}
		<div class="toast">{buyMsg}</div>
	{/if}

	<!-- Header -->
	<div class="shop-header">
		<div class="shop-header-left">
			<div class="page-eyebrow">
				<span class="eyebrow-dot"></span>
				Adventurer's Supply
			</div>
			<h1 class="page-title">Shop</h1>
			<p class="page-sub">Stock your satchel. The labyrinth waits for no one.</p>
		</div>
		<div class="balance-card">
			<span class="balance-label">Balance</span>
			<div class="balance-amount">
				<img src="/images/coin.png" alt="" class="balance-coin" aria-hidden="true" />
				<span>{gameStore.player.coinCount.toLocaleString()}</span>
			</div>
			{#if (gameStore.player.gemCount ?? 0) > 0}
				<div class="gem-balance">
					<span class="gem-icon">💎</span>
					<span>{gameStore.player.gemCount}</span>
				</div>
			{/if}
		</div>
	</div>

	<!-- Active effects banner -->
	{#if gameStore.activeEvent}
		<div class="event-banner" style="--event-accent:{gameStore.activeEvent.themeAccent}">
			<div>
				<strong>{gameStore.activeEvent.name}</strong>
				<span>{gameStore.activeEvent.description}</span>
			</div>
		</div>
	{/if}

	{#if gameStore.player.doubleCoinsActive}
		<div class="active-effect-banner">
			<span class="effect-icon">🪙🪙</span>
			<div>
				<strong>Double Coins Active</strong>
				<span>Your next maze completion earns 2× coins.</span>
			</div>
		</div>
	{/if}

	<!-- Section: Consumables -->
	<div class="section-header">
		<h2 class="section-title">Powerups</h2>
		<p class="section-sub">Consumable items used during maze runs.</p>
	</div>

	<div class="items-grid">
		{#each gameStore.shopCatalog as item}
			{@const rs = RARITY_STYLES[item.rarity]}
			{@const owned = getOwned(item.name)}
			{@const canAfford = gameStore.player.coinCount >= item.cost}
			<div
				class="shop-card"
				style="--card-accent:{rs.accent}; --card-border:{rs.border}; --card-glow:{rs.glow};"
			>
				<!-- Rarity ribbon -->
				<div class="rarity-ribbon" style="color:{rs.label}; border-color:{rs.border}; background:{rs.accent};">
					{item.rarity}
				</div>

				<div class="card-emoji-wrap" style="background:{rs.accent}; border-color:{rs.border};">
					<span class="card-emoji" role="img" aria-hidden="true">{item.icon}</span>
				</div>

				<div class="card-body">
					<h3 class="card-title">{item.displayName}</h3>
					<p class="card-desc">{item.description}</p>
					<p class="card-flavor">{item.flavorText}</p>
				</div>

				<div class="card-footer">
					<div class="owned-badge">
						<svg width="11" height="11" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
							<path d="M20 7H4a2 2 0 00-2 2v10a2 2 0 002 2h16a2 2 0 002-2V9a2 2 0 00-2-2zM4 5h16v2H4V5z"/>
						</svg>
						Owned: {owned}
					</div>
					<button
						class="buy-btn"
						class:cant-afford={!canAfford}
						disabled={!canAfford}
						onclick={() => buyItem(item.name, item.cost)}
					>
						<img src="/images/coin.png" alt="" class="btn-coin" aria-hidden="true" />
						{item.cost.toLocaleString()}
					</button>
				</div>
			</div>
		{/each}
	</div>
</div>

<style>
	@keyframes fade-up {
		from { opacity: 0; transform: translateY(16px); }
		to   { opacity: 1; transform: translateY(0); }
	}
	@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }
	@keyframes shimmer {
		0%   { background-position: -200% 0; }
		100% { background-position:  200% 0; }
	}

	.event-banner {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-3);
		padding: var(--space-4) var(--space-5);
		margin-bottom: var(--space-5);
		border-radius: var(--radius-xl);
		background: linear-gradient(135deg, color-mix(in srgb, var(--event-accent) 18%, transparent), rgba(15, 23, 42, 0.75));
		border: 1px solid color-mix(in srgb, var(--event-accent) 35%, transparent);
	}

	.event-banner strong {
		display: block;
		margin-bottom: 0.25rem;
		color: #f8fafc;
	}

	.event-banner span {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
	}

	.shop-page {
		max-width: 960px;
		margin: 0 auto;
		animation: fade-up 0.4s ease both;
		padding: clamp(var(--space-2), 1vw, var(--space-4));
		background-image:
			radial-gradient(circle at top right, color-mix(in srgb, var(--color-accent-primary) 8%, transparent) 0%, transparent 34%),
			linear-gradient(180deg, color-mix(in srgb, var(--color-accent-secondary) 3%, transparent) 0%, transparent 100%);
	}

	/* ── Header ─────────────────────────────────── */
	.shop-header {
		display: flex;
		align-items: flex-end;
		justify-content: space-between;
		gap: var(--space-6);
		margin-bottom: var(--space-8);
		flex-wrap: wrap;
	}

	.page-eyebrow {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 5px 14px;
		background: color-mix(in srgb, var(--color-accent-primary) 10%, transparent);
		border: 1px solid color-mix(in srgb, var(--color-accent-primary) 24%, transparent);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--color-accent-primary) 72%, white 28%);
		margin-bottom: var(--space-3);
	}
	.eyebrow-dot {
		width: 6px; height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 6px var(--color-accent-primary);
		animation: pulse 2s ease-in-out infinite;
	}
	.page-title {
		font-family: var(--font-display);
		font-size: clamp(1.8rem, 4vw, 2.5rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		margin-bottom: var(--space-2);
	}
	.page-sub { color: var(--color-text-secondary); font-size: var(--text-base); }

	/* Balance card */
	.balance-card {
		display: flex;
		flex-direction: column;
		align-items: flex-end;
		gap: var(--space-1);
		padding: var(--space-4) var(--space-6);
		background:
			radial-gradient(circle at top right, color-mix(in srgb, var(--color-accent-secondary) 22%, transparent), transparent 55%),
			linear-gradient(180deg, color-mix(in srgb, var(--color-bg-card) 94%, var(--color-accent-primary) 6%), var(--color-bg-elevated));
		border: 1px solid color-mix(in srgb, var(--color-accent-secondary) 30%, transparent);
		border-radius: var(--radius-xl);
		backdrop-filter: blur(12px);
		flex-shrink: 0;
		box-shadow: var(--shadow-card);
	}
	.balance-label {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--color-accent-secondary) 68%, transparent);
	}
	.balance-amount {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		font-family: var(--font-display);
		font-size: var(--text-3xl);
		font-weight: 700;
		color: var(--color-accent-gold);
	}
	.balance-coin { width: 28px; height: 28px; object-fit: contain; }
	.gem-balance {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 700;
		color: #a78bfa;
	}
	.gem-icon { font-size: 14px; }

	/* Active effect banner */
	.active-effect-banner {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-4) var(--space-5);
		background: rgba(245,158,11,0.10);
		border: 1px solid rgba(245,158,11,0.35);
		border-radius: var(--radius-xl);
		margin-bottom: var(--space-6);
		animation: fade-up 0.3s ease both;
	}
	.effect-icon { font-size: 1.5rem; }
	.active-effect-banner strong {
		display: block;
		font-family: var(--font-display);
		font-weight: 700;
		color: var(--color-accent-gold);
		font-size: var(--text-base);
	}
	.active-effect-banner span {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}

	/* Section header */
	.section-header { margin-bottom: var(--space-5); }
	.section-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: var(--space-1);
	}
	.section-sub { color: var(--color-text-secondary); font-size: var(--text-sm); }

	/* ── Items Grid ─────────────────────────────── */
	.items-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
		gap: var(--space-5);
	}

	.shop-card {
		background: var(--color-bg-card);
		border: 1px solid var(--card-border, rgba(56,189,248,0.2));
		border-radius: var(--radius-xl);
		padding: var(--space-5);
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
		backdrop-filter: blur(12px);
		box-shadow: var(--shadow-card);
		transition: all var(--transition-base);
		position: relative;
		overflow: hidden;
	}
	.shop-card::before {
		content: '';
		position: absolute;
		inset: 0;
		background: var(--card-accent, transparent);
		opacity: 0;
		transition: opacity var(--transition-base);
		border-radius: var(--radius-xl);
	}
	.shop-card:hover {
		transform: translateY(-4px);
		box-shadow: var(--shadow-card), 0 0 32px var(--card-glow, rgba(56,189,248,0.15));
		border-color: var(--card-border, rgba(56,189,248,0.4));
	}
	.shop-card:hover::before { opacity: 1; }

	/* Rarity ribbon */
	.rarity-ribbon {
		position: absolute;
		top: var(--space-3);
		right: var(--space-3);
		font-size: 9px;
		font-weight: 800;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		padding: 3px 8px;
		border: 1px solid;
		border-radius: var(--radius-full);
		z-index: 1;
	}

	.card-emoji-wrap {
		position: relative;
		z-index: 1;
		width: 56px;
		height: 56px;
		display: flex;
		align-items: center;
		justify-content: center;
		border: 1px solid var(--card-border, rgba(56,189,248,0.2));
		border-radius: var(--radius-xl);
		flex-shrink: 0;
	}
	.card-emoji { font-size: 1.8rem; }

	.card-body {
		position: relative;
		z-index: 1;
		flex: 1;
		display: flex;
		flex-direction: column;
		gap: var(--space-1);
	}
	.card-title {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		font-weight: 700;
		color: var(--color-text-primary);
	}
	.card-desc {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		line-height: 1.5;
	}
	.card-flavor {
		font-size: var(--text-xs);
		font-style: italic;
		color: var(--color-text-muted);
		line-height: 1.5;
		margin-top: var(--space-1);
	}

	.card-footer {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-3);
		margin-top: auto;
		padding-top: var(--space-3);
		border-top: 1px solid color-mix(in srgb, var(--color-accent-primary) 12%, rgba(255,255,255,0.06));
	}

	.owned-badge {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		white-space: nowrap;
	}

	.buy-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: var(--space-1);
		padding: 9px var(--space-4);
		background: var(--color-accent-primary);
		color: #fff;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		cursor: pointer;
		transition: all var(--transition-fast);
		box-shadow: 0 0 16px color-mix(in srgb, var(--color-accent-primary) 22%, transparent);
		white-space: nowrap;
		flex-shrink: 0;
	}
	.buy-btn:hover:not(:disabled) {
		background: color-mix(in srgb, var(--color-accent-primary) 82%, white 18%);
		box-shadow: 0 0 28px color-mix(in srgb, var(--color-accent-primary) 36%, transparent);
		transform: translateY(-1px);
	}
	.buy-btn.cant-afford {
		background: rgba(255,255,255,0.06);
		border: 1px solid rgba(255,255,255,0.1);
		color: rgba(255,255,255,0.3);
		box-shadow: none;
		cursor: not-allowed;
	}
	.btn-coin { width: 14px; height: 14px; object-fit: contain; }

	/* Toast */
	:global(.toast) {
		position: fixed;
		bottom: var(--space-6);
		left: 50%;
		transform: translateX(-50%);
		background: var(--color-bg-elevated);
		border: 1px solid var(--color-accent-primary);
		color: var(--color-accent-primary);
		padding: var(--space-3) var(--space-6);
		border-radius: var(--radius-full);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		z-index: 9999;
		pointer-events: none;
		animation: fade-up 0.25s ease both;
	}
</style>
