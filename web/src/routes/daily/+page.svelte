<script lang="ts">
	import { base } from '$app/paths';
	import { onDestroy, onMount, tick } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getDailyMazeForDate, getDailyMazesForMonth, getDailyMazeSeed } from '$lib/core/daily';
	import { createGameSession, getHint, getCompassPath, calculateStars, getMoveThresholdsForOptimalPath } from '$lib/core/session';
	import type { GameSessionState } from '$lib/core/session';
	import type { MasteryRewardUnlock } from '$lib/core/deities';
	import { canMove, applyMove } from '$lib/core/maze';
	import type { Direction, DailyMazeLevel } from '$lib/core/types';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import MazeIntroOverlay from '$lib/components/MazeIntroOverlay.svelte';
	import MazeOutroOverlay from '$lib/components/MazeOutroOverlay.svelte';
	import AchievementUnlockPopup from '$lib/components/AchievementUnlockPopup.svelte';
	import MasteryRewardPopup from '$lib/components/MasteryRewardPopup.svelte';

	const today = new Date();
	let viewYear = $state(today.getFullYear());
	let viewMonth = $state(today.getMonth());
	let viewport = $state({ width: 0, height: 0 });
	let calendarDays = $derived(getDailyMazesForMonth(viewYear, viewMonth, viewport));

	let selectedDate = $state<string | null>(null);
	let selectedDaily = $state<DailyMazeLevel | null>(null);
	let session = $state<GameSessionState | null>(null);
	let elapsed = $state(0);
	let timerInterval: ReturnType<typeof setInterval>;
	let showIntro = $state(false);
	let showOutro = $state(false);
    let victoryStars = $state({ star1: false, star2: false, star3: false, star4: false, star5: false, total: 0 });
	let coinsEarned = $state(0);
	let previousBestTime = $state(0);
	let previousBestMoves = $state(0);
	let visitedCells = $state(new Set<string>());
	let playing = $state(false);
	let moveTargets = $state({ twoStarMoves: 0, fiveStarMoves: 0 });
	let hourglassFrozen = $state(false);
	let hourglassTimer: ReturnType<typeof setTimeout> | undefined;
	let compassTimer: ReturnType<typeof setTimeout> | undefined;
	let masteryUnlocks = $state<MasteryRewardUnlock[]>([]);
	let newlyUnlocked = $state<string[]>([]);
	let shownMasteryUnlock = $derived(masteryUnlocks[0] ?? null);
	let shownAchievementId = $derived(newlyUnlocked[0] ?? null);

	const DAILY_PHRASES = [
		"Shuffling today's challenge…",
		'Seeding the labyrinth…',
		'Laying down bricks…',
		'Routing dead ends…',
		'Maze ready!'
	];
	const ENABLE_DAILY_INTRO = true;

	// Check if daily unlocked (requires World 1 Level 10 completed)
	const unlocked = $derived(gameStore.isDailyMazeUnlocked());
	const activeEvent = $derived(gameStore.activeEvent);
	const activeEventProgress = $derived(activeEvent ? gameStore.getEventProgress(activeEvent.id) : undefined);
	const canAcceptInput = $derived(playing && session !== null && !session.isComplete && !showIntro && !showOutro);
	const selectedDateLabel = $derived(selectedDate ? formatDisplayDate(selectedDate) : '');

	const monthNames = [
		'January', 'February', 'March', 'April', 'May', 'June',
		'July', 'August', 'September', 'October', 'November', 'December'
	];

	function updateViewport() {
		if (typeof window === 'undefined') return;
		viewport = {
			width: window.innerWidth,
			height: window.innerHeight
		};
	}

	function isToday(dateStr: string) {
		const d = new Date(dateStr);
		return d.getFullYear() === today.getFullYear() &&
			d.getMonth() === today.getMonth() &&
			d.getDate() === today.getDate();
	}

	function isFuture(dateStr: string) {
		const d = new Date(dateStr);
		d.setHours(23,59,59);
		return d > today;
	}

	function getDayResult(dateStr: string) {
		return gameStore.getDailyResult(dateStr);
	}

	function formatDisplayDate(dateStr: string) {
		const parsed = new Date(dateStr);
		if (Number.isNaN(parsed.getTime())) return dateStr;
		return new Intl.DateTimeFormat('en-US', {
			month: 'short',
			day: 'numeric',
			year: 'numeric'
		}).format(parsed);
	}

	function prevMonth() {
		if (viewMonth === 0) {
			viewMonth = 11;
			viewYear--;
		} else {
			viewMonth--;
		}
	}

	function nextMonth() {
		if (viewMonth === 11) {
			viewMonth = 0;
			viewYear++;
		} else {
			viewMonth++;
		}
	}

	async function startDailyMaze(dateStr: string) {
		if (isFuture(dateStr) || !unlocked) return;

		clearInterval(timerInterval);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
		showIntro = false;
		showOutro = false;
		elapsed = 0;
		coinsEarned = 0;
		hourglassFrozen = false;
		masteryUnlocks = [];
		newlyUnlocked = [];
		victoryStars = { star1: false, star2: false, star3: false, star4: false, star5: false, total: 0 };

		const d = new Date(dateStr);
		const dailyLevel = getDailyMazeForDate(d, viewport);
		const seed = getDailyMazeSeed(d);

		session = createGameSession({
			width: dailyLevel.width,
			height: dailyLevel.height,
			algorithm: dailyLevel.levelType,
			seed,
			twoStarMoves: dailyLevel.movesNeeded,
			threeStarTime: dailyLevel.timeNeeded
		});
		moveTargets = getMoveThresholdsForOptimalPath(session.maze.pathLength);
		selectedDate = dateStr;
		selectedDaily = dailyLevel;
		playing = true;
		visitedCells = new Set([`${session.maze.start.x},${session.maze.start.y}`]);
		await tick();
		showIntro = ENABLE_DAILY_INTRO;
		if (!ENABLE_DAILY_INTRO) {
			startGameplay();
		}
	}

	function startGameplay() {
		showIntro = false;
		clearInterval(timerInterval);
		timerInterval = setInterval(() => {
			if (session && !session.isComplete && !hourglassFrozen) elapsed += 0.01;
		}, 10);
	}

	function handleMove(direction: Direction) {
		if (!canAcceptInput || !session) return;

		const { maze, playerPos, moves } = session;
		if (!canMove(maze.cells, playerPos, direction, maze.width, maze.height)) return;

		const newPos = applyMove(playerPos, direction);
		const isComplete = newPos.x === maze.end.x && newPos.y === maze.end.y;

		session = {
			maze,
			playerPos: newPos,
			moves: moves + 1,
			elapsed: session.elapsed,
			isComplete,
			hintsUsed: session.hintsUsed,
			hintPath: null
		};

		visitedCells = new Set([...visitedCells, `${newPos.x},${newPos.y}`]);
		if (isComplete) onComplete();
	}

	function onComplete() {
		clearInterval(timerInterval);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
		hourglassFrozen = false;
		if (!session || !selectedDate || !selectedDaily) return;

		const stars = calculateStars(
			session.moves,
			elapsed,
			session.hintsUsed,
			moveTargets.twoStarMoves,
			selectedDaily.timeNeeded,
			moveTargets.fiveStarMoves,
			Math.floor(selectedDaily.timeNeeded * 0.6)
		);
		victoryStars = stars;

		coinsEarned = 50 + stars.total * 25;
		gameStore.addCoins(coinsEarned);
		gameStore.addCrystalShards(stars.total);

		// Track algorithm mastery for the deity system
		if (selectedDaily.levelType) {
			const unlockedMasteryRewards = gameStore.recordAlgoMastery(selectedDaily.levelType);
			if (unlockedMasteryRewards.length > 0) {
				masteryUnlocks = [...unlockedMasteryRewards];
			}
		}

		gameStore.incrementMazesCompleted();

		if (elapsed < 30) gameStore.markAchievementProgress('speed_runner', 1);
		if (session.hintsUsed === 0) gameStore.markAchievementProgress('hint_free', 1);

		const result: DailyMazeLevel = {
			...selectedDaily,
			status: 'completed',
			completionTime: elapsed,
			completionMoves: session.moves
		};
		const priorResult = gameStore.getDailyResult(selectedDate);
		previousBestTime = priorResult?.completionTime ?? 0;
		previousBestMoves = priorResult?.completionMoves ?? 0;
		gameStore.saveDailyResult(result);
		if (activeEvent) {
			gameStore.incrementEventProgress(activeEvent.id, 1);
		}

		const unlockedAchievements = gameStore.checkAchievements();
		if (unlockedAchievements.length > 0) {
			newlyUnlocked = [...unlockedAchievements];
		}

		showOutro = true;
	}

	function useHint() {
		if (!session || !canAcceptInput) return;
		if (gameStore.usePowerup('hint')) {
			const nextSession = { ...session };
			const hintPath = getHint(nextSession);
			session = { ...nextSession, hintPath };
		}
	}

	function useCompass() {
		if (!session || !canAcceptInput) return;
		if (gameStore.usePowerup('compass')) {
			const nextSession = { ...session };
			const hintPath = getCompassPath(nextSession);
			session = { ...nextSession, hintPath };
			clearTimeout(compassTimer);
			compassTimer = setTimeout(() => {
				if (!session) return;
				session = { ...session, hintPath: null };
			}, 3000);
		}
	}

	function useHourglass() {
		if (!session || !canAcceptInput || hourglassFrozen) return;
		if (gameStore.usePowerup('hourglass')) {
			hourglassFrozen = true;
			clearTimeout(hourglassTimer);
			hourglassTimer = setTimeout(() => {
				hourglassFrozen = false;
			}, 15000);
		}
	}

	function useBlinkScroll() {
		if (!session || !canAcceptInput) return;
		if (gameStore.usePowerup('blinkScroll')) {
			const candidates: Array<{ x: number; y: number }> = [];
			for (let y = 0; y < session.maze.height; y++) {
				for (let x = 0; x < session.maze.width; x++) {
					if (x !== session.playerPos.x || y !== session.playerPos.y) {
						candidates.push({ x, y });
					}
				}
			}
			const destination = candidates[Math.floor(Math.random() * candidates.length)];
			if (!destination) return;
			visitedCells = new Set([...visitedCells, `${destination.x},${destination.y}`]);
			session = { ...session, playerPos: destination, hintPath: null };
		}
	}

	function useDoubleCoins() {
		if (!session || !canAcceptInput) return;
		gameStore.usePowerup('doubleCoinsToken');
	}

	function backToCalendar() {
		clearInterval(timerInterval);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
		showIntro = false;
		showOutro = false;
		playing = false;
		hourglassFrozen = false;
		session = null;
	}

	function handleKeydown(e: KeyboardEvent) {
		if (!canAcceptInput) return;
		const dirMap: Record<string, Direction> = {
			ArrowUp: 'up', ArrowDown: 'down', ArrowLeft: 'left', ArrowRight: 'right',
			w: 'up', s: 'down', a: 'left', d: 'right'
		};
		const dir = dirMap[e.key];
		if (dir) { e.preventDefault(); handleMove(dir); }
	}

	let touchStartX = 0;
	let touchStartY = 0;
	function handleTouchStart(e: TouchEvent) { touchStartX = e.touches[0].clientX; touchStartY = e.touches[0].clientY; }
	function handleTouchEnd(e: TouchEvent) {
		if (!canAcceptInput) return;
		const dx = e.changedTouches[0].clientX - touchStartX;
		const dy = e.changedTouches[0].clientY - touchStartY;
		if (Math.abs(dx) > Math.abs(dy)) { if (Math.abs(dx) > 30) handleMove(dx > 0 ? 'right' : 'left'); }
		else { if (Math.abs(dy) > 30) handleMove(dy > 0 ? 'down' : 'up'); }
	}

	// Leading zeros for padding
	function padDay(dateStr: string) {
		return new Date(dateStr).getDate();
	}

	function getFirstDayOffset() {
		return new Date(viewYear, viewMonth, 1).getDay();
	}

	onDestroy(() => {
		clearInterval(timerInterval);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
	});
	onMount(() => {
		updateViewport();
	});
