<script lang="ts">
	import { gameStore } from '$lib/stores/gameStore.svelte';

	const items = [
		{
			key: 'hint' as const,
			label: 'Hint',
			emoji: '💡',
			desc: 'Reveals the next step toward the exit.',
			cost: 200,
			owned: () => gameStore.player.hintsOwned,
			accent: 'rgba(56, 189, 248, 0.15)',
			border: 'rgba(56, 189, 248, 0.35)',
			glow: 'rgba(56, 189, 248, 0.2)',
		},
		{
			key: 'extraTime' as const,
			label: 'Extra Time',
			emoji: '⏱️',
			desc: 'Adds bonus seconds toward 3-star time.',
			cost: 150,
			owned: () => gameStore.player.extraTimesOwned,
			accent: 'rgba(167, 139, 250, 0.15)',
			border: 'rgba(167, 139, 250, 0.35)',
			glow: 'rgba(167, 139, 250, 0.2)',
		},
		{
			key: 'extraMoves' as const,
			label: 'Extra Moves',
			emoji: '👣',
			desc: 'Adds bonus moves toward 2-star threshold.',
			cost: 50,
			owned: () => gameStore.player.extraMovesOwned,
			accent: 'rgba(52, 211, 153, 0.15)',
			border: 'rgba(52, 211, 153, 0.35)',
			glow: 'rgba(52, 211, 153, 0.2)',
		},
	];

	let buyMsg = $state('');
	let buyTimeout: ReturnType<typeof setTimeout>;

	function buyItem(key: 'hint' | 'extraTime' | 'extraMoves', cost: number) {
		if (gameStore.spendCoins(cost)) {
			gameStore.addPowerup(key);
			clearTimeout(buyTimeout);
			buyMsg = `Purchased!`;
			buyTimeout = setTimeout(() => buyMsg = '', 2000);
		}
	}
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
				Powerup Store
			</div>
			<h1 class="page-title">Shop</h1>
			<p class="page-sub">Spend your hard-earned coins on powerups.</p>
		</div>
		<div class="balance-card">
			<span class="balance-label">Balance</span>
			<div class="balance-amount">
				<img src="/images/coin.png" alt="" class="balance-coin" aria-hidden="true" />
				<span>{gameStore.player.coinCount.toLocaleString()}</span>
			</div>
		</div>
	</div>

	<!-- Items -->
	<div class="items-grid">
		{#each items as item}
			{@const canAfford = gameStore.player.coinCount >= item.cost}
			<div
				class="shop-card"
				style="--card-accent: {item.accent}; --card-border: {item.border}; --card-glow: {item.glow};"
			>
				<div class="card-emoji-wrap">
					<span class="card-emoji" role="img" aria-hidden="true">{item.emoji}</span>
				</div>
				<div class="card-body">
					<h3 class="card-title">{item.label}</h3>
					<p class="card-desc">{item.desc}</p>
				</div>
				<div class="card-footer">
					<div class="owned-badge">
						<svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
							<path d="M20 7H4a2 2 0 00-2 2v10a2 2 0 002 2h16a2 2 0 002-2V9a2 2 0 00-2-2zM4 5h16v2H4V5z"/>
						</svg>
						Owned: {item.owned()}
					</div>
					<button
						class="buy-btn"
						class:cant-afford={!canAfford}
						disabled={!canAfford}
						onclick={() => buyItem(item.key, item.cost)}
					>
						<img src="/images/coin.png" alt="" class="btn-coin" aria-hidden="true" />
						Buy — {item.cost}
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

	.shop-page {
		max-width: 860px;
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
		margin-bottom: var(--space-10);
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
	.page-sub {
		color: var(--color-text-secondary);
		font-size: var(--text-base);
	}

	/* Balance card */
	.balance-card {
		display: flex;
		flex-direction: column;
		align-items: flex-end;
		gap: var(--space-1);
		padding: var(--space-4) var(--space-6);
		background:
			radial-gradient(circle at top right, color-mix(in srgb, var(--color-accent-secondary) 22%, transparent), transparent 55%),
			linear-gradient(180deg, color-mix(in srgb, var(--color-bg-card) 94%, var(--color-accent-primary) 6%), color-mix(in srgb, var(--color-bg-elevated) 94%, transparent));
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
	.balance-coin {
		width: 28px;
		height: 28px;
		object-fit: contain;
	}

	/* ── Items Grid ─────────────────────────────── */
	.items-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
		gap: var(--space-5);
	}

	.shop-card {
		background: var(--color-bg-card);
		border: 1px solid var(--card-border, rgba(56,189,248,0.2));
		border-radius: var(--radius-xl);
		padding: var(--space-6);
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
		backdrop-filter: blur(12px);
		box-shadow: var(--shadow-card);
		transition: all var(--transition-base);
		position: relative;
		overflow: hidden;
		background-image:
			radial-gradient(circle at top right, color-mix(in srgb, var(--color-accent-primary) 9%, transparent) 0%, transparent 42%),
			linear-gradient(180deg, color-mix(in srgb, var(--color-accent-secondary) 4%, transparent) 0%, transparent 100%);
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

	.card-emoji-wrap {
		position: relative;
		z-index: 1;
		width: 64px;
		height: 64px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: var(--card-accent, rgba(56,189,248,0.08));
		border: 1px solid var(--card-border, rgba(56,189,248,0.2));
		border-radius: var(--radius-xl);
	}
	.card-emoji { font-size: 2rem; }

	.card-body {
		position: relative;
		z-index: 1;
		flex: 1;
	}
	.card-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: var(--space-2);
	}
	.card-desc {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		line-height: 1.5;
	}

	.card-footer {
		position: relative;
		z-index: 1;
		display: flex;
		flex-direction: column;
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
	}

	.buy-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		padding: 11px var(--space-5);
		background: var(--color-accent-primary);
		color: #fff;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		cursor: pointer;
		transition: all var(--transition-fast);
		box-shadow: 0 0 20px color-mix(in srgb, var(--color-accent-primary) 22%, transparent);
	}
	.buy-btn:hover:not(:disabled) {
		background: color-mix(in srgb, var(--color-accent-primary) 82%, white 18%);
		box-shadow: 0 0 32px color-mix(in srgb, var(--color-accent-primary) 36%, transparent);
		transform: translateY(-1px);
	}
	.buy-btn.cant-afford {
		background: rgba(255,255,255,0.06);
		border: 1px solid rgba(255,255,255,0.1);
		color: rgba(255,255,255,0.3);
		box-shadow: none;
		cursor: not-allowed;
	}
	.btn-coin {
		width: 18px;
		height: 18px;
		object-fit: contain;
	}
</style>
