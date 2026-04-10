-- MazeEscape Supabase Schema
-- Normalized schema for user progress, inventory, and daily mazes.
-- Run this via Supabase SQL editor or as a migration.

-- Enable UUID generation
create extension if not exists "uuid-ossp";

-- ============================================================================
-- Users / Profiles
-- ============================================================================

create table if not exists profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  player_name text not null default 'Player',
  coin_count integer not null default 0,
  hints_owned integer not null default 0,
  extra_times_owned integer not null default 0,
  extra_moves_owned integer not null default 0,
  current_skin_id integer not null default 0,
  wall_color text not null default '#000000',
  month_prize1_achieved boolean not null default false,
  month_prize2_achieved boolean not null default false,
  most_recent_month text not null default '',
  special_item_ids text[] not null default '{}',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

alter table profiles add column if not exists special_item_ids text[] not null default '{}';

-- RLS: users can only read/write their own profile
alter table profiles enable row level security;

drop policy if exists "Users can read own profile" on profiles;
drop policy if exists "Users can update own profile" on profiles;
drop policy if exists "Users can insert own profile" on profiles;

create policy "Users can read own profile"
  on profiles for select using (auth.uid() = id);

create policy "Users can update own profile"
  on profiles for update using (auth.uid() = id);

create policy "Users can insert own profile"
  on profiles for insert with check (auth.uid() = id);

-- ============================================================================
-- Campaign Level Progress
-- ============================================================================

create table if not exists level_progress (
  id uuid primary key default uuid_generate_v4(),
  user_id uuid not null references profiles(id) on delete cascade,
  world_id integer not null,
  level_number text not null,
  completed boolean not null default false,
  star1 boolean not null default false,
  star2 boolean not null default false,
  star3 boolean not null default false,
  best_moves integer not null default 0,
  best_time_seconds numeric not null default 0,
  best_run_moves text[] null,
  updated_at timestamptz not null default now(),

  unique(user_id, world_id, level_number)
);

alter table level_progress add column if not exists best_run_moves text[] null;

alter table level_progress enable row level security;

drop policy if exists "Users can read own progress" on level_progress;
drop policy if exists "Users can upsert own progress" on level_progress;
drop policy if exists "Users can update own progress" on level_progress;

create policy "Users can read own progress"
  on level_progress for select using (auth.uid() = user_id);

create policy "Users can upsert own progress"
  on level_progress for insert with check (auth.uid() = user_id);

create policy "Users can update own progress"
  on level_progress for update using (auth.uid() = user_id);

-- ============================================================================
-- Seasonal Event Progress
-- ============================================================================

create table if not exists event_progress (
  id uuid primary key default uuid_generate_v4(),
  user_id uuid not null references profiles(id) on delete cascade,
  event_id text not null,
  progress_value integer not null default 0,
  completed_milestones integer[] not null default '{}',
  updated_at timestamptz not null default now(),

  unique(user_id, event_id)
);

alter table event_progress enable row level security;

drop policy if exists "Users can read own event progress" on event_progress;
drop policy if exists "Users can upsert own event progress" on event_progress;
drop policy if exists "Users can update own event progress" on event_progress;

create policy "Users can read own event progress"
  on event_progress for select using (auth.uid() = user_id);

create policy "Users can upsert own event progress"
  on event_progress for insert with check (auth.uid() = user_id);

create policy "Users can update own event progress"
  on event_progress for update using (auth.uid() = user_id);

-- ============================================================================
-- Daily Maze Results
-- ============================================================================

create table if not exists daily_maze_results (
  id uuid primary key default uuid_generate_v4(),
  user_id uuid not null references profiles(id) on delete cascade,
  short_date text not null,        -- "m/d/yyyy"
  month_year text not null,        -- "MM-yyyy"
  status text not null default 'not_started',  -- not_started | completed | completed_late
  completion_time numeric not null default 0,
  completion_moves integer not null default 0,
  updated_at timestamptz not null default now(),

  unique(user_id, short_date)
);

alter table daily_maze_results enable row level security;

drop policy if exists "Users can read own daily results" on daily_maze_results;
drop policy if exists "Users can upsert own daily results" on daily_maze_results;
drop policy if exists "Users can update own daily results" on daily_maze_results;

create policy "Users can read own daily results"
  on daily_maze_results for select using (auth.uid() = user_id);

create policy "Users can upsert own daily results"
  on daily_maze_results for insert with check (auth.uid() = user_id);

create policy "Users can update own daily results"
  on daily_maze_results for update using (auth.uid() = user_id);

-- ============================================================================
-- Inventory (Owned Skins)
-- ============================================================================

create table if not exists owned_skins (
  id uuid primary key default uuid_generate_v4(),
  user_id uuid not null references profiles(id) on delete cascade,
  skin_id integer not null,
  acquired_at timestamptz not null default now(),

  unique(user_id, skin_id)
);

