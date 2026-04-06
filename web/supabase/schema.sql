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
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- RLS: users can only read/write their own profile
alter table profiles enable row level security;

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
  updated_at timestamptz not null default now(),

  unique(user_id, world_id, level_number)
);

alter table level_progress enable row level security;

create policy "Users can read own progress"
  on level_progress for select using (auth.uid() = user_id);

create policy "Users can upsert own progress"
  on level_progress for insert with check (auth.uid() = user_id);

create policy "Users can update own progress"
  on level_progress for update using (auth.uid() = user_id);

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

create policy "Users can read own world progress"
  on world_progress for select using (auth.uid() = user_id);

create policy "Users can upsert own world progress"
  on world_progress for insert with check (auth.uid() = user_id);

create policy "Users can update own world progress"
  on world_progress for update using (auth.uid() = user_id);

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

create trigger profiles_updated_at before update on profiles
  for each row execute function update_updated_at_column();

create trigger level_progress_updated_at before update on level_progress
  for each row execute function update_updated_at_column();

create trigger daily_maze_results_updated_at before update on daily_maze_results
  for each row execute function update_updated_at_column();

create trigger world_progress_updated_at before update on world_progress
  for each row execute function update_updated_at_column();
