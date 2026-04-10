<script lang="ts">
	import { base } from '$app/paths';
	import { onDestroy, tick } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getDailyMazeForDate, getDailyMazesForMonth, getDailyMazeSeed } from '$lib/core/daily';
	import { createGameSession, getHint, calculateStars } from '$lib/core/session';
	import type { GameSessionState } from '$lib/core/session';
	import { canMove, applyMove } from '$lib/core/maze';
	import type { Direction, DailyMazeLevel } from '$lib/core/types';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import { getSkinById } from '$lib/core/skins';
	import MazeIntroOverlay from '$lib/components/MazeIntroOverlay.svelte';
	import MazeOutroOverlay from '$lib/components/MazeOutroOverlay.svelte';

	const today = new Date();
	const skinImageUrl = $derived(getSkinById(gameStore.player.currentSkinId)?.imageUrl ?? 'player_image0');
	let viewYear = $state(today.getFullYear());
	let viewMonth = $state(today.getMonth());
	let calendarDays = $derived(getDailyMazesForMonth(viewYear, viewMonth));

	let selectedDate = $state<string | null>(null);
	let selectedDaily = $state<DailyMazeLevel | null>(null);
	let session = $state<GameSessionState | null>(null);
	let elapsed = $state(0);
	let timerInterval: ReturnType<typeof setInterval>;
	let showIntro = $state(false);
	let showOutro = $state(false);
	let victoryStars = $state({ star1: false, star2: false, star3: false, total: 0 });
	let coinsEarned = $state(0);
	let visitedCells = $state(new Set<string>());
	let playing = $state(false);

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
	const canAcceptInput = $derived(playing && session !== null && !session.isComplete && !showIntro && !showOutro);

	const monthNames = [
		'January', 'February', 'March', 'April', 'May', 'June',
		'July', 'August', 'September', 'October', 'November', 'December'
	];

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
		showIntro = false;
		showOutro = false;
		elapsed = 0;
		coinsEarned = 0;
		victoryStars = { star1: false, star2: false, star3: false, total: 0 };

		const d = new Date(dateStr);
		const dailyLevel = getDailyMazeForDate(d);
		const seed = getDailyMazeSeed(d);

		session = createGameSession({
			width: dailyLevel.width,
			height: dailyLevel.height,
			algorithm: dailyLevel.levelType,
			seed,
			twoStarMoves: dailyLevel.movesNeeded,
			threeStarTime: dailyLevel.timeNeeded
		});
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
			if (session && !session.isComplete) elapsed += 0.01;
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
			hintPath: null
		};

		visitedCells = new Set([...visitedCells, `${newPos.x},${newPos.y}`]);
		if (isComplete) onComplete();
	}

	function onComplete() {
		clearInterval(timerInterval);
		if (!session || !selectedDate || !selectedDaily) return;

		const stars = calculateStars(
			session.moves,
			elapsed,
			session.hintsUsed,
			selectedDaily.movesNeeded,
			selectedDaily.timeNeeded,
			Math.floor(selectedDaily.movesNeeded * 0.6),
			Math.floor(selectedDaily.timeNeeded * 0.6)
		);
		victoryStars = stars;

		coinsEarned = 50 + stars.total * 25;
		gameStore.addCoins(coinsEarned);

		const result: DailyMazeLevel = {
			...selectedDaily,
			status: 'completed',
			completionTime: elapsed,
			completionMoves: session.moves
		};
		gameStore.saveDailyResult(result);

		showOutro = true;
	}

	function useHint() {
		if (!session || !canAcceptInput) return;
		if (gameStore.usePowerup('hint')) {
			const hintPath = getHint(session);
			session = { ...session, hintPath };
		}
	}

	function backToCalendar() {
		clearInterval(timerInterval);
		showIntro = false;
		showOutro = false;
		playing = false;
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

	onDestroy(() => clearInterval(timerInterval));
</script>

<svelte:window onkeydown={handleKeydown} />

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
		<h1 class="page-title">📅 Daily Maze</h1>
		<p class="page-subtitle">A new maze every day. Come back to complete your streak!</p>

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
			<span class="hud-label">📅 {selectedDate}</span>
			<div class="hud-stats">
				<span class="hud-stat">⏱️ {elapsed.toFixed(1)}s</span>
				<span class="hud-stat">👣 {session.moves}</span>
			</div>
		</div>

		<div class="maze-area">
			<MazeRenderer
				maze={session.maze}
				playerPos={session.playerPos}
				wallColor={gameStore.player.wallColor}
				hintPath={session.hintPath}
				showVisited={true}
				{visitedCells}
				{skinImageUrl}
			/>
		</div>

		<div class="controls-bar">
			<button class="powerup-btn" onclick={useHint} disabled={gameStore.player.hintsOwned <= 0}>
				💡 Hint ({gameStore.player.hintsOwned})
			</button>
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
		subtitle={selectedDate ?? ''}
		phrases={DAILY_PHRASES}
		accentColor="#34d399"
		ondismiss={startGameplay}
	/>
{/if}

{#if showOutro && session && selectedDaily}
	<MazeOutroOverlay
		title="Daily Maze Complete!"
		playerName={gameStore.player.playerName}
		time={elapsed}
		moves={session.moves}
		stars={victoryStars.total}
		twoStarMoves={selectedDaily.movesNeeded}
		threeStarTime={selectedDaily.timeNeeded}
		coins={coinsEarned}
		accentColor="#34d399"
		actions={[
			{ label: 'Retry', onclick: () => { showOutro = false; if (selectedDate) startDailyMaze(selectedDate); } },
			{ label: '← Calendar', primary: true, onclick: () => { showOutro = false; backToCalendar(); } }
		]}
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

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-1);
	}

	.page-subtitle {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
		margin-bottom: var(--space-6);
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
	}

	.hud-stats {
		display: flex;
		gap: var(--space-3);
		font-size: var(--text-sm);
	}

	.hud-stat {
		font-family: var(--font-mono);
	}

	.maze-area {
		flex: 1;
		min-height: 0;
		border-radius: var(--radius-lg);
		border: 1px solid var(--color-border);
		overflow: hidden;
	}

	.controls-bar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: var(--space-2) 0;
		flex-shrink: 0;
	}

	.powerup-btn {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		font-size: var(--text-sm);
		color: var(--color-text-primary);
		cursor: pointer;
	}

	.powerup-btn:disabled { opacity: 0.4; cursor: default; }

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
