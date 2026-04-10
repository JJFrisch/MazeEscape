<script lang="ts">
	import { ACHIEVEMENT_CATALOG } from '$lib/core/achievements';

	let {
		achievementId,
		ondismiss
	}: {
		achievementId: string;
		ondismiss: () => void;
	} = $props();

	const achievement = $derived(ACHIEVEMENT_CATALOG.find((entry) => entry.id === achievementId));

	function rewardLabel() {
		if (!achievement) return '';
		if (achievement.rewardType === 'powerup') {
			const names: Record<string, string> = {
				hint: 'Hint',
				extraTime: 'Extra Time',
				extraMoves: 'Extra Moves',
				compass: 'Compass',
				hourglass: 'Hourglass',
				blinkScroll: 'Blink Scroll',
				streakShield: 'Streak Shield',
				doubleCoinsToken: 'Double Coins Token'
			};
			const label = names[achievement.rewardPowerup ?? ''] ?? 'Powerup';
			return `${achievement.rewardAmount}x ${label}`;
		}
		if (achievement.rewardType === 'coins') return `${achievement.rewardAmount.toLocaleString()} coins`;
		if (achievement.rewardType === 'gems') return `${achievement.rewardAmount} gem${achievement.rewardAmount === 1 ? '' : 's'}`;
		if (achievement.rewardType === 'shards') return `${achievement.rewardAmount} crystal shard${achievement.rewardAmount === 1 ? '' : 's'}`;
		return 'Milestone trophy recorded';
	}
</script>

{#if achievement}
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<div class="popup-backdrop" onclick={ondismiss} role="dialog" aria-modal="true" aria-label="Achievement unlocked" tabindex="-1">
		<!-- svelte-ignore a11y_click_events_have_key_events -->
		<!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
		<div class="popup-card" onclick={(event) => event.stopPropagation()} role="document">
			<button class="close-btn" onclick={ondismiss} aria-label="Dismiss achievement">✕</button>

			<div class="icon-burst" aria-hidden="true">
				<div class="icon-ring"></div>
				<div class="icon-ring ring2"></div>
				<span class="icon-emoji">{achievement.icon}</span>
			</div>

			<p class="eyebrow">Guild Commendation Unlocked</p>
			<h2 class="item-name">{achievement.name}</h2>
			<p class="reward-desc">{achievement.description}</p>

			<div class="reward-pill">
				<span class="reward-label">Reward</span>
				<strong>{rewardLabel()}</strong>
			</div>

			<button class="collect-btn" onclick={ondismiss}>Continue</button>
		</div>
	</div>
{/if}

<style>
	.popup-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(5, 13, 26, 0.84);
		z-index: 220;
		display: flex;
		align-items: center;
		justify-content: center;
		backdrop-filter: blur(10px);
		animation: fade-in 0.2s ease-out;
	}

	@keyframes fade-in {
		from { opacity: 0; }
		to { opacity: 1; }
	}

	.popup-card {
		background: linear-gradient(180deg, #0f1f34, #0a1525);
		border: 1.5px solid rgba(250, 204, 21, 0.5);
		border-radius: 22px;
		padding: 2.75rem 2rem 2rem;
		max-width: 360px;
		width: min(92vw, 360px);
		text-align: center;
		position: relative;
		box-shadow: 0 0 72px rgba(250, 204, 21, 0.18);
		animation: pop-in 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
	}

	@keyframes pop-in {
		from { transform: scale(0.72); opacity: 0; }
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
		border: 2px solid rgba(250, 204, 21, 0.55);
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
		filter: drop-shadow(0 0 14px rgba(250, 204, 21, 0.55));
	}

	.eyebrow {
		font-size: 0.72rem;
		font-weight: 800;
		letter-spacing: 0.14em;
		text-transform: uppercase;
		color: #facc15;
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
		background: rgba(250, 204, 21, 0.08);
		border: 1px solid rgba(250, 204, 21, 0.2);
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
		color: #fef08a;
	}

	.collect-btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 0.75rem 2.5rem;
		background: #facc15;
		color: #231502;
		font-size: 1rem;
		font-weight: 800;
		border: none;
		border-radius: 40px;
		cursor: pointer;
		box-shadow: 0 4px 20px rgba(250, 204, 21, 0.28);
	}

	.collect-btn:hover {
		opacity: 0.9;
	}

	.collect-btn:active {
		transform: scale(0.98);
	}
</style>