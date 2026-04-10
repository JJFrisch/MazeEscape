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
			buyMsg = `${confirmSkin.name} equipped!`;
			gameStore.equipSkin(confirmSkin.id);
			confirmSkin = null;
			setTimeout(() => buyMsg = '', 2500);
		}
	}
</script>

<svelte:head>
	<title>Equip – Maze Escape: Pathbound</title>
</svelte:head>

{#if buyMsg}
	<div class="toast">{buyMsg}</div>
{/if}

<div class="equip-page">
	<!-- Header -->
	<div class="equip-header">
		<div>
			<div class="page-eyebrow">
				<span class="eyebrow-dot"></span>
				Customization
			</div>
			<h1 class="page-title">Skins</h1>
			<p class="page-sub">Customize your maze runner. Tap to equip or purchase.</p>
		</div>
		<div class="balance-chips">
			<div class="balance-chip coin-chip">
				<img src="{base}/images/coin.png" alt="" class="chip-icon" aria-hidden="true" />
				<span>{gameStore.player.coinCount.toLocaleString()}</span>
			</div>
			<div class="balance-chip gem-chip">
				<span class="gem-icon">💎</span>
				<span>{gameStore.player.gemCount}</span>
			</div>
		</div>
	</div>

	<!-- Grid -->
	<div class="skins-grid">
		{#each SKIN_CATALOG as skin}
			{@const owned = ownedIds.has(skin.id)}
			{@const equipped = equippedId === skin.id}
			{@const isSpecialLocked = !owned && skin.coinPrice === 0 && skin.gemPrice === 0 && skin.isSpecialSkin}
			<button
				class="skin-card"
				class:owned
				class:equipped
				class:special-locked={isSpecialLocked}
				onclick={() => onSkinClick(skin)}
				title={skin.name}
				disabled={isSpecialLocked}
			>
				<!-- Equipped ring -->
				{#if equipped}
					<div class="equipped-ring" aria-hidden="true"></div>
				{/if}

				<div class="skin-img-wrap">
					<img
						src="{base}/images/{skin.imageUrl}_icon.png"
						alt={skin.name}
						class="skin-img"
					/>
				</div>

				<span class="skin-name">{skin.name}</span>

				{#if equipped}
					<span class="skin-badge badge-equipped">Equipped</span>
				{:else if owned}
					<span class="skin-badge badge-owned">Owned</span>
				{:else if skin.coinPrice > 0}
					<span class="skin-badge badge-coin">{skin.coinPrice.toLocaleString()} 🪙</span>
				{:else if skin.gemPrice > 0}
					<span class="skin-badge badge-gem">{skin.gemPrice} 💎</span>
				{:else}
					<span class="skin-badge badge-special">🔒 Special</span>
				{/if}
			</button>
		{/each}
	</div>
</div>

<!-- Purchase modal -->
<Modal open={!!confirmSkin} onclose={() => confirmSkin = null}>
	{#if confirmSkin}
		<div class="confirm-modal">
			<div class="confirm-img-wrap">
				<img
					src="{base}/images/{confirmSkin.imageUrl}_icon.png"
					alt={confirmSkin.name}
					class="confirm-img"
				/>
			</div>
			<h2 class="confirm-title">Buy {confirmSkin.name}?</h2>

			{#if confirmSkin.gemPrice > 0}
				<div class="confirm-price gem-price">{confirmSkin.gemPrice} 💎</div>
				<p class="confirm-balance">Your gems: {gameStore.player.gemCount} 💎</p>
				{#if gameStore.player.gemCount < confirmSkin.gemPrice}
					<p class="confirm-error">Not enough gems!</p>
				{/if}
			{:else}
				<div class="confirm-price coin-price">
					<img src="{base}/images/coin.png" alt="" class="confirm-coin" aria-hidden="true" />
					{confirmSkin.coinPrice.toLocaleString()}
				</div>
				<p class="confirm-balance">Your coins: {gameStore.player.coinCount.toLocaleString()}</p>
				{#if gameStore.player.coinCount < confirmSkin.coinPrice}
					<p class="confirm-error">Not enough coins!</p>
				{/if}
			{/if}

			<div class="confirm-actions">
				<button class="modal-btn btn-cancel" onclick={() => confirmSkin = null}>Cancel</button>
				<button
					class="modal-btn btn-buy"
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
	@keyframes fade-up {
		from { opacity: 0; transform: translateY(16px); }
		to   { opacity: 1; transform: translateY(0); }
	}
	@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }
	@keyframes ring-spin {
		0%   { transform: rotate(0deg); }
		100% { transform: rotate(360deg); }
	}

	.equip-page {
		max-width: 960px;
		margin: 0 auto;
		animation: fade-up 0.4s ease both;
	}

	/* ── Header ─────────────────────────────────── */
	.equip-header {
		display: flex;
		align-items: flex-start;
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
		background: rgba(56, 189, 248, 0.08);
		border: 1px solid rgba(56, 189, 248, 0.25);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: #93c5fd;
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

	/* Balance chips */
	.balance-chips {
		display: flex;
		gap: var(--space-2);
		flex-shrink: 0;
		margin-top: var(--space-3);
	}
	.balance-chip {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		padding: 7px 14px;
		border-radius: var(--radius-full);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
	}
	.coin-chip {
		background: rgba(245,158,11,0.08);
		border: 1px solid rgba(245,158,11,0.25);
		color: var(--color-accent-gold);
	}
	.gem-chip {
		background: rgba(124,58,237,0.12);
		border: 1px solid rgba(124,58,237,0.3);
		color: #a78bfa;
	}
	.chip-icon {
		width: 16px;
		height: 16px;
		object-fit: contain;
	}
	.gem-icon { font-size: 14px; }

	/* ── Skins Grid ─────────────────────────────── */
	.skins-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
		gap: var(--space-3);
	}

	.skin-card {
		position: relative;
		background: var(--color-bg-card);
		border: 1px solid rgba(255,255,255,0.07);
		border-radius: var(--radius-xl);
		padding: var(--space-4) var(--space-3);
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-2);
		cursor: pointer;
		transition: all var(--transition-fast);
		backdrop-filter: blur(8px);
		box-shadow: var(--shadow-card);
		overflow: hidden;
	}
	.skin-card::before {
		content: '';
		position: absolute;
		inset: 0;
		background: linear-gradient(135deg, rgba(56,189,248,0.04), transparent);
		opacity: 0;
		transition: opacity var(--transition-fast);
	}
	.skin-card:hover:not(:disabled) {
		border-color: rgba(56,189,248,0.3);
		transform: translateY(-3px);
		box-shadow: var(--shadow-card), 0 0 20px rgba(56,189,248,0.1);
	}
	.skin-card:hover:not(:disabled)::before { opacity: 1; }

	.skin-card.equipped {
		border-color: var(--color-accent-primary);
		background: rgba(56,189,248,0.06);
		box-shadow: var(--shadow-card), 0 0 24px rgba(56,189,248,0.2);
	}
	.skin-card.special-locked {
		opacity: 0.45;
		cursor: not-allowed;
		filter: grayscale(0.4);
	}

	/* Equipped ring */
	.equipped-ring {
		position: absolute;
		inset: -1px;
		border-radius: var(--radius-xl);
		border: 2px solid var(--color-accent-primary);
		box-shadow: inset 0 0 16px rgba(56,189,248,0.15), 0 0 16px rgba(56,189,248,0.2);
		pointer-events: none;
	}

	.skin-img-wrap {
		position: relative;
		z-index: 1;
		width: 64px;
		height: 64px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(255,255,255,0.04);
		border-radius: var(--radius-xl);
		border: 1px solid rgba(255,255,255,0.06);
	}
	.skin-img {
		width: 50px;
		height: 50px;
		object-fit: contain;
	}

	.skin-name {
		position: relative;
		z-index: 1;
		font-size: var(--text-xs);
		font-weight: 600;
		color: var(--color-text-primary);
		text-align: center;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		max-width: 100%;
	}

	.skin-badge {
		position: relative;
		z-index: 1;
		font-size: 10px;
		font-weight: 700;
		padding: 2px 8px;
		border-radius: var(--radius-full);
	}
	.badge-equipped {
		background: rgba(56,189,248,0.2);
		border: 1px solid rgba(56,189,248,0.5);
		color: var(--color-accent-primary);
	}
	.badge-owned {
		background: rgba(16,185,129,0.15);
		border: 1px solid rgba(16,185,129,0.4);
		color: #34d399;
	}
	.badge-coin {
		background: rgba(245,158,11,0.12);
		border: 1px solid rgba(245,158,11,0.3);
		color: var(--color-accent-gold);
	}
	.badge-gem {
		background: rgba(124,58,237,0.15);
		border: 1px solid rgba(124,58,237,0.35);
		color: #a78bfa;
	}
	.badge-special {
		background: rgba(255,255,255,0.05);
		border: 1px solid rgba(255,255,255,0.1);
		color: rgba(255,255,255,0.35);
	}

	/* ── Confirm Modal ───────────────────────────── */
	.confirm-modal {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-2);
		text-align: center;
	}
	.confirm-img-wrap {
		width: 90px;
		height: 90px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.1);
		border-radius: var(--radius-xl);
	}
	.confirm-img {
		width: 72px;
		height: 72px;
		object-fit: contain;
	}
	.confirm-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
	}
	.confirm-price {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
	}
	.coin-price { color: var(--color-accent-gold); }
	.gem-price  { color: #a78bfa; }
	.confirm-coin { width: 22px; height: 22px; object-fit: contain; }
	.confirm-balance {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}
	.confirm-error {
		font-size: var(--text-sm);
		font-weight: 600;
		color: var(--color-accent-red);
	}
	.confirm-actions {
		display: flex;
		gap: var(--space-3);
		width: 100%;
	}
	.modal-btn {
		flex: 1;
		padding: 11px var(--space-5);
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		cursor: pointer;
		border: none;
		transition: all var(--transition-fast);
	}
	.btn-cancel {
		background: rgba(255,255,255,0.06);
		border: 1px solid rgba(255,255,255,0.12);
		color: var(--color-text-secondary);
	}
	.btn-cancel:hover { background: rgba(255,255,255,0.1); color: var(--color-text-primary); }
	.btn-buy {
		background: var(--color-accent-primary);
		color: #fff;
		box-shadow: 0 0 20px rgba(56,189,248,0.25);
	}
	.btn-buy:hover:not(:disabled) {
		background: #0ea5e9;
		box-shadow: 0 0 32px rgba(56,189,248,0.4);
	}
	.btn-buy:disabled { opacity: 0.4; cursor: not-allowed; box-shadow: none; }
</style>
