<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';

	let nameInput = $state(gameStore.player.playerName);
	let wallColor = $state(gameStore.player.wallColor || '#6366f1');
	let saved = $state(false);

	function saveName() {
		gameStore.setPlayerName(nameInput.trim());
		flash();
	}

	function saveWallColor(e: Event) {
		const target = e.target as HTMLInputElement;
		wallColor = target.value;
		gameStore.setWallColor(wallColor);
		flash();
	}

	function flash() {
		saved = true;
		setTimeout(() => saved = false, 1500);
	}

	async function syncNow() {
		await gameStore.syncNow();
	}

	async function signOut() {
		await authStore.signOut();
	}

	const presetColors = ['#6366f1', '#8b5cf6', '#ec4899', '#ef4444', '#f59e0b', '#10b981', '#06b6d4', '#ffffff'];
</script>

<svelte:head>
	<title>Settings – Maze Escape: Pathbound</title>
</svelte:head>

<div class="settings-page">
	<h1 class="page-title">⚙️ Settings</h1>

	{#if saved}
		<div class="toast">Saved!</div>
	{/if}

	<section class="setting-group">
		<label class="setting-label" for="player-name">Player Name</label>
		<div class="input-row">
			<input
				id="player-name"
				class="text-input"
				type="text"
				bind:value={nameInput}
				maxlength="24"
				placeholder="Enter your name"
			/>
			<button class="save-btn" onclick={saveName}>Save</button>
		</div>
	</section>

	<section class="setting-group">
		<div class="setting-label">Wall Color</div>
		<div class="color-presets">
			{#each presetColors as color}
				<button
					class="color-swatch"
					class:active={wallColor === color}
					style="background: {color}"
					onclick={() => { wallColor = color; gameStore.setWallColor(color); flash(); }}
					aria-label="Select color {color}"
				></button>
			{/each}
			<input
				type="color"
				class="color-picker"
				value={wallColor}
				oninput={saveWallColor}
				aria-label="Custom color picker"
			/>
		</div>
		<div class="color-preview">
			<span>Preview:</span>
			<div class="wall-sample" style="background: {wallColor}"></div>
		</div>
	</section>

	<section class="setting-group">
		<h3 class="setting-label">Data</h3>
		<p class="setting-desc">
			{#if authStore.isAuthenticated}
				Cloud sync is enabled for {authStore.user?.email}.
			{:else}
				Your progress is currently stored locally in your browser.
			{/if}
		</p>
		<div class="data-stats">
			<span>Coins: {gameStore.player.coinCount}</span>
			<span>Skins: {gameStore.player.unlockedSkinIds.length}</span>
		</div>
		<div class="data-stats sync-row">
			<span>Status: {gameStore.cloudSyncStatusLabel}</span>
			{#if gameStore.lastSyncedAtLabel}
				<span>Last sync: {gameStore.lastSyncedAtLabel}</span>
			{/if}
		</div>
		{#if gameStore.cloudSyncError}
			<p class="sync-error">{gameStore.cloudSyncError}</p>
		{/if}
		<div class="sync-actions">
			{#if authStore.isAuthenticated}
				<button class="save-btn" onclick={syncNow} disabled={gameStore.cloudSyncBusy}>Sync Now</button>
				<button class="secondary-btn" onclick={signOut} disabled={authStore.loading}>Sign Out</button>
			{:else}
				<a class="auth-link" href="{base}/auth">Sign in to enable sync</a>
			{/if}
		</div>
	</section>
</div>

<style>
	.settings-page {
		max-width: 500px;
		margin: 0 auto;
		padding: var(--space-4);
	}

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-6);
	}

	.toast {
		background: var(--color-accent-green);
		color: #000;
		padding: var(--space-2) var(--space-4);
		border-radius: var(--radius-md);
		text-align: center;
		font-weight: 600;
		margin-bottom: var(--space-4);
	}

	.setting-group {
		margin-bottom: var(--space-8);
	}

	.setting-label {
		display: block;
		font-weight: 600;
		font-size: var(--text-sm);
		text-transform: uppercase;
		letter-spacing: 0.5px;
		color: var(--color-text-muted);
		margin-bottom: var(--space-3);
	}

	.setting-desc {
		color: var(--color-text-muted);
		font-size: var(--text-sm);
		margin-bottom: var(--space-3);
	}

	.input-row {
		display: flex;
		gap: var(--space-2);
	}

	.text-input {
		flex: 1;
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		padding: var(--space-3);
		color: var(--color-text-primary);
		font-size: var(--text-base);
	}

	.text-input:focus {
		outline: none;
		border-color: var(--color-accent-primary);
	}

	.save-btn {
		background: var(--color-accent-primary);
		color: white;
		border: none;
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-4);
		font-weight: 600;
		cursor: pointer;
	}

	.color-presets {
		display: flex;
		gap: var(--space-2);
		flex-wrap: wrap;
		margin-bottom: var(--space-3);
	}

	.color-swatch {
		width: 36px;
		height: 36px;
		border-radius: var(--radius-full);
		border: 2px solid transparent;
		cursor: pointer;
		transition: all var(--transition-fast);
	}

	.color-swatch.active {
		border-color: white;
		box-shadow: 0 0 0 2px var(--color-accent-primary);
	}

	.color-picker {
		width: 36px;
		height: 36px;
		border-radius: var(--radius-full);
		border: 1px solid var(--color-border);
		cursor: pointer;
		padding: 0;
		background: none;
	}

	.color-preview {
		display: flex;
		align-items: center;
		gap: var(--space-3);
		font-size: var(--text-sm);
		color: var(--color-text-muted);
	}

	.wall-sample {
		width: 80px;
		height: 6px;
		border-radius: var(--radius-full);
	}

	.data-stats {
		display: flex;
		gap: var(--space-4);
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
	}

	.sync-row {
		margin-top: var(--space-3);
		flex-wrap: wrap;
	}

	.sync-actions {
		display: flex;
		gap: var(--space-2);
		align-items: center;
		margin-top: var(--space-4);
	}

	.secondary-btn,
	.auth-link {
		padding: var(--space-2) var(--space-4);
		border-radius: var(--radius-md);
		border: 1px solid var(--color-border);
		background: var(--color-bg-card);
		color: var(--color-text-primary);
		font-weight: 600;
		text-decoration: none;
	}

	.sync-error {
		margin-top: var(--space-3);
		color: var(--color-accent-red);
		font-size: var(--text-sm);
	}
</style>
