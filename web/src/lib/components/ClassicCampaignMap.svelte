<!--
  ClassicCampaignMap.svelte

  The campaign map as the native (.NET MAUI) app draws it: a hand-drawn maze you
  scroll sideways, glowing level orbs sitting in its cells, wooden star gates,
  and fog over the areas you have not reached.

  Every number here comes from CampaignPage.xaml / CampaignPage.xaml.cs. The
  native map is a 1700 x 470 canvas holding a 20 x 5 grid; positions below are
  in those units and the whole canvas scales to the available height.
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import { getWorldMapLayout } from '$lib/core/campaignMapLayout';
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

	/** (column, row) per level, in play order. Areas 1 to 5 from the native page. */
	const MAIN_CELLS: [number, number][] = [
		[3, 3], [3, 2], [2, 2], [2, 1],
		[1, 2], [1, 3], [2, 3], [2, 4],
		[4, 4], [5, 4], [5, 3], [4, 3],
		[5, 2], [6, 2], [6, 1], [5, 1],
		[7, 1], [8, 1], [8, 0], [7, 0]
	];
	const BONUS_CELLS: Record<string, [number, number]> = {
		'4b': [1, 1],
		'8b': [3, 4],
		'12b': [4, 2],
		'16b': [4, 1]
	};

	interface Gate {
		x: number; y: number; w: number; h: number;
		rotate: number;
		labelX: number; labelY: number;
		/** Level that must be reachable before the gate opens; null = the world portal. */
		opensFor: string | null;
	}
	const GATES: Gate[] = [
		{ x: 24, y: 243, w: 35, h: 80, rotate: 270, labelX: 38.5, labelY: 274.5, opensFor: '5' },
		{ x: 235, y: 91, w: 35, h: 98, rotate: 0, labelX: 250, labelY: 130.5, opensFor: '9' },
		{ x: 319, y: 187, w: 35, h: 98, rotate: 0, labelX: 334.2, labelY: 225.5, opensFor: '13' },
		{ x: 570, y: 94, w: 55, h: 96, rotate: 0, labelX: 589.2, labelY: 130.5, opensFor: '17' },
		{ x: 654.5, y: 375, w: 55, h: 96, rotate: 0, labelX: 674.5, labelY: 412.5, opensFor: null }
	];

	/** Width of the fog bank (anchored to the right edge) while each area is the furthest reached. */
	const FOG_WIDTH: Record<number, number> = { 1: 1360, 2: 1020, 3: 590, 4: 248 };

	const worldDef = $derived(getAllWorlds().find((w) => w.worldId === worldId));
	const levels = $derived(worldDef?.levels ?? []);
	const webLayout = $derived(getWorldMapLayout(worldId));
	const stars = $derived(gameStore.getWorldStarCount(worldId));
	const highestArea = $derived(gameStore.getHighestAreaUnlocked(worldId));
	const portalNode = $derived(webLayout?.nodes.find((n) => n.type === 'portal'));
	const gateStars = $derived([
		...(worldDef?.gateStarRequired ?? []),
		portalNode?.starsRequired ?? 0
	]);

	function progress(num: string) {
		return gameStore.getLevelProgress(worldId, num);
	}

	function isUnlocked(num: string): boolean {
		const def = levels.find((l) => l.levelNumber === num);
		if (!def) return false;
		if (stars < (def.minimumStarsToUnlock ?? 0)) return false;
		if (num === '1') return true;
		return levels.some(
			(l) => progress(l.levelNumber)?.completed && (l.connectTo1 === num || l.connectTo2 === num)
		);
	}

	const orbs = $derived(
		levels
			.map((level) => {
				const num = level.levelNumber;
				const bonus = num.includes('b');
				const cell = bonus ? BONUS_CELLS[num] : MAIN_CELLS[parseInt(num) - 1];
				if (!cell) return null;
				const unlocked = isUnlocked(num);
				const earned = progress(num)?.numberOfStars ?? 0;
				// The native orb art carries up to three stars; the web game awards up to five.
				const icon = unlocked ? `${bonus ? 'bonus' : 'level'}_${Math.min(earned, 3)}` : `${bonus ? 'bonus' : 'level'}_locked`;
				return {
					num, bonus, unlocked, earned, icon,
					left: PAD_X + (cell[0] + 0.5) * CELL_W - ORB / 2,
					top: (cell[1] + 0.5) * CELL_H - ORB / 2
				};
			})
			.filter((o): o is NonNullable<typeof o> => o !== null)
	);

	function gateOpen(i: number): boolean {
		const gate = GATES[i];
		if (stars < (gateStars[i] ?? 0)) return false;
		if (gate.opensFor) return isUnlocked(gate.opensFor);
		const last = portalNode?.levelNumber;
		return last ? !!progress(last)?.completed : true;
	}

	const fogWidth = $derived(FOG_WIDTH[highestArea] ?? 0);

	// ── Scale the native canvas to the height we are given ──────────────────
	let viewportH = $state(0);
	const scale = $derived(viewportH > 0 ? viewportH / MAP_H : 1);

	// ── Interaction ─────────────────────────────────────────────────────────
	let activeNode = $state<MapNode | null>(null);
	const activeLevelDef = $derived(
		activeNode && worldDef ? getLevelByNumber(worldDef, activeNode.levelNumber ?? '') : undefined
	);
	let notice = $state('');
	let noticeTimer: ReturnType<typeof setTimeout> | undefined;

	function say(text: string) {
		notice = text;
		clearTimeout(noticeTimer);
		noticeTimer = setTimeout(() => (notice = ''), 2600);
	}

	function openLevel(num: string) {
		if (!isUnlocked(num)) {
			const def = levels.find((l) => l.levelNumber === num);
			const need = def?.minimumStarsToUnlock ?? 0;
			say(need > stars ? `Level ${num} needs ${need} stars. You have ${stars}.` : `Finish the level before ${num} first.`);
			return;
		}
		activeNode =
			webLayout?.nodes.find((n) => n.levelNumber === num) ??
			({ id: `classic-${num}`, type: num.includes('b') ? 'bonus_level' : 'level', levelNumber: num } as MapNode);
	}

	function pressGate(i: number) {
		const need = gateStars[i] ?? 0;
		if (!gateOpen(i)) {
			say(stars < need ? `This gate needs ${need} stars. You have ${stars}.` : 'Finish the levels before this gate first.');
			return;
		}
		if (GATES[i].opensFor === null) void goto(`${base}/campaign/worlds/${worldId + 1}`);
		else say('Gate open.');
	}
