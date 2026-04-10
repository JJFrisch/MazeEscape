<script lang="ts">
	import { onMount } from 'svelte';
	import {
		getSupabasePublicConfigDiagnostics,
		SupabaseConfigError,
		SupabaseUrlError
	} from '$lib/supabase/client';
	import {
		fetchCampaignLeaderboard,
		fetchDailyLeaderboard,
		fetchDailyLeaderboardDays,
		type CampaignLeaderboardEntryRow,
		type DailyLeaderboardDayRow,
		type DailyLeaderboardEntryRow
	} from '$lib/supabase/leaderboards';

	type LeaderboardTab = 'daily' | 'campaign';
	type LeaderboardErrorKind = 'config' | 'query' | '';

	let activeTab = $state<LeaderboardTab>('daily');
	let loading = $state(true);
	let dailyLoading = $state(false);
	let errorMessage = $state('');
	let errorKind = $state<LeaderboardErrorKind>('');
	let diagnosticsMessage = $state('');
	let selectedDay = $state('');
	let availableDays = $state<DailyLeaderboardDayRow[]>([]);
	let dailyEntries = $state<DailyLeaderboardEntryRow[]>([]);
	let campaignEntries = $state<CampaignLeaderboardEntryRow[]>([]);

	const dailyPodium = $derived(dailyEntries.slice(0, 3));
	const campaignPodium = $derived(campaignEntries.slice(0, 3));
	const selectedDaySummary = $derived(availableDays.find((day) => day.short_date === selectedDay) ?? null);

	function formatDateLabel(dateValue: string): string {
		const parsed = new Date(dateValue);
		if (Number.isNaN(parsed.getTime())) return dateValue;
		return new Intl.DateTimeFormat('en-US', {
			month: 'long',
			day: 'numeric',
			year: 'numeric'
		}).format(parsed);
	}

	function formatRunTime(seconds: number | null | undefined): string {
		if (!seconds || seconds <= 0) return 'N/A';
		const totalSeconds = Math.floor(seconds);
		const minutes = Math.floor(totalSeconds / 60);
		const remainder = totalSeconds % 60;
		const hundredths = Math.round((seconds - totalSeconds) * 100)
			.toString()
			.padStart(2, '0');
		if (minutes > 0) return `${minutes}:${remainder.toString().padStart(2, '0')}.${hundredths}`;
		return `${remainder}.${hundredths}s`;
	}

	function formatRank(rank: number): string {
		const mod100 = rank % 100;
		if (mod100 >= 11 && mod100 <= 13) return `${rank}th`;
		switch (rank % 10) {
			case 1: return `${rank}st`;
			case 2: return `${rank}nd`;
			case 3: return `${rank}rd`;
			default: return `${rank}th`;
		}
	}

	function podiumLabel(index: number): string {
		return ['First', 'Second', 'Third'][index] ?? `Top ${index + 1}`;
	}

	function resetErrorState() {
		errorMessage = '';
		errorKind = '';
		diagnosticsMessage = '';
	}

	function getErrorMessage(error: unknown, fallback: string): string {
		return error instanceof Error && error.message ? error.message : fallback;
	}

	function isFetchFailure(error: unknown): boolean {
		if (error instanceof TypeError && error.message.includes('Failed to fetch')) return true;
		if (typeof error !== 'object' || error === null) return false;
		const maybeError = error as { message?: unknown; details?: unknown };
		return [maybeError.message, maybeError.details].some(
			(value) => typeof value === 'string' && value.includes('Failed to fetch')
		);
	}

	function handleLeaderboardError(scope: 'daily' | 'initial', error: unknown, fallback: string) {
		console.error(`[leaderboard] ${scope} load failed`, error);
		const configDiagnostics = getSupabasePublicConfigDiagnostics();

		if (error instanceof SupabaseConfigError) {
			errorKind = 'config';
			errorMessage = 'Supabase public env vars were missing when this site was built.';
			diagnosticsMessage = 'Build diagnostics: re-run the GitHub Pages web deploy after updating PUBLIC_SUPABASE_URL and PUBLIC_SUPABASE_PUBLISHABLE_KEY.';
			return;
		}

		if (error instanceof SupabaseUrlError) {
			errorKind = 'config';
			errorMessage = error.message;
			diagnosticsMessage = `Build diagnostics: PUBLIC_SUPABASE_URL is not a valid URL. Current value: ${configDiagnostics.url || 'missing'}.`;
			return;
		}

		errorKind = 'query';
		errorMessage = getErrorMessage(error, fallback);

		if (isFetchFailure(error)) {
			const hostLabel = configDiagnostics.hostname || configDiagnostics.url || 'unknown host';
			diagnosticsMessage = `Query diagnostics: the browser could not reach the configured Supabase host, ${hostLabel}. Check PUBLIC_SUPABASE_URL in the deployed build and confirm the hostname resolves.`;
			return;
		}

		diagnosticsMessage = 'Query diagnostics: Supabase did not return leaderboard data. Check the browser console for the full error payload and verify the leaderboard views remain publicly readable.';
	}

	async function loadDailyEntries(shortDate: string) {
		if (!shortDate) {
			dailyEntries = [];
			return;
		}

		dailyLoading = true;
		resetErrorState();
		try {
			dailyEntries = await fetchDailyLeaderboard(shortDate);
		} catch (error) {
			handleLeaderboardError('daily', error, 'Failed to load the daily leaderboard.');
			dailyEntries = [];
		} finally {
			dailyLoading = false;
		}
	}

	async function loadLeaderboards() {
		loading = true;
		resetErrorState();
		try {
			const [days, campaign] = await Promise.all([
				fetchDailyLeaderboardDays(),
				fetchCampaignLeaderboard()
			]);

			availableDays = days;
			campaignEntries = campaign;
			selectedDay = days[0]?.short_date ?? '';

			if (selectedDay) {
				await loadDailyEntries(selectedDay);
			} else {
				dailyEntries = [];
			}
		} catch (error) {
			handleLeaderboardError('initial', error, 'Failed to load the leaderboard.');
			availableDays = [];
			dailyEntries = [];
			campaignEntries = [];
		} finally {
			loading = false;
		}
	}

	async function handleDayChange(event: Event) {
		const target = event.currentTarget as HTMLSelectElement;
		selectedDay = target.value;
		await loadDailyEntries(selectedDay);
	}

	onMount(() => {
		void loadLeaderboards();
	});
