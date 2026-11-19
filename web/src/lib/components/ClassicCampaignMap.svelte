<!--
  ClassicCampaignMap.svelte

  The campaign map as the native (.NET MAUI) app draws it: a hand-drawn maze you
  scroll sideways, level buttons sitting in its cells, star gates, treasure at
  the ends of the side passages, and fog over the areas you have not reached.

  The layout of each world lives in $lib/core/classicMaps.
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds, getLevelByNumber } from '$lib/core/levels';
	import EncounterCard from '$lib/components/EncounterCard.svelte';
	import { getClassicMap, type Cell, type ClassicGate } from '$lib/core/classicMaps';
	import { isZoneUnlocked, zoneLockReason } from '$lib/core/zoneLocks';
	import type { MapNode } from '$lib/core/types';

	let { worldId }: { worldId: number } = $props();

	const cfg = $derived(getClassicMap(worldId)!);
	const PAD_X = 2;
	const TREASURE = 60;
	const KEY_ID = 'classic_k1';

	const cellW = $derived((cfg.mapW - PAD_X * 2) / cfg.cols);
	const cellH = $derived(cfg.mapH / cfg.rows);

	const allSpots = $derived(
		cfg.areas.flatMap((a, i) => [
			...a.main.map((cell, k) => ({ num: String(a.first + k), area: i + 1, cell })),
			...a.bonus.map((cell, k) => ({ num: `${a.firstBonus + k}b`, area: i + 1, cell }))
		])
	);
	// A boss stands in for the level whose cell it takes.
	const spots = $derived(allSpots.filter((s) => s.num !== cfg.boss?.replaces));

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
		if (cfg.areaEntry.length === 0) return cfg.areas.length;
		let area = 1;
		for (const entry of cfg.areaEntry) {
			if (!reached(entry.level) || stars < entry.stars) break;
			area++;
		}
		return area;
	});

	function left(cell: Cell, size: number): number {
		return PAD_X + (cell[0] + 0.5) * cellW - size / 2;
	}
	function top(cell: Cell, size: number): number {
		return (cell[1] + 0.5) * cellH - size / 2;
	}

	// The native page only draws the buttons of areas that are out of the fog.
	const orbs = $derived(
		spots.filter((s) => s.area <= highestArea).map((s) => {
			const bonus = s.num.includes('b');
			const unlocked = isUnlocked(s.num);
			const earned = progress(s.num)?.numberOfStars ?? 0;
			const kind = bonus ? cfg.bonusIcon : cfg.mainIcon;
			return {
				num: s.num, bonus, unlocked, earned,
				icon: unlocked ? `${kind}_${Math.min(earned, 3)}` : `${kind}_locked`,
				left: left(s.cell, cfg.orb),
				top: top(s.cell, cfg.orb)
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

	function gateOpen(gate: ClassicGate): boolean {
		if (gate.final) return hasKey && done(cfg.portal.finalLevel);
		return stars >= gate.stars && gate.needs.every(reached);
	}

	// Smaller than a level button and raised a little, so the disc clears the maze walls.
	const BOSS = 56;
	const BOSS_LIFT = 4;
	const boss = $derived.by(() => {
		const b = cfg.boss;
		const spot = b && allSpots.find((s) => s.num === b.replaces);
		if (!b || !spot) return null;
		return {
			num: b.num,
			unlocked: isUnlocked(b.num),
			beaten: done(b.num),
			left: left(spot.cell, BOSS),
			top: top(spot.cell, BOSS) - BOSS_LIFT
		};
	});

	function openBoss() {
		if (!boss) return;
		if (!boss.unlocked) {
			const need = levels.find((l) => l.levelNumber === boss.num)?.minimumStarsToUnlock ?? 0;
			say(reached(boss.num) && need > stars ? `The boss needs ${need} stars. You have ${stars}.` : 'Finish the level before the boss first.');
			return;
		}
		activeNode = { id: `classic-${boss.num}`, type: 'boss', levelNumber: boss.num } as MapNode;
	}

	// The native app points an arrow at the first level until it is finished.
	const ARROW = 64;
	const startArrow = $derived.by(() => {
		const first = spots.find((s) => s.num === '1');
		if (!first || done('1')) return null;
		const cx = PAD_X + (first.cell[0] + 0.5) * cellW;
		const cy = (first.cell[1] + 0.5) * cellH;
		const reach = cfg.orb * 0.3;
		// The art points down and to the right, with its tip at 82% across and 62% down.
		const mirrored = cx - reach - ARROW * 0.82 < 0;
		return {
			mirrored,
			left: mirrored ? cx + reach - ARROW * 0.18 : cx - reach - ARROW * 0.82,
			top: cy - reach - ARROW * 0.62
		};
	});

	const portalOpen = $derived(
		(hasKey || !cfg.portal.needsKey) && done(cfg.portal.finalLevel) && stars >= cfg.portal.stars
	);

	const treasures = $derived(
		cfg.treasures.map((t) => {
			const kind = t.kind ?? 'chest';
			const opened = collected(kind === 'key' ? KEY_ID : `classic_${t.id}`);
			return {
				...t,
				kind,
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
	const scale = $derived(viewportH > 0 ? viewportH / cfg.mapH : 1);

	onMount(() => {
		// Open on the level the player is up to. A player who has finished nothing
		// here starts at the left edge, where the map begins.
		requestAnimationFrame(() => {
			const spot = spots.find((s) => s.num === currentNum);
			if (!scroller || !spot || !spots.some((s) => done(s.num))) return;
			const x = (PAD_X + (spot.cell[0] + 0.5) * cellW) * scale;
			scroller.scrollLeft = Math.max(0, x - scroller.clientWidth / 2);
		});
	});

	// ── Interaction ─────────────────────────────────────────────────────────
	let activeNode = $state<MapNode | null>(null);
	const activeLevelDef = $derived(
		activeNode && worldDef ? getLevelByNumber(worldDef, activeNode.levelNumber ?? '') : undefined
	);
	let notice = $state('');
	// The dock's Equip and Shop buttons say what unlocks the Vault until it opens.
	const vaultOpen = $derived(isZoneUnlocked('vault'));
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

	function pressGate(gate: ClassicGate) {
		if (gateOpen(gate)) return;
		if (gate.final) say('This gate needs the key and the last level.');
		else say(stars < gate.stars ? `This gate needs ${gate.stars} stars. You have ${stars}.` : 'Finish the levels before this gate first.');
	}

	function openTreasure(t: (typeof treasures)[number]) {
		if (!t.ready) {
			if (!t.opened) say(`Finish level ${t.after} to reach this.`);
			return;
		}
		gameStore.collectMapItem(worldId, t.kind === 'key' ? KEY_ID : `classic_${t.id}`);
		itemsVersion++;
		if (t.kind === 'key') {
			say('Key found.');
		} else if (t.kind === 'skin' && t.skinId !== undefined) {
			gameStore.unlockSkin(t.skinId);
			say(`${t.skinName} skin unlocked.`);
		} else {
			// The native chest pays a random 100 to 999 coins.
			const coins = 100 + Math.floor(Math.random() * 900);
			gameStore.addCoins(coins);
			lastPrize = { id: t.id, coins };
		}
	}

	function treasureImage(t: (typeof treasures)[number]): string {
		if (t.kind === 'key') return 'classic/key6.webp';
		if (t.kind === 'skin') return `player_image${t.skinId}_icon.webp`;
		if (lastPrize?.id === t.id) return 'classic/coin_pile.webp';
		return t.opened ? cfg.openedChestImage : cfg.chestImage;
	}

	function pressPortal() {
		const p = cfg.portal;
		if (portalOpen) void goto(worldId === 1 ? `${base}/campaign/worlds/2` : `${base}/campaign/worlds`);
		else if (!done(p.finalLevel) || (p.needsKey && !hasKey)) say(p.needsKey ? 'The portal needs the key and the last level.' : 'The portal needs the last level.');
		else say(`The portal needs ${p.stars} stars. You have ${stars}.`);
	}
</script>

<div class="classic" style="background-image: url('{base}/images/{cfg.backdrop}');">
	<div class="scroller" bind:this={scroller} bind:clientHeight={viewportH}>
		<div class="canvas" style="--s: {scale}; --orb: {cfg.orb}px; --orb-font: {cfg.orbFont}px; --orb-locked: {cfg.lockedNumColor}; --gate-label: {cfg.gateLabelColor}; width: {cfg.mapW * scale}px; height: {cfg.mapH * scale}px;">
			<img class="maze" src="{base}/images/{cfg.mazeImage}" alt="" draggable="false" />

			{#each cfg.gates as gate}
				{@const open = gateOpen(gate)}
				<button
					type="button"
					class="gate"
					style="left: calc(var(--s) * {gate.x}px); top: calc(var(--s) * {gate.y}px); width: calc(var(--s) * {gate.w}px); height: calc(var(--s) * {gate.h}px); transform: rotate({gate.rotate}deg);"
					aria-label={open ? 'Open gate' : gate.final ? 'Gate, needs the key' : `Gate, needs ${gate.stars} stars`}
					onclick={() => pressGate(gate)}
				>
					<img src="{base}/images/{open ? 'classic/gate_opened.webp' : cfg.gateImage}" alt="" draggable="false" />
				</button>
				{#if !open}
					<span
						class="gate-label"
						class:small={gate.small}
						style="left: calc(var(--s) * {gate.x + gate.w / 2}px); top: calc(var(--s) * {gate.y + gate.h / 2}px);"
						aria-hidden="true"
					>
						{#if gate.final}
							<img src="{base}/images/classic/key6.webp" alt="" draggable="false" />
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
					<img src="{base}/images/{orb.icon}.webp" alt="" draggable="false" />
					{#if !orb.bonus}
						<span class="orb-num">{orb.num}</span>
					{/if}
				</button>
			{/each}

			{#if boss}
				<button
					type="button"
					class="boss"
					class:locked={!boss.unlocked}
					class:beaten={boss.beaten}
					style="left: calc(var(--s) * {boss.left}px); top: calc(var(--s) * {boss.top}px);"
					aria-label="Boss{boss.beaten ? ', beaten' : boss.unlocked ? '' : ', locked'}"
					onclick={openBoss}
				>☠</button>
			{/if}

			{#each treasures.filter((t) => !(t.opened && t.kind !== 'chest')) as t, i (t.id)}
				<button
					type="button"
					class="treasure"
					class:wobble={!t.opened}
					style="left: calc(var(--s) * {t.left}px); top: calc(var(--s) * {t.top}px); --delay: {(i * 1.7) % 9}s;"
					aria-label={t.kind === 'key' ? 'Key' : t.kind === 'skin' ? `${t.skinName} skin` : t.opened ? 'Opened chest' : 'Chest'}
					onclick={() => openTreasure(t)}
				>
					<img src="{base}/images/{treasureImage(t)}" alt="" draggable="false" />
					{#if lastPrize?.id === t.id}
						<span class="prize">{lastPrize.coins}</span>
					{/if}
				</button>
			{/each}

			<button
				type="button"
				class="portal"
				class:shut={!portalOpen}
				style="left: calc(var(--s) * {left(cfg.portal.cell, 40)}px); top: calc(var(--s) * {top(cfg.portal.cell, 60)}px);"
				aria-label={portalOpen ? 'Portal to the next world' : 'Portal, closed'}
				onclick={pressPortal}
			>
				<img src="{base}/images/portal_purple.gif" alt="" draggable="false" />
			</button>

			{#if startArrow}
				<img
					class="start-arrow"
					class:mirrored={startArrow.mirrored}
					src="{base}/images/classic/pointer_arrow.webp"
					alt=""
					draggable="false"
					style="left: calc(var(--s) * {startArrow.left}px); top: calc(var(--s) * {startArrow.top}px);"
				/>
			{/if}

			{#each cfg.fogWidths as width, i}
				{#if highestArea <= i + 1}
					<img
						class="fog"
						src="{base}/images/classic/fog.webp"
						alt=""
						draggable="false"
						style="width: calc(var(--s) * {width}px);"
					/>
				{/if}
			{/each}
		</div>
	</div>

	<nav class="dock" aria-label="Campaign shortcuts" style="background-image: url('{base}/images/{cfg.dockImage}');">
		<div class="dock-bar" class:dark={cfg.dockDark}>
			{#if vaultOpen}
				<a href="{base}/equip" aria-label="Equip"><img src="{base}/images/classic/toolkit.webp" alt="" /></a>
			{:else}
				<button type="button" aria-label="Equip" onclick={() => say(zoneLockReason('vault'))}><img src="{base}/images/classic/toolkit.webp" alt="" /></button>
			{/if}
			<a href="{base}/campaign/worlds" aria-label="All worlds"><img src="{base}/images/classic/home_button_icon.webp" alt="" /></a>
			{#if vaultOpen}
				<a href="{base}/shop" aria-label="Shop"><img src="{base}/images/classic/shop_button_image.webp" alt="" /></a>
			{:else}
				<button type="button" aria-label="Shop" onclick={() => say(zoneLockReason('vault'))}><img src="{base}/images/classic/shop_button_image.webp" alt="" /></button>
			{/if}
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
		width: calc(var(--s) * var(--orb));
		height: calc(var(--s) * var(--orb));
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
		font-size: calc(var(--s) * var(--orb-font));
		pointer-events: none;
	}

	.orb.locked .orb-num { color: var(--orb-locked); }

	.boss {
		position: absolute;
		box-sizing: border-box;
		width: calc(var(--s) * 56px);
		height: calc(var(--s) * 56px);
		padding: 0;
		border: calc(var(--s) * 3px) solid #fb923c;
		border-radius: 50%;
		background: #3a1708;
		color: #fdba74;
		font-size: calc(var(--s) * 30px);
		line-height: 1;
		cursor: pointer;
	}

	.boss.locked { border-color: #7c2d12; color: #7c2d12; cursor: default; }
	.boss.beaten { background: #f97316; color: #3a1708; }

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

	.start-arrow {
		position: absolute;
		width: calc(var(--s) * 64px);
		height: calc(var(--s) * 64px);
		pointer-events: none;
		user-select: none;
	}

	.start-arrow.mirrored { transform: scaleX(-1); }

	@media (prefers-reduced-motion: no-preference) {
		.start-arrow { animation: arrow-fade 2s ease-in-out 10; }
		.orb.current { animation: orb-pulse 2s ease-in-out infinite; }
		.treasure.wobble img { animation: chest-wobble 9s ease-in-out var(--delay, 0s) infinite; }
	}

	@keyframes arrow-fade {
		0%, 100% { opacity: 1; }
		50% { opacity: 0.2; }
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
		color: var(--gate-label);
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

	.dock-bar.dark { background: rgba(0, 0, 0, 0.6); }

	.dock-bar a,
	.dock-bar button {
		display: grid;
		place-items: center;
		height: 70px;
		transition: transform 120ms ease, opacity 120ms ease;
	}

	.dock-bar a:hover,
	.dock-bar button:hover { opacity: 0.78; }
	.dock-bar a:active,
	.dock-bar button:active { transform: scale(0.95); opacity: 0.6; }
	.dock-bar button { padding: 0; border: 0; background: none; cursor: pointer; }
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