alter table owned_skins enable row level security;

drop policy if exists "Users can read own skins" on owned_skins;
drop policy if exists "Users can insert own skins" on owned_skins;

create policy "Users can read own skins"
  on owned_skins for select using (auth.uid() = user_id);

create policy "Users can insert own skins"
  on owned_skins for insert with check (auth.uid() = user_id);

-- ============================================================================
-- World Unlock State
-- ============================================================================

create table if not exists world_progress (
  id uuid primary key default uuid_generate_v4(),
  user_id uuid not null references profiles(id) on delete cascade,
  world_id integer not null,
  locked boolean not null default true,
  star_count integer not null default 0,
  highest_beaten_level integer not null default 0,
  highest_area_unlocked integer not null default 0,
  unlocked_gate_numbers integer[] not null default '{}',
  updated_at timestamptz not null default now(),

  unique(user_id, world_id)
);

alter table world_progress enable row level security;

drop policy if exists "Users can read own world progress" on world_progress;
drop policy if exists "Users can upsert own world progress" on world_progress;
drop policy if exists "Users can update own world progress" on world_progress;

create policy "Users can read own world progress"
  on world_progress for select using (auth.uid() = user_id);

create policy "Users can upsert own world progress"
  on world_progress for insert with check (auth.uid() = user_id);

create policy "Users can update own world progress"
  on world_progress for update using (auth.uid() = user_id);

-- ============================================================================
-- Public Leaderboards
-- ============================================================================

create or replace view public.daily_leaderboard_days as
select
  d.short_date,
  to_date(d.short_date, 'FMMM/FMDD/YYYY') as played_on,
  count(*)::integer as entry_count,
  min(d.completion_time) as best_time_seconds
from daily_maze_results d
where d.status = 'completed'
group by d.short_date, to_date(d.short_date, 'FMMM/FMDD/YYYY');

create or replace view public.daily_leaderboard_entries as
select
  dense_rank() over (
    partition by d.short_date
    order by d.completion_time asc, d.completion_moves asc, d.updated_at asc, d.user_id asc
  )::integer as rank,
  d.short_date,
  to_date(d.short_date, 'FMMM/FMDD/YYYY') as played_on,
  d.user_id,
  p.player_name,
  d.completion_time,
  d.completion_moves,
  d.updated_at as completed_at
from daily_maze_results d
join profiles p on p.id = d.user_id
where d.status = 'completed';

create or replace view public.campaign_star_leaderboard as
with campaign_totals as (
  select
    lp.user_id,
    p.player_name,
    sum(
      (case when lp.star1 then 1 else 0 end) +
      (case when lp.star2 then 1 else 0 end) +
      (case when lp.star3 then 1 else 0 end)
    )::integer as total_stars,
    count(*) filter (where lp.completed)::integer as completed_levels,
    min(nullif(lp.best_time_seconds, 0)) as best_time_seconds,
    min(nullif(lp.best_moves, 0)) as best_moves,
    max(lp.updated_at) as last_updated_at
  from level_progress lp
  join profiles p on p.id = lp.user_id
  group by lp.user_id, p.player_name
)
select
  dense_rank() over (
    order by total_stars desc,
    completed_levels desc,
    coalesce(best_time_seconds, 1000000000) asc,
    coalesce(best_moves, 2147483647) asc,
    last_updated_at asc,
    user_id asc
  )::integer as rank,
  user_id,
  player_name,
  total_stars,
  completed_levels,
  best_time_seconds,
  best_moves,
  last_updated_at
from campaign_totals
where total_stars > 0 or completed_levels > 0;

grant select on public.daily_leaderboard_days to anon, authenticated;
grant select on public.daily_leaderboard_entries to anon, authenticated;
grant select on public.campaign_star_leaderboard to anon, authenticated;

-- ============================================================================
-- Updated-at trigger function
-- ============================================================================

create or replace function update_updated_at_column()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql;

drop trigger if exists profiles_updated_at on profiles;
create trigger profiles_updated_at before update on profiles
  for each row execute function update_updated_at_column();

drop trigger if exists level_progress_updated_at on level_progress;
create trigger level_progress_updated_at before update on level_progress
  for each row execute function update_updated_at_column();

drop trigger if exists daily_maze_results_updated_at on daily_maze_results;
create trigger daily_maze_results_updated_at before update on daily_maze_results
  for each row execute function update_updated_at_column();

drop trigger if exists world_progress_updated_at on world_progress;
create trigger world_progress_updated_at before update on world_progress
  for each row execute function update_updated_at_column();

drop trigger if exists event_progress_updated_at on event_progress;
create trigger event_progress_updated_at before update on event_progress
  for each row execute function update_updated_at_column();
