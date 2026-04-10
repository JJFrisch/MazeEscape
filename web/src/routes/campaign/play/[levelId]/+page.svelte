<script lang="ts">
	import { browser } from '$app/environment';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { onDestroy, untrack } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { createGameSession, getHint, getCompassPath, calculateStars, buildReplayPositions } from '$lib/core/session';
	import type { MasteryRewardUnlock } from '$lib/core/deities';
	import { canMove, applyMove } from '$lib/core/maze';
	import {
		generateHexMaze,
		generateHexMazeKruskals,
		generateHexMazePrims,
		canMoveHex,
		applyMoveHex,
		keyToHexDir
	} from '$lib/core/hexMaze';
	import {
		generateCircularMaze,
		generateCircularMazePrims,
		canMoveCircular,
		keyToCircularDir
	} from '$lib/core/circularMaze';
	import {
		generateTriMaze,
		generateTriMazeHuntAndKill,
		generateTriMazeKruskals,
		canMoveTri,
		applyMoveTri,
		keyToTriDir
	} from '$lib/core/triMaze';
	import { getResponsiveCampaignLevelConfig, type CampaignViewport } from '$lib/core/campaignSizing';
	import type { CircularMazeData, Direction, HexMazeData, MapCollectible, MazeData, Position, TriMazeData } from '$lib/core/types';
	import { getWorldTheme } from '$lib/worldThemes';
	import MazeRenderer from '$lib/components/MazeRenderer.svelte';
	import HexMazeRenderer from '$lib/components/HexMazeRenderer.svelte';
	import CircularMazeRenderer from '$lib/components/CircularMazeRenderer.svelte';
	import TriMazeRenderer from '$lib/components/TriMazeRenderer.svelte';
	import MazeIntroOverlay from '$lib/components/MazeIntroOverlay.svelte';
	import MazeOutroOverlay from '$lib/components/MazeOutroOverlay.svelte';
	import AchievementUnlockPopup from '../../../../lib/components/AchievementUnlockPopup.svelte';
	import MasteryRewardPopup from '$lib/components/MasteryRewardPopup.svelte';
	import MapCollectiblePopup from '$lib/components/MapCollectiblePopup.svelte';

	type RectCampaignSession = {
		shape: 'rectangular';
		maze: MazeData;
		playerPos: Position;
		moves: number;
		elapsed: number;
		hintsUsed: number;
		isComplete: boolean;
		hintPath: Position[] | null;
	};

	type HexCampaignSession = {
		shape: 'hexagonal';
		maze: HexMazeData;
		playerPos: { col: number; row: number };
		moves: number;
		elapsed: number;
		hintsUsed: number;
		isComplete: boolean;
		hintPath: null;
	};

	type CircularCampaignSession = {
		shape: 'circular';
		maze: CircularMazeData;
		playerPos: { ring: number; sector: number };
		moves: number;
		elapsed: number;
		hintsUsed: number;
		isComplete: boolean;
		hintPath: null;
	};

	type TriCampaignSession = {
		shape: 'triangular';
		maze: TriMazeData;
		playerPos: { col: number; row: number };
		moves: number;
		elapsed: number;
		hintsUsed: number;
		isComplete: boolean;
		hintPath: null;
	};

	type CampaignSessionState = RectCampaignSession | HexCampaignSession | CircularCampaignSession | TriCampaignSession;

	// Parse route: "worldId-levelNumber" e.g. "1-5" or "2-3b"
	const levelKey = $derived($page.params.levelId as string);
	const worldId = $derived(Number((levelKey ?? '').split('-')[0]));
	const levelNumber = $derived((levelKey ?? '').split('-').slice(1).join('-'));
	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levelDef = $derived(worldDef ? getLevelByNumber(worldDef, levelNumber) : undefined);
	const theme = $derived(getWorldTheme(worldId));
	let viewport = $state<CampaignViewport>(browser ? { width: window.innerWidth, height: window.innerHeight } : { width: 0, height: 0 });
	const activeLevelConfig = $derived(levelDef ? getResponsiveCampaignLevelConfig(levelDef, viewport) : null);
	const activeLevelDimensions = $derived(activeLevelConfig ? { width: activeLevelConfig.width, height: activeLevelConfig.height } : null);
	const activeTwoStarMoves = $derived(activeLevelConfig?.twoStarMoves ?? levelDef?.twoStarMoves ?? 0);
	const activeThreeStarTime = $derived(activeLevelConfig?.threeStarTime ?? levelDef?.threeStarTime ?? 0);
	const activeFiveStarMoves = $derived(activeLevelConfig?.fiveStarMoves ?? levelDef?.fiveStarMoves ?? 0);
	const activeFiveStarTime = $derived(activeLevelConfig?.fiveStarTime ?? levelDef?.fiveStarTime ?? 0);

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

	let session = $state<CampaignSessionState | null>(null);
	let elapsed = $state(0);
	let timerInterval: ReturnType<typeof setInterval> | undefined;
	let showIntro = $state(false);
	let showOutro = $state(false);
	let loadError = $state('');
	let initializedLevelKey = $state('');
	let victoryStars = $state({ star1: false, star2: false, star3: false, total: 0 });
	let coinsEarned = $state(0);
	let previousBestTime = $state(0);
	let previousBestMoves = $state(0);
	let visitedCells = $state(new Set<string>());
	let moveQueue: Direction[] = [];
	const MOVE_QUEUE_MAX = 2;
	let currentRunMoves = $state<Direction[]>([]);
	let ghostPositions = $state<{ x: number; y: number }[]>([]);
	let ghostIndex = $state(0);
	let ghostPos = $state<{ x: number; y: number } | null>(null);
	let ghostTimer: ReturnType<typeof setInterval> | undefined;
	let rewardPopup = $state<MapCollectible | null>(null);
	// Powerup state
	let hourglassFrozen = $state(false);
	let hourglassTimer: ReturnType<typeof setTimeout> | undefined;
	let compassTimer: ReturnType<typeof setTimeout> | undefined;
	// Completion reward queues
	let masteryUnlocks = $state<MasteryRewardUnlock[]>([]);
	let newlyUnlocked = $state<string[]>([]);
	let shownMasteryUnlock = $derived(masteryUnlocks[0] ?? null);
	let shownAchievementId = $derived(newlyUnlocked[0] ?? null);

	function updateViewport() {
		if (!browser) return;
		viewport = {
			width: window.innerWidth,
			height: window.innerHeight
		};
	}

	function createHexCampaignSession(width: number, height: number, algorithm: string, seed: number): HexCampaignSession {
		const maze = algorithm === 'prims'
			? generateHexMazePrims(width, height, seed)
			: algorithm === 'kruskals'
				? generateHexMazeKruskals(width, height, seed)
				: generateHexMaze(width, height, seed);

		return {
			shape: 'hexagonal',
			maze,
			playerPos: { ...maze.start },
			moves: 0,
			elapsed: 0,
			hintsUsed: 0,
			isComplete: false,
			hintPath: null
		};
	}

	function createCircularCampaignSession(size: number, algorithm: string, seed: number): CircularCampaignSession {
		const maze = algorithm === 'prims'
			? generateCircularMazePrims(size, seed)
			: generateCircularMaze(size, seed);

		return {
			shape: 'circular',
			maze,
			playerPos: { ...maze.start },
			moves: 0,
			elapsed: 0,
			hintsUsed: 0,
			isComplete: false,
			hintPath: null
		};
	}

	function createTriCampaignSession(width: number, height: number, algorithm: string, seed: number): TriCampaignSession {
		const maze = algorithm === 'huntAndKill'
			? generateTriMazeHuntAndKill(width, height, seed)
			: algorithm === 'kruskals'
				? generateTriMazeKruskals(width, height, seed)
				: generateTriMaze(width, height, seed);

		return {
			shape: 'triangular',
			maze,
			playerPos: { ...maze.start },
			moves: 0,
			elapsed: 0,
			hintsUsed: 0,
			isComplete: false,
			hintPath: null
		};
	}

	function createCampaignSession(seed: number): CampaignSessionState {
		if (!levelDef || !activeLevelConfig) {
			throw new Error('Level configuration is unavailable.');
		}

		switch (levelDef.mazeShape) {
			case 'hexagonal':
				return createHexCampaignSession(activeLevelConfig.width, activeLevelConfig.height, levelDef.levelType, seed);
			case 'circular':
				return createCircularCampaignSession(activeLevelConfig.height, levelDef.levelType, seed);
			case 'triangular':
				return createTriCampaignSession(activeLevelConfig.width, activeLevelConfig.height, levelDef.levelType, seed);
			default: {
				const rectangularSession = createGameSession({
					width: activeLevelConfig.width,
					height: activeLevelConfig.height,
					algorithm: levelDef.levelType,
					seed,
					twoStarMoves: activeLevelConfig.twoStarMoves,
					threeStarTime: activeLevelConfig.threeStarTime
				});

				return {
					shape: 'rectangular',
					...rectangularSession
				};
			}
		}
	}

	function supportsHintOverlay(activeSession: CampaignSessionState | null): activeSession is RectCampaignSession {
		return !!activeSession && activeSession.shape === 'rectangular';
	}

	function getSessionGridSize(activeSession: CampaignSessionState | null): { width: number; height: number } {
		if (!activeSession) {
			return activeLevelDimensions ?? { width: levelDef?.width ?? 0, height: levelDef?.height ?? 0 };
		}

		switch (activeSession.shape) {
			case 'rectangular':
				return { width: activeSession.maze.width, height: activeSession.maze.height };
			case 'hexagonal':
				return { width: activeSession.maze.cols, height: activeSession.maze.rows };
			case 'triangular':
				return { width: activeSession.maze.cols, height: activeSession.maze.rows };
			case 'circular':
				return { width: activeSession.maze.numRings, height: activeSession.maze.numRings };
		}
	}

	function tryMoveCampaignSession(activeSession: CampaignSessionState, direction: Direction): CampaignSessionState | null {
		switch (activeSession.shape) {
			case 'rectangular': {
				if (!canMove(activeSession.maze.cells, activeSession.playerPos, direction, activeSession.maze.width, activeSession.maze.height)) {
					return null;
				}

				const playerPos = applyMove(activeSession.playerPos, direction);
				return {
					...activeSession,
					playerPos,
					moves: activeSession.moves + 1,
					hintPath: null,
					isComplete: playerPos.x === activeSession.maze.end.x && playerPos.y === activeSession.maze.end.y
				};
			}
			case 'hexagonal': {
				for (const candidate of keyToHexDir(direction)) {
					if (!canMoveHex(activeSession.maze.cells, activeSession.playerPos.col, activeSession.playerPos.row, candidate)) continue;
					const playerPos = applyMoveHex(activeSession.playerPos.col, activeSession.playerPos.row, candidate);
					return {
						...activeSession,
						playerPos,
						moves: activeSession.moves + 1,
						isComplete: playerPos.col === activeSession.maze.end.col && playerPos.row === activeSession.maze.end.row
					};
				}

				return null;
			}
			case 'circular': {
				const directions = keyToCircularDir(activeSession.playerPos.sector, activeSession.maze.rings[activeSession.playerPos.ring].length, direction);
				for (const candidate of directions) {
					const next = canMoveCircular(activeSession.maze, activeSession.playerPos.ring, activeSession.playerPos.sector, candidate);
					if (!next.canMove) continue;
					const playerPos = { ring: next.ring, sector: next.sector };
					return {
						...activeSession,
						playerPos,
						moves: activeSession.moves + 1,
						isComplete: playerPos.ring === activeSession.maze.end.ring && playerPos.sector === activeSession.maze.end.sector
					};
				}

				return null;
			}
			case 'triangular': {
				const pointsUp = activeSession.maze.cells[activeSession.playerPos.row][activeSession.playerPos.col].pointsUp;
				for (const candidate of keyToTriDir(pointsUp, direction)) {
					if (!canMoveTri(activeSession.maze.cells, activeSession.playerPos.col, activeSession.playerPos.row, candidate, activeSession.maze.cols, activeSession.maze.rows)) continue;
					const playerPos = applyMoveTri(activeSession.playerPos.col, activeSession.playerPos.row, pointsUp, candidate);
					return {
						...activeSession,
						playerPos,
						moves: activeSession.moves + 1,
						isComplete: playerPos.col === activeSession.maze.end.col && playerPos.row === activeSession.maze.end.row
					};
				}

				return null;
			}
		}
	}

	function getBlinkDestination(activeSession: CampaignSessionState): CampaignSessionState['playerPos'] | null {
		switch (activeSession.shape) {
			case 'rectangular': {
				const candidates: Position[] = [];
				for (let y = 0; y < activeSession.maze.height; y++) {
					for (let x = 0; x < activeSession.maze.width; x++) {
						if (x !== activeSession.playerPos.x || y !== activeSession.playerPos.y) candidates.push({ x, y });
					}
				}
				return candidates[Math.floor(Math.random() * candidates.length)] ?? null;
			}
			case 'hexagonal': {
				const candidates = Array.from(activeSession.maze.cells.values())
					.filter((cell) => cell.col !== activeSession.playerPos.col || cell.row !== activeSession.playerPos.row)
					.map((cell) => ({ col: cell.col, row: cell.row }));
				return candidates[Math.floor(Math.random() * candidates.length)] ?? null;
			}
			case 'circular': {
				const candidates: Array<{ ring: number; sector: number }> = [];
				for (let ring = 0; ring < activeSession.maze.numRings; ring++) {
					for (let sector = 0; sector < activeSession.maze.rings[ring].length; sector++) {
						if (ring !== activeSession.playerPos.ring || sector !== activeSession.playerPos.sector) {
							candidates.push({ ring, sector });
						}
					}
				}
				return candidates[Math.floor(Math.random() * candidates.length)] ?? null;
			}
			case 'triangular': {
				const candidates = activeSession.maze.cells.flat()
					.filter((cell) => cell.col !== activeSession.playerPos.col || cell.row !== activeSession.playerPos.row)
					.map((cell) => ({ col: cell.col, row: cell.row }));
				return candidates[Math.floor(Math.random() * candidates.length)] ?? null;
			}
		}
	}

	function startGame() {
		hourglassFrozen = false;
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
		clearInterval(ghostTimer);
		masteryUnlocks = [];
		newlyUnlocked = [];
		rewardPopup = null;
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

			session = createCampaignSession(seed);
			loadError = '';
			elapsed = 0;
			showOutro = false;
			visitedCells = session.shape === 'rectangular'
				? new Set([`${session.maze.start.x},${session.maze.start.y}`])
				: new Set();
			moveQueue = [];
			currentRunMoves = [];
			const savedGhost = session.shape === 'rectangular'
				? gameStore.getGhostRun(worldId, levelDef.levelNumber)
				: undefined;
			ghostPositions = session.shape === 'rectangular' && savedGhost?.moves?.length
				? buildReplayPositions(session.maze, savedGhost.moves)
				: [];
			ghostIndex = 0;
			ghostPos = ghostPositions[0] ?? null;
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
		clearInterval(ghostTimer);
		timerInterval = setInterval(() => {
			if (session && !session.isComplete && !hourglassFrozen) {
				elapsed += 0.01;
			}
		}, 10);
		if (session.shape === 'rectangular' && ghostPositions.length > 1) {
			ghostTimer = setInterval(() => {
				if (showOutro) return;
				ghostIndex = (ghostIndex + 1) % ghostPositions.length;
				ghostPos = ghostPositions[ghostIndex] ?? ghostPositions[ghostPositions.length - 1] ?? null;
			}, 180);
		}
	}

	function handleMove(direction: Direction) {
		if (!session || session.isComplete || showIntro) return;

		const nextSession = tryMoveCampaignSession(session, direction);
		if (!nextSession) return;

		currentRunMoves = [...currentRunMoves, direction];
		session = nextSession;

		if (nextSession.shape === 'rectangular') {
			visitedCells = new Set([...visitedCells, `${nextSession.playerPos.x},${nextSession.playerPos.y}`]);
		}

		if (nextSession.isComplete) {
			onLevelComplete();
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
		clearInterval(ghostTimer);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
		hourglassFrozen = false;

		if (!levelDef || !session) return;

		const stars = calculateStars(
			session.moves,
			elapsed,
			session.hintsUsed,
			activeTwoStarMoves,
			activeThreeStarTime,
			activeFiveStarMoves,
			activeFiveStarTime
		);
		victoryStars = stars;

		// Coins: base + bonus for stars
		const baseCoins = Math.floor(Math.random() * 100) + 50;
		coinsEarned = baseCoins + stars.total * 25;
		gameStore.addCoins(coinsEarned);

		// Crystal shards: 1 per star earned
		gameStore.addCrystalShards(stars.total);

		// Track algorithm mastery for the deity system
		const masteryRewardUnlocks = gameStore.recordAlgoMastery(levelDef.levelType);
 		if (masteryRewardUnlocks.length > 0) {
			masteryUnlocks = [...masteryRewardUnlocks];
		}

		// Increment total mazes completed
		gameStore.incrementMazesCompleted();

		// Achievement progress for event-based achievements
		if (elapsed < 30) gameStore.markAchievementProgress('speed_runner', 1);
		if (session.hintsUsed === 0) gameStore.markAchievementProgress('hint_free', 1);

		// Save progress
		const priorProgress = gameStore.getLevelProgress(worldId, levelDef.levelNumber);
		previousBestTime = priorProgress?.bestTime ?? 0;
		previousBestMoves = priorProgress?.bestMoves ?? 0;
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
		const wasCompleted = !!gameStore.getLevelProgress(worldId, levelDef.levelNumber)?.completed;
		gameStore.saveLevelProgress(worldId, updatedLevel, currentRunMoves);

		if (!wasCompleted && levelDef.levelReward?.reward.specialItemId) {
			const granted = gameStore.awardSpecialItem(levelDef.levelReward.reward.specialItemId);
			if (granted) {
				rewardPopup = {
					id: `reward-${levelDef.levelNumber}`,
					type: levelDef.levelReward.type,
					tile: { col: 0, row: 0 },
					area: 0,
					label: levelDef.levelReward.label,
					reward: levelDef.levelReward.reward
				};
			}
		}

		if (gameStore.activeEvent) {
			gameStore.incrementEventProgress(gameStore.activeEvent.id, 1);
		}

		// Check achievements and queue popups
		const unlocked = gameStore.checkAchievements();
		if (unlocked.length > 0) {
			newlyUnlocked = [...unlocked];
		}

		showOutro = true;
	}

	function useHint() {
		if (!supportsHintOverlay(session) || session.isComplete || showIntro) return;
		if (gameStore.usePowerup('hint')) {
			const nextSession = { ...session };
			const hintPath = getHint(nextSession);
			session = { ...nextSession, hintPath };
		}
	}

	function useCompass() {
		if (!supportsHintOverlay(session) || session.isComplete || showIntro) return;
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
		if (!session || session.isComplete || showIntro || hourglassFrozen) return;
		if (gameStore.usePowerup('hourglass')) {
			hourglassFrozen = true;
			clearTimeout(hourglassTimer);
			hourglassTimer = setTimeout(() => { hourglassFrozen = false; }, 15000);
		}
	}

	function useBlinkScroll() {
		if (!session || session.isComplete || showIntro) return;
		if (gameStore.usePowerup('blinkScroll')) {
			const destination = getBlinkDestination(session);
			if (!destination) return;
			session = { ...session, playerPos: destination, hintPath: null } as CampaignSessionState;
		}
	}

	function useDoubleCoins() {
		if (!session || session.isComplete || showIntro) return;
		gameStore.usePowerup('doubleCoinsToken');
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
		clearInterval(ghostTimer);
		clearTimeout(hourglassTimer);
		clearTimeout(compassTimer);
	});

	// Restart game when route changes (untrack so startGame's reactive reads
	// don't become additional effect dependencies)
	$effect(() => {
		if (!levelKey || initializedLevelKey === levelKey) return;
		initializedLevelKey = levelKey;
		untrack(() => startGame());
	});
