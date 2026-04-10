<script lang="ts">
	import { getDeityByAlgorithm, type MasteryRewardUnlock } from '$lib/core/deities';
	import { POWERUP_COSTS } from '$lib/core/types';
	import { getSkinById } from '$lib/core/skins';

	let {
		unlock,
		ondismiss
	}: {
		unlock: MasteryRewardUnlock;
		ondismiss: () => void;
	} = $props();

	const deity = $derived(getDeityByAlgorithm(unlock.algorithm));
	const rewardLabel = $derived.by(() => {
		if (unlock.rewardType === 'powerup') {
			const powerup = POWERUP_COSTS.find((entry) => entry.name === unlock.powerupName);
			return `${unlock.amount ?? 1}x ${powerup?.displayName ?? 'Relic'}`;
		}
		if (unlock.rewardType === 'coins') {
			return `${(unlock.coinAmount ?? 0).toLocaleString()} coins`;
		}
		const skin = getSkinById(unlock.skinId ?? -1);
		return skin?.name ?? unlock.skinName ?? 'Mastery Skin';
	});
	const milestoneLabel = $derived(`${unlock.milestone} completions`);
</script>

<!-- svelte-ignore a11y_click_events_have_key_events -->
<div class="popup-backdrop" onclick={ondismiss} role="dialog" aria-modal="true" aria-label="Mastery reward unlocked" tabindex="-1">
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
	<div class="popup-card" onclick={(event) => event.stopPropagation()} role="document" style="--deity-color:{deity?.color ?? '#38bdf8'}; --deity-dim:{deity?.colorDim ?? 'rgba(56,189,248,0.18)'};">
		<button class="close-btn" onclick={ondismiss} aria-label="Dismiss mastery reward">✕</button>

		<div class="icon-burst" aria-hidden="true">
			<div class="icon-ring"></div>
			<div class="icon-ring ring2"></div>
			<span class="icon-emoji">{unlock.rewardType === 'powerup' ? '🎁' : unlock.rewardType === 'coins' ? '🪙' : '✨'}</span>
		</div>

		<p class="eyebrow">Deity Reward Claimed</p>
		<h2 class="item-name">{deity?.name ?? 'Unknown Deity'}</h2>
		<p class="reward-desc">Your bond with this deity reached <strong>{milestoneLabel}</strong>. A mastery reward has been added to your account.</p>

		<div class="reward-pill">
			<span class="reward-label">Reward</span>
			<strong>{rewardLabel}</strong>
		</div>

		<button class="collect-btn" onclick={ondismiss}>Continue</button>
	</div>
</div>

<style>
	.popup-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(5, 13, 26, 0.84);
		z-index: 221;
		display: flex;
		align-items: center;
		justify-content: center;
		backdrop-filter: blur(10px);
	}

	.popup-card {
		background: linear-gradient(180deg, #0f1f34, #0a1525);
		border: 1.5px solid var(--deity-color);
		border-radius: 22px;
		padding: 2.75rem 2rem 2rem;
		max-width: 360px;
		width: min(92vw, 360px);
		text-align: center;
		position: relative;
		box-shadow: 0 0 72px color-mix(in srgb, var(--deity-color) 22%, transparent);
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
	}

	.icon-burst {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
		width: 108px;
		height: 108px;
		margin: 0 auto 1.2rem;
	}

	.icon-ring {
		position: absolute;
		inset: 0;
		border-radius: 50%;
		border: 2px solid color-mix(in srgb, var(--deity-color) 70%, white 30%);
		opacity: 0.24;
		animation: ring-pulse 2s ease-in-out infinite;
	}

	.ring2 {
		inset: -10px;
		opacity: 0.14;
		animation-delay: 0.6s;
	}

	@keyframes ring-pulse {
		0%, 100% { transform: scale(1); opacity: 0.25; }
		50% { transform: scale(1.08); opacity: 0.08; }
	}

	.icon-emoji {
		font-size: 3.5rem;
		line-height: 1;
		filter: drop-shadow(0 0 14px color-mix(in srgb, var(--deity-color) 75%, white 25%));
	}

	.eyebrow {
		font-size: 0.72rem;
		font-weight: 800;
		letter-spacing: 0.14em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--deity-color) 75%, white 25%);
		margin-bottom: 0.55rem;
	}

	.item-name {
		font-size: 1.4rem;
		font-weight: 700;
		color: #fff7cc;
		margin-bottom: 0.45rem;
		line-height: 1.2;
	}

	.reward-desc {
		font-size: 0.95rem;
		color: rgba(255,255,255,0.76);
		margin-bottom: 1.2rem;
		line-height: 1.55;
	}

	.reward-pill {
		display: grid;
		gap: 0.2rem;
		padding: 0.85rem 1rem;
		margin-bottom: 1.5rem;
		background: color-mix(in srgb, var(--deity-color) 10%, transparent);
		border: 1px solid color-mix(in srgb, var(--deity-color) 22%, transparent);
		border-radius: 14px;
	}

	.reward-label {
		font-size: 0.7rem;
		font-weight: 800;
		letter-spacing: 0.1em;
		text-transform: uppercase;
		color: rgba(255,255,255,0.55);
	}

	.reward-pill strong {
		font-size: 1rem;
		color: color-mix(in srgb, var(--deity-color) 70%, white 30%);
	}

	.collect-btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 0.75rem 2.5rem;
		background: color-mix(in srgb, var(--deity-color) 82%, white 18%);
		color: #07111d;
		font-size: 1rem;
		font-weight: 800;
		border: none;
		border-radius: 40px;
		cursor: pointer;
		box-shadow: 0 4px 20px color-mix(in srgb, var(--deity-color) 28%, transparent);
	}
</style>