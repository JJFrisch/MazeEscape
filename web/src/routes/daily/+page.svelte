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
		const selected = new Date(dateStr);
		const selectedDayStart = new Date(selected.getFullYear(), selected.getMonth(), selected.getDate());
		const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
		return selectedDayStart > todayStart;
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
	{@const todayFloors   = calendarDays.filter(d => isToday(d.date))}
	{@const pastFloors    = [...calendarDays].filter(d => !isToday(d.date) && !isFuture(d.date)).reverse()}
	{@const futureFloors  = calendarDays.filter(d => isFuture(d.date))}

	<div class="spire-page">

		<!-- ── Atmospheric glow ────────────────── -->
		<div class="spire-atmos" aria-hidden="true"></div>

		<!-- ── Spire header ───────────────────── -->
		<div class="spire-header">
			<div class="spire-emblem" aria-hidden="true">◉</div>
			<div class="spire-identity">
				<div class="spire-eyebrow">DAILY ASCENT</div>
				<h1 class="spire-name">The Spire</h1>
			</div>
			<div class="spire-streak-panel">
				<div class="streak-pip" class:streak-lit={(gameStore.player.currentStreak ?? 0) > 0}>
					<span class="streak-flame">{(gameStore.player.currentStreak ?? 0) > 0 ? '🔥' : '💤'}</span>
					<span class="streak-val">{gameStore.player.currentStreak ?? 0}</span>
					<span class="streak-lbl">streak</span>
				</div>
				<div class="streak-divider"></div>
				<div class="streak-pip streak-pip-dim">
					<span class="streak-val">{gameStore.player.bestStreak ?? 0}</span>
					<span class="streak-lbl">best</span>
				</div>
				{#if (gameStore.player.streakShieldsOwned ?? 0) > 0}
					<div class="shield-badge" title="Streak shields owned">🛡️ {gameStore.player.streakShieldsOwned}</div>
				{/if}
			</div>
		</div>

		<!-- ── Streak milestone ────────────────── -->
		{#if (gameStore.player.currentStreak ?? 0) > 0}
			{@const next = [3, 7, 14, 30, 60, 100].find(m => m > (gameStore.player.currentStreak ?? 0))}
			{#if next}
				<div class="milestone-bar">
					<div class="milestone-fill" style="width:{Math.min(((gameStore.player.currentStreak ?? 0) / next) * 100, 100)}%;"></div>
					<span class="milestone-label">🎯 {next - (gameStore.player.currentStreak ?? 0)} more day{next - (gameStore.player.currentStreak ?? 0) !== 1 ? 's' : ''} to {next}-day milestone</span>
				</div>
			{/if}
		{/if}

		<!-- ── Event panel ────────────────────── -->
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

		<!-- ── Month navigation ────────────────── -->
		<div class="spire-month-nav">
			<button class="month-nav-btn" onclick={prevMonth} aria-label="Previous month">
				<svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
					<path d="M10 3L5 8l5 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
				</svg>
			</button>
			<span class="month-name">{monthNames[viewMonth]} {viewYear}</span>
			<button class="month-nav-btn" onclick={nextMonth} aria-label="Next month">
				<svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
					<path d="M6 3l5 5-5 5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
				</svg>
			</button>
		</div>

		<!-- ── The Spire shaft ─────────────────── -->
		<div class="spire-shaft">
			<!-- Decorative spine line (only over today + past section) -->
			<div class="shaft-spine" aria-hidden="true"></div>

			<!-- TODAY'S FLOOR — featured at top -->
			{#each todayFloors as daily}
				<div class="floor-row floor-today">
					<div class="floor-marker" aria-hidden="true">
						<div class="floor-dot floor-dot-today">
							<div class="dot-inner-ring"></div>
						</div>
					</div>
					<button
						class="floor-card floor-card-today"
						onclick={() => startDailyMaze(daily.date)}
						aria-label="Today's floor — Ascend now"
					>
						<div class="floor-header-row">
							<span class="floor-num-badge">Floor {padDay(daily.date)}</span>
							<span class="floor-today-badge">TODAY</span>
						</div>
						<div class="floor-today-date">{formatDisplayDate(daily.date)}</div>
						<div class="floor-ascend-cta">
							<span>Ascend</span>
							<svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true">
								<path d="M8 12V4M4 8l4-4 4 4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
							</svg>
						</div>
					</button>
				</div>
			{/each}

			<!-- Separator between today and history -->
			{#if todayFloors.length > 0 && pastFloors.length > 0}
				<div class="shaft-section-label" aria-hidden="true">
					<span>— Floors Climbed —</span>
				</div>
			{/if}

			<!-- PAST FLOORS (reverse chrono, newest first) -->
			{#each pastFloors as daily}
				{@const result = getDayResult(daily.shortDate)}
				{@const isCompleted = result?.status === 'completed' || result?.status === 'completed_late'}
				<div class="floor-row" class:floor-completed={isCompleted} class:floor-missed={!isCompleted}>
					<div class="floor-marker" aria-hidden="true">
						<div class="floor-dot" class:floor-dot-completed={isCompleted} class:floor-dot-missed={!isCompleted}>
							{#if isCompleted}
								<svg width="7" height="7" viewBox="0 0 10 10" fill="none" aria-hidden="true">
									<path d="M1.5 5l2.5 2.5 4.5-4.5" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/>
								</svg>
							{/if}
						</div>
					</div>
					<button
						class="floor-card"
						onclick={() => startDailyMaze(daily.date)}
						aria-label="Floor {padDay(daily.date)} — {isCompleted ? 'Completed' : 'Missed — click to retry'}"
					>
						<div class="floor-left">
							<span class="floor-num">Floor {padDay(daily.date)}</span>
							<span class="floor-date">{formatDisplayDate(daily.date)}</span>
						</div>
						<div class="floor-right">
							{#if isCompleted}
								<span class="floor-check-label">✓</span>
								<span class="floor-stats-text">
									{result?.completionTime?.toFixed(1)}s &middot; {result?.completionMoves} mv
								</span>
							{:else}
								<span class="floor-retry-label">Retry ↑</span>
							{/if}
						</div>
					</button>
				</div>
			{/each}

			<!-- FUTURE FLOORS — upcoming, grayed out -->
			{#if futureFloors.length > 0}
				<div class="shaft-section-label shaft-section-future" aria-hidden="true">
					<span>— Upcoming Floors —</span>
				</div>
				{#each futureFloors as daily}
					<div class="floor-row floor-future">
						<div class="floor-marker" aria-hidden="true">
							<div class="floor-dot floor-dot-future"></div>
						</div>
						<div class="floor-card floor-card-future" aria-hidden="true">
							<div class="floor-left">
								<span class="floor-num">Floor {padDay(daily.date)}</span>
								<span class="floor-date">{formatDisplayDate(daily.date)}</span>
							</div>
							<div class="floor-right">
								<span class="floor-locked-label">🔒</span>
							</div>
						</div>
					</div>
				{/each}
			{/if}

		</div><!-- /.spire-shaft -->
	</div><!-- /.spire-page -->
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
	/* ── Spire page ─────────────────────────────── */
	.spire-page {
		position: relative;
		max-width: 580px;
		margin: 0 auto;
		padding: var(--space-4) var(--space-4) var(--space-16);
	}

	/* Atmospheric top glow (purple, The Spire color) */
	.spire-atmos {
		position: fixed;
		top: var(--header-height);
		left: 50%;
		transform: translateX(-50%);
		width: 700px;
		height: 340px;
		background: radial-gradient(ellipse at 50% 0%, rgba(192,132,252,0.09) 0%, transparent 65%);
		pointer-events: none;
		z-index: 0;
	}

	/* ── Spire header ────────────────────────────── */
	.spire-header {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-4) 0 var(--space-5);
		border-bottom: 1px solid rgba(192,132,252,0.15);
		margin-bottom: var(--space-4);
	}

	.spire-emblem {
		font-size: 1.6rem;
		color: #c084fc;
		line-height: 1;
		filter: drop-shadow(0 0 10px rgba(192,132,252,0.6));
		animation: emblem-pulse 3s ease-in-out infinite;
	}
	@keyframes emblem-pulse {
		0%, 100% { filter: drop-shadow(0 0 8px rgba(192,132,252,0.5)); }
		50%       { filter: drop-shadow(0 0 18px rgba(192,132,252,0.9)); }
	}

	.spire-identity { flex: 1; }
	.spire-eyebrow {
		font-size: 10px;
		font-weight: 700;
		letter-spacing: 0.18em;
		text-transform: uppercase;
		color: rgba(192,132,252,0.65);
		margin-bottom: 2px;
	}
	.spire-name {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.01em;
	}

	/* Streak panel in header */
	.spire-streak-panel {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-2) var(--space-3);
		background: rgba(192,132,252,0.06);
		border: 1px solid rgba(192,132,252,0.15);
		border-radius: var(--radius-xl);
		flex-shrink: 0;
	}
	.streak-pip {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2px;
	}
	.streak-pip-dim { opacity: 0.65; }
	.streak-flame { font-size: 1rem; line-height: 1; }
	.streak-val {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		font-weight: 700;
		color: var(--color-text-primary);
		line-height: 1;
	}
	.streak-pip.streak-lit .streak-val { color: #f97316; }
	.streak-lbl {
		font-size: 9px;
		font-weight: 700;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}
	.streak-divider { width: 1px; height: 28px; background: rgba(255,255,255,0.08); }
	.shield-badge {
		font-size: var(--text-xs);
		font-weight: 700;
		padding: 3px 7px;
		background: rgba(148,163,184,0.10);
		border: 1px solid rgba(148,163,184,0.22);
		border-radius: var(--radius-full);
		color: #94a3b8;
	}

	/* ── Milestone bar ───────────────────────────── */
	.milestone-bar {
		position: relative;
		z-index: 1;
		height: 26px;
		background: rgba(249,115,22,0.05);
		border: 1px solid rgba(249,115,22,0.18);
		border-radius: var(--radius-full);
		margin-bottom: var(--space-4);
		overflow: hidden;
	}
	.milestone-fill {
		position: absolute;
		top: 0; left: 0; bottom: 0;
		background: linear-gradient(90deg, rgba(249,115,22,0.22), rgba(249,115,22,0.10));
		border-radius: inherit;
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

	/* ── Month navigation ────────────────────────── */
	.spire-month-nav {
		position: relative;
		z-index: 1;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: var(--space-2) 0 var(--space-4);
	}
	.month-nav-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 32px;
		height: 32px;
		background: rgba(192,132,252,0.07);
		border: 1px solid rgba(192,132,252,0.18);
		border-radius: var(--radius-md);
		color: #c084fc;
		cursor: pointer;
		transition: all 0.15s ease;
	}
	.month-nav-btn:hover {
		background: rgba(192,132,252,0.14);
		border-color: rgba(192,132,252,0.4);
	}
	.month-name {
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 600;
		color: var(--color-text-primary);
	}

	/* ── Spire shaft ─────────────────────────────── */
	.spire-shaft {
		position: relative;
		z-index: 1;
		display: flex;
		flex-direction: column;
	}

	/* The continuous vertical spine */
	.shaft-spine {
		position: absolute;
		left: 15px;
		top: 0;
		bottom: 0;
		width: 2px;
		background: linear-gradient(180deg,
			rgba(192,132,252,0.7) 0%,
			rgba(192,132,252,0.35) 60%,
			rgba(192,132,252,0.08) 100%
		);
		border-radius: 1px;
	}

	/* Section divider labels */
	.shaft-section-label {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-3) 0 var(--space-2) 44px;
		font-size: 10px;
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: rgba(192,132,252,0.45);
	}
	.shaft-section-future {
		color: var(--color-text-muted);
		opacity: 0.5;
	}

	/* ── Floor rows ──────────────────────────────── */
	.floor-row {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		padding: 3px 0;
	}

	/* Spine marker column */
	.floor-marker {
		position: relative;
		z-index: 2;
		width: 32px;
		height: 32px;
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	/* Base dot */
	.floor-dot {
		width: 12px;
		height: 12px;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(192,132,252,0.18);
		border: 1.5px solid rgba(192,132,252,0.3);
		color: transparent;
		transition: all 0.2s ease;
	}

	/* Today — large pulsing dot */
	.floor-dot-today {
		width: 18px;
		height: 18px;
		background: #c084fc;
		border-color: #c084fc;
		box-shadow: 0 0 14px rgba(192,132,252,0.7), 0 0 28px rgba(192,132,252,0.3);
		animation: spine-pulse 2s ease-in-out infinite;
	}
	@keyframes spine-pulse {
		0%, 100% { box-shadow: 0 0 10px rgba(192,132,252,0.6), 0 0 22px rgba(192,132,252,0.25); }
		50%       { box-shadow: 0 0 20px rgba(192,132,252,0.9), 0 0 40px rgba(192,132,252,0.45); }
	}
	.dot-inner-ring {
		width: 7px; height: 7px;
		border-radius: 50%;
		border: 1.5px solid rgba(255,255,255,0.7);
	}

	/* Completed dot */
	.floor-dot-completed {
		background: rgba(52,211,153,0.22);
		border-color: rgba(52,211,153,0.55);
		color: #34d399;
	}

	/* Missed dot */
	.floor-dot-missed {
		background: rgba(244,63,94,0.1);
		border-color: rgba(244,63,94,0.25);
	}

	/* Future dot */
	.floor-dot-future {
		width: 8px; height: 8px;
		background: rgba(255,255,255,0.06);
		border-color: rgba(255,255,255,0.1);
	}

	/* ── Floor cards ──────────────────────────────── */
	.floor-card {
		flex: 1;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-3);
		padding: 10px 14px;
		background: rgba(8,12,28,0.8);
		border: 1px solid rgba(255,255,255,0.05);
		border-radius: var(--radius-lg);
		cursor: pointer;
		text-align: left;
		color: var(--color-text-primary);
		transition: border-color 0.18s ease, background 0.18s ease, box-shadow 0.18s ease;
		min-height: 48px;
	}
	.floor-row.floor-completed .floor-card:hover {
		border-color: rgba(52,211,153,0.3);
		background: rgba(52,211,153,0.04);
	}
	.floor-row.floor-missed .floor-card:hover {
		border-color: rgba(192,132,252,0.25);
		background: rgba(192,132,252,0.04);
	}

	/* TODAY featured card */
	.floor-card-today {
		flex-direction: column;
		align-items: flex-start;
		padding: 16px 18px;
		background: rgba(192,132,252,0.07);
		border-color: rgba(192,132,252,0.3);
		box-shadow: 0 0 24px rgba(192,132,252,0.1), inset 0 0 32px rgba(192,132,252,0.04);
		min-height: 0;
		gap: 6px;
	}
	.floor-card-today:hover {
		border-color: rgba(192,132,252,0.55);
		background: rgba(192,132,252,0.11);
		box-shadow: 0 0 36px rgba(192,132,252,0.18), inset 0 0 40px rgba(192,132,252,0.06);
	}

	/* Future card (non-interactive) */
	.floor-card-future {
		opacity: 0.35;
		cursor: default;
		pointer-events: none;
	}

	/* Today card rows */
	.floor-header-row {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		width: 100%;
	}
	.floor-num-badge {
		font-family: var(--font-display);
		font-size: var(--text-sm);
		font-weight: 700;
		color: #c084fc;
	}
	.floor-today-badge {
		font-size: 9px;
		font-weight: 700;
		letter-spacing: 0.12em;
		padding: 2px 7px;
		background: rgba(192,132,252,0.18);
		border: 1px solid rgba(192,132,252,0.35);
		border-radius: var(--radius-full);
		color: #c084fc;
	}
	.floor-today-date {
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
		margin-bottom: 4px;
	}
	.floor-ascend-cta {
		display: inline-flex;
		align-items: center;
		gap: 5px;
		padding: 6px 14px;
		background: rgba(192,132,252,0.18);
		border: 1px solid rgba(192,132,252,0.4);
		border-radius: var(--radius-full);
		font-size: var(--text-sm);
		font-weight: 700;
		color: #c084fc;
		font-family: var(--font-display);
		letter-spacing: 0.02em;
		transition: all 0.18s ease;
	}
	.floor-card-today:hover .floor-ascend-cta {
		background: rgba(192,132,252,0.28);
		border-color: rgba(192,132,252,0.65);
		box-shadow: 0 0 16px rgba(192,132,252,0.3);
	}

	/* Past floor card internals */
	.floor-left {
		display: flex;
		flex-direction: column;
		gap: 2px;
		min-width: 0;
	}
	.floor-num {
		font-family: var(--font-display);
		font-size: var(--text-sm);
		font-weight: 600;
		color: var(--color-text-primary);
	}
	.floor-date {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
	}
	.floor-right {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		flex-shrink: 0;
	}
	.floor-check-label {
		font-size: var(--text-sm);
		color: #34d399;
		font-weight: 700;
	}
	.floor-stats-text {
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
		font-family: var(--font-mono);
	}
	.floor-retry-label {
		font-size: var(--text-xs);
		font-weight: 600;
		color: rgba(192,132,252,0.55);
		letter-spacing: 0.04em;
	}
	.floor-locked-label {
		font-size: var(--text-sm);
		opacity: 0.5;
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

	/* Gameplay styles */
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
