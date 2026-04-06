<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { onDestroy, untrack } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { createGameSession, tryMove, getHint, calculateStars } from '$lib/core/session';
	import type { GameSessionState, GameSessionConfig } from '$lib/core/session';
	import type { Direction } from '$lib/core/types';
	import { getWorldTheme } from '$lib/worldThemes';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import MazeIntroOverlay from '$lib/components/MazeIntroOverlay.svelte';
	import MazeOutroOverlay from '$lib/components/MazeOutroOverlay.svelte';

	// Parse route: "worldId-levelNumber" e.g. "1-5" or "2-3b"
	const levelKey = $derived($page.params.levelId as string);
	const worldId = $derived(Number((levelKey ?? '').split('-')[0]));
	const levelNumber = $derived((levelKey ?? '').split('-').slice(1).join('-'));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levelDef = $derived(worldDef ? getLevelByNumber(worldDef, levelNumber) : undefined);
	const theme = $derived(getWorldTheme(worldId));

	// Per-world intro phrases keyed by motif
	const INTRO_PHRASES: Record<'tech' | 'space' | 'elemental', string[]> = {
		tech: [
			'Routing neural pathways…',
			'Compiling the grid…',
			'Encrypting dead ends…',
			'Charging exit nodes…',
			'Calibrating the labyrinth…'
		],
		space: [
			'Mapping star clusters…',
			'Plotting warp routes…',
			'Anchoring gravity wells…',
			'Charting the nebula…',
			'Scanning exit vectors…'
		],
		elemental: [
			'Weaving ancient paths…',
			'Placing stepping stones…',
			'Summoning the maze spirit…',
			'Listening to the wind…',
			'The way reveals itself…'
		]
	};

	const introPhrases = $derived(INTRO_PHRASES[theme.motif]);
	const ENABLE_LEVEL_INTRO = false; // temporary fallback while debugging intro/browser behavior

	let session = $state<GameSessionState | null>(null);
	let elapsed = $state(0);
	let timerInterval: ReturnType<typeof setInterval> | undefined;
	let showIntro = $state(false);
	let showOutro = $state(false);
	let loadError = $state('');
	let initializedLevelKey = $state('');
	let victoryStars = $state({ star1: false, star2: false, star3: false, total: 0 });
	let coinsEarned = $state(0);
	let visitedCells = $state(new Set<string>());
	let moveQueue: Direction[] = [];
	const MOVE_QUEUE_MAX = 2;

	function startGame() {
		if (!levelDef) {
			loadError = `Level ${levelNumber} could not be loaded.`;
			session = null;
			showIntro = false;
			clearInterval(timerInterval);
			return;
		}

		try {
			const seed = worldId * 10000 + (parseInt(levelDef.levelNumber) || 0) * 100 +
				(levelDef.levelNumber.includes('b') ? 50 : 0);

			const config: GameSessionConfig = {
				width: levelDef.width,
				height: levelDef.height,
				algorithm: levelDef.levelType,
				seed,
				twoStarMoves: levelDef.twoStarMoves,
				threeStarTime: levelDef.threeStarTime
			};

			session = createGameSession(config);
			loadError = '';
			elapsed = 0;
			showOutro = false;
			visitedCells = new Set([`${session.maze.start.x},${session.maze.start.y}`]);
			moveQueue = [];
			clearInterval(timerInterval);
			showIntro = ENABLE_LEVEL_INTRO;

			if (!ENABLE_LEVEL_INTRO) {
				startGameplay();
			}
		} catch (error) {
			console.error('Failed to start level', { levelKey, error });
			loadError = 'This level failed to initialize.';
			session = null;
			showIntro = false;
			clearInterval(timerInterval);
		}
	}

	function startGameplay() {
		if (!session) return;
		showIntro = false;
		clearInterval(timerInterval);
		timerInterval = setInterval(() => {
			if (session && !session.isComplete) {
				elapsed += 0.01;
			}
		}, 10);
	}

	function handleMove(direction: Direction) {
		if (!session || session.isComplete || showIntro) return;

		const result = tryMove(session, direction);
		if (result.moved) {
			visitedCells = new Set([...visitedCells, `${result.x},${result.y}`]);

			// Trigger reactivity
			session = session;

			if (session.isComplete) {
				onLevelComplete();
			}
		}
	}

	function processQueue() {
		if (moveQueue.length > 0 && session && !session.isComplete) {
			const dir = moveQueue.shift()!;
			handleMove(dir);
		}
	}

	function queueMove(direction: Direction) {
		if (showIntro) return;
		if (moveQueue.length < MOVE_QUEUE_MAX) {
			moveQueue.push(direction);
		}
		processQueue();
	}

	function onLevelComplete() {
		clearInterval(timerInterval);

		if (!levelDef || !session) return;

		const stars = calculateStars(
			session.moves,
			elapsed,
			levelDef.twoStarMoves,
			levelDef.threeStarTime
		);
		victoryStars = stars;

		// Coins: base + bonus for stars
		const baseCoins = Math.floor(Math.random() * 100) + 50;
		coinsEarned = baseCoins + stars.total * 25;
		gameStore.addCoins(coinsEarned);

		// Save progress
		const updatedLevel = {
			...levelDef,
			completed: true,
			star1: stars.star1,
			star2: stars.star2,
			star3: stars.star3,
			numberOfStars: stars.total,
			bestMoves: session.moves,
			bestTime: elapsed
		};
		gameStore.saveLevelProgress(worldId, updatedLevel);

		showOutro = true;
	}

	function useHint() {
		if (!session || session.isComplete || showIntro) return;
		if (gameStore.usePowerup('hint')) {
			getHint(session);
			session = session; // trigger reactivity
		}
	}

	function handleKeydown(e: KeyboardEvent) {
		if (showIntro || showOutro) return;
		const dirMap: Record<string, Direction> = {
			ArrowUp: 'up',
			ArrowDown: 'down',
			ArrowLeft: 'left',
			ArrowRight: 'right',
			w: 'up',
			s: 'down',
			a: 'left',
			d: 'right'
		};
		const dir = dirMap[e.key];
		if (dir) {
			e.preventDefault();
			queueMove(dir);
		}
	}

	// Swipe support
	let touchStartX = 0;
	let touchStartY = 0;

	function handleTouchStart(e: TouchEvent) {
		touchStartX = e.touches[0].clientX;
		touchStartY = e.touches[0].clientY;
	}

	function handleTouchEnd(e: TouchEvent) {
		if (showIntro) return;
		const dx = e.changedTouches[0].clientX - touchStartX;
		const dy = e.changedTouches[0].clientY - touchStartY;
		const threshold = 30;

		if (Math.abs(dx) > Math.abs(dy)) {
			if (Math.abs(dx) > threshold) {
				queueMove(dx > 0 ? 'right' : 'left');
			}
		} else {
			if (Math.abs(dy) > threshold) {
				queueMove(dy > 0 ? 'down' : 'up');
			}
		}
	}

	function goToNextLevel() {
		if (!levelDef || !worldDef) return;
		const nextNum = levelDef.connectTo1;
		if (nextNum) {
			goto(`${base}/campaign/play/${worldId}-${nextNum}`);
		} else {
			goto(`${base}/campaign/worlds/${worldId}`);
		}
	}

	onDestroy(() => {
		clearInterval(timerInterval);
	});

	// Restart game when route changes (untrack so startGame's reactive reads
	// don't become additional effect dependencies)
	$effect(() => {
		if (!levelKey || initializedLevelKey === levelKey) return;
		initializedLevelKey = levelKey;
		untrack(() => startGame());
	});