</script>

<svelte:head>
	<title>Leaderboard | Maze Escape: Pathbound</title>
</svelte:head>

<div class="leaderboard-page">
	<section class="hero-shell">
		<div class="hero-grid"></div>
		<div class="hero-orb hero-orb-cyan"></div>
		<div class="hero-orb hero-orb-gold"></div>

		<div class="hero-copy">
			<div class="eyebrow">Guild rankings</div>
			<h1>Leaderboard</h1>
			<p>
				Track the fastest daily clears and the players who have carved the most campaign stars out of the labyrinth.
			</p>
		</div>

		<div class="hero-stats">
			<div class="hero-stat-card">
				<span class="hero-stat-value">{availableDays.length}</span>
				<span class="hero-stat-label">Daily boards indexed</span>
			</div>
			<div class="hero-stat-card gold">
				<span class="hero-stat-value">{campaignEntries.length}</span>
				<span class="hero-stat-label">Campaign legends listed</span>
			</div>
		</div>
	</section>

	<div class="tab-row" role="tablist" aria-label="Leaderboard sections">
		<button class="tab-btn" class:active={activeTab === 'daily'} onclick={() => activeTab = 'daily'}>
			Daily Sprint
		</button>
		<button class="tab-btn" class:active={activeTab === 'campaign'} onclick={() => activeTab = 'campaign'}>
			Campaign Dominion
		</button>
	</div>

	{#if loading}
		<section class="state-card">
			<div class="state-title">Loading rankings</div>
			<p>Connecting to the guild archives.</p>
		</section>
	{:else if errorMessage}
		<section class="state-card error">
			<div class:state-diagnostic-config={errorKind === 'config'} class:state-diagnostic-query={errorKind === 'query'} class="state-diagnostic">
				{errorKind === 'config' ? 'Build diagnostics' : 'Query diagnostics'}
			</div>
			<div class="state-title">Leaderboard unavailable</div>
			<p>{errorMessage}</p>
			<p class="state-note">{diagnosticsMessage}</p>
		</section>
	{:else}
		{#if activeTab === 'daily'}
			<section class="panel-card control-bar">
				<div>
					<div class="panel-title">Daily Sprint</div>
					<p class="panel-copy">Ranked by fastest clear time, then fewest moves.</p>
				</div>
				<label class="day-picker">
					<span>Maze date</span>
					<select value={selectedDay} onchange={handleDayChange} disabled={availableDays.length === 0 || dailyLoading}>
						{#each availableDays as day}
							<option value={day.short_date}>{formatDateLabel(day.played_on)}</option>
						{/each}
					</select>
				</label>
			</section>

			{#if selectedDaySummary}
				<section class="summary-strip">
					<div class="summary-chip">
						<span class="summary-label">Featured day</span>
						<span class="summary-value">{formatDateLabel(selectedDaySummary.played_on)}</span>
					</div>
					<div class="summary-chip">
						<span class="summary-label">Entrants</span>
						<span class="summary-value">{selectedDaySummary.entry_count}</span>
					</div>
					<div class="summary-chip gold">
						<span class="summary-label">Best clear</span>
						<span class="summary-value">{formatRunTime(selectedDaySummary.best_time_seconds)}</span>
					</div>
				</section>
			{/if}

			{#if dailyLoading}
				<section class="state-card">
					<div class="state-title">Loading daily board</div>
					<p>Refreshing the selected maze standings.</p>
				</section>
			{:else if dailyEntries.length === 0}
				<section class="state-card">
					<div class="state-title">No daily runs yet</div>
					<p>The board will populate once players start finishing that maze on time.</p>
				</section>
			{:else}
				<section class="podium-grid">
					{#each dailyPodium as entry, index}
						<article class="podium-card" class:gold={index === 0}>
							<div class="podium-rank">{podiumLabel(index)}</div>
							<div class="podium-player">{entry.player_name}</div>
							<div class="podium-metric">{formatRunTime(entry.completion_time)}</div>
							<div class="podium-sub">{entry.completion_moves} moves</div>
						</article>
					{/each}
				</section>

				<section class="panel-card standings-card">
					<div class="panel-title">All daily entries</div>
					<div class="standings-list">
						{#each dailyEntries as entry}
							<div class="standing-row">
								<div class="standing-rank">{formatRank(entry.rank)}</div>
								<div class="standing-player">{entry.player_name}</div>
								<div class="standing-stat">{formatRunTime(entry.completion_time)}</div>
								<div class="standing-stat muted">{entry.completion_moves} moves</div>
							</div>
						{/each}
					</div>
				</section>
			{/if}
		{:else}
			<section class="panel-card control-bar">
				<div>
					<div class="panel-title">Campaign Dominion</div>
					<p class="panel-copy">Ranked by total stars, then completed levels, best time, and best moves.</p>
				</div>
			</section>

			{#if campaignEntries.length === 0}
				<section class="state-card">
					<div class="state-title">No campaign data yet</div>
					<p>The campaign rankings will appear once synced players complete levels.</p>
				</section>
			{:else}
				<section class="podium-grid">
					{#each campaignPodium as entry, index}
						<article class="podium-card" class:gold={index === 0}>
							<div class="podium-rank">{podiumLabel(index)}</div>
							<div class="podium-player">{entry.player_name}</div>
							<div class="podium-metric">{entry.total_stars} stars</div>
							<div class="podium-sub">{entry.completed_levels} levels cleared</div>
						</article>
					{/each}
				</section>

				<section class="panel-card standings-card">
					<div class="panel-title">All campaign entries</div>
					<div class="standings-list">
						{#each campaignEntries as entry}
							<div class="standing-row campaign">
								<div class="standing-rank">{formatRank(entry.rank)}</div>
								<div class="standing-player">{entry.player_name}</div>
								<div class="standing-stat star">{entry.total_stars} stars</div>
								<div class="standing-stat muted">{entry.completed_levels} clears</div>
								<div class="standing-stat muted">Best {formatRunTime(entry.best_time_seconds)}</div>
							</div>
						{/each}
					</div>
				</section>
			{/if}
		{/if}
	{/if}
</div>

<style>
	.leaderboard-page {
		display: flex;
		flex-direction: column;
		gap: var(--space-6);
	}

	.hero-shell {
		position: relative;
		overflow: hidden;
		display: grid;
		grid-template-columns: minmax(0, 1.5fr) minmax(280px, 0.85fr);
		gap: var(--space-6);
		padding: var(--space-8);
		border-radius: var(--radius-2xl);
		border: 1px solid rgba(255, 255, 255, 0.08);
		background:
			radial-gradient(circle at top left, rgba(34, 211, 238, 0.2), transparent 40%),
			radial-gradient(circle at bottom right, rgba(245, 158, 11, 0.15), transparent 35%),
			linear-gradient(135deg, rgba(4, 12, 24, 0.98), rgba(10, 20, 38, 0.96));
		box-shadow: var(--shadow-lg);
	}

	.hero-grid {
		position: absolute;
		inset: 0;
		background-image:
			linear-gradient(rgba(255, 255, 255, 0.035) 1px, transparent 1px),
			linear-gradient(90deg, rgba(255, 255, 255, 0.035) 1px, transparent 1px);
		background-size: 28px 28px;
		opacity: 0.25;
		pointer-events: none;
	}

	.hero-orb {
		position: absolute;
		border-radius: 50%;
		filter: blur(12px);
		opacity: 0.75;
		pointer-events: none;
	}

	.hero-orb-cyan {
		top: -48px;
		left: 58%;
		width: 180px;
		height: 180px;
		background: rgba(34, 211, 238, 0.18);
	}

	.hero-orb-gold {
		bottom: -72px;
		right: 10%;
		width: 220px;
		height: 220px;
		background: rgba(245, 158, 11, 0.14);
	}

	.hero-copy,
	.hero-stats {
		position: relative;
		z-index: 1;
	}

	.eyebrow {
		display: inline-flex;
		align-items: center;
		padding: 0.4rem 0.8rem;
		border-radius: var(--radius-full);
		background: rgba(255, 255, 255, 0.08);
		border: 1px solid rgba(255, 255, 255, 0.1);
		font-size: var(--text-sm);
		text-transform: uppercase;
		letter-spacing: 0.12em;
		color: var(--color-accent-cyan);
	}

	h1 {
		margin: var(--space-4) 0 var(--space-3);
		font-family: var(--font-display);
		font-size: clamp(2.4rem, 5vw, 4rem);
		line-height: 0.95;
	}

	.hero-copy p,
	.panel-copy,
	.state-card p,
	.state-note {
		margin: 0;
		max-width: 58ch;
		color: var(--color-text-secondary);
		line-height: 1.6;
	}

	.hero-stats {
		display: grid;
		gap: var(--space-4);
		align-content: center;
	}

	.hero-stat-card,
	.panel-card,
	.state-card,
	.summary-chip,
	.podium-card {
		border-radius: var(--radius-xl);
		border: 1px solid var(--color-border);
		background: rgba(7, 14, 28, 0.72);
		box-shadow: var(--shadow-card);
	}

	.hero-stat-card {
		display: flex;
		flex-direction: column;
		gap: 0.35rem;
		padding: var(--space-5);
		backdrop-filter: blur(10px);
	}

	.hero-stat-card.gold {
		border-color: rgba(245, 158, 11, 0.25);
		background: linear-gradient(180deg, rgba(34, 18, 4, 0.75), rgba(12, 15, 27, 0.84));
	}

	.hero-stat-value {
		font-family: var(--font-display);
		font-size: clamp(1.8rem, 4vw, 2.6rem);
		font-weight: 700;
		color: var(--color-text-primary);
	}

	.hero-stat-label,
	.summary-label,
	.podium-rank,
	.state-note {
		font-size: var(--text-sm);
		letter-spacing: 0.04em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}

	.tab-row {
		display: inline-flex;
		gap: var(--space-3);
		flex-wrap: wrap;
	}

	.tab-btn {
		padding: 0.85rem 1.15rem;
		border-radius: var(--radius-full);
		border: 1px solid var(--color-border);
		background: rgba(255, 255, 255, 0.03);
		color: var(--color-text-secondary);
		font: inherit;
		font-weight: 600;
		cursor: pointer;
		transition: transform var(--transition-fast), border-color var(--transition-fast), color var(--transition-fast), background var(--transition-fast);
	}

	.tab-btn.active {
		background: linear-gradient(135deg, rgba(56, 189, 248, 0.2), rgba(14, 165, 233, 0.08));
		border-color: var(--color-border-bright);
		color: var(--color-text-primary);
		transform: translateY(-1px);
	}

	.control-bar {
		display: flex;
		justify-content: space-between;
		align-items: end;
		gap: var(--space-4);
		padding: var(--space-5);
	}

	.panel-title,
	.state-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
	}

	.day-picker {
		display: flex;
		flex-direction: column;
		gap: 0.45rem;
		min-width: min(280px, 100%);
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
	}

	.day-picker select {
		padding: 0.9rem 1rem;
		border-radius: var(--radius-lg);
		border: 1px solid var(--color-border);
		background: rgba(7, 14, 28, 0.92);
		color: var(--color-text-primary);
		font: inherit;
	}

	.summary-strip {
		display: grid;
		grid-template-columns: repeat(3, minmax(0, 1fr));
		gap: var(--space-4);
	}

	.summary-chip {
		display: flex;
		flex-direction: column;
		gap: 0.35rem;
		padding: var(--space-4);
	}

	.summary-chip.gold {
		border-color: rgba(245, 158, 11, 0.25);
		background: linear-gradient(180deg, rgba(34, 18, 4, 0.78), rgba(12, 15, 27, 0.88));
	}

	.summary-value,
	.podium-player,
	.podium-metric {
		color: var(--color-text-primary);
	}

	.summary-value {
		font-size: var(--text-lg);
		font-weight: 600;
	}

	.podium-grid {
		display: grid;
		grid-template-columns: repeat(3, minmax(0, 1fr));
		gap: var(--space-4);
	}

	.podium-card {
		position: relative;
		overflow: hidden;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
		padding: var(--space-5);
		background:
			linear-gradient(180deg, rgba(13, 22, 39, 0.96), rgba(9, 15, 29, 0.88)),
			radial-gradient(circle at top right, rgba(56, 189, 248, 0.18), transparent 45%);
	}

	.podium-card.gold {
		border-color: rgba(245, 158, 11, 0.3);
		background:
			linear-gradient(180deg, rgba(38, 24, 7, 0.94), rgba(16, 14, 24, 0.9)),
			radial-gradient(circle at top right, rgba(245, 158, 11, 0.18), transparent 42%);
	}

	.podium-player {
		font-family: var(--font-display);
		font-size: 1.35rem;
		font-weight: 700;
	}

	.podium-metric {
		font-size: 1.1rem;
		font-weight: 600;
	}

	.podium-sub,
	.standing-stat.muted,
	.standing-player {
		color: var(--color-text-secondary);
	}

	.standings-card {
		padding: var(--space-5);
	}

	.standings-list {
		display: flex;
		flex-direction: column;
		margin-top: var(--space-4);
		border-top: 1px solid rgba(255, 255, 255, 0.06);
	}

	.standing-row {
		display: grid;
		grid-template-columns: 88px minmax(0, 1fr) 160px 130px;
		align-items: center;
		gap: var(--space-4);
		padding: 1rem 0.25rem;
		border-bottom: 1px solid rgba(255, 255, 255, 0.06);
	}

	.standing-row.campaign {
		grid-template-columns: 88px minmax(0, 1fr) 150px 120px 150px;
	}

	.standing-rank {
		font-family: var(--font-display);
		font-weight: 700;
		color: var(--color-accent-cyan);
	}

	.standing-player {
		font-weight: 600;
		color: var(--color-text-primary);
	}

	.standing-stat {
		text-align: right;
		font-variant-numeric: tabular-nums;
		color: var(--color-text-primary);
	}

	.standing-stat.star {
		color: var(--color-accent-gold);
		font-weight: 700;
	}

	.state-card {
		padding: var(--space-6);
	}

	.state-diagnostic {
		display: inline-flex;
		align-items: center;
		margin-bottom: var(--space-3);
		padding: 0.35rem 0.7rem;
		border-radius: var(--radius-full);
		border: 1px solid rgba(255, 255, 255, 0.14);
		font-size: 0.72rem;
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}

	.state-diagnostic-config {
		background: rgba(245, 158, 11, 0.14);
		border-color: rgba(245, 158, 11, 0.3);
		color: rgba(253, 224, 71, 0.95);
	}

	.state-diagnostic-query {
		background: rgba(56, 189, 248, 0.12);
		border-color: rgba(56, 189, 248, 0.28);
		color: rgba(125, 211, 252, 0.98);
	}

	.state-card.error {
		border-color: rgba(244, 63, 94, 0.28);
		background: linear-gradient(180deg, rgba(40, 10, 20, 0.82), rgba(14, 10, 20, 0.92));
	}

	@media (max-width: 960px) {
		.hero-shell,
		.summary-strip,
		.podium-grid {
			grid-template-columns: 1fr;
		}

		.control-bar {
			align-items: stretch;
			flex-direction: column;
		}

		.standing-row,
		.standing-row.campaign {
			grid-template-columns: 72px minmax(0, 1fr);
		}

		.standing-stat {
			text-align: left;
		}
	}

	@media (max-width: 640px) {
		.hero-shell,
		.panel-card,
		.state-card,
		.podium-card {
			padding: var(--space-5);
		}

		h1 {
			font-size: 2.25rem;
		}
	}
</style>