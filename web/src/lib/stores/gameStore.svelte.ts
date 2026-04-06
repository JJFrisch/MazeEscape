/**
 * Game state store using Svelte 5 runes ($state).
 * Manages all player data with localStorage persistence.
 * Can optionally sync with Supabase when configured.
 */

import type { PlayerData, CampaignLevel, DailyMazeLevel } from '$lib/core/types';
import { SKIN_CATALOG } from '$lib/core/skins';
import { getAllWorlds, type WorldDefinition } from '$lib/core/levels';

const STORAGE_KEY = 'mazeescape_player';
const LEVEL_STORAGE_KEY = 'mazeescape_levels';
const DAILY_STORAGE_KEY = 'mazeescape_daily';

// ---------------------------------------------------------------------------
// Default state
// ---------------------------------------------------------------------------

function defaultPlayerData(): PlayerData {
	return {
		playerId: crypto.randomUUID(),
		playerName: 'Player',
		coinCount: 0,
		hintsOwned: 0,
		extraTimesOwned: 0,
		extraMovesOwned: 0,
		currentWorldIndex: 0,
		currentSkinId: 0,
		unlockedSkinIds: [0], // Default skin is always unlocked
		wallColor: '#000000',
		monthPrize1Achieved: false,
		monthPrize2Achieved: false,
		mostRecentMonth: ''
	};
}

// ---------------------------------------------------------------------------
// Persistence helpers
// ---------------------------------------------------------------------------

function loadFromStorage<T>(key: string, fallback: T): T {
	if (typeof window === 'undefined') return fallback;
	try {
		const raw = localStorage.getItem(key);
		if (!raw) return fallback;
		return JSON.parse(raw) as T;
	} catch {
		return fallback;
	}
}

function saveToStorage(key: string, value: unknown): void {
	if (typeof window === 'undefined') return;
	try {
		localStorage.setItem(key, JSON.stringify(value));
	} catch {
		// Ignore quota errors
	}
}

// ---------------------------------------------------------------------------
// Game store (Svelte 5 module-level state)
// ---------------------------------------------------------------------------

export const gameStore = createGameStore();

function createGameStore() {
	let player = $state<PlayerData>(defaultPlayerData());
	let worlds = $state<WorldDefinition[]>(getAllWorlds());
	let levelProgress = $state<Record<string, CampaignLevel>>({});
	let dailyResults = $state<Record<string, DailyMazeLevel>>({});
	let initialized = $state(false);

	function init() {
		if (initialized) return;
		player = loadFromStorage(STORAGE_KEY, defaultPlayerData());
		levelProgress = loadFromStorage(LEVEL_STORAGE_KEY, {});
		dailyResults = loadFromStorage(DAILY_STORAGE_KEY, {});
		worlds = getAllWorlds();
		initialized = true;
	}

	function save() {
		saveToStorage(STORAGE_KEY, player);
		saveToStorage(LEVEL_STORAGE_KEY, levelProgress);
		saveToStorage(DAILY_STORAGE_KEY, dailyResults);
	}

	// --- Player mutations ---

	function addCoins(amount: number) {
		player.coinCount += amount;
		save();
	}

	function spendCoins(amount: number): boolean {
		if (player.coinCount < amount) return false;
		player.coinCount -= amount;
		save();
		return true;
	}

	function setPlayerName(name: string) {
		if (!name.trim()) return;
		player.playerName = name.trim();
		save();
	}

	function setWallColor(color: string) {
		player.wallColor = color;
		save();
	}

	function equipSkin(skinId: number) {
		if (!player.unlockedSkinIds.includes(skinId)) return;
		player.currentSkinId = skinId;
		save();
	}

	function unlockSkin(skinId: number) {
		if (player.unlockedSkinIds.includes(skinId)) return;
		player.unlockedSkinIds.push(skinId);
		save();
	}

	function buySkin(skinId: number): boolean {
		const skin = SKIN_CATALOG.find((s) => s.id === skinId);
		if (!skin || skin.isSpecialSkin) return false;
		if (player.unlockedSkinIds.includes(skinId)) return false;
		if (!spendCoins(skin.coinPrice)) return false;
		unlockSkin(skinId);
		return true;
	}

	function addPowerup(name: 'hint' | 'extraTime' | 'extraMoves') {
		if (name === 'hint') player.hintsOwned++;
		else if (name === 'extraTime') player.extraTimesOwned++;
		else if (name === 'extraMoves') player.extraMovesOwned++;
		save();
	}

	function usePowerup(name: 'hint' | 'extraTime' | 'extraMoves'): boolean {
		if (name === 'hint' && player.hintsOwned > 0) {
			player.hintsOwned--;
			save();
			return true;
		}
		if (name === 'extraTime' && player.extraTimesOwned > 0) {
			player.extraTimesOwned--;
			save();
			return true;
		}
		if (name === 'extraMoves' && player.extraMovesOwned > 0) {
			player.extraMovesOwned--;
			save();
			return true;
		}
		return false;
	}

	// --- Level progress ---

	function getLevelProgress(worldId: number, levelNumber: string): CampaignLevel | undefined {
		return levelProgress[`${worldId}:${levelNumber}`];
	}

	function saveLevelProgress(worldId: number, level: CampaignLevel) {
		const key = `${worldId}:${level.levelNumber}`;
		const existing = levelProgress[key];

		if (existing) {
			// Only improve — never overwrite with worse stats
			level.star1 = level.star1 || existing.star1;
			level.star2 = level.star2 || existing.star2;
			level.star3 = level.star3 || existing.star3;
			level.numberOfStars = (level.star1 ? 1 : 0) + (level.star2 ? 1 : 0) + (level.star3 ? 1 : 0);
			level.bestMoves = existing.bestMoves > 0 ? Math.min(level.bestMoves || Infinity, existing.bestMoves) : level.bestMoves;
			level.bestTime = existing.bestTime > 0 ? Math.min(level.bestTime || Infinity, existing.bestTime) : level.bestTime;
			level.completed = level.completed || existing.completed;
		}

		levelProgress[key] = { ...level };
		save();
	}

	function getWorldStarCount(worldId: number): number {
		let total = 0;
		for (const [key, level] of Object.entries(levelProgress)) {
			if (key.startsWith(`${worldId}:`)) {
				total += level.numberOfStars;
			}
		}
		return total;
	}

	// --- Daily maze ---

	function getDailyResult(shortDate: string): DailyMazeLevel | undefined {
		return dailyResults[shortDate];
	}

	function saveDailyResult(result: DailyMazeLevel) {
		dailyResults[result.shortDate] = { ...result };
		save();
	}

	function isDailyMazeUnlocked(): boolean {
		const w1l10 = getLevelProgress(1, '10');
		return w1l10?.star1 === true;
	}

	return {
		get player() { return player; },
		get worlds() { return worlds; },
		get levelProgress() { return levelProgress; },
		get dailyResults() { return dailyResults; },
		get initialized() { return initialized; },

		init,
		save,
		addCoins,
		spendCoins,
		setPlayerName,
		setWallColor,
		equipSkin,
		unlockSkin,
		buySkin,
		addPowerup,
		usePowerup,
		getLevelProgress,
		saveLevelProgress,
		getWorldStarCount,
		getDailyResult,
		saveDailyResult,
		isDailyMazeUnlocked
	};
}
