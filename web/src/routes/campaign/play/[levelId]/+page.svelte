<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { onDestroy, tick, untrack } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { getSkinById } from '$lib/core/skins';
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
        import MapCollectiblePopup from '$lib/components/MapCollectiblePopup.svelte';
        import type { MapCollectible, LevelReward } from '$lib/core/types';
	const levelKey = $derived($page.params.levelId as string);
	const worldId = $derived(Number((levelKey ?? '').split('-')[0]));
	const levelNumber = $derived((levelKey ?? '').split('-').slice(1).join('-'));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levelDef = $derived(worldDef ? getLevelByNumber(worldDef, levelNumber) : undefined);
	const theme = $derived(getWorldTheme(worldId));
	const skinImageUrl = $derived(getSkinById(gameStore.player.currentSkinId)?.imageUrl ?? 'player_image0');

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

	// Algorithm display names and algorithm encyclopedia IDs
	const ALGO_MAP: Record<string, { name: string; id: string }> = {
		backtracking:        { name: 'Recursive Backtracking', id: 'backtracking' },
		huntAndKill:         { name: 'Hunt and Kill',          id: 'huntAndKill' },
		prims:               { name: "Prim's Algorithm",        id: 'prims' },
		kruskals:            { name: "Kruskal's Algorithm",     id: 'kruskals' },
		growingTree_50_50:   { name: 'Growing Tree (50/50)',   id: 'growingTree' },
		growingTree_75_25:   { name: 'Growing Tree (75/25)',   id: 'growingTree' },
		growingTree_25_75:   { name: 'Growing Tree (25/75)',   id: 'growingTree' },
		growingTree_50_0:    { name: 'Growing Tree (50/0)',    id: 'growingTree' },
	};

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
	let prevBestTime = $state(0);
	let prevBestMoves = $state(0);
	let visitedCells = $state(new Set<string>());
        /** Set after level completion if this level has a reward; cleared when player collects it */
        let pendingLevelReward = $state<LevelReward | null>(null);
        let showLevelRewardPopup = $state(false);

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

		// Capture personal bests before overwriting
		const prev = gameStore.getLevelProgress(worldId, levelDef.levelNumber);
		prevBestTime  = prev?.bestTime  ?? 0;
		prevBestMoves = prev?.bestMoves ?? 0;

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

                // If this level has a reward and it hasn't been collected yet, show it first
                const rewardKey = `level_reward_${worldId}_${levelDef.levelNumber}`;
                if (levelDef.levelReward && !gameStore.isMapItemCollected(worldId, rewardKey)) {
                        pendingLevelReward = levelDef.levelReward;
                        showLevelRewardPopup = true;
                } else {
                        showOutro = true;
                }
        }

        function collectLevelReward() {
                if (!pendingLevelReward || !levelDef) return;
                const r = pendingLevelReward.reward;
                const rewardKey = `level_reward_${worldId}_${levelDef.levelNumber}`;
                gameStore.collectMapItem(worldId, rewardKey);
                if (r.coins) gameStore.addCoins(r.coins);
                if (r.powerup && r.powerupCount) {
                        for (let i = 0; i < r.powerupCount; i++) gameStore.addPowerup(r.powerup!);
                }
                if (r.skinId != null) gameStore.unlockSkin(r.skinId);
                if (r.keyItemId) gameStore.addKey(r.keyItemId);
                pendingLevelReward = null;
                showLevelRewardPopup = false;
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

	function goHome() {
		goto(`${base}/`);
	}

	function goShop() {
		goto(`${base}/shop`);
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
	<title>Level {levelNumber} – Maze Escape: Pathbound</title>
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
					{skinImageUrl}
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

{#if showLevelRewardPopup && pendingLevelReward}
        {@const pseudoCollectible = {
                id: `level_reward_${worldId}_${levelDef?.levelNumber ?? ''}`,
                type: pendingLevelReward.type,
                tile: { col: 0, row: 0 },
                area: 1,
                label: pendingLevelReward.label,
                reward: pendingLevelReward.reward
        } satisfies import('$lib/core/types').MapCollectible}
        <MapCollectiblePopup
                collectible={pseudoCollectible}
                onCollect={collectLevelReward}
                onClose={collectLevelReward}
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
		mazeWidth={levelDef.width}
		mazeHeight={levelDef.height}
		algoName={ALGO_MAP[levelDef.levelType]?.name ?? levelDef.levelType}
		algoId={ALGO_MAP[levelDef.levelType]?.id ?? ''}
		algoLinkBase={base}
		bestTime={prevBestTime}
		bestMoves={prevBestMoves}
		actions={[
			{ label: '🏠 Home', onclick: goHome },
			{ label: '🛒 Shop', onclick: goShop },
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

	/* ── HUD ──────────────────────────────────────── */
	.game-hud {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: var(--space-4);
		padding: var(--space-2) var(--space-4);
		flex-shrink: 0;
		background: rgba(6, 14, 36, 0.80);
		backdrop-filter: blur(14px);
		-webkit-backdrop-filter: blur(14px);
		border: 1px solid rgba(56, 189, 248, 0.15);
		border-radius: var(--radius-lg);
		box-shadow: 0 4px 20px rgba(0, 0, 0, 0.35);
	}

	.hud-back {
		font-size: var(--text-sm);
		color: rgba(148, 163, 184, 0.8);
		text-decoration: none;
		font-family: 'Courier New', monospace;
		letter-spacing: 0.04em;
		transition: color 0.15s;
	}

	.hud-back:hover { color: #38bdf8; }

	.hud-center {
		display: flex;
		align-items: center;
		gap: var(--space-2);
	}

	.hud-label {
		font-family: 'Courier New', monospace;
		font-weight: 700;
		font-size: var(--text-base);
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: #e2e8f0;
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
		gap: var(--space-3);
	}

	.hud-stat {
		display: flex;
		align-items: baseline;
		gap: 4px;
		font-size: var(--text-xs);
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-md);
		padding: 3px 8px;
	}

	.hud-stat-label { color: rgba(148, 163, 184, 0.7); }

	.hud-stat-value {
		font-weight: 700;
		font-family: 'Courier New', monospace;
		font-size: var(--text-sm);
		color: #4ade80;
		transition: color 0.2s;
	}

	@keyframes red-flash {
		0%, 100% { color: #f87171; }
		50% { color: #fca5a5; text-shadow: 0 0 6px #f87171; }
	}

	.hud-stat-value.over-threshold {
		animation: red-flash 1s ease-in-out infinite;
	}

	.hud-stat-target {
		color: rgba(148, 163, 184, 0.5);
		font-size: 0.65rem;
	}

	/* ── Maze area ────────────────────────────────── */
	@keyframes maze-border-pulse {
		0%, 100% { border-color: rgba(56, 189, 248, 0.18); box-shadow: 0 0 24px rgba(56, 189, 248, 0.07), inset 0 0 40px rgba(0, 0, 0, 0.4); }
		50%       { border-color: rgba(56, 189, 248, 0.38); box-shadow: 0 0 36px rgba(56, 189, 248, 0.18), inset 0 0 40px rgba(0, 0, 0, 0.4); }
	}

	.maze-area {
		flex: 1;
		min-height: 0;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: var(--radius-lg);
		border: 1px solid rgba(56, 189, 248, 0.2);
		animation: maze-border-pulse 3s ease-in-out infinite;
		background: #080e1e;
		overflow: hidden;
	}

	/* ── Controls bar ─────────────────────────────── */
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
		position: relative;
		display: flex;
		align-items: center;
		gap: var(--space-2);
		background: rgba(15, 23, 42, 0.75);
		border: 1px solid rgba(56, 189, 248, 0.22);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		font-size: var(--text-sm);
		color: #bae6fd;
		cursor: pointer;
		transition: all var(--transition-fast);
		overflow: hidden;
		backdrop-filter: blur(8px);
	}

	/* Shimmer on hover */
	.powerup-btn::before {
		content: '';
		position: absolute;
		inset: 0;
		background: linear-gradient(105deg, transparent 40%, rgba(56,189,248,0.12) 50%, transparent 60%);
		background-size: 200% 100%;
		background-position: -200% 0;
		transition: background-position 0.5s ease;
		pointer-events: none;
	}

	.powerup-btn:not(:disabled):hover {
		border-color: rgba(56, 189, 248, 0.5);
		box-shadow: 0 0 12px rgba(56, 189, 248, 0.18);
	}

	.powerup-btn:not(:disabled):hover::before {
		background-position: 200% 0;
	}

	.powerup-btn:disabled {
		opacity: 0.4;
		cursor: default;
	}

	.powerup-count {
		color: rgba(148,163,184,0.6);
		font-size: var(--text-xs);
	}

	/* ── D-pad ────────────────────────────────────── */
	.dpad {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 4px;
		padding: var(--space-3);
		background: rgba(6, 14, 36, 0.70);
		border: 1px solid rgba(56, 189, 248, 0.14);
		border-radius: var(--radius-xl);
		backdrop-filter: blur(12px);
		-webkit-backdrop-filter: blur(12px);
		box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
	}

	.dpad-middle {
		display: flex;
		align-items: center;
		gap: 4px;
	}

	.dpad-center {
		width: 52px;
		height: 52px;
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
		background: rgba(56, 189, 248, 0.4);
		box-shadow: 0 0 6px rgba(56, 189, 248, 0.4);
	}

	.dpad-btn {
		width: 52px;
		height: 52px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: linear-gradient(145deg, rgba(30, 41, 59, 0.92), rgba(15, 23, 42, 0.96));
		border: 1px solid rgba(56, 189, 248, 0.28);
		border-radius: 11px;
		color: rgba(186, 230, 253, 0.9);
		cursor: pointer;
		transition: all 100ms ease;
		user-select: none;
		-webkit-user-select: none;
		box-shadow: 0 3px 10px rgba(0, 0, 0, 0.45), inset 0 1px 0 rgba(255, 255, 255, 0.07);
	}

	.dpad-btn:hover:not(:disabled) {
		background: linear-gradient(145deg, rgba(30, 64, 120, 0.95), rgba(15, 38, 80, 0.95));
		border-color: rgba(56, 189, 248, 0.65);
		color: #e0f2fe;
		box-shadow: 0 0 18px rgba(56, 189, 248, 0.28), 0 3px 10px rgba(0, 0, 0, 0.45);
	}

	@keyframes dpad-burst {
		0%   { box-shadow: 0 0 0px rgba(56, 189, 248, 0); }
		30%  { box-shadow: 0 0 22px rgba(56, 189, 248, 0.55); }
		100% { box-shadow: 0 0 6px rgba(56, 189, 248, 0.15); }
	}

	.dpad-btn:active:not(:disabled) {
		background: linear-gradient(145deg, rgba(56, 189, 248, 0.35), rgba(14, 116, 144, 0.45));
		border-color: #38bdf8;
		color: #fff;
		animation: dpad-burst 0.25s ease-out forwards;
		transform: scale(0.91);
	}

	.dpad-btn:disabled {
		opacity: 0.35;
		cursor: default;
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
			width: 56px;
			height: 56px;
		}
		.dpad-center {
			width: 56px;
			height: 56px;
		}
	}
</style>
