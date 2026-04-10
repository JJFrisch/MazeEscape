<!--
  EncounterCard — D&D-style level encounter popup shown when a map node is clicked.
  Replaces the previous direct-navigate behavior on the campaign map.
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getDeityByAlgorithm, generateLevelName } from '$lib/core/deities';
	import type { MapNode } from '$lib/core/types';
	import type { CampaignLevel } from '$lib/core/types';

	let {
		worldId,
		node,
		levelDef,
		onclose,
	}: {
		worldId: number;
		node: MapNode;
		levelDef: CampaignLevel | undefined;
		onclose: () => void;
	} = $props();

	const deity = $derived(levelDef ? getDeityByAlgorithm(levelDef.levelType) : undefined);
	const levelName = $derived(levelDef ? generateLevelName(levelDef.levelType, node.levelNumber ?? '') : '???');
	const progress = $derived(gameStore.getLevelProgress(worldId, node.levelNumber ?? ''));

	const shapeLabel: Record<string, string> = {
		rectangular: 'Rectangular',
		hexagonal: 'Hexagonal',
		circular: 'Circular',
		triangular: 'Triangular',
	};

	function formatTime(s: number): string {
		if (!s) return '—';
		if (s < 60) return `${s.toFixed(1)}s`;
		return `${Math.floor(s / 60)}m ${(s % 60).toFixed(0)}s`;
	}

	function enterLevel() {
		goto(`${base}/campaign/play/${worldId}-${node.levelNumber}`);
	}

	// Difficulty derived from traits
	const difficultyLabel = $derived.by(() => {
		if (!deity) return 'Unknown';
		const d = deity.traits.difficulty;
		if (d <= 1) return 'Novice';
		if (d <= 2) return 'Apprentice';
		if (d <= 3) return 'Journeyman';
		if (d <= 4) return 'Veteran';
		return 'Archon';
	});

	const difficultyColor = $derived.by(() => {
		if (!deity) return '#94a3b8';
		const d = deity.traits.difficulty;
		if (d <= 1) return '#34d399';
		if (d <= 2) return '#38bdf8';
		if (d <= 3) return '#f59e0b';
		if (d <= 4) return '#f97316';
		return '#ef4444';
	});

	function handleBackdropClick(e: MouseEvent) {
		if (e.target === e.currentTarget) onclose();
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') onclose();
	}
</script>

<svelte:window onkeydown={handleKeydown} />

