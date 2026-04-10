<script lang="ts">
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { DEITY_CATALOG } from '$lib/core/deities';
	import { SKIN_CATALOG } from '$lib/core/skins';
	import type { MazeAlgorithm } from '$lib/core/types';

	// ── Derived stats ──────────────────────────────────────────────

	// Level progress aggregates
	const allLevels = $derived(Object.values(gameStore.levelProgress));
	const completedLevels = $derived(allLevels.filter(l => l.completed));
	const totalStars = $derived(allLevels.reduce((sum, l) => sum + (l.numberOfStars ?? 0), 0));
	const threeStarLevels = $derived(allLevels.filter(l => l.star1 && l.star2 && l.star3));

	// Best campaign time/moves across all levels
	const bestCampaignTime = $derived.by(() => {
		let best = Infinity;
		for (const l of allLevels) {
			if (l.bestTime > 0) best = Math.min(best, l.bestTime);
		}
		return best === Infinity ? 0 : best;
	});
	const bestCampaignMoves = $derived.by(() => {
		let best = Infinity;
		for (const l of allLevels) {
			if (l.bestMoves > 0) best = Math.min(best, l.bestMoves);
		}
		return best === Infinity ? 0 : best;
	});

	// Daily stats
	const allDailies = $derived(Object.values(gameStore.dailyResults));
	const completedDailies = $derived(allDailies.filter(d => d.status === 'completed' || d.status === 'completed_late'));
	const bestDailyTime = $derived.by(() => {
		let best = Infinity;
		for (const d of completedDailies) {
			if (d.completionTime > 0) best = Math.min(best, d.completionTime);
		}
		return best === Infinity ? 0 : best;
	});

	// Guild rank based on levels completed
	function guildRank(completed: number): { title: string; color: string; icon: string } {
		if (completed >= 100) return { title: 'Archon of the Labyrinth', color: '#f59e0b', icon: '👑' };
		if (completed >= 60)  return { title: 'Master Cartographer',    color: '#a78bfa', icon: '🗺️' };
		if (completed >= 30)  return { title: 'Veteran Delver',         color: '#38bdf8', icon: '⚔️' };
		if (completed >= 15)  return { title: 'Journeyman Explorer',    color: '#34d399', icon: '🧭' };
		if (completed >= 5)   return { title: 'Apprentice Pathfinder',  color: '#94a3b8', icon: '🔦' };
		return { title: 'Novice Wanderer', color: '#64748b', icon: '🕯️' };
	}
	const rank = $derived(guildRank(completedLevels.length));

	// Favourite deity (highest mastery count)
	const favoriteDeity = $derived.by(() => {
		const mastery = gameStore.player.algoMasteryCount ?? {};
		let topAlgo: MazeAlgorithm | null = null;
		let topCount = 0;
		for (const [algo, count] of Object.entries(mastery)) {
			if ((count ?? 0) > topCount) {
				topCount = count ?? 0;
				topAlgo = algo as MazeAlgorithm;
			}
		}
		if (!topAlgo) return null;
		return DEITY_CATALOG.find(d => d.algorithm === topAlgo) ?? null;
	});

	// Total mastery completions
	const totalMasteryCompletions = $derived(
		Object.values(gameStore.player.algoMasteryCount ?? {}).reduce((s, c) => s + (c ?? 0), 0)
	);

	// Deities sorted by mastery desc
	const deitiesByMastery = $derived.by(() => {
		return [...DEITY_CATALOG].sort((a, b) => {
			const ca = (gameStore.player.algoMasteryCount ?? {})[a.algorithm] ?? 0;
			const cb = (gameStore.player.algoMasteryCount ?? {})[b.algorithm] ?? 0;
			return cb - ca;
		});
	});

	// Current skin
	const currentSkin = $derived(SKIN_CATALOG.find(s => s.id === gameStore.player.currentSkinId));

	// Total items owned
	const totalItemsOwned = $derived(
		(gameStore.player.hintsOwned ?? 0) +
		(gameStore.player.extraTimesOwned ?? 0) +
		(gameStore.player.extraMovesOwned ?? 0) +
		(gameStore.player.compassOwned ?? 0) +
		(gameStore.player.hourglassOwned ?? 0) +
		(gameStore.player.blinkScrollsOwned ?? 0) +
		(gameStore.player.streakShieldsOwned ?? 0) +
		(gameStore.player.doubleCoinsTokensOwned ?? 0)
	);

	// Format seconds → m:ss
	function fmtTime(secs: number): string {
		if (!secs) return '—';
		const m = Math.floor(secs / 60);
		const s = Math.floor(secs % 60);
		return `${m}:${s.toString().padStart(2, '0')}`;
	}

	// Mastery tier label
	function masteryTierLabel(n: number): string {
		if (n >= 30) return 'Champion';
		if (n >= 20) return 'Disciple';
		if (n >= 10) return 'Student';
		if (n > 0)   return 'Initiate';
		return 'Unknown';
	}
	function masteryTierColor(n: number): string {
		if (n >= 30) return '#a78bfa';
		if (n >= 20) return '#38bdf8';
		if (n >= 10) return '#34d399';
		if (n > 0)   return '#94a3b8';
		return '#334155';
	}
