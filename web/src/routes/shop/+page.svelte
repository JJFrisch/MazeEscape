<script lang="ts">
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { POWERUP_COSTS } from '$lib/core/types';

	const items = [
		{ key: 'hint' as const, label: 'Hint', emoji: '💡', desc: 'Reveals the next step toward the exit.', cost: 200, owned: () => gameStore.player.hintsOwned },
		{ key: 'extraTime' as const, label: 'Extra Time', emoji: '⏱️', desc: 'Adds bonus seconds toward 3-star time.', cost: 150, owned: () => gameStore.player.extraTimesOwned },
		{ key: 'extraMoves' as const, label: 'Extra Moves', emoji: '👣', desc: 'Adds bonus moves toward 2-star threshold.', cost: 50, owned: () => gameStore.player.extraMovesOwned },
	];

	let buyMsg = $state('');
	let buyTimeout: ReturnType<typeof setTimeout>;

	function buyItem(key: 'hint' | 'extraTime' | 'extraMoves', cost: number) {
		if (gameStore.spendCoins(cost)) {
			gameStore.addPowerup(key);
			clearTimeout(buyTimeout);
			buyMsg = `Purchased 1 ${key === 'hint' ? 'Hint' : key === 'extraTime' ? 'Extra Time' : 'Extra Moves'}!`;
			buyTimeout = setTimeout(() => buyMsg = '', 2000);
		}
	}
</script>

<svelte:head>
	<title>Shop – Maze Escape: Pathbound</title>
</svelte:head>

<div class="shop-page">
	<h1 class="page-title">🛒 Shop</h1>
	<p class="page-subtitle">Spend your hard-earned coins on powerups.</p>

	{#if buyMsg}
		<div class="buy-toast">{buyMsg}</div>
	{/if}

	<div class="balance">
		<span class="balance-label">Balance</span>
		<span class="balance-amount">{gameStore.player.coinCount} 🪙</span>
	</div>

	<div class="items-grid">
		{#each items as item}
			{@const canAfford = gameStore.player.coinCount >= item.cost}
			<div class="shop-card">
				<div class="card-icon">{item.emoji}</div>
				<h3 class="card-title">{item.label}</h3>
				<p class="card-desc">{item.desc}</p>
				<div class="card-owned">Owned: {item.owned()}</div>
				<button
					class="buy-btn"
					class:cant-afford={!canAfford}
					disabled={!canAfford}
					onclick={() => buyItem(item.key, item.cost)}
				>
					Buy – {item.cost} 🪙
				</button>
			</div>
		{/each}
	</div>
</div>

<style>
	.shop-page {
		max-width: 700px;
		margin: 0 auto;
		padding: var(--space-4);
	}

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-1);
	}

	.page-subtitle {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
		margin-bottom: var(--space-6);
	}

	.balance {
		display: flex;
		align-items: baseline;
		gap: var(--space-3);
		margin-bottom: var(--space-6);
		padding: var(--space-4);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
	}

	.balance-label {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
		text-transform: uppercase;
		letter-spacing: 1px;
	}

	.balance-amount {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
		color: var(--color-accent-gold);
	}

	.buy-toast {
		background: var(--color-accent-green);
		color: #000;
		padding: var(--space-2) var(--space-4);
		border-radius: var(--radius-md);
		text-align: center;
		font-weight: 600;
		margin-bottom: var(--space-4);
		animation: slideIn 0.2s ease-out;
	}

	@keyframes slideIn {
		from { opacity: 0; transform: translateY(-8px); }
		to { opacity: 1; transform: translateY(0); }
	}

	.items-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: var(--space-4);
	}

	.shop-card {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		padding: var(--space-6);
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
		text-align: center;
		transition: border-color var(--transition-fast);
	}

	.shop-card:hover {
		border-color: var(--color-accent-primary);
	}

	.card-icon {
		font-size: 2.5rem;
	}

	.card-title {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		font-weight: 700;
	}

	.card-desc {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
		line-height: 1.4;
	}

	.card-owned {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}

	.buy-btn {
		margin-top: auto;
		background: var(--color-accent-primary);
		color: white;
		border: none;
		border-radius: var(--radius-md);
		padding: var(--space-3);
		font-weight: 600;
		cursor: pointer;
		transition: all var(--transition-fast);
	}

	.buy-btn:hover:not(:disabled) {
		background: var(--color-accent-glow);
		box-shadow: var(--shadow-glow);
	}

	.buy-btn.cant-afford {
		opacity: 0.4;
		cursor: not-allowed;
	}
</style>
