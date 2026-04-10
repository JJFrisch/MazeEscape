/**
 * Game state store using Svelte 5 runes ($state).
 * Manages all player data with localStorage persistence.
 * Can optionally sync with Supabase when configured.
 */

import { browser } from '$app/environment';
import type { PlayerData, CampaignLevel, DailyMazeLevel, MazeAlgorithm, MasteryRewardClaimState, PowerupName, Direction, GhostRunData, EventProgress } from '$lib/core/types';
import { getActiveEvent, getEventPowerupCatalog } from '$lib/core/events';
import { ACHIEVEMENT_CATALOG } from '$lib/core/achievements';
import { SKIN_CATALOG } from '$lib/core/skins';
import { getDeityMasteryRewards, type MasteryRewardUnlock } from '$lib/core/deities';
import { getDailyMazeForDate } from '$lib/core/daily';
import { getAllWorlds, getLevelByNumber, type WorldDefinition } from '$lib/core/levels';
import { getSupabaseBrowserClient } from '$lib/supabase/client';

const STORAGE_KEY = 'mazeescape_player';
const LEVEL_STORAGE_KEY = 'mazeescape_levels';
const DAILY_STORAGE_KEY = 'mazeescape_daily';
const GHOST_STORAGE_KEY = 'mazeescape_level_ghosts';
const EVENT_PROGRESS_STORAGE_KEY = 'mazeescape_event_progress';
const SYNC_METADATA_STORAGE_KEY = 'mazeescape_sync_metadata';

type CloudSyncStatus = 'offline' | 'syncing' | 'synced' | 'error';

interface SyncMetadata {
	playerUpdatedAt: number;
	levelUpdatedAt: Record<string, number>;
	dailyUpdatedAt: Record<string, number>;
	skinUpdatedAt: Record<string, number>;
	eventUpdatedAt: Record<string, number>;
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
	special_item_ids?: string[];
	latest_special_item_id?: string | null;
	updated_at?: string;
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
	best_run_moves?: Direction[] | null;
	updated_at?: string;
}

interface EventProgressRow {
	user_id: string;
	event_id: string;
	progress_value: number;
	completed_milestones: number[];
	updated_at?: string;
}

interface DailyMazeResultRow {
	user_id: string;
	short_date: string;
	month_year: string;
	status: DailyMazeLevel['status'];
	completion_time: number;
	completion_moves: number;
	updated_at?: string;
}

interface OwnedSkinRow {
	user_id: string;
	skin_id: number;
	acquired_at?: string;
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
		gemCount: 0,
		hintsOwned: 0,
		extraTimesOwned: 0,
		extraMovesOwned: 0,
		// New consumables
		compassOwned: 0,
		hourglassOwned: 0,
		blinkScrollsOwned: 0,
		streakShieldsOwned: 0,
		doubleCoinsTokensOwned: 0,
		doubleCoinsActive: false,
		currentWorldIndex: 0,
		currentSkinId: 0,
		unlockedSkinIds: [0], // Default skin is always unlocked
		wallColor: '#000000',
		monthPrize1Achieved: false,
		monthPrize2Achieved: false,
		mostRecentMonth: '',
		// Streak
		currentStreak: 0,
		bestStreak: 0,
		lastDailyDate: '',
		// Mastery
		algoMasteryCount: {},
		masteryRewardsClaimed: {},
		// Lifetime
		coinsEarnedLifetime: 0,
		// Achievements
		achievements: {},
		// Crafting
		crystalShards: 0,
		// Total mazes completed
		mazesCompleted: 0,
		specialItemIds: [],
		latestSpecialItemId: null,
	};
}

