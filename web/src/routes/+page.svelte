<!--
  The hub, picked by Jake on 2026-11-19 as take C "play first" in
  static/mock/hub-and-back.html: the native world card for the world the
  player is up to, with Play World as the one primary action, and the other
  five zones as plain rows beside it (under it on a phone).
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { afterNavigate, goto } from '$app/navigation';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { getWorldMapLayout } from '$lib/core/campaignMapLayout';
	import { isZoneUnlocked, zoneLockReason } from '$lib/core/zoneLocks';

	const worlds = getAllWorlds();
	// The same art the worlds picker puts behind each card.
	const ART: Record<number, string> = {
		1: 'background_maze_3.webp',
		2: 'space_background11.webp',
		3: 'classic/carousel_maze_4.webp'
	};

	// A player who has not finished a level yet arrives on the World 1 map, one
	// press from playing. Only the arrival is redirected, so the logo still opens
	// this hub, and anyone with progress lands here as before.
	let skippingHub = $state(false);
	afterNavigate(({ type }) => {
		if (type !== 'enter') return;
		gameStore.init();
		if (Object.values(gameStore.levelProgress).some((level) => level.completed)) return;
		skippingHub = true;
		goto(`${base}/campaign/worlds/1`, { replaceState: true });
	});

	// A world opens when the previous world's portal opens; the worlds picker
	// applies the same rule.
	function isPortalOpen(worldId: number): boolean {
		const portal = getWorldMapLayout(worldId)?.nodes.find((n) => n.type === 'portal');
		const finalLevelDone = portal?.levelNumber
			? !!gameStore.getLevelProgress(worldId, portal.levelNumber)?.completed
			: true;
		return gameStore.getWorldStarCount(worldId) >= (portal?.starsRequired ?? 300) && finalLevelDone;
	}

	/** The furthest world the player can play: the card shows this one. */
	const world = $derived.by(() => {
		let current = worlds[0];
		for (let i = 1; i < worlds.length; i++) {
			if (worlds[i].levels.length === 0 || !isPortalOpen(worlds[i - 1].worldId)) break;
			current = worlds[i];
		}
		return current;
	});
	const mainLevels = $derived(world.levels.filter((l) => /^\d+$/.test(l.levelNumber)));
	const beaten = $derived(
		mainLevels
			.filter((l) => gameStore.getLevelProgress(world.worldId, l.levelNumber)?.completed)
			.reduce((top, l) => Math.max(top, parseInt(l.levelNumber)), 0)
	);
	const stars = $derived(gameStore.getWorldStarCount(world.worldId));

	const dailyStreak = $derived(gameStore.player.currentStreak ?? 0);
	const coins = $derived(gameStore.player.coinCount);
	// Until the Archives unlock, their row leads to the Algorithms page alone,
	// which is open at every level.
	const archivesLocked = $derived(!isZoneUnlocked('archives'));

	type Row = {
		id: string;
		name: string;
		subtitle: string;
		href: string;
		stat: string;
		description: string;
		links?: { label: string; href: string }[];
		lockReason?: string;
	};

	const rows: Row[] = $derived([
		{
			id: 'spire',
			name: 'The Spire',
			subtitle: 'Daily Maze',
			href: `${base}/daily`,
			stat: dailyStreak > 0 ? `🔥 ${dailyStreak} day streak` : 'No streak yet',
			description: 'A fresh labyrinth generated every 24 hours',
			lockReason: isZoneUnlocked('spire') ? undefined : zoneLockReason('spire')
		},
		{
			id: 'arena',
			name: 'The Arena',
			subtitle: 'Leaderboard',
			href: `${base}/leaderboard`,
			stat: 'Global rankings',
			description: 'Compete for the top of the board',
			lockReason: isZoneUnlocked('arena') ? undefined : zoneLockReason('arena')
		},
		{
			id: 'vault',
			name: 'The Vault',
			subtitle: 'Shop & Equip',
			href: `${base}/shop`,
			stat: `${coins.toLocaleString()} coins`,
			description: 'Powerups, skins, and trail effects',
			links: [{ label: 'Equip', href: `${base}/equip` }],
			lockReason: isZoneUnlocked('vault') ? undefined : zoneLockReason('vault')
		},
		{
			id: 'archives',
			name: 'The Archives',
			subtitle: 'Knowledge',
			href: archivesLocked ? `${base}/algorithms` : `${base}/codex`,
			stat: 'Lore & mastery',
			description: 'Codex of worlds, algorithm lore & chronicles',
			links: archivesLocked
				? undefined
				: [
						{ label: 'Algorithms', href: `${base}/algorithms` },
						{ label: 'Chronicles', href: `${base}/stats` }
					]
		},
		{
			id: 'sanctum',
			name: 'The Sanctum',
			subtitle: 'Settings & Account',
			href: `${base}/settings`,
			stat: authStore.user ? (authStore.user.email?.split('@')[0] ?? 'Signed in') : 'Sign In',
			description: 'Configure your journey and sync your progress',
			links: [{ label: 'Account', href: `${base}/auth` }]
		}
	]);
