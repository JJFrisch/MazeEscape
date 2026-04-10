<!--
  MapCollectiblePopup — full-screen overlay shown when a player taps
  an uncollected item on the campaign map (chest, key, gem, cloak, or powerup).
-->
<script lang="ts">
	import { base } from '$app/paths';
	import type { MapCollectible } from '$lib/core/types';

	let {
		collectible,
		onCollect,
		onClose
	}: {
		collectible: MapCollectible;
		onCollect: () => void;
		onClose: () => void;
	} = $props();

	const ICON: Record<MapCollectible['type'], string> = {
		chest: '🪙',
		key: '🗝️',
		gem: '💎',
		cloak: '🧥',
		powerup_hint: '💡',
		powerup_time: '⏱️',
		powerup_moves: '👟',
		boss_relic: '🜂'
	};

	const COLOR: Record<MapCollectible['type'], string> = {
		chest: '#fbbf24',
		key: '#facc15',
		gem: '#a78bfa',
		cloak: '#fb923c',
		powerup_hint: '#4ade80',
		powerup_time: '#4ade80',
		powerup_moves: '#4ade80',
		boss_relic: '#f97316'
	};

	function rewardLabel(): string {
		const r = collectible.reward;
		if (r.keyItemId) return `You found a key! It unlocks a sealed gate ahead.`;
		if (r.skinId != null) return `Unlocks a new player skin!`;
		if (r.specialItemId) return `A boss relic has been claimed and added to your archive.`;
		if (r.powerup && r.powerupCount) {
			const name = r.powerup === 'hint' ? 'Hint' : r.powerup === 'extraTime' ? 'Extra Time' : 'Extra Moves';
			return `+${r.powerupCount} ${name} powerup${r.powerupCount > 1 ? 's' : ''}!`;
		}
		if (r.coins) return `+${r.coins.toLocaleString()} coins!`;
		return '';
	}

	const icon = $derived(ICON[collectible.type]);
	const color = $derived(COLOR[collectible.type]);
	const description = $derived(rewardLabel());
	const relicVaultHref = $derived(`${base}/stats#relic-vault`);
</script>

<!-- svelte-ignore a11y_click_events_have_key_events -->
<div class="popup-backdrop" onclick={onClose} role="dialog" aria-modal="true" aria-label={collectible.label} tabindex="-1">
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
	<div class="popup-card" onclick={(e) => e.stopPropagation()} role="document" style="--item-color: {color}">
		<!-- Close button -->
		<button class="close-btn" onclick={onClose} aria-label="Close">✕</button>

		<!-- Icon burst -->
		<div class="icon-burst">
			<div class="icon-ring"></div>
			<div class="icon-ring ring2"></div>
			<span class="icon-emoji">{icon}</span>
		</div>

		<!-- Text -->
		<h2 class="item-name">{collectible.label}</h2>
		<p class="reward-desc">{description}</p>
		{#if collectible.type === 'boss_relic'}
			<a class="vault-link" href={relicVaultHref}>View in Relic Vault</a>
		{/if}

		<!-- Action -->
		<button class="collect-btn" onclick={onCollect}>
			Collect!
		</button>
	</div>
</div>

<style>
	.popup-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(5, 13, 26, 0.82);
		z-index: 200;
		display: flex;
		align-items: center;
		justify-content: center;
		backdrop-filter: blur(8px);
		animation: fade-in 0.2s ease-out;
	}

	@keyframes fade-in {
		from { opacity: 0; }
		to { opacity: 1; }
	}

	.popup-card {
		background: #0d1f35;
		border: 1.5px solid var(--item-color, #38bdf8);
		border-radius: 20px;
		padding: 2.5rem 2rem 2rem;
		max-width: 320px;
		width: 90%;
		text-align: center;
		position: relative;
		box-shadow: 0 0 60px color-mix(in srgb, var(--item-color, #38bdf8) 25%, transparent);
		animation: pop-in 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
	}

	@keyframes pop-in {
		from { transform: scale(0.7); opacity: 0; }
		to { transform: scale(1); opacity: 1; }
	}

	.close-btn {
		position: absolute;
		top: 0.75rem;
		right: 0.75rem;
		background: none;
		border: none;
		color: rgba(255,255,255,0.45);
		font-size: 1rem;
		cursor: pointer;
		padding: 0.25rem 0.4rem;
		border-radius: 50%;
		transition: color 0.15s, background 0.15s;
	}
	.close-btn:hover {
		color: #fff;
		background: rgba(255,255,255,0.1);
	}

	/* Icon burst */
	.icon-burst {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
		width: 100px;
		height: 100px;
		margin: 0 auto 1.2rem;
	}

	.icon-ring {
		position: absolute;
		inset: 0;
		border-radius: 50%;
		border: 2px solid var(--item-color, #38bdf8);
		opacity: 0.25;
		animation: ring-pulse 2s ease-in-out infinite;
	}
	.ring2 {
		inset: -10px;
		opacity: 0.12;
		animation-delay: 0.6s;
	}

	@keyframes ring-pulse {
		0%, 100% { transform: scale(1); opacity: 0.25; }
		50% { transform: scale(1.08); opacity: 0.1; }
	}

	.icon-emoji {
		font-size: 3.2rem;
		line-height: 1;
		filter: drop-shadow(0 0 12px var(--item-color, #38bdf8));
		animation: float 3s ease-in-out infinite;
	}

	@keyframes float {
		0%, 100% { transform: translateY(0); }
		50% { transform: translateY(-5px); }
	}

	.item-name {
		font-size: 1.35rem;
		font-weight: 700;
		color: var(--item-color, #38bdf8);
		margin-bottom: 0.4rem;
		line-height: 1.2;
	}

	.reward-desc {
		font-size: 0.95rem;
		color: rgba(255,255,255,0.75);
		margin-bottom: 0.8rem;
		line-height: 1.5;
	}

	.vault-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 1rem;
		padding: 0.5rem 0.9rem;
		border-radius: 999px;
		border: 1px solid color-mix(in srgb, var(--item-color, #38bdf8) 40%, transparent);
		background: color-mix(in srgb, var(--item-color, #38bdf8) 10%, transparent);
		color: #f8fafc;
		font-size: 0.82rem;
		font-weight: 700;
		text-decoration: none;
	}

	.vault-link:hover {
		background: color-mix(in srgb, var(--item-color, #38bdf8) 16%, transparent);
	}

	.collect-btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 0.7rem 2.4rem;
		background: var(--item-color, #38bdf8);
		color: #050d1a;
		font-size: 1rem;
		font-weight: 700;
		border: none;
		border-radius: 40px;
		cursor: pointer;
		transition: opacity 0.15s, transform 0.15s;
		box-shadow: 0 4px 20px color-mix(in srgb, var(--item-color, #38bdf8) 40%, transparent);
	}
	.collect-btn:hover {
		opacity: 0.88;
		transform: scale(1.04);
	}
	.collect-btn:active {
		transform: scale(0.97);
	}
</style>
