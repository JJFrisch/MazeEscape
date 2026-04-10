<script lang="ts">
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { DEITY_CATALOG, getDeityMasteryRewards } from '$lib/core/deities';
	import { getSkinById } from '$lib/core/skins';
	import { POWERUP_COSTS } from '$lib/core/types';
	import type { PowerupName } from '$lib/core/types';
	import { ACHIEVEMENT_CATALOG, type AchievementCategory, type AchievementDef } from '$lib/core/achievements';

	// ── Tab state ──────────────────────────────────────────────
	type Tab = 'satchel' | 'forge' | 'chronicles' | 'trophies' | 'bestiary';
	let activeTab = $state<Tab>('satchel');
	let forgeMessage = $state('');

	// ── Satchel helpers ────────────────────────────────────────
	function getOwned(name: PowerupName): number {
		const p = gameStore.player;
		switch (name) {
			case 'hint':             return p.hintsOwned;
			case 'extraTime':        return p.extraTimesOwned;
			case 'extraMoves':       return p.extraMovesOwned;
			case 'compass':          return p.compassOwned ?? 0;
			case 'hourglass':        return p.hourglassOwned ?? 0;
			case 'blinkScroll':      return p.blinkScrollsOwned ?? 0;
			case 'streakShield':     return p.streakShieldsOwned ?? 0;
			case 'doubleCoinsToken': return p.doubleCoinsTokensOwned ?? 0;
		}
	}

	const RARITY_COLORS: Record<string, string> = {
		common:   '#94a3b8',
		uncommon: '#34d399',
		rare:     '#38bdf8',
	};

	// ── Bestiary helpers ───────────────────────────────────────
	const MASTERY_MILESTONES = [20, 80, 120] as const;

	function getMastery(algo: string): number {
		return (gameStore.player.algoMasteryCount ?? {})[algo as keyof typeof gameStore.player.algoMasteryCount] ?? 0;
	}

	function getMasteryClaims(algo: string) {
		return gameStore.player.masteryRewardsClaimed?.[algo as keyof typeof gameStore.player.masteryRewardsClaimed] ?? { item20: false, coins80: false, skin120: false };
	}

	function masteryTier(count: number): 0 | 1 | 2 | 3 {
		if (count >= 120) return 3;
		if (count >= 80) return 2;
		if (count >= 20) return 1;
		return 0;
	}

	const MASTERY_TIER_LABELS = ['Uninitiated', 'Favored', 'Ascendant', 'Mythic'];
	const MASTERY_TIER_COLORS = ['#475569', '#34d399', '#38bdf8', '#a78bfa'];
	const ACHIEVEMENT_CATEGORY_ORDER: AchievementCategory[] = ['Daily', 'Exploration', 'Mastery', 'Collection', 'Prestige'];
	const TROPHY_ACHIEVEMENT_IDS = ['world1_clear', 'world2_clear', 'world3_clear', 'century_explorer'] as const;

	type ForgeRecipe = {
		id: string;
		name: string;
		description: string;
		shardCost: number;
		ingredients: Array<{ name: PowerupName; amount: number }>;
		rewards: Array<{ name: PowerupName; amount: number }>;
		rarity: 'common' | 'uncommon' | 'rare';
	};

	const FORGE_RECIPES: ForgeRecipe[] = [
		{
			id: 'trailfinder-compass',
			name: 'Trailfinder Compass',
			description: 'Bind three common insights into a compass that reveals the next stretch of the way.',
			shardCost: 5,
			ingredients: [
				{ name: 'hint', amount: 2 },
				{ name: 'extraMoves', amount: 1 }
			],
			rewards: [{ name: 'compass', amount: 1 }],
			rarity: 'common'
		},
		{
			id: 'tempered-hourglass',
			name: 'Tempered Hourglass',
			description: 'Fuse extra time and a steady stride into a brief pause in the clock.',
			shardCost: 8,
			ingredients: [
				{ name: 'extraTime', amount: 2 },
				{ name: 'extraMoves', amount: 1 }
			],
			rewards: [{ name: 'hourglass', amount: 1 }],
			rarity: 'uncommon'
		},
		{
			id: 'blink-transcript',
			name: 'Blink Transcript',
			description: 'Distill a compass and an hourglass into a rare scroll of relocation.',
			shardCost: 10,
			ingredients: [
				{ name: 'compass', amount: 1 },
				{ name: 'hourglass', amount: 1 }
			],
			rewards: [{ name: 'blinkScroll', amount: 1 }],
			rarity: 'rare'
		},
		{
			id: 'merchant-sigil',
			name: 'Merchant Sigil',
			description: 'A bright token etched for prosperous runs ahead.',
			shardCost: 12,
			ingredients: [
				{ name: 'hint', amount: 2 },
				{ name: 'compass', amount: 1 }
			],
			rewards: [{ name: 'doubleCoinsToken', amount: 1 }],
			rarity: 'uncommon'
		},
		{
			id: 'warden-aegis',
			name: 'Warden Aegis',
			description: 'Set spare momentum into a shield that guards a fragile streak.',
			shardCost: 10,
			ingredients: [
				{ name: 'extraMoves', amount: 2 },
				{ name: 'extraTime', amount: 1 }
			],
			rewards: [{ name: 'streakShield', amount: 1 }],
			rarity: 'rare'
		}
	];

	function getAchievementEntry(id: string) {
		return gameStore.player.achievements?.[id] ?? { progress: 0, unlocked: false };
	}

	function getAchievementProgress(def: AchievementDef): number {
		return Math.min(getAchievementEntry(def.id).progress ?? 0, def.target);
	}

	function isAchievementUnlocked(def: AchievementDef): boolean {
		return getAchievementEntry(def.id).unlocked ?? false;
	}

	function getAchievementDate(def: AchievementDef): string {
		return getAchievementEntry(def.id).dateUnlocked ?? '';
	}

	function getAchievementRewardLabel(def: AchievementDef): string {
		if (def.rewardType === 'powerup') {
			const rewardDef = POWERUP_COSTS.find((powerup) => powerup.name === def.rewardPowerup);
			return `${def.rewardAmount}x ${rewardDef?.displayName ?? 'Powerup'}`;
		}
		if (def.rewardType === 'coins') return `${def.rewardAmount.toLocaleString()} coins`;
		if (def.rewardType === 'gems') return `${def.rewardAmount} gem${def.rewardAmount === 1 ? '' : 's'}`;
		if (def.rewardType === 'shards') return `${def.rewardAmount} shard${def.rewardAmount === 1 ? '' : 's'}`;
		return 'Trophy etched in the wall';
	}

	function canCraft(recipe: ForgeRecipe): boolean {
		if ((gameStore.player.crystalShards ?? 0) < recipe.shardCost) return false;
		return recipe.ingredients.every((ingredient) => getOwned(ingredient.name) >= ingredient.amount);
	}

	function craftRecipe(recipe: ForgeRecipe) {
		if (!canCraft(recipe)) {
			forgeMessage = 'You are missing shards or ingredients for that recipe.';
			return;
		}

		if (!gameStore.spendCrystalShards(recipe.shardCost)) {
			forgeMessage = 'The forge sputtered. Your shard bank did not cover the cost.';
			return;
		}

		for (const ingredient of recipe.ingredients) {
			for (let count = 0; count < ingredient.amount; count += 1) {
				gameStore.usePowerup(ingredient.name);
			}
		}

		for (const reward of recipe.rewards) {
			for (let count = 0; count < reward.amount; count += 1) {
				gameStore.addPowerup(reward.name);
			}
		}

		forgeMessage = `${recipe.name} completed. The forge yielded ${recipe.rewards.map((reward) => `${reward.amount}x ${POWERUP_COSTS.find((powerup) => powerup.name === reward.name)?.displayName ?? reward.name}`).join(', ')}.`;
	}

	function getUnlockedAchievementCount(): number {
		return ACHIEVEMENT_CATALOG.filter((achievement) => isAchievementUnlocked(achievement)).length;
	}

	function getUnlockedTrophyCount(): number {
		const worldHonors = TROPHY_ACHIEVEMENT_IDS.filter((id) => getAchievementEntry(id).unlocked).length;
		const masteryHonors = DEITY_CATALOG.reduce((total, deity) => {
			const claims = getMasteryClaims(deity.algorithm);
			return total + (claims.item20 ? 1 : 0) + (claims.coins80 ? 1 : 0) + (claims.skin120 ? 1 : 0);
		}, 0);
		return worldHonors + masteryHonors;
	}

	// Track expanded bestiary cards
	let expandedDeity = $state<string | null>(null);
