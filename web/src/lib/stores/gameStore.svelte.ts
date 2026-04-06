/**
 * Game state store using Svelte 5 runes ($state).
 * Manages all player data with localStorage persistence.
 * Can optionally sync with Supabase when configured.
 */

import { browser } from '$app/environment';
import type { PlayerData, CampaignLevel, DailyMazeLevel } from '$lib/core/types';
import { SKIN_CATALOG } from '$lib/core/skins';
import { getDailyMazeForDate } from '$lib/core/daily';
import { getAllWorlds, getLevelByNumber, type WorldDefinition } from '$lib/core/levels';
import { getSupabaseBrowserClient } from '$lib/supabase/client';

const STORAGE_KEY = 'mazeescape_player';
const LEVEL_STORAGE_KEY = 'mazeescape_levels';
const DAILY_STORAGE_KEY = 'mazeescape_daily';
const SYNC_METADATA_STORAGE_KEY = 'mazeescape_sync_metadata';

type CloudSyncStatus = 'offline' | 'syncing' | 'synced' | 'error';

interface SyncMetadata {
	playerUpdatedAt: number;
	levelUpdatedAt: Record<string, number>;
	dailyUpdatedAt: Record<string, number>;
	skinUpdatedAt: Record<string, number>;
}

interface ProfileRow {
	id: string;
	player_name: string;
	coin_count: number;
	hints_owned: number;
	extra_times_owned: number;
	extra_moves_owned: number;
	current_skin_id: number;
	wall_color: string;
	month_prize1_achieved: boolean;
	month_prize2_achieved: boolean;
	most_recent_month: string;
	updated_at: string;
}

interface LevelProgressRow {
	user_id: string;
	world_id: number;
	level_number: string;
	completed: boolean;
	star1: boolean;
	star2: boolean;
	star3: boolean;
	best_moves: number;
	best_time_seconds: number;
	updated_at: string;
}

interface DailyMazeResultRow {
	user_id: string;
	short_date: string;
	month_year: string;
	status: DailyMazeLevel['status'];
	completion_time: number;
	completion_moves: number;
	updated_at: string;
}

interface OwnedSkinRow {
	user_id: string;
	skin_id: number;
	acquired_at: string;
}

interface WorldProgressRow {
	user_id: string;
	world_id: number;
	locked: boolean;
	star_count: number;
	highest_beaten_level: number;
	highest_area_unlocked: number;
	unlocked_gate_numbers: number[];
}

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

function parseShortDate(shortDate: string): Date {
	const [month, day, year] = shortDate.split('/').map(Number);
	return new Date(year, month - 1, day);
}

function timestampFromIso(iso: string | null | undefined): number {
	if (!iso) return 0;
	const parsed = Date.parse(iso);
	return Number.isNaN(parsed) ? 0 : parsed;
}

function createDefaultSyncMetadata(
	playerData: PlayerData,
	levels: Record<string, CampaignLevel>,
	dailies: Record<string, DailyMazeLevel>
): SyncMetadata {
	const now = Date.now();
	return {
		playerUpdatedAt: now,
		levelUpdatedAt: Object.fromEntries(Object.keys(levels).map((key) => [key, now])),
		dailyUpdatedAt: Object.fromEntries(Object.keys(dailies).map((key) => [key, now])),
		skinUpdatedAt: Object.fromEntries(playerData.unlockedSkinIds.map((skinId) => [String(skinId), now]))
	};
}