</script>

<svelte:window onkeydown={handleKeydown} />

<svelte:head>
	<title>Level {levelNumber} – MazeEscape</title>
</svelte:head>

{#if session && levelDef}
	<div
		class="gameplay"
		ontouchstart={handleTouchStart}
		ontouchend={handleTouchEnd}
		role="application"
		aria-label="Maze gameplay"
	>
		<!-- Top bar: back, timer, moves -->
		<div class="game-hud">
			<a href="{base}/campaign/worlds/{worldId}" class="hud-back">← Back</a>

			<div class="hud-center">
				<span class="hud-label">Level {levelNumber}</span>
				{#if levelDef.levelNumber.includes('b')}
					<span class="bonus-badge">Bonus</span>
				{/if}
			</div>

			<div class="hud-stats">
				<div class="hud-stat">
					<span class="hud-stat-label">⏱️</span>
					<span class="hud-stat-value" class:over-threshold={elapsed > levelDef.threeStarTime}>
						{elapsed.toFixed(1)}s
					</span>
					<span class="hud-stat-target">/ {levelDef.threeStarTime}s</span>
				</div>
				<div class="hud-stat">
					<span class="hud-stat-label">👣</span>
					<span class="hud-stat-value" class:over-threshold={session.moves > levelDef.twoStarMoves}>
						{session.moves}
					</span>
					<span class="hud-stat-target">/ {levelDef.twoStarMoves}</span>
				</div>
			</div>
		</div>

		<!-- Maze -->
		<div class="maze-area">
			<MazeRenderer
				maze={session.maze}
				playerPos={session.playerPos}
				wallColor={gameStore.player.wallColor}
				hintPath={session.hintPath}
				showVisited={true}
				{visitedCells}
			/>
		</div>

		<!-- Powerups & Controls -->
		<div class="controls-bar">
			<div class="powerups">
				<button class="powerup-btn" onclick={useHint} disabled={gameStore.player.hintsOwned <= 0}>
					💡 Hint ({gameStore.player.hintsOwned})
				</button>
			</div>
			<div class="dpad" role="group" aria-label="Movement controls">
				<button class="dpad-btn dpad-up" onclick={() => queueMove('up')} aria-label="Move up">▲</button>
				<div class="dpad-middle">
					<button class="dpad-btn dpad-left" onclick={() => queueMove('left')} aria-label="Move left">◀</button>
					<button class="dpad-btn dpad-right" onclick={() => queueMove('right')} aria-label="Move right">▶</button>
				</div>
				<button class="dpad-btn dpad-down" onclick={() => queueMove('down')} aria-label="Move down">▼</button>
			</div>
		</div>
	</div>


{:else if loadError}
	<div class="loading">
		<p>{loadError}</p>
		<a href="{base}/campaign/worlds">← Back to worlds</a>
	</div>

{:else}
	<div class="loading">
		<p>Loading level...</p>
		<a href="{base}/campaign/worlds">← Back to worlds</a>
	</div>
{/if}

<!-- Overlays live outside the session block so their lifecycle is solely
     controlled by showIntro/showOutro, not by session reassignments -->
{#if showIntro}
	<MazeIntroOverlay
		title="Level {levelNumber}"
		subtitle={worldDef?.worldName ?? ''}
		phrases={introPhrases}
		accentColor={theme.accentColor}
		ondismiss={startGameplay}
	/>
{/if}

{#if showOutro && session && levelDef}
	<MazeOutroOverlay
		title="Level Complete!"
		playerName={gameStore.player.playerName}
		time={elapsed}
		moves={session.moves}
		stars={victoryStars.total}
		twoStarMoves={levelDef.twoStarMoves}
		threeStarTime={levelDef.threeStarTime}
		coins={coinsEarned}
		accentColor={theme.accentColor}
		actions={[
			{ label: 'Retry', onclick: () => { showOutro = false; startGame(); } },
			{ label: levelDef.connectTo1 ? 'Next Level →' : 'Back to World', primary: true, onclick: goToNextLevel }
		]}
	/>
{/if}

<style>
	.gameplay {
		display: flex;
		flex-direction: column;
		height: calc(100dvh - var(--header-height) - var(--space-12));
		max-height: 900px;
		gap: var(--space-3);
	}

	/* HUD */
	.game-hud {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-4);
		padding: var(--space-2) 0;
		flex-shrink: 0;
	}

	.hud-back {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
		text-decoration: none;
	}

	.hud-back:hover {
		color: var(--color-accent-secondary);
	}

	.hud-center {
		display: flex;
		align-items: center;
		gap: var(--space-2);
	}

	.hud-label {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
	}

	.bonus-badge {
		font-size: var(--text-xs);
		background: var(--color-accent-gold);
		color: #000;
		padding: 1px 6px;
		border-radius: var(--radius-full);
		font-weight: 600;
	}

	.hud-stats {
		display: flex;
		gap: var(--space-4);
	}

	.hud-stat {
		display: flex;
		align-items: baseline;
		gap: var(--space-1);
		font-size: var(--text-sm);
	}

	.hud-stat-value {
		font-weight: 700;
		font-family: var(--font-mono);
		color: var(--color-accent-green);
	}

	.hud-stat-value.over-threshold {
		color: var(--color-accent-red);
	}

	.hud-stat-target {
		color: var(--color-text-muted);
		font-size: var(--text-xs);
	}

	/* Maze */
	.maze-area {
		flex: 1;
		min-height: 0;
		border-radius: var(--radius-lg);
		border: 1px solid var(--color-border);
		overflow: hidden;
	}

	/* Controls */
	.controls-bar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-4);
		padding: var(--space-2) 0;
		flex-shrink: 0;
	}

	.powerups {
		display: flex;
		gap: var(--space-2);
	}

	.powerup-btn {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		font-size: var(--text-sm);
		color: var(--color-text-primary);
		cursor: pointer;
		transition: all var(--transition-fast);
	}

	.powerup-btn:not(:disabled):hover {
		border-color: var(--color-accent-primary);
		background: var(--color-bg-card-hover);
	}

	.powerup-btn:disabled {
		opacity: 0.4;
		cursor: default;
	}

	.dpad {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 2px;
	}

	.dpad-middle {
		display: flex;
		gap: 2px;
	}

	.dpad-btn {
		width: 44px;
		height: 44px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		color: var(--color-text-primary);
		font-size: var(--text-base);
		cursor: pointer;
		transition: all var(--transition-fast);
		user-select: none;
		-webkit-user-select: none;
	}

	.dpad-btn:hover {
		background: var(--color-bg-card-hover);
		border-color: var(--color-accent-primary);
	}

	.dpad-btn:active {
		background: var(--color-accent-primary);
		color: white;
	}

	.loading {
		text-align: center;
		padding: var(--space-16);
		color: var(--color-text-muted);
	}

	@media (max-width: 480px) {
		.hud-stats {
			flex-direction: column;
			gap: var(--space-1);
			align-items: flex-end;
		}
		.dpad-btn {
			width: 52px;
			height: 52px;
			font-size: var(--text-lg);
		}
	}
</style>
