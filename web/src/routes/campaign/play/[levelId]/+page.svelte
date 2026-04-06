<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { onDestroy, tick, untrack } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { createGameSession, getHint, calculateStars } from '$lib/core/session';
	import type { GameSessionState, GameSessionConfig } from '$lib/core/session';
	import { canMove, applyMove } from '$lib/core/maze';
	import type { Direction, HexMazeData, CircularMazeData, TriMazeData } from '$lib/core/types';
	import { generateHexMaze, canMoveHex, applyMoveHex, keyToHexDir } from '$lib/core/hexMaze';
	import { generateCircularMaze, canMoveCircular, keyToCircularDir } from '$lib/core/circularMaze';
	import { generateTriMaze, canMoveTri, applyMoveTri, keyToTriDir } from '$lib/core/triMaze';
	import { getWorldTheme } from '$lib/worldThemes';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import HexMazeRenderer from '$lib/components/HexMazeRenderer.svelte';
	import CircularMazeRenderer from '$lib/components/CircularMazeRenderer.svelte';
	import TriMazeRenderer from '$lib/components/TriMazeRenderer.svelte';
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
			'Weaving ancient paths… ',
			'Placing stepping stones…',
			'Summoning the maze spirit…',
			'Listening to the wind…',
			'The way reveals itself…'
		]
	};

	const introPhrases = $derived(INTRO_PHRASES[theme.motif]);
	const ENABLE_LEVEL_INTRO = true;

	let session = $state<GameSessionState | null>(null);
	// Non-rectangular maze state
	let hexState = $state<{ maze: HexMazeData; pos: { col: number; row: number }; moves: number; isComplete: boolean } | null>(null);
	let circState = $state<{ maze: CircularMazeData; pos: { ring: number; sector: number }; moves: number; isComplete: boolean } | null>(null);
	let triState = $state<{ maze: TriMazeData; pos: { col: number; row: number }; moves: number; isComplete: boolean } | null>(null);
	const activeShape = $derived(levelDef?.mazeShape ?? 'rectangular' as const);
	const currentMoves = $derived(
		activeShape === 'hexagonal' ? (hexState?.moves ?? 0) :
		activeShape === 'circular' ? (circState?.moves ?? 0) :
		activeShape === 'triangular' ? (triState?.moves ?? 0) :
		(session?.moves ?? 0)
	);
	const currentIsComplete = $derived(
		activeShape === 'hexagonal' ? (hexState?.isComplete ?? false) :
		activeShape === 'circular' ? (circState?.isComplete ?? false) :
		activeShape === 'triangular' ? (triState?.isComplete ?? false) :
		(session?.isComplete ?? false)
	);
	const gameIsActive = $derived(
		activeShape === 'hexagonal' ? hexState !== null :
		activeShape === 'circular' ? circState !== null :
		activeShape === 'triangular' ? triState !== null :
		session !== null
	);
	let elapsed = $state(0);
	let timerInterval: ReturnType<typeof setInterval> | undefined;
	let showIntro = $state(false);
	let showOutro = $state(false);
	const canAcceptInput = $derived(gameIsActive && !currentIsComplete && !showIntro && !showOutro);
	let loadError = $state('');
	let initializedLevelKey = $state('');
	let victoryStars = $state({ star1: false, star2: false, star3: false, total: 0 });
	let coinsEarned = $state(0);
	let visitedCells = $state(new Set<string>());

	function hasActiveGameState() {
		return session !== null || hexState !== null || circState !== null || triState !== null;
	}

	async function startGame() {
		if (!levelDef) {
			loadError = `Level ${levelNumber} could not be loaded.`;
			session = null;
			hexState = null;
			circState = null;
			triState = null;
			showIntro = false;
			clearInterval(timerInterval);
			return;
		}

		try {
			clearInterval(timerInterval);
			showIntro = false;
			showOutro = false;
			elapsed = 0;
			coinsEarned = 0;
			victoryStars = { star1: false, star2: false, star3: false, total: 0 };
			loadError = '';

			const seed = worldId * 10000 + (parseInt(levelDef.levelNumber) || 0) * 100 +
				(levelDef.levelNumber.includes('b') ? 50 : 0);
			const shape = levelDef.mazeShape ?? 'rectangular';

			// Reset all shape states
			session = null;
			hexState = null;
			circState = null;
			triState = null;

			if (shape === 'hexagonal') {
				const maze = generateHexMaze(levelDef.width, levelDef.height, seed);
				hexState = { maze, pos: { col: maze.start.col, row: maze.start.row }, moves: 0, isComplete: false };
				visitedCells = new Set([`${maze.start.col},${maze.start.row}`]);
			} else if (shape === 'circular') {
				const maze = generateCircularMaze(levelDef.width, seed); // width = numRings
				circState = { maze, pos: { ring: maze.start.ring, sector: maze.start.sector }, moves: 0, isComplete: false };
				visitedCells = new Set([`${maze.start.ring},${maze.start.sector}`]);
			} else if (shape === 'triangular') {
				const maze = generateTriMaze(levelDef.width, levelDef.height, seed);
				triState = { maze, pos: { col: maze.start.col, row: maze.start.row }, moves: 0, isComplete: false };
				visitedCells = new Set([`${maze.start.col},${maze.start.row}`]);
			} else {
				const config: GameSessionConfig = {
					width: levelDef.width,
					height: levelDef.height,
					algorithm: levelDef.levelType,
					seed,
					twoStarMoves: levelDef.twoStarMoves,
					threeStarTime: levelDef.threeStarTime
				};
				session = createGameSession(config);
				visitedCells = new Set([`${session.maze.start.x},${session.maze.start.y}`]);
			}

			await tick();

			showIntro = ENABLE_LEVEL_INTRO;

			if (!ENABLE_LEVEL_INTRO) {
				startGameplay();
			}
		} catch (error) {
			console.error('Failed to start level', { levelKey, error });
			loadError = 'This level failed to initialize.';
			session = null;
			hexState = null;
			circState = null;
			triState = null;
			showIntro = false;
			clearInterval(timerInterval);
		}
	}

	function startGameplay() {
		if (!hasActiveGameState()) return;
		showIntro = false;
		showOutro = false;
		clearInterval(timerInterval);
		timerInterval = setInterval(() => {
			if (hasActiveGameState() && !currentIsComplete) {
				elapsed += 0.01;
			}
		}, 10);
	}

	function handleMove(direction: Direction) {
		if (!canAcceptInput) return;

		const shape = activeShape;

		if (shape === 'hexagonal' && hexState) {
			const { maze, pos, moves } = hexState;
			const dirs = keyToHexDir(direction);
			for (const hdir of dirs) {
				if (canMoveHex(maze.cells, pos.col, pos.row, hdir)) {
					const newPos = applyMoveHex(pos.col, pos.row, hdir);
					const done = newPos.col === maze.end.col && newPos.row === maze.end.row;
					hexState = { maze, pos: newPos, moves: moves + 1, isComplete: done };
					visitedCells = new Set([...visitedCells, `${newPos.col},${newPos.row}`]);
					if (done) onLevelComplete();
					return;
				}
			}
		} else if (shape === 'circular' && circState) {
			const { maze, pos, moves } = circState;
			const dirs = keyToCircularDir(pos.sector, maze.rings[pos.ring]?.length ?? 1, direction);
			for (const cdir of dirs) {
				const result = canMoveCircular(maze, pos.ring, pos.sector, cdir);
				if (result.canMove) {
					const newPos = { ring: result.ring, sector: result.sector };
					const done = newPos.ring === maze.end.ring && newPos.sector === maze.end.sector;
					circState = { maze, pos: newPos, moves: moves + 1, isComplete: done };
					visitedCells = new Set([...visitedCells, `${newPos.ring},${newPos.sector}`]);
					if (done) onLevelComplete();
					return;
				}
			}
		} else if (shape === 'triangular' && triState) {
			const { maze, pos, moves } = triState;
			const pointsUp = (pos.col + pos.row) % 2 === 0;
			const dirs = keyToTriDir(pointsUp, direction);
			for (const tdir of dirs) {
				if (canMoveTri(maze.cells, pos.col, pos.row, tdir, maze.cols, maze.rows)) {
					const newPos = applyMoveTri(pos.col, pos.row, pointsUp, tdir);
					const done = newPos.col === maze.end.col && newPos.row === maze.end.row;
					triState = { maze, pos: newPos, moves: moves + 1, isComplete: done };
					visitedCells = new Set([...visitedCells, `${newPos.col},${newPos.row}`]);
					if (done) onLevelComplete();
					return;
				}
			}
		} else if (session) {
			// Rectangular (default)
			const { maze, playerPos, moves } = session;
			if (!canMove(maze.cells, playerPos, direction, maze.width, maze.height)) return;
			const newPos = applyMove(playerPos, direction);
			const isComplete = newPos.x === maze.end.x && newPos.y === maze.end.y;
			session = { maze, playerPos: newPos, moves: moves + 1, elapsed: session.elapsed, isComplete, hintPath: null };
			visitedCells = new Set([...visitedCells, `${newPos.x},${newPos.y}`]);
			if (isComplete) onLevelComplete();
		}
	}

	function dispatchMove(direction: Direction) {
		handleMove(direction);
	}

	function onLevelComplete() {
		clearInterval(timerInterval);

		if (!levelDef || !gameIsActive) return;

		const stars = calculateStars(
			currentMoves,
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
			bestMoves: currentMoves,
			bestTime: elapsed
		};
		gameStore.saveLevelProgress(worldId, updatedLevel);

		showOutro = true;
	}

	function useHint() {
		if (!session || session.isComplete || !canAcceptInput || activeShape !== 'rectangular') return;
		if (gameStore.usePowerup('hint')) {
			const hintPath = getHint(session);
			session = { ...session, hintPath };
		}
	}

	function handleKeydown(e: KeyboardEvent) {
		if (!canAcceptInput) return;
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
			dispatchMove(dir);
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
		if (!canAcceptInput) return;
		const dx = e.changedTouches[0].clientX - touchStartX;
		const dy = e.changedTouches[0].clientY - touchStartY;
		const threshold = 30;

		if (Math.abs(dx) > Math.abs(dy)) {
			if (Math.abs(dx) > threshold) {
				dispatchMove(dx > 0 ? 'right' : 'left');
			}
		} else {
			if (Math.abs(dy) > threshold) {
				dispatchMove(dy > 0 ? 'down' : 'up');
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

{#if gameIsActive && levelDef}
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
					<span class="hud-stat-value" class:over-threshold={currentMoves > levelDef.twoStarMoves}>
						{currentMoves}
					</span>
					<span class="hud-stat-target">/ {levelDef.twoStarMoves}</span>
				</div>
			</div>
		</div>

		<!-- Maze -->
		<div class="maze-area">
			{#if activeShape === 'hexagonal' && hexState}
				<HexMazeRenderer
					maze={hexState.maze}
					playerPos={hexState.pos}
					wallColor={gameStore.player.wallColor}
				/>
			{:else if activeShape === 'circular' && circState}
				<CircularMazeRenderer
					maze={circState.maze}
					playerPos={circState.pos}
					wallColor={gameStore.player.wallColor}
				/>
			{:else if activeShape === 'triangular' && triState}
				<TriMazeRenderer
					maze={triState.maze}
					playerPos={triState.pos}
					wallColor={gameStore.player.wallColor}
				/>
			{:else if session}
				<MazeRenderer
					maze={session.maze}
					playerPos={session.playerPos}
					wallColor={gameStore.player.wallColor}
					hintPath={session.hintPath}
					showVisited={true}
					{visitedCells}
				/>
			{/if}
		</div>

		<!-- Powerups & Controls -->
		<div class="controls-bar">
			<div class="powerups">
				<button class="powerup-btn" onclick={useHint} disabled={gameStore.player.hintsOwned <= 0}>
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16" aria-hidden="true">
						<circle cx="12" cy="12" r="10"/>
						<path d="M12 8v4M12 16h.01"/>
					</svg>
					Hint
					<span class="powerup-count">({gameStore.player.hintsOwned})</span>
				</button>
			</div>
			<div class="dpad" role="group" aria-label="Movement controls">
				<button class="dpad-btn dpad-up" onclick={() => dispatchMove('up')} aria-label="Move up" disabled={!canAcceptInput}>
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
						<polyline points="18 15 12 9 6 15"/>
					</svg>
				</button>
				<div class="dpad-middle">
					<button class="dpad-btn dpad-left" onclick={() => dispatchMove('left')} aria-label="Move left" disabled={!canAcceptInput}>
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
							<polyline points="15 18 9 12 15 6"/>
						</svg>
					</button>
					<div class="dpad-center" aria-hidden="true"></div>
					<button class="dpad-btn dpad-right" onclick={() => dispatchMove('right')} aria-label="Move right" disabled={!canAcceptInput}>
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
							<polyline points="9 18 15 12 9 6"/>
						</svg>
					</button>
				</div>
				<button class="dpad-btn dpad-down" onclick={() => dispatchMove('down')} aria-label="Move down" disabled={!canAcceptInput}>
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
						<polyline points="6 9 12 15 18 9"/>
					</svg>
				</button>
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

{#if showOutro && gameIsActive && levelDef}
	<MazeOutroOverlay
		title="Level Complete!"
		playerName={gameStore.player.playerName}
		time={elapsed}
		moves={currentMoves}
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
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: var(--radius-lg);
		border: 1px solid rgba(56, 189, 248, 0.2);
		box-shadow: 0 0 24px rgba(56, 189, 248, 0.08), inset 0 0 40px rgba(0, 0, 0, 0.4);
		background: #080e1e;
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

	.powerup-count {
		color: var(--color-text-muted);
		font-size: var(--text-xs);
	}

	.dpad {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 3px;
	}

	.dpad-middle {
		display: flex;
		align-items: center;
		gap: 3px;
	}

	.dpad-center {
		width: 48px;
		height: 48px;
		display: flex;
		align-items: center;
		justify-content: center;
		pointer-events: none;
	}

	.dpad-center::after {
		content: '';
		width: 6px;
		height: 6px;
		border-radius: 50%;
		background: rgba(56, 189, 248, 0.3);
	}

	.dpad-btn {
		width: 48px;
		height: 48px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: linear-gradient(145deg, rgba(30, 41, 59, 0.9), rgba(15, 23, 42, 0.95));
		border: 1px solid rgba(56, 189, 248, 0.25);
		border-radius: 10px;
		color: rgba(186, 230, 253, 0.9);
		font-size: 18px;
		cursor: pointer;
		transition: all 120ms ease;
		user-select: none;
		-webkit-user-select: none;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.06);
	}

	.dpad-btn:hover {
		background: linear-gradient(145deg, rgba(30, 58, 100, 0.95), rgba(15, 35, 72, 0.95));
		border-color: rgba(56, 189, 248, 0.55);
		color: #bae6fd;
		box-shadow: 0 0 12px rgba(56, 189, 248, 0.2), 0 2px 8px rgba(0, 0, 0, 0.4);
	}

	.dpad-btn:active {
		background: linear-gradient(145deg, rgba(56, 189, 248, 0.3), rgba(14, 116, 144, 0.4));
		border-color: #38bdf8;
		color: white;
		box-shadow: 0 0 16px rgba(56, 189, 248, 0.4);
		transform: scale(0.93);
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
		}
		.dpad-center {
			width: 52px;
			height: 52px;
		}
	}
</style>