function normalizeSyncMetadata(
	metadata: SyncMetadata | null | undefined,
	playerData: PlayerData,
	levels: Record<string, CampaignLevel>,
	dailies: Record<string, DailyMazeLevel>
): SyncMetadata {
	const fallback = createDefaultSyncMetadata(playerData, levels, dailies);
	const nextMetadata: SyncMetadata = {
		playerUpdatedAt: metadata?.playerUpdatedAt ?? fallback.playerUpdatedAt,
		levelUpdatedAt: { ...(metadata?.levelUpdatedAt ?? {}) },
		dailyUpdatedAt: { ...(metadata?.dailyUpdatedAt ?? {}) },
		skinUpdatedAt: { ...(metadata?.skinUpdatedAt ?? {}) }
	};

	for (const key of Object.keys(levels)) {
		if (!(key in nextMetadata.levelUpdatedAt)) {
			nextMetadata.levelUpdatedAt[key] = fallback.levelUpdatedAt[key];
		}
	}

	for (const key of Object.keys(dailies)) {
		if (!(key in nextMetadata.dailyUpdatedAt)) {
			nextMetadata.dailyUpdatedAt[key] = fallback.dailyUpdatedAt[key];
		}
	}

	for (const skinId of playerData.unlockedSkinIds) {
		const key = String(skinId);
		if (!(key in nextMetadata.skinUpdatedAt)) {
			nextMetadata.skinUpdatedAt[key] = fallback.skinUpdatedAt[key];
		}
	}

	return nextMetadata;
}

function getLevelTemplate(worldId: number, levelNumber: string): CampaignLevel | undefined {
	const world = getAllWorlds().find((entry) => entry.worldId === worldId);
	if (!world) return undefined;
	return getLevelByNumber(world, levelNumber);
}

function mapProfileRowToPlayerData(profile: ProfileRow | null, ownedSkins: OwnedSkinRow[]): PlayerData | undefined {
	if (!profile) return undefined;

	return {
		playerId: profile.id,
		playerName: profile.player_name,
		coinCount: profile.coin_count,
		hintsOwned: profile.hints_owned,
		extraTimesOwned: profile.extra_times_owned,
		extraMovesOwned: profile.extra_moves_owned,
		currentWorldIndex: 0,
		currentSkinId: profile.current_skin_id,
		unlockedSkinIds: Array.from(new Set([0, ...ownedSkins.map((row) => row.skin_id)])),
		wallColor: profile.wall_color,
		monthPrize1Achieved: profile.month_prize1_achieved,
		monthPrize2Achieved: profile.month_prize2_achieved,
		mostRecentMonth: profile.most_recent_month
	};
}

function mapLevelRowToCampaignLevel(row: LevelProgressRow): CampaignLevel | undefined {
	const template = getLevelTemplate(row.world_id, row.level_number);
	if (!template) return undefined;

	return {
		...template,
		completed: row.completed,
		star1: row.star1,
		star2: row.star2,
		star3: row.star3,
		numberOfStars: (row.star1 ? 1 : 0) + (row.star2 ? 1 : 0) + (row.star3 ? 1 : 0),
		bestMoves: row.best_moves,
		bestTime: Number(row.best_time_seconds)
	};
}

