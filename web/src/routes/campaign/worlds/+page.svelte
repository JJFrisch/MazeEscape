<!--
  The worlds picker as the native app draws it (WorldsPage.xaml): one tall card
  per world that you swipe between, the world's art behind a name band, a small
  score panel, and the Play World button.
-->
<script lang="ts">
	import { base } from '$app/paths';
	import { goto } from '$app/navigation';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { getAllWorlds } from '$lib/core/levels';
	import { getWorldMapLayout } from '$lib/core/campaignMapLayout';

	const worlds = getAllWorlds();
	const ART: Record<number, string> = {
		1: 'background_maze_3.png',
		2: 'space_background11.png',
		3: 'classic/carousel_maze_4.png'
	};

	// A world opens when the previous world's portal opens: same star count and
	// final-level rule the campaign map applies to the portal.
	function isPortalOpen(worldId: number): boolean {
		const portal = getWorldMapLayout(worldId)?.nodes.find((n) => n.type === 'portal');
		const needed = portal?.starsRequired ?? 300;
		const finalLevelDone = portal?.levelNumber
			? !!gameStore.getLevelProgress(worldId, portal.levelNumber)?.completed
			: true;
		return gameStore.getWorldStarCount(worldId) >= needed && finalLevelDone;
	}

	const cards = $derived(
		worlds.map((world, i) => {
			const main = world.levels.filter((l) => /^\d+$/.test(l.levelNumber));
			const beaten = main
				.filter((l) => gameStore.getLevelProgress(world.worldId, l.levelNumber)?.completed)
				.reduce((top, l) => Math.max(top, parseInt(l.levelNumber)), 0);
			return {
				world,
				locked: world.locked && i > 0 && !isPortalOpen(worlds[i - 1].worldId),
				empty: world.levels.length === 0,
				beaten,
				total: main.length,
				stars: gameStore.getWorldStarCount(world.worldId)
			};
		})
	);

	let track = $state<HTMLDivElement | null>(null);
	let index = $state(0);
	let notice = $state('');
	let noticeTimer: ReturnType<typeof setTimeout> | undefined;

	function say(text: string) {
		notice = text;
		clearTimeout(noticeTimer);
		noticeTimer = setTimeout(() => (notice = ''), 2600);
	}

	function show(i: number) {
		const card = track?.children[i] as HTMLElement | undefined;
		if (!track || !card) return;
		track.scrollTo({ left: card.offsetLeft - track.offsetLeft, behavior: 'smooth' });
	}

	function onScroll() {
		if (!track) return;
		const step = (track.children[0] as HTMLElement).offsetWidth + 20;
		index = Math.round(track.scrollLeft / step);
	}

	function choose(card: (typeof cards)[number]) {
		if (card.locked) say('Defeat the previous world to unlock this one.');
		else if (card.empty) say('World Coming Soon!');
		else void goto(`${base}/campaign/worlds/${card.world.worldId}`);
	}
</script>

<svelte:head>
	<title>Campaign Worlds – Maze Escape: Pathbound</title>
</svelte:head>

<div class="worlds">
	<header class="titles">
		<p class="game">Maze Escape</p>
		<h1>Worlds</h1>
	</header>

	<div class="track" bind:this={track} onscroll={onScroll}>
		{#each cards as card (card.world.worldId)}
			<button
				type="button"
				class="card"
				class:locked={card.locked}
				style="background-image: url('{base}/images/{ART[card.world.worldId]}');"
				aria-label="{card.world.worldName}{card.locked ? ', locked' : ''}"
				onclick={() => choose(card)}
			>
				<span class="name">{card.world.worldName}</span>

				{#if card.locked}
					<img class="lock" src="{base}/images/lock.png" alt="" draggable="false" />
				{:else}
					<span class="panel">
						<img src="{base}/images/classic/grid_background.png" alt="" draggable="false" />
						<span class="score beaten">{card.beaten} / {card.total}</span>
						<span class="score stars">{card.stars} Stars</span>
					</span>
					<img class="play" src="{base}/images/classic/play_world_button.png" alt="Play World" draggable="false" />
				{/if}
			</button>
		{/each}
	</div>

	{#if index > 0}
		<button type="button" class="arrow prev" aria-label="Previous world" onclick={() => show(index - 1)}>
			<img src="{base}/images/classic/arrow.png" alt="" />
		</button>
	{/if}
	{#if index < cards.length - 1}
		<button type="button" class="arrow next" aria-label="Next world" onclick={() => show(index + 1)}>
			<img src="{base}/images/classic/arrow.png" alt="" />
		</button>
	{/if}

	<a class="home" href="{base}/" aria-label="Home">
		<img src="{base}/images/classic/home_button_icon.png" alt="" />
	</a>

	{#if notice}
		<p class="notice" role="status">{notice}</p>
	{/if}
</div>

<style>
	.worlds {
		position: relative;
		height: calc(100dvh - var(--header-height) - 3rem);
		min-height: 520px;
		max-width: 700px;
		margin: 0 auto;
		background: #000;
		overflow: hidden;
	}

	.titles {
		position: absolute;
		inset: 20px 0 auto;
		z-index: 2;
		text-align: center;
		pointer-events: none;
	}

	.game {
		margin: 0;
		color: whitesmoke;
		font-size: 1.4rem;
		line-height: 1.5;
	}

	.titles h1 {
		margin: 6px 0 0;
		color: lightblue;
		font-size: 1.1rem;
		font-weight: 400;
	}

	.track {
		display: flex;
		gap: 20px;
		height: 100%;
		overflow-x: auto;
		overflow-y: hidden;
		scroll-snap-type: x mandatory;
		scrollbar-width: none;
	}

	.track::-webkit-scrollbar { display: none; }

	.card {
		position: relative;
		flex: 0 0 100%;
		height: 100%;
		padding: 0;
		border: 0;
		scroll-snap-align: start;
		background-color: #000;
		background-size: cover;
		background-position: center;
		color: #fff;
		font: inherit;
		cursor: pointer;
	}

	.card.locked::before {
		content: '';
		position: absolute;
		inset: 0;
		background: rgba(0, 0, 0, 0.3);
	}

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
		font-size: 20px;
		font-weight: 700;
	}

	.panel {
		position: absolute;
		top: 50%;
		left: 50%;
		width: 250px;
		transform: translate(-50%, -75%);
	}

	.panel img {
		display: block;
		width: 100%;
	}

	.score {
		position: absolute;
		top: 60%;
		width: 50%;
		color: #222;
		font-size: 12px;
		font-weight: 700;
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

	.card:hover .play { transform: translateX(-50%) scale(1.04); }

	.lock {
		position: absolute;
		top: 42%;
		left: 50%;
		width: 30%;
		transform: translateX(-50%);
	}

	.arrow {
		position: absolute;
		top: 50%;
		z-index: 2;
		width: 50px;
		height: 50px;
		padding: 0;
		border: 0;
		background: transparent;
		transform: translateY(-50%);
		cursor: pointer;
	}

	.arrow img { width: 100%; height: 100%; object-fit: contain; }
	.arrow.prev { left: 0; }
	.arrow.next { right: 0; }
	.arrow.next img { transform: rotate(180deg); }

	.home {
		position: absolute;
		left: 50%;
		bottom: 3%;
		z-index: 2;
		height: 55px;
		transform: translateX(-50%);
	}

	.home img { height: 100%; width: auto; }

	.notice {
		position: absolute;
		left: 50%;
		bottom: 90px;
		z-index: 3;
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
