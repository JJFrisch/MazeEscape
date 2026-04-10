import { getSupabaseBrowserClient } from './client';

export interface DailyLeaderboardDayRow {
	short_date: string;
	played_on: string;
	entry_count: number;
	best_time_seconds: number | null;
}

export interface DailyLeaderboardEntryRow {
	rank: number;
	short_date: string;
	played_on: string;
	user_id: string;
	player_name: string;
	completion_time: number;
	completion_moves: number;
	completed_at: string;
}

export interface CampaignLeaderboardEntryRow {
	rank: number;
	user_id: string;
	player_name: string;
	total_stars: number;
	completed_levels: number;
	best_time_seconds: number | null;
	best_moves: number | null;
	last_updated_at: string;
}

export async function fetchDailyLeaderboardDays(limit = 14): Promise<DailyLeaderboardDayRow[]> {
	const supabase = getSupabaseBrowserClient();
	const { data, error } = await supabase
		.from('daily_leaderboard_days')
		.select('*')
		.order('played_on', { ascending: false })
		.limit(limit)
		.returns<DailyLeaderboardDayRow[]>();

	if (error) throw error;
	return data ?? [];
}

export async function fetchDailyLeaderboard(shortDate: string, limit = 50): Promise<DailyLeaderboardEntryRow[]> {
	const supabase = getSupabaseBrowserClient();
	const { data, error } = await supabase
		.from('daily_leaderboard_entries')
		.select('*')
		.eq('short_date', shortDate)
		.order('rank', { ascending: true })
		.limit(limit)
		.returns<DailyLeaderboardEntryRow[]>();

	if (error) throw error;
	return data ?? [];
}

export async function fetchCampaignLeaderboard(limit = 50): Promise<CampaignLeaderboardEntryRow[]> {
	const supabase = getSupabaseBrowserClient();
	const { data, error } = await supabase
		.from('campaign_star_leaderboard')
		.select('*')
		.order('rank', { ascending: true })
		.limit(limit)
		.returns<CampaignLeaderboardEntryRow[]>();

	if (error) throw error;
	return data ?? [];
}