</script>

<svelte:head>
	<title>Codex – Maze Escape: Pathbound</title>
</svelte:head>

<div class="codex-page">
	<!-- Header -->
	<div class="page-header">
		<div>
			<div class="page-eyebrow">
				<span class="eyebrow-dot"></span>
				Adventurer's Tome
			</div>
			<h1 class="page-title">Codex</h1>
			<p class="page-sub">Your satchel, forge, trophies, chronicles, and bestiary.</p>
		</div>

		<!-- Currency chips -->
		<div class="currency-chips">
			<div class="chip coin-chip">
				<img src="/images/coin.png" alt="" class="chip-coin" aria-hidden="true" />
				<span>{gameStore.player.coinCount.toLocaleString()}</span>
			</div>
			<div class="chip shard-chip">✦ {gameStore.player.crystalShards ?? 0}</div>
			{#if (gameStore.player.gemCount ?? 0) > 0}
				<div class="chip gem-chip">💎 {gameStore.player.gemCount}</div>
			{/if}
		</div>
	</div>

	<!-- Tab bar -->
	<div class="tab-bar" role="tablist">
		<button
			class="tab-btn"
			class:active={activeTab === 'satchel'}
			role="tab"
			aria-selected={activeTab === 'satchel'}
			onclick={() => activeTab = 'satchel'}
		>
			<span class="tab-icon">🎒</span>
			Satchel
		</button>
		<button
			class="tab-btn"
			class:active={activeTab === 'forge'}
			role="tab"
			aria-selected={activeTab === 'forge'}
			onclick={() => activeTab = 'forge'}
		>
			<span class="tab-icon">🔥</span>
			Forge
		</button>
		<button
			class="tab-btn"
			class:active={activeTab === 'chronicles'}
			role="tab"
			aria-selected={activeTab === 'chronicles'}
			onclick={() => activeTab = 'chronicles'}
		>
			<span class="tab-icon">🏅</span>
			Chronicles
		</button>
		<button
			class="tab-btn"
			class:active={activeTab === 'trophies'}
			role="tab"
			aria-selected={activeTab === 'trophies'}
			onclick={() => activeTab = 'trophies'}
		>
			<span class="tab-icon">🏆</span>
			Trophies
		</button>
		<button
			class="tab-btn"
			class:active={activeTab === 'bestiary'}
			role="tab"
			aria-selected={activeTab === 'bestiary'}
			onclick={() => activeTab = 'bestiary'}
		>
			<span class="tab-icon">📖</span>
			Bestiary
		</button>
	</div>

	<!-- ── SATCHEL TAB ────────────────────────────────────────── -->
	{#if activeTab === 'satchel'}
		<div class="tab-content" role="tabpanel">

			{#if gameStore.player.doubleCoinsActive}
				<div class="active-banner">
					<span>🪙🪙</span>
					<div>
						<strong>Double Coins Active</strong>
						<span>Your next maze completion earns 2× coins.</span>
					</div>
				</div>
			{/if}

			{#each [
				{ label: 'Navigation Aids', items: ['hint', 'compass'] as PowerupName[] },
				{ label: 'Time & Movement', items: ['extraTime', 'extraMoves', 'hourglass', 'blinkScroll'] as PowerupName[] },
				{ label: 'Economy & Protection', items: ['doubleCoinsToken', 'streakShield'] as PowerupName[] },
			] as group}
				<div class="satchel-group">
					<h2 class="group-title">{group.label}</h2>
					<div class="satchel-grid">
						{#each group.items as name}
							{@const def = POWERUP_COSTS.find(p => p.name === name)!}
							{@const owned = getOwned(name)}
							{@const color = RARITY_COLORS[def.rarity]}
							<div
								class="satchel-card"
								class:empty={owned === 0}
								style="--item-color:{color};"
							>
								<div class="satchel-icon-wrap" style="background:color-mix(in srgb, {color} 12%, transparent); border-color:color-mix(in srgb, {color} 30%, transparent);">
									<span class="satchel-icon">{def.icon}</span>
								</div>
								<div class="satchel-body">
									<div class="satchel-top">
										<span class="satchel-name">{def.displayName}</span>
										<span class="satchel-rarity" style="color:{color};">{def.rarity}</span>
									</div>
									<p class="satchel-desc">{def.description}</p>
									<p class="satchel-flavor">{def.flavorText}</p>
								</div>
								<div class="satchel-qty" class:qty-zero={owned === 0} style="--qty-color:{color};">
									×{owned}
								</div>
							</div>
						{/each}
					</div>
				</div>
			{/each}
		</div>

	<!-- ── FORGE TAB ─────────────────────────────────────────── -->
	{:else if activeTab === 'forge'}
		<div class="tab-content forge-layout" role="tabpanel">
			<div class="forge-bank">
				<div class="forge-bank-header">
					<span class="forge-kicker">Crystal Shard Bank</span>
					<h2>Fuel for upgraded relics</h2>
					<p>Every star won in the maze leaves residue in the forge. Spend shards to transmute common tools into rarer ones.</p>
				</div>

				<div class="shard-vault">
					<div class="shard-orb" aria-hidden="true">✦</div>
					<div>
						<span class="shard-count-label">Stored shards</span>
						<strong class="shard-count">{gameStore.player.crystalShards ?? 0}</strong>
					</div>
				</div>

				<div class="recipe-progress-list">
					{#each FORGE_RECIPES as recipe}
						<div class="recipe-progress-row">
							<div class="recipe-progress-head">
								<span>{recipe.name}</span>
								<span>{Math.min(gameStore.player.crystalShards ?? 0, recipe.shardCost)} / {recipe.shardCost}</span>
							</div>
							<div class="recipe-progress-track">
								<div class="recipe-progress-fill" style="width:{Math.min(((gameStore.player.crystalShards ?? 0) / recipe.shardCost) * 100, 100)}%;"></div>
							</div>
						</div>
					{/each}
				</div>

				{#if forgeMessage}
					<div class="forge-message">{forgeMessage}</div>
				{/if}
			</div>

			<div class="forge-recipes">
				{#each FORGE_RECIPES as recipe}
					<div class="forge-card" style="--recipe-color:{RARITY_COLORS[recipe.rarity]};">
						<div class="forge-card-top">
							<div>
								<span class="forge-rarity">{recipe.rarity}</span>
								<h3>{recipe.name}</h3>
							</div>
							<span class="forge-shard-cost">✦ {recipe.shardCost}</span>
						</div>

						<p class="forge-copy">{recipe.description}</p>

						<div class="forge-meta-block">
							<span class="forge-meta-title">Ingredients</span>
							<div class="forge-chip-list">
								{#each recipe.ingredients as ingredient}
									{@const def = POWERUP_COSTS.find((powerup) => powerup.name === ingredient.name)!}
									<div class="forge-chip" class:met={getOwned(ingredient.name) >= ingredient.amount}>
										<span>{def.icon}</span>
										<span>{ingredient.amount}x {def.displayName}</span>
										<small>{getOwned(ingredient.name)} owned</small>
									</div>
								{/each}
							</div>
						</div>

						<div class="forge-meta-block">
							<span class="forge-meta-title">Result</span>
							<div class="forge-chip-list reward-list">
								{#each recipe.rewards as reward}
									{@const rewardDef = POWERUP_COSTS.find((powerup) => powerup.name === reward.name)!}
									<div class="forge-chip reward-chip met">
										<span>{rewardDef.icon}</span>
										<span>{reward.amount}x {rewardDef.displayName}</span>
									</div>
								{/each}
							</div>
						</div>

						<button class="forge-btn" onclick={() => craftRecipe(recipe)} disabled={!canCraft(recipe)}>
							{canCraft(recipe) ? 'Craft' : 'Need more materials'}
						</button>
					</div>
				{/each}
			</div>
		</div>

	<!-- ── CHRONICLES TAB ────────────────────────────────────── -->
	{:else if activeTab === 'chronicles'}
		<div class="tab-content" role="tabpanel">
			<div class="chronicles-hero">
				<div>
					<span class="forge-kicker">Guild Commendations</span>
					<h2>Progress etched into the record</h2>
					<p>Every daily oath, world clear, and deity milestone is recorded here with its current standing.</p>
				</div>
				<div class="chronicles-summary">
					<strong>{getUnlockedAchievementCount()} / {ACHIEVEMENT_CATALOG.length}</strong>
					<span>commendations unlocked</span>
				</div>
			</div>

			<div class="chronicles-track">
				<div class="chronicles-track-fill" style="width:{(getUnlockedAchievementCount() / ACHIEVEMENT_CATALOG.length) * 100}%;"></div>
			</div>

			{#each ACHIEVEMENT_CATEGORY_ORDER as category}
				<div class="chronicle-group">
					<h2 class="group-title">{category}</h2>
					<div class="achievement-grid">
						{#each ACHIEVEMENT_CATALOG.filter((achievement) => achievement.category === category) as achievement}
							{@const progress = getAchievementProgress(achievement)}
							{@const unlockedState = isAchievementUnlocked(achievement)}
							<div class="achievement-card" class:locked={!unlockedState}>
								<div class="achievement-head">
									<div class="achievement-icon">{achievement.icon}</div>
									<div>
										<h3>{achievement.name}</h3>
										<p>{achievement.description}</p>
									</div>
								</div>

								<div class="achievement-progress-row">
									<span>{progress} / {achievement.target}</span>
									<span>{unlockedState ? 'Unlocked' : 'In Progress'}</span>
								</div>
								<div class="achievement-progress-track">
									<div class="achievement-progress-fill" style="width:{(progress / achievement.target) * 100}%;"></div>
								</div>

								<div class="achievement-footer">
									<span class="achievement-reward">Reward: {getAchievementRewardLabel(achievement)}</span>
									{#if unlockedState}
										<span class="achievement-date">{getAchievementDate(achievement)}</span>
									{/if}
								</div>
							</div>
						{/each}
					</div>
				</div>
			{/each}
		</div>

	<!-- ── TROPHIES TAB ──────────────────────────────────────── -->
	{:else if activeTab === 'trophies'}
		<div class="tab-content" role="tabpanel">
			<div class="trophies-hero">
				<div>
					<span class="forge-kicker">Reward Wall</span>
					<h2>Earned prestige and mastery relics</h2>
					<p>Major clears live here, along with each deity's reward ladder, including mastery skin unlocks at Champion rank.</p>
				</div>
				<div class="chronicles-summary">
					<strong>{getUnlockedTrophyCount()} / {TROPHY_ACHIEVEMENT_IDS.length + DEITY_CATALOG.length * 3}</strong>
					<span>rewards visible</span>
				</div>
			</div>

			<div class="trophy-section">
				<h2 class="group-title">World Honors</h2>
				<div class="trophy-grid">
					{#each TROPHY_ACHIEVEMENT_IDS as trophyId}
						{@const achievement = ACHIEVEMENT_CATALOG.find((entry) => entry.id === trophyId)!}
						{@const unlockedState = getAchievementEntry(trophyId).unlocked}
						<div class="trophy-card" class:locked={!unlockedState}>
							<div class="trophy-icon">{unlockedState ? achievement.icon : '❔'}</div>
							<h3>{unlockedState ? achievement.name : 'Unknown Honor'}</h3>
							<p>{unlockedState ? achievement.description : 'Continue your ascent through the planes to reveal this honor.'}</p>
							<span class="trophy-badge">{unlockedState ? 'Etched' : 'Locked'}</span>
						</div>
					{/each}
				</div>
			</div>

			<div class="trophy-section">
				<h2 class="group-title">Mastery Rewards</h2>
				<div class="mastery-trophy-grid">
					{#each DEITY_CATALOG as deity}
						{@const mastery = getMastery(deity.algorithm)}
						{@const claims = getMasteryClaims(deity.algorithm)}
						{@const rewardDef = getDeityMasteryRewards(deity.algorithm)}
						{@const masterySkin = getSkinById(rewardDef.skin120Id)}
						<div class="mastery-trophy-card" style="--deity-color:{deity.color}; --deity-dim:{deity.colorDim};">
							<div class="mastery-trophy-head">
								<div class="deity-sigil-wrap small">
									<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke={deity.color} stroke-width="1.5" aria-hidden="true">
										<path d={deity.sigilPath} />
									</svg>
								</div>
								<div>
									<h3>{deity.name}</h3>
									<p>{mastery} encounters recorded</p>
								</div>
							</div>

							<div class="mastery-reward-list">
								{#each [
									{ at: 20, label: `${rewardDef.item20.amount}x ${POWERUP_COSTS.find((powerup) => powerup.name === rewardDef.item20.powerup)?.displayName ?? rewardDef.item20.label}`, icon: '🎁', claimed: claims.item20 },
									{ at: 80, label: `${rewardDef.coins80.toLocaleString()} Coins`, icon: '🪙', claimed: claims.coins80 },
									{ at: 120, label: masterySkin?.name ?? rewardDef.skin120Name, icon: '✨', claimed: claims.skin120 }
								] as reward}
									<div class="mastery-reward-row" class:unlocked={reward.claimed}>
										<span class="reward-threshold">{reward.icon} {reward.at}</span>
										<span>{reward.label}</span>
										<strong>{reward.claimed ? 'Claimed' : 'Locked'}</strong>
									</div>
								{/each}
							</div>
						</div>
					{/each}
				</div>
			</div>
		</div>

	<!-- ── BESTIARY TAB ──────────────────────────────────────── -->
	{:else}
		<div class="tab-content" role="tabpanel">
			<div class="bestiary-intro">
				<p>The fifteen Algorithm Deities who conjure every labyrinth you traverse. Study them. Master them.</p>
				{#each [DEITY_CATALOG.filter(d => getMasteryClaims(d.algorithm).item20).length] as totalMastered}
				<div class="mastery-summary">
					<div class="mastery-bar-wrap">
						<div class="mastery-bar-track">
							<div class="mastery-bar-fill" style="width:{(totalMastered / 15) * 100}%;"></div>
						</div>
					</div>
					<span class="mastery-summary-text">{totalMastered}/15 deities studied</span>
				</div>
				{/each}
			</div>

			<div class="bestiary-list">
				{#each DEITY_CATALOG as deity}
					{@const mastery = getMastery(deity.algorithm)}
					{@const claims = getMasteryClaims(deity.algorithm)}
					{@const rewardDef = getDeityMasteryRewards(deity.algorithm)}
					{@const tier = masteryTier(mastery)}
					{@const nextMilestone = MASTERY_MILESTONES.find(m => m > mastery) ?? 120}
					{@const progressToNext = tier < 3 ? (mastery - (MASTERY_MILESTONES[tier - 1] ?? 0)) / (nextMilestone - (MASTERY_MILESTONES[tier - 1] ?? 0)) : 1}
					{@const isExpanded = expandedDeity === deity.algorithm}

					<div
						class="deity-card"
						class:expanded={isExpanded}
						style="--deity-color:{deity.color}; --deity-dim:{deity.colorDim};"
					>
						<!-- Card header (always visible) -->
						<button
							class="deity-header"
							onclick={() => expandedDeity = isExpanded ? null : deity.algorithm}
							aria-expanded={isExpanded}
						>
							<!-- Sigil -->
							<div class="deity-sigil-wrap">
								<svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke={deity.color} stroke-width="1.5" aria-hidden="true">
									<path d={deity.sigilPath} />
								</svg>
							</div>

							<!-- Identity -->
							<div class="deity-identity">
								<span class="deity-name">{deity.name}</span>
								<span class="deity-domain" style="color:{deity.color};">{deity.domain}</span>
							</div>

							<!-- Mastery ring + tier -->
							<div class="deity-mastery">
								<div class="mastery-ring-wrap">
									<svg width="44" height="44" viewBox="0 0 44 44" class="mastery-ring">
										<!-- Track -->
										<circle cx="22" cy="22" r="18" fill="none" stroke="rgba(255,255,255,0.08)" stroke-width="3"/>
										<!-- Fill — progress to next milestone -->
										<circle
											cx="22" cy="22" r="18"
											fill="none"
											stroke={deity.color}
											stroke-width="3"
											stroke-dasharray="{progressToNext * 113.1} 113.1"
											stroke-dashoffset="28.3"
											stroke-linecap="round"
											opacity={tier < 3 ? 0.9 : 1}
										/>
										<!-- Count label -->
										<text x="22" y="26" text-anchor="middle" fill="white" font-size="11" font-weight="700" font-family="var(--font-display)">{mastery}</text>
									</svg>
								</div>
								<span class="tier-label" style="color:{MASTERY_TIER_COLORS[tier]};">{MASTERY_TIER_LABELS[tier]}</span>
							</div>

							<div class="expand-chevron" class:rotated={isExpanded}>
								<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" aria-hidden="true">
									<polyline points="6 9 12 15 18 9"/>
								</svg>
							</div>
						</button>

						<!-- Expandable body -->
						{#if isExpanded}
							<div class="deity-body">
								<!-- Quote -->
								<blockquote class="deity-quote" style="border-color:{deity.colorDim};">
									"{deity.quote}"
								</blockquote>

								<!-- Description -->
								<p class="deity-description">{deity.description}</p>

								<!-- Trait bars -->
								<div class="trait-section">
									{#each [
										{ label: 'Corridor Length', val: deity.traits.corridorLength },
										{ label: 'Dead End Density', val: deity.traits.deadEndDensity },
										{ label: 'Difficulty', val: deity.traits.difficulty },
										{ label: 'Randomness', val: deity.traits.randomness },
									] as trait}
										<div class="trait-row">
											<span class="trait-label">{trait.label}</span>
											<div class="trait-track">
												<div
													class="trait-fill"
													style="width:{trait.val * 20}%; background:{deity.color}; box-shadow: 0 0 8px {deity.color}60;"
												></div>
											</div>
											<span class="trait-val">{trait.val}/5</span>
										</div>
									{/each}
								</div>

								<!-- Mastery milestone track -->
								<div class="milestone-track">
									<div class="milestone-header">
										<span class="milestone-title">Mastery Progress</span>
										<span class="milestone-count" style="color:{deity.color};">{mastery} / 120 encounters</span>
									</div>
									<div class="milestone-bar-outer">
										<div class="milestone-bar-fill" style="width:{Math.min((mastery / 120) * 100, 100)}%; background: linear-gradient(90deg, {deity.color}80, {deity.color});"></div>
										{#each MASTERY_MILESTONES as m}
											<div
												class="milestone-pip"
												class:reached={mastery >= m}
												style="left:{(m / 120) * 100}%; --pip-color:{deity.color};"
											>
												<span class="pip-label">{m}</span>
											</div>
										{/each}
									</div>
									<div class="milestone-rewards">
										{#each [
											{ at: 20, label: `${rewardDef.item20.amount}x ${POWERUP_COSTS.find((powerup) => powerup.name === rewardDef.item20.powerup)?.displayName ?? rewardDef.item20.label}`, icon: '🎁', claimed: claims.item20 },
											{ at: 80, label: `${rewardDef.coins80.toLocaleString()} coins`, icon: '🪙', claimed: claims.coins80 },
											{ at: 120, label: rewardDef.skin120Name, icon: '✨', claimed: claims.skin120 },
										] as reward}
											<div class="reward-pip" class:unlocked={reward.claimed}>
												<span class="reward-icon">{reward.icon}</span>
												<span class="reward-label">{reward.at} — {reward.label}</span>
											</div>
										{/each}
									</div>
								</div>

								{#if tier < 3}
									<p class="next-milestone-hint" style="color:{deity.color};">
										{nextMilestone - mastery} encounter{nextMilestone - mastery !== 1 ? 's' : ''} until next milestone
									</p>
								{:else}
									<p class="mastery-complete" style="color:{deity.color};">
										✦ Champion of {deity.name} ✦
									</p>
								{/if}
							</div>
						{/if}
					</div>
				{/each}
			</div>
		</div>
	{/if}
</div>

<style>
	@keyframes fade-up {
		from { opacity: 0; transform: translateY(14px); }
		to   { opacity: 1; transform: translateY(0); }
	}
	@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }

	.codex-page {
		max-width: 900px;
		margin: 0 auto;
		animation: fade-up 0.4s ease both;
	}

	/* ── Header ─────────────────────────────────── */
	.page-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: var(--space-6);
		margin-bottom: var(--space-8);
		flex-wrap: wrap;
	}
	.page-eyebrow {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 5px 14px;
		background: color-mix(in srgb, var(--color-accent-primary) 10%, transparent);
		border: 1px solid color-mix(in srgb, var(--color-accent-primary) 24%, transparent);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: color-mix(in srgb, var(--color-accent-primary) 72%, white 28%);
		margin-bottom: var(--space-3);
	}
	.eyebrow-dot {
		width: 6px; height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 6px var(--color-accent-primary);
		animation: pulse 2s ease-in-out infinite;
	}
	.page-title {
		font-family: var(--font-display);
		font-size: clamp(1.8rem, 4vw, 2.5rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		margin-bottom: var(--space-2);
	}
	.page-sub { color: var(--color-text-secondary); font-size: var(--text-base); }

	.currency-chips { display: flex; gap: var(--space-2); align-items: center; flex-wrap: wrap; }
	.chip {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		padding: 7px 14px;
		border-radius: var(--radius-full);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
	}
	.coin-chip {
		background: rgba(245,158,11,0.08);
		border: 1px solid rgba(245,158,11,0.25);
		color: var(--color-accent-gold);
	}
	.chip-coin { width: 16px; height: 16px; object-fit: contain; }
	.gem-chip {
		background: rgba(124,58,237,0.12);
		border: 1px solid rgba(124,58,237,0.3);
		color: #a78bfa;
	}
	.shard-chip {
		background: rgba(34,197,94,0.08);
		border: 1px solid rgba(34,197,94,0.26);
		color: #86efac;
	}

	/* ── Tab bar ─────────────────────────────────── */
	.tab-bar {
		display: flex;
		gap: var(--space-2);
		border-bottom: 1px solid var(--color-border);
		margin-bottom: var(--space-8);
		padding-bottom: 0;
	}
	.tab-btn {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		padding: var(--space-3) var(--space-5);
		background: none;
		border: none;
		border-bottom: 2px solid transparent;
		border-radius: var(--radius-md) var(--radius-md) 0 0;
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		color: var(--color-text-muted);
		cursor: pointer;
		transition: all var(--transition-fast);
		margin-bottom: -1px;
	}
	.tab-btn:hover { color: var(--color-text-primary); background: rgba(255,255,255,0.04); }
	.tab-btn.active {
		color: var(--color-accent-primary);
		border-bottom-color: var(--color-accent-primary);
		background: color-mix(in srgb, var(--color-accent-primary) 6%, transparent);
	}
	.tab-icon { font-size: 1rem; }

	/* ── Tab content ─────────────────────────────── */
	.tab-content { animation: fade-up 0.25s ease both; }

	/* Active banner */
	.active-banner {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-4) var(--space-5);
		background: rgba(245,158,11,0.10);
		border: 1px solid rgba(245,158,11,0.35);
		border-radius: var(--radius-xl);
		margin-bottom: var(--space-6);
		font-size: 1.3rem;
	}
	.active-banner strong { display: block; font-family: var(--font-display); font-weight: 700; color: var(--color-accent-gold); }
	.active-banner span { font-size: var(--text-sm); color: var(--color-text-secondary); }

	/* ── Satchel ─────────────────────────────────── */
	.satchel-group { margin-bottom: var(--space-8); }
	.group-title {
		font-family: var(--font-display);
		font-size: var(--text-sm);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: var(--color-text-muted);
		margin-bottom: var(--space-4);
		padding-left: var(--space-1);
	}
	.satchel-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
		gap: var(--space-4);
	}
	.satchel-card {
		display: flex;
		align-items: flex-start;
		gap: var(--space-4);
		padding: var(--space-4);
		background: var(--color-bg-card);
		border: 1px solid color-mix(in srgb, var(--item-color) 25%, rgba(255,255,255,0.06));
		border-radius: var(--radius-xl);
		backdrop-filter: blur(8px);
		transition: all var(--transition-fast);
		position: relative;
	}
	.satchel-card:hover { transform: translateY(-2px); box-shadow: 0 8px 32px rgba(0,0,0,0.3); }
	.satchel-card.empty { opacity: 0.5; filter: grayscale(0.4); }

	.satchel-icon-wrap {
		width: 48px; height: 48px;
		display: flex;
		align-items: center;
		justify-content: center;
		border: 1px solid;
		border-radius: var(--radius-lg);
		flex-shrink: 0;
	}
	.satchel-icon { font-size: 1.5rem; }
	.satchel-body { flex: 1; min-width: 0; }
	.satchel-top { display: flex; align-items: baseline; justify-content: space-between; gap: var(--space-2); margin-bottom: 4px; }
	.satchel-name { font-family: var(--font-display); font-weight: 700; font-size: var(--text-base); color: var(--color-text-primary); }
	.satchel-rarity { font-size: 9px; font-weight: 800; letter-spacing: 0.10em; text-transform: uppercase; }
	.satchel-desc { font-size: var(--text-sm); color: var(--color-text-secondary); line-height: 1.5; margin-bottom: 4px; }
	.satchel-flavor { font-size: var(--text-xs); font-style: italic; color: var(--color-text-muted); line-height: 1.5; }

	.satchel-qty {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 800;
		color: var(--qty-color, #38bdf8);
		flex-shrink: 0;
		min-width: 36px;
		text-align: right;
	}
	.satchel-qty.qty-zero { color: rgba(255,255,255,0.2); }

	/* ── Forge ───────────────────────────────────── */
	.forge-layout {
		display: grid;
		grid-template-columns: minmax(260px, 320px) minmax(0, 1fr);
		gap: var(--space-5);
	}
	.forge-bank,
	.forge-card,
	.chronicles-hero,
	.trophies-hero,
	.achievement-card,
	.trophy-card,
	.mastery-trophy-card {
		background: var(--color-bg-card);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-xl);
	}
	.forge-bank {
		padding: var(--space-5);
		display: flex;
		flex-direction: column;
		gap: var(--space-5);
		position: sticky;
		top: calc(var(--header-height) + var(--space-4));
		height: fit-content;
	}
	.forge-kicker {
		display: inline-block;
		font-size: 10px;
		font-weight: 800;
		letter-spacing: 0.14em;
		text-transform: uppercase;
		color: #86efac;
		margin-bottom: var(--space-2);
	}
	.forge-bank-header h2,
	.chronicles-hero h2,
	.trophies-hero h2 {
		font-family: var(--font-display);
		font-size: clamp(1.3rem, 2.6vw, 1.8rem);
		margin-bottom: var(--space-2);
	}
	.forge-bank-header p,
	.chronicles-hero p,
	.trophies-hero p {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		line-height: 1.6;
	}
	.shard-vault {
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-4);
		background: linear-gradient(135deg, rgba(34,197,94,0.10), rgba(56,189,248,0.08));
		border: 1px solid rgba(134,239,172,0.18);
		border-radius: var(--radius-xl);
	}
	.shard-orb {
		width: 56px;
		height: 56px;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 50%;
		background: rgba(255,255,255,0.08);
		font-size: 1.6rem;
		box-shadow: 0 0 24px rgba(134,239,172,0.18);
	}
	.shard-count-label {
		display: block;
		font-size: var(--text-xs);
		text-transform: uppercase;
		letter-spacing: 0.08em;
		color: var(--color-text-muted);
		margin-bottom: 4px;
	}
	.shard-count {
		font-family: var(--font-display);
		font-size: clamp(1.7rem, 5vw, 2.2rem);
		color: #dcfce7;
	}
	.recipe-progress-list {
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
	}
	.recipe-progress-head {
		display: flex;
		justify-content: space-between;
		gap: var(--space-3);
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
		margin-bottom: 6px;
	}
	.recipe-progress-track,
	.chronicles-track,
	.achievement-progress-track {
		height: 8px;
		background: rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		overflow: hidden;
	}
	.recipe-progress-fill,
	.chronicles-track-fill,
	.achievement-progress-fill {
		height: 100%;
		background: linear-gradient(90deg, #86efac, #38bdf8);
		border-radius: var(--radius-full);
	}
	.forge-message {
		padding: var(--space-3) var(--space-4);
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.18);
		border-radius: var(--radius-lg);
		font-size: var(--text-sm);
		color: #bae6fd;
		line-height: 1.55;
	}
	.forge-recipes {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
		gap: var(--space-4);
	}
	.forge-card {
		padding: var(--space-5);
		border-color: color-mix(in srgb, var(--recipe-color) 22%, rgba(255,255,255,0.08));
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
	}
	.forge-card-top {
		display: flex;
		justify-content: space-between;
		gap: var(--space-3);
		align-items: flex-start;
	}
	.forge-card h3,
	.achievement-card h3,
	.trophy-card h3,
	.mastery-trophy-card h3 {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		color: var(--color-text-primary);
	}
	.forge-rarity {
		font-size: 10px;
		font-weight: 800;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: var(--recipe-color);
	}
	.forge-shard-cost {
		padding: 6px 10px;
		border-radius: var(--radius-full);
		background: rgba(255,255,255,0.04);
		font-size: var(--text-xs);
		font-weight: 700;
		white-space: nowrap;
	}
	.forge-copy {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		line-height: 1.6;
	}
	.forge-meta-block {
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}
	.forge-meta-title {
		font-size: 10px;
		font-weight: 800;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: var(--color-text-muted);
	}
	.forge-chip-list {
		display: flex;
		flex-wrap: wrap;
		gap: var(--space-2);
	}
	.forge-chip {
		display: grid;
		gap: 2px;
		padding: 8px 10px;
		border-radius: var(--radius-lg);
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.08);
		font-size: var(--text-xs);
		color: var(--color-text-secondary);
	}
	.forge-chip.met {
		border-color: rgba(134,239,172,0.28);
		background: rgba(134,239,172,0.08);
		color: #dcfce7;
	}
	.forge-chip small {
		font-size: 10px;
		color: var(--color-text-muted);
	}
	.reward-chip {
		min-width: 120px;
	}
	.forge-btn {
		margin-top: auto;
		padding: 12px 16px;
		border: none;
		border-radius: var(--radius-lg);
		background: linear-gradient(135deg, color-mix(in srgb, var(--recipe-color) 80%, white 20%), var(--recipe-color));
		color: #07111d;
		font-weight: 800;
		cursor: pointer;
	}
	.forge-btn:disabled {
		opacity: 0.45;
		cursor: default;
	}

	/* ── Chronicles ─────────────────────────────── */
	.chronicles-hero,
	.trophies-hero {
		padding: var(--space-5);
		margin-bottom: var(--space-6);
		display: flex;
		justify-content: space-between;
		gap: var(--space-5);
		align-items: end;
		flex-wrap: wrap;
	}
	.chronicles-summary {
		display: grid;
		gap: 4px;
		text-align: right;
	}
	.chronicles-summary strong {
		font-family: var(--font-display);
		font-size: clamp(1.8rem, 4vw, 2.4rem);
	}
	.chronicles-summary span {
		font-size: var(--text-xs);
		text-transform: uppercase;
		letter-spacing: 0.12em;
		color: var(--color-text-muted);
	}
	.chronicles-track {
		margin-bottom: var(--space-6);
	}
	.chronicle-group {
		margin-bottom: var(--space-7);
	}
	.achievement-grid,
	.trophy-grid,
	.mastery-trophy-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
		gap: var(--space-4);
	}
	.achievement-card,
	.trophy-card,
	.mastery-trophy-card {
		padding: var(--space-5);
	}
	.achievement-card.locked,
	.trophy-card.locked {
		opacity: 0.72;
		filter: grayscale(0.35);
	}
	.achievement-head {
		display: flex;
		gap: var(--space-3);
		margin-bottom: var(--space-4);
	}
	.achievement-icon,
	.trophy-icon {
		width: 46px;
		height: 46px;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 14px;
		background: rgba(255,255,255,0.05);
		font-size: 1.4rem;
		flex-shrink: 0;
	}
	.achievement-head p,
	.trophy-card p,
	.mastery-trophy-head p {
		margin-top: 4px;
		font-size: var(--text-sm);
		line-height: 1.55;
		color: var(--color-text-secondary);
	}
	.achievement-progress-row,
	.achievement-footer {
		display: flex;
		justify-content: space-between;
		gap: var(--space-3);
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		margin-bottom: var(--space-2);
		flex-wrap: wrap;
	}
	.achievement-progress-fill {
		background: linear-gradient(90deg, #38bdf8, #a78bfa);
	}
	.achievement-footer {
		margin-top: var(--space-3);
		margin-bottom: 0;
	}
	.achievement-reward {
		color: #bae6fd;
	}
	.achievement-date {
		color: #fef08a;
	}

	/* ── Trophies ───────────────────────────────── */
	.trophy-section {
		margin-bottom: var(--space-7);
	}
	.trophy-card {
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
	}
	.trophy-badge {
		align-self: flex-start;
		padding: 6px 10px;
		border-radius: var(--radius-full);
		background: rgba(250,204,21,0.10);
		border: 1px solid rgba(250,204,21,0.22);
		font-size: var(--text-xs);
		font-weight: 800;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: #facc15;
	}
	.mastery-trophy-card {
		border-color: color-mix(in srgb, var(--deity-color) 20%, rgba(255,255,255,0.08));
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
	}
	.deity-sigil-wrap.small {
		width: 42px;
		height: 42px;
	}
	.mastery-trophy-head {
		display: flex;
		gap: var(--space-3);
		align-items: center;
	}
	.mastery-reward-list {
		display: grid;
		gap: var(--space-2);
	}
	.mastery-reward-row {
		display: grid;
		grid-template-columns: auto 1fr auto;
		gap: var(--space-3);
		align-items: center;
		padding: 10px 12px;
		border-radius: var(--radius-lg);
		background: rgba(255,255,255,0.03);
		border: 1px solid rgba(255,255,255,0.06);
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}
	.mastery-reward-row.unlocked {
		background: color-mix(in srgb, var(--deity-color) 10%, transparent);
		border-color: color-mix(in srgb, var(--deity-color) 30%, transparent);
		color: white;
	}
	.reward-threshold {
		font-weight: 800;
		color: var(--deity-color);
	}

	/* ── Bestiary ─────────────────────────────────── */
	.bestiary-intro {
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
		margin-bottom: var(--space-6);
		padding: var(--space-5);
		background: rgba(255,255,255,0.02);
		border: 1px solid rgba(255,255,255,0.06);
		border-radius: var(--radius-xl);
	}
	.bestiary-intro p { color: var(--color-text-secondary); font-size: var(--text-base); line-height: 1.6; }
	.mastery-summary { display: flex; align-items: center; gap: var(--space-3); }
	.mastery-bar-wrap { flex: 1; }
	.mastery-bar-track {
		height: 6px;
		background: rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		overflow: hidden;
	}
	.mastery-bar-fill {
		height: 100%;
		background: linear-gradient(90deg, #38bdf8, #a78bfa);
		border-radius: var(--radius-full);
		transition: width 0.8s ease;
	}
	.mastery-summary-text { font-size: var(--text-sm); font-weight: 700; color: var(--color-text-muted); white-space: nowrap; }

	.bestiary-list { display: flex; flex-direction: column; gap: var(--space-3); }

	.deity-card {
		background: var(--color-bg-card);
		border: 1px solid color-mix(in srgb, var(--deity-color) 20%, rgba(255,255,255,0.07));
		border-radius: var(--radius-xl);
		overflow: hidden;
		transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
	}
	.deity-card:hover, .deity-card.expanded {
		border-color: color-mix(in srgb, var(--deity-color) 45%, transparent);
		box-shadow: 0 0 24px var(--deity-dim, rgba(56,189,248,0.1));
	}

	.deity-header {
		width: 100%;
		display: flex;
		align-items: center;
		gap: var(--space-4);
		padding: var(--space-4) var(--space-5);
		background: none;
		border: none;
		cursor: pointer;
		text-align: left;
		transition: background var(--transition-fast);
	}
	.deity-header:hover { background: rgba(255,255,255,0.025); }

	.deity-sigil-wrap {
		width: 48px; height: 48px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: var(--deity-dim, rgba(56,189,248,0.1));
		border: 1px solid color-mix(in srgb, var(--deity-color) 30%, transparent);
		border-radius: var(--radius-lg);
		flex-shrink: 0;
	}

	.deity-identity { flex: 1; min-width: 0; }
	.deity-name {
		display: block;
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: 2px;
	}
	.deity-domain {
		font-size: var(--text-xs);
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
	}

	.deity-mastery { display: flex; flex-direction: column; align-items: center; gap: 4px; flex-shrink: 0; }
	.mastery-ring { overflow: visible; }
	.tier-label { font-size: 9px; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; }

	.expand-chevron {
		color: var(--color-text-muted);
		transition: transform var(--transition-fast);
		flex-shrink: 0;
	}
	.expand-chevron.rotated { transform: rotate(180deg); }

	/* Deity body (expanded) */
	.deity-body {
		padding: 0 var(--space-5) var(--space-5);
		border-top: 1px solid rgba(255,255,255,0.06);
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
		animation: fade-up 0.2s ease both;
	}

	.deity-quote {
		margin: 0;
		padding: var(--space-3) var(--space-4);
		border-left: 2px solid var(--deity-dim, rgba(56,189,248,0.3));
		font-style: italic;
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.6;
	}
	.deity-description { font-size: var(--text-sm); color: var(--color-text-secondary); line-height: 1.65; }

	.trait-section { display: flex; flex-direction: column; gap: var(--space-2); }
	.trait-row { display: flex; align-items: center; gap: var(--space-3); }
	.trait-label {
		font-size: 10px; font-weight: 600; letter-spacing: 0.06em; text-transform: uppercase;
		color: var(--color-text-muted); width: 100px; flex-shrink: 0;
	}
	.trait-track { flex: 1; height: 4px; background: rgba(255,255,255,0.08); border-radius: var(--radius-full); overflow: hidden; }
	.trait-fill { height: 100%; border-radius: var(--radius-full); transition: width 0.6s ease; }
	.trait-val { font-size: 10px; color: var(--color-text-muted); font-weight: 600; width: 24px; text-align: right; }

	/* Milestone track */
	.milestone-track { display: flex; flex-direction: column; gap: var(--space-3); }
	.milestone-header { display: flex; justify-content: space-between; align-items: center; }
	.milestone-title {
		font-size: var(--text-xs); font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase;
		color: var(--color-text-muted);
	}
	.milestone-count { font-size: var(--text-sm); font-weight: 700; }

	.milestone-bar-outer {
		position: relative;
		height: 6px;
		background: rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		overflow: visible;
		margin-bottom: var(--space-2);
	}
	.milestone-bar-fill {
		height: 100%;
		border-radius: var(--radius-full);
		transition: width 0.8s ease;
	}
	.milestone-pip {
		position: absolute;
		top: 50%;
		transform: translate(-50%, -50%);
		width: 10px; height: 10px;
		background: rgba(255,255,255,0.12);
		border: 2px solid rgba(255,255,255,0.2);
		border-radius: 50%;
		transition: all var(--transition-fast);
	}
	.milestone-pip.reached {
		background: var(--pip-color, #38bdf8);
		border-color: var(--pip-color, #38bdf8);
		box-shadow: 0 0 8px var(--pip-color, #38bdf8);
	}
	.pip-label {
		position: absolute;
		bottom: -18px;
		left: 50%;
		transform: translateX(-50%);
		font-size: 8px;
		font-weight: 700;
		color: var(--color-text-muted);
		white-space: nowrap;
	}

	.milestone-rewards { display: flex; gap: var(--space-3); flex-wrap: wrap; margin-top: var(--space-2); }
	.reward-pip {
		display: flex;
		align-items: center;
		gap: var(--space-1);
		padding: 4px 10px;
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.08);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		transition: all var(--transition-fast);
	}
	.reward-pip.unlocked {
		background: color-mix(in srgb, var(--deity-color) 10%, transparent);
		border-color: color-mix(in srgb, var(--deity-color) 35%, transparent);
		color: var(--deity-color);
	}
	.reward-icon { font-size: 12px; }

	.next-milestone-hint, .mastery-complete {
		font-size: var(--text-sm);
		font-weight: 600;
		text-align: center;
		opacity: 0.8;
	}
	.mastery-complete {
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 700;
		letter-spacing: 0.08em;
		opacity: 1;
	}

	@media (max-width: 820px) {
		.forge-layout {
			grid-template-columns: 1fr;
		}
		.forge-bank {
			position: static;
		}
	}

	@media (max-width: 640px) {
		.tab-bar {
			flex-wrap: wrap;
		}
		.chronicles-summary {
			text-align: left;
		}
		.mastery-reward-row {
			grid-template-columns: 1fr;
		}
	}
</style>