<!-- Backdrop -->
<!-- svelte-ignore a11y_no_static_element_interactions -->
<div class="backdrop" onclick={handleBackdropClick} role="dialog" aria-modal="true" aria-label="Level encounter">
	<div
		class="card"
		style="--deity-color:{deity?.color ?? '#38bdf8'}; --deity-dim:{deity?.colorDim ?? 'rgba(56,189,248,0.15)'};"
	>
		<!-- Close button -->
		<button class="close-btn" onclick={onclose} aria-label="Close">✕</button>

		<!-- Deity sigil / header accent -->
		{#if deity}
			<div class="deity-stripe" style="background: linear-gradient(90deg, {deity.colorDim}, transparent);"></div>
		{/if}

		<!-- Header -->
		<div class="card-header">
			<div class="level-badge">
				{node.type === 'bonus_level' ? '★ Bonus' : `Level ${node.levelNumber}`}
			</div>
			<h2 class="level-name">{levelName}</h2>

			{#if deity}
				<div class="deity-tag" style="color:{deity.color}; border-color:{deity.colorDim}; background:{deity.colorDim};">
					<svg class="deity-sigil" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
						<path d={deity.sigilPath} />
					</svg>
					{deity.name} · {deity.domain}
				</div>
			{/if}
		</div>

		<!-- Stats row -->
		{#if levelDef}
			<div class="stats-row">
				<div class="stat">
					<span class="stat-label">Grid</span>
					<span class="stat-value">{levelDef.width}×{levelDef.height}</span>
				</div>
				<div class="stat">
					<span class="stat-label">Shape</span>
					<span class="stat-value">{shapeLabel[levelDef.mazeShape] ?? levelDef.mazeShape}</span>
				</div>
				<div class="stat">
					<span class="stat-label">Difficulty</span>
					<span class="stat-value" style="color:{difficultyColor};">{difficultyLabel}</span>
				</div>
				<div class="stat">
					<span class="stat-label">Par Moves</span>
					<span class="stat-value">{levelDef.twoStarMoves}</span>
				</div>
			</div>
		{/if}

		<!-- Deity quote -->
		{#if deity}
			<blockquote class="deity-quote" style="border-color:{deity.colorDim};">
				"{deity.quote}"
				<cite>— {deity.name}</cite>
			</blockquote>
		{/if}

		<!-- Player best -->
		{#if progress?.completed}
			<div class="best-run">
				<div class="best-header">Your Best Run</div>
				<div class="best-stats">
					<div class="best-stars">
						{#each Array(5) as _, i}
							<span class="star" class:filled={i < progress.numberOfStars}>★</span>
						{/each}
						<span class="star-count">{progress.numberOfStars}/5</span>
					</div>
					<div class="best-nums">
						<span>⏱ {formatTime(progress.bestTime)}</span>
						<span>👣 {progress.bestMoves} moves</span>
					</div>
				</div>
			</div>
		{:else}
			<div class="not-played">
				<span class="np-icon">⚔️</span>
				<span>Not yet attempted</span>
			</div>
		{/if}

		<!-- Deity trait bars -->
		{#if deity}
			<div class="trait-bars">
				{#each [
					{ label: 'Corridor Length', val: deity.traits.corridorLength },
					{ label: 'Dead Ends', val: deity.traits.deadEndDensity },
					{ label: 'Randomness', val: deity.traits.randomness },
				] as trait}
					<div class="trait-row">
						<span class="trait-label">{trait.label}</span>
						<div class="trait-track">
							<div class="trait-fill" style="width:{trait.val * 20}%; background:{deity.color};"></div>
						</div>
					</div>
				{/each}
			</div>
		{/if}

		<!-- Actions -->
		<div class="actions">
			<button class="btn-enter" onclick={enterLevel}>
				<svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
					<polygon points="6,3 20,12 6,21"/>
				</svg>
				Enter Dungeon
			</button>
			{#if progress?.completed}
				<button class="btn-retry" onclick={enterLevel}>
					↺ Retry
				</button>
			{/if}
		</div>
	</div>
</div>

<style>
	@keyframes card-rise {
		from { opacity: 0; transform: translateY(20px) scale(0.97); }
		to   { opacity: 1; transform: translateY(0) scale(1); }
	}
	@keyframes fade-in {
		from { opacity: 0; }
		to   { opacity: 1; }
	}

	.backdrop {
		position: fixed;
		inset: 0;
		z-index: 200;
		display: flex;
		align-items: flex-end;
		justify-content: center;
		background: rgba(0, 0, 0, 0.55);
		backdrop-filter: blur(3px);
		padding: var(--space-4);
		animation: fade-in 0.2s ease both;
	}
	@media (min-width: 640px) {
		.backdrop { align-items: center; }
	}

	.card {
		position: relative;
		width: 100%;
		max-width: 440px;
		background: linear-gradient(160deg, rgba(8,18,36,0.98), rgba(4,10,24,0.99));
		border: 1px solid var(--deity-color, #38bdf8);
		border-radius: var(--radius-2xl);
		padding: var(--space-6);
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
		box-shadow:
			0 0 60px var(--deity-dim, rgba(56,189,248,0.15)),
			0 24px 64px rgba(0,0,0,0.6);
		animation: card-rise 0.3s cubic-bezier(0.22,1,0.36,1) both;
		overflow: hidden;
		max-height: 90dvh;
		overflow-y: auto;
	}

	.deity-stripe {
		position: absolute;
		top: 0; left: 0; right: 0;
		height: 3px;
		border-radius: var(--radius-2xl) var(--radius-2xl) 0 0;
	}

	.close-btn {
		position: absolute;
		top: var(--space-3);
		right: var(--space-3);
		width: 28px; height: 28px;
		background: rgba(255,255,255,0.06);
		border: 1px solid rgba(255,255,255,0.1);
		border-radius: 50%;
		color: var(--color-text-muted);
		font-size: 12px;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		transition: all var(--transition-fast);
		z-index: 2;
	}
	.close-btn:hover { background: rgba(255,255,255,0.12); color: var(--color-text-primary); }

	/* Header */
	.card-header { display: flex; flex-direction: column; gap: var(--space-2); }

	.level-badge {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}

	.level-name {
		font-family: var(--font-display);
		font-size: clamp(1.2rem, 3vw, 1.5rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		line-height: 1.2;
	}

	.deity-tag {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 4px 10px;
		border: 1px solid;
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 600;
		align-self: flex-start;
	}
	.deity-sigil { flex-shrink: 0; }

	/* Stats row */
	.stats-row {
		display: grid;
		grid-template-columns: repeat(4, 1fr);
		gap: var(--space-2);
		padding: var(--space-3);
		background: rgba(255,255,255,0.03);
		border: 1px solid rgba(255,255,255,0.06);
		border-radius: var(--radius-lg);
	}
	.stat { display: flex; flex-direction: column; align-items: center; gap: 2px; }
	.stat-label {
		font-size: 9px;
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}
	.stat-value {
		font-family: var(--font-display);
		font-size: var(--text-sm);
		font-weight: 700;
		color: var(--color-text-primary);
	}

	/* Quote */
	.deity-quote {
		margin: 0;
		padding: var(--space-3) var(--space-4);
		border-left: 2px solid var(--deity-dim, rgba(56,189,248,0.3));
		font-size: var(--text-sm);
		font-style: italic;
		color: var(--color-text-secondary);
		line-height: 1.6;
	}
	.deity-quote cite {
		display: block;
		font-style: normal;
		font-size: var(--text-xs);
		font-weight: 600;
		margin-top: var(--space-2);
		color: var(--deity-color, #38bdf8);
		opacity: 0.8;
	}

	/* Best run */
	.best-run {
		padding: var(--space-3) var(--space-4);
		background: rgba(52,211,153,0.06);
		border: 1px solid rgba(52,211,153,0.2);
		border-radius: var(--radius-lg);
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}
	.best-header {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: #34d399;
	}
	.best-stats { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: var(--space-2); }
	.best-stars { display: flex; align-items: center; gap: 2px; }
	.star { font-size: 14px; color: rgba(255,255,255,0.2); transition: color var(--transition-fast); }
	.star.filled { color: var(--color-accent-gold); text-shadow: 0 0 8px rgba(245,158,11,0.5); }
	.star-count {
		font-size: var(--text-xs);
		font-weight: 700;
		color: var(--color-accent-gold);
		margin-left: var(--space-2);
	}
	.best-nums {
		display: flex;
		gap: var(--space-3);
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}

	.not-played {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-3) var(--space-4);
		background: rgba(255,255,255,0.03);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-lg);
		font-size: var(--text-sm);
		color: var(--color-text-muted);
	}
	.np-icon { font-size: 1.1rem; }

	/* Trait bars */
	.trait-bars {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
		padding: var(--space-3) var(--space-4);
		background: rgba(255,255,255,0.02);
		border: 1px solid rgba(255,255,255,0.06);
		border-radius: var(--radius-lg);
	}
	.trait-row { display: flex; align-items: center; gap: var(--space-3); }
	.trait-label {
		font-size: 10px;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--color-text-muted);
		width: 90px;
		flex-shrink: 0;
	}
	.trait-track {
		flex: 1;
		height: 4px;
		background: rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		overflow: hidden;
	}
	.trait-fill {
		height: 100%;
		border-radius: var(--radius-full);
		transition: width 0.6s cubic-bezier(0.22,1,0.36,1);
		box-shadow: 0 0 6px currentColor;
	}

	/* Actions */
	.actions { display: flex; gap: var(--space-3); }
	.btn-enter {
		flex: 1;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: var(--space-2);
		padding: 12px var(--space-5);
		background: var(--deity-color, #38bdf8);
		color: #000;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		cursor: pointer;
		transition: all var(--transition-fast);
		box-shadow: 0 0 24px var(--deity-dim, rgba(56,189,248,0.3));
	}
	.btn-enter:hover {
		filter: brightness(1.1);
		box-shadow: 0 0 40px var(--deity-dim, rgba(56,189,248,0.45));
		transform: translateY(-1px);
	}
	.btn-retry {
		padding: 12px var(--space-4);
		background: rgba(255,255,255,0.06);
		border: 1px solid rgba(255,255,255,0.12);
		border-radius: var(--radius-lg);
		color: var(--color-text-secondary);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		cursor: pointer;
		transition: all var(--transition-fast);
		white-space: nowrap;
	}
	.btn-retry:hover { background: rgba(255,255,255,0.10); color: var(--color-text-primary); }
</style>