function mapDailyRowToDailyLevel(row: DailyMazeResultRow): DailyMazeLevel {
	const base = getDailyMazeForDate(parseShortDate(row.short_date));
	return {
		...base,
		status: row.status,
		completionTime: Number(row.completion_time),
		completionMoves: row.completion_moves
	};
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
	let syncMetadata = $state<SyncMetadata>(createDefaultSyncMetadata(player, levelProgress, dailyResults));
	let initialized = $state(false);
	let cloudUserId = $state<string | null>(null);
	let cloudSyncStatus = $state<CloudSyncStatus>('offline');
	let cloudSyncError = $state('');
	let lastSyncedAt = $state<number | null>(null);
	let cloudSyncBusy = $state(false);
	let syncTimer: ReturnType<typeof setTimeout> | null = null;

	function init() {
		if (initialized) return;
		player = loadFromStorage(STORAGE_KEY, defaultPlayerData());
		levelProgress = loadFromStorage(LEVEL_STORAGE_KEY, {});
		dailyResults = loadFromStorage(DAILY_STORAGE_KEY, {});
		syncMetadata = normalizeSyncMetadata(loadFromStorage<SyncMetadata | null>(SYNC_METADATA_STORAGE_KEY, null), player, levelProgress, dailyResults);
		worlds = getAllWorlds();
		initialized = true;
	}

	function save() {
		saveToStorage(STORAGE_KEY, player);
		saveToStorage(LEVEL_STORAGE_KEY, levelProgress);
		saveToStorage(DAILY_STORAGE_KEY, dailyResults);
		saveToStorage(SYNC_METADATA_STORAGE_KEY, syncMetadata);
		scheduleCloudPush();
	}

	function touchPlayer(timestamp = Date.now()) {
		syncMetadata.playerUpdatedAt = timestamp;
	}

	function touchLevel(key: string, timestamp = Date.now()) {
		syncMetadata.levelUpdatedAt[key] = timestamp;
	}

	function touchDaily(key: string, timestamp = Date.now()) {
		syncMetadata.dailyUpdatedAt[key] = timestamp;
	}

	function touchSkin(skinId: number, timestamp = Date.now()) {
		syncMetadata.skinUpdatedAt[String(skinId)] = timestamp;
	}

	function setCloudStatus(status: CloudSyncStatus, error = '') {
		cloudSyncStatus = status;
		cloudSyncError = error;
		if (status === 'synced') {
			lastSyncedAt = Date.now();
		}
	}

	function scheduleCloudPush() {
		if (!browser || !cloudUserId) return;
		if (syncTimer) clearTimeout(syncTimer);
		syncTimer = setTimeout(() => {
			void pushToCloud();
		}, 300);
	}

	function buildWorldProgressRows(userId: string): WorldProgressRow[] {
		return worlds
			.filter((world) => world.levels.length > 0)
			.map((world) => {
				const starCount = getWorldStarCount(world.worldId);
				const completedMainLevels = world.levels
					.filter((level) => !level.levelNumber.includes('b'))
					.filter((level) => getLevelProgress(world.worldId, level.levelNumber)?.completed)
					.map((level) => Number.parseInt(level.levelNumber, 10))
					.filter(Number.isFinite);
				const unlockedGateNumbers = world.gateStarRequired
					.map((required, index) => ({ required, index: index + 1 }))
					.filter((entry) => starCount >= entry.required)
					.map((entry) => entry.index);

				return {
					user_id: userId,
					world_id: world.worldId,
					locked: world.worldId !== 1 && getWorldStarCount(world.worldId - 1) === 0,
					star_count: starCount,
					highest_beaten_level: completedMainLevels.length > 0 ? Math.max(...completedMainLevels) : 0,
					highest_area_unlocked: unlockedGateNumbers.length + 1,
					unlocked_gate_numbers: unlockedGateNumbers
				};
			});
	}

	async function pullFromCloud() {
		if (!cloudUserId) return;

		cloudSyncBusy = true;
		setCloudStatus('syncing');

		try {
			const supabase = getSupabaseBrowserClient();
			const [profileResult, levelResult, dailyResult, skinResult] = await Promise.all([
				supabase.from('profiles').select('*').eq('id', cloudUserId).maybeSingle<ProfileRow>(),
				supabase.from('level_progress').select('*').eq('user_id', cloudUserId).returns<LevelProgressRow[]>(),
				supabase.from('daily_maze_results').select('*').eq('user_id', cloudUserId).returns<DailyMazeResultRow[]>(),
				supabase.from('owned_skins').select('user_id, skin_id').eq('user_id', cloudUserId).returns<OwnedSkinRow[]>()
			]);

			const errors = [profileResult.error, levelResult.error, dailyResult.error, skinResult.error].filter(Boolean);
			if (errors.length > 0) {
				throw new Error(errors[0]?.message ?? 'Failed to load cloud save.');
			}

			const remotePlayer = mapProfileRowToPlayerData(profileResult.data ?? null, skinResult.data ?? []);
			player = mergePlayerData(player, remotePlayer);

			const remoteLevelMap: Record<string, CampaignLevel> = {};
			for (const row of levelResult.data ?? []) {
				const mapped = mapLevelRowToCampaignLevel(row);
				if (mapped) {
					remoteLevelMap[`${row.world_id}:${row.level_number}`] = mapped;
				}
			}

			const mergedLevelKeys = new Set([...Object.keys(levelProgress), ...Object.keys(remoteLevelMap)]);
			const mergedLevelProgress: Record<string, CampaignLevel> = {};
			for (const key of mergedLevelKeys) {
				const [worldIdText, levelNumber] = key.split(':');
				const template = getLevelTemplate(Number(worldIdText), levelNumber);
				if (!template) continue;
				mergedLevelProgress[key] = mergeCampaignLevel(levelProgress[key], remoteLevelMap[key], template);
			}
			levelProgress = mergedLevelProgress;

			const remoteDailyMap: Record<string, DailyMazeLevel> = {};
			for (const row of dailyResult.data ?? []) {
				remoteDailyMap[row.short_date] = mapDailyRowToDailyLevel(row);
			}
			const mergedDailyKeys = new Set([...Object.keys(dailyResults), ...Object.keys(remoteDailyMap)]);
			const mergedDailyResults: Record<string, DailyMazeLevel> = {};
			for (const key of mergedDailyKeys) {
				const remote = remoteDailyMap[key];
				if (!remote) {
					mergedDailyResults[key] = { ...dailyResults[key] };
					continue;
				}
				mergedDailyResults[key] = mergeDailyLevel(dailyResults[key], remote);
			}
			dailyResults = mergedDailyResults;

			saveToStorage(STORAGE_KEY, player);
			saveToStorage(LEVEL_STORAGE_KEY, levelProgress);
			saveToStorage(DAILY_STORAGE_KEY, dailyResults);
			setCloudStatus('synced');
		} catch (error) {
			setCloudStatus('error', error instanceof Error ? error.message : 'Failed to sync from cloud.');
		} finally {
			cloudSyncBusy = false;
		}
	}

	async function pushToCloud() {
		if (!cloudUserId) return;

		cloudSyncBusy = true;
		setCloudStatus('syncing');

		try {
			const supabase = getSupabaseBrowserClient();

			const profileRow: ProfileRow = {
				id: cloudUserId,
				player_name: player.playerName,
				coin_count: player.coinCount,
				hints_owned: player.hintsOwned,
				extra_times_owned: player.extraTimesOwned,
				extra_moves_owned: player.extraMovesOwned,
				current_skin_id: player.currentSkinId,
				wall_color: player.wallColor,
				month_prize1_achieved: player.monthPrize1Achieved,
				month_prize2_achieved: player.monthPrize2Achieved,
				most_recent_month: player.mostRecentMonth
			};

			const levelRows: LevelProgressRow[] = Object.entries(levelProgress).map(([key, level]) => {
				const [worldIdText] = key.split(':');
				return {
					user_id: cloudUserId!,
					world_id: Number(worldIdText),
					level_number: level.levelNumber,
					completed: level.completed,
					star1: level.star1,
					star2: level.star2,
					star3: level.star3,
					best_moves: level.bestMoves,
					best_time_seconds: level.bestTime
				};
			});

			const dailyRows: DailyMazeResultRow[] = Object.values(dailyResults).map((result) => ({
				user_id: cloudUserId!,
				short_date: result.shortDate,
				month_year: result.monthYear,
				status: result.status,
				completion_time: result.completionTime,
				completion_moves: result.completionMoves
			}));

			const skinRows: OwnedSkinRow[] = Array.from(new Set(player.unlockedSkinIds)).map((skinId) => ({
				user_id: cloudUserId!,
				skin_id: skinId
			}));

			const worldRows = buildWorldProgressRows(cloudUserId);

			const [profileUpsert, levelUpsert, dailyUpsert, skinUpsert, worldUpsert] = await Promise.all([
				supabase.from('profiles').upsert(profileRow, { onConflict: 'id' }),
				levelRows.length > 0
					? supabase.from('level_progress').upsert(levelRows, { onConflict: 'user_id,world_id,level_number' })
					: Promise.resolve({ error: null }),
				dailyRows.length > 0
					? supabase.from('daily_maze_results').upsert(dailyRows, { onConflict: 'user_id,short_date' })
					: Promise.resolve({ error: null }),
				skinRows.length > 0
					? supabase.from('owned_skins').upsert(skinRows, { onConflict: 'user_id,skin_id' })
					: Promise.resolve({ error: null }),
				worldRows.length > 0
					? supabase.from('world_progress').upsert(worldRows, { onConflict: 'user_id,world_id' })
					: Promise.resolve({ error: null })
			]);

			const errors = [profileUpsert.error, levelUpsert.error, dailyUpsert.error, skinUpsert.error, worldUpsert.error].filter(Boolean);
			if (errors.length > 0) {
				throw new Error(errors[0]?.message ?? 'Failed to save cloud data.');
			}

			setCloudStatus('synced');
		} catch (error) {
			setCloudStatus('error', error instanceof Error ? error.message : 'Failed to sync to cloud.');
		} finally {
			cloudSyncBusy = false;
		}
	}

	async function handleAuthStateChanged(userId: string | null) {
		if (!initialized) init();
		cloudUserId = userId;
		if (!userId) {
			setCloudStatus('offline');
			return;
		}

		player.playerId = userId;
		await pullFromCloud();
		await pushToCloud();
	}

	async function syncNow() {
		await pushToCloud();
	}

	// --- Player mutations ---

	function addCoins(amount: number) {
		player.coinCount += amount;
		touchPlayer();
		save();
	}

	function spendCoins(amount: number): boolean {
		if (player.coinCount < amount) return false;
		player.coinCount -= amount;
		touchPlayer();
		save();
		return true;
	}

	function setPlayerName(name: string) {
		if (!name.trim()) return;
		player.playerName = name.trim();
		touchPlayer();
		save();
	}

	function setWallColor(color: string) {
		player.wallColor = color;
		touchPlayer();
		save();
	}

	function equipSkin(skinId: number) {
		if (!player.unlockedSkinIds.includes(skinId)) return;
		player.currentSkinId = skinId;
		touchPlayer();
		save();
	}

	function unlockSkin(skinId: number) {
		if (player.unlockedSkinIds.includes(skinId)) return;
		player.unlockedSkinIds.push(skinId);
		touchPlayer();
		touchSkin(skinId);
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
		touchPlayer();
		save();
	}

	function usePowerup(name: 'hint' | 'extraTime' | 'extraMoves'): boolean {
		if (name === 'hint' && player.hintsOwned > 0) {
			player.hintsOwned--;
			touchPlayer();
			save();
			return true;
		}
		if (name === 'extraTime' && player.extraTimesOwned > 0) {
			player.extraTimesOwned--;
			touchPlayer();
			save();
			return true;
		}
		if (name === 'extraMoves' && player.extraMovesOwned > 0) {
			player.extraMovesOwned--;
			touchPlayer();
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
		touchLevel(key);
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
		touchDaily(result.shortDate);
		save();
	}

	function isDailyMazeUnlocked(): boolean {
		const w1l10 = getLevelProgress(1, '10');
		return w1l10?.star1 === true;
	}

	function getCloudSyncStatusLabel(): string {
		if (cloudSyncStatus === 'offline') return cloudUserId ? 'Offline' : 'Local only';
		if (cloudSyncStatus === 'syncing') return 'Syncing...';
		if (cloudSyncStatus === 'synced') return 'Synced';
		return 'Sync error';
	}

	function getLastSyncedAtLabel(): string {
		if (!lastSyncedAt) return '';
		return new Date(lastSyncedAt).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
	}

	return {
		get player() { return player; },
		get worlds() { return worlds; },
		get levelProgress() { return levelProgress; },
		get dailyResults() { return dailyResults; },
		get initialized() { return initialized; },
		get cloudUserId() { return cloudUserId; },
		get cloudSyncError() { return cloudSyncError; },
		get cloudSyncBusy() { return cloudSyncBusy; },
		get cloudSyncStatusLabel() { return getCloudSyncStatusLabel(); },
		get lastSyncedAtLabel() { return getLastSyncedAtLabel(); },

		init,
		save,
		handleAuthStateChanged,
		syncNow,
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
