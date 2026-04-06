<!--
  MazeOutroOverlay: Full-screen fade results screen shown after completing any maze level.
  Displays the player's run data (time, moves, stars, coins) and route-specific
  continuation actions.
-->
<script lang="ts">
	import Stars from '$lib/components/Stars.svelte';

	interface OutroAction {
		label: string;
		primary?: boolean;
		onclick: () => void;
	}

	let {
		open = false,
		title,
		playerName = '',
		time,
		moves,
		stars = -1,
		twoStarMoves = 0,
		threeStarTime = 0,
		coins,
		accentColor = '#38bdf8',
		actions = []
	}: {
		open?: boolean;
		title: string;
		playerName?: string;
		time: number;
		moves: number;
		/** -1 = hide the star row */
		stars?: number;
		twoStarMoves?: number;
		threeStarTime?: number;
		coins: number;
		accentColor?: string;
		actions: OutroAction[];
	} = $props();

	const showStars = $derived(stars >= 0);

	function fmtTime(s: number): string {
		if (s < 60) return `${s.toFixed(1)}s`;
		const m = Math.floor(s / 60);
		const rem = (s % 60).toFixed(0).padStart(2, '0');
		return `${m}m ${rem}s`;
	}
</script>

{#if open}
	<div class="outro-overlay" style="--accent: {accentColor}">
		<div class="outro-inner">

			<!-- Heading -->
			<div class="outro-heading">
				<h2 class="outro-title">{title}</h2>
				{#if playerName}
					<p class="outro-player">{playerName}</p>
				{/if}
			</div>

			<!-- Stars -->
			{#if showStars}
				<div class="outro-stars">
					<Stars count={stars} size="3rem" />
				</div>
			{/if}

			<!-- Stats -->
			<div class="stat-grid">
				<div class="stat-card">
					<span class="stat-icon" aria-hidden="true">⏱️</span>
					<span class="stat-value">{fmtTime(time)}</span>
					<span class="stat-label">Time</span>
					{#if threeStarTime > 0}
						<span class="stat-goal">goal ≤ {threeStarTime}s</span>
					{/if}
				</div>

				<div class="stat-card">
					<span class="stat-icon" aria-hidden="true">👣</span>
					<span class="stat-value">{moves}</span>
					<span class="stat-label">Moves</span>
					{#if twoStarMoves > 0}
						<span class="stat-goal">goal ≤ {twoStarMoves}</span>
					{/if}
				</div>

				<div class="stat-card gold">
					<span class="stat-icon" aria-hidden="true">🪙</span>
					<span class="stat-value">+{coins}</span>
					<span class="stat-label">Coins earned</span>
				</div>
			</div>

			<!-- Actions -->
			<div class="outro-actions">
				{#each actions as action}
					<button
						class="outro-btn"
						class:primary={action.primary}
						onclick={action.onclick}
					>
						{action.label}
					</button>
				{/each}
			</div>

		</div>
	</div>
{/if}

<style>
	.outro-overlay {
		position: fixed;
		inset: 0;
		z-index: 300;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(4, 8, 22, 0.93);
		backdrop-filter: blur(6px);
		padding: 1.5rem;
		animation: outro-fade-in 450ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	.outro-inner {
		text-align: center;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2rem;
		max-width: 500px;
		width: 100%;
		animation: outro-rise 520ms cubic-bezier(0.16, 1, 0.3, 1) both;
	}

	/* ── Heading ─────────────────────────────────── */
	.outro-heading {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	.outro-title {
		font-family: var(--font-display);
		font-size: clamp(1.75rem, 6vw, 3rem);
		font-weight: 700;
		color: #f0f6ff;
		text-shadow: 0 0 50px color-mix(in srgb, var(--accent) 50%, transparent);
	}

	.outro-player {
		font-size: 0.8125rem;
		letter-spacing: 0.1em;
		text-transform: uppercase;
		color: rgba(240, 246, 255, 0.45);
	}

	/* ── Stars ───────────────────────────────────── */
	.outro-stars {
		display: flex;
		justify-content: center;
	}

	/* ── Stats ───────────────────────────────────── */
	.stat-grid {
		display: flex;
		gap: 1rem;
		justify-content: center;
		flex-wrap: wrap;
		width: 100%;
	}

	.stat-card {
		flex: 1;
		min-width: 90px;
		max-width: 140px;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.2rem;
		background: rgba(240, 246, 255, 0.05);
		border: 1px solid rgba(240, 246, 255, 0.1);
		border-radius: 0.75rem;
		padding: 1rem 0.75rem 0.85rem;
	}

	.stat-icon {
		font-size: 1.5rem;
		line-height: 1;
		margin-bottom: 0.15rem;
	}

	.stat-value {
		font-family: var(--font-mono);
		font-size: 1.375rem;
		font-weight: 700;
		color: #f0f6ff;
		line-height: 1;
	}

	.stat-card.gold .stat-value {
		color: var(--color-accent-gold, #d97706);
		text-shadow: 0 0 16px rgba(217, 119, 6, 0.4);
	}

	.stat-label {
		font-size: 0.6875rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		color: rgba(240, 246, 255, 0.4);
		margin-top: 0.1rem;
	}

	.stat-goal {
		font-size: 0.625rem;
		color: rgba(240, 246, 255, 0.28);
		letter-spacing: 0.04em;
	}

	/* ── Actions ─────────────────────────────────── */
	.outro-actions {
		display: flex;
		gap: 0.75rem;
		justify-content: center;
		flex-wrap: wrap;
	}

	.outro-btn {
		padding: 0.75rem 1.5rem;
		border-radius: 0.75rem;
		font-family: var(--font-display);
		font-weight: 600;
		font-size: 0.9375rem;
		cursor: pointer;
		border: 1px solid rgba(240, 246, 255, 0.18);
		background: rgba(255, 255, 255, 0.07);
		color: rgba(240, 246, 255, 0.7);
		transition: all 150ms ease;
	}

	.outro-btn:hover {
		background: rgba(255, 255, 255, 0.13);
		color: #f0f6ff;
		border-color: rgba(240, 246, 255, 0.3);
	}

	.outro-btn.primary {
		background: var(--accent);
		color: rgba(4, 8, 22, 0.9);
		border-color: transparent;
		font-weight: 700;
	}

	.outro-btn.primary:hover {
		filter: brightness(1.12);
		box-shadow: 0 0 22px color-mix(in srgb, var(--accent) 45%, transparent);
	}

	/* ── Animations ──────────────────────────────── */
	@keyframes outro-fade-in {
		from { opacity: 0; }
		to   { opacity: 1; }
	}

	@keyframes outro-rise {
		from { opacity: 0; transform: translateY(28px); }
		to   { opacity: 1; transform: translateY(0); }
	}
</style>