</script>

<svelte:head>
	<title>Maze Escape: Pathbound, World Map</title>
</svelte:head>

{#if !skippingHub}
	<div class="hub">
		<a
			class="wcard"
			href="{base}/campaign/worlds/{world.worldId}"
			aria-label={world.worldName}
			style="background-image: url('{base}/images/{ART[world.worldId]}');"
		>
			<span class="titles">
				<span class="game">Maze Escape</span>
				<span class="worlds">Worlds</span>
			</span>
			<span class="name">{world.worldName}</span>
			<span class="panel">
				<img src="{base}/images/classic/grid_background.webp" alt="" draggable="false" />
				<span class="score beaten">{beaten} / {mainLevels.length}</span>
				<span class="score stars">{stars} Stars</span>
			</span>
			<img class="play" src="{base}/images/classic/play_world_button.webp" alt="Play World" draggable="false" />
		</a>

		<nav class="rows" aria-label="Game zones">
			{#each rows as row (row.id)}
				{#if row.lockReason}
					<div class="row locked">
						<h2>{row.name}<span>{row.subtitle}</span></h2>
						<span class="stat"></span>
						<p class="desc"><img src="{base}/images/lock.webp" alt="" />{row.lockReason}</p>
					</div>
				{:else}
					<!-- The zone name is the row's link and covers the whole row; the
					     smaller links sit above it. Approved by Jake on 2026-11-19 as an
					     addition to take C, styled as the pills in the mockup's take B. -->
					<div class="row open">
						<h2><a class="main" href={row.href}>{row.name}</a><span>{row.subtitle}</span></h2>
						<span class="stat">{row.stat}</span>
						<p class="desc">{row.description}</p>
						{#if row.links}
							<span class="links">
								{#each row.links as link}
									<a href={link.href}>{link.label}</a>
								{/each}
							</span>
						{/if}
					</div>
				{/if}
			{/each}
		</nav>
	</div>

	<div class="foot">
		<a class="dl" href="{base}/download">Desktop App</a>
		<a href="{base}/privacy">Privacy</a>
		<a href="{base}/terms">Terms</a>
		<span class="end">© 2026 Jake Frischmann</span>
	</div>
{/if}

<style>
	/* Values are the mockup's. The three custom properties stand in for its fixed
	   dark colours so the rows stay readable in the light theme, which the
	   mockup does not show. Approved by Jake on 2026-11-19. */
	.hub,
	.foot {
		--hub-dim: #64748b;
		--hub-line: rgba(255, 255, 255, 0.12);
		--hub-hover: rgba(255, 255, 255, 0.04);
		width: 100%;
		max-width: 1120px;
		margin: 0 auto;
	}
	:global(html[data-theme='light']) .hub,
	:global(html[data-theme='light']) .foot {
		--hub-line: rgba(0, 0, 0, 0.12);
		--hub-hover: rgba(0, 0, 0, 0.04);
	}

	.hub {
		display: grid;
		grid-template-columns: minmax(0, 5fr) minmax(0, 4fr);
		gap: 24px;
		padding: 24px;
	}

	/* ── The native world card ─────────────────────────────── */
	.wcard {
		position: relative;
		display: block;
		min-height: 560px;
		border-radius: 12px;
		overflow: hidden;
		background: #000 center / cover;
		color: #fff;
		text-align: center;
		text-decoration: none;
		transition: transform 120ms ease;
	}
	.wcard:active { transform: scale(0.98); }
	.wcard:focus-visible { outline: 2px solid var(--color-accent-primary); outline-offset: 2px; }

	.titles { position: absolute; inset: 20px 0 auto; }
	.titles span { display: block; }
	.game { font: 400 22px/1.5 var(--font-body); color: whitesmoke; }
	.worlds { margin-top: 6px; font: 400 18px/1.2 var(--font-body); color: lightblue; }

	.name {
		position: absolute;
		top: 130px;
		left: 50%;
		width: min(300px, 90%);
		height: 40px;
		transform: translateX(-50%);
		display: grid;
		place-items: center;
		background: rgba(0, 0, 0, 0.5);
		font: 700 20px/1 var(--font-body);
	}

	.panel {
		position: absolute;
		top: 50%;
		left: 50%;
		width: 250px;
		transform: translate(-50%, -75%);
	}
	.panel img { display: block; width: 100%; }
	.score {
		position: absolute;
		top: 60%;
		width: 50%;
		font: 700 12px/1 var(--font-body);
		color: #222;
		text-align: center;
	}
	.score.beaten { left: 0; }
	.score.stars { right: 0; }

	.play {
		position: absolute;
		top: 72%;
		left: 50%;
		width: 230px;
		transform: translateX(-50%);
		transition: transform 120ms ease;
	}
	.wcard:hover .play { transform: translateX(-50%) scale(1.04); }

	/* ── The other zones ───────────────────────────────────── */
	.rows { display: flex; flex-direction: column; align-self: center; }
	.row {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 4px 16px;
		align-items: baseline;
		padding: 16px 8px;
		border-top: 1px solid var(--hub-line);
		color: var(--color-text-primary);
		text-decoration: none;
		transition: background var(--transition-fast);
	}
	.row:last-child { border-bottom: 1px solid var(--hub-line); }
	.row.open { position: relative; }
	.row.open:hover { background: var(--hub-hover); }
	.row.open:has(.main:active) { transform: scale(0.99); }
	.main { color: inherit; text-decoration: none; }
	.main::after { content: ''; position: absolute; inset: 0; }
	.main:focus-visible { outline: none; }
	.main:focus-visible::after { outline: 2px solid var(--color-accent-primary); outline-offset: 2px; }
	.links { grid-column: 1 / -1; display: flex; gap: 8px; padding-top: 8px; }
	.links a {
		position: relative;
		z-index: 1;
		padding: 6px 12px;
		border-radius: 999px;
		border: 1px solid var(--hub-line);
		font: 500 12px/1 var(--font-body);
		color: var(--color-text-secondary);
		text-decoration: none;
		transition: color var(--transition-fast), border-color var(--transition-fast);
	}
	.links a:hover { color: var(--color-text-primary); border-color: var(--color-text-secondary); }
	.links a:focus-visible { outline: 2px solid var(--color-accent-primary); outline-offset: 2px; }
	.row h2 { margin: 0; font: 700 17px/1.2 var(--font-display); }
	.row h2 span { margin-left: 8px; font: 500 13px/1 var(--font-body); color: var(--color-text-secondary); }
	.row .stat { font: 600 13px/1 var(--font-display); color: var(--color-text-secondary); text-align: right; }
	.row .desc { grid-column: 1 / -1; margin: 0; font: 400 13px/1.5 var(--font-body); color: var(--color-text-secondary); }
	.row.locked { cursor: not-allowed; }
	.row.locked h2,
	.row.locked h2 span { color: var(--hub-dim); }
	.row.locked .desc { display: flex; align-items: center; gap: 8px; }
	.row.locked .desc img { width: 14px; height: 15px; }

	/* ── Footer ────────────────────────────────────────────── */
	.foot {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		gap: 8px 24px;
		padding: 16px;
		font: 500 12px/1 var(--font-body);
		color: var(--hub-dim);
	}
	.foot a { color: var(--color-text-secondary); text-decoration: none; }
	.foot a:hover { color: var(--color-text-primary); }
	.foot .dl {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 8px 12px;
		border-radius: 999px;
		border: 1px solid rgba(245, 158, 11, 0.35);
		color: var(--color-accent-gold);
	}
	.foot .end { margin-left: auto; }

	@media (max-width: 640px) {
		.hub { grid-template-columns: 1fr; padding: 16px; }
		.wcard { min-height: 480px; }
	}
</style>
