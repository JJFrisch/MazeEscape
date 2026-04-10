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
		fiveStarMoves = 0,
		fiveStarTime = 0,
		coins,
		accentColor = '#38bdf8',
		subtitle = '',
		rewardSummary = '',
		mazeWidth = 0,
		mazeHeight = 0,
		algoName = '',
		algoId = '',
		algoLinkBase = '',
		bestTime = 0,
		bestMoves = 0,
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
		fiveStarMoves?: number;
		fiveStarTime?: number;
		coins: number;
		accentColor?: string;
		subtitle?: string;
		rewardSummary?: string;
		mazeWidth?: number;
		mazeHeight?: number;
		algoName?: string;
		algoId?: string;
		algoLinkBase?: string;
		bestTime?: number;
		bestMoves?: number;
		actions: OutroAction[];
	} = $props();

	const showStars = $derived(stars >= 0);
	const showLevelInfo = $derived(mazeWidth > 0 && algoName.length > 0);
	const algoHref = $derived(algoId ? `${algoLinkBase}/algorithms#${algoId}` : `${algoLinkBase}/algorithms`);

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
			<div class="outro-aura outro-aura-one"></div>
			<div class="outro-aura outro-aura-two"></div>

			<!-- Heading -->
			<div class="outro-heading">
				{#if subtitle}
					<p class="outro-subtitle">{subtitle}</p>
				{/if}
				<h2 class="outro-title">{title}</h2>
				{#if playerName}
					<p class="outro-player">{playerName}</p>
				{/if}
			</div>

			<!-- Stars -->
			{#if showStars}
				<div class="outro-stars">
					<Stars count={stars} max={5} size="3rem" />
				</div>
			{/if}

			<!-- Level info strip -->
			{#if showLevelInfo}
				<div class="level-info-strip">
					<span class="level-info-size">{mazeWidth}×{mazeHeight}</span>
					<span class="level-info-sep" aria-hidden="true">·</span>
					<span class="level-info-algo">{algoName}</span>
					<a
						href={algoHref}
						target="_blank"
						rel="noopener noreferrer"
						class="level-info-link"
						title="Learn about this algorithm"
						aria-label="Learn about {algoName} (opens algorithm encyclopedia)"
					>↗</a>
				</div>
			{/if}

			<!-- Stats -->
			<div class="stat-grid">
				<div class="stat-card">
					<span class="stat-icon" aria-hidden="true">⏱️</span>
					<span class="stat-value">{fmtTime(time)}</span>
					<span class="stat-label">Time</span>
					{#if threeStarTime > 0}
						<span class="stat-goal">★★★ ≤ {threeStarTime}s</span>
					{/if}
					{#if fiveStarTime > 0}
						<span class="stat-goal">★★★★★ ≤ {fiveStarTime}s</span>
					{/if}
					{#if bestTime > 0}
						<span class="stat-best">best {fmtTime(bestTime)}</span>
					{/if}
				</div>

				<div class="stat-card">
					<span class="stat-icon" aria-hidden="true">👣</span>
					<span class="stat-value">{moves}</span>
					<span class="stat-label">Moves</span>
					{#if twoStarMoves > 0}
						<span class="stat-goal">★★ ≤ {twoStarMoves}</span>
					{/if}
					{#if fiveStarMoves > 0}
						<span class="stat-goal">★★★★★ ≤ {fiveStarMoves}</span>
					{/if}
					{#if bestMoves > 0}
						<span class="stat-best">best {bestMoves}</span>
					{/if}
				</div>

				<div class="stat-card gold">
					<span class="stat-icon" aria-hidden="true">🪙</span>
					<span class="stat-value">+{coins}</span>
					<span class="stat-label">Coins earned</span>
				</div>
			</div>

			{#if rewardSummary}
				<div class="reward-summary">{rewardSummary}</div>
			{/if}

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
		background:
			radial-gradient(circle at top center, color-mix(in srgb, var(--accent) 16%, transparent) 0%, transparent 34%),
			color-mix(in srgb, var(--color-bg-primary, #040816) 88%, rgba(4, 8, 22, 0.93));
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
		padding: 1.75rem;
		border-radius: 1.75rem;
		border: 1px solid color-mix(in srgb, var(--accent) 24%, rgba(255,255,255,0.1));
		background: linear-gradient(180deg, color-mix(in srgb, var(--accent) 7%, rgba(255,255,255,0.04)) 0%, color-mix(in srgb, var(--color-bg-card, #0b1220) 90%, transparent) 100%);
		box-shadow: 0 24px 80px color-mix(in srgb, var(--accent) 12%, rgba(0,0,0,0.45));
		animation: outro-rise 520ms cubic-bezier(0.16, 1, 0.3, 1) both;
		position: relative;
		overflow: hidden;
	}

	.outro-aura {
		position: absolute;
		border-radius: 999px;
		filter: blur(60px);
		pointer-events: none;
		opacity: 0.35;
	}

	.outro-aura-one {
		width: 220px;
		height: 220px;
		top: -80px;
		left: -70px;
		background: color-mix(in srgb, var(--accent) 48%, transparent);
	}

	.outro-aura-two {
		width: 180px;
		height: 180px;
		right: -40px;
		bottom: -60px;
		background: color-mix(in srgb, var(--accent) 30%, transparent);
	}

	/* ── Heading ─────────────────────────────────── */
	.outro-heading {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
		position: relative;
		z-index: 1;
	}

	.outro-subtitle {
		font-size: 0.75rem;
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--accent) 72%, white 28%);
	}

	.outro-title {
		font-family: var(--font-display);
		font-size: clamp(1.75rem, 6vw, 3rem);
		font-weight: 700;
		color: var(--color-text-primary, #f0f6ff);
		text-shadow: 0 0 50px color-mix(in srgb, var(--accent) 50%, transparent);
	}

	.outro-player {
		font-size: 0.8125rem;
		letter-spacing: 0.1em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--color-text-secondary, #cbd5e1) 70%, transparent);
	}

	/* ── Stars ───────────────────────────────────── */
	.outro-stars {
		display: flex;
		justify-content: center;
		position: relative;
		z-index: 1;
	}

	/* ── Stats ───────────────────────────────────── */
	.stat-grid {
		display: flex;
		gap: 1rem;
		justify-content: center;
		flex-wrap: wrap;
		width: 100%;
		position: relative;
		z-index: 1;
	}

	.stat-card {
		flex: 1;
		min-width: 90px;
		max-width: 140px;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.2rem;
		background: linear-gradient(180deg, color-mix(in srgb, var(--accent) 8%, rgba(240, 246, 255, 0.04)) 0%, color-mix(in srgb, var(--color-bg-card, #0b1220) 92%, transparent) 100%);
		border: 1px solid color-mix(in srgb, var(--accent) 20%, rgba(240, 246, 255, 0.08));
		border-radius: 0.75rem;
		padding: 1rem 0.75rem 0.85rem;
		box-shadow: 0 10px 24px color-mix(in srgb, var(--accent) 8%, rgba(0,0,0,0.2));
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
		color: var(--color-text-primary, #f0f6ff);
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

	.stat-best {
		font-size: 0.625rem;
		color: rgba(240, 246, 255, 0.38);
		letter-spacing: 0.04em;
		font-style: italic;
	}

	/* ── Level info strip ────────────────────────── */
	.level-info-strip {
		display: flex;
		align-items: center;
		gap: 0.45rem;
		font-size: 0.75rem;
		background: color-mix(in srgb, var(--accent) 8%, transparent);
		border: 1px solid color-mix(in srgb, var(--accent) 22%, transparent);
		border-radius: 9999px;
		padding: 0.3rem 0.85rem;
		color: color-mix(in srgb, var(--color-text-secondary, #cbd5e1) 82%, transparent);
		position: relative;
		z-index: 1;
	}

	.reward-summary {
		position: relative;
		z-index: 1;
		padding: 0.8rem 1rem;
		border-radius: 1rem;
		background: color-mix(in srgb, var(--accent) 10%, rgba(15, 23, 42, 0.85));
		border: 1px solid color-mix(in srgb, var(--accent) 20%, transparent);
		font-size: 0.9rem;
		line-height: 1.55;
		color: var(--color-text-secondary, #cbd5e1);
	}

	.level-info-size {
		font-family: var(--font-mono);
		font-weight: 700;
		color: var(--accent);
	}

	.level-info-sep {
		opacity: 0.4;
	}

	.level-info-algo {
		color: color-mix(in srgb, var(--color-text-primary, #f0f6ff) 82%, transparent);
	}

	.level-info-link {
		color: var(--accent);
		text-decoration: none;
		opacity: 0.75;
		transition: opacity 0.15s;
		font-size: 0.8rem;
	}
	.level-info-link:hover { opacity: 1; }

	/* ── Actions ─────────────────────────────────── */
	.outro-actions {
		display: flex;
		gap: 0.75rem;
		justify-content: center;
		flex-wrap: wrap;
		position: relative;
		z-index: 1;
	}

	.outro-btn {
		padding: 0.75rem 1.5rem;
		border-radius: 0.75rem;
		font-family: var(--font-display);
		font-weight: 600;
		font-size: 0.9375rem;
		cursor: pointer;
		border: 1px solid color-mix(in srgb, var(--accent) 24%, rgba(240, 246, 255, 0.12));
		background: color-mix(in srgb, var(--accent) 9%, rgba(255, 255, 255, 0.05));
		color: color-mix(in srgb, var(--color-text-primary, #f0f6ff) 82%, transparent);
		transition: all 150ms ease;
	}

	.outro-btn:hover {
		background: color-mix(in srgb, var(--accent) 14%, rgba(255, 255, 255, 0.08));
		color: var(--color-text-primary, #f0f6ff);
		border-color: color-mix(in srgb, var(--accent) 34%, rgba(240, 246, 255, 0.2));
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
