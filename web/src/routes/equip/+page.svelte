<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { SKIN_CATALOG } from '$lib/core/skins';
	import type { SkinModel } from '$lib/core/types';
	import Modal from '$lib/components/Modal.svelte';

	let confirmSkin = $state<SkinModel | null>(null);
	let buyMsg = $state('');

	const ownedIds = $derived(new Set(gameStore.player.unlockedSkinIds));
	const equippedId = $derived(gameStore.player.currentSkinId);

	function onSkinClick(skin: SkinModel) {
		if (ownedIds.has(skin.id)) {
			gameStore.equipSkin(skin.id);
		} else if (skin.coinPrice > 0 || skin.gemPrice > 0) {
			confirmSkin = skin;
		}
	}

	function confirmBuy() {
		if (!confirmSkin) return;
		if (gameStore.buySkin(confirmSkin.id)) {
			buyMsg = `Bought ${confirmSkin.name}!`;
			gameStore.equipSkin(confirmSkin.id);
			confirmSkin = null;
			setTimeout(() => buyMsg = '', 2000);
		}
	}
</script>

<svelte:head>
	<title>Equip – Maze Escape: Pathbound</title>
</svelte:head>

<div class="equip-page">
	<h1 class="page-title">🎨 Skins</h1>
	<p class="page-subtitle">Customize your maze runner. Tap to equip or purchase.</p>
	<div class="balance-row">
		<span class="balance-item">{gameStore.player.coinCount} 🪙</span>
		<span class="balance-item gem">{gameStore.player.gemCount} 💎</span>
	</div>

	{#if buyMsg}
		<div class="toast">{buyMsg}</div>
	{/if}

	<div class="skins-grid">
		{#each SKIN_CATALOG as skin}
			{@const owned = ownedIds.has(skin.id)}
			{@const equipped = equippedId === skin.id}
			<button
				class="skin-card"
				class:owned
				class:equipped
				class:locked={!owned && skin.coinPrice === 0 && skin.gemPrice === 0 && skin.isSpecialSkin}
				onclick={() => onSkinClick(skin)}
			>
				<div class="skin-preview">
					<img
						src="{base}/images/{skin.imageUrl}_icon.png"
						alt={skin.name}
						class="skin-img"
					/>
				</div>
				<span class="skin-name">{skin.name}</span>
				{#if equipped}
					<span class="skin-badge equipped-badge">Equipped</span>
				{:else if owned}
					<span class="skin-badge owned-badge">Owned</span>
				{:else if skin.coinPrice > 0}
					<span class="skin-badge price-badge">{skin.coinPrice} 🪙</span>
				{:else if skin.gemPrice > 0}
					<span class="skin-badge gem-badge">{skin.gemPrice} 💎</span>
				{:else}
					<span class="skin-badge locked-badge">🔒 Special</span>
				{/if}
			</button>
		{/each}
	</div>
</div>

<Modal open={!!confirmSkin} onclose={() => confirmSkin = null}>
	{#if confirmSkin}
		<div class="confirm-buy">
			<div class="confirm-preview">
				<img
					src="{base}/images/{confirmSkin.imageUrl}_icon.png"
					alt={confirmSkin.name}
					class="confirm-skin-img"
				/>
			</div>
			<h2>Buy {confirmSkin.name}?</h2>
		{#if confirmSkin.gemPrice > 0}
			<p class="confirm-price">{confirmSkin.gemPrice} 💎</p>
			<p class="confirm-balance">Gems: {gameStore.player.gemCount} 💎</p>
			{#if gameStore.player.gemCount < confirmSkin.gemPrice}
				<p class="confirm-cant-afford">Not enough gems!</p>
			{/if}
		{:else}
			<p class="confirm-price">{confirmSkin.coinPrice} 🪙</p>
			<p class="confirm-balance">Balance: {gameStore.player.coinCount} 🪙</p>
			{#if gameStore.player.coinCount < confirmSkin.coinPrice}
				<p class="confirm-cant-afford">Not enough coins!</p>
			{/if}
			{/if}
			<div class="confirm-actions">
				<button class="btn btn-ghost" onclick={() => confirmSkin = null}>Cancel</button>
				<button
					class="btn btn-primary"
					disabled={confirmSkin.gemPrice > 0
						? gameStore.player.gemCount < confirmSkin.gemPrice
						: gameStore.player.coinCount < confirmSkin.coinPrice}
					onclick={confirmBuy}
				>
					Buy & Equip
				</button>
			</div>
		</div>
	{/if}
</Modal>

<style>
	.equip-page {
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

	.toast {
		background: var(--color-accent-green);
		color: #000;
		padding: var(--space-2) var(--space-4);
		border-radius: var(--radius-md);
		text-align: center;
		font-weight: 600;
		margin-bottom: var(--space-4);
	}

	.skins-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
		gap: var(--space-3);
	}

	.skin-card {
		background: var(--color-bg-card);
		border: 2px solid var(--color-border);
		border-radius: var(--radius-lg);
		padding: var(--space-3);
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-2);
		cursor: pointer;
		transition: all var(--transition-fast);
	}

	.skin-card:hover { border-color: var(--color-accent-secondary); }
	.skin-card.equipped { border-color: var(--color-accent-primary); box-shadow: var(--shadow-glow); }
	.skin-card.locked { opacity: 0.5; cursor: default; }

	.skin-preview {
		width: 56px;
		height: 56px;
		border-radius: var(--radius-full);
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.skin-img { width: 48px; height: 48px; object-fit: contain; border-radius: var(--radius-sm); }

	.skin-name {
		font-size: var(--text-xs);
		font-weight: 600;
		text-align: center;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		max-width: 100%;
	}

	.skin-badge {
		font-size: 10px;
		padding: 1px 6px;
		border-radius: var(--radius-full);
	}

	.equipped-badge { background: var(--color-accent-primary); color: white; }
	.owned-badge { background: var(--color-accent-green); color: #000; }
	.price-badge { background: var(--color-accent-gold); color: #000; font-weight: 600; }
	.gem-badge { background: #7c3aed; color: white; font-weight: 600; }
	.locked-badge { background: var(--color-bg-secondary); }

	.balance-row {
		display: flex;
		gap: var(--space-4);
		margin-bottom: var(--space-4);
	}
	.balance-item {
		font-size: var(--text-sm);
		font-weight: 600;
		padding: var(--space-1) var(--space-3);
		background: var(--color-bg-secondary);
		border-radius: var(--radius-full);
	}
	.balance-item.gem { color: #7c3aed; }

	.confirm-buy {
		text-align: center;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-4);
	}

	.confirm-preview {
		width: 80px;
		height: 80px;
		border-radius: var(--radius-full);
		display: flex;
		align-items: center;
		justify-content: center;
	}
	.confirm-skin-img { width: 72px; height: 72px; object-fit: contain; border-radius: var(--radius-sm); }

	.confirm-price {
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-accent-gold);
	}

	.confirm-balance { color: var(--color-text-muted); font-size: var(--text-sm); }
	.confirm-cant-afford { color: var(--color-accent-red); font-weight: 600; }

	.confirm-actions {
		display: flex;
		gap: var(--space-3);
	}

	.btn {
		padding: var(--space-3) var(--space-6);
		border-radius: var(--radius-lg);
		font-weight: 600;
		cursor: pointer;
		border: none;
	}
	.btn-primary { background: var(--color-accent-primary); color: white; }
	.btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }
	.btn-ghost { background: transparent; color: var(--color-text-secondary); border: 1px solid var(--color-border); }
</style>