function normalizePlayerData(value: Partial<PlayerData> | null | undefined): PlayerData {
	const defaults = defaultPlayerData();
	if (!value) return defaults;

	return {
		...defaults,
		...value,
		unlockedSkinIds: Array.from(new Set([0, ...(value.unlockedSkinIds ?? defaults.unlockedSkinIds)])).sort((a, b) => a - b),
		specialItemIds: Array.from(new Set(value.specialItemIds ?? defaults.specialItemIds)).sort(),
		latestSpecialItemId: value.latestSpecialItemId ?? defaults.latestSpecialItemId,
		algoMasteryCount: value.algoMasteryCount ?? defaults.algoMasteryCount,
		masteryRewardsClaimed: value.masteryRewardsClaimed ?? defaults.masteryRewardsClaimed,
		achievements: value.achievements ?? defaults.achievements
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
		skinUpdatedAt: Object.fromEntries(playerData.unlockedSkinIds.map((skinId) => [String(skinId), now])),
		eventUpdatedAt: {}
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
		skinUpdatedAt: { ...(metadata?.skinUpdatedAt ?? {}) },
		eventUpdatedAt: { ...(metadata?.eventUpdatedAt ?? {}) }
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

function mapProfileRowToPlayerData(
	profile: ProfileRow | null,
	ownedSkins: OwnedSkinRow[],
	currentPlayer?: PlayerData
): PlayerData | undefined {
	if (!profile) return undefined;

	const fallback = normalizePlayerData(currentPlayer);

	return {
		...fallback,
		playerId: profile.id,
		playerName: profile.player_name,
		coinCount: profile.coin_count,
		hintsOwned: profile.hints_owned,
		extraTimesOwned: profile.extra_times_owned,
		extraMovesOwned: profile.extra_moves_owned,
		currentSkinId: profile.current_skin_id,
		unlockedSkinIds: Array.from(new Set([0, ...fallback.unlockedSkinIds, ...ownedSkins.map((row) => row.skin_id)])).sort((a, b) => a - b),
		wallColor: profile.wall_color,
		monthPrize1Achieved: profile.month_prize1_achieved,
		monthPrize2Achieved: profile.month_prize2_achieved,
		mostRecentMonth: profile.most_recent_month,
		specialItemIds: Array.from(new Set(profile.special_item_ids ?? fallback.specialItemIds)).sort(),
		latestSpecialItemId: profile.latest_special_item_id ?? fallback.latestSpecialItemId
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

function mapEventRowToEventProgress(row: EventProgressRow): EventProgress {
	return {
		eventId: row.event_id,
		progress: row.progress_value,
		completedMilestones: [...(row.completed_milestones ?? [])],
		updatedAt: timestampFromIso(row.updated_at)
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
	let ghostRuns = $state<Record<string, GhostRunData>>({});
	let eventProgress = $state<Record<string, EventProgress>>({});
	let syncMetadata = $state<SyncMetadata>({
		playerUpdatedAt: 0,
		levelUpdatedAt: {},
		dailyUpdatedAt: {},
		skinUpdatedAt: {},
		eventUpdatedAt: {}
	});
	let initialized = $state(false);
	let cloudUserId = $state<string | null>(null);
	let cloudSyncStatus = $state<CloudSyncStatus>('offline');
	let cloudSyncError = $state('');
	let lastSyncedAt = $state<number | null>(null);
	let cloudSyncBusy = $state(false);
	let syncTimer: ReturnType<typeof setTimeout> | null = null;

	function init() {
		if (initialized) return;
		player = normalizePlayerData(loadFromStorage<Partial<PlayerData>>(STORAGE_KEY, defaultPlayerData()));
		levelProgress = loadFromStorage(LEVEL_STORAGE_KEY, {});
		dailyResults = loadFromStorage(DAILY_STORAGE_KEY, {});
		ghostRuns = loadFromStorage(GHOST_STORAGE_KEY, {});
		eventProgress = loadFromStorage(EVENT_PROGRESS_STORAGE_KEY, {});
		const retroactiveMasteryRewards = reconcileAllMasteryRewards(false);
		syncMetadata = normalizeSyncMetadata(loadFromStorage<SyncMetadata | null>(SYNC_METADATA_STORAGE_KEY, null), player, levelProgress, dailyResults);
		worlds = getAllWorlds();
		initialized = true;
		if (retroactiveMasteryRewards.length > 0) {
			save();
		}
	}

	function save() {
		saveToStorage(STORAGE_KEY, player);
		saveToStorage(LEVEL_STORAGE_KEY, levelProgress);
		saveToStorage(DAILY_STORAGE_KEY, dailyResults);
		saveToStorage(GHOST_STORAGE_KEY, ghostRuns);
		saveToStorage(EVENT_PROGRESS_STORAGE_KEY, eventProgress);
		saveToStorage(SYNC_METADATA_STORAGE_KEY, syncMetadata);
		scheduleCloudPush();
	}

	function touchPlayer(timestamp = Date.now()) {
		syncMetadata.playerUpdatedAt = timestamp;
	}

	function touchLevel(key: string, timestamp = Date.now()) {
		syncMetadata.levelUpdatedAt[key] = timestamp;
	}

	function touchEvent(eventId: string, timestamp = Date.now()) {
		syncMetadata.eventUpdatedAt[eventId] = timestamp;
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
			const [profileResult, levelResult, dailyResult, skinResult, eventResult] = await Promise.all([
				supabase.from('profiles').select('*').eq('id', cloudUserId).maybeSingle<ProfileRow>(),
				supabase.from('level_progress').select('*').eq('user_id', cloudUserId).returns<LevelProgressRow[]>(),
				supabase.from('daily_maze_results').select('*').eq('user_id', cloudUserId).returns<DailyMazeResultRow[]>(),
				supabase.from('owned_skins').select('user_id, skin_id, acquired_at').eq('user_id', cloudUserId).returns<OwnedSkinRow[]>(),
				supabase.from('event_progress').select('*').eq('user_id', cloudUserId).returns<EventProgressRow[]>()
			]);

			const errors = [profileResult.error, levelResult.error, dailyResult.error, skinResult.error, eventResult.error].filter(Boolean);
			if (errors.length > 0) {
				throw new Error(errors[0]?.message ?? 'Failed to load cloud save.');
			}

			const remoteProfileTimestamp = timestampFromIso(profileResult.data?.updated_at);
			const remotePlayer = mapProfileRowToPlayerData(profileResult.data ?? null, skinResult.data ?? [], player);
			if (remotePlayer && remoteProfileTimestamp > syncMetadata.playerUpdatedAt) {
				player = normalizePlayerData(remotePlayer);
				syncMetadata.playerUpdatedAt = remoteProfileTimestamp;
			} else {
				player = { ...player, playerId: cloudUserId };
			}

			const skinIds = new Set<number>([0, ...player.unlockedSkinIds, ...(skinResult.data ?? []).map((row) => row.skin_id)]);
			for (const skinRow of skinResult.data ?? []) {
				const skinKey = String(skinRow.skin_id);
				const remoteSkinTimestamp = timestampFromIso(skinRow.acquired_at);
				const localSkinTimestamp = syncMetadata.skinUpdatedAt[skinKey] ?? 0;
				syncMetadata.skinUpdatedAt[skinKey] = Math.max(localSkinTimestamp, remoteSkinTimestamp);
			}
			for (const skinId of skinIds) {
				if (!(String(skinId) in syncMetadata.skinUpdatedAt)) {
					touchSkin(skinId, syncMetadata.playerUpdatedAt || Date.now());
				}
			}
			player.unlockedSkinIds = Array.from(skinIds).sort((a, b) => a - b);
			if (!player.unlockedSkinIds.includes(player.currentSkinId)) {
				player.currentSkinId = 0;
			}

			const remoteLevelMap: Record<string, CampaignLevel> = {};
			const remoteLevelTimestamps: Record<string, number> = {};
			for (const row of levelResult.data ?? []) {
				const mapped = mapLevelRowToCampaignLevel(row);
				if (mapped) {
					const key = `${row.world_id}:${row.level_number}`;
					remoteLevelMap[key] = mapped;
					if (row.best_run_moves?.length) {
						ghostRuns[key] = {
							moves: [...row.best_run_moves],
							updatedAt: timestampFromIso(row.updated_at)
						};
					}
					remoteLevelTimestamps[key] = timestampFromIso(row.updated_at);
				}
			}

			const mergedLevelKeys = new Set([...Object.keys(levelProgress), ...Object.keys(remoteLevelMap)]);
			const mergedLevelProgress: Record<string, CampaignLevel> = {};
			for (const key of mergedLevelKeys) {
				const localLevel = levelProgress[key];
				const remoteLevel = remoteLevelMap[key];
				const localTimestamp = syncMetadata.levelUpdatedAt[key] ?? 0;
				const remoteTimestamp = remoteLevelTimestamps[key] ?? 0;

				if (remoteLevel && (!localLevel || remoteTimestamp > localTimestamp)) {
					mergedLevelProgress[key] = { ...remoteLevel };
					syncMetadata.levelUpdatedAt[key] = remoteTimestamp;
				} else if (localLevel) {
					mergedLevelProgress[key] = { ...localLevel };
					syncMetadata.levelUpdatedAt[key] = localTimestamp || Date.now();
				}
			}
			levelProgress = mergedLevelProgress;

			const remoteDailyMap: Record<string, DailyMazeLevel> = {};
			const remoteDailyTimestamps: Record<string, number> = {};
			for (const row of dailyResult.data ?? []) {
				remoteDailyMap[row.short_date] = mapDailyRowToDailyLevel(row);
				remoteDailyTimestamps[row.short_date] = timestampFromIso(row.updated_at);
			}
			const mergedDailyKeys = new Set([...Object.keys(dailyResults), ...Object.keys(remoteDailyMap)]);
			const mergedDailyResults: Record<string, DailyMazeLevel> = {};
			for (const key of mergedDailyKeys) {
				const localDaily = dailyResults[key];
				const remoteDaily = remoteDailyMap[key];
				const localTimestamp = syncMetadata.dailyUpdatedAt[key] ?? 0;
				const remoteTimestamp = remoteDailyTimestamps[key] ?? 0;

				if (remoteDaily && (!localDaily || remoteTimestamp > localTimestamp)) {
					mergedDailyResults[key] = { ...remoteDaily };
					syncMetadata.dailyUpdatedAt[key] = remoteTimestamp;
				} else if (localDaily) {
					mergedDailyResults[key] = { ...localDaily };
					syncMetadata.dailyUpdatedAt[key] = localTimestamp || Date.now();
				}
			}
			dailyResults = mergedDailyResults;

			const remoteEventMap: Record<string, EventProgress> = {};
			for (const row of eventResult.data ?? []) {
				const mapped = mapEventRowToEventProgress(row);
				remoteEventMap[row.event_id] = mapped;
			}
			const mergedEventKeys = new Set([...Object.keys(eventProgress), ...Object.keys(remoteEventMap)]);
			const mergedEventProgress: Record<string, EventProgress> = {};
			for (const key of mergedEventKeys) {
				const localEvent = eventProgress[key];
				const remoteEvent = remoteEventMap[key];
				const localTimestamp = syncMetadata.eventUpdatedAt[key] ?? localEvent?.updatedAt ?? 0;
				const remoteTimestamp = remoteEvent?.updatedAt ?? 0;

				if (remoteEvent && (!localEvent || remoteTimestamp > localTimestamp)) {
					mergedEventProgress[key] = { ...remoteEvent };
					syncMetadata.eventUpdatedAt[key] = remoteTimestamp;
				} else if (localEvent) {
					mergedEventProgress[key] = { ...localEvent };
					syncMetadata.eventUpdatedAt[key] = localTimestamp || Date.now();
				}
			}
			eventProgress = mergedEventProgress;

			saveToStorage(STORAGE_KEY, player);
			saveToStorage(LEVEL_STORAGE_KEY, levelProgress);
			saveToStorage(DAILY_STORAGE_KEY, dailyResults);
			saveToStorage(GHOST_STORAGE_KEY, ghostRuns);
			saveToStorage(EVENT_PROGRESS_STORAGE_KEY, eventProgress);
			saveToStorage(SYNC_METADATA_STORAGE_KEY, syncMetadata);
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
				most_recent_month: player.mostRecentMonth,
				special_item_ids: [...player.specialItemIds],
				latest_special_item_id: player.latestSpecialItemId
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
					best_time_seconds: level.bestTime,
					best_run_moves: ghostRuns[key]?.moves ?? null
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

			const eventRows: EventProgressRow[] = Object.values(eventProgress).map((entry) => ({
				user_id: cloudUserId!,
				event_id: entry.eventId,
				progress_value: entry.progress,
				completed_milestones: [...entry.completedMilestones]
			}));

			const worldRows = buildWorldProgressRows(cloudUserId);

			const [profileUpsert, levelUpsert, dailyUpsert, skinUpsert, worldUpsert, eventUpsert] = await Promise.all([
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
					: Promise.resolve({ error: null }),
				eventRows.length > 0
					? supabase.from('event_progress').upsert(eventRows, { onConflict: 'user_id,event_id' })
					: Promise.resolve({ error: null })
			]);

			const errors = [profileUpsert.error, levelUpsert.error, dailyUpsert.error, skinUpsert.error, worldUpsert.error, eventUpsert.error].filter(Boolean);
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
		await pullFromCloud();
		await pushToCloud();
	}

	// --- Player mutations ---

	function addCoins(amount: number) {
		// Double-coin token: consume and double the reward
		const effective = player.doubleCoinsActive ? amount * 2 : amount;
		if (player.doubleCoinsActive) player.doubleCoinsActive = false;
		player.coinCount += effective;
		player.coinsEarnedLifetime = (player.coinsEarnedLifetime ?? 0) + effective;
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

	function addPowerup(name: import('$lib/core/types').PowerupName) {
		if (name === 'hint') player.hintsOwned++;
		else if (name === 'extraTime') player.extraTimesOwned++;
		else if (name === 'extraMoves') player.extraMovesOwned++;
		else if (name === 'compass') player.compassOwned = (player.compassOwned ?? 0) + 1;
		else if (name === 'hourglass') player.hourglassOwned = (player.hourglassOwned ?? 0) + 1;
		else if (name === 'blinkScroll') player.blinkScrollsOwned = (player.blinkScrollsOwned ?? 0) + 1;
		else if (name === 'streakShield') player.streakShieldsOwned = (player.streakShieldsOwned ?? 0) + 1;
		else if (name === 'doubleCoinsToken') player.doubleCoinsTokensOwned = (player.doubleCoinsTokensOwned ?? 0) + 1;
		touchPlayer();
		save();
	}

	function awardSpecialItem(specialItemId: string): boolean {
		if (player.specialItemIds.includes(specialItemId)) return false;
		player.specialItemIds = [...player.specialItemIds, specialItemId].sort();
		player.latestSpecialItemId = specialItemId;
		touchPlayer();
		save();
		return true;
	}

	function grantPowerupDirect(name: PowerupName, amount = 1) {
		for (let count = 0; count < amount; count += 1) {
			if (name === 'hint') player.hintsOwned++;
			else if (name === 'extraTime') player.extraTimesOwned++;
			else if (name === 'extraMoves') player.extraMovesOwned++;
			else if (name === 'compass') player.compassOwned = (player.compassOwned ?? 0) + 1;
			else if (name === 'hourglass') player.hourglassOwned = (player.hourglassOwned ?? 0) + 1;
			else if (name === 'blinkScroll') player.blinkScrollsOwned = (player.blinkScrollsOwned ?? 0) + 1;
			else if (name === 'streakShield') player.streakShieldsOwned = (player.streakShieldsOwned ?? 0) + 1;
			else if (name === 'doubleCoinsToken') player.doubleCoinsTokensOwned = (player.doubleCoinsTokensOwned ?? 0) + 1;
		}
	}

	function grantCoinsDirect(amount: number) {
		player.coinCount += amount;
		player.coinsEarnedLifetime = (player.coinsEarnedLifetime ?? 0) + amount;
	}

	function ensureMasteryClaimState(algorithm: MazeAlgorithm): MasteryRewardClaimState {
		if (!player.masteryRewardsClaimed) player.masteryRewardsClaimed = {};
		const existing = player.masteryRewardsClaimed[algorithm];
		if (existing) return existing;
		const fresh: MasteryRewardClaimState = { item20: false, coins80: false, skin120: false };
		player.masteryRewardsClaimed[algorithm] = fresh;
		return fresh;
	}

	function collectPendingMasteryRewards(algorithm: MazeAlgorithm): MasteryRewardUnlock[] {
		const masteryCount = player.algoMasteryCount?.[algorithm] ?? 0;
		const claims = ensureMasteryClaimState(algorithm);
		const rewardDef = getDeityMasteryRewards(algorithm);
		const unlocks: MasteryRewardUnlock[] = [];

		if (masteryCount >= 20 && !claims.item20) {
			grantPowerupDirect(rewardDef.item20.powerup, rewardDef.item20.amount);
			claims.item20 = true;
			unlocks.push({
				algorithm,
				milestone: 20,
				rewardType: 'powerup',
				powerupName: rewardDef.item20.powerup,
				amount: rewardDef.item20.amount
			});
		}

		if (masteryCount >= 80 && !claims.coins80) {
			grantCoinsDirect(rewardDef.coins80);
			claims.coins80 = true;
			unlocks.push({
				algorithm,
				milestone: 80,
				rewardType: 'coins',
				coinAmount: rewardDef.coins80
			});
		}

		if (masteryCount >= 120 && !claims.skin120) {
			if (!player.unlockedSkinIds.includes(rewardDef.skin120Id)) {
				player.unlockedSkinIds.push(rewardDef.skin120Id);
				touchSkin(rewardDef.skin120Id);
			}
			claims.skin120 = true;
			unlocks.push({
				algorithm,
				milestone: 120,
				rewardType: 'skin',
				skinId: rewardDef.skin120Id,
				skinName: rewardDef.skin120Name
			});
		}

		return unlocks;
	}

	function reconcileAllMasteryRewards(saveAfter = true): MasteryRewardUnlock[] {
		const algorithms = Object.keys(player.algoMasteryCount ?? {}) as MazeAlgorithm[];
		const unlocks = algorithms.flatMap((algorithm) => collectPendingMasteryRewards(algorithm));
		if (unlocks.length > 0) {
			touchPlayer();
			if (saveAfter) save();
		}
		return unlocks;
	}

	function usePowerup(name: import('$lib/core/types').PowerupName): boolean {
		if (name === 'hint' && player.hintsOwned > 0) {
			player.hintsOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'extraTime' && player.extraTimesOwned > 0) {
			player.extraTimesOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'extraMoves' && player.extraMovesOwned > 0) {
			player.extraMovesOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'compass' && (player.compassOwned ?? 0) > 0) {
			player.compassOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'hourglass' && (player.hourglassOwned ?? 0) > 0) {
			player.hourglassOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'blinkScroll' && (player.blinkScrollsOwned ?? 0) > 0) {
			player.blinkScrollsOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'streakShield' && (player.streakShieldsOwned ?? 0) > 0) {
			player.streakShieldsOwned--;
			touchPlayer(); save(); return true;
		}
		if (name === 'doubleCoinsToken' && (player.doubleCoinsTokensOwned ?? 0) > 0) {
			player.doubleCoinsTokensOwned--;
			player.doubleCoinsActive = true;
			touchPlayer(); save(); return true;
		}
		return false;
	}

	// --- Key inventory (for key-gate collectibles) ---
	function hasKey(keyItemId: string): boolean {
		const store = loadFromStorage<Record<string, boolean>>('mazeescape_keys', {});
		return !!store[keyItemId];
	}
	function addKey(keyItemId: string) {
		const store = loadFromStorage<Record<string, boolean>>('mazeescape_keys', {});
		store[keyItemId] = true;
		saveToStorage('mazeescape_keys', store);
	}

	// --- Map collectibles (campaign map chest/gem/etc collection state) ---
	const MAP_ITEMS_KEY = 'mazeescape_map_items';
	function _getMapItems(): Record<string, boolean> {
		if (typeof window === 'undefined') return {};
		try { return JSON.parse(localStorage.getItem(MAP_ITEMS_KEY) ?? '{}'); } catch { return {}; }
	}
	function isMapItemCollected(worldId: number, itemId: string): boolean {
		return !!_getMapItems()[`${worldId}:${itemId}`];
	}
	function collectMapItem(worldId: number, itemId: string) {
		const items = _getMapItems();
		items[`${worldId}:${itemId}`] = true;
		if (typeof window !== 'undefined') {
			try { localStorage.setItem(MAP_ITEMS_KEY, JSON.stringify(items)); } catch { /* ignore */ }
		}
	}

	// --- World area progression ---
	function getHighestAreaUnlocked(worldId: number): number {
		const world = worlds.find(w => w.worldId === worldId);
		if (!world) return 1;
		const stars = getWorldStarCount(worldId);
		let highest = 1;
		for (let i = 0; i < world.gateStarRequired.length; i++) {
			if (stars >= world.gateStarRequired[i]) highest = i + 2;
		}
		return highest;
	}

	/** Track algorithm mastery whenever a maze is completed */
	function recordAlgoMastery(algorithm: MazeAlgorithm): MasteryRewardUnlock[] {
		if (!player.algoMasteryCount) player.algoMasteryCount = {};
		player.algoMasteryCount[algorithm] = (player.algoMasteryCount[algorithm] ?? 0) + 1;
		const unlocks = collectPendingMasteryRewards(algorithm);
		touchPlayer();
		save();
		return unlocks;
	}

	// --- Level progress ---

	function getLevelProgress(worldId: number, levelNumber: string): CampaignLevel | undefined {
		return levelProgress[`${worldId}:${levelNumber}`];
	}

	function getGhostRun(worldId: number, levelNumber: string): GhostRunData | undefined {
		return ghostRuns[`${worldId}:${levelNumber}`];
	}

	function shouldReplaceBestRun(existing: CampaignLevel | undefined, moves: number, time: number): boolean {
		if (!existing || existing.bestMoves <= 0) return true;
		if (moves < existing.bestMoves) return true;
		if (moves === existing.bestMoves) {
			if (existing.bestTime <= 0) return true;
			return time < existing.bestTime;
		}
		return false;
	}

	function saveLevelProgress(worldId: number, level: CampaignLevel, bestRunMoves?: Direction[]) {
		const key = `${worldId}:${level.levelNumber}`;
		const existing = levelProgress[key];
		const replaceBestRun = bestRunMoves && shouldReplaceBestRun(existing, level.bestMoves, level.bestTime);

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
		if (replaceBestRun) {
			ghostRuns[key] = { moves: [...bestRunMoves], updatedAt: Date.now() };
		}
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
		// Only update streak if this is a new completion (not a retry of already-completed)
		const existing = dailyResults[result.shortDate];
		const isFirstCompletion = !existing || existing.status !== 'completed';

		dailyResults[result.shortDate] = { ...result };
		touchDaily(result.shortDate);

		if (isFirstCompletion && result.status === 'completed') {
			_updateStreak(result.shortDate);
		}

		save();
	}

	function getEventProgress(eventId: string): EventProgress | undefined {
		return eventProgress[eventId];
	}

	function incrementEventProgress(eventId: string, amount = 1) {
		const existing = eventProgress[eventId] ?? { eventId, progress: 0, completedMilestones: [] };
		const nextProgress = existing.progress + amount;
		const activeEvent = getActiveEvent();
		const milestones = activeEvent?.id === eventId
			? activeEvent.milestones.filter((milestone) => nextProgress >= milestone.at).map((milestone) => milestone.at)
			: existing.completedMilestones;
		eventProgress[eventId] = {
			eventId,
			progress: nextProgress,
			completedMilestones: milestones,
			updatedAt: Date.now()
		};
		touchEvent(eventId);
		save();
	}

	function _updateStreak(shortDate: string) {
		// Parse shortDate 'M/D/YYYY' into a Date (local noon to avoid DST edge cases)
		function parseSd(sd: string): Date {
			const [m, d, y] = sd.split('/').map(Number);
			return new Date(y, m - 1, d, 12, 0, 0);
		}
		function daysBetween(a: Date, b: Date): number {
			return Math.round((b.getTime() - a.getTime()) / 86_400_000);
		}

		const last = player.lastDailyDate;
		const todayDate = parseSd(shortDate);

		if (!last) {
			player.currentStreak = 1;
		} else {
			if (last === shortDate) {
				// Same day repeat — no change
				return;
			}
			const diff = daysBetween(parseSd(last), todayDate);
			if (diff === 1) {
				player.currentStreak = (player.currentStreak ?? 0) + 1;
			} else if (diff === 2 && (player.streakShieldsOwned ?? 0) > 0) {
				// Gap of exactly one missed day — burn a shield
				player.streakShieldsOwned--;
				player.currentStreak = (player.currentStreak ?? 0) + 1;
			} else {
				player.currentStreak = 1;
			}
		}

		player.bestStreak = Math.max(player.bestStreak ?? 0, player.currentStreak);
		player.lastDailyDate = shortDate;
		touchPlayer();
	}

	function isDailyMazeUnlocked(): boolean {
		const w1l10 = getLevelProgress(1, '10');
		return w1l10?.star1 === true;
	}

	// --- Crystal shards ---

	function addCrystalShards(n: number) {
		player.crystalShards = (player.crystalShards ?? 0) + n;
		touchPlayer();
		save();
	}

	function spendCrystalShards(n: number): boolean {
		if ((player.crystalShards ?? 0) < n) return false;
		player.crystalShards -= n;
		touchPlayer();
		save();
		return true;
	}

	function incrementMazesCompleted() {
		player.mazesCompleted = (player.mazesCompleted ?? 0) + 1;
		touchPlayer();
		save();
	}

	// --- Achievements ---

	/**
	 * Evaluate all achievements against current player state.
	 * Unlocks any newly met criteria, awards rewards, and returns IDs of
	 * achievements that were unlocked this call.
	 */
	function checkAchievements(): string[] {
		if (!player.achievements) player.achievements = {};
		const newlyUnlocked: string[] = [];
		const today = new Date();
		const dateStr = today.toLocaleDateString('en-US', { month: 'numeric', day: 'numeric', year: 'numeric' });

		for (const def of ACHIEVEMENT_CATALOG) {
			const entry = player.achievements[def.id] ?? { progress: 0, unlocked: false };
			if (entry.unlocked) continue;

			// Compute current progress
			let currentProgress = 0;
			switch (def.id) {
				case 'first_daily': {
					const completedDailies = Object.values(dailyResults).filter(d => d.status === 'completed' || d.status === 'completed_late');
					currentProgress = completedDailies.length > 0 ? 1 : 0;
					break;
				}
				case 'daily_devotion': {
					currentProgress = Object.values(dailyResults).filter(d => d.status === 'completed' || d.status === 'completed_late').length;
					break;
				}
				case 'streak_7':
					currentProgress = player.bestStreak ?? 0;
					break;
				case 'speed_runner':
					// Unlocked externally via flagging in play pages; check stored progress
					currentProgress = entry.progress;
					break;
				case 'hint_free':
					currentProgress = entry.progress;
					break;
				case 'three_star_three': {
					const threeStarLevels = Object.values(levelProgress).filter(l => l.numberOfStars >= 3);
					currentProgress = Math.min(threeStarLevels.length, def.target);
					break;
				}
				case 'century_explorer':
					currentProgress = player.mazesCompleted ?? 0;
					break;
				case 'world1_clear': {
					const w1 = worlds.find(w => w.worldId === 1);
					const w1Cleared = w1 ? w1.levels.filter(l => !l.levelNumber.includes('b')).every(l => getLevelProgress(1, l.levelNumber)?.completed) : false;
					currentProgress = w1Cleared ? 1 : 0;
					break;
				}
				case 'world2_clear': {
					const w2 = worlds.find(w => w.worldId === 2);
					const w2Cleared = w2 && w2.levels.length > 0 ? w2.levels.filter(l => !l.levelNumber.includes('b')).every(l => getLevelProgress(2, l.levelNumber)?.completed) : false;
					currentProgress = w2Cleared ? 1 : 0;
					break;
				}
				case 'world3_clear': {
					const w3 = worlds.find(w => w.worldId === 3);
					const w3Cleared = w3 && w3.levels.length > 0 ? w3.levels.filter(l => !l.levelNumber.includes('b')).every(l => getLevelProgress(3, l.levelNumber)?.completed) : false;
					currentProgress = w3Cleared ? 1 : 0;
					break;
				}
				case 'deity_student': {
					const maxMastery = Math.max(0, ...Object.values(player.algoMasteryCount ?? {}));
					currentProgress = Math.min(maxMastery, def.target);
					break;
				}
				case 'deity_disciple': {
					const maxMastery = Math.max(0, ...Object.values(player.algoMasteryCount ?? {}));
					currentProgress = Math.min(maxMastery, def.target);
					break;
				}
				case 'deity_champion': {
					const maxMastery = Math.max(0, ...Object.values(player.algoMasteryCount ?? {}));
					currentProgress = Math.min(maxMastery, def.target);
					break;
				}
				case 'coin_hoarder':
					currentProgress = player.coinsEarnedLifetime ?? 0;
					break;
				case 'full_satchel': {
					const allOwned =
						(player.hintsOwned > 0) &&
						(player.extraTimesOwned > 0) &&
						(player.extraMovesOwned > 0) &&
						((player.compassOwned ?? 0) > 0) &&
						((player.hourglassOwned ?? 0) > 0) &&
						((player.blinkScrollsOwned ?? 0) > 0) &&
						((player.streakShieldsOwned ?? 0) > 0) &&
						((player.doubleCoinsTokensOwned ?? 0) > 0);
					currentProgress = allOwned ? 1 : 0;
					break;
				}
				default:
					currentProgress = entry.progress;
			}

			entry.progress = currentProgress;

			if (currentProgress >= def.target) {
				entry.unlocked = true;
				entry.dateUnlocked = dateStr;
				// Award reward
				if (def.rewardType === 'coins') player.coinCount += def.rewardAmount;
				else if (def.rewardType === 'gems') player.gemCount = (player.gemCount ?? 0) + def.rewardAmount;
				else if (def.rewardType === 'shards') player.crystalShards = (player.crystalShards ?? 0) + def.rewardAmount;
				else if (def.rewardType === 'powerup' && def.rewardPowerup) addPowerup(def.rewardPowerup as import('$lib/core/types').PowerupName);
				newlyUnlocked.push(def.id);
			}

			player.achievements[def.id] = entry;
		}

		if (newlyUnlocked.length > 0) {
			touchPlayer();
			save();
		} else {
			// Still persist progress updates quietly
			save();
		}

		return newlyUnlocked;
	}

	/**
	 * Manually advance progress for achievements that can't be auto-computed
	 * (speed_runner, hint_free). Returns true if the value changed.
	 */
	function markAchievementProgress(id: string, progress: number) {
		if (!player.achievements) player.achievements = {};
		const entry = player.achievements[id] ?? { progress: 0, unlocked: false };
		if (entry.unlocked) return;
		entry.progress = Math.max(entry.progress, progress);
		player.achievements[id] = entry;
		touchPlayer();
		save();
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
		get ghostRuns() { return ghostRuns; },
		get eventProgress() { return eventProgress; },
		get activeEvent() { return getActiveEvent(); },
		get shopCatalog() { return getEventPowerupCatalog(); },
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
		awardSpecialItem,
		usePowerup,
		recordAlgoMastery,
		getLevelProgress,
		getGhostRun,
		saveLevelProgress,
		getWorldStarCount,
		getDailyResult,
		saveDailyResult,
		getEventProgress,
		incrementEventProgress,
		isDailyMazeUnlocked,
		hasKey,
		addKey,
		isMapItemCollected,
		collectMapItem,
		getHighestAreaUnlocked,
		addCrystalShards,
		spendCrystalShards,
		incrementMazesCompleted,
		reconcileAllMasteryRewards,
		checkAchievements,
		markAchievementProgress
	};
}