</script>

<svelte:window onkeydown={handleKeydown} onresize={updateViewport} />

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
				{#if levelDef.levelKind === 'boss'}
					<span class="boss-badge">Boss</span>
				{/if}
				{#if levelDef.levelNumber.includes('b')}
					<span class="bonus-badge">Bonus</span>
				{/if}
			</div>

			<div class="hud-stats">
				<div class="hud-stat">
					<span class="hud-stat-label">⏱️</span>
					<span class="hud-stat-value" class:over-threshold={elapsed > activeThreeStarTime}>
						{elapsed.toFixed(1)}s
					</span>
					{#if hourglassFrozen}
						<span class="frozen-badge">FROZEN</span>
					{:else}
						<span class="hud-stat-target">/ {activeThreeStarTime}s</span>
					{/if}
				</div>
				<div class="hud-stat">
					<span class="hud-stat-label">👣</span>
					<span class="hud-stat-value" class:over-threshold={session.moves > activeTwoStarMoves}>
						{session.moves}
					</span>
					<span class="hud-stat-target">/ {activeTwoStarMoves}</span>
				</div>
			</div>
		</div>

		<!-- Maze -->
		<div class="maze-area">
			{#if session.shape === 'rectangular'}
				<MazeRenderer
					maze={session.maze}
					playerPos={session.playerPos}
					ghostPos={ghostPos}
					wallColor={gameStore.player.wallColor}
					backgroundColor={gameStore.player.mazeBackgroundColor}
					hintPath={session.hintPath}
					showVisited={true}
					{visitedCells}
				/>
			{:else if session.shape === 'hexagonal'}
				<HexMazeRenderer
					maze={session.maze}
					playerPos={session.playerPos}
					wallColor={gameStore.player.wallColor}
					backgroundColor={gameStore.player.mazeBackgroundColor}
				/>
			{:else if session.shape === 'circular'}
				<CircularMazeRenderer
					maze={session.maze}
					playerPos={session.playerPos}
					wallColor={gameStore.player.wallColor}
					backgroundColor={gameStore.player.mazeBackgroundColor}
				/>
			{:else}
				<TriMazeRenderer
					maze={session.maze}
					playerPos={session.playerPos}
					wallColor={gameStore.player.wallColor}
					backgroundColor={gameStore.player.mazeBackgroundColor}
				/>
			{/if}
		</div>

		<!-- Powerups & Controls -->
		<div class="controls-bar">
			<div class="powerups">
				<button class="powerup-btn" onclick={useHint} disabled={gameStore.player.hintsOwned <= 0 || !supportsHintOverlay(session)}>
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16" aria-hidden="true">
						<circle cx="12" cy="12" r="10"/>
						<path d="M12 8v4M12 16h.01"/>
					</svg>
					Hint
					<span class="powerup-count">({gameStore.player.hintsOwned})</span>
				</button>
				<button class="powerup-btn" onclick={useCompass} disabled={(gameStore.player.compassOwned ?? 0) <= 0 || !supportsHintOverlay(session)}>
					🧭
					Compass
					<span class="powerup-count">({gameStore.player.compassOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useHourglass} disabled={(gameStore.player.hourglassOwned ?? 0) <= 0 || hourglassFrozen}>
					⏳
					Hourglass
					<span class="powerup-count">({gameStore.player.hourglassOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useBlinkScroll} disabled={(gameStore.player.blinkScrollsOwned ?? 0) <= 0}>
					✨
					Blink
					<span class="powerup-count">({gameStore.player.blinkScrollsOwned ?? 0})</span>
				</button>
				<button class="powerup-btn" onclick={useDoubleCoins} disabled={(gameStore.player.doubleCoinsTokensOwned ?? 0) <= 0 || gameStore.player.doubleCoinsActive}>
					🪙
					2× Coins
					<span class="powerup-count">({gameStore.player.doubleCoinsTokensOwned ?? 0})</span>
				</button>
			</div>
			<div class="dpad" role="group" aria-label="Movement controls">
				<button class="dpad-btn dpad-up" onclick={() => queueMove('up')} aria-label="Move up">
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
						<polyline points="18 15 12 9 6 15"/>
					</svg>
				</button>
				<div class="dpad-middle">
					<button class="dpad-btn dpad-left" onclick={() => queueMove('left')} aria-label="Move left">
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
							<polyline points="15 18 9 12 15 6"/>
						</svg>
					</button>
					<div class="dpad-center" aria-hidden="true"></div>
					<button class="dpad-btn dpad-right" onclick={() => queueMove('right')} aria-label="Move right">
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
							<polyline points="9 18 15 12 9 6"/>
						</svg>
					</button>
				</div>
				<button class="dpad-btn dpad-down" onclick={() => queueMove('down')} aria-label="Move down">
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" width="18" height="18" aria-hidden="true">
						<polyline points="6 9 12 15 18 9"/>
					</svg>
				</button>
			</div>
		</div>

		{#if session.shape === 'rectangular' && ghostPositions.length > 1}
			<div class="ghost-banner">
				<span class="ghost-label">Ghost mode</span>
				<span class="ghost-copy">Best run replaying in the background</span>
			</div>
		{/if}
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
		open={showOutro}
		title="Level Complete!"
		subtitle={levelDef.levelKind === 'boss' ? 'Boss encounter cleared' : `World ${worldId} • Level ${levelNumber}`}
		playerName={gameStore.player.playerName}
		time={elapsed}
		moves={session.moves}
		stars={victoryStars.total}
		twoStarMoves={activeTwoStarMoves}
		threeStarTime={activeThreeStarTime}
		fiveStarMoves={activeFiveStarMoves}
		fiveStarTime={activeFiveStarTime}
		coins={coinsEarned}
		accentColor={theme.accentColor}
		rewardSummary={levelDef.levelKind === 'boss'
			? `Boss rewards secured. ${levelDef.levelReward?.label ?? 'A relic'} is now archived if this was your first clear.`
			: 'Stars, coins, and mastery progress have been recorded for this run.'}
		mazeWidth={getSessionGridSize(session).width}
		mazeHeight={getSessionGridSize(session).height}
		algoName={levelDef.levelType}
		algoId={levelDef.levelType}
		algoLinkBase={base}
		bestTime={previousBestTime}
		bestMoves={previousBestMoves}
		actions={[
			{ label: 'Retry', onclick: () => { showOutro = false; startGame(); } },
			{ label: levelDef.connectTo1 ? 'Next Level →' : 'Back to World', primary: true, onclick: goToNextLevel }
		]}
	/>
{/if}

{#if shownMasteryUnlock}
	<MasteryRewardPopup
		unlock={shownMasteryUnlock}
		ondismiss={() => { masteryUnlocks = masteryUnlocks.slice(1); }}
	/>
{:else if shownAchievementId}
	<AchievementUnlockPopup
		achievementId={shownAchievementId}
		ondismiss={() => { newlyUnlocked = newlyUnlocked.slice(1); }}
	/>
{/if}

{#if rewardPopup}
	<MapCollectiblePopup
		collectible={rewardPopup}
		onClose={() => { rewardPopup = null; }}
		onCollect={() => { rewardPopup = null; }}
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

	.boss-badge {
		font-size: var(--text-xs);
		background: rgba(249, 115, 22, 0.22);
		color: #fdba74;
		padding: 1px 7px;
		border-radius: var(--radius-full);
		font-weight: 700;
		border: 1px solid rgba(249, 115, 22, 0.45);
	}

	.hud-stats {
		display: flex;
		gap: var(--space-4);
	}

	.ghost-banner {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		align-self: center;
		padding: 0.45rem 0.8rem;
		margin-top: calc(var(--space-2) * -1);
		border-radius: var(--radius-full);
		background: rgba(148, 163, 184, 0.1);
		border: 1px solid rgba(148, 163, 184, 0.25);
	}

	.ghost-label {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: #e2e8f0;
	}

	.ghost-copy {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
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

	.frozen-badge {
		font-size: 10px;
		font-weight: 700;
		letter-spacing: 0.05em;
		color: #38bdf8;
		background: rgba(56, 189, 248, 0.12);
		border: 1px solid rgba(56, 189, 248, 0.4);
		border-radius: var(--radius-full);
		padding: 1px 6px;
		animation: pulse 1s ease-in-out infinite alternate;
	}

	@keyframes pulse {
		from { opacity: 0.6; }
		to { opacity: 1; }
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
		flex-wrap: wrap;
		max-width: 220px;
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