</script>

<div class="classic" style="background-image: url('{base}/images/background_blue_1.png');">
	<div class="scroller" bind:clientHeight={viewportH}>
		<div class="canvas" style="--s: {scale}; width: {MAP_W * scale}px; height: {MAP_H * scale}px;">
			<img class="maze" src="{base}/images/classic/campaign_background_1.png" alt="" draggable="false" />

			{#each orbs as orb (orb.num)}
				<button
					type="button"
					class="orb"
					class:locked={!orb.unlocked}
					style="left: calc(var(--s) * {orb.left}px); top: calc(var(--s) * {orb.top}px);"
					aria-label="{orb.bonus ? 'Bonus level' : 'Level'} {orb.num}{orb.unlocked ? `, ${orb.earned} of 5 stars` : ', locked'}"
					onclick={() => openLevel(orb.num)}
				>
					<img src="{base}/images/classic/{orb.icon}.png" alt="" draggable="false" />
					{#if !orb.bonus}
						<span class="orb-num">{orb.num}</span>
					{/if}
				</button>
			{/each}

			{#each GATES as gate, i}
				{@const open = gateOpen(i)}
				<button
					type="button"
					class="gate"
					style="left: calc(var(--s) * {gate.x}px); top: calc(var(--s) * {gate.y}px); width: calc(var(--s) * {gate.w}px); height: calc(var(--s) * {gate.h}px); transform: rotate({gate.rotate}deg);"
					aria-label="{open ? 'Open gate' : `Gate, needs ${gateStars[i] ?? 0} stars`}"
					onclick={() => pressGate(i)}
				>
					<img src="{base}/images/classic/{open ? 'gate_opened' : 'gate4'}.png" alt="" draggable="false" />
				</button>
				{#if !open}
					<span
						class="gate-label"
						style="left: calc(var(--s) * {gate.labelX}px); top: calc(var(--s) * {gate.labelY}px);"
						aria-hidden="true"
					>{gateStars[i] ?? ''}</span>
				{/if}
			{/each}

			{#if fogWidth > 0}
				<img
					class="fog"
					src="{base}/images/classic/fog.png"
					alt=""
					draggable="false"
					style="width: calc(var(--s) * {fogWidth}px);"
				/>
			{/if}
		</div>
	</div>

	<nav class="dock" aria-label="Campaign shortcuts" style="background-image: url('{base}/images/classic/background_blue_3.png');">
		<div class="dock-bar">
			<a href="{base}/equip" aria-label="Equip"><img src="{base}/images/classic/toolkit.png" alt="" /></a>
			<a href="{base}/" aria-label="Home"><img src="{base}/images/classic/home_button_icon.png" alt="" /></a>
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
	.gate {
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
	.gate img {
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
		color: #d3d3d3;
		font-size: calc(var(--s) * 16px);
		pointer-events: none;
	}

	.gate:hover { opacity: 0.85; }

	.gate-label {
		position: absolute;
		color: gold;
		font-weight: 700;
		font-size: calc(var(--s) * 12px);
		line-height: 1;
		pointer-events: none;
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