</script>

<svelte:window onkeydown={handleKeydown} onresize={updateViewport} />

<svelte:head>
	<title>Daily Maze – Maze Escape: Pathbound</title>
</svelte:head>

{#if !unlocked}
	<div class="locked-shell">
		<div class="locked-glow-1" aria-hidden="true"></div>
		<div class="locked-glow-2" aria-hidden="true"></div>
		<div class="locked-card">
			<div class="locked-icon-wrap">
				<svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<rect x="3" y="11" width="18" height="11" rx="2" stroke="currentColor" stroke-width="2"/>
					<path d="M7 11V7a5 5 0 0110 0v4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
				</svg>
			</div>
			<h1 class="locked-title">Daily Maze Locked</h1>
			<p class="locked-desc">
				Complete <strong>World 1, Level 10</strong> to unlock the Daily Maze —
				a new procedural challenge every day.
			</p>
			<a href="{base}/campaign/worlds/1" class="locked-cta">
				<svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<polygon points="6,3 20,12 6,21" fill="currentColor"/>
				</svg>
				Play World 1
			</a>
		</div>
	</div>
{:else if !playing}
	<div class="daily-page">
		<!-- Page header -->
		<div class="page-header">
			<div class="page-header-left">
				<h1 class="page-title">📅 Daily Maze</h1>
				<p class="page-subtitle">A new challenge every day. The labyrinth never rests.</p>
			</div>

			<!-- Streak counter -->
			<div class="streak-panel">
				<div class="streak-block" class:streak-active={(gameStore.player.currentStreak ?? 0) > 0}>
					<span class="streak-flame">{(gameStore.player.currentStreak ?? 0) > 0 ? '🔥' : '💤'}</span>
					<div class="streak-info">
						<span class="streak-number">{gameStore.player.currentStreak ?? 0}</span>
						<span class="streak-label">day streak</span>
					</div>
				</div>
				<div class="streak-divider"></div>
				<div class="streak-best">
					<span class="streak-best-num">{gameStore.player.bestStreak ?? 0}</span>
					<span class="streak-label">best</span>
				</div>
				{#if (gameStore.player.streakShieldsOwned ?? 0) > 0}
					<div class="streak-shield-badge" title="Streak shields owned">
						🛡️ {gameStore.player.streakShieldsOwned}
					</div>
				{/if}
			</div>
		</div>

		{#if activeEvent}
			<div class="event-panel" style="--event-accent:{activeEvent.themeAccent}">
				<div class="event-copy">
					<span class="event-eyebrow">Seasonal event</span>
					<h2>{activeEvent.name}</h2>
					<p>{activeEvent.description}</p>
				</div>
				<div class="event-progress">
					<span class="event-progress-label">{activeEvent.trackerLabel}</span>
					<span class="event-progress-value">{activeEventProgress?.progress ?? 0} / {activeEvent.milestones[activeEvent.milestones.length - 1]?.at ?? 0}</span>
					<div class="event-progress-track">
						<div class="event-progress-fill" style="width:{Math.min(((activeEventProgress?.progress ?? 0) / (activeEvent.milestones[activeEvent.milestones.length - 1]?.at ?? 1)) * 100, 100)}%;"></div>
					</div>
				</div>
			</div>
		{/if}

		<!-- Streak milestone hint -->
		{#if (gameStore.player.currentStreak ?? 0) > 0}
			{@const next = [3, 7, 14, 30, 60, 100].find(m => m > (gameStore.player.currentStreak ?? 0))}
			{#if next}
				<div class="streak-milestone-bar">
					<div class="milestone-fill" style="width:{Math.min(((gameStore.player.currentStreak ?? 0) / next) * 100, 100)}%;"></div>
					<span class="milestone-label">🎯 {next - (gameStore.player.currentStreak ?? 0)} day{next - (gameStore.player.currentStreak ?? 0) !== 1 ? 's' : ''} to {next}-day milestone</span>
				</div>
			{/if}
		{/if}

		<div class="calendar-header">
			<button class="cal-nav" onclick={prevMonth}>◀</button>
			<h2 class="cal-month">{monthNames[viewMonth]} {viewYear}</h2>
			<button class="cal-nav" onclick={nextMonth}>▶</button>
		</div>

		<div class="calendar">
			<div class="cal-weekdays">
				{#each ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'] as day}
					<span class="cal-weekday">{day}</span>
				{/each}
			</div>

			<div class="cal-grid">
				{#each Array(getFirstDayOffset()) as _}
					<div class="cal-cell empty"></div>
				{/each}

				{#each calendarDays as daily}
					{@const result = getDayResult(daily.shortDate)}
					{@const future = isFuture(daily.date)}
					{@const todayClass = isToday(daily.date)}
					{@const isCompleted = result?.status === 'completed' || result?.status === 'completed_late'}
					<button
						class="cal-cell"
						class:future
						class:today={todayClass}
						class:completed={isCompleted}
						disabled={future}
						onclick={() => startDailyMaze(daily.date)}
					>
						<span class="cal-day">{padDay(daily.date)}</span>
						{#if isCompleted}
							<span class="cal-stars">✓</span>
						{:else if todayClass}
							<span class="cal-new">NEW</span>
						{/if}
					</button>
				{/each}
			</div>
		</div>
	</div>
{:else if session}
	<div
		class="gameplay"
		ontouchstart={handleTouchStart}
		ontouchend={handleTouchEnd}
		role="application"
		aria-label="Daily maze gameplay"
	>
		<div class="game-hud">
			<button class="hud-back" onclick={backToCalendar}>← Calendar</button>
			<span class="hud-label">📅 {selectedDateLabel}</span>
			<div class="hud-stats">
				<div class="hud-stat">
					<span>⏱️ {elapsed.toFixed(1)}s</span>
					{#if hourglassFrozen}
						<span class="frozen-badge">FROZEN</span>
					{:else}
						<span class="hud-stat-target">/ {selectedDaily?.timeNeeded ?? 0}s</span>
					{/if}
				</div>
				<div class="hud-stat">
					<span>👣 {session.moves}</span>
					<span class="hud-stat-target">/ {moveTargets.twoStarMoves}</span>
				</div>
			</div>
		</div>

		<div class="maze-area">
			<MazeRenderer
				maze={session.maze}
				playerPos={session.playerPos}
				wallColor={gameStore.player.wallColor}
				backgroundColor={gameStore.player.mazeBackgroundColor}
				hintPath={session.hintPath}
				showVisited={true}
				{visitedCells}
			/>
		</div>

		<div class="controls-bar">
			<div class="powerups">
				<button class="powerup-btn" onclick={useHint} disabled={gameStore.player.hintsOwned <= 0}>
					💡 Hint
					<span class="powerup-count">({gameStore.player.hintsOwned})</span>
				</button>
				<button class="powerup-btn" onclick={useCompass} disabled={(gameStore.player.compassOwned ?? 0) <= 0}>
					🧭 Compass
					<span class="powerup-count">({gameStore.player.compassOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useHourglass} disabled={(gameStore.player.hourglassOwned ?? 0) <= 0 || hourglassFrozen}>
					⏳ Hourglass
					<span class="powerup-count">({gameStore.player.hourglassOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useBlinkScroll} disabled={(gameStore.player.blinkScrollsOwned ?? 0) <= 0}>
					✨ Blink
					<span class="powerup-count">({gameStore.player.blinkScrollsOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useDoubleCoins} disabled={(gameStore.player.doubleCoinsTokensOwned ?? 0) <= 0 || gameStore.player.doubleCoinsActive}>
					🪙 2× Coins
					<span class="powerup-count">({gameStore.player.doubleCoinsTokensOwned ?? 0})</span>
				</button>
			</div>
			<div class="dpad" role="group" aria-label="Movement controls">
				<button class="dpad-btn" onclick={() => handleMove('up')} disabled={!canAcceptInput}>▲</button>
				<div class="dpad-middle">
					<button class="dpad-btn" onclick={() => handleMove('left')} disabled={!canAcceptInput}>◀</button>
					<button class="dpad-btn" onclick={() => handleMove('right')} disabled={!canAcceptInput}>▶</button>
				</div>
				<button class="dpad-btn" onclick={() => handleMove('down')} disabled={!canAcceptInput}>▼</button>
			</div>
		</div>
	</div>
{/if}

<!-- Overlays live outside the session/playing block so their lifecycle is solely
     controlled by showIntro/showOutro, not by session reassignments -->
{#if showIntro}
	<MazeIntroOverlay
		title="Daily Maze"
		subtitle={selectedDateLabel}
		phrases={DAILY_PHRASES}
		accentColor="#34d399"
		ondismiss={startGameplay}
	/>
{/if}

{#if showOutro && session && selectedDaily}
	<MazeOutroOverlay
		open={showOutro}
		title="Daily Maze Complete!"
		subtitle={selectedDateLabel}
		playerName={gameStore.player.playerName}
		time={elapsed}
		moves={session.moves}
		stars={victoryStars.total}
		twoStarMoves={moveTargets.twoStarMoves}
		threeStarTime={selectedDaily.timeNeeded}
		fiveStarMoves={moveTargets.fiveStarMoves}
		fiveStarTime={Math.floor(selectedDaily.timeNeeded * 0.6)}
		coins={coinsEarned}
		accentColor="#34d399"
		rewardSummary={activeEvent
			? `${activeEvent.name} progress has advanced with this clear, along with your coins and mastery records.`
			: 'Today’s clear has been archived with your stars, coins, and mastery progress.'}
		mazeWidth={selectedDaily.width}
		mazeHeight={selectedDaily.height}
		algoName={selectedDaily.levelType}
		algoId={selectedDaily.levelType}
		algoLinkBase={base}
		bestTime={previousBestTime}
		bestMoves={previousBestMoves}
		actions={[
			{ label: 'Retry', onclick: () => { showOutro = false; if (selectedDate) startDailyMaze(selectedDate); } },
			{ label: '← Calendar', primary: true, onclick: () => { showOutro = false; backToCalendar(); } }
		]}
	/>
{/if}

{#if shownMasteryUnlock}
	<MasteryRewardPopup
		unlock={shownMasteryUnlock}
		ondismiss={() => {
			masteryUnlocks = masteryUnlocks.slice(1);
		}}
	/>
{:else if shownAchievementId}
	<AchievementUnlockPopup
		achievementId={shownAchievementId}
		ondismiss={() => {
			newlyUnlocked = newlyUnlocked.slice(1);
		}}
	/>
{/if}

<style>
	.daily-page {
		max-width: 600px;
		margin: 0 auto;
		padding: var(--space-4);
	}

	/* ── Locked state ───────────────────────────── */
	.locked-shell {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 55vh;
		padding: var(--space-8) var(--space-4);
	}
	.locked-glow-1 {
		position: absolute;
		width: 500px; height: 400px;
		top: -60px; left: 50%;
		transform: translateX(-50%);
		background: radial-gradient(ellipse, rgba(56,189,248,0.07), transparent 65%);
		filter: blur(70px);
		pointer-events: none;
	}
	.locked-glow-2 {
		position: absolute;
		width: 400px; height: 300px;
		bottom: -40px; left: 50%;
		transform: translateX(-50%);
		background: radial-gradient(ellipse, rgba(245,158,11,0.05), transparent 65%);
		filter: blur(70px);
		pointer-events: none;
	}
	.locked-card {
		position: relative;
		z-index: 1;
		max-width: 420px;
		text-align: center;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-5);
		padding: var(--space-10) var(--space-8);
		background: rgba(8,16,32,0.85);
		border: 1px solid rgba(56,189,248,0.15);
		border-radius: var(--radius-2xl);
		backdrop-filter: blur(20px);
		box-shadow: 0 8px 48px rgba(0,0,0,0.5);
		animation: fade-up 0.5s ease both;
	}

	.event-panel {
		display: grid;
		grid-template-columns: minmax(0, 1.6fr) minmax(220px, 1fr);
		gap: var(--space-4);
		padding: var(--space-5);
		margin-bottom: var(--space-6);
		border-radius: var(--radius-xl);
		background: linear-gradient(135deg, color-mix(in srgb, var(--event-accent) 15%, transparent), rgba(15, 23, 42, 0.84));
		border: 1px solid color-mix(in srgb, var(--event-accent) 32%, transparent);
	}

	.event-eyebrow {
		display: inline-block;
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		margin-bottom: 0.5rem;
		color: color-mix(in srgb, var(--event-accent) 78%, white 22%);
	}

	.event-copy h2 {
		margin-bottom: 0.35rem;
		font-family: var(--font-display);
	}

	.event-copy p {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
	}

	.event-progress {
		display: grid;
		align-content: center;
		gap: 0.45rem;
	}

	.event-progress-label,
	.event-progress-value {
		font-size: var(--text-sm);
	}

	.event-progress-track {
		height: 10px;
		border-radius: 999px;
		background: rgba(15, 23, 42, 0.7);
		overflow: hidden;
		border: 1px solid rgba(148, 163, 184, 0.16);
	}

	.event-progress-fill {
		height: 100%;
		border-radius: inherit;
		background: linear-gradient(90deg, color-mix(in srgb, var(--event-accent) 70%, white 30%), var(--event-accent));
	}

	@media (max-width: 720px) {
		.event-panel {
			grid-template-columns: 1fr;
		}
	}
	@keyframes fade-up { from{opacity:0;transform:translateY(20px);} to{opacity:1;transform:translateY(0);} }

	.locked-icon-wrap {
		width: 72px; height: 72px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.2);
		border-radius: var(--radius-2xl);
		color: var(--color-accent-primary);
		animation: float-y 4s ease-in-out infinite;
	}
	.locked-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
	}
	.locked-desc {
		font-size: var(--text-base);
		color: var(--color-text-secondary);
		line-height: 1.65;
		max-width: 320px;
	}
	.locked-desc strong { color: var(--color-text-primary); }
	.locked-cta {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 13px 28px;
		background: var(--color-accent-primary);
		color: #fff;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		text-decoration: none;
		transition: all var(--transition-base);
		box-shadow: 0 0 28px rgba(56,189,248,0.3);
		margin-top: var(--space-2);
	}
	.locked-cta:hover {
		background: #0ea5e9;
		box-shadow: 0 0 40px rgba(56,189,248,0.45);
		transform: translateY(-2px);
		color: #fff;
	}

	/* ── Page header ────────────────────────────── */
	.page-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: var(--space-4);
		margin-bottom: var(--space-5);
		flex-wrap: wrap;
	}
	.page-header-left { flex: 1; min-width: 0; }

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-1);
	}
	.page-subtitle {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
	}

	/* ── Streak panel ────────────────────────────── */
	.streak-panel {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-3) var(--space-4);
		background: rgba(255,255,255,0.03);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-xl);
		backdrop-filter: blur(8px);
		flex-shrink: 0;
	}
	.streak-block {
		display: flex;
		align-items: center;
		gap: var(--space-2);
	}
	.streak-flame { font-size: 1.4rem; line-height: 1; }
	.streak-info { display: flex; flex-direction: column; align-items: flex-start; }
	.streak-number {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		font-weight: 700;
		color: var(--color-text-primary);
		line-height: 1;
	}
	.streak-block.streak-active .streak-number { color: #f97316; }
	.streak-label {
		font-size: 9px;
		font-weight: 700;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}
	.streak-divider {
		width: 1px;
		height: 32px;
		background: rgba(255,255,255,0.08);
	}
	.streak-best {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2px;
	}
	.streak-best-num {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		font-weight: 700;
		color: var(--color-accent-gold);
		line-height: 1;
	}
	.streak-shield-badge {
		font-size: var(--text-sm);
		font-weight: 700;
		padding: 4px 8px;
		background: rgba(148,163,184,0.10);
		border: 1px solid rgba(148,163,184,0.25);
		border-radius: var(--radius-full);
		color: #94a3b8;
		cursor: default;
	}

	/* ── Streak milestone bar ────────────────────── */
	.streak-milestone-bar {
		position: relative;
		height: 28px;
		background: rgba(249,115,22,0.06);
		border: 1px solid rgba(249,115,22,0.18);
		border-radius: var(--radius-full);
		margin-bottom: var(--space-5);
		overflow: hidden;
	}
	.milestone-fill {
		position: absolute;
		top: 0; left: 0; bottom: 0;
		background: linear-gradient(90deg, rgba(249,115,22,0.25), rgba(249,115,22,0.12));
		border-radius: var(--radius-full);
		transition: width 0.8s cubic-bezier(0.22,1,0.36,1);
	}
	.milestone-label {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		height: 100%;
		padding: 0 var(--space-4);
		font-size: var(--text-xs);
		font-weight: 600;
		color: #f97316;
	}

	.calendar-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: var(--space-4);
	}

	.cal-month {
		font-family: var(--font-display);
		font-size: var(--text-xl);
	}

	.cal-nav {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		color: var(--color-text-primary);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		cursor: pointer;
	}

	.cal-weekdays {
		display: grid;
		grid-template-columns: repeat(7, 1fr);
		gap: 2px;
		margin-bottom: var(--space-2);
	}

	.cal-weekday {
		text-align: center;
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		font-weight: 600;
		text-transform: uppercase;
	}

	.cal-grid {
		display: grid;
		grid-template-columns: repeat(7, 1fr);
		gap: 4px;
	}

	.cal-cell {
		aspect-ratio: 1;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		cursor: pointer;
		transition: all var(--transition-fast);
		gap: 2px;
	}

	.cal-cell.empty {
		background: transparent;
		border-color: transparent;
		cursor: default;
	}

	.cal-cell:not(.empty):not(.future):hover {
		border-color: var(--color-accent-primary);
		background: var(--color-bg-card-hover);
	}

	.cal-cell.today {
		border-color: var(--color-accent-primary);
		box-shadow: 0 0 12px rgba(99, 102, 241, 0.3);
	}

	.cal-cell.completed {
		background: rgba(52, 211, 153, 0.1);
		border-color: var(--color-accent-green);
	}

	.cal-cell.future {
		opacity: 0.3;
		cursor: default;
	}

	.cal-day {
		font-weight: 600;
		font-size: var(--text-sm);
	}

	.cal-stars {
		font-size: 9px;
		color: var(--color-accent-gold);
	}

	.cal-new {
		font-size: 8px;
		font-weight: 700;
		color: var(--color-accent-primary);
		letter-spacing: 0.5px;
	}

	/* Gameplay styles (reused pattern) */
	.gameplay {
		display: flex;
		flex-direction: column;
		height: calc(100dvh - var(--header-height) - var(--space-12));
		max-height: 900px;
		gap: var(--space-3);
	}

	.game-hud {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: var(--space-2) 0;
		flex-shrink: 0;
	}

	.hud-back {
		background: none;
		border: none;
		color: var(--color-text-muted);
		cursor: pointer;
		font-size: var(--text-sm);
	}

	.hud-label {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		max-width: min(42vw, 260px);
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.hud-stats {
		display: flex;
		gap: var(--space-3);
		font-size: var(--text-sm);
		flex-wrap: wrap;
		justify-content: flex-end;
	}

	.hud-stat {
		display: flex;
		align-items: baseline;
		gap: var(--space-1);
		font-family: var(--font-mono);
	}

	.hud-stat-target {
		color: var(--color-text-muted);
		font-size: var(--text-xs);
	}

	.frozen-badge {
		font-size: 10px;
		font-weight: 700;
		letter-spacing: 0.05em;
		color: #38bdf8;
		background: rgba(56, 189, 248, 0.12);
		border: 1px solid rgba(56, 189, 248, 0.4);
		border-radius: var(--radius-full);
		padding: 1px 6px;
	}

	.maze-area {
		flex: 1;
		min-height: 0;
		border-radius: var(--radius-lg);
		border: 1px solid var(--color-border);
		overflow: hidden;
		background: color-mix(in srgb, var(--color-accent-primary) 4%, var(--color-bg-card));
		box-shadow: var(--shadow-card), 0 0 30px color-mix(in srgb, var(--color-accent-primary) 12%, transparent);
	}

	.controls-bar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: var(--space-2) 0;
		flex-shrink: 0;
	}

	.powerups {
		display: flex;
		gap: var(--space-2);
		flex-wrap: wrap;
		max-width: 260px;
	}

	.powerup-btn {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		font-size: var(--text-sm);
		color: var(--color-text-primary);
		cursor: pointer;
	}

	.powerup-btn:disabled { opacity: 0.4; cursor: default; }

	.powerup-count {
		color: var(--color-text-muted);
		font-size: var(--text-xs);
	}

	.dpad { display: flex; flex-direction: column; align-items: center; gap: 2px; }
	.dpad-middle { display: flex; gap: 2px; }
	.dpad-btn {
		width: 44px; height: 44px; display: flex; align-items: center; justify-content: center;
		background: var(--color-bg-card); border: 1px solid var(--color-border);
		border-radius: var(--radius-md); color: var(--color-text-primary);
		cursor: pointer; user-select: none;
	}
	.dpad-btn:active { background: var(--color-accent-primary); color: white; }

</style>
