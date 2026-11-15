<!--
  ClassicCampaignMap.svelte

  The campaign map as the native (.NET MAUI) app draws it: a hand-drawn maze you
  scroll sideways, glowing level orbs sitting in its cells, wooden star gates,
  and fog over the areas you have not reached.

  Every number here comes from CampaignPage.xaml / CampaignPage.xaml.cs as they
  stood with the full 67 levels and 22 bonus levels (before commit ebe3396). The
  native map is a 1700 x 470 canvas holding a 20 x 5 grid; positions below are
  in those units and the whole canvas scales to the available height.
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import EncounterCard from '$lib/components/EncounterCard.svelte';
	import type { MapNode } from '$lib/core/types';

	let { worldId }: { worldId: number } = $props();

	const MAP_W = 1700;
	const MAP_H = 470;
	const COLS = 20;
	const ROWS = 5;
	const PAD_X = 2;
	const CELL_W = (MAP_W - PAD_X * 2) / COLS;
	const CELL_H = MAP_H / ROWS;
	const ORB = 80;
	const TREASURE = 60;

	type Cell = [number, number];

	/** (column, row) of every level button, per area, in the native page's order. */
	const AREAS: { first: number; main: Cell[]; firstBonus: number; bonus: Cell[] }[] = [
		{
			first: 1,
			main: [[3, 3], [3, 4], [2, 4], [2, 3], [2, 2], [1, 2], [0, 2], [0, 1], [0, 0], [1, 0], [2, 0], [3, 0], [3, 1], [3, 2]],
			firstBonus: 1,
			bonus: [[0, 3], [0, 4], [1, 4], [2, 1]]
		},
		{
			first: 15,
			main: [[4, 2], [4, 1], [5, 1], [5, 0], [6, 0], [6, 1], [6, 2], [5, 2], [5, 3], [4, 3], [4, 4], [5, 4], [6, 4], [7, 4]],
			firstBonus: 5,
			bonus: [[7, 1], [7, 2], [7, 3]]
		},
		{
			first: 29,
			main: [[8, 4], [9, 4], [9, 3], [9, 2], [8, 2], [8, 1], [9, 1], [10, 1], [10, 0], [11, 0], [12, 0], [12, 1], [11, 1], [11, 2], [12, 2]],
			firstBonus: 8,
			bonus: [[9, 0], [8, 0]]
		},
		{
			first: 44,
			main: [[13, 2], [14, 2], [14, 1], [13, 1], [13, 0], [14, 0], [15, 0], [15, 1], [15, 2], [16, 2], [16, 3], [16, 4]],
			firstBonus: 10,
			bonus: [[15, 4], [14, 4], [13, 4], [12, 4], [11, 4], [10, 4], [10, 3], [11, 3], [12, 3], [13, 3], [14, 3]]
		},
		{
			first: 56,
			main: [[17, 4], [17, 3], [18, 3], [18, 4], [19, 4], [19, 3], [19, 2], [19, 1], [18, 1], [17, 1], [17, 0], [18, 0]],
			firstBonus: 21,
			bonus: [[17, 2], [16, 0]]
		}
	];

	const SPOTS: { num: string; area: number; cell: Cell }[] = AREAS.flatMap((a, i) => [
		...a.main.map((cell, k) => ({ num: String(a.first + k), area: i + 1, cell })),
		...a.bonus.map((cell, k) => ({ num: `${a.firstBonus + k}b`, area: i + 1, cell }))
	]);

	/** The level that must be reached, and the stars needed, before each area's fog lifts. */
	const AREA_ENTRY: { level: string; stars: number }[] = [
		{ level: '15', stars: 30 },
		{ level: '29', stars: 80 },
		{ level: '44', stars: 120 },
		{ level: '56', stars: 200 }
	];

	const KEY_ID = 'classic_k1';
	const FINAL_LEVEL = '67';
	const PORTAL_STARS = 250;

	interface Gate {
		x: number; y: number; w: number; h: number;
		rotate: number;
		stars: number;
		small?: boolean;
		/** Levels that must be reached before the gate opens. */
		needs: string[];
		/** The last gate also needs the key and the final level. */
		final?: boolean;
	}
	const GATES: Gate[] = [
		{ x: 24, y: 243, w: 35, h: 80, rotate: 270, stars: 20, needs: ['1b'] },
		{ x: 235, y: 91, w: 35, h: 98, rotate: 0, stars: 45, needs: ['4b'] },
		{ x: 319, y: 187, w: 35, h: 98, rotate: 0, stars: 30, needs: ['15'] },
		{ x: 570, y: 94, w: 55, h: 96, rotate: 0, stars: 60, needs: ['5b'] },
		{ x: 654.5, y: 375, w: 55, h: 96, rotate: 0, stars: 80, needs: ['29'] },
		{ x: 824, y: 0, w: 55, h: 96, rotate: 0, stars: 100, small: true, needs: ['8b'] },
		{ x: 1078, y: 188, w: 55, h: 96, rotate: 0, stars: 120, small: true, needs: ['44'] },
		{ x: 1332.5, y: 375, w: 55, h: 96, rotate: 0, stars: 150, small: true, needs: ['10b'] },
		{ x: 1419, y: 375, w: 55, h: 96, rotate: 0, stars: 200, small: true, needs: ['56'] },
		{ x: 1460, y: 145, w: 55, h: 88, rotate: 270, stars: 230, small: true, needs: ['21b'] },
		{ x: 1418.5, y: 0, w: 55, h: 96, rotate: 0, stars: 240, small: true, needs: ['22b'] },
		{ x: 1585.5, y: 0, w: 55, h: 96, rotate: 0, stars: 0, small: true, needs: [], final: true }
	];

	/** Fog banks, anchored to the right edge. Bank n stays until area n + 1 opens, so they stack. */
	const FOG_WIDTHS = [1360, 1020, 590, 248];

	/** Treasure at the end of each side passage: (column, row) and the level that leads to it. */
	const TREASURES: { id: string; cell: Cell; after: string; key?: boolean }[] = [
		{ id: 'c1', cell: [1, 3], after: '3b' },
		{ id: 'c2', cell: [1, 1], after: '4b' },
		{ id: 'c3', cell: [4, 0], after: '18' },
		{ id: 'c4', cell: [6, 3], after: '7b' },
		{ id: 'c5', cell: [8, 3], after: '33' },
		{ id: 'c6', cell: [7, 0], after: '9b' },
		{ id: 'c7', cell: [10, 2], after: '16b' },
		{ id: 'c8', cell: [15, 3], after: '20b', key: true },
		{ id: 'c9', cell: [18, 2], after: '21b' },
		{ id: 'c10', cell: [16, 1], after: '22b' }
	];
	const PORTAL_CELL: Cell = [19, 0];

	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levels = $derived(worldDef?.levels ?? []);
	const stars = $derived(gameStore.getWorldStarCount(worldId));

	function progress(num: string) {
		return gameStore.getLevelProgress(worldId, num);
	}

	function done(num: string): boolean {
		return !!progress(num)?.completed;
	}

	/** A level is reached once the level leading to it is finished. */
	function reached(num: string): boolean {
		if (num === '1') return true;
		return levels.some((l) => (l.connectTo1 === num || l.connectTo2 === num) && done(l.levelNumber));
	}

	function isUnlocked(num: string): boolean {
		const def = levels.find((l) => l.levelNumber === num);
		if (!def) return false;
		return reached(num) && stars >= (def.minimumStarsToUnlock ?? 0);
	}

	const highestArea = $derived.by(() => {
		let area = 1;
		for (const entry of AREA_ENTRY) {
			if (!reached(entry.level) || stars < entry.stars) break;
			area++;
		}
		return area;
	});

	function left(cell: Cell, size: number): number {
		return PAD_X + (cell[0] + 0.5) * CELL_W - size / 2;
	}
	function top(cell: Cell, size: number): number {
		return (cell[1] + 0.5) * CELL_H - size / 2;
	}

	// The native page only draws the buttons of areas that are out of the fog.
	const orbs = $derived(
		SPOTS.filter((s) => s.area <= highestArea).map((s) => {
			const bonus = s.num.includes('b');
			const unlocked = isUnlocked(s.num);
			const earned = progress(s.num)?.numberOfStars ?? 0;
			const kind = bonus ? 'bonus' : 'level';
			return {
				num: s.num, bonus, unlocked, earned,
				icon: unlocked ? `${kind}_${Math.min(earned, 3)}` : `${kind}_locked`,
				left: left(s.cell, ORB),
				top: top(s.cell, ORB)
			};
		})
	);

	/** The furthest main level that can be played; it pulses, as in the native app. */
	const currentNum = $derived(
		orbs.filter((o) => !o.bonus && o.unlocked).reduce((best, o) => (parseInt(o.num) > parseInt(best) ? o.num : best), '1')
	);

	// Collected map items live in storage, which is not reactive; bump this to re-read them.
	let itemsVersion = $state(0);
	function collected(id: string): boolean {
		void itemsVersion;
		return gameStore.isMapItemCollected(worldId, id);
	}

	const hasKey = $derived(collected(KEY_ID));

	function gateOpen(gate: Gate): boolean {
		if (gate.final) return hasKey && done(FINAL_LEVEL);
		return stars >= gate.stars && gate.needs.every(reached);
	}

	const portalOpen = $derived(hasKey && done(FINAL_LEVEL) && stars >= PORTAL_STARS);

	const treasures = $derived(
		TREASURES.filter((t) => !(t.key && hasKey)).map((t) => {
			const opened = collected(`classic_${t.id}`);
			return {
				...t,
				opened,
				ready: done(t.after) && !opened,
				left: left(t.cell, TREASURE),
				top: top(t.cell, TREASURE)
			};
		})
	);

	// ── Scale the native canvas to the height we are given ──────────────────
	let viewportH = $state(0);
	let scroller = $state<HTMLDivElement | null>(null);
	const scale = $derived(viewportH > 0 ? viewportH / MAP_H : 1);

	onMount(() => {
		// Open on the level the player is up to.
		requestAnimationFrame(() => {
			const spot = SPOTS.find((s) => s.num === currentNum);
			if (!scroller || !spot) return;
			const x = (PAD_X + (spot.cell[0] + 0.5) * CELL_W) * scale;
			scroller.scrollLeft = Math.max(0, x - scroller.clientWidth / 2);
		});
	});

	// ── Interaction ─────────────────────────────────────────────────────────
	let activeNode = $state<MapNode | null>(null);
	const activeLevelDef = $derived(
		activeNode && worldDef ? getLevelByNumber(worldDef, activeNode.levelNumber ?? '') : undefined
	);
	let notice = $state('');
	let noticeTimer: ReturnType<typeof setTimeout> | undefined;
	let lastPrize = $state<{ id: string; coins: number } | null>(null);

	function say(text: string) {
		notice = text;
		clearTimeout(noticeTimer);
		noticeTimer = setTimeout(() => (notice = ''), 2600);
	}

	function openLevel(num: string) {
		if (!isUnlocked(num)) {
			const need = levels.find((l) => l.levelNumber === num)?.minimumStarsToUnlock ?? 0;
			say(reached(num) && need > stars ? `Level ${num} needs ${need} stars. You have ${stars}.` : `Finish the level before ${num} first.`);
			return;
		}
		activeNode = { id: `classic-${num}`, type: num.includes('b') ? 'bonus_level' : 'level', levelNumber: num } as MapNode;
	}

	function pressGate(gate: Gate) {
		if (gateOpen(gate)) return;
		if (gate.final) say('This gate needs the key and the last level.');
		else say(stars < gate.stars ? `This gate needs ${gate.stars} stars. You have ${stars}.` : 'Finish the levels before this gate first.');
	}

	function openTreasure(t: (typeof treasures)[number]) {
		if (!t.ready) {
			if (!t.opened) say(`Finish level ${t.after} to reach this.`);
			return;
		}
		gameStore.collectMapItem(worldId, t.key ? KEY_ID : `classic_${t.id}`);
		itemsVersion++;
		if (t.key) {
			say('Key found.');
			return;
		}
		// The native chest pays a random 100 to 999 coins.
		const coins = 100 + Math.floor(Math.random() * 900);
		gameStore.addCoins(coins);
		lastPrize = { id: t.id, coins };
	}

	function pressPortal() {
		if (portalOpen) void goto(`${base}/campaign/worlds/${worldId + 1}`);
		else if (!done(FINAL_LEVEL) || !hasKey) say('The portal needs the key and the last level.');
		else say(`The portal needs ${PORTAL_STARS} stars. You have ${stars}.`);
	}