</script>

<svelte:head>
	<title>Chronicles – Maze Escape: Pathbound</title>
</svelte:head>

<div class="stats-page">

	<!-- ── PAGE HEADER ─────────────────────────────────────── -->
	<div class="page-header">
		<div>
			<div class="page-eyebrow">
				<span class="eyebrow-dot"></span>
				Hall of Records
			</div>
			<h1 class="page-title">Chronicles</h1>
			<p class="page-sub">Your legend, written in stone and starlight.</p>
		</div>
	</div>

	<!-- ── PROFILE CARD ─────────────────────────────────────── -->
	<div class="profile-card" style="--rank-color: {rank.color}">
		<!-- Skin avatar placeholder -->
		<div class="avatar-ring" style="border-color: {rank.color}">
			<div class="avatar-inner">
				<span class="avatar-icon">{rank.icon}</span>
			</div>
		</div>

		<div class="profile-info">
			<div class="player-name">{gameStore.player.playerName}</div>
			<div class="rank-badge" style="color: {rank.color}; border-color: {rank.color}40">
				{rank.title}
			</div>
			{#if currentSkin}
				<div class="skin-label">Skin: <em>{currentSkin.name}</em></div>
			{/if}
		</div>

		{#if favoriteDeity}
			<div class="patron-block" style="--deity-color: {favoriteDeity.color}">
				<div class="patron-label">Patron Deity</div>
				<div class="patron-name" style="color: {favoriteDeity.color}">{favoriteDeity.name}</div>
				<div class="patron-domain">{favoriteDeity.domain}</div>
				<!-- Sigil -->
				<svg class="patron-sigil" viewBox="0 0 24 24" fill="none" stroke={favoriteDeity.color} stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
					<path d={favoriteDeity.sigilPath} />
				</svg>
			</div>
		{:else}
			<div class="patron-block patron-unknown">
				<div class="patron-label">Patron Deity</div>
				<div class="patron-name">Unknown</div>
				<div class="patron-domain">Complete mazes to reveal your affinity</div>
			</div>
		{/if}
	</div>

	<!-- ── STREAK BANNER ─────────────────────────────────────── -->
	<div class="streak-row">
		<div class="streak-card">
			<div class="streak-val">{gameStore.player.currentStreak ?? 0}<span class="streak-flame">🔥</span></div>
			<div class="streak-lbl">Current Streak</div>
		</div>
		<div class="streak-card gold">
			<div class="streak-val">{gameStore.player.bestStreak ?? 0}<span class="streak-flame">⭐</span></div>
			<div class="streak-lbl">Best Streak</div>
		</div>
		<div class="streak-card">
			<div class="streak-val">{gameStore.player.streakShieldsOwned ?? 0}<span class="streak-flame">🛡️</span></div>
			<div class="streak-lbl">Streak Shields</div>
		</div>
	</div>

	<!-- ── STATS CLUSTERS ─────────────────────────────────────── -->

	<!-- Paths Carved -->
	<section class="stats-section">
		<h2 class="section-title">
			<span class="section-icon">🗺️</span>
			Paths Carved
		</h2>
		<div class="stats-grid">
			<div class="stat-card">
				<div class="stat-value">{completedLevels.length}</div>
				<div class="stat-label">Campaign Levels Completed</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{allLevels.length}</div>
				<div class="stat-label">Levels Attempted</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{completedDailies.length}</div>
				<div class="stat-label">Daily Mazes Conquered</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{fmtTime(bestDailyTime)}</div>
				<div class="stat-label">Best Daily Time</div>
			</div>
		</div>
	</section>

	<!-- Stars & Glory -->
	<section class="stats-section">
		<h2 class="section-title">
			<span class="section-icon">⭐</span>
			Stars &amp; Glory
		</h2>
		<div class="stats-grid">
			<div class="stat-card highlight">
				<div class="stat-value gold">{totalStars}</div>
				<div class="stat-label">Total Stars Earned</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{threeStarLevels.length}</div>
				<div class="stat-label">3-Star Completions</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{fmtTime(bestCampaignTime)}</div>
				<div class="stat-label">Best Campaign Time</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{bestCampaignMoves > 0 ? bestCampaignMoves : '—'}</div>
				<div class="stat-label">Fewest Moves (Campaign)</div>
			</div>
		</div>
	</section>

	<!-- The Vault -->
	<section class="stats-section">
		<h2 class="section-title">
			<span class="section-icon">🏦</span>
			The Vault
		</h2>
		<div class="stats-grid">
			<div class="stat-card highlight">
				<div class="stat-value gold">{(gameStore.player.coinsEarnedLifetime ?? 0).toLocaleString()}</div>
				<div class="stat-label">Lifetime Coins Earned</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{gameStore.player.coinCount.toLocaleString()}</div>
				<div class="stat-label">Current Balance</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{gameStore.player.unlockedSkinIds.length}</div>
				<div class="stat-label">Skins Collected</div>
			</div>
			<div class="stat-card">
				<div class="stat-value">{totalItemsOwned}</div>
				<div class="stat-label">Items in Satchel</div>
			</div>
		</div>
	</section>

	<!-- Deity Affinity -->
	<section class="stats-section">
		<h2 class="section-title">
			<span class="section-icon">🔮</span>
			Deity Affinity
		</h2>
		<div class="affinity-summary">
			<div class="affinity-meta">
				<span class="affinity-total">{totalMasteryCompletions} total consecrations</span>
				<span class="affinity-sep">·</span>
				<span>{deitiesByMastery.filter(d => ((gameStore.player.algoMasteryCount ?? {})[d.algorithm] ?? 0) > 0).length} of {DEITY_CATALOG.length} deities encountered</span>
			</div>
		</div>

		<div class="deity-mastery-list">
			{#each deitiesByMastery as deity}
				{@const count = (gameStore.player.algoMasteryCount ?? {})[deity.algorithm] ?? 0}
				{@const barW = Math.min(100, (count / 30) * 100)}
				{@const tierColor = masteryTierColor(count)}
				<div class="deity-row" class:encountered={count > 0} style="--deity-color: {deity.color}">
					<!-- Sigil -->
					<div class="deity-sigil-wrap" style="background: {deity.colorDim}; border-color: {count > 0 ? deity.color + '60' : '#1e293b'}">
						<svg viewBox="0 0 24 24" fill="none" stroke={count > 0 ? deity.color : '#475569'} stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="deity-sigil-svg">
							<path d={deity.sigilPath} />
						</svg>
					</div>

					<!-- Name + tier -->
					<div class="deity-info">
						<div class="deity-name" style="color: {count > 0 ? deity.color : '#475569'}">{deity.name}</div>
						<div class="deity-domain" style="color: {count > 0 ? '#94a3b8' : '#334155'}">{deity.domain}</div>
					</div>

					<!-- Bar + count -->
					<div class="deity-bar-col">
						<div class="deity-bar-track">
							<div class="deity-bar-fill" style="width: {barW}%; background: {deity.color}"></div>
						</div>
						<div class="deity-count">
							<span style="color: {tierColor}; font-weight: 600">{count}</span>
							{#if count > 0}
								<span class="deity-tier" style="color: {tierColor}">{masteryTierLabel(count)}</span>
							{/if}
						</div>
					</div>
				</div>
			{/each}
		</div>
	</section>

</div>

<style>
	/* ── Page shell ── */
	.stats-page {
		max-width: 700px;
		margin: 0 auto;
		padding: 1.5rem 1rem 4rem;
		display: flex;
		flex-direction: column;
		gap: 2rem;
	}

	/* ── Header ── */
	.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
	.page-eyebrow {
		display: flex; align-items: center; gap: 0.4rem;
		font-size: 0.7rem; font-weight: 700; letter-spacing: 0.12em;
		text-transform: uppercase; color: #a78bfa; margin-bottom: 0.35rem;
	}
	.eyebrow-dot {
		width: 6px; height: 6px; border-radius: 50%;
		background: #a78bfa; flex-shrink: 0;
	}
	.page-title { font-size: 1.9rem; font-weight: 800; color: #f1f5f9; margin: 0 0 0.2rem; }
	.page-sub   { font-size: 0.85rem; color: #64748b; margin: 0; }

	/* ── Profile card ── */
	.profile-card {
		background: #0f172a;
		border: 1px solid #1e293b;
		border-radius: 16px;
		padding: 1.25rem;
		display: flex;
		align-items: center;
		gap: 1rem;
		position: relative;
		overflow: hidden;
	}
	.profile-card::before {
		content: '';
		position: absolute; inset: 0;
		background: linear-gradient(135deg, var(--rank-color, #a78bfa)10 0%, transparent 60%);
		opacity: 0.06;
		pointer-events: none;
	}
	.avatar-ring {
		width: 64px; height: 64px; border-radius: 50%;
		border: 2px solid;
		display: flex; align-items: center; justify-content: center;
		flex-shrink: 0;
		background: #1e293b;
	}
	.avatar-inner { display: flex; align-items: center; justify-content: center; }
	.avatar-icon  { font-size: 1.6rem; }

	.profile-info { flex: 1; min-width: 0; }
	.player-name  { font-size: 1.2rem; font-weight: 700; color: #f1f5f9; }
	.rank-badge {
		display: inline-block;
		font-size: 0.7rem; font-weight: 700; letter-spacing: 0.05em;
		border: 1px solid; border-radius: 99px;
		padding: 0.2rem 0.6rem; margin: 0.3rem 0;
	}
	.skin-label { font-size: 0.75rem; color: #64748b; }
	.skin-label em { font-style: normal; color: #94a3b8; }

	/* Patron deity block */
	.patron-block {
		text-align: right; flex-shrink: 0;
		display: flex; flex-direction: column; align-items: flex-end; gap: 0.15rem;
		position: relative;
	}
	.patron-label  { font-size: 0.65rem; text-transform: uppercase; letter-spacing: 0.1em; color: #475569; }
	.patron-name   { font-size: 0.85rem; font-weight: 700; }
	.patron-domain { font-size: 0.7rem; color: #64748b; }
	.patron-sigil  { width: 28px; height: 28px; margin-top: 0.25rem; opacity: 0.85; }
	.patron-unknown .patron-name   { color: #475569; }
	.patron-unknown .patron-domain { font-size: 0.68rem; }

	/* ── Streak row ── */
	.streak-row {
		display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem;
	}
	.streak-card {
		background: #0f172a; border: 1px solid #1e293b; border-radius: 12px;
		padding: 0.9rem 0.75rem; text-align: center;
	}
	.streak-card.gold { border-color: #92400e40; background: #1a1008; }
	.streak-val  { font-size: 1.5rem; font-weight: 800; color: #f1f5f9; line-height: 1; }
	.streak-flame { font-size: 1rem; margin-left: 0.2rem; }
	.streak-lbl  { font-size: 0.68rem; color: #64748b; margin-top: 0.3rem; letter-spacing: 0.03em; }

	/* ── Section ── */
	.stats-section { display: flex; flex-direction: column; gap: 0.75rem; }
	.section-title {
		font-size: 0.75rem; font-weight: 700; letter-spacing: 0.1em;
		text-transform: uppercase; color: #94a3b8;
		display: flex; align-items: center; gap: 0.5rem;
		margin: 0; padding-bottom: 0.5rem;
		border-bottom: 1px solid #1e293b;
	}
	.section-icon { font-size: 0.9rem; }

	/* Stats grid */
	.stats-grid {
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 0.75rem;
	}
	@media (min-width: 480px) {
		.stats-grid { grid-template-columns: repeat(4, 1fr); }
	}
	.stat-card {
		background: #0f172a; border: 1px solid #1e293b; border-radius: 12px;
		padding: 0.9rem 0.75rem; text-align: center;
	}
	.stat-card.highlight { border-color: #2d3748; }
	.stat-value { font-size: 1.4rem; font-weight: 800; color: #e2e8f0; line-height: 1; }
	.stat-value.gold { color: #f59e0b; }
	.stat-label { font-size: 0.65rem; color: #64748b; margin-top: 0.35rem; line-height: 1.3; }

	/* ── Deity affinity ── */
	.affinity-summary { display: flex; gap: 0.4rem; }
	.affinity-meta { font-size: 0.75rem; color: #475569; }
	.affinity-sep  { color: #334155; margin: 0 0.15rem; }
	.affinity-total { color: #64748b; }

	.deity-mastery-list { display: flex; flex-direction: column; gap: 0.5rem; }
	.deity-row {
		display: flex; align-items: center; gap: 0.75rem;
		background: #0a0f1a; border: 1px solid #1e293b; border-radius: 10px;
		padding: 0.6rem 0.75rem;
		opacity: 0.45;
		transition: opacity 0.2s;
	}
	.deity-row.encountered { opacity: 1; }

	.deity-sigil-wrap {
		width: 34px; height: 34px; border-radius: 8px; border: 1px solid;
		display: flex; align-items: center; justify-content: center; flex-shrink: 0;
	}
	.deity-sigil-svg { width: 20px; height: 20px; }

	.deity-info { flex: 1; min-width: 0; }
	.deity-name { font-size: 0.8rem; font-weight: 700; line-height: 1.2; }
	.deity-domain { font-size: 0.65rem; }

	.deity-bar-col { width: 110px; flex-shrink: 0; }
	.deity-bar-track {
		height: 4px; background: #1e293b; border-radius: 99px; overflow: hidden; margin-bottom: 0.3rem;
	}
	.deity-bar-fill { height: 100%; border-radius: 99px; transition: width 0.4s ease; }
	.deity-count { display: flex; align-items: center; gap: 0.4rem; justify-content: flex-end; font-size: 0.7rem; }
	.deity-tier { font-size: 0.6rem; letter-spacing: 0.04em; }
</style>
