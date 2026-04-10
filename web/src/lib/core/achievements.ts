/**
 * Achievement catalog — Guild Commendations.
 * progress / target are integers. The store manages per-player progress.
 */

export type AchievementRewardType = 'coins' | 'gems' | 'shards' | 'powerup' | 'trophy';

export type AchievementCategory =
	| 'Daily'
	| 'Mastery'
	| 'Exploration'
	| 'Prestige'
	| 'Collection';

export interface AchievementDef {
	id: string;
	name: string;
	description: string;
	icon: string;
	category: AchievementCategory;
	/** denominator for the progress bar */
	target: number;
	rewardType: AchievementRewardType;
	rewardAmount: number;
	/** optional powerup name when rewardType === 'powerup' */
	rewardPowerup?: string;
}

export const ACHIEVEMENT_CATALOG: AchievementDef[] = [
	// ── Daily ────────────────────────────────────────────────────
	{
		id: 'first_daily',
		name: "Initiate's Mark",
		description: 'Complete your first daily maze.',
		icon: '🗺️',
		category: 'Daily',
		target: 1,
		rewardType: 'coins',
		rewardAmount: 100
	},
	{
		id: 'daily_devotion',
		name: 'Devoted Wanderer',
		description: 'Complete 7 daily mazes (any days).',
		icon: '📅',
		category: 'Daily',
		target: 7,
		rewardType: 'powerup',
		rewardAmount: 1,
		rewardPowerup: 'compass'
	},
	{
		id: 'streak_7',
		name: 'Oath-Keeper',
		description: 'Maintain a 7-day daily completion streak.',
		icon: '🔥',
		category: 'Daily',
		target: 7,
		rewardType: 'powerup',
		rewardAmount: 1,
		rewardPowerup: 'hourglass'
	},

	// ── Exploration ──────────────────────────────────────────────
	{
		id: 'speed_runner',
		name: 'Quicksilver',
		description: 'Finish any maze in under 30 seconds.',
		icon: '⚡',
		category: 'Exploration',
		target: 1,
		rewardType: 'coins',
		rewardAmount: 50
	},
	{
		id: 'hint_free',
		name: 'Unaided',
		description: 'Complete any maze using zero hints.',
		icon: '👁️',
		category: 'Exploration',
		target: 1,
		rewardType: 'powerup',
		rewardAmount: 1,
		rewardPowerup: 'blinkScroll'
	},
	{
		id: 'three_star_three',
		name: 'Trinary Star',
		description: 'Earn at least 3 stars in 3 different campaign mazes.',
		icon: '⭐',
		category: 'Exploration',
		target: 3,
		rewardType: 'gems',
		rewardAmount: 1
	},
	{
		id: 'century_explorer',
		name: 'The Centurion',
		description: 'Complete 100 mazes in total.',
		icon: '🏛️',
		category: 'Exploration',
		target: 100,
		rewardType: 'trophy',
		rewardAmount: 1
	},

	// ── Prestige ────────────────────────────────────────────────
	{
		id: 'world1_clear',
		name: 'Cybernetic Conqueror',
		description: 'Complete every level in Cybernetic Labyrinths.',
		icon: '🤖',
		category: 'Prestige',
		target: 1,
		rewardType: 'trophy',
		rewardAmount: 1
	},
	{
		id: 'world2_clear',
		name: 'Galactic Conqueror',
		description: 'Complete every level in Galactic Grids.',
		icon: '🚀',
		category: 'Prestige',
		target: 1,
		rewardType: 'trophy',
		rewardAmount: 1
	},
	{
		id: 'world3_clear',
		name: 'Elemental Master',
		description: 'Complete every level in Elemental Whispers.',
		icon: '🌿',
		category: 'Prestige',
		target: 1,
		rewardType: 'trophy',
		rewardAmount: 1
	},

	// ── Mastery ──────────────────────────────────────────────────
	{
		id: 'deity_student',
		name: 'The Willing',
		description: 'Reach Student mastery (10 runs) with any deity.',
		icon: '📖',
		category: 'Mastery',
		target: 10,
		rewardType: 'shards',
		rewardAmount: 5
	},
	{
		id: 'deity_disciple',
		name: 'Path-Walker',
		description: 'Reach Disciple mastery (20 runs) with any deity.',
		icon: '🧭',
		category: 'Mastery',
		target: 20,
		rewardType: 'gems',
		rewardAmount: 1
	},
	{
		id: 'deity_champion',
		name: 'Champion Eternal',
		description: 'Reach Champion mastery (30 runs) with any deity.',
		icon: '👑',
		category: 'Mastery',
		target: 30,
		rewardType: 'gems',
		rewardAmount: 2
	},

	// ── Collection ───────────────────────────────────────────────
	{
		id: 'coin_hoarder',
		name: 'Treasury Ward',
		description: 'Earn 5,000 coins over your lifetime.',
		icon: '🪙',
		category: 'Collection',
		target: 5000,
		rewardType: 'gems',
		rewardAmount: 1
	},
	{
		id: 'full_satchel',
		name: 'Provisioned',
		description: 'Own at least 1 of every powerup type at the same time.',
		icon: '🎒',
		category: 'Collection',
		target: 1,
		rewardType: 'gems',
		rewardAmount: 1
	}
];