</script>

<div class="classic" style="background-image: url('{base}/images/background_blue_1.png');">
	<div class="scroller" bind:this={scroller} bind:clientHeight={viewportH}>
		<div class="canvas" style="--s: {scale}; width: {MAP_W * scale}px; height: {MAP_H * scale}px;">
			<img class="maze" src="{base}/images/classic/campaign_background_1.png" alt="" draggable="false" />

			{#each GATES as gate, i}
				{@const open = gateOpen(gate)}
				<button
					type="button"
					class="gate"
					style="left: calc(var(--s) * {gate.x}px); top: calc(var(--s) * {gate.y}px); width: calc(var(--s) * {gate.w}px); height: calc(var(--s) * {gate.h}px); transform: rotate({gate.rotate}deg);"
					aria-label={open ? 'Open gate' : gate.final ? 'Gate, needs the key' : `Gate, needs ${gate.stars} stars`}
					onclick={() => pressGate(gate)}
				>
					<img src="{base}/images/classic/{open ? 'gate_opened' : 'gate4'}.png" alt="" draggable="false" />
				</button>
				{#if !open}
					<span
						class="gate-label"
						class:small={gate.small}
						style="left: calc(var(--s) * {gate.x + gate.w / 2}px); top: calc(var(--s) * {gate.y + gate.h / 2}px);"
						aria-hidden="true"
					>
						{#if gate.final}
							<img src="{base}/images/classic/key6.png" alt="" draggable="false" />
						{:else}
							{gate.stars}
						{/if}
					</span>
				{/if}
			{/each}

			{#each orbs as orb (orb.num)}
				<button
					type="button"
					class="orb"
					class:locked={!orb.unlocked}
					class:current={orb.num === currentNum}
					style="left: calc(var(--s) * {orb.left}px); top: calc(var(--s) * {orb.top}px);"
					aria-label="{orb.bonus ? 'Bonus level' : 'Level'} {orb.num}{orb.unlocked ? `, ${orb.earned} stars` : ', locked'}"
					onclick={() => openLevel(orb.num)}
				>
					<img src="{base}/images/classic/{orb.icon}.png" alt="" draggable="false" />
					{#if !orb.bonus}
						<span class="orb-num">{orb.num}</span>
					{/if}
				</button>
			{/each}

			{#each treasures as t, i (t.id)}
				<button
					type="button"
					class="treasure"
					class:wobble={!t.opened}
					style="left: calc(var(--s) * {t.left}px); top: calc(var(--s) * {t.top}px); --delay: {(i * 1.7) % 9}s;"
					aria-label={t.key ? 'Key' : t.opened ? 'Opened chest' : 'Chest'}
					onclick={() => openTreasure(t)}
				>
					<img
						src="{base}/images/{t.key ? 'classic/key6' : lastPrize?.id === t.id ? 'classic/coin_pile' : t.opened ? 'classic/opened_chest' : 'chest'}.png"
						alt=""
						draggable="false"
					/>
					{#if lastPrize?.id === t.id}
						<span class="prize">{lastPrize.coins}</span>
					{/if}
				</button>
			{/each}

			<button
				type="button"
				class="portal"
				class:shut={!portalOpen}
				style="left: calc(var(--s) * {left(PORTAL_CELL, 40)}px); top: calc(var(--s) * {top(PORTAL_CELL, 60)}px);"
				aria-label={portalOpen ? 'Portal to the next world' : 'Portal, closed'}
				onclick={pressPortal}
			>
				<img src="{base}/images/portal_purple.gif" alt="" draggable="false" />
			</button>

			{#each FOG_WIDTHS as width, i}
				{#if highestArea <= i + 1}
					<img
						class="fog"
						src="{base}/images/classic/fog.png"
						alt=""
						draggable="false"
						style="width: calc(var(--s) * {width}px);"
					/>
				{/if}
			{/each}
		</div>
	</div>

	<nav class="dock" aria-label="Campaign shortcuts" style="background-image: url('{base}/images/classic/background_blue_3.png');">
		<div class="dock-bar">
			<a href="{base}/equip" aria-label="Equip"><img src="{base}/images/classic/toolkit.png" alt="" /></a>
			<a href="{base}/campaign/worlds" aria-label="All worlds"><img src="{base}/images/classic/home_button_icon.png" alt="" /></a>
			<a href="{base}/shop" aria-label="Shop"><img src="{base}/images/classic/shop_button_image.png" alt="" /></a>
		</div>
	</nav>

	{#if notice}
		<p class="notice" role="status">{notice}</p>
	{/if}
</div>

{#if activeNode}
	<EncounterCard {worldId} node={activeNode} levelDef={activeLevelDef} onclose={() => (activeNode = null)} />
{/if}

<style>
	.classic {
		position: relative;
		display: flex;
		flex-direction: column;
		width: 100%;
		height: 100%;
		background-size: cover;
		background-position: center;
		overflow: hidden;
	}

	.scroller {
		flex: 1;
		min-height: 0;
		overflow-x: auto;
		overflow-y: hidden;
		overscroll-behavior-x: contain;
		scrollbar-width: thin;
	}

	.canvas {
		position: relative;
		background: #fff;
	}

	.maze {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		user-select: none;
	}

	.orb,
	.gate,
	.treasure,
	.portal {
		position: absolute;
		padding: 0;
		border: 0;
		background: transparent;
		cursor: pointer;
	}

	.orb {
		width: calc(var(--s) * 80px);
		height: calc(var(--s) * 80px);
		transition: transform 120ms ease, opacity 120ms ease;
	}

	.orb img,
	.gate img,
	.treasure img,
	.portal img {
		width: 100%;
		height: 100%;
		object-fit: contain;
		pointer-events: none;
		user-select: none;
	}

	.orb:hover { opacity: 0.8; }
	.orb:active { transform: scale(0.95); opacity: 0.6; }
	.orb.locked { cursor: default; }

	.orb-num {
		position: absolute;
		inset: 0;
		display: grid;
		place-items: center;
		color: #fff;
		font-size: calc(var(--s) * 16px);
		pointer-events: none;
	}

	.orb.locked .orb-num { color: #d3d3d3; }

	.treasure {
		width: calc(var(--s) * 60px);
		height: calc(var(--s) * 60px);
	}

	.portal {
		width: calc(var(--s) * 40px);
		height: calc(var(--s) * 60px);
		border: 1px solid #000;
		border-radius: 50%;
		overflow: hidden;
	}

	.portal img { object-fit: fill; }
	.portal.shut { cursor: default; }

	.prize {
		position: absolute;
		left: 0;
		right: 0;
		top: calc(var(--s) * -14px);
		color: gold;
		font-weight: 700;
		font-size: calc(var(--s) * 16px);
		text-align: center;
		text-shadow: 0 1px 2px #000;
		pointer-events: none;
	}

	@media (prefers-reduced-motion: no-preference) {
		.orb.current { animation: orb-pulse 2s ease-in-out infinite; }
		.treasure.wobble img { animation: chest-wobble 9s ease-in-out var(--delay, 0s) infinite; }
	}

	@keyframes orb-pulse {
		0%, 100% { transform: scale(1); opacity: 0.7; }
		50% { transform: scale(1.1); opacity: 1; }
	}

	@keyframes chest-wobble {
		0%, 78%, 100% { transform: rotate(0deg); }
		83% { transform: rotate(10deg); }
		92% { transform: rotate(-10deg); }
	}

	.gate:hover { opacity: 0.85; }

	.gate-label {
		position: absolute;
		transform: translate(-50%, -50%);
		color: gold;
		font-weight: 700;
		font-size: calc(var(--s) * 12px);
		line-height: 1;
		pointer-events: none;
	}

	.gate-label.small { font-size: calc(var(--s) * 9px); }

	.gate-label img {
		display: block;
		width: calc(var(--s) * 20px);
		height: calc(var(--s) * 20px);
	}

	.fog {
		position: absolute;
		top: 0;
		right: 0;
		height: 100%;
		opacity: 0.9;
		pointer-events: none;
		user-select: none;
	}

	.dock {
		flex: 0 0 auto;
		display: grid;
		place-items: center;
		padding: 10px 0 12px;
		background-size: cover;
		background-position: center;
	}

	.dock-bar {
		display: flex;
		align-items: center;
		justify-content: space-around;
		width: min(85%, 420px);
		height: 75px;
		border-radius: 10px;
		background: rgba(255, 255, 255, 0.3);
	}

	.dock-bar a {
		display: grid;
		place-items: center;
		height: 70px;
		transition: transform 120ms ease, opacity 120ms ease;
	}

	.dock-bar a:hover { opacity: 0.78; }
	.dock-bar a:active { transform: scale(0.95); opacity: 0.6; }
	.dock-bar img { height: 62px; width: auto; }

	.notice {
		position: absolute;
		left: 50%;
		bottom: 112px;
		transform: translateX(-50%);
		margin: 0;
		padding: 8px 14px;
		border-radius: 10px;
		background: rgba(0, 0, 0, 0.78);
		color: #fff;
		font-size: 0.9rem;
		white-space: nowrap;
	}
</style>